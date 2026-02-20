<script setup lang="ts">
import { ref, reactive, computed, onMounted } from 'vue'
import { useToast } from 'primevue/usetoast'
import InputText from 'primevue/inputtext'
import Textarea from 'primevue/textarea'
import Dropdown from 'primevue/dropdown'
import Button from 'primevue/button'
import Accordion from 'primevue/accordion'
import AccordionTab from 'primevue/accordiontab'
import { useSettingStore } from '@/features/settings/stores/settingStore'
import { storeToRefs } from 'pinia'

type FormData = {
  nome: string
  email: string
  telefone: string
  assunto: string
  mensagem: string
}

const toast = useToast()
const settingsStore = useSettingStore()
const { settings } = storeToRefs(settingsStore)

const form = reactive<FormData>({
  nome: '',
  email: '',
  telefone: '',
  assunto: '',
  mensagem: '',
})

const assuntos = ref([
  { label: 'Dúvida sobre criação de solicitação', value: 'criacao_solicitacao' },
  { label: 'Dúvida sobre status e ajustes', value: 'status_ajustes' },
  { label: 'Problema no catálogo de itens', value: 'catalogo_itens' },
  { label: 'Acesso ao sistema', value: 'acesso_sistema' },
  { label: 'Sugestão de melhoria', value: 'sugestao' },
  { label: 'Outros', value: 'outros' },
])

const errors = ref<Partial<FormData>>({})
const fallbackContactEmail = 'contato.compras@instituicao.br'

const contactEmail = computed(() => {
  return settings.value?.emailContatoPrincipal || fallbackContactEmail
})

const notificationEmail = computed(() => {
  return settings.value?.emailParaNotificacoes || contactEmail.value
})

const selectedSubjectLabel = computed(() => {
  const selected = assuntos.value.find((item) => item.value === form.assunto)
  return selected?.label || 'Contato via sistema de solicitações'
})

const validateForm = () => {
  errors.value = {}

  if (!form.nome.trim()) {
    errors.value.nome = 'Nome é obrigatório'
  }

  if (!form.email.trim()) {
    errors.value.email = 'E-mail é obrigatório'
  } else if (!/\S+@\S+\.\S+/.test(form.email)) {
    errors.value.email = 'E-mail inválido'
  }

  if (!form.assunto) {
    errors.value.assunto = 'Assunto é obrigatório'
  }

  if (!form.mensagem.trim()) {
    errors.value.mensagem = 'Mensagem é obrigatória'
  } else if (form.mensagem.trim().length < 10) {
    errors.value.mensagem = 'Mensagem deve ter pelo menos 10 caracteres'
  }

  return Object.keys(errors.value).length === 0
}

const submitForm = () => {
  if (!validateForm()) {
    toast.add({
      severity: 'error',
      summary: 'Erro',
      detail: 'Por favor, corrija os campos em vermelho',
      life: 5000,
    })
    return
  }

  const body = [
    `Nome: ${form.nome}`,
    `Email: ${form.email}`,
    `Telefone: ${form.telefone || 'Não informado'}`,
    '',
    'Mensagem:',
    form.mensagem,
  ].join('\n')

  const mailToUrl = `mailto:${contactEmail.value}?subject=${encodeURIComponent(
    `[Sistema de Compras] ${selectedSubjectLabel.value}`,
  )}&body=${encodeURIComponent(body)}`

  window.location.href = mailToUrl

  toast.add({
    severity: 'success',
    summary: 'Pronto',
    detail: 'Seu aplicativo de email foi aberto para concluir o envio da mensagem.',
    life: 5000,
  })

  clearForm()
}

const clearForm = () => {
  form.nome = ''
  form.email = ''
  form.telefone = ''
  form.assunto = ''
  form.mensagem = ''
  errors.value = {}
}

onMounted(() => {
  if (!settings.value) {
    settingsStore.fetchSettings()
  }
})
</script>

