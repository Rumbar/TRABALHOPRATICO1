using System.ComponentModel.DataAnnotations;

namespace LocadoraVeiculos.DTOs
{
    /// <summary>Dados aceitos na criacao e na atualizacao de um funcionario.</summary>
    public class FuncionarioRequest
    {
        [Required(ErrorMessage = "O nome do funcionario e obrigatorio.")]
        [StringLength(120, MinimumLength = 3, ErrorMessage = "O nome deve ter entre 3 e 120 caracteres.")]
        public string Nome { get; set; }

        [Required(ErrorMessage = "O CPF e obrigatorio.")]
        [RegularExpression(@"^\d{11}$", ErrorMessage = "O CPF deve conter exatamente 11 digitos, sem pontos ou tracos.")]
        public string Cpf { get; set; }

        [Required(ErrorMessage = "A matricula e obrigatoria.")]
        [StringLength(20, MinimumLength = 2, ErrorMessage = "A matricula deve ter entre 2 e 20 caracteres.")]
        public string Matricula { get; set; }

        [StringLength(60, ErrorMessage = "O cargo deve ter no maximo 60 caracteres.")]
        public string Cargo { get; set; }

        [Required(ErrorMessage = "A data de admissao e obrigatoria.")]
        public DateTime DataAdmissao { get; set; }
    }

    /// <summary>Dados devolvidos pelas rotas de funcionario.</summary>
    public class FuncionarioResponse
    {
        public int FuncionarioId { get; set; }
        public string Nome { get; set; }
        public string Cpf { get; set; }
        public string Matricula { get; set; }
        public string Cargo { get; set; }
        public DateTime DataAdmissao { get; set; }
        public int QuantidadeAlugueis { get; set; }
    }
}
