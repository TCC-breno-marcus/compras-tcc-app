<script setup lang="ts">
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import Skeleton from 'primevue/skeleton'
import { ref } from 'vue'

const fakeItems = ref(Array.from({ length: 5 }))
</script>

<template>
  <div class="p-4">
    <DataTable :value="fakeItems">
      <Column field="name" header="">
        <template #header><Skeleton width="10rem" height="1.5rem" /></template>
        <template #body><Skeleton height="1.5rem" /></template>
      </Column>

      <Column field="name" header="">
        <template #header><Skeleton width="10rem" height="1.5rem" /></template>
        <template #body><Skeleton height="1.5rem" /></template>
      </Column>

      <Column field="name" header="">
        <template #header><Skeleton width="10rem" height="1.5rem" /></template>
        <template #body><Skeleton height="1.5rem" /></template>
      </Column>

      <Column field="name" header="">
        <template #header><Skeleton width="10rem" height="1.5rem" /></template>
        <template #body><Skeleton height="1.5rem" /></template>
      </Column>

      <Column field="status" header="">
        <template #header><Skeleton width="6rem" height="1.5rem" /></template>
        <template #body><Skeleton width="6rem" height="1.5rem" /></template>
      </Column>

      <Column field="date" header="">
        <template #header><Skeleton width="5rem" height="1.5rem" /></template>
        <template #body><Skeleton width="8rem" height="1.5rem" /></template>
      </Column>

      <Column field="date" header="">
        <template #header><Skeleton width="5rem" height="1.5rem" /></template>
        <template #body><Skeleton width="8rem" height="1.5rem" /></template>
      </Column>
    </DataTable>

    <div class="flex justify-content-between align-items-center mt-3">
      <Skeleton width="15rem" height="1.5rem" />
      <div class="flex gap-1">
        <Skeleton shape="circle" size="2.5rem" />
        <Skeleton width="5rem" height="2.5rem" />
        <Skeleton shape="circle" size="2.5rem" />
      </div>
    </div>
  </div>
</template>

<style scoped></style>
