namespace Models.Dtos
{
    public class ConfiguracaoDto
    {
        public DateTime? PrazoSubmissao { get; set; }
        public int MaxQuantidadePorItem { get; set; }
        public int MaxItensDiferentesPorSolicitacao { get; set; }
        public required string EmailContatoPrincipal { get; set; }
        public required string EmailParaNotificacoes { get; set; }
    }
}
