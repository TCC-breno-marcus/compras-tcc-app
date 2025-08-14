namespace Models.Dtos
{
    public class SolicitacaoResultDto
    {
        public long Id { get; set; }
        public DateTime DataCriacao { get; set; }
        public string? JustificativaGeral { get; set; }
        public required SolicitanteDto Solicitante { get; set; }
        public List<ItemSolicitacaoResultDto> Itens { get; set; } = new();
    }
}
