<script setup lang="ts">
import { ref, computed, onUnmounted, onMounted, watch } from 'vue'
import { storeToRefs } from 'pinia'

// PrimeVue Components
import Panel from 'primevue/panel'
import Select from 'primevue/select'
import Button from 'primevue/button'
import DatePicker from 'primevue/datepicker'
import FloatLabel from 'primevue/floatlabel'
import Card from 'primevue/card'
import Chart from 'primevue/chart'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import ProgressSpinner from 'primevue/progressspinner'
import Message from 'primevue/message'

// Utils (Supondo que existam baseados no seu exemplo)
import { formatCurrency } from '@/utils/currency'
import { useReportStore } from '@/features/reports/stores/reportStore'
import type { ReportType } from '@/features/reports/types'
import { useRoute } from 'vue-router'

const reportStore = useReportStore()
const { isLoading, error, activeReportType, centerData, categoryData } = storeToRefs(reportStore)

// --- Filtros ---
const dateRange = ref<Date[]>()
const selectedType = ref<ReportType>(null)

const route = useRoute()

const reportOptions = [
  { label: 'Gastos por Centro', value: 'GASTOS_CENTRO' },
  { label: 'Consumo por Categoria', value: 'CONSUMO_CATEGORIA' },
]

// --- Ações ---
const handleGenerate = () => {
  if (!dateRange.value || dateRange.value.length < 2 || !selectedType.value) {
    return // Poderia adicionar um toast de aviso aqui
  }

  const start = dateRange.value[0].toISOString()
  const end = dateRange.value[1]
  end.setHours(23, 59, 59, 999)

  reportStore.generateReport(selectedType.value, {
    DataInicio: start,
    DataFim: end.toISOString(),
  })
}

const resetScreen = () => {
  reportStore.$reset()
  dateRange.value = undefined
  selectedType.value = null
}

watch(
  () => route.path,
  () => {
    resetScreen()
  },
  { immediate: true },
)

const setChartOptions = (title: string, type: string) => {
  const documentStyle = getComputedStyle(document.documentElement)
  const textColor = documentStyle.getPropertyValue('--p-text-color')
  const textColorSecondary = documentStyle.getPropertyValue('--p-text-muted-color')
  const surfaceBorder = documentStyle.getPropertyValue('--p-content-border-color')

  const baseOptions = {
    maintainAspectRatio: false,
    aspectRatio: 1.5,
    plugins: {
      legend: {
        position: type === 'bar' ? 'top' : 'right',
        labels: { color: textColor },
      },
      title: {
        display: true,
        text: title,
        font: { size: 16, weight: 'bold' },
        color: textColor,
      },
    },
  }

  if (type !== 'bar' && type !== 'horizontalBar') return baseOptions

  return {
    ...baseOptions,
    indexAxis: type === 'horizontalBar' ? 'y' : 'x',
    scales: {
      x: {
        ticks: { color: textColorSecondary, font: { weight: 500 } },
        grid: { color: surfaceBorder },
      },
      y: {
        ticks: { color: textColorSecondary },
        grid: { color: surfaceBorder },
      },
    },
  }
}

//TODO: Ajustar responsividade do componente todo

// Dados para o Gráfico de Centros
const centerChartData = computed(() => {
  if (!centerData.value.length) return undefined
  const documentStyle = getComputedStyle(document.body)

  return {
    labels: centerData.value.map((c) => c.centroSigla),
    datasets: [
      {
        label: 'Valor Gasto (R$)',
        data: centerData.value.map((c) => c.valorTotalGasto),
        backgroundColor: documentStyle.getPropertyValue('--p-primary-400'),
        borderColor: documentStyle.getPropertyValue('--p-primary-500'),
        borderWidth: 1,
      },
    ],
  }
})

// Dados para o Gráfico de Categorias
const categoryChartData = computed(() => {
  if (!categoryData.value.length) return undefined

  return {
    labels: categoryData.value.map((c) => c.categoriaNome),
    datasets: [
      {
        data: categoryData.value.map((c) => c.valorTotal),
        backgroundColor: ['#3b82f6', '#ef4444', '#10b981', '#f59e0b', '#8b5cf6', '#06b6d4'],
      },
    ],
  }
})

const hasData = computed(() => {
  if (activeReportType.value === 'GASTOS_CENTRO') {
    return centerData.value && centerData.value.length > 0
  }
  if (activeReportType.value === 'CONSUMO_CATEGORIA') {
    return categoryData.value && categoryData.value.length > 0
  }
  return false
})
</script>

