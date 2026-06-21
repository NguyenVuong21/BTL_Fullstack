<template>
  <v-container fluid class="fill-height login-bg px-4">
    <v-row justify="center" align="center" class="w-100">
      <v-col cols="12" sm="8" md="6" lg="5">
        <v-card class="pa-6 text-center text-surface-variant rounded-xl border border-slate-800" color="#111827">
          
          <div class="mb-4">
            <v-icon color="primary" size="48" class="mb-2">mdi-clock-digital</v-icon>
            <h1 class="text-h4 font-weight-black text-white">TRẠM CHẤM CÔNG</h1>
            <p class="text-subtitle-2 text-disabled">Hệ thống ghi nhận thời gian thực qua API Gateway</p>
          </div>

          <v-alert variant="tonal" color="cyan" class="mb-6 rounded-xl text-left">
            <div class="d-flex align-center">
              <v-icon size="32" class="mr-3">mdi-account-circle-outline</v-icon>
              <div>
                <div class="text-subtitle-1 font-weight-bold text-white">{{ currentUser.fullName || 'Nhân viên Nexus' }}</div>
                <div class="text-caption text-slate-400">Mã NV: <span class="font-weight-bold text-cyan-accent-2">{{ currentUser.employeeId }}</span> | Chức vụ: {{ currentUser.role }}</div>
              </div>
            </div>
          </v-alert>

          <div class="my-8">
            <v-btn
              :loading="loading"
              color="primary"
              size="x-large"
              block
              class="gradient-btn py-8 font-weight-bold text-h5 rounded-xl"
              prepend-icon="mdi-fingerprint"
              @click="handleCheckIn"
            >
              BẤM ĐỂ CHẤM CÔNG
            </v-btn>
          </div>

          <v-alert
            v-if="statusMessage"
            :type="statusType"
            variant="tonal"
            class="rounded-lg font-weight-medium text-left mb-4"
          >
            {{ statusMessage }}
          </v-alert>

          <v-divider class="my-4"></v-divider>
          <div class="text-left text-subtitle-2 text-disabled mb-2">Nhật ký hoạt động gần đây:</div>
          <v-card variant="outlined" class="border-slate-800 rounded-lg pa-3 bg-slate-950 text-left">
            <div class="d-flex justify-space-between text-caption text-slate-400">
              <span>Phương thức kết nối:</span>
              <span class="text-success font-weight-bold">Ocelot Gateway (Port 8001)</span>
            </div>
          </v-card>

        </v-card>
      </v-col>
    </v-row>
  </v-container>
</template>

<script setup>
import { ref, onMounted } from 'vue'

const currentUser = ref({
  employeeId: '',
  fullName: '',
  role: ''
})

const loading = ref(false)
const statusMessage = ref('')
const statusType = ref('info')

// Lấy thông tin user đăng nhập từ localStorage khi load trang
onMounted(() => {
  const sessionData = localStorage.getItem('user_login')
  if (sessionData) {
    const user = JSON.parse(sessionData)
    currentUser.value = {
      employeeId: user.employeeId || 'NV' + Math.floor(Math.random() * 1000),
      fullName: user.fullName || user.username,
      role: user.role || 'Employee'
    }
  } else {
    currentUser.value = { employeeId: 'NHANVIEN04', fullName: 'Nguyễn Văn Nhân Viên', role: 'Employee' }
  }
})

// Hàm xử lý khi ấn nút chấm công điện tử
const handleCheckIn = async () => {
  loading.value = true
  statusMessage.value = ''

  try {
    // 🚀 BẮN DỮ LIỆU ĐẾN API GATEWAY CỔNG 8001 CHUẨN XÁC
    const response = await fetch('http://localhost:8001/api/hr/attendance/check', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({
        employeeId: currentUser.value.employeeId
      })
    })

    const data = await response.json()

    if (response.ok) {
      statusType.value = 'success'
      statusMessage.value = data.message || `Chấm công thành công lúc ${new Date().toLocaleTimeString()}!`
    } else {
      statusType.value = 'error'
      statusMessage.value = data.message || 'Giao dịch chấm công bị từ chối!'
    }
  } catch (error) {
    console.error(error)
    statusType.value = 'error'
    statusMessage.value = 'Không thể kết nối đến API Gateway cổng 8001! Bạn nhớ kiểm tra các service nhé.'
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
.gradient-btn {
  background: linear-gradient(45deg, #6366f1, #a855f7) !important;
  transition: transform 0.2s;
}
.gradient-btn:hover {
  transform: scale(1.02);
}
</style>