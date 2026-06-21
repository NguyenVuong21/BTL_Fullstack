using System;
using Microsoft.EntityFrameworkCore;
using Nexus.HRCore.Models;

namespace Nexus.HRCore.Data
{
    public class HRDbContext : DbContext
    {
        public HRDbContext(DbContextOptions<HRDbContext> options) : base(options) { }

        public DbSet<Department> Departments { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Attendance> Attendances { get; set; } 
        public DbSet<User> Users { get; set; } 
        
        // 🕒 🛠️ ĐÃ THÊM: Đăng ký bảng quản lý nghỉ phép vào bối cảnh dữ liệu
        public DbSet<LeaveRequest> LeaveRequests { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Bỏ qua cơ chế cảnh báo thay đổi Model nghiêm ngặt của .NET
            modelBuilder.HasAnnotation("ProductVersion", "10.0.0");

            // 1. Cấu hình mối quan hệ tự tham chiếu dạng cây cho Phòng Ban
            modelBuilder.Entity<Department>()
                .HasOne(d => d.Parent)
                .WithMany(d => d.Children)
                .HasForeignKey(d => d.ParentId)
                .OnDelete(DeleteBehavior.Restrict);

            // 2. Cấu hình khoá ngoại rõ ràng giữa User và Employee (Chống lỗi cascade delete)
            modelBuilder.Entity<User>()
                .HasOne(u => u.Employee)
                .WithMany() // Một nhân viên có thể có 1 tài khoản
                .HasForeignKey(u => u.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            // 3. Khởi tạo dữ liệu mẫu cho Phòng Ban
            modelBuilder.Entity<Department>().HasData(
                new Department { Id = 1, Name = "Ban Giám Đốc", ParentId = null },
                new Department { Id = 2, Name = "Phòng Công Nghệ", ParentId = 1 },
                new Department { Id = 3, Name = "Phòng Nhân Sự", ParentId = 1 }
            );

            // 4. Khởi tạo dữ liệu mẫu cho Nhân viên
            modelBuilder.Entity<Employee>().HasData(
                new Employee { EmployeeId = "NV001", FullName = "Nguyễn Văn Vượng", Email = "vuong.nv@nexus-hrm.io", DepartmentId = 2, Position = "Kỹ sư Phần mềm", ContractType = "Chính thức", Status = "Hoạt động", JoinDate = new DateTime(2024, 1, 15) },
                new Employee { EmployeeId = "NV002", FullName = "Trần Thị Mai", Email = "mai.tt@nexus-hrm.io", DepartmentId = 3, Position = "Trưởng phòng HR", ContractType = "Chính thức", Status = "Hoạt động", JoinDate = new DateTime(2023, 5, 10) }
            );
        }
    }
}