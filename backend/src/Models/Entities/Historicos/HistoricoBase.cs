using System.ComponentModel.DataAnnotations;
using ComprasTccApp.Backend.Enums;
using ComprasTccApp.Models.Entities.Pessoas;

namespace ComprasTccApp.Models.Entities.Historicos
{
    public class HistoricoBase
    {
        public long Id { get; set; }
        public long PessoaId { get; set; }
        public DateTime DataOcorrencia { get; set; }

        [Required]
        public AcaoHistoricoEnum Acao { get; set; }

        [StringLength(500)]
        public string? Detalhes { get; set; }

        [StringLength(500)]
        public string? Observacoes { get; set; }
        public Pessoa Pessoa { get; set; } = null!;
    }
}
