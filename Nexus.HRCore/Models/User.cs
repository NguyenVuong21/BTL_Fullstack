using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nexus.HRCore.Models
{
    [Table("Users")]
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Username { get; set; }

        [Required]
        public string PasswordHash { get; set; } // Mật khẩu đã mã hoá

        [Required]
        [StringLength(20)]
        public string Role { get; set; } // "Admin" hoặc "Employee"

        // Liên kết 1-1 hoặc nhiều-1 với Nhân viên
        [Required]
        public string EmployeeId { get; set; }

        [ForeignKey("EmployeeId")]
        public Employee? Employee { get; set; }
    }
}