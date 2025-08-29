<script setup lang="ts">
import { useRouter } from 'vue-router'
import Card from 'primevue/card'
import Button from 'primevue/button'

const props = defineProps<{
  title: string
  titleIcon: string
  contentText: string
  buttonLabel: string
  buttonRoute: string
  buttonLabel2?: string
  buttonRoute2?: string
  color?: string
}>()

const router = useRouter()
</script>

<template>
  <Card class="h-full flex flex-column" :style="{ borderTop: `0px solid ${props.color}` }">
    <template #title>
      <div class="flex align-items-center justify-content-between">
        <span class="font-semibold" :style="{ color: props.color }">{{ props.title }}</span>
        <span class="material-symbols-outlined" :style="{ color: props.color }">{{
          props.titleIcon
        }}</span>
      </div>
    </template>
    <template #content>
      <div class="flex h-1rem">
        <p class="">{{ props.contentText }}</p>
      </div>
    </template>
    <template #footer>
      <div class="flex justify-content-end w-full gap-2 mt-6">
        <Button
          :label="props.buttonLabel"
          @click="router.push(props.buttonRoute)"
          size="small"
          icon="pi pi-arrow-right"
          icon-pos="right"
          :style="{ color: props.color }"
          text
        />
        <Button
          v-if="props.buttonLabel2 && props.buttonRoute2"
          :label="props.buttonLabel2"
          @click="router.push(props.buttonRoute2)"
          text
          icon="pi pi-arrow-right"
          icon-pos="right"
          size="small"
          :style="{ color: props.color }"
        />
      </div>
    </template>
  </Card>
</template>
