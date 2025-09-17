namespace Models.Dtos
{
    public class UnidadeOrganizacionalDto
    {
        public int Id { get; set; }
        public required string Nome { get; set; }
        public required string Sigla { get; set; }
        public string? Email { get; set; }
        public string? Telefone { get; set; }
        public required string Tipo { get; set; }
    }
}
