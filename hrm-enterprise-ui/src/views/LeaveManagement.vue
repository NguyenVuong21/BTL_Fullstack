<template>
  <v-container fluid>
    <!-- Tiêu đề trang -->
    <div class="mb-6">
      <h1 class="text-h4 font-weight-bold text-white">Quản lý & Duyệt Nghỉ phép</h1>
      <p class="text-subtitle-2 text-disabled">Danh sách đơn xin nghỉ phép của nhân sự cấp dưới chờ phê duyệt</p>
    </div>

    <!-- Thống kê nhanh tình trạng đơn nhảy số tự động từ DB -->
    <v-row class="mb-6">
      <v-col cols="12" sm="4">
        <v-card class="pa-4 border border-slate-800 rounded-xl" color="#111827">
          <div class="text-overline text-disabled font-weight-bold">Đơn chờ duyệt</div>
          <h2 class="text-h4 font-weight-black text-warning my-1">{{ pendingCount }}</h2>
        </v-card>
      </v-col>
      <v-col cols="12" sm="4">
        <v-card class="pa-4 border border-slate-800 rounded-xl" color="#111827">
          <div class="text-overline text-disabled font-weight-bold">Đã chấp thuận</div>
          <h2 class="text-h4 font-weight-black text-success my-1">{{ approvedCount }}</h2>
        </v-card>
      </v-col>
      <v-col cols="12" sm="4">
        <v-card class="pa-4 border border-slate-800 rounded-xl" color="#111827">
          <div class="text-overline text-disabled font-weight-bold">Đã từ chối</div>
          <h2 class="text-h4 font-weight-black text-error my-1">{{ rejectedCount }}</h2>
        </v-card>
      </v-col>
    </v-row>

    <!-- Bảng danh sách đơn xin nghỉ phép -->
    <v-card class="pa-4 border border-slate-800 rounded-xl" color="#111827">
      <h3 class="text-title font-weight-bold text-white mb-4">Danh sách đơn yêu cầu</h3>
      <v-table hover>
        <thead>
          <tr>
            <th class="text-left text-slate-300">Mã NV</th>
            <th class="text-left text-slate-300">Nhân viên</th>
            <th class="text-left text-slate-300">Loại phép</th>
            <th class="text-left text-slate-300">Thời gian</th>
            <th class="text-left text-slate-300">Lý do</th>
            <th class="text-center text-slate-300">Trạng thái</th>
            <th class="text-center text-slate-300">Hành động</th>
          </tr>
        </thead>
        <tbody>
          <!-- 🔄 VÒNG LẶP ĐỘNG ĐỔ DỮ LIỆU THỜI GIAN THỰC -->
          <tr v-for="(leave, index) in leaveRequests" :key="index">
            <td class="text-cyan-accent-2 font-weight-bold">{{ leave.employeeId }}</td>
            <td class="text-white font-weight-medium">{{ leave.fullName }}</td>
            <td class="text-slate-300 text-caption">{{ leave.leaveType }}</td>
            <td class="text-slate-400 text-caption">{{ leave.duration }}</td>
            <td class="text-slate-400 max-w-xs text-truncate" :title="leave.reason">{{ leave.reason }}</td>
            <td class="text-center">
              <v-chip size="small" :color="getStatusColor(leave.status)" class="font-weight-bold">
                {{ leave.status }}
              </v-chip>
            </td>
            <td class="text-center">
              <!-- Hiện nút hành động nếu đơn đang ở trạng thái Chờ duyệt -->
              <div v-if="leave.status === 'Chờ duyệt'" class="d-flex justify-center">
                <v-btn color="success" size="x-small" class="font-weight-bold mr-1" @click="updateStatus(leave.id, 'Đã duyệt')">
                  Duyệt
                </v-btn>
                <v-btn color="error" size="x-small" class="font-weight-bold" @click="updateStatus(leave.id, 'Từ chối')">
                  Từ chối
                </v-btn>
              </div>
              <span v-else class="text-caption text-disabled">Đã xử lý</span>
            </td>
          </tr>
          <tr v-if="leaveRequests.length === 0">
            <td colspan="7" class="text-center text-disabled py-4">Hiện không có đơn xin nghỉ phép nào trên hệ thống.</td>
          </tr>
        </tbody>
      </v-table>
    </v-card>
  </v-container>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue'

const leaveRequests = ref([])

// 🚀 API: Tải toàn bộ đơn xin nghỉ phép từ SQL Server qua API Gateway 8001
const fetchAdminLeaveList = async () => {
  try {
    const response = await fetch('http://localhost:8001/api/hr/leave/admin-list')
    if (response.ok) {
      leaveRequests.value = await response.json()
    }
  } catch (error) {
    console.error('Không thể lấy danh sách nghỉ phép qua Gateway:', error)
  }
}

// 🚀 API: Cập nhật phê duyệt hoặc từ chối trực tiếp xuống Database
const updateStatus = async (id, actionStatus) => {
  try {
    const response = await fetch(`http://localhost:8001/api/hr/leave/action/${id}`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ status: actionStatus })
    })

    if (response.ok) {
      alert(`Đã cập nhật trạng thái đơn thành công: ${actionStatus}`)
      // Tải lại bảng để cập nhật đồng bộ v-chip và card thống kê tức thì
      await fetchAdminLeaveList()
    } else {
      alert('Cập nhật thất bại, vui lòng thử lại.')
    }
  } catch (error) {
    console.error('Lỗi khi gọi API cập nhật trạng thái:', error)
  }
}

// 📊 Thuộc tính computed tính toán động các con số thống kê theo mảng dữ liệu thực tế
const pendingCount = computed(() => leaveRequests.value.filter(l => l.status === 'Chờ duyệt').length)
const approvedCount = computed(() => leaveRequests.value.filter(l => l.status === 'Đã duyệt' || l.status === 'Chấp thuận').length)
const rejectedCount = computed(() => leaveRequests.value.filter(l => l.status === 'Từ chối').length)

const getStatusColor = (status) => {
  if (status === 'Đã duyệt' || status === 'Chấp thuận') return 'success'
  if (status === 'Từ chối') return 'error'
  return 'warning'
}

onMounted(() => {
  fetchAdminLeaveList()
})
</script>

<style scoped>
.gap-2 {
  gap: 8px;
}
.max-w-xs {
  max-width: 180px;
}
</style>