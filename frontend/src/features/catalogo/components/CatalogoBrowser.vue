<script setup lang="ts">
import { inject, reactive, ref, watch } from 'vue'
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
import { mapQueryToFilters, applyPreFilters } from '../utils/queryHelper'
import { SolicitationContextKey } from '@/features/solicitations/keys'
import CreateItemDialog from './CreateItemDialog.vue'
import CatalogUpload from './CatalogUpload.vue'

const props = defineProps<{
  /**
   * Define uma lista inicial de nomes de categoria para filtrar o catálogo.
   * Se não for fornecido, nenhum filtro de categoria padrão será aplicado.
   */
  categoryNames?: string[]
}>()

const solicitationContext = inject(SolicitationContextKey)
// Todo: Injetar também depois o contexto de management

const route = useRoute()
const router = useRouter()

const catalogoStore = useCatalogoStore()
const { items, loading, error, totalCount, pageNumber, pageSize, totalPages } =
  storeToRefs(catalogoStore)
const categoriaStore = useCategoriaStore()
const { categorias, loading: loadingCategorys } = storeToRefs(categoriaStore)

const initFilters = reactive<CatalogoFilters>({
  searchTerm: '',
  nome: '',
  descricao: '',
  catMat: '',
  especificacao: '',
  categoriaId: [],
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
    const finalFilters = applyPreFilters(filtersFromUrl, categorias.value, {
      categoryNames: props.categoryNames,
      status: solicitationContext ? 'ativo' : '',
    })
    catalogoStore.fetchItems(finalFilters)
  },
  { immediate: true },
)

const isCreateDialogVisible = ref(false)
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
    <div
      class="flex flex-column sm:flex-row w-full justify-content-between align-items-center sm:align-items-start gap-2"
    >
      <ItemFilters
        class="flex-order-2"
        :initialFilters="initFilters"
        :showStatus="!solicitationContext"
        @apply-filters="applyFilters"
        @clear-filters="clearFilters"
      />

      <div
        v-if="!solicitationContext"
        class="flex flex-order-1 sm:flex-order-3 flex-row align-items-center gap-2 p-3 pb-0 sm:pb-3 xl:p-0"
      >
        <!-- TODO: ao invés de dois botões, explorar outras ideias como talvez um botão só que abre um menu e escolhe 
         entre criar um item ou múltiplos itens com a planilha -->
        <Button
          type="button"
          label="Criar"
          icon="pi pi-plus"
          size="small"
          text
          @click="isCreateDialogVisible = true"
        />
        <!-- TODO: Removi a opção de importar. Mais info: https://github.com/TCC-breno-marcus/compras-tcc-app/issues/53-->
        <!-- <CatalogUpload /> -->
      </div>
    </div>
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
      :responsive-breakpoint="solicitationContext ? 'xl' : 'md'"
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

    <CreateItemDialog
      v-model:visible="isCreateDialogVisible"
      @update:visible="isCreateDialogVisible = false"
      @update-dialog="handleUpdateDialog"
    />
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
