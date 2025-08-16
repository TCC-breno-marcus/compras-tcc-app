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
import type { SolicitationResult } from '..'
import { solicitationService } from '../services/solicitationService'
import { useSettingStore } from '@/stores/settingStore'
import { formatDate } from '@/utils/dateUtils'

const route = useRoute()
const authStore = useAuthStore()
const settingStore = useSettingStore()
const { deadline } = storeToRefs(settingStore)
const { user } = storeToRefs(authStore)
const solicitation = ref<SolicitationResult | null>(null)
const isLoading = ref(true)
const error = ref<string | null>(null)

const showDeadlineWarning = computed(() => {
  if (deadline.value) {
    return new Date() < deadline.value
  }
  return false
})

const solicitationContext = reactive<SolicitationContext>({
  dialogMode: '',
  isGeneral: true,
})

const isEditing = ref<boolean>(false)

const fetchSolicitation = async (id: number) => {
  isLoading.value = true
  error.value = null
  try {
    solicitation.value = await solicitationService.getById(id)
  } catch (err) {
    error.value = 'Falha ao carregar os detalhes da solicitação.'
    console.error(err)
  } finally {
    isLoading.value = false
  }
}

watch(
  () => route.params.id,
  (newId) => {
    if (newId && typeof newId === 'string') {
      fetchSolicitation(parseInt(newId, 10))
    }
  },
  {
    immediate: true,
  },
)

onMounted(() => {
  settingStore.fetchPrazoSubmissao()
})

provide(SolicitationContextKey, readonly(solicitationContext))
</script>

<!-- TODO: esse componente está grande. Tentar quebrar mais ele -->
<template>
  <div class="p-2" v-if="solicitation">
    <div class="flex align-items-center justify-content-between mb-4">
      <h3 class="m-0">Detalhes da Solicitação #{{ solicitation.id }}</h3>
      <div class="flex gap-2">
        <Message
          v-if="showDeadlineWarning"
          icon="pi pi-info-circle"
          severity="warn"
          size="small"
          :closable="false"
        >
          Prazo final para ajustes: {{ formatDate(deadline, 'short') }}
        </Message>
        <Message
          v-else="showDeadlineWarning"
          icon="pi pi-info-circle"
          severity="error"
          size="small"
          :closable="false"
        >
          O prazo final para ajustes foi encerrado.
        </Message>

        <!-- TODO: EDITAR SOMENTE SE O USER LOGADO É O MESMO PRÓPRIO SOLICITANTE -->
        <Button
          v-if="!isEditing"
          icon="pi pi-pencil"
          label="Editar"
          size="small"
          @click="isEditing = true"
        />
        <Button
          v-if="isEditing"
          icon="pi pi-pencil"
          text
          severity="danger"
          label="Cancelar"
          size="small"
          @click="isEditing = false"
        />
      </div>
    </div>

    <div class="grid">
      <div class="col-12 lg:col-4">
        <Card class="h-full">
          <template #content>
            <ul class="list-none p-0 m-0">
              <li class="flex align-items-center mb-4">
                <i class="pi pi-user text-primary text-xl mr-3"></i>
                <div>
                  <span class="text-sm text-surface-500">Requisitante</span>
                  <p class="font-bold m-0">{{ solicitation.solicitante.nome }}</p>
                  <!-- TODO: deve mostrar o departamento dele tbm -->
                </div>
              </li>
              <li class="flex align-items-center">
                <i class="pi pi-envelope text-primary text-xl mr-3"></i>
                <div>
                  <span class="text-sm text-surface-500">Contato</span>
                  <p class="font-bold m-0">{{ solicitation.solicitante.email }}</p>
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
                <i class="pi pi-calendar text-primary text-xl mr-3"></i>
                <div>
                  <span class="text-sm text-surface-500">Data da Solicitação</span>
                  <p class="font-bold m-0">{{ formatDate(solicitation.dataCriacao, 'long') }}</p>
                </div>
              </li>
              <li class="flex align-items-center">
                <i class="pi pi-envelope text-primary text-xl mr-3"></i>
                <div>
                  <span class="text-sm text-surface-500">Tipo da Solicitação</span>
                  <p class="font-bold m-0">
                    {{ solicitationContext.isGeneral ? 'Geral' : 'Patrimonial' }}
                  </p>
                </div>
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
              <span class="text-sm text-surface-500 font-bold">Justificativa</span>
            </div>
          </template>
          <template #content>
            <p class="m-0">
              {{ solicitation.justificativaGeral }}
            </p>
          </template>
        </Card>
      </div>
    </div>

    <Tabs value="0" class="mt-3">
      <TabList>
        <Tab value="0">Itens Solicitados</Tab>
        <Tab value="1">Análise da Solicitação</Tab>
      </TabList>
      <TabPanels>
        <TabPanel value="0">
          <SolicitationList :is-editing="isEditing" />
          <!-- TODO: deve agora pegar a lista de itens que já está no em solicitation
           Vai ser melhor colocar a solicitacao atual no store pra ser acessivel no filho -->
        </TabPanel>
        <TabPanel value="1">
          <SolicitationAnalysis />
        </TabPanel>
      </TabPanels>
    </Tabs>
  </div>
</template>

<style scoped>
.p-card {
  height: 100%;
}

.p-dark .p-card {
  background-color: transparent;
}
</style>
