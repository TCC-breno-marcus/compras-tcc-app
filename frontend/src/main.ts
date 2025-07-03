import './assets/main.css'

import { createApp } from 'vue'
import { createPinia } from 'pinia'
import PrimeVue from 'primevue/config';
import 'primeflex/primeflex.css'
import MyPreset from './theme'
import 'primeicons/primeicons.css'
import Tooltip from 'primevue/tooltip';

import App from './App.vue'
import router from './router'

const app = createApp(App)

app.use(createPinia())
app.use(PrimeVue, {
  theme: {
    preset: MyPreset,
    options: {
      prefix: 'p',
      darkModeSelector: '.p-dark',
      cssLayer: {
        name: 'primevue',
        order: 'app-styles, primevue, another-css-library'
    }
    }
  }
});

app.use(router)

app.directive('tooltip', Tooltip);

app.mount('#app')
