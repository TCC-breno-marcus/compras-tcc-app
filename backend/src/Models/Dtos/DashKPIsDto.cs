namespace Models.Dtos
{
    public class DashKPIsDto
    {
        public decimal ValorTotalEstimado { get; set; }
        public decimal CustoMedioSolicitacao { get; set; }
        public int TotalItensUnicos { get; set; }
        public decimal TotalUnidadesSolicitadas { get; set; }
        public int TotalDepartamentosSolicitantes { get; set; }
        public int TotalSolicitacoes { get; set; }
    }
}
