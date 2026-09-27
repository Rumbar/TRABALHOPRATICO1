using LocadoraVeiculos.Data;
using LocadoraVeiculos.DTOs;
using LocadoraVeiculos.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LocadoraVeiculos.Controllers
{
    /// <summary>Operacoes de CRUD sobre os veiculos da frota.</summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class VeiculosController : ControllerBase
    {
        private readonly ApplicationContext _context;

        public VeiculosController(ApplicationContext context)
        {
            _context = context;
        }

        /// <summary>Lista os veiculos da frota, com filtros opcionais.</summary>
        /// <param name="status">1-Disponivel, 2-Alugado, 3-EmManutencao, 4-Inativo.</param>
        /// <param name="modelo">Filtro por parte do modelo.</param>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<VeiculoResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<VeiculoResponse>>> Get(
            [FromQuery] StatusVeiculo? status,
            [FromQuery] string modelo)
        {
            var consulta = _context.Veiculos.AsQueryable();

            if (status.HasValue)
                consulta = consulta.Where(v => v.Status == status.Value);

            if (!string.IsNullOrWhiteSpace(modelo))
                consulta = consulta.Where(v => v.Modelo.Contains(modelo));

            var veiculos = await consulta
                .OrderBy(v => v.Modelo)
                .Select(v => new VeiculoResponse
                {
                    VeiculoId = v.VeiculoId,
                    Placa = v.Placa,
                    Modelo = v.Modelo,
                    AnoFabricacao = v.AnoFabricacao,
                    Quilometragem = v.Quilometragem,
                    Cor = v.Cor,
                    ValorDiaria = v.ValorDiaria,
                    Status = v.Status.ToString(),
                    FabricanteId = v.FabricanteId,
                    Fabricante = v.Fabricante.Nome,
                    CategoriaId = v.CategoriaId,
                    Categoria = v.Categoria.Nome
                })
                .ToListAsync();

            return Ok(veiculos);
        }

        /// <summary>Busca um veiculo pelo seu identificador.</summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(VeiculoResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<VeiculoResponse>> GetPorId(int id)
        {
            var veiculo = await _context.Veiculos
                .Where(v => v.VeiculoId == id)
                .Select(v => new VeiculoResponse
                {
                    VeiculoId = v.VeiculoId,
                    Placa = v.Placa,
                    Modelo = v.Modelo,
                    AnoFabricacao = v.AnoFabricacao,
                    Quilometragem = v.Quilometragem,
                    Cor = v.Cor,
                    ValorDiaria = v.ValorDiaria,
                    Status = v.Status.ToString(),
                    FabricanteId = v.FabricanteId,
                    Fabricante = v.Fabricante.Nome,
                    CategoriaId = v.CategoriaId,
                    Categoria = v.Categoria.Nome
                })
                .FirstOrDefaultAsync();

            if (veiculo is null)
                return NotFound(new { mensagem = $"Nenhum veiculo encontrado com o id {id}." });

            return Ok(veiculo);
        }

        /// <summary>Busca um veiculo pela placa.</summary>
        [HttpGet("placa/{placa}")]
        [ProducesResponseType(typeof(VeiculoResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<VeiculoResponse>> GetPorPlaca(string placa)
        {
            var normalizada = placa.Trim().ToUpper();

            var veiculo = await _context.Veiculos
                .Where(v => v.Placa == normalizada)
                .Select(v => new VeiculoResponse
                {
                    VeiculoId = v.VeiculoId,
                    Placa = v.Placa,
                    Modelo = v.Modelo,
                    AnoFabricacao = v.AnoFabricacao,
                    Quilometragem = v.Quilometragem,
                    Cor = v.Cor,
                    ValorDiaria = v.ValorDiaria,
                    Status = v.Status.ToString(),
                    FabricanteId = v.FabricanteId,
                    Fabricante = v.Fabricante.Nome,
                    CategoriaId = v.CategoriaId,
                    Categoria = v.Categoria.Nome
                })
                .FirstOrDefaultAsync();

            if (veiculo is null)
                return NotFound(new { mensagem = $"Nenhum veiculo encontrado com a placa {placa}." });

            return Ok(veiculo);
        }

        /// <summary>Cadastra um novo veiculo na frota.</summary>
        [HttpPost]
        [ProducesResponseType(typeof(VeiculoResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<VeiculoResponse>> Post([FromBody] VeiculoRequest request)
        {
            var placa = request.Placa.Trim().ToUpper();

            if (await _context.Veiculos.AnyAsync(v => v.Placa == placa))
                return Conflict(new { mensagem = $"Ja existe um veiculo cadastrado com a placa {placa}." });

            if (!await _context.Fabricantes.AnyAsync(f => f.FabricanteId == request.FabricanteId))
                return BadRequest(new { mensagem = $"Nao existe fabricante com o id {request.FabricanteId}." });

            if (!await _context.Categorias.AnyAsync(c => c.CategoriaId == request.CategoriaId))
                return BadRequest(new { mensagem = $"Nao existe categoria com o id {request.CategoriaId}." });

            if (request.AnoFabricacao > DateTime.Today.Year + 1)
                return BadRequest(new { mensagem = "O ano de fabricacao nao pode ser superior ao proximo ano." });

            var veiculo = new Veiculo
            {
                Placa = placa,
                Modelo = request.Modelo.Trim(),
                AnoFabricacao = request.AnoFabricacao,
                Quilometragem = request.Quilometragem,
                Cor = request.Cor?.Trim(),
                ValorDiaria = request.ValorDiaria,
                Status = request.Status,
                FabricanteId = request.FabricanteId,
                CategoriaId = request.CategoriaId
            };

            _context.Veiculos.Add(veiculo);
            await _context.SaveChangesAsync();

            await _context.Entry(veiculo).Reference(v => v.Fabricante).LoadAsync();
            await _context.Entry(veiculo).Reference(v => v.Categoria).LoadAsync();

            var resposta = new VeiculoResponse
            {
                VeiculoId = veiculo.VeiculoId,
                Placa = veiculo.Placa,
                Modelo = veiculo.Modelo,
                AnoFabricacao = veiculo.AnoFabricacao,
                Quilometragem = veiculo.Quilometragem,
                Cor = veiculo.Cor,
                ValorDiaria = veiculo.ValorDiaria,
                Status = veiculo.Status.ToString(),
                FabricanteId = veiculo.FabricanteId,
                Fabricante = veiculo.Fabricante?.Nome,
                CategoriaId = veiculo.CategoriaId,
                Categoria = veiculo.Categoria?.Nome
            };

            return CreatedAtAction(nameof(GetPorId), new { id = veiculo.VeiculoId }, resposta);
        }

        /// <summary>Atualiza os dados de um veiculo existente.</summary>
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Put(int id, [FromBody] VeiculoRequest request)
        {
            var veiculo = await _context.Veiculos.FindAsync(id);

            if (veiculo is null)
                return NotFound(new { mensagem = $"Nenhum veiculo encontrado com o id {id}." });

            var placa = request.Placa.Trim().ToUpper();

            if (await _context.Veiculos.AnyAsync(v => v.VeiculoId != id && v.Placa == placa))
                return Conflict(new { mensagem = $"A placa {placa} ja pertence a outro veiculo." });

            if (!await _context.Fabricantes.AnyAsync(f => f.FabricanteId == request.FabricanteId))
                return BadRequest(new { mensagem = $"Nao existe fabricante com o id {request.FabricanteId}." });

            if (!await _context.Categorias.AnyAsync(c => c.CategoriaId == request.CategoriaId))
                return BadRequest(new { mensagem = $"Nao existe categoria com o id {request.CategoriaId}." });

            if (request.Quilometragem < veiculo.Quilometragem)
                return BadRequest(new { mensagem = $"A quilometragem nao pode ser reduzida (atual: {veiculo.Quilometragem} km)." });

            veiculo.Placa = placa;
            veiculo.Modelo = request.Modelo.Trim();
            veiculo.AnoFabricacao = request.AnoFabricacao;
            veiculo.Quilometragem = request.Quilometragem;
            veiculo.Cor = request.Cor?.Trim();
            veiculo.ValorDiaria = request.ValorDiaria;
            veiculo.Status = request.Status;
            veiculo.FabricanteId = request.FabricanteId;
            veiculo.CategoriaId = request.CategoriaId;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensagem = "Veiculo atualizado com sucesso.",
                veiculo = new VeiculoResponse
                {
                    VeiculoId = veiculo.VeiculoId,
                    Placa = veiculo.Placa,
                    Modelo = veiculo.Modelo,
                    AnoFabricacao = veiculo.AnoFabricacao,
                    Quilometragem = veiculo.Quilometragem,
                    Cor = veiculo.Cor,
                    ValorDiaria = veiculo.ValorDiaria,
                    Status = veiculo.Status.ToString(),
                    FabricanteId = veiculo.FabricanteId,
                    CategoriaId = veiculo.CategoriaId
                }
            });
        }

        /// <summary>Exclui um veiculo que nao possua alugueis registrados.</summary>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Delete(int id)
        {
            var veiculo = await _context.Veiculos.FindAsync(id);

            if (veiculo is null)
                return NotFound(new { mensagem = $"Nenhum veiculo encontrado com o id {id}." });

            var possuiAlugueis = await _context.Alugueis.AnyAsync(a => a.VeiculoId == id);

            if (possuiAlugueis)
                return Conflict(new { mensagem = "Nao e possivel excluir: existem alugueis registrados para este veiculo." });

            _context.Veiculos.Remove(veiculo);
            await _context.SaveChangesAsync();

            return Ok(new { mensagem = $"Veiculo {veiculo.Placa} excluido com sucesso." });
        }
    }
}
