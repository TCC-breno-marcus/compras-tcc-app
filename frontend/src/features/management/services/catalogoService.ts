// src/features/management/services/catalogoService.ts

import { apiClient } from '@/service/apiClient'
import type {
  CatalogoParams,
  Item,
  ItemParams,
  PaginatedResponse,
} from '@/features/management/types'
import imageCompression from 'browser-image-compression'

interface ICatalogoService {
  getItens(params?: CatalogoParams): Promise<PaginatedResponse<Item>>
  getItemById(id: number): Promise<Item>
  getItensSemelhantes(id: number): Promise<Item[]>
  editarItem(id: number, params: ItemParams): Promise<Item>
  atualizarImagemItem(id: number, arquivo: File): Promise<Item>
  removerImagemItem(id: number): Promise<void>
  criarItem(params: ItemParams): Promise<Item>
}

/**
 * Comprimir imagem para melhor otimização de espaço
 * @param arquivoOriginal Arquivo original
 */
async function _processarImagem(arquivoOriginal: File): Promise<File> {
  // console.log(`Tamanho original: ${(arquivoOriginal.size / 1024 / 1024).toFixed(2)} MB`)

  const options = {
    maxSizeMB: 1,
    maxWidthOrHeight: 720,
    useWebWorker: true,
    fileType: 'image/webp',
  }

  const arquivoComprimido = await imageCompression(arquivoOriginal, options)
  // console.log(`Tamanho comprimido: ${(arquivoComprimido.size / 1024 / 1024).toFixed(2)} MB`)

  const nomeOriginal = arquivoOriginal.name
  const ultimoPonto = nomeOriginal.lastIndexOf('.')
  const nomeBase = ultimoPonto > 0 ? nomeOriginal.substring(0, ultimoPonto) : nomeOriginal
  const nomeFinalWebp = `${nomeBase}.webp`

  return new File([arquivoComprimido], nomeFinalWebp, { type: 'image/webp' })
}

export const catalogoService: ICatalogoService = {
  /**
   * Busca itens do catálogo de forma paginada e com filtros.
   * @param params Um objeto com os filtros e paginação.
   */
  async getItens(params) {
    const response = await apiClient.get<PaginatedResponse<Item>>('/catalogo', { params })
    return response.data
  },

  /**
   * Busca dados de um item específico do catálogo através de seu ID.
   * @param id ID do item.
   */
  async getItemById(id) {
    const response = await apiClient.get<Item>(`/catalogo/${id}`)
    return response.data
  },

  /**
   * Busca items com mesmo nome do item com ID informado.
   * @param id ID do item.
   */
  async getItensSemelhantes(id) {
    const response = await apiClient.get<Item[]>(`/catalogo/${id}/itens-semelhantes`)
    return response.data
  },

  /**
   * Edita um item.
   * @param id ID do item a editar.
   * @param params Objeto com os dados a serem alterados.
   */
  async editarItem(id, params) {
    const response = await apiClient.put<Item>(`/catalogo/${id}`, params)
    return response.data
  },

  /**
   * Cria um item.
   * @param params Objeto com os dados do novo item.
   */
  async criarItem(params) {
    const response = await apiClient.post<Item>(`/catalogo`, {
      ...params,
      linkImagem: ''
    })
    return response.data
  },

  /**
   * Atualiza a imagem de um item.
   * @param id ID do item.
   * @param arquivo Arquivo de imagem.
   */
  async atualizarImagemItem(id, arquivo) {
    const arquivoFinal = await _processarImagem(arquivo)

    const formData = new FormData()
    formData.append('imagem', arquivoFinal, arquivoFinal.name || 'upload.jpg')

    const response = await apiClient.post<Item>(`/catalogo/${id}/imagem`, formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    })
    return response.data
  },

  /**
   * Remove a imagem de um item.
   * @param id ID do item.
   */
  async removerImagemItem(id) {
    await apiClient.delete(`/catalogo/${id}/imagem`)
  },
}
