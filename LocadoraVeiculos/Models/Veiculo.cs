using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LocadoraVeiculos.Models
{
    /// <summary>
    /// Veiculo da frota. Todo veiculo pertence a um fabricante e a uma categoria,
    /// e registra modelo, ano de fabricacao e quilometragem atual.
    /// </summary>
    [Table("Veiculo")]
    public class Veiculo
    {
        // Chave primaria
        [Key]
        public int VeiculoId { get; set; }

        [Required]
        [StringLength(8)]
        public string Placa { get; set; }

        [Required]
        [StringLength(80)]
        public string Modelo { get; set; }

        [Required]
        public int AnoFabricacao { get; set; }

        /// <summary>
        /// Quilometragem atual do veiculo, atualizada a cada devolucao.
        /// </summary>
        [Required]
        public int Quilometragem { get; set; }

        [StringLength(30)]
        public string Cor { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal ValorDiaria { get; set; }

        public StatusVeiculo Status { get; set; } = StatusVeiculo.Disponivel;

        // Chave estrangeira -> Fabricante
        [Required]
        public int FabricanteId { get; set; }

        [ForeignKey(nameof(FabricanteId))]
        public Fabricante Fabricante { get; set; }

        // Chave estrangeira -> Categoria
        [Required]
        public int CategoriaId { get; set; }

        [ForeignKey(nameof(CategoriaId))]
        public Categoria Categoria { get; set; }

        // Propriedade de navegacao: um veiculo pode ter varios alugueis ao longo do tempo
        public ICollection<Aluguel> Alugueis { get; set; } = new List<Aluguel>();
    }
}
