<script setup lang="ts">
import IconField from 'primevue/iconfield'
import InputIcon from 'primevue/inputicon'
import InputText from 'primevue/inputtext'
import { Button, ButtonGroup, Tag, FloatLabel, Select, useToast } from 'primevue'
import { computed, nextTick, onMounted, reactive, ref } from 'vue'
import Column from 'primevue/column'
import DataTable from 'primevue/datatable'
import { watch } from 'vue'
import type { ItemDepartmentResponse, ItemsDepartmentFilters } from '@/features/reports/types'
import { useRoute } from 'vue-router'
import router from '@/router'
import { mapQueryToFilters } from '@/features/reports/utils/queryHelper'
import { useReportStore } from '@/features/reports/stores/reportStore'
import { storeToRefs } from 'pinia'
import { useAuthStore } from '@/features/autentication/stores/authStore'
import { formatDate } from '@/utils/dateUtils'
import { formatCurrency } from '@/utils/currency'
import { formatQuantity } from '@/utils/number'
import CustomPopOverItem from './CustomPopOverItem.vue'
import { Popover } from 'primevue'
import { useCategoriaStore } from '@/features/catalogo/stores/categoriaStore'
import CustomPaginator from '@/components/ui/CustomPaginator.vue'
import ItemPerDepartmentCard from './ItemPerDepartmentCard.vue'
import { reportService } from '@/features/reports/services/reportService'

// TODO: esse componente é um backup pra o caso de decidir usar tabela pra visualizar esses dados ao invés de cards

const expandedRows = ref([])
const reportLoading = ref(false)
const route = useRoute()
const reportStore = useReportStore()
const { itemsDepartment, isLoading, totalCount, pageNumber, pageSize, totalPages } =
  storeToRefs(reportStore)
const authStore = useAuthStore()
const { departamentos } = storeToRefs(authStore)
const dt = ref()
const op = ref()
const selectedItemDetails = ref<ItemDepartmentResponse | null>(null)
const categoriaStore = useCategoriaStore()
const { categorias } = storeToRefs(categoriaStore)
const toast = useToast()
const reportType = ref<'geral' | 'patrimonial'>('geral')

const filter = reactive<ItemsDepartmentFilters>({
  searchTerm: '',
  categoriaNome: '',
  itemsType: null,
  departamento: '',
  sortOrder: null,
  pageNumber: '1',
  pageSize: '50',
})

const applyFilters = () => {
  const query: any = {}

  if (filter.searchTerm) query.searchTerm = filter.searchTerm
  if (filter.categoriaNome) query.categoriaNome = filter.categoriaNome
  if (filter.departamento) query.departamento = filter.departamento
  if (filter.sortOrder) query.sortOrder = filter.sortOrder

  // Reseta para a primeira página
  query.pageNumber = '1'

  router.push({ query })
}

const clearFilters = () => {
  router.push({ query: {} })
}

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
    return { text: 'Ordenar de A-Z', icon: 'pi pi-sort-amount-up' }
  }
  if (filter.sortOrder === 'desc') {
    return { text: 'Ordenar de Z-A', icon: 'pi pi-sort-amount-down' }
  }
  return { text: 'Ordenar por Nome', icon: 'pi pi-sort-alt' }
})

const showPopOverItem = (event: MouseEvent, item: ItemDepartmentResponse) => {
  if (op.value) {
    op.value.hide()
  }

  if (selectedItemDetails.value?.id === item.id) {
    selectedItemDetails.value = null
    return
  }

  selectedItemDetails.value = item

  nextTick(() => {
    if (op.value && selectedItemDetails.value) {
      op.value.show(event)
    }
  })
}

const handleExportCsv = async () => {
  try {
    reportLoading.value = true

    const csvBlob = await reportService.getCsvItemsPerDepartment(reportType.value)

    const url = window.URL.createObjectURL(csvBlob)
    const link = document.createElement('a')
    link.href = url
    link.download = `relatorio_itens_por_departamento_${reportType.value}.csv`

    document.body.appendChild(link)
    link.click()
    document.body.removeChild(link)
    window.URL.revokeObjectURL(url)
    toast.add({
      severity: 'success',
      summary: 'Sucesso',
      detail: 'Relatório exportado com sucesso.',
      life: 3000,
    })
  } catch (error) {
    console.error('Erro ao exportar CSV:', error)
    toast.add({
      severity: 'error',
      summary: 'Erro',
      detail: 'Falha ao exportar relatório.',
      life: 3000,
    })
  } finally {
    reportLoading.value = false
  }
}

