<script setup lang="ts">
import Skeleton from 'primevue/skeleton'
</script>

<template>
  <div class="item-card m-2">
    <div class="image-preview-container flex justify-content-center align-items-center p-1">
      <Skeleton class="w-full h-full rounded-md" shape="rectangle" />
    </div>
    <div class="item-details-container flex align-items-center justify-content-between p-2">
      <div class="item-details">
        <Skeleton width="160px" height="1rem" class="mb-1"></Skeleton>
        <Skeleton width="80px" height="1rem"></Skeleton>
        
      </div>
      <Skeleton width="50px" height="1.2rem" borderRadius="4px"></Skeleton>
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
  /* height: 100%; */
}

.p-dark .item-card {
  border: 1px solid var(--p-surface-800);
}

.image-preview-container {
  position: relative;
  overflow: hidden;
  height: 160px;
  background-color: var(--p-surface-50);
}

.p-dark .image-preview-container {
  background-color: initial;
}

.item-details-container {
  margin-top: auto;
}

.item-details {
  text-align: start;
}

.item-details p {
  margin: 0;
}
</style>
