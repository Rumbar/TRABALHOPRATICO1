using System.ComponentModel.DataAnnotations;

namespace LocadoraVeiculos.DTOs
{
    /// <summary>Dados aceitos na criacao e na atualizacao de um cliente.</summary>
    public class ClienteRequest
    {
        [Required(ErrorMessage = "O nome do cliente e obrigatorio.")]
        [StringLength(120, MinimumLength = 3, ErrorMessage = "O nome deve ter entre 3 e 120 caracteres.")]
        public string Nome { get; set; }

        [Required(ErrorMessage = "O CPF e obrigatorio.")]
        [RegularExpression(@"^\d{11}$", ErrorMessage = "O CPF deve conter exatamente 11 digitos, sem pontos ou tracos.")]
        public string Cpf { get; set; }

        [Required(ErrorMessage = "O e-mail e obrigatorio.")]
        [EmailAddress(ErrorMessage = "Informe um e-mail valido.")]
        [StringLength(120, ErrorMessage = "O e-mail deve ter no maximo 120 caracteres.")]
        public string Email { get; set; }

        [StringLength(20, ErrorMessage = "O telefone deve ter no maximo 20 caracteres.")]
        public string Telefone { get; set; }

        public DateTime? DataNascimento { get; set; }

        [RegularExpression(@"^\d{11}$", ErrorMessage = "O numero da CNH deve conter 11 digitos.")]
        public string NumeroCnh { get; set; }

        [StringLength(3, ErrorMessage = "A categoria da CNH deve ter no maximo 3 caracteres.")]
        public string CategoriaCnh { get; set; }
    }

    /// <summary>Dados devolvidos pelas rotas de cliente.</summary>
    public class ClienteResponse
    {
        public int ClienteId { get; set; }
        public string Nome { get; set; }
        public string Cpf { get; set; }
        public string Email { get; set; }
        public string Telefone { get; set; }
        public DateTime? DataNascimento { get; set; }
        public string NumeroCnh { get; set; }
        public string CategoriaCnh { get; set; }
        public DateTime DataCadastro { get; set; }
        public int QuantidadeAlugueis { get; set; }
    }
}
