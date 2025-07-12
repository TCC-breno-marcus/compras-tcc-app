<script setup lang="ts">
import Panel from 'primevue/panel'
import Select from 'primevue/select'
import Button from 'primevue/button'
import DataTable from 'primevue/datatable'
import Chart from 'primevue/chart'
import ProgressSpinner from 'primevue/progressspinner'
import { ref } from 'vue'
import IconField from 'primevue/iconfield'
import InputIcon from 'primevue/inputicon'
import SplitButton from 'primevue/splitbutton'
import InputText from 'primevue/inputtext'
import DatePicker from 'primevue/datepicker'
import FloatLabel from 'primevue/floatlabel'

interface ReportData {
  chart: any
  table: any[]
}

const isLoading = ref(false)
const reportData = ref<ReportData | null>(null)

const categories = ref([
  { name: 'Todas', code: 'todas' },
  { name: 'Vidraria', code: 'vidraria' },
  { name: 'Componentes Eletrônicos', code: 'componentes-eletronicos' },
])

const selectedCategory = ref()
const date = ref()

const generateReport = () => {
  isLoading.value = true
  setTimeout(() => {
    reportData.value = {
      chart: {
        /* dados do gráfico */
      },
      table: [
        /* dados da tabela */
      ],
    }
    isLoading.value = false
  }, 3000)
}
</script>

<template>
  <div class="p-4">
    <Panel header="Filtros do Relatório" toggleable>
      <div class="flex flex-column lg:flex-row lg:align-items-center justify-content-between gap-4">
        <div class="flex flex-column md:flex-row md:flex-wrap align-items-center gap-2">
          <div class="flex flex-column sm:flex-row gap-2">
            <IconField iconPosition="left">
              <InputIcon class="pi pi-search"></InputIcon>
              <InputText size="small" placeholder="Nome/Descrição/CATMAT" />
            </IconField>

            <Select
              v-model="selectedCategory"
              :options="categories"
              optionLabel="name"
              placeholder="Categoria"
              class="w-full md:w-56"
              size="small"
            />
          </div>

          <div class="flex align-items-center gap-2">
            <FloatLabel variant="on">
              <DatePicker v-model="date" inputId="on_label" showIcon iconDisplay="input" />
              <label for="on_label">Período</label>
            </FloatLabel>
          </div>
        </div>

        <CatalogUpload />
      </div>
      <div class="flex justify-content-end gap-2 mt-4">
        <Button
          label="Limpar Filtros"
          icon="pi pi-times"
          severity="secondary"
          outlined
          size="small"
        />
        <Button
          label="Gerar Relatório"
          icon="pi pi-search"
          size="small"
          @click="generateReport"
        />
      </div>
    </Panel>

    <div class="mt-4">
      <div v-if="isLoading" class="flex justify-content-center p-8">
        <ProgressSpinner />
      </div>

      <div v-else-if="reportData" class="grid">
        <div class="col-12">
          <Chart type="bar" :data="reportData.chart" />
        </div>
        <div class="col-12">
          <DataTable :value="reportData.table"> </DataTable>
        </div>
      </div>

      <div v-else class="text-center p-8 bg-surface-50 border-round">
        <i class="pi pi-chart-bar text-4xl text-surface-500"></i>
        <p class="mt-3 text-surface-500">
          Selecione os filtros acima e clique em "Gerar Relatório" para visualizar os dados.
        </p>
      </div>
    </div>
  </div>
</template>
