
using e360_clone.DataAccess;
using e360_clone.Repositories;
using e360_clone_api.Services;
using e360_clone.DataAccess.DAOs;
using e360_clone.Controllers;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace e360_clone
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers();
            builder.Services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var errors = context.ModelState
                        .Where(kvp => kvp.Value?.Errors.Count > 0)
                        .Select(kvp => new
                        {
                            Field = kvp.Key,
                            Messages = kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                        })
                        .ToList();

                    var message = errors.Count == 0
                        ? "Dữ liệu không hợp lệ."
                        : $"Dữ liệu không hợp lệ: {errors[0].Field} - {errors[0].Messages.FirstOrDefault()}";

                    return new BadRequestObjectResult(new ApiResponse<object>
                    {
                        Success = false,
                        Message = message,
                        Data = errors
                    });
                };
            });
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Logging.AddSimpleConsole(options =>
            {
                options.TimestampFormat = "HH:mm:ss ";
                options.IncludeScopes = true;
            });

            // Add DbContext with PostgreSQL
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(
                    builder.Configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly("e360_clone")
                ));

            // Register repositories
            builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<AccountDAO>();
            builder.Services.AddScoped<IAccountRepository, AccountRepository>();
            builder.Services.AddScoped<StudentDAO>();
            builder.Services.AddScoped<IStudentRepository, StudentRepository>();
            builder.Services.AddScoped<StudentSubjectDAO>();
            builder.Services.AddScoped<IStudentSubjectRepository, StudentSubjectRepository>();
            builder.Services.AddScoped<SubjectDAO>();
            builder.Services.AddScoped<ISubjectRepository, SubjectRepository>();
            builder.Services.AddScoped<ClassDAO>();
            builder.Services.AddScoped<IClassRepository, ClassRepository>();
            builder.Services.AddScoped<MajorDAO>();
            builder.Services.AddScoped<IMajorRepository, MajorRepository>();
            builder.Services.AddScoped<TeachingAssignmentDAO>();
            builder.Services.AddScoped<ITeachingAssignmentRepository, TeachingAssignmentRepository>();
            builder.Services.AddScoped<ExamDAO>();
            builder.Services.AddScoped<IExamRepository, ExamRepository>();
            builder.Services.AddScoped<ExamRoomDAO>();
            builder.Services.AddScoped<IExamRoomRepository, ExamRoomRepository>();
            builder.Services.AddScoped<ExamRoomAllocationDAO>();
            builder.Services.AddScoped<IExamRoomAllocationRepository, ExamRoomAllocationRepository>();
            builder.Services.AddScoped<LecturerDAO>();
            builder.Services.AddScoped<ILecturerRepository, LecturerRepository>();
            builder.Services.AddScoped<AttendanceDAO>();
            builder.Services.AddScoped<IAttendanceRepository, AttendanceRepository>();
            builder.Services.AddScoped<GradeDAO>();
            builder.Services.AddScoped<IGradeRepository, GradeRepository>();
            builder.Services.AddScoped<ProctorAssignmentDAO>();
            builder.Services.AddScoped<IProctorAssignmentRepository, ProctorAssignmentRepository>();
            builder.Services.AddScoped<IDashboardRepository, DashboardRepository>();
            builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
            builder.Services.AddScoped<IEmailService, SmtpEmailService>();
            builder.Services.Configure<CronJobSettings>(builder.Configuration.GetSection("CronJobSettings"));
            builder.Services.AddHostedService<ExamCronJobService>();
            builder.Services.Configure<ExamNotificationSettings>(builder.Configuration.GetSection("ExamNotificationSettings"));
            builder.Services.AddSingleton<IExamNotificationQueue, ExamNotificationQueue>();
            builder.Services.AddHostedService<ExamNotificationWorker>();

            // Configure JWT Authentication
            var jwtSettings = builder.Configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"] ?? "YourSuperSecretKeyThatIsAtLeast32CharactersLong!";

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
                    ValidIssuer = jwtSettings["Issuer"] ?? "e360-clone",
                    ValidAudience = jwtSettings["Audience"] ?? "e360-clone-users",
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                    ClockSkew = TimeSpan.Zero
                };
            });

            builder.Services.AddAuthorization();

            // Configure CORS for frontend API calls
            var allowedOrigins = builder.Configuration.GetSection("CorsSettings:AllowedOrigins").Get<string[]>()
                ?? new[] { "http://localhost:5000", "https://localhost:5001" };
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", policy =>
                {
                    policy.WithOrigins(allowedOrigins)
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });

            var app = builder.Build();

            // Seed database
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var context = services.GetRequiredService<AppDbContext>();
                    await e360_clone.Seeders.AccountSeeder.SeedAsync(context);
                    Console.WriteLine("âœ… Database seeded successfully!");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"âŒ Error seeding database: {ex.Message}");
                }
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseCors("AllowFrontend");
            var enableHttpsRedirection = builder.Configuration.GetValue<bool>("EnableHttpsRedirection");
            if (enableHttpsRedirection)
            {
                app.UseHttpsRedirection();
            }
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}

