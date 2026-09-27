using System.ComponentModel.DataAnnotations;

namespace LocadoraVeiculos.DTOs
{
    /// <summary>Dados aceitos na abertura de um contrato de aluguel.</summary>
    public class AluguelRequest : IValidatableObject
    {
        [Required(ErrorMessage = "O cliente e obrigatorio.")]
        [Range(1, int.MaxValue, ErrorMessage = "Informe um cliente valido.")]
        public int ClienteId { get; set; }

        [Required(ErrorMessage = "O veiculo e obrigatorio.")]
        [Range(1, int.MaxValue, ErrorMessage = "Informe um veiculo valido.")]
        public int VeiculoId { get; set; }

        [Required(ErrorMessage = "O funcionario responsavel e obrigatorio.")]
        [Range(1, int.MaxValue, ErrorMessage = "Informe um funcionario valido.")]
        public int FuncionarioId { get; set; }

        [Required(ErrorMessage = "A data de retirada e obrigatoria.")]
        public DateTime DataRetirada { get; set; }

        [Required(ErrorMessage = "A data prevista de devolucao e obrigatoria.")]
        public DateTime DataPrevistaDevolucao { get; set; }

        [StringLength(300, ErrorMessage = "A observacao deve ter no maximo 300 caracteres.")]
        public string Observacao { get; set; }

        /// <summary>Validacao cruzada entre as datas informadas.</summary>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (DataPrevistaDevolucao.Date <= DataRetirada.Date)
            {
                yield return new ValidationResult(
                    "A data prevista de devolucao deve ser posterior a data de retirada.",
                    new[] { nameof(DataPrevistaDevolucao) });
            }
        }
    }

    /// <summary>Dados aceitos na atualizacao de um contrato em andamento.</summary>
    public class AluguelUpdateRequest
    {
        [Required(ErrorMessage = "A data prevista de devolucao e obrigatoria.")]
        public DateTime DataPrevistaDevolucao { get; set; }

        [StringLength(300, ErrorMessage = "A observacao deve ter no maximo 300 caracteres.")]
        public string Observacao { get; set; }
    }

    /// <summary>Dados aceitos no registro da devolucao do veiculo.</summary>
    public class DevolucaoRequest
    {
        [Required(ErrorMessage = "A data de devolucao e obrigatoria.")]
        public DateTime DataDevolucao { get; set; }

        [Required(ErrorMessage = "A quilometragem final e obrigatoria.")]
        [Range(0, int.MaxValue, ErrorMessage = "A quilometragem final nao pode ser negativa.")]
        public int QuilometragemFinal { get; set; }

        [StringLength(300, ErrorMessage = "A observacao deve ter no maximo 300 caracteres.")]
        public string Observacao { get; set; }
    }

    /// <summary>Dados devolvidos pelas rotas de aluguel.</summary>
    public class AluguelResponse
    {
        public int AluguelId { get; set; }
        public int ClienteId { get; set; }
        public string Cliente { get; set; }
        public string CpfCliente { get; set; }
        public int VeiculoId { get; set; }
        public string Veiculo { get; set; }
        public string Placa { get; set; }
        public int FuncionarioId { get; set; }
        public string Funcionario { get; set; }
        public DateTime DataRetirada { get; set; }
        public DateTime DataPrevistaDevolucao { get; set; }
        public DateTime? DataDevolucao { get; set; }
        public int QuilometragemInicial { get; set; }
        public int? QuilometragemFinal { get; set; }
        public int? QuilometragemPercorrida { get; set; }
        public int QuantidadeDiarias { get; set; }
        public decimal ValorDiaria { get; set; }
        public decimal ValorTotal { get; set; }
        public string Status { get; set; }
        public string Observacao { get; set; }
    }
}
