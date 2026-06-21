using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nexus.HRCore.Data;
using Nexus.HRCore.Models;
using System.Threading.Tasks;

namespace Nexus.HRCore.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly HRDbContext _context;

        public AuthController(HRDbContext context)
        {
            _context = context;
        }

        // 🚀 API 1: ĐĂNG KÝ TÀI KHOẢN (Để liên kết với nhân viên có sẵn trong DB)
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            // 1. Kiểm tra xem EmployeeId có tồn tại trong bảng Employees không
            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.EmployeeId == request.EmployeeId);
            if (employee == null)
            {
                return BadRequest(new { message = "Mã nhân viên không tồn tại trong hệ thống!" });
            }

            // 2. Kiểm tra tài khoản đã bị trùng chưa
            var userExists = await _context.Users.AnyAsync(u => u.Username == request.Username);
            if (userExists)
            {
                return BadRequest(new { message = "Tên tài khoản này đã được sử dụng!" });
            }

            // 3. Tạo tài khoản mới (Mẹo: Ở đây lưu tạm chuỗi thô để Vượng dễ test bằng Postman, 
            // nếu muốn bảo mật có thể cài BCrypt để Hash sau nha)
            var newUser = new User
            {
                Username = request.Username.Trim(),
                PasswordHash = request.Password, // Lưu pass thô để test nhanh
                Role = string.IsNullOrEmpty(request.Role) ? "Employee" : request.Role,
                EmployeeId = request.EmployeeId
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đăng ký tài khoản thành công!" });
        }

        // 🔑 API 2: ĐĂNG NHẬP THẬT
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            // Tìm user dựa trên Username và nạp luôn thông tin Employee đi kèm (để lấy FullName)
            var user = await _context.Users
                .Include(u => u.Employee)
                .FirstOrDefaultAsync(u => u.Username == request.Username.Trim());

            // Kiểm tra user có tồn tại và đúng mật khẩu không
            if (user == null || user.PasswordHash != request.Password)
            {
                return BadRequest(new { message = "Tài khoản hoặc mật khẩu không chính xác!" });
            }

            // Trả về đúng định dạng Json mà Frontend Vue 3 đang đợi hứng
            return Ok(new
            {
                username = user.Username,
                role = user.Role,
                employeeId = user.EmployeeId,
                fullName = user.Employee != null ? user.Employee.FullName : "Nhân viên chưa có tên"
            });
        }
    }

    // Các DTO Đón Dữ Liệu từ Body
    public class RegisterRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string EmployeeId { get; set; }
        public string? Role { get; set; }
    }

    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}