using SqlInjectionPresenter.ZapScanner;

//ujesto da koristimo hardkodirane url uzimamo ih iz terminala
var zapUrl = GetArg(args, "--zap-url") ?? "http://localhost:8090";
var targetUrl = GetArg(args, "--target") ?? "http://localhost:5000";
var outputDir = GetArg(args, "--out") ?? "./reports";

//kreiramo directory u koji cemo spremat outout
Directory.CreateDirectory(outputDir);

//timer da ako potraje preko 30 min da se prekin... da ne ode u beskonacu
using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(30));
//instancirtamo ZapClienta i saljemo mu zapUrl
var zap = new ZapClient(zapUrl);

Console.WriteLine("=================================================================");
Console.WriteLine("  SQL Injection Presenter — DAST scan via OWASP ZAP REST API");
Console.WriteLine("=================================================================");
Console.WriteLine($"  ZAP:    {zapUrl}");
Console.WriteLine($"  Target: {targetUrl}");
Console.WriteLine($"  Output: {outputDir}");
Console.WriteLine();

// ceka da se ZAP pokrene da onda moze pocet slat zahtjeve
// ZAP se pinga svako 3 sek dok ne odgovori
await zap.WaitForZapAsync();

Console.WriteLine("[1/4] Postavljanje second-order payloada u bazu (demonstracija)...");
try
{
    //kreiramo klijenrta preko kojeg cemo slat zahtjeve na nas backend
    using var setupHttp = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
    //usernamei su SQLi payloadi koje cemo pohranjivat u bazu
    var payloads = new[]
    {
        new { username = "admin'--", note = "Second-order SQLi payload #1" },
        new { username = "' OR '1'='1", note = "Second-order SQLi payload #2" },
        new { username = "test'; WAITFOR DELAY '0:0:2'--", note = "Second-order time-based payload" },
    };

    foreach (var payload in payloads)
    {
        //pretvara payload objekta u JSON string i sprtema ha u HTTP body zahtjeba..
        var content = new StringContent(
            System.Text.Json.JsonSerializer.Serialize(payload),
            System.Text.Encoding.UTF8,
            "application/json");
        //salje post req s onim payloadaom
        var resp = await setupHttp.PostAsync($"{targetUrl}/api/demo/store-profile", content);
        var status = resp.IsSuccessStatusCode ? "OK" : $"GREŠKA {(int)resp.StatusCode}";
        Console.WriteLine($"  Pohranjen payload '{payload.username}': {status}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"  UPOZORENJE: Nije moguće pohraniti payloade — {ex.Message}");
    Console.WriteLine("  (Backend možda nije pokrenut. Nastavljam sa ZAP scanom.)");
}

// faza 2: dodaj endpointe u ZAP sitemap
Console.WriteLine();
Console.WriteLine("[2/4] Dodavanje endpointa u ZAP sitemap...");
var getEndpoints = new[]
{
    $"{targetUrl}/api/demo/users",
    $"{targetUrl}/api/demo/search?q=test",
    $"{targetUrl}/api/demo/login?username=test&password=test",
    $"{targetUrl}/api/demo/user?username=test",
    $"{targetUrl}/api/demo/stored-profiles",
};

//dodavanje endpointa
foreach (var ep in getEndpoints)
{
    await zap.AccessUrlAsync(ep);
    Console.WriteLine($"  + {ep}");
}

// faza 3: Active Scan
Console.WriteLine("[3/4] Pokretanje Active Scana (SQL injection, XSS, itd.)...");
Console.WriteLine("  NAPOMENA: Active scan može trajati 10-30 minuta.");
//pokrece active scan - toe dio di zap anpada nasu aplikaciju
var activeScanId = await zap.StartActiveScanAsync(targetUrl);
await zap.WaitForActiveScanAsync(activeScanId);
Console.WriteLine("  Active scan završen.");

Console.WriteLine();
Console.WriteLine("[4/4] Dohvaćanje nalaza...");
//dohvaca sve balaze koej je ZAP pronasa
var alerts = await zap.GetAlertsAsync(targetUrl);
Console.WriteLine($"  Ukupno ZAP nalaza: {alerts.Count}");

//filtiramo sve sql nalaze
var sqliAlerts = alerts.Where(a =>
    a.PluginId is "40018" or "40019" or "40020" or "40021" or "40022" ||
    a.Name.Contains("SQL Injection", StringComparison.OrdinalIgnoreCase)).ToList();

Console.WriteLine($"  SQL Injection nalazi: {sqliAlerts.Count}");

    // projverava second order koej nece detektirat
var secondOrderDetected = sqliAlerts.Any(a =>
    a.Url.Contains("second-order", StringComparison.OrdinalIgnoreCase));
Console.WriteLine($"  Second-order detektiran: {(secondOrderDetected ? "DA (iznimno!)" : "NE (očekivano)")}");

Console.WriteLine();
Console.WriteLine("Generiranje usporednog izvještaja...");
//generiramo usporedbu dasta i sasta
var report = ReportGenerator.Build(targetUrl, alerts);

var htmlPath = Path.Combine(outputDir, "sast-vs-dast-report.html");
var jsonPath = Path.Combine(outputDir, "sast-vs-dast-report.json");

ReportGenerator.SaveHtml(report, htmlPath);
ReportGenerator.SaveJson(report, jsonPath);



// ── Ispis DAST sažetka ───────────────────────────────────────────────────────
Console.WriteLine();
Console.WriteLine("=================================================================");
Console.WriteLine("  DAST REZULTATI — OWASP ZAP");
Console.WriteLine("=================================================================");
Console.WriteLine();
Console.WriteLine($"  Ukupno nalaza     : {alerts.Count}");
Console.WriteLine($"  SQL Injection     : {sqliAlerts.Count}");
Console.WriteLine($"  Second-order      : {(secondOrderDetected ? "DA" : "NE — ZAP ne može modelirati višekoračni napad")}");
Console.WriteLine();

//ispisuje se svaki nalaz u erminal
if (sqliAlerts.Count > 0)
{
    Console.WriteLine("─── SQL INJECTION NALAZI ───────────────────────────────────────────");
    var seen = new HashSet<string>();
    var broj = 1;
    foreach (var a in sqliAlerts)
    {
        var key = $"{a.Url}|{a.Parameter}|{a.PluginId}";
        if (!seen.Add(key)) continue;
        Console.WriteLine($"  Nalaz #{broj++}");
        Console.WriteLine($"  {'─' * 40}");
        Console.WriteLine($"  Plugin    : {a.PluginId} — {a.Name}");
        Console.WriteLine($"  Metoda    : {a.Method}");
        Console.WriteLine($"  URL       : {a.Url}");
        Console.WriteLine($"  Parametar : {a.Parameter}");
        Console.WriteLine($"  Attack    : {a.Attack}");
        Console.WriteLine();
    }
}
//nalazi koji nisu SQLi
Console.WriteLine("─── OSTALI NALAZI ──────────────────────────────────────────────────");
foreach (var a in alerts.Where(a => !sqliAlerts.Contains(a)))
    Console.WriteLine($"  [{a.PluginId}] {a.Name} — {a.Url}");

Console.WriteLine();
Console.WriteLine($"  Izvještaji:");
Console.WriteLine($"    HTML: {Path.GetFullPath(htmlPath)}");
Console.WriteLine($"    JSON: {Path.GetFullPath(jsonPath)}");

Console.WriteLine();
Console.WriteLine("=================================================================");
Console.WriteLine();

// args je niz svega nakon dotnet run-a
//npr ["--zap-url", "http://localhost:8090"].
static string? GetArg(string[] args, string key)
{
    var idx = Array.IndexOf(args, key);
    return idx >= 0 && idx + 1 < args.Length ? args[idx + 1] : null;
}
