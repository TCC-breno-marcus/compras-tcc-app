<template>
  <div class="server-error-container">
    <h1>500</h1>
    <h2>Erro Interno do Servidor</h2>
    <p>Ocorreu um problema inesperado em nossos sistemas. Nossa equipe já foi notificada.</p>
    <p>Por favor, tente novamente mais tarde.</p>
    <button @click="$router.push('/')">Voltar para a Home</button>
  </div>
</template>

<style scoped>
.server-error-container {
  text-align: center;
  margin-top: 50px;
}
</style>