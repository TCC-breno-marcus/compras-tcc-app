<script setup lang="ts">
import Skeleton from 'primevue/skeleton'
import Divider from 'primevue/divider'
import { ref } from 'vue'

const fakeUsers = ref(Array.from({ length: 5 }))
</script>

<template>
  <ul class="list-none p-0 m-0">
    <template v-for="(item, index) in fakeUsers" :key="index">
      <li class="py-3">
        <div class="flex align-items-center justify-content-between w-full">
          <div class="flex align-items-center gap-3">
            <Skeleton shape="circle" size="2.5rem" />

            <div class="flex flex-column gap-2">
              <Skeleton width="10rem" height="0.75rem" />
              <Skeleton width="14rem" height="0.75rem" />
              <Skeleton width="12rem" height="0.75rem" />
            </div>
          </div>

          <div>
            <Skeleton width="7rem" height="1rem" />
          </div>
        </div>
      </li>
      <Divider v-if="index < fakeUsers.length - 1" />
    </template>
  </ul>
</template>
