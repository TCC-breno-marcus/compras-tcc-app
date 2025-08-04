<script setup lang="ts">
import type { Item } from '../types'

const props = defineProps<{
  item: Item
}>()

const emit = defineEmits(['viewDetails'])

const onShowDetailsClick = () => {
  emit('viewDetails', props.item)
}


</script>

<template>
  <div class="item-card m-2">
    <div
      class="image-preview-container flex justify-content-center align-items-center p-1"
      @click="onShowDetailsClick"
    >
      <img v-if="item.linkImagem" :src="item.linkImagem" :alt="item.nome" class="item-image" />
      <div v-else class="image-placeholder">
        <span class="material-symbols-outlined placeholder-icon"> hide_image </span>
      </div>

      <div class="preview-overlay">
        <i class="pi pi-eye preview-icon text-3xl"></i>
      </div>
    </div>
    <div class="item-details-container flex align-items-center justify-content-between p-2">
      <div class="item-details">
        <p class="font-bold text-sm">{{ item.nome }}</p>
        <p class="text-xs text-color-secondary">CATMAT {{ item.catMat }}</p>
      </div>
      <div class="item-actions">
        <slot name="actions"></slot>
      </div>
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
  /* height: 100%; */
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
  height: 160px;
  /* background-color: var(--p-surface-50); */
}

.p-dark .image-preview-container {
  background-color: initial;
}

.item-image {
  /* width: 100%; */
  height: 100%;
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

.item-details-container {
  margin-top: auto;
}

.item-details {
  text-align: start;
  /* min-height: 70px;   */
}

.item-details p {
  margin: 0;
}

.image-placeholder {
  width: 100%;
  height: 100%;
  display: flex;
  justify-content: center;
  align-items: center;
  border-radius: 6px;
}

.placeholder-icon {
  font-size: 3rem;
  /* color: var(--p-surface-500);  */
}
</style>
