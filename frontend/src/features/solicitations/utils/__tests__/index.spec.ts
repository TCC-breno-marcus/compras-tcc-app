import { describe, it, expect } from 'vitest'
import { transformSolicitation, getSolicitationStatusOptions } from '../index'
import type { Solicitation } from '../../types'

describe('Solicitations Utils', () => {

 const createMockSolicitation = (overrides?: Partial<Solicitation>): Solicitation => ({
    id: 1,
    dataCriacao: '2023-01-01',
    justificativaGeral: 'Preciso de um mouse',
    externalId: 'SOL-123',
    status: { id: 1, nome: 'Pendente', severity: 'info', icon: 'pi pi-clock' },
    solicitante: {
      id: 1,
      nome: 'Marcos',
      email: 'marcos@teste.com',
      cpf: '123',
      unidade: { id: 1, nome: 'Tecnologia', sigla: 'TI' }
    },
    itens: [],
    kpis: {} as any, 
    valorPorCategoria: {} as any,
    topItensPorValor: [],
    ...overrides
  } as Solicitation)


  describe('transformSolicitation', () => {
    
    it('deve definir typeDisplay como "Geral" se houver justificativaGeral', () => {
      const solicitation = createMockSolicitation({
        justificativaGeral: 'Justificativa válida'
      })

      const result = transformSolicitation(solicitation, 'mySolicitations')

      expect(result.typeDisplay).toBe('Geral')
    })

    it('deve definir typeDisplay como "Patrimonial" se justificativaGeral for vazia', () => {
      const solicitation = createMockSolicitation({
        justificativaGeral: '' 
      })

      const result = transformSolicitation(solicitation, 'mySolicitations')

      expect(result.typeDisplay).toBe('Patrimonial')
    })

    it('deve extrair a sigla do departamento corretamente', () => {
      const solicitation = createMockSolicitation()
      
      const result = transformSolicitation(solicitation, 'mySolicitations')

      expect(result.department).toBe('TI')
    })

    it('deve lidar com departamento indefinido (safe navigation)', () => {
      const solicitation = createMockSolicitation({
        solicitante: {
          nome: 'Marcos',
          unidade: undefined
        } as any
      })
      
      const result = transformSolicitation(solicitation, 'mySolicitations')

      expect(result.department).toBeUndefined()
    })

    describe('Contexto: allSolicitations (Visão do Gestor)', () => {
      it('deve incluir o campo "requester" com o nome do solicitante', () => {
        const solicitation = createMockSolicitation()

        const result = transformSolicitation(solicitation, 'allSolicitations')

        expect(result.requester).toBe('Marcos')
      })
    })

    describe('Contexto: mySolicitations (Visão do Solicitante)', () => {
      it('não deve incluir o campo "requester" (pois é redundante)', () => {
        const solicitation = createMockSolicitation()

        const result = transformSolicitation(solicitation, 'mySolicitations')

        expect(result.requester).toBeUndefined()
      })
    })

    it('deve preservar as outras propriedades originais da solicitação', () => {
      const solicitation = createMockSolicitation({ externalId: 'XYZ-999' })

      const result = transformSolicitation(solicitation, 'mySolicitations')

      expect(result.id).toBe(1)
      expect(result.externalId).toBe('XYZ-999')
      expect(result.status.id).toBe(1)
    })
  })

  // --- Testes do getSolicitationStatusOptions ---
  
  describe('getSolicitationStatusOptions', () => {
    
    it('deve retornar o objeto de status correto para um ID válido', () => {
      // Testando alguns IDs conhecidos do seu array
      const statusPendente = getSolicitationStatusOptions(1)
      const statusAprovada = getSolicitationStatusOptions(3)
      const statusRejeitada = getSolicitationStatusOptions(4)

      expect(statusPendente?.nome).toBe('Pendente')
      expect(statusPendente?.severity).toBe('info')

      expect(statusAprovada?.nome).toBe('Aprovada')
      expect(statusAprovada?.severity).toBe('success')

      expect(statusRejeitada?.nome).toBe('Rejeitada')
      expect(statusRejeitada?.severity).toBe('danger')
    })

    it('deve retornar null para um ID inexistente', () => {
      const statusInvalido = getSolicitationStatusOptions(999)
      expect(statusInvalido).toBeNull()
    })
    
    it('deve retornar null para ID 0 ou negativo', () => {
      expect(getSolicitationStatusOptions(0)).toBeNull()
      expect(getSolicitationStatusOptions(-1)).toBeNull()
    })
  })
})