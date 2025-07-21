<script setup>
import { computed, ref, watch } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import Button from 'primevue/button'
import Select from 'primevue/select'

const props = defineProps({
  currentUrl: {
    type: String,
    required: true,
  },
  totalCount: {
    type: Number,
    required: true,
  },
  hasNextPage: {
    type: Boolean,
    required: true,
  },
  pageSize: {
    type: Number,
    required: true,
  },
  pageNumber: {
    type: Number,
    required: true,
  },
})

const itemInicial = computed(() => (props.pageNumber - 1) * props.pageSize + 1)
const itemFinal = computed(() => Math.min(props.pageNumber * props.pageSize, props.totalCount))

const router = useRouter()
const route = useRoute()

const currentPage = computed(() => {
  return Number(route.query.pageNumber) || 1
})

const totalPages = computed(() => {
  const qtdPages = props.totalCount / props.pageSize
  return qtdPages % 1 !== 0 ? Math.floor(qtdPages) + 1 : qtdPages
})

const handleNavigation = (action) => {
  const actionMap = {
    first: 1,
    last: totalPages.value,
    next: Math.min(currentPage.value + 1, totalPages.value),
    previous: Math.max(currentPage.value - 1, 1),
  }

  const page = typeof action === 'string' ? actionMap[action] : action

  // 1. Comece com uma cópia de todos os parâmetros de query existentes
  const newQuery = { ...route.query }

  // 2. Atualize apenas o parâmetro da página
  newQuery.pageNumber = page.toString()
  newQuery.pageSize = itensPorPagina.value.code

  // 3. Envie o objeto de query diretamente para o router
  router.push({ query: newQuery })
}

const visiblePages = computed(() => {
  const links = []
  const maxVisiblePages = 5
  let startPage = Math.max(1, currentPage.value - Math.floor(maxVisiblePages / 2))
  let endPage = Math.min(totalPages.value, startPage + maxVisiblePages - 1)

  if (endPage - startPage < maxVisiblePages - 1) {
    startPage = Math.max(1, endPage - maxVisiblePages + 1)
  }

  for (let i = startPage; i <= endPage; i++) {
    links.push(i)
  }
  return links
})

const itensPorPagina = ref({ name: props.pageSize.toString(), code: props.pageSize.toString() })
const opcoesItens = ref([
  { name: '10', code: '10' },
  { name: '20', code: '20' },
  { name: '30', code: '30' },
  { name: '40', code: '40' },
  { name: '50', code: '50' },
])

watch(itensPorPagina, () => {
  handleNavigation(1) // Voltar para primeira página
})
</script>

<template>
  <div class="flex align-items-center justify-content-between w-full">
    <div class="caption-list">
      Exibindo {{ itemInicial }} - {{ itemFinal }} de {{ totalCount }} resultados
    </div>

    <div class="flex align-items-center justify-content-center gap-1 mt-2">
      <Button
        :pt="{
          root: {
            class: 'buttonNavigation',
          },
        }"
        type="button"
        rounded
        icon="pi pi-angle-double-left"
        @click="handleNavigation('first')"
        :disabled="currentPage === 1"
        text
        size="small"
      />
      <Button
        :pt="{
          root: {
            class: 'buttonNavigation',
          },
        }"
        type="button"
        rounded
        icon="pi pi-angle-left"
        @click="handleNavigation('previous')"
        :disabled="currentPage === 1"
        text
        size="small"
      />

      <Button
        v-for="pageNumber in visiblePages"
        :key="`page-button-${pageNumber}`"
        :pt="{
          root: {
            class: ['buttonNavigation', { buttonNavigationCurrent: pageNumber === currentPage }],
          },
        }"
        type="button"
        :label="pageNumber.toString()"
        rounded
        @click="handleNavigation(pageNumber)"
        text
        size="small"
      />

      <Button
        :pt="{
          root: {
            class: 'buttonNavigation',
          },
        }"
        type="button"
        rounded
        icon="pi pi-angle-right"
        @click="handleNavigation('next')"
        :disabled="!props.hasNextPage"
        text
        size="small"
      />
      <Button
        :pt="{
          root: {
            class: 'buttonNavigation',
          },
        }"
        type="button"
        rounded
        icon="pi pi-angle-double-right"
        @click="handleNavigation('last')"
        :disabled="!props.hasNextPage"
        text
        size="small"
      />
    </div>

    <div class="caption-list">
      Itens por página:
      <Select
        v-model="itensPorPagina"
        :options="opcoesItens"
        optionLabel="name"
        placeholder="Itens"
        size="small"
      />
    </div>
  </div>
</template>

<style scoped>
.buttonNavigation {
  color: var(--p-surface-400) !important;
  width: 40px !important;
  height: 40px !important;
}

.buttonNavigationCurrent {
  background-color: var(--p-button-text-primary-hover-background) !important;
  color: var(--p-surface-500) !important;
}

.caption-list {
  color: var(--p-surface-500);
}
</style>
