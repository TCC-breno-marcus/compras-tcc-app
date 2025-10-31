<script setup lang="ts">
import Card from 'primevue/card'
import { onMounted, computed } from 'vue'
import Chart from 'primevue/chart'
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

const statusChartData = computed(() => {
  return setChartData(visaoGeralStatus.value)
})

const deptoChartData = computed(() => {
  return setChartData(valorPorDepartamento.value)
})

const topItemsQtyData = computed(() => {
  return setBarChartData(topItensPorQuantidade.value, 'Quantidade')
})

const topItemsValorData = computed(() => {
  return setBarChartData(topItensPorValorTotal.value, 'Valor (R$)')
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
            <div v-if="isLoading" class="flex">
              <ProgressSpinner style="width: 50px; height: 50px" strokeWidth="4" />
            </div>
            <Chart
              v-else
              type="bar"
              :data="topItemsValorData"
              :options="setChartOptions('Itens De Maior Valor', 'bar')"
              class="graph"
            />
          </template>
        </Card>
      </div>

      <div class="card w-full lg:w-8 xl:w-4">
        <Card class="h-full">
          <template #content>
            <div v-if="isLoading" class="flex">
              <ProgressSpinner style="width: 50px; height: 50px" strokeWidth="4" />
            </div>
            <Chart
              v-else
              type="bar"
              :data="topItemsQtyData"
              :options="setChartOptions('Itens Mais Solicitados', 'bar')"
              class="graph"
            />
          </template>
        </Card>
      </div>

      <div class="card w-full lg:w-8 xl:w-4">
        <Card class="flex justify-content-center align-items-center w-full h-full">
          <template #content>
            <ProgressSpinner v-if="isLoading" style="width: 50px; height: 50px" strokeWidth="4" />
            <Chart
              v-else
              type="doughnut"
              :data="deptoChartData"
              :options="setChartOptions('Valor por Departamento (R$)', 'doughnut')"
              class="graph"
            />
          </template>
        </Card>
      </div>

      <div class="card w-full lg:w-8 xl:w-4">
        <Card class="flex justify-content-center align-items-center w-full h-full">
          <template #content>
            <ProgressSpinner v-if="isLoading" style="width: 50px; height: 50px" strokeWidth="4" />
            <Chart
              v-else
              type="pie"
              :data="statusChartData"
              :options="setChartOptions('Status das Solicitações', 'pie')"
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
</style>
