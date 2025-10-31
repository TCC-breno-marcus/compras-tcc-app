namespace Models.Dtos
{
    public class DashResultDto
    {
        public DashKPIsDto Kpis { get; set; } = null!;
        public ChartDataDto ValorPorDepartamento { get; set; } = null!;
        public ChartDataDto ValorPorCategoria { get; set; } = null!;
        public ChartDataDto VisaoGeralStatus { get; set; } = null!;
        public List<DashTopItemDto> TopItensPorQuantidade { get; set; } = null!;
        public List<DashTopItemDto> TopItensPorValorTotal { get; set; } = null!;
    }
}
