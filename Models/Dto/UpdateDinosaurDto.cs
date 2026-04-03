// DinoAPI/Models/Dto/UpdateDinosaurDto.cs
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace DinoAPI.Models.Dto;

public class UpdateDinosaurDto
{
    [MaxLength(100)]
    public string? Name { get; set; }

    // Существующие поля
    [MaxLength(100)]
    public string? Clade { get; set; }

    [MaxLength(50)]
    public string? Era { get; set; }

    [MaxLength(50)]
    public string? Period { get; set; }

    [MaxLength(100)]
    public string? GroupName { get; set; }

    [MaxLength(100)]
    public string? Genus { get; set; }

    [MaxLength(100)]
    public string? Species { get; set; }

    public string? Description { get; set; }

    // ========== НОВЫЕ ПОЛЯ ==========
    public string? Size { get; set; }
    public string? FullDescription { get; set; }
    public string? Diet { get; set; }
    public string? Locomotion { get; set; }
    public string? Continent { get; set; }
    public string? Status { get; set; }
    public bool IsFeatured { get; set; }
    public bool AllowComments { get; set; }
    public string? DiscoveryLocation { get; set; }

    // Изображения
    [Url]
    public string? PhotoUrl { get; set; }
    public IFormFile? ImageFile { get; set; }
    public string? Comments { get; set; }
}