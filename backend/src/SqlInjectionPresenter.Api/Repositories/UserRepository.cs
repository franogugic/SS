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
        {
            await connection.OpenAsync();
        }

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
        {
            await connection.OpenAsync();
        }

        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.CommandTimeout = 10;

        await command.ExecuteNonQueryAsync();
    }
}
