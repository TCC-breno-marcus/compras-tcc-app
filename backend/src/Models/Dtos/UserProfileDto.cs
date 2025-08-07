namespace ComprasTccApp.Backend.DTOs // Ajuste o namespace se necessário
{
  public class UserProfileDto
  {
    public long Id { get; set; }
    public required string Nome { get; set; }
    public required string Email { get; set; }
    public required string Telefone { get; set; }
    public required string CPF { get; set; }
    public required string Role { get; set; }
    public string? Departamento { get; set; }
  }
}