<script setup lang="ts">
import Dialog from 'primevue/dialog'
import Button from 'primevue/button'
import { Divider } from 'primevue'
import { ref, watch, computed } from 'vue'
import { catalogoService } from '../services/catalogoService'
import type { Item } from '../types'
import Skeleton from 'primevue/skeleton'
import InputText from 'primevue/inputtext'
import FloatLabel from 'primevue/floatlabel'
import InputNumber from 'primevue/inputnumber'
import ToggleSwitch from 'primevue/toggleswitch'
import Textarea from 'primevue/textarea'
import Tag from 'primevue/tag'
import { useToast } from 'primevue/usetoast'
import { useConfirm } from 'primevue/useconfirm'
import FileUpload, { FileUploadRemoveEvent, type FileUploadSelectEvent } from 'primevue/fileupload'
import { dataHasBeenChanged } from '@/utils/objectUtils'

const confirm = useConfirm()
const toast = useToast()

const props = defineProps<{
  visible: boolean
  item: Item | null
}>()

const emit = defineEmits(['update:visible', 'update-dialog'])

const detailedItem = ref<Item | null>(null)
const itensSemelhantes = ref<Item[]>([])
const isLoading = ref(false)
const error = ref<string | null>(null)
const isEditing = ref(false)
const formData = ref<Partial<Item>>({})
const arquivoDeImagem = ref<File | null>(null)

const onFileSelect = async (event: FileUploadSelectEvent) => {
  const file = event.files[0]
  if (!file) return
  arquivoDeImagem.value = file
}

const onFileRemove = (event: FileUploadRemoveEvent) => {
  // console.log('Arquivo removido da pré-visualização:', event.file.name)
  arquivoDeImagem.value = null
}

