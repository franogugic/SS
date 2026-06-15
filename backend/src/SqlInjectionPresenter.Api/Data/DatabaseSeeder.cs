using Microsoft.EntityFrameworkCore;
using SqlInjectionPresenter.Api.Models;

namespace SqlInjectionPresenter.Api.Data;

public class DatabaseSeeder(ApplicationDbContext db)
{
    public async Task SeedAsync()
    {
        if (await db.Users.AnyAsync())
        {
            return;
        }

        db.Users.AddRange(
            new AppUser { Username = "admin", Password = "admin123", FullName = "Administrator sustava", Role = "Administrator" },
            new AppUser { Username = "root", Password = "root123", FullName = "Root User", Role = "Student" },
            new AppUser { Username = "ivan", Password = "lozinka", FullName = "Ivan Horvat", Role = "Korisnik" });

        await db.SaveChangesAsync();
    }
}
