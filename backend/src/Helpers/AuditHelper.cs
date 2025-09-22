using System.Collections;
using System.Globalization;
using System.Reflection;
using ComprasTccApp.Backend.Models.Entities.Items;
using ComprasTccApp.Models.Entities.Categorias;
using ComprasTccApp.Models.Entities.Itens;
using Models.Dtos;

namespace ComprasTccApp.Backend.Helpers
{
    public static class AuditHelper
    {
        public static List<string> Compare<T>(T oldObject, T newObject)
            where T : class
        {
            var changes = new List<string>();
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in properties)
            {
                // Ignora coleções (listas), pois elas precisam de tratamento especial
                if (typeof(ICollection).IsAssignableFrom(prop.PropertyType))
                {
                    continue;
                }

                var oldValue = prop.GetValue(oldObject)?.ToString();
                var newValue = prop.GetValue(newObject)?.ToString();

                if (!string.Equals(oldValue, newValue))
                {
                    changes.Add(
                        $"{prop.Name} alterado de |{oldValue ?? "vazio"}| para |{newValue ?? "vazio"}|."
                    );
                }
            }
            return changes;
        }

        public static List<string> CompareSolicitacaoItens(
            IEnumerable<SolicitacaoItem> itensAntigos,
            IEnumerable<SolicitacaoItemDto> itensNovos,
            Dictionary<long, Item> catalogoItens
        )
        {
            var changes = new List<string>();
            var dicionarioAntigos = itensAntigos.ToDictionary(i => i.ItemId);
            var dicionarioNovos = itensNovos.ToDictionary(i => i.ItemId);

            // 1. Encontra itens REMOVIDOS
            foreach (var itemId in dicionarioAntigos.Keys.Except(dicionarioNovos.Keys))
            {
                changes.Add(
                    $"Item '{dicionarioAntigos[itemId].Item.Nome}' (CATMAT '{dicionarioAntigos[itemId].Item.CatMat}') removido."
                );
            }

            // 2. Encontra itens ADICIONADOS
            foreach (var itemId in dicionarioNovos.Keys.Except(dicionarioAntigos.Keys))
            {
                var itemNovo = dicionarioNovos[itemId];
                var nomeItem = catalogoItens.TryGetValue(itemId, out var itemInfo)
                    ? itemInfo.Nome
                    : "Desconhecido";
                var catmatItem = catalogoItens.TryGetValue(itemId, out itemInfo)
                    ? itemInfo.CatMat
                    : "N/A";
                changes.Add(
                    $"Item '{nomeItem}' (CATMAT '{catmatItem}') adicionado com quantidade '{itemNovo.Quantidade}'."
                );
            }

            // 3. Encontra itens MODIFICADOS
            foreach (var itemId in dicionarioAntigos.Keys.Intersect(dicionarioNovos.Keys))
            {
                var itemAntigo = dicionarioAntigos[itemId];
                var itemNovo = dicionarioNovos[itemId];
                var nomeItem = itemAntigo.Item.Nome;
                var catmatItem = itemAntigo.Item.CatMat;

                if (itemAntigo.Quantidade != itemNovo.Quantidade)
                {
                    changes.Add(
                        $"Quantidade do item '{nomeItem}' (CATMAT '{catmatItem}') alterada de '{itemAntigo.Quantidade}' para '{itemNovo.Quantidade}'."
                    );
                }
                if (itemAntigo.ValorUnitario != itemNovo.ValorUnitario)
                {
                    changes.Add(
                        $"Preço unitário do item '{nomeItem}' (CATMAT '{catmatItem}') alterado de '{FormatarMoeda(itemAntigo.ValorUnitario)}' para '{FormatarMoeda(itemNovo.ValorUnitario)}'."
                    );
                }
                if (itemAntigo.Justificativa != itemNovo.Justificativa)
                {
                    changes.Add(
                        $"Justificativa do item '{nomeItem}' (CATMAT '{catmatItem}') alterada de '{itemAntigo.Justificativa}' para '{itemNovo.Justificativa}'."
                    );
                }
            }

            return changes;
        }

        public static string FormatarMoeda(decimal valor)
        {
            return valor.ToString("C", new CultureInfo("pt-BR"));
        }

        public static List<string> CompareItem(
            Item itemAntigo,
            ItemUpdateDto itemNovo,
            Dictionary<long, Categoria>? catalogoCategorias = null
        )
        {
            var changes = new List<string>();

            // Comparar Nome
            if (!string.IsNullOrEmpty(itemNovo.Nome) && itemAntigo.Nome != itemNovo.Nome)
            {
                changes.Add($"Nome alterado de '{itemAntigo.Nome}' para '{itemNovo.Nome}'.");
            }

            // Comparar CatMat
            if (!string.IsNullOrEmpty(itemNovo.CatMat) && itemAntigo.CatMat != itemNovo.CatMat)
            {
                changes.Add($"CATMAT alterado de '{itemAntigo.CatMat}' para '{itemNovo.CatMat}'.");
            }

            // Comparar Descrição
            if (
                !string.IsNullOrEmpty(itemNovo.Descricao)
                && itemAntigo.Descricao != itemNovo.Descricao
            )
            {
                changes.Add(
                    $"Descrição alterada de '{itemAntigo.Descricao}' para '{itemNovo.Descricao}'."
                );
            }

            // Comparar Especificação
            if (
                !string.IsNullOrEmpty(itemNovo.Especificacao)
                && itemAntigo.Especificacao != itemNovo.Especificacao
            )
            {
                changes.Add(
                    $"Especificação alterada de '{itemAntigo.Especificacao}' para '{itemNovo.Especificacao}'."
                );
            }

            // Comparar Preço Sugerido
            if (
                itemNovo.PrecoSugerido.HasValue
                && itemAntigo.PrecoSugerido != itemNovo.PrecoSugerido.Value
            )
            {
                changes.Add(
                    $"Preço sugerido alterado de '{FormatarMoeda(itemAntigo.PrecoSugerido)}' para '{FormatarMoeda(itemNovo.PrecoSugerido.Value)}'."
                );
            }

            // Comparar Categoria
            if (
                itemNovo.CategoriaId.HasValue
                && itemAntigo.CategoriaId != itemNovo.CategoriaId.Value
            )
            {
                var nomeAntigo =
                    catalogoCategorias?.TryGetValue(itemAntigo.CategoriaId, out var catAntiga)
                    == true
                        ? catAntiga.Nome
                        : $"ID {itemAntigo.CategoriaId}";

                var nomeNovo =
                    catalogoCategorias?.TryGetValue(itemNovo.CategoriaId.Value, out var catNova)
                    == true
                        ? catNova.Nome
                        : $"ID {itemNovo.CategoriaId.Value}";

                changes.Add($"Categoria alterada de '{nomeAntigo}' para '{nomeNovo}'.");
            }

            // Comparar Status Ativo
            if (itemNovo.IsActive.HasValue && itemAntigo.IsActive != itemNovo.IsActive.Value)
            {
                var statusAntigo = itemAntigo.IsActive ? "Ativo" : "Inativo";
                var statusNovo = itemNovo.IsActive.Value ? "Ativo" : "Inativo";
                changes.Add($"Status alterado de '{statusAntigo}' para '{statusNovo}'.");
            }

            return changes;
        }
    }
}
