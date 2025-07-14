using System.ComponentModel.DataAnnotations;
using ComprasTccApp.Models.Entities.Itens;
using ComprasTccApp.Models.Entities.Solicitacoes;

namespace ComprasTccApp.Backend.Models.Entities.Items
{
    public class Item
    {
        public long Id { get; set; }

        [Required, StringLength(50)]
        public required string CatMat { get; set; }

        [Required, StringLength(250)]
        public required string Descricao { get; set; }

        [Required, StringLength(250)]
        public required string LinkImagem { get; set; }

        [Required]
        public decimal Quantidade { get; set; }

        [Required, StringLength(50)]
        public required string UnidadeMedida { get; set; }

        [Required]
        public decimal ValorUnitario { get; set; }

        [Required]
        public bool IsActive { get; set; }

        public List<SolicitacaoItem> SolicitacoesItem { get; set; } = new List<SolicitacaoItem>();
    }
}