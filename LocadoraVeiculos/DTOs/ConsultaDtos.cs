using LocadoraVeiculos.Models;

namespace LocadoraVeiculos.DTOs
{
    /// <summary>Resultado da consulta de veiculos disponiveis (INNER JOIN com Fabricante e Categoria).</summary>
    public class VeiculoDisponivelResponse
    {
        public int VeiculoId { get; set; }
        public string Placa { get; set; }
        public string Modelo { get; set; }
        public string Fabricante { get; set; }
        public string Categoria { get; set; }
        public int AnoFabricacao { get; set; }
        public int Quilometragem { get; set; }
        public decimal ValorDiaria { get; set; }
        public decimal ValorDiariaComAjuste { get; set; }
    }

    /// <summary>Resultado das consultas de alugueis (INNER JOIN entre Aluguel, Cliente, Veiculo e Funcionario).</summary>
    public class AluguelConsultaResponse
    {
        public int AluguelId { get; set; }
        public string Cliente { get; set; }
        public string Cpf { get; set; }
        public string Veiculo { get; set; }
        public string Placa { get; set; }
        public string Fabricante { get; set; }
        public string Funcionario { get; set; }
        public DateTime DataRetirada { get; set; }
        public DateTime DataPrevistaDevolucao { get; set; }
        public DateTime? DataDevolucao { get; set; }
        public decimal ValorTotal { get; set; }
        public string Status { get; set; }
        public int? DiasEmAtraso { get; set; }
    }

    /// <summary>
    /// Linha intermediaria das consultas de aluguel: recebe o resultado do join no banco
    /// antes do calculo dos dias de atraso, feito em memoria.
    /// </summary>
    public class AluguelLinhaConsulta
    {
        public int AluguelId { get; set; }
        public string Cliente { get; set; }
        public string Cpf { get; set; }
        public string Veiculo { get; set; }
        public string Placa { get; set; }
        public string Fabricante { get; set; }
        public string Funcionario { get; set; }
        public DateTime DataRetirada { get; set; }
        public DateTime DataPrevistaDevolucao { get; set; }
        public DateTime? DataDevolucao { get; set; }
        public decimal ValorTotal { get; set; }
        public StatusAluguel Status { get; set; }
    }

    /// <summary>Resultado da consulta de clientes sem aluguel (LEFT JOIN com Aluguel).</summary>
    public class ClienteSemAluguelResponse
    {
        public int ClienteId { get; set; }
        public string Nome { get; set; }
        public string Cpf { get; set; }
        public string Email { get; set; }
        public DateTime DataCadastro { get; set; }
    }

    /// <summary>Resultado da consulta de veiculos nunca alugados (LEFT JOIN com Aluguel).</summary>
    public class VeiculoNuncaAlugadoResponse
    {
        public int VeiculoId { get; set; }
        public string Placa { get; set; }
        public string Modelo { get; set; }
        public string Fabricante { get; set; }
        public string Categoria { get; set; }
        public decimal ValorDiaria { get; set; }
        public string Status { get; set; }
    }

    /// <summary>Resultado do faturamento por categoria (LEFT JOIN em duas tabelas com agrupamento).</summary>
    public class FaturamentoCategoriaResponse
    {
        public int CategoriaId { get; set; }
        public string Categoria { get; set; }
        public int QuantidadeVeiculos { get; set; }
        public int QuantidadeAlugueis { get; set; }
        public int AlugueisFinalizados { get; set; }
        public decimal ValorTotalFaturado { get; set; }
        public decimal TicketMedio { get; set; }
    }
}
