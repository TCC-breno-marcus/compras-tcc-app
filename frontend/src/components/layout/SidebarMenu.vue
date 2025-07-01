<script setup lang="ts">
import { ref, computed } from 'vue';
import { useLayoutStore } from '@/stores/layout';
import { useRouter } from 'vue-router';

const router = useRouter();

const menuItems = [
    { label: 'Início', icon: 'pi pi-home', route: '/' },
    { label: 'E-mail', icon: 'pi pi-envelope', route: '/email' },
    {
        label: 'Recursos',
        icon: 'pi pi-folder',
        items: [
            { label: 'Laboratórios', icon: 'pi pi-desktop', route: '/recursos/labs' },
            { label: 'Equipamentos', icon: 'pi pi-print', route: '/recursos/equipamentos' },
            { label: 'Salas', icon: 'pi pi-building', route: '/recursos/salas' },
        ]
    },
    {
        label: 'Requerimentos',
        icon: 'pi pi-file-edit',
        items: [
            { label: 'Solicitar', icon: 'pi pi-plus', route: '/requerimentos/solicitar' },
            { label: 'Acompanhar', icon: 'pi pi-search', route: '/requerimentos/acompanhar' },
        ]
    },
    { separator: true },
    { label: 'Sobre o AdminDcomp', icon: 'pi pi-info-circle', route: '/sobre' },
    { label: 'Reportar um problema', icon: 'pi pi-question-circle', route: '/reportar' },
];

const layoutStore = useLayoutStore();
const isSidebarCollapsed = computed(() => layoutStore.isSidebarCollapsed);

// Para controlar qual submenu está aberto
const openSubmenuLabel = ref<string | null>(null);

function handleMenuClick(item: any) {
    if (item.items) { // Se o item tem um submenu
        // Se o submenu clicado já está aberto, fecha. Senão, abre.
        openSubmenuLabel.value = openSubmenuLabel.value === item.label ? null : item.label;
    }
}
</script>

<template>
    <div class="sidebar-container flex flex-column align-items-center p-2 gap-2 h-screen">
        <h1 style="color: ;">
            {{ isSidebarCollapsed ? 'LG' : 'LOGO' }}
        </h1>
        <aside class="sidebar-container" :class="{ 'sidebar-collapsed': isSidebarCollapsed }">
            <div class="menu-header">
                <span class="menu-label">MENU</span>
            </div>
            <ul class="menu-list">
                <li v-for="(item, index) in menuItems" :key="index" class="menu-item-wrapper">
                    <div v-if="item.separator" class="menu-separator"></div>

                    <div v-else class="menu-item-link" @click="handleMenuClick(item)">
                        <router-link :to="item.route || '#'" class="menu-link">
                            <i :class="item.icon"></i>
                            <span class="label">{{ item.label }}</span>
                            <i v-if="item.items" class="pi pi-angle-down sub-indicator"
                                :class="{ 'rotated': openSubmenuLabel === item.label }"></i>
                        </router-link>

                        <ul v-if="item.items" class="submenu"
                            :class="{ 'submenu-open': openSubmenuLabel === item.label }">
                            <li v-for="subItem in item.items" :key="subItem.label">
                                <router-link :to="subItem.route">
                                    <i :class="subItem.icon"></i>
                                    <span class="label">{{ subItem.label }}</span>
                                </router-link>
                            </li>
                        </ul>

                        <!-- Tooltip para o menu colapsado -->
                        <div class="menu-tooltip">
                            <div class="tooltip-content">
                                <div class="tooltip-main-item">
                                    <i :class="item.icon"></i>
                                    <span>{{ item.label }}</span>
                                </div>
                                <div v-if="item.items" class="tooltip-submenu">
                                    <router-link 
                                        v-for="subItem in item.items" 
                                        :key="subItem.label" 
                                        :to="subItem.route"
                                        class="tooltip-subitem"
                                    >
                                        <i :class="subItem.icon"></i>
                                        <span>{{ subItem.label }}</span>
                                    </router-link>
                                </div>
                            </div>
                        </div>
                    </div>
                </li>
            </ul>
        </aside>
    </div>
