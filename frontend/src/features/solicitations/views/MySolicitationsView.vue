<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import CustomBreadcrumb from '@/components/ui/CustomBreadcrumb.vue'
import Button from 'primevue/button'
import { useRoute } from 'vue-router'
import { useMySolicitationListStore } from '../stores/mySolicitationList'
import { storeToRefs } from 'pinia'
import { solicitationService } from '../services/solicitationService'
import { formatDate } from '@/utils/dateUtils'
import { formatCurrency } from '@/utils/currency'

const route = useRoute()
const mySolicitationListStore = useMySolicitationListStore()
const { solicitations } = storeToRefs(mySolicitationListStore)

watch(
  () => route.query,
  async (newQuery) => {
    mySolicitationListStore.fetchAll()
  },
  { immediate: true },
)

const columns = [
  { field: 'id', header: 'ID' },
  { field: 'typeDisplay', header: 'Tipo' },
  { field: 'dataCriacao', header: 'Data de Criação' },
  { field: 'itemsCount', header: 'Itens Únicos' },
  { field: 'totalItemsQuantity', header: 'Total de Itens' }, 
  { field: 'totalEstimatedPrice', header: 'Preço Total Estimado' },
]
</script>

<template>
  <div class="flex flex-column w-full h-full align-items-center p-2 gap-3">
    <div class="flex w-full">
      <CustomBreadcrumb />
    </div>
    <div class="flex align-items-center justify-content-between w-full">
      <h3 class="m-0 pl-2">Minhas Solicitações</h3>
      <Button icon="pi pi-download" label="Exportar" size="small" />
    </div>

    <DataTable :value="solicitations" tableStyle="min-width: 50rem">
      <Column v-for="col of columns" :key="col.field" :field="col.field" :header="col.header">
        <template #body="slotProps">
          <span v-if="col.field === 'dataCriacao'">
            {{ formatDate(slotProps.data.dataCriacao, 'long') }}
          </span>

          <span v-else-if="col.field === 'totalEstimatedPrice'">
            {{ formatCurrency(slotProps.data.totalEstimatedPrice) }}
          </span>

          <span v-else>
            {{ slotProps.data[col.field] }}
          </span>
        </template>
      </Column>
    </DataTable>

    <!-- todo: insert pagination -->
  </div>
</template>
