<script setup lang="ts">
import { ref, computed } from 'vue'
import TabMenu from 'primevue/tabmenu'
import { useRoute, useRouter } from 'vue-router'

const router = useRouter()
const route = useRoute()

const items = ref([
  {
    label: 'Geral',
    icon: 'pi pi-sliders-h',
    command: () => router.push('/configuracoes/geral'),
    path: '/configuracoes/geral',
  },
  {
    label: 'Solicitações',
    icon: 'pi pi-file-edit',
    command: () => router.push('/configuracoes/solicitacoes'),
    path: '/configuracoes/solicitacoes',
  },
  {
    label: 'Usuários e Permissões',
    icon: 'pi pi-users',
    command: () => router.push('/configuracoes/usuarios'),
    path: '/configuracoes/usuarios',
  },
  {
    label: 'Notificações',
    icon: 'pi pi-bell',
    command: () => router.push('/configuracoes/notificacoes'),
    path: '/configuracoes/notificacoes',
  },
])

const activeRoute = computed(() => {
  return items.value.findIndex((item) => item.path === route.path)
})
</script>

// TODO: terminar toda logica desse componente
<template>
  <div class="flex flex-column w-full h-full p-2">
    <div class="grid align-items-center">
      <h2 class="col text-start">Configurações do Sistema</h2>
    </div>
    <TabMenu :model="items" :activeIndex="activeRoute" />
    <RouterView class="mt-2" />
  </div>
</template>

<style scoped></style>
