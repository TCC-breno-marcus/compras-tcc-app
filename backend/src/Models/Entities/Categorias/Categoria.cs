using System.ComponentModel.DataAnnotations;
using ComprasTccApp.Backend.Models.Entities.Items;
using Models.Dtos;

namespace ComprasTccApp.Models.Entities.Categorias
{
    public class Categoria
    {
        public long Id { get; set; }

        [Required, StringLength(50)]
        public required string Nome { get; set; }

        [Required, MaxLength(250)]
        public required string Descricao { get; set; }

        public bool IsActive { get; set; } = true;

        public List<Item> Itens { get; set; } = new List<Item>();

        public static implicit operator Categoria(CategoriaDto v)
        {
            throw new NotImplementedException();
        }
    }
}
