using System.ComponentModel.DataAnnotations;
using ComprasTccApp.Models.Entities.Servidores;
using ComprasTccApp.Models.Entities.Solicitacoes;

namespace ComprasTccApp.Models.Entities.Solicitantes
{
    public class Solicitante
    {
        public long Id { get; set; }

        public long ServidorId { get; set; }

        [Required]
        public required Servidor Servidor { get; set; }

        public List<Solicitacao> Solicitacoes { get; set; } = new List<Solicitacao>();

        [Required]
        public DateTime DataUltimaSolicitacao { get; set; }

        [Required]
        public required string Unidade { get; set; }
    }
}
