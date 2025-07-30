namespace ComprasTccApp.Models.Dtos
{
    public class ItemUpdateDto
    {
        public string? Nome { get; set; }
        public string? CatMat { get; set; }
        public string? Descricao { get; set; }
        public string? Especificacao { get; set; }
        public decimal? PrecoSugerido { get; set; }
        public long? CategoriaId { get; set; }
        public bool? IsActive { get; set; }
    }
}
