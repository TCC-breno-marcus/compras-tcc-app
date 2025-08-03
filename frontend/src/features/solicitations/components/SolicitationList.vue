<script setup lang="ts">
import { ref } from 'vue'
import { Button } from 'primevue'
import ItemSolicitation from './ItemSolicitation.vue'
import { IftaLabel } from 'primevue'
import Textarea from 'primevue/textarea'

const items = ref([
  {
    title: 'ALCOÔMETRO',
    img: '/items_img/408548.png',
    code: '408548',
    suggestedUnitPrice: 17.99,
  },
  {
    title: 'ANEL COM MUFA',
    img: '/items_img/421658.png',
    code: '421658',
    suggestedUnitPrice: 17.99,
  },
  {
    title: 'BALÃO FUNDO CHATO',
    img: '/items_img/409255.png',
    code: '409255',
    suggestedUnitPrice: 17.99,
  },
  {
    title: 'BALÃO VOLUMÉTRICO',
    img: '/items_img/409415.png',
    code: '409415',
    suggestedUnitPrice: 17.99,
  },
  {
    title: 'FRASCO DE VIDRO ÂMBAR',
    img: '/items_img/409441.png',
    code: '409441',
    suggestedUnitPrice: 17.99,
  },
  {
    title: 'FUNIL ANALÍTICO LISO HASTE CURTA',
    img: '/items_img/410110.png',
    code: '410110',
    suggestedUnitPrice: 17.99,
  },
  {
    title: 'GARRA PARA CONDENSADOR',
    img: '/items_img/426234.png',
    code: '426234',
    suggestedUnitPrice: 17.99,
  },
  {
    title: 'GRAL COM PISTILO',
    img: '/items_img/421491.png',
    code: '421491',
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
  max-height: 480px;
}
</style>
