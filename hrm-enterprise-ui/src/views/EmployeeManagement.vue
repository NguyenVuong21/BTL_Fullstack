<template>
  <div>
    <div class="d-flex justify-space-between align-center mb-6">
      <div>
        <h1 class="text-h4 font-weight-bold text-white">Hồ sơ Nhân viên</h1>
        <p class="text-subtitle-2 text-disabled">Quản lý vòng đời nhân sự thuộc phân hệ HR Core Service</p>
      </div>
      <v-btn color="primary" prepend-icon="mdi-plus" class="gradient-btn" @click="openAddDialog">
        Thêm nhân viên mới
      </v-btn>
    </div>

    <v-card class="pa-4">
      <v-row class="mb-4 d-flex align-center">
        <v-col cols="12" sm="4">
          <v-text-field
            v-model="search"
            label="Tìm kiếm nhân viên..."
            prepend-inner-icon="mdi-magnify"
            variant="outlined"
            density="compact"
            hide-details
          ></v-text-field>
        </v-col>
        <v-col cols="12" sm="3">
          <v-select
            v-model="selectedDept"
            :items="departments"
            label="Phòng ban"
            variant="outlined"
            density="compact"
            hide-details
          ></v-select>
        </v-col>
        <v-spacer></v-spacer>
        <v-col cols="auto">
          <v-btn variant="outlined" prepend-icon="mdi-file-excel" class="mr-2" color="success" @click="exportExcel">Xuất Excel</v-btn>
          <v-btn variant="outlined" prepend-icon="mdi-file-pdf-box" color="error" @click="exportPdf">Xuất PDF</v-btn>
        </v-col>
      </v-row>

      <v-table hover>
        <thead>
          <tr>
            <th class="text-left font-weight-bold">Mã NV</th>
            <th class="text-left font-weight-bold">Nhân viên</th>
            <th class="text-left font-weight-bold">Phòng ban</th>
            <th class="text-left font-weight-bold">Chức vụ</th>
            <th class="text-left font-weight-bold">Loại hợp đồng</th>
            <th class="text-left font-weight-bold">Ngày vào làm</th>
            <th class="text-left font-weight-bold">Trạng thái</th>
            <th class="text-center font-weight-bold">Hành động</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="emp in filteredEmployees" :key="emp.id">
            <td><v-chip size="small" variant="tonal" color="primary">{{ emp.id }}</v-chip></td>
            <td>
              <div class="d-flex align-center py-2">
                <v-avatar size="32" color="accent" class="mr-3">
                  {{ emp.name ? emp.name.charAt(0) : 'N' }}
                </v-avatar>
                <div>
                  <div class="font-weight-bold">{{ emp.name }}</div>
                  <div class="text-caption text-disabled">{{ emp.email }}</div>
                </div>
              </div>
            </td>
            <td>{{ emp.dept }}</td>
            <td>{{ emp.position }}</td>
            <td>
              <v-chip size="x-small" :color="getContractColor(emp.contractType)" variant="flat">
                {{ emp.contractType }}
              </v-chip>
            </td>
            <td>{{ emp.joinDate }}</td>
            <td>
              <v-badge dot inline :color="emp.status === 'Hoạt động' ? 'success' : 'grey'"></v-badge>
              <span>{{ emp.status }}</span>
            </td>
            <td class="text-center">
              <v-btn icon="mdi-eye-outline" variant="text" size="small" color="info" @click="viewEmployee(emp.id)"></v-btn>
              <v-btn icon="mdi-pencil-outline" variant="text" size="small" color="warning" @click="openEditDialog(emp)"></v-btn>
              <v-btn icon="mdi-delete-outline" variant="text" size="small" color="error" @click="deleteEmployee(emp.id)"></v-btn>
            </td>
          </tr>
          <tr v-if="filteredEmployees.length === 0">
            <td colspan="8" class="text-center text-disabled py-4">Không tìm thấy nhân viên nào phù hợp.</td>
          </tr>
        </tbody>
      </v-table>
    </v-card>

    <v-dialog v-model="isModalOpen" max-w="600px">
      <v-card class="pa-2 bg-slate-900 border border-slate-800">
        <v-card-title class="d-flex justify-space-between align-center border-b pb-3">
          <span class="text-h6 font-weight-bold text-primary">
            <v-icon icon="mdi-account-card-details-outline" class="mr-2"></v-icon>Chi tiết Hồ sơ Nhân viên
          </span>
          <v-btn icon="mdi-close" variant="text" size="small" @click="isModalOpen = false"></v-btn>
        </v-card-title>

        <v-card-text class="pt-4">
          <v-row dense>
            <v-col cols="12" sm="6" class="mb-3">
              <div class="text-caption text-disabled mb-1">Mã nhân viên</div>
              <div class="pa-2 bg-grey-darken-3 rounded font-weight-bold text-primary font-mono">{{ currentEmployee.id }}</div>
            </v-col>
            <v-col cols="12" sm="6" class="mb-3">
              <div class="text-caption text-disabled mb-1">Họ và tên</div>
              <div class="pa-2 bg-grey-darken-3 rounded font-weight-bold">{{ currentEmployee.name }}</div>
            </v-col>
            <v-col cols="12" sm="6" class="mb-3">
              <div class="text-caption text-disabled mb-1">Địa chỉ Email</div>
              <div class="pa-2 bg-grey-darken-3 rounded">{{ currentEmployee.email }}</div>
            </v-col>
            <v-col cols="12" sm="6" class="mb-3">
              <div class="text-caption text-disabled mb-1">Phòng ban</div>
              <div class="pa-2 bg-grey-darken-3 rounded">{{ currentEmployee.dept }}</div>
            </v-col>
            <v-col cols="12" sm="6" class="mb-3">
              <div class="text-caption text-disabled mb-1">Chức vụ</div>
              <div class="pa-2 bg-grey-darken-3 rounded">{{ currentEmployee.position }}</div>
            </v-col>
            <v-col cols="12" sm="6" class="mb-3">
              <div class="text-caption text-disabled mb-1">Loại hợp đồng</div>
              <div class="pa-2 bg-grey-darken-3 rounded">{{ currentEmployee.contractType }}</div>
            </v-col>
            <v-col cols="12" sm="6" class="mb-3">
              <div class="text-caption text-disabled mb-1">Ngày vào làm</div>
              <div class="pa-2 bg-grey-darken-3 rounded">{{ currentEmployee.joinDate }}</div>
            </v-col>
            <v-col cols="12" sm="6" class="mb-3">
              <div class="text-caption text-disabled mb-1">Trạng thái</div>
              <div class="pt-1">
                <v-chip :color="currentEmployee.status === 'Hoạt động' ? 'success' : 'grey'" size="small" variant="flat">
                  {{ currentEmployee.status }}
                </v-chip>
              </div>
            </v-col>
          </v-row>
        </v-card-text>

        <v-card-actions class="justify-end border-t pt-3">
          <v-btn color="grey" variant="outlined" @click="isModalOpen = false">Đóng lại</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <v-dialog v-model="isAddDialogOpen" max-w="600px">
      <v-card class="pa-2 bg-slate-900 border border-slate-800">
        <v-card-title class="d-flex justify-space-between align-center border-b pb-3">
          <span class="text-h6 font-weight-bold" :class="isEditMode ? 'text-warning' : 'text-success'">
            <v-icon :icon="isEditMode ? 'mdi-pencil-outline' : 'mdi-account-plus-outline'" class="mr-2"></v-icon>
            {{ isEditMode ? 'Cập nhật Hồ sơ Nhân viên' : 'Thêm Nhân viên Mới' }}
          </span>
          <v-btn icon="mdi-close" variant="text" size="small" @click="isAddDialogOpen = false"></v-btn>
        </v-card-title>

        <v-card-text class="pt-4">
          <v-form ref="addForm">
            <v-row dense>
              <v-col cols="12" sm="6">
                <v-text-field
                  v-model="newEmployee.employeeId"
                  label="Mã nhân viên (Ví dụ: NV003)"
                  variant="outlined"
                  density="compact"
                  :readonly="isEditMode"
                  :disabled="isEditMode"
                  required
                ></v-text-field>
              </v-col>
              <v-col cols="12" sm="6">
                <v-text-field
                  v-model="newEmployee.fullName"
                  label="Họ và tên"
                  variant="outlined"
                  density="compact"
                  required
                ></v-text-field>
              </v-col>
              <v-col cols="12">
                <v-text-field
                  v-model="newEmployee.email"
                  label="Địa chỉ Email"
                  variant="outlined"
                  density="compact"
                  required
                ></v-text-field>
              </v-col>
              <v-col cols="12" sm="6">
                <v-text-field
                  v-model="newEmployee.position"
                  label="Chức vụ"
                  variant="outlined"
                  density="compact"
                  required
                ></v-text-field>
              </v-col>
              <v-col cols="12" sm="6">
                <v-select
                  v-model="newEmployee.contractType"
                  :items="['Chính thức', 'Thử việc', 'Thời vụ']"
                  label="Loại hợp đồng"
                  variant="outlined"
                  density="compact"
                ></v-select>
              </v-col>
              <v-col cols="12" sm="6">
                <v-select
                  v-model="newEmployee.status"
                  :items="['Hoạt động', 'Thôi việc']"
                  label="Trạng thái"
                  variant="outlined"
                  density="compact"
                ></v-select>
              </v-col>
              <v-col cols="12" sm="6">
                <v-select
                  v-model="newEmployee.departmentId"
                  :items="[
                    { title: 'Ban Giám Đốc (Mã 1)', value: 1 },
                    { title: 'Phòng Công nghệ (Mã 2)', value: 2 },
                    { title: 'Phòng Nhân sự (Mã 3)', value: 3 },
                    { title: 'Phòng Tài chính (Mã 4)', value: 4 }
                  ]"
                  item-title="title"
                  item-value="value"
                  label="Phòng ban trực thuộc"
                  variant="outlined"
                  density="compact"
                ></v-select>
              </v-col>
            </v-row>
          </v-form>
        </v-card-text>

        <v-card-actions class="justify-end border-t pt-3">
          <v-btn color="grey" variant="outlined" @click="isAddDialogOpen = false">Hủy bỏ</v-btn>
          <v-btn :color="isEditMode ? 'warning' : 'success'" variant="flat" class="px-4" @click="saveEmployee">
            {{ isEditMode ? 'Lưu thay đổi' : 'Lưu hồ sơ' }}
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'

