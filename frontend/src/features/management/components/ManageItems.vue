<script setup lang="ts">
import CatalogUpload from './CatalogUpload.vue'
import { useRoute } from 'vue-router'
import { ref } from 'vue'
import IconField from 'primevue/iconfield'
import InputIcon from 'primevue/inputicon'
import SplitButton from 'primevue/splitbutton'
import InputText from 'primevue/inputtext'
import Select from 'primevue/select'
import ItemComponent from './ItemComponent.vue'
import ItemDetailsDialog from './ItemDetailsDialog.vue'
import type { ItemCatalogo } from '@/types/itemsCatalogo'
import { Button } from 'primevue'
import CustomPaginator from '@/components/CustomPaginator.vue'

const route = useRoute()

const filters = ref([
  {
    label: 'Ordenar por',
    disabled: true,
  },
  {
    label: 'Mais Pedidos',
    icon: 'pi pi-arrow-up',
  },
  {
    label: 'Menos Pedidos',
    icon: 'pi pi-arrow-down',
  },
])

const selectedCategory = ref()

const categories = ref([
  { name: 'Todas', code: 'todas' },
  { name: 'Vidraria', code: 'vidraria' },
  { name: 'Componentes Eletrônicos', code: 'componentes-eletronicos' },
])

const items = ref([
  {
    title: 'Seringa Descartável',
    img: '/items_img/img1.png',
    code: '214560',
    status: 'Ativo',
  },
  {
    title: 'Gaze Estéril Pacote',
    img: '/items_img/img1.png',
    code: '348790',
    status: 'Ativo',
  },
  {
    title: 'Luva Cirúrgica (Par)',
    img: '/items_img/img1.png',
    code: '551230',
    status: 'Ativo',
  },
  {
    title: 'Álcool Etílico 70% 1L',
    img: '/items_img/img1.png',
    code: '987010',
    status: 'Inativo',
  },
  {
    title: 'Seringa Descartável',
    img: '/items_img/img1.png',
    code: '214560',
    status: 'Inativo',
  },
  {
    title: 'Gaze Estéril Pacote',
    img: '/items_img/img1.png',
    code: '348790',
    status: 'Ativo',
  },
  {
    title: 'Luva Cirúrgica (Par)',
    img: '/items_img/img1.png',
    code: '551230',
    status: 'Inativo',
  },
  {
    title: 'Álcool Etílico 70% 1L',
    img: '/items_img/img1.png',
    code: '987010',
    status: 'Ativo',
  },
  {
    title: 'Seringa Descartável',
    img: '/items_img/img1.png',
    code: '214560',
    status: 'Ativo',
  },
  {
    title: 'Gaze Estéril Pacote',
    img: '/items_img/img1.png',
    code: '348790',
    status: 'Ativo',
  },
  {
    title: 'Luva Cirúrgica (Par)',
    img: '/items_img/img1.png',
    code: '551230',
    status: 'Ativo',
  },
  {
    title: 'Álcool Etílico 70% 1L',
    img: '/items_img/img1.png',
    code: '987010',
    status: 'Inativo',
  },
  {
    title: 'Seringa Descartável',
    img: '/items_img/img1.png',
    code: '214560',
    status: 'Inativo',
  },
  {
    title: 'Gaze Estéril Pacote',
    img: '/items_img/img1.png',
    code: '348790',
    status: 'Ativo',
  },
  {
    title: 'Luva Cirúrgica (Par)',
    img: '/items_img/img1.png',
    code: '551230',
    status: 'Inativo',
  },
  {
    title: 'Álcool Etílico 70% 1L',
    img: '/items_img/img1.png',
    code: '987010',
    status: 'Ativo',
  },
  {
    title: 'Seringa Descartável',
    img: '/items_img/img1.png',
    code: '214560',
    status: 'Ativo',
  },
  {
    title: 'Gaze Estéril Pacote',
    img: '/items_img/img1.png',
    code: '348790',
    status: 'Ativo',
  },
  {
    title: 'Luva Cirúrgica (Par)',
    img: '/items_img/img1.png',
    code: '551230',
    status: 'Ativo',
  },
  {
    title: 'Álcool Etílico 70% 1L',
    img: '/items_img/img1.png',
    code: '987010',
    status: 'Inativo',
  },
  {
    title: 'Seringa Descartável',
    img: '/items_img/img1.png',
    code: '214560',
    status: 'Inativo',
  },
  {
    title: 'Gaze Estéril Pacote',
    img: '/items_img/img1.png',
    code: '348790',
    status: 'Ativo',
  },
  {
    title: 'Luva Cirúrgica (Par)',
    img: '/items_img/img1.png',
    code: '551230',
    status: 'Inativo',
  },
  {
    title: 'Álcool Etílico 70% 1L',
    img: '/items_img/img1.png',
    code: '987010',
    status: 'Ativo',
  },
])

const isDialogVisible = ref(false)
const selectedItem = ref<ItemCatalogo | null>(null)
const handleViewDetails = (item: ItemCatalogo) => {
  selectedItem.value = item
  isDialogVisible.value = true
}
</script>

<template>
  <div class="flex flex-column w-full h-full">
    <div class="flex flex-wrap align-items-center justify-content-between gap-2 md:gap-4 mt-2">
      <div class="flex flex-wrap align-items-center gap-2">
        <div class="flex flex-column sm:flex-row gap-2">
          <IconField iconPosition="left">
            <InputIcon class="pi pi-search"></InputIcon>
            <InputText size="small" placeholder="Nome/Descrição/CATMAT" />
          </IconField>

          <Select
            v-model="selectedCategory"
            :options="categories"
            optionLabel="name"
            placeholder="Categoria"
            class="w-full md:w-56"
            size="small"
          />
        </div>

        <div class="flex align-items-center gap-2">
          <SplitButton text size="small" label="Ordenar por" :model="filters" />
          <Button type="button" label="Filtrar" icon="pi pi-filter" size="small" />
        </div>
      </div>

      <div class="flex align-items-center gap-2">
        <Button type="button" label="Criar" icon="pi pi-plus" size="small" />
        <CatalogUpload />
      </div>
    </div>

    <div class="items-grid mt-2 gap-2">
      <ItemComponent
        v-for="item in items"
        :key="item.code"
        :item="item"
        @viewDetails="handleViewDetails"
      />
      <CustomPaginator :current-url="route.path" :total-records="300" :has-next-page="true" />
    </div>

    <ItemDetailsDialog v-model:visible="isDialogVisible" :item="selectedItem" />
  </div>
</template>

<style scoped>
.items-grid {
  display: flex;
  flex-wrap: wrap;
  justify-content: center;
  max-height: calc(100vh - 250px);
  overflow-y: auto;
  /* Para Firefox */
  scrollbar-width: thin;
  scrollbar-color: var(--p-surface-400) transparent;
}

:deep(.p-toolbar-start) {
  flex: 1 1 auto;
}
</style>
