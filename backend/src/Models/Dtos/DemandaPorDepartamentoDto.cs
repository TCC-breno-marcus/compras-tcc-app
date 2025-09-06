namespace Models.Dtos
{
    public class DemandaPorDepartamentoDto
    {
        public required string Departamento { get; set; }
        public decimal QuantidadeTotal { get; set; }

        public required string Justificativa { get; set; }
    }
}
