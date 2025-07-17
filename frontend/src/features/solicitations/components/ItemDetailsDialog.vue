<script setup lang="ts">
import Dialog from 'primevue/dialog'
import Button from 'primevue/button'
import type { ItemCatalogo } from '@/types/itemsCatalogo'
import { Divider } from 'primevue'

defineProps<{
  visible: boolean
  item: ItemCatalogo | null
}>()

const emit = defineEmits(['update:visible'])

const closeModal = () => {
  emit('update:visible', false)
}
</script>

<template>
  <Dialog
    v-if="item"
    :visible="visible"
    @update:visible="closeModal"
    modal
    :header="`Detalhes do Material`"
    class="w-11 md:w-8 xl:w-6"
  >
    <div class="dialog-content flex">
      <div class="flex flex-column justify-content-between text-justify">
        <div>
          <p><strong>Título:</strong> {{ item.title }}</p>
          <p><strong>CATMAT:</strong> {{ item.code }}</p>
          <p>
            <strong>Descrição:</strong> Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed
            non risus. Suspendisse lectus tortor, dignissim sit amet, adipiscing nec, ultricies sed,
            dolor.
          </p>
        </div>
        <div>
          <Divider align="left" type="solid">
            <b>Materiais Semelhantes</b>
          </Divider>
          <p>Balão Fundo Chato 2000 mL - CATMAT: 409256</p>
          <p>Balão Fundo 6000 mL - CATMAT: 413186</p>
        </div>
      </div>
      <div class="dialog-img">
        <img src="/items_img/img1.png" :alt="item.title" class="dialog-image mb-4" />
      </div>
    </div>

    <template #footer>
      <Button
        label="Fechar"
        severity="danger"
        icon="pi pi-times"
        @click="closeModal"
        text
        size="small"
      />
      <Button label="Adicionar à Solicitação" icon="pi pi-plus" size="small" />
    </template>
  </Dialog>
</template>

<style scoped>
.dialog-image {
  object-fit: cover;
  border-radius: 8px;
  max-width: 8rem;
}

.dialog-content p {
  line-height: 1.6;
}
</style>
