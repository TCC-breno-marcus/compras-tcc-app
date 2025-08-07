<script setup lang="ts">
import { reactive, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import ItemDetailsDialog from './ItemDetailsDialog.vue'
import type { CatalogoFilters, Item } from '../types'
import { Button } from 'primevue'
import CustomPaginator from '@/components/ui/CustomPaginator.vue'
import { useCatalogoStore } from '../stores/catalogoStore'
import { storeToRefs } from 'pinia'
import NotFoundSvg from '@/assets/NotFoundSvg.vue'
import ItemComponentSkeleton from './ItemComponentSkeleton.vue'
import ItemFilters from './ItemFilters.vue'
import ItemList from './ItemList.vue'
import { useCategoriaStore } from '../stores/categoriaStore'
import { categorysIdFilterPerName } from '../utils/categoriaTransformer'
import { mapQueryToFilters, mountQueryWithPreFilterCategory } from '../utils/queryHelper'

const props = defineProps<{
  /**
   * Define uma lista inicial de nomes de categoria para filtrar o catálogo.
   * Se não for fornecido, nenhum filtro de categoria padrão será aplicado.
   */
  categoryNames?: string[]
}>()

const route = useRoute()
const router = useRouter()

const catalogoStore = useCatalogoStore()
const { items, loading, error, totalCount, pageNumber, pageSize, totalPages } =
  storeToRefs(catalogoStore)
const categoriaStore = useCategoriaStore()
const { categorias, loading: loadingCategorys } = storeToRefs(categoriaStore)

const filters = reactive<CatalogoFilters>({
  searchTerm: '',
  nome: '',
  descricao: '',
  catMat: '',
  especificacao: '',
  categoriaId: props.categoryNames
    ? categorysIdFilterPerName(categorias.value, props.categoryNames)
    : [],
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

watch(
  () => route.query,
  async (newQuery) => {
    const filtersFromUrl = mapQueryToFilters(newQuery)

    if (props.categoryNames && props.categoryNames.length > 0) {
      await categoriaStore.fetch({ nome: props.categoryNames })
    }
    const finalFilters = mountQueryWithPreFilterCategory(
      filtersFromUrl,
      categorias.value,
      props.categoryNames,
    )
    catalogoStore.fetchItems(finalFilters)
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
    const filters = mapQueryToFilters(route.query)
    catalogoStore.fetchItems(filters)
  }
  if (payload.item) {
    selectedItem.value = payload.item
  }
}
const closeDialog = () => {
  isDialogVisible.value = false
}

defineExpose({
  closeDialog,
})
</script>

<template>
  <div class="flex flex-column w-full h-full">
    <ItemFilters
      :initialFilters="filters"
      @apply-filters="applyFilters"
      @clear-filters="clearFilters"
    />

    <div v-if="loading" class="items-grid mt-2 mb-6 gap-2">
      <ItemComponentSkeleton v-for="n in 50" :key="n" />
    </div>

    <ItemList v-if="items.length > 0 && !loading" :items="items" @view-details="handleViewDetails">
      <template #actions="{ item }">
        <slot name="actions" :item="item"></slot>
      </template>
    </ItemList>

    <div
      v-if="items.length === 0 && !loading && !loadingCategorys"
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
    >
      <template #dialog-actions="{ item }">
        <slot name="dialog-actions" :item="item"></slot>
      </template>
    </ItemDetailsDialog>
  </div>
</template>

<style scoped>
.items-grid {
  display: flex;
  flex-wrap: wrap;
  justify-content: center;
  max-height: calc(100vh - 300px);
  overflow-y: auto;
  /* Para Firefox */
  scrollbar-width: thin;
  scrollbar-color: var(--p-surface-400) transparent;
}

:deep(.p-toolbar-start) {
  flex: 1 1 auto;
}
</style>
