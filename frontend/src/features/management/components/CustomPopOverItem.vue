<script setup lang="ts">
import type { ItemDepartmentResponse } from '@/features/reports/types'
import { formatCurrency } from '@/utils/currency'
import { formatQuantity } from '@/utils/number'
import Avatar from 'primevue/avatar'
import Badge from 'primevue/badge'
import Divider from 'primevue/divider'
import Tag from 'primevue/tag'

defineProps<{
  item: ItemDepartmentResponse | null
}>()
</script>

<template>
  <div v-if="item" class="p-4" style="max-width: 450px; min-width: 350px">
    <div class="flex gap-3 mb-4">
      <Avatar
        v-if="item.linkImagem"
        :image="item.linkImagem"
        shape="circle"
        size="xlarge"
        class="flex-shrink-0"
      />
      <Avatar
        v-else
        icon="pi pi-image"
        shape="circle"
        size="xlarge"
        class="flex-shrink-0 bg-transparent"
      />

      <div class="flex-1">
        <h4 class="m-0 mb-0 text-lg font-medium">{{ item.nome }}</h4>
        <p class="m-0 mb-2 text-xs text-color-secondary line-height-3">CATMAT: {{ item.catMat }}</p>
        <p class="m-0 text-sm text-color-secondary line-height-3">
          {{ item.descricao }}
        </p>
        <!-- <p class="m-0 text-sm text-color-secondary line-height-3">
          {{ item.descricao }} especificacao
        </p> -->
      </div>
    </div>

    <div class="space-y-2">
      <div class="flex justify-content-between align-items-center">
        <span class="text-sm font-medium">Categoria:</span>
        <span class="text-sm text-color-secondary">{{ item.categoriaNome }}</span>
      </div>
      <div v-if="item.especificacao" class="flex justify-content-between align-items-center">
        <span class="text-sm font-medium">Especificação:</span>
        <span class="text-sm text-color-secondary">{{ item.especificacao }}</span>
      </div>
    </div>

    <Divider class="my-3" />

    <div class="space-y-3">
      <div
        v-if="item.quantidadeTotalSolicitada"
        class="flex justify-content-between align-items-center"
      >
        <span class="text-sm font-medium">Quantidade Total:</span>
        <Tag :value="formatQuantity(item.quantidadeTotalSolicitada)" severity="info" />
      </div>

      <div v-if="item.numeroDeSolicitacoes" class="flex justify-content-between align-items-center">
        <span class="text-sm font-medium">Nº de Solicitações:</span>
        <Tag :value="item.numeroDeSolicitacoes" severity="info" />
      </div>

      <div v-if="item.valorTotalSolicitado" class="flex justify-content-between align-items-center">
        <span class="text-sm font-medium">Valor Total:</span>
        <Tag severity="success" :value="formatCurrency(item.valorTotalSolicitado)" />
      </div>

      <div v-if="item.precoMedio" class="flex justify-content-between align-items-center">
        <span class="text-sm font-medium">Preço Médio:</span>
        <Tag severity="success" :value="formatCurrency(item.precoMedio)" />
      </div>

      <div class="flex justify-content-between align-items-center">
        <span class="text-sm font-medium">Faixa de Preço:</span>
        <div class="flex gap-1">
          <Tag severity="success" :value="formatCurrency(item.precoMinimo)" />
          <span class="text-xs align-self-center">-</span>
          <Tag severity="success" :value="formatCurrency(item.precoMaximo)" />
        </div>
      </div>

      <div v-if="item.quantidadesPorDepartamento && item.quantidadesPorDepartamento.length > 0">
        <Divider class="my-3" />
        <h5 class="m-0 mb-2 text-sm font-medium">Departamentos:</h5>
        <div class="space-y-2">
          <div
            v-for="dept in item.quantidadesPorDepartamento"
            :key="dept.departamento"
            class="flex justify-content-between align-items-center text-sm"
          >
            <span>{{ dept.departamento }}</span>
            <Badge :value="dept.quantidadeTotal" />
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.space-y-3 > * + * {
  margin-top: 0.75rem;
}

.space-y-2 > * + * {
  margin-top: 0.5rem;
}
</style>
