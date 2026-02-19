using System.Security.Claims;
using ComprasTccApp.Backend.Enums;
using ComprasTccApp.Backend.Helpers;
using ComprasTccApp.Backend.Models.Entities.Items;
using ComprasTccApp.Models.Entities.Historicos;
using Database;
using Microsoft.EntityFrameworkCore;
using Models.Dtos;
using Services.Interfaces;

namespace Services
{
    public class CatalogoService : ICatalogoService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<CatalogoService> _logger;
        private readonly string _IMAGE_BASE_URL;

        public CatalogoService(
            AppDbContext context,
            ILogger<CatalogoService> logger,
            IConfiguration configuration
        )
        {
            _context = context;
            _logger = logger;
            _IMAGE_BASE_URL = configuration["IMAGE_BASE_URL"] ?? "";
        }

        /// <summary>
        /// Consulta itens do catálogo com filtros, ordenação e paginação.
        /// Quando <paramref name="searchTerm"/> é informado, aplica busca textual ampla e ignora filtros individuais.
        /// </summary>
        /// <param name="id">Identificador exato do item para filtro.</param>
        /// <param name="catMat">Código CATMAT parcial ou completo para filtro.</param>
        /// <param name="nome">Nome parcial do item para filtro.</param>
        /// <param name="descricao">Descrição parcial para filtro.</param>
        /// <param name="categoriaId">Lista de categorias permitidas na consulta.</param>
        /// <param name="especificacao">Especificação parcial para filtro.</param>
        /// <param name="isActive">Status de ativação do item.</param>
        /// <param name="searchTerm">Termo livre para busca textual ampla.</param>
        /// <param name="pageNumber">Número da página (base 1).</param>
        /// <param name="pageSize">Quantidade de registros por página.</param>
        /// <param name="sortOrder">Ordenação por nome: <c>asc</c> ou <c>desc</c>.</param>
        /// <returns>Resultado paginado contendo itens e metadados de paginação.</returns>
        /// <exception cref="Exception">Propaga falhas inesperadas durante a consulta ao banco.</exception>
        public async Task<PaginatedResultDto<ItemDto>> GetAllItensAsync(
            long? id,
            string? catMat,
            string? nome,
            string? descricao,
            List<long> categoriaId,
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
                var query = _context.Items.Include(item => item.Categoria).AsQueryable();

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    query = query.Where(item =>
                        item.Nome.ToLower().Contains(searchTerm.ToLower())
                        || item.Descricao.ToLower().Contains(searchTerm.ToLower())
                        || item.CatMat.Contains(searchTerm.ToLower())
                        || item.Especificacao.ToLower().Contains(searchTerm.ToLower())
                        || item.Categoria.Nome.ToLower().Contains(searchTerm.ToLower()) // validar e fzr esse commit na outra branch
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
                        query = query.Where(item =>
                            item.Descricao.ToLower().Contains(descricao.ToLower())
                        );
                    }
                    if (categoriaId != null && categoriaId.Count != 0)
                    {
                        query = query.Where(item => categoriaId.Contains(item.CategoriaId));
                    }
                    if (!string.IsNullOrWhiteSpace(especificacao))
                    {
                        query = query.Where(item =>
                            item.Especificacao.ToLower().Contains(especificacao.ToLower())
                        );
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

                var itensDto = itensPaginados
                    .Select(item => new ItemDto
                    {
                        Id = item.Id,
                        Nome = item.Nome,
                        Descricao = item.Descricao,
                        CatMat = item.CatMat,
                        Categoria = new CategoriaDto
                        {
                            Id = item.Categoria.Id,
                            Nome = item.Categoria.Nome,
                            Descricao = item.Categoria.Descricao,
                            IsActive = item.Categoria.IsActive,
                        },
                        LinkImagem = string.IsNullOrWhiteSpace(item.LinkImagem)
                            ? item.LinkImagem
                            : $"{_IMAGE_BASE_URL}{item.LinkImagem}",
                        PrecoSugerido = item.PrecoSugerido,
                        Especificacao = item.Especificacao,
                        IsActive = item.IsActive,
                    })
                    .ToList();

                _logger.LogInformation(
                    "Busca concluída. Retornando {Count} de um total de {Total} itens.",
                    itensDto.Count,
                    totalCount
                );

                return new PaginatedResultDto<ItemDto>(itensDto, totalCount, pageNumber, pageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Ocorreu um erro ao buscar os itens do banco de dados com filtros."
                );
                throw;
            }
        }