watch([() => props.item, () => props.visible], async ([currentItem, isVisible]) => {
  if (currentItem && props.item && isVisible) {
    isLoading.value = true
    error.value = null
    detailedItem.value = null
    itensSemelhantes.value = []
    isEditing.value = false
    formData.value = {}
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
})

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

const abrirDetalhes = (item: Item) => {
  if (isEditing.value) {
    confirm.require({
      message: `Você possui alterações não salvas neste item. Se prosseguir, elas serão perdidas.`,
      header: 'Descartar Alterações?',
      icon: 'pi pi-exclamation-triangle',
      rejectProps: {
        label: 'Continuar Editando',
        severity: 'secondary',
        text: true,
        icon: 'pi pi-arrow-left',
        size: 'small',
      },
      acceptProps: {
        label: 'Descartar e Abrir',
        icon: 'pi pi-trash',
        size: 'small',
        severity: 'danger',
      },
      accept: () => {
        formData.value = {}
        isEditing.value = false
        emit('update-dialog', item)
      },
    })
    return
  }
  emit('update-dialog', item)
}

const wasChanged = computed(() => {
  const textDataChanged = dataHasBeenChanged(detailedItem.value, formData.value)
  const newImageSelected = arquivoDeImagem.value !== null
  return textDataChanged || newImageSelected
})

const closeModal = () => {
  if (isEditing.value && wasChanged.value) {
    confirm.require({
      message: `As alterações feitas serão descartadas. Deseja continuar?`,
      header: 'Descartar Alterações?',
      icon: 'pi pi-exclamation-triangle',
      rejectProps: {
        label: 'Continuar Editando',
        severity: 'secondary',
        text: true,
        icon: 'pi pi-arrow-left',
        size: 'small',
      },
      acceptProps: {
        label: 'Descartar',
        icon: 'pi pi-trash',
        size: 'small',
        severity: 'danger',
      },
      accept: () => {
        formData.value = {}
        isEditing.value = false
        emit('update:visible', false)
      },
    })
    return
  }
  emit('update:visible', false)
}

const enterEditMode = () => {
  if (detailedItem.value) {
    // Copia os dados atuais do item para o formulário de edição
    formData.value = {
      ...detailedItem.value,
      precoSugerido: Number(detailedItem.value.precoSugerido),
    }
  }
  isEditing.value = true
}

const resetAndExitEditMode = () => {
  formData.value = {}
  arquivoDeImagem.value = null
  isEditing.value = false
}

const cancelEdit = () => {
  if (wasChanged.value) {
    confirm.require({
      message: `As alterações feitas serão descartadas. Deseja continuar?`,
      header: 'Descartar Alterações?',
      icon: 'pi pi-exclamation-triangle',
      rejectProps: {
        label: 'Continuar Editando',
        severity: 'secondary',
        text: true,
        icon: 'pi pi-arrow-left',
        size: 'small',
      },
      acceptProps: {
        label: 'Descartar',
        icon: 'pi pi-trash',
        size: 'small',
        severity: 'danger',
      },
      accept: resetAndExitEditMode,
    })
    return
  }
  resetAndExitEditMode()
}

const saveChanges = () => {
  confirm.require({
    message: `As alterações serão salvas. Deseja continuar?`,
    header: 'Salvar Alterações?',
    icon: 'pi pi-exclamation-triangle',
    rejectProps: {
      label: 'Cancelar',
      severity: 'danger',
      text: true,
      icon: 'pi pi-times',
      size: 'small',
    },
    acceptProps: {
      label: 'Salvar',
      icon: 'pi pi-check',
      size: 'small',
      severity: 'success',
    },
    accept: async () => {
      if (detailedItem.value) {
        isLoading.value = true

        if (arquivoDeImagem.value) {
          try {
            await catalogoService.atualizarImagemItem(detailedItem.value.id, arquivoDeImagem.value)
            toast.add({
              severity: 'success',
              summary: 'Sucesso',
              detail: 'A imagem do item foi atualizada com sucesso.',
            })
          } catch (error) {
            console.error('Falha ao fazer upload da imagem', error)
            toast.add({
              severity: 'error',
              summary: 'Erro',
              detail: 'Não foi possível enviar a nova imagem.',
              life: 3000,
            })
            return
          } finally {
            isLoading.value = false
          }
        }

        toast.removeAllGroups()

        toast.add({
          severity: 'info',
          summary: 'Atualizando',
          detail: 'O Item está sendo atualizado.',
        })
        try {
          const newData = {
            nome: formData.value?.nome,
            catMat: formData.value?.catMat,
            descricao: formData.value?.descricao,
            especificacao: formData.value?.especificacao,
            precoSugerido: formData.value?.precoSugerido,
            isActive: formData.value?.isActive,
          }
          const updatedItem = await catalogoService.editarItem(detailedItem.value.id, newData)
          toast.removeAllGroups()
          toast.add({
            severity: 'success',
            summary: 'Sucesso',
            detail: 'O item foi editado com sucesso.',
            life: 3000,
          })
          isEditing.value = false
          emit('update-dialog', updatedItem)
        } catch (err) {
          console.error('Erro ao tentar editar o item:', err)
          error.value = 'Não foi possível editar o item.'
          toast.add({
            severity: 'error',
            summary: 'Erro',
            detail: 'Não foi possível editar o item.',
            life: 3000,
          })
        } finally {
          isLoading.value = false
        }
      }
    },
  })
}

const removerImagem = () => {
  confirm.require({
    message: 'Tem certeza que deseja remover a imagem atual? O item ficará sem imagem.',
    header: 'Remover Imagem?',
    icon: 'pi pi-trash',
    acceptProps: {
      label: 'Remover',
      icon: 'pi pi-trash',
      size: 'small',
      severity: 'danger',
    },
    rejectProps: {
      label: 'Cancelar',
      severity: 'secondary',
      text: true,
      icon: 'pi pi-times',
      size: 'small',
    },
    accept: async () => {
      if (detailedItem.value) {
        try {
          await catalogoService.removerImagemItem(detailedItem.value.id)

          if (formData.value) {
            formData.value.linkImagem = ''
          }
          if (detailedItem.value) {
            detailedItem.value.linkImagem = ''
          }

          toast.add({
            severity: 'success',
            summary: 'Sucesso',
            detail: 'Imagem removida.',
            life: 3000,
          })
        } catch (error) {
          toast.add({
            severity: 'error',
            summary: 'Erro',
            detail: 'Não foi possível remover a imagem.',
            life: 3000,
          })
        }
      }
    },
  })
}
</script>

<template>
  <Dialog
    v-if="detailedItem"
    :visible="visible"
    @update:visible="closeModal"
    modal
    :header="`Detalhes do Material ${detailedItem.catMat}`"
    :style="{ width: '90vw', maxWidth: '800px' }"
  >
    <div v-if="isLoading" class="dialog-content flex">
      <div class="dialog-img">
        <Skeleton width="8rem" height="8rem" borderRadius="8px"></Skeleton>
      </div>
      <div class="flex flex-column flex-grow-1 pl-4">
        <Skeleton width="70%" height="1.5rem" class="mb-3"></Skeleton>
        <Skeleton width="40%" height="1rem" class="mb-4"></Skeleton>
        <Skeleton width="100%" height="1rem" class="mb-2"></Skeleton>
        <Skeleton width="100%" height="1rem" class="mb-2"></Skeleton>
        <Skeleton width="85%" height="1rem" class="mb-5"></Skeleton>

        <Skeleton width="50%" height="1.2rem" class="mb-3"></Skeleton>
        <Skeleton width="60%" height="1rem" class="mb-2"></Skeleton>
        <Skeleton width="65%" height="1rem"></Skeleton>
      </div>
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
          v-if="detailedItem.linkImagem"
          :src="detailedItem.linkImagem"
          :alt="detailedItem.nome"
          class="dialog-image"
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
              :severity="item.isActive ? 'success' : 'danger'"
              :value="item.isActive ? 'Ativo' : 'Inativo'"
              class="ml-2"
            ></Tag>
          </p>
        </div>

        <div v-else class="flex flex-column gap-3">
          <div class="flex flex-column md:flex-row w-full gap-2">
            <FloatLabel variant="on" class="w-9">
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
            <FloatLabel variant="on" class="w-3">
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

        <Divider align="left" type="solid" class="mt-4">
          <b>Materiais Semelhantes</b>
        </Divider>
        <p v-if="itensSemelhantes.length === 0" class="text-sm text-color-secondary">
          Nenhum material semelhante encontrado.
        </p>
        <div v-else class="semelhantes-list flex flex-column max-h-16rem overflow-auto">
          <a
            v-for="item in itensSemelhantes"
            :key="item.id"
            @click="abrirDetalhes(item)"
            class="semelhante-item flex flex-column p-2"
            v-tooltip.bottom="'Acessar Item'"
          >
            <span class="nome">{{ item.nome }} - {{ item.especificacao }}</span>
            <span class="catmat text-xs">CATMAT: {{ item.catMat }}</span>
          </a>
        </div>
      </div>
    </div>

    <template #footer>
      <Button
        v-if="!isEditing"
        label="Fechar"
        severity="danger"
        icon="pi pi-times"
        @click="closeModal"
        text
        size="small"
      />
      <Button
        v-else
        label="Cancelar"
        severity="danger"
        icon="pi pi-times"
        @click="cancelEdit"
        text
        size="small"
      />
      <Button
        v-if="!isEditing"
        label="Editar"
        icon="pi pi-pencil"
        size="small"
        @click="enterEditMode"
      />
      <Button
        v-else
        label="Salvar"
        icon="pi pi-save"
        size="small"
        @click="saveChanges()"
        severity="success"
        :disabled="!wasChanged"
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
  color: var(--text-color-secondary);
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