watch(
  [() => route.query, reportType],
  async ([newQuery, newReportType]) => {
    const cleanFilters = mapQueryToFilters(newQuery)

    cleanFilters.itemsType = newReportType

    Object.assign(filter, cleanFilters)
    reportStore.fetchItemsPerDepartment(cleanFilters)
  },
  { immediate: true },
)

onMounted(() => {
  if (!authStore.departamentos?.length) {
    authStore.fetchDeptos()
  }
  if (!categoriaStore.categorias.length) {
    categoriaStore.fetch()
  }
})
</script>

<template>
  <div class="flex flex-column w-full h-full">
    <div
      class="flex flex-column sm:flex-row w-full justify-content-between align-items-center sm:align-items-start gap-2"
    >
      <div class="flex flex-wrap align-items-center gap-2">
        <FloatLabel class="w-full sm:w-16rem" variant="on">
          <IconField iconPosition="left">
            <InputIcon class="pi pi-search"></InputIcon>
            <InputText
              v-model="filter.searchTerm"
              size="small"
              class="w-full"
              inputId="id-search"
              @keyup.enter="applyFilters"
            />
          </IconField>
          <label for="id-search">Pesquisar item</label>
        </FloatLabel>

        <!-- <FloatLabel class="w-full sm:w-16rem" variant="on">
          <Select
            v-model="filter.categoriaNome"
            :options="categorias"
            optionLabel="nome"
            optionValue="nome"
            inputId="categoria"
            size="small"
            class="w-full"
            id="categoria"
            :showClear="true"
            filter
          />
          <label for="categoria">Categoria do Item</label>
        </FloatLabel> -->

        <FloatLabel class="w-full sm:w-16rem" variant="on">
          <Select
            v-model="filter.departamento"
            :options="departamentos"
            inputId="departamento"
            size="small"
            class="w-full"
            id="departamento"
            :showClear="true"
            filter
          />
          <label for="departamento">Departamento</label>
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
          icon="pi pi-times"
          severity="danger"
          text
          @click="clearFilters"
          size="small"
        />
        <!-- TODO: trocar icones dos filtros de busca e clear pra esses que estão aqui -->
        <Button
          type="button"
          label="Buscar"
          icon="pi pi-search"
          size="small"
          @click="applyFilters"
        />
      </div>

      <!-- <div
        class="flex flex-order-1 sm:flex-order-3 flex-row align-items-center gap-2 p-3 pb-0 sm:pb-3 xl:p-0"
      >
        <Button
          type="button"
          label="Exportar"
          icon="pi pi-download"
          size="small"
          text
          :onClick="handleExportCsv"
          :loading="reportLoading"
        />
      </div> -->
    </div>

    <div class="flex justify-content-between w-full mt-4 mb-2">
      <div class="flex">
        <ButtonGroup>
          <Button
            type="button"
            label="Itens Gerais"
            icon="pi pi-box"
            size="small"
            :outlined="reportType !== 'geral'"
            @click="reportType = 'geral'"
          />
          <Button
            type="button"
            label="Itens Patrimoniais"
            icon="pi pi-building"
            size="small"
            :outlined="reportType !== 'patrimonial'"
            @click="reportType = 'patrimonial'"
          />
        </ButtonGroup>
      </div>
      <Button
        type="button"
        label="Exportar"
        icon="pi pi-download"
        size="small"
        text
        :onClick="handleExportCsv"
        :loading="reportLoading"
      />
    </div>

    <div class="table-container">
      <ItemPerDepartmentCard
        v-if="itemsDepartment.length > 0"
        v-for="item in itemsDepartment"
        :key="item.id"
        :item="item"
        @show-pop-over="showPopOverItem"
      />
    </div>

    <Popover ref="op" :dismissable="true">
      <CustomPopOverItem :item="selectedItemDetails" />
    </Popover>

    <div
      v-if="itemsDepartment.length === 0 && !isLoading"
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
      v-if="itemsDepartment.length > 0 && !isLoading"
      :current-url="route.path"
      :total-count="totalCount"
      :has-next-page="pageNumber < totalPages"
      :page-size="pageSize"
      :page-number="pageNumber"
    />
  </div>
</template>

<style scoped>
.table-container {
  justify-content: center;
  max-height: calc(100vh - 340px);
  overflow-y: auto;
  /* Para Firefox */
  scrollbar-width: thin;
  scrollbar-color: var(--p-surface-400) transparent;
}

:deep(.p-toolbar-start) {
  flex: 1 1 auto;
}

.image-placeholder {
  width: 100%;
  height: 100%;
  display: flex;
  justify-content: center;
  align-items: center;
  border-radius: 6px;
}

.placeholder-icon {
  font-size: 2rem;
  /* color: var(--p-surface-500);  */
}
</style>
