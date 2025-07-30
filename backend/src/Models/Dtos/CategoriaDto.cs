namespace ComprasTccApp.Models.Dtos
{
    public class CategoriaDto
    {
        public long Id { get; set; }
        public required string Nome { get; set; }
        public required string Descricao { get; set; }
        public bool IsActive { get; set; }
    }
}
