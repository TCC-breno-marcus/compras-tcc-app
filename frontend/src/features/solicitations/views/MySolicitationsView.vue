<script setup lang="ts">
import { ref, computed, onMounted, watch, reactive } from 'vue'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import CustomBreadcrumb from '@/components/ui/CustomBreadcrumb.vue'
import Button from 'primevue/button'
import { useRoute, type LocationQuery } from 'vue-router'
import { useMySolicitationListStore } from '../stores/mySolicitationList'
import { storeToRefs } from 'pinia'
import { formatDate } from '@/utils/dateUtils'
import { formatCurrency } from '@/utils/currency'
import CustomPaginator from '@/components/ui/CustomPaginator.vue'
import router from '@/router'
import { MultiSelect, Tag } from 'primevue'
import IconField from 'primevue/iconfield'
import InputIcon from 'primevue/inputicon'
import InputText from 'primevue/inputtext'
import Select from 'primevue/select'
import FloatLabel from 'primevue/floatlabel'
import type { MySolicitationFilters } from '../types'
import DatePicker from 'primevue/datepicker'
import MySolicitationsSkeleton from '../components/MySolicitationsSkeleton.vue'
import { mapQueryToFilters } from '../utils/queryHelper'
import { getSolicitationStatusOptions } from '../utils'
import { SOLICITATION_STATUS } from '../constants'

const route = useRoute()
const mySolicitationListStore = useMySolicitationListStore()
const { solicitations, isLoading, error, totalCount, pageNumber, pageSize, totalPages } =
  storeToRefs(mySolicitationListStore)

const filter = reactive<MySolicitationFilters>({
  externalId: '',
  tipo: '',
  dateRange: null,
  sortOrder: null,
  pageNumber: '1',
  pageSize: '10',
  statusIds: [],
})

const typeOptions = ref(['Geral', 'Patrimonial'])

const applyFilters = () => {
  const query: any = {}

  // Adiciona os filtros que estiverem preenchidos
  if (filter.externalId) query.externalId = filter.externalId
  if (filter.tipo) query.tipo = filter.tipo

  if (filter.dateRange) {
    if (Array.isArray(filter.dateRange)) {
      if (filter.dateRange[0]) {
        query.dataInicial = filter.dateRange[0].toISOString().split('T')[0]
      }
      if (filter.dateRange[1]) {
        query.dataFinal = filter.dateRange[1].toISOString().split('T')[0]
      }
    } else {
      query.dataInicial = filter.dateRange.toISOString().split('T')[0]
    }
  }

  if (filter.sortOrder) query.sortOrder = filter.sortOrder

  if (filter.statusIds && filter.statusIds.length > 0) {
    query.statusIds = filter.statusIds
  }

  // Reseta para a primeira página
  query.pageNumber = '1'

  router.push({ query })
}

const clearFilters = () => {
  router.push({ query: {} })
}

const columns = [
  { field: 'externalId', header: 'Código' },
  { field: 'dataCriacao', header: 'Data de Criação' },
  { field: 'typeDisplay', header: 'Tipo' },
  { field: 'status', header: 'Status' },

  // { field: 'itemsCount', header: 'Itens Únicos' },
  // { field: 'totalItemsQuantity', header: 'Total de Itens' },
  // { field: 'totalEstimatedPrice', header: 'Preço Total Estimado' },
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
    return { text: 'Mais Antigos', icon: 'pi pi-sort-amount-up' }
  }
  if (filter.sortOrder === 'desc') {
    return { text: 'Mais Recentes', icon: 'pi pi-sort-amount-down' }
  }
  return { text: 'Ordenar por Data', icon: 'pi pi-sort-alt' }
})

const verDetalhes = (id: number) => {
  router.push(`/solicitacoes/${id}`)
}

const goToCreatePage = (type: 'geral' | 'patrimonial') => {
  router.push(`/solicitacoes/criar/${type}`)
}

