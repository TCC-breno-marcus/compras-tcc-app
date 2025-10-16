<script setup lang="ts">
import { useAuthStore } from '@/features/autentication/stores/authStore'
import { storeToRefs } from 'pinia'
import HomeCard from '@/components/ui/HomeCard.vue'
import PendingSolicitations from '@/features/solicitations/components/PendingSolicitations.vue'

const authStore = useAuthStore()

const { user, isAdmin, isGestor, isSolicitante } = storeToRefs(authStore)
</script>

<template>
  <div class="flex flex-column mt-2">
    <div class="mb-4">
      <h2 class="m-0 text-2xl text-primary">Olá, {{ user?.nome }}</h2>
      <p class="mt-2 text-color-secondary text-base">O que você gostaria de fazer hoje?</p>
    </div>

    <div class="flex flex-column lg:flex-row gap-3">
      <div class="grid w-12 lg:w-7">
        <div v-if="isAdmin || isSolicitante" class="col-12 md:col-6 md:col-4">
          <HomeCard
            title="Nova Solicitação"
            title-icon="add_shopping_cart"
            content-text="Inicie uma nova solicitação para itens gerais ou patrimoniais."
            button-label="Geral"
            button-route="/solicitacoes/criar/geral"
            button-label2="Patrimonial"
            button-route2="/solicitacoes/criar/patrimonial"
            color="#17b287"
          />
        </div>

        <div v-if="isAdmin || isSolicitante" class="col-12 md:col-6 md:col-4">
          <HomeCard
            title="Minhas Solicitações"
            title-icon="assignment_ind "
            content-text="Visualize ou edite todas as suas solicitações."
            button-label="Ver Histórico"
            button-route="/solicitacoes"
            color="#17b287"
          />
        </div>

        <div v-if="isAdmin || isGestor" class="col-12 md:col-6 md:col-4">
          <HomeCard
            title="Itens por Departamento"
            title-icon="segment"
            content-text="Visualize os itens solicitados divididos por departamento."
            button-label="Itens por Departamento"
            button-route="/gestor/departamento"
            color="#3d97f4"
          />
        </div>

        <div v-if="isAdmin || isGestor" class="col-12 md:col-6 md:col-4">
          <HomeCard
            title="Solicitações"
            title-icon="checklist"
            content-text="Visualize e revise as solicitações de compra abertas."
            button-label="Ver Solicitações"
            button-route="/gestor/solicitacoes"
            color="#3d97f4"
          />
        </div>

        <div v-if="isAdmin || isGestor" class="col-12 md:col-6 md:col-4">
          <HomeCard
            title="Catálogo"
            title-icon="inventory_2"
            content-text="Adicione, edite ou remova itens e controle a disponibilidade deles no sistema."
            button-label="Acessar Catálogo"
            button-route="/gestor/catalogo"
            color="#3d97f4"
          />
        </div>

        <div v-if="isAdmin || isGestor" class="col-12 md:col-6 md:col-4">
          <HomeCard
            title="Dashboards"
            title-icon="bar_chart"
            content-text="Visualize gráficos e indicadores sobre as solicitações de compra."
            button-label="Ver Gráficos"
            button-route="/gestor/dashboard"
            color="#f26c4f"
          />
        </div>

        <div v-if="isAdmin || isGestor" class="col-12 md:col-6 md:col-4">
          <HomeCard
            title="Relatórios"
            title-icon="article"
            content-text="Gere relatórios detalhados sobre as solicitações de compra."
            button-label="Gerar Relatórios"
            button-route="/gestor/relatorios"
            color="#f26c4f"
          />
        </div>

        <div v-if="isAdmin || isGestor" class="col-12 md:col-6 md:col-4">
          <HomeCard
            title="Usuários"
            title-icon="group"
            content-text="Crie e visualize usuários do sistema e gerencie seus perfis de acesso."
            button-label="Gerenciar Usuários"
            button-route="/gestor/usuarios"
            color="#f26c4f"
          />
        </div>
      </div>

      <div v-if="isAdmin || isGestor" class="w-12 lg:w-5">
        <PendingSolicitations />
      </div>
    </div>
  </div>
</template>
