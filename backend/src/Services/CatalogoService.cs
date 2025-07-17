using ComprasTccApp.Models.Dtos;
using Database;
using Microsoft.EntityFrameworkCore;
using Services.Interfaces;
using ComprasTccApp.Backend.Models.Entities.Items;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Services
{
    public class CatalogoService : ICatalogoService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<CatalogoService> _logger;

        public CatalogoService
        (
            AppDbContext context,
             ILogger<CatalogoService> logger
        )
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<ItemDto>> GetAllItensAsync()
        {
            _logger.LogInformation("Buscando todos os itens do catálogo no serviço...");

            try
            {
                var itensDoBanco = await _context.Items.ToListAsync();

                var itensDto = itensDoBanco
                    .Select(item => new ItemDto
                    {
                        Id = item.Id,
                        Nome = item.Nome,
                        Descricao = item.Descricao,
                        CatMat = item.CatMat,
                        LinkImagem = item.LinkImagem,
                        Especificacao = item.Especificacao,
                        IsActive = item.IsActive,
                    })
                    .ToList();

                _logger.LogInformation("Encontrados {Count} itens.", itensDto.Count);

                return itensDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocorreu um erro ao buscar os itens do banco de dados.");
                throw;
            }
        }

        public async Task<ItemDto?> EditarItemAsync(int id, JsonPatchDocument<ItemUpdateDto> patchDoc)
        {
            // 1. Encontre o item original no banco de dados.
            var itemDoBanco = await _context.Items.FirstOrDefaultAsync(item => item.Id == id);
            if (itemDoBanco == null)
            {
                _logger.LogWarning("Tentativa de editar item com ID {Id} não encontrado.", id);
                return null; // Retorna nulo se o item não existe
            }

            // 2. Crie um DTO a partir do item do banco para aplicar o patch.
            var itemParaAtualizar = new ItemUpdateDto
            {
                Nome = itemDoBanco.Nome,
                Descricao = itemDoBanco.Descricao,
                Especificacao = itemDoBanco.Especificacao,
                IsActive = itemDoBanco.IsActive
            };

            // 3. Aplique as alterações do patch ao DTO.
            // O ModelState é usado para capturar erros durante a aplicação do patch (ex: tentar alterar um campo que não existe).
            var modelState = new ModelStateDictionary();
            patchDoc.ApplyTo(itemParaAtualizar, error =>
            {
                modelState.AddModelError(error.Operation?.path ?? string.Empty, error.ErrorMessage);
            });

            if (!modelState.IsValid)
            {
                // Opcional: log dos erros para debug
                foreach (var entry in modelState)
                {
                    foreach (var error in entry.Value.Errors)
                    {
                        _logger.LogWarning("Erro ao aplicar patch: {Path} - {Error}", entry.Key, error.ErrorMessage);
                    }
                }

                throw new InvalidOperationException("O patch document contém erros.");
            }
            
            // 4. Mapeie as alterações do DTO de volta para a entidade do banco.
            itemDoBanco.Nome = itemParaAtualizar.Nome;
            itemDoBanco.Descricao = itemParaAtualizar.Descricao;
            itemDoBanco.Especificacao = itemParaAtualizar.Especificacao;
            itemDoBanco.IsActive = itemParaAtualizar.IsActive ?? itemDoBanco.IsActive; // Mantém o valor antigo se não for alterado

            // 5. Salve as mudanças no banco.
            await _context.SaveChangesAsync();

            _logger.LogInformation("Item com ID {Id} foi atualizado com sucesso.", id);
            
            // 6. Retorne o DTO completo do item atualizado.
            return new ItemDto
            {
                Id = itemDoBanco.Id,
                Nome = itemDoBanco.Nome,
                Descricao = itemDoBanco.Descricao,
                CatMat = itemDoBanco.CatMat,
                Especificacao = itemDoBanco.Especificacao,
                LinkImagem = itemDoBanco.LinkImagem,
                IsActive = itemDoBanco.IsActive
            };
        }

        public async Task ImportarItensAsync(IEnumerable<ItemImportacaoDto> itensParaImportar)
        {
            var codigosParaVerificar = itensParaImportar.Select(dto => dto.Codigo).Distinct();
            
            var itensExistentes = await _context.Items
                                        .Where(item => codigosParaVerificar.Contains(item.CatMat))
                                        .ToDictionaryAsync(item => item.CatMat, item => item);
            
            _logger.LogInformation("Verificando {Count} itens. {ExistingCount} já existem no banco e serão atualizados.", 
                itensParaImportar.Count(), itensExistentes.Count);
            
            foreach (var dto in itensParaImportar)
            {
                if (itensExistentes.TryGetValue(dto.Codigo, out var itemExistente))
                {
                    itemExistente.Nome = dto.Nome;
                    itemExistente.Descricao = dto.Descricao;
                    itemExistente.Especificacao = dto.Especificacao;
                    itemExistente.IsActive = true; 
                }
                else
                {
                    var novoItem = new Item
                    {
                        Nome = dto.Nome,
                        Descricao = dto.Descricao,
                        CatMat = dto.Codigo,
                        Especificacao = dto.Especificacao,
                        LinkImagem = dto.LinkImagem, 
                        IsActive = true
                    };
                    await _context.Items.AddAsync(novoItem);
                }
            }

            var registrosAfetados = await _context.SaveChangesAsync();

            _logger.LogInformation("Importação concluída. {Count} registros foram afetados no banco de dados.", registrosAfetados);
        }

        public async Task<string> PopularImagensAsync(string caminhoDasImagens)
        {
            _logger.LogInformation("Iniciando rotina para popular imagens a partir do caminho: {Caminho}", caminhoDasImagens);

            if (!Directory.Exists(caminhoDasImagens))
            {
                var erroMsg = $"ERRO: O diretório de imagens não foi encontrado em '{caminhoDasImagens}'.";
                _logger.LogError(erroMsg);
                throw new DirectoryNotFoundException(erroMsg);
            }

            var arquivosDeImagem = Directory.GetFiles(caminhoDasImagens);
            int produtosAtualizados = 0;
            int produtosNaoEncontrados = 0;

            _logger.LogInformation("Encontrados {Count} arquivos de imagem para processar.", arquivosDeImagem.Length);

            var todosOsItens = await _context.Items.ToListAsync(); // Carrega todos os itens para otimizar a busca

            foreach (var caminhoCompletoArquivo in arquivosDeImagem)
            {
                var nomeDoArquivo = Path.GetFileName(caminhoCompletoArquivo);
                var codigoProduto = Path.GetFileNameWithoutExtension(caminhoCompletoArquivo);

                // Busca na lista em memória para evitar múltiplos acessos ao BD no loop
                var produto = todosOsItens.FirstOrDefault(p => p.CatMat == codigoProduto); 

                if (produto != null)
                {
                    produto.LinkImagem = nomeDoArquivo; 
                    produtosAtualizados++;
                }
                else
                {
                    produtosNaoEncontrados++;
                }
            }
            
            string resumo;
            if (produtosAtualizados > 0)
            {
                await _context.SaveChangesAsync();
                resumo = $"SUCESSO! {produtosAtualizados} produtos foram atualizados no banco de dados.";
                _logger.LogInformation(resumo);
            }
            else
            {
                resumo = "Nenhum produto foi atualizado. Verifique se os nomes dos arquivos correspondem aos códigos dos itens.";
                _logger.LogWarning(resumo);
            }
            
            if (produtosNaoEncontrados > 0)
            {
                resumo += $" {produtosNaoEncontrados} imagens não encontraram um produto correspondente.";
                _logger.LogWarning("{Count} imagens não encontraram um produto correspondente.", produtosNaoEncontrados);
            }

            return resumo;
        }

    }
}