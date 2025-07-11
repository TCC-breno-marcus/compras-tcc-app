<script setup lang="ts">
import { ref, computed } from 'vue'
import Divider from 'primevue/divider'
import Card from 'primevue/card'
import Tabs from 'primevue/tabs'
import TabList from 'primevue/tablist'
import Tab from 'primevue/tab'
import TabPanels from 'primevue/tabpanels'
import TabPanel from 'primevue/tabpanel'
import Popover from 'primevue/popover'
import Button from 'primevue/button'
import InputGroup from 'primevue/inputgroup'
import InputGroupAddon from 'primevue/inputgroupaddon'
import { InputText } from 'primevue'
import Message from 'primevue/message'
import ScrollPanel from 'primevue/scrollpanel'
import SolicitationList from '../components/SolicitationList.vue'

// Seus dados da solicitação
const solicitation = ref({
  id: 8,
  userRequest: 'Juliana Alves (DCOMP)',
  userContact: 'juliana.alves@email.com',
  date: 1752211200000,
  itemsQuantity: 6,
  totalPrice: 2100.0,
  justification: 'Materiais necessários para o novo laboratório de redes, conforme projeto anexo.',
})

// Helper para formatar a data
const formattedDate = new Date(solicitation.value.date).toLocaleString('pt-BR', {
  dateStyle: 'long',
  timeStyle: 'short',
})

// Helper para formatar o preço
const formattedPrice = new Intl.NumberFormat('pt-BR', {
  style: 'currency',
  currency: 'BRL',
}).format(solicitation.value.totalPrice)

const deadline = new Date('2025-07-31T23:59:59')
const showDeadlineWarning = computed(() => new Date() < deadline)
</script>

<template>
  <div class="p-2" v-if="solicitation">
    <div class="flex items-center justify-content-between mb-4">
      <h3 class="m-0">Detalhes da Solicitação #{{ solicitation.id }}</h3>
      <div class="flex gap-2">
        <Message v-if="showDeadlineWarning" icon="pi pi-info-circle" severity="warn" size="small" :closable="false">
          Prazo final para ajustes: 31/07/2025
        </Message>
        <Message v-else="showDeadlineWarning" icon="pi pi-info-circle" severity="warn" size="small" :closable="false">
          O prazo final para ajustes foi encerrado.
        </Message>

        <!-- EDITAR SOMENTE SE O USER LOGADO NÃO FOR GESTOR -->
        <Button icon="pi pi-pencil" label="Editar" size="small" />
      </div>
    </div>

    <div class="grid">
      <div class="col-12 md:col-4">
        <Card>
          <template #content>
            <ul class="list-none p-0 m-0">
              <li class="flex items-center mb-3">
                <i class="pi pi-user text-primary text-xl mr-3"></i>
                <div>
                  <span class="text-sm text-surface-500">Requisitante</span>
                  <p class="font-bold m-0">{{ solicitation.userRequest }}</p>
                </div>
              </li>
              <li class="flex items-center mb-3">
                <i class="pi pi-envelope text-primary text-xl mr-3"></i>
                <div>
                  <span class="text-sm text-surface-500">Contato</span>
                  <!-- <a :href="`mailto:${solicitation.userContact}`" class="font-bold m-0 no-underline text-color hover:text-primary-500">
                    {{ solicitation.userContact }}
                  </a> -->
                  <p class="font-bold m-0">{{ solicitation.userContact }}</p>
                </div>
              </li>
              <li class="flex items-center">
                <i class="pi pi-calendar text-primary text-xl mr-3"></i>
                <div>
                  <span class="text-sm text-surface-500">Data da Solicitação</span>
                  <p class="font-bold m-0">{{ formattedDate }}</p>
                </div>
              </li>
            </ul>
          </template>
        </Card>
      </div>

      <div class="col-12 md:col-4">
        <Card class="h-full">
          <template #content>
            <ul class="list-none p-0 m-0">
              <li class="flex items-center mb-3">
                <i class="pi pi-box text-primary text-xl mr-3"></i>
                <div>
                  <span class="text-sm text-surface-500">Quantidade de Itens</span>
                  <p class="font-bold m-0">{{ solicitation.itemsQuantity }}</p>
                </div>
              </li>
              <li class="flex items-center">
                <i class="pi pi-dollar text-primary text-xl mr-3"></i>
                <div>
                  <span class="text-sm text-surface-500">Valor Total Estimado</span>
                  <p class="font-bold m-0">{{ formattedPrice }}</p>
                </div>
              </li>
            </ul>
          </template>
        </Card>
      </div>

      <div class="col-12 md:col-4">
        <Card class="h-full">
          <template #content>
            <ul class="list-none p-0 m-0">
              <li class="flex items-center mb-3">
                <i class="pi pi-info-circle text-primary text-xl mr-3"></i>
                <div>
                  <span class="text-sm text-surface-500 font-bold">Justificativa</span>
                  <ScrollPanel
                    style="width: 100%; height: 100px"
                    :dt="{
                      bar: {
                        background: 'var(--p-surface-400)',
                      },
                    }"
                  >
                    <p class="m-0 mt-2">
                      {{ solicitation.justification }}
                    </p>
                  </ScrollPanel>
                </div>
              </li>
            </ul>
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
          <SolicitationList />
        </TabPanel>
        <TabPanel value="1">
          <p class="m-0">(Aqui entrarão os gráficos de análise)</p>
        </TabPanel>
      </TabPanels>
    </Tabs>
  </div>
</template>

<style scoped>
/* Estilos para garantir que o card ocupe 100% da altura da coluna no grid */
.p-card {
  height: 100%;
}

.p-dark .p-card {
  background-color: transparent;
}
</style>