// --- KHAI BÁO BIẾN TRẠNG THÁI ---
const search = ref('')
const selectedDept = ref('Tất cả phòng ban')
const departments = ['Tất cả phòng ban', 'Phòng Công nghệ', 'Phòng Nhân sự', 'Phòng Tài chính']
const employees = ref([])

// Kiểm soát đóng/mở Pop-up Xem hồ sơ
const isModalOpen = ref(false)
const currentEmployee = ref({
  id: '', name: '', email: '', dept: '', position: '', contractType: '', joinDate: '', status: ''
})

// Biến điều khiển chế độ: false = Thêm mới, true = Chỉnh sửa
const isEditMode = ref(false)
const isAddDialogOpen = ref(false)
const newEmployee = ref({
  employeeId: '',
  fullName: '',
  email: '',
  position: '',
  contractType: 'Chính thức',
  status: 'Hoạt động',
  departmentId: 2
})

// --- ĐIỀU PHỐI MÀU SẮC CHIP HỢP ĐỒNG ---
const getContractColor = (type) => {
  if (type === 'Thử việc') return 'warning'
  if (type === 'Thời vụ') return 'purple'
  return 'primary'
}

// ============================================
//   CÁC HÀM XỬ LÝ API KẾT NỐI GATEWAY CỔNG 8001
// ============================================

