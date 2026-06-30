<script setup>
import { ref, onMounted, watch } from 'vue'

// 1. Khai báo các biến Reactivity chuẩn Vue 3 Composition API
const selectedMonth = ref('2026-06') 
const payrollData = ref(null)
const empId = ref('')

// 2. Lấy thông tin user đăng nhập để lấy mã nhân viên tra cứu lương
const initUser = () => {
  const savedUser = localStorage.getItem('user_login')
  if (savedUser) {
    const user = JSON.parse(savedUser)
    empId.value = user.employeeId || 'NV001' 
  } else {
    empId.value = 'NV001' // Dự phòng phục vụ demo nếu chưa đăng nhập
  }
}

// 3. API: Gọi dữ liệu phiếu lương qua API Gateway cổng 8001
const fetchPayroll = async () => {
  if (!empId.value) return
  try {
    const response = await fetch(`http://localhost:8001/api/hr/payroll/detail/${empId.value}/${selectedMonth.value}`)
    if (response.ok) {
      payrollData.value = await response.json()
    } else {
      payrollData.value = null
    }
  } catch (error) {
    console.error('Lỗi khi tải phiếu lương từ Gateway:', error)
    payrollData.value = null
  }
}

// 4. Hàm format định dạng tiền tệ VNĐ
const formatMoney = (value) => {
  if (value === undefined || value === null) return '0 VND'
  return value.toLocaleString('vi-VN') + ' VND'
}

// 5. Watcher: Tự động tải lại lương khi đổi tháng trên Combobox
watch(selectedMonth, () => {
  fetchPayroll()
})

onMounted(() => {
  initUser()
  fetchPayroll()
})
</script>

<template>
  <div class="payslip-container">
    <h2 class="title">Phiếu lương Chi tiết</h2>
    <p class="subtitle">Tra cứu thu nhập, tiền công và các khoản trích khấu trừ theo tháng</p>

    <div class="card-salary">
      <div class="card-header">
        <div>
          <h3>PHIẾU LƯƠNG NHÂN VIÊN</h3>
          <p>Kỳ tính lương: <span class="highlight">{{ selectedMonth }}</span></p> 
        </div>
        <div>
          <select v-model="selectedMonth" class="month-picker">
            <option value="2026-06">2026-06</option>
            <option value="2026-05">2026-05</option>
            <option value="2026-04">2026-04</option>
          </select>
        </div>
      </div>

      <hr />

      <div class="employee-info" v-if="payrollData">
        <p>Mã nhân viên: <span class="highlight">{{ empId }}</span></p>
        <p>Họ và tên: <strong>{{ payrollData.FullName || payrollData.fullName || 'Nguyễn Văn Vượng' }}</strong></p>
      </div>

      <div class="salary-details" v-if="payrollData">
        <div class="section-title">💰 Các khoản thu nhập</div>
        <div class="detail-row">
          <span>Lương hành chính cơ bản:</span>
          <strong>{{ formatMoney(payrollData.BasicSalary) }}</strong>
        </div>
        <div class="detail-row">
          <span>Phụ cấp công việc & đi lại:</span>
          <strong>{{ formatMoney(payrollData.Allowances) }}</strong>
        </div>

        <div class="section-title deduction">🔴 Các khoản khấu trừ</div>
        <div class="detail-row">
          <span>Bảo hiểm xã hội & Y tế (10.5%):</span>
          <strong class="minus">-{{ formatMoney(payrollData.Deductions) }}</strong>
        </div>

        <div class="net-salary-block">
          <span>💵 Thực lĩnh cuối cùng (Net):</span>
          <span class="net-amount">{{ formatMoney(payrollData.NetSalary) }}</span>
        </div>
      </div>

      <div v-else class="no-data">
        <p>⚠️ Không tìm thấy dữ liệu phiếu lương của kỳ {{ selectedMonth }}</p>
      </div>
    </div>
  </div>
</template>

<style scoped>
.payslip-container { padding: 20px; color: #fff; max-width: 800px; margin: 0 auto; }
.title { font-size: 24px; font-weight: bold; margin-bottom: 5px; }
.subtitle { color: #9ca3af; font-size: 14px; margin-bottom: 20px; }
.card-salary { background: #111927; border: 1px solid #223147; border-radius: 12px; padding: 24px; }
.card-header { display: flex; justify-content: space-between; align-items: center; }
.card-header h3 { font-size: 18px; margin: 0 0 5px 0; color: #00ffcc; }
.card-header p { margin: 0; color: #9ca3af; font-size: 14px; }
.month-picker { background: #1f2937; color: #fff; border: 1px solid #374151; padding: 8px 12px; border-radius: 6px; cursor: pointer; outline: none; }
hr { border: 0; border-top: 1px solid #223147; margin: 20px 0; }
.employee-info { display: flex; gap: 40px; margin-bottom: 25px; color: #e5e7eb; font-size: 15px; }
.highlight { color: #00ffcc; font-weight: bold; }
.section-title { font-weight: bold; margin-top: 25px; margin-bottom: 12px; color: #38bdf8; font-size: 16px; border-bottom: 1px solid #1f2937; padding-bottom: 5px; }
.section-title.deduction { color: #f43f5e; }
.detail-row { display: flex; justify-content: space-between; padding: 12px 0; font-size: 15px; }
.minus { color: #f43f5e; }
.net-salary-block { display: flex; justify-content: space-between; align-items: center; background: #1e293b; padding: 18px; border-radius: 8px; margin-top: 30px; border-left: 4px solid #10b981; }
.net-amount { color: #10b981; font-size: 22px; font-weight: bold; }
.no-data { text-align: center; color: #9ca3af; padding: 40px 0; font-style: italic; }
</style>