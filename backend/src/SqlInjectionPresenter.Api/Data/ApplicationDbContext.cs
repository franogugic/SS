using Microsoft.EntityFrameworkCore;
using SqlInjectionPresenter.Api.Models;

namespace SqlInjectionPresenter.Api.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<StoredProfile> StoredProfiles => Set<StoredProfile>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.HasIndex(user => user.Username).IsUnique();
            entity.Property(user => user.Username).HasMaxLength(80).IsRequired();
            entity.Property(user => user.Password).HasMaxLength(80).IsRequired();
            entity.Property(user => user.FullName).HasMaxLength(120).IsRequired();
            entity.Property(user => user.Role).HasMaxLength(60).IsRequired();
        });

        modelBuilder.Entity<StoredProfile>(entity =>
        {
            entity.Property(p => p.Username).HasMaxLength(200).IsRequired();
            entity.Property(p => p.Note).HasMaxLength(500).IsRequired();
        });
    }
}
