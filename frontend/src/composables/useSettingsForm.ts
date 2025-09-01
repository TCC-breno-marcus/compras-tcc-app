import { ref, watch, computed, type Ref } from 'vue'
import { dataHasBeenChanged } from '@/utils/objectUtils'
import { useToast } from 'primevue/usetoast'
import { useSettingStore } from '@/features/settings/stores/settingStore'
import type { Setting } from '@/features/settings/types'
import { formatDate, parseDateString } from '@/utils/dateUtils'
import { storeToRefs } from 'pinia'
import { SAVE_CONFIRMATION } from '@/utils/confirmationFactoryUtils'
import { useConfirm } from 'primevue'

/**
 * Composable para gerenciar a lógica de um formulário de configurações.
 * @param initialData A ref reativa da store com os dados originais.
 */
export function useSettingsForm(initialData: Ref<any>) {
  const settingStore = useSettingStore()
  const { settings } = storeToRefs(settingStore)

  const confirm = useConfirm()

  const toast = useToast()

  const isEditing = ref(false)
  const isLoading = ref(false)

  const formData = ref<Partial<Setting>>({})
  const formBackup = ref<Partial<Setting>>({})

  const isDirty = computed(() => dataHasBeenChanged(formBackup.value, formData.value))

  const prazoSubmissaoDatePicker = computed({
    get() {
      return formData.value.prazoSubmissao ? parseDateString(formData.value.prazoSubmissao) : null
    },
    set(newValue: Date | null) {
      if (formData.value) {
        formData.value.prazoSubmissao = newValue ? formatDate(newValue, 'iso') : undefined
      }
    },
  })

  watch(
    settings,
    (newSettings) => {
      if (newSettings) {
        formData.value = JSON.parse(JSON.stringify(newSettings))
        formBackup.value = JSON.parse(JSON.stringify(newSettings))
      }
    },
    { immediate: true, deep: true },
  )

  const acceptSaveChanges = async () => {
    if (!formData.value) return

    isLoading.value = true
    try {
      const success = await settingStore.updateSettings(formData.value)
      if (success) {
        formBackup.value = JSON.parse(JSON.stringify(formData.value))
      }
      toast.add({
        severity: 'success',
        summary: 'Sucesso',
        detail: 'O item foi salvo com sucesso.',
        life: 3000,
      })
      isEditing.value = false
    } catch (err) {
      console.error('Erro ao salvar as alterações:', err)
      toast.add({
        severity: 'error',
        summary: 'Erro',
        detail: 'Não foi possível salvar as alterações.',
        life: 3000,
      })
    } finally {
      isLoading.value = false
    }
  }

  const handleSave = () => {
    if (!isFormValid(formData.value)) return
    confirm.require({
      ...SAVE_CONFIRMATION,
      accept: async () => acceptSaveChanges(),
    })
  }

  const handleCancel = () => {
    formData.value = JSON.parse(JSON.stringify(formBackup.value))
    isEditing.value = false
  }

  const isFormValid = (data: Partial<Setting>) => {
    let isValid = true
    if (!data.prazoSubmissao) {
      isValid = false
      toast.add({
        severity: 'error',
        summary: 'Campo Obrigatório',
        detail: 'O prazo final para criação/edição não pode ser vazio.',
        life: 3000,
      })
      isValid = false
    }

    if (!data.maxQuantidadePorItem || data.maxQuantidadePorItem < 1) {
      isValid = false
      toast.add({
        severity: 'error',
        summary: 'Campo Obrigatório',
        detail: 'A quantidade máxima por item deve ser maior que um (1).',
        life: 3000,
      })
      isValid = false
    }

    if (!data.maxItensDiferentesPorSolicitacao || data.maxItensDiferentesPorSolicitacao < 1) {
      isValid = false
      toast.add({
        severity: 'error',
        summary: 'Campo Obrigatório',
        detail: 'A quantidade máxima de itens diferentes na solicitação deve ser maior que um (1).',
        life: 3000,
      })
      isValid = false
    }

    if (!data.emailContatoPrincipal) {
      isValid = false
      toast.add({
        severity: 'error',
        summary: 'Campo Obrigatório',
        detail: 'O email para contato principal não deve ser vazio.',
        life: 3000,
      })
      isValid = false
    }

    if (!data.emailParaNotificacoes) {
      isValid = false
      toast.add({
        severity: 'error',
        summary: 'Campo Obrigatório',
        detail: 'O email para notificações não deve ser vazio.',
        life: 3000,
      })
      isValid = false
    }

    return isValid
  }

  return {
    isEditing,
    isLoading,
    formData,
    isDirty,
    prazoSubmissaoDatePicker,
    handleSave,
    handleCancel,
  }
}
