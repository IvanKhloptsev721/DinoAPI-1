using DinoAPI.Models.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using DinoAPI.Data;
using DinoAPI.Models;

namespace DinoAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DinosaursController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public DinosaursController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/dinosaurs
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Dinosaur>>> GetDinosaurs()
    {
        return await _context.Dinosaurs.ToListAsync();
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

        return dinosaur;
    }

    // POST: api/dinosaurs
    [HttpPost]
    public async Task<ActionResult<Dinosaur>> CreateDinosaur(CreateDinosaurDto dto)
    {
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
            PhotoUrl = dto.PhotoUrl ?? "https://example.com/default-dino.jpg",
            Comments = dto.Comments,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Dinosaurs.Add(dinosaur);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetDinosaur), new { id = dinosaur.Id }, dinosaur);
    }

    // PUT: api/dinosaurs/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateDinosaur(int id, UpdateDinosaurDto dto)
    {
        var dinosaur = await _context.Dinosaurs.FindAsync(id);

        if (dinosaur == null)
        {
            return NotFound();
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

        _context.Dinosaurs.Remove(dinosaur);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // GET: api/dinosaurs/era/Мел
    [HttpGet("era/{era}")]
    public async Task<ActionResult<IEnumerable<Dinosaur>>> GetDinosaursByEra(string era)
    {
        return await _context.Dinosaurs
            .Where(d => d.Era == era)
            .ToListAsync();
    }

    // GET: api/dinosaurs/clade/Theropoda
    [HttpGet("clade/{clade}")]
    public async Task<ActionResult<IEnumerable<Dinosaur>>> GetDinosaursByClade(string clade)
    {
        return await _context.Dinosaurs
            .Where(d => d.Clade == clade)
            .ToListAsync();
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

public static class SlugHelper
{
    public static string GenerateSlug(string name)
    {
        return name.ToLower()
            .Replace(" ", "-")
            .Replace(".", "")
            .Replace("'", "")
            .Trim();
    }
}