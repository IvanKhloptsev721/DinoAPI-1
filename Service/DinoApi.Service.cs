using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
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
        private const int MaxFileSize = 5 * 1024 * 1024; // 5 MB

        public ImageService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<string?> SaveImageAsync(IFormFile imageFile, string dinosaurName)
        {
            if (imageFile == null || imageFile.Length == 0)
                return null;

            // Проверка размера файла
            if (imageFile.Length > MaxFileSize)
                throw new InvalidOperationException($"Файл слишком большой. Максимальный размер: {MaxFileSize / 1024 / 1024} MB");

            // Проверка расширения файла
            var extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
            if (!_allowedExtensions.Contains(extension))
                throw new InvalidOperationException($"Недопустимый формат файла. Разрешенные форматы: {string.Join(", ", _allowedExtensions)}");

            // Создаем уникальное имя файла
            var fileName = $"{DateTime.Now:yyyyMMdd_HHmmss}_{dinosaurName}_{Guid.NewGuid():N}{extension}";
            var safeFileName = MakeSafeFileName(fileName);

            var uploadPath = Path.Combine(_environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), _imagesFolder);

            // Создаем директорию если её нет
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            var filePath = Path.Combine(uploadPath, safeFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }

            return Path.Combine(_imagesFolder, safeFileName).Replace("\\", "/");
        }

        public async Task DeleteImageAsync(string imagePath)
        {
            if (string.IsNullOrEmpty(imagePath))
                return;

            var fullPath = Path.Combine(_environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), imagePath);

            if (File.Exists(fullPath))
            {
                await Task.Run(() => File.Delete(fullPath));
            }
        }

        public string GetImageUrl(string imagePath)
        {
            if (string.IsNullOrEmpty(imagePath))
                return string.Empty;

            return $"/{imagePath.Replace("\\", "/")}";
        }

        private string MakeSafeFileName(string fileName)
        {
            return string.Join("_", fileName.Split(Path.GetInvalidFileNameChars()));
        }
    }
}