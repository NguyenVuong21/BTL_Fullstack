<template>
  <v-container fluid>
    <div class="mb-6">
      <h1 class="text-h4 font-weight-bold text-white">Phiếu lương Chi tiết</h1>
      <p class="text-subtitle-2 text-disabled">Tra cứu thu nhập, tiền công và các khoản trích khấu trừ theo tháng</p>
    </div>

    <v-card v-if="loading" class="pa-12 text-center rounded-xl border border-slate-800 mx-auto" color="#111827" max-width="800">
        <v-progress-circular indeterminate color="primary" size="64"></v-progress-circular>
        <div class="text-subtitle-1 text-disabled mt-4">Đang tính toán bảng lương động từ Database...</div>
    </v-card>

    <v-card v-else class="pa-6 border border-slate-800 rounded-xl mx-auto" color="#111827" max-width="800">
      <div class="d-flex justify-space-between align-center border-b border-slate-800 pb-4 mb-6">
        <div>
          <div class="text-h5 font-weight-black text-cyan-accent-2">PHIẾU LƯƠNG NHÂN VIÊN</div>
          <div class="text-caption text-disabled mt-1">Kỳ tính lương: {{ selectedMonth }}</div>
        </div>
        <v-select
          v-model="selectedMonth"
          :items="['2026-06', '2026-05', '2026-04']"
          variant="outlined"
          density="compact"
          hide-details
          style="max-width: 180px;"
        ></v-select>
      </div>

      <v-row class="mb-4 text-subtitle-2">
        <v-col cols="6" class="text-slate-400">Mã nhân viên: <span class="text-cyan-accent-2 font-weight-bold">{{ payrollData?.employeeId }}</span></v-col>
        <v-col cols="6" class="text-slate-400">Họ và tên: <span class="text-white font-weight-bold">{{ payrollData?.fullName }}</span></v-col>
      </v-row>

      <v-divider class="border-slate-800 my-4"></v-divider>

      <div class="text-subtitle-1 font-weight-bold text-white mb-2">💰 Các khoản thu nhập</div>
      <div class="d-flex justify-space-between text-subtitle-2 py-2 text-slate-300">
        <span>Lương hành chính cơ bản:</span>
        <span class="font-weight-bold text-white">{{ formatMoney(payrollData?.baseSalary) }}</span>
      </div>
      <div class="d-flex justify-space-between text-subtitle-2 py-2 text-slate-300">
        <span>Phụ cấp công việc & đi lại:</span>
        <span class="font-weight-bold text-white">{{ formatMoney(payrollData?.allowance) }}</span>
      </div>

      <div class="text-subtitle-1 font-weight-bold text-white mt-6 mb-2">🛑 Các khoản khấu trừ</div>
      <div class="d-flex justify-space-between text-subtitle-2 py-2 text-slate-300">
        <span>Bảo hiểm xã hội & Y tế (10.5%):</span>
        <span class="font-weight-bold text-error">-{{ formatMoney(payrollData?.insurance) }}</span>
      </div>
      <div class="d-flex justify-space-between text-subtitle-2 py-2 text-slate-300">
        <span>Phạt đi muộn ({{ payrollData?.lateCount }} lần):</span>
        <span class="font-weight-bold text-error">-{{ formatMoney(payrollData?.fineAmount) }}</span>
      </div>

      <v-divider class="border-slate-700 my-4"></v-divider>

      <v-card color="rgba(6, 182, 212, 0.1)" class="pa-4 rounded-lg border border-cyan-800 d-flex justify-space-between align-center">
        <span class="text-subtitle-1 font-weight-bold text-white">💵 Thực lĩnh cuối cùng (Net):</span>
        <span class="text-h4 font-weight-black text-cyan-accent-2">{{ formatMoney(payrollData?.netSalary) }}</span>
      </v-card>
    </v-card>
  </v-container>
</template>

<script setup>
import { ref, onMounted, watch } from 'vue'
import { useRoute } from 'vue-router'

const route = useRoute()
const selectedMonth = ref('2026-06')
const payrollData = ref(null)
const loading = ref(false)
const targetEmpId = ref('')

// Xử lý tìm mã nhân viên thông minh khi trang được tải lên
const initEmployeeId = () => {
  // 1. Kiểm tra nếu sếp truyền tham số ID trên URL (?id=NV004)
  if (route.query.id) {
    targetEmpId.value = route.query.id
    return
  }

  // 2. Nếu không có tham số URL, tự động bốc từ tài khoản đang đăng nhập
  const savedUser = localStorage.getItem('user_login')
  if (savedUser) {
    const user = JSON.parse(savedUser)
    targetEmpId.value = user.employeeId || 'NV004'
  }
}

// 🚀 API: Gọi dữ liệu phiếu lương động qua Gateway 8001
const fetchPayrollData = async () => {
  if (!targetEmpId.value) return
  loading.value = true
  try {
    const response = await fetch(`http://localhost:8001/api/hr/payroll/detail/${targetEmpId.value}/${selectedMonth.value}`)
    if (response.ok) {
      payrollData.value = await response.json()
    }
  } catch (error) {
    console.error('Lỗi khi tải phiếu lương từ Gateway 8001:', error)
  } finally {
    loading.value = false
  }
}

// Định dạng tiền tệ VND cho đẹp mắt chuyên nghiệp
const formatMoney = (value) => {
  if (value === undefined || value === null) return '0 VND'
  return value.toLocaleString('vi-VN') + ' VND'
}

// Lắng nghe nếu sếp chọn đổi tháng khác thì kéo lại dữ liệu lương tháng đó
watch(selectedMonth, () => {
  fetchPayrollData()
})

onMounted(() => {
  initEmployeeId()
  fetchPayrollData()
})
</script>