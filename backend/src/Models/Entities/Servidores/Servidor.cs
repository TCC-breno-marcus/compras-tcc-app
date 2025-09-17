using System.ComponentModel.DataAnnotations;
using ComprasTccApp.Models.Entities.Gestores;
using ComprasTccApp.Models.Entities.Pessoas;
using ComprasTccApp.Models.Entities.Solicitantes;

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

        public Solicitante? Solicitante { get; set; }

        // Um Servidor pode ser um Gestor (relação 1-para-1 opcional)
        public Gestor? Gestor { get; set; }
    }
}
