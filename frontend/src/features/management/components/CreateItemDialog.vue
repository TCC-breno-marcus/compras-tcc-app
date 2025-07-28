<script setup lang="ts">
import Dialog from 'primevue/dialog'
import Button from 'primevue/button'
import { ref, computed, watch } from 'vue'
import { catalogoService } from '../services/catalogoService'
import type { Item } from '../types'
import InputText from 'primevue/inputtext'
import FloatLabel from 'primevue/floatlabel'
import InputNumber from 'primevue/inputnumber'
import ToggleSwitch from 'primevue/toggleswitch'
import Textarea from 'primevue/textarea'
import { useToast } from 'primevue/usetoast'
import { useConfirm } from 'primevue/useconfirm'
import FileUpload, {
  type FileUploadRemoveEvent,
  type FileUploadSelectEvent,
} from 'primevue/fileupload'
import { dataHasBeenChanged } from '@/utils/objectUtils'
import { CLOSE_CONFIRMATION, SAVE_CONFIRMATION } from '@/utils/confirmationFactoryUtils'
import axios from 'axios'

const props = defineProps<{
  visible: boolean
}>()

const confirm = useConfirm()
const toast = useToast()

const isLoading = ref(true)
const error = ref<string | null>(null)
const formDataInitial = {
  nome: '',
  descricao: '',
  catMat: '',
  especificacao: '',
  precoSugerido: 0,
  isActive: false,
}
const formData = ref<Partial<Item>>(formDataInitial)
const dirtyFieldsInitial = {
  nome: false,
  descricao: false,
  catmat: false,
}
const dirtyFields = ref(dirtyFieldsInitial)

const onBlur = (fieldName: keyof typeof dirtyFieldsInitial) => {
  dirtyFields.value[fieldName] = true
}

const arquivoDeImagem = ref<File | null>(null)

const emit = defineEmits(['update:visible'])

const onFileSelect = async (event: FileUploadSelectEvent) => {
  const file = event.files[0]
  if (!file) return
  arquivoDeImagem.value = file
}

const onFileRemove = (event: FileUploadRemoveEvent) => {
  // console.log('Arquivo removido da pré-visualização:', event.file.name)
  arquivoDeImagem.value = null
}

const resetAllStates = () => {
  error.value = null
  formData.value = { ...formDataInitial }
  dirtyFields.value = { ...dirtyFieldsInitial }
  arquivoDeImagem.value = null
}

const closeModal = () => {
  if (dataHasBeenChanged(formDataInitial, formData.value)) {
    confirm.require({
      ...CLOSE_CONFIRMATION,
      accept: () => {
        resetAllStates()
        emit('update:visible', false)
      },
    })
    return
  }
  resetAllStates()
  emit('update:visible', false)
}

const createItem = async () => {
  isLoading.value = true
  try {
    const newData = {
      nome: formData.value?.nome,
      catMat: formData.value?.catMat,
      descricao: formData.value?.descricao,
      especificacao: formData.value?.especificacao,
      precoSugerido: formData.value?.precoSugerido,
      isActive: formData.value?.isActive,
    }
    const createdItem = await catalogoService.criarItem(newData)
    console.log({ createdItem })

    if (arquivoDeImagem.value) {
      await catalogoService.atualizarImagemItem(createdItem.id, arquivoDeImagem.value)
    }

    toast.add({
      severity: 'success',
      summary: 'Sucesso',
      detail: 'O item foi criado com sucesso.',
      life: 3000,
    })
    emit('update:visible', false)

  } catch (err) {
    if (axios.isAxiosError(err)) {
      const mensagem = err.response?.data?.message || 'Erro desconhecido'
      console.error('Erro ao criar item:', mensagem)
      toast.add({
        severity: 'error',
        summary: 'Erro',
        detail: mensagem,
        life: 3000,
      })
    } else {
      console.error('Erro inesperado:', err)
    }
  } finally {
    isLoading.value = false
  }
}

const previewUrl = computed(() => {
  if (arquivoDeImagem.value) {
    return URL.createObjectURL(arquivoDeImagem.value)
  }

  return null
})

const allRequiredFieldFilled = (): boolean => {
  return !!(
    formData.value.nome?.trim() &&
    formData.value.descricao?.trim() &&
    formData.value.catMat?.trim()
  )
}

watch(
  () => props.visible,
  (isNowVisible: boolean) => {
    if (isNowVisible) {
      resetAllStates()
    }
  },
)
</script>

