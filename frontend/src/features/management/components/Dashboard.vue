<script setup lang="ts">
import Card from 'primevue/card'
import { onMounted, computed, ref } from 'vue'
import Chart from 'primevue/chart'
import ToggleSwitch from 'primevue/toggleswitch'
import { useDashboardStore } from '@/features/dashboards/stores/dashStore'
import { storeToRefs } from 'pinia'
import { formatQuantity } from '@/utils/number'
import { formatCurrency } from '@/utils/currency'
import type { CardKPI, TopItemData } from '@/features/dashboards/types'
import { ProgressSpinner } from 'primevue'

const dashStore = useDashboardStore()
const {
  isLoading,
  kpis,
  valorPorDepartamento,
  valorPorCategoria,
  visaoGeralStatus,
  topItensPorQuantidade,
  topItensPorValorTotal,
} = storeToRefs(dashStore)

// toggles por gráfico (false = valor bruto, true = porcentagem)
const showTopValorPercent = ref(false)
const showTopQtyPercent = ref(false)
const showDeptoPercent = ref(false)
const showStatusPercent = ref(false)

const kpiCards = computed<CardKPI[]>(() => {
  if (!kpis.value)
    return Array.from({ length: 4 }, () => ({
      title: '',
      value: '',
      icon: '',
      color: '',
    }))
  return [
    {
      title: 'Total de Solicitações',
      value: formatQuantity(kpis.value.totalSolicitacoes),
      icon: 'pi pi-file-edit',
      color: 'text-blue-500',
    },
    {
      title: 'Departamentos Solicitantes',
      value: formatQuantity(kpis.value.totalDepartamentosSolicitantes),
      icon: 'pi pi-building',
      color: 'text-indigo-500',
    },
    {
      title: 'Total de Itens Únicos',
      value: formatQuantity(kpis.value.totalItensUnicos),
      icon: 'pi pi-box',
      color: 'text-purple-500',
    },
    {
      title: 'Total de Unidades Solicitadas',
      value: formatQuantity(kpis.value.totalUnidadesSolicitadas),
      icon: 'pi pi-shopping-cart',
      color: 'text-cyan-500',
    },
    {
      title: 'Valor Total Estimado',
      value: formatCurrency(kpis.value.valorTotalEstimado),
      icon: 'pi pi-dollar',
      color: 'text-green-500',
    },
    {
      title: 'Custo Médio por Solicitação',
      value: formatCurrency(kpis.value.custoMedioSolicitacao),
      icon: 'pi pi-chart-line',
      color: 'text-orange-500',
    },
  ]
})

const setChartData = (data: { labels: string[]; data: number[] } | undefined, percent = false) => {
  if (!data) return undefined
  const documentStyle = getComputedStyle(document.body)
  const values = data.data || []
  if (percent) {
    const total = values.reduce((s, v) => s + (v || 0), 0)
    const mapped =
      total > 0
        ? values.map((v) => Math.round(((v || 0) / total) * 10000) / 100)
        : values.map(() => 0)
    return {
      labels: data.labels,
      datasets: [
        {
          data: mapped,
          backgroundColor: [
            documentStyle.getPropertyValue('--p-primary-400'),
            documentStyle.getPropertyValue('--p-orange-400'),
            documentStyle.getPropertyValue('--p-cyan-400'),
            documentStyle.getPropertyValue('--p-green-400'),
            documentStyle.getPropertyValue('--p-purple-400'),
          ],
        },
      ],
    }
  }

  return {
    labels: data.labels,
    datasets: [
      {
        data: values,
        backgroundColor: [
          documentStyle.getPropertyValue('--p-primary-400'),
          documentStyle.getPropertyValue('--p-orange-400'),
          documentStyle.getPropertyValue('--p-cyan-400'),
          documentStyle.getPropertyValue('--p-green-400'),
          documentStyle.getPropertyValue('--p-purple-400'),
        ],
      },
    ],
  }
}

