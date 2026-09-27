using LocadoraVeiculos.Data;
using LocadoraVeiculos.DTOs;
using LocadoraVeiculos.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LocadoraVeiculos.Controllers
{
    /// <summary>
    /// Consultas filtradas que combinam varias tabelas.
    /// As rotas deste controller utilizam INNER JOIN, LEFT OUTER JOIN e GROUP JOIN com agregacao.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ConsultasController : ControllerBase
    {
        private readonly ApplicationContext _context;

        public ConsultasController(ApplicationContext context)
        {
            _context = context;
        }

        /// <summary>
        /// FILTRO 1 - Veiculos disponiveis para locacao.
        /// INNER JOIN entre Veiculo, Fabricante e Categoria.
        /// </summary>
        /// <param name="categoriaId">Filtro opcional por categoria.</param>
        /// <param name="fabricanteId">Filtro opcional por fabricante.</param>
        /// <param name="valorMaximo">Valor maximo da diaria.</param>
        [HttpGet("veiculos-disponiveis")]
        [ProducesResponseType(typeof(IEnumerable<VeiculoDisponivelResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<VeiculoDisponivelResponse>>> VeiculosDisponiveis(
            [FromQuery] int? categoriaId,
            [FromQuery] int? fabricanteId,
            [FromQuery] decimal? valorMaximo)
        {
            var consulta = from v in _context.Veiculos
                           join f in _context.Fabricantes on v.FabricanteId equals f.FabricanteId
                           join c in _context.Categorias on v.CategoriaId equals c.CategoriaId
                           where v.Status == StatusVeiculo.Disponivel
                           select new
                           {
                               v.VeiculoId,
                               v.Placa,
                               v.Modelo,
                               v.AnoFabricacao,
                               v.Quilometragem,
                               v.ValorDiaria,
                               FabricanteId = f.FabricanteId,
                               Fabricante = f.Nome,
                               CategoriaId = c.CategoriaId,
                               Categoria = c.Nome,
                               c.PercentualAjuste
                           };

            if (categoriaId.HasValue)
                consulta = consulta.Where(x => x.CategoriaId == categoriaId.Value);

            if (fabricanteId.HasValue)
                consulta = consulta.Where(x => x.FabricanteId == fabricanteId.Value);

            if (valorMaximo.HasValue)
                consulta = consulta.Where(x => x.ValorDiaria <= valorMaximo.Value);

            var linhas = await consulta.OrderBy(x => x.ValorDiaria).ToListAsync();

            var resultado = linhas.Select(x => new VeiculoDisponivelResponse
            {
                VeiculoId = x.VeiculoId,
                Placa = x.Placa,
                Modelo = x.Modelo,
                Fabricante = x.Fabricante,
                Categoria = x.Categoria,
                AnoFabricacao = x.AnoFabricacao,
                Quilometragem = x.Quilometragem,
                ValorDiaria = x.ValorDiaria,
                ValorDiariaComAjuste = Math.Round(x.ValorDiaria * (1 + x.PercentualAjuste / 100m), 2)
            }).ToList();

            return Ok(resultado);
        }

        /// <summary>
        /// FILTRO 2 - Historico de alugueis de um cliente, localizado pelo CPF.
        /// INNER JOIN entre Aluguel, Cliente, Veiculo, Fabricante e Funcionario.
        /// </summary>
        [HttpGet("alugueis-por-cliente/{cpf}")]
        [ProducesResponseType(typeof(IEnumerable<AluguelConsultaResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<AluguelConsultaResponse>>> AlugueisPorCliente(string cpf)
        {
            var clienteExiste = await _context.Clientes.AnyAsync(c => c.Cpf == cpf);

            if (!clienteExiste)
                return NotFound(new { mensagem = $"Nenhum cliente encontrado com o CPF {cpf}." });

            var linhas = await (from a in _context.Alugueis
                                join cl in _context.Clientes on a.ClienteId equals cl.ClienteId
                                join v in _context.Veiculos on a.VeiculoId equals v.VeiculoId
                                join fab in _context.Fabricantes on v.FabricanteId equals fab.FabricanteId
                                join fun in _context.Funcionarios on a.FuncionarioId equals fun.FuncionarioId
                                where cl.Cpf == cpf
                                orderby a.DataRetirada descending
                                select new AluguelLinhaConsulta
                                {
                                    AluguelId = a.AluguelId,
                                    Cliente = cl.Nome,
                                    Cpf = cl.Cpf,
                                    Veiculo = v.Modelo,
                                    Placa = v.Placa,
                                    Fabricante = fab.Nome,
                                    Funcionario = fun.Nome,
                                    DataRetirada = a.DataRetirada,
                                    DataPrevistaDevolucao = a.DataPrevistaDevolucao,
                                    DataDevolucao = a.DataDevolucao,
                                    ValorTotal = a.ValorTotal,
                                    Status = a.Status
                                }).ToListAsync();

            return Ok(linhas.Select(MontarConsulta).ToList());
        }

        /// <summary>
        /// FILTRO 3 - Contratos em aberto cuja data prevista de devolucao ja passou.
        /// INNER JOIN entre Aluguel, Cliente, Veiculo, Fabricante e Funcionario.
        /// </summary>
        [HttpGet("alugueis-em-atraso")]
        [ProducesResponseType(typeof(IEnumerable<AluguelConsultaResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<AluguelConsultaResponse>>> AlugueisEmAtraso()
        {
            var hoje = DateTime.Today;

            var linhas = await (from a in _context.Alugueis
                                join cl in _context.Clientes on a.ClienteId equals cl.ClienteId
                                join v in _context.Veiculos on a.VeiculoId equals v.VeiculoId
                                join fab in _context.Fabricantes on v.FabricanteId equals fab.FabricanteId
                                join fun in _context.Funcionarios on a.FuncionarioId equals fun.FuncionarioId
                                where a.DataDevolucao == null
                                      && a.Status == StatusAluguel.EmAndamento
                                      && a.DataPrevistaDevolucao < hoje
                                orderby a.DataPrevistaDevolucao
                                select new AluguelLinhaConsulta
                                {
                                    AluguelId = a.AluguelId,
                                    Cliente = cl.Nome,
                                    Cpf = cl.Cpf,
                                    Veiculo = v.Modelo,
                                    Placa = v.Placa,
                                    Fabricante = fab.Nome,
                                    Funcionario = fun.Nome,
                                    DataRetirada = a.DataRetirada,
                                    DataPrevistaDevolucao = a.DataPrevistaDevolucao,
                                    DataDevolucao = a.DataDevolucao,
                                    ValorTotal = a.ValorTotal,
                                    Status = a.Status
                                }).ToListAsync();

            return Ok(linhas.Select(MontarConsulta).ToList());
        }

        /// <summary>
        /// FILTRO 4 - Alugueis retirados dentro de um intervalo de datas.
        /// INNER JOIN entre Aluguel, Cliente, Veiculo e Funcionario.
        /// </summary>
        /// <param name="inicio">Data inicial do periodo.</param>
        /// <param name="fim">Data final do periodo.</param>
        [HttpGet("alugueis-por-periodo")]
        [ProducesResponseType(typeof(IEnumerable<AluguelConsultaResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<AluguelConsultaResponse>>> AlugueisPorPeriodo(
            [FromQuery] DateTime inicio,
            [FromQuery] DateTime fim)
        {
            if (inicio == default || fim == default)
                return BadRequest(new { mensagem = "Informe as datas de inicio e fim no formato aaaa-mm-dd." });

            if (fim.Date < inicio.Date)
                return BadRequest(new { mensagem = "A data final nao pode ser anterior a data inicial." });

            var dataFinal = fim.Date.AddDays(1).AddTicks(-1);

            var linhas = await (from a in _context.Alugueis
                                join cl in _context.Clientes on a.ClienteId equals cl.ClienteId
                                join v in _context.Veiculos on a.VeiculoId equals v.VeiculoId
                                join fab in _context.Fabricantes on v.FabricanteId equals fab.FabricanteId
                                join fun in _context.Funcionarios on a.FuncionarioId equals fun.FuncionarioId
                                where a.DataRetirada >= inicio.Date && a.DataRetirada <= dataFinal
                                orderby a.DataRetirada
                                select new AluguelLinhaConsulta
                                {
                                    AluguelId = a.AluguelId,
                                    Cliente = cl.Nome,
                                    Cpf = cl.Cpf,
                                    Veiculo = v.Modelo,
                                    Placa = v.Placa,
                                    Fabricante = fab.Nome,
                                    Funcionario = fun.Nome,
                                    DataRetirada = a.DataRetirada,
                                    DataPrevistaDevolucao = a.DataPrevistaDevolucao,
                                    DataDevolucao = a.DataDevolucao,
                                    ValorTotal = a.ValorTotal,
                                    Status = a.Status
                                }).ToListAsync();

            return Ok(linhas.Select(MontarConsulta).ToList());
        }

        /// <summary>
        /// FILTRO 5 - Clientes cadastrados que nunca alugaram um veiculo.
        /// LEFT OUTER JOIN entre Cliente e Aluguel (GroupJoin + DefaultIfEmpty).
        /// </summary>
        [HttpGet("clientes-sem-aluguel")]
        [ProducesResponseType(typeof(IEnumerable<ClienteSemAluguelResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ClienteSemAluguelResponse>>> ClientesSemAluguel()
        {
            var resultado = await (from c in _context.Clientes
                                   join a in _context.Alugueis on c.ClienteId equals a.ClienteId into alugueis
                                   from a in alugueis.DefaultIfEmpty()
                                   where a == null
                                   orderby c.Nome
                                   select new ClienteSemAluguelResponse
                                   {
                                       ClienteId = c.ClienteId,
                                       Nome = c.Nome,
                                       Cpf = c.Cpf,
                                       Email = c.Email,
                                       DataCadastro = c.DataCadastro
                                   }).ToListAsync();

            return Ok(resultado);
        }

        /// <summary>
        /// FILTRO 6 - Veiculos da frota que nunca foram alugados.
        /// LEFT OUTER JOIN com Aluguel combinado a INNER JOIN com Fabricante e Categoria.
        /// </summary>
        [HttpGet("veiculos-nunca-alugados")]
        [ProducesResponseType(typeof(IEnumerable<VeiculoNuncaAlugadoResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<VeiculoNuncaAlugadoResponse>>> VeiculosNuncaAlugados()
        {
            var linhas = await (from v in _context.Veiculos
                                join f in _context.Fabricantes on v.FabricanteId equals f.FabricanteId
                                join c in _context.Categorias on v.CategoriaId equals c.CategoriaId
                                join a in _context.Alugueis on v.VeiculoId equals a.VeiculoId into alugueis
                                from a in alugueis.DefaultIfEmpty()
                                where a == null
                                orderby v.Modelo
                                select new
                                {
                                    v.VeiculoId,
                                    v.Placa,
                                    v.Modelo,
                                    Fabricante = f.Nome,
                                    Categoria = c.Nome,
                                    v.ValorDiaria,
                                    v.Status
                                }).ToListAsync();

            var resultado = linhas.Select(x => new VeiculoNuncaAlugadoResponse
            {
                VeiculoId = x.VeiculoId,
                Placa = x.Placa,
                Modelo = x.Modelo,
                Fabricante = x.Fabricante,
                Categoria = x.Categoria,
                ValorDiaria = x.ValorDiaria,
                Status = x.Status.ToString()
            }).ToList();

            return Ok(resultado);
        }

        /// <summary>
        /// FILTRO 7 - Faturamento consolidado por categoria de veiculo.
        /// LEFT OUTER JOIN em duas tabelas (Categoria -> Veiculo -> Aluguel) com agrupamento e soma.
        /// </summary>
        [HttpGet("faturamento-por-categoria")]
        [ProducesResponseType(typeof(IEnumerable<FaturamentoCategoriaResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<FaturamentoCategoriaResponse>>> FaturamentoPorCategoria()
        {
            var linhas = await (from c in _context.Categorias
                                join v in _context.Veiculos on c.CategoriaId equals v.CategoriaId into veiculos
                                from v in veiculos.DefaultIfEmpty()
                                join a in _context.Alugueis on v.VeiculoId equals a.VeiculoId into alugueis
                                from a in alugueis.DefaultIfEmpty()
                                select new
                                {
                                    c.CategoriaId,
                                    CategoriaNome = c.Nome,
                                    VeiculoId = (int?)v.VeiculoId,
                                    AluguelId = (int?)a.AluguelId,
                                    ValorTotal = (decimal?)a.ValorTotal,
                                    Status = (StatusAluguel?)a.Status
                                }).ToListAsync();

            var resultado = linhas
                .GroupBy(x => new { x.CategoriaId, x.CategoriaNome })
                .Select(g =>
                {
                    var finalizados = g.Where(x => x.Status == StatusAluguel.Finalizado).ToList();
                    var faturado = finalizados.Sum(x => x.ValorTotal ?? 0m);

                    return new FaturamentoCategoriaResponse
                    {
                        CategoriaId = g.Key.CategoriaId,
                        Categoria = g.Key.CategoriaNome,
                        QuantidadeVeiculos = g.Where(x => x.VeiculoId.HasValue)
                                              .Select(x => x.VeiculoId.Value)
                                              .Distinct()
                                              .Count(),
                        QuantidadeAlugueis = g.Count(x => x.AluguelId.HasValue),
                        AlugueisFinalizados = finalizados.Count,
                        ValorTotalFaturado = faturado,
                        TicketMedio = finalizados.Count > 0
                            ? Math.Round(faturado / finalizados.Count, 2)
                            : 0m
                    };
                })
                .OrderByDescending(x => x.ValorTotalFaturado)
                .ToList();

            return Ok(resultado);
        }

        /// <summary>Converte a linha da consulta em objeto de resposta, calculando os dias de atraso.</summary>
        private static AluguelConsultaResponse MontarConsulta(AluguelLinhaConsulta x)
        {
            var referencia = x.DataDevolucao ?? DateTime.Today;
            var atraso = (referencia.Date - x.DataPrevistaDevolucao.Date).Days;

            return new AluguelConsultaResponse
            {
                AluguelId = x.AluguelId,
                Cliente = x.Cliente,
                Cpf = x.Cpf,
                Veiculo = x.Veiculo,
                Placa = x.Placa,
                Fabricante = x.Fabricante,
                Funcionario = x.Funcionario,
                DataRetirada = x.DataRetirada,
                DataPrevistaDevolucao = x.DataPrevistaDevolucao,
                DataDevolucao = x.DataDevolucao,
                ValorTotal = x.ValorTotal,
                Status = x.Status.ToString(),
                DiasEmAtraso = atraso > 0 ? atraso : 0
            };
        }
    }
}
