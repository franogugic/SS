using SqlInjectionPresenter.Api.DTOs;
using SqlInjectionPresenter.Api.Models;
using SqlInjectionPresenter.Api.Repositories;
using System.Diagnostics;

namespace SqlInjectionPresenter.Api.Services;

public class AuthDemoService(IUserRepository users) : IAuthDemoService
{
    public async Task<IReadOnlyList<DemoUserDto>> GetUsersAsync()
    {
        var allUsers = await users.GetAllAsync();
        return allUsers.Select(ToDto).ToList();
    }

    public async Task<LoginResultDto> VulnerableLoginAsync(LoginRequest request)
    {
        var sql = $"""
            SELECT [Id], [Username], [Password], [FullName], [Role]
            FROM [Users]
            WHERE [Username] = '{request.Username}' AND [Password] = '{request.Password}'
            """;

        var matchedUsers = await users.FindWithUnsafeSqlAsync(sql);
        var message = matchedUsers.Count > 0
            ? "Prijava je prosla jer je unos promijenio logiku SQL upita."
            : "Prijava nije prosla.";

        return new LoginResultDto(
            matchedUsers.Count > 0,
            "Ranjivi raw SQL",
            message,
            sql,
            matchedUsers.Select(ToDto).ToList());
    }

    public async Task<LoginResultDto> SafeLoginAsync(LoginRequest request)
    {
        var user = await users.FindWithSafeQueryAsync(request.Username, request.Password);
        var safeSql = """
            SELECT TOP(1) [Id], [Username], [Password], [FullName], [Role]
            FROM [Users]
            WHERE [Username] = @username AND [Password] = @password
            """;

        var matchedUsers = user is null ? [] : new[] { ToDto(user) };
        var message = user is null
            ? "Prijava nije prosla jer se unos tretira kao obican podatak."
            : "Prijava je prosla s ispravnim korisnickim podacima.";

        return new LoginResultDto(
            user is not null,
            "Sigurni EF upit",
            message,
            safeSql,
            matchedUsers);
    }

    public async Task<ScenarioResultDto> UnionAttackAsync(ScenarioRequest request)
    {
        var sql = $"""
            SELECT [Id], [Username], [Password], [FullName], [Role]
            FROM [Users]
            WHERE [Username] LIKE '%{request.Payload}%'
            """;

        var watch = Stopwatch.StartNew();
        var matchedUsers = await users.FindWithUnsafeSqlAsync(sql);
        watch.Stop();

        return new ScenarioResultDto(
            matchedUsers.Count > 0,
            "UNION izvlačenje podataka",
            "Napadač kroz polje za pretragu ubacuje UNION SELECT i dobiva zapise iz tablice Users.",
            sql,
            null,
            watch.ElapsedMilliseconds,
            matchedUsers.Select(ToDto).ToList());
    }

    public async Task<ScenarioResultDto> UnionSafeAsync(ScenarioRequest request)
    {
        var safeSql = """
            SELECT [Id], [Username], [Password], [FullName], [Role]
            FROM [Users]
            WHERE [Username] LIKE '%' + @payload + '%'
            """;

        var watch = Stopwatch.StartNew();
        var matchedUsers = await users.SearchByUsernameSafeAsync(request.Payload);
        watch.Stop();

        return new ScenarioResultDto(
            matchedUsers.Count > 0,
            "Sigurni UNION scenarij",
            "Payload se koristi kao vrijednost parametra, pa UNION ostaje tekst za pretragu i ne izvršava se.",
            safeSql,
            null,
            watch.ElapsedMilliseconds,
            matchedUsers.Select(ToDto).ToList());
    }

    public async Task<ScenarioResultDto> ErrorAttackAsync(ScenarioRequest request)
    {
        var sql = $"""
            SELECT [Id], [Username], [Password], [FullName], [Role]
            FROM [Users]
            WHERE [Username] = '{request.Payload}'
            """;

        var watch = Stopwatch.StartNew();

        try
        {
            var matchedUsers = await users.FindWithUnsafeSqlAsync(sql);
            watch.Stop();

            return new ScenarioResultDto(
                false,
                "Error-based napad",
                "Upit se izvršio bez greške. Za error-based demo ubaci payload koji namjerno lomi SQL sintaksu.",
                sql,
                null,
                watch.ElapsedMilliseconds,
                matchedUsers.Select(ToDto).ToList());
        }
        catch (Exception ex)
        {
            watch.Stop();

            return new ScenarioResultDto(
                true,
                "Error-based napad",
                "Baza je vratila grešku. U stvarnoj aplikaciji takva poruka može otkriti strukturu upita ili baze.",
                sql,
                ex.Message,
                watch.ElapsedMilliseconds,
                []);
        }
    }

