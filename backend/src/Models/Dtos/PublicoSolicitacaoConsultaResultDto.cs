namespace Models.Dtos
{
    public class PublicoSolicitacaoConsultaResultDto
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public decimal TotalItensSolicitados { get; set; }
        public decimal ValorTotalSolicitado { get; set; }
        public List<PublicoSolicitacaoDto> Data { get; set; } = [];
    }

    public class PublicoSolicitacaoDto
    {
        public long Id { get; set; }
        public string? ExternalId { get; set; }
        public DateTime DataCriacao { get; set; }
        public string TipoSolicitacao { get; set; } = string.Empty;
        public int StatusId { get; set; }
        public string StatusNome { get; set; } = string.Empty;
        public string SolicitanteNomeMascarado { get; set; } = string.Empty;
        public string SolicitanteEmailMascarado { get; set; } = string.Empty;
        public string SolicitanteTelefoneMascarado { get; set; } = string.Empty;
        public string SolicitanteCpfMascarado { get; set; } = string.Empty;
        public string DepartamentoNome { get; set; } = string.Empty;
        public string DepartamentoSigla { get; set; } = string.Empty;
        public decimal ValorTotalSolicitacao { get; set; }
        public List<PublicoSolicitacaoItemDto> Itens { get; set; } = [];
    }

    public class PublicoSolicitacaoItemDto
    {
        public long ItemId { get; set; }
        public string ItemNome { get; set; } = string.Empty;
        public string CatMat { get; set; } = string.Empty;
        public string CategoriaNome { get; set; } = string.Empty;
        public decimal Quantidade { get; set; }
        public decimal ValorUnitario { get; set; }
        public decimal ValorTotal { get; set; }
        public string? Justificativa { get; set; }
    }
}
