namespace Models.Dtos
{
    public class StatusSolicitacaoDto
    {
        public int Id { get; set; }
        public required string Nome { get; set; }
        public string? Descricao { get; set; }
    }
}
