import { defineConfig } from 'cypress'

export default defineConfig({
  e2e: {
    specPattern: 'cypress/e2e/**/*.{cy,spec}.{js,jsx,ts,tsx}',
    baseUrl: process.env.CYPRESS_baseUrl || 'http://localhost:5173',
    
    env: {
      apiUrl: 'http://backend-service:8080',
      frontendWrongUrl: 'http://localhost:5000'
    },
    video: true,
    screenshotOnRunFailure: true,
     
    setupNodeEvents(on, config) {
      on('task', {
        log(message) {
          console.log(message)
          return null
        },
        table(message) {
          console.table(message)
          return null
        }
      })
      
      return config
    },
  },
})