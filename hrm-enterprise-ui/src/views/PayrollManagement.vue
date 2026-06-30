<template>
  <v-container fluid>
    <div class="d-flex justify-space-between align-center mb-6">
      <div>
        <h1 class="text-h4 font-weight-bold text-white">Quản lý Bảng lương</h1>
        <p class="text-subtitle-2 text-disabled">Chốt lương nhân viên — mức lương cơ bản tự động đồng bộ theo Chức vụ</p>
      </div>
      <v-btn color="primary" prepend-icon="mdi-plus" class="font-weight-bold" @click="openCalculateDialog">
        Chốt lương mới
      </v-btn>
    </div>

    <v-card class="border border-slate-800 rounded-xl pa-2" color="#111827">
      <v-table hover>
        <thead>
          <tr>
            <th class="text-left text-slate-300">Mã NV</th>
            <th class="text-left text-slate-300">Họ tên</th>
            <th class="text-left text-slate-300">Kỳ lương</th>
            <th class="text-right text-slate-300">Lương cơ bản</th>
            <th class="text-right text-slate-300">Phụ cấp</th>
            <th class="text-right text-slate-300">Khấu trừ</th>
            <th class="text-right text-slate-300">Thực lĩnh</th>
            <th class="text-center text-slate-300">Trạng thái</th>
            <th class="text-center text-slate-300">Hành động</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="p in payrolls" :key="p.id">
            <td class="text-cyan-accent-2 font-weight-bold">{{ p.employeeId }}</td>
            <td class="text-white">{{ p.fullName }}</td>
            <td class="text-slate-300">{{ p.month }}/{{ p.year }}</td>
            <td class="text-right text-white">{{ formatMoney(p.BasicSalary) }}</td>
            <td class="text-right text-white">{{ formatMoney(p.Allowances) }}</td>
            <td class="text-right text-error">-{{ formatMoney(p.Deductions) }}</td>
            <td class="text-right text-success font-weight-bold">{{ formatMoney(p.NetSalary) }}</td>
            <td class="text-center">
              <v-chip :color="p.status === 'Đã thanh toán' ? 'success' : 'warning'" size="small" variant="tonal">
                {{ p.status }}
              </v-chip>
            </td>
            <td class="text-center">
              <v-btn
                v-if="p.status !== 'Đã thanh toán'"
                size="small"
                color="primary"
                variant="tonal"
                class="mr-1"
                @click="markAsPaid(p.id)"
              >
                Thanh toán
              </v-btn>
              <v-btn
                size="small"
                color="error"
                variant="text"
                icon="mdi-delete"
                @click="deletePayroll(p.id)"
              ></v-btn>
            </td>
          </tr>
          <tr v-if="payrolls.length === 0">
            <td colspan="9" class="text-center text-disabled py-6">Chưa có phiếu lương nào được chốt.</td>
          </tr>
        </tbody>
      </v-table>
    </v-card>

    <!-- Dialog Chốt lương mới -->
    <v-dialog v-model="isDialogOpen" max-width="520">
      <v-card color="#111827" class="border border-slate-800 rounded-xl pa-4 text-white">
        <v-card-title class="text-h5 font-weight-bold text-cyan-accent-2">
          Chốt lương nhân viên
        </v-card-title>
        <v-card-text>
          <v-select
            v-model="selectedEmployeeId"
            :items="employeeOptions"
            item-title="label"
            item-value="value"
            label="Chọn nhân viên"
            variant="outlined"
            class="mb-3"
            @update:model-value="onEmployeeChange"
          ></v-select>

          <v-row dense>
            <v-col cols="6">
              <v-select
                v-model.number="form.month"
                :items="Array.from({length:12}, (_, i) => i + 1)"
                label="Tháng"
                variant="outlined"
                @update:model-value="syncLateFine"
              ></v-select>
            </v-col>
            <v-col cols="6">
              <v-text-field
                v-model.number="form.year"
                label="Năm"
                type="number"
                variant="outlined"
                @update:model-value="syncLateFine"
              ></v-text-field>
            </v-col>
          </v-row>

          <v-text-field
            v-model.number="form.basicSalary"
            label="Lương cơ bản (tự động theo chức vụ — có thể chỉnh)"
            type="number"
            variant="outlined"
            class="mb-3"
            :hint="positionHint"
            persistent-hint
          ></v-text-field>

          <v-text-field
            v-model.number="form.allowances"
            label="Phụ cấp"
            type="number"
            variant="outlined"
            class="mb-3 mt-3"
          ></v-text-field>

          <v-row dense class="mb-1">
            <v-col cols="7">
              <v-text-field
                v-model.number="finePerLate"
                label="Mức phạt / lần đi muộn"
                type="number"
                variant="outlined"
                density="compact"
                @update:model-value="recalcDeductions"
              ></v-text-field>
            </v-col>
            <v-col cols="5" class="d-flex align-center">
              <v-chip color="error" variant="tonal" class="font-weight-bold">
                {{ lateCount }} lần đi muộn trong kỳ
              </v-chip>
            </v-col>
          </v-row>

          <v-text-field
            v-model.number="form.deductions"
            label="Tổng khấu trừ (BHXH 10.5% + Phạt đi muộn)"
            type="number"
            variant="outlined"
            append-inner-icon="mdi-calculator-variant"
            @click:append-inner="recalcDeductions"
            hint="Bấm icon máy tính để tự tính lại theo công thức"
            persistent-hint
          ></v-text-field>
        </v-card-text>
        <v-card-actions>
          <v-spacer></v-spacer>
          <v-btn variant="text" color="slate-400" @click="isDialogOpen = false">Hủy</v-btn>
          <v-btn color="primary" variant="flat" class="px-4 font-weight-bold" :loading="saving" @click="submitCalculate">
            Chốt lương
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>
  </v-container>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'

