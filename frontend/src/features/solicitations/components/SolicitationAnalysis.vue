<script setup lang="ts">
import Card from 'primevue/card'
import { computed, ref } from 'vue'
import Chart from 'primevue/chart'
import ToggleSwitch from 'primevue/toggleswitch'
import { useSolicitationStore } from '../stores/solicitationStore'
import { storeToRefs } from 'pinia'
import type { CardKPI, TopItemData } from '@/features/dashboards/types'
import { formatQuantity } from '@/utils/number'
import { formatCurrency } from '@/utils/currency'

const solicitationStore = useSolicitationStore()
const { currentSolicitation } = storeToRefs(solicitationStore)

const showCategoryPercent = ref(false)
const showTopValorPercent = ref(false)

const kpiCards = computed<CardKPI[]>(() => {
  if (!currentSolicitation.value?.kpis)
    return Array.from({ length: 3 }, () => ({
      title: '',
      value: '',
      icon: '',
      color: '',
    }))
  return [
    {
      title: 'Total de Itens Únicos',
      value: formatQuantity(currentSolicitation.value.kpis.totalItensUnicos),
      icon: 'pi pi-box',
      color: 'text-purple-500',
    },
    {
      title: 'Total de Unidades Solicitadas',
      value: formatQuantity(currentSolicitation.value.kpis.totalUnidades),
      icon: 'pi pi-shopping-cart',
      color: 'text-cyan-500',
    },
    {
      title: 'Valor Total Estimado',
      value: formatCurrency(currentSolicitation.value.kpis.valorTotalEstimado),
      icon: 'pi pi-dollar',
      color: 'text-green-500',
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
      // render title via HTML above canvas to allow toggle next to it
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

const categoryChartData = computed(() => {
  return setChartData(currentSolicitation.value?.valorPorCategoria, showCategoryPercent.value)
})

const topItensPorValorData = computed(() => {
  return setBarChartData(
    currentSolicitation.value?.topItensPorValor,
    showTopValorPercent.value ? 'Valor (%)' : 'Valor (R$)',
    showTopValorPercent.value,
  )
})
</script>

<template>
  <div class="dashboard-container">
    <div class="grid justify-content-evenly">
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
    </div>

    <!-- single grid with headers+toggles above each chart (no duplicated charts) -->
    <div class="grid">
      <div class="col-12 lg:col-6">
        <Card class="h-full">
          <template #content>
            <div class="chart-wrapper">
              <div class="chart-header">
                <span class="chart-title">Valor por Categoria</span>
                <div class="flex align-items-center">
                  <ToggleSwitch
                    v-model="showCategoryPercent"
                    onLabel="%"
                    offLabel="R$"
                    aria-label="Alternar entre valor em reais e porcentagem"
                  />
                  <span class="ml-2 text-xs text-color-secondary">{{
                    showCategoryPercent ? 'Exibindo: %' : 'Exibindo: R$'
                  }}</span>
                </div>
              </div>

              <Chart
                type="doughnut"
                :data="categoryChartData"
                :options="
                  setChartOptions(
                    'Valor por Categoria (R$)',
                    'doughnut',
                    showCategoryPercent ? 'percent' : 'currency',
                  )
                "
                style="height: 300px"
                class="graph"
              />
            </div>
          </template>
        </Card>
      </div>

      <div class="col-12 lg:col-6">
        <Card class="h-full">
          <template #content>
            <div class="chart-wrapper">
              <div class="chart-header">
                <span class="chart-title">Itens de Maior Custo Total</span>
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

              <Chart
                type="bar"
                :data="topItensPorValorData"
                :options="
                  setChartOptions(
                    'Itens de Maior Custo Total',
                    'bar',
                    showTopValorPercent ? 'percent' : 'currency',
                  )
                "
                style="height: 300px"
                class="graph"
              />
            </div>
          </template>
        </Card>
      </div>
    </div>
  </div>
</template>

<style scoped>
.dashboard-container {
  max-height: calc(100vh - 420px);
  overflow-y: auto;
  overflow-x: hidden;
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
  height: 300px;
}

.card {
  min-width: 400px;
}

.chart-wrapper {
  display: flex;
  flex-direction: column;
  height: 100%;
}

.chart-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1rem;
  border-bottom: 1px solid var(--p-content-border-color);
}

.chart-title {
  font-size: 1.125rem;
  font-weight: 600;
  color: var(--p-text-color);
}
</style>
