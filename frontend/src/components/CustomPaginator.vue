<script setup>
import { computed } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import Button from 'primevue/button'

const props = defineProps({
  currentUrl: {
    type: String,
    required: true,
  },
  totalRecords: {
    type: Number,
    required: true,
  },
  hasNextPage: {
    type: Boolean,
    required: true,
  },
})

const router = useRouter()
const route = useRoute()

const currentPage = computed(() => {
  return Number(route.query.pageNumber) || 1
})

const MAX_PAGE_SIZE = 50

const totalPages = computed(() => {
  const qtdPages = props.totalRecords / MAX_PAGE_SIZE
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
</script>

<template>
  <div class="flex align-items-center gap-1">
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
</style>
