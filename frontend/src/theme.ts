// src/theme.ts

import { definePreset } from '@primeuix/themes'
import Aura from '@primeuix/themes/aura'

const MyPreset = definePreset(Aura, {
  semantic: {
    // Cores Semânticas Principais
    primary: {
      50: '#eff8ff',
      100: '#dbeefe',
      200: '#c0e2fd',
      300: '#94d0fc',
      400: '#62b5f8',
      500: '#3d97f4', // Fourth
      600: '#2779e9',
      700: '#1f63d6',
      800: '#2050ad',
      900: '#1f4789',
      950: '#1b325f', // First
    },
    secondary: {
      '50': '#f2f7fc',
      '100': '#e2edf7',
      '200': '#ccdff1',
      '300': '#9cc4e4', // Second
      '400': '#7fb0db',
      '500': '#6194d0',
      '600': '#4d7cc3',
      '700': '#4369b2',
      '800': '#3c5791',
      '900': '#344a74',
      '950': '#232f48',
    },
    success: {
      '50': '#edfcf6',
      '100': '#d3f8e7',
      '200': '#aaf0d4',
      '300': '#73e2bc',
      '400': '#3bcc9f',
      '500': '#17b287', // Green
      '600': '#0b906e',
      '700': '#09735b',
      '800': '#0a5b48',
      '900': '#094b3e',
      '950': '#042a23',
    },
    warning: {
      '50': '#fefbe8',
      '100': '#fdf5c4',
      '200': '#fce98c',
      '300': '#fad64a',
      '400': '#f6bf19',
      '500': '#e8a80c', // Yellow
      '600': '#c78007',
      '700': '#9e5b0a',
      '800': '#834810',
      '900': '#6f3b14',
      '950': '#411d07',
    },
    danger: {
      '50': '#fef4f2',
      '100': '#fee7e2',
      '200': '#ffd3c9',
      '300': '#fdb4a4',
      '400': '#fa886f',
      '500': '#f26c4f', // Laranja
      '600': '#de4524',
      '700': '#bb371a',
      '800': '#9b3019',
      '900': '#802e1c',
      '950': '#461409',
    },
    // CORES DE SUPERFÍCIE, TEXTO E BORDAS (O MAIS IMPORTANTE!)
    surface: {
      0: '#F8F9FA', // Background
      50: '#F8F9FA', // Fundo principal
      100: '#E9F2F9', // Third (ideal para fundo de item com hover)
      200: '#DEE2E6', // Border (bordas suaves)
      300: '#ced4da', // Borda de input/hover
      400: '#adb5bd', // Borda de input/focus
      500: '#6c757d', // Cor de texto secundária (Text2)
      600: '#495057',
      700: '#343a40',
      800: '#212529', // Cor de texto principal (Text)
      900: '#1a1e21',
      950: '#101214',
    },

    textColor: '#212529', // Text
    mutedTextColor: '#6C757D', // Text2
  },

  // Suas personalizações de componentes
  components: {
    button: {
      root: {
        borderRadius: '8px',
        paddingY: '4px',
      },
    },
    card: {
      root: {
        borderRadius: '12px',
        background: '{surface.0}',
      },
    },
    inputtext: {
      root: {
        borderRadius: '10px',
        borderColor: '{surface.300}',
        paddingY: '4px',
      },
    },
    select: {
      root: {
        paddingY: '4px',
        borderRadius: '10px',
      },
    },
    toolbar: {
      root: {
        padding: '6px',
        borderColor: 'transparent',
      },
    },
    tag: {
      root: {
        fontSize: '12px',
      },
    }
  },
})

export default MyPreset
