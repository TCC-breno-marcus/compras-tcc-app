<script setup lang="ts">
import { ref, computed } from 'vue'
import TabMenu from 'primevue/tabmenu'
import { useRoute, useRouter } from 'vue-router'

const router = useRouter()
const route = useRoute()

const items = ref([
  {
    label: 'Dashboard',
    icon: 'pi pi-chart-bar',
    command: () => router.push('/gestor/dashboard'),
    path: '/gestor/dashboard',
  },
  {
    label: 'Itens por Departamento',
    icon: 'pi pi-chart-pie',
    command: () => router.push('/gestor/departamento'),
    path: '/gestor/departamento',
  },
  {
    label: 'Solicitações',
    icon: 'pi pi-list',
    command: () => router.push('/gestor/solicitacoes'),
    path: '/gestor/solicitacoes',
  },
  {
    label: 'Gerenciar Catálogo',
    icon: 'pi pi-book',
    command: () => router.push('/gestor/catalogo'),
    path: '/gestor/catalogo',
  },
  {
    label: 'Relatórios',
    icon: 'pi pi-book',
    command: () => router.push('/gestor/relatorios'),
    path: '/gestor/relatorios',
  },
])

const activeRoute = computed(() => {
  return items.value.findIndex((item) => item.path === route.path)
})
</script>

<template>
  <div class="flex flex-column w-full h-full p-2">
    <div class="flex items-center justify-content-between">
      <h3>Painel do Gestor</h3>
    </div>
    <TabMenu :model="items" :activeIndex="activeRoute" />
    <RouterView class="mt-2" />
  </div>
</template>

<style scoped></style>
