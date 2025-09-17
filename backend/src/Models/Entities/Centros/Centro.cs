using System.ComponentModel.DataAnnotations;
using ComprasTccApp.Models.Entities.Departamentos;
using ComprasTccApp.Models.Entities.Gestores;

namespace ComprasTccApp.Models.Entities.Centros
{
    public class Centro
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public required string Nome { get; set; }

        [Required, StringLength(10)]
        public required string Sigla { get; set; }

        [Required, StringLength(120)]
        public required string Email { get; set; }

        [Required, StringLength(20)]
        public required string Telefone { get; set; }
        public ICollection<Departamento> Departamentos { get; set; } = [];
        public ICollection<Gestor> Gestores { get; set; } = [];
    }
}
