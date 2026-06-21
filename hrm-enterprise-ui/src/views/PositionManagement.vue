<template>
  <v-container fluid>
    <div class="mb-6">
      <h1 class="text-h4 font-weight-bold text-white">Quản lý Chức vụ</h1>
      <p class="text-subtitle-2 text-disabled">Cấu hình danh mục chức vụ, vị trí phòng ban và định mức lương cơ bản</p>
    </div>

    <v-card class="pa-4 mb-6 border border-slate-800 rounded-xl" color="#111827">
      <v-row density="compact" align="center">
        <v-col cols="12" sm="6">
          <v-text-field
            v-model="search"
            label="Tìm kiếm tên hoặc mã chức vụ..."
            prepend-inner-icon="mdi-magnify"
            variant="outlined"
            density="compact"
            hide-details
          ></v-text-field>
        </v-col>
        <v-col cols="12" sm="6" class="d-flex justify-end">
          <v-btn color="primary" prepend-icon="mdi-plus" class="font-weight-bold" @click="openAddDialog">
            Thêm chức vụ mới
          </v-btn>
        </v-col>
      </v-row>
    </v-card>

    <v-card class="border border-slate-800 rounded-xl pa-2" color="#111827">
      <v-table hover>
        <thead>
          <tr>
            <th class="font-weight-bold text-left text-subtitle-1 text-slate-300">Mã chức vụ</th>
            <th class="font-weight-bold text-left text-subtitle-1 text-slate-300">Tên chức vụ</th>
            <th class="font-weight-bold text-left text-subtitle-1 text-slate-300">Mức lương định mức</th>
            <th class="font-weight-bold text-center text-subtitle-1 text-slate-300">Hành động</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="pos in filteredPositions" :key="pos.positionId">
            <td class="font-weight-bold text-cyan-accent-2">{{ pos.positionId }}</td>
            <td class="text-white font-weight-medium">{{ pos.positionName }}</td>
            <td class="text-success font-mono font-weight-bold">{{ formatMoney(pos.baseSalary) }}</td>
            <td class="text-center">
              <v-btn icon variant="text" color="amber" size="small" @click="openEditDialog(pos)">
                <v-icon>mdi-pencil</v-icon>
              </v-btn>
              <v-btn icon variant="text" color="error" size="small" @click="deletePosition(pos.positionId)">
                <v-icon>mdi-delete</v-icon>
              </v-btn>
            </td>
          </tr>
          <tr v-if="filteredPositions.length === 0">
            <td colspan="4" class="text-center text-disabled py-6">Không tìm thấy chức vụ nào phù hợp.</td>
          </tr>
        </tbody>
      </v-table>
    </v-card>

    <v-dialog v-model="isDialogOpen" max-width="500">
      <v-card color="#111827" class="border border-slate-800 rounded-xl pa-4 text-white">
        <v-card-title class="text-h5 font-weight-bold text-cyan-accent-2">
          {{ isEditMode ? 'Cập nhật Chức vụ' : 'Thêm Chức vụ Mới' }}
        </v-card-title>
        <v-card-text>
          <v-form ref="formRef">
            <v-text-field
              v-model="currentPosition.positionId"
              label="Mã chức vụ"
              variant="outlined"
              :disabled="isEditMode"
              class="mb-3"
            ></v-text-field>
            <v-text-field
              v-model="currentPosition.positionName"
              label="Tên chức vụ"
              variant="outlined"
              class="mb-3"
            ></v-text-field>
            <v-text-field
              v-model.number="currentPosition.baseSalary"
              label="Lương cơ bản định mức (VND)"
              type="number"
              variant="outlined"
            ></v-text-field>
          </v-form>
        </v-card-text>
        <v-card-actions>
          <v-spacer></v-spacer>
          <v-btn variant="text" color="slate-400" @click="isDialogOpen = false">Hủy</v-btn>
          <v-btn color="primary" variant="flat" class="px-4 font-weight-bold" @click="savePosition">
            Lưu cấu hình
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>
  </v-container>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'

const positions = ref([])
const search = ref('')
const isDialogOpen = ref(false)
const isEditMode = ref(false)

const currentPosition = ref({
  positionId: '',
  positionName: '',
  baseSalary: 0
})

// 🚀 1. FETCH DANH SÁCH QUA GATEWAY 8001
const fetchPositions = async () => {
  try {
    const response = await fetch('http://localhost:8001/api/hr/positions')
    if (response.ok) {
      positions.value = await response.json()
    }
  } catch (error) {
    console.error('Lỗi lấy danh sách chức vụ qua Gateway 8001:', error)
  }
}

// 2. MỞ DIALOG CHẾ ĐỘ THÊM
const openAddDialog = () => {
  isEditMode.value = false
  currentPosition.value = { positionId: '', positionName: '', baseSalary: 0 }
  isDialogOpen.value = true
}

// 3. MỞ DIALOG CHẾ ĐỘ SỬA
const openEditDialog = (pos) => {
  isEditMode.value = true
  currentPosition.value = { ...pos }
  isDialogOpen.value = true
}

const savePosition = () => {
  if (isEditMode.value) {
    submitUpdatePosition()
  } else {
    submitCreatePosition()
  }
}

// 🚀 4. API POST: THÊM MỚI QUA GATEWAY 8001
const submitCreatePosition = async () => {
  try {
    const response = await fetch('http://localhost:8001/api/hr/positions', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(currentPosition.value)
    })
    if (response.ok) {
      alert('Thêm chức vụ mới thành công rực rỡ! 🎉')
      isDialogOpen.value = false
      fetchPositions()
    }
  } catch (error) {
    console.error(error)
  }
}

// 🚀 5. API PUT: CẬP NHẬT QUA GATEWAY 8001
const submitUpdatePosition = async () => {
  try {
    const id = currentPosition.value.positionId
    const response = await fetch(`http://localhost:8001/api/hr/positions/${id}`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      // 🔑 SỬA Ở ĐÂY: Gửi kèm cả baseSalary lên Backend nữa nhé Vượng
      body: JSON.stringify({ 
        positionName: currentPosition.value.positionName,
        baseSalary: currentPosition.value.baseSalary 
      })
    })
    if (response.ok) {
      alert(`Cập nhật thông tin chức vụ thành công! 🎉`)
      isDialogOpen.value = false
      fetchPositions() // Load lại bảng dữ liệu mới
    }
  } catch (error) {
    console.error(error)
  }
}
// 🚀 6. API DELETE: XÓA QUA GATEWAY 8001
const deletePosition = async (id) => {
  if (confirm(`Bạn có chắc chắn muốn xóa chức vụ mã ${id}?`)) {
    try {
      const response = await fetch(`http://localhost:8001/api/hr/positions/${id}`, {
        method: 'DELETE'
      })
      if (response.ok) {
        alert('Đã xóa chức vụ thành công!')
        fetchPositions()
      }
    } catch (error) {
      console.error(error)
    }
  }
}

const formatMoney = (val) => {
  return val ? val.toLocaleString('vi-VN') + ' VND' : '0 VND'
}

const filteredPositions = computed(() => {
  return positions.value.filter(pos => 
    pos.positionName.toLowerCase().includes(search.value.toLowerCase()) ||
    pos.positionId.toLowerCase().includes(search.value.toLowerCase())
  )
})

onMounted(() => {
  fetchPositions()
})
</script>