const setBarChartData = (data: TopItemData[] | undefined, label: string, percent = false) => {
  if (!data) return undefined
  const documentStyle = getComputedStyle(document.body)
  const values = data.map((item) => item.valor || 0)
  if (percent) {
    const total = values.reduce((s, v) => s + v, 0)
    const mapped =
      total > 0 ? values.map((v) => Math.round((v / total) * 10000) / 100) : values.map(() => 0)
    return {
      labels: data.map((item) => `${item.nome} (${item.catMat})`),
      datasets: [
        {
          label: `${label} (%)`,
          data: mapped,
          backgroundColor: documentStyle.getPropertyValue('--p-primary-200'),
          borderColor: documentStyle.getPropertyValue('--p-primary-500'),
          borderWidth: 1,
        },
      ],
    }
  }

  return {
    labels: data.map((item) => `${item.nome} (${item.catMat})`),
    datasets: [
      {
        label: label,
        data: values,
        backgroundColor: documentStyle.getPropertyValue('--p-primary-200'),
        borderColor: documentStyle.getPropertyValue('--p-primary-500'),
        borderWidth: 1,
      },
    ],
  }
}

const statusChartData = computed(() => {
  return setChartData(visaoGeralStatus.value, showStatusPercent.value)
})

const deptoChartData = computed(() => {
  return setChartData(valorPorDepartamento.value, showDeptoPercent.value)
})

const topItemsQtyData = computed(() => {
  return setBarChartData(
    topItensPorQuantidade.value,
    showTopQtyPercent.value ? 'Quantidade (%)' : 'Quantidade',
    showTopQtyPercent.value,
  )
})

const topItemsValorData = computed(() => {
  return setBarChartData(
    topItensPorValorTotal.value,
    showTopValorPercent.value ? 'Valor (%)' : 'Valor (R$)',
    showTopValorPercent.value,
  )
})

const hasChartValues = (chart: { data: number[] } | undefined) => {
  return !!chart && Array.isArray(chart.data) && chart.data.length > 0
}

const hasTopItemsValores = computed(() => {
  return Array.isArray(topItensPorValorTotal.value) && topItensPorValorTotal.value.length > 0
})

const hasTopItemsQuantidade = computed(() => {
  return Array.isArray(topItensPorQuantidade.value) && topItensPorQuantidade.value.length > 0
})

const hasDeptoData = computed(() => hasChartValues(valorPorDepartamento.value))
const hasStatusData = computed(() => hasChartValues(visaoGeralStatus.value))

const setChartOptions = (
  title: string,
  type: string,
  valueType: 'currency' | 'percent' | 'quantity' = 'currency',
) => {
  const documentStyle = getComputedStyle(document.documentElement)
  const textColor = documentStyle.getPropertyValue('--p-text-color')
  const textColorSecondary = documentStyle.getPropertyValue('--p-text-muted-color')
  const surfaceBorder = documentStyle.getPropertyValue('--p-content-border-color')

  const formatTooltipValue = (value: number) => {
    if (valueType === 'currency') return formatCurrency(value)
    if (valueType === 'percent') return `${value}%`
    return formatQuantity(value)
  }

  const baseOptions = {
    maintainAspectRatio: false,
    aspectRatio: 1.5,
    plugins: {
      // Title will be rendered via HTML header above the canvas to allow placing toggle next to it
      title: {
        display: false,
        text: title,
        font: {
          size: 16,
          weight: 'bold',
        },
        color: textColor,
      },
      tooltip: {
        callbacks: {
          label: function (context: any) {
            const v = context.raw
            return formatTooltipValue(Number(v))
          },
        },
      },
      legend: {
        labels: {
          color: textColorSecondary,
        },
      },
    },
  }

  if (type !== 'bar') {
    return baseOptions
  }

  return {
    ...baseOptions,
    indexAxis: 'y',
    scales: {
      x: {
        ticks: {
          color: textColorSecondary,
          font: {
            weight: 500,
          },
        },
        grid: {
          color: surfaceBorder,
        },
      },
      y: {
        ticks: {
          color: textColorSecondary,
        },
        grid: {
          color: surfaceBorder,
        },
      },
    },
  }
}

