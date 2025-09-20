using ComprasTccApp.Backend.Models.Entities.Items;

namespace ComprasTccApp.Models.Entities.Historicos
{
    public class HistoricoItem : HistoricoBase
    {
        public long ItemId { get; set; }
        public Item Item { get; set; } = null!;
    }
}
