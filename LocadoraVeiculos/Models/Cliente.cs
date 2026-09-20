using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LocadoraVeiculos.Models
{
    /// <summary>
    /// Cliente da locadora. Possui no minimo nome, CPF e e-mail.
    /// Relacionamento 1:N com Aluguel.
    /// </summary>
    [Table("Cliente")]
    public class Cliente
    {
        // Chave primaria
        [Key]
        public int ClienteId { get; set; }

        [Required]
        [StringLength(120)]
        public string Nome { get; set; }

        [Required]
        [StringLength(11)]
        public string Cpf { get; set; }

        [Required]
        [StringLength(120)]
        [EmailAddress]
        public string Email { get; set; }

        [StringLength(20)]
        public string Telefone { get; set; }

        [Column(TypeName = "date")]
        public DateTime? DataNascimento { get; set; }

        [StringLength(11)]
        public string NumeroCnh { get; set; }

        [StringLength(3)]
        public string CategoriaCnh { get; set; }

        [Column(TypeName = "date")]
        public DateTime DataCadastro { get; set; } = DateTime.Today;

        // Propriedade de navegacao: um cliente pode realizar varios alugueis
        public ICollection<Aluguel> Alugueis { get; set; } = new List<Aluguel>();
    }
}
