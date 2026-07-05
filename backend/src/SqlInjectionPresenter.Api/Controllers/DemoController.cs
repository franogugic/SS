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

    // --- GET endpointi s URL parametrima (za DAST/ZAP testiranje) ---

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string q = "")
    {
        var result = await authDemoService.UnionAttackAsync(new ScenarioRequest(q));
        return Ok(result);
    }

    [HttpGet("login")]
    public async Task<IActionResult> LoginGet([FromQuery] string username = "", [FromQuery] string password = "")
    {
        var result = await authDemoService.VulnerableLoginAsync(new LoginRequest(username, password));
        return Ok(result);
    }

    [HttpGet("user")]
    public async Task<IActionResult> UserGet([FromQuery] string username = "")
    {
        var result = await authDemoService.BlindBooleanAttackAsync(new ScenarioRequest(username));
        return Ok(result);
    }

    // --- Second-order SQL injection ---

    [HttpGet("stored-profiles")]
    public async Task<IActionResult> GetStoredProfiles()
    {
        var profiles = await authDemoService.GetStoredProfilesAsync();
        return Ok(profiles);
    }

    [HttpPost("store-profile")]
    public async Task<IActionResult> StoreProfile(StoreProfileRequest request)
    {
        var profile = await authDemoService.StoreProfileAsync(request);
        return Ok(profile);
    }

    [HttpPost("second-order-attack")]
    public async Task<IActionResult> SecondOrderAttack(SecondOrderRequest request)
    {
        var result = await authDemoService.SecondOrderAttackAsync(request);
        return Ok(result);
    }

    [HttpPost("second-order-safe")]
    public async Task<IActionResult> SecondOrderSafe(SecondOrderRequest request)
    {
        var result = await authDemoService.SecondOrderSafeAsync(request);
        return Ok(result);
    }
}
