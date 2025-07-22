import './assets/main.css'

import { createApp } from 'vue'
import { createPinia } from 'pinia'
import piniaPluginPersistedstate from 'pinia-plugin-persistedstate'
import PrimeVue from 'primevue/config'
import 'primeflex/primeflex.css'
import MyPreset from './theme'
import 'primeicons/primeicons.css'
import Tooltip from 'primevue/tooltip'
import ToastService from 'primevue/toastservice';
import App from './App.vue'
import router from './router'
import Ripple from 'primevue/ripple';

const app = createApp(App)

const pinia = createPinia()
app.use(pinia)
pinia.use(piniaPluginPersistedstate)

app.use(PrimeVue, {
  theme: {
    preset: MyPreset,
    options: {
      prefix: 'p',
      darkModeSelector: '.p-dark'
    },
  },
  ripple: true
})
app.use(ToastService)
app.use(router)

app.directive('ripple', Ripple)
app.directive('tooltip', Tooltip)

app.mount('#app')