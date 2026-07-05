using Microsoft.EntityFrameworkCore;
using SqlInjectionPresenter.Api.Models;

namespace SqlInjectionPresenter.Api.Data;

public class DatabaseSeeder(ApplicationDbContext db)
{
    public async Task SeedAsync()
    {
        if (!await db.Users.AnyAsync())
        {
            db.Users.AddRange(
                new AppUser { Username = "admin", Password = "admin123", FullName = "Administrator sustava", Role = "Administrator" },
                new AppUser { Username = "root", Password = "root123", FullName = "Root User", Role = "Student" },
                new AppUser { Username = "ivan", Password = "lozinka", FullName = "Ivan Horvat", Role = "Korisnik" });

            await db.SaveChangesAsync();
        }

        if (!await db.StoredProfiles.AnyAsync())
        {
            // Bezopasni profil - za usporedbu ponašanja
            db.StoredProfiles.Add(new StoredProfile
            {
                Username = "normalni_korisnik",
                Note = "Obican korisnik bez posebnih prava.",
                CreatedAt = DateTime.UtcNow
            });

            await db.SaveChangesAsync();
        }
    }
}
