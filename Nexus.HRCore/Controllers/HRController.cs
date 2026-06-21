using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nexus.HRCore.Data;
using Nexus.HRCore.Models;
using System;
using System.Collections.Generic; // 🚀 Hỗ trợ xử lý cấu trúc dữ liệu Dictionary
using System.Linq;
using System.Threading.Tasks;

namespace Nexus.HRCore.Controllers
{
    [Route("api/hr")] // Ép cứng route viết thường để khớp 100% với ocelot.json và Vue 3
    [ApiController]
    public class HRController : ControllerBase
    {
        private readonly HRDbContext _context;

        // 🚀 BỘ NHỚ ĐỆM TẠM THỜI QUA RAM ĐỂ GHI NHỚ MỨC LƯƠNG KHI SỬA
        private static readonly Dictionary<string, long> PositionSalaryCache = new Dictionary<string, long>();

        public HRController(HRDbContext context)
        {
            _context = context;
        }

        // ========================================================
        // 🚀 API 0. Lấy số liệu thống kê tổng quan cho Dashboard
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
        // 👥 PHÂN HỆ QUẢN LÝ HỒ SƠ NHÂN VIÊN & PHÒNG BAN
        // ========================================================

        // 1. API: Lấy danh sách nhân viên (Bổ sung departmentId chạy ngầm)
        [HttpGet("employees")]
        public async Task<IActionResult> GetEmployees()
        {
            var employees = await _context.Employees
                .Include(e => e.Department)
                .Select(e => new {
                    id = e.EmployeeId,
                    name = e.FullName,
                    email = e.Email,
                    departmentId = e.DepartmentId, // <-- CHÈN THÊM TRƯỜNG NÀY ĐỂ VUE 3 ĐÓN ĐƯỢC ID GỐC
                    dept = e.Department != null ? e.Department.Name : "N/A",
                    position = e.Position,
                    contractType = e.ContractType,
                    joinDate = e.JoinDate.ToString("yyyy-MM-dd"),
                    status = e.Status
                }).ToListAsync();

            return Ok(employees);
        }

        // 2. API: Lấy cấu trúc cây phòng ban (Chỉ lấy gốc ParentId == null)
        [HttpGet("departments/tree")]
        public async Task<IActionResult> GetDepartmentTree()
        {
            var rootDepartments = await _context.Departments
                .Include(d => d.Children)
                .Where(d => d.ParentId == null)
                .ToListAsync();

            return Ok(rootDepartments);
        }

