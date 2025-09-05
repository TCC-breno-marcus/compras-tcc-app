<script setup lang="ts">
import { useAuthStore } from '@/features/autentication/stores/authStore'
import { useUserStore } from '@/features/users/stores/userStore'
import { storeToRefs } from 'pinia'
import Breadcrumb from 'primevue/breadcrumb'
import { ref, watch } from 'vue'
import { useRoute } from 'vue-router'

interface BreadcrumbItem {
  label: string
  route?: string
  isLast: boolean
}

const props = defineProps<{
  dynamicLabel?: string
}>()

const route = useRoute()
const authStore = useAuthStore()
const { isGestor } = storeToRefs(authStore)

const home = ref({
  icon: 'pi pi-home',
  route: '/',
})
const items = ref<BreadcrumbItem[]>([])

const routeNamesMap: { [key: string]: { label: string; route?: string } } = {
  gestor: { label: 'Painel do Gestor', route: '/gestor' },
  catalogo: { label: 'Gerenciar Catálogo', route: '/gestor/gerenciar-catalogo' },
  solicitacoes: { label: 'Solicitações', route: `${isGestor.value ? '/gestor' : ''}/solicitacoes` },
  criar: { label: 'Criar' },
  patrimonial: { label: 'Patrimonial', route: '/solicitacoes/criar/patrimonial' },
  geral: { label: 'Geral', route: '/solicitacoes/criar/geral' },
}

watch(
  [() => route.path, () => props.dynamicLabel],
  ([newPath, newLabel]) => {
    const pathSegments = newPath.split('/').filter((p) => p)
    items.value = pathSegments.map((segment, index) => {
      const isLast = index === pathSegments.length - 1
      const isDynamicSegment = !isNaN(Number(segment))

      if (isLast && isDynamicSegment && newLabel) {
        return {
          label: newLabel,
          route: undefined,
          isLast: true,
        }
      }

      const mapping = routeNamesMap[segment]
      return {
        label: mapping ? mapping.label : segment,
        route: mapping && mapping.route ? mapping.route : undefined,
        isLast,
      }
    })
  },
  { immediate: true },
)
</script>

<template>
  <div class="card flex justify-center">
    <Breadcrumb :home="home" :model="items">
      <template #item="{ item, props }">
        <router-link v-if="item.route" v-slot="{ href, navigate }" :to="item.route" custom>
          <a :href="href" v-bind="props.action" @click="navigate">
            <span :class="[item.icon, 'text-color']" />
            <span class="font-semibold" :class="item.isLast ? 'text-primary' : ''">{{
              item.label
            }}</span>
          </a>
        </router-link>
        <a v-else :href="item.url" :target="item.target" v-bind="props.action">
          <span class="font-semibold">{{ item.label }}</span>
        </a>
      </template>
    </Breadcrumb>
  </div>
</template>

<style scoped></style>
