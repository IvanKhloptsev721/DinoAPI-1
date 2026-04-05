using DinoAPI.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace DinoAPI.Data;
public class ApplicationDbContext : IdentityDbContext<IdentityUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Dinosaur> Dinosaurs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder); 

        modelBuilder.Entity<Dinosaur>()
            .HasIndex(d => d.Slug)
            .IsUnique();


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
               Size = "12 метров в длину, 5 метров в высоту",
               FullDescription = "Тираннозавр был одним из самых крупных хищников...",
               Diet = "Хищник",
               Locomotion = "Двуногий",
               Continent = "Северная Америка",
               Status = "Вымерший",
               IsFeatured = true,
               AllowComments = true,
               DiscoveryLocation = "Формация Хелл-Крик, Монтана",
               PhotoUrl = "https://avatars.mds.yandex.net/i?id=174ae4ec130900a9d18a64f258c4ee80_l-7016679-images-thumbs&n=13",
               CreatedAt = DateTime.UtcNow,
               UpdatedAt = DateTime.UtcNow
           }
       );
    }
}