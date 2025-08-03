<script setup lang="ts">
import { reactive, ref, watch } from 'vue'
import { useRoute, useRouter, type LocationQueryValue } from 'vue-router'
import ItemDetailsDialog from './ItemDetailsDialog.vue'
import type { CatalogoFilters, Item } from '../types'
import { Button } from 'primevue'
import CustomPaginator from '@/components/CustomPaginator.vue'
import { useCatalogoStore } from '../stores/catalogoStore'
import { storeToRefs } from 'pinia'
import NotFoundSvg from '@/assets/NotFoundSvg.vue'
import ItemComponentSkeleton from './ItemComponentSkeleton.vue'
import ItemFilters from './ItemFilters.vue'
import ItemList from './ItemList.vue'

const route = useRoute()
const router = useRouter()

const catalogoStore = useCatalogoStore()
const { items, loading, error, totalCount, pageNumber, pageSize, totalPages } =
  storeToRefs(catalogoStore)

const filters = reactive<CatalogoFilters>({
  searchTerm: '',
  nome: '',
  descricao: '',
  catMat: '',
  especificacao: '',
  categoriaId: '',
  sortOrder: null,
  status: '',
})

const applyFilters = (newFilters: CatalogoFilters) => {
  const query: any = {}

  // Adiciona os filtros que estiverem preenchidos
  if (newFilters.searchTerm) query.searchTerm = newFilters.searchTerm
  if (newFilters.nome) query.nome = newFilters.nome
  if (newFilters.descricao) query.descricao = newFilters.descricao
  if (newFilters.catMat) query.catMat = newFilters.catMat
  if (newFilters.especificacao) query.especificacao = newFilters.especificacao
  if (newFilters.categoriaId) query.categoriaId = newFilters.categoriaId
  if (newFilters.status) {
    query.isActive = (newFilters.status === 'ativo').toString()
  }
  if (newFilters.sortOrder) query.sortOrder = newFilters.sortOrder

  // Reseta para a primeira página
  query.pageNumber = '1'

  router.push({ query })
}

const clearFilters = () => {
  router.push({ query: {} })
}

const getFirstQueryValue = (value: LocationQueryValue | LocationQueryValue[]): string => {
  if (Array.isArray(value)) {
    return value[0] || ''
  }
  return value || ''
}

const getStatusFromQuery = (value: LocationQueryValue | LocationQueryValue[]): string => {
  if (value === 'true') {
    return 'ativo'
  }
  if (value === 'false') {
    return 'inativo'
  }
  return ''
}

watch(
  () => route.query,
  (newQuery) => {
    filters.searchTerm = getFirstQueryValue(newQuery.searchTerm)
    filters.nome = getFirstQueryValue(newQuery.nome)
    filters.descricao = getFirstQueryValue(newQuery.descricao)
    filters.catMat = getFirstQueryValue(newQuery.catmat)
    filters.especificacao = getFirstQueryValue(newQuery.especificacao)
    filters.categoriaId = getFirstQueryValue(newQuery.categoriaId)
    filters.status = getStatusFromQuery(newQuery.isActive)

    const querySort = getFirstQueryValue(newQuery.sortOrder)
    filters.sortOrder = querySort === 'asc' || querySort === 'desc' ? querySort : null

    catalogoStore.fetchItems(newQuery)
  },
  { immediate: true },
)

const isDialogVisible = ref(false)
const selectedItem = ref<Item | null>(null)
const itemWasSaveChanged = ref(false)

const handleViewDetails = (item: Item) => {
  selectedItem.value = item
  isDialogVisible.value = true
}

const handleUpdateDialog = (payload: { item?: Item; action: string }) => {
  if (payload.action === 'updateItems') {
    catalogoStore.fetchItems(route.query)
  }
  if (payload.item) {
    selectedItem.value = payload.item
  }
}
const closeDialog = () => {
  isDialogVisible.value = false
}
</script>

<template>
  <div class="flex flex-column w-full h-full">
    <ItemFilters
      :initialFilters="filters"
      @apply-filters="applyFilters"
      @clear-filters="clearFilters"
    />

    <div v-if="loading" class="items-grid mt-4 gap-2">
      <ItemComponentSkeleton v-for="n in 10" :key="n" />
    </div>

    <ItemList
      v-if="items.length > 0 && !loading"
      :items="items"
      @view-details="handleViewDetails"
    />

    <div
      v-if="items.length === 0 && !loading"
      class="flex flex-column align-items-center mt-6 gap-2"
    >
      <div class="w-18rem sm:w-24rem md:w-30rem">
        <NotFoundSvg />
      </div>
      <h3 class="mb-2">Nenhum resultado encontrado.</h3>
      <p>Tente ajustar seus filtros ou utilize termos de busca diferentes.</p>
      <Button label="Limpar Filtros" icon="pi pi-filter-slash" @click="clearFilters" size="small" />
    </div>

    <CustomPaginator
      v-if="items.length > 0 && !loading"
      :current-url="route.path"
      :total-count="totalCount"
      :has-next-page="pageNumber < totalPages"
      :page-size="pageSize"
      :page-number="pageNumber"
    />

    <ItemDetailsDialog
      v-model:visible="isDialogVisible"
      :item="selectedItem"
      @update:visible="closeDialog"
      @update-dialog="handleUpdateDialog"
      @item-saved="itemWasSaveChanged = true"
    />
  </div>
</template>

<style scoped></style>
