<script setup>
import { ref, onMounted, watch } from 'vue'

const selectedMonth = ref('2026-06') // Mặc định tháng hiện tại theo UI của Vượng
const payrollData = ref(null)
const empId = ref('')

// Lấy mã nhân viên đang đăng nhập từ hệ thống để tra cứu đúng người
const initUser = () => {
  const savedUser = localStorage.getItem('user_login')
  if (savedUser) {
    const user = JSON.parse(savedUser)
    empId.value = user.employeeId || 'NV001' // Dự phòng NV001 nếu lỗi
  }
}

// 🚀 API: Gọi dữ liệu phiếu lương động qua Gateway 8001
const fetchPayroll = async () => {
  if (!empId.value) return
  try {
    const response = await fetch(`http://localhost:8001/api/hr/payroll/detail/${empId.value}/${selectedMonth.value}`)
    if (response.ok) {
      payrollData.value = await response.json()
    }
  } catch (error) {
    console.error('Lỗi khi tải phiếu lương từ Gateway:', error)
  }
}

// Hàm format tiền tệ VNĐ cho đẹp mắt giống UI
const formatMoney = (value) => {
  if (value === undefined || value === null) return '0 VND'
  return value.toLocaleString('vi-VN') + ' VND'
}

// Theo dõi nếu sếp đổi chọn tháng khác trên combobox thì tự động fetch lại lương tháng đó
watch(selectedMonth, () => {
  fetchPayroll()
})

onMounted(() => {
  initUser()
  fetchPayroll()
})
</script>