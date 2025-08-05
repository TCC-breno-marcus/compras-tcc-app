<script setup lang="ts">
import Dialog from 'primevue/dialog'
import Button from 'primevue/button'
import { Divider } from 'primevue'
import { ref, watch, computed, inject } from 'vue'
import { catalogoService } from '../services/catalogoService'
import type { Item } from '../types'
import InputText from 'primevue/inputtext'
import FloatLabel from 'primevue/floatlabel'
import InputNumber from 'primevue/inputnumber'
import ToggleSwitch from 'primevue/toggleswitch'
import Textarea from 'primevue/textarea'
import Tag from 'primevue/tag'
import { useToast } from 'primevue/usetoast'
import { useConfirm } from 'primevue/useconfirm'
import FileUpload, {
  type FileUploadRemoveEvent,
  type FileUploadSelectEvent,
} from 'primevue/fileupload'
import { dataHasBeenChanged } from '@/utils/objectUtils'
import {
  ACCESS_ITEM_CONFIRMATION,
  CANCEL_CONFIRMATION,
  CLOSE_CONFIRMATION,
  DEL_IMAGE_CONFIRMATION,
  DEL_ITEM_CONFIRMATION,
  SAVE_CONFIRMATION,
} from '@/utils/confirmationFactoryUtils'
import ItemDetailsDialogSkeleton from './ItemDetailsDialogSkeleton.vue'
import { useCategoriaStore } from '../stores/categoriaStore'
import { storeToRefs } from 'pinia'
import Select from 'primevue/select'

const props = defineProps<{
  visible: boolean
  item: Item | null
}>()

const solicitationContext = inject('solicitationContext', { 
    dialogMode: 'selection', 
});

const confirm = useConfirm()
const toast = useToast()

const detailedItem = ref<Item | null>(null)
const itensSemelhantes = ref<Item[]>([])
const isLoading = ref(true)
const error = ref<string | null>(null)
const isEditing = ref(false)
const formData = ref<Partial<Item & { categoriaId?: number }>>({})
const originalFormData = ref<Partial<Item & { categoriaId?: number }>>({})
const arquivoDeImagem = ref<File | null>(null)
const isImageMarkedForRemoval = ref(false)
const categoriaStore = useCategoriaStore()
const {
  categorias,
  loading: categoriasLoading,
  error: categoriasError,
} = storeToRefs(categoriaStore)

const emit = defineEmits(['update:visible', 'update-dialog'])

const onFileSelect = async (event: FileUploadSelectEvent) => {
  const file = event.files[0]
  if (!file) return
  arquivoDeImagem.value = file
}

const onFileRemove = (event: FileUploadRemoveEvent) => {
  // console.log('Arquivo removido da pré-visualização:', event.file.name)
  arquivoDeImagem.value = null
}

const resetAllStates = (resetAll?: boolean) => {
  error.value = null
  if (resetAll) detailedItem.value = null
  itensSemelhantes.value = []
  isEditing.value = false
  formData.value = {}
  originalFormData.value = {}
  isImageMarkedForRemoval.value = false
  arquivoDeImagem.value = null
}

watch(
  () => props.item,
  async (currentItem) => {
    if (currentItem && props.item) {
      isLoading.value = true
      resetAllStates(true)
      try {
        const promiseDados = catalogoService.getItemById(props.item.id)
        const promiseSemelhantes = catalogoService.getItensSemelhantes(props.item.id)
        const promiseImagem = props.item.linkImagem
          ? precarregarImagem(props.item.linkImagem)
          : Promise.resolve()
        const [responseDados, _, responseSemelhantes] = await Promise.all([
          promiseDados,
          promiseImagem,
          promiseSemelhantes,
        ])
        detailedItem.value = responseDados
        itensSemelhantes.value = responseSemelhantes
      } catch (err) {
        console.error('Erro ao buscar detalhes ou pré-carregar imagem:', err)
        error.value = 'Não foi possível carregar os detalhes do item.'
      } finally {
        isLoading.value = false
      }
    }
  },
)

