<script setup lang="ts">
import { ref, computed, reactive, provide, readonly, watch, onMounted } from 'vue'
import Card from 'primevue/card'
import Tabs from 'primevue/tabs'
import TabList from 'primevue/tablist'
import Tab from 'primevue/tab'
import TabPanels from 'primevue/tabpanels'
import TabPanel from 'primevue/tabpanel'
import Button from 'primevue/button'
import Message from 'primevue/message'
import SolicitationList from '../components/SolicitationList.vue'
import SolicitationAnalysis from '../components/SolicitationAnalysis.vue'
import { SolicitationContextKey, type SolicitationContext } from '../keys'
import { useAuthStore } from '@/features/autentication/stores/authStore'
import { storeToRefs } from 'pinia'
import { useRoute } from 'vue-router'
import { useSettingStore } from '@/features/settings/stores/settingStore'
import { formatDate } from '@/utils/dateUtils'
import SolicitationDetailsSkeleton from '../components/SolicitationDetailsSkeleton.vue'
import { useSolicitationStore } from '../stores/solicitationStore'
import CustomBreadcrumb from '@/components/ui/CustomBreadcrumb.vue'
import { Select, useConfirm, useToast } from 'primevue'
import { SAVE_CONFIRMATION } from '@/utils/confirmationFactoryUtils'
import type { Solicitation } from '../types'
import Textarea from 'primevue/textarea'
import { toTitleCase } from '@/utils/stringUtils'
import SolicitationHistory from '../components/SolicitationHistory.vue'
import { useSolicitationHistoryStore } from '../stores/historySolicitationStore'

const solicitationContext = reactive<SolicitationContext>({
  dialogMode: 'selection',
  isGeneral: false, // initial
})

provide(SolicitationContextKey, readonly(solicitationContext))

const route = useRoute()
const authStore = useAuthStore()
const toast = useToast()
const confirm = useConfirm()
const settingStore = useSettingStore()
const { deadline, deadlineHasExpired } = storeToRefs(settingStore)
const { user, isGestor } = storeToRefs(authStore)
const solicitationStore = useSolicitationStore()
const { currentSolicitation, isLoading, error, currentSolicitationBackup } =
  storeToRefs(solicitationStore)
const historyStore = useSolicitationHistoryStore()

const activeTab = ref('0')

const loggedUserCreatedIt = computed(() => {
  return currentSolicitation.value?.solicitante.unidade.sigla === user.value?.unidade?.sigla
})

const isEditing = ref<boolean>(false)

const isEditingStatus = ref<boolean>(false)
const selectedStatus = ref<number | undefined>(currentSolicitation.value?.status.id)
const statusOptions = [
  { id: 1, nome: 'Pendente' },
  { id: 2, nome: 'Aguardando Ajustes' },
  { id: 3, nome: 'Aprovada' },
  { id: 4, nome: 'Rejeitada' },
  { id: 5, nome: 'Cancelada' },
  { id: 6, nome: 'Encerrada' },
]

const handleCancel = () => {
  const id = currentSolicitation.value?.id
  if (id && solicitationStore.isDirty) {
    solicitationStore.fetchById(id)
  }
  isEditing.value = false
}

const acceptSaveChanges = async () => {
  if (!currentSolicitation.value) return
  const success = await solicitationStore.update(currentSolicitation.value)
  if (success) {
    isEditing.value = false
    historyStore.clearHistory()
    historyStore.fetchSolicitationHistory(currentSolicitation.value.id)
    activeTab.value = '0'
  }
}

const saveChanges = () => {
  if (!isSolicitationValid(currentSolicitation?.value)) return
  confirm.require({
    ...SAVE_CONFIRMATION,
    accept: async () => acceptSaveChanges(),
  })
}

