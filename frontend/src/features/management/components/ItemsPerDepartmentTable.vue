<script setup lang="ts">
import IconField from 'primevue/iconfield'
import InputIcon from 'primevue/inputicon'
import InputText from 'primevue/inputtext'
import { Button, Tag, FloatLabel, Select } from 'primevue'
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

const expandedRows = ref([])
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

const filter = reactive<ItemsDepartmentFilters>({
  searchTerm: '',
  categoriaNome: '',
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

const columns = [
  { field: 'nome', header: 'Item' },
  { field: 'catMat', header: 'CATMAT' },
  { field: 'quantidadeTotalSolicitada', header: 'Qtde. Total' },
  { field: 'numeroDeSolicitacoes', header: 'Nº de Solicitações' },
  { field: 'valorTotalSolicitado', header: 'Valor Total' },
  { field: 'precoMedio', header: 'Preço Médio' },
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

watch(
  () => route.query,
  async (newQuery) => {
    const cleanFilters = mapQueryToFilters(newQuery)
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
      class="flex align-items-center justify-content-between lg:justify-content-start w-full gap-2"
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

        <FloatLabel class="w-full sm:w-16rem" variant="on">
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
        </FloatLabel>

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

    <div class="table-container mt-4 gap-2">
      <DataTable
        v-if="itemsDepartment.length > 0"
        :value="itemsDepartment"
        v-model:expandedRows="expandedRows"
        dataKey="id"
        tableStyle="min-width: 50rem"
        size="small"
        scrollable
        scrollHeight="550px"
        ref="dt"
        removableSort
      >
        <Column :expander="true" style="width: 3rem" />

        <Column
          v-for="col of columns"
          :key="col.field"
          :field="col.field"
          :header="col.header"
          sortable
        >
          <template #body="slotProps">
            <span
              v-if="
                col.field === 'quantidadeTotalSolicitada' || col.field === 'numeroDeSolicitacoes'
              "
            >
              <Tag :value="formatQuantity(slotProps.data[col.field])" severity="info" />
            </span>

            <span
              v-else-if="
                col.field === 'valorTotalSolicitado' ||
                col.field === 'precoMedio' ||
                col.field === 'precoMinimo' ||
                col.field === 'precoMaximo'
              "
            >
              <Tag severity="success" :value="formatCurrency(slotProps.data[col.field])" />
            </span>

            <span
              v-else-if="col.field === 'nome'"
              class="flex align-items-center justify-content-center gap-2"
            >
              {{ slotProps.data[col.field] }}
              <Button
                icon="pi pi-info-circle"
                text
                rounded
                severity="secondary"
                size="small"
                @click="showPopOverItem($event, slotProps.data)"
                v-tooltip="'Ver Detalhes do Item'"
              />
            </span>

            <span v-else>
              {{ slotProps.data[col.field] }}
            </span>
          </template>
        </Column>

        <Column header="Faixa de Preço (Min - Max)">
          <template #body="slotProps">
            <Tag severity="success" :value="formatCurrency(slotProps.data.precoMinimo)" />
            -
            <Tag severity="success" :value="formatCurrency(slotProps.data.precoMaximo)" />
          </template>
        </Column>

        <!-- <template #expansion="slotProps">
          <div class="flex gap-4 py-2 px-6">
            <div class="w-4rem flex-shrink-0">
              <img
                v-if="slotProps.data.linkImagem"
                :src="slotProps.data.linkImagem"
                :alt="slotProps.data.descricao"
                class="w-full"
              />
              <div v-else class="image-placeholder">
                <span class="material-symbols-outlined placeholder-icon"> hide_image </span>
              </div>
            </div>
            <div>
              <DataTable :value="slotProps.data.quantidadesPorDepartamento" size="small">
                <Column field="departamento" header="Departamento"></Column>
                <Column field="quantidadeTotal" header="Quantidade Solicitada"></Column>
              </DataTable>
            </div>
          </div>
        </template> -->
        <!-- TODO: ajustar qual será a exibição -->
        <template #expansion="slotProps">
          <div class="p-4 surface-50 border-round-md mx-2 mb-2">
            <!-- Header da expansão -->
            <div class="flex align-items-start gap-4 mb-4">
              <!-- Imagem -->
              <div class="w-6rem flex-shrink-0">
                <img
                  v-if="slotProps.data.linkImagem"
                  :src="slotProps.data.linkImagem"
                  :alt="slotProps.data.descricao"
                  class="w-full border-round shadow-2"
                />
                <div
                  v-else
                  class="image-placeholder w-full h-6rem flex align-items-center justify-content-center border-round surface-200"
                >
                  <i class="pi pi-image text-3xl text-500"></i>
                </div>
              </div>

              <!-- Informações principais -->
              <div class="flex-1">
                <div class="flex align-items-start justify-content-between mb-3">
                  <div>
                    <h5 class="m-0 mb-1 text-lg font-medium">{{ slotProps.data.nome }}</h5>
                    <p
                      v-if="slotProps.data.descricao"
                      class="m-0 text-sm text-color-secondary line-height-3 mb-2"
                    >
                      {{ slotProps.data.descricao }}
                    </p>
                  </div>

                  <!-- Tags de resumo -->
                  <div class="flex gap-2 flex-wrap">
                    <Tag
                      :value="`${formatQuantity(slotProps.data.quantidadeTotalSolicitada)} total`"
                      severity="info"
                      size="small"
                    />
                    <Tag
                      :value="`${slotProps.data.numeroDeSolicitacoes} solicitações`"
                      severity="secondary"
                      size="small"
                    />
                  </div>
                </div>

                <!-- Valores em cards pequenos -->
                <div class="grid grid-nogutter gap-3">
                  <div v-if="slotProps.data.valorTotalSolicitado" class="col-12 md:col-6 lg:col-4">
                    <div class="p-3 border-round surface-card text-center">
                      <div class="text-xs text-color-secondary mb-1">Valor Total</div>
                      <div class="text-lg font-bold text-green-600">
                        {{ formatCurrency(slotProps.data.valorTotalSolicitado) }}
                      </div>
                    </div>
                  </div>

                  <div v-if="slotProps.data.precoMedio" class="col-12 md:col-6 lg:col-4">
                    <div class="p-3 border-round surface-card text-center">
                      <div class="text-xs text-color-secondary mb-1">Preço Médio</div>
                      <div class="text-lg font-bold text-blue-600">
                        {{ formatCurrency(slotProps.data.precoMedio) }}
                      </div>
                    </div>
                  </div>

                  <div
                    v-if="slotProps.data.precoMinimo && slotProps.data.precoMaximo"
                    class="col-12 md:col-6 lg:col-4"
                  >
                    <div class="p-3 border-round surface-card text-center">
                      <div class="text-xs text-color-secondary mb-1">Faixa de Preço</div>
                      <div class="text-sm font-medium text-orange-600">
                        {{ formatCurrency(slotProps.data.precoMinimo) }} -
                        {{ formatCurrency(slotProps.data.precoMaximo) }}
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>

            <!-- Divisor -->
            <Divider class="my-4" />

            <!-- Tabela de departamentos com visual melhorado -->
            <div>
              <div class="flex align-items-center gap-2 mb-3">
                <i class="pi pi-building text-sm text-primary"></i>
                <h6 class="m-0 text-sm font-medium text-primary">Distribuição por Departamento</h6>
              </div>

              <!-- Cards ao invés de tabela para melhor visual -->
              <div class="grid grid-nogutter gap-2">
                <div
                  v-for="dept in slotProps.data.quantidadesPorDepartamento"
                  :key="dept.departamento"
                  class="col-12 sm:col-6 md:col-4 lg:col-3"
                >
                  <div
                    class="p-3 border-round-md surface-card border-1 surface-border hover:shadow-2 transition-all transition-duration-200"
                  >
                    <div class="flex align-items-center justify-content-between">
                      <div class="flex-1">
                        <div class="text-sm font-medium mb-1">{{ dept.departamento }}</div>
                        <div class="text-xs text-color-secondary">Quantidade solicitada</div>
                      </div>
                      <Badge
                        :value="formatQuantity(dept.quantidadeTotal)"
                        severity="success"
                        size="large"
                      />
                    </div>
                  </div>
                </div>
              </div>

              <!-- Fallback para quando não há dados de departamento -->
              <div
                v-if="
                  !slotProps.data.quantidadesPorDepartamento ||
                  slotProps.data.quantidadesPorDepartamento.length === 0
                "
                class="text-center py-4 text-color-secondary"
              >
                <i class="pi pi-info-circle text-2xl mb-2"></i>
                <div class="text-sm">Nenhuma informação de departamento disponível</div>
              </div>
            </div>
          </div>
        </template>
      </DataTable>
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
        <Button
          label="Limpar Filtros"
          icon="pi pi-filter-slash"
          @click="clearFilters"
          size="small"
        />
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
  </div>
</template>

<style scoped>
.table-container {
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
