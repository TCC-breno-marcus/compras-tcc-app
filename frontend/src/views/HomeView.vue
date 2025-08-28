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
    <h2 class="mb-4">Bem-vindo, Solicitante!</h2>
    <div class="grid">
      <div class="col-12 md:col-6 lg:col-4">
        <Card>
          <template #title>
            <div class="flex align-items-center gap-2">
              <span class="material-symbols-outlined">add_2</span>
              <span>Criar Nova Solicitação</span>
            </div>
          </template>
          <template #content>
            <p>Inicie uma nova solicitação para itens gerais ou patrimoniais.</p>
          </template>
          <template #footer>
            <div class="flex gap-2">
              <Button
                label="Geral"
                @click="router.push('/solicitacoes/criar/geral')"
                size="small"
              />
              <Button
                label="Patrimonial"
                @click="router.push('/solicitacoes/criar/patrimonial')"
                outlined
                size="small"
              />
            </div>
          </template>
        </Card>
      </div>

      <div class="col-12 md:col-6 lg:col-4">
        <Card>
          <template #title>
            <div class="flex align-items-center gap-2">
              <span class="material-symbols-outlined">assignment</span>
              <span>Minhas Solicitações</span>
            </div>
          </template>
          <template #content>
            <p>Visualize ou edite todas as suas solicitações.</p>
          </template>
          <template #footer>
            <Button
              label="Ver Minhas Solicitações"
              icon="pi pi-arrow-right"
              icon-pos="right"
              @click="router.push('/solicitacoes')"
              size="small"
            />
          </template>
        </Card>
      </div>
    </div>
  </div>
</template>
