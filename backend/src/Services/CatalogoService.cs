using ComprasTccApp.Models.Dtos;
using Database;
using Microsoft.EntityFrameworkCore;
using Services.Interfaces;
using ComprasTccApp.Backend.Models.Entities.Items;

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

        public async Task<PaginatedResultDto<ItemDto>> GetAllItensAsync
        (
            long? id,
            string? catMat,
            string? nome,
            string? descricao,
            string? especificacao,
            bool? isActive,
            string? searchTerm,
            int pageNumber,
            int pageSize,
            string? sortOrder
        )
        {
            _logger.LogInformation("Iniciando busca avançada de itens com filtros e paginação...");

            try
            {
                var query = _context.Items.AsQueryable();

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    query = query.Where(item =>
                        item.Nome.ToLower().Contains(searchTerm.ToLower()) ||
                        item.Descricao.ToLower().Contains(searchTerm.ToLower()) ||
                        item.CatMat.Contains(searchTerm.ToLower()) ||
                        item.Especificacao.ToLower().Contains(searchTerm.ToLower())
                    );

                    if (isActive.HasValue)
                    {
                        query = query.Where(item => item.IsActive == isActive.Value);
                    }
                }
                else
                {
                    if (id.HasValue)
                    {
                        query = query.Where(item => item.Id == id.Value);
                    }
                    if (!string.IsNullOrWhiteSpace(catMat))
                    {
                        query = query.Where(item => item.CatMat.Contains(catMat));
                    }
                    if (!string.IsNullOrWhiteSpace(nome))
                    {
                        query = query.Where(item => item.Nome.ToLower().Contains(nome.ToLower()));
                    }
                    if (!string.IsNullOrWhiteSpace(descricao))
                    {
                        query = query.Where(item => item.Descricao.ToLower().Contains(descricao.ToLower()));
                    }
                    if (!string.IsNullOrWhiteSpace(especificacao))
                    {
                        query = query.Where(item => item.Especificacao.ToLower().Contains(especificacao.ToLower()));
                    }
                    if (isActive.HasValue)
                    {
                        query = query.Where(item => item.IsActive == isActive.Value);
                    }
                    if (!string.IsNullOrWhiteSpace(sortOrder))
                    {
                        if (sortOrder.ToLower() == "asc")
                        {
                            query = query.OrderBy(item => item.Nome);
                        }
                        else if (sortOrder.ToLower() == "desc")
                        {
                            query = query.OrderByDescending(item => item.Nome);
                        }
                    }
                    else
                    {
                        query = query.OrderBy(item => item.Id);
                    }

                }

                var totalCount = await query.CountAsync();

                var itensPaginados = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var itensDto = itensPaginados.Select(item => new ItemDto
                {
                    Id = item.Id,
                    Nome = item.Nome,
                    Descricao = item.Descricao,
                    CatMat = item.CatMat,
                    // TODO: o link abaixo deve estar em variável de ambiente
                    LinkImagem = string.IsNullOrWhiteSpace(item.LinkImagem)
                        ? item.LinkImagem
                        : $"http://localhost:8088/images/{item.LinkImagem}",
                    PrecoSugerido = item.PrecoSugerido,
                    Especificacao = item.Especificacao,
                    IsActive = item.IsActive,
                }).ToList();

                _logger.LogInformation("Busca concluída. Retornando {Count} de um total de {Total} itens.", itensDto.Count, totalCount);

                return new PaginatedResultDto<ItemDto>(itensDto, totalCount, pageNumber, pageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocorreu um erro ao buscar os itens do banco de dados com filtros.");
                throw;
            }
        }

        public async Task<ItemDto?> EditarItemAsync(int id, ItemUpdateDto updateDto)
        {
            var itemDoBanco = await _context.Items.FirstOrDefaultAsync(i => i.Id == id);

            if (itemDoBanco == null)
            {
                _logger.LogWarning("Tentativa de editar item com ID {Id} não encontrado.", id);
                return null;
            }

            if (!string.IsNullOrEmpty(updateDto.Nome))
                itemDoBanco.Nome = updateDto.Nome;

            if (!string.IsNullOrEmpty(updateDto.Descricao))
                itemDoBanco.Descricao = updateDto.Descricao;

            if (!string.IsNullOrEmpty(updateDto.Especificacao))
                itemDoBanco.Especificacao = updateDto.Especificacao;

            if (updateDto.PrecoSugerido.HasValue)
                itemDoBanco.PrecoSugerido = updateDto.PrecoSugerido.Value;

            if (updateDto.IsActive.HasValue)
                itemDoBanco.IsActive = updateDto.IsActive.Value;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Item com ID {Id} atualizado.", id);

            return new ItemDto
            {
                Id = itemDoBanco.Id,
                Nome = itemDoBanco.Nome,
                Descricao = itemDoBanco.Descricao,
                CatMat = itemDoBanco.CatMat,
                PrecoSugerido = itemDoBanco.PrecoSugerido,
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

        public async Task<ItemDto?> GetItemByIdAsync(long id)
        {
            _logger.LogInformation("Iniciando busca de um item pelo ID...");
            _logger.LogWarning("ID {Id}", id);


            try
            {
                var item = await _context.Items
                    .AsNoTracking()
                    .FirstOrDefaultAsync(i => i.Id == id);

                if (item == null)
                {
                    _logger.LogWarning("Item com ID {Id} não encontrado.", id);
                    return null;
                }

                _logger.LogInformation("Item com ID {Id} encontrado.", id);

                return new ItemDto
                {
                    Id = item.Id,
                    Nome = item.Nome,
                    Descricao = item.Descricao,
                    CatMat = item.CatMat,
                    // TODO: o link abaixo deve estar em variável de ambiente
                    LinkImagem = string.IsNullOrWhiteSpace(item.LinkImagem)
                        ? item.LinkImagem
                        : $"http://localhost:8088/images/{item.LinkImagem}",
                    PrecoSugerido = item.PrecoSugerido,
                    Especificacao = item.Especificacao,
                    IsActive = item.IsActive
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar item por ID {Id}.", id);
                throw;
            }
        }

        public async Task<ItemDto> CriarItemAsync(ItemDto dto)
        {
            var itemExistente = await _context.Items
                .AnyAsync(item => item.CatMat == dto.CatMat);

            if (itemExistente)
            {
                throw new InvalidOperationException($"Já existe um item cadastrado com o CATMAT {dto.CatMat}.");
            }


            var novoItem = new Item
            {
                Nome = dto.Nome,
                Descricao = dto.Descricao,
                CatMat = dto.CatMat,
                Especificacao = dto.Especificacao,
                LinkImagem = dto.LinkImagem,
                PrecoSugerido = dto.PrecoSugerido,
                IsActive = dto.IsActive
            };

            await _context.Items.AddAsync(novoItem);
            await _context.SaveChangesAsync();

            return new ItemDto
            {
                Id = novoItem.Id,
                Nome = novoItem.Nome,
                Descricao = novoItem.Descricao,
                CatMat = novoItem.CatMat,
                Especificacao = novoItem.Especificacao,
                LinkImagem = novoItem.LinkImagem,
                PrecoSugerido = novoItem.PrecoSugerido,
                IsActive = novoItem.IsActive
            };
        }

        public async Task<bool> DeleteItemAsync(long id)
        {
            var item = await _context.Items.FindAsync(id);

            if (item == null)
            {
                _logger.LogWarning("Tentativa de deletar item com ID {Id}, mas não foi encontrado.", id);
                return false;
            }

            _context.Items.Remove(item);

            await _context.SaveChangesAsync();

            _logger.LogInformation("Item com ID {Id} foi deletado com sucesso.", id);
            return true;
        }

        public async Task<IEnumerable<ItemDto>?> GetItensSemelhantesAsync(long id)
        {
            _logger.LogInformation("Iniciando busca de itens semelhantes ao item de ID {Id}", id);

            try
            {
                var itemOriginal = await _context.Items.FindAsync(id);

                if (itemOriginal == null)
                {
                    _logger.LogWarning("Item original com ID {Id} não encontrado.", id);
                    return null;
                }

                var nomeParaBusca = itemOriginal.Nome;
                _logger.LogInformation("Item original encontrado: '{Nome}'. Buscando por semelhantes.", nomeParaBusca);

                var itensSemelhantes = await _context.Items
                    .AsNoTracking()
                    .Where(item => item.Nome == nomeParaBusca && item.Id != id)
                    .ToListAsync();

                _logger.LogInformation("Encontrados {Count} itens semelhantes.", itensSemelhantes.Count);

                var itensDto = itensSemelhantes.Select(item => new ItemDto
                {
                    Id = item.Id,
                    Nome = item.Nome,
                    Descricao = item.Descricao,
                    CatMat = item.CatMat,
                    // TODO: o link abaixo deve estar em variável de ambiente
                    LinkImagem = string.IsNullOrWhiteSpace(item.LinkImagem)
                        ? item.LinkImagem
                        : $"http://localhost:8088/images/{item.LinkImagem}",
                    PrecoSugerido = item.PrecoSugerido,
                    Especificacao = item.Especificacao,
                    IsActive = item.IsActive
                }).ToList();

                return itensDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar itens semelhantes para o ID {Id}.", id);
                throw;
            }
        }

        public async Task<ItemDto> AtualizarImagemAsync(long id, IFormFile imagem)
        {
            var item = await _context.Items.FindAsync(id);
            if (item == null)
            {
                return null;
            }

            var extensao = Path.GetExtension(imagem.FileName);
            var novoNomeArquivo = $"{item.CatMat}{extensao}";
            var caminhoParaSalvar = Path.Combine("/app/uploads", novoNomeArquivo);

            if (!string.IsNullOrEmpty(item.LinkImagem))
            {
                var caminhoImagemAntiga = Path.Combine("/app/uploads", item.LinkImagem);
                if (File.Exists(caminhoImagemAntiga))
                {
                    File.Delete(caminhoImagemAntiga);
                }
            }

            await using (var stream = new FileStream(caminhoParaSalvar, FileMode.Create))
            {
                await imagem.CopyToAsync(stream);
            }

            item.LinkImagem = novoNomeArquivo;
            await _context.SaveChangesAsync();

            return new ItemDto
            {
                Id = item.Id,
                Nome = item.Nome,
                Descricao = item.Descricao,
                CatMat = item.CatMat,
                // TODO: o link abaixo deve estar em variável de ambiente
                LinkImagem = string.IsNullOrWhiteSpace(item.LinkImagem)
                        ? item.LinkImagem
                        : $"http://localhost:8088/images/{item.LinkImagem}",
                PrecoSugerido = item.PrecoSugerido,
                Especificacao = item.Especificacao,
                IsActive = item.IsActive
            };
        }

        public async Task<bool> RemoverImagemAsync(long id)
        {
            var item = await _context.Items.FindAsync(id);
            if (item == null)
            {
                return false; 
            }

            if (!string.IsNullOrEmpty(item.LinkImagem))
            {
                var caminhoDoArquivo = Path.Combine("/app/uploads", item.LinkImagem);
                if (File.Exists(caminhoDoArquivo))
                {
                    File.Delete(caminhoDoArquivo);
                    _logger.LogInformation("Arquivo físico '{FileName}' deletado.", item.LinkImagem);
                }
            }

            item.LinkImagem = null;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Link da imagem para o item ID {Id} removido do banco.", id);
            return true;
        }
    }
}