<script setup lang="ts">
import Divider from 'primevue/divider';
import Tabs from 'primevue/tabs';
import TabList from 'primevue/tablist';
import Tab from 'primevue/tab';
import TabPanels from 'primevue/tabpanels';
import TabPanel from 'primevue/tabpanel';
import { ref, shallowRef } from "vue";
import Button from 'primevue/button';
import ManageItems from '../components/ManageItems.vue';
import Dashboard from '../components/Dashboard.vue';

const items = ref([
  { label: 'Dashboard', icon: 'pi pi-chart-bar', component: shallowRef(Dashboard) },
  // { label: 'Itens por Departamento', icon: 'pi pi-chart-pie', component: shallowRef(DepartmentChartPanel) },
  // { label: 'Solicitações', icon: 'pi pi-file-check', component: shallowRef(RequestsListPanel) },
  { label: 'Gerenciar Catálogo', icon: 'pi pi-book', component: shallowRef(ManageItems) }
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
.p-dark .tabs-container :deep(.p-tablist-tab-list),
.p-dark .tabs-container :deep(.p-tabpanels) {
  background-color: transparent !important;
}
</style>