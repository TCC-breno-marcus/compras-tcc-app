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
        public long SolicitanteId { get; set; }
        public long? GestorId { get; set; }

        public Solicitante Solicitante { get; set; } = null!;

        public Gestor? Gestor { get; set; }

        [Required]
        public DateTime DataCriacao { get; set; }
    }
}
