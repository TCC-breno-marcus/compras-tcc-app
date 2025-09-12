<script setup lang="ts">
import { ref, computed, onMounted, nextTick } from 'vue'
import { useLayoutStore } from '@/stores/layout'
import { useMenuStore } from '@/stores/menu'
import TieredMenu from 'primevue/tieredmenu'
import Footer from './Footer.vue'

const layoutStore = useLayoutStore()
const isSidebarCollapsed = computed(() => layoutStore.isSidebarCollapsed)

const toggleSidebar = () => {
  layoutStore.toggleSidebar()
}

const { itemsMenu } = useMenuStore()
</script>

<template>
  <div
    class="sidebar-container flex flex-column align-items-center justify-content-between p-2 gap-2 h-screen"
    :class="{ 'sidebar-collapsed': isSidebarCollapsed }"
  >
    <div
      class="sidebar-container flex justify-center text-sm gap-1"
      :class="{ 'sidebar-collapsed': isSidebarCollapsed }"
    >
      <div
        class="menu-header flex align-items-center gap-1 px-2"
        :class="isSidebarCollapsed ? 'justify-content-center' : 'justify-content-start'"
      >
        <span
          class="menu-open-icon material-symbols-outlined px-2 py-1"
          :style="{
            transform: isSidebarCollapsed ? 'scaleX(-1)' : 'scaleX(1)',
          }"
          @click="toggleSidebar"
          v-tooltip="isSidebarCollapsed ? { value: 'Menu' } : null"
        >
          menu_open
        </span>
        <span v-if="!isSidebarCollapsed" class="menu-label text-sm">Menu</span>
      </div>

      <TieredMenu
        :model="itemsMenu"
        :pt="{
          root: {
            style: {
              'background-color': 'transparent',
              border: 'none',
            },
          },
        }"
      >
        <template #item="{ item, props, hasSubmenu }">
          <router-link v-if="item.route" v-slot="{ href, navigate }" :to="item.route" custom>
            <a v-ripple :href="href" v-bind="props.action" @click="navigate">
              <span v-if="item.materialIcon" class="material-symbols-outlined">
                {{ item.icon }}
              </span>
              <span v-else :class="item.icon" />

              <span class="tieredmenu-subitem-label">{{ item.label }}</span>
            </a>
          </router-link>
          <a
            v-else
            v-ripple
            :href="item.url"
            :target="item.target"
            v-bind="props.action"
            v-tooltip="isSidebarCollapsed ? { value: item.label } : null"
          >
            <span v-if="item.materialIcon" class="material-symbols-outlined">
              {{ item.icon }}
            </span>
            <span v-else :class="item.icon" />

            <span class="tieredmenu-item-label">{{ item.label }}</span>
            <span v-if="hasSubmenu" class="submenu-icon pi pi-angle-right ml-auto" />
          </a>
        </template>
      </TieredMenu>
    </div>

    <Footer :is-sidebar-collapsed="isSidebarCollapsed" />
  </div>
</template>

<style scoped>
.sidebar-container {
  background-color: var(--p-surface-900);
  /* background-color: #1B325F; */
  color: var(--p-surface-100);
  /* padding: 1rem 0; */
  transition: width 0.3s ease;
  display: flex;
  flex-direction: column;
  width: 220px;
  font-weight: 500;
}

.sidebar-container.sidebar-collapsed {
  width: 60px;
}

.menu-header {
  /* padding-left: 1.2rem !important; */
  font-weight: 600;
  color: var(--p-surface-300);
  white-space: nowrap;
  opacity: 1;
  transition: opacity 0.3s ease;
}

.menu-open-icon {
  color: var(--p-surface-300);
  cursor: pointer;
  transition: transform 0.3s ease;
  border-radius: 4px;
  font-size: 1.6rem;
}

.menu-header {
  position: relative;
}

.menu-header::after {
  content: '';
  position: absolute;
  bottom: 0;
  left: 0%;
  right: 0%;
  height: 1px;
  background: linear-gradient(to right, transparent 0%, var(--p-surface-500) 50%, transparent 100%);
}

.menu-header-collapsed {
  padding: 0;
}

.menu-header-collapsed .menu-label {
  display: none;
}

.tieredmenu-item-label {
  transition: opacity 0.3s ease;
}

.tieredmenu-item-label {
  transition: opacity 0.3s ease;
  white-space: nowrap;
}

.sidebar-collapsed .tieredmenu-item-label {
  display: none;
}

.menu-label {
  transition: opacity 0.3s ease;
  white-space: nowrap;
}

.sidebar-collapsed .menu-label {
  display: none;
}

:deep(.p-tieredmenu) {
  width: max-content;
  align-self: center;
}

:deep(.sidebar-collapsed .p-tieredmenu-submenu .tieredmenu-item-label) {
  display: flex;
}

/* Remove seta de submenu */
.sidebar-collapsed .submenu-icon {
  display: none;
}

/* Ajusta o menu principal quando colapsado */
:deep(.sidebar-collapsed .p-tieredmenu) {
  min-width: auto;
}

:deep(.p-tieredmenu-submenu) {
  width: max-content;
  z-index: 999;
}

.sidebar-container :deep(.submenu-title) a {
  font-weight: 700 !important;
}

.sidebar-collapsed :deep(.submenu-title) a {
  display: block;
}

:deep(.p-tieredmenu-item-link),
:deep(.p-tieredmenu-item-icon) {
  color: var(--p-surface-300);
}

:deep(.p-tieredmenu-submenu .p-tieredmenu-item-link),
:deep(.p-tieredmenu-submenu .p-tieredmenu-item-icon) {
  color: var(--p-text-color);
}

:deep(.p-tieredmenu-item-content) {
  background-color: transparent !important;
  transition: background-color 0.15s ease;
}

:deep(.p-tieredmenu-submenu .p-tieredmenu-item-content) {
  background-color: transparent !important;
  transition: background-color 0.15s ease;
}

/* Hover states */
:deep(.p-tieredmenu-item-content:hover),
.menu-open-icon:hover {
  background-color: var(--p-surface-800) !important;
}

:deep(.p-tieredmenu-submenu .p-tieredmenu-item-content:hover) {
  background-color: var(--p-surface-100) !important;
}

/* Focus states */
:deep(.p-tieredmenu-item-content:focus),
.menu-open-icon:focus {
  background-color: var(--p-surface-800) !important;
}

:deep(.p-tieredmenu-submenu .p-tieredmenu-item-content:focus) {
  background-color: var(--p-surface-100) !important;
}

:deep(.p-tieredmenu-separator) {
  border-block-start: 1px solid var(--p-surface-500);
}
</style>

<style>
html.p-dark .p-tieredmenu-submenu .p-tieredmenu-item-content:hover {
  background-color: var(--p-surface-700) !important;
}
</style>
