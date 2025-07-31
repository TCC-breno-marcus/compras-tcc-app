<script setup lang="ts">
import CatalogUpload from './CatalogUpload.vue'
import { ref, watch, computed, onMounted } from 'vue'
import { useRoute, useRouter, type LocationQueryValue } from 'vue-router'
import type { Ref } from 'vue'
import IconField from 'primevue/iconfield'
import InputIcon from 'primevue/inputicon'
import InputText from 'primevue/inputtext'
import Select from 'primevue/select'
import ItemComponent from './ItemComponent.vue'
import ItemDetailsDialog from './ItemDetailsDialog.vue'
import type { Item } from '../types'
import { Button } from 'primevue'
import CustomPaginator from '@/components/CustomPaginator.vue'
import OverlayPanel from 'primevue/overlaypanel'
import { useCatalogoStore } from '../stores/catalogoStore'
import { storeToRefs } from 'pinia'
import FloatLabel from 'primevue/floatlabel'
import NotFoundSvg from '@/assets/NotFoundSvg.vue'
import ItemComponentSkeleton from './ItemComponentSkeleton.vue'
import CreateItemDialog from './CreateItemDialog.vue'
import { useCategoriaStore } from '../stores/categoriaStore'

const route = useRoute()
const router = useRouter()

const catalogoStore = useCatalogoStore()
const { items, loading, error, totalCount, pageNumber, pageSize, totalPages } =
  storeToRefs(catalogoStore)

const categoriaStore = useCategoriaStore()
const {
  categorias,
  loading: categoriasLoading,
  error: categoriasError,
} = storeToRefs(categoriaStore)

const getFirstQueryValue = (value: LocationQueryValue | LocationQueryValue[]): string => {
  if (Array.isArray(value)) {
    return value[0] || ''
  }
  return value || ''
}

const op = ref()
const simpleSearch = ref(getFirstQueryValue(route.query.searchTerm))
const nomeFilter = ref(getFirstQueryValue(route.query.nome))
const descricaoFilter = ref(getFirstQueryValue(route.query.descricao))
const catmatFilter = ref(getFirstQueryValue(route.query.catmat))
const especificacaoFilter = ref(getFirstQueryValue(route.query.especificacao))
const categoriaIdFilter = ref(getFirstQueryValue(route.query.categoriaId))
const statusFilter = ref('')
const opcoesStatus = ref([
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
  delete currentQuery.searchTerm
  delete currentQuery.nome
  delete currentQuery.descricao
  delete currentQuery.catmat
  delete currentQuery.especificacao
  delete currentQuery.categoriaId
  delete currentQuery.isActive
  delete currentQuery.sortOrder

  // Adiciona os filtros que estiverem preenchidos
  if (simpleSearch.value) currentQuery.searchTerm = simpleSearch.value
  if (nomeFilter.value) currentQuery.nome = nomeFilter.value
  if (descricaoFilter.value) currentQuery.descricao = descricaoFilter.value
  if (catmatFilter.value) currentQuery.catmat = catmatFilter.value
  if (especificacaoFilter.value) currentQuery.especificacao = especificacaoFilter.value
  if (categoriaIdFilter.value) currentQuery.categoriaId = categoriaIdFilter.value
  if (statusFilter.value) {
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
  categoriaIdFilter.value = ''
  statusFilter.value = ''
  simpleSearch.value = ''
  sortOrder.value = null
  applyFilters()
}

watch(
  () => route.query,
  (newQuery) => {
    simpleSearch.value = getFirstQueryValue(newQuery.searchTerm)
    nomeFilter.value = getFirstQueryValue(newQuery.nome)
    descricaoFilter.value = getFirstQueryValue(newQuery.descricao)
    catmatFilter.value = getFirstQueryValue(newQuery.catmat)
    especificacaoFilter.value = getFirstQueryValue(newQuery.especificacao)
    categoriaIdFilter.value = getFirstQueryValue(newQuery.categoriaId)

    if (newQuery.isActive === 'true') {
      statusFilter.value = 'ativo'
    } else if (newQuery.isActive === 'false') {
      statusFilter.value = 'inativo'
    } else {
      statusFilter.value = ''
    }
    catalogoStore.fetchItems(newQuery)
  },
  { immediate: true },
)

onMounted(() => {
  categoriaStore.fetch()
})

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
    <!-- TODO: ESSES BOTOES DE FILTRO NÃO ESTAO LEGAL PRA TELA media -->

    <div
      class="flex flex-column lg:flex-row lg:align-items-center justify-content-between mt-2 w-full"
    >
      <!-- TODO: essa div de criar e importar ainda parece jogada no layout -->
      <div class="flex lg:flex-order-2 w-full sm:w-auto justify-content-end gap-2">
        <Button
          type="button"
          label="Criar"
          icon="pi pi-plus"
          size="small"
          text
          @click="isCreateDialogVisible = true"
        />
        <CatalogUpload />
      </div>
      <div
        class="filters flex lg:flex-order-1 flex-column sm:flex-wrap sm:flex-row align-items-center sm:w-full lg:w-auto gap-2 p-3 xl:p-0"
      >
        <FloatLabel class="w-full sm:w-18rem" variant="on">
          <IconField iconPosition="left">
            <InputIcon class="pi pi-search"></InputIcon>
            <InputText
              v-model="simpleSearch"
              size="small"
              class="w-full"
              inputId="simple-search"
              @keyup.enter="applyFilters"
            />
          </IconField>
          <label for="simple-search">Pesquisar item</label>
        </FloatLabel>

        <FloatLabel class="w-full sm:w-16rem mt-1 sm:mt-0" variant="on">
          <Select
            v-model="categoriaIdFilter"
            :options="categorias"
            optionLabel="nome"
            optionValue="id"
            inputId="categoria-filter"
            size="small"
            class="w-full sm:w-16rem"
            :showClear="true"
          />
          <label for="categoria-filter">Categoria</label>
        </FloatLabel>

        <FloatLabel class="w-full sm:w-8rem mt-1 sm:mt-0" variant="on">
          <Select
            v-model="statusFilter"
            :options="opcoesStatus"
            optionLabel="name"
            optionValue="code"
            inputId="status-filter"
            size="small"
            class="w-full sm:w-8rem"
            :showClear="true"
          />
          <label for="status-filter">Status</label>
        </FloatLabel>

        <div class="flex sm:flex-wrap w-full sm:w-auto gap-2">
          <Button
            :icon="computedSort.icon"
            @click="toggleSortDirection"
            label="Ordem"
            outlined
            size="small"
            aria-label="Ordem"
            v-tooltip.top="computedSort.text"
          />
          <Button
            type="button"
            label="Filtros Avançados"
            icon="pi pi-filter"
            size="small"
            outlined
            class="w-12rem"
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

        <div class="flex sm:flex-wrap w-full sm:w-auto gap-2 justify-content-end">
          <Button
            label="Limpar"
            icon="pi pi-filter-slash"
            severity="danger"
            text
            @click="clearFilters"
            size="small"
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
    </div>

    <div v-if="loading" class="items-grid mt-4 gap-2">
      <ItemComponentSkeleton v-for="n in 10" :key="n" />
    </div>

    <div v-if="items.length > 0 && !loading" class="items-grid mt-4 gap-2">
      <ItemComponent
        v-for="item in items"
        :key="item.catMat"
        :item="item"
        @viewDetails="handleViewDetails"
      />
    </div>
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