        /// <summary>
        /// Atualiza os dados de um item e registra histórico quando houver alteração efetiva de campos.
        /// </summary>
        /// <param name="id">Identificador do item a ser editado.</param>
        /// <param name="updateDto">Dados de atualização parcial do item.</param>
        /// <param name="user">Usuário autenticado responsável pela alteração.</param>
        /// <returns>Item atualizado; retorna <see langword="null"/> quando o item não existe.</returns>
        /// <exception cref="InvalidOperationException">
        /// Lançada quando a categoria final associada ao item não é encontrada após a atualização.
        /// </exception>
        /// <exception cref="FormatException">Lançada quando o claim de identificador do usuário é inválido.</exception>
        public async Task<ItemDto?> EditarItemAsync(
            int id,
            ItemUpdateDto updateDto,
            ClaimsPrincipal user
        )
        {
            var itemDoBanco = await _context.Items.FirstOrDefaultAsync(i => i.Id == id);

            if (itemDoBanco == null)
            {
                _logger.LogWarning("Tentativa de editar item com ID {Id} não encontrado.", id);
                return null;
            }

            var catalogoCategorias = _context
                .Categorias.Where(c =>
                    c.Id == itemDoBanco.CategoriaId || c.Id == updateDto.CategoriaId
                )
                .ToDictionary(c => c.Id);
            var alteracoes = AuditHelper.CompareItem(itemDoBanco, updateDto, catalogoCategorias);

            if (!string.IsNullOrEmpty(updateDto.Nome))
                itemDoBanco.Nome = updateDto.Nome;

            if (!string.IsNullOrEmpty(updateDto.CatMat))
                itemDoBanco.CatMat = updateDto.CatMat;

            if (!string.IsNullOrEmpty(updateDto.Descricao))
                itemDoBanco.Descricao = updateDto.Descricao;

            if (updateDto.CategoriaId.HasValue)
            {
                itemDoBanco.CategoriaId = updateDto.CategoriaId.Value;
            }

            if (updateDto.Especificacao != null)
                itemDoBanco.Especificacao = updateDto.Especificacao;

            if (updateDto.PrecoSugerido.HasValue)
                itemDoBanco.PrecoSugerido = updateDto.PrecoSugerido.Value;

            if (updateDto.IsActive.HasValue)
                itemDoBanco.IsActive = updateDto.IsActive.Value;

            if (alteracoes.Any())
            {
                var pessoaId = long.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

                var historico = new HistoricoItem
                {
                    ItemId = itemDoBanco.Id,
                    DataOcorrencia = DateTime.UtcNow,
                    PessoaId = pessoaId,
                    Acao = AcaoHistoricoEnum.Edicao,
                    Detalhes = string.Join(" | ", alteracoes),
                    Observacoes = null,
                };
                await _context.HistoricoItens.AddAsync(historico);
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Item com ID {Id} atualizado.", id);

            var categoriaDoItem = await _context
                .Categorias.AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == itemDoBanco.CategoriaId);

            if (categoriaDoItem == null)
            {
                throw new InvalidOperationException(
                    "A categoria associada ao item não foi encontrada após a criação."
                );
            }

            return new ItemDto
            {
                Id = itemDoBanco.Id,
                Nome = itemDoBanco.Nome,
                Descricao = itemDoBanco.Descricao,
                CatMat = itemDoBanco.CatMat,
                Categoria = new CategoriaDto
                {
                    Id = categoriaDoItem.Id,
                    Nome = categoriaDoItem.Nome,
                    Descricao = categoriaDoItem.Descricao,
                    IsActive = categoriaDoItem.IsActive,
                },
                PrecoSugerido = itemDoBanco.PrecoSugerido,
                Especificacao = itemDoBanco.Especificacao,
                LinkImagem = itemDoBanco.LinkImagem,
                IsActive = itemDoBanco.IsActive,
            };
        }

