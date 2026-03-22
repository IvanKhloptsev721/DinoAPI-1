using DinoAPI.Models.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using DinoAPI.Data;
using DinoAPI.Models;
using DinoAPI.Services;

namespace DinoAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DinosaursController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IImageService _imageService;

    public DinosaursController(ApplicationDbContext context, IImageService imageService)
    {
        _context = context;
        _imageService = imageService;
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
        // Сохраняем изображение если оно было загружено
        string? imagePath = null;
        if (dto.ImageFile != null)
        {
            try
            {
                imagePath = await _imageService.SaveImageAsync(dto.ImageFile, dto.Name);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        var dinosaur = new Dinosaur
        {
            Name = dto.Name,
            Slug = GenerateSlug(dto.Name),
            Clade = dto.Clade,
            Era = dto.Era,
            Period = dto.Period,
            GroupName = dto.GroupName,
            Genus = dto.Genus,
            Species = dto.Species,
            Description = dto.Description,
            PhotoUrl = dto.PhotoUrl ?? (imagePath != null ? _imageService.GetImageUrl(imagePath) : "https://example.com/default-dino.jpg"),
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
            return NotFound();
        }

        // Обработка нового изображения
        if (dto.ImageFile != null)
        {
            try
            {
                // Удаляем старое изображение если оно есть
                if (!string.IsNullOrEmpty(dinosaur.ImagePath))
                {
                    await _imageService.DeleteImageAsync(dinosaur.ImagePath);
                }

                // Сохраняем новое изображение
                var newImagePath = await _imageService.SaveImageAsync(dto.ImageFile, dto.Name ?? dinosaur.Name);
                dinosaur.ImagePath = newImagePath;
                dinosaur.PhotoUrl = _imageService.GetImageUrl(newImagePath);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // Обновляем только переданные поля
        dinosaur.Name = dto.Name ?? dinosaur.Name;
        dinosaur.Clade = dto.Clade ?? dinosaur.Clade;
        dinosaur.Era = dto.Era ?? dinosaur.Era;
        dinosaur.Period = dto.Period ?? dinosaur.Period;
        dinosaur.GroupName = dto.GroupName ?? dinosaur.GroupName;
        dinosaur.Genus = dto.Genus ?? dinosaur.Genus;
        dinosaur.Species = dto.Species ?? dinosaur.Species;
        dinosaur.Description = dto.Description ?? dinosaur.Description;
        dinosaur.PhotoUrl = dto.PhotoUrl ?? dinosaur.PhotoUrl;
        dinosaur.Comments = dto.Comments ?? dinosaur.Comments;
        dinosaur.UpdatedAt = DateTime.UtcNow;

        // Обновляем slug если изменилось имя
        if (dto.Name != null)
        {
            dinosaur.Slug = GenerateSlug(dto.Name);
        }

        await _context.SaveChangesAsync();

        return NoContent();
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

    private string GenerateSlug(string name)
    {
        return name.ToLower()
            .Replace(" ", "-")
            .Replace(".", "")
            .Replace("'", "")
            .Trim();
    }
}