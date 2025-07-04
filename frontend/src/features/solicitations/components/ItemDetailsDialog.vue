<script setup lang="ts">
import Dialog from 'primevue/dialog';
import Button from 'primevue/button';
import type { ItemCatalogo } from '@/types/itemsCatalogo';

// 1. Defina as PROPS que este componente vai receber do pai
defineProps<{
  visible: boolean;
  item: ItemCatalogo | null; // <-- AQUI ESTÁ A CORREÇÃO!
}>();

// 2. Defina os EVENTOS que este componente pode emitir para o pai
const emit = defineEmits(['update:visible']);

// 3. Função para fechar o dialog, emitindo o evento de volta para o pai
const closeModal = () => {
  emit('update:visible', false);
};
</script>

<template>
  <Dialog v-if="item" :visible="visible" @update:visible="closeModal" modal :header="item.title"
    :style="{ width: 'clamp(300px, 50vw, 700px)' }">
    <div class="dialog-content">
      <img :src="`https://primefaces.org/cdn/primevue/images/galleria/galleria${item.code.slice(-1)}.jpg`"
        :alt="item.title" class="dialog-image mb-4" />
      <p>Aqui estão mais informações sobre o item **{{ item.title }}**.</p>
      <p><strong>Código CATMAT:</strong> {{ item.code }}</p>
      <p><strong>Descrição:</strong> Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed non risus. Suspendisse
        lectus tortor, dignissim sit amet, adipiscing nec, ultricies sed, dolor.</p>
    </div>

    <template #footer>
      <Button label="Fechar" icon="pi pi-times" @click="closeModal" text />
      <Button label="Adicionar à Solicitação" icon="pi pi-plus" />
    </template>
  </Dialog>
</template>

<style scoped>
.dialog-image {
  width: 100%;
  max-height: 300px;
  object-fit: cover;
  border-radius: 8px;
}

.dialog-content p {
  line-height: 1.6;
}
</style>