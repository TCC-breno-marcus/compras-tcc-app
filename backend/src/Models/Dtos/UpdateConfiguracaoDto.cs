namespace Models.Dtos
{
    public class UpdateConfiguracaoDto
    {
        public DateTime? PrazoSubmissao { get; set; }
        public int? MaxQuantidadePorItem { get; set; }
        public int? MaxItensDiferentesPorSolicitacao { get; set; }
        public string? EmailContatoPrincipal { get; set; }
        public string? EmailParaNotificacoes { get; set; }
    }
}