const getTagProps = (statusId: number) => {
  const statusData = getSolicitationStatusOptions(statusId)
  return {
    value: statusData?.nome,
    severity: statusData?.severity,
    icon: statusData?.icon,
  }
}

watch(
  () => route.query,
  async (newQuery) => {
    const cleanFilters = mapQueryToFilters(newQuery, 'MySolicitationFilters')
    Object.assign(filter, cleanFilters)
    mySolicitationListStore.fetchAll(cleanFilters as MySolicitationFilters)
  },
  { immediate: true },
)
</script>

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
              v-model="filter.externalId"
              size="small"
              class="w-full"
              inputId="id-search"
              @keyup.enter="applyFilters"
            />
          </IconField>
          <label for="id-search">Código</label>
        </FloatLabel>

        <FloatLabel variant="on" class="w-full sm:w-14rem">
          <DatePicker
            v-model="filter.dateRange"
            selectionMode="range"
            dateFormat="dd/mm/yy"
            inputId="date-filter"
            showIcon
            class="w-full sm:w-14rem"
            iconDisplay="input"
            size="small"
          />
          <label for="date-filter">Data de Criação</label>
        </FloatLabel>

        <FloatLabel class="w-full sm:w-10rem mt-1 sm:mt-0" variant="on">
          <Select
            v-model="filter.tipo"
            :options="typeOptions"
            inputId="tipo-filter"
            size="small"
            class="w-full sm:w-10rem"
            :showClear="true"
          />
          <label for="tipo-filter">Tipo</label>
        </FloatLabel>

        <FloatLabel class="w-full sm:w-14rem mt-1 sm:mt-0" variant="on">
          <MultiSelect
            id="status"
            v-model="filter.statusIds"
            :options="SOLICITATION_STATUS"
            optionLabel="nome"
            option-value="id"
            filter
            class="w-full"
            size="small"
            display="chip"
          />
          <label for="status">Status</label>
        </FloatLabel>

        <Button
          :icon="computedSort.icon"
          @click="toggleSortDirection"
          :label="computedSort.text"
          outlined
          size="small"
          aria-label="Ordenar"
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

    <MySolicitationsSkeleton v-if="isLoading" />

    <DataTable
      v-if="solicitations.length > 0 && !isLoading"
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

          <span v-else-if="col.field === 'status'">
            <Tag v-bind="getTagProps(slotProps.data.status.id)" />
          </span>

          <!-- <span v-else-if="col.field === 'itemsCount'">
            <Tag :value="slotProps.data.itemsCount" severity="secondary" />
          </span>

          <span v-else-if="col.field === 'totalItemsQuantity'">
            <Tag :value="slotProps.data.totalItemsQuantity" />
          </span>

          <span v-else-if="col.field === 'totalEstimatedPrice'">
            <Tag severity="success" :value="formatCurrency(slotProps.data.totalEstimatedPrice)" />
          </span> -->

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
            v-tooltip.left="'Ver Detalhes'"
            @click="verDetalhes(slotProps.data.id)"
          />
        </template>
      </Column>
    </DataTable>

    <div v-if="solicitations.length === 0 && !isLoading" class="text-center p-5">
      <div class="flex flex-column align-items-center">
        <i class="pi pi-inbox text-4xl text-color-secondary mb-3"></i>

        <h4 class="font-bold mt-0 mb-2">Nenhuma Solicitação Encontrada</h4>
        <p class="text-color-secondary mt-0 mb-4">
          Clique em um dos botões abaixo para criar sua primeira solicitação ou refaça os filtros.
        </p>

        <div class="flex align-items-center gap-2">
          <Button label="Geral" icon="pi pi-plus" @click="goToCreatePage('geral')" size="small" />
          <Button
            label="Patrimonial"
            icon="pi pi-plus"
            @click="goToCreatePage('patrimonial')"
            size="small"
          />
        </div>
      </div>
    </div>

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
