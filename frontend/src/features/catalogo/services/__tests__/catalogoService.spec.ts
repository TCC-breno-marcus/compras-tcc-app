import { beforeEach, describe, expect, it, vi } from 'vitest'
import type { PaginatedResponse } from '@/types'
import type { Item } from '../../types'

const { apiClientMock, imageCompressionMock } = vi.hoisted(() => ({
  apiClientMock: {
    get: vi.fn(),
    post: vi.fn(),
    put: vi.fn(),
    delete: vi.fn(),
  },
  imageCompressionMock: vi.fn(),
}))

vi.mock('@/services/apiClient', () => ({
  apiClient: apiClientMock,
}))

vi.mock('browser-image-compression', () => ({
  default: imageCompressionMock,
}))

import { catalogoService } from '../catalogoService'

describe('Service: catalogoService', () => {
  const baseItem: Item = {
    id: 1,
    nome: 'ITEM TESTE',
    catMat: '123456',
    descricao: 'desc',
    especificacao: 'esp',
    categoria: { id: 1, nome: 'DIVERSOS', descricao: 'd', isActive: true },
    linkImagem: '',
    precoSugerido: 10,
    isActive: true,
  }

  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('deve serializar filtros e transformar nomes no getItens', async () => {
    const response: PaginatedResponse<Item> = {
      pageNumber: 1,
      pageSize: 10,
      totalCount: 1,
      totalPages: 1,
      data: [baseItem],
    }
    apiClientMock.get.mockResolvedValue({ data: response })

    const result = await catalogoService.getItens({
      searchTerm: 'item',
      nome: '',
      descricao: '',
      catMat: '',
      especificacao: '',
      categoriaId: [1, 2],
      status: 'ativo',
      sortOrder: 'asc',
      pageNumber: '1',
      pageSize: '10',
    })

    const [, config] = apiClientMock.get.mock.calls[0]
    const params = config.params as URLSearchParams
    expect(params.get('isActive')).toBe('true')
    expect(params.getAll('categoriaId')).toEqual(['1', '2'])
    expect(result.data[0].nome).toBe('Item Teste')
  })

  it('deve criar item enviando linkImagem vazio', async () => {
    apiClientMock.post.mockResolvedValue({ data: baseItem })

    await catalogoService.criarItem({
      nome: 'Novo',
      catMat: '123456',
      descricao: 'Desc',
      categoriaId: 1,
    })

    expect(apiClientMock.post).toHaveBeenCalledWith('/catalogo', {
      nome: 'Novo',
      catMat: '123456',
      descricao: 'Desc',
      categoriaId: 1,
      linkImagem: '',
    })
  })

  it('deve comprimir e enviar imagem no atualizarImagemItem', async () => {
    const original = new File(['abc'], 'foto.png', { type: 'image/png' })
    const compressed = new File(['xyz'], 'foto.webp', { type: 'image/webp' })
    imageCompressionMock.mockResolvedValue(compressed)
    apiClientMock.post.mockResolvedValue({ data: baseItem })

    await catalogoService.atualizarImagemItem(10, original)

    expect(imageCompressionMock).toHaveBeenCalledTimes(1)
    const [url, formData, config] = apiClientMock.post.mock.calls[0]
    expect(url).toBe('/catalogo/10/imagem')
    expect(formData).toBeInstanceOf(FormData)
    expect((formData as FormData).get('imagem')).toBeTruthy()
    expect(config.headers['Content-Type']).toBe('multipart/form-data')
  })

  it('deve lançar erro padronizado ao falhar histórico', async () => {
    apiClientMock.get.mockRejectedValue(new Error('boom'))

    await expect(catalogoService.getItemHistory(1)).rejects.toThrow(
      'Não foi possível buscar histórico do item.',
    )
  })
})

