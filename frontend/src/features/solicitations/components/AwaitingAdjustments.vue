<script setup lang="ts">
import { useRouter } from 'vue-router'
import Card from 'primevue/card'
import Button from 'primevue/button'
import { computed, ref, watch } from 'vue'
import { useMySolicitationListStore } from '../stores/mySolicitationList'
import { storeToRefs } from 'pinia'
import type { MySolicitationFilters } from '../types'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import { formatDate } from '@/utils/dateUtils'
import { getSolicitationStatusOptions } from '../utils'
import { Tag } from 'primevue'

const router = useRouter()
const myListStore = useMySolicitationListStore()
const { solicitations, isLoading, error } = storeToRefs(myListStore)

const sortOrder = ref<'desc' | 'asc'>('desc')
const toggleSortOrder = () => {
  sortOrder.value = sortOrder.value === 'desc' ? 'asc' : 'desc'
}
const sortButton = computed(() => {
  if (sortOrder.value === 'desc') {
    return { label: 'Mais Recentes', icon: 'pi pi-sort-amount-down-alt' }
  } else {
    return { label: 'Mais Antigos', icon: 'pi pi-sort-amount-up' }
  }
})

const columns = [
  { field: 'externalId', header: 'Código' },
  { field: 'dataCriacao', header: 'Data' },
  { field: 'department', header: 'Departamento' },
]

const getTagProps = (statusId: number) => {
  const statusData = getSolicitationStatusOptions(statusId)
  return {
    value: statusData?.nome,
    severity: statusData?.severity,
    icon: statusData?.icon,
  }
}

const verDetalhes = (id: number) => {
  router.push(`/solicitacoes/${id}`)
}

const latestSolicitations = computed(() => {
  return solicitations.value.slice(0, 5)
})

watch(
  sortOrder,
  (newSortOrder) => {
    const filters: MySolicitationFilters = {
      externalId: '',
      tipo: '',
      dateRange: null,
      pageNumber: '1',
      pageSize: '10',
      statusIds: [2], // Filtro fixo para Aguardando Ajustes
      sortOrder: newSortOrder,
    }
    myListStore.fetchAll(filters)
  },
  { immediate: true },
)
</script>

<template>
  <Card>
    <template #title>
      <div class="flex justify-content-between align-items-center gap-4 mb-2">
        <div class="flex align-items-center gap-2">
          <div
            class="flex align-items-center justify-content-center border-round"
            style="width: 3rem; height: 3rem; background-color: #f6c84f1a; color: #f6c84f"
          >
            <span class="material-symbols-outlined" style="font-size: 1.5rem">report_problem</span>
          </div>
          <span class="font-semibold text-xl">Solicitações Aguardando Ajustes</span>
        </div>
      </div>
    </template>
    <template #content>
      <DataTable
        v-if="solicitations.length > 0 && !isLoading"
        :value="latestSolicitations"
        class="w-full"
        size="small"
        removableSort
      >
        <Column
          v-for="col of columns"
          :key="col.field"
          :field="col.field"
          :header="col.header"
          sortable
        >
          <template #body="slotProps">
            <span v-if="col.field === 'dataCriacao'">
              {{ formatDate(slotProps.data.dataCriacao, 'short') }}
            </span>

            <span v-else-if="col.field === 'status'">
              <Tag v-bind="getTagProps(slotProps.data.status.id)" />
            </span>

            <span v-else>
              {{ slotProps.data[col.field] }}
            </span>
          </template>
        </Column>
        <Column header="Ações">
          <template #body="slotProps">
            <Button
              icon="pi pi-eye"
              text
              rounded
              aria-label="Ver Detalhes"
              v-tooltip.left="'Ver Detalhes'"
              @click="verDetalhes(slotProps.data.id)"
            />
          </template>
        </Column>
      </DataTable>
      <div v-else class="text-center p-2">
        <div class="flex flex-column align-items-center">
          <p class="text-color-secondary mt-0">
            No momento você não possui solicitações devolvidas para ajuste. Quando o gestor
            solicitar correções, elas aparecerão aqui.
          </p>
        </div>
      </div>
    </template>
    <template #footer>
      <div class="flex align-items-center justify-content-end w-full mt-2">
        <Button
          label="Ver Todas"
          text
          icon="pi pi-arrow-right"
          icon-pos="right"
          @click="router.push('/solicitacoes')"
          size="small"
        />
      </div>
    </template>
  </Card>
</template>

<style scoped></style>
