using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LocadoraVeiculos.Models
{
    /// <summary>
    /// Categoria de locacao do veiculo (Economico, Intermediario, SUV, Luxo...).
    /// Quinta entidade do modelo, usada para classificar a frota e ajustar o valor da diaria.
    /// Relacionamento 1:N com Veiculo.
    /// </summary>
    [Table("Categoria")]
    public class Categoria
    {
        // Chave primaria
        [Key]
        public int CategoriaId { get; set; }

        [Required]
        [StringLength(50)]
        public string Nome { get; set; }

        [StringLength(200)]
        public string Descricao { get; set; }

        /// <summary>
        /// Percentual aplicado sobre o valor base da diaria dos veiculos da categoria.
        /// </summary>
        [Column(TypeName = "decimal(5,2)")]
        public decimal PercentualAjuste { get; set; }

        // Propriedade de navegacao: uma categoria agrupa varios veiculos
        public ICollection<Veiculo> Veiculos { get; set; } = new List<Veiculo>();
    }
}
