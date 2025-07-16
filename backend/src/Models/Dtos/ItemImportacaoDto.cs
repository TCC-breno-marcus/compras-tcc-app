using System.Text.Json.Serialization;

namespace ComprasTccApp.Models.Dtos
{
  public class ItemImportacaoDto
  {
    [JsonPropertyName("nome")]
    public string Nome { get; set; }

    [JsonPropertyName("descricao")]
    public string Descricao { get; set; }

    [JsonPropertyName("codigo")]
    public string Codigo { get; set; }

    [JsonPropertyName("especificacao")]
    public string Especificacao { get; set; }

    [JsonPropertyName("unidade_de_medida")]
    public string UnidadeMedida { get; set; }

    [JsonPropertyName("link_imagem")]
    public string LinkImagem { get; set; }

  }
}