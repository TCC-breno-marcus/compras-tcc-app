<script setup lang="ts">
import Dialog from 'primevue/dialog';
import Button from 'primevue/button';
import type { ItemCatalogo } from '@/types/itemsCatalogo';
import { Divider } from 'primevue';
import { ref } from 'vue';

defineProps<{
  visible: boolean;
  item: ItemCatalogo | null;
}>();

// 2. Defina os EVENTOS que este componente pode emitir para o pai
const emit = defineEmits(['update:visible']);

const closeModal = () => {
  emit('update:visible', false);
};

const isEditing = ref(false);

const enterEditMode = () => {
  isEditing.value = true;
};

const saveChanges = () => {
  console.log('Lógica para salvar os dados vai aqui...');
  isEditing.value = false;
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
      <Button v-if="!isEditing" label="Fechar" severity="danger" icon="pi pi-times" @click="closeModal" text
        size="small" />
      <Button v-else label="Cancelar" severity="danger" icon="pi pi-times" @click="isEditing = false" text size="small" />
      <Button v-if="!isEditing" label="Editar" icon="pi pi-pencil" size="small" @click="enterEditMode" />
      <Button v-else label="Salvar" icon="pi pi-save" size="small" @click="saveChanges" severity="success" />

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