<template>
  <div class="report-container">
    <Panel header="Gerador de Relatórios" toggleable>
      <div class="flex flex-column md:flex-row justify-content-between align-items-center gap-3">
        
        <div class="flex flex-column md:flex-row gap-2 w-full md:w-auto">
          
          <div class="w-full md:w-auto">
            <FloatLabel variant="on" class="w-full md:w-16rem">
              <DatePicker
                v-model="dateRange"
                selectionMode="range"
                dateFormat="dd/mm/yy"
                inputId="date-filter"
                showIcon
                class="w-full md:w-16rem"
                iconDisplay="input"
                size="small"
              />
              <label for="date-filter">Período</label>
            </FloatLabel>
          </div>

          <div class="w-full md:w-auto">
            <FloatLabel variant="on" class="w-full md:w-16rem">
              <Select
                v-model="selectedType"
                :options="reportOptions"
                optionLabel="label"
                optionValue="value"
                inputId="type-filter"
                class="w-full md:w-16rem"
                size="small"
              />
              <label for="type-filter">Tipo de Relatório</label>
            </FloatLabel>
          </div>
          
        </div>

        <div class="w-full md:w-auto flex justify-content-end">
          <Button
            label="Gerar Relatório"
            icon="pi pi-search"
            :loading="isLoading"
            @click="handleGenerate"
            size="small"
            class="w-full md:w-auto"
          />
        </div>

      </div>
    </Panel>

    <div class="mt-4">
      <div v-if="isLoading" class="flex justify-content-center p-6">
        <ProgressSpinner style="width: 50px; height: 50px" strokeWidth="4" />
      </div>

      <div v-else-if="error">
        <Message severity="error">{{ error }}</Message>
      </div>

      <div v-else-if="activeReportType" class="fadein animation-duration-500">
        <div v-if="hasData">
          <div v-if="activeReportType === 'GASTOS_CENTRO'" class="grid">
            <div class="col-12">
              <Card class="h-full">
                <template #content>
                  <Chart
                    type="bar"
                    :data="centerChartData"
                    :options="setChartOptions('Gastos por Centro (Top View)', 'bar')"
                    class="h-30rem"
                  />
                </template>
              </Card>
            </div>

            <div class="col-12">
              <Card>
                <template #title>Detalhamento por Centro</template>
                <template #content>
                  <DataTable :value="centerData" stripedRows paginator :rows="10" size="small">
                    <Column field="centroSigla" header="Sigla" sortable></Column>
                    <Column field="centroNome" header="Nome" sortable></Column>
                    <Column
                      field="quantidadeSolicitacoes"
                      header="Solicitações"
                      sortable
                      class="text-center"
                    ></Column>
                    <Column field="departamentoMaiorGasto" header="Maior Consumidor"></Column>
                    <Column
                      field="valorTotalGasto"
                      header="Total Gasto"
                      sortable
                      class="text-right"
                    >
                      <template #body="{ data }">
                        {{ formatCurrency(data.valorTotalGasto) }}
                      </template>
                    </Column>
                  </DataTable>
                </template>
              </Card>
            </div>
          </div>

          <div v-if="activeReportType === 'CONSUMO_CATEGORIA'" class="grid">
            <div class="col-12 lg:col-5">
              <Card class="h-full flex align-items-center justify-content-center">
                <template #content>
                  <Chart
                    type="doughnut"
                    :data="categoryChartData"
                    :options="setChartOptions('Distribuição por Categoria', 'doughnut')"
                    class="w-full md:w-25rem"
                  />
                </template>
              </Card>
            </div>

            <div class="col-12 lg:col-7">
              <Card class="h-full">
                <template #title>Detalhamento por Categoria</template>
                <template #content>
                  <DataTable :value="categoryData" stripedRows paginator :rows="10" size="small">
                    <Column field="categoriaNome" header="Categoria" sortable></Column>
                    <Column
                      field="quantidadeItensVendidos"
                      header="Qtd. Itens"
                      sortable
                      class="text-center"
                    ></Column>
                    <Column field="percentualDoTotal" header="%" sortable>
                      <template #body="{ data }">
                        <div class="flex align-items-center gap-2">
                          <span class="font-bold text-sm">{{ data.percentualDoTotal }}%</span>
                          <div class="surface-200 border-round w-4rem h-4px overflow-hidden">
                            <div
                              class="bg-primary h-full"
                              :style="{ width: data.percentualDoTotal + '%' }"
                            ></div>
                          </div>
                        </div>
                      </template>
                    </Column>
                    <Column field="valorTotal" header="Valor Total" sortable class="text-right">
                      <template #body="{ data }">
                        {{ formatCurrency(data.valorTotal) }}
                      </template>
                    </Column>
                  </DataTable>
                </template>
              </Card>
            </div>
          </div>
        </div>
        <div
          v-else
          class="flex flex-column align-items-center justify-content-center p-6 surface-card border-round shadow-1 mt-4"
        >
          <div class="border-circle bg-orange-100 p-4 mb-3">
            <i class="pi pi-exclamation-triangle text-4xl text-orange-500"></i>
          </div>
          <span class="text-xl font-medium text-900 mb-2">Sem resultados</span>
          <p class="text-600 text-center line-height-3 m-0">
            Não encontramos dados para o filtro selecionado.<br />
            Tente ajustar as datas ou o tipo de relatório.
          </p>
        </div>
      </div>

      <div v-else class="text-center p-8 surface-card border-round shadow-1 mt-4">
        <i class="pi pi-chart-bar text-6xl text-primary mb-4"></i>
        <div class="text-xl font-medium text-900 mb-2">Selecione os parâmetros</div>
        <p class="text-600 mb-0">
          Escolha o período e o tipo de relatório acima para visualizar os dados.
        </p>
      </div>
    </div>
  </div>
</template>

<style scoped>
/* Estilos auxiliares para garantir a responsividade dos gráficos igual ao seu dash */
.p-card {
  height: 100%;
}
</style>
