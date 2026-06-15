namespace SqlInjectionPresenter.Api.DTOs;

public record LoginRequest(string Username, string Password);

public record DemoUserDto(int Id, string Username, string Password, string FullName, string Role);

public record LoginResultDto(
    bool Success,
    string Mode,
    string Message,
    string ExecutedSql,
    IReadOnlyList<DemoUserDto> Users);

public record ScenarioRequest(string Payload);

public record ScenarioResultDto(
    bool Success,
    string Scenario,
    string Message,
    string ExecutedSql,
    string? DatabaseMessage,
    long ElapsedMilliseconds,
    IReadOnlyList<DemoUserDto> Users);
