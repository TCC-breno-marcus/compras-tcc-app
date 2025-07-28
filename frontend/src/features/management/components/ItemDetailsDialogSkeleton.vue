<script setup lang="ts">
import Skeleton from 'primevue/skeleton'
</script>

<template>
  <div>
    <Skeleton width="8rem" height="8rem" borderRadius="8px"></Skeleton>
  </div>
  <div class="flex flex-column flex-grow-1 pl-4">
    <Skeleton width="70%" height="1.5rem" class="mb-3"></Skeleton>
    <Skeleton width="40%" height="1rem" class="mb-4"></Skeleton>
    <Skeleton width="100%" height="1rem" class="mb-2"></Skeleton>
    <Skeleton width="100%" height="1rem" class="mb-2"></Skeleton>
    <Skeleton width="85%" height="1rem" class="mb-5"></Skeleton>

    <Skeleton width="50%" height="1.2rem" class="mb-3"></Skeleton>
    <Skeleton width="60%" height="1rem" class="mb-2"></Skeleton>
    <Skeleton width="65%" height="1rem"></Skeleton>
  </div>
</template>

<style scoped></style>
