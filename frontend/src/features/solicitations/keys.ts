import type { InjectionKey } from 'vue'

export interface SolicitationContext {
  dialogMode: string
  isGeneral: boolean
}
export const SolicitationContextKey: InjectionKey<SolicitationContext> = Symbol()

export interface SolicitationDetailsContext {
  isGeneral: boolean
}
export const SolicitationDetailsContextKey: InjectionKey<SolicitationDetailsContext> = Symbol()
