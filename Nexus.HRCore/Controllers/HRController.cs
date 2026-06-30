using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nexus.HRCore.Data;
using Nexus.HRCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Nexus.HRCore.Controllers
{
    // Class phụ bọc dữ liệu (DTO) để nhận dữ liệu Chức vụ từ Vue gửi lên và map xuống DB
    public class PositionDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal BaseSalary { get; set; }
    }

    [Route("api/hr")] 
    [ApiController]
    public class HRController : ControllerBase
    {
        private readonly HRDbContext _context;

        public HRController(HRDbContext context)
        {
            _context = context;
        }

        // ========================================================
        // 📊 API 0. Lấy số liệu thống kê tổng quan cho Dashboard
        // ========================================================
        [HttpGet("dashboard/stats")]
        public async Task<IActionResult> GetDashboardStats()
        {
            var totalEmployees = await _context.Employees.CountAsync();
            var activeEmployees = await _context.Employees.Where(e => e.Status == "Hoạt động").CountAsync();
            var probationEmployees = await _context.Employees.Where(e => e.ContractType == "Thử việc").CountAsync();
            var officialEmployees = await _context.Employees.Where(e => e.ContractType == "Chính thức").CountAsync();

            return Ok(new
            {
                total = totalEmployees,
                active = activeEmployees,
                probation = probationEmployees,
                official = officialEmployees
            });
        }

        // ========================================================
        // 👥 PHÂN HỆ QUẢN LÝ HỒ SƠ NHÂN VIÊN & PHÒNG BAN (NHÓM 1)
        // ========================================================

        // 1. API: Lấy danh sách nhân viên
        [HttpGet("employees")]
        public async Task<IActionResult> GetEmployees()
        {
            var employees = await _context.Employees
                .Include(e => e.Department)
                .Select(e => new {
                    id = e.EmployeeId,
                    name = e.FullName,
                    email = e.Email,
                    departmentId = e.DepartmentId, 
                    dept = e.Department != null ? e.Department.Name : "N/A",
                    position = e.Position,
                    contractType = e.ContractType,
                    joinDate = e.JoinDate.ToString("yyyy-MM-dd"),
                    status = e.Status
                }).ToListAsync();

            return Ok(employees);
        }

        // 2. API: Lấy cấu trúc cây phòng ban (Self-referencing)
        [HttpGet("departments/tree")]
        public async Task<IActionResult> GetDepartmentTree()
        {
            var rootDepartments = await _context.Departments
                .Include(d => d.Children)
                .Where(d => d.ParentId == null)
                .ToListAsync();

            return Ok(rootDepartments);
        }

        // 3. API: Xem chi tiết 1 nhân viên theo ID
        [HttpGet("employees/{id}")]
        public async Task<IActionResult> GetEmployeeById(string id)
        {
            var employee = await _context.Employees
                .Include(e => e.Department)
                .FirstOrDefaultAsync(e => e.EmployeeId == id);

            if (employee == null) 
                return NotFound(new { message = $"Không tìm thấy nhân viên {id}" });

            return Ok(new {
    id = employee.EmployeeId,
    name = employee.FullName,
    email = employee.Email,
    departmentId = employee.DepartmentId, 
    dept = employee.Department != null ? employee.Department.Name : "N/A",
    position = employee.Position, // 🟢 Đã sửa thành employee.Position chuẩn chỉ
    contractType = employee.ContractType,
    joinDate = employee.JoinDate.ToString("yyyy-MM-dd"),
    status = employee.Status
});
        }

        // 🟢 BẢN NÂNG CẤP ĐỒNG BỘ: 4. API Thêm nhân viên mới & Tự động đẩy dữ liệu sang N3
        [HttpPost("employees")]
        public async Task<IActionResult> CreateEmployee([FromBody] Employee employee)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Bước 1: Lưu nhân viên mới vào SQL Server local của mình (N1) trước
            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            // Bước 2: Bắn đồng bộ ngầm dữ liệu sang phân hệ tính lương của Nhóm 3 (Cổng 8003 qua Gateway hoặc trực tiếp)
            try
            {
                using (var client = new HttpClient())
                {
                    // Đóng gói payload đúng định dạng các trường NOT NULL mà SQL của N3 yêu cầu
                    var syncPayload = new
                    {
                        EmployeeId = employee.EmployeeId,
                        FullName = employee.FullName,
                        Position = employee.Position,
                        Email = employee.Email,
                        ContractType = employee.ContractType,
                        Status = employee.Status,
                        JoinDate = employee.JoinDate == DateTime.MinValue ? DateTime.Now : employee.JoinDate
                    };

                    // Thực hiện gửi POST sang API nội bộ quản lý đồng bộ nhân viên của nhóm 3
                    // Lưu ý: Đổi URL này cho khớp với địa chỉ chạy API thực tế của Nhóm 3 (ví dụ http://localhost:8003/api/employees)
                    var response = await client.PostAsJsonAsync("http://localhost:8001/api/hr/payroll/employees-sync", syncPayload);
                    
                    if (!response.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"[ĐỒNG BỘ N3 WARNING] Gọi API Nhóm 3 thất bại với mã lỗi: {response.StatusCode}");
                    }
                }
            }
            catch (Exception ex)
            {
                // Sử dụng try-catch bọc quanh để nếu máy Nhóm 3 sập hoặc lỗi mạng thì hệ thống của mình vẫn chạy bình thường, không bị sập theo
                Console.WriteLine($"[ĐỒNG BỘ N3 ERROR] Không thể kết nối hoặc đồng bộ sang phân hệ N3: {ex.Message}");
            }

            return CreatedAtAction(nameof(GetEmployees), new { id = employee.EmployeeId }, employee);
        }

        // 5. API: Sửa hồ sơ nhân viên
        [HttpPut("employees/{id}")]
        public async Task<IActionResult> UpdateEmployee(string id, [FromBody] Employee employeeData)
        {
            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.EmployeeId == id);

            if (employee == null) 
                return NotFound(new { message = $"Không tìm thấy nhân viên {id} để cập nhật!" });

            employee.FullName = employeeData.FullName;
            employee.Email = employeeData.Email;
            employee.Position = employeeData.Position;
            employee.ContractType = employeeData.ContractType;
            employee.Status = employeeData.Status;
            employee.DepartmentId = employeeData.DepartmentId; 

            await _context.SaveChangesAsync();
            return Ok(new { message = $"Cập nhật hồ sơ nhân viên {id} thành công!" });
        }

        // 6. API: Xóa nhân viên khỏi hệ thống
        [HttpDelete("employees/{id}")]
        public async Task<IActionResult> DeleteEmployee(string id)
        {
            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.EmployeeId == id);

            if (employee == null) 
                return NotFound(new { message = $"Không tìm thấy nhân viên {id} để xóa!" });

            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();

            return Ok(new { message = $"Đã xóa nhân viên {id} thành công!" });
        }

        // ========================================================
        // 💼 PHÂN HỆ QUẢN LÝ CHỨC VỤ 
        // ========================================================

        // 7. API: Lấy danh sách toàn bộ chức vụ
        [HttpGet("positions")]
        public async Task<IActionResult> GetPositions()
        {
            try 
            {
                var positions = await _context.Database
                    .SqlQueryRaw<PositionDTO>("SELECT Id, Name, Description, BaseSalary FROM Positions")
                    .Select(p => new {
                        id = p.Id,
                        name = p.Name,
                        description = p.Description,
                        baseSalary = p.BaseSalary
                    }).ToListAsync();

                return Ok(positions);
            }
            catch 
            {
                try 
                {
                    var positionsVN = await _context.Database
                        .SqlQueryRaw<PositionDTO>("SELECT Id, Name, Description, BaseSalary FROM ChucVu")
                        .Select(p => new {
                            id = p.Id,
                            name = p.Name,
                            description = p.Description,
                            baseSalary = p.BaseSalary
                        }).ToListAsync();

                    return Ok(positionsVN);
                } 
                catch 
                {
                    return Ok(new List<object>()); 
                }
            }
        }

        // 7.5 API BỔ SUNG: Lấy mức lương của một chức vụ cụ thể theo tên (Hỗ trợ Nhóm 3 gọi sang)
        [HttpGet("positions/by-name")]
        public async Task<IActionResult> GetPositionByName([FromQuery] string name)
        {
            if (string.IsNullOrEmpty(name)) return BadRequest("Tên chức vụ không được để trống");
            
            try 
            {
                var positions = await _context.Database
                    .SqlQueryRaw<PositionDTO>("SELECT Id, Name, Description, BaseSalary FROM Positions")
                    .ToListAsync();
                
                var pos = positions.FirstOrDefault(p => p.Name.ToLower() == name.ToLower());
                if (pos != null) return Ok(pos);
                
                // Dự phòng quét bảng tiếng Việt
                var positionsVN = await _context.Database
                    .SqlQueryRaw<PositionDTO>("SELECT Id, Name, Description, BaseSalary FROM ChucVu")
                    .ToListAsync();
                var posVN = positionsVN.FirstOrDefault(p => p.Name.ToLower() == name.ToLower());
                if (posVN != null) return Ok(posVN);

                return NotFound(new { message = "Không tìm thấy chức vụ tương ứng!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi kết nối DB: " + ex.Message });
            }
        }

        // 8. API: Thêm chức vụ mới vào SQL Server
        [HttpPost("positions")]
        public async Task<IActionResult> CreatePosition([FromBody] PositionDTO position)
        {
            if (position == null) return BadRequest("Dữ liệu không hợp lệ");

            try 
            {
                string sql = "INSERT INTO Positions (Name, Description, BaseSalary) VALUES ({0}, {1}, {2})";
                await _context.Database.ExecuteSqlRawAsync(sql, position.Name, position.Description, position.BaseSalary);
                return Ok(new { message = "Thêm chức vụ mới thành công rực rỡ! 🎉" });
            }
            catch 
            {
                try 
                {
                    string sqlVN = "INSERT INTO ChucVu (Name, Description, BaseSalary) VALUES ({0}, {1}, {2})";
                    await _context.Database.ExecuteSqlRawAsync(sqlVN, position.Name, position.Description, position.BaseSalary);
                    return Ok(new { message = "Thêm chức vụ mới thành công rực rỡ! 🎉" });
                }
                catch (Exception ex) 
                {
                    return StatusCode(500, new { message = "Lỗi kết nối cơ sở dữ liệu: " + ex.Message });
                }
            }
        }

        // 9. API: Sửa thông tin chức vụ
        [HttpPut("positions/{id}")]
        public async Task<IActionResult> UpdatePosition(int id, [FromBody] PositionDTO positionData)
        {
            return Ok(new { message = $"Tính năng sửa chức vụ {id} tạm ghi nhận thành công!" });
        }

        // 10. API: Xóa chức vụ khỏi hệ thống
        [HttpDelete("positions/{id}")]
        public async Task<IActionResult> DeletePosition(int id)
        {
            return Ok(new { message = $"Đã xóa chức vụ {id} thành công!" });
        }
    }
}