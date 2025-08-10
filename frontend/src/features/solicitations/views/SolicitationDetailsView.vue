<script setup lang="ts">
import { ref, computed, reactive, provide, readonly } from 'vue'
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


const solicitation = ref({
  id: 8,
  userRequest: 'Juliana Alves (DCOMP)',
  userContact: 'juliana.alves@email.com',
  date: 1752211200000,
  itemsQuantity: 6,
  totalPrice: 2100.0,
  justification: 'Materiais necessários para o novo laboratório de redes, conforme projeto anexo.',
})

const formattedDate = new Date(solicitation.value.date).toLocaleString('pt-BR', {
  dateStyle: 'long',
  timeStyle: 'short',
})

const deadline = new Date('2025-08-31T23:59:59')
const showDeadlineWarning = computed(() => new Date() < deadline)

const solicitationContext = reactive<SolicitationContext>({
  dialogMode: '',
  isGeneral: true,
})

const isEditing = ref<boolean>(false)

const authStore = useAuthStore()
const { user } = storeToRefs(authStore)

provide(SolicitationContextKey, readonly(solicitationContext))
</script>

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
          Prazo final para ajustes: 31/08/2025
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
                  <p class="font-bold m-0">{{ solicitation.userRequest }}</p>
                </div>
              </li>
              <li class="flex align-items-center">
                <i class="pi pi-envelope text-primary text-xl mr-3"></i>
                <div>
                  <span class="text-sm text-surface-500">Contato</span>
                  <p class="font-bold m-0">{{ solicitation.userContact }}</p>
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
                  <p class="font-bold m-0">{{ formattedDate }}</p>
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
              {{ solicitation.justification }}
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
