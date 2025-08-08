import type { InjectionKey } from 'vue'

export interface SolicitationContext {
  dialogMode: string
  isGeneral: boolean
}

export const SolicitationContextKey: InjectionKey<SolicitationContext> = Symbol()
