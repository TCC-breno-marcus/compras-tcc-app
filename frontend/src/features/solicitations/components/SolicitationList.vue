<script setup lang="ts">
import { ref } from 'vue'
import { Button } from 'primevue'
import ItemSolicitation from './ItemSolicitation.vue'
import { IftaLabel } from 'primevue'
import Textarea from 'primevue/textarea'

const items = ref([
  {
    title: 'Seringa Descartável',
    img: '/items_img/img1.png',
    code: '214560',
    suggestedUnitPrice: 17.99,
  },
  {
    title: 'Gaze Estéril Pacote',
    img: '/items_img/img1.png',
    code: '348790',
    suggestedUnitPrice: 17.99,
  },
  {
    title: 'Luva Cirúrgica (Par)',
    img: '/items_img/img1.png',
    code: '551230',
    suggestedUnitPrice: 17.99,
  },
  {
    title: 'Álcool Etílico 70% 1L',
    img: '/items_img/img1.png',
    code: '987010',
    suggestedUnitPrice: 17.99,
  },
  {
    title: 'Seringa Descartável',
    img: '/items_img/img1.png',
    code: '214560',
    suggestedUnitPrice: 17.99,
  },
  {
    title: 'Gaze Estéril Pacote',
    img: '/items_img/img1.png',
    code: '348790',
    suggestedUnitPrice: 17.99,
  },
  {
    title: 'Luva Cirúrgica (Par)',
    img: '/items_img/img1.png',
    code: '551230',
    suggestedUnitPrice: 17.99,
  },
  {
    title: 'Álcool Etílico 70% 1L',
    img: '/items_img/img1.png',
    code: '987010',
    suggestedUnitPrice: 17.99,
  },
])

const justification = ref('')
</script>

<template>
  <div class="flex flex-column justify-content-between w-full lg:w-6">
    <div class="items-list overflow-y-auto">
      <ItemSolicitation v-for="item in items" :key="item.code" :item="item" />
    </div>

    <!-- <div class="flex w-full justify-content-between mt-2 gap-2">
      <div class="flex align-items-end">
        <Button label="Solicitar" icon="pi pi-send" size="small" />
      </div>
    </div> -->
  </div>
</template>

<style scoped>
.items-list {
  max-height: 450px;
}
</style>
