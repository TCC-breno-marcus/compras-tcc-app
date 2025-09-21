namespace Models.Dtos
{
    public class HistoricoSolicitacaoDto
    {
        public long Id { get; set; }
        public DateTime DataOcorrencia { get; set; }
        public required string Acao { get; set; }
        public string? Detalhes { get; set; }
        public string? Observacoes { get; set; }
        public required string NomePessoa { get; set; }
    }
}
