<script setup lang="ts">
import Card from 'primevue/card'
import { ref, onMounted } from 'vue'
import Chart from 'primevue/chart'

const topItemsByQuantityData = ref()
const topItemsByQuantityOptions = ref()

const topItemsByPriceData = ref()
const topItemsByPriceOptions = ref()

onMounted(() => {
  topItemsByQuantityData.value = setTopItemsData('quantity')
  topItemsByQuantityOptions.value = setChartOptions('Top 5 Itens por Quantidade', 'y')

  topItemsByPriceData.value = setTopItemsData('price')
  topItemsByPriceOptions.value = setChartOptions('Top 5 Itens por Custo (Valor Total)', 'x')
})

const setTopItemsData = (dataType: 'quantity' | 'price') => {
  const labelsData = {
    quantity: [
      'Gaze Estéril',
      'Seringa Descartável 5ml',
      'Luva Cirúrgica (Par)',
      'Álcool Etílico 70% 1L',
      'Máscara N95',
    ],
    price: [
      'Termômetro Digital',
      'Fio de Sutura 3-0',
      'Cateter Intravenoso 22G',
      'Atadura de Crepom',
      'Bolsa de Colostomia',
    ],
  }
  const valuesData = {
    quantity: [984, 872, 753, 621, 559],
    price: [4888.78, 2341.05, 947.9, 545.51, 121.55],
  }
  const documentStyle = getComputedStyle(document.body)

  return {
    labels: labelsData[dataType],
    datasets: [
      {
        label: dataType === 'quantity' ? 'Quantidade Solicitada' : 'Valor Total (R$)',
        data: valuesData[dataType],
        backgroundColor: documentStyle.getPropertyValue('--p-primary-200'),
        borderColor: documentStyle.getPropertyValue('--p-primary-500'),
        borderWidth: 1,
      },
    ],
  }
}

const setChartOptions = (titleText: string, axis: 'x' | 'y') => {
  const documentStyle = getComputedStyle(document.documentElement)
  const textColor = documentStyle.getPropertyValue('--p-text-color')
  const textColorSecondary = documentStyle.getPropertyValue('--p-text-muted-color')
  const surfaceBorder = documentStyle.getPropertyValue('--p-content-border-color')

  return {
    indexAxis: axis, // 'y' para horizontal, 'x' para vertical
    maintainAspectRatio: false,
    aspectRatio: 1.5,
    plugins: {
      title: {
        display: true,
        text: titleText,
        font: { size: 16, weight: 'bold' },
        color: textColor,
      },
      legend: {
        display: false,
      },
    },
    scales: {
      x: { ticks: { color: textColorSecondary }, grid: { color: surfaceBorder } },
      y: { ticks: { color: textColorSecondary }, grid: { color: surfaceBorder } },
    },
  }
}

const kpiCards = ref([
  {
    title: 'Total de Itens Únicos',
    value: '342',
    icon: 'pi pi-box',
    color: 'text-primary',
  },
  {
    title: 'Total de Unidades',
    value: '15.890',
    icon: 'pi pi-server',
    color: 'text-cyan-500',
  },
  {
    title: 'Valor Total Estimado',
    value: 'R$ 2.450.123,50',
    icon: 'pi pi-dollar',
    color: 'text-green-500',
  },
])
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
              type="bar"
              :data="topItemsByQuantityData"
              :options="topItemsByQuantityOptions"
              style="height: 300px"
            />
          </template>
        </Card>
      </div>

      <div class="col-12 lg:col-6">
        <Card class="h-full">
          <template #content>
            <Chart
              type="bar"
              :data="topItemsByPriceData"
              :options="topItemsByPriceOptions"
              style="height: 300px"
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
