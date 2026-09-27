using System.ComponentModel.DataAnnotations;

namespace LocadoraVeiculos.DTOs
{
    /// <summary>Dados aceitos na criacao e na atualizacao de uma categoria.</summary>
    public class CategoriaRequest
    {
        [Required(ErrorMessage = "O nome da categoria e obrigatorio.")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "O nome deve ter entre 2 e 50 caracteres.")]
        public string Nome { get; set; }

        [StringLength(200, ErrorMessage = "A descricao deve ter no maximo 200 caracteres.")]
        public string Descricao { get; set; }

        [Range(0, 999.99, ErrorMessage = "O percentual de ajuste deve estar entre 0 e 999,99.")]
        public decimal PercentualAjuste { get; set; }
    }

    /// <summary>Dados devolvidos pelas rotas de categoria.</summary>
    public class CategoriaResponse
    {
        public int CategoriaId { get; set; }
        public string Nome { get; set; }
        public string Descricao { get; set; }
        public decimal PercentualAjuste { get; set; }
        public int QuantidadeVeiculos { get; set; }
    }
}
