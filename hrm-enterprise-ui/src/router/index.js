import { createRouter, createWebHistory } from 'vue-router'
import Login from '../views/Login.vue'
import DashboardLayout from '../layouts/DashboardLayout.vue'
import MainDashboard from '../views/MainDashboard.vue'
import EmployeeManagement from '../views/EmployeeManagement.vue'
import SelfServiceAttendance from '../views/SelfServiceAttendance.vue'
import AttendanceAdmin from '../views/AttendanceAdmin.vue' 
import SystemHealth from '../views/SystemHealth.vue'
import DepartmentTree from '../views/DepartmentTree.vue' 

// 🚀 IMPORT ĐẦY ĐỦ CÁC VIEWS PHÂN HỆ MỚI
import LeaveManagement from '../views/LeaveManagement.vue'
import LeaveRequest from '../views/LeaveRequest.vue'
import PayslipDetail from '../views/PayslipDetail.vue'
import PositionManagement from '../views/PositionManagement.vue' // <--- 🌟 BƯỚC 1: IMPORT TRANG CHỨC VỤ
import PayrollManagement from '../views/PayrollManagement.vue' // <--- 🌟 BƯỚC MỚI: IMPORT TRANG CHỐT LƯƠNG

const routes = [
  // 🔑 1. Trang Login độc lập hẳn ra ngoài, cấu hình path gốc '/' trỏ thẳng vào đây
  { path: '/', name: 'RootLogin', component: Login },
  { path: '/login', name: 'Login', component: Login },

  // 🔑 2. Cụm chức năng chạy Sidebar (Dùng một path bọc ngoài khác biệt hoàn toàn)
  {
    path: '/sys',
    component: DashboardLayout,
    children: [
      { path: 'dashboard', name: 'Dashboard', component: MainDashboard }, 
      { path: 'employees', name: 'Employees', component: EmployeeManagement }, 
      { path: 'organization', name: 'Organization', component: DepartmentTree },   
      { path: 'attendance-check', name: 'AttendanceCheck', component: SelfServiceAttendance }, 
      { path: 'attendance-admin', name: 'AttendanceAdmin', component: AttendanceAdmin }, 
      { path: 'system-health', name: 'SystemHealth', component: SystemHealth },
      
      // 🕒 🛠️ ĐÃ BỔ SUNG: Khai báo route cho trang Duyệt nghỉ phép của Manager
      { path: 'leave-requests', name: 'LeaveRequests', component: LeaveManagement },

      // 👤 Các tính năng tự phục vụ (Self-Service)
      { path: 'leave', name: 'LeaveRequest', component: LeaveRequest },
      { path: 'payslip', name: 'PayslipDetail', component: PayslipDetail },

      // 🚀 🌟 BƯỚC 2: KHAI BÁO PATH CON CHO QUẢN LÝ CHỨC VỤ
      { path: 'positions', name: 'PositionManagement', component: PositionManagement },

      // 💰 🌟 BƯỚC MỚI: KHAI BÁO PATH CHO QUẢN LÝ/CHỐT LƯƠNG
      { path: 'payroll-management', name: 'PayrollManagement', component: PayrollManagement }
    ]
  },
  
  // 🛰️ Trang dự phòng nếu gõ URL lỗi -> Tự động chuyển hướng về Login
  { path: '/:pathMatch(.*)*', redirect: '/login' }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

// 🔐 BẢO VỆ ROUTER: Kiểm tra quyền chặt chẽ từng phân quyền
router.beforeEach((to, from, next) => {
  const savedUser = localStorage.getItem('user_login')
  
  // 1. Nếu chưa đăng nhập mà cố tình vào các trang hệ thống bên trong -> Ép về Login
  if (to.path !== '/login' && to.path !== '/' && !savedUser) {
    return next('/login')
  }

  // 2. Nếu đã đăng nhập thành công
  if (savedUser) {
    const user = JSON.parse(savedUser)
    
    // Nếu đã login rồi mà cố gõ / hoặc /login -> Điều hướng thẳng vào trang chủ phù hợp với quyền
    if (to.path === '/login' || to.path === '/') {
      return user.role === 'Employee' ? next('/sys/attendance-check') : next('/sys/dashboard')
    }
    
    // 🛑 PHÂN QUYỀN: Nhân viên thường (Employee) chỉ được phép vào các trang Self-Service
    if (user.role === 'Employee') {
      const basicEmployeePaths = ['/sys/attendance-check', '/sys/leave', '/sys/payslip']
      if (!basicEmployeePaths.includes(to.path)) {
        return next('/sys/attendance-check')
      }
    }

    // 🛑 🌟 BƯỚC 3: PHÂN QUYỀN CHO QUẢN LÝ (Manager)
    // Chặn thêm trang '/sys/positions' vì chức năng cấu hình định mức lương/chức vụ này thuộc về Admin quyền tối cao.
    // 🛑 Chặn thêm '/sys/payroll-management' vì chốt lương cũng là tác vụ quyền cao, dành riêng cho Admin.
    if (user.role === 'Manager') {
      const managerBlockedPaths = ['/sys/employees', '/sys/system-health', '/sys/positions', '/sys/payroll-management']
      if (managerBlockedPaths.includes(to.path)) {
        // Cố tình vào trang cấm sẽ bị đá về trang dashboard công ty
        return next('/sys/dashboard')
      }
    }
  }

  // Đúng quyền -> Cho qua
  next()
})

export default router