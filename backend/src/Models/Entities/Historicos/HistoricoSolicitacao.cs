using System.ComponentModel.DataAnnotations;
using ComprasTccApp.Models.Entities.Pessoas;
using ComprasTccApp.Models.Entities.Solicitacoes;
using ComprasTccApp.Models.Entities.Status;

namespace ComprasTccApp.Models.Entities.Historicos
{
    public class HistoricoSolicitacao
    {
        public long Id { get; set; }
        public long SolicitacaoId { get; set; }
        public DateTime DataOcorrencia { get; set; }
        public int StatusAnteriorId { get; set; }
        public int StatusNovoId { get; set; }

        public long PessoaId { get; set; }

        [StringLength(500)]
        public string? Observacoes { get; set; }

        public Solicitacao Solicitacao { get; set; } = null!;
        public Pessoa Pessoa { get; set; } = null!;
        public StatusSolicitacao StatusAnterior { get; set; } = null!;
        public StatusSolicitacao StatusNovo { get; set; } = null!;
    }
}