const GATEWAY = 'http://localhost:8001/api/hr'

const payrolls = ref([])
const employees = ref([])
const positions = ref([])

const isDialogOpen = ref(false)
const saving = ref(false)
const selectedEmployeeId = ref(null)

const form = ref({
  month: new Date().getMonth() + 1,
  year: new Date().getFullYear(),
  basicSalary: 0,
  allowances: 0,
  deductions: 0
})

const lateCount = ref(0)
const finePerLate = ref(100000) // Mức phạt mỗi lần đi muộn, có thể chỉnh

// Danh sách nhân viên cho dropdown
// Lưu ý: API /api/hr/employees trả về field "id" và "name" (không phải employeeId/fullName)
const employeeOptions = computed(() =>
  employees.value.map(e => ({
    label: `${e.id} - ${e.name}`,
    value: e.id
  }))
)

// Gợi ý hiển thị nhân viên đang giữ chức vụ gì, mức lương định mức bao nhiêu
// Employee chỉ có field "position" dạng TÊN chuỗi (vd "Giám Đốc"),
// nên phải đối chiếu theo TÊN với position.name, không có positionId dạng số
const positionHint = computed(() => {
  const emp = employees.value.find(e => e.id === selectedEmployeeId.value)
  if (!emp) return ''
  const pos = positions.value.find(p => p.name === emp.position)
  return pos ? `Chức vụ: ${pos.name} — định mức ${formatMoney(pos.baseSalary)}` : `Chức vụ "${emp.position}" chưa có định mức lương`
})

// ====== FETCH DỮ LIỆU ======
const fetchPayrolls = async () => {
  try {
    const res = await fetch(`${GATEWAY}/payroll/all`)
    if (res.ok) payrolls.value = await res.json()
  } catch (err) {
    console.error('Lỗi tải bảng lương:', err)
  }
}

const fetchEmployees = async () => {
  try {
    const res = await fetch(`${GATEWAY}/employees`)
    if (res.ok) employees.value = await res.json()
  } catch (err) {
    console.error('Lỗi tải danh sách nhân viên:', err)
  }
}

const fetchPositions = async () => {
  try {
    const res = await fetch(`${GATEWAY}/positions`)
    if (res.ok) positions.value = await res.json()
  } catch (err) {
    console.error('Lỗi tải danh sách chức vụ:', err)
  }
}

// ====== DIALOG ======
const openCalculateDialog = () => {
  selectedEmployeeId.value = null
  lateCount.value = 0
  form.value = {
    month: new Date().getMonth() + 1,
    year: new Date().getFullYear(),
    basicSalary: 0,
    allowances: 0,
    deductions: 0
  }
  isDialogOpen.value = true
}

