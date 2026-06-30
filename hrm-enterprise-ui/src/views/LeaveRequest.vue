<template>
  <v-container fluid>
    <div class="mb-6">
      <h1 class="text-h4 font-weight-bold text-white">Đăng ký Nghỉ phép</h1>
      <p class="text-subtitle-2 text-disabled">Gửi yêu cầu nghỉ phép và theo dõi trạng thái phê duyệt</p>
    </div>

    <v-row>
      <v-col cols="12" md="5">
        <v-card class="pa-6 border border-slate-800 rounded-xl" color="#111827">
          <h3 class="text-title font-weight-bold text-white mb-4">Tạo đơn mới</h3>
          <v-form @submit.prevent="submitLeave">
            <v-select
              v-model="leaveType"
              :items="['Nghỉ phép năm', 'Nghỉ ốm', 'Nghỉ việc riêng', 'Nghỉ thai sản']"
              label="Loại nghỉ phép"
              variant="outlined"
              class="mb-2 text-white"
            ></v-select>

            <v-text-field
              v-model="startDate"
              label="Từ ngày"
              type="date"
              variant="outlined"
              class="mb-2 text-white"
            ></v-text-field>

            <v-text-field
              v-model="endDate"
              label="Đến ngày"
              type="date"
              variant="outlined"
              class="mb-2 text-white"
            ></v-text-field>

            <v-textarea
              v-model="reason"
              label="Lý do xin nghỉ"
              variant="outlined"
              rows="3"
              class="mb-4 text-white"
            ></v-textarea>

            <v-btn type="submit" color="primary" block size="large" class="font-weight-bold" :loading="loading">
              Gửi đơn xin duyệt
            </v-btn>
          </v-form>
        </v-card>
      </v-col>

      <v-col cols="12" md="7">
        <v-card class="pa-4 border border-slate-800 rounded-xl" color="#111827">
          <h3 class="text-title font-weight-bold text-white mb-4">Lịch sử yêu cầu</h3>
          <v-table hover>
            <thead>
              <tr>
                <th class="text-left text-slate-300">Loại phép</th>
                <th class="text-left text-slate-300">Thời gian nghỉ</th>
                <th class="text-center text-slate-300">Trạng thái</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="(item, index) in leaveHistory" :key="index">
                <td class="text-white font-weight-medium">{{ item.leaveType }}</td>
                <td class="text-slate-400 text-caption">{{ item.duration }}</td>
                <td class="text-center">
                  <v-chip size="small" :color="getStatusColor(item.status)" class="font-weight-bold">
                    {{ item.status }}
                  </v-chip>
                </td>
              </tr>
              <tr v-if="leaveHistory.length === 0">
                <td colspan="3" class="text-center text-disabled py-4">Bạn chưa gửi đơn xin nghỉ phép nào.</td>
              </tr>
            </tbody>
          </v-table>
        </v-card>
      </v-col>
    </v-row>
  </v-container>
</template>

<script setup>
import { ref, onMounted } from 'vue'

const loading = ref(false)
const leaveType = ref('Nghỉ phép năm')
const startDate = ref('')
const endDate = ref('')
const reason = ref('')
const leaveHistory = ref([])
const empId = ref('')

// Lấy mã nhân viên hiện tại để load lịch sử
const initEmployeeData = () => {
  const savedUser = localStorage.getItem('user_login')
  if (savedUser) {
    const user = JSON.parse(savedUser)
    empId.value = user.employeeId || ''
  }
}

// 🚀 API: Tải lịch sử nghỉ phép từ SQL Server
const fetchLeaveHistory = async () => {
  if (!empId.value) return
  try {
    const response = await fetch(`http://localhost:8001/api/hr/leave/history/${empId.value}`)
    if (response.ok) {
      leaveHistory.value = await response.json()
    }
  } catch (error) {
    console.error('Lỗi lấy lịch sử nghỉ phép:', error)
  }
}

// 🚀 API: Gửi đơn xin nghỉ phép mới lên SQL Server
const submitLeave = async () => {
  if (!startDate.value || !endDate.value || !reason.value.trim()) {
    alert('Vui lòng điền đầy đủ thông tin thời gian và lý do nghỉ phép!')
    return
  }

  loading.value = true
  try {
    const response = await fetch('http://localhost:8001/api/hr/leave/submit', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        employeeId: empId.value,
        leaveType: leaveType.value,
        startDate: startDate.value,
        endDate: endDate.value,
        reason: reason.value.trim()
      })
    })

    if (response.ok) {
      alert('Gửi đơn xin nghỉ phép thành công!')
      reason.value = ''
      startDate.value = ''
      endDate.value = ''
      // Load lại lịch sử ngay lập tức để cập nhật dòng "Chờ duyệt" mới lên bảng
      await fetchLeaveHistory()
    } else {
      alert('Gửi đơn thất bại, vui lòng kiểm tra lại dữ liệu.')
    }
  } catch (error) {
    console.error('Lỗi kết nối API nghỉ phép:', error)
  } finally {
    loading.value = false
  }
}

const getStatusColor = (status) => {
  if (status === 'Đã duyệt' || status === 'Chấp thuận') return 'success'
  if (status === 'Từ chối') return 'error'
  return 'warning'
}

onMounted(() => {
  initEmployeeData()
  fetchLeaveHistory()
})
</script>