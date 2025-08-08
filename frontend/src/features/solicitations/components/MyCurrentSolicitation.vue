<script setup lang="ts">
import { inject, ref } from 'vue'
import { Button } from 'primevue'
import ItemSolicitation from './ItemSolicitation.vue'
import { IftaLabel } from 'primevue'
import Textarea from 'primevue/textarea'
import { useSolicitationStore } from '../stores/solicitationStore'
import { storeToRefs } from 'pinia'
import { SolicitationContextKey } from '../keys'

const solicitationContext = inject(SolicitationContextKey)

const solicitationStore = useSolicitationStore()
const { solicitationItems, justification } = storeToRefs(solicitationStore)

const createSolicitation = () => {
  // TODO: implementar criação para chamar o backend
  const newSolicitation = {
    solicitationItems,
    justification,
  }
  try {
    // const response = solicitationStore.create(newSolicitation)
  } catch (error) {}
  console.log(newSolicitation)
}

</script>

<template>
  <div class="flex flex-column justify-content-between w-full h-full">
    <div v-if="solicitationItems.length > 0" class="items-list overflow-y-auto">
      <ItemSolicitation v-for="item in solicitationItems" :key="item.catMat" :item="item" />
    </div>
    <div v-else class="flex flex-column align-items-center text-center p-4">
      <i class="pi pi pi-box text-3xl text-color-secondary mb-2"></i>
      <span class="font-bold">Não há itens na sua solicitação.</span>
      <p class="text-color-secondary mt-2">
        Navegue pelo catálogo para encontrar e adicionar os materiais que você precisa.
      </p>
    </div>
    <div class="flex w-full justify-content-end mt-2 gap-2">
      <IftaLabel v-if="solicitationContext?.isGeneral" class="w-full">
        <Textarea
          id="textarea_label"
          class="w-full h-full"
          size="small"
          v-model="justification"
          style="resize: none"
        />
        <label for="textarea_label">Justificativa Geral</label>
      </IftaLabel>
      <div class="flex align-items-end">
        <Button label="Solicitar" icon="pi pi-send" size="small" @click="createSolicitation" />
      </div>
    </div>
  </div>
</template>

<style scoped>
.items-list {
  max-height: calc(100vh - 250px);
}
</style>
