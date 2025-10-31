<script setup lang="ts">
import Card from 'primevue/card'
import { computed } from 'vue'
import Chart from 'primevue/chart'
import { useSolicitationStore } from '../stores/solicitationStore'
import { storeToRefs } from 'pinia'
import type { CardKPI, TopItemData } from '@/features/dashboards/types'
import { formatQuantity } from '@/utils/number'
import { formatCurrency } from '@/utils/currency'

const solicitationStore = useSolicitationStore()
const { currentSolicitation } = storeToRefs(solicitationStore)

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

const setChartData = (data: { labels: string[]; data: number[] } | undefined) => {
  if (!data) return undefined
  const documentStyle = getComputedStyle(document.body)
  return {
    labels: data.labels,
    datasets: [
      {
        data: data.data,
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

const setChartOptions = (title: string, type: string) => {
  const documentStyle = getComputedStyle(document.documentElement)
  const textColor = documentStyle.getPropertyValue('--p-text-color')
  const textColorSecondary = documentStyle.getPropertyValue('--p-text-muted-color')
  const surfaceBorder = documentStyle.getPropertyValue('--p-content-border-color')

  const baseOptions = {
    maintainAspectRatio: false,
    aspectRatio: 1.5,
    plugins: {
      title: {
        display: true,
        text: title,
        font: {
          size: 16,
          weight: 'bold',
        },
        color: textColor,
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

const setBarChartData = (data: TopItemData[] | undefined, label: string) => {
  if (!data) return undefined
  const documentStyle = getComputedStyle(document.body)

  return {
    labels: data.map((item) => `${item.nome} (${item.catMat})`),
    datasets: [
      {
        label: label,
        data: data.map((item) => item.valor),
        backgroundColor: documentStyle.getPropertyValue('--p-primary-200'),
        borderColor: documentStyle.getPropertyValue('--p-primary-500'),
        borderWidth: 1,
      },
    ],
  }
}

const categoryChartData = computed(() => {
  return setChartData(currentSolicitation.value?.valorPorCategoria)
})

const topItensPorValorData = computed(() => {
  return setBarChartData(currentSolicitation.value?.topItensPorValor, 'Valor (R$)')
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

    <div class="grid">
      <div class="col-12 lg:col-6">
        <Card class="h-full">
          <template #content>
            <Chart
              type="doughnut"
              :data="categoryChartData"
              :options="setChartOptions('Valor por Departamento (R$)', 'doughnut')"
              style="height: 300px"
              class="graph"
            />
          </template>
        </Card>
      </div>

      <div class="col-12 lg:col-6">
        <Card class="h-full">
          <template #content>
            <Chart
              type="bar"
              :data="topItensPorValorData"
              :options="setChartOptions('Itens de Maior Custo Total', 'bar')"
              style="height: 300px"
              class="graph"
            />
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
</style>
