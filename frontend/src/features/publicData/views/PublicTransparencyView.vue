<script setup lang="ts">
import { computed, reactive, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { storeToRefs } from 'pinia'
import { useToast } from 'primevue/usetoast'
import Button from 'primevue/button'
import Card from 'primevue/card'
import Column from 'primevue/column'
import DataTable from 'primevue/datatable'
import DatePicker from 'primevue/datepicker'
import FloatLabel from 'primevue/floatlabel'
import InputText from 'primevue/inputtext'
import Message from 'primevue/message'
import ProgressSpinner from 'primevue/progressspinner'
import Select from 'primevue/select'
import Tag from 'primevue/tag'
import Logo from '@/components/ui/Logo.vue'
import CustomPaginator from '@/components/ui/CustomPaginator.vue'
import { formatCurrency } from '@/utils/currency'
import { formatDate } from '@/utils/dateUtils'
import { useAuthStore } from '@/features/autentication/stores/authStore'
import { usePublicDataStore } from '@/features/publicData/stores/publicDataStore'
import type { PublicSolicitationFilters } from '@/features/publicData/types'

const router = useRouter()
const route = useRoute()
const toast = useToast()

const authStore = useAuthStore()
const publicDataStore = usePublicDataStore()

const {
  solicitations,
  totalCount,
  pageNumber,
  pageSize,
  hasNextPage,
  totalItemsRequested,
  totalAmountRequested,
  isLoading,
  isExporting,
  error,
} = storeToRefs(publicDataStore)

const itemTypeOptions = [
  { label: 'Todos os tipos', value: '' },
  { label: 'Itens Gerais', value: 'geral' },
  { label: 'Itens Patrimoniais', value: 'patrimonial' },
]

const activeOptions = [
  { label: 'Todos os status', value: '' },
  { label: 'Somente solicitações ativas', value: 'true' },
  { label: 'Incluir inativas', value: 'false' },
]

const filters = reactive<PublicSolicitationFilters>({
  dataInicio: '',
  dataFim: '',
  statusNome: '',
  siglaDepartamento: '',
  categoriaNome: '',
  itemNome: '',
  catMat: '',
  itemsType: '',
  valorMinimo: '',
  valorMaximo: '',
  somenteSolicitacoesAtivas: 'true',
  pageNumber: '1',
  pageSize: '25',
})

const startDate = ref<Date | null>(null)
const endDate = ref<Date | null>(null)

const toIsoStartDate = (date: Date) => {
  const start = new Date(date)
  start.setHours(0, 0, 0, 0)
  return start.toISOString()
}

const toIsoEndDate = (date: Date) => {
  const end = new Date(date)
  end.setHours(23, 59, 59, 999)
  return end.toISOString()
}

const normalizeQuery = (query: Record<string, any>): PublicSolicitationFilters => {
  return {
    dataInicio: typeof query.dataInicio === 'string' ? query.dataInicio : '',
    dataFim: typeof query.dataFim === 'string' ? query.dataFim : '',
    statusNome: typeof query.statusNome === 'string' ? query.statusNome : '',
    siglaDepartamento: typeof query.siglaDepartamento === 'string' ? query.siglaDepartamento : '',
    categoriaNome: typeof query.categoriaNome === 'string' ? query.categoriaNome : '',
    itemNome: typeof query.itemNome === 'string' ? query.itemNome : '',
    catMat: typeof query.catMat === 'string' ? query.catMat : '',
    itemsType:
      query.itemsType === 'geral' || query.itemsType === 'patrimonial'
        ? query.itemsType
        : '',
    valorMinimo: typeof query.valorMinimo === 'string' ? query.valorMinimo : '',
    valorMaximo: typeof query.valorMaximo === 'string' ? query.valorMaximo : '',
    somenteSolicitacoesAtivas:
      query.somenteSolicitacoesAtivas === 'false' || query.somenteSolicitacoesAtivas === 'true'
        ? query.somenteSolicitacoesAtivas
        : 'true',
    pageNumber: typeof query.pageNumber === 'string' ? query.pageNumber : '1',
    pageSize: typeof query.pageSize === 'string' ? query.pageSize : '25',
  }
}

const applyFilters = () => {
  const query: Record<string, string> = {}

  if (startDate.value) {
    query.dataInicio = toIsoStartDate(startDate.value)
  }
  if (endDate.value) {
    query.dataFim = toIsoEndDate(endDate.value)
  }

  if (filters.statusNome) query.statusNome = filters.statusNome
  if (filters.siglaDepartamento) query.siglaDepartamento = filters.siglaDepartamento
  if (filters.categoriaNome) query.categoriaNome = filters.categoriaNome
  if (filters.itemNome) query.itemNome = filters.itemNome
  if (filters.catMat) query.catMat = filters.catMat
  if (filters.itemsType) query.itemsType = filters.itemsType
  if (filters.valorMinimo) query.valorMinimo = filters.valorMinimo
  if (filters.valorMaximo) query.valorMaximo = filters.valorMaximo

  query.somenteSolicitacoesAtivas = filters.somenteSolicitacoesAtivas || 'true'
  query.pageNumber = '1'
  query.pageSize = filters.pageSize || '25'

  router.push({ query })
}

const clearFilters = () => {
  startDate.value = null
  endDate.value = null
  router.push({
    query: {
      somenteSolicitacoesAtivas: 'true',
      pageNumber: '1',
      pageSize: '25',
    },
  })
}

const buildFiltersForRequest = () => {
  return {
    ...filters,
    dataInicio: startDate.value ? toIsoStartDate(startDate.value) : '',
    dataFim: endDate.value ? toIsoEndDate(endDate.value) : '',
  }
}

const exportCsv = async () => {
  try {
    const blob = await publicDataStore.exportPublicSolicitationsCsv(buildFiltersForRequest())
    const url = window.URL.createObjectURL(blob)
    const link = document.createElement('a')
    link.href = url
    link.download = `dados_publicos_solicitacoes_${new Date().toISOString().slice(0, 10)}.csv`

    document.body.appendChild(link)
    link.click()
    document.body.removeChild(link)
    window.URL.revokeObjectURL(url)

    toast.add({
      severity: 'success',
      summary: 'Sucesso',
      detail: 'Arquivo CSV exportado com sucesso.',
      life: 3000,
    })
  } catch {
    toast.add({
      severity: 'error',
      summary: 'Erro',
      detail: 'Não foi possível exportar o CSV de dados públicos.',
      life: 3000,
    })
  }
}

const goToLogin = () => {
  router.push('/login')
}

const goToSystemHome = () => {
  router.push('/')
}

watch(
  () => route.query,
  async (newQuery) => {
    const queryFilters = normalizeQuery(newQuery)

    Object.assign(filters, queryFilters)

    startDate.value = queryFilters.dataInicio ? new Date(queryFilters.dataInicio) : null
    endDate.value = queryFilters.dataFim ? new Date(queryFilters.dataFim) : null

    await publicDataStore.fetchPublicSolicitations(queryFilters)
  },
  { immediate: true },
)

const hasFiltersApplied = computed(() => {
  return !!(
    filters.statusNome ||
    filters.siglaDepartamento ||
    filters.categoriaNome ||
    filters.itemNome ||
    filters.catMat ||
    filters.itemsType ||
    filters.valorMinimo ||
    filters.valorMaximo ||
    startDate.value ||
    endDate.value
  )
})

const statusSeverity = (status: string) => {
  const normalized = status.toLowerCase()

  if (normalized.includes('aprov')) return 'success'
  if (normalized.includes('pend')) return 'warn'
  if (normalized.includes('neg') || normalized.includes('cancel')) return 'danger'

  return 'info'
}
</script>

<template>
  <div class="surface-ground min-h-screen">
    <header class="surface-card border-bottom-1 border-200 px-3 md:px-5 py-3">
      <div class="flex justify-content-between align-items-center gap-2">
        <router-link to="/transparencia" class="no-underline">
          <Logo />
        </router-link>

        <Button
          v-if="authStore.isAuthenticated"
          label="Voltar ao sistema"
          icon="pi pi-arrow-left"
          text
          size="small"
          @click="goToSystemHome"
        />
        <Button v-else label="Entrar" icon="pi pi-sign-in" text size="small" @click="goToLogin" />
      </div>
    </header>

    <main class="px-3 md:px-5 py-4 md:py-5">
      <div class="mb-4">
        <h1 class="m-0 text-2xl md:text-4xl text-primary">Portal de Transparência</h1>
        <p class="m-0 mt-2 text-color-secondary">
          Consulte solicitações públicas com dados mascarados, aplique filtros e exporte os resultados.
        </p>
      </div>

      <Card class="mb-4">
        <template #content>
          <div class="grid">
            <div class="col-12 md:col-6 lg:col-3">
              <FloatLabel variant="on" class="w-full">
                <DatePicker
                  v-model="startDate"
                  inputId="data-inicio"
                  showIcon
                  iconDisplay="input"
                  dateFormat="dd/mm/yy"
                  class="w-full"
                  size="small"
                />
                <label for="data-inicio">Data início</label>
              </FloatLabel>
            </div>

            <div class="col-12 md:col-6 lg:col-3">
              <FloatLabel variant="on" class="w-full">
                <DatePicker
                  v-model="endDate"
                  inputId="data-fim"
                  showIcon
                  iconDisplay="input"
                  dateFormat="dd/mm/yy"
                  class="w-full"
                  size="small"
                />
                <label for="data-fim">Data fim</label>
              </FloatLabel>
            </div>

            <div class="col-12 md:col-6 lg:col-3">
              <FloatLabel variant="on" class="w-full">
                <InputText v-model="filters.statusNome" inputId="status" class="w-full" size="small" />
                <label for="status">Status</label>
              </FloatLabel>
            </div>

            <div class="col-12 md:col-6 lg:col-3">
              <FloatLabel variant="on" class="w-full">
                <InputText
                  v-model="filters.siglaDepartamento"
                  inputId="sigla-departamento"
                  class="w-full"
                  size="small"
                />
                <label for="sigla-departamento">Sigla do departamento</label>
              </FloatLabel>
            </div>

            <div class="col-12 md:col-6 lg:col-3">
              <FloatLabel variant="on" class="w-full">
                <InputText v-model="filters.itemNome" inputId="item" class="w-full" size="small" />
                <label for="item">Item</label>
              </FloatLabel>
            </div>

            <div class="col-12 md:col-6 lg:col-3">
              <FloatLabel variant="on" class="w-full">
                <InputText v-model="filters.categoriaNome" inputId="categoria" class="w-full" size="small" />
                <label for="categoria">Categoria</label>
              </FloatLabel>
            </div>

            <div class="col-12 md:col-6 lg:col-2">
              <FloatLabel variant="on" class="w-full">
                <InputText v-model="filters.catMat" inputId="catmat" class="w-full" size="small" />
                <label for="catmat">CATMAT</label>
              </FloatLabel>
            </div>

            <div class="col-12 md:col-6 lg:col-2">
              <FloatLabel variant="on" class="w-full">
                <Select
                  v-model="filters.itemsType"
                  :options="itemTypeOptions"
                  optionLabel="label"
                  optionValue="value"
                  inputId="tipo-item"
                  class="w-full"
                  size="small"
                />
                <label for="tipo-item">Tipo</label>
              </FloatLabel>
            </div>

            <div class="col-12 md:col-6 lg:col-2">
              <FloatLabel variant="on" class="w-full">
                <InputText
                  v-model="filters.valorMinimo"
                  inputId="valor-min"
                  class="w-full"
                  size="small"
                />
                <label for="valor-min">Valor mínimo</label>
              </FloatLabel>
            </div>

            <div class="col-12 md:col-6 lg:col-2">
              <FloatLabel variant="on" class="w-full">
                <InputText
                  v-model="filters.valorMaximo"
                  inputId="valor-max"
                  class="w-full"
                  size="small"
                />
                <label for="valor-max">Valor máximo</label>
              </FloatLabel>
            </div>

            <div class="col-12 md:col-6 lg:col-3">
              <FloatLabel variant="on" class="w-full">
                <Select
                  v-model="filters.somenteSolicitacoesAtivas"
                  :options="activeOptions"
                  optionLabel="label"
                  optionValue="value"
                  inputId="status-ativo"
                  class="w-full"
                  size="small"
                />
                <label for="status-ativo">Situação da solicitação</label>
              </FloatLabel>
            </div>

            <div class="col-12">
              <div class="flex flex-column sm:flex-row gap-2 sm:justify-content-end">
                <Button
                  label="Limpar"
                  icon="pi pi-times"
                  severity="secondary"
                  text
                  @click="clearFilters"
                />
                <Button label="Buscar" icon="pi pi-search" @click="applyFilters" />
                <Button
                  label="Exportar CSV"
                  icon="pi pi-download"
                  outlined
                  :loading="isExporting"
                  @click="exportCsv"
                />
              </div>
            </div>
          </div>
        </template>
      </Card>

      <div class="grid mb-4">
        <div class="col-12 md:col-4">
          <Card>
            <template #content>
              <div class="text-sm text-color-secondary">Solicitações filtradas</div>
              <div class="text-2xl font-semibold mt-2">{{ totalCount }}</div>
            </template>
          </Card>
        </div>

        <div class="col-12 md:col-4">
          <Card>
            <template #content>
              <div class="text-sm text-color-secondary">Itens solicitados</div>
              <div class="text-2xl font-semibold mt-2">{{ totalItemsRequested }}</div>
            </template>
          </Card>
        </div>

        <div class="col-12 md:col-4">
          <Card>
            <template #content>
              <div class="text-sm text-color-secondary">Valor total solicitado</div>
              <div class="text-2xl font-semibold mt-2">{{ formatCurrency(totalAmountRequested) }}</div>
            </template>
          </Card>
        </div>
      </div>

      <Message v-if="error" severity="error" class="mb-3">{{ error }}</Message>

      <Card>
        <template #content>
          <div v-if="isLoading" class="flex justify-content-center py-6">
            <ProgressSpinner style="width: 50px; height: 50px" strokeWidth="4" />
          </div>

          <div v-else>
            <DataTable
              :value="solicitations"
              dataKey="id"
              stripedRows
              size="small"
              scrollable
              class="mb-3"
            >
              <Column field="externalId" header="Protocolo" style="min-width: 10rem">
                <template #body="{ data }">
                  {{ data.externalId || '-' }}
                </template>
              </Column>

              <Column field="dataCriacao" header="Data" style="min-width: 9rem">
                <template #body="{ data }">
                  {{ formatDate(data.dataCriacao, 'short') }}
                </template>
              </Column>

              <Column field="tipoSolicitacao" header="Tipo" style="min-width: 9rem" />

              <Column field="statusNome" header="Status" style="min-width: 10rem">
                <template #body="{ data }">
                  <Tag :value="data.statusNome" :severity="statusSeverity(data.statusNome)" />
                </template>
              </Column>

              <Column field="departamentoSigla" header="Departamento" style="min-width: 8rem" />

              <Column field="solicitanteNomeMascarado" header="Solicitante" style="min-width: 14rem" />

              <Column field="valorTotalSolicitacao" header="Valor" style="min-width: 10rem">
                <template #body="{ data }">
                  {{ formatCurrency(data.valorTotalSolicitacao) }}
                </template>
              </Column>

              <Column header="Itens" style="min-width: 6rem">
                <template #body="{ data }">
                  {{ data.itens?.length || 0 }}
                </template>
              </Column>
            </DataTable>

            <div v-if="totalCount > 0" class="mb-3">
              <CustomPaginator
                :current-url="route.path"
                :total-count="totalCount"
                :has-next-page="hasNextPage"
                :page-size="pageSize"
                :page-number="pageNumber"
                responsive-breakpoint="md"
              />
            </div>

            <div v-if="!solicitations.length" class="text-center py-5 text-color-secondary">
              <span v-if="hasFiltersApplied">Nenhum resultado encontrado para os filtros aplicados.</span>
              <span v-else>Nenhuma solicitação pública disponível no momento.</span>
            </div>
          </div>
        </template>
      </Card>
    </main>
  </div>
</template>