        /// <summary>
        /// Importa itens em lote, atualizando registros existentes por CATMAT e inserindo novos itens.
        /// </summary>
        /// <param name="itensParaImportar">Coleção de itens de origem externa para sincronização do catálogo.</param>
        /// <returns>Uma <see cref="Task"/> representando a conclusão da operação.</returns>
        public async Task ImportarItensAsync(IEnumerable<ItemImportacaoDto> itensParaImportar)
        {
            var codigosParaVerificar = itensParaImportar.Select(dto => dto.Codigo).Distinct();

            var itensExistentes = await _context
                .Items.Where(item => codigosParaVerificar.Contains(item.CatMat))
                .ToDictionaryAsync(item => item.CatMat, item => item);

            _logger.LogInformation(
                "Verificando {Count} itens. {ExistingCount} já existem no banco e serão atualizados.",
                itensParaImportar.Count(),
                itensExistentes.Count
            );

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
                        CategoriaId = dto.CategoriaId,
                        LinkImagem = dto.LinkImagem,
                        IsActive = true,
                    };
                    await _context.Items.AddAsync(novoItem);
                }
            }

            var registrosAfetados = await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Importação concluída. {Count} registros foram afetados no banco de dados.",
                registrosAfetados
            );
        }

        /// <summary>
        /// Associa imagens aos itens do catálogo com base no nome do arquivo (CATMAT) e persiste os links correspondentes.
        /// </summary>
        /// <param name="caminhoDasImagens">Diretório local contendo os arquivos de imagem.</param>
        /// <returns>Resumo textual da quantidade de itens atualizados e imagens não correspondentes.</returns>
        /// <exception cref="DirectoryNotFoundException">Lançada quando o diretório informado não existe.</exception>
        public async Task<string> PopularImagensAsync(string caminhoDasImagens)
        {
            _logger.LogInformation(
                "Iniciando rotina para popular imagens a partir do caminho: {Caminho}",
                caminhoDasImagens
            );

            if (!Directory.Exists(caminhoDasImagens))
            {
                var erroMsg =
                    $"ERRO: O diretório de imagens não foi encontrado em '{caminhoDasImagens}'.";
                _logger.LogError(erroMsg);
                throw new DirectoryNotFoundException(erroMsg);
            }

            var arquivosDeImagem = Directory.GetFiles(caminhoDasImagens);
            int produtosAtualizados = 0;
            int produtosNaoEncontrados = 0;

            _logger.LogInformation(
                "Encontrados {Count} arquivos de imagem para processar.",
                arquivosDeImagem.Length
            );

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
                resumo =
                    $"SUCESSO! {produtosAtualizados} produtos foram atualizados no banco de dados.";
                _logger.LogInformation(resumo);
            }
            else
            {
                resumo =
                    "Nenhum produto foi atualizado. Verifique se os nomes dos arquivos correspondem aos códigos dos itens.";
                _logger.LogWarning(resumo);
            }

            if (produtosNaoEncontrados > 0)
            {
                resumo +=
                    $" {produtosNaoEncontrados} imagens não encontraram um produto correspondente.";
                _logger.LogWarning(
                    "{Count} imagens não encontraram um produto correspondente.",
                    produtosNaoEncontrados
                );
            }

            return resumo;
        }

        /// <summary>
        /// Obtém um item por identificador incluindo dados da categoria.
        /// </summary>
        /// <param name="id">Identificador do item.</param>
        /// <returns>Item encontrado; retorna <see langword="null"/> quando inexistente.</returns>
        /// <exception cref="Exception">Propaga falhas inesperadas durante a leitura no banco.</exception>
        public async Task<ItemDto?> GetItemByIdAsync(long id)
        {
            _logger.LogInformation("Iniciando busca de um item pelo ID...");
            _logger.LogWarning("ID {Id}", id);

            try
            {
                var item = await _context
                    .Items.Include(item => item.Categoria)
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
                    Categoria = new CategoriaDto
                    {
                        Id = item.Categoria.Id,
                        Nome = item.Categoria.Nome,
                        Descricao = item.Categoria.Descricao,
                        IsActive = item.Categoria.IsActive,
                    },
                    LinkImagem = string.IsNullOrWhiteSpace(item.LinkImagem)
                        ? item.LinkImagem
                        : $"{_IMAGE_BASE_URL}{item.LinkImagem}",
                    PrecoSugerido = item.PrecoSugerido,
                    Especificacao = item.Especificacao,
                    IsActive = item.IsActive,
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar item por ID {Id}.", id);
                throw;
            }
        }

        /// <summary>
        /// Cria um novo item no catálogo, valida unicidade de CATMAT e registra histórico de criação.
        /// </summary>
        /// <param name="dto">Dados de criação do item.</param>
        /// <param name="user">Usuário autenticado responsável pela criação.</param>
        /// <returns>DTO do item criado com dados de categoria.</returns>
        /// <exception cref="InvalidOperationException">
        /// Lançada quando já existe item com o mesmo CATMAT ou quando a categoria associada não é encontrada.
        /// </exception>
        /// <exception cref="FormatException">Lançada quando o claim de identificador do usuário é inválido.</exception>
        public async Task<ItemDto> CriarItemAsync(CreateItemDto dto, ClaimsPrincipal user)
        {
            var itemExistente = await _context.Items.AnyAsync(item => item.CatMat == dto.CatMat);

            if (itemExistente)
            {
                throw new InvalidOperationException(
                    $"Já existe um item cadastrado com o CATMAT {dto.CatMat}."
                );
            }

            var novoItem = new Item
            {
                Nome = dto.Nome,
                Descricao = dto.Descricao,
                CatMat = dto.CatMat,
                Especificacao = dto.Especificacao,
                CategoriaId = dto.CategoriaId,
                LinkImagem = dto.LinkImagem,
                PrecoSugerido = dto.PrecoSugerido,
                IsActive = dto.IsActive,
            };

            await _context.Items.AddAsync(novoItem);
            await _context.SaveChangesAsync();

            var categoriaDoItem = await _context
                .Categorias.AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == novoItem.CategoriaId);

            if (categoriaDoItem == null)
            {
                throw new InvalidOperationException(
                    "A categoria associada ao item não foi encontrada após a criação."
                );
            }

            var pessoaId = long.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var historico = new HistoricoItem
            {
                ItemId = novoItem.Id,
                DataOcorrencia = DateTime.UtcNow,
                PessoaId = pessoaId,
                Acao = AcaoHistoricoEnum.Criacao,
                Detalhes = "Item criado.",
                Observacoes = null,
            };
            await _context.HistoricoItens.AddAsync(historico);
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
                IsActive = novoItem.IsActive,
                Categoria = new CategoriaDto
                {
                    Id = categoriaDoItem.Id,
                    Nome = categoriaDoItem.Nome,
                    Descricao = categoriaDoItem.Descricao,
                    IsActive = categoriaDoItem.IsActive,
                },
            };
        }

        /// <summary>
        /// Remove um item do catálogo aplicando hard delete para itens sem uso e soft delete para itens já solicitados.
        /// </summary>
        /// <param name="id">Identificador do item a remover.</param>
        /// <param name="user">Usuário autenticado responsável pela remoção.</param>
        /// <returns>
        /// Tupla indicando sucesso da operação e mensagem de resultado para o fluxo de negócio.
        /// </returns>
        /// <exception cref="FormatException">Lançada quando o claim de identificador do usuário é inválido no fluxo de soft delete.</exception>
        public async Task<(bool sucesso, string mensagem)> DeleteItemAsync(
            long id,
            ClaimsPrincipal user
        )
        {
            var item = await _context.Items.FindAsync(id);

            if (item == null)
            {
                _logger.LogWarning(
                    "Tentativa de deletar item com ID {Id}, mas não foi encontrado.",
                    id
                );
                return (false, "Item não encontrado.");
            }

            var jaFoiSolicitado = await _context.SolicitacaoItens.AnyAsync(si => si.ItemId == id);

            if (jaFoiSolicitado)
            {
                // Soft delete - item já tem histórico
                item.IsActive = false;

                var pessoaId = long.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var historico = new HistoricoItem
                {
                    ItemId = item.Id,
                    DataOcorrencia = DateTime.UtcNow,
                    PessoaId = pessoaId,
                    Acao = AcaoHistoricoEnum.Remocao,
                    Detalhes = "Item desativado pois já foi utilizado em solicitações.",
                    Observacoes = null,
                };
                await _context.HistoricoItens.AddAsync(historico);

                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Item com ID {Id} foi desativado (soft delete) pois já foi utilizado em solicitações.",
                    id
                );
                return (true, "Item foi desativado pois já foi utilizado em solicitações.");
            }
            else
            {
                // Hard delete - item nunca foi usado
                _context.Items.Remove(item);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Item com ID {Id} foi deletado permanentemente.", id);
                return (true, "Item foi removido permanentemente.");
            }
        }

        /// <summary>
        /// Lista itens semelhantes ao item de referência usando igualdade de nome e excluindo o próprio item.
        /// </summary>
        /// <param name="id">Identificador do item de referência.</param>
        /// <returns>Lista de itens semelhantes; retorna <see langword="null"/> quando o item base não existe.</returns>
        /// <exception cref="Exception">Propaga falhas inesperadas durante a consulta no banco.</exception>
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
                _logger.LogInformation(
                    "Item original encontrado: '{Nome}'. Buscando por semelhantes.",
                    nomeParaBusca
                );

                var itensSemelhantes = await _context
                    .Items.AsNoTracking()
                    .Include(item => item.Categoria)
                    .Where(item => item.Nome == nomeParaBusca && item.Id != id)
                    .ToListAsync();

                _logger.LogInformation(
                    "Encontrados {Count} itens semelhantes.",
                    itensSemelhantes.Count
                );

                var itensDto = itensSemelhantes
                    .Select(item => new ItemDto
                    {
                        Id = item.Id,
                        Nome = item.Nome,
                        Descricao = item.Descricao,
                        CatMat = item.CatMat,
                        Categoria = new CategoriaDto
                        {
                            Id = item.Categoria.Id,
                            Nome = item.Categoria.Nome,
                            Descricao = item.Categoria.Descricao,
                            IsActive = item.Categoria.IsActive,
                        },
                        LinkImagem = string.IsNullOrWhiteSpace(item.LinkImagem)
                            ? item.LinkImagem
                            : $"{_IMAGE_BASE_URL}{item.LinkImagem}",
                        PrecoSugerido = item.PrecoSugerido,
                        Especificacao = item.Especificacao,
                        IsActive = item.IsActive,
                    })
                    .ToList();

                return itensDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar itens semelhantes para o ID {Id}.", id);
                throw;
            }
        }

        /// <summary>
        /// Atualiza a imagem de um item, substitui o arquivo físico anterior e registra histórico da alteração.
        /// </summary>
        /// <param name="id">Identificador do item.</param>
        /// <param name="imagem">Arquivo de imagem enviado para associação ao item.</param>
        /// <param name="user">Usuário autenticado responsável pela alteração.</param>
        /// <returns>Item atualizado com o link da nova imagem; retorna <see langword="null"/> quando o item não existe.</returns>
        /// <exception cref="IOException">Lançada quando ocorre falha de leitura/escrita no sistema de arquivos.</exception>
        /// <exception cref="FormatException">Lançada quando o claim de identificador do usuário é inválido.</exception>
        public async Task<ItemDto?> AtualizarImagemAsync(
            long id,
            IFormFile imagem,
            ClaimsPrincipal user
        )
        {
            var item = await _context
                .Items.Include(item => item.Categoria)
                .FirstOrDefaultAsync(item => item.Id == id);
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

            var pessoaId = long.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var historico = new HistoricoItem
            {
                ItemId = item.Id,
                DataOcorrencia = DateTime.UtcNow,
                PessoaId = pessoaId,
                Acao = AcaoHistoricoEnum.Edicao,
                Detalhes = "Imagem do item alterada.",
                Observacoes = null,
            };
            await _context.HistoricoItens.AddAsync(historico);

            await _context.SaveChangesAsync();

            return new ItemDto
            {
                Id = item.Id,
                Nome = item.Nome,
                Descricao = item.Descricao,
                CatMat = item.CatMat,
                Categoria = new CategoriaDto
                {
                    Id = item.Categoria.Id,
                    Nome = item.Categoria.Nome,
                    Descricao = item.Categoria.Descricao,
                    IsActive = item.Categoria.IsActive,
                },
                LinkImagem = string.IsNullOrWhiteSpace(item.LinkImagem)
                    ? item.LinkImagem
                    : $"{_IMAGE_BASE_URL}{item.LinkImagem}",
                PrecoSugerido = item.PrecoSugerido,
                Especificacao = item.Especificacao,
                IsActive = item.IsActive,
            };
        }

        /// <summary>
        /// Remove a imagem associada ao item no banco e no disco (quando existente), registrando histórico da ação.
        /// </summary>
        /// <param name="id">Identificador do item.</param>
        /// <param name="user">Usuário autenticado responsável pela remoção.</param>
        /// <returns><see langword="true"/> quando o item é encontrado e atualizado; caso contrário, <see langword="false"/>.</returns>
        /// <exception cref="FormatException">Lançada quando o claim de identificador do usuário é inválido.</exception>
        public async Task<bool> RemoverImagemAsync(long id, ClaimsPrincipal user)
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
                    _logger.LogInformation(
                        "Arquivo físico '{FileName}' deletado.",
                        item.LinkImagem
                    );
                }
            }

            item.LinkImagem = "";

            var pessoaId = long.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var historico = new HistoricoItem
            {
                ItemId = item.Id,
                DataOcorrencia = DateTime.UtcNow,
                PessoaId = pessoaId,
                Acao = AcaoHistoricoEnum.Edicao,
                Detalhes = "Imagem do item removida.",
                Observacoes = null,
            };
            await _context.HistoricoItens.AddAsync(historico);

            await _context.SaveChangesAsync();

            _logger.LogInformation("Link da imagem para o item ID {Id} removido do banco.", id);
            return true;
        }

        /// <summary>
        /// Recupera o histórico de alterações de um item ordenado da ocorrência mais recente para a mais antiga.
        /// </summary>
        /// <param name="itemId">Identificador do item para consulta do histórico.</param>
        /// <returns>Lista de eventos do histórico; retorna <see langword="null"/> quando o item não existe.</returns>
        public async Task<List<HistoricoItemDto>?> GetHistoricoItemAsync(long itemId)
        {
            var item = await _context
                .Items.Include(item => item.Categoria)
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.Id == itemId);

            if (item == null)
            {
                _logger.LogWarning("Item com ID: {Id} não encontrado.", itemId);
                return null;
            }

            var historico = await _context
                .HistoricoItens.AsNoTracking()
                .Where(h => h.ItemId == itemId)
                .Include(h => h.Pessoa)
                .OrderByDescending(h => h.DataOcorrencia)
                .Select(h => new HistoricoItemDto
                {
                    Id = h.Id,
                    DataOcorrencia = h.DataOcorrencia,
                    Acao = h.Acao.ToString(),
                    Detalhes = h.Detalhes,
                    Observacoes = h.Observacoes,
                    NomePessoa = h.Pessoa.Nome,
                })
                .ToListAsync();

            return historico;
        }
    }
}
