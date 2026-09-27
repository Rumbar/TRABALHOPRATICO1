using LocadoraVeiculos.Data;
using LocadoraVeiculos.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LocadoraVeiculos.Controllers
{
    /// <summary>
    /// Carga de registros de exemplo, usada para demonstrar as rotas de consulta.
    /// Os fabricantes e as categorias ja sao criados pela migracao inicial.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class SeedController : ControllerBase
    {
        private readonly ApplicationContext _context;

        public SeedController(ApplicationContext context)
        {
            _context = context;
        }

        /// <summary>Insere funcionarios, clientes, veiculos e alugueis de demonstracao.</summary>
        [HttpPost("carga-exemplo")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> CargaExemplo()
        {
            if (await _context.Alugueis.AnyAsync() || await _context.Veiculos.AnyAsync())
            {
                return Conflict(new
                {
                    mensagem = "O banco ja possui registros. Use DELETE /api/seed/limpar antes de executar a carga novamente."
                });
            }

            var fabricantes = await _context.Fabricantes.OrderBy(f => f.FabricanteId).ToListAsync();
            var categorias = await _context.Categorias.OrderBy(c => c.CategoriaId).ToListAsync();

            if (fabricantes.Count < 4 || categorias.Count < 4)
            {
                return Conflict(new
                {
                    mensagem = "Fabricantes ou categorias ausentes. Aplique a migracao inicial com 'dotnet ef database update'."
                });
            }

            var funcionarios = new List<Funcionario>
            {
                new Funcionario { Nome = "Diogo Ramos",       Cpf = "10020030040", Matricula = "F1285293", Cargo = "Atendente",  DataAdmissao = new DateTime(2024, 3, 11) },
                new Funcionario { Nome = "Camila Rezende",    Cpf = "20030040050", Matricula = "F2039118", Cargo = "Supervisora", DataAdmissao = new DateTime(2022, 8, 1) },
                new Funcionario { Nome = "Paulo Vieira",      Cpf = "30040050060", Matricula = "F3098774", Cargo = "Atendente",  DataAdmissao = new DateTime(2025, 1, 20) }
            };
            _context.Funcionarios.AddRange(funcionarios);

            var clientes = new List<Cliente>
            {
                new Cliente { Nome = "Marcos Antunes",  Cpf = "11122233344", Email = "marcos.antunes@email.com",  Telefone = "31988770011", DataNascimento = new DateTime(1990, 5, 14), NumeroCnh = "01122334455", CategoriaCnh = "B", DataCadastro = DateTime.Today.AddMonths(-8) },
                new Cliente { Nome = "Renata Lopes",    Cpf = "22233344455", Email = "renata.lopes@email.com",    Telefone = "31977660022", DataNascimento = new DateTime(1986, 11, 2), NumeroCnh = "02233445566", CategoriaCnh = "B", DataCadastro = DateTime.Today.AddMonths(-6) },
                new Cliente { Nome = "Tiago Moreira",   Cpf = "33344455566", Email = "tiago.moreira@email.com",   Telefone = "31966550033", DataNascimento = new DateTime(1995, 2, 27), NumeroCnh = "03344556677", CategoriaCnh = "AB", DataCadastro = DateTime.Today.AddMonths(-4) },
                new Cliente { Nome = "Priscila Damiao", Cpf = "44455566677", Email = "priscila.damiao@email.com", Telefone = "31955440044", DataNascimento = new DateTime(1992, 9, 9),  NumeroCnh = "04455667788", CategoriaCnh = "B", DataCadastro = DateTime.Today.AddMonths(-2) },
                new Cliente { Nome = "Eduardo Bastos",  Cpf = "55566677788", Email = "eduardo.bastos@email.com",  Telefone = "31944330055", DataNascimento = new DateTime(1979, 7, 18), NumeroCnh = "05566778899", CategoriaCnh = "AB", DataCadastro = DateTime.Today.AddDays(-20) }
            };
            _context.Clientes.AddRange(clientes);

            var veiculos = new List<Veiculo>
            {
                new Veiculo { Placa = "ABC1D23", Modelo = "Gol 1.0",       AnoFabricacao = 2022, Quilometragem = 38400, Cor = "Branco",  ValorDiaria = 110.00m, FabricanteId = fabricantes[0].FabricanteId, CategoriaId = categorias[0].CategoriaId },
                new Veiculo { Placa = "BCD2E34", Modelo = "Polo Track",    AnoFabricacao = 2023, Quilometragem = 21750, Cor = "Prata",   ValorDiaria = 145.00m, FabricanteId = fabricantes[0].FabricanteId, CategoriaId = categorias[1].CategoriaId },
                new Veiculo { Placa = "CDE3F45", Modelo = "Mobi Like",     AnoFabricacao = 2021, Quilometragem = 54900, Cor = "Vermelho",ValorDiaria = 98.00m,  FabricanteId = fabricantes[1].FabricanteId, CategoriaId = categorias[0].CategoriaId },
                new Veiculo { Placa = "DEF4G56", Modelo = "Pulse Drive",   AnoFabricacao = 2024, Quilometragem = 12300, Cor = "Cinza",   ValorDiaria = 189.00m, FabricanteId = fabricantes[1].FabricanteId, CategoriaId = categorias[2].CategoriaId },
                new Veiculo { Placa = "EFG5H67", Modelo = "Corolla XEi",   AnoFabricacao = 2023, Quilometragem = 29800, Cor = "Preto",   ValorDiaria = 265.00m, FabricanteId = fabricantes[2].FabricanteId, CategoriaId = categorias[3].CategoriaId },
                new Veiculo { Placa = "FGH6I78", Modelo = "Corolla Cross", AnoFabricacao = 2024, Quilometragem = 15600, Cor = "Branco",  ValorDiaria = 298.00m, FabricanteId = fabricantes[2].FabricanteId, CategoriaId = categorias[2].CategoriaId },
                new Veiculo { Placa = "GHI7J89", Modelo = "Onix LT",       AnoFabricacao = 2022, Quilometragem = 41200, Cor = "Azul",    ValorDiaria = 132.00m, FabricanteId = fabricantes[3].FabricanteId, CategoriaId = categorias[1].CategoriaId },
                new Veiculo { Placa = "HIJ8K90", Modelo = "Tracker Premier",AnoFabricacao = 2023,Quilometragem = 33100, Cor = "Grafite", ValorDiaria = 245.00m, FabricanteId = fabricantes[3].FabricanteId, CategoriaId = categorias[2].CategoriaId }
            };
            _context.Veiculos.AddRange(veiculos);

            await _context.SaveChangesAsync();

            var hoje = DateTime.Today;

            var alugueis = new List<Aluguel>
            {
                // Finalizado, devolvido no prazo
                new Aluguel
                {
                    ClienteId = clientes[0].ClienteId, VeiculoId = veiculos[0].VeiculoId, FuncionarioId = funcionarios[0].FuncionarioId,
                    DataRetirada = hoje.AddDays(-45), DataPrevistaDevolucao = hoje.AddDays(-40), DataDevolucao = hoje.AddDays(-40),
                    QuilometragemInicial = 37200, QuilometragemFinal = 38400,
                    ValorDiaria = 110.00m, ValorTotal = 550.00m,
                    Status = StatusAluguel.Finalizado, Observacao = "Devolucao no prazo."
                },
                // Finalizado, devolvido com atraso
                new Aluguel
                {
                    ClienteId = clientes[1].ClienteId, VeiculoId = veiculos[4].VeiculoId, FuncionarioId = funcionarios[1].FuncionarioId,
                    DataRetirada = hoje.AddDays(-30), DataPrevistaDevolucao = hoje.AddDays(-25), DataDevolucao = hoje.AddDays(-22),
                    QuilometragemInicial = 27950, QuilometragemFinal = 29800,
                    ValorDiaria = 424.00m, ValorTotal = 3392.00m,
                    Status = StatusAluguel.Finalizado, Observacao = "Devolvido com 3 dias de atraso."
                },
                // Finalizado
                new Aluguel
                {
                    ClienteId = clientes[0].ClienteId, VeiculoId = veiculos[6].VeiculoId, FuncionarioId = funcionarios[2].FuncionarioId,
                    DataRetirada = hoje.AddDays(-18), DataPrevistaDevolucao = hoje.AddDays(-15), DataDevolucao = hoje.AddDays(-15),
                    QuilometragemInicial = 40100, QuilometragemFinal = 41200,
                    ValorDiaria = 151.80m, ValorTotal = 455.40m,
                    Status = StatusAluguel.Finalizado
                },
                // Em andamento, dentro do prazo
                new Aluguel
                {
                    ClienteId = clientes[2].ClienteId, VeiculoId = veiculos[3].VeiculoId, FuncionarioId = funcionarios[0].FuncionarioId,
                    DataRetirada = hoje.AddDays(-2), DataPrevistaDevolucao = hoje.AddDays(5),
                    QuilometragemInicial = 12300,
                    ValorDiaria = 255.15m, ValorTotal = 1786.05m,
                    Status = StatusAluguel.EmAndamento, Observacao = "Contrato em andamento."
                },
                // Em andamento e em atraso
                new Aluguel
                {
                    ClienteId = clientes[1].ClienteId, VeiculoId = veiculos[5].VeiculoId, FuncionarioId = funcionarios[1].FuncionarioId,
                    DataRetirada = hoje.AddDays(-12), DataPrevistaDevolucao = hoje.AddDays(-4),
                    QuilometragemInicial = 15600,
                    ValorDiaria = 402.30m, ValorTotal = 3218.40m,
                    Status = StatusAluguel.EmAndamento, Observacao = "Cliente nao devolveu na data prevista."
                }
            };

            _context.Alugueis.AddRange(alugueis);

            // Os veiculos com contrato aberto ficam indisponiveis
            veiculos[3].Status = StatusVeiculo.Alugado;
            veiculos[5].Status = StatusVeiculo.Alugado;

            await _context.SaveChangesAsync();

            return StatusCode(StatusCodes.Status201Created, new
            {
                mensagem = "Carga de exemplo criada com sucesso.",
                funcionarios = funcionarios.Count,
                clientes = clientes.Count,
                veiculos = veiculos.Count,
                alugueis = alugueis.Count,
                observacao = "2 clientes ficaram sem aluguel e 3 veiculos nunca foram alugados, para demonstrar as consultas com LEFT JOIN."
            });
        }

        /// <summary>Remove alugueis, veiculos, clientes e funcionarios, preservando fabricantes e categorias.</summary>
        [HttpDelete("limpar")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Limpar()
        {
            var alugueis = await _context.Alugueis.ToListAsync();
            _context.Alugueis.RemoveRange(alugueis);
            await _context.SaveChangesAsync();

            var veiculos = await _context.Veiculos.ToListAsync();
            _context.Veiculos.RemoveRange(veiculos);

            var clientes = await _context.Clientes.ToListAsync();
            _context.Clientes.RemoveRange(clientes);

            var funcionarios = await _context.Funcionarios.ToListAsync();
            _context.Funcionarios.RemoveRange(funcionarios);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensagem = "Registros removidos com sucesso.",
                alugueisRemovidos = alugueis.Count,
                veiculosRemovidos = veiculos.Count,
                clientesRemovidos = clientes.Count,
                funcionariosRemovidos = funcionarios.Count
            });
        }
    }
}