// 🎯 Khi chọn nhân viên -> tự động đồng bộ lương cơ bản theo Chức vụ hiện tại của họ
// Đối chiếu theo TÊN chức vụ (employee.position === position.name)
// Đồng thời tự tính khấu trừ = BHXH & Y tế (10.5% lương cơ bản) + Phạt đi muộn (đếm từ Attendance Service - N2)
const INSURANCE_RATE = 0.105

const onEmployeeChange = async () => {
  const emp = employees.value.find(e => e.id === selectedEmployeeId.value)
  if (!emp) return
  const pos = positions.value.find(p => p.name === emp.position)
  const basic = pos ? pos.baseSalary : 0
  form.value.basicSalary = basic

  await syncLateFine()
}

// 📅 Gọi sang Attendance Service (N2) lấy lịch sử chấm công, đếm số lần "Đi muộn" đúng tháng/năm đang chọn
const syncLateFine = async () => {
  if (!selectedEmployeeId.value) return
  try {
    const res = await fetch(`${GATEWAY}/attendance/history/${selectedEmployeeId.value}`)
    if (res.ok) {
      const history = await res.json()
      const targetMonth = String(form.value.month).padStart(2, '0')
      const targetYear = String(form.value.year)

      const lateRecords = Array.isArray(history)
        ? history.filter(r => {
            // date dạng "2026-06-30" -> tách năm/tháng để so khớp đúng kỳ lương đang chọn
            const [y, m] = (r.date || '').split('-')
            return y === targetYear && m === targetMonth && r.status === 'Đi muộn'
          })
        : []

      lateCount.value = lateRecords.length
    } else {
      lateCount.value = 0
    }
  } catch (err) {
    console.error('Lỗi tải lịch sử chấm công từ Attendance Service (N2):', err)
    lateCount.value = 0
  } finally {
    recalcDeductions()
  }
}

// 💰 Tính tổng khấu trừ = BHXH 10.5% + (số lần đi muộn × mức phạt mỗi lần)
const recalcDeductions = () => {
  const insurance = Math.round((form.value.basicSalary || 0) * INSURANCE_RATE)
  const fine = lateCount.value * finePerLate.value
  form.value.deductions = insurance + fine
}

const submitCalculate = async () => {
  if (!selectedEmployeeId.value) {
    alert('Vui lòng chọn nhân viên!')
    return
  }
  saving.value = true
  try {
    const payload = {
      employeeId: selectedEmployeeId.value,
      month: form.value.month,
      year: form.value.year,
      basicSalary: form.value.basicSalary,
      allowances: form.value.allowances,
      deductions: form.value.deductions
    }
    const res = await fetch(`${GATEWAY}/payroll/calculate`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload)
    })
    if (res.ok) {
      isDialogOpen.value = false
      await fetchPayrolls()
    } else {
      const err = await res.json().catch(() => null)
      alert(err?.message || 'Chốt lương thất bại!')
    }
  } catch (err) {
    console.error('Lỗi chốt lương:', err)
    alert('Lỗi kết nối tới Gateway!')
  } finally {
    saving.value = false
  }
}

const markAsPaid = async (id) => {
  if (!confirm('Xác nhận đã thanh toán lương cho phiếu này?')) return
  try {
    const res = await fetch(`${GATEWAY}/payroll/${id}/pay`, { method: 'PUT' })
    if (res.ok) await fetchPayrolls()
  } catch (err) {
    console.error('Lỗi cập nhật trạng thái thanh toán:', err)
  }
}

// 🗑️ Xóa phiếu lương (dùng để dọn các bản ghi tạo trùng/sai)
const deletePayroll = async (id) => {
  if (!confirm(`Xác nhận xóa phiếu lương Id=${id}? Hành động này không thể hoàn tác.`)) return
  try {
    const res = await fetch(`${GATEWAY}/payroll/${id}`, { method: 'DELETE' })
    if (res.ok) {
      await fetchPayrolls()
    } else {
      const err = await res.json().catch(() => null)
      alert(err?.message || 'Xóa phiếu lương thất bại!')
    }
  } catch (err) {
    console.error('Lỗi xóa phiếu lương:', err)
    alert('Lỗi kết nối tới Gateway!')
  }
}

const formatMoney = (val) => {
  if (val === undefined || val === null) return '0 VND'
  return val.toLocaleString('vi-VN') + ' VND'
}

onMounted(async () => {
  await Promise.all([fetchPayrolls(), fetchEmployees(), fetchPositions()])
})
</script>