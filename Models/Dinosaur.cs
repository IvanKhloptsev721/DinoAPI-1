using System;

namespace DinoAPI.Models;
public class Dinosaur
{
    public int Id { get; set; }

    // Обязательные поля
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;

    // Необязательные поля - все с ? и = null!
    public string? Clade { get; set; }
    public string? Era { get; set; }
    public string? Period { get; set; }
    public string? GroupName { get; set; }
    public string? Genus { get; set; }
    public string? Species { get; set; }
    public string? Description { get; set; }
    public string? PhotoUrl { get; set; }

    // Метаданные
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? Comments { get; set; }
}