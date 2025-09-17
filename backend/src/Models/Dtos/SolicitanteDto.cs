using ComprasTccApp.Models.Entities.Departamentos;

namespace Models.Dtos
{
    public class SolicitanteDto
    {
        public long Id { get; set; }
        public required string Nome { get; set; }
        public required string Email { get; set; }
        public UnidadeOrganizacionalDto? Unidade { get; set; }
    }
}
