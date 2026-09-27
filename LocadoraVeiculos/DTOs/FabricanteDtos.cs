using System.ComponentModel.DataAnnotations;

namespace LocadoraVeiculos.DTOs
{
    /// <summary>Dados aceitos na criacao e na atualizacao de um fabricante.</summary>
    public class FabricanteRequest
    {
        [Required(ErrorMessage = "O nome do fabricante e obrigatorio.")]
        [StringLength(80, MinimumLength = 2, ErrorMessage = "O nome deve ter entre 2 e 80 caracteres.")]
        public string Nome { get; set; }

        [StringLength(60, ErrorMessage = "O pais de origem deve ter no maximo 60 caracteres.")]
        public string PaisOrigem { get; set; }

        [Range(1800, 2100, ErrorMessage = "O ano de fundacao deve estar entre 1800 e 2100.")]
        public int? AnoFundacao { get; set; }
    }

    /// <summary>Dados devolvidos pelas rotas de fabricante.</summary>
    public class FabricanteResponse
    {
        public int FabricanteId { get; set; }
        public string Nome { get; set; }
        public string PaisOrigem { get; set; }
        public int? AnoFundacao { get; set; }
        public int QuantidadeVeiculos { get; set; }
    }
}
