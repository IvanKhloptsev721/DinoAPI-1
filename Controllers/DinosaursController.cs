using DinoAPI.Models.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DinoAPI.Data;
using DinoAPI.Models;
using DinoAPI.Services;
using System.IO;
using System.Linq; 
namespace DinoAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DinosaursController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IImageService _imageService;
    private readonly IWebHostEnvironment _environment;

    public DinosaursController(ApplicationDbContext context, IImageService imageService, IWebHostEnvironment environment)
    {
        _context = context;
        _imageService = imageService;
        _environment = environment;
    }

    // GET: api/dinosaurs
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Dinosaur>>> GetDinosaurs()
    {
        var dinosaurs = await _context.Dinosaurs.ToListAsync();

        // Добавляем полные URL для изображений
        foreach (var dino in dinosaurs)
        {
            if (!string.IsNullOrEmpty(dino.ImagePath))
            {
                dino.PhotoUrl = _imageService.GetImageUrl(dino.ImagePath);
            }
        }

        return dinosaurs;
    }

    // GET: api/dinosaurs/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Dinosaur>> GetDinosaur(int id)
    {
        var dinosaur = await _context.Dinosaurs.FindAsync(id);

        if (dinosaur == null)
        {
            return NotFound();
        }

        // Добавляем полный URL для изображения
        if (!string.IsNullOrEmpty(dinosaur.ImagePath))
        {
            dinosaur.PhotoUrl = _imageService.GetImageUrl(dinosaur.ImagePath);

            // Отладочная информация
            Console.WriteLine($"=== Dinosaur {id} ===");
            Console.WriteLine($"ImagePath: {dinosaur.ImagePath}");
            Console.WriteLine($"PhotoUrl: {dinosaur.PhotoUrl}");
            Console.WriteLine($"==================");
        }

        return dinosaur;
    }

    // GET: api/dinosaurs/slug/tyrannosaurus-rex
    [HttpGet("slug/{slug}")]
    public async Task<ActionResult<Dinosaur>> GetDinosaurBySlug(string slug)
    {
        var dinosaur = await _context.Dinosaurs
            .FirstOrDefaultAsync(d => d.Slug == slug);

        if (dinosaur == null)
        {
            return NotFound();
        }

        // Добавляем полный URL для изображения
        if (!string.IsNullOrEmpty(dinosaur.ImagePath))
        {
            dinosaur.PhotoUrl = _imageService.GetImageUrl(dinosaur.ImagePath);
        }

        return dinosaur;
    }

    // POST: api/dinosaurs
    [HttpPost]
    public async Task<ActionResult<Dinosaur>> CreateDinosaur([FromForm] CreateDinosaurDto dto)
    {
        // Проверяем уникальность slug
        var slug = GenerateSlug(dto.Name);
        if (await _context.Dinosaurs.AnyAsync(d => d.Slug == slug))
        {
            return BadRequest(new { error = "Динозавр с таким именем уже существует" });
        }

        string? imagePath = null;
        string? photoUrl = null;

        // Сохраняем изображение если оно было загружено
        if (dto.ImageFile != null)
        {
            try
            {
                imagePath = await _imageService.SaveImageAsync(dto.ImageFile, dto.Name);
                photoUrl = _imageService.GetImageUrl(imagePath);

                // Отладочная информация
                Console.WriteLine($"=== Сохранение изображения ===");
                Console.WriteLine($"ImagePath: {imagePath}");
                Console.WriteLine($"PhotoUrl: {photoUrl}");
                Console.WriteLine($"==============================");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        var dinosaur = new Dinosaur
        {
            Name = dto.Name,
            Slug = slug,
            Clade = dto.Clade,
            Era = dto.Era,
            Period = dto.Period,
            GroupName = dto.GroupName,
            Genus = dto.Genus,
            Species = dto.Species,
            Description = dto.Description,
            PhotoUrl = photoUrl ?? dto.PhotoUrl ?? "https://example.com/default-dino.jpg",
            ImagePath = imagePath,
            Comments = dto.Comments,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Dinosaurs.Add(dinosaur);
        await _context.SaveChangesAsync();

        // Добавляем полный URL для изображения в ответе
        if (!string.IsNullOrEmpty(dinosaur.ImagePath))
        {
            dinosaur.PhotoUrl = _imageService.GetImageUrl(dinosaur.ImagePath);
        }

        return CreatedAtAction(nameof(GetDinosaur), new { id = dinosaur.Id }, dinosaur);
    }

    // PUT: api/dinosaurs/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateDinosaur(int id, [FromForm] UpdateDinosaurDto dto)
    {
        var dinosaur = await _context.Dinosaurs.FindAsync(id);

        if (dinosaur == null)
        {
            return NotFound(new { error = "Динозавр не найден" });
        }

        // Начинаем транзакцию
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            string? oldImagePath = null;

            // ========== ОБРАБОТКА ИЗОБРАЖЕНИЯ ==========
            if (dto.ImageFile != null && dto.ImageFile.Length > 0)
            {
                try
                {
                    // Сохраняем путь старого изображения для удаления
                    oldImagePath = dinosaur.ImagePath;

                    // Сохраняем новое изображение
                    var imageName = !string.IsNullOrEmpty(dto.Name) ? dto.Name : dinosaur.Name;
                    var newImagePath = await _imageService.SaveImageAsync(dto.ImageFile, imageName);

                    dinosaur.ImagePath = newImagePath;
                    dinosaur.PhotoUrl = _imageService.GetImageUrl(newImagePath);

                    Console.WriteLine($"=== ОБНОВЛЕНИЕ ИЗОБРАЖЕНИЯ ===");
                    Console.WriteLine($"Старый путь: {oldImagePath}");
                    Console.WriteLine($"Новый путь: {newImagePath}");
                    Console.WriteLine($"Новый URL: {dinosaur.PhotoUrl}");
                }
                catch (InvalidOperationException ex)
                {
                    return BadRequest(new { error = ex.Message });
                }
            }
            else if (!string.IsNullOrEmpty(dto.PhotoUrl))
            {
                // Если передан PhotoUrl и нет файла, обновляем только URL
                dinosaur.PhotoUrl = dto.PhotoUrl;
                Console.WriteLine($"Обновлен PhotoUrl: {dinosaur.PhotoUrl}");
            }
            // Если нет ни файла, ни PhotoUrl - оставляем существующее изображение

            // ========== ОБНОВЛЕНИЕ ОСНОВНЫХ ПОЛЕЙ ==========
            if (!string.IsNullOrEmpty(dto.Name))
            {
                var newSlug = GenerateSlug(dto.Name);

                // Проверяем уникальность slug
                if (newSlug != dinosaur.Slug &&
                    await _context.Dinosaurs.AnyAsync(d => d.Slug == newSlug && d.Id != id))
                {
                    return BadRequest(new { error = "Динозавр с таким именем уже существует" });
                }

                dinosaur.Name = dto.Name;
                dinosaur.Slug = newSlug;
            }

            // Обновляем все поля (даже если null, оставляем существующие)
            if (dto.Clade != null) dinosaur.Clade = dto.Clade;
            if (dto.Era != null) dinosaur.Era = dto.Era;
            if (dto.Period != null) dinosaur.Period = dto.Period;
            if (dto.GroupName != null) dinosaur.GroupName = dto.GroupName;
            if (dto.Genus != null) dinosaur.Genus = dto.Genus;
            if (dto.Species != null) dinosaur.Species = dto.Species;
            if (dto.Description != null) dinosaur.Description = dto.Description;
            if (dto.Comments != null) dinosaur.Comments = dto.Comments;

            dinosaur.UpdatedAt = DateTime.UtcNow;

            // Сохраняем изменения в БД
            await _context.SaveChangesAsync();

            // Удаляем старое изображение ТОЛЬКО если было загружено новое
            if (oldImagePath != null)
            {
                await _imageService.DeleteImageAsync(oldImagePath);
                Console.WriteLine($"Старое изображение удалено: {oldImagePath}");
            }

            await transaction.CommitAsync();

            // Получаем обновленного динозавра с правильным URL
            var updatedDinosaur = await _context.Dinosaurs.FindAsync(id);
            if (!string.IsNullOrEmpty(updatedDinosaur?.ImagePath))
            {
                updatedDinosaur.PhotoUrl = _imageService.GetImageUrl(updatedDinosaur.ImagePath);
            }

            Console.WriteLine($"=== ДИНОЗАВР УСПЕШНО ОБНОВЛЕН ===");
            Console.WriteLine($"ID: {updatedDinosaur?.Id}");
            Console.WriteLine($"Name: {updatedDinosaur?.Name}");
            Console.WriteLine($"ImagePath: {updatedDinosaur?.ImagePath}");
            Console.WriteLine($"PhotoUrl: {updatedDinosaur?.PhotoUrl}");

            return Ok(updatedDinosaur);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            Console.WriteLine($"ОШИБКА ПРИ ОБНОВЛЕНИИ: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            return StatusCode(500, new { error = "Внутренняя ошибка сервера", details = ex.Message });
        }
    }
    [HttpGet("debug/check/{id}")]
    public async Task<IActionResult> DebugCheck(int id)
    {
        var dinosaur = await _context.Dinosaurs.FindAsync(id);

        if (dinosaur == null)
            return NotFound();

        var webRootPath = _environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        var fullPath = !string.IsNullOrEmpty(dinosaur.ImagePath)
            ? Path.Combine(webRootPath, dinosaur.ImagePath.Replace("/", "\\"))
            : null;

        var result = new
        {
            dinosaur.Id,
            dinosaur.Name,
            dinosaur.Slug,
            dinosaur.ImagePath,
            dinosaur.PhotoUrl,
            FullPath = fullPath,
            FileExists = fullPath != null && System.IO.File.Exists(fullPath),
            WebRootPath = webRootPath,
            WebRootExists = Directory.Exists(webRootPath),
            UploadFolderExists = !string.IsNullOrEmpty(dinosaur.ImagePath) &&
                Directory.Exists(Path.GetDirectoryName(fullPath))
        };

        return Ok(result);
    }

    // DELETE: api/dinosaurs/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDinosaur(int id)
    {
        var dinosaur = await _context.Dinosaurs.FindAsync(id);

        if (dinosaur == null)
        {
            return NotFound();
        }

        // Удаляем изображение если оно есть
        if (!string.IsNullOrEmpty(dinosaur.ImagePath))
        {
            await _imageService.DeleteImageAsync(dinosaur.ImagePath);
        }

        _context.Dinosaurs.Remove(dinosaur);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // GET: api/dinosaurs/era/Мел
    [HttpGet("era/{era}")]
    public async Task<ActionResult<IEnumerable<Dinosaur>>> GetDinosaursByEra(string era)
    {
        var dinosaurs = await _context.Dinosaurs
            .Where(d => d.Era == era)
            .ToListAsync();

        // Добавляем полные URL для изображений
        foreach (var dino in dinosaurs)
        {
            if (!string.IsNullOrEmpty(dino.ImagePath))
            {
                dino.PhotoUrl = _imageService.GetImageUrl(dino.ImagePath);
            }
        }

        return dinosaurs;
    }

    // GET: api/dinosaurs/clade/Theropoda
    [HttpGet("clade/{clade}")]
    public async Task<ActionResult<IEnumerable<Dinosaur>>> GetDinosaursByClade(string clade)
    {
        var dinosaurs = await _context.Dinosaurs
            .Where(d => d.Clade == clade)
            .ToListAsync();

        // Добавляем полные URL для изображений
        foreach (var dino in dinosaurs)
        {
            if (!string.IsNullOrEmpty(dino.ImagePath))
            {
                dino.PhotoUrl = _imageService.GetImageUrl(dino.ImagePath);
            }
        }

        return dinosaurs;
    }
    // GET: api/dinosaurs/debug/images - информация о сохраненных изображениях
    [HttpGet("debug/images")]
    public IActionResult DebugImages()
    {
        var webRootPath = _environment.WebRootPath ?? System.IO.Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        var uploadPath = System.IO.Path.Combine(webRootPath, "uploads", "dinosaurs");

        var filesList = new List<object>();

        if (System.IO.Directory.Exists(uploadPath))
        {
            var files = System.IO.Directory.GetFiles(uploadPath);
            foreach (var f in files)
            {
                filesList.Add(new
                {
                    FullPath = f,
                    Name = System.IO.Path.GetFileName(f),
                    Size = new System.IO.FileInfo(f).Length,
                    LastModified = System.IO.File.GetLastWriteTime(f),
                    Url = $"/uploads/dinosaurs/{System.IO.Path.GetFileName(f)}"
                });
            }
        }

        var dinosaursList = new List<object>();
        var dinosaurs = _context.Dinosaurs.ToList();
        foreach (var d in dinosaurs)
        {
            dinosaursList.Add(new
            {
                d.Id,
                d.Name,
                d.ImagePath,
                d.PhotoUrl,
                FullUrl = !string.IsNullOrEmpty(d.ImagePath) ? _imageService.GetImageUrl(d.ImagePath) : null
            });
        }

        var result = new
        {
            WebRootPath = webRootPath,
            WebRootExists = System.IO.Directory.Exists(webRootPath),
            UploadPath = uploadPath,
            UploadExists = System.IO.Directory.Exists(uploadPath),
            Files = filesList,
            Dinosaurs = dinosaursList
        };

        return Ok(result);
    }

    // GET: api/dinosaurs/debug/check-image - проверка конкретного изображения
    [HttpGet("debug/check-image")]
    public IActionResult CheckImage([FromQuery] string imagePath) // ← Переименовал параметр
    {
        if (string.IsNullOrEmpty(imagePath))
            return BadRequest("Path is required");

        var webRootPath = _environment.WebRootPath ?? System.IO.Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        var fullPath = System.IO.Path.Combine(webRootPath, imagePath.Replace("/", "\\"));

        var result = new
        {
            RequestedPath = imagePath,
            FullPath = fullPath,
            FileExists = System.IO.File.Exists(fullPath),
            FileSize = System.IO.File.Exists(fullPath) ? new System.IO.FileInfo(fullPath).Length : 0,
            WebRootPath = webRootPath,
            WebRootExists = System.IO.Directory.Exists(webRootPath)
        };

        return Ok(result);
    }

    // GET: api/dinosaurs/debug/test - тестовый эндпоинт
    [HttpGet("debug/test")]
    public IActionResult Test()
    {
        var webRootPath = _environment.WebRootPath ?? System.IO.Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

        return Ok(new
        {
            Status = "OK",
            WebRootPath = webRootPath,
            WebRootExists = System.IO.Directory.Exists(webRootPath),
            Environment = _environment.EnvironmentName,
            ContentRootPath = _environment.ContentRootPath
        });
    }

    private string GenerateSlug(string name)
    {
        return name.ToLower()
            .Replace(" ", "-")
            .Replace(".", "")
            .Replace("'", "")
            .Replace("?", "")
            .Replace("&", "")
            .Replace("/", "")
            .Replace("\\", "")
            .Trim();
    }
}