    public async Task<ScenarioResultDto> ErrorSafeAsync(ScenarioRequest request)
    {
        var safeSql = """
            SELECT [Id], [Username], [Password], [FullName], [Role]
            FROM [Users]
            WHERE [Username] = @payload
        """;

        var watch = Stopwatch.StartNew();
        var matchedUsers = await users.FindByUsernameSafeAsync(request.Payload);
        watch.Stop();
        var isQuoteOnly = request.Payload == "'";
        var message = matchedUsers.Count > 0
            ? "Upit je sigurno izvršen s parametrom i pronašao je korisnika."
            : isQuoteOnly
                ? "Jedan navodnik je obična vrijednost parametra, pa SQL sintaksa ostaje ispravna i baza ne vraća grešku."
                : "Upit je sigurno izvršen s parametrom, ali nema korisnika s tom vrijednošću.";

        return new ScenarioResultDto(
            matchedUsers.Count > 0,
            "Sigurni error scenarij",
            message,
            safeSql,
            null,
            watch.ElapsedMilliseconds,
            matchedUsers.Select(ToDto).ToList());
    }

    public async Task<ScenarioResultDto> BlindBooleanAttackAsync(ScenarioRequest request)
    {
        var sql = $"""
            SELECT [Id], [Username], [Password], [FullName], [Role]
            FROM [Users]
            WHERE [Username] = '{request.Payload}'
            """;

        var watch = Stopwatch.StartNew();
        var matchedUsers = await users.FindWithUnsafeSqlAsync(sql);
        watch.Stop();

        var success = matchedUsers.Count > 0;
        var message = success
            ? "Aplikacija prikazuje isti odgovor kao da korisnik postoji. Napadač iz toga zaključuje da je pretpostavka u payloadu točna."
            : "Aplikacija prikazuje odgovor kao da korisnik nije pronađen. Napadač iz toga zaključuje da pretpostavka nije točna.";
        var databaseMessage = success
            ? "Vidljivi odgovor aplikacije: Korisnik postoji"
            : "Vidljivi odgovor aplikacije: Korisnik nije pronađen";

        return new ScenarioResultDto(
            success,
            "Blind boolean napad",
            message,
            sql,
            databaseMessage,
            watch.ElapsedMilliseconds,
            matchedUsers.Select(ToDto).ToList());
    }

    public async Task<ScenarioResultDto> BlindBooleanSafeAsync(ScenarioRequest request)
    {
        var safeSql = """
            SELECT COUNT(*)
            FROM [Users]
            WHERE [Username] = @payload
            """;

        var watch = Stopwatch.StartNew();
        var matchedUsers = await users.FindByUsernameSafeAsync(request.Payload);
        watch.Stop();
        var message = matchedUsers.Count > 0
            ? "Upit je sigurno izvršen s parametrom i pronašao je korisnika."
            : "Cijeli payload se uspoređuje kao korisničko ime. Ne postaje uvjet nad lozinkom, pa nema curenja informacije.";
        var databaseMessage = matchedUsers.Count > 0
            ? "Vidljivi odgovor aplikacije: Korisnik postoji"
            : "Vidljivi odgovor aplikacije: Korisnik nije pronađen";

        return new ScenarioResultDto(
            matchedUsers.Count > 0,
            "Sigurni blind boolean scenarij",
            message,
            safeSql,
            databaseMessage,
            watch.ElapsedMilliseconds,
            matchedUsers.Select(ToDto).ToList());
    }

    public async Task<ScenarioResultDto> TimeBasedAttackAsync(ScenarioRequest request)
    {
        var sql = $"""
            SELECT [Id], [Username], [Password], [FullName], [Role]
            FROM [Users]
            WHERE [Username] = '{request.Payload}'
            """;

        var watch = Stopwatch.StartNew();
        var matchedUsers = await users.FindWithUnsafeSqlAsync(sql);
        watch.Stop();

        var success = watch.ElapsedMilliseconds >= 1800;
        var message = success
            ? "Odgovor je kasnio jer je payload izvršio WAITFOR DELAY."
            : "Nema vidljivog kašnjenja jer payload nije izazvao WAITFOR DELAY.";

        return new ScenarioResultDto(
            success,
            "Time-based blind napad",
            message,
            sql,
            null,
            watch.ElapsedMilliseconds,
            matchedUsers.Select(ToDto).ToList());
    }

    public async Task<ScenarioResultDto> TimeBasedSafeAsync(ScenarioRequest request)
    {
        var safeSql = """
            SELECT [Id], [Username], [Password], [FullName], [Role]
            FROM [Users]
            WHERE [Username] = @payload
            """;

        var watch = Stopwatch.StartNew();
        var matchedUsers = await users.FindByUsernameSafeAsync(request.Payload);
        watch.Stop();

        return new ScenarioResultDto(
            matchedUsers.Count > 0,
            "Sigurni time-based scenarij",
            "Payload se ne izvršava, pa WAITFOR DELAY nikada ne postaje dio SQL naredbe.",
            safeSql,
            null,
            watch.ElapsedMilliseconds,
            matchedUsers.Select(ToDto).ToList());
    }

    private static DemoUserDto ToDto(AppUser user) =>
        new(user.Id, user.Username, user.Password, user.FullName, user.Role);
}
