<script setup lang="ts">
import Dialog from 'primevue/dialog'
import Button from 'primevue/button'
import { Divider } from 'primevue'
import { ref, watch } from 'vue'
import { catalogoService } from '../services/catalogoService'
import type { Item } from '../types'
import Skeleton from 'primevue/skeleton'

const props = defineProps<{
  visible: boolean
  item: Item | null
}>()

const emit = defineEmits(['update:visible'])

const detailedItem = ref<Item | null>(null)
const isLoading = ref(false)
const error = ref<string | null>(null)

watch(
  () => props.visible,
  async (isNowVisible) => {
    if (isNowVisible && props.item) {
      isLoading.value = true
      error.value = null
      detailedItem.value = null
      try {
        const urlDaImagem = `http://localhost:8088/images/${props.item.linkImagem}`
        const promiseDados = catalogoService.getItemById(props.item.id)
        const promiseImagem = props.item.linkImagem
          ? precarregarImagem(urlDaImagem)
          : Promise.resolve()
        const [responseDados] = await Promise.all([promiseDados, promiseImagem])
        detailedItem.value = responseDados
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
    v-if="item"
    :visible="visible"
    @update:visible="closeModal"
    modal
    :header="`Detalhes do Material`"
    :style="{ width: 'clamp(300px, 50vw, 700px)' }"
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

    <div v-else-if="detailedItem" class="dialog-content flex align-items-center">
      <div class="flex flex-column justify-content-between text-justify">
        <div>
          <p><strong>Nome:</strong> {{ detailedItem.nome }}</p>
          <p><strong>CATMAT:</strong> {{ detailedItem.catMat }}</p>
          <p><strong>Descrição:</strong> {{ detailedItem.descricao }}</p>
        </div>
        <div>
          <Divider align="left" type="solid">
            <b>Materiais Semelhantes</b>
          </Divider>
          <!-- TODO: fazer get items semelhantes -->
          <p>Balão Fundo Chato 2000 mL - CATMAT: 409256</p>
          <p>Balão Fundo 6000 mL - CATMAT: 413186</p>
        </div>
      </div>
      <div class="dialog-img">
        <img
          v-if="detailedItem.linkImagem"
          :src="`http://localhost:8088/images/${detailedItem.linkImagem}`"
          :alt="detailedItem.nome"
          class="dialog-image mb-4 ml-4"
        />
        <div v-else class="dialog-image mb-4 ml-4">
          <span class="material-symbols-outlined placeholder-icon"> hide_image </span>
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
.dialog-image {
  object-fit: cover;
  border-radius: 8px;
  /* max-width: 8rem; */
  max-height: 10rem;
}

.dialog-content p {
  line-height: 1.6;
}

.placeholder-icon {
  font-size: 4rem;
  /* color: var(--p-surface-500); */
}
</style>
