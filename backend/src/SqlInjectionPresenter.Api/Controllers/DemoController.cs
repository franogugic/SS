using Microsoft.AspNetCore.Mvc;
using SqlInjectionPresenter.Api.DTOs;
using SqlInjectionPresenter.Api.Services;

namespace SqlInjectionPresenter.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DemoController(IAuthDemoService authDemoService) : ControllerBase
{
    [HttpGet("users")]
    public async Task<IActionResult> GetUsers()
    {
        var users = await authDemoService.GetUsersAsync();
        return Ok(users);
    }

    [HttpPost("vulnerable-login")]
    public async Task<IActionResult> VulnerableLogin(LoginRequest request)
    {
        var result = await authDemoService.VulnerableLoginAsync(request);
        return Ok(result);
    }

    [HttpPost("safe-login")]
    public async Task<IActionResult> SafeLogin(LoginRequest request)
    {
        var result = await authDemoService.SafeLoginAsync(request);
        return Ok(result);
    }

    [HttpPost("union-attack")]
    public async Task<IActionResult> UnionAttack(ScenarioRequest request)
    {
        var result = await authDemoService.UnionAttackAsync(request);
        return Ok(result);
    }

    [HttpPost("union-safe")]
    public async Task<IActionResult> UnionSafe(ScenarioRequest request)
    {
        var result = await authDemoService.UnionSafeAsync(request);
        return Ok(result);
    }

    [HttpPost("error-attack")]
    public async Task<IActionResult> ErrorAttack(ScenarioRequest request)
    {
        var result = await authDemoService.ErrorAttackAsync(request);
        return Ok(result);
    }

    [HttpPost("error-safe")]
    public async Task<IActionResult> ErrorSafe(ScenarioRequest request)
    {
        var result = await authDemoService.ErrorSafeAsync(request);
        return Ok(result);
    }

    [HttpPost("blind-boolean-attack")]
    public async Task<IActionResult> BlindBooleanAttack(ScenarioRequest request)
    {
        var result = await authDemoService.BlindBooleanAttackAsync(request);
        return Ok(result);
    }

    [HttpPost("blind-boolean-safe")]
    public async Task<IActionResult> BlindBooleanSafe(ScenarioRequest request)
    {
        var result = await authDemoService.BlindBooleanSafeAsync(request);
        return Ok(result);
    }

    [HttpPost("time-based-attack")]
    public async Task<IActionResult> TimeBasedAttack(ScenarioRequest request)
    {
        var result = await authDemoService.TimeBasedAttackAsync(request);
        return Ok(result);
    }

    [HttpPost("time-based-safe")]
    public async Task<IActionResult> TimeBasedSafe(ScenarioRequest request)
    {
        var result = await authDemoService.TimeBasedSafeAsync(request);
        return Ok(result);
    }
}
