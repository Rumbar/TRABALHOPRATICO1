using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LocadoraVeiculos.Models
{
    /// <summary>
    /// Contrato de aluguel. Vincula um cliente, um veiculo e o funcionario responsavel
    /// em um periodo de tempo, registrando a devolucao, a quilometragem inicial e final,
    /// o valor da diaria e o valor total da locacao.
    /// </summary>
    [Table("Aluguel")]
    public class Aluguel
    {
        // Chave primaria
        [Key]
        public int AluguelId { get; set; }

        // Chave estrangeira -> Cliente
        [Required]
        public int ClienteId { get; set; }

        [ForeignKey(nameof(ClienteId))]
        public Cliente Cliente { get; set; }

        // Chave estrangeira -> Veiculo
        [Required]
        public int VeiculoId { get; set; }

        [ForeignKey(nameof(VeiculoId))]
        public Veiculo Veiculo { get; set; }

        // Chave estrangeira -> Funcionario responsavel pelo contrato
        [Required]
        public int FuncionarioId { get; set; }

        [ForeignKey(nameof(FuncionarioId))]
        public Funcionario Funcionario { get; set; }

        /// <summary>Data em que o veiculo foi retirado pelo cliente.</summary>
        [Required]
        public DateTime DataRetirada { get; set; }

        /// <summary>Data combinada para a devolucao do veiculo.</summary>
        [Required]
        public DateTime DataPrevistaDevolucao { get; set; }

        /// <summary>Data efetiva da devolucao. Nula enquanto o aluguel estiver em andamento.</summary>
        public DateTime? DataDevolucao { get; set; }

        /// <summary>Quilometragem do veiculo no momento da retirada.</summary>
        [Required]
        public int QuilometragemInicial { get; set; }

        /// <summary>Quilometragem do veiculo no momento da devolucao.</summary>
        public int? QuilometragemFinal { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal ValorDiaria { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal ValorTotal { get; set; }

        public StatusAluguel Status { get; set; } = StatusAluguel.EmAndamento;

        [StringLength(300)]
        public string Observacao { get; set; }

        /// <summary>Quantidade de diarias contratadas (nao persistida no banco).</summary>
        [NotMapped]
        public int QuantidadeDiarias =>
            Math.Max(1, (DataPrevistaDevolucao.Date - DataRetirada.Date).Days);

        /// <summary>Distancia percorrida durante a locacao (nao persistida no banco).</summary>
        [NotMapped]
        public int? QuilometragemPercorrida =>
            QuilometragemFinal.HasValue ? QuilometragemFinal - QuilometragemInicial : null;
    }
}
