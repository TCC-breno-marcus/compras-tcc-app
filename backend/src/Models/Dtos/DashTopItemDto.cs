namespace Models.Dtos
{
    public class DashTopItemDto
    {
        public long ItemId { get; set; }
        public required string Nome { get; set; }
        public required string CatMat { get; set; }
        public decimal Valor { get; set; } // Valor (Pode ser Quantidade ou Custo Total)
    }
}
