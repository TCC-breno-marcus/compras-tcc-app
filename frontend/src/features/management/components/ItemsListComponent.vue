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
import { Button } from 'primevue';

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
    img: '/items_img/img1.png',
    code: '214560'
  },
  {
    title: 'Gaze Estéril Pacote',
    img: '/items_img/img1.png',
    code: '348790'
  },
  {
    title: 'Luva Cirúrgica (Par)',
    img: '/items_img/img1.png',
    code: '551230'
  },
  {
    title: 'Álcool Etílico 70% 1L',
    img: '/items_img/img1.png',
    code: '987010'
  },
  {
    title: 'Seringa Descartável',
    img: '/items_img/img1.png',
    code: '214560'
  },
  {
    title: 'Gaze Estéril Pacote',
    img: '/items_img/img1.png',
    code: '348790'
  },
  {
    title: 'Luva Cirúrgica (Par)',
    img: '/items_img/img1.png',
    code: '551230'
  },
  {
    title: 'Álcool Etílico 70% 1L',
    img: '/items_img/img1.png',
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
        <div class="flex align-items-center gap-2">
          <IconField iconPosition="left">
            <InputIcon class="pi pi-search "></InputIcon>
            <InputText size="small" placeholder="Nome/Descrição/CATMAT" />
          </IconField>
          <Select v-model="selectedCategory" :options="categories" optionLabel="name" placeholder="Categoria"
            class="w-full md:w-56" size="small" />
        </div>
      </template>
      <template #end>
        <div class="flex align-items-center gap-2">
          <SplitButton text size="small" label="Ordenar por" :model="filters" />
          <Button type="button" label="Filtrar" icon="pi pi-filter" size="small" />
        </div>

      </template>
    </Toolbar>

    <div class="items-grid mt-2">
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

</style>