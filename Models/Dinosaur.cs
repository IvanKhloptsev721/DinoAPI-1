// DinoAPI/Models/Dinosaur.cs
using System.ComponentModel.DataAnnotations;

namespace DinoAPI.Models;

public class Dinosaur
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Slug { get; set; } = string.Empty;

    // Существующие поля
    public string? Clade { get; set; }
    public string? Era { get; set; }
    public string? Period { get; set; }
    public string? GroupName { get; set; }
    public string? Genus { get; set; }
    public string? Species { get; set; }
    public string? Description { get; set; }

    // ========== НОВЫЕ ПОЛЯ ==========
    public string? Size { get; set; }                    // Размер
    public string? FullDescription { get; set; }         // Полное описание
    public string? Diet { get; set; }                    // Тип питания
    public string? Locomotion { get; set; }              // Передвижение
    public string? Continent { get; set; }               // Континент
    public string? Status { get; set; }                  // Статус
    public bool IsFeatured { get; set; }                 // Избранный
    public bool AllowComments { get; set; }              // Разрешены комментарии
    public string? DiscoveryLocation { get; set; }       // Место обнаружения

    // Изображения
    public string? PhotoUrl { get; set; }
    public string? ImagePath { get; set; }

    // Комментарии (сохраняем как текст с разделителями)
    public string? Comments { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}