// 1. LẤY DANH SÁCH NHÂN VIÊN TỪ DATABASE
const fetchEmployees = async () => {
  try {
    const response = await fetch('http://localhost:8001/api/hr/employees')
    if (response.ok) {
      employees.value = await response.json()
    } else {
      console.error('Lỗi khi fetch dữ liệu nhân viên.')
    }
  } catch (error) {
    console.error('Không thể kết nối tới API Gateway cổng 8001:', error)
  }
}

// 2. XEM CHI TIẾT HỒ SƠ NHÂN VIÊN (CLICK ICON CON MẮT)
const viewEmployee = async (id) => {
  try {
    const response = await fetch(`http://localhost:8001/api/hr/employees/${id}`)
    if (response.ok) {
      currentEmployee.value = await response.json()
      isModalOpen.value = true
    } else {
      alert('Không thể tìm thấy chi tiết hồ sơ nhân viên này!')
    }
  } catch (error) {
    console.error('Lỗi kết nối API lấy chi tiết qua Gateway 8001:', error)
  }
}

// 3. MỞ FORM Ở CHẾ ĐỘ THÊM MỚI NHÂN VIÊN
const openAddDialog = () => {
  isEditMode.value = false
  newEmployee.value = {
    employeeId: '', fullName: '', email: '', position: '',
    contractType: 'Chính thức', status: 'Hoạt động', 
    departmentId: null
  }
  isAddDialogOpen.value = true
}

