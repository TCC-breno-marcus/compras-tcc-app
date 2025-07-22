<script setup lang="ts">
import Dialog from 'primevue/dialog'
import Button from 'primevue/button'
import { Divider } from 'primevue'
import { ref, watch } from 'vue'
import { catalogoService } from '../services/catalogoService'
import type { Item } from '../types'
import Skeleton from 'primevue/skeleton'
import InputText from 'primevue/inputtext'
import FloatLabel from 'primevue/floatlabel'
import InputNumber from 'primevue/inputnumber'
import ToggleSwitch from 'primevue/toggleswitch'
import Textarea from 'primevue/textarea'
import Tag from 'primevue/tag'

const props = defineProps<{
  visible: boolean
  item: Item | null
}>()

const emit = defineEmits(['update:visible', 'update-dialog'])
const abrirDetalhes = (item: Item) => {
  //TODO: perguntar ao usuário se ele tem certeza se estiver em modo de edição
  emit('update-dialog', item)
}

const detailedItem = ref<Item | null>(null)
const itensSemelhantes = ref<Item[]>([])
const isLoading = ref(false)
const error = ref<string | null>(null)
const isEditing = ref(false)
const formData = ref<Partial<Item>>({})

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

/**
 * Função auxiliar que pré-carrega uma imagem e retorna uma Promise.
 * A Promise resolve quando a imagem termina de carregar.
 */
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

const closeModal = () => {
  //TODO: perguntar ao usuário se ele tem certeza se estiver em modo de edição
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

const cancelEdit = () => {
  //TODO: perguntar ao usuário se ele tem certeza
  formData.value = {}
  isEditing.value = false
}

const saveChanges = () => {
  //TODO: perguntar ao usuário se ele tem certeza
  console.log('Lógica para salvar os dados vai aqui...')
  isEditing.value = false
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
      <div class="flex flex-column flex-grow-1 pr-4">
        <Skeleton width="70%" height="1.5rem" class="mb-3"></Skeleton>
        <Skeleton width="40%" height="1rem" class="mb-4"></Skeleton>
        <Skeleton width="100%" height="1rem" class="mb-2"></Skeleton>
        <Skeleton width="100%" height="1rem" class="mb-2"></Skeleton>
        <Skeleton width="85%" height="1rem" class="mb-5"></Skeleton>

        <Skeleton width="50%" height="1.2rem" class="mb-3"></Skeleton>
        <Skeleton width="60%" height="1rem" class="mb-2"></Skeleton>
        <Skeleton width="65%" height="1rem"></Skeleton>
      </div>

      <div class="dialog-img">
        <Skeleton width="8rem" height="8rem" borderRadius="8px"></Skeleton>
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
        <!-- TODO: ativar modo de edição (substituir imagem) -->
        <img
          v-if="detailedItem.linkImagem"
          :src="detailedItem.linkImagem"
          :alt="detailedItem.nome"
          class="dialog-image"
        />
        <div v-else class="image-placeholder">
          <span class="material-symbols-outlined placeholder-icon"> hide_image </span>
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
            <!-- {{ detailedItem.isActive ? 'Ativo' : 'Inativo' }} -->
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
        <div v-else class="semelhantes-list flex flex-column">
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
        @click="saveChanges"
        severity="success"
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

/* :deep(.p-textarea) {
  padding-bottom: 1.75rem;
} */
</style>
