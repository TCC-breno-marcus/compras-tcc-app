namespace Models.Dtos
{
    public class ItemSolicitacaoResultDto
    {
        public long Id { get; set; }
        public required string Nome { get; set; }
        public required string CatMat { get; set; }
        public required decimal Quantidade { get; set; }
        public required string LinkImagem { get; set; }
        public decimal PrecoSugerido { get; set; }
        public string? Justificativa { get; set; }
    }
}
