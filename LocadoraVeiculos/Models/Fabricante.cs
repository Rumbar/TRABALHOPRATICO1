using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LocadoraVeiculos.Models
{
    /// <summary>
    /// Fabricante (marca) dos veiculos da frota.
    /// Relacionamento 1:N com Veiculo.
    /// </summary>
    [Table("Fabricante")]
    public class Fabricante
    {
        // Chave primaria
        [Key]
        public int FabricanteId { get; set; }

        [Required]
        [StringLength(80)]
        public string Nome { get; set; }

        [StringLength(60)]
        public string PaisOrigem { get; set; }

        public int? AnoFundacao { get; set; }

        // Propriedade de navegacao: um fabricante possui varios veiculos
        public ICollection<Veiculo> Veiculos { get; set; } = new List<Veiculo>();
    }
}
