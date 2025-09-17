using System.ComponentModel.DataAnnotations;
using ComprasTccApp.Models.Entities.Centros;
using ComprasTccApp.Models.Entities.Solicitantes;

namespace ComprasTccApp.Models.Entities.Departamentos
{
    public class Departamento
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
        public int CentroId { get; set; }
        public Centro Centro { get; set; } = null!;

        public ICollection<Solicitante> Solicitantes { get; set; } = [];
    }
}
