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
        public string Position { get; set; } = string.Empty; // Kỹ sư, Trưởng phòng...

        [StringLength(30)]
        public string ContractType { get; set; } = "Chính thức"; // Chính thức, Thử việc, Part-time

        public DateTime JoinDate { get; set; } // Để trống phần gán mặc định để tránh lỗi model thay đổi liên tục

        [StringLength(20)]
        public string Status { get; set; } = "Hoạt động";

        // Khóa ngoại nối tới Phòng ban
        public int DepartmentId { get; set; }
        
        [ForeignKey("DepartmentId")]
        public virtual Department? Department { get; set; }
    }
}