onMounted(() => {
  dashStore.fetchGestorDashboard()
})
</script>

<template>
  <div class="dashboard-container grid">
    <div v-for="card in kpiCards" :key="card.title" class="col-12 md:col-6 lg:col-3">
      <Card class="h-full">
        <template #title>
          <div class="flex align-items-center justify-content-between">
            <span class="text-sm font-medium text-surface-500">{{ card.title }}</span>
            <i :class="[card.icon, card.color]"></i>
          </div>
        </template>
        <template #content>
          <p class="text-base xl:text-xl font-bold mt-2 mb-0">
            {{ card.value }}
          </p>
        </template>
      </Card>
    </div>

    <div class="graph-container m-2 gap-4">
      <div class="card w-full lg:w-8 xl:w-4">
        <Card class="h-full">
          <template #content>
            <div class="chart-wrapper">
              <div class="chart-header">
                <span class="chart-title">Itens De Maior Valor</span>
                <div class="flex align-items-center">
                  <ToggleSwitch
                    v-model="showTopValorPercent"
                    onLabel="%"
                    offLabel="R$"
                    aria-label="Alternar entre valor em reais e porcentagem"
                  />
                  <span class="ml-2 text-xs text-color-secondary">{{
                    showTopValorPercent ? 'Exibindo: %' : 'Exibindo: R$'
                  }}</span>
                </div>
              </div>

              <div v-if="isLoading" class="flex">
                <ProgressSpinner style="width: 50px; height: 50px" strokeWidth="4" />
              </div>
              <Chart
                v-else-if="topItemsValorData && hasTopItemsValores"
                type="bar"
                :data="topItemsValorData"
                :options="
                  setChartOptions(
                    'Itens De Maior Valor',
                    'bar',
                    showTopValorPercent ? 'percent' : 'currency',
                  )
                "
                class="graph"
              />
              <div v-else class="empty-chart-message">Sem dados para exibir.</div>
            </div>
          </template>
        </Card>
      </div>

      <div class="card w-full lg:w-8 xl:w-4">
        <Card class="h-full">
          <template #content>
            <div class="chart-wrapper">
              <div class="chart-header">
                <span class="chart-title">Itens Mais Solicitados</span>
                <div class="flex align-items-center">
                  <ToggleSwitch
                    v-model="showTopQtyPercent"
                    onLabel="%"
                    offLabel="Qtd"
                    aria-label="Alternar entre quantidade absoluta e porcentagem"
                  />
                  <span class="ml-2 text-xs text-color-secondary">{{
                    showTopQtyPercent ? 'Exibindo: %' : 'Exibindo: Qtde'
                  }}</span>
                </div>
              </div>

              <div v-if="isLoading" class="flex">
                <ProgressSpinner style="width: 50px; height: 50px" strokeWidth="4" />
              </div>
              <Chart
                v-else-if="topItemsQtyData && hasTopItemsQuantidade"
                type="bar"
                :data="topItemsQtyData"
                :options="
                  setChartOptions(
                    'Itens Mais Solicitados',
                    'bar',
                    showTopQtyPercent ? 'percent' : 'quantity',
                  )
                "
                class="graph"
              />
              <div v-else class="empty-chart-message">Sem dados para exibir.</div>
            </div>
          </template>
        </Card>
      </div>

      <div class="card w-full lg:w-8 xl:w-4">
        <Card class="flex justify-content-center align-items-center w-full h-full">
          <template #content>
            <div class="chart-wrapper">
              <div class="chart-header">
                <span class="chart-title">Valor por Departamento</span>
                <div class="flex align-items-center">
                  <ToggleSwitch
                    v-model="showDeptoPercent"
                    onLabel="%"
                    offLabel="R$"
                    aria-label="Alternar entre valor em reais e porcentagem"
                  />
                  <span class="ml-2 text-xs text-color-secondary">{{
                    showDeptoPercent ? 'Exibindo: %' : 'Exibindo: R$'
                  }}</span>
                </div>
              </div>

              <ProgressSpinner v-if="isLoading" style="width: 50px; height: 50px" strokeWidth="4" />
              <Chart
                v-else-if="deptoChartData && hasDeptoData"
                type="doughnut"
                :data="deptoChartData"
                :options="
                  setChartOptions(
                    'Valor por Departamento (R$)',
                    'doughnut',
                    showDeptoPercent ? 'percent' : 'currency',
                  )
                "
                class="graph"
              />
              <div v-else class="empty-chart-message">Sem dados para exibir.</div>
            </div>
          </template>
        </Card>
      </div>

      <div class="card w-full lg:w-8 xl:w-4">
        <Card class="flex justify-content-center align-items-center w-full h-full">
          <template #content>
            <div class="chart-wrapper">
              <div class="chart-header">
                <span class="chart-title">Status das Solicitações</span>
                <div class="flex align-items-center">
                  <ToggleSwitch
                    v-model="showStatusPercent"
                    onLabel="%"
                    offLabel="Qtd"
                    aria-label="Alternar entre quantidade absoluta e porcentagem"
                  />
                  <span class="ml-2 text-xs text-color-secondary">{{
                    showStatusPercent ? 'Exibindo: %' : 'Exibindo: Qtde'
                  }}</span>
                </div>
              </div>

              <ProgressSpinner v-if="isLoading" style="width: 50px; height: 50px" strokeWidth="4" />
              <Chart
                v-else-if="statusChartData && hasStatusData"
                type="pie"
                :data="statusChartData"
                :options="
                  setChartOptions(
                    'Status das Solicitações',
                    'pie',
                    showStatusPercent ? 'percent' : 'quantity',
                  )
                "
                class="graph"
              />
              <div v-else class="empty-chart-message">Sem dados para exibir.</div>
            </div>
          </template>
        </Card>
      </div>
    </div>
  </div>
