<script setup lang="ts">
import { provide, reactive, readonly } from 'vue'
import CreateSolicitationGeneric from '../components/CreateSolicitationGeneric.vue'
import { SolicitationContextKey, type SolicitationContext } from '../keys'

const solicitationContext = reactive<SolicitationContext>({
  dialogMode: 'selection',
  isGeneral: true,
})

provide(SolicitationContextKey, readonly(solicitationContext))
</script>

<template>
  <CreateSolicitationGeneric />
</template>
