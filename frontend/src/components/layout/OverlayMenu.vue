<script setup lang="ts">
import Drawer from 'primevue/drawer'
import { Button } from 'primevue'
import { ref, watch } from 'vue'
import { useMenuStore } from '@/stores/menu'
import Menu from 'primevue/menu'
import { useRoute } from 'vue-router'

const { itemsMenuOverlay } = useMenuStore()
const route = useRoute()

const visible = ref(false)

watch(
  () => route.path,
  () => {
    visible.value = false
  },
)
</script>

<template>
  <div>
    <Drawer
      v-model:visible="visible"
      header="Menu"
      class="w-min"
      :pt="{
        root: {
          style: {
            background: 'var(--p-surface-800)',
            color: 'var(--p-surface-100)',
            border: 0,
          },
        },
      }"
    >
      <div class="card flex justify-center text-sm" style="color: var(--p-text-color)">
        <Menu :model="itemsMenuOverlay" style="background-color: var(--p-surface-800); border: 0">
          <template #item="{ item, props }">
            <router-link v-if="item.route" v-slot="{ href, navigate }" :to="item.route" custom>
              <a v-ripple :href="href" v-bind="props.action" @click="navigate">
                <span v-if="item.icon" class="material-symbols-outlined">
                  {{ item.icon }}
                </span>
                <span>{{ item.label }}</span>
              </a>
            </router-link>
            <a v-else v-ripple :href="item.url" :target="item.target" v-bind="props.action">
              <span v-if="item.icon" class="material-symbols-outlined">
                {{ item.icon }}
              </span>
              <span>{{ item.label }}</span>
            </a>
          </template>
        </Menu>
      </div>
    </Drawer>
    <Button class="button-menu-overlay p-1" @click="visible = true" text>
      <template #icon>
        <span class="material-symbols-outlined">menu</span>
      </template>
    </Button>
  </div>
</template>

<style scoped>
.button-menu-overlay {
  color: var(--p-text-color);
}

:deep(.p-menu-item-link),
:deep(.p-menu-item-icon) {
  color: var(--p-surface-300);
}

:deep(.p-menu-submenu .p-menu-item-link),
:deep(.p-menu-submenu .p-menu-item-icon) {
  color: var(--p-surface-700);
}

:deep(.p-menu-item-content) {
  background-color: transparent !important;
  transition: background-color 0.15s ease;
}

:deep(.p-menu-submenu .p-menu-item-content) {
  background-color: transparent !important;
  transition: background-color 0.15s ease;
}

/* Hover states */
:deep(.p-menu-item-content:hover),
.menu-open-icon:hover {
  background-color: var(--p-surface-900) !important;
}

:deep(.p-menu-submenu .p-menu-item-content:hover) {
  background-color: var(--p-surface-100) !important;
}

/* Focus states */
:deep(.p-menu-item-content:focus),
.menu-open-icon:focus {
  background-color: var(--p-surface-900) !important;
}

:deep(.p-menu-submenu .p-menu-item-content:focus) {
  background-color: var(--p-surface-100) !important;
}

:deep(.p-menu-submenu-label) {
  color: var(--p-surface-400);
}
</style>
