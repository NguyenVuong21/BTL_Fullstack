<template>
  <div>
    <div class="d-flex justify-space-between align-center mb-6">
      <div>
        <h1 class="text-h4 font-weight-bold tracking-tight">Hệ thống Điều hành Tổng hợp</h1>
        <p class="text-subtitle-2 text-disabled">Dữ liệu thời gian thực đồng bộ từ các Microservices</p>
      </div>
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

// Biến lưu trữ giá trị đếm động từ Database
const totalEmp = ref(0)
const activeEmp = ref(0)
const probationEmp = ref(0)
const officialEmp = ref(0)

// Cấu hình các thẻ hiển thị số liệu lên màn hình
const kpis = computed(() => [
  { title: 'Tổng nhân sự', value: totalEmp.value, icon: 'mdi-account-group-outline', color: 'primary', change: '+4.2%', glowColor: 'rgba(99,102,241,0.08)' },
  { title: 'Nhân sự hoạt động', value: activeEmp.value, icon: 'mdi-account-check-outline', color: 'success', change: '+1.5%', glowColor: 'rgba(16,185,129,0.08)' },
  { title: 'Nhân sự thử việc', value: probationEmp.value, icon: 'mdi-clock-outline', color: 'warning', change: '-5%', glowColor: 'rgba(245,158,11,0.08)' },
  { title: 'Nhân sự chính thức', value: officialEmp.value, icon: 'mdi-file-certificate-outline', color: 'accent', change: '+2.1%', glowColor: 'rgba(168,85,247,0.08)' }
])

// 🚀 HÀM GỌI API THẬT QUA GATEWAY ĐỂ ĐẾM SỐ LIỆU ĐỘNG TỪ SQL SERVER
const fetchDashboardStats = async () => {
  try {
    console.log('Đang kết nối API Gateway cổng 8001 để lấy danh sách nhân sự thật...')
    const response = await fetch('http://localhost:8001/api/hr/employees')
    
    if (response.ok) {
      const employees = await response.json()
      console.log('Dữ liệu nhân viên thực tế nhận từ SQL Server:', employees)
      
      if (Array.isArray(employees)) {
        // 📊 1. Đếm tổng số nhân viên đang có trong DB
        totalEmp.value = employees.length
        
        // 📊 2. Lọc nhân viên đang hoạt động (tự động kiểm tra cả tiếng Anh/Việt/Mã số tùy Toàn viết)
        activeEmp.value = employees.filter(emp => 
          emp.status === 'Active' || 
          emp.status === 'Hoạt động' || 
          emp.trangThai === 1 ||
          emp.status === true
        ).length
        
        // 📊 3. Lọc nhân viên thử việc dựa theo loại hợp đồng hoặc trạng thái dữ liệu thật
        probationEmp.value = employees.filter(emp => 
          emp.status === 'Probation' || 
          emp.status === 'Thử việc' || 
          emp.contractType?.toLowerCase().includes('thử việc') ||
          emp.contractType?.toLowerCase().includes('probation')
        ).length
        
        // 📊 4. Tính toán số nhân sự chính thức còn lại
        officialEmp.value = totalEmp.value - probationEmp.value
        
        console.log('Đồng bộ số liệu Dashboard thật thành công rực rỡ!')
      }
    } else {
      console.error('Lỗi phản hồi từ Gateway. Mã lỗi:', response.status)
    }
  } catch (error) {
    console.error('Lỗi kết nối mạng đến API Gateway:', error)
  }
}

// Tự động kích hoạt khi load trang
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