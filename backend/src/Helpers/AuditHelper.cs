using System.Collections;
using System.Globalization;
using System.Reflection;
using ComprasTccApp.Backend.Models.Entities.Items;
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
    }
}
