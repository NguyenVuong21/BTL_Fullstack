using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Nexus.HRCore.Models
{
    public class Department
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        // ID của phòng ban cha (Nếu null tức là phòng ban gốc cao nhất)
        public int? ParentId { get; set; }

        [ForeignKey("ParentId")]
        [JsonIgnore] // Tránh vòng lặp vô hạn khi serialize JSON
        public virtual Department? Parent { get; set; }

        // Danh sách các phòng ban con thuộc phòng này
        public virtual ICollection<Department> Children { get; set; } = new List<Department>();

        // Danh sách nhân viên thuộc phòng ban
        [JsonIgnore]
        public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
    }
}