<script setup lang="ts">
import { ref } from 'vue';
import Button from 'primevue/button';

// 2. Defina que este componente espera receber uma propriedade chamada "item"
// que será um objeto com os dados do item.
const props = defineProps({
  item: {
    type: Object,
    required: true
  }
});

// 2. Defina o evento customizado que ele vai emitir
const emit = defineEmits(['viewDetails']);

// 3. Função que é chamada no clique e emite o evento com os dados do item
const onShowDetailsClick = () => {
  emit('viewDetails', props.item);
};

</script>
<template>
  <div class="item-card m-2">
    <div class="image-preview-container flex justify-content-center align-items-center" @click="onShowDetailsClick">
      <img src="/items_img/img1.png" :alt="item.title" class="item-image"  />
      <div class="preview-overlay">
        <i class="pi pi-eye preview-icon"></i>
      </div>
    </div>
    <div class="item-details p-2">
      <p class="font-bold">{{ item.title }}</p>
      <p class="text-sm text-color-secondary">CATMAT {{ item.code }}</p>
    </div>
    <div class="item-actions flex justify-content-end pb-3 pr-3">
      <Button icon="pi pi-plus" severity="success" aria-label="Adicionar ao Carrinho" />
    </div>
  </div>
</template>

<style scoped>

.item-card {
  display: flex;
  flex-direction: column;
  border: 1px solid var(--p-surface-200);
  border-radius: 10px;
  overflow: hidden;
  width: 250px;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.05);
  transition: box-shadow 0.3s ease;
}

.p-dark .item-card {
  border: 1px solid var(--p-surface-800);
}

.item-card:hover {
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
}

.image-preview-container {
  position: relative;
  cursor: pointer;
  overflow: hidden;
  height: 180px;
  background-color: var(--p-surface-50);
}

.p-dark .image-preview-container {
  background-color: initial;
}

.item-image {
  max-width: 6rem;
  object-fit: cover;
  display: block;
  transition: transform 0.4s ease;
}

.preview-overlay {
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background-color: rgba(0, 0, 0, 0.5);
  display: flex;
  justify-content: center;
  align-items: center;
  opacity: 0;
  transition: opacity 0.4s ease;
}

.preview-icon {
  font-size: 2.5rem;
  color: white;
  transform: scale(0.8);
  transition: transform 0.4s ease;
}

.image-preview-container:hover .preview-overlay {
  opacity: 1;
}

.image-preview-container:hover .item-image {
  transform: scale(1.1);
}

.image-preview-container:hover .preview-icon {
  transform: scale(1);
}

.item-details {
  text-align: center;
}

.item-details p {
  margin: 0;
}

</style>