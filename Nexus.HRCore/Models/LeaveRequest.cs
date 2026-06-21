using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nexus.HRCore.Models
{
    [Table("LeaveRequests")] // Ép cứng tên bảng dưới SQL Server
    public class LeaveRequest
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Tự động tăng ID
        public int Id { get; set; }
        
        public string EmployeeId { get; set; } = string.Empty;
        
        public string LeaveType { get; set; } = string.Empty;
        
        public DateTime StartDate { get; set; }
        
        public DateTime EndDate { get; set; }
        
        public string Reason { get; set; } = string.Empty;
        
        public string Status { get; set; } = "Chờ duyệt";
    }
}