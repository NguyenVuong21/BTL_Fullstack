using Microsoft.EntityFrameworkCore;
using Nexus.HRCore.Data;

var builder = WebApplication.CreateBuilder(args);

// Cấu hình chuỗi kết nối SQL Server (Tự động tạo database mang tên HRCoreDB)
builder.Services.AddDbContext<HRDbContext>(options =>
    options.UseSqlServer("Server=localhost;Database=HRCoreDB;Trusted_Connection=True;TrustServerCertificate=True;"));

// ========================================================
// 1. THÊM ĐOẠN NÀY: Khai báo chính sách CORS mở cho Frontend gọi vào
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()   // Cho phép cả GET, POST, PUT, DELETE
              .AllowAnyHeader();  // Chấp nhận mọi Header dữ liệu
    });
});
// ========================================================

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ========================================================
// 2. THÊM DÒNG NÀY: Kích hoạt CORS (Bắt buộc nằm TRƯỚC UseAuthorization)
app.UseCors("CorsPolicy");
// ========================================================

app.UseAuthorization();
app.MapControllers();

app.Run();