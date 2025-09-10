<template>
  <div class="logo-link logo-avancado-wrapper">
    <span class="material-symbols-outlined">shopping_cart</span>
    <h2 class="logo-gradiente">SIGAM-UFS</h2>
  </div>
</template>

<style scoped>
.logo-link {
  text-decoration: none;
}

.logo-avancado-wrapper {
  display: flex;
  align-items: center;
  gap: 10px;
}

.logo-avancado-wrapper span {
  font-size: 1.8rem;
  color: var(--p-primary-500);
  font-weight: bold;
}

.logo-gradiente {
  font-family: 'Poppins', sans-serif;
  font-weight: 800;
  margin: 0;
  background: linear-gradient(45deg, var(--p-primary-500), var(--p-surface-800));
  -webkit-background-clip: text;
  background-clip: text;
  -webkit-text-fill-color: transparent;
  color: transparent;
}
</style>