const precarregarImagem = (url: string): Promise<void> => {
  return new Promise((resolve, reject) => {
    const img = new Image()
    img.src = url
    img.onload = () => resolve()
    img.onerror = () => {
      console.warn(`Falha ao pré-carregar a imagem: ${url}`)
      resolve()
    }
  })
}

const accessOtherItem = (item: Item) => {
  if (isEditing.value) {
    confirm.require({
      ...ACCESS_ITEM_CONFIRMATION,
      accept: () => {
        resetAllStates(false)
        emit('update-dialog', { item: item, action: 'changeItem' })
      },
    })
    return
  }
  emit('update-dialog', { item: item, action: 'changeItem' })
}

const wasChanged = computed(() => {
  const textDataChanged = dataHasBeenChanged(originalFormData.value, formData.value)
  const newImageSelected = arquivoDeImagem.value !== null
  return textDataChanged || newImageSelected
})

const closeModal = () => {
  if (isEditing.value && wasChanged.value) {
    confirm.require({
      ...CLOSE_CONFIRMATION,
      accept: () => {
        resetAllStates(false)
        emit('update:visible', false)
      },
    })
    return
  }
  resetAllStates(false)
  emit('update:visible', false)
}

const enterEditMode = () => {
  if (detailedItem.value) {
    const initialFormState = {
      ...detailedItem.value,
      categoriaId: detailedItem.value.categoria?.id,
      precoSugerido: Number(detailedItem.value.precoSugerido),
    }

    formData.value = { ...initialFormState }
    originalFormData.value = { ...initialFormState }
  }
  isEditing.value = true
}

const cancelEdit = () => {
  if (wasChanged.value) {
    confirm.require({
      ...CANCEL_CONFIRMATION,
      accept: () => resetAllStates(false),
    })
    return
  }
  resetAllStates(false)
}

