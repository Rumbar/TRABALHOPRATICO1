using System.ComponentModel.DataAnnotations;
using LocadoraVeiculos.Models;

namespace LocadoraVeiculos.DTOs
{
    /// <summary>Dados aceitos na criacao e na atualizacao de um veiculo.</summary>
    public class VeiculoRequest
    {
        [Required(ErrorMessage = "A placa e obrigatoria.")]
        [RegularExpression(@"^[A-Za-z]{3}-?\d[A-Za-z0-9]\d{2}$",
            ErrorMessage = "Informe uma placa valida, no formato ABC1D23 ou ABC-1234.")]
        public string Placa { get; set; }

        [Required(ErrorMessage = "O modelo e obrigatorio.")]
        [StringLength(80, MinimumLength = 2, ErrorMessage = "O modelo deve ter entre 2 e 80 caracteres.")]
        public string Modelo { get; set; }

        [Required(ErrorMessage = "O ano de fabricacao e obrigatorio.")]
        [Range(1950, 2100, ErrorMessage = "O ano de fabricacao deve estar entre 1950 e 2100.")]
        public int AnoFabricacao { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "A quilometragem nao pode ser negativa.")]
        public int Quilometragem { get; set; }

        [StringLength(30, ErrorMessage = "A cor deve ter no maximo 30 caracteres.")]
        public string Cor { get; set; }

        [Range(0.01, 99999.99, ErrorMessage = "O valor da diaria deve ser maior que zero.")]
        public decimal ValorDiaria { get; set; }

        [Required(ErrorMessage = "O fabricante e obrigatorio.")]
        [Range(1, int.MaxValue, ErrorMessage = "Informe um fabricante valido.")]
        public int FabricanteId { get; set; }

        [Required(ErrorMessage = "A categoria e obrigatoria.")]
        [Range(1, int.MaxValue, ErrorMessage = "Informe uma categoria valida.")]
        public int CategoriaId { get; set; }

        [EnumDataType(typeof(StatusVeiculo), ErrorMessage = "Status invalido. Use 1-Disponivel, 2-Alugado, 3-EmManutencao ou 4-Inativo.")]
        public StatusVeiculo Status { get; set; } = StatusVeiculo.Disponivel;
    }

    /// <summary>Dados devolvidos pelas rotas de veiculo.</summary>
    public class VeiculoResponse
    {
        public int VeiculoId { get; set; }
        public string Placa { get; set; }
        public string Modelo { get; set; }
        public int AnoFabricacao { get; set; }
        public int Quilometragem { get; set; }
        public string Cor { get; set; }
        public decimal ValorDiaria { get; set; }
        public string Status { get; set; }
        public int FabricanteId { get; set; }
        public string Fabricante { get; set; }
        public int CategoriaId { get; set; }
        public string Categoria { get; set; }
    }
}
