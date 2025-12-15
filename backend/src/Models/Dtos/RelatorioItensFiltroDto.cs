public class RelatorioItensFiltroDto
{
    public required string SiglaDepartamento { get; set; }
    public DateTime DataInicio { get; set; }
    public DateTime DataFim { get; set; }

    public string? SearchTerm { get; set; }
    public string? CategoriaNome { get; set; }
    public string? ItemsType { get; set; } // "patrimonial" ou "geral"
    public string? SortOrder { get; set; } // "asc" ou "desc"
}