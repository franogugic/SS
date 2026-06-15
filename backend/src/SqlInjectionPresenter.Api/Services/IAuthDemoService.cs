using SqlInjectionPresenter.Api.DTOs;

namespace SqlInjectionPresenter.Api.Services;

public interface IAuthDemoService
{
    Task<IReadOnlyList<DemoUserDto>> GetUsersAsync();
    Task<LoginResultDto> VulnerableLoginAsync(LoginRequest request);
    Task<LoginResultDto> SafeLoginAsync(LoginRequest request);
    Task<ScenarioResultDto> UnionAttackAsync(ScenarioRequest request);
    Task<ScenarioResultDto> UnionSafeAsync(ScenarioRequest request);
    Task<ScenarioResultDto> ErrorAttackAsync(ScenarioRequest request);
    Task<ScenarioResultDto> ErrorSafeAsync(ScenarioRequest request);
    Task<ScenarioResultDto> BlindBooleanAttackAsync(ScenarioRequest request);
    Task<ScenarioResultDto> BlindBooleanSafeAsync(ScenarioRequest request);
    Task<ScenarioResultDto> TimeBasedAttackAsync(ScenarioRequest request);
    Task<ScenarioResultDto> TimeBasedSafeAsync(ScenarioRequest request);
}
