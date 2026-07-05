using System.Text;
using System.Text.Json;

namespace SqlInjectionPresenter.ZapScanner;

/// <summary>
/// Generira usporedni SAST vs DAST izvještaj kao HTML i JSON.
/// </summary>
public static class ReportGenerator
{
    // Poznati SAST nalazi iz SonarQube analize (pravilo S3649).
    // Ovi su ručno mappirani iz koda — u stvarnoj integraciji bi se dohvatili
    // iz SonarQube Web API-ja (/api/issues/search).
    public static readonly List<SastFinding> KnownSastFindings =
    [
        new("Services/AuthDemoService.cs", "VulnerableLoginAsync",
            "S3649", "SQL injection — direktna konkatenacija Username i Password u WHERE klauzuli.", false),
        new("Services/AuthDemoService.cs", "UnionAttackAsync",
            "S3649", "SQL injection — UNION napad, Payload konkateniran u LIKE klauzulu.", false),
        new("Services/AuthDemoService.cs", "ErrorAttackAsync",
            "S3649", "SQL injection — error-based, Payload konkateniran u WHERE klauzulu.", false),
        new("Services/AuthDemoService.cs", "BlindBooleanAttackAsync",
            "S3649", "SQL injection — blind boolean, Payload konkateniran u WHERE klauzulu.", false),
        new("Services/AuthDemoService.cs", "TimeBasedAttackAsync",
            "S3649", "SQL injection — time-based blind, Payload konkateniran u WHERE klauzulu.", false),
        new("Repositories/UserRepository.cs", "FindUsersByStoredUsernameUnsafeAsync",
            "S3649", "SECOND-ORDER SQL injection — pohranjeni username iz DB konkateniran u FromSqlRaw.", true),
    ];

    public static ComparisonReport Build(string targetUrl, List<ZapAlert> allAlerts)
    {
        var sqliAlerts = allAlerts
            .Where(a => IsSqliAlert(a))
            .Select(a => new DastFinding(a.Url, a.Method, a.Parameter, a.Attack, a.RiskDescription, a.PluginId))
            .ToList();

        // ZAP ne može detektirati second-order jer ne zna da /store-profile + /second-order-attack
        // tvore napadački lanac — svaki endpoint testira nezavisno.
        var dastDetectedSecondOrder = sqliAlerts
            .Any(f => f.Url.Contains("second-order", StringComparison.OrdinalIgnoreCase));

        var summary = new ComparisonSummary(
            SastTotalSqli: KnownSastFindings.Count,
            SastSecondOrder: KnownSastFindings.Count(f => f.IsSecondOrder),
            DastTotalSqli: sqliAlerts.Count,
            DastDetectedSecondOrder: dastDetectedSecondOrder,
            KeyInsight: BuildInsight(sqliAlerts.Count, dastDetectedSecondOrder));

        return new ComparisonReport(DateTime.UtcNow, targetUrl, KnownSastFindings, sqliAlerts, allAlerts, summary);
    }

    public static void SaveJson(ComparisonReport report, string path)
    {
        var json = JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(path, json);
    }

    public static void SaveHtml(ComparisonReport report, string path)
    {
        var html = BuildHtml(report);
        File.WriteAllText(path, html);
    }

    private static bool IsSqliAlert(ZapAlert a) =>
        a.PluginId is "40018" or "40019" or "40020" or "40021" or "40022" ||
        a.Name.Contains("SQL Injection", StringComparison.OrdinalIgnoreCase) ||
        a.Name.Contains("SQLi", StringComparison.OrdinalIgnoreCase);

    private static string BuildInsight(int dastCount, bool dastGotSecondOrder) =>
        dastGotSecondOrder
            ? "I SAST i DAST su detektirali second-order injection — rijedak slučaj ako DAST ima kontekst višekoračnog napada."
            : dastCount > 0
                ? "SAST detektira SVE SQLi uključujući second-order (statička analiza koda). " +
                  "DAST detektira klasični SQLi (HTTP payloadi), ali PROPUŠTA second-order jer " +
                  "ne može povezati /store-profile i /second-order-attack kao jedan napadački lanac."
                : "DAST nije pronašao SQL injection. SAST je pronašao ranjivosti statičkom analizom koda. " +
                  "Moguće: ZAP scan nije bio dovoljno dubok, backend nije dostupan iz ZAP kontejnera, " +
                  "ili endpointi zahtijevaju specifičan Content-Type/body format koji ZAP nije poslao.";

