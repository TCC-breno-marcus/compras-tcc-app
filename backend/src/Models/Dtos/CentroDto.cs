namespace Models.Dtos
{
    public class CentroDto
    {
        public int Id { get; set; }
        public required string Nome { get; set; }
        public required string Sigla { get; set; }
        public string? Email { get; set; }
        public string? Telefone { get; set; }
    }
}
