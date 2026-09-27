using LocadoraVeiculos.Data;
using LocadoraVeiculos.DTOs;
using LocadoraVeiculos.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LocadoraVeiculos.Controllers
{
    /// <summary>Operacoes de CRUD sobre as categorias de locacao.</summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class CategoriasController : ControllerBase
    {
        private readonly ApplicationContext _context;

        public CategoriasController(ApplicationContext context)
        {
            _context = context;
        }

        /// <summary>Lista todas as categorias cadastradas.</summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<CategoriaResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<CategoriaResponse>>> Get()
        {
            var categorias = await _context.Categorias
                .OrderBy(c => c.Nome)
                .Select(c => new CategoriaResponse
                {
                    CategoriaId = c.CategoriaId,
                    Nome = c.Nome,
                    Descricao = c.Descricao,
                    PercentualAjuste = c.PercentualAjuste,
                    QuantidadeVeiculos = c.Veiculos.Count()
                })
                .ToListAsync();

            return Ok(categorias);
        }

        /// <summary>Busca uma categoria pelo seu identificador.</summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(CategoriaResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CategoriaResponse>> GetPorId(int id)
        {
            var categoria = await _context.Categorias
                .Where(c => c.CategoriaId == id)
                .Select(c => new CategoriaResponse
                {
                    CategoriaId = c.CategoriaId,
                    Nome = c.Nome,
                    Descricao = c.Descricao,
                    PercentualAjuste = c.PercentualAjuste,
                    QuantidadeVeiculos = c.Veiculos.Count()
                })
                .FirstOrDefaultAsync();

            if (categoria is null)
                return NotFound(new { mensagem = $"Nenhuma categoria encontrada com o id {id}." });

            return Ok(categoria);
        }

        /// <summary>Cadastra uma nova categoria.</summary>
        [HttpPost]
        [ProducesResponseType(typeof(CategoriaResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<CategoriaResponse>> Post([FromBody] CategoriaRequest request)
        {
            var nomeEmUso = await _context.Categorias
                .AnyAsync(c => c.Nome.ToLower() == request.Nome.ToLower());

            if (nomeEmUso)
                return Conflict(new { mensagem = $"Ja existe uma categoria cadastrada com o nome {request.Nome}." });

            var categoria = new Categoria
            {
                Nome = request.Nome.Trim(),
                Descricao = request.Descricao?.Trim(),
                PercentualAjuste = request.PercentualAjuste
            };

            _context.Categorias.Add(categoria);
            await _context.SaveChangesAsync();

            var resposta = new CategoriaResponse
            {
                CategoriaId = categoria.CategoriaId,
                Nome = categoria.Nome,
                Descricao = categoria.Descricao,
                PercentualAjuste = categoria.PercentualAjuste,
                QuantidadeVeiculos = 0
            };

            return CreatedAtAction(nameof(GetPorId), new { id = categoria.CategoriaId }, resposta);
        }

        /// <summary>Atualiza os dados de uma categoria existente.</summary>
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Put(int id, [FromBody] CategoriaRequest request)
        {
            var categoria = await _context.Categorias.FindAsync(id);

            if (categoria is null)
                return NotFound(new { mensagem = $"Nenhuma categoria encontrada com o id {id}." });

            var nomeEmUso = await _context.Categorias
                .AnyAsync(c => c.CategoriaId != id && c.Nome.ToLower() == request.Nome.ToLower());

            if (nomeEmUso)
                return Conflict(new { mensagem = $"Ja existe outra categoria cadastrada com o nome {request.Nome}." });

            categoria.Nome = request.Nome.Trim();
            categoria.Descricao = request.Descricao?.Trim();
            categoria.PercentualAjuste = request.PercentualAjuste;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensagem = "Categoria atualizada com sucesso.",
                categoria = new CategoriaResponse
                {
                    CategoriaId = categoria.CategoriaId,
                    Nome = categoria.Nome,
                    Descricao = categoria.Descricao,
                    PercentualAjuste = categoria.PercentualAjuste
                }
            });
        }

        /// <summary>Exclui uma categoria que nao possua veiculos vinculados.</summary>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Delete(int id)
        {
            var categoria = await _context.Categorias.FindAsync(id);

            if (categoria is null)
                return NotFound(new { mensagem = $"Nenhuma categoria encontrada com o id {id}." });

            var possuiVeiculos = await _context.Veiculos.AnyAsync(v => v.CategoriaId == id);

            if (possuiVeiculos)
                return Conflict(new { mensagem = "Nao e possivel excluir: existem veiculos vinculados a esta categoria." });

            _context.Categorias.Remove(categoria);
            await _context.SaveChangesAsync();

            return Ok(new { mensagem = $"Categoria {categoria.Nome} excluida com sucesso." });
        }
    }
}
