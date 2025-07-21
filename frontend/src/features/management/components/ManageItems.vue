<script setup lang="ts">
import CatalogUpload from './CatalogUpload.vue'
import { ref, watch, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import type { Ref } from 'vue'
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
import SelectButton from 'primevue/selectbutton'
import IftaLabel from 'primevue/iftalabel'
import FloatLabel from 'primevue/floatlabel'
import RadioButton from 'primevue/radiobutton'
import RadioButtonGroup from 'primevue/radiobuttongroup'
import NotFoundSvg from '@/assets/NotFoundSvg.vue'

const route = useRoute()
const router = useRouter()
const catalogoStore = useCatalogoStore()
const { items, loading, error, totalCount, pageNumber, pageSize, totalPages } =
  storeToRefs(catalogoStore)

// TODO: a logica do simpleSearch ainda nao existe no backend: fazer
const simpleSearch = ref(route.query.search || '')

const op = ref() // Ref para o componente OverlayPanel
const nomeFilter = ref(route.query.nome || '')
const descricaoFilter = ref(route.query.descricao || '')
const catmatFilter = ref(route.query.catmat || '')
const especificacaoFilter = ref(route.query.catmat || '')
const statusFilter = ref('todos')
const opcoesStatus = ref([
  { name: 'Todos', code: 'todos' },
  { name: 'Ativo', code: 'ativo' },
  { name: 'Inativo', code: 'inativo' },
])
const sortOrder: Ref<'asc' | 'desc' | null> = ref(null)
const toggleSortDirection = () => {
  if (sortOrder.value === null) {
    sortOrder.value = 'asc'
  } else if (sortOrder.value === 'asc') {
    sortOrder.value = 'desc'
  } else {
    sortOrder.value = null // Volta para o padrão
  }
}
const computedSort = computed(() => {
  if (sortOrder.value === 'asc') {
    return { text: 'Ordenar de A-Z', icon: 'pi pi-sort-alpha-down' }
  }
  if (sortOrder.value === 'desc') {
    return { text: 'Ordenar de Z-A', icon: 'pi pi-sort-alpha-up' }
  }
  return { text: 'Não Ordenar', icon: 'pi pi-sort-alt' }
})

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
  delete currentQuery.isActive
  delete currentQuery.sortOrder

  // Adiciona os filtros que estiverem preenchidos
  if (simpleSearch.value) currentQuery.search = simpleSearch.value
  if (nomeFilter.value) currentQuery.nome = nomeFilter.value
  if (descricaoFilter.value) currentQuery.descricao = descricaoFilter.value
  if (catmatFilter.value) currentQuery.catmat = catmatFilter.value
  if (especificacaoFilter.value) currentQuery.especificacao = especificacaoFilter.value
  if (statusFilter.value && statusFilter.value !== 'todos') {
    currentQuery.isActive = (statusFilter.value === 'ativo').toString()
  }
  if (sortOrder.value) currentQuery.sortOrder = sortOrder.value

  // Reseta para a primeira página
  currentQuery.pageNumber = '1'

  router.push({ query: currentQuery })

  // Se o painel de filtros avançados estiver aberto, feche-o.
  if (op.value) {
    op.value.hide()
  }
}

const clearFilters = () => {
  nomeFilter.value = ''
  descricaoFilter.value = ''
  catmatFilter.value = ''
  especificacaoFilter.value = ''
  statusFilter.value = 'todos'
  simpleSearch.value = ''
  sortOrder.value = null
  applyFilters()
}

// TODO: Implementar logica de loading pra tudo que precisa de carregamento
watch(
  () => route.query,
  (newQuery) => {
    simpleSearch.value = newQuery.search || ''
    nomeFilter.value = newQuery.nome || ''
    descricaoFilter.value = newQuery.descricao || ''
    catmatFilter.value = newQuery.catmat || ''
    especificacaoFilter.value = newQuery.especificacao || ''

    if (newQuery.isActive === 'true') {
      statusFilter.value = 'ativo'
    } else if (newQuery.isActive === 'false') {
      statusFilter.value = 'inativo'
    } else {
      statusFilter.value = 'todos'
    }

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
          <FloatLabel class="w-16rem" variant="on">
            <IconField iconPosition="left">
              <InputIcon class="pi pi-search"></InputIcon>
              <InputText v-model="simpleSearch" size="small" @keyup.enter="applyFilters" />
            </IconField>
            <label for="status-filter">Pesquisar item</label>
          </FloatLabel>

          <FloatLabel class="w-7rem" variant="on">
            <Select
              v-model="statusFilter"
              :options="opcoesStatus"
              optionLabel="name"
              optionValue="code"
              inputId="status-filter"
              size="small"
              class="w-full"
            />
            <label for="status-filter">Status</label>
          </FloatLabel>

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
              <span class="p-text-secondary"><strong>Filtros Avançados</strong></span>

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

              <div class="flex justify-content-end gap-2">
                <Button
                  label="Limpar"
                  icon="pi pi-filter-slash"
                  severity="danger"
                  text
                  @click="clearFilters"
                  size="small"
                />
              </div>
            </div>
          </OverlayPanel>
        </div>

        <div class="flex align-items-center gap-2">
          <Button
            :icon="computedSort.icon"
            @click="toggleSortDirection"
            label="Ordem"
            text
            rounded
            size="small"
            aria-label="Ordem"
            v-tooltip.top="computedSort.text"
          />
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
        <Button type="button" label="Criar" icon="pi pi-plus" size="small" text />
        <CatalogUpload />
      </div>
    </div>

    <div v-if="items.length > 0" class="items-grid mt-4 gap-2">
      <ItemComponent
        v-for="item in items"
        :key="item.code"
        :item="item"
        @viewDetails="handleViewDetails"
      />
    </div>
    <div v-else class="flex flex-column align-items-center mt-6 gap-2">
      <div class="w-18rem sm:w-24rem md:w-30rem">
        <NotFoundSvg />
      </div>
      <h3 class="mb-2">Nenhum resultado encontrado.</h3>
      <p>Tente ajustar seus filtros ou utilize termos de busca diferentes.</p>
      <Button
        label="Limpar Filtros"
        icon="pi pi-filter-slash"
        @click="clearFilters"
        size="small"
      />
    </div>
    <CustomPaginator
      v-if="items.length > 0"
      :current-url="route.path"
      :total-count="totalCount"
      :has-next-page="pageNumber < totalPages"
      :page-size="pageSize"
      :page-number="pageNumber"
    />

    <ItemDetailsDialog v-model:visible="isDialogVisible" :item="selectedItem" />
  </div>
</template>

<style scoped>
.items-grid {
  display: flex;
  flex-wrap: wrap;
  justify-content: center;
  max-height: calc(100vh - 320px);
  overflow-y: auto;
  /* Para Firefox */
  scrollbar-width: thin;
  scrollbar-color: var(--p-surface-400) transparent;
}

:deep(.p-toolbar-start) {
  flex: 1 1 auto;
}
</style>
