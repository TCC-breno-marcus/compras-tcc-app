<script setup lang="ts">
import { ref, computed, onMounted, watch, reactive } from 'vue'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import CustomBreadcrumb from '@/components/ui/CustomBreadcrumb.vue'
import Button from 'primevue/button'
import { useRoute } from 'vue-router'
import { useMySolicitationListStore } from '../stores/mySolicitationList'
import { storeToRefs } from 'pinia'
import { formatDate } from '@/utils/dateUtils'
import { formatCurrency } from '@/utils/currency'
import CustomPaginator from '@/components/ui/CustomPaginator.vue'
import router from '@/router'
import { Tag } from 'primevue'
import IconField from 'primevue/iconfield'
import InputIcon from 'primevue/inputicon'
import InputText from 'primevue/inputtext'
import Select from 'primevue/select'
import FloatLabel from 'primevue/floatlabel'
import type { MySolicitationFilters } from '..'
import DatePicker from 'primevue/datepicker'

const route = useRoute()
const mySolicitationListStore = useMySolicitationListStore()
const { solicitations, isLoading, error, totalCount, pageNumber, pageSize, totalPages } =
  storeToRefs(mySolicitationListStore)

watch(
  () => route.query,
  async (newQuery) => {
    mySolicitationListStore.fetchAll()
  },
  { immediate: true },
)

const filter = reactive<MySolicitationFilters>({
  idFilter: '',
  typeFilter: '',
  dateFilter: null,
  sortOrder: null,
})

const typeOptions = ref(['Geral', 'Patrimonial'])

const applyFilters = () => {
  const query: any = {}

  // Adiciona os filtros que estiverem preenchidos
  if (filter.idFilter) query.idFilter = filter.idFilter
  if (filter.typeFilter) query.typeFilter = filter.typeFilter
  if (filter.dateFilter) {
    query.dateFilter = filter.dateFilter.toISOString().split('T')[0]
  }
  if (filter.sortOrder) query.sortOrder = filter.sortOrder

  // Reseta para a primeira página
  query.pageNumber = '1'

  router.push({ query })
}

const clearFilters = () => {
  router.push({ query: {} })
}

const columns = [
  { field: 'id', header: 'ID' },
  { field: 'typeDisplay', header: 'Tipo' },
  { field: 'dataCriacao', header: 'Data de Criação' },
  { field: 'itemsCount', header: 'Itens Únicos' },
  { field: 'totalItemsQuantity', header: 'Total de Itens' },
  { field: 'totalEstimatedPrice', header: 'Preço Total Estimado' },
]

const toggleSortDirection = () => {
  if (filter.sortOrder === null) {
    filter.sortOrder = 'asc'
  } else if (filter.sortOrder === 'asc') {
    filter.sortOrder = 'desc'
  } else {
    filter.sortOrder = null
  }
  applyFilters()
}

const computedSort = computed(() => {
  if (filter.sortOrder === 'asc') {
    return { text: 'Ordenar de A-Z', icon: 'pi pi-sort-alpha-down' }
  }
  if (filter.sortOrder === 'desc') {
    return { text: 'Ordenar de Z-A', icon: 'pi pi-sort-alpha-up' }
  }
  return { text: 'Não Ordenar', icon: 'pi pi-sort-alt' }
})

const verDetalhes = (id: number) => {
  router.push(`/solicitacoes/${id}`)
}
</script>

<!-- TODO: ajustar repsonsivdade dessa página -->
<template>
  <div class="flex flex-column w-full h-full align-items-center p-2 gap-3">
    <div class="flex w-full">
      <CustomBreadcrumb />
    </div>
    <div class="flex align-items-center justify-content-between w-full">
      <h3 class="m-0 pl-2">Minhas Solicitações</h3>
    </div>

    <div
      class="flex align-items-center justify-content-between lg:justify-content-start w-full gap-2"
    >
      <div class="flex flex-wrap align-items-center gap-2">
        <FloatLabel class="w-full sm:w-12rem" variant="on">
          <IconField iconPosition="left">
            <InputIcon class="pi pi-search"></InputIcon>
            <InputText
              v-model="filter.idFilter"
              size="small"
              class="w-full"
              inputId="id-search"
              @keyup.enter="applyFilters"
            />
          </IconField>
          <label for="id-search">Buscar ID</label>
        </FloatLabel>

        <FloatLabel class="w-full sm:w-10rem mt-1 sm:mt-0" variant="on">
          <Select
            v-model="filter.typeFilter"
            :options="typeOptions"
            inputId="typeFilter-filter"
            size="small"
            class="w-full sm:w-10rem"
            :showClear="true"
          />
          <label for="typeFilter-filter">Tipo</label>
        </FloatLabel>
        <FloatLabel variant="on" class="w-full sm:w-12rem">
          <DatePicker
            v-model="filter.dateFilter"
            dateFormat="dd/mm/yy"
            inputId="date-filter"
            showIcon
            class="w-full sm:w-12rem"
            iconDisplay="input"
          />
          <label for="date-filter">Data de Criação</label>
        </FloatLabel>

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

    <DataTable
      :value="solicitations"
      tableStyle="min-width: 50rem"
      class="w-full"
      size="small"
      removableSort
    >
      <Column
        v-for="col of columns"
        :key="col.field"
        :field="col.field"
        :header="col.header"
        sortable
      >
        <template #body="slotProps">
          <span v-if="col.field === 'dataCriacao'">
            {{ formatDate(slotProps.data.dataCriacao, 'short') }}
          </span>

          <span v-else-if="col.field === 'itemsCount'">
            <Tag :value="slotProps.data.itemsCount" severity="secondary" />
          </span>

          <span v-else-if="col.field === 'totalItemsQuantity'">
            <Tag :value="slotProps.data.totalItemsQuantity" />
          </span>

          <span v-else-if="col.field === 'totalEstimatedPrice'">
            <Tag severity="success" :value="formatCurrency(slotProps.data.totalEstimatedPrice)" />
          </span>

          <span v-else>
            {{ slotProps.data[col.field] }}
          </span>
        </template>
      </Column>
      <Column header="Ações">
        <template #body="slotProps">
          <Button
            icon="pi pi-eye"
            text
            rounded
            aria-label="Ver Detalhes"
            v-tooltip="'Ver Detalhes'"
            @click="verDetalhes(slotProps.data.id)"
          />
        </template>
      </Column>
    </DataTable>

    <CustomPaginator
      v-if="solicitations.length > 0 && !isLoading"
      :current-url="route.path"
      :total-count="totalCount"
      :has-next-page="pageNumber < totalPages"
      :page-size="pageSize"
      :page-number="pageNumber"
    />
  </div>
</template>
