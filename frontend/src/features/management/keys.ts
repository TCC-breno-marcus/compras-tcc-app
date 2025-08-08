import type { InjectionKey, Ref } from 'vue'

export interface managementContext {
}

export const managementContextKey: InjectionKey<Ref<managementContext>> = Symbol()
