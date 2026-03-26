using Microsoft.EntityFrameworkCore;
using DinoAPI.Models;

namespace DinoAPI.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Dinosaur> Dinosaurs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Настройка уникальности Slug
        modelBuilder.Entity<Dinosaur>()
            .HasIndex(d => d.Slug)
            .IsUnique();

        // Начальные данные (seed) - ИСПРАВЛЕНО
        modelBuilder.Entity<Dinosaur>().HasData(
            new Dinosaur
            {
                Id = 1,
                Name = "Tyrannosaurus Rex",
                Slug = "tyrannosaurus-rex",
                Clade = "Theropoda",
                Era = "Мезозой",
                Period = "Мел",
                GroupName = "Tyrannosauridae",
                Genus = "Tyrannosaurus",
                Species = "T. rex",
                Description = "Один из крупнейших наземных хищников всех времен.",
                PhotoUrl = "https://example.com/trex.jpg",
                ImagePath = null, // Добавлено явно
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Comments = null
            },
            new Dinosaur
            {
                Id = 2,
                Name = "Stegosaurus",
                Slug = "stegosaurus",
                Clade = "Thyreophora",
                Era = "Мезозой",
                Period = "Юра",
                GroupName = "Stegosauridae",
                Genus = "Stegosaurus",
                Species = null,
                Description = "Травоядный динозавр с характерными пластинами на спине.",
                PhotoUrl = "https://example.com/stegosaurus.jpg",
                ImagePath = null, // Добавлено явно
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Comments = null
            }
        );
    }
}