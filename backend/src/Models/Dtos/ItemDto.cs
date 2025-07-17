namespace ComprasTccApp.Models.Dtos
{
    public class ItemDto
    {
        public long Id { get; set; }
        public required string Nome { get; set; }
        public required string Descricao { get; set; }
        public required string CatMat { get; set; }
        public required string LinkImagem { get; set; }
        public required string Especificacao { get; set; }
        public required string PrecoSugerido { get; set; }
        public bool IsActive { get; set; }
    }
}