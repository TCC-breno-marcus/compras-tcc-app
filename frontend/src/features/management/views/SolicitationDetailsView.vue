<script setup lang="ts">
import { ref } from "vue";
import Divider from 'primevue/divider';
import Avatar from 'primevue/avatar';
import Card from 'primevue/card';

// Seus dados da solicitação
const solicitation = ref({
  id: 8,
  userRequest: 'Juliana Alves (DCOMP)',
  date: 1752211200000,
  itemsQuantity: 6,
  totalPrice: 2100.0,
});

// Helper para formatar a data
const formattedDate = new Date(solicitation.value.date).toLocaleString('pt-BR', {
    dateStyle: 'long',
    timeStyle: 'short'
});

// Helper para formatar o preço
const formattedPrice = new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(solicitation.value.totalPrice);
</script>

<template>
  <div class="p-2">
    <div class="flex items-center justify-content-between mb-4">
      <h3 class="m-0">Detalhes da Solicitação #{{ solicitation.id }}</h3>
      </div>

    <div class="grid">
      <div class="col-12 md:col-8">
        <Card>
          <template #content>
            <ul class="list-none p-0 m-0">
              <li class="flex items-center mb-3">
                <i class="pi pi-user text-primary text-xl mr-3"></i>
                <div>
                  <span class="text-sm text-surface-500">Requisitante</span>
                  <p class="font-bold m-0">{{ solicitation.userRequest }}</p>
                </div>
              </li>
              <li class="flex items-center">
                <i class="pi pi-calendar text-primary text-xl mr-3"></i>
                <div>
                  <span class="text-sm text-surface-500">Data da Solicitação</span>
                  <p class="font-bold m-0">{{ formattedDate }}</p>
                </div>
              </li>
            </ul>
          </template>
        </Card>
      </div>

      <div class="col-12 md:col-4">
        <Card class="h-full">
          <template #content>
             <ul class="list-none p-0 m-0">
              <li class="flex items-center mb-3">
                <i class="pi pi-box text-primary text-xl mr-3"></i>
                <div>
                  <span class="text-sm text-surface-500">Quantidade de Itens</span>
                  <p class="font-bold m-0">{{ solicitation.itemsQuantity }}</p>
                </div>
              </li>
              <li class="flex items-center">
                <i class="pi pi-dollar text-primary text-xl mr-3"></i>
                <div>
                  <span class="text-sm text-surface-500">Valor Total Estimado</span>
                  <p class="font-bold m-0">{{ formattedPrice }}</p>
                </div>
              </li>
            </ul>
          </template>
        </Card>
      </div>
    </div>
    
    <Divider align="left" type="solid" class="my-4">
        <b>Itens da Solicitação</b>
    </Divider>

    <div>
      <p class="text-center text-surface-500">
        (Área reservada para a tabela de itens)
      </p>
    </div>
  </div>
</template>

<style scoped>
/* Estilos para garantir que o card ocupe 100% da altura da coluna no grid */
.p-card {
  height: 100%;
}
</style>