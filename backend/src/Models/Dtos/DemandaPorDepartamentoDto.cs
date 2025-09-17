namespace Models.Dtos
{
    public class DemandaPorDepartamentoDto
    {
        public required UnidadeOrganizacionalDto Unidade { get; set; }
        public decimal QuantidadeTotal { get; set; }

        public required string Justificativa { get; set; }
    }
}
