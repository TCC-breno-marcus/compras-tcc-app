using System.ComponentModel.DataAnnotations;
using ComprasTccApp.Models.Entities.Pessoas;

namespace ComprasTccApp.Models.Entities.Servidores
{
    public class Servidor
    {
        public long Id { get; set; }

        public long PessoaId { get; set; }

        [Required]
        public required Pessoa Pessoa { get; set; }

        [Required, StringLength(50)]
        public required string IdentificadorInterno { get; set; }

        public bool IsGestor { get; set; }
    }
}