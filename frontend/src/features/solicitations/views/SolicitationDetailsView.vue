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
import { useSettingStore } from '@/stores/settingStore'
import { formatDate } from '@/utils/dateUtils'
import SolicitationDetailsSkeleton from '../components/SolicitationDetailsSkeleton.vue'
import { useSolicitationStore } from '../stores/solicitationStore'
import CustomBreadcrumb from '@/components/ui/CustomBreadcrumb.vue'

const solicitationContext = reactive<SolicitationContext>({
  dialogMode: 'selection',
})

provide(SolicitationContextKey, readonly(solicitationContext))

const route = useRoute()
const authStore = useAuthStore()
const settingStore = useSettingStore()
const { deadline } = storeToRefs(settingStore)
const { user } = storeToRefs(authStore)
const solicitationStore = useSolicitationStore()
const { currentSolicitation, isLoading, error } = storeToRefs(solicitationStore)

const showDeadlineWarning = computed(() => {
  if (deadline.value) {
    return new Date() < deadline.value
  }
  return false
})

const isEditing = ref<boolean>(false)

const handleCancel = () => {
  const id = currentSolicitation.value?.id
  if (id && solicitationStore.isDirty) {
    console.log('oi')
    solicitationStore.fetchById(id)
  }
  isEditing.value = false
}

watch(
  () => route.params.id,
  (newId) => {
    if (newId && typeof newId === 'string') {
      solicitationStore.fetchById(parseInt(newId, 10))
    }
  },
  {
    immediate: true,
  },
)

onMounted(() => {
  settingStore.fetchPrazoSubmissao()
})
</script>

<!-- TODO: esse componente está grande. Tentar quebrar mais ele -->
<!-- TODO: UM SOLICITANTE SÓ PODE VISUALIZAR OS DETALHES DE SOLICITAÇÕES FEITAS PELO SEU DEPARTAMENTO-->
<!-- TODO: deve ter um botão para adicionar itens ao pedido. O botão irá abrir um modal que irá usar o componente CatalogoBrowser -->
<template>
  <SolicitationDetailsSkeleton v-if="isLoading" />
  <div class="p-2" v-if="currentSolicitation">
    <!-- TODO: ajustar responsividade dessa div abaixo  -->
    <div class="flex align-items-center justify-content-between mb-2">
      <CustomBreadcrumb />
    </div>
    <div class="flex align-items-center justify-content-between mb-2">
      <h3 class="m-0">Detalhes da Solicitação #{{ currentSolicitation.id }}</h3>
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

        <!-- TODO: só pode editar se a solicitação foi feita pelo próprio usuário logado -->
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
          @click="handleCancel"
        />
        <Button
          v-if="isEditing"
          icon="pi pi-save"
          label="Salvar"
          size="small"
          @click="isEditing = false"
          :disabled="!solicitationStore.isDirty"
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
                  <span class="text-sm text-color-secondary">Requisitante</span>
                  <p class="font-bold m-0">
                    {{ currentSolicitation.solicitante.nome }} ({{
                      currentSolicitation.solicitante.departamento
                    }})
                  </p>
                </div>
              </li>
              <li class="flex align-items-center">
                <i class="pi pi-envelope text-primary text-xl mr-3"></i>
                <div>
                  <span class="text-sm text-color-secondary">Contato</span>
                  <p class="font-bold m-0">{{ currentSolicitation.solicitante.email }}</p>
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
                  <span class="text-sm text-color-secondary">Data da Solicitação</span>
                  <p class="font-bold m-0">
                    {{ formatDate(currentSolicitation.dataCriacao, 'long') }}
                  </p>
                </div>
              </li>
              <li class="flex align-items-center">
                <i class="pi pi-envelope text-primary text-xl mr-3"></i>
                <div>
                  <span class="text-sm text-color-secondary">Tipo da Solicitação</span>
                  <p class="font-bold m-0">
                    {{ currentSolicitation.justificativaGeral ? 'Geral' : 'Patrimonial' }}
                  </p>
                </div>
              </li>
            </ul>
          </template>
        </Card>
      </div>

      <div v-if="currentSolicitation.justificativaGeral" class="col-12 lg:col-4">
        <Card class="h-full">
          <template #title>
            <div class="flex align-items-center">
              <i class="pi pi-info-circle text-primary text-xl mr-3"></i>
              <span class="text-sm text-color-secondary">Justificativa Geral</span>
            </div>
          </template>
          <template #content>
            <em class="m-0">
              {{ currentSolicitation.justificativaGeral }}
            </em>
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
