using Microsoft.EntityFrameworkCore;
using SqlInjectionPresenter.Api.Data;
using SqlInjectionPresenter.Api.Models;
using System.Data;

namespace SqlInjectionPresenter.Api.Repositories;

public class UserRepository(ApplicationDbContext db) : IUserRepository
{
    public async Task<IReadOnlyList<AppUser>> GetAllAsync() =>
        await db.Users
            .AsNoTracking()
            .OrderBy(user => user.Id)
            .ToListAsync();

    public async Task<IReadOnlyList<AppUser>> FindWithUnsafeSqlAsync(string sql) =>
        await db.Users
            .FromSqlRaw(sql)
            .AsNoTracking()
            .ToListAsync();

    public async Task<AppUser?> FindWithSafeQueryAsync(string username, string password) =>
        await db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(user => user.Username == username && user.Password == password);

    public async Task<IReadOnlyList<AppUser>> FindByUsernameSafeAsync(string username) =>
        await db.Users
            .AsNoTracking()
            .Where(user => user.Username == username)
            .OrderBy(user => user.Id)
            .ToListAsync();

    public async Task<IReadOnlyList<AppUser>> SearchByUsernameSafeAsync(string searchTerm) =>
        await db.Users
            .AsNoTracking()
            .Where(user => user.Username.Contains(searchTerm))
            .OrderBy(user => user.Id)
            .ToListAsync();

    public async Task<int> CountByUsernameSafeAsync(string username) =>
        await db.Users
            .AsNoTracking()
            .CountAsync(user => user.Username == username);

    public async Task<int> ReadUnsafeNumberAsync(string sql)
    {
        var connection = db.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.CommandTimeout = 10;
        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task ExecuteUnsafeCommandAsync(string sql)
    {
        var connection = db.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.CommandTimeout = 10;
        await command.ExecuteNonQueryAsync();
    }

    // --- Second-order SQLi ---

    public async Task<StoredProfile?> GetStoredProfileByIdAsync(int id) =>
        await db.StoredProfiles.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);

    // Faza 1: sigurna pohrana — parameterized INSERT putem EF Core-a.
    // Napadač ovdje može pohraniti payload poput: admin'--
    public async Task<StoredProfile> StoreProfileSafeAsync(string username, string note)
    {
        var profile = new StoredProfile
        {
            Username = username,
            Note = note,
            CreatedAt = DateTime.UtcNow
        };
        db.StoredProfiles.Add(profile);
        await db.SaveChangesAsync();
        return profile;
    }

    // Faza 2 (RANJIVA): dohvaća pohranjeni username iz baze i direktno ga
    // konkatenira u SQL upit — klasični second-order injection.
    // semgrep-tag: second-order-sqli
    // SonarQube (SAST) detektira ovu metodu jer vidi FromSqlRaw s interpoliranim stringom.
    // OWASP ZAP (DAST) NE detektira jer payload šalje u fazi 1, a injection se dogodi u fazi 2.
    public async Task<IReadOnlyList<AppUser>> FindUsersByStoredUsernameUnsafeAsync(string storedUsername)
    {
        var sql = $"""
            SELECT [Id], [Username], [Password], [FullName], [Role]
            FROM [Users]
            WHERE [Username] = '{storedUsername}'
            """;

        return await db.Users
            .FromSqlRaw(sql)
            .AsNoTracking()
            .ToListAsync();
    }

    // Faza 2 (SIGURNA): isti scenarij ali s parametriziranim upitom.
    public async Task<IReadOnlyList<AppUser>> FindUsersByStoredUsernameSafeAsync(string storedUsername) =>
        await db.Users
            .AsNoTracking()
            .Where(u => u.Username == storedUsername)
            .ToListAsync();

    public async Task<IReadOnlyList<StoredProfile>> GetAllStoredProfilesAsync() =>
        await db.StoredProfiles
            .AsNoTracking()
            .OrderBy(p => p.Id)
            .ToListAsync();
}
