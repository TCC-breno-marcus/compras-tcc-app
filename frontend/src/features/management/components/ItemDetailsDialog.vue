<script setup lang="ts">
import Dialog from 'primevue/dialog'
import Button from 'primevue/button'
import { Divider } from 'primevue'
import { ref, watch } from 'vue'
import { catalogoService } from '../services/catalogoService'
import type { Item } from '../types'
import Skeleton from 'primevue/skeleton'
import { detail } from '@primeuix/themes/aura/toast'

const props = defineProps<{
  visible: boolean
  item: Item | null
}>()

const emit = defineEmits(['update:visible', 'update-dialog'])
const abrirDetalhes = (item: Item) => {
  emit('update-dialog', item)
}

const detailedItem = ref<Item | null>(null)
const itensSemelhantes = ref<Item[]>([])
const isLoading = ref(false)
const error = ref<string | null>(null)

watch(
  () => props.item,
  async (currentItem) => {
    if (currentItem && props.item) {
      isLoading.value = true
      error.value = null
      detailedItem.value = null
      itensSemelhantes.value = []
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
  emit('update:visible', false)
}

const isEditing = ref(false)

const enterEditMode = () => {
  isEditing.value = true
}

const saveChanges = () => {
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
      <div class="w-10rem flex-shrink-0 align-self-start mb-4 md:mb-0 mr-0 md:mr-4">
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
      <div class="flex-grow-1 md:pr-4 text-justify mb-2 md:mb-0">
        <p><strong>Nome:</strong> {{ detailedItem.nome }}</p>
        <p><strong>CATMAT:</strong> {{ detailedItem.catMat }}</p>
        <p><strong>Descrição:</strong> {{ detailedItem.descricao }}</p>

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
        @click="isEditing = false"
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
</style>
