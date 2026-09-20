using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LocadoraVeiculos.Models
{
    /// <summary>
    /// Funcionario da locadora responsavel por registrar o contrato de aluguel.
    /// Sexta entidade do modelo. Relacionamento 1:N com Aluguel.
    /// </summary>
    [Table("Funcionario")]
    public class Funcionario
    {
        // Chave primaria
        [Key]
        public int FuncionarioId { get; set; }

        [Required]
        [StringLength(120)]
        public string Nome { get; set; }

        [Required]
        [StringLength(11)]
        public string Cpf { get; set; }

        [Required]
        [StringLength(20)]
        public string Matricula { get; set; }

        [StringLength(60)]
        public string Cargo { get; set; }

        [Column(TypeName = "date")]
        public DateTime DataAdmissao { get; set; }

        // Propriedade de navegacao: um funcionario registra varios alugueis
        public ICollection<Aluguel> Alugueis { get; set; } = new List<Aluguel>();
    }
}
