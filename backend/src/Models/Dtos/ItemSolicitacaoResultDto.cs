namespace Models.Dtos
{
    public class ItemSolicitacaoResultDto
    {
        public long ItemId { get; set; }
        public required string NomeDoItem { get; set; }
        public required string CatMat { get; set; }
        public required decimal Quantidade { get; set; }
        public decimal ValorUnitario { get; set; }
        public string? Justificativa { get; set; }
    }
}
