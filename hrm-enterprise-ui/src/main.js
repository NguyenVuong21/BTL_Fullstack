import { createApp } from 'vue'
import App from './App.vue'
import vuetify from './plugins/vuetify'
import router from './router'
import { createPinia } from 'pinia'
import './assets/main.scss' // Nếu bạn có dùng file scss tùy chỉnh

const app = createApp(App)
app.use(vuetify)
app.use(router)
app.use(createPinia())
app.mount('#app')