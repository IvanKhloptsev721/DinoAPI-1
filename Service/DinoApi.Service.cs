using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DinoAPI.Services
{
    public interface IImageService
    {
        Task<string?> SaveImageAsync(IFormFile imageFile, string dinosaurName);
        Task DeleteImageAsync(string imagePath);
        string GetImageUrl(string imagePath);
    }

    public class ImageService : IImageService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly string _imagesFolder = "uploads/dinosaurs";
        private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        private const int MaxFileSize = 5 * 1024 * 1024; 

        public ImageService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        private string GetUploadPath()
        {
            var webRootPath = _environment.WebRootPath;
            if (string.IsNullOrEmpty(webRootPath))
            {
                webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                if (!Directory.Exists(webRootPath))
                {
                    Directory.CreateDirectory(webRootPath);
                }
            }

            var uploadPath = Path.Combine(webRootPath, _imagesFolder);
            return uploadPath;
        }

        public async Task<string?> SaveImageAsync(IFormFile imageFile, string dinosaurName)
        {
            if (imageFile == null || imageFile.Length == 0)
                return null;

            if (imageFile.Length > MaxFileSize)
                throw new InvalidOperationException($"Файл слишком большой. Максимальный размер: {MaxFileSize / 1024 / 1024} MB");

            var extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
            if (!_allowedExtensions.Contains(extension))
                throw new InvalidOperationException($"Недопустимый формат файла. Разрешенные форматы: {string.Join(", ", _allowedExtensions)}");

            var safeDinoName = string.Join("_", dinosaurName.Split(Path.GetInvalidFileNameChars()));
            var fileName = $"{DateTime.Now:yyyyMMdd_HHmmss}_{safeDinoName}_{Guid.NewGuid():N}{extension}";
            var safeFileName = string.Join("_", fileName.Split(Path.GetInvalidFileNameChars()));

            var uploadPath = GetUploadPath();

            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            var filePath = Path.Combine(uploadPath, safeFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }

            var relativePath = Path.Combine(_imagesFolder, safeFileName).Replace("\\", "/");

            Console.WriteLine($"Файл сохранен: {filePath}");
            Console.WriteLine($"Относительный путь: {relativePath}");

            return relativePath;
        }

        public async Task DeleteImageAsync(string imagePath)
        {
            if (string.IsNullOrEmpty(imagePath))
                return;

            var webRootPath = _environment.WebRootPath;
            if (string.IsNullOrEmpty(webRootPath))
            {
                webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            }

            var fullPath = Path.Combine(webRootPath, imagePath.Replace("/", "\\"));

            if (File.Exists(fullPath))
            {
                await Task.Run(() => File.Delete(fullPath));
                Console.WriteLine($"Файл удален: {fullPath}");
            }
        }

        public string GetImageUrl(string imagePath)
        {
            if (string.IsNullOrEmpty(imagePath))
                return string.Empty;

            var url = $"/{imagePath.Replace("\\", "/")}";
            Console.WriteLine($"Сгенерирован URL: {url}");
            return url;
        }
    }
}