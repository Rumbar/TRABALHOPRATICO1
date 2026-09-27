using LocadoraVeiculos.Data;
using LocadoraVeiculos.DTOs;
using LocadoraVeiculos.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LocadoraVeiculos.Controllers
{
    /// <summary>Operacoes de CRUD sobre os funcionarios da locadora.</summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class FuncionariosController : ControllerBase
    {
        private readonly ApplicationContext _context;

        public FuncionariosController(ApplicationContext context)
        {
            _context = context;
        }

        /// <summary>Lista os funcionarios cadastrados.</summary>
        /// <param name="cargo">Filtro opcional por cargo.</param>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<FuncionarioResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<FuncionarioResponse>>> Get([FromQuery] string cargo)
        {
            var consulta = _context.Funcionarios.AsQueryable();

            if (!string.IsNullOrWhiteSpace(cargo))
                consulta = consulta.Where(f => f.Cargo.Contains(cargo));

            var funcionarios = await consulta
                .OrderBy(f => f.Nome)
                .Select(f => new FuncionarioResponse
                {
                    FuncionarioId = f.FuncionarioId,
                    Nome = f.Nome,
                    Cpf = f.Cpf,
                    Matricula = f.Matricula,
                    Cargo = f.Cargo,
                    DataAdmissao = f.DataAdmissao,
                    QuantidadeAlugueis = f.Alugueis.Count()
                })
                .ToListAsync();

            return Ok(funcionarios);
        }

        /// <summary>Busca um funcionario pelo seu identificador.</summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(FuncionarioResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<FuncionarioResponse>> GetPorId(int id)
        {
            var funcionario = await _context.Funcionarios
                .Where(f => f.FuncionarioId == id)
                .Select(f => new FuncionarioResponse
                {
                    FuncionarioId = f.FuncionarioId,
                    Nome = f.Nome,
                    Cpf = f.Cpf,
                    Matricula = f.Matricula,
                    Cargo = f.Cargo,
                    DataAdmissao = f.DataAdmissao,
                    QuantidadeAlugueis = f.Alugueis.Count()
                })
                .FirstOrDefaultAsync();

            if (funcionario is null)
                return NotFound(new { mensagem = $"Nenhum funcionario encontrado com o id {id}." });

            return Ok(funcionario);
        }

        /// <summary>Cadastra um novo funcionario.</summary>
        [HttpPost]
        [ProducesResponseType(typeof(FuncionarioResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<FuncionarioResponse>> Post([FromBody] FuncionarioRequest request)
        {
            if (await _context.Funcionarios.AnyAsync(f => f.Cpf == request.Cpf))
                return Conflict(new { mensagem = $"Ja existe um funcionario cadastrado com o CPF {request.Cpf}." });

            if (await _context.Funcionarios.AnyAsync(f => f.Matricula == request.Matricula))
                return Conflict(new { mensagem = $"Ja existe um funcionario com a matricula {request.Matricula}." });

            var funcionario = new Funcionario
            {
                Nome = request.Nome.Trim(),
                Cpf = request.Cpf,
                Matricula = request.Matricula.Trim(),
                Cargo = request.Cargo?.Trim(),
                DataAdmissao = request.DataAdmissao.Date
            };

            _context.Funcionarios.Add(funcionario);
            await _context.SaveChangesAsync();

            var resposta = new FuncionarioResponse
            {
                FuncionarioId = funcionario.FuncionarioId,
                Nome = funcionario.Nome,
                Cpf = funcionario.Cpf,
                Matricula = funcionario.Matricula,
                Cargo = funcionario.Cargo,
                DataAdmissao = funcionario.DataAdmissao,
                QuantidadeAlugueis = 0
            };

            return CreatedAtAction(nameof(GetPorId), new { id = funcionario.FuncionarioId }, resposta);
        }

        /// <summary>Atualiza os dados de um funcionario existente.</summary>
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Put(int id, [FromBody] FuncionarioRequest request)
        {
            var funcionario = await _context.Funcionarios.FindAsync(id);

            if (funcionario is null)
                return NotFound(new { mensagem = $"Nenhum funcionario encontrado com o id {id}." });

            if (await _context.Funcionarios.AnyAsync(f => f.FuncionarioId != id && f.Cpf == request.Cpf))
                return Conflict(new { mensagem = $"O CPF {request.Cpf} ja pertence a outro funcionario." });

            if (await _context.Funcionarios.AnyAsync(f => f.FuncionarioId != id && f.Matricula == request.Matricula))
                return Conflict(new { mensagem = $"A matricula {request.Matricula} ja pertence a outro funcionario." });

            funcionario.Nome = request.Nome.Trim();
            funcionario.Cpf = request.Cpf;
            funcionario.Matricula = request.Matricula.Trim();
            funcionario.Cargo = request.Cargo?.Trim();
            funcionario.DataAdmissao = request.DataAdmissao.Date;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensagem = "Funcionario atualizado com sucesso.",
                funcionario = new FuncionarioResponse
                {
                    FuncionarioId = funcionario.FuncionarioId,
                    Nome = funcionario.Nome,
                    Cpf = funcionario.Cpf,
                    Matricula = funcionario.Matricula,
                    Cargo = funcionario.Cargo,
                    DataAdmissao = funcionario.DataAdmissao
                }
            });
        }

        /// <summary>Exclui um funcionario que nao possua alugueis registrados.</summary>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Delete(int id)
        {
            var funcionario = await _context.Funcionarios.FindAsync(id);

            if (funcionario is null)
                return NotFound(new { mensagem = $"Nenhum funcionario encontrado com o id {id}." });

            var possuiAlugueis = await _context.Alugueis.AnyAsync(a => a.FuncionarioId == id);

            if (possuiAlugueis)
                return Conflict(new { mensagem = "Nao e possivel excluir: existem alugueis registrados por este funcionario." });

            _context.Funcionarios.Remove(funcionario);
            await _context.SaveChangesAsync();

            return Ok(new { mensagem = $"Funcionario {funcionario.Nome} excluido com sucesso." });
        }
    }
}
