<script setup lang="ts">
import Dialog from 'primevue/dialog';
import Button from 'primevue/button';
import type { ItemCatalogo } from '@/types/itemsCatalogo';
import { Divider } from 'primevue';

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
  <Dialog v-if="item" :visible="visible" @update:visible="closeModal" modal :header="`Detalhes do Material`"
    :style="{ width: 'clamp(300px, 50vw, 700px)' }">
    <div class="dialog-content flex">
      <div class="flex flex-column justify-content-between text-justify">
        <div>
          <p><strong>Título:</strong> {{ item.title }}</p>
          <p><strong>CATMAT:</strong> {{ item.code }}</p>
          <p><strong>Descrição:</strong> Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed non risus.
            Suspendisse
            lectus tortor, dignissim sit amet, adipiscing nec, ultricies sed, dolor.</p>
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
      <Button label="Fechar" severity="danger" icon="pi pi-times" @click="closeModal" text size="small"/>
      <Button label="Adicionar à Solicitação" icon="pi pi-plus" size="small"/>
    </template>
  </Dialog>
</template>

<style scoped>
.dialog-image {
  object-fit: cover;
  border-radius: 8px;
  max-width: 10rem;
}

.dialog-content p {
  line-height: 1.6;
}
</style>