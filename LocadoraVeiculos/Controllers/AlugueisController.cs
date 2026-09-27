using LocadoraVeiculos.Data;
using LocadoraVeiculos.DTOs;
using LocadoraVeiculos.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LocadoraVeiculos.Controllers
{
    /// <summary>Operacoes de CRUD sobre os contratos de aluguel, incluindo o registro da devolucao.</summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AlugueisController : ControllerBase
    {
        private readonly ApplicationContext _context;

        public AlugueisController(ApplicationContext context)
        {
            _context = context;
        }

        /// <summary>Lista os contratos de aluguel, com filtros opcionais.</summary>
        /// <param name="status">1-EmAndamento, 2-Finalizado, 3-Cancelado.</param>
        /// <param name="clienteId">Filtra os contratos de um cliente.</param>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<AluguelResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<AluguelResponse>>> Get(
            [FromQuery] StatusAluguel? status,
            [FromQuery] int? clienteId)
        {
            var consulta = _context.Alugueis
                .Include(a => a.Cliente)
                .Include(a => a.Veiculo)
                .Include(a => a.Funcionario)
                .AsQueryable();

            if (status.HasValue)
                consulta = consulta.Where(a => a.Status == status.Value);

            if (clienteId.HasValue)
                consulta = consulta.Where(a => a.ClienteId == clienteId.Value);

            var alugueis = await consulta
                .OrderByDescending(a => a.DataRetirada)
                .ToListAsync();

            return Ok(alugueis.Select(MontarResposta).ToList());
        }

        /// <summary>Busca um contrato de aluguel pelo seu identificador.</summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(AluguelResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AluguelResponse>> GetPorId(int id)
        {
            var aluguel = await _context.Alugueis
                .Include(a => a.Cliente)
                .Include(a => a.Veiculo)
                .Include(a => a.Funcionario)
                .FirstOrDefaultAsync(a => a.AluguelId == id);

            if (aluguel is null)
                return NotFound(new { mensagem = $"Nenhum aluguel encontrado com o id {id}." });

            return Ok(MontarResposta(aluguel));
        }

        /// <summary>Abre um novo contrato de aluguel e marca o veiculo como alugado.</summary>
        [HttpPost]
        [ProducesResponseType(typeof(AluguelResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<AluguelResponse>> Post([FromBody] AluguelRequest request)
        {
            var cliente = await _context.Clientes.FindAsync(request.ClienteId);
            if (cliente is null)
                return BadRequest(new { mensagem = $"Nao existe cliente com o id {request.ClienteId}." });

            var funcionario = await _context.Funcionarios.FindAsync(request.FuncionarioId);
            if (funcionario is null)
                return BadRequest(new { mensagem = $"Nao existe funcionario com o id {request.FuncionarioId}." });

            var veiculo = await _context.Veiculos
                .Include(v => v.Categoria)
                .FirstOrDefaultAsync(v => v.VeiculoId == request.VeiculoId);

            if (veiculo is null)
                return BadRequest(new { mensagem = $"Nao existe veiculo com o id {request.VeiculoId}." });

            if (veiculo.Status != StatusVeiculo.Disponivel)
                return Conflict(new
                {
                    mensagem = $"O veiculo {veiculo.Placa} nao esta disponivel para locacao.",
                    situacaoAtual = veiculo.Status.ToString()
                });

            var diarias = Math.Max(1, (request.DataPrevistaDevolucao.Date - request.DataRetirada.Date).Days);
            var percentual = veiculo.Categoria != null ? veiculo.Categoria.PercentualAjuste : 0m;
            var valorDiaria = Math.Round(veiculo.ValorDiaria * (1 + percentual / 100m), 2);

            var aluguel = new Aluguel
            {
                ClienteId = request.ClienteId,
                VeiculoId = request.VeiculoId,
                FuncionarioId = request.FuncionarioId,
                DataRetirada = request.DataRetirada,
                DataPrevistaDevolucao = request.DataPrevistaDevolucao,
                QuilometragemInicial = veiculo.Quilometragem,
                ValorDiaria = valorDiaria,
                ValorTotal = Math.Round(valorDiaria * diarias, 2),
                Status = StatusAluguel.EmAndamento,
                Observacao = request.Observacao?.Trim()
            };

            veiculo.Status = StatusVeiculo.Alugado;

            _context.Alugueis.Add(aluguel);
            await _context.SaveChangesAsync();

            aluguel.Cliente = cliente;
            aluguel.Veiculo = veiculo;
            aluguel.Funcionario = funcionario;

            return CreatedAtAction(nameof(GetPorId), new { id = aluguel.AluguelId }, MontarResposta(aluguel));
        }

        /// <summary>Atualiza a data prevista de devolucao e a observacao de um contrato em andamento.</summary>
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Put(int id, [FromBody] AluguelUpdateRequest request)
        {
            var aluguel = await _context.Alugueis
                .Include(a => a.Cliente)
                .Include(a => a.Veiculo)
                .Include(a => a.Funcionario)
                .FirstOrDefaultAsync(a => a.AluguelId == id);

            if (aluguel is null)
                return NotFound(new { mensagem = $"Nenhum aluguel encontrado com o id {id}." });

            if (aluguel.Status != StatusAluguel.EmAndamento)
                return Conflict(new { mensagem = $"Somente contratos em andamento podem ser alterados. Situacao atual: {aluguel.Status}." });

            if (request.DataPrevistaDevolucao.Date <= aluguel.DataRetirada.Date)
                return BadRequest(new { mensagem = "A data prevista de devolucao deve ser posterior a data de retirada." });

            aluguel.DataPrevistaDevolucao = request.DataPrevistaDevolucao;
            aluguel.Observacao = request.Observacao?.Trim();

            var diarias = Math.Max(1, (aluguel.DataPrevistaDevolucao.Date - aluguel.DataRetirada.Date).Days);
            aluguel.ValorTotal = Math.Round(aluguel.ValorDiaria * diarias, 2);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensagem = "Aluguel atualizado com sucesso.",
                aluguel = MontarResposta(aluguel)
            });
        }

        /// <summary>Registra a devolucao do veiculo, recalcula o valor total e libera o veiculo.</summary>
        [HttpPut("{id:int}/devolucao")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> RegistrarDevolucao(int id, [FromBody] DevolucaoRequest request)
        {
            var aluguel = await _context.Alugueis
                .Include(a => a.Cliente)
                .Include(a => a.Veiculo)
                .Include(a => a.Funcionario)
                .FirstOrDefaultAsync(a => a.AluguelId == id);

            if (aluguel is null)
                return NotFound(new { mensagem = $"Nenhum aluguel encontrado com o id {id}." });

            if (aluguel.Status != StatusAluguel.EmAndamento)
                return Conflict(new { mensagem = $"Este contrato ja foi encerrado. Situacao atual: {aluguel.Status}." });

            if (request.DataDevolucao.Date < aluguel.DataRetirada.Date)
                return BadRequest(new { mensagem = "A data de devolucao nao pode ser anterior a data de retirada." });

            if (request.QuilometragemFinal < aluguel.QuilometragemInicial)
                return BadRequest(new
                {
                    mensagem = $"A quilometragem final deve ser maior ou igual a inicial ({aluguel.QuilometragemInicial} km)."
                });

            aluguel.DataDevolucao = request.DataDevolucao;
            aluguel.QuilometragemFinal = request.QuilometragemFinal;
            aluguel.Status = StatusAluguel.Finalizado;

            if (!string.IsNullOrWhiteSpace(request.Observacao))
                aluguel.Observacao = request.Observacao.Trim();

            var diariasReais = Math.Max(1, (request.DataDevolucao.Date - aluguel.DataRetirada.Date).Days);
            aluguel.ValorTotal = Math.Round(aluguel.ValorDiaria * diariasReais, 2);

            if (aluguel.Veiculo != null)
            {
                aluguel.Veiculo.Quilometragem = request.QuilometragemFinal;
                aluguel.Veiculo.Status = StatusVeiculo.Disponivel;
            }

            await _context.SaveChangesAsync();

            var diasAtraso = (request.DataDevolucao.Date - aluguel.DataPrevistaDevolucao.Date).Days;

            return Ok(new
            {
                mensagem = "Devolucao registrada com sucesso.",
                diariasCobradas = diariasReais,
                diasEmAtraso = diasAtraso > 0 ? diasAtraso : 0,
                quilometragemPercorrida = request.QuilometragemFinal - aluguel.QuilometragemInicial,
                aluguel = MontarResposta(aluguel)
            });
        }

        /// <summary>Exclui um contrato de aluguel. Se estiver em andamento, o veiculo volta a ficar disponivel.</summary>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var aluguel = await _context.Alugueis
                .Include(a => a.Veiculo)
                .FirstOrDefaultAsync(a => a.AluguelId == id);

            if (aluguel is null)
                return NotFound(new { mensagem = $"Nenhum aluguel encontrado com o id {id}." });

            if (aluguel.Status == StatusAluguel.EmAndamento && aluguel.Veiculo != null)
                aluguel.Veiculo.Status = StatusVeiculo.Disponivel;

            _context.Alugueis.Remove(aluguel);
            await _context.SaveChangesAsync();

            return Ok(new { mensagem = $"Aluguel {id} excluido com sucesso." });
        }

        /// <summary>Converte a entidade em objeto de resposta da API.</summary>
        private static AluguelResponse MontarResposta(Aluguel a)
        {
            var diarias = Math.Max(1, (a.DataPrevistaDevolucao.Date - a.DataRetirada.Date).Days);

            return new AluguelResponse
            {
                AluguelId = a.AluguelId,
                ClienteId = a.ClienteId,
                Cliente = a.Cliente?.Nome,
                CpfCliente = a.Cliente?.Cpf,
                VeiculoId = a.VeiculoId,
                Veiculo = a.Veiculo?.Modelo,
                Placa = a.Veiculo?.Placa,
                FuncionarioId = a.FuncionarioId,
                Funcionario = a.Funcionario?.Nome,
                DataRetirada = a.DataRetirada,
                DataPrevistaDevolucao = a.DataPrevistaDevolucao,
                DataDevolucao = a.DataDevolucao,
                QuilometragemInicial = a.QuilometragemInicial,
                QuilometragemFinal = a.QuilometragemFinal,
                QuilometragemPercorrida = a.QuilometragemFinal.HasValue
                    ? a.QuilometragemFinal.Value - a.QuilometragemInicial
                    : (int?)null,
                QuantidadeDiarias = diarias,
                ValorDiaria = a.ValorDiaria,
                ValorTotal = a.ValorTotal,
                Status = a.Status.ToString(),
                Observacao = a.Observacao
            };
        }
    }
}
