<script setup lang="ts">
import Divider from 'primevue/divider';
import Tabs from 'primevue/tabs';
import TabList from 'primevue/tablist';
import Tab from 'primevue/tab';
import TabPanels from 'primevue/tabpanels';
import TabPanel from 'primevue/tabpanel';
import { ref, shallowRef } from "vue";
import ManageItems from '../components/ManageItems.vue';
import Dashboard from '../components/Dashboard.vue';
import ItemsPerDepartment from '../components/ItemsPerDepartment.vue';
import Solicitations from '../components/Solicitations.vue';
import Reports from '../components/Reports.vue';

const items = ref([
  { label: 'Dashboard', icon: 'pi pi-chart-bar', component: shallowRef(Dashboard) },
  { label: 'Itens por Departamento', icon: 'pi pi-chart-pie', component: shallowRef(ItemsPerDepartment) },
  { label: 'Solicitações', icon: 'pi pi-list', component: shallowRef(Solicitations) },
  { label: 'Gerenciar Catálogo', icon: 'pi pi-book', component: shallowRef(ManageItems) },
  { label: 'Relatórios', icon: 'pi pi-book', component: shallowRef(Reports) }
]);

const activeTab = ref(items.value[0].label); // Define a primeira aba como ativa
</script>

<template>
  <div class="flex flex-column w-full h-full p-2">
    <div class="flex items-center justify-content-between">
      <h3>Painel do Gestor</h3>
    </div>

    <Tabs v-model:value="activeTab" class="mt-2 tabs-container">
      <TabList>
        <Tab v-for="tab in items" :key="tab.label" :value="tab.label">
          <div class="flex items-center gap-2">
            <i :class="tab.icon" />
            <span>{{ tab.label }}</span>
          </div>
        </Tab>
      </TabList>

      <TabPanels>
        <TabPanel v-for="tab in items" :key="tab.label" :value="tab.label">
          <component :is="tab.component" />
        </TabPanel>
      </TabPanels>
    </Tabs>
  </div>
</template>

<style scoped>
</style>