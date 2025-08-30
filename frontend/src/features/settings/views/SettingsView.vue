<script setup lang="ts">
import { ref, computed, provide } from 'vue'
import TabMenu from 'primevue/tabmenu'
import { useRoute, useRouter } from 'vue-router'
import { Button } from 'primevue'
import { SettingsContextKey } from '../settings.keys'

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

const isEditing = ref(false)
const childActions = ref({
  save: async () => console.warn('Nenhuma ação de salvar registrada.'),
  cancel: () => console.warn('Nenhuma ação de cancelar registrada.'),
})

const handleSave = async () => {
  await childActions.value.save()
  isEditing.value = false
}

const handleCancel = () => {
  childActions.value.cancel()
  isEditing.value = false
}

const registerActions = (actions: { save: () => Promise<void>; cancel: () => void }) => {
  childActions.value = actions
}

provide(SettingsContextKey, {
  isEditing,
  registerActions,
})
</script>

<template>
  <div class="flex flex-column w-full h-full p-2">
    <div class="grid align-items-center">
      <i class="pi pi-cog"></i>
      <h2 class="col-12 md:col text-start">Configurações do Sistema</h2>
      <div>
        <Button
          v-if="!isEditing"
          label="Editar"
          icon="pi pi-pencil"
          size="small"
          @click="isEditing = true"
        />
        <div v-else class="flex gap-2">
          <Button label="Cancelar" severity="secondary" size="small" @click="handleCancel" text />
          <Button label="Salvar Alterações" icon="pi pi-check" size="small" @click="handleSave" />
        </div>
      </div>
    </div>
    <TabMenu :model="items" :activeIndex="activeRoute" />
    <RouterView class="mt-2" />
  </div>
</template>

<style scoped></style>