<template>
  <Dialog
    :visible="visible"
    @update:visible="closeModal"
    modal
    :header="`Criar Novo Item`"
    :style="{ width: '90vw', maxWidth: '800px' }"
  >
    <div class="dialog-content flex flex-column md:flex-row">
      <div
        class="w-10rem flex-shrink-0 align-self-center md:align-self-start mb-4 md:mb-0 mr-0 md:mr-4"
      >
        <img
          v-if="previewUrl"
          :src="previewUrl"
          :alt="`Imagem do item`"
          class="dialog-image mb-2"
        />
        <div v-else class="image-placeholder mb-3">
          <span class="material-symbols-outlined placeholder-icon"> hide_image </span>
        </div>
        <div class="field flex flex-column align-items-center">
          <FileUpload
            name="imagem"
            @select="onFileSelect"
            @remove="onFileRemove"
            :showUploadButton="false"
            :showCancelButton="false"
            accept="image/*"
            chooseLabel="Selecionar"
            chooseIcon="pi pi-plus"
            :maxFileSize="10000000"
            invalidFileSizeMessage="O tamanho do arquivo não pode exceder 10 MB."
            :previewWidth="85"
            class="upload-button text-sm"
          >
            <template #empty>
              <small class="text-color-secondary text-center"
                >Arraste e solte uma imagem aqui ou clique para selecionar.</small
              >
            </template>
          </FileUpload>
        </div>
      </div>

      <div class="flex-grow-1 md:pr-4 text-justify mt-2 mb-2 md:mb-0">
        <div class="flex flex-column gap-3">
          <div class="flex flex-column md:flex-row w-full gap-2">
            <FloatLabel variant="on" class="w-9">
              <InputText
                id="nome"
                v-model="formData.nome"
                :invalid="dirtyFields.nome && !formData.nome"
                @blur="onBlur('nome')"
                size="small"
                class="w-full"
                :maxlength="100"
              />
              <label for="nome">Nome *</label>
            </FloatLabel>
            <FloatLabel variant="on" class="w-3">
              <InputText
                id="catMat"
                v-model="formData.catMat"
                :invalid="dirtyFields.catmat && !formData.catMat"
                @blur="onBlur('catmat')"
                size="small"
                class="w-full"
                :maxlength="6"
              />
              <label for="catMat">Catmat *</label>
            </FloatLabel>
          </div>
          <FloatLabel variant="on">
            <Textarea
              id="descricao"
              v-model="formData.descricao"
              :invalid="dirtyFields.descricao && !formData.descricao"
              @blur="onBlur('descricao')"
              autoResize
              class="w-full pb-4"
              size="small"
              :maxlength="500"
            />
            <label for="descricao">Descrição *</label>
            <small class="char-counter">
              {{ formData.descricao?.length || 0 }}/500 caracteres
            </small>
          </FloatLabel>
          <FloatLabel variant="on">
            <InputText
              id="especificacao"
              v-model="formData.especificacao"
              size="small"
              class="w-full"
              :maxlength="500"
            />
            <label for="especificacao">Especificação</label>
          </FloatLabel>
          <div class="flex flex-column md:flex-row w-full gap-2">
            <FloatLabel variant="on" class="price-input w-6">
              <InputNumber
                v-model="formData.precoSugerido"
                inputId="precoSugerido"
                mode="currency"
                currency="BRL"
                locale="pt-BR"
                :min="0"
                size="small"
                class="w-full"
              />
              <label for="precoSugerido">Preço Sugerido</label>
            </FloatLabel>

            <div class="status-container border-1 border-round-xl w-6">
              <div class="flex justify-content-between align-items-center">
                <label for="isActive">Status Ativo</label>
                <ToggleSwitch v-model="formData.isActive" id="isActive" />
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <template #footer>
      <Button
        label="Cancelar"
        severity="danger"
        icon="pi pi-times"
        @click="closeModal"
        text
        size="small"
      />
      <Button
        label="Criar"
        icon="pi pi-plus"
        size="small"
        @click="createItem()"
        severity="success"
        :disabled="!allRequiredFieldFilled()"
      />
    </template>
  </Dialog>
</template>

<style scoped>
.dialog-content p {
  line-height: 1.6;
}

.dialog-image,
.image-placeholder {
  width: 100%;
  height: 100%;
  border-radius: 6px;
}

.dialog-image {
  object-fit: contain;
}

.image-placeholder {
  background-color: var(--surface-100);
  display: flex;
  justify-content: center;
  align-items: center;
}

.placeholder-icon {
  font-size: 2.5rem;
  color: var(--surface-400);
}

.semelhante-item {
  border-radius: 10px;
  cursor: pointer;
  transition: background-color 0.2s;
}

.semelhante-item:hover {
  background-color: var(--p-surface-100) !important;
}

.p-dark .semelhante-item:hover {
  background-color: var(--p-surface-800) !important;
}

.semelhante-item .nome {
  color: var(--primary-color);
  font-weight: 600;
}

.semelhante-item .catmat {
  color: var(--text-color-secondary);
}

.texto-padrao {
  font-style: italic;
  color: var(--p-surface-500);
}

.status-container {
  border-color: var(--p-surface-300);
  padding: 3px 10px;
}

.char-counter {
  position: absolute;
  bottom: 0.5rem;
  right: 0.75rem;
  font-size: 0.7rem;
  color: var(--text-color-secondary);
  pointer-events: none;
}
</style>
