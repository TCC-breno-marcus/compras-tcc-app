<script setup lang="ts">
import Avatar from 'primevue/avatar';
import OverlayMenu from './OverlayMenu.vue';
import { useBreakpoint } from '@/composables/useBreakpoint';
const { isLargeScreen } = useBreakpoint();
import { ref, computed } from "vue";
import Popover from 'primevue/popover';
import { Divider } from 'primevue';
import { useThemeStore } from '@/stores/theme';

const themeStore = useThemeStore();

const actionItems = computed(() => {
  return [
    {
      label: 'Meu Perfil',
      icon: 'pi pi-user',
      route: '/perfil'
    },
    {
      label: 'Configurações',
      icon: 'pi pi-cog',
      route: '/configuracoes'
    },
    {
      label: 'Alternar Tema',
      icon: themeStore.isDarkMode ? 'pi pi-sun' : 'pi pi-moon',
      command: () => {
        themeStore.toggleTheme();
      }
    }
  ];
});

const op = ref();

const toggle = (event: Event) => {
  op.value.toggle(event);
}


</script>

<template>
  <div class="header-container flex align-items-center justify-content-between px-4 py-2">
    <OverlayMenu v-if="!isLargeScreen" />
    <router-link to="/" class="logo-link logo-avancado-wrapper">
      <span class="material-symbols-outlined">shopping_cart</span>
      <h2 class="logo-gradiente">Compras TCC</h2>
    </router-link>
    <div class="flex align-items-center">
      <Avatar label="J" class="mr-2 cursor-pointer" shape="circle" @click="toggle" aria-haspopup="true"
        aria-controls="overlay_menu" />
      <!-- <p class="text-sm">João da Silva</p> -->
      <Popover ref="op">
        <div class="user-menu-content flex flex-column gap-2 w-14rem text-sm">

          <div class="flex flex-column align-items-center p-">
            <Avatar label="J" size="large" shape="circle" />
            <span class="font-bold mt-2">João da Silva</span>
            <span class="text-sm text-color-secondary">DCOMP</span>
          </div>

          <Divider />

          <ul class="list-none p-0 m-0">
            <li v-for="item in actionItems" :key="item.label">
              <router-link :to="item.route || '#'" class="profile-menu-item p-2" @click="item.command">
                <!-- <span class="material-symbols-outlined">{{ item.icon }}</span> -->
                <i :class="item.icon"></i>
                <span>{{ item.label }}</span>
              </router-link>
            </li>
          </ul>

          <Divider />

          <a @click="console.log('Saindo...')" class="profile-menu-item logout-item p-2">
            <i class="pi pi-sign-out"></i>
            <span>Sair</span>
          </a>

        </div>
      </Popover>
      <!-- <p class="text-sm">João da Silva (DCOMP)</p> -->
    </div>
  </div>
</template>

<style scoped>
.logo-link {
  text-decoration: none;
}

.logo-avancado-wrapper {
  display: flex;
  align-items: center;
  gap: 10px;
}

.logo-avancado-wrapper span {
  font-size: 1.8rem;
  color: #3498db;
}

.logo-gradiente {
  font-family: 'Poppins', sans-serif;
  font-weight: 800;
  margin: 0;
  background: linear-gradient(45deg, #3498db, #17B287);
  -webkit-background-clip: text;
  background-clip: text;
  -webkit-text-fill-color: transparent;
  color: transparent;
}

/* .header-container {
  background: linear-gradient(to bottom, transparent 0%, var(--p-surface-50) 100%);
} */

/* OU */

.header-container {
  position: relative;
}

.header-container::after {
  content: '';
  position: absolute;
  bottom: 0;
  left: 0%;
  right: 0%;
  height: 1px;
  background: linear-gradient(to right, transparent 0%, var(--p-surface-200) 50%, transparent 100%);
}

.profile-trigger {
  /* Garante que o container do gatilho tenha um bom espaçamento e aparência */
  padding: 0.5rem;
  border-radius: 6px;
  transition: background-color 0.2s;
}

.profile-trigger:hover {
  background-color: var(--p-surface-100);
}


.profile-menu-item {
  display: flex;
  align-items: center;
  border-radius: 6px;
  color: var(--p-text-color);
  text-decoration: none;
  cursor: pointer;
  transition: background-color 0.2s;
}

.profile-menu-item:hover {
  background-color: var(--p-surface-100);
}

.p-dark .profile-menu-item:hover {
  background-color: var(--p-surface-700);
}

.profile-menu-item i {
  margin-right: 0.75rem;
  color: var(--p-text-color-secondary);
}

/* Estilo especial para o item de logout */
.logout-item:hover {
  background-color: var(--p-red-50);
}

.logout-item:hover,
.logout-item:hover i {
  color: var(--p-red-500);
}
</style>
