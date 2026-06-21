import '@mdi/font/css/materialdesignicons.css'
import 'vuetify/styles'
import { createVuetify } from 'vuetify'
import * as components from 'vuetify/components'
import * as directives from 'vuetify/directives'

const darkGlassTheme = {
  dark: true,
  colors: {
    background: '#090d16',
    surface: 'rgba(17, 25, 40, 0.65)',
    primary: '#6366f1',
    secondary: '#00f2fe',
    accent: '#a855f7',
    success: '#10b981',
    warning: '#f59e0b',
    error: '#ef4444',
    info: '#3b82f6'
  }
}

const lightGlassTheme = {
  dark: false,
  colors: {
    background: '#f8fafc',
    surface: 'rgba(255, 255, 255, 0.75)',
    primary: '#4f46e5',
    secondary: '#06b6d4',
    accent: '#8b5cf6',
    success: '#10b981',
    warning: '#f59e0b',
    error: '#ef4444',
    info: '#3b82f6'
  }
}

export default createVuetify({
  components,
  directives,
  theme: {
    defaultTheme: 'darkGlassTheme',
    themes: {
      darkGlassTheme,
      lightGlassTheme
    }
  },
  defaults: {
    VCard: {
      elevation: 0,
      rounded: 'xl',
      style: 'backdrop-filter: blur(16px) saturate(180%); border: 1px solid rgba(255,255,255,0.08);'
    },
    VBtn: {
      rounded: 'lg',
      style: 'text-transform: none; font-weight: 600; letter-spacing: 0px;'
    }
  }
})