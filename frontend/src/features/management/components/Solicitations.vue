<script setup lang="ts">
import IconField from 'primevue/iconfield'
import InputIcon from 'primevue/inputicon'
import InputText from 'primevue/inputtext'
import { Button } from 'primevue'
import { ref } from 'vue'
import Column from 'primevue/column'
import DataTable from 'primevue/datatable'

const expandedRows = ref([])

const materials = ref([
  {
    id: 1,
    userRequest: 'João Silva (DCOMP)',
    date: 1752038400000,
    itemsQuantity: 5,
    totalPrice: 1200.5,
  },
  {
    id: 2,
    userRequest: 'Maria Souza (DMA)',
    date: 1752124800000,
    itemsQuantity: 3,
    totalPrice: 850.0,
  },
  {
    id: 3,
    userRequest: 'Carlos Pereira (DEC)',
    date: 1752211200000,
    itemsQuantity: 10,
    totalPrice: 4520.75,
  },
  {
    id: 4,
    userRequest: 'Ana Oliveira (DMA)',
    date: 1752211200000,
    itemsQuantity: 2,
    totalPrice: 300.0,
  },
  {
    id: 5,
    userRequest: 'Pedro Santos (DEC)',
    date: 1752211200000,
    itemsQuantity: 7,
    totalPrice: 1780.2,
  },
  {
    id: 6,
    userRequest: 'Fernanda Lima (DCOMP)',
    date: 1752211200000,
    itemsQuantity: 1,
    totalPrice: 99.9,
  },
  {
    id: 7,
    userRequest: 'Rafael Costa (DQI)',
    date: 1752211200000,
    itemsQuantity: 4,
    totalPrice: 560.0,
  },
  {
    id: 8,
    userRequest: 'Juliana Alves (DCOMP)',
    date: 1752211200000,
    itemsQuantity: 6,
    totalPrice: 2100.0,
  },
  {
    id: 9,
    userRequest: 'Lucas Barbosa (DMA)',
    date: 1752211200000,
    itemsQuantity: 8,
    totalPrice: 3200.45,
  },
  {
    id: 10,
    userRequest: 'Patrícia Dias (DEC)',
    date: 1752211200000,
    itemsQuantity: 9,
    totalPrice: 3899.99,
  },
])

const dt = ref()
const exportCSV = () => {
  dt.value.exportCSV()
}
</script>

<template>
  <div class="flex flex-column w-full h-full">
    <div
      class="flex flex-wrap align-items-center justify-content-between gap-2 md:gap-4 mt-2"
    >
      <div class="flex flex-wrap align-items-center gap-2">
        <div class="flex flex-column sm:flex-row gap-2">
          <IconField iconPosition="left">
            <InputIcon class="pi pi-search"></InputIcon>
            <InputText size="small" placeholder="Nome/Descrição/CATMAT" />
          </IconField>
        </div>

        <div class="flex align-items-center gap-2">
          <Button type="button" label="Filtrar" icon="pi pi-filter" size="small" />
        </div>
      </div>
      <div class="flex align-items-center">
        <Button
          type="button"
          label="Exportar"
          icon="pi pi-download"
          size="small"
          @click="exportCSV"
        />
      </div>
    </div>

    <div class="table-container mt-4 gap-2">
      <DataTable
        class="table"
        :value="materials"
        dataKey="id"
        tableStyle="min-width: 30rem"
        size="small"
        paginator
        :rows="50"
        :rowsPerPageOptions="[5, 10, 20, 50]"
        scrollable
        ref="dt"
        scrollHeight="550px"
      >
        <Column field="id" header="ID"></Column>
        <Column field="date" header="DATA">
          <template #body="slotProps">
            {{
              new Date(slotProps.data.date).toLocaleString('pt-BR', {
                day: '2-digit',
                month: '2-digit',
                year: 'numeric',
                hour: '2-digit',
                minute: '2-digit',
              })
            }}
          </template>
        </Column>
        <Column field="userRequest" header="REQUISITANTE"></Column>
        <Column field="itemsQuantity" header="QUANTIDADE DE ITENS"> </Column>
        <Column field="totalPrice" header="VALOR TOTAL">
          <template #body="slotProps">
            {{
              new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(
                slotProps.data.totalPrice,
              )
            }}
          </template>
        </Column>
        <Column header="Ações">
          <template #body="slotProps">
            <router-link :to="{ name: 'SolicitationDetails', params: { id: slotProps.data.id } }">
              <Button
                icon="pi pi-eye"
                severity="info"
                text
                rounded
                aria-label="Ver Detalhes"
                v-tooltip.top="'Ver Detalhes'"
              />
            </router-link>
          </template>
        </Column>
      </DataTable>
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
</style>
