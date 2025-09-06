<script setup lang="ts">
import type { ItemDepartmentResponse } from '@/features/reports/types'
import { formatCurrency } from '@/utils/currency'
import { formatQuantity } from '@/utils/number'
import Avatar from 'primevue/avatar'
import Tag from 'primevue/tag'
import Accordion from 'primevue/accordion'
import AccordionPanel from 'primevue/accordionpanel'
import AccordionHeader from 'primevue/accordionheader'
import AccordionContent from 'primevue/accordioncontent'
import Divider from 'primevue/divider'
import { Badge, Button, OverlayBadge } from 'primevue'

const props = defineProps<{
  item: ItemDepartmentResponse
}>()

const emit = defineEmits(['show-pop-over'])

const handleClickInfo = (event: MouseEvent) => {
  emit('show-pop-over', event, props.item)
}
</script>

<template>
  <div v-if="item" class="surface-card border-round-lg shadow-1 pr-4 py-2">
    <div class="flex flex-column md:flex-row gap-4">
      <div class="flex-1 flex align-items-center gap-3">
        <Avatar
          v-if="item.linkImagem"
          :image="item.linkImagem"
          shape="circle"
          class="flex-shrink-0"
        />
        <Avatar v-else icon="pi pi-image" shape="circle" class="flex-shrink-0 bg-transparent" />
        <div>
          <div class="flex align-items-center gap-2">
            <p class="font-bold m-0">{{ item.nome }}</p>
            <span
              class="cursor-pointer"
              @click="handleClickInfo"
              v-tooltip="'Ver Detalhes do Item'"
            >
              <i class="pi pi-info-circle text-color-secondary text-sm"></i>
            </span>
          </div>
          <p class="m-0 text-xs text-color-secondary">CATMAT: {{ item.catMat }}</p>
        </div>
      </div>

      <div class="flex align-items-center justify-content-end gap-3 md:gap-5">
        <div class="flex flex-column gap-1">
          <div class="text-color-secondary">Qtde. Total</div>
          <div class="flex justify-content-center">
            <Tag :value="formatQuantity(item.quantidadeTotalSolicitada)" severity="info" />
          </div>
        </div>
        <div class="flex flex-column gap-1">
          <div class="text-color-secondary">Nº de Solicitações</div>
          <div class="flex justify-content-center">
            <Tag :value="formatQuantity(item.numeroDeSolicitacoes)" severity="info" />
          </div>
        </div>
        <div class="flex flex-column gap-1">
          <div class="text-color-secondary">Valor Total</div>
          <div class="flex justify-content-center">
            <Tag :value="formatCurrency(item.valorTotalSolicitado)" severity="success" />
          </div>
        </div>
      </div>
    </div>

    <Accordion>
      <AccordionPanel value="0">
        <AccordionHeader>
          <span class="flex align-items-center gap-2 text-color-secondary">
            <i class="pi pi-plus text-sm"></i>
            Detalhes
          </span>
        </AccordionHeader>
        <AccordionContent>
          <div class="grid">
            <div class="col-12 sm:col-4 md:col-2">
              <div class="text-sm font-medium">Preço Médio</div>
              <Tag severity="success" :value="formatCurrency(item.precoMedio)"></Tag>
            </div>
            <div class="col-12 sm:col-6 md:col-4">
              <div class="text-sm font-medium">Faixa de Preço (Mín-Máx)</div>
              <div class="flex gap-1">
                <Tag severity="success" :value="formatCurrency(item.precoMinimo)"></Tag>
                <span>-</span>
                <Tag severity="success" :value="formatCurrency(item.precoMaximo)"></Tag>
              </div>
            </div>
          </div>
          <Divider class="my-3" />
          <div>
            <div class="text-sm font-medium mb-2">Distribuição por Departamento</div>
            <div v-if="item.quantidadesPorDepartamento?.length > 0" class="flex flex-wrap gap-2">
              <Tag
                v-for="dept in item.quantidadesPorDepartamento"
                :key="dept.departamento"
                severity="secondary"
              >
                <div class="flex align-items-center gap-2">
                  <span>{{ dept.departamento }}</span>
                  <Badge :value="dept.quantidadeTotal" severity="info"></Badge>
                </div>
              </Tag>
            </div>
            <!-- TODO: deve mostrar um campo de justificativa por departamento também pra itens do tipo mobiliario e eletrodomesticos -->
            <p v-else class="text-sm text-color-secondary">Não há dados de departamento.</p>
          </div>
        </AccordionContent>
      </AccordionPanel>
    </Accordion>
  </div>
</template>
