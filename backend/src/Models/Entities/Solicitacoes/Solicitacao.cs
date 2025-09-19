using System.ComponentModel.DataAnnotations;
using ComprasTccApp.Models.Entities.Gestores;
using ComprasTccApp.Models.Entities.Historicos;
using ComprasTccApp.Models.Entities.Itens;
using ComprasTccApp.Models.Entities.Solicitantes;
using ComprasTccApp.Models.Entities.Status;

namespace ComprasTccApp.Models.Entities.Solicitacoes
{
    public class Solicitacao
    {
        public long Id { get; set; }
        public long SolicitanteId { get; set; }
        public long? GestorId { get; set; }

        [StringLength(100)]
        public string? ExternalId { get; set; }

        public Solicitante Solicitante { get; set; } = null!;

        public Gestor? Gestor { get; set; }

        [Required]
        public DateTime DataCriacao { get; set; }

        public int StatusId { get; set; }
        public StatusSolicitacao Status { get; set; } = null!;

        public List<HistoricoSolicitacao> Historico { get; set; } = [];

        public List<SolicitacaoItem> ItemSolicitacao { get; set; } = [];
    }
}
