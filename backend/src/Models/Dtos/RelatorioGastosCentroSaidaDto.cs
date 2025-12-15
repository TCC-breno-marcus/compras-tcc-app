namespace Models.Dtos;

public class RelatorioGastosCentroSaidaDto
{
    public int CentroId { get; set; }
    public string CentroNome { get; set; } = string.Empty;
    public string CentroSigla { get; set; } = string.Empty;
    public int QuantidadeSolicitacoes { get; set; }
    public decimal ValorTotalGasto { get; set; }
    public string DepartamentoMaiorGasto { get; set; } = string.Empty;
}