    private static string BuildHtml(ComparisonReport r)
    {
        var sb = new StringBuilder();
        sb.AppendLine("""
            <!DOCTYPE html>
            <html lang="hr">
            <head>
              <meta charset="UTF-8">
              <meta name="viewport" content="width=device-width, initial-scale=1.0">
              <title>SAST vs DAST Usporedni Izvještaj — SQL Injection</title>
              <style>
                body { font-family: 'Segoe UI', sans-serif; margin: 0; padding: 20px; background: #f5f5f5; color: #333; }
                h1 { color: #1a1a2e; }
                h2 { color: #16213e; border-bottom: 2px solid #0f3460; padding-bottom: 6px; }
                .meta { color: #666; font-size: 0.9em; margin-bottom: 20px; }
                .grid { display: grid; grid-template-columns: 1fr 1fr; gap: 20px; margin-bottom: 30px; }
                .card { background: white; border-radius: 8px; padding: 20px; box-shadow: 0 2px 8px rgba(0,0,0,0.1); }
                .sast { border-top: 4px solid #e94560; }
                .dast { border-top: 4px solid #0f3460; }
                .summary { border-top: 4px solid #533483; grid-column: 1 / -1; }
                table { width: 100%; border-collapse: collapse; margin-top: 10px; font-size: 0.85em; }
                th { background: #1a1a2e; color: white; padding: 8px; text-align: left; }
                td { padding: 7px 8px; border-bottom: 1px solid #eee; }
                tr:hover { background: #f9f9f9; }
                .badge { padding: 2px 8px; border-radius: 12px; font-size: 0.8em; font-weight: bold; }
                .high { background: #ffcccc; color: #c00; }
                .medium { background: #ffe0b2; color: #e65100; }
                .second-order { background: #e8f5e9; color: #2e7d32; }
                .insight { background: #fff3e0; border-left: 4px solid #ff9800; padding: 15px; border-radius: 4px; margin-top: 10px; }
                .stat { font-size: 2em; font-weight: bold; color: #0f3460; }
                .stat-label { font-size: 0.85em; color: #666; }
                .stats-row { display: flex; gap: 30px; margin: 15px 0; }
                .stat-box { text-align: center; }
                code { background: #f0f0f0; padding: 1px 4px; border-radius: 3px; font-size: 0.85em; }
              </style>
            </head>
            <body>
            """);

        sb.AppendLine($"<h1>SAST vs DAST — SQL Injection Usporedba</h1>");
        sb.AppendLine($"<div class='meta'>Generirano: {r.GeneratedAt:yyyy-MM-dd HH:mm:ss} UTC | Target: <code>{r.TargetUrl}</code></div>");

        // Summary card
        sb.AppendLine("<div class='grid'>");
        sb.AppendLine("<div class='card summary'>");
        sb.AppendLine("<h2>Sažetak</h2>");
        sb.AppendLine("<div class='stats-row'>");
        AddStat(sb, r.Summary.SastTotalSqli.ToString(), "SAST SQLi nalazi");
        AddStat(sb, r.Summary.SastSecondOrder.ToString(), "SAST second-order");
        AddStat(sb, r.Summary.DastTotalSqli.ToString(), "DAST SQLi nalazi");
        AddStat(sb, r.Summary.DastDetectedSecondOrder ? "DA" : "NE", "DAST detektirao second-order");
        sb.AppendLine("</div>");
        sb.AppendLine($"<div class='insight'><strong>Ključni zaključak:</strong> {r.Summary.KeyInsight}</div>");
        sb.AppendLine("</div>");

        // SAST card
        sb.AppendLine("<div class='card sast'>");
        sb.AppendLine("<h2>SAST — SonarQube (S3649)</h2>");
        sb.AppendLine("<table><tr><th>Datoteka</th><th>Metoda</th><th>Vrsta</th></tr>");
        foreach (var f in r.SastFindings)
        {
            var badge = f.IsSecondOrder
                ? "<span class='badge second-order'>Second-order</span>"
                : "<span class='badge high'>Direct</span>";
            sb.AppendLine($"<tr><td><code>{f.File}</code></td><td><code>{f.Method}</code></td><td>{badge}</td></tr>");
        }
        sb.AppendLine("</table>");
        sb.AppendLine("<p style='font-size:0.85em;color:#666;margin-top:10px;'>" +
                      "SAST analizira izvorni kôd i prati tok podataka statički. " +
                      "Detektira second-order jer vidi <code>FromSqlRaw</code> s varijablom — " +
                      "neovisno o tome odakle varijabla dolazi (HTTP, baza, file).</p>");
        sb.AppendLine("</div>");

        // DAST card
        sb.AppendLine("<div class='card dast'>");
        sb.AppendLine("<h2>DAST — OWASP ZAP</h2>");
        if (r.DastSqliFindings.Count == 0)
        {
            sb.AppendLine("<p>ZAP nije pronašao SQL injection nalaze.</p>");
            sb.AppendLine("<p style='font-size:0.85em;color:#666;'>Mogući razlozi: " +
                          "JSON API zahtijeva Content-Type: application/json koji ZAP možda nije slao za sve endpointe; " +
                          "ZAP active scan plugin za SQLi (40018) nije uključen; " +
                          "backend nije bio dostupan iz ZAP Docker kontejnera.</p>");
        }
        else
        {
            sb.AppendLine("<table><tr><th>URL</th><th>Metoda</th><th>Parametar</th><th>Risk</th></tr>");
            foreach (var f in r.DastSqliFindings)
            {
                var riskText = f.Risk ?? "Unknown";
                var risk = riskText.Contains("High", StringComparison.OrdinalIgnoreCase) ? "high" : "medium";
                sb.AppendLine($"<tr><td><code>{Truncate(f.Url, 60)}</code></td><td>{f.Method}</td>" +
                              $"<td><code>{f.Parameter}</code></td>" +
                              $"<td><span class='badge {risk}'>{riskText}</span></td></tr>");
            }
            sb.AppendLine("</table>");
        }

        sb.AppendLine("<p style='font-size:0.85em;color:#666;margin-top:10px;'>" +
                      "DAST šalje stvarne HTTP zahtjeve i promatra odgovore. " +
                      "Za second-order SQLi: ZAP šalje payload u <code>/store-profile</code>, " +
                      "ali napad se aktivira tek u <code>/second-order-attack</code> — " +
                      "ZAP ne zna da mora pozvati drugi endpoint da aktivira injection.</p>");
        sb.AppendLine("</div>");

        // Svi DAST nalazi
        if (r.AllDastAlerts.Count > 0)
        {
            sb.AppendLine("<div class='card' style='grid-column: 1 / -1;'>");
            sb.AppendLine("<h2>Svi DAST nalazi</h2>");
            sb.AppendLine("<table><tr><th>Plugin</th><th>Naziv</th><th>Risk</th><th>URL</th><th>Parametar</th></tr>");
            foreach (var a in r.AllDastAlerts.OrderByDescending(x => x.RiskCode ?? "0"))
            {
                var risk = a.RiskCode == "3" ? "high" : a.RiskCode == "2" ? "medium" : "";
                sb.AppendLine($"<tr><td>{a.PluginId}</td><td>{a.Name}</td>" +
                              $"<td><span class='badge {risk}'>{a.RiskDescription ?? "-"}</span></td>" +
                              $"<td><code>{Truncate(a.Url, 55)}</code></td><td>{a.Parameter}</td></tr>");
            }
            sb.AppendLine("</table>");
            sb.AppendLine("</div>");
        }

        sb.AppendLine("</div>"); // grid
        sb.AppendLine("</body></html>");
        return sb.ToString();
    }

    private static void AddStat(StringBuilder sb, string value, string label)
    {
        sb.AppendLine($"<div class='stat-box'><div class='stat'>{value}</div><div class='stat-label'>{label}</div></div>");
    }

    private static string Truncate(string s, int max) =>
        s.Length <= max ? s : s[..max] + "…";
}
