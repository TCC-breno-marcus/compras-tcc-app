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
import { toTitleCase } from '@/utils/stringUtils'

const props = defineProps<{
  item: ItemDepartmentResponse
}>()

const emit = defineEmits(['show-pop-over'])

const handleClickInfo = (event: MouseEvent) => {
  emit('show-pop-over', event, props.item)
}
</script>

<template>
  <div v-if="item" class="border-200 border-round-lg border-1 px-4 pt-3 mb-3">
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
            <p class="font-semibold text-primary">{{ item.nome }}</p>
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
        <div class="flex flex-column align-items-center mb-1">
          <div class="text-color-secondary">Qtde. Total</div>
          <div class="flex justify-content-center">
            <Tag :value="formatQuantity(item.quantidadeTotalSolicitada)" severity="info" />

            <!-- <p className="font-semibold text-primary">
              {{ formatQuantity(item.quantidadeTotalSolicitada) }}
            </p> -->
          </div>
        </div>
        <div class="flex flex-column align-items-center mb-1">
          <div class="text-color-secondary">Nº de Solicitações</div>
          <div class="flex justify-content-center">
            <Tag :value="formatQuantity(item.numeroDeSolicitacoes)" severity="info" />

            <!-- <p className="font-semibold text-primary">
              {{ formatQuantity(item.numeroDeSolicitacoes) }}
            </p> -->
          </div>
        </div>
        <div class="flex flex-column align-items-center">
          <div class="text-color-secondary">Valor Total</div>
          <div class="flex justify-content-center">
            <Tag :value="formatCurrency(item.valorTotalSolicitado)" severity="success" />

            <!-- <p className="font-semibold text-green-500">
              {{ formatCurrency(item.valorTotalSolicitado) }}
            </p> -->
          </div>
        </div>
      </div>
    </div>

    <Accordion expandIcon="none" collapseIcon="none">
      <AccordionPanel value="0">
        <AccordionHeader>
          <template #default="{ active }">
            <span class="flex align-items-center gap-1">
              <p class="text-sm font-normal">Detalhes</p>
              <i
                class="pi text-sm"
                :class="{ 'pi-angle-down': active, 'pi-angle-right': !active }"
              ></i>
            </span>
          </template>
        </AccordionHeader>

        <AccordionContent class="accordion-content">
          <div>
            <div>
              <p class="font-semibold pt-3 mb-1">Análise de Custos</p>
              <ul class="list-none p-0 m-0">
                <li class="flex align-items-center justify-content-between mb-1">
                  <span class="text-color-secondary">Preço Médio</span>
                  <Tag severity="success" :value="formatCurrency(item.precoMedio)" />
                </li>
                <li class="flex align-items-center justify-content-between">
                  <span class="text-color-secondary">Faixa de Preço (Mín-Máx)</span>
                  <div class="flex align-items-center gap-1">
                    <Tag severity="secondary" :value="formatCurrency(item.precoMinimo)" />
                    <span>-</span>
                    <Tag severity="secondary" :value="formatCurrency(item.precoMaximo)" />
                  </div>
                </li>
              </ul>
            </div>

            <Divider class="my-3" />

            <div>
              <p class="font-semibold m-0 mb-1">Distribuição por Departamento</p>
              <div v-if="item.demandaPorDepartamento?.length > 0">
                <ul class="list-none p-0 m-0">
                  <li
                    v-for="dept in item.demandaPorDepartamento"
                    :key="dept.departamento"
                    class="pb-1"
                  >
                    <div class="flex align-items-center justify-content-between">
                      <span class="text-color-secondary">{{ toTitleCase(dept.departamento) }}</span>
                      <!-- <Badge :value="formatQuantity(dept.quantidadeTotal)" severity="info" /> -->
                      <Tag :value="formatQuantity(dept.quantidadeTotal)" severity="info" />
                    </div>

                    <div v-if="dept.justificativa && dept.justificativa.trim() !== ''">
                      <p
                        class="flex align-items-center text-sm text-color-secondary line-height-3 gap-2"
                      >
                        <i class="pi pi-info-circle text-sm" v-tooltip="'Justificativa'"></i>
                        {{ dept.justificativa }}
                      </p>
                    </div>
                  </li>
                </ul>
              </div>
              <p v-else class="text-sm text-color-secondary m-0">
                Não há dados de departamento para este item.
              </p>
            </div>
          </div>
        </AccordionContent>
      </AccordionPanel>
    </Accordion>
  </div>
</template>

<style scoped>
:deep(.p-accordionpanel) {
  border: 0;
}

:deep(.p-accordionheader) {
  padding: 1rem;
  background-color: transparent !important;
}

.accordion-content {
  padding-left: 1.75rem;
  margin-bottom: 0.75rem;
}
</style>