</template>

<style scoped>
.sidebar-container {
    background-color: var(--p-surface-800);
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
    color: var(--p-surface-400);
    white-space: nowrap;
    opacity: 1;
    transition: opacity 0.3s ease;
}

.menu-list {
    list-style: none;
    padding: 0;
    margin: 0;
    overflow-y: auto;
    overflow-x: hidden;
}

.menu-item-wrapper {
    position: relative;
}

.menu-item-link {
    position: relative;
}

.menu-link {
    display: flex;
    align-items: center;
    padding: 0.75rem 1.5rem;
    color: var(--p-surface-100);
    text-decoration: none;
    transition: background-color 0.2s;
    white-space: nowrap;
}

.menu-link:hover {
    background-color: var(--p-surface-700);
}

.menu-link i {
    font-size: 1.25rem;
    width: 2rem;
    text-align: center;
}

.menu-link .label {
    margin-left: 0.75rem;
    opacity: 1;
    transition: opacity 0.3s ease;
}

.menu-separator {
    height: 1px;
    background-color: var(--p-surface-700);
    margin: 0.5rem 1.5rem;
}

.sub-indicator {
    margin-left: auto;
    transition: transform 0.3s ease;
}

.sub-indicator.rotated {
    transform: rotate(90deg);
}

.submenu {
    list-style: none;
    padding-left: 2rem;
    margin: 0;
    max-height: 0;
    overflow: hidden;
    transition: max-height 0.3s ease-in-out;
}

.submenu-open {
    max-height: 200px;
}

.submenu a {
    padding: 0.5rem 1.5rem;
    font-size: 0.9rem;
}

/* TOOLTIP PARA MENU COLAPSADO */
.menu-tooltip {
    position: absolute;
    left: calc(100% + 10px);
    top: 50%;
    transform: translateY(-50%);
    z-index: 1000;
    opacity: 0;
    visibility: hidden;
    pointer-events: none;
    transition: all 0.2s ease;
}

.tooltip-content {
    background-color: var(--p-surface-800);
    border: 1px solid var(--p-surface-600);
    border-radius: 6px;
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.4);
    min-width: 200px;
    overflow: hidden;
}

.tooltip-main-item {
    display: flex;
    align-items: center;
    padding: 0.75rem 1rem;
    color: var(--p-surface-100);
    white-space: nowrap;
    font-weight: 500;
    background-color: var(--p-surface-750);
}

.tooltip-main-item i {
    font-size: 1.1rem;
    width: 1.5rem;
    text-align: center;
    margin-right: 0.75rem;
}

.tooltip-submenu {
    border-top: 1px solid var(--p-surface-600);
}

.tooltip-subitem {
    display: flex;
    align-items: center;
    padding: 0.6rem 1rem;
    color: var(--p-surface-200);
    text-decoration: none;
    transition: background-color 0.2s;
    font-size: 0.9rem;
    border-bottom: 1px solid var(--p-surface-700);
}

.tooltip-subitem:last-child {
    border-bottom: none;
}

.tooltip-subitem:hover {
    background-color: var(--p-surface-700);
    color: var(--p-surface-100);
}

.tooltip-subitem i {
    font-size: 1rem;
    width: 1.5rem;
    text-align: center;
    margin-right: 0.75rem;
}

/* ESTADO COLAPSADO */
.sidebar-collapsed {
    width: 80px;
}

.sidebar-collapsed .menu-header {
    opacity: 0;
    width: 0;
    pointer-events: none;
}

.sidebar-collapsed .menu-link .label,
.sidebar-collapsed .sub-indicator {
    opacity: 0;
    width: 0;
    pointer-events: none;
}

.sidebar-collapsed .submenu {
    display: none !important;
}

/* Mostrar tooltip ao passar o mouse quando colapsado */
.sidebar-collapsed .menu-item-link:hover .menu-tooltip {
    opacity: 1;
    visibility: visible;
    pointer-events: auto;
}

/* Garantir que o tooltip só apareça quando realmente colapsado */
.sidebar-collapsed .menu-tooltip {
    display: block;
}

.menu-tooltip {
    display: none;
}
</style>