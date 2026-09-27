using LocadoraVeiculos.Data;
using LocadoraVeiculos.DTOs;
using LocadoraVeiculos.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LocadoraVeiculos.Controllers
{
    /// <summary>Operacoes de CRUD sobre os fabricantes (marcas) dos veiculos.</summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class FabricantesController : ControllerBase
    {
        private readonly ApplicationContext _context;

        public FabricantesController(ApplicationContext context)
        {
            _context = context;
        }

        /// <summary>Lista todos os fabricantes cadastrados.</summary>
        /// <param name="nome">Filtro opcional por parte do nome do fabricante.</param>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<FabricanteResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<FabricanteResponse>>> Get([FromQuery] string nome)
        {
            var consulta = _context.Fabricantes.AsQueryable();

            if (!string.IsNullOrWhiteSpace(nome))
                consulta = consulta.Where(f => f.Nome.Contains(nome));

            var fabricantes = await consulta
                .OrderBy(f => f.Nome)
                .Select(f => new FabricanteResponse
                {
                    FabricanteId = f.FabricanteId,
                    Nome = f.Nome,
                    PaisOrigem = f.PaisOrigem,
                    AnoFundacao = f.AnoFundacao,
                    QuantidadeVeiculos = f.Veiculos.Count()
                })
                .ToListAsync();

            return Ok(fabricantes);
        }

        /// <summary>Busca um fabricante pelo seu identificador.</summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(FabricanteResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<FabricanteResponse>> GetPorId(int id)
        {
            var fabricante = await _context.Fabricantes
                .Where(f => f.FabricanteId == id)
                .Select(f => new FabricanteResponse
                {
                    FabricanteId = f.FabricanteId,
                    Nome = f.Nome,
                    PaisOrigem = f.PaisOrigem,
                    AnoFundacao = f.AnoFundacao,
                    QuantidadeVeiculos = f.Veiculos.Count()
                })
                .FirstOrDefaultAsync();

            if (fabricante is null)
                return NotFound(new { mensagem = $"Nenhum fabricante encontrado com o id {id}." });

            return Ok(fabricante);
        }

        /// <summary>Cadastra um novo fabricante.</summary>
        [HttpPost]
        [ProducesResponseType(typeof(FabricanteResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<FabricanteResponse>> Post([FromBody] FabricanteRequest request)
        {
            var nomeEmUso = await _context.Fabricantes
                .AnyAsync(f => f.Nome.ToLower() == request.Nome.ToLower());

            if (nomeEmUso)
                return Conflict(new { mensagem = $"Ja existe um fabricante cadastrado com o nome {request.Nome}." });

            var fabricante = new Fabricante
            {
                Nome = request.Nome.Trim(),
                PaisOrigem = request.PaisOrigem?.Trim(),
                AnoFundacao = request.AnoFundacao
            };

            _context.Fabricantes.Add(fabricante);
            await _context.SaveChangesAsync();

            var resposta = new FabricanteResponse
            {
                FabricanteId = fabricante.FabricanteId,
                Nome = fabricante.Nome,
                PaisOrigem = fabricante.PaisOrigem,
                AnoFundacao = fabricante.AnoFundacao,
                QuantidadeVeiculos = 0
            };

            return CreatedAtAction(nameof(GetPorId), new { id = fabricante.FabricanteId }, resposta);
        }

        /// <summary>Atualiza os dados de um fabricante existente.</summary>
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Put(int id, [FromBody] FabricanteRequest request)
        {
            var fabricante = await _context.Fabricantes.FindAsync(id);

            if (fabricante is null)
                return NotFound(new { mensagem = $"Nenhum fabricante encontrado com o id {id}." });

            var nomeEmUso = await _context.Fabricantes
                .AnyAsync(f => f.FabricanteId != id && f.Nome.ToLower() == request.Nome.ToLower());

            if (nomeEmUso)
                return Conflict(new { mensagem = $"Ja existe outro fabricante cadastrado com o nome {request.Nome}." });

            fabricante.Nome = request.Nome.Trim();
            fabricante.PaisOrigem = request.PaisOrigem?.Trim();
            fabricante.AnoFundacao = request.AnoFundacao;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensagem = "Fabricante atualizado com sucesso.",
                fabricante = new FabricanteResponse
                {
                    FabricanteId = fabricante.FabricanteId,
                    Nome = fabricante.Nome,
                    PaisOrigem = fabricante.PaisOrigem,
                    AnoFundacao = fabricante.AnoFundacao
                }
            });
        }

        /// <summary>Exclui um fabricante que nao possua veiculos vinculados.</summary>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Delete(int id)
        {
            var fabricante = await _context.Fabricantes.FindAsync(id);

            if (fabricante is null)
                return NotFound(new { mensagem = $"Nenhum fabricante encontrado com o id {id}." });

            var possuiVeiculos = await _context.Veiculos.AnyAsync(v => v.FabricanteId == id);

            if (possuiVeiculos)
                return Conflict(new { mensagem = "Nao e possivel excluir: existem veiculos vinculados a este fabricante." });

            _context.Fabricantes.Remove(fabricante);
            await _context.SaveChangesAsync();

            return Ok(new { mensagem = $"Fabricante {fabricante.Nome} excluido com sucesso." });
        }
    }
}