const acceptSaveChanges = async () => {
  if (!detailedItem.value) return

  isLoading.value = true
  try {
    if (arquivoDeImagem.value) {
      await catalogoService.atualizarImagemItem(detailedItem.value.id, arquivoDeImagem.value)
    } else if (isImageMarkedForRemoval.value) {
      await catalogoService.removerImagemItem(detailedItem.value.id)
    }

    const newData = {
      nome: formData.value?.nome,
      catMat: formData.value?.catMat,
      descricao: formData.value?.descricao,
      especificacao: formData.value?.especificacao,
      categoriaId: formData.value?.categoriaId,
      precoSugerido: formData.value?.precoSugerido,
      isActive: formData.value?.isActive,
    }
    const updatedItem = await catalogoService.editarItem(detailedItem.value.id, newData)

    toast.add({
      severity: 'success',
      summary: 'Sucesso',
      detail: 'O item foi salvo com sucesso.',
      life: 3000,
    })

    isEditing.value = false
    emit('update-dialog', { item: updatedItem, action: 'updateItems' })
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

const saveChanges = () => {
  if (!isFormValid()) return
  confirm.require({
    ...SAVE_CONFIRMATION,
    accept: async () => acceptSaveChanges(),
  })
}

const previewUrl = computed(() => {
  if (isEditing.value) {
    if (arquivoDeImagem.value) {
      return URL.createObjectURL(arquivoDeImagem.value)
    }
    if (formData.value?.linkImagem) {
      return formData.value.linkImagem
    }
  } else {
    if (detailedItem.value?.linkImagem) {
      return detailedItem.value.linkImagem
    }
  }
  return null
})

const removerImagem = () => {
  confirm.require({
    ...DEL_IMAGE_CONFIRMATION,
    accept: async () => {
      isImageMarkedForRemoval.value = true
      if (formData.value) {
        formData.value.linkImagem = ''
      }
    },
  })
}

const isFormValid = (): boolean => {
  let isValid = true
  if (!formData.value?.nome) {
    toast.add({
      severity: 'error',
      summary: 'Campo Obrigatório',
      detail: 'O campo "Nome" não pode ser vazio.',
      life: 3000,
    })
    isValid = false
  }
  if (!formData.value?.catMat) {
    toast.add({
      severity: 'error',
      summary: 'Campo Obrigatório',
      detail: 'O campo "Catmat" não pode ser vazio.',
      life: 3000,
    })
    isValid = false
  }
  if (!formData.value?.descricao) {
    toast.add({
      severity: 'error',
      summary: 'Campo Obrigatório',
      detail: 'O campo "Descrição" não pode ser vazio.',
      life: 3000,
    })
    isValid = false
  }
  return isValid
}

const acceptDeleteItem = async () => {
  if (!detailedItem.value) return

  isLoading.value = true
  try {
    await catalogoService.deletarItem(detailedItem.value.id)

    toast.add({
      severity: 'success',
      summary: 'Sucesso',
      detail: 'O item foi excluído com sucesso.',
      life: 3000,
    })

    isEditing.value = false
    emit('update:visible', false)
    emit('update-dialog', { action: 'updateItems' })
  } catch (err) {
    console.error('Erro ao excluir o item:', err)
    toast.add({
      severity: 'error',
      summary: 'Erro',
      detail: 'Não foi possível excluir o item.',
      life: 3000,
    })
  } finally {
    isLoading.value = false
  }
}

const deleteItem = () => {
  confirm.require({
    ...DEL_ITEM_CONFIRMATION,
    accept: async () => acceptDeleteItem(),
  })
}

const fileUploadPT = ref({
  root: {
    class: 'sm:flex sm:flex-row sm:align-items-center md:flex-column',
  },
  header: {
    class: 'p-4',
  },
  content: {
    class: 'w-16rem sm:w-22rem md:w-full sm:p-0 md:px-4 md:pb-4',
  },
})
</script>

<template>
  <Dialog
    :visible="visible"
    @update:visible="closeModal"
    modal
    :header="`Detalhes do Material ${detailedItem?.catMat || ''}`"
    :style="{ width: '90vw', maxWidth: '800px' }"
  >
    <div v-if="isLoading" class="dialog-content flex">
      <ItemDetailsDialogSkeleton />
    </div>

    <div v-else-if="error" class="flex flex-column align-items-center p-5">
      <i class="pi pi-exclamation-triangle text-3xl text-red-500"></i>
      <p class="mt-2">{{ error }}</p>
    </div>

    <div v-else-if="detailedItem" class="dialog-content flex flex-column md:flex-row">
      <div
        class="w-10rem flex-shrink-0 align-self-center md:align-self-start mb-4 md:mb-0 mr-0 md:mr-4"
      >
        <img
          v-if="previewUrl"
          :src="previewUrl"
          :alt="detailedItem.nome"
          class="dialog-image mb-2"
        />
        <div v-else class="image-placeholder mb-3">
          <span class="material-symbols-outlined placeholder-icon"> hide_image </span>
        </div>
        <div v-if="isEditing" class="field flex flex-column align-items-center">
          <Button
            v-if="formData.linkImagem"
            label="Remover"
            icon="pi pi-trash"
            severity="danger"
            text
            size="small"
            @click="removerImagem"
            class="mb-2"
          />
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
            :pt="fileUploadPT"
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
        <div v-if="!isEditing">
          <p><strong>Nome:</strong> {{ detailedItem.nome }}</p>
          <p><strong>CATMAT:</strong> {{ detailedItem.catMat }}</p>
          <p><strong>Descrição:</strong> {{ detailedItem.descricao }}</p>
          <p>
            <strong>Especificação: </strong>
            <span :class="{ 'texto-padrao': !detailedItem.especificacao }">
              {{ detailedItem.especificacao || 'Não informada.' }}
            </span>
          </p>
          <p><strong>Categoria:</strong> {{ detailedItem.categoria.nome }}</p>
          <p v-if="detailedItem.precoSugerido != null">
            <strong>Preço Sugerido:</strong>
            {{
              detailedItem.precoSugerido.toLocaleString('pt-BR', {
                style: 'currency',
                currency: 'BRL',
              })
            }}
          </p>
          <p>
            <strong>Status:</strong>
            <Tag
              :severity="item?.isActive ? 'success' : 'danger'"
              :value="item?.isActive ? 'Ativo' : 'Inativo'"
              class="ml-2"
            ></Tag>
          </p>
        </div>

        <div v-else class="flex flex-column gap-3">
          <div class="flex flex-column sm:flex-row w-full gap-3 sm:gap-2">
            <FloatLabel variant="on" class="w-full sm:w-9">
              <InputText
                id="nome"
                v-model="formData.nome"
                :invalid="!formData.nome"
                size="small"
                class="w-full"
                :maxlength="100"
              />
              <label for="nome">Nome</label>
            </FloatLabel>
            <FloatLabel variant="on" class="w-full sm:w-3">
              <InputText
                id="catMat"
                v-model="formData.catMat"
                :invalid="!formData.catMat"
                size="small"
                class="w-full"
                :maxlength="6"
              />
              <label for="catMat">Catmat</label>
            </FloatLabel>
          </div>
          <FloatLabel variant="on">
            <Textarea
              id="descricao"
              v-model="formData.descricao"
              :invalid="!formData.descricao"
              autoResize
              class="w-full pb-4"
              size="small"
              :maxlength="500"
            />
            <label for="descricao">Descrição</label>
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
          <div class="flex flex-column sm:flex-row w-full gap-3 sm:gap-2">
            <FloatLabel class="w-full sm:w-6 mt-1 sm:mt-0" variant="on">
              <Select
                v-model="formData.categoriaId"
                :options="categorias"
                optionLabel="nome"
                optionValue="id"
                inputId="categoria-filter"
                size="small"
                class="w-full"
              />
              <label for="categoria-filter">Categoria</label>
            </FloatLabel>
            <FloatLabel variant="on" class="price-input w-full sm:w-3">
              <InputNumber
                v-model="formData.precoSugerido"
                inputId="precoSugerido"
                mode="currency"
                currency="BRL"
                locale="pt-BR"
                :min="0"
                size="small"
                class="w-full"
                inputClass="w-full"
              />
              <label for="precoSugerido">Preço Sugerido</label>
            </FloatLabel>

            <div class="status-container border-1 border-round-xl w-full sm:w-3">
              <div class="flex justify-content-between align-items-center">
                <label for="isActive">Status</label>
                <ToggleSwitch v-model="formData.isActive" id="isActive" />
              </div>
            </div>
          </div>
        </div>

        <Divider align="left" type="solid" class="mt-4">
          <b>Materiais Semelhantes</b>
        </Divider>
        <p v-if="itensSemelhantes.length === 0" class="text-sm text-color-secondary font-italic">
          Nenhum material semelhante encontrado.
        </p>
        <div v-else class="semelhantes-list flex flex-column max-h-16rem overflow-auto">
          <a
            v-for="item in itensSemelhantes"
            :key="item.id"
            @click="accessOtherItem(item)"
            class="semelhante-item flex flex-column p-2"
            v-tooltip.bottom="'Acessar Item'"
          >
            <span class="nome">
              {{ item.nome }}
              <span v-if="item.especificacao"> - {{ item.especificacao }}</span>
            </span>

            <span class="catmat text-xs">CATMAT: {{ item.catMat }}</span>
          </a>
        </div>
      </div>
    </div>

    <template #footer>
      <div class="flex justify-content-between w-full">
        <div>
          <!-- SOMENTE SE O USER FOR GESTOR OU ADMIN -->
          <Button
            v-if="isEditing"
            label="Excluir"
            icon="pi pi-trash"
            severity="danger"
            text
            @click="deleteItem"
            size="small"
            :disabled="isLoading"
          />
        </div>
        <div class="flex gap-2">
          <Button
            v-if="isEditing"
            label="Cancelar"
            severity="danger"
            icon="pi pi-times"
            @click="cancelEdit"
            text
            size="small"
            :disabled="isLoading"
          />
          <Button
            v-if="!isEditing && solicitationContext.dialogMode === 'management'"
            label="Editar"
            icon="pi pi-pencil"
            size="small"
            @click="enterEditMode"
            :disabled="isLoading"
          />
          <Button
            v-if="isEditing"
            label="Salvar"
            icon="pi pi-save"
            size="small"
            @click="saveChanges()"
            severity="success"
            :disabled="!wasChanged || isLoading"
          />
          <slot name="dialog-actions" :item="detailedItem"></slot>
        </div>
      </div>
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