<!-- TODO: ajeitar essa pagina -->
<template>
  <div class="min-h-screen py-2">
    <div class="max-w-6xl mx-auto px-2">
      <!-- Header -->
      <div class="mb-4">
        <h2 class="text-xl mb-2">Fale Conosco</h2>
        <p class="max-w-2xl mx-auto">
          Utilize este canal para tirar dúvidas sobre solicitações, catálogo, status e acesso ao
          sistema.
        </p>
      </div>

      <div class="grid">
        <!-- Formulário -->
        <div class="col-12 lg:col-6">
          <div class="rounded-lg shadow-md p-6">
            <h3 class="font-semibold mb-6">Envie sua Mensagem</h3>

            <form @submit.prevent="submitForm" class="space-y-4">
              <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div class="field">
                  <label for="nome" class="block text-sm font-medium mb-2">
                    Nome *
                  </label>
                  <InputText
                    id="nome"
                    v-model="form.nome"
                    placeholder="Seu nome completo"
                    class="w-full"
                    :class="{ 'p-invalid': errors.nome }"
                    size="small"
                  />
                  <small v-if="errors.nome" class="p-error">{{ errors.nome }}</small>
                </div>

                <div class="field">
                  <label for="email" class="block text-sm font-medium mb-2">
                    E-mail *
                  </label>
                  <InputText
                    id="email"
                    v-model="form.email"
                    type="email"
                    placeholder="seu@email.com"
                    class="w-full"
                    :class="{ 'p-invalid': errors.email }"
                    size="small"
                  />
                  <small v-if="errors.email" class="p-error">{{ errors.email }}</small>
                </div>
              </div>

              <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div class="field">
                  <label for="telefone" class="block text-sm font-medium mb-2">
                    Telefone
                  </label>
                  <InputText
                    id="telefone"
                    v-model="form.telefone"
                    placeholder="(79) 99999-9999"
                    class="w-full"
                    size="small"
                  />
                </div>

                <div class="field">
                  <label for="assunto" class="block text-sm font-medium mb-2">
                    Assunto *
                  </label>
                  <Dropdown
                    id="assunto"
                    v-model="form.assunto"
                    :options="assuntos"
                    optionLabel="label"
                    optionValue="value"
                    placeholder="Selecione um assunto"
                    class="w-full"
                    :class="{ 'p-invalid': errors.assunto }"
                    size="small"
                  />
                  <small v-if="errors.assunto" class="p-error">{{ errors.assunto }}</small>
                </div>
              </div>

              <div class="field">
                <label for="mensagem" class="block text-sm font-medium mb-2">
                  Mensagem *
                </label>
                <Textarea
                  id="mensagem"
                  v-model="form.mensagem"
                  rows="6"
                  placeholder="Digite sua mensagem aqui..."
                  class="w-full"
                  :class="{ 'p-invalid': errors.mensagem }"
                  size="small"
                />
                <small v-if="errors.mensagem" class="p-error">{{ errors.mensagem }}</small>
              </div>

              <div class="flex justify-content-end gap-2 pt-2">
                <Button
                  type="button"
                  label="Limpar"
                  icon="pi pi-times"
                  class="p-button-outlined"
                  @click="clearForm"
                  size="small"
                />
                <Button
                  type="submit"
                  label="Enviar Mensagem"
                  icon="pi pi-send"
                  size="small"
                />
              </div>
              <small class="text-color-secondary mt-2 block">
                Ao clicar em <strong>Enviar Mensagem</strong>, seu aplicativo de email será aberto
                para concluir o envio.
              </small>
            </form>
          </div>
        </div>

        <!-- Informações de Contato -->
        <div class="col-12 lg:col-6 mt-4">
          <div class="rounded-lg shadow-md p-6 h-fit">
            <h3 class="font-semibold mb-6">Informações de Contato</h3>

            <div class="flex gap-4">
              <div class="flex items-start gap-2">
                <i class="pi pi-phone text-primary mt-1"></i>
                <div>
                  <span class="font-semibold">Telefone</span>
                  <p>Consulte o setor responsável da sua unidade.</p>
                </div>
              </div>

              <div class="flex items-start gap-2">
                <i class="pi pi-envelope text-primary mt-1"></i>
                <div>
                  <span class="font-semibold">E-mail de contato</span>
                  <p>{{ contactEmail }}</p>
                  <p class="text-sm text-color-secondary">Notificações: {{ notificationEmail }}</p>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- FAQ Section -->
      <div class="mt-12">
        <div class="rounded-lg shadow-md p-4">
          <h3 class="font-semibold mb-4">Perguntas Frequentes</h3>

          <Accordion :activeIndex="0">
            <AccordionTab header="Como acompanho o status das minhas solicitações?">
              <p>
                Acesse <strong>Solicitações &gt; Minhas Solicitações</strong>, use os filtros e
                clique em <strong>Ver Detalhes</strong> para visualizar status, histórico e
                observações.
              </p>
            </AccordionTab>

            <AccordionTab header="Por que não consigo enviar ou editar minha solicitação?">
              <p>
                O sistema aplica o prazo final de submissão e ajustes definido pela gestão. Após o
                vencimento, envio e edição ficam bloqueados.
              </p>
            </AccordionTab>

            <AccordionTab header="Posso ter uma solicitação Geral e Patrimonial em andamento ao mesmo tempo?">
              <p>
                Não. O sistema mantém apenas <strong>uma solicitação em andamento</strong> no
                carrinho por vez.
              </p>
            </AccordionTab>

            <AccordionTab header="O que acontece se eu começar uma solicitação de outro tipo?">
              <p>
                Se você iniciou uma solicitação <strong>Geral</strong> e abrir a criação
                <strong>Patrimonial</strong> (ou o contrário), o sistema pede confirmação para
                descartar a solicitação atual antes de continuar.
              </p>
            </AccordionTab>

            <AccordionTab header="E se eu sair da tela com alterações não salvas?">
              <p>
                O sistema exibe um alerta para evitar perda de dados e permite decidir se deseja
                continuar ou descartar as alterações.
              </p>
            </AccordionTab>

            <AccordionTab header="Quais são as regras de justificativa da solicitação?">
              <p>
                Em solicitação <strong>Geral</strong>, a <strong>Justificativa Geral</strong> é
                obrigatória. Em solicitação <strong>Patrimonial</strong>, cada item deve ter sua
                própria justificativa.
              </p>
            </AccordionTab>

            <AccordionTab header="Existe limite de itens por solicitação?">
              <p>
                Sim. O gestor configura limites como quantidade máxima por item e quantidade máxima
                de itens diferentes por solicitação.
              </p>
            </AccordionTab>

            <AccordionTab header="Quais status permitem que eu edite a solicitação?">
              <p>
                A edição pelo solicitante é permitida, em regra, quando a solicitação está em
                <strong>Pendente</strong> ou <strong>Aguardando Ajustes</strong> e dentro do prazo
                definido.
              </p>
            </AccordionTab>

            <AccordionTab header="Por que um item não pode ser adicionado ou ter quantidade aumentada?">
              <p>
                Isso ocorre quando a solicitação atinge os limites configurados de
                <strong>itens diferentes</strong> ou de <strong>quantidade máxima por item</strong>.
              </p>
            </AccordionTab>
          </Accordion>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.field {
  margin-bottom: 1rem;
}

.field-checkbox {
  margin-bottom: 1rem;
  display: flex;
  align-items: flex-start;
}

.p-error {
  color: #e24c4c;
  font-size: 0.875rem;
  margin-top: 0.25rem;
  display: block;
}

.p-invalid {
  border-color: #e24c4c;
}

.p-button {
  transition: all 0.3s ease;
}

.p-button:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
}

.shadow-md {
  box-shadow:
    0 4px 6px -1px rgba(0, 0, 0, 0.1),
    0 2px 4px -1px rgba(0, 0, 0, 0.06);
}

.rounded-lg {
  border-radius: 0.5rem;
}

.text-primary {
  color: var(--primary-color, #007bff);
}

.hover\:underline:hover {
  text-decoration: underline;
}
</style>
