using Microsoft.EntityFrameworkCore;
using Infertility_Treatment_Managements.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Infertility_Treatment_Managements.Services;
using Infertility_Treatment_Managements.Helpers;

var builder = WebApplication.CreateBuilder(args);

// Thêm đoạn này để đảm bảo ứng dụng lắng nghe trên cổng đúng
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(port))
{
    builder.WebHost.UseUrls($"http://*:{port}");
}

// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddDbContext<InfertilityTreatmentManagementContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register EmailService
builder.Services.AddScoped<IEmailService, EmailService>();

builder.Services.AddEndpointsApiExplorer();

// Configure Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Infertility Treatment Management API",
        Version = "v1",
        Description = "API for Infertility Treatment Management System"
    });

    // Add JWT authentication to Swagger
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
            new string[] {}
        }
    });
});

// Add CORS configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder => builder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader()
            .WithExposedHeaders("Authorization"));
});

// Add JWT configuration
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

// Cấu hình Kestrel để lắng nghe trên cổng do Render quy định
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    // Sửa đoạn code cấu hình URL để đảm bảo hỗ trợ cả HTTPS
    var port = Environment.GetEnvironmentVariable("PORT");
    if (!string.IsNullOrEmpty(port))
    {
        // Chỉ định rõ rằng ứng dụng chấp nhận cả HTTP và HTTPS
        builder.WebHost.UseUrls($"http://*:{port}", $"https://*:{port}");
    }

    // Thêm đoạn này để cấu hình ForwardedHeaders
    builder.Services.Configure<ForwardedHeadersOptions>(options =>
    {
        options.ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor | Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto;
        options.KnownNetworks.Clear();
        options.KnownProxies.Clear();
    });
});

var app = builder.Build();
// Thêm dòng này vào đầu middleware pipeline, ngay sau var app = builder.Build();
app.UseForwardedHeaders();
// Xóa điều kiện app.Environment.IsDevelopment() để Swagger hoạt động trong mọi môi trường
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Infertility Treatment Management API v1");
    c.RoutePrefix = "swagger";
});

// Chỉ sử dụng HTTPS Redirection trong môi trường phát triển
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
// Bật HSTS trong môi trường sản xuất
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}
// Thêm middleware xử lý ngoại lệ
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
// Thay thế hàm TruncateAllTables hiện tại bằng hàm sau
static void TruncateAllTables(InfertilityTreatmentManagementContext context)
{
    try
    {
        // Cách an toàn: Xóa dữ liệu theo thứ tự phù hợp với ràng buộc khóa ngoại
        Console.WriteLine("Bắt đầu xóa dữ liệu từ tất cả các bảng...");

        // 1. Xóa dữ liệu từ các bảng con trước (không có bảng khác phụ thuộc)
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

        // 3. Xóa các bảng cha trước khi xóa các liên kết
        context.Database.ExecuteSqlRaw("DELETE FROM \"TreatmentPlans\";");
        context.Database.ExecuteSqlRaw("DELETE FROM \"PatientDetails\";");
        context.Database.ExecuteSqlRaw("DELETE FROM \"Patients\";");
        context.Database.ExecuteSqlRaw("DELETE FROM \"Doctors\";");
        context.Database.ExecuteSqlRaw("DELETE FROM \"Users\";");
        context.Database.ExecuteSqlRaw("DELETE FROM \"Services\";");
        context.Database.ExecuteSqlRaw("DELETE FROM \"Slots\";");
        context.Database.ExecuteSqlRaw("DELETE FROM \"BlogPosts\";");
        context.Database.ExecuteSqlRaw("DELETE FROM \"ContentPages\";");

        // 4. Cuối cùng xóa các bảng cơ bản
        context.Database.ExecuteSqlRaw("DELETE FROM \"Users\" WHERE \"Username\" != 'admin';"); // Giữ lại admin
                                                                                                // context.Database.ExecuteSqlRaw("DELETE FROM \"Roles\";"); // Không xóa Roles

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

app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();