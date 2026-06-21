using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

// ĐẢM BẢO CÓ DÒNG NÀY: Ép Gateway đọc file ocelot.json từ thư mục gốc
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

// Cấu hình CORS để Vue 3 không bị chặn gọi API
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()   // Cho phép GET, POST, PUT, DELETE, OPTIONS
              .AllowAnyHeader();  // Cho phép mọi loại Header từ Frontend gửi lên
    });
});
// Nạp dịch vụ Ocelot vào hệ thống
builder.Services.AddOcelot();

var app = builder.Build();
app.UseRouting();
app.UseCors("CorsPolicy");
app.UseAuthorization();

// Kích hoạt Middleware điều phối của Ocelot
await app.UseOcelot();

app.Run();