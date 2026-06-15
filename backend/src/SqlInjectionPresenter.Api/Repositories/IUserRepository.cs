using SqlInjectionPresenter.Api.Models;

namespace SqlInjectionPresenter.Api.Repositories;

public interface IUserRepository
{
    Task<IReadOnlyList<AppUser>> GetAllAsync();
    Task<IReadOnlyList<AppUser>> FindWithUnsafeSqlAsync(string sql);
    Task<AppUser?> FindWithSafeQueryAsync(string username, string password);
    Task<IReadOnlyList<AppUser>> FindByUsernameSafeAsync(string username);
    Task<IReadOnlyList<AppUser>> SearchByUsernameSafeAsync(string searchTerm);
    Task<int> CountByUsernameSafeAsync(string username);
    Task<int> ReadUnsafeNumberAsync(string sql);
    Task ExecuteUnsafeCommandAsync(string sql);
}
