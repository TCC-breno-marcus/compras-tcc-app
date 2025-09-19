using System.ComponentModel.DataAnnotations;
using ComprasTccApp.Models.Entities.Departamentos;
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

        public int DepartamentoId { get; set; }
        public Departamento Departamento { get; set; } = null!;

        public List<Solicitacao> Solicitacoes { get; set; } = [];

        [Required]
        public DateTime DataUltimaSolicitacao { get; set; }
    }
}
