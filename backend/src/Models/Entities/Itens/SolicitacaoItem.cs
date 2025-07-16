using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ComprasTccApp.Backend.Models.Entities.Items;
using ComprasTccApp.Models.Entities.Solicitacoes;

namespace ComprasTccApp.Models.Entities.Itens
{
    public class SolicitacaoItem
    {
        public long SolicitacaoId { get; set; }
        public long ItemId { get; set; }
        public required Solicitacao Solicitacao { get; set; }
        public required Item Item { get; set; }

        [Required]
        public decimal Quantidade { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal ValorUnitarioNaCompra { get; set; }
    }
}