<template>
  <v-container fluid>
    <div class="mb-6">
      <h1 class="text-h4 font-weight-bold text-white">Quản lý & Giám sát Chấm công</h1>
      <p class="text-subtitle-2 text-disabled">Theo dõi thời gian thực trạng thái đi làm của nhân sự trong ngày</p>
    </div>

    <v-card class="pa-4 mb-6 border border-slate-800 rounded-xl" color="#111827">
      <v-row density="compact" align="center">
        <v-col cols="12" sm="4">
          <v-text-field
            v-model="searchQuery"
            label="Tìm theo Tên hoặc Mã nhân viên"
            prepend-inner-icon="mdi-account-search"
            variant="outlined"
            density="compact"
            hide-details
          ></v-text-field>
        </v-col>
        <v-col cols="12" sm="4">
          <v-select
            v-model="filterAttendance"
            :items="['Tất cả nhân sự', 'Đã chấm công', 'Chưa chấm công']"
            label="Trạng thái Đi làm Hôm nay"
            variant="outlined"
            density="compact"
            hide-details
          ></v-select>
        </v-col>
        <v-col cols="12" sm="4" class="d-flex justify-end">
          <v-btn color="primary" prepend-icon="mdi-refresh" class="mr-2 font-weight-bold" @click="fetchTodayStatus">
            Tải lại dữ liệu
          </v-btn>
        </v-col>
      </v-row>
    </v-card>

    <v-card class="border border-slate-800 rounded-xl pa-2" color="#111827">
      <v-table hover>
        <thead>
          <tr>
            <th class="font-weight-bold text-left text-subtitle-1 text-slate-300">Mã nhân viên</th>
            <th class="font-weight-bold text-left text-subtitle-1 text-slate-300">Họ và Tên</th>
            <th class="font-weight-bold text-left text-subtitle-1 text-slate-300">Chức vụ</th>
            <th class="font-weight-bold text-left text-subtitle-1 text-slate-300">Giờ vào (Check-In)</th>
            <th class="font-weight-bold text-center text-subtitle-1 text-slate-300">Trạng thái điểm danh</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="emp in filteredEmployees" :key="emp.employeeId">
            <td class="font-weight-bold text-cyan-accent-2">{{ emp.employeeId }}</td>
            <td class="text-white font-weight-medium">{{ emp.fullName }}</td>
            <td class="text-slate-400">{{ emp.position }}</td>
            <td class="font-mono text-subtitle-2" :class="emp.hasCheckedIn ? 'text-success font-weight-bold' : 'text-disabled'">
              {{ emp.checkInTime }}
            </td>
            <td class="text-center">
              <v-chip
                size="small"
                :color="emp.hasCheckedIn ? (emp.status === 'Đi muộn' ? 'error' : 'success') : 'grey-darken-1'"
                variant="flat"
                class="font-weight-bold"
              >
                {{ emp.status }}
              </v-chip>
            </td>
          </tr>
          <tr v-if="filteredEmployees.length === 0">
            <td colspan="5" class="text-center text-disabled py-6">
              Không tìm thấy nhân viên nào khớp với bộ lọc.
            </td>
          </tr>
        </tbody>
      </v-table>
    </v-card>
  </v-container>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'

const employeeList = ref([])
const searchQuery = ref('')
const filterAttendance = ref('Tất cả nhân sự')

// --- 🚀 ĐÃ SỬA: LẤY DỮ LIỆU QUA API GATEWAY CỔNG 8001 ---
const fetchTodayStatus = async () => {
  try {
    const response = await fetch('http://localhost:8001/api/hr/attendance/today-status')
    if (response.ok) {
      employeeList.value = await response.json()
    }
  } catch (error) {
    console.error('Lỗi kết nối API lấy trạng thái ngày qua Gateway 8001:', error)
  }
}

// --- LOGIC LỌC TÌM KIẾM ĐỘNG TRÊN GIAO DIỆN ---
const filteredEmployees = computed(() => {
  return employeeList.value.filter(emp => {
    // 1. Lọc theo tên hoặc mã
    const matchQuery = emp.fullName.toLowerCase().includes(searchQuery.value.toLowerCase().trim()) ||
                       emp.employeeId.toLowerCase().includes(searchQuery.value.toLowerCase().trim())
    
    // 2. Lọc theo trạng thái chấm công chọn ở combobox
    let matchStatus = true
    if (filterAttendance.value === 'Đã chấm công') {
      matchStatus = emp.hasCheckedIn === true
    } else if (filterAttendance.value === 'Chưa chấm công') {
      matchStatus = emp.hasCheckedIn === false
    }

    return matchQuery && matchStatus
  })
})

onMounted(() => {
  fetchTodayStatus()
})
</script>

<style scoped>
.font-mono {
  font-family: 'Courier New', Courier, monospace;
}
</style>