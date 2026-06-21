<template>
  <div>
    <div class="mb-6">
      <h1 class="text-h4 font-weight-bold tracking-tight">Sơ đồ Cơ cấu Tổ chức</h1>
      <p class="text-subtitle-2 text-disabled">Mô hình phân cấp phòng ban hình cây thuộc phân hệ HR Core Service</p>
    </div>

    <v-row>
      <v-col cols="12" md="5">
        <v-card class="pa-4 fill-height" min-height="450px">
          <div class="text-subtitle-1 font-weight-bold mb-4 text-primary">
            <v-icon icon="mdi-sitemap-outline" class="mr-2"></v-icon>Phân cấp Phòng ban
          </div>

          <v-divider class="mb-4"></v-divider>

          <v-list open-strategy="multiple" class="bg-transparent">
            <template v-for="dept in departmentTree">
              <v-list-group v-if="dept.children && dept.children.length > 0" :key="'group-' + dept.id" :value="dept.name">
                <template v-slot:activator="{ props }">
                  <v-list-item
                    v-bind="props"
                    prepend-icon="mdi-domain"
                    :title="dept.name"
                    :subtitle="`Mã PB: ${dept.id}`"
                    @click="selectDepartment(dept)"
                    :class="{ 'bg-grey-darken-3 text-primary': selectedDeptId === dept.id }"
                    class="rounded-lg mb-1"
                  ></v-list-item>
                </template>

                <v-list-item
                  v-for="subDept in dept.children"
                  :key="'sub-' + subDept.id"
                  prepend-icon="mdi-account-hierarchy-outline"
                  :title="subDept.name"
                  :subtitle="`Mã PB: ${subDept.id}`"
                  @click="selectDepartment(subDept)"
                  :class="{ 'bg-grey-darken-3 text-primary': selectedDeptId === subDept.id }"
                  class="pl-8 rounded-lg mb-1"
                ></v-list-item>
              </v-list-group>

              <v-list-item
                v-else
                :key="'item-' + dept.id"
                prepend-icon="mdi-domain"
                :title="dept.name"
                :subtitle="`Mã PB: ${dept.id}`"
                @click="selectDepartment(dept)"
                :class="{ 'bg-grey-darken-3 text-primary': selectedDeptId === dept.id }"
                class="rounded-lg mb-1"
              ></v-list-item>
            </template>
          </v-list>
          
          <div v-if="departmentTree.length === 0" class="text-center text-disabled py-8">
            Đang tải cấu trúc sơ đồ cây...
          </div>
        </v-card>
      </v-col>

      <v-col cols="12" md="7">
        <v-card class="pa-5 fill-height">
          <div class="text-subtitle-1 font-weight-bold mb-4 text-success">
            <v-icon icon="mdi-account-group-outline" class="mr-2"></v-icon>
            Nhân sự thuộc: <span class="text-white">{{ selectedDeptName }}</span>
          </div>

          <v-divider class="mb-4"></v-divider>

          <v-table hover v-if="filteredEmployees.length > 0">
            <thead>
              <tr>
                <th class="font-weight-bold">Mã NV</th>
                <th class="font-weight-bold">Họ và Tên</th>
                <th class="font-weight-bold">Chức vụ</th>
                <th class="font-weight-bold">Trạng thái</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="emp in filteredEmployees" :key="emp.id">
                <td><v-chip size="small" variant="tonal" color="primary">{{ emp.id }}</v-chip></td>
                <td>
                  <div class="font-weight-bold">{{ emp.name }}</div>
                  <div class="text-caption text-disabled">{{ emp.email }}</div>
                </td>
                <td>{{ emp.position }}</td>
                <td>
                  <v-badge dot inline :color="emp.status === 'Hoạt động' ? 'success' : 'grey'"></v-badge>
                  <span class="text-caption ml-1">{{ emp.status }}</span>
                </td>
              </tr>
            </tbody>
          </v-table>

          <div v-else class="text-center text-disabled py-12 border border-dashed rounded-xl mt-4">
            <v-icon size="40" class="mb-2">mdi-account-search-outline</v-icon>
            <p>Không có nhân viên nào thuộc phòng ban này hoặc bạn chưa chọn phòng ban.</p>
          </div>
        </v-card>
      </v-col>
    </v-row>
  </div>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue'

// --- KHAI BÁO BIẾN TRẠNG THÁI QUẢN LÝ ---
const departmentTree = ref([])
const allEmployees = ref([])
const selectedDeptId = ref(null)
const selectedDeptName = ref('Chưa chọn')

// 1. GỌI API ĐỂ LẤY CÂY PHÒNG BAN TỪ SQL SERVER QUA GATEWAY 8001
const fetchDepartmentTree = async () => {
  try {
    // 🚀 ĐÃ SỬA: Chuyển sang cổng 8001
    const response = await fetch('http://localhost:8001/api/hr/departments/tree')
    if (response.ok) {
      departmentTree.value = await response.json()
    }
  } catch (error) {
    console.error('Lỗi lấy cây phòng ban qua Gateway 8001:', error)
  }
}

// 2. GỌI API LẤY DANH SÁCH TẤT CẢ NHÂN VIÊN QUA GATEWAY 8001
const fetchAllEmployees = async () => {
  try {
    // 🚀 ĐÃ SỬA: Chuyển sang cổng 8001
    const response = await fetch('http://localhost:8001/api/hr/employees')
    if (response.ok) {
      allEmployees.value = await response.json()
    }
  } catch (error) {
    console.error('Lỗi lấy danh sách nhân viên qua Gateway 8001:', error)
  }
}

// Xử lý sự kiện khi Vượng click vào một nút phòng ban cụ thể trên cây
const selectDepartment = (dept) => {
  selectedDeptId.value = dept.id
  selectedDeptName.value = dept.name
}

// BỘ LỌC TỰ ĐỘNG (COMPUTED): Lọc ra nhân viên thuộc phòng ban đang được click chọn
const filteredEmployees = computed(() => {
  if (!selectedDeptId.value) return []
  return allEmployees.value.filter(emp => emp.dept === selectedDeptName.value)
})

// Tự động kích hoạt khi trang web được tải
onMounted(() => {
  fetchDepartmentTree()
  fetchAllEmployees()
})
</script>

<style scoped>
.v-list-item--active {
  color: inherit !important;
}
</style>