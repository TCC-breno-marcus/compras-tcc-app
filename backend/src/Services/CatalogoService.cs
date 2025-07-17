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
            int pageNumber,
            int pageSize,
            string? sortOrder
        )
        {
            _logger.LogInformation("Iniciando busca avançada de itens com filtros e paginação...");

            try
            {
                var query = _context.Items.AsQueryable();

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
                    query = query.Where(item => item.Descricao.ToLower().Contains(especificacao.ToLower()));
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
                    LinkImagem = item.LinkImagem,
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

    }
}