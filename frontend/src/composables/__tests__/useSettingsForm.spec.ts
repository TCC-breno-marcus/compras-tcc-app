import { beforeEach, describe, expect, it, vi } from 'vitest'
import { createPinia, setActivePinia } from 'pinia'
import { nextTick, ref } from 'vue'

const toastAddMock = vi.fn()
const confirmRequireMock = vi.fn()

vi.mock('primevue/usetoast', () => ({
  useToast: () => ({
    add: toastAddMock,
  }),
}))

vi.mock('primevue', () => ({
  useConfirm: () => ({
    require: confirmRequireMock,
  }),
}))

import { useSettingStore } from '@/features/settings/stores/settingStore'
import { useSettingsForm } from '../useSettingsForm'

describe('Composable: useSettingsForm', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
  })

  const baseSettings = {
    prazoSubmissao: '2026-12-31',
    maxQuantidadePorItem: 3,
    maxItensDiferentesPorSolicitacao: 5,
    emailContatoPrincipal: 'contato@sistema.com',
    emailParaNotificacoes: 'notif@sistema.com',
  }

  it('deve refletir mudanças em isDirty', async () => {
    const settingStore = useSettingStore()
    settingStore.settings = { ...baseSettings }

    const form = useSettingsForm(ref({}))
    await nextTick()

    expect(form.isDirty.value).toBe(false)
    form.formData.value.maxQuantidadePorItem = 99
    expect(form.isDirty.value).toBe(true)
  })

  it('deve validar formulário e impedir save inválido', async () => {
    const settingStore = useSettingStore()
    settingStore.settings = { ...baseSettings }
    const form = useSettingsForm(ref({}))
    await nextTick()

    form.formData.value.prazoSubmissao = undefined
    form.handleSave()

    expect(confirmRequireMock).not.toHaveBeenCalled()
    expect(toastAddMock).toHaveBeenCalled()
  })

  it('deve abrir confirmação e salvar quando válido', async () => {
    const settingStore = useSettingStore()
    settingStore.settings = { ...baseSettings }
    vi.spyOn(settingStore, 'updateSettings').mockResolvedValue(true)

    const form = useSettingsForm(ref({}))
    await nextTick()

    form.handleSave()
    expect(confirmRequireMock).toHaveBeenCalledTimes(1)

    const args = confirmRequireMock.mock.calls[0][0]
    await args.accept()

    expect(settingStore.updateSettings).toHaveBeenCalledTimes(1)
    expect(toastAddMock).toHaveBeenCalledWith(expect.objectContaining({ severity: 'success' }))
  })

  it('deve restaurar backup no cancel', async () => {
    const settingStore = useSettingStore()
    settingStore.settings = { ...baseSettings }
    const form = useSettingsForm(ref({}))
    await nextTick()

    form.formData.value.emailContatoPrincipal = 'outro@sistema.com'
    form.handleCancel()

    expect(form.formData.value.emailContatoPrincipal).toBe('contato@sistema.com')
    expect(form.isEditing.value).toBe(false)
  })
})

