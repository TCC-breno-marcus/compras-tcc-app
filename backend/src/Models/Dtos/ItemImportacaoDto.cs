using System.Text.Json.Serialization;

namespace ComprasTccApp.Models.Dtos
{
  public class ItemImportacaoDto
  {
    [JsonPropertyName("nome")]
    public required string Nome { get; set; }

    [JsonPropertyName("descricao")]
    public required string Descricao { get; set; }

    [JsonPropertyName("codigo")]
    public required string Codigo { get; set; }

    [JsonPropertyName("especificacao")]
    public string Especificacao { get; set; } = "";

    [JsonPropertyName("link_imagem")]
    public string LinkImagem { get; set; } = "";

  }
}