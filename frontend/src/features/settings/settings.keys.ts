import type { InjectionKey, Ref } from 'vue'

export interface SettingsContext {
  isEditing: Ref<boolean>
  registerActions: (actions: { save: () => Promise<void>; cancel: () => void }) => void
}

export const SettingsContextKey: InjectionKey<SettingsContext> = Symbol('SettingsContext')
