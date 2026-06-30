<template>
  <v-app id="hrm-app">
    <v-navigation-drawer v-model="drawer" fixed class="sidebar-glass" width="280">
      <div class="pa-5 d-flex align-center">
        <v-icon color="primary" size="28" class="mr-2">mdi-layers-triple</v-icon>
        <span class="text-h6 font-weight-black text-white">NEXUS HRM</span>
        <v-chip size="x-small" color="accent" class="ml-2" variant="flat">{{ currentRole }}</v-chip>
      </div>

      <v-divider class="rgba-white-divider"></v-divider>

      <v-list density="compact" nav class="px-3 py-4">
        
        <div v-if="currentRole !== 'Employee'">
          <v-list-subheader class="text-disabled font-weight-bold text-uppercase text-caption">Tổng quan</v-list-subheader>
          <v-list-item prepend-icon="mdi-view-dashboard-outline" title="Dashboard" to="/sys/dashboard" value="db"></v-list-item>
        </div>

        <div v-if="currentRole === 'Admin'">
  <v-list-subheader class="text-disabled font-weight-bold text-uppercase text-caption mt-4">HR Core Service</v-list-subheader>
  <v-list-item prepend-icon="mdi-account-multiple-outline" title="Hồ sơ nhân viên" to="/sys/employees" value="emp"></v-list-item>
  <v-list-item prepend-icon="mdi-sitemap-outline" title="Cơ cấu tổ chức (Tree)" to="/sys/organization" value="org"></v-list-item>
  <v-list-item prepend-icon="mdi-briefcase-variant-outline" title="Quản lý chức vụ" to="/sys/positions" value="pos"></v-list-item>
  <v-list-item prepend-icon="mdi-cash-register" title="Quản lý Bảng lương" to="/sys/payroll-management" value="payroll-mgmt"></v-list-item>
</div>

        <div v-if="currentRole === 'Admin' || currentRole === 'Manager'">
          <v-list-subheader class="text-disabled font-weight-bold text-uppercase text-caption mt-4">Attendance Service</v-list-subheader>
          <v-list-item prepend-icon="mdi-calendar-check-outline" title="Quản lý chấm công" to="/sys/attendance-admin" value="att-adm"></v-list-item>
          <v-list-item prepend-icon="mdi-email-open-multiple-outline" title="Duyệt nghỉ phép" to="/sys/leave-requests" value="leave-adm"></v-list-item>
        </div>

        <v-list-subheader class="text-disabled font-weight-bold text-uppercase text-caption mt-4">Cá nhân (Self-Service)</v-list-subheader>
        <v-list-item prepend-icon="mdi-fingerprint" title="Chấm công & Check-In" to="/sys/attendance-check" value="ss-att"></v-list-item>
        <v-list-item prepend-icon="mdi-file-document-edit-outline" title="Đăng ký nghỉ phép" to="/sys/leave" value="ss-leave"></v-list-item>
        <v-list-item prepend-icon="mdi-cash-multiple" title="Phiếu lương chi tiết" to="/sys/payslip" value="ss-pay"></v-list-item>

        <div v-if="currentRole === 'Admin'">
          <v-list-subheader class="text-disabled font-weight-bold text-uppercase text-caption mt-4">Hạ tầng Đồ án</v-list-subheader>
          <v-list-item prepend-icon="mdi-server-network" title="Trạng thái Microservices" to="/sys/system-health" value="health"></v-list-item>
        </div>
      </v-list>
    </v-navigation-drawer>

    <v-app-bar flat class="navbar-glass px-4">
      <v-app-bar-nav-icon @click="drawer = !drawer"></v-app-bar-nav-icon>
      <v-spacer></v-spacer>
      
      <v-btn icon class="mr-2" @click="toggleTheme">
        <v-icon>{{ isDark ? 'mdi-white-balance-sunny' : 'mdi-weather-night' }}</v-icon>
      </v-btn>

      <v-badge dot color="error" class="mr-4">
        <v-icon>mdi-bell-outline</v-icon>
      </v-badge>

      <v-menu min-width="200px" rounded>
        <template v-slot:activator="{ props }">
          <v-avatar color="primary" size="38" class="cursor-pointer" v-bind="props">
            <span class="text-subtitle-2 font-weight-bold">{{ userInitials }}</span>
          </v-avatar>
        </template>
        <v-card class="mt-2 border border-slate-800" color="#111827">
          <v-card-text class="pa-4 text-center">
            <h3 class="text-subtitle-1 font-weight-bold text-white">{{ fullName }}</h3>
            <p class="text-caption text-disabled mt-1">{{ currentRole }}</p>
            <v-divider class="my-3 rgba-white-divider"></v-divider>
            <v-btn color="error" variant="tonal" block size="small" prepend-icon="mdi-logout" @click="handleLogout">
              Đăng xuất tài khoản
            </v-btn>
          </v-card-text>
        </v-card>
      </v-menu>
    </v-app-bar>

    <v-main class="workspace-bg">
      <v-container fluid class="pa-6">
        <router-view />
      </v-container>
    </v-main>
  </v-app>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue'
import { useTheme } from 'vuetify'

const drawer = ref(true)
const theme = useTheme()
const isDark = ref(true)
const currentRole = ref('Employee')
const fullName = ref('Nhân sự Nexus')

// Tạo chữ viết tắt cho Avatar (Ví dụ: Nguyễn Văn C -> NV)
const userInitials = computed(() => {
  if (!fullName.value) return 'NV'
  const parts = fullName.value.trim().split(' ')
  if (parts.length >= 2) {
    return (parts[parts.length - 2][0] + parts[parts.length - 1][0]).toUpperCase()
  }
  return parts[0].substring(0, 2).toUpperCase()
})

onMounted(() => {
  const savedUser = localStorage.getItem('user_login')
  if (savedUser) {
    const user = JSON.parse(savedUser)
    currentRole.value = user.role
    fullName.value = user.fullName || 'Nhân sự Nexus'
  }
})

// 🚀 HÀM ĐĂNG XUẤT THẬT: Xóa bộ nhớ và ép về trang đăng nhập đầu tiên
const handleLogout = () => {
  localStorage.removeItem('user_login')
  window.location.href = '/login'
}

const toggleTheme = () => {
  isDark.value = !isDark.value
  theme.global.name.value = isDark.value ? 'darkGlassTheme' : 'lightGlassTheme'
}
</script>

<style scoped>
.sidebar-glass {
  background: rgba(13, 20, 38, 0.8) !important;
  backdrop-filter: blur(25px);
  border-right: 1px solid rgba(255, 255, 255, 0.05) !important;
}
.navbar-glass {
  background: rgba(9, 13, 22, 0.7) !important;
  backdrop-filter: blur(15px);
  border-bottom: 1px solid rgba(255, 255, 255, 0.05) !important;
}
.workspace-bg {
  background-color: rgb(var(--v-theme-background));
}
.rgba-white-divider {
  border-color: rgba(255, 255, 255, 0.06);
}
.cursor-pointer {
  cursor: pointer;
}
</style>