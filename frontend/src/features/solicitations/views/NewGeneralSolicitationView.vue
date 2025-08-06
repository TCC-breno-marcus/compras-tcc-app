<script setup lang="ts">
import { provide, reactive, readonly } from 'vue'
import CreateSolicitationGeneric from '../components/CreateSolicitationGeneric.vue'

const solicitationContext = reactive({
  dialogMode: 'selection',
  isGeneral: true,
})

provide('solicitationContext', readonly(solicitationContext))
</script>

<template>
  <CreateSolicitationGeneric />
</template>
