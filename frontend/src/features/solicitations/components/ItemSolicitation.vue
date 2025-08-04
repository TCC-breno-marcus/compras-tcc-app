<script setup lang="ts">
import { ref } from 'vue'
import Button from 'primevue/button'
import InputNumber from 'primevue/inputnumber'
import { FloatLabel } from 'primevue'
import type { SolicitationItem } from '../types'
import { useSolicitationStore } from '../stores/solicitationStore'
import { useToast } from 'primevue/usetoast'

const props = defineProps<{
  item: SolicitationItem
}>()

const solicitationStore = useSolicitationStore()
const toast = useToast()

const removeItem = () => {
  solicitationStore.removeItem(props.item.id)
  toast.add({
    severity: 'warn',
    summary: 'Removido',
    detail: `Item ${props.item.nome} (${props.item.catMat}) removido da solicitação.`,
    life: 3000,
  })
}
</script>
<template>
  <div
    class="item-card flex md:flex-column lg:flex-row align-items-center justify-content-between lg:justify-content-between p-2 mb-2 w-full gap-2"
  >
    <div class="flex justify-content-start align-items-center gap-2 mr-2">
      <img v-if="item.linkImagem" :src="item.linkImagem" :alt="item.nome" class="item-image" />
      <div v-else class="image-placeholder">
        <span class="material-symbols-outlined placeholder-icon"> hide_image </span>
      </div>
      <div>
        <p class="font-bold text-sm">{{ item.nome }}</p>
        <p class="text-xs text-color-secondary">CATMAT {{ item.catMat }}</p>
      </div>
    </div>

    <div class="flex align-items-center gap-2 justify-content-end">
      <FloatLabel variant="on" class="quantity-input w-full sm:w-3">
        <InputNumber
          v-model="item.quantity"
          inputId="on_label_qtde"
          :min="1"
          :max="9999"
          size="small"
          fluid
          class="w-full"
          inputClass="w-full"
        />
        <label for="on_label_qtde">Qtde.</label>
      </FloatLabel>
      <FloatLabel variant="on" class="price-input">
        <InputNumber
          v-model="item.precoSugerido"
          inputId="on_label_price"
          mode="currency"
          currency="BRL"
          locale="pt-BR"
          fluid
          :min="0"
          size="small"
        />
        <label for="on_label_price">Preço Unitário</label>
      </FloatLabel>
      <Button
        icon="pi pi-trash"
        variant="text"
        severity="danger"
        size="small"
        @click="removeItem"
      />
    </div>
  </div>
</template>

<style scoped>
.item-card {
  /* background-color: var(--p-surface-50); */
  border-bottom: 1px solid var(--p-surface-200);
}

.p-dark .item-card {
  border-bottom: 1px solid var(--p-surface-800);
  background-color: initial;
}

.item-image {
  max-width: 2rem;
  object-fit: cover;
  display: block;
  transition: transform 0.4s ease;
}

.image-placeholder {
  max-width: 2rem;

  height: 100%;
  display: flex;
  justify-content: center;
  align-items: center;
  border-radius: 6px;
}

.placeholder-icon {
  font-size: 2rem;
  /* color: var(--p-surface-500);  */
}

.quantity-input {
  width: 5rem;
}

.price-input {
  width: 7rem !important;
}
</style>
