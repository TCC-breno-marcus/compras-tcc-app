public class RelatorioItemSaidaDto
{
    public long ItemId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string CatMat { get; set; } = string.Empty;
    public string UnidadeMedida { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public int QuantidadeSolicitada { get; set; }
    public decimal ValorMedioUnitario { get; set; }
    public decimal ValorTotalGasto { get; set; }
}