using Microsoft.EntityFrameworkCore;
using DinoAPI.Data;
using DinoAPI.Services;

namespace DinoAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers();

            // Регистрируем Entity Framework с SQLite
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite("Data Source=dinosaurus.db"));

            // Регистрируем сервис для работы с изображениями
            builder.Services.AddScoped<IImageService, ImageService>();

            // Настраиваем CORS для доступа из клиентского приложения
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });

            // Swagger для тестирования API
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Автоматически создаем базу данных при запуске
            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                dbContext.Database.EnsureCreated(); // Создает БД, если её нет
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // Добавляем статические файлы для доступа к загруженным изображениям
            app.UseStaticFiles();

            // Используем CORS
            app.UseCors("AllowAll");

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();

            // Создаем директорию для загрузки изображений, если её нет
            var uploadPath = Path.Combine(app.Environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads", "dinosaurs");
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            app.Run();
        }
    }
}