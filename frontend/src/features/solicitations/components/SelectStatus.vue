<script setup lang="ts">
import { Button, Divider, FloatLabel, Message, Popover, Textarea } from 'primevue'
import { ref, computed } from 'vue'

interface Status {
  id: number
  nome: string
  severity: string
  icon: string
}

const props = defineProps<{
  currentStatusId: number
}>()

const emit = defineEmits(['status-change'])

const op = ref()
const selectedStatusId = ref(0)
const observation = ref('')
const showMessageError = ref(false)
const status = ref([
  { id: 1, nome: 'Pendente', severity: 'warning', icon: 'pi-clock' },
  { id: 2, nome: 'Aguardando Ajustes', severity: 'info', icon: 'pi-exclamation-triangle' },
  { id: 3, nome: 'Aprovada', severity: 'success', icon: 'pi-check' },
  { id: 4, nome: 'Rejeitada', severity: 'danger', icon: 'pi-times' },
  { id: 5, nome: 'Cancelada', severity: 'secondary', icon: 'pi-ban' },
])

const availableStatus = computed(() => status.value.filter((s) => s.id !== props.currentStatusId))

const toggle = (event: Event) => {
  op.value.toggle(event)
  showMessageError.value = false
  observation.value = ''
}

const selectStatus = (newStatus: Status) => {
  selectedStatusId.value = newStatus.id

  if (observationIsInvalid()) {
    showMessageError.value = true
    return
  }

  emit('status-change', newStatus.id, observation.value)
  op.value.hide()
}

const observationIsInvalid = () => {
  if (observation.value.trim() !== '') {
    return false
  }

  if (observation.value.trim() === '' && selectedStatusId.value === 3) {
    return false
  }

  return true
}
</script>

<template>
  <div class="flex align-items-center gap-2">
    <Button
      icon="pi pi-pencil"
      text
      size="small"
      @click="toggle"
      :disabled="currentStatusId === 5 || currentStatusId === 6"
      v-tooltip.left="
        currentStatusId === 5 || currentStatusId === 6 ? 'Solicitação em status irreversível (ação desativada)' : 'Alterar status'
      "
    />

    <Popover ref="op" class="w-64">
      <div class="pt-2">
        <div class="px-3 pb-2">
          <span class="font-medium text-surface-700">Alterar Status</span>
        </div>

        <Divider class="m-0" />

        <div class="p-2">
          <ul class="list-none p-0 m-0">
            <li
              v-for="statusOption in availableStatus"
              :key="statusOption.id"
              class="status-option flex align-items-center gap-2 p-2 cursor-pointer rounded-md transition-colors"
              @click="selectStatus(statusOption)"
            >
              <i
                :class="`pi ${statusOption.icon} text-${statusOption.severity === 'warning' ? 'orange' : statusOption.severity === 'info' ? 'blue' : statusOption.severity === 'success' ? 'green' : statusOption.severity === 'danger' ? 'red' : 'gray'}-500`"
              ></i>
              <div class="flex-1">
                <span class="font-medium">{{ statusOption.nome }}</span>
              </div>
            </li>
          </ul>
          <FloatLabel variant="on" class="w-full my-2">
            <Textarea
              id="observation"
              v-model="observation"
              size="small"
              class="w-full"
              :maxlength="500"
              autoResize
            />
            <label for="observation">Justificativa</label>
          </FloatLabel>
          <Message
            v-if="showMessageError"
            class="ml-1 text-sm"
            severity="error"
            size="small"
            variant="simple"
          >
            A justificativa para essa ação é obrigatória.
          </Message>
        </div>
      </div>
    </Popover>
  </div>
</template>

<style scoped>
.status-option:hover {
  background-color: var(--p-primary-50);
}

.p-dark .status-option:hover {
  background-color: var(--p-primary-900);
}
</style>
