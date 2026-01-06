<script setup lang="ts">
import Timeline from 'primevue/timeline'
import Card from 'primevue/card'
import Tag from 'primevue/tag'
import { computed, onMounted } from 'vue'
import { formatDate } from '@/utils/dateUtils'
import { storeToRefs } from 'pinia'
import { useSolicitationHistoryStore } from '../stores/historySolicitationStore'

const props = defineProps<{
  solicitationId: number
}>()

const historyStore = useSolicitationHistoryStore()
const { solicitationHistory, isLoading } = storeToRefs(historyStore)

onMounted(() => {
  historyStore.fetchSolicitationHistory(props.solicitationId)
})

const mapAcaoToDisplay: { [key: string]: string } = {
  edicao: 'Edição',
  criacao: 'Criação',
  mudancadestatus: 'Mudança de Status',
  ajuste: 'Ajuste',
  aprovada: 'Aprovada',
  cancelamento: 'Cancelamento',
  arquivamento: 'Arquivamento',
  remocao: 'Remoção',
}

const processedHistory = computed(() => {
  return solicitationHistory.value.map((event) => {
    const detailsList = event.detalhes
      ? event.detalhes
          .split('|')
          .map((d) => d.trim())
          .filter((d) => d)
      : []

    let icon = 'pi pi-info-circle'
    let color = 'var(--blue-500)'
    let severity = 'info'

    if (
      event.acao.toLowerCase() === 'edicao' ||
      event.acao.toLowerCase() == 'mudancadestatus' ||
      event.acao.toLowerCase() == 'ajuste'
    ) {
      icon = 'pi pi-pencil'
      color = 'var(--orange-500)'
      severity = 'warn'
    } else if (event.acao.toLowerCase() === 'criacao') {
      icon = 'pi pi-plus'
      color = 'var(--green-500)'
      severity = 'success'
    } else if (event.acao.toLowerCase() === 'aprovada') {
      icon = 'pi pi-check'
      color = 'var(--green-500)'
      severity = 'success'
    } else if (
      event.acao.toLowerCase() === 'cancelamento' ||
      event.acao.toLowerCase() === 'arquivamento' ||
      event.acao.toLowerCase() === 'remocao'
    ) {
      icon = 'pi pi-times'
      color = 'var(--red-500)'
      severity = 'danger'
    }

    return {
      ...event,
      detailsList,
      icon,
      color,
      severity,
    }
  })
})

const formatDetailText = (text: string) => {
  return text.replace(/'([^']*)'/g, '<span class="font-bold">$1</span>')
}
</script>

<template>
  <div class="history-container justify-content-start">
    <div v-if="processedHistory.length > 0">
      <Timeline :value="processedHistory">
        <template #content="slotProps">
          <Card class="mt-3">
            <template #title>
              <Tag
                :value="mapAcaoToDisplay[slotProps.item.acao.toLowerCase()] || slotProps.item.acao"
                :icon="slotProps.item.icon"
                :severity="slotProps.item.severity"
                size="small"
                rounded
              ></Tag>
              <span class="text-sm text-color-secondary">
                por <strong>{{ slotProps.item.nomePessoa }}</strong>
              </span>
            </template>
            <template #subtitle>
              <small class="text-xs text-color-secondary font-medium uppercase">
                {{ formatDate(slotProps.item.dataOcorrencia, 'long') }}
              </small>
            </template>
            <template #content>
              <div v-if="slotProps.item.detailsList.length > 0">
                <p class="font-semibold text-sm mt-0 mb-2">Alterações realizadas:</p>
                <ul class="list-none p-0 m-0 p-3 surface-50 border-round">
                  <li
                    v-for="(detail, index) in slotProps.item.detailsList"
                    :key="index"
                    class="flex text-sm mb-1 align-items-end"
                  >
                    <i class="pi pi-chevron-right mr-2 mt-1 text-color-secondary text-xs"></i>
                    <p v-html="formatDetailText(detail)"></p>
                    <span
                      v-if="slotProps.item.observacoes"
                      class="text-xs text-color-secondary ml-2 "
                    >
                      <i class="pi pi-info-circle text-color-secondary text-xs"></i>
                      Observações: {{ slotProps.item.observacoes }}</span
                    >
                  </li>
                </ul>
              </div>
              <p
                v-else
                class="text-sm text-color-secondary m-0 font-italic text-center p-3 surface-50 border-round"
              >
                Nenhum detalhe adicional para esta ação.
              </p>
            </template>
          </Card>
        </template>
      </Timeline>
    </div>
    <div v-else class="text-center p-4">
      <div v-if="isLoading">
        <p>Carregando histórico...</p>
      </div>
      <p v-else class="text-color-secondary">
        Nenhum histórico de alterações encontrado para esta solicitação.
      </p>
    </div>
  </div>
</template>

<style scoped>
.history-container {
  width: 100%;
  height: 520px;
  overflow-y: auto;
}

:deep(.p-timeline-event-opposite) {
  display: none !important;
}
</style>
