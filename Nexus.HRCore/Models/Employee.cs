using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nexus.HRCore.Models
{
    public class Employee
    {
        [Key]
        [StringLength(20)]
        public string EmployeeId { get; set; } = string.Empty; // Ví dụ: NV001, NV002

        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Email { get; set; } = string.Empty;

        [StringLength(50)]
        public string Position { get; set; } = string.Empty; // Lưu chữ cũ

        // --- CHỈ GIỮ LẠI CỘT NÀY ĐỂ ĐỒNG BỘ DỮ LIỆU SỐ VỚI DATABASE ---
        public int? PositionId { get; set; } 
        // -------------------------------------------------------------

        [StringLength(30)]
        public string ContractType { get; set; } = "Chính thức"; 

        public DateTime JoinDate { get; set; } 

        [StringLength(20)]
        public string Status { get; set; } = "Hoạt động";

        // Khóa ngoại nối tới Phòng ban
        public int DepartmentId { get; set; }
        
        [ForeignKey("DepartmentId")]
        public virtual Department? Department { get; set; }
    }
}