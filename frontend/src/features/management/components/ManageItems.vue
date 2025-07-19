<script setup lang="ts">
import CatalogUpload from './CatalogUpload.vue'
import { ref, watch, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
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
import OverlayPanel from 'primevue/overlaypanel'
import { useCatalogoStore } from '../stores/catalogoStore'
import { storeToRefs } from 'pinia'

const catalogoStore = useCatalogoStore()
const { items, loading, error, totalCount, pageNumber, pageSize, totalPages } =
  storeToRefs(catalogoStore)

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

const route = useRoute()
const router = useRouter()

// Estado para a busca simples (já existente)
const simpleSearch = ref(route.query.search || '')
// TODO: a logica do simpleSearch ainda nao existe no backend: fazer

// 2. Refs para controlar o painel e os valores dos inputs
const op = ref() // Ref para o componente OverlayPanel
const nomeFilter = ref(route.query.nome || '')
const descricaoFilter = ref(route.query.descricao || '')
const catmatFilter = ref(route.query.catmat || '')
const especificacaoFilter = ref(route.query.catmat || '')

// Função para abrir/fechar o painel de filtros
const toggleAdvancedFilter = (event: MouseEvent) => {
  if (op.value) {
    op.value.toggle(event)
  }
}

const applyFilters = () => {
  const currentQuery = { ...route.query }

  // Limpa todos os filtros antigos antes de adicionar os novos
  delete currentQuery.search
  delete currentQuery.nome
  delete currentQuery.descricao
  delete currentQuery.catmat
  delete currentQuery.especificacao

  // Adiciona os filtros que estiverem preenchidos
  if (simpleSearch.value) currentQuery.search = simpleSearch.value
  if (nomeFilter.value) currentQuery.nome = nomeFilter.value
  if (descricaoFilter.value) currentQuery.descricao = descricaoFilter.value
  if (catmatFilter.value) currentQuery.catmat = catmatFilter.value
  if (especificacaoFilter.value) currentQuery.especificacao = especificacaoFilter.value

  // Reseta para a primeira página
  currentQuery.page = '1'

  router.push({ query: currentQuery })

  // Se o painel de filtros avançados estiver aberto, feche-o.
  if (op.value) {
    op.value.hide()
  }
}

const clearAdvancedFilters = () => {
  nomeFilter.value = ''
  descricaoFilter.value = ''
  catmatFilter.value = ''
  especificacaoFilter.value = ''
  applyFilters()
}

watch(
  () => route.query,
  (newQuery) => {
    simpleSearch.value = newQuery.search || ''
    nomeFilter.value = newQuery.nome || ''
    descricaoFilter.value = newQuery.descricao || ''
    catmatFilter.value = newQuery.catmat || ''

    catalogoStore.fetchItems(newQuery)
  },
  { immediate: true },
)

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
            <InputText
              v-model="simpleSearch"
              size="small"
              placeholder="Nome/Descrição/CATMAT"
              @keyup.enter="applyFilters"
            />
          </IconField>

          <Button
            type="button"
            label="Filtros Avançados"
            icon="pi pi-filter"
            size="small"
            text
            @click="toggleAdvancedFilter"
          />

          <OverlayPanel ref="op">
            <div class="flex flex-column gap-2 p-2" style="min-width: 250px">
              <span class="p-text-secondary">Filtros Avançados</span>

              <IconField iconPosition="rigth">
                <InputIcon class="pi pi-search"></InputIcon>
                <InputText v-model="nomeFilter" size="small" placeholder="Nome do item" />
              </IconField>

              <IconField iconPosition="rigth">
                <InputIcon class="pi pi-search"></InputIcon>
                <InputText v-model="descricaoFilter" size="small" placeholder="Descrição" />
              </IconField>

              <IconField iconPosition="rigth">
                <InputIcon class="pi pi-search"></InputIcon>
                <InputText v-model="catmatFilter" size="small" placeholder="CatMat" />
              </IconField>

              <IconField iconPosition="rigth">
                <InputIcon class="pi pi-search"></InputIcon>
                <InputText v-model="especificacaoFilter" size="small" placeholder="Especificação" />
              </IconField>

              <!-- TODO: deve ser um toggle button para alternar entre ativo e inativo -->
              <IconField iconPosition="rigth">
                <InputIcon class="pi pi-search"></InputIcon>
                <InputText v-model="isActiveFilter" size="small" placeholder="Ativo" />
              </IconField>

              <div class="flex justify-content-end gap-2">
                <Button
                  label="Limpar"
                  icon="pi pi-filter-slash"
                  severity="danger"
                  text
                  @click="clearAdvancedFilters"
                  size="small"
                />
              </div>
            </div>
          </OverlayPanel>

          <!-- <Select
            v-model="selectedCategory"
            :options="categories"
            optionLabel="name"
            placeholder="Categoria"
            class="w-full md:w-56"
            size="small"
          /> -->
        </div>

        <div class="flex align-items-center gap-2">
          <SplitButton text size="small" label="Ordenar por" :model="filters" />
          <Button
            type="button"
            label="Buscar"
            icon="pi pi-filter"
            size="small"
            @click="applyFilters"
          />
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
