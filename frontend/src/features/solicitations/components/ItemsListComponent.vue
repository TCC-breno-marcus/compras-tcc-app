<script setup lang="ts">
import { ref } from 'vue';
import Toolbar from 'primevue/toolbar';
import IconField from 'primevue/iconfield';
import InputIcon from 'primevue/inputicon';
import SplitButton from 'primevue/splitbutton';
import InputText from 'primevue/inputtext';
import Select from 'primevue/select';
import ItemComponent from './ItemComponent.vue';
import ItemDetailsDialog from './ItemDetailsDialog.vue';
import type { ItemCatalogo } from '@/types/itemsCatalogo';

const filters = ref([
  {
    label: 'Ordenar por',
    disabled: true
  },
  {
    label: 'Mais Pedidos',
    icon: 'pi pi-arrow-up'
  },
  {
    label: 'Menos Pedidos',
    icon: 'pi pi-arrow-down'
  }
]);

const selectedCategory = ref();

const categories = ref([
  { name: 'Todas', code: 'todas' },
  { name: 'Vidraria', code: 'vidraria' },
  { name: 'Componentes Eletrônicos', code: 'componentes-eletronicos' },
]);

const items = ref([
  {
    title: 'Seringa Descartável',
    img: './img',
    code: '214560'
  },
  {
    title: 'Gaze Estéril Pacote',
    img: './img',
    code: '348790'
  },
  {
    title: 'Luva Cirúrgica (Par)',
    img: './img',
    code: '551230'
  },
  {
    title: 'Álcool Etílico 70% 1L',
    img: './img',
    code: '987010'
  },
  {
    title: 'Seringa Descartável',
    img: './img',
    code: '214560'
  },
  {
    title: 'Gaze Estéril Pacote',
    img: './img',
    code: '348790'
  },
  {
    title: 'Luva Cirúrgica (Par)',
    img: './img',
    code: '551230'
  },
  {
    title: 'Álcool Etílico 70% 1L',
    img: './img',
    code: '987010'
  }
]);

const isDialogVisible = ref(false);
const selectedItem = ref<ItemCatalogo | null>(null);
const handleViewDetails = (item: ItemCatalogo) => {
  selectedItem.value = item;   
  isDialogVisible.value = true; 
};


</script>

<template>
  <div class="card">
    <Toolbar>
      <template #start>
        <IconField iconPosition="left">
          <InputIcon class="pi pi-search"></InputIcon>
          <InputText placeholder="Nome/Descrição/CATMAT" />
        </IconField>
      </template>
      <template #center>
        <Select v-model="selectedCategory" :options="categories" optionLabel="name" placeholder="Categoria"
          class="w-full md:w-56" />
      </template>
      <template #end>
        <SplitButton label="Ordenar por" :model="filters"></SplitButton>
      </template>
    </Toolbar>

    <div class="items-grid mt-4">
      <ItemComponent v-for="item in items" :key="item.code" :item="item" @viewDetails="handleViewDetails" />
    </div>
  </div>

  <ItemDetailsDialog v-model:visible="isDialogVisible" :item="selectedItem" />

</template>

<style scoped>
.items-grid {
  display: flex;
  flex-wrap: wrap;
  gap: 1rem;
  justify-content: center;
  max-height: calc(100vh - 250px);
  overflow-y: auto;
  /* Para Firefox */
  scrollbar-width: thin;
  scrollbar-color: var(--p-surface-400) transparent;
}

/* Para Chrome, Safari, Edge, etc. */
.items-grid::-webkit-scrollbar {
  width: 5px;
}

.items-grid::-webkit-scrollbar-track {
  background: transparent;
}

.items-grid::-webkit-scrollbar-thumb {
  background: var(--p-surface-400, #ccc);
  border-radius: 10px;
}

.items-grid::-webkit-scrollbar-thumb:hover {
  background: var(--p-surface-500, #aaa);
}
</style>