<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue'
import type { PropType, Ref } from 'vue'
import IconField from 'primevue/iconfield'
import InputIcon from 'primevue/inputicon'
import InputText from 'primevue/inputtext'
import Select from 'primevue/select'
import { Button } from 'primevue'
import OverlayPanel from 'primevue/overlaypanel'
import { storeToRefs } from 'pinia'
import FloatLabel from 'primevue/floatlabel'
import { useCategoriaStore } from '../stores/categoriaStore'
import type { CatalogoFilters } from '../types'

const props = defineProps({
  initialFilters: {
    type: Object as PropType<CatalogoFilters>,
    required: true,
  },
})

const emit = defineEmits(['apply-filters', 'clear-filters'])

const categoriaStore = useCategoriaStore()
const {
  categorias,
  loading: categoriasLoading,
  error: categoriasError,
} = storeToRefs(categoriaStore)

const op = ref()
const searchTerm = ref(props.initialFilters.searchTerm)
const nomeFilter = ref(props.initialFilters.nome)
const descricaoFilter = ref(props.initialFilters.descricao)
const catmatFilter = ref(props.initialFilters.catMat)
const especificacaoFilter = ref(props.initialFilters.especificacao)
const categoriaIdFilter = ref(props.initialFilters.categoriaId || [])
const statusFilter = ref(props.initialFilters.status)

const opcoesStatus = ref([
  { name: 'Ativo', code: 'ativo' },
  { name: 'Inativo', code: 'inativo' },
])
const sortOrder = ref(props.initialFilters.sortOrder || null);

const toggleSortDirection = () => {
  if (sortOrder.value === null) {
    sortOrder.value = 'asc'
  } else if (sortOrder.value === 'asc') {
    sortOrder.value = 'desc'
  } else {
    sortOrder.value = null // Volta para o padrão
  }
}

const computedSort = computed(() => {
  if (sortOrder.value === 'asc') {
    return { text: 'Ordenar de A-Z', icon: 'pi pi-sort-alpha-down' }
  }
  if (sortOrder.value === 'desc') {
    return { text: 'Ordenar de Z-A', icon: 'pi pi-sort-alpha-up' }
  }
  return { text: 'Não Ordenar', icon: 'pi pi-sort-alt' }
})

const toggleAdvancedFilter = (event: MouseEvent) => {
  if (op.value) {
    op.value.toggle(event)
  }
}

const handleApply = () => {
  const filters = {
    searchTerm: searchTerm.value,
    nome: nomeFilter.value,
    descricao: descricaoFilter.value,
    catmat: catmatFilter.value,
    especificacao: especificacaoFilter.value,
    categoriaId: categoriaIdFilter.value,
    status: statusFilter.value,
    sortOrder: sortOrder.value,
  }

  // Fecha o painel antes de emitir, se estiver aberto
  if (op.value) {
    op.value.hide()
  }

  emit('apply-filters', filters)
}

const handleClear = () => {
  nomeFilter.value = ''
  descricaoFilter.value = ''
  catmatFilter.value = ''
  especificacaoFilter.value = ''
  categoriaIdFilter.value = []
  statusFilter.value = ''
  searchTerm.value = ''
  sortOrder.value = null
  emit('clear-filters')
}

// Garante que o estado interno do ItemFilters sempre reflita as props que o pai envia.
watch(
  () => props.initialFilters,
  (newFilters) => {
    searchTerm.value = newFilters.searchTerm || ''
    nomeFilter.value = newFilters.nome || ''
    descricaoFilter.value = newFilters.descricao || ''
    catmatFilter.value = newFilters.catMat || ''
    especificacaoFilter.value = newFilters.especificacao || '' 
    categoriaIdFilter.value = newFilters.categoriaId || []
    statusFilter.value = newFilters.status || ''
    sortOrder.value = newFilters.sortOrder || null
  },
  { deep: true, immediate: true }, 
)