const isSolicitationValid = (solicitation: Solicitation | null): boolean => {
  if (!solicitation) {
    toast.add({
      severity: 'error',
      summary: 'Erro de Validação',
      detail: 'Não há dados da solicitação para validar.',
      life: 3000,
    })
    return false
  }

  if (!solicitation.itens || solicitation.itens.length === 0) {
    toast.add({
      severity: 'error',
      summary: 'Nenhum Item',
      detail: 'A solicitação deve conter pelo menos um item.',
      life: 3000,
    })
    return false
  }

  let isValid = true

  if (
    solicitationContext.isGeneral &&
    (!solicitation.justificativaGeral || solicitation.justificativaGeral.trim() === '')
  ) {
    toast.add({
      severity: 'error',
      summary: 'Campo Obrigatório',
      detail: 'A "Justificativa Geral" é obrigatória para solicitações do tipo Geral.',
      life: 3000,
    })
    isValid = false
  }

  for (const item of solicitation.itens) {
    if (!item.quantidade || item.quantidade <= 0) {
      toast.add({
        severity: 'error',
        summary: 'Quantidade Inválida',
        detail: `O item "${item.nome}" deve ter uma quantidade maior que zero.`,
        life: 3000,
      })
      isValid = false
    }

    if (!item.precoSugerido || item.precoSugerido <= 0) {
      toast.add({
        severity: 'error',
        summary: 'Preço Inválido',
        detail: `O item "${item.nome}" deve ter um preço sugerido maior que zero.`,
        life: 3000,
      })
      isValid = false
    }

    if (
      !solicitationContext.isGeneral &&
      (!item.justificativa || item.justificativa.trim() === '')
    ) {
      toast.add({
        severity: 'error',
        summary: 'Campo Obrigatório',
        detail: `A justificativa é obrigatória para o item "${item.nome}" em solicitações patrimoniais.`,
        life: 3000,
      })
      isValid = false
    }
  }

  return isValid
}

const handleEdit = () => {
  isEditing.value = true
  activeTab.value = '0'
}

const cancelChangeStatus = () => {
  selectedStatus.value = currentSolicitation.value?.status.id
  isEditingStatus.value = false
}

const handleStatusChange = () => {
  console.log('')
}

watch(
  () => route.params.id,
  (newId) => {
    if (newId && typeof newId === 'string') {
      solicitationStore.fetchById(parseInt(newId, 10))
      historyStore.clearHistory()
    }
  },
  {
    immediate: true,
  },
)

watch(
  currentSolicitationBackup,
  (newSolicitation) => {
    if (newSolicitation) {
      solicitationContext.isGeneral = !!newSolicitation.justificativaGeral
    } else {
      solicitationContext.isGeneral = false
    }
  },
  {
    deep: true,
  },
)

onMounted(() => {
  settingStore.fetchSettings()
})
</script>

<!-- TODO: componentizar mais esse componente -->
<!-- TODO: UM SOLICITANTE SÓ PODE VISUALIZAR OS DETALHES DE SOLICITAÇÕES FEITAS PELO SEU DEPARTAMENTO-->

