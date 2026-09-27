using LocadoraVeiculos.Data;
using LocadoraVeiculos.DTOs;
using LocadoraVeiculos.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LocadoraVeiculos.Controllers
{
    /// <summary>Operacoes de CRUD sobre os clientes da locadora.</summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ClientesController : ControllerBase
    {
        private readonly ApplicationContext _context;

        public ClientesController(ApplicationContext context)
        {
            _context = context;
        }

        /// <summary>Lista os clientes cadastrados.</summary>
        /// <param name="nome">Filtro opcional por parte do nome.</param>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ClienteResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ClienteResponse>>> Get([FromQuery] string nome)
        {
            var consulta = _context.Clientes.AsQueryable();

            if (!string.IsNullOrWhiteSpace(nome))
                consulta = consulta.Where(c => c.Nome.Contains(nome));

            var clientes = await consulta
                .OrderBy(c => c.Nome)
                .Select(c => new ClienteResponse
                {
                    ClienteId = c.ClienteId,
                    Nome = c.Nome,
                    Cpf = c.Cpf,
                    Email = c.Email,
                    Telefone = c.Telefone,
                    DataNascimento = c.DataNascimento,
                    NumeroCnh = c.NumeroCnh,
                    CategoriaCnh = c.CategoriaCnh,
                    DataCadastro = c.DataCadastro,
                    QuantidadeAlugueis = c.Alugueis.Count()
                })
                .ToListAsync();

            return Ok(clientes);
        }

        /// <summary>Busca um cliente pelo seu identificador.</summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ClienteResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ClienteResponse>> GetPorId(int id)
        {
            var cliente = await _context.Clientes
                .Where(c => c.ClienteId == id)
                .Select(c => new ClienteResponse
                {
                    ClienteId = c.ClienteId,
                    Nome = c.Nome,
                    Cpf = c.Cpf,
                    Email = c.Email,
                    Telefone = c.Telefone,
                    DataNascimento = c.DataNascimento,
                    NumeroCnh = c.NumeroCnh,
                    CategoriaCnh = c.CategoriaCnh,
                    DataCadastro = c.DataCadastro,
                    QuantidadeAlugueis = c.Alugueis.Count()
                })
                .FirstOrDefaultAsync();

            if (cliente is null)
                return NotFound(new { mensagem = $"Nenhum cliente encontrado com o id {id}." });

            return Ok(cliente);
        }

        /// <summary>Busca um cliente pelo CPF.</summary>
        [HttpGet("cpf/{cpf}")]
        [ProducesResponseType(typeof(ClienteResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ClienteResponse>> GetPorCpf(string cpf)
        {
            var cliente = await _context.Clientes
                .Where(c => c.Cpf == cpf)
                .Select(c => new ClienteResponse
                {
                    ClienteId = c.ClienteId,
                    Nome = c.Nome,
                    Cpf = c.Cpf,
                    Email = c.Email,
                    Telefone = c.Telefone,
                    DataNascimento = c.DataNascimento,
                    NumeroCnh = c.NumeroCnh,
                    CategoriaCnh = c.CategoriaCnh,
                    DataCadastro = c.DataCadastro,
                    QuantidadeAlugueis = c.Alugueis.Count()
                })
                .FirstOrDefaultAsync();

            if (cliente is null)
                return NotFound(new { mensagem = $"Nenhum cliente encontrado com o CPF {cpf}." });

            return Ok(cliente);
        }

        /// <summary>Cadastra um novo cliente.</summary>
        [HttpPost]
        [ProducesResponseType(typeof(ClienteResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ClienteResponse>> Post([FromBody] ClienteRequest request)
        {
            if (await _context.Clientes.AnyAsync(c => c.Cpf == request.Cpf))
                return Conflict(new { mensagem = $"Ja existe um cliente cadastrado com o CPF {request.Cpf}." });

            if (await _context.Clientes.AnyAsync(c => c.Email.ToLower() == request.Email.ToLower()))
                return Conflict(new { mensagem = $"Ja existe um cliente cadastrado com o e-mail {request.Email}." });

            if (request.DataNascimento.HasValue && request.DataNascimento.Value.Date > DateTime.Today)
                return BadRequest(new { mensagem = "A data de nascimento nao pode ser futura." });

            var cliente = new Cliente
            {
                Nome = request.Nome.Trim(),
                Cpf = request.Cpf,
                Email = request.Email.Trim().ToLower(),
                Telefone = request.Telefone?.Trim(),
                DataNascimento = request.DataNascimento?.Date,
                NumeroCnh = request.NumeroCnh,
                CategoriaCnh = request.CategoriaCnh?.Trim().ToUpper(),
                DataCadastro = DateTime.Today
            };

            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync();

            var resposta = new ClienteResponse
            {
                ClienteId = cliente.ClienteId,
                Nome = cliente.Nome,
                Cpf = cliente.Cpf,
                Email = cliente.Email,
                Telefone = cliente.Telefone,
                DataNascimento = cliente.DataNascimento,
                NumeroCnh = cliente.NumeroCnh,
                CategoriaCnh = cliente.CategoriaCnh,
                DataCadastro = cliente.DataCadastro,
                QuantidadeAlugueis = 0
            };

            return CreatedAtAction(nameof(GetPorId), new { id = cliente.ClienteId }, resposta);
        }

        /// <summary>Atualiza os dados de um cliente existente.</summary>
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Put(int id, [FromBody] ClienteRequest request)
        {
            var cliente = await _context.Clientes.FindAsync(id);

            if (cliente is null)
                return NotFound(new { mensagem = $"Nenhum cliente encontrado com o id {id}." });

            if (await _context.Clientes.AnyAsync(c => c.ClienteId != id && c.Cpf == request.Cpf))
                return Conflict(new { mensagem = $"O CPF {request.Cpf} ja pertence a outro cliente." });

            if (await _context.Clientes.AnyAsync(c => c.ClienteId != id && c.Email.ToLower() == request.Email.ToLower()))
                return Conflict(new { mensagem = $"O e-mail {request.Email} ja pertence a outro cliente." });

            cliente.Nome = request.Nome.Trim();
            cliente.Cpf = request.Cpf;
            cliente.Email = request.Email.Trim().ToLower();
            cliente.Telefone = request.Telefone?.Trim();
            cliente.DataNascimento = request.DataNascimento?.Date;
            cliente.NumeroCnh = request.NumeroCnh;
            cliente.CategoriaCnh = request.CategoriaCnh?.Trim().ToUpper();

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensagem = "Cliente atualizado com sucesso.",
                cliente = new ClienteResponse
                {
                    ClienteId = cliente.ClienteId,
                    Nome = cliente.Nome,
                    Cpf = cliente.Cpf,
                    Email = cliente.Email,
                    Telefone = cliente.Telefone,
                    DataNascimento = cliente.DataNascimento,
                    NumeroCnh = cliente.NumeroCnh,
                    CategoriaCnh = cliente.CategoriaCnh,
                    DataCadastro = cliente.DataCadastro
                }
            });
        }

        /// <summary>Exclui um cliente que nao possua alugueis registrados.</summary>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Delete(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);

            if (cliente is null)
                return NotFound(new { mensagem = $"Nenhum cliente encontrado com o id {id}." });

            var possuiAlugueis = await _context.Alugueis.AnyAsync(a => a.ClienteId == id);

            if (possuiAlugueis)
                return Conflict(new { mensagem = "Nao e possivel excluir: existem alugueis registrados para este cliente." });

            _context.Clientes.Remove(cliente);
            await _context.SaveChangesAsync();

            return Ok(new { mensagem = $"Cliente {cliente.Nome} excluido com sucesso." });
        }
    }
}
