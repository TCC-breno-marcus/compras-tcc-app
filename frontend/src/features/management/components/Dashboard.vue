<script setup lang="ts">
import Card from 'primevue/card';
import { ref, onMounted } from "vue";
import Chart from 'primevue/chart';

onMounted(() => {
  chartData.value = setChartData();
  chartOptions.value = setChartOptions();
});

const chartData = ref();
const chartOptions = ref();

const setChartData = () => {
  // 1. Simule seus dados brutos em um formato mais fácil de manipular
  const topItemsData = [
    { name: 'Gaze Estéril', quantity: 984 },
    { name: 'Seringa Descartável 5ml', quantity: 872 },
    { name: 'Luva Cirúrgica (Par)', quantity: 753 },
    { name: 'Álcool Etílico 70% 1L', quantity: 621 },
    { name: 'Máscara N95', quantity: 559 },
    { name: 'Termômetro Digital', quantity: 488 },
    { name: 'Fio de Sutura 3-0', quantity: 345 },
    { name: 'Cateter Intravenoso 22G', quantity: 210 },
    { name: 'Atadura de Crepom', quantity: 156 },
    { name: 'Bolsa de Colostomia', quantity: 97 }
  ];

  // 2. Ordene os dados (embora já estejam, é uma boa prática garantir)
  topItemsData.sort((a, b) => b.quantity - a.quantity);

  // 3. Mapeie os dados para o formato que o Chart.js espera
  const labels = topItemsData.map(item => item.name);
  const data = topItemsData.map(item => item.quantity);

  const documentStyle = getComputedStyle(document.body);

  return {
    labels: labels,
    datasets: [
      {
        label: 'Quantidade Solicitada',
        data: data,
        // Usar uma única cor destaca que é um ranking (comparação de valores)
        backgroundColor: documentStyle.getPropertyValue('--p-primary-200'),
        borderColor: documentStyle.getPropertyValue('--p-primary-500'),
        borderWidth: 1
      }
    ]
  };
};

const setChartOptions = () => {
  const documentStyle = getComputedStyle(document.documentElement);
  const textColor = documentStyle.getPropertyValue('--p-text-color');
  const textColorSecondary = documentStyle.getPropertyValue('--p-text-muted-color');
  const surfaceBorder = documentStyle.getPropertyValue('--p-content-border-color');

  return {
    // 1. Adiciona a opção para tornar o gráfico horizontal
    indexAxis: 'y',
    maintainAspectRatio: false,
    aspectRatio: 1.5,
    plugins: {
      // 2. Adiciona um título claro ao gráfico
      title: {
        display: true,
        text: 'Top 10 Itens Mais Solicitados',
        font: {
          size: 16,
          weight: 'bold'
        },
        color: textColor
      },
      // 3. Remove a legenda, pois com uma só barra ela é redundante
      legend: {
        display: false
      }
    },
    scales: {
      x: {
        ticks: {
          color: textColorSecondary,
          font: {
            weight: 500
          }
        },
        grid: {
          color: surfaceBorder
        }
      },
      y: {
        ticks: {
          color: textColorSecondary
        },
        grid: {
          color: surfaceBorder
        }
      }
    }
  };
};

const kpiCards = ref([
  {
    title: 'Total de Itens Únicos',
    value: '342',
    icon: 'pi pi-box',
    color: 'text-primary'
  },
  {
    title: 'Total de Unidades',
    value: '15.890',
    icon: 'pi pi-server',
    color: 'text-cyan-500'
  },
  {
    title: 'Valor Total Estimado',
    value: 'R$ 2.450.123,50',
    icon: 'pi pi-dollar',
    color: 'text-green-500'
  },
  {
    title: 'Departamentos Solicitantes',
    value: '28 / 32',
    icon: 'pi pi-building',
    color: 'text-orange-500'
  }
]);

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
            <Chart type="bar" :data="chartData" :options="chartOptions" class="graph" />
          </template>
        </Card>
      </div>

      <div class="card w-full lg:w-8 xl:w-4">
        <Card class="h-full">
          <template #content>
            <Chart type="bar" :data="chartData" :options="chartOptions" class="graph" />
          </template>
        </Card>
      </div>

      <div class="card w-full lg:w-8 xl:w-4">
        <Card class="h-full">
          <template #content>
            <Chart type="bar" :data="chartData" :options="chartOptions" class="graph" />
          </template>
        </Card>
      </div>

      <div class="card w-full lg:w-8 xl:w-4">
        <Card class="h-full">
          <template #content>
            <Chart type="bar" :data="chartData" :options="chartOptions" class="graph" />
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