</script>

<template>
  <!-- TODO: ESSES BOTOES DE FILTRO NÃO ESTAO LEGAL PRA TELA media -->
  <div
    class="filters flex flex-column sm:flex-wrap sm:flex-row align-items-center sm:w-full lg:w-auto gap-2 p-3 xl:p-0"
  >
    <FloatLabel class="w-full sm:w-18rem" variant="on">
      <IconField iconPosition="left">
        <InputIcon class="pi pi-search"></InputIcon>
        <InputText
          v-model="searchTerm"
          size="small"
          class="w-full"
          inputId="simple-search"
          @keyup.enter="handleApply"
        />
      </IconField>
      <label for="simple-search">Pesquisar item</label>
    </FloatLabel>

    <FloatLabel class="w-full sm:w-16rem mt-1 sm:mt-0" variant="on">
      <!-- TODO: quando aplico o filtro o label fica vazio -->
      <Select
        v-model="categoriaIdFilter"
        multiple
        :options="categorias"
        optionLabel="nome"
        optionValue="id"
        inputId="categoria-filter"
        size="small"
        class="w-full sm:w-16rem"
        :showClear="true"
      />
      <label for="categoria-filter">Categoria</label>
    </FloatLabel>

    <!-- TODO: Filtro de Status não deve aparecer nas paginas de solicitação  -->
    <FloatLabel class="w-full sm:w-8rem mt-1 sm:mt-0" variant="on">
      <Select
        v-model="statusFilter"
        :options="opcoesStatus"
        optionLabel="name"
        optionValue="code"
        inputId="status-filter"
        size="small"
        class="w-full sm:w-8rem"
        :showClear="true"
      />
      <label for="status-filter">Status</label>
    </FloatLabel>

    <div class="flex sm:flex-wrap w-full sm:w-auto gap-2">
      <Button
        :icon="computedSort.icon"
        @click="toggleSortDirection"
        label="Ordem"
        outlined
        size="small"
        aria-label="Ordem"
        v-tooltip.top="computedSort.text"
      />
      <Button
        type="button"
        label="Filtros Avançados"
        icon="pi pi-filter"
        size="small"
        outlined
        class="w-12rem"
        @click="toggleAdvancedFilter"
      />
      <OverlayPanel ref="op">
        <div class="flex flex-column gap-2 p-2" style="min-width: 250px">
          <span class="p-text-secondary"><strong>Filtros Avançados</strong></span>

          <IconField iconPosition="rigth">
            <InputIcon class="pi pi-search"></InputIcon>
            <InputText v-model="nomeFilter" size="small" placeholder="Nome do item" />
          </IconField>

          <IconField iconPosition="rigth">
            <InputIcon class="pi pi-search"></InputIcon>
            <InputText v-model="descricaoFilter" size="small" placeholder="Descrição" />
          </IconField>

          <IconField iconPosition="rigth">
            <InputIcon class="pi pi-search"></InputIcon>
            <InputText v-model="catmatFilter" size="small" placeholder="CatMat" />
          </IconField>

          <IconField iconPosition="rigth">
            <InputIcon class="pi pi-search"></InputIcon>
            <InputText v-model="especificacaoFilter" size="small" placeholder="Especificação" />
          </IconField>

          <div class="flex justify-content-end gap-2">
            <Button
              label="Limpar"
              icon="pi pi-filter-slash"
              severity="danger"
              text
              @click="handleClear"
              size="small"
            />
          </div>
        </div>
      </OverlayPanel>
    </div>

    <div class="flex sm:flex-wrap w-full sm:w-auto gap-2 justify-content-end">
      <Button
        label="Limpar"
        icon="pi pi-filter-slash"
        severity="danger"
        text
        @click="handleClear"
        size="small"
      />
      <Button type="button" label="Buscar" icon="pi pi-filter" size="small" @click="handleApply" />
    </div>
  </div>
</template>

<style scoped></style>
