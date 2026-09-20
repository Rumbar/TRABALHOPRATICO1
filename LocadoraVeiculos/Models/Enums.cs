namespace LocadoraVeiculos.Models
{
    /// <summary>
    /// Situacao do veiculo na frota da locadora.
    /// </summary>
    public enum StatusVeiculo
    {
        Disponivel = 1,
        Alugado = 2,
        EmManutencao = 3,
        Inativo = 4
    }

    /// <summary>
    /// Situacao do contrato de aluguel.
    /// </summary>
    public enum StatusAluguel
    {
        EmAndamento = 1,
        Finalizado = 2,
        Cancelado = 3
    }
}