<template>
  <SolicitationDetailsSkeleton v-if="isLoading" />
  <div class="p-2" v-if="currentSolicitation">
    <div class="flex align-items-center justify-content-between mb-2">
      <CustomBreadcrumb :dynamic-label="currentSolicitation?.externalId" />
    </div>
    <div
      class="flex flex-column md:flex-row md:align-items-center md:justify-content-between mb-2 gap-2 md:gap-0"
    >
      <h3 class="m-0">Detalhes da Solicitação {{ currentSolicitation.externalId }}</h3>
      <div class="flex justify-content-end gap-2">
        <Message
          v-if="loggedUserCreatedIt"
          icon="pi pi-info-circle"
          :severity="deadlineHasExpired ? 'error' : 'warn'"
          size="small"
          :closable="false"
        >
          {{
            deadlineHasExpired
              ? 'Prazo para ajustes encerrado.'
              : `Prazo para ajustes: ${deadline ? formatDate(deadline, 'short') : 'Não definido'}`
          }}
        </Message>
        <!-- TODO: só devo poder editar se o status da solicitacao nao for cancelada, rejeitada ou encerrada -->
        <Button
          v-if="!isEditing && !deadlineHasExpired && loggedUserCreatedIt"
          icon="pi pi-pencil"
          label="Editar"
          size="small"
          @click="handleEdit"
        />
        <Button
          v-if="isEditing"
          icon="pi pi-pencil"
          text
          severity="danger"
          label="Cancelar"
          size="small"
          @click="handleCancel"
        />
        <Button
          v-if="isEditing"
          icon="pi pi-save"
          label="Salvar"
          size="small"
          @click="saveChanges()"
          :disabled="!solicitationStore.isDirty"
        />
      </div>
    </div>

    <div class="grid">
      <div class="col-12 lg:col-4">
        <Card class="h-full">
          <template #content>
            <ul class="list-none p-0 m-0">
              <li class="flex align-items-start mb-4">
                <i class="pi pi-user text-primary text-xl mr-3"></i>
                <div>
                  <span class="text-sm text-color-secondary">Requisitante</span>
                  <p class="font-bold m-0">
                    {{ currentSolicitation.solicitante.nome }} ({{
                      currentSolicitation.solicitante.unidade.sigla
                    }})
                  </p>
                </div>
              </li>
              <li class="flex align-items-start">
                <i class="pi pi-calendar text-primary text-xl mr-3"></i>
                <div>
                  <span class="text-sm text-color-secondary">Data da Criação</span>
                  <p class="font-bold m-0">
                    {{ formatDate(currentSolicitation.dataCriacao, 'long') }}
                  </p>
                </div>
              </li>
            </ul>
          </template>
        </Card>
      </div>

      <div class="col-12 lg:col-4">
        <Card class="h-full">
          <template #content>
            <ul class="list-none p-0 m-0">
              <li class="flex align-items-center mb-4">
                <i class="pi pi-envelope text-primary text-xl mr-3"></i>
                <div>
                  <span class="text-sm text-color-secondary">Tipo da Solicitação</span>
                  <p class="font-bold m-0">
                    {{ solicitationContext.isGeneral ? 'Geral' : 'Patrimonial' }}
                  </p>
                </div>
              </li>
              <li class="flex align-items-start">
                <i class="pi pi-clock text-primary text-xl mr-3"></i>
                <div class="flex-1">
                  <span class="text-sm text-color-secondary">Status</span>
                  <div class="flex flex-wrap align-items-center gap-2">
                    <p v-if="!isEditingStatus" class="font-bold m-0">
                      {{ toTitleCase(currentSolicitation.status.nome) }}
                    </p>
                    <Select
                      v-else
                      v-model="selectedStatus"
                      :options="statusOptions"
                      option-label="nome"
                      option-value="id"
                      class="w-full sm:w-13rem mt-1"
                      size="small"
                    />

                    <div v-if="isGestor" class="flex gap-2">
                      <Button
                        v-if="isEditingStatus"
                        icon="pi pi-times"
                        severity="danger"
                        text
                        @click="cancelChangeStatus"
                        v-tooltip.top="'Cancelar'"
                        size="small"
                      />
                      <Button
                        v-if="isEditingStatus"
                        icon="pi pi-check"
                        severity="success"
                        text
                        @click="handleStatusChange"
                        v-tooltip.top="'Salvar'"
                        size="small"
                      />
                    </div>
                  </div>
                </div>
                <Button
                  v-if="isGestor && !isEditingStatus"
                  icon="pi pi-pencil"
                  text
                  size="small"
                  severity="secondary"
                  @click="isEditingStatus = true"
                  v-tooltip="'Mudar Status'"
                />
              </li>
            </ul>
          </template>
        </Card>
      </div>

      <div v-if="solicitationContext.isGeneral" class="col-12 lg:col-4">
        <Card class="h-full">
          <template #title>
            <div class="flex align-items-center">
              <i class="pi pi-info-circle text-primary text-xl mr-3"></i>
              <span class="text-sm text-color-secondary">Justificativa Geral</span>
            </div>
          </template>
          <template #content>
            <em v-if="!isEditing" class="m-0">
              {{ currentSolicitation.justificativaGeral }}
            </em>
            <Textarea
              v-else
              id="textarea_label"
              class="w-full h-full"
              size="small"
              v-model="currentSolicitation.justificativaGeral"
              :invalid="currentSolicitation.justificativaGeral?.trim() === ''"
              style="resize: none"
              rows="3"
              :maxlength="500"
            />
          </template>
        </Card>
      </div>
    </div>

    <Tabs v-model:value="activeTab" class="mt-3">
      <TabList>
        <Tab value="0">Itens Solicitados</Tab>
        <Tab value="1">Insights</Tab>
        <Tab value="2">Histórico</Tab>
      </TabList>
      <TabPanels>
        <TabPanel value="0">
          <SolicitationList :is-editing="isEditing" />
        </TabPanel>
        <TabPanel value="1">
          <SolicitationAnalysis />
        </TabPanel>
        <TabPanel value="2">
          <SolicitationHistory :solicitation-id="currentSolicitation.id" />
        </TabPanel>
      </TabPanels>
    </Tabs>
  </div>
</template>

<style scoped>
.p-card {
  height: 100%;
}
</style>
