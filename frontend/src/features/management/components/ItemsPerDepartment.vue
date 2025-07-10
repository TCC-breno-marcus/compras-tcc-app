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
    catmat: '408559',
    description: 'Bandeja retangular 30 cm X 20 cm X 5 cm',
    unit: 'UNIDADE',
    unitPrice: 40.0,
    quantity: 16,
    totalPrice: 64.0,
    image: '/items_img/img1.png',
    departments: [
      { name: 'DQI', quantity: 10 },
      { name: 'DEC', quantity: 6 },
    ],
  },
  {
    id: 2,
    catmat: '604032',
    description: 'Cadinho (Porcelana 100ml)',
    unit: 'UNIDADE',
    unitPrice: 39.0,
    quantity: 40,
    totalPrice: 1560.0,
    image: '/items_img/img1.png',
    departments: [
      { name: 'DEQ', quantity: 25 },
      { name: 'DCC', quantity: 15 },
    ],
  },
  {
    id: 3,
    catmat: '604032',
    description: 'Cadinho (Porcelana 100ml)',
    unit: 'UNIDADE',
    unitPrice: 39.0,
    quantity: 40,
    totalPrice: 1560.0,
    image: '/items_img/img1.png',
    departments: [
      { name: 'DEQ', quantity: 25 },
      { name: 'DCC', quantity: 15 },
    ],
  },
  {
    id: 4,
    catmat: '604032',
    description: 'Cadinho (Porcelana 100ml)',
    unit: 'UNIDADE',
    unitPrice: 39.0,
    quantity: 40,
    totalPrice: 1560.0,
    image: '/items_img/img1.png',
    departments: [
      { name: 'DEQ', quantity: 25 },
      { name: 'DCC', quantity: 15 },
    ],
  },
  {
    id: 5,
    catmat: '604032',
    description: 'Cadinho (Porcelana 100ml)',
    unit: 'UNIDADE',
    unitPrice: 39.0,
    quantity: 40,
    totalPrice: 1560.0,
    image: '/items_img/img1.png',
    departments: [
      { name: 'DEQ', quantity: 25 },
      { name: 'DCC', quantity: 15 },
    ],
  },
  {
    id: 6,
    catmat: '604032',
    description: 'Cadinho (Porcelana 100ml)',
    unit: 'UNIDADE',
    unitPrice: 39.0,
    quantity: 40,
    totalPrice: 1560.0,
    image: '/items_img/img1.png',
    departments: [
      { name: 'DEQ', quantity: 25 },
      { name: 'DCC', quantity: 15 },
    ],
  },
  {
    id: 7,
    catmat: '604032',
    description: 'Cadinho (Porcelana 100ml)',
    unit: 'UNIDADE',
    unitPrice: 39.0,
    quantity: 40,
    totalPrice: 1560.0,
    image: '/items_img/img1.png',
    departments: [
      { name: 'DEQ', quantity: 25 },
      { name: 'DCC', quantity: 15 },
    ],
  },
  {
    id: 8,
    catmat: '604032',
    description: 'Cadinho (Porcelana 100ml)',
    unit: 'UNIDADE',
    unitPrice: 39.0,
    quantity: 40,
    totalPrice: 1560.0,
    image: '/items_img/img1.png',
    departments: [
      { name: 'DEQ', quantity: 25 },
      { name: 'DCC', quantity: 15 },
    ],
  },
  {
    id: 9,
    catmat: '604032',
    description: 'Cadinho (Porcelana 100ml)',
    unit: 'UNIDADE',
    unitPrice: 39.0,
    quantity: 40,
    totalPrice: 1560.0,
    image: '/items_img/img1.png',
    departments: [
      { name: 'DEQ', quantity: 25 },
      { name: 'DCC', quantity: 15 },
    ],
  },
  {
    id: 10,
    catmat: '604032',
    description: 'Cadinho (Porcelana 100ml)',
    unit: 'UNIDADE',
    unitPrice: 39.0,
    quantity: 40,
    totalPrice: 1560.0,
    image: '/items_img/img1.png',
    departments: [
      { name: 'DEQ', quantity: 25 },
      { name: 'DCC', quantity: 15 },
    ],
  },
  {
    id: 11,
    catmat: '604032',
    description: 'Cadinho (Porcelana 100ml)',
    unit: 'UNIDADE',
    unitPrice: 39.0,
    quantity: 40,
    totalPrice: 1560.0,
    image: '/items_img/img1.png',
    departments: [
      { name: 'DEQ', quantity: 25 },
      { name: 'DCC', quantity: 15 },
    ],
  },
  {
    id: 12,
    catmat: '604032',
    description: 'Cadinho (Porcelana 100ml)',
    unit: 'UNIDADE',
    unitPrice: 39.0,
    quantity: 40,
    totalPrice: 1560.0,
    image: '/items_img/img1.png',
    departments: [
      { name: 'DEQ', quantity: 25 },
      { name: 'DCC', quantity: 15 },
    ],
  },
  {
    id: 13,
    catmat: '604032',
    description: 'Cadinho (Porcelana 100ml)',
    unit: 'UNIDADE',
    unitPrice: 39.0,
    quantity: 40,
    totalPrice: 1560.0,
    image: '/items_img/img1.png',
    departments: [
      { name: 'DEQ', quantity: 25 },
      { name: 'DCC', quantity: 15 },
    ],
  },
  {
    id: 14,
    catmat: '604032',
    description: 'Cadinho (Porcelana 100ml)',
    unit: 'UNIDADE',
    unitPrice: 39.0,
    quantity: 40,
    totalPrice: 1560.0,
    image: '/items_img/img1.png',
    departments: [
      { name: 'DEQ', quantity: 25 },
      { name: 'DCC', quantity: 15 },
    ],
  },
  {
    id: 15,
    catmat: '604032',
    description: 'Cadinho (Porcelana 100ml)',
    unit: 'UNIDADE',
    unitPrice: 39.0,
    quantity: 40,
    totalPrice: 1560.0,
    image: '/items_img/img1.png',
    departments: [
      { name: 'DEQ', quantity: 25 },
      { name: 'DCC', quantity: 15 },
    ],
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
      class="flex flex-column lg:flex-row lg:align-items-center justify-content-between gap-4 mt-2"
    >
      <div class="flex flex-column md:flex-row md:flex-wrap align-items-center gap-2">
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
        :value="materials"
        v-model:expandedRows="expandedRows"
        dataKey="id"
        tableStyle="min-width: 60rem"
        size="small"
        paginator
        :rows="50"
        :rowsPerPageOptions="[5, 10, 20, 50]"
        scrollable
        scrollHeight="550px"
        ref="dt"
      >
        <Column :expander="true" style="width: 3rem" />

        <Column field="catmat" header="CATMAT"></Column>
        <Column field="description" header="DESCRIÇÃO"></Column>
        <Column field="unit" header="UNIDADE DE MEDIDA"></Column>
        <Column field="quantity" header="QUANTIDADE"></Column>
        <Column field="unitPrice" header="VALOR UNITÁRIO">
          <template #body="slotProps">
            {{
              new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(
                slotProps.data.unitPrice,
              )
            }}
          </template>
        </Column>
        <Column field="totalPrice" header="VALOR TOTAL">
          <template #body="slotProps">
            {{
              new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(
                slotProps.data.totalPrice,
              )
            }}
          </template>
        </Column>

        <template #expansion="slotProps">
          <div class="flex gap-4 p-4">
            <div class="w-4rem flex-shrink-0">
              <img :src="slotProps.data.image" :alt="slotProps.data.description" class="w-full" />
            </div>
            <div>
              <DataTable :value="slotProps.data.departments" size="small">
                <Column field="name" header="Departamento"></Column>
                <Column field="quantity" header="Quantidade Solicitada"></Column>
              </DataTable>
            </div>
          </div>
        </template>
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

/* Para Chrome, Safari, Edge, etc. */
.table-container::-webkit-scrollbar {
  width: 5px;
}

.table-container::-webkit-scrollbar-track {
  background: transparent;
}

.table-container::-webkit-scrollbar-thumb {
  background: var(--p-surface-400, #ccc);
  border-radius: 10px;
}

.table-container::-webkit-scrollbar-thumb:hover {
  background: var(--p-surface-500, #aaa);
}

:deep(.p-toolbar-start) {
  /* flex: 1 1 auto; é uma propriedade do Flexbox que diz:
    - flex-grow: 1 (Pode crescer para ocupar espaço livre)
    - flex-shrink: 1 (Pode encolher se necessário)
    - flex-basis: auto (O tamanho inicial é o do conteúdo)
    Isso faz com que este grupo seja o "dominante" e ocupe o espaço.
  */
  flex: 1 1 auto;
}
</style>
