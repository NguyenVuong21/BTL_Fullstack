using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nexus.HRCore.Models
{
    [Table("Attendances")]
    public class Attendance
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string EmployeeId { get; set; } // Tự động nhận diện ăn theo Db

        [Required]
        [Column(TypeName = "DATE")]
        public DateTime LogDate { get; set; }

        public TimeSpan? CheckIn { get; set; }
        
        public TimeSpan? CheckOut { get; set; }

        [StringLength(50)]
        // 💡 ĐÃ SỬA: Thêm dấu ? để biến thành Nullable string, giải quyết triệt để lỗi Validation 400
        public string? Status { get; set; } 
    }
}