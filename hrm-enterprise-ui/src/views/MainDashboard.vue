<template>
  <div>
    <div class="d-flex justify-space-between align-center mb-6">
      <div>
        <h1 class="text-h4 font-weight-bold tracking-tight">Hệ thống Điều hành Tổng hợp</h1>
        <p class="text-subtitle-2 text-disabled">Dữ liệu thời gian thực đồng bộ từ các Microservices</p>
      </div>
      <!-- Kích hoạt hàm fetchDashboardStats khi click nút đồng bộ -->
      <v-btn color="primary" prepend-icon="mdi-refresh" variant="tonal" @click="fetchDashboardStats">
        Đồng bộ lại dữ liệu
      </v-btn>
    </div>

    <v-row class="mb-6">
      <v-col cols="12" sm="6" lg="3" v-for="(kpi, i) in kpis" :key="i">
        <v-card class="pa-5 relative-card overflow-hidden">
          <div class="d-flex justify-space-between align-start">
            <div>
              <span class="text-overline text-disabled font-weight-bold">{{ kpi.title }}</span>
              <h2 class="text-h3 font-weight-black my-2">{{ kpi.value }}</h2>
            </div>
            <v-avatar :color="kpi.color" variant="tonal" rounded="lg">
              <v-icon>{{ kpi.icon }}</v-icon>
            </v-avatar>
          </div>
          <div class="mt-2 text-caption d-flex align-center">
            <v-icon color="success" size="16" class="mr-1">mdi-trending-up</v-icon>
            <span class="text-success font-weight-bold mr-1">{{ kpi.change }}</span>
            <span class="text-disabled">so với tháng trước</span>
          </div>
          <div class="card-glow" :style="{ background: kpi.glowColor }"></div>
        </v-card>
      </v-col>
    </v-row>

    <v-row>
      <v-col cols="12" lg="8">
        <v-card class="pa-6 fill-height">
          <h3 class="text-title font-weight-bold mb-4">Xu hướng Chi phí Lương & Chấm công</h3>
          <div class="chart-container d-flex align-center justify-center text-disabled border border-dashed rounded-xl" style="height: 320px;">
            <div class="text-center">
              <v-icon size="48" class="mb-2">mdi-chart-box-outline</v-icon>
              <p>[Biểu đồ Trực quan hóa tích hợp từ Phân hệ Payroll & Report Service]</p>
            </div>
          </div>
        </v-card>
      </v-col>

      <v-col cols="12" lg="4">
        <v-card class="pa-6 fill-height">
          <h3 class="text-title font-weight-bold mb-4">Hồ sơ Sự kiện Thực tế (Event-Driven Logs)</h3>
          <v-timeline density="compact" align="start">
            <v-timeline-item dot-color="primary" size="x-small">
              <div class="mb-1 text-caption font-weight-bold">Event: employee.created</div>
              <p class="text-caption text-disabled">Đồng bộ nhân viên Nguyễn Văn Vượng sang AttendanceDB và PayrollDB thành công.</p>
            </v-timeline-item>
            <v-timeline-item dot-color="success" size="x-small">
              <div class="mb-1 text-caption font-weight-bold">Event: attendance.monthly.closed</div>
              <p class="text-caption text-disabled">Đóng bảng chấm công tháng 06. Payroll Service tự động kích hoạt tính lương.</p>
            </v-timeline-item>
          </v-timeline>
        </v-card>
      </v-col>
    </v-row>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'

// Khai báo các biến lưu trữ giá trị số liệu động mặc định bằng 0
const totalEmp = ref(0)
const activeEmp = ref(0)
const probationEmp = ref(0)
const officialEmp = ref(0)

// Map biến động vào cấu trúc mảng KPIs để hiển thị lên Vuetify Card
const kpis = computed(() => [
  { title: 'Tổng nhân sự', value: totalEmp.value || 0, icon: 'mdi-account-group-outline', color: 'primary', change: '+4.2%', glowColor: 'rgba(99,102,241,0.08)' },
  { title: 'Nhân sự hoạt động', value: activeEmp.value || 0, icon: 'mdi-account-check-outline', color: 'success', change: '+1.5%', glowColor: 'rgba(16,185,129,0.08)' },
  { title: 'Nhân sự thử việc', value: probationEmp.value || 0, icon: 'mdi-clock-outline', color: 'warning', change: '-5%', glowColor: 'rgba(245,158,11,0.08)' },
  { title: 'Nhân sự chính thức', value: officialEmp.value || 0, icon: 'mdi-file-certificate-outline', color: 'accent', change: '+2.1%', glowColor: 'rgba(168,85,247,0.08)' }
])

// HÀM GỌI API ĐỂ THU THẬP SỐ LIỆU ĐỘNG TỪ SQL SERVER QUA GATEWAY
const fetchDashboardStats = async () => {
  try {
    // Gọi qua Ocelot Gateway cổng 8000 điều hướng trực tiếp vào API của HRController.cs
    const response = await fetch('http://localhost:8000/api/hr/dashboard/stats')
    if (response.ok) {
      const data = await response.json()
      
      // Đổ dữ liệu thật nhận được từ API vào các biến phản xạ (ref)
      totalEmp.value = data.total
      activeEmp.value = data.active
      probationEmp.value = data.probation
      officialEmp.value = data.official
    } else {
      console.error('Lỗi phản hồi từ API Gateway khi lấy số liệu Dashboard.')
    }
  } catch (error) {
    console.error('Không thể kết nối đến API Gateway cổng 8000:', error)
  }
}

// Tự động chạy hàm cập nhật số liệu ngay khi mở trang Dashboard
onMounted(() => {
  fetchDashboardStats()
})
</script>

<style scoped>
.relative-card {
  position: relative;
}
.card-glow {
  position: absolute;
  top: -50%;
  right: -20%;
  width: 150px;
  height: 150px;
  filter: blur(40px);
  border-radius: 50%;
  pointer-events: none;
}
</style>