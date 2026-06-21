<template>
  <v-container fluid class="fill-height login-bg px-4">
    <v-row justify="center" align="center" class="w-100">
      <v-col cols="12" sm="8" md="5" lg="4">
        <v-card class="pa-8 text-center text-surface-variant rounded-xl border border-slate-800" color="#111827">
          <div class="d-flex align-center justify-center mb-4">
            <v-icon color="primary" size="40" class="mr-2">mdi-layers-triple</v-icon>
            <span class="text-h4 font-weight-black text-gradient">NEXUS HRM</span>
          </div>
          <p class="text-subtitle-1 text-muted mb-6">Hệ thống Quản trị Nhân sự kiến trúc Microservices</p>

          <v-form @submit.prevent="handleLogin">
            <v-text-field
              v-model="username"
              label="Tên tài khoản (Username)"
              prepend-inner-icon="mdi-account-outline"
              variant="outlined"
              color="primary"
              flat
              class="mb-2 text-white"
              :disabled="loading"
            ></v-text-field>

            <v-text-field
              v-model="password"
              :type="showPassword ? 'text' : 'password'"
              label="Mật khẩu"
              prepend-inner-icon="mdi-lock-outline"
              :append-inner-icon="showPassword ? 'mdi-eye-off' : 'mdi-eye'"
              @click:append-inner="showPassword = !showPassword"
              variant="outlined"
              color="primary"
              flat
              class="mb-2 text-white"
              :disabled="loading"
            ></v-text-field>

            <div class="d-flex align-center justify-space-between mb-4">
              <v-checkbox v-model="rememberMe" label="Ghi nhớ đăng nhập" hide-details color="primary" :disabled="loading"></v-checkbox>
              <a href="#" class="text-caption text-primary text-decoration-none">Quên mật khẩu?</a>
            </div>

            <v-btn 
              type="submit" 
              block 
              size="large" 
              color="primary" 
              class="gradient-btn mb-4 font-weight-bold"
              :loading="loading"
            >
              Đăng nhập vào Hệ thống
            </v-btn>
          </v-form>

          <v-alert
            v-if="errorMessage"
            type="error"
            variant="tonal"
            class="rounded-lg font-weight-medium text-sm text-left mb-4"
          >
            {{ errorMessage }}
          </v-alert>

          <div class="d-flex align-center my-4">
            <v-divider></v-divider>
            <span class="px-3 text-caption text-disabled">HOẶC</span>
            <v-divider></v-divider>
          </div>

          <v-row dense>
            <v-col cols="6"><v-btn block variant="outlined" prepend-icon="mdi-google" :disabled="loading">Google</v-btn></v-col>
            <v-col cols="6"><v-btn block variant="outlined" prepend-icon="mdi-microsoft-azure" :disabled="loading">Azure ID</v-btn></v-col>
          </v-row>
        </v-card>
      </v-col>
    </v-row>
  </v-container>
</template>

<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'

const router = useRouter()
const username = ref('')
const password = ref('')
const showPassword = ref(false)
const rememberMe = ref(true)

const loading = ref(false)
const errorMessage = ref('')

const handleLogin = async () => {
  if (!username.value.trim() || !password.value.trim()) {
    errorMessage.value = "Vui lòng nhập đầy đủ Tài khoản và Mật khẩu!"
    return
  }

  loading.value = true
  errorMessage.value = ''

  try {
    // 🚀 ĐỒNG BỘ: Bắn token đăng nhập chuẩn qua API GATEWAY CỔNG 8001
    const response = await fetch('http://localhost:8001/api/auth/login', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        username: username.value.trim(),
        password: password.value.trim()
      })
    })

    const data = await response.json()

    if (response.ok) {
      // 💾 Lưu toàn bộ phiên làm việc thật (role, employeeId, fullName) vào máy
      localStorage.setItem('user_login', JSON.stringify(data))
      localStorage.setItem('user_role', data.role) 

      // 🧭 Điều hướng đồng bộ dải URL chứa tiền tố hệ thống /sys/
      if (data.role === 'Admin' || data.role === 'Manager') {
        router.push('/sys/dashboard') // Các cấp quản lý đưa về Dashboard điều hành tổng hợp
      } else {
        router.push('/sys/attendance-check') // Nhân viên bình thường đưa thẳng vào máy quét chấm công
      }
    } else {
      errorMessage.value = data.message || "Tài khoản hoặc mật khẩu không chính xác!"
    }
  } catch (error) {
    console.error(error)
    // 📝 Đổi nội dung chuỗi log lỗi hiển thị cổng 8001 cho đồng bộ kiến trúc
    errorMessage.value = "Lỗi kết nối nghiêm trọng đến Gateway cổng 8001!"
  } finally {
    loading.value = false
  }
}
</script>

<style scoped>
.login-bg {
  background: radial-gradient(circle at top right, rgba(99,102,241,0.15), transparent),
              radial-gradient(circle at bottom left, rgba(168,85,247,0.15), transparent),
              #090d16;
}
.text-gradient {
  background: linear-gradient(45deg, #6366f1, #00f2fe);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  font-family: 'SF Pro Display', -apple-system, sans-serif;
}
</style>