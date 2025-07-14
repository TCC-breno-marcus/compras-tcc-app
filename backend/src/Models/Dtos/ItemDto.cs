namespace ComprasTccApp.Models.Dtos
{
    public class ItemDto
    {
        public long Id { get; set; }
        public required string Descricao { get; set; }
        public required string CatMat { get; set; }
        public required string LinkImagem { get; set; }
        public required string UnidadeMedida { get; set; }
        public bool IsActive { get; set; }
    }
}