<script setup lang="ts">
import { ref, watch } from 'vue'
import Button from 'primevue/button'
import InputNumber from 'primevue/inputnumber'
import InputText from 'primevue/inputtext'
import { FloatLabel } from 'primevue'
import type { SolicitationItem } from '..'

const props = defineProps<{
  item: SolicitationItem
  isEditing: boolean
}>()

const emit = defineEmits(['removeItem', 'updateItem'])

const localItem = ref<SolicitationItem>({ ...props.item })

watch(
  () => props.item,
  (newItem) => {
    localItem.value = { ...newItem }
  },
  { deep: true },
)

const removeItem = () => {
  emit('removeItem', props.item.id)
}

const onFieldUpdate = () => {
  emit('updateItem', localItem.value)
}
</script>
<template>
  <div
    class="flex md:flex-column lg:flex-row justify-content-between lg:justify-content-between p-2 w-full gap-2"
    :class="item.justificativa ? '' : 'custom-border'"
  >
    <div class="flex justify-content-start align-items-center gap-2 mr-2">
      <div class="image-preview-container flex justify-content-center align-items-center">
        <img v-if="item.linkImagem" :src="item.linkImagem" :alt="item.nome" class="item-image" />
        <div v-else class="image-placeholder">
          <span class="material-symbols-outlined placeholder-icon"> hide_image </span>
        </div>
      </div>
      <div>
        <p class="font-bold text-sm">{{ item.nome }}</p>
        <p class="text-xs text-color-secondary">CATMAT {{ item.catMat }}</p>
      </div>
    </div>

    <div class="flex align-items-center gap-4 justify-content-end">
      <FloatLabel v-if="isEditing" variant="on" class="quantity-input w-full sm:w-3">
        <InputNumber
          v-model="item.quantidade"
          inputId="on_label_qtde"
          :min="1"
          :max="9999"
          size="small"
          fluid
          class="w-full"
          inputClass="w-full"
          @update:modelValue="onFieldUpdate"
        />
        <label for="on_label_qtde">Qtde.</label>
      </FloatLabel>
      <div v-else class="flex flex-column align-items-center text-center">
        <p class="text-color-secondary">Quantidade</p>
        <p class="font-bold">{{ item.quantidade }}</p>
      </div>

      <FloatLabel v-if="isEditing" variant="on" class="price-input">
        <InputNumber
          v-model="item.precoSugerido"
          inputId="on_label_price"
          mode="currency"
          currency="BRL"
          locale="pt-BR"
          fluid
          :min="0"
          size="small"
          @update:modelValue="onFieldUpdate"
        />
        <label for="on_label_price">Preço Unitário</label>
      </FloatLabel>
      <div v-else class="flex flex-column align-items-center text-center">
        <p class="text-color-secondary">Preço Unitário</p>
        <p class="font-bold">
          {{
            item.precoSugerido?.toLocaleString('pt-BR', {
              style: 'currency',
              currency: 'BRL',
            })
          }}
        </p>
      </div>
      <Button
        v-if="isEditing"
        icon="pi pi-trash"
        variant="text"
        severity="danger"
        size="small"
        v-tooltip="'Remover Item'"
        @click="removeItem"
      />
    </div>
  </div>
  <div
    v-if="item.justificativa"
    class="flex justify-content-end w-full pb-2 pl-7 md:pt-2 lg:pt-0"
    :class="item.justificativa ? 'custom-border' : ''"
  >
    <FloatLabel v-if="isEditing" variant="on" class="w-full">
      <InputText
        v-model="item.justificativa"
        inputId="on_label_justification"
        :invalid="item.justificativa.trim() === ''"
        size="small"
        class="w-full"
        inputClass="w-full"
        :maxlength="250"
        @update:modelValue="onFieldUpdate"
      />
      <label for="on_label_justification">Justificativa</label>
    </FloatLabel>
    <div v-else>
      <span class="text-color-secondary">Justificativa: </span>
      <em class="text-center">
        {{ item.justificativa }}
      </em>
    </div>
  </div>
</template>

<style scoped>
.custom-border {
  /* background-color: var(--p-surface-50); */
  border-bottom: 1px solid var(--p-surface-200);
}

.p-dark .custom-border {
  border-bottom: 1px solid var(--p-surface-800);
  background-color: initial;
}

.image-preview-container {
  position: relative;
  cursor: pointer;
  overflow: hidden;
  height: 48px;
  width: 48px;
  /* background-color: var(--p-surface-50); */
}

.p-dark .image-preview-container {
  background-color: initial;
}

.item-image {
  width: 100%;
  height: 100%;
  object-fit: contain;
  display: block;
  transition: transform 0.4s ease;
}

.placeholder-icon {
  font-size: 1.5rem;
  padding: 15px;
  /* color: var(--p-surface-500);  */
}

.quantity-input {
  width: 5rem !important;
}

.price-input {
  width: 7rem !important;
}
</style>
