namespace Models.Dtos;

public class RelatorioCategoriaSaidaDto
{
    public string CategoriaNome { get; set; } = string.Empty;
    public int QuantidadeItensVendidos { get; set; }
    public decimal ValorTotal { get; set; }
    public decimal PercentualDoTotal { get; set; }
}