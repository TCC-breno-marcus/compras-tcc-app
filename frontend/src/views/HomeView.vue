<script setup lang="ts">
import { useRouter } from 'vue-router'
import Card from 'primevue/card'
import Button from 'primevue/button'
import { useAuthStore } from '@/features/autentication/stores/authStore'
import { storeToRefs } from 'pinia'

const router = useRouter()
const authStore = useAuthStore()

const { user } = storeToRefs(authStore)
</script>

<template>
  <div class="flex flex-column mt-2">
    <div class="my-4">
      <h2 class="m-0 text-2xl text-primary">Olá, {{ user?.nome }}!</h2>
      <p class="mt-2 text-color-secondary text-lg">O que você gostaria de fazer hoje?</p>
    </div>

    <div class="grid ml-2">
      <div class="col-12 md:col-6 lg:col-4">
        <Card>
          <template #title>
            <div class="flex align-items-center justify-content-between">
              <span class="font-semibold text-primary">Criar Nova Solicitação</span>
              <span class="material-symbols-outlined text-primary">add_2</span>
            </div>
          </template>
          <template #content>
            <p class="">Inicie uma nova solicitação para itens gerais ou patrimoniais.</p>
            <div class="flex justify-content-end w-full gap-2 mt-6">
              <Button
                label="Geral"
                @click="router.push('/solicitacoes/criar/geral')"
                size="small"
              />
              <Button
                label="Patrimonial"
                @click="router.push('/solicitacoes/criar/patrimonial')"
                text
                size="small"
              />
            </div>
          </template>
        </Card>
      </div>

      <div class="col-12 md:col-6 lg:col-4">
        <Card>
          <template #title>
            <div class="flex align-items-center justify-content-between">
              <span class="font-semibold text-primary">Minhas Solicitações</span>
              <span class="material-symbols-outlined text-primary">assignment</span>
            </div>
          </template>
          <template #content>
            <p>Visualize ou edite todas as suas solicitações.</p>
            <div class="flex justify-content-end w-full gap-2 mt-6">
              <Button
                label="Ver Minhas Solicitações"
                icon="pi pi-arrow-right"
                icon-pos="right"
                @click="router.push('/solicitacoes')"
                size="small"
              />
            </div>
          </template>
        </Card>
      </div>
    </div>
  </div>
</template>
