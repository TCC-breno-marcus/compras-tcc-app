namespace Models.Dtos
{
    public class ItemPorDepartamentoDto
    {
        public long Id { get; set; }
        public required string Nome { get; set; }
        public required string CatMat { get; set; }
        public string? Descricao { get; set; }
        public string? Especificacao { get; set; }
        public required string CategoriaNome { get; set; }
        public string? LinkImagem { get; set; }
        public decimal PrecoSugerido { get; set; }
        public decimal QuantidadeTotalSolicitada { get; set; }

        public decimal ValorTotalSolicitado { get; set; }
        public decimal PrecoMedio { get; set; }
        public decimal PrecoMinimo { get; set; }
        public decimal PrecoMaximo { get; set; }

        // Insight de Demanda
        public int NumeroDeSolicitacoes { get; set; }
        public required List<QuantidadePorDepartamentoDto> QuantidadesPorDepartamento { get; set; }
    }
}
