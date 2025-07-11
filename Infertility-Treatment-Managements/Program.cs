using Microsoft.EntityFrameworkCore;
using Infertility_Treatment_Managements.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Infertility_Treatment_Managements.Services;
using Infertility_Treatment_Managements.Helpers;

var builder = WebApplication.CreateBuilder(args);

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

//builder.WebHost.ConfigureKestrel(serverOptions =>
//{
//    serverOptions.ListenAnyIP(5156); // HTTP port
//    serverOptions.ListenAnyIP(7147, listenOptions => // HTTPS port
//    {
//        listenOptions.UseHttps();
//    });
//});Render sẽ tự động xử lý SSL/HTTPS cho bạn:

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Infertility Treatment Management API v1");
        c.RoutePrefix = "swagger";
    });

    // Database setup
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var dbContext = services.GetRequiredService<InfertilityTreatmentManagementContext>();

        //không được chỉnh sửa ******************************
        // Truncate all tables instead of dropping database
        //TruncateAllTables(dbContext);
        //// Recreate database
        //// Không cần EnsureDeleted/EnsureCreated khi dùng migration
        //dbContext.Database.EnsureDeleted();
        //dbContext.Database.EnsureCreated();

        // Seed data
        //SeedData.Initialize(services);
        //***************************************************
    }
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


app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();