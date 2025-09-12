<script setup lang="ts">
import { useRouter } from 'vue-router'
import Card from 'primevue/card'
import Button from 'primevue/button'
import { computed } from 'vue'

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

const iconStyle = computed(() => ({
  backgroundColor: `${props.color}1A`,
  color: props.color,
}))
</script>

<template>
  <Card
    class="h-full flex flex-column shadow-1 hover:shadow-4 transition-duration-200 cursor-pointer"
    @click="router.push(props.buttonRoute)"
  >
    <template #content>
      <div class="flex flex-column h-10rem">
        <div class="flex align-items-center gap-2 mb-2">
          <div
            class="flex align-items-center justify-content-center border-round"
            :style="iconStyle"
            style="width: 3rem; height: 3rem"
          >
            <span class="material-symbols-outlined" style="font-size: 1.5rem">
              {{ props.titleIcon }}
            </span>
          </div>
          <h3 class="font-semibold text-xl">{{ props.title }}</h3>
        </div>

        <p class="mt-0 mb-4 text-color-secondary line-height-3 flex-grow-1">
          {{ props.contentText }}
        </p>

        <div class="flex justify-content-end w-full gap-2 mt-auto">
          <template v-if="props.buttonLabel2 && props.buttonRoute2">
            <Button
              :label="props.buttonLabel"
              @click.stop="router.push(props.buttonRoute)"
              size="small"
              text
            />
            <Button
              :label="props.buttonLabel2"
              @click.stop="router.push(props.buttonRoute2)"
              outlined
              size="small"
              text
            />
          </template>
          <template v-else>
            <Button
              :label="props.buttonLabel"
              @click.stop="router.push(props.buttonRoute)"
              icon="pi pi-arrow-right"
              icon-pos="right"
              size="small"
              text
            />
          </template>
        </div>
      </div>
    </template>
  </Card>
</template>

<style scoped>
.p-dark .p-card:hover {
  background-color: var(--p-surface-800);
}
</style>