</template>

<style scoped>
.dashboard-container {
  max-height: calc(100vh - 200px);
  overflow-y: auto;
  /* Para Firefox */
  scrollbar-width: thin;
  scrollbar-color: var(--p-surface-400) transparent;
}

.p-card {
  display: flex;
  flex-direction: column;
}

.p-dark .p-card {
  background-color: var(--p-surface-800);
}

.p-card-content {
  flex-grow: 1;
}

.graph-container {
  display: flex;
  width: 100%;
  flex-wrap: wrap;
  justify-content: center;
}

.graph {
  width: 100%;
  /* min-width: 400px; */
  height: 400px;
}

.card {
  min-width: 400px;
}

.toggle-container {
  margin-top: 2rem;
}

.toggle-container .card {
  margin-bottom: 1rem;
}

.chart-wrapper {
  position: relative;
  padding-top: 2.5rem; /* reserve space for header overlay */
}
.chart-header {
  position: absolute;
  top: 8px;
  left: 12px;
  right: 12px;
  display: flex;
  justify-content: space-between;
  align-items: center;
  z-index: 6;
  pointer-events: none; /* allow clicks to pass to toggle container unless we enable pointer-events on child */
}
.chart-header .flex {
  pointer-events: auto; /* enable interaction with toggle inside header */
}
.chart-title {
  font-weight: 600;
}

.empty-chart-message {
  min-height: 220px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: var(--p-text-muted-color);
  font-size: 0.95rem;
}
.chart-overlay {
  position: absolute;
  top: 8px;
  right: 12px;
  z-index: 5;
  display: flex;
  align-items: center;
}
</style>
