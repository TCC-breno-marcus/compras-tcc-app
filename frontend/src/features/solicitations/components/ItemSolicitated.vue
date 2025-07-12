<script setup lang="ts">
import { ref } from 'vue';
import Button from 'primevue/button';
import InputNumber from 'primevue/inputnumber';
import { FloatLabel } from 'primevue';

const props = defineProps({
  item: {
    type: Object,
    required: true
  }
});

const qtde = ref(1);

</script>
<template>
  <div class="item-card flex justify-content-between p-2 mb-2 w-full">

    <div class="flex justify-content-start align-items-center gap-2 mr-2">
      <img :src="item.img" :alt="item.title" class="item-image" />
      <div>
        <p class="font-bold text-sm">{{ item.title }}</p>
        <p class="text-xs text-color-secondary">CATMAT {{ item.code }}</p>
      </div>
    </div>

    <div class="flex align-items-center gap-2">
      <FloatLabel variant="on" class="quantity-input" >
        <InputNumber v-model="qtde" inputId="on_label_qtde" :min="1" :max="9999"
          size="small" fluid />
        <label for="on_label_qtde">Qtde.</label>
      </FloatLabel>
      <FloatLabel variant="on" class="price-input">
        <InputNumber v-model="item.suggestedUnitPrice" inputId="on_label_price" mode="currency" currency="BRL"
          locale="pt-BR" fluid :min="0" size="small"  />
        <label for="on_label_price">Preço Unitário</label>
      </FloatLabel>
      <Button icon="pi pi-trash" variant="text" severity="danger" size="small"/>
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

.quantity-input {
  width: 5rem;
}

.price-input {
  width: 7rem !important;
}
</style>