// 4. MỞ FORM Ở CHẾ ĐỘ SỬA HỒ SƠ NHÂN VIÊN
const openEditDialog = (emp) => {
  isEditMode.value = true 
  
  let currentDeptId = emp.departmentId || emp.deptId;
  
  if (!currentDeptId) {
    if (emp.dept === 'Ban Giám Đốc') currentDeptId = 1;
    else if (emp.dept === 'Phòng Công nghệ' || emp.dept === 'Phòng Công Nghệ Thông Tin') currentDeptId = 2;
    else if (emp.dept === 'Phòng Nhân sự') currentDeptId = 3;
    else if (emp.dept === 'Phòng Tài chính') currentDeptId = 4;
    else currentDeptId = 2; 
  }

  newEmployee.value = {
    employeeId: emp.id,
    fullName: emp.name,
    email: emp.email,
    position: emp.position,
    contractType: emp.contractType,
    status: emp.status,
    departmentId: Number(currentDeptId) 
  }
  
  isAddDialogOpen.value = true 
}

const saveEmployee = () => {
  if (isEditMode.value) {
    submitUpdateEmployee()
  } else {
    submitCreateEmployee()
  }
}

// 5. LUỒNG API POST: THÊM MỚI NHÂN VIÊN
const submitCreateEmployee = async () => {
  if (!newEmployee.value.employeeId || !newEmployee.value.fullName || !newEmployee.value.email) {
    alert('Vui lòng điền đầy đủ Mã NV, Họ tên và Email nhé Vượng!')
    return
  }

  try {
    const response = await fetch('http://localhost:8001/api/hr/employees', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(newEmployee.value)
    })

    if (response.ok) {
      alert('Thêm nhân viên mới thành công rực rỡ! 🎉')
      isAddDialogOpen.value = false
      fetchEmployees()
    } else {
      const err = await response.json()
      alert('Backend từ chối lưu: ' + JSON.stringify(err))
    }
  } catch (error) {
    console.error('Lỗi khi thêm mới qua 8001:', error)
    alert('Không thể kết nối tới API Gateway cổng 8001 để lưu!')
  }
}

// 6. LUỒNG API PUT: CẬP NHẬT SỬA ĐỔI THÔNG TIN NHÂN VIÊN
const submitUpdateEmployee = async () => {
  try {
    const id = newEmployee.value.employeeId
    const response = await fetch(`http://localhost:8001/api/hr/employees/${id}`, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(newEmployee.value)
    })

    if (response.ok) {
      alert(`Cập nhật hồ sơ nhân viên ${id} thành công! 🎉`)
      isAddDialogOpen.value = false
      fetchEmployees()
    } else {
      const err = await response.json()
      alert('Lỗi khi cập nhật dữ liệu: ' + JSON.stringify(err))
    }
  } catch (error) {
    console.error('Lỗi khi gọi API PUT qua 8001:', error)
    alert('Không thể kết nối tới API Gateway cổng 8001 để cập nhật!')
  }
}

// 7. CHỨC NĂNG XÓA HỒ SƠ THẬT
const deleteEmployee = async (id) => {
  if (confirm(`Bạn có chắc chắn muốn xóa nhân viên có mã ${id} khỏi hệ thống không?`)) {
    try {
      const response = await fetch(`http://localhost:8001/api/hr/employees/${id}`, {
        method: 'DELETE'
      })

      if (response.ok) {
        alert(`Đã xóa nhân viên ${id} thành công khỏi hệ thống!`)
        fetchEmployees()
      } else {
        const err = await response.json()
        alert('Lỗi khi xóa: ' + JSON.stringify(err))
      }
    } catch (error) {
      console.error('Lỗi khi thực hiện xóa qua 8001:', error)
      alert('Không thể kết nối tới API Gateway cổng 8001 để xóa!')
    }
  }
}

const exportExcel = () => { alert('Hệ thống đang chuẩn bị kết nối API xuất Excel...') }
const exportPdf = () => { alert('Hệ thống đang chuẩn bị kết nối API xuất PDF...') }

onMounted(() => {
  fetchEmployees()
})

// 🟢 BẢN FIX: Chống lệch chữ hoa / chữ thường tuyệt đối ở Bộ lọc phòng ban
const filteredEmployees = computed(() => {
  return employees.value.filter(emp => {
    const name = emp.name ? emp.name.toLowerCase().trim() : ''
    const id = emp.id ? emp.id.toLowerCase().trim() : ''
    
    const matchesSearch = name.includes(search.value.toLowerCase().trim()) || id.includes(search.value.toLowerCase().trim())
    
    // Ép chữ thường và xóa khoảng trắng cả 2 vế để "Phòng Công nghệ" khớp hoàn toàn với "Phòng Công Nghệ"
    const matchesDept = selectedDept.value === 'Tất cả phòng ban' || 
      (emp.dept && emp.dept.toLowerCase().trim() === selectedDept.value.toLowerCase().trim())
      
    return matchesSearch && matchesDept
  })
})
</script>