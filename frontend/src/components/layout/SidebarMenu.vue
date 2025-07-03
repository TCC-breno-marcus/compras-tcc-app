<script setup lang="ts">
import { ref, computed, onMounted, nextTick } from 'vue';
import { useLayoutStore } from '@/stores/layout';
import { useRouter } from 'vue-router';
import TieredMenu from 'primevue/tieredmenu';

const layoutStore = useLayoutStore();
const isSidebarCollapsed = computed(() => layoutStore.isSidebarCollapsed);

const router = useRouter();

const items = ref([
    {
        label: 'Solicitações',
        icon: 'pi pi-shopping-cart',
        items: [
            {
                label: 'Solicitações',
                class: 'submenu-title',
                disabled: true
            },
            {
                label: 'Nova Solicitação',
                icon: 'pi pi-cart-plus',
                route: '/solicitacoes/criar'
            },
            {
                label: 'Minhas Solicitações',
                icon: 'pi pi-list',
                route: '/solicitacoes/minhas'
            }
        ]
    },
    {
        label: 'Painel do Gestor',
        icon: 'pi pi-th-large',
        command: () => {
            router.push('/painel-gestor');
        }
    },
    {
        separator: true
    },
    {
        label: 'Fale Conosco',
        icon: 'pi pi-envelope',
        command: () => {
            router.push('/fale-conosco');
        }
    }
]);

</script>

<template>
    <div class="sidebar-container flex flex-column align-items-center p-2 gap-2 h-screen">
        <h2>
            {{ isSidebarCollapsed ? 'LG' : 'LOGO' }}
        </h2>
        <div class="sidebar-container flex justify-center text-sm" :class="{ 'sidebar-collapsed': isSidebarCollapsed }">
            <div class="menu-header" :class="{ 'menu-header-collapsed': isSidebarCollapsed }">
                <span class="menu-label ">MENU</span>
            </div>
            <TieredMenu :model="items" :pt="{
                root: {
                    style: {
                        'background-color': 'transparent',
                        'border': 'none'
                    }
                }
            }">

                <template #item="{ item, props, hasSubmenu }">
                    <router-link v-if="item.route" v-slot="{ href, navigate }" :to="item.route" custom>
                        <a v-ripple :href="href" v-bind="props.action" @click="navigate">
                            <span :class="item.icon" />
                            <span class="tieredmenu-subitem-label ml-2">{{ item.label }}</span>
                        </a>
                    </router-link>
                    <a v-else v-ripple :href="item.url" :target="item.target" v-bind="props.action"
                        v-tooltip="isSidebarCollapsed ? { value: item.label } : null">
                        <span :class="item.icon" />
                        <span class="tieredmenu-item-label ml-2">{{ item.label }}</span>
                        <span v-if="hasSubmenu" class="submenu-icon pi pi-angle-right ml-auto" />
                    </a>
                </template>

            </TieredMenu>
        </div>
    </div>
</template>

<style scoped>
.sidebar-container {
    background-color: var(--p-surface-800);
    /* background-color: #1B325F; */
    color: var(--p-surface-100);
    padding: 1rem 0;
    transition: width 0.3s ease;
    display: flex;
    flex-direction: column;
}

.menu-header {
    padding: 0 1.5rem 1rem 1.5rem;
    font-size: 0.8rem;
    font-weight: 600;
    color: var(--p-surface-300);
    white-space: nowrap;
    opacity: 1;
    transition: opacity 0.3s ease;
}

.menu-header-collapsed {
    padding: 0;
}

.menu-header-collapsed .menu-label {
    display: none;
}

.sidebar-collapsed .tieredmenu-item-label {
    display: none;
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
}


.sidebar-container :deep(.submenu-title) a {
    display: none;
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
    color: var(--p-surface-700);
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
:deep(.p-tieredmenu-item-content:hover) {
    background-color: var(--p-surface-900) !important;
}

:deep(.p-tieredmenu-submenu .p-tieredmenu-item-content:hover) {
    background-color: var(--p-surface-100) !important;
}

/* Focus states */
:deep(.p-tieredmenu-item-content:focus) {
    background-color: var(--p-surface-900) !important;
}

:deep(.p-tieredmenu-submenu .p-tieredmenu-item-content:focus) {
    background-color: var(--p-surface-100) !important;
}

:deep(.p-tieredmenu-separator) {
    border-block-start: 1px solid var(--p-surface-500)
}
</style>