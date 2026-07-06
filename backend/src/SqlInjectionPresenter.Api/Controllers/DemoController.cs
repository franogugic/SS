using Microsoft.AspNetCore.Mvc;
using SqlInjectionPresenter.Api.DTOs;
using SqlInjectionPresenter.Api.Services;

namespace SqlInjectionPresenter.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DemoController(IAuthDemoService authDemoService) : ControllerBase
{
    // Dohvaća sve korisnike iz baze — koristi se za prikaz tablice na frontendu
    [HttpGet("users")]
    public async Task<IActionResult> GetUsers()
    {
        var users = await authDemoService.GetUsersAsync();
        return Ok(users);
    }

    // Ranjiva prijava — SQL se gradi konkatenacijom username i password, omogućava bypass
    [HttpPost("vulnerable-login")]
    public async Task<IActionResult> VulnerableLogin(LoginRequest request)
    {
        var result = await authDemoService.VulnerableLoginAsync(request);
        return Ok(result);
    }

    // Sigurna prijava — koristi parametrizirani EF Core upit, injection nije moguć
    [HttpPost("safe-login")]
    public async Task<IActionResult> SafeLogin(LoginRequest request)
    {
        var result = await authDemoService.SafeLoginAsync(request);
        return Ok(result);
    }

    // UNION napad — payload se konkatenira u LIKE klauzulu, omogućava izvlačenje podataka UNION SELECT-om
    [HttpPost("union-attack")]
    public async Task<IActionResult> UnionAttack(ScenarioRequest request)
    {
        var result = await authDemoService.UnionAttackAsync(request);
        return Ok(result);
    }

    // Sigurna pretraga — payload se koristi kao parametar, UNION ostaje obični tekst
    [HttpPost("union-safe")]
    public async Task<IActionResult> UnionSafe(ScenarioRequest request)
    {
        var result = await authDemoService.UnionSafeAsync(request);
        return Ok(result);
    }

    // Error-based napad — payload lomi SQL sintaksu, poruka greške otkriva strukturu upita
    [HttpPost("error-attack")]
    public async Task<IActionResult> ErrorAttack(ScenarioRequest request)
    {
        var result = await authDemoService.ErrorAttackAsync(request);
        return Ok(result);
    }

    // Sigurni error scenarij — payload je parametar, sintaksa ostaje ispravna, nema greške
    [HttpPost("error-safe")]
    public async Task<IActionResult> ErrorSafe(ScenarioRequest request)
    {
        var result = await authDemoService.ErrorSafeAsync(request);
        return Ok(result);
    }

    // Blind boolean napad — payload mijenja WHERE uvjet, aplikacija otkriva informaciju kroz da/ne odgovor
    [HttpPost("blind-boolean-attack")]
    public async Task<IActionResult> BlindBooleanAttack(ScenarioRequest request)
    {
        var result = await authDemoService.BlindBooleanAttackAsync(request);
        return Ok(result);
    }

    // Sigurni blind boolean — payload se uspoređuje doslovno, ne izvršava se kao SQL uvjet
    [HttpPost("blind-boolean-safe")]
    public async Task<IActionResult> BlindBooleanSafe(ScenarioRequest request)
    {
        var result = await authDemoService.BlindBooleanSafeAsync(request);
        return Ok(result);
    }

    // Time-based napad — payload ubacuje WAITFOR DELAY, kašnjenje odgovora potvrđuje uvjet
    [HttpPost("time-based-attack")]
    public async Task<IActionResult> TimeBasedAttack(ScenarioRequest request)
    {
        var result = await authDemoService.TimeBasedAttackAsync(request);
        return Ok(result);
    }

    // Sigurni time-based — payload je parametar, WAITFOR DELAY se ne izvršava
    [HttpPost("time-based-safe")]
    public async Task<IActionResult> TimeBasedSafe(ScenarioRequest request)
    {
        var result = await authDemoService.TimeBasedSafeAsync(request);
        return Ok(result);
    }

    // GET endpointi s URL parametrima — dodani za ZAP DAST testiranje
    // ZAP može testirati GET parametre (q, username, password) SQL injection payloadima

    // Ranjiva pretraga korisnika po imenu — q parametar se konkatenira u SQL (za ZAP)
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string q = "")
    {
        var result = await authDemoService.UnionAttackAsync(new ScenarioRequest(q));
        return Ok(result);
    }

    // Ranjiva GET prijava — username i password parametri se konkateniraju u SQL (za ZAP)
    [HttpGet("login")]
    public async Task<IActionResult> LoginGet([FromQuery] string username = "", [FromQuery] string password = "")
    {
        var result = await authDemoService.VulnerableLoginAsync(new LoginRequest(username, password));
        return Ok(result);
    }

    // Ranjivi dohvat korisnika po username-u — parametar se konkatenira u SQL (za ZAP)
    [HttpGet("user")]
    public async Task<IActionResult> UserGet([FromQuery] string username = "")
    {
        var result = await authDemoService.BlindBooleanAttackAsync(new ScenarioRequest(username));
        return Ok(result);
    }

    // Second-order SQL injection endpointi

    // Dohvaća sve pohranjene profile iz baze
    [HttpGet("stored-profiles")]
    public async Task<IActionResult> GetStoredProfiles()
    {
        var profiles = await authDemoService.GetStoredProfilesAsync();
        return Ok(profiles);
    }

    // Faza 1: sigurna pohrana payloada — EF Core parametrizirani INSERT, injection nije moguć ovdje
    [HttpPost("store-profile")]
    public async Task<IActionResult> StoreProfile(StoreProfileRequest request)
    {
        var profile = await authDemoService.StoreProfileAsync(request);
        return Ok(profile);
    }

    // Faza 2 (ranjiva): dohvaća pohranjeni username i konkatenira ga u SQL — second-order injection
    [HttpPost("second-order-attack")]
    public async Task<IActionResult> SecondOrderAttack(SecondOrderRequest request)
    {
        var result = await authDemoService.SecondOrderAttackAsync(request);
        return Ok(result);
    }

    // Faza 2 (sigurna): isti scenarij ali s parametriziranim upitom — injection nije moguć
    [HttpPost("second-order-safe")]
    public async Task<IActionResult> SecondOrderSafe(SecondOrderRequest request)
    {
        var result = await authDemoService.SecondOrderSafeAsync(request);
        return Ok(result);
    }
}
