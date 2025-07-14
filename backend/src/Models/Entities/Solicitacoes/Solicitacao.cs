using System.ComponentModel.DataAnnotations;
using ComprasTccApp.Backend.Models.Entities.Items;
using ComprasTccApp.Models.Entities.Gestores;
using ComprasTccApp.Models.Entities.Itens;
using ComprasTccApp.Models.Entities.Solicitantes;

namespace ComprasTccApp.Models.Entities.Solicitacoes
{
    public class Solicitacao
    {
        public long Id { get; set; }

        [Required]
        public required Solicitante Solicitante { get; set; }

        [Required]
        public required Gestor Gestor { get; set; }

        [Required]
        public DateTime DataCriacao { get; set; }

        [Required, StringLength(500)]
        public required string JustificativaGeral { get; set; }

        [Required]
        public List<SolicitacaoItem> ItemSolicitacao { get; set; } = new List<SolicitacaoItem>();
    }
}