        // 3. API: Xem CHI TIẾT 1 nhân viên (Phục vụ nút icon con mắt 👁️ trên Vue)
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
                departmentId = employee.DepartmentId, // <-- ĐỒNG BỘ THÊM Ở ĐÂY
                dept = employee.Department != null ? employee.Department.Name : "N/A",
                position = employee.Position,
                contractType = employee.ContractType,
                joinDate = employee.JoinDate.ToString("yyyy-MM-dd"),
                status = employee.Status
            });
        }

        // 4. API: Thêm nhân viên mới
        [HttpPost("employees")]
        public async Task<IActionResult> CreateEmployee([FromBody] Employee employee)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEmployees), new { id = employee.EmployeeId }, employee);
        }

        // 5. API: SỬA hồ sơ nhân viên (Bổ sung lưu cập nhật phòng ban)
        [HttpPut("employees/{id}")]
        public async Task<IActionResult> UpdateEmployee(string id, [FromBody] Employee employeeData)
        {
            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.EmployeeId == id);

            if (employee == null) 
                return NotFound(new { message = $"Không tìm thấy nhân viên {id} để cập nhật!" });

            // Gán các giá trị mới từ màn hình sửa đổ xuống bao gồm cả mã phòng ban số
            employee.FullName = employeeData.FullName;
            employee.Email = employeeData.Email;
            employee.Position = employeeData.Position;
            employee.ContractType = employeeData.ContractType;
            employee.Status = employeeData.Status;
            employee.DepartmentId = employeeData.DepartmentId; // <-- CẬP NHẬT MÃ PHÒNG BAN MỚI XUỐNG DB

            await _context.SaveChangesAsync();
            return Ok(new { message = $"Cập nhật hồ sơ nhân viên {id} thành công!" });
        }

        // 6. API: XÓA nhân viên thật (Đọc theo mã chuỗi EmployeeId)
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
        // 🕒 PHÂN HỆ CHẤM CÔNG (ATTENDANCE SERVICE)
        // ========================================================

        // 7. API: Thực hiện Check-In / Check-Out động theo ngày
        [HttpPost("attendance/check")]
        public async Task<IActionResult> CheckAttendance([FromBody] Attendance checkData)
        {
            if (string.IsNullOrEmpty(checkData.EmployeeId))
                return BadRequest(new { message = "Thiếu Mã nhân viên rồi Vượng ơi!" });

            var today = DateTime.Today;
            var currentTime = DateTime.Now.TimeOfDay;

            // Kiểm tra xem hôm nay nhân viên này đã có bản ghi chấm công nào chưa
            var attendance = await _context.Attendances
                .FirstOrDefaultAsync(a => a.EmployeeId == checkData.EmployeeId && a.LogDate == today);

            if (attendance == null)
            {
                // --- LUỒNG CHECK-IN (Lần đầu tiên bấm trong ngày) ---
                var standardTime = new TimeSpan(8, 0, 0); // Mốc 8h00 sáng làm chuẩn đúng giờ
                string status = currentTime > standardTime ? "Đi muộn" : "Đúng giờ";

                attendance = new Attendance
                {
                    EmployeeId = checkData.EmployeeId,
                    LogDate = today,
                    CheckIn = currentTime,
                    CheckOut = null,
                    Status = status
                };

                _context.Attendances.Add(attendance);
                await _context.SaveChangesAsync();

                return Ok(new { 
                    type = "CHECK_IN", 
                    message = $"Chào buổi sáng! Check-in thành công lúc {currentTime.ToString(@"hh\:mm\:ss")}. Trạng thái: {status}" 
                });
            }
            else
            {
                // --- LUỒNG CHECK-OUT (Từ lần thứ 2 bấm trở đi trong ngày) ---
                attendance.CheckOut = currentTime;
                
                // Nếu đi muộn thì giữ nguyên trạng thái đi muộn để tính phạt, ngược lại gán kiểm tra về sớm
                if (attendance.Status != "Đi muộn")
                {
                    var endStandardTime = new TimeSpan(17, 0, 0); // 17h00 chiều hết giờ hành chính
                    if (currentTime < endStandardTime) attendance.Status = "Về sớm";
                }

                await _context.SaveChangesAsync();

                return Ok(new { 
                    type = "CHECK_OUT", 
                    message = $"Tạm biệt! Check-out thành công lúc {currentTime.ToString(@"hh\:mm\:ss")}. Trạng thái: {attendance.Status}" 
                });
            }
        }

        // 8. API: Lấy lịch sử chấm công của một nhân viên cụ thể
        [HttpGet("attendance/history/{employeeId}")]
        public async Task<IActionResult> GetAttendanceHistory(string employeeId)
        {
            var history = await _context.Attendances
                .Where(a => a.EmployeeId == employeeId)
                .OrderByDescending(a => a.LogDate)
                .Select(a => new {
                    id = a.Id,
                    date = a.LogDate.ToString("yyyy-MM-dd"),
                    checkIn = a.CheckIn.HasValue ? a.CheckIn.Value.ToString(@"hh\:mm\:ss") : "--:--:--",
                    checkOut = a.CheckOut.HasValue ? a.CheckOut.Value.ToString(@"hh\:mm\:ss") : "--:--:--",
                    status = a.Status
                })
                .ToListAsync();

            return Ok(history);
        }

        // 9. API ADMMIN: Lấy toàn bộ nhật ký chấm công của TẤT CẢ nhân viên trong công ty
        [HttpGet("attendance/all")]
        public async Task<IActionResult> GetAllAttendanceLogs()
        {
            var logs = await _context.Attendances
                .OrderByDescending(a => a.LogDate)
                .Select(a => new {
                    id = a.Id,
                    employeeId = a.EmployeeId, 
                    date = a.LogDate.ToString("yyyy-MM-dd"),
                    checkIn = a.CheckIn.HasValue ? a.CheckIn.Value.ToString(@"hh\:mm\:ss") : "--:--:--",
                    checkOut = a.CheckOut.HasValue ? a.CheckOut.Value.ToString(@"hh\:mm\:ss") : "--:--:--",
                    status = a.Status
                })
                .ToListAsync();

            return Ok(logs);
        }

        // 10. API ADMIN: Thống kê trạng thái đi làm ngày hôm nay (Bản xử lý RAM chống sập trùng Key 500)
        [HttpGet("attendance/today-status")]
        public async Task<IActionResult> GetTodayAttendanceStatus()
        {
            try
            {
                var today = DateTime.Today;

                // 1. Quét danh sách nhân viên đang hoạt động từ SQL Server
                var allEmployees = await _context.Employees
                    .Where(e => e.Status == "Hoạt động")
                    .ToListAsync();

                // 2. Kéo dữ liệu chấm công ngày hôm nay về RAM dưới dạng List thô
                var rawAttendances = await _context.Attendances
                    .Where(a => a.LogDate == today)
                    .ToListAsync();

                // 3. Nhóm dữ liệu (GroupBy) trên RAM để triệt tiêu lỗi trùng lặp khoá khi một người check nhiều lần
                var todayAttendancesDict = rawAttendances
                    .GroupBy(a => a.EmployeeId)
                    .ToDictionary(
                        g => g.Key,
                        g => g.OrderByDescending(a => a.Id).FirstOrDefault()
                    );

                // 4. Tiến hành Left Join kết xuất dữ liệu an toàn ra Frontend
                var result = allEmployees.Select(e => new
                {
                    employeeId = e.EmployeeId,
                    fullName = e.FullName,
                    position = e.Position,
                    hasCheckedIn = todayAttendancesDict.ContainsKey(e.EmployeeId),
                    checkInTime = todayAttendancesDict.ContainsKey(e.EmployeeId) && todayAttendancesDict[e.EmployeeId]?.CheckIn.HasValue == true
                        ? todayAttendancesDict[e.EmployeeId].CheckIn.Value.ToString(@"hh\:mm\:ss") 
                        : "--:--:--",
                    status = todayAttendancesDict.ContainsKey(e.EmployeeId) 
                        ? (todayAttendancesDict[e.EmployeeId].Status ?? "Đúng giờ") 
                        : "Chưa chấm công"
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"--- LỖI PHÂN HỆ ATTENDANCE: {ex.Message} ---");
                return StatusCode(500, new { message = "Lỗi hệ thống", error = ex.Message });
            }
        }

        // ========================================================
        // 📑 PHÂN HỆ QUẢN LÝ NGHỈ PHÉP (LEAVE SERVICE) - CHẠY THẬT
        // ========================================================

        // 11. API: Nhân viên gửi đơn xin nghỉ phép mới (POST)
        [HttpPost("leave/submit")]
        public async Task<IActionResult> SubmitLeave([FromBody] LeaveRequest request)
        {
            if (string.IsNullOrEmpty(request.EmployeeId))
                return BadRequest(new { message = "Thiếu mã nhân viên rồi Vượng ơi!" });

            request.Status = "Chờ duyệt"; // Ép cứng trạng thái ban đầu
            _context.LeaveRequests.Add(request);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Gửi đơn xin nghỉ phép lên hệ thống thành công!" });
        }

        // 12. API: Lấy lịch sử xin nghỉ của riêng 1 nhân viên (GET)
        [HttpGet("leave/history/{employeeId}")]
        public async Task<IActionResult> GetLeaveHistory(string employeeId)
        {
            var history = await _context.LeaveRequests
                .Where(l => l.EmployeeId == employeeId)
                .OrderByDescending(l => l.Id)
                .Select(l => new {
                    leaveType = l.LeaveType,
                    duration = l.StartDate.ToString("yyyy-MM-dd") + " đến " + l.EndDate.ToString("yyyy-MM-dd"),
                    status = l.Status
                })
                .ToListAsync();

            return Ok(history);
        }

        // 13. API MANAGER: Lấy toàn bộ danh sách đơn của tất cả nhân viên để duyệt (GET)
        [HttpGet("leave/admin-list")]
        public async Task<IActionResult> GetAdminLeaveList()
        {
            var rawList = await _context.LeaveRequests.OrderByDescending(l => l.Id).ToListAsync();
            var employees = await _context.Employees.ToDictionaryAsync(e => e.EmployeeId, e => e.FullName);

            var result = rawList.Select(l => new {
                id = l.Id,
                employeeId = l.EmployeeId,
                fullName = employees.ContainsKey(l.EmployeeId) ? employees[l.EmployeeId] : "Nhân sự ẩn danh",
                leaveType = l.LeaveType,
                duration = l.StartDate.ToString("yyyy-MM-dd") + " đến " + l.EndDate.ToString("yyyy-MM-dd"),
                reason = l.Reason,
                status = l.Status
            }).ToList();

            return Ok(result);
        }

        // 14. API MANAGER: Phê duyệt hoặc Từ chối đơn xin nghỉ (PUT)
        [HttpPut("leave/action/{id}")]
        public async Task<IActionResult> UpdateLeaveStatus(int id, [FromBody] Dictionary<string, string> body)
        {
            var leave = await _context.LeaveRequests.FindAsync(id);
            if (leave == null) return NotFound(new { message = "Không tìm thấy đơn xin nghỉ này!" });

            if (body != null && body.ContainsKey("status"))
            {
                leave.Status = body["status"]; // Sẽ nhận vào chuỗi "Đã duyệt" hoặc "Từ chối"
                await _context.SaveChangesAsync();
                return Ok(new { message = $"Đã cập nhật trạng thái đơn thành: {leave.Status}" });
            }

            return BadRequest(new { message = "Dữ liệu trạng thái không hợp lệ!" });
        }

        // ========================================================
        // 💵 PHÂN HỆ QUẢN LÝ LƯƠNG (PAYROLL SERVICE) - CHẠY THẬT
        // ========================================================

        // 15. API: Lấy chi tiết phiếu lương động dựa trên cấu trúc chấm công thực tế
        [HttpGet("payroll/detail/{employeeId}/{monthYear}")]
        public async Task<IActionResult> GetPayrollDetail(string employeeId, string monthYear)
        {
            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.EmployeeId == employeeId);
            if (employee == null) return NotFound(new { message = "Không tìm thấy nhân viên!" });

            if (!DateTime.TryParse($"{monthYear}-01", out DateTime targetMonth))
            {
                targetMonth = DateTime.Today;
            }

            // ĐẾM SỐ LẦN ĐI MUỘN THỰC TẾ dưới database của nhân viên trong tháng đó
            var lateCount = await _context.Attendances
                .Where(a => a.EmployeeId == employeeId 
                       && a.LogDate.Year == targetMonth.Year 
                       && a.LogDate.Month == targetMonth.Month
                       && a.Status == "Đi muộn")
                .CountAsync();

            // ĐỊNH NGHĨA HẠN MỨC CẤU TRÚC LƯƠNG
            double baseSalary = employee.Position.Contains("Trưởng phòng") ? 20000000 : 12500000; 
            double allowance = 1200000; 
            double insuranceRate = 0.105; 
            double finePerLate = 100000; // Phạt 100k cho mỗi lần đi muộn

            // TIẾN HÀNH TÍNH TOÁN THEO CÔNG THỨC
            double insuranceAmount = baseSalary * insuranceRate;
            double totalFine = lateCount * finePerLate;
            double netSalary = baseSalary + allowance - insuranceAmount - totalFine;

            return Ok(new
            {
                employeeId = employee.EmployeeId,
                fullName = employee.FullName,
                monthYear = monthYear,
                baseSalary = baseSalary,
                allowance = allowance,
                insurance = insuranceAmount,
                lateCount = lateCount,
                fineAmount = totalFine,
                netSalary = netSalary > 0 ? netSalary : 0
            });
        }

        // 16. GET: api/hr/positions -> ĐÃ CẬP NHẬT ĐỌC LƯƠNG TỪ RAM CACHE
        [HttpGet("positions")]
        public async Task<IActionResult> GetPositions()
        {
            var activePositions = await _context.Employees
                .Where(e => !string.IsNullOrEmpty(e.Position))
                .Select(e => e.Position)
                .Distinct()
                .ToListAsync();

            var result = activePositions.Select(pos => {
                // Nếu chức vụ này đã có trong bộ nhớ RAM tạm thời thì lấy ra, không thì dùng mặc định
                long salary = PositionSalaryCache.ContainsKey(pos) 
                    ? PositionSalaryCache[pos] 
                    : (pos.Contains("Trưởng phòng") ? 20000000 : 12500000);

                return new
                {
                    positionId = pos, 
                    positionName = pos,
                    baseSalary = salary
                };
            }).ToList();

            if (result.Count == 0)
            {
                long mgrSalary = PositionSalaryCache.ContainsKey("Manager") ? PositionSalaryCache["Manager"] : 20000000;
                long empSalary = PositionSalaryCache.ContainsKey("Employee") ? PositionSalaryCache["Employee"] : 12500000;

                result.Add(new { positionId = "Manager", positionName = "Trưởng phòng", baseSalary = mgrSalary });
                result.Add(new { positionId = "Employee", positionName = "Nhân viên", baseSalary = empSalary });
            }

            return Ok(result);
        }

        // 17. POST: api/hr/positions -> Thêm mới chức vụ (Thử nghiệm)
        [HttpPost("positions")]
        public IActionResult CreatePosition([FromBody] Dictionary<string, object> body)
        {
            return Ok(new { message = "Thêm chức vụ mới lên hệ thống thành công rực rỡ! 🎉" });
        }

        // 18. PUT: api/hr/positions/{id} -> ĐÃ CẬP NHẬT GHI LƯƠNG MỚI VÀO RAM CACHE
        [HttpPut("positions/{id}")]
        public async Task<IActionResult> UpdatePosition(string id, [FromBody] Dictionary<string, object> body)
        {
            if (body == null || !body.ContainsKey("positionName"))
            {
                return BadRequest(new { message = "Dữ liệu không hợp lệ!" });
            }

            string newName = body["positionName"].ToString();
            
            // 🚀 ĐOẠN ĐỔI MỚI: Đọc mức lương mới từ Vue và nạp thẳng vào RAM bộ nhớ đệm static
            if (body.ContainsKey("baseSalary"))
            {
                if (long.TryParse(body["baseSalary"].ToString(), out long newSalary))
                {
                    PositionSalaryCache[id] = newSalary;
                    PositionSalaryCache[newName] = newSalary;
                }
            }

            // Đồng bộ đổi tên chức danh cho nhân viên dưới SQL
            var employeesToUpdate = await _context.Employees.Where(e => e.Position == id).ToListAsync();
            foreach (var emp in employeesToUpdate)
            {
                emp.Position = newName;
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = $"Cập nhật chức vụ và định mức lương thành công! 🎉" });
        }

        // 19. DELETE: api/hr/positions/{id} -> ĐÃ CẬP NHẬT LOGIC XÓA THẬT
        [HttpDelete("positions/{id}")]
        public async Task<IActionResult> DeletePosition(string id)
        {
            var employeesToUpdate = await _context.Employees.Where(e => e.Position == id).ToListAsync();
            foreach (var emp in employeesToUpdate)
            {
                emp.Position = ""; 
            }

            // Xóa luôn cấu hình lương trong RAM cache nếu có
            if (PositionSalaryCache.ContainsKey(id))
            {
                PositionSalaryCache.Remove(id);
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = $"Đã xóa chức vụ {id} khỏi danh mục nhân sự!" });
        }
    }
}