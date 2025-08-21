<script setup lang="ts">
import Skeleton from 'primevue/skeleton'
import Card from 'primevue/card'
import Tabs from 'primevue/tabs'
import TabList from 'primevue/tablist'
import Tab from 'primevue/tab'
import TabPanels from 'primevue/tabpanels'
import TabPanel from 'primevue/tabpanel'
</script>

<template>
  <div class="p-2">
    <div class="flex align-items-center justify-content-between mb-4">
      <Skeleton width="18rem" height="2rem" />
      <div class="flex gap-2">
        <Skeleton width="16rem" height="2.5rem" />
        <Skeleton width="6rem" height="2.5rem" />
      </div>
    </div>

    <div class="grid">
      <div class="col-12 lg:col-4">
        <Card class="h-full">
          <template #content>
            <ul class="list-none p-0 m-0">
              <li class="flex align-items-center mb-4">
                <Skeleton shape="circle" size="2.5rem" class="mr-3" />
                <div>
                  <Skeleton width="6rem" height="0.75rem" class="mb-2" />
                  <Skeleton width="10rem" height="1.2rem" />
                </div>
              </li>
              <li class="flex align-items-center">
                <Skeleton shape="circle" size="2.5rem" class="mr-3" />
                <div>
                  <Skeleton width="4rem" height="0.75rem" class="mb-2" />
                  <Skeleton width="12rem" height="1.2rem" />
                </div>
              </li>
            </ul>
          </template>
        </Card>
      </div>

      <div class="col-12 lg:col-4">
        <Card class="h-full">
          <template #content>
            <ul class="list-none p-0 m-0">
              <li class="flex align-items-center mb-4">
                <Skeleton shape="circle" size="2.5rem" class="mr-3" />
                <div>
                  <Skeleton width="8rem" height="0.75rem" class="mb-2" />
                  <Skeleton width="11rem" height="1.2rem" />
                </div>
              </li>
              <li class="flex align-items-center">
                <Skeleton shape="circle" size="2.5rem" class="mr-3" />
                <div>
                  <Skeleton width="8rem" height="0.75rem" class="mb-2" />
                  <Skeleton width="6rem" height="1.2rem" />
                </div>
              </li>
            </ul>
          </template>
        </Card>
      </div>

      <div class="col-12 lg:col-4">
        <Card class="h-full">
          <template #title>
            <div class="flex align-items-center">
              <Skeleton shape="circle" size="1.5rem" class="mr-2" />
              <Skeleton width="8rem" height="1.2rem" />
            </div>
          </template>
          <template #content>
            <Skeleton width="100%" height="1rem" />
            <Skeleton width="85%" height="1rem" class="mt-2" />
          </template>
        </Card>
      </div>
    </div>

    <Tabs value="0" class="mt-3">
      <TabList>
        <Tab value="0"><Skeleton width="8rem" height="1.5rem" /></Tab>
        <Tab value="1"><Skeleton width="11rem" height="1.5rem" /></Tab>
      </TabList>
      <TabPanels>
        <TabPanel value="0">
          <div v-for="i in 5" :key="i" class="surface-border border-top-1 p-3">
             <div class="flex flex-column md:flex-row md:align-items-center md:justify-content-between">
                <div class="flex align-items-center">
                  <Skeleton size="4rem" class="mr-3" />
                  <div>
                    <Skeleton width="10rem" height="1.2rem" class="mb-2"/>
                    <Skeleton width="6rem" height="0.8rem"/>
                  </div>
                </div>
                <div class="mt-3 md:mt-0">
                  <Skeleton width="15rem" height="2.5rem" />
                </div>
            </div>
          </div>
        </TabPanel>
        <TabPanel value="1">
          </TabPanel>
      </TabPanels>
    </Tabs>
  </div>
</template>