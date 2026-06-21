<template>
  <v-row justify="center" class="pa-4">
    <v-col cols="12" md="6">
      <v-card class="pa-6 text-center text-surface-variant relative-card border border-slate-800 rounded-xl card-glass">
        <h2 class="text-h5 font-weight-bold mb-2 text-white">Trạm Chấm công Điện tử</h2>
        <p class="text-subtitle-2 text-disabled mb-6">Phân hệ Chấm công Thời gian thực - Attendance Service</p>

        <div v-if="userSession.fullName" class="text-subtitle-1 text-white mb-2">
          Xin chào, <span class="font-weight-bold text-cyan-accent-2">{{ userSession.fullName }}</span>
        </div>

        <div class="time-display my-6">
          <div class="text-h2 font-weight-black text-cyan-accent-2 tracking-widest font-mono">{{ currentTime }}</div>
          <div class="text-subtitle-1 text-primary font-weight-bold mt-2">{{ currentDate }}</div>
        </div>

        <v-sheet class="mx-auto rounded-xl face-mockup mb-6 d-flex flex-column align-center justify-center" max-width="340" height="220" color="black">
          <v-icon size="64" color="rgba(255,255,255,0.15)">mdi-face-recognition</v-icon>
          <div class="face-scan-line"></div>
          <div class="text-caption text-success mt-2 font-weight-bold absolute-bottom">
            <v-icon size="14" color="success" class="mr-1">mdi-map-marker-radius</v-icon>
            Định vị GPS: Trụ sở chính văn phòng (Hợp lệ)
          </div>
        </v-sheet>

        <v-alert v-if="statusMessage" :type="isError ? 'error' : 'success'" variant="tonal" class="mb-6 rounded-lg text-left">
          {{ statusMessage }}
        </v-alert>

        <v-row>
          <v-col cols="6">
            <v-btn 
              :loading="loading"
              block 
              size="large" 
              color="success" 
              class="gradient-btn-green py-4 font-weight-bold text-white" 
              height="54" 
              prepend-icon="mdi-login" 
              @click="handleCheckIn"
            >
              Check-In Ca Sáng
            </v-btn>
          </v-col>
          <v-col cols="6">
        <v-btn 
              :loading="loading"
              block 
              size="large" 
              color="error" 
              variant="outlined" 
              class="font-weight-bold" 
              height="54" 
              prepend-icon="mdi-logout" 
              @click="handleCheckOut"
            >
             Check-Out Ca Chiều
           </v-btn>
        </v-col>
        </v-row>
      </v-card>
    </v-col>
  </v-row>
</template>

<script setup>
import { ref, onMounted, onUnmounted } from 'vue'

// --- KHAI BÁO BIẾN TRẠNG THÁI ---
const currentTime = ref('08:00:00')
const currentDate = ref('')
const loading = ref(false)
const statusMessage = ref('')
const isError = ref(false)

// Session người dùng lưu từ lúc đăng nhập thật
const userSession = ref({
  username: '',
  role: '',
  employeeId: '',
  fullName: ''
})

// --- ĐỒNG HỒ ĐẾM GIỜ REALTIME HỆ THỐNG ---
const updateTime = () => {
  const now = new Date()
  currentTime.value = now.toTimeString().split(' ')[0]
  currentDate.value = now.toLocaleDateString('vi-VN', { 
    weekday: 'long', 
    year: 'numeric', 
    month: 'long', 
    day: 'numeric' 
  })
}

let timer
onMounted(() => {
  updateTime()
  timer = setInterval(updateTime, 1000)

  // 🚀 ĐỌC DATA ĐĂNG NHẬP THẬT TỪ BỘ NHỚ TRÌNH DUYỆT
  const savedUser = localStorage.getItem('user_login')
  if (savedUser) {
    userSession.value = JSON.parse(savedUser)
  } else {
    statusMessage.value = '⚠️ Không tìm thấy phiên làm việc! Vui lòng quay lại trang đăng nhập.'
    isError.value = true
  }
})

onUnmounted(() => {
  if (timer) clearInterval(timer)
})

// --- HÀM XỬ LÝ GỌI API CHẤM CÔNG CHUNG XUỐNG GATEWAY CỔNG 8001 ---
const postAttendance = async () => {
  if (!userSession.value.employeeId) {
    statusMessage.value = 'Lỗi: Không tìm thấy Mã nhân viên để truyền dữ liệu!'
    isError.value = true
    return
  }

  loading.value = true
  statusMessage.value = ''
  isError.value = false

  try {
    // 🚀 ĐÃ SỬA: Đổi cổng kết nối từ 8000 sang 8001 chuẩn API Gateway Ocelot
    const response = await fetch('http://localhost:8001/api/hr/attendance/check', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({
        employeeId: userSession.value.employeeId // Truyền dynamic id thật từ tài khoản login
      })
    })

    const data = await response.json()

    if (response.ok) {
      statusMessage.value = data.message // Đẩy chuỗi phản hồi động "Chào buổi sáng/Tạm biệt..." ra v-alert
      isError.value = false
    } else {
      statusMessage.value = data.message || 'Chấm công thất bại!'
      isError.value = true
    }
  } catch (error) {
    console.error('Lỗi kết nối API chấm công:', error)
    // 📝 Cập nhật thông báo lỗi hiển thị cho đồng bộ sang cổng 8001
    statusMessage.value = 'Không thể kết nối đến API Gateway cổng 8001! Bạn nhớ kiểm tra các service nhé.'
    isError.value = true
  } finally {
    loading.value = false
  }
}

// --- LIÊN KẾT LUỒNG VÀO VĂN BẢN ĐIỂM DANH ---
const handleCheckIn = () => {
  postAttendance()
}

const handleCheckOut = () => {
  postAttendance()
}
</script>

<style scoped>
.card-glass {
  background: rgba(17, 24, 39, 0.85) !important;
  backdrop-filter: blur(20px);
}
.font-mono {
  font-family: 'Courier New', Courier, monospace;
}
.face-mockup {
  border: 2px solid rgba(99, 102, 241, 0.4);
  position: relative;
  overflow: hidden;
}
.face-scan-line {
  position: absolute;
  width: 100%;
  height: 2px;
  background: linear-gradient(to right, transparent, #00f2fe, transparent);
  animation: scan 2.5s infinite linear;
}
@keyframes scan {
  0% { top: 0%; }
  50% { top: 100%; }
  100% { top: 0%; }
}
.absolute-bottom {
  position: absolute;
  bottom: 12px;
}
.gradient-btn-green {
  background: linear-gradient(45deg, #2e7d32, #4caf50) !important;
}
</style>