using Microsoft.EntityFrameworkCore;
using DinoAPI.Data;

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

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}