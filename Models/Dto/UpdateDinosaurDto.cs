using System.ComponentModel.DataAnnotations;

namespace DinoAPI.Models.Dto;

public class UpdateDinosaurDto
{
    [MaxLength(100)]
    public string? Name { get; set; }

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

    [Url]
    public string? PhotoUrl { get; set; }

    public string? Comments { get; set; }

    // Добавляем поле для загрузки файла
    public IFormFile? ImageFile { get; set; }
}