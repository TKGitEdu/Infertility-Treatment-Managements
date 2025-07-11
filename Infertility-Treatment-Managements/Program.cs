using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

using Infertility_Treatment_Managements.Models;
using Infertility_Treatment_Managements.Services;
using Infertility_Treatment_Managements.Helpers;

var builder = WebApplication.CreateBuilder(args);

// Cấu hình để lắng nghe trên cổng từ biến môi trường PORT (Render sử dụng)
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(port) && int.TryParse(port, out var parsedPort))
{
    builder.WebHost.UseUrls($"http://*:{parsedPort}");
}

// Cấu hình ForwardedHeaders để xử lý đúng proxy headers từ Render
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// Đăng ký các dịch vụ
builder.Services.AddControllers();
builder.Services.AddDbContext<InfertilityTreatmentManagementContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddEndpointsApiExplorer();

// Cấu hình Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Infertility Treatment Management API",
        Version = "v1",
        Description = "API for Infertility Treatment Management System"
    });

    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme."
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Cấu hình CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policyBuilder => policyBuilder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader()
            .WithExposedHeaders("Authorization"));
});

// Cấu hình JWT
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"] ?? "defaultsecretkey12345678901234567890");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(secretKey)
    };
});

// Xây dựng ứng dụng
var app = builder.Build();

// Đảm bảo các proxy header được xử lý đúng
app.UseForwardedHeaders();

// Luôn hiển thị Swagger UI bất kể môi trường
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Infertility Treatment Management API v1");
    c.RoutePrefix = "swagger";
});

// Xử lý ngoại lệ và bảo mật dựa trên môi trường
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseHttpsRedirection();

    // Database setup chỉ trong môi trường phát triển
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var dbContext = services.GetRequiredService<InfertilityTreatmentManagementContext>();
        // Seed data hoặc các tác vụ database khác nếu cần
    }
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// Đăng ký và cấu hình các middleware
app.UseDefaultFiles(); // Cho phép file mặc định như index.html
app.UseStaticFiles(); // Cho phép phục vụ file tĩnh từ thư mục wwwroot
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Thêm fallback route để đảm bảo route không tìm thấy được xử lý đúng
app.MapFallbackToController("Get", "Home");

app.Run();

// Hàm hỗ trợ - giữ lại nếu cần
static void TruncateAllTables(InfertilityTreatmentManagementContext context)
{
    try
    {
        // Cách an toàn: Xóa dữ liệu theo thứ tự phù hợp với ràng buộc khóa ngoại
        Console.WriteLine("Bắt đầu xóa dữ liệu từ tất cả các bảng...");

        // 1. Xóa dữ liệu từ các bảng con trước
        context.Database.ExecuteSqlRaw("DELETE FROM \"Examinations\";");
        context.Database.ExecuteSqlRaw("DELETE FROM \"Payments\";");
        context.Database.ExecuteSqlRaw("DELETE FROM \"Ratings\";");
        context.Database.ExecuteSqlRaw("DELETE FROM \"Feedbacks\";");
        context.Database.ExecuteSqlRaw("DELETE FROM \"Notifications\";");
        context.Database.ExecuteSqlRaw("DELETE FROM \"TreatmentMedications\";");
        context.Database.ExecuteSqlRaw("DELETE FROM \"TreatmentSteps\";");

        // 2. Xóa các bảng trung gian
        context.Database.ExecuteSqlRaw("DELETE FROM \"TreatmentProcesses\";");
        context.Database.ExecuteSqlRaw("DELETE FROM \"Bookings\";");

        // 3. Xóa các bảng cha
        context.Database.ExecuteSqlRaw("DELETE FROM \"TreatmentPlans\";");
        context.Database.ExecuteSqlRaw("DELETE FROM \"PatientDetails\";");
        context.Database.ExecuteSqlRaw("DELETE FROM \"Patients\";");
        context.Database.ExecuteSqlRaw("DELETE FROM \"Doctors\";");
        context.Database.ExecuteSqlRaw("DELETE FROM \"Users\";");
        context.Database.ExecuteSqlRaw("DELETE FROM \"Services\";");
        context.Database.ExecuteSqlRaw("DELETE FROM \"Slots\";");
        context.Database.ExecuteSqlRaw("DELETE FROM \"BlogPosts\";");
        context.Database.ExecuteSqlRaw("DELETE FROM \"ContentPages\";");

        // 4. Giữ lại admin
        context.Database.ExecuteSqlRaw("DELETE FROM \"Users\" WHERE \"Username\" != 'admin';");

        Console.WriteLine("Đã xóa dữ liệu thành công");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Lỗi khi xóa dữ liệu: {ex.Message}");
        if (ex.InnerException != null)
        {
            Console.WriteLine($"Chi tiết: {ex.InnerException.Message}");
        }
    }
}