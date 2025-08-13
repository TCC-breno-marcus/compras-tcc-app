<script setup lang="ts">
import { ref } from 'vue'
import ItemSolicitation from './ItemSolicitation.vue'

const props = defineProps<{
  isEditing: boolean
}>()

const items = ref([
  {
    id: 1,
    nome: 'Agulha Bacteriológica',
    descricao: 'Agulha bacteriológica em platina, sem cabo.',
    catMat: '408828',
    categoria: {
      id: 5,
      nome: 'Materiais de Laboratório',
      descricao:
        'Utensílios, consumíveis e pequenos equipamentos para uso geral em laboratório que não são vidrarias ou reagentes.',
      isActive: true,
    },
    linkImagem: 'http://localhost:8088/images/408828.png',
    especificacao: '5 cm',
    precoSugerido: 12,
    isActive: true,
    quantity: 2,
  },
  {
    id: 8,
    nome: 'Alonga',
    descricao: 'Alonga de borracha para usar em kitassato.',
    catMat: '421644',
    categoria: {
      id: 5,
      nome: 'Materiais de Laboratório',
      descricao:
        'Utensílios, consumíveis e pequenos equipamentos para uso geral em laboratório que não são vidrarias ou reagentes.',
      isActive: true,
    },
    linkImagem: 'http://localhost:8088/images/421644.png',
    especificacao: '15 x 40 mm',
    precoSugerido: 15,
    isActive: true,
    quantity: 6,
  },
  {
    id: 11,
    nome: 'Anel Com Mufa',
    descricao: 'Anel de ferro com mufa',
    catMat: '431837',
    categoria: {
      id: 5,
      nome: 'Materiais de Laboratório',
      descricao:
        'Utensílios, consumíveis e pequenos equipamentos para uso geral em laboratório que não são vidrarias ou reagentes.',
      isActive: true,
    },
    linkImagem: 'http://localhost:8088/images/431837.png',
    especificacao: '100 mm',
    precoSugerido: 90,
    isActive: true,
    quantity: 12,
  },
  {
    id: 15,
    nome: 'Balão Fundo Chato',
    descricao:
      'Balão de vidro com fundo chato, fabricado em vidro borossilicato 3.3. Altamente resistente a choques térmicos e ao calor, resistente à água, ácidos fortes, soluções alcalinas e salinas. Classe A.',
    catMat: '413186',
    categoria: {
      id: 5,
      nome: 'Materiais de Laboratório',
      descricao:
        'Utensílios, consumíveis e pequenos equipamentos para uso geral em laboratório que não são vidrarias ou reagentes.',
      isActive: true,
    },
    linkImagem: 'http://localhost:8088/images/413186.png',
    especificacao: '6000 mL',
    precoSugerido: 9,
    isActive: true,
    quantity: 14,
  },
  {
    id: 20,
    nome: 'Balão Fundo Redondo Com Junta E Gargalo Curto',
    descricao:
      'Balão de vidro com fundo redondo, com junta esmerilhada intercambiável 24/40 cônica e com gargalo curto. Fabricado com vidro borosilicato 3.3. Extremamente resistente à corrosão química, a choques térmicos e ao calor. Classe A.',
    catMat: '409262',
    categoria: {
      id: 5,
      nome: 'Materiais de Laboratório',
      descricao:
        'Utensílios, consumíveis e pequenos equipamentos para uso geral em laboratório que não são vidrarias ou reagentes.',
      isActive: true,
    },
    linkImagem: 'http://localhost:8088/images/409262.png',
    especificacao: '500 mL',
    precoSugerido: 10,
    isActive: true,
    quantity: 2,
  },
  {
    id: 22,
    nome: 'Balão Para Destilação',
    descricao:
      'Balão de vidro para destilação com fundo redondo, gargalo longo e saída lateral. Fabricado com vidro borosilicato 3.3. Extremamente resistente à corrosão química, a choques térmicos e ao calor. Classe A.',
    catMat: '409280',
    categoria: {
      id: 5,
      nome: 'Materiais de Laboratório',
      descricao:
        'Utensílios, consumíveis e pequenos equipamentos para uso geral em laboratório que não são vidrarias ou reagentes.',
      isActive: true,
    },
    linkImagem: 'http://localhost:8088/images/409280.png',
    especificacao: '250 mL',
    precoSugerido: 12,
    isActive: true,
    quantity: 7,
  },
  {
    id: 33,
    nome: 'Balão Volumétrico',
    descricao: 'Balão volumétrico de plástico com rolha de plástico.',
    catMat: '409413',
    categoria: {
      id: 5,
      nome: 'Materiais de Laboratório',
      descricao:
        'Utensílios, consumíveis e pequenos equipamentos para uso geral em laboratório que não são vidrarias ou reagentes.',
      isActive: true,
    },
    linkImagem: 'http://localhost:8088/images/409413.png',
    especificacao: '250 mL',
    precoSugerido: 14,
    isActive: true,
    quantity: 9,
  },
])
</script>

<template>
  <div class="flex flex-column justify-content-between w-full lg:w-6">
    <div class="items-list overflow-y-auto">
      <ItemSolicitation
        v-for="item in items"
        :key="item.catMat"
        :item="item"
        :is-editing="props.isEditing"
      />
    </div>
  </div>
</template>

<style scoped>
.items-list {
  max-height: 480px;
}
</style>
