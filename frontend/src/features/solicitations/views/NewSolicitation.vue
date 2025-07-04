<script setup lang="ts">
import Divider from 'primevue/divider';
import ItemsListComponent from '../components/ItemsListComponent.vue';
</script>

<template>
  <div class="flex w-full">
    <div class="flex flex-column w-full p-2">
      <h3 class="mb-2">Catálogo de Materiais</h3>
      <ItemsListComponent />
    </div>
    <Divider layout="vertical" />
    <div class="flex flex-column align-content-end w-full p-2">
      <h3 class="text-right mb-2">Sua Solicitação</h3>
    </div>

  </div>

</template>
