using System.ComponentModel.DataAnnotations;
using ComprasTccApp.Models.Entities.Itens;
using ComprasTccApp.Models.Entities.Solicitacoes;
using System.ComponentModel.DataAnnotations.Schema;
using ComprasTccApp.Models.Entities.Categorias;

namespace ComprasTccApp.Backend.Models.Entities.Items
{
    public class Item
    {
        public long Id { get; set; }

        [Required, StringLength(100)]
        public required string Nome { get; set; }

        [Required, StringLength(50)]
        public required string CatMat { get; set; }

        [Required, MaxLength(2000)]
        public required string Descricao { get; set; }

        [Required, StringLength(250)]
        public required string LinkImagem { get; set; }

        [Required, StringLength(500)]
        public required string Especificacao { get; set; }

        [Required]
        public decimal PrecoSugerido { get; set; }

        [Required]
        public bool IsActive { get; set; }

        public long CategoriaId { get; set; }

        [ForeignKey("CategoriaId")] 
        public Categoria Categoria { get; set; } = null!;

        public List<SolicitacaoItem> SolicitacoesItem { get; set; } = new List<SolicitacaoItem>();
    }
}