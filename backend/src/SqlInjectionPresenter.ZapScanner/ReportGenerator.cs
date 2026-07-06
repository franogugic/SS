using System.Text;
using System.Text.Json;

namespace SqlInjectionPresenter.ZapScanner;

public static class ReportGenerator
{
    public static readonly List<SastFinding> KnownSastFindings =
    [
        new("Services/AuthDemoService.cs", "VulnerableLoginAsync",
            "csharp-commandtext-injection", "SQL injection — direktna konkatenacija Username i Password u WHERE klauzuli.", false),
        new("Services/AuthDemoService.cs", "UnionAttackAsync",
            "csharp-commandtext-injection", "SQL injection — UNION napad, payload konkateniran u LIKE klauzulu.", false),
        new("Services/AuthDemoService.cs", "ErrorAttackAsync",
            "csharp-commandtext-injection", "SQL injection — error-based, payload konkateniran u WHERE klauzulu.", false),
        new("Services/AuthDemoService.cs", "BlindBooleanAttackAsync",
            "csharp-commandtext-injection", "SQL injection — blind boolean, payload konkateniran u WHERE klauzulu.", false),
        new("Services/AuthDemoService.cs", "TimeBasedAttackAsync",
            "csharp-commandtext-injection", "SQL injection — time-based blind, payload konkateniran u WHERE klauzulu.", false),
        new("Repositories/UserRepository.cs", "FindUsersByStoredUsernameUnsafeAsync",
            "csharp-fromsqlraw-injection", "SECOND-ORDER SQL injection — pohranjeni username iz DB konkateniran u FromSqlRaw.", true),
    ];

    public static ComparisonReport Build(string targetUrl, List<ZapAlert> allAlerts)
    {
        var sqliAlerts = allAlerts
            .Where(IsSqliAlert)
            .Select(a => new DastFinding(a.Url, a.Method, a.Parameter, a.Attack, a.RiskDescription, a.PluginId))
            .ToList();

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
        File.WriteAllText(path, BuildHtml(report));
    }

    private static bool IsSqliAlert(ZapAlert a) =>
        a.PluginId is "40018" or "40019" or "40020" or "40021" or "40022" ||
        a.Name.Contains("SQL Injection", StringComparison.OrdinalIgnoreCase) ||
        a.Name.Contains("SQLi", StringComparison.OrdinalIgnoreCase);

    private static string BuildInsight(int dastCount, bool dastGotSecondOrder) =>
        dastGotSecondOrder
            ? "I SAST i DAST su detektirali second-order injection — rijedak slučaj ako DAST ima kontekst višekoračnog napada."
            : dastCount > 0
                ? "SAST (Semgrep) detektira SVE SQLi uključujući second-order statičkom analizom koda. " +
                  "DAST (OWASP ZAP) detektira klasični SQLi HTTP payloadima, ali PROPUŠTA second-order jer " +
                  "ne može povezati /store-profile i /second-order-attack kao jedan napadački lanac."
                : "DAST nije pronašao SQL injection. SAST je pronašao ranjivosti statičkom analizom koda. " +
                  "Moguće: ZAP scan nije bio dovoljno dubok ili backend nije bio dostupan iz ZAP kontejnera.";

    private static string BuildHtml(ComparisonReport r)
    {
        var sb = new StringBuilder();
        sb.AppendLine($$"""
            <!DOCTYPE html>
            <html lang="hr">
            <head>
              <meta charset="UTF-8">
              <meta name="viewport" content="width=device-width, initial-scale=1.0">
              <title>SAST vs DAST — SQL Injection Izvještaj</title>
              <link rel="preconnect" href="https://fonts.googleapis.com">
              <link href="https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700&display=swap" rel="stylesheet">
              <style>
                *, *::before, *::after { box-sizing: border-box; }
                body {
                  font-family: Inter, ui-sans-serif, system-ui, sans-serif;
                  margin: 0;
                  background: #f8faf6;
                  color: #14181f;
                  font-size: 14px;
                  line-height: 1.6;
                }
                .shell {
                  max-width: 1200px;
                  margin: 0 auto;
                  padding: 24px 20px;
                  display: flex;
                  flex-direction: column;
                  gap: 24px;
                }
                .header {
                  background: white;
                  border: 1px solid #e7e5e4;
                  border-radius: 10px;
                  padding: 24px;
                  box-shadow: 0 1px 3px rgba(0,0,0,0.06);
                }
                .header-label {
                  font-size: 12px;
                  font-weight: 600;
                  text-transform: uppercase;
                  letter-spacing: 0.05em;
                  color: #0f766e;
                  margin-bottom: 8px;
                }
                .header h1 {
                  margin: 0 0 6px 0;
                  font-size: 22px;
                  font-weight: 600;
                  color: #14181f;
                }
                .header-meta {
                  font-size: 12px;
                  color: #78716c;
                }
                .grid2 { display: grid; grid-template-columns: 1fr 1fr; gap: 20px; }
                .full { grid-column: 1 / -1; }
                .card {
                  background: white;
                  border: 1px solid #e7e5e4;
                  border-radius: 10px;
                  padding: 20px;
                  box-shadow: 0 1px 3px rgba(0,0,0,0.06);
                }
                .card-sast { border-top: 3px solid #be123c; }
                .card-dast { border-top: 3px solid #0f766e; }
                .card-summary { border-top: 3px solid #14181f; }
                .card h2 {
                  margin: 0 0 16px 0;
                  font-size: 14px;
                  font-weight: 600;
                  color: #14181f;
                  padding-bottom: 10px;
                  border-bottom: 1px solid #f5f5f4;
                }
                .stats-row { display: flex; gap: 24px; margin-bottom: 16px; flex-wrap: wrap; }
                .stat-box { text-align: center; min-width: 80px; }
                .stat-num { font-size: 28px; font-weight: 700; color: #14181f; line-height: 1; }
                .stat-lbl { font-size: 11px; color: #78716c; margin-top: 4px; }
                .insight {
                  background: #fefce8;
                  border-left: 3px solid #b45309;
                  padding: 12px 14px;
                  border-radius: 4px;
                  font-size: 13px;
                  color: #44403c;
                }
                table { width: 100%; border-collapse: collapse; margin-top: 4px; }
                th {
                  background: #f5f5f4;
                  color: #44403c;
                  font-size: 11px;
                  font-weight: 600;
                  text-transform: uppercase;
                  letter-spacing: 0.04em;
                  padding: 8px 10px;
                  text-align: left;
                }
                td { padding: 8px 10px; border-bottom: 1px solid #f5f5f4; font-size: 13px; }
                tr:last-child td { border-bottom: none; }
                tr:hover td { background: #fafaf9; }
                code {
                  background: #f5f5f4;
                  border: 1px solid #e7e5e4;
                  padding: 1px 5px;
                  border-radius: 4px;
                  font-size: 12px;
                  font-family: ui-monospace, monospace;
                }
                .badge {
                  display: inline-block;
                  padding: 2px 8px;
                  border-radius: 99px;
                  font-size: 11px;
                  font-weight: 600;
                }
                .badge-high { background: #ffe4e6; color: #be123c; }
                .badge-medium { background: #fef3c7; color: #b45309; }
                .badge-second { background: #dcfce7; color: #15803d; }
                .badge-direct { background: #fee2e2; color: #be123c; }
                .badge-rule { background: #ede9fe; color: #6d28d9; }
                .note { font-size: 12px; color: #78716c; margin-top: 12px; line-height: 1.5; }
                .empty { color: #78716c; font-size: 13px; padding: 12px 0; }
                @media (max-width: 700px) { .grid2 { grid-template-columns: 1fr; } .full { grid-column: 1; } }
              </style>
            </head>
            <body>
            <div class="shell">
            """);

        sb.AppendLine($"""
            <div class="header">
              <div class="header-label">SQL Injection Demo — Usporedni Izvještaj</div>
              <h1>SAST vs DAST analiza</h1>
              <div class="header-meta">
                Generirano: {r.GeneratedAt:yyyy-MM-dd HH:mm} UTC &nbsp;·&nbsp; Target: <code>{r.TargetUrl}</code>
              </div>
            </div>
            """);

        // Summary
        sb.AppendLine("<div class='grid2'>");
        sb.AppendLine("<div class='card card-summary full'>");
        sb.AppendLine("<h2>Sažetak</h2>");
        sb.AppendLine("<div class='stats-row'>");
        AddStat(sb, r.Summary.SastTotalSqli.ToString(), "SAST SQLi nalazi");
        AddStat(sb, r.Summary.SastSecondOrder.ToString(), "SAST second-order");
        AddStat(sb, r.Summary.DastTotalSqli.ToString(), "DAST SQLi nalazi");
        AddStat(sb, r.Summary.DastDetectedSecondOrder ? "DA" : "NE", "DAST second-order");
        sb.AppendLine("</div>");
        sb.AppendLine($"<div class='insight'><strong>Zaključak:</strong> {r.Summary.KeyInsight}</div>");
        sb.AppendLine("</div>");

        // SAST
        sb.AppendLine("<div class='card card-sast'>");
        sb.AppendLine("<h2>SAST — Semgrep</h2>");
        sb.AppendLine("<table><tr><th>Datoteka</th><th>Metoda</th><th>Pravilo</th><th>Vrsta</th></tr>");
        foreach (var f in r.SastFindings)
        {
            var kind = f.IsSecondOrder
                ? "<span class='badge badge-second'>Second-order</span>"
                : "<span class='badge badge-direct'>Direktni</span>";
            sb.AppendLine($"<tr><td><code>{f.File}</code></td><td><code>{f.Method}</code></td>" +
                          $"<td><span class='badge badge-rule'>{f.RuleId}</span></td><td>{kind}</td></tr>");
        }
        sb.AppendLine("</table>");
        sb.AppendLine("<p class='note'>Semgrep analizira izvorni kôd statički. Detektira second-order jer vidi <code>FromSqlRaw</code> s varijabilnim argumentom, neovisno odakle dolaze podaci.</p>");
        sb.AppendLine("</div>");

        // DAST
        sb.AppendLine("<div class='card card-dast'>");
        sb.AppendLine("<h2>DAST — OWASP ZAP</h2>");
        if (r.DastSqliFindings.Count == 0)
        {
            sb.AppendLine("<p class='empty'>ZAP nije pronašao SQL injection nalaze.</p>");
        }
        else
        {
            sb.AppendLine("<table><tr><th>URL</th><th>Metoda</th><th>Parametar</th><th>Risk</th></tr>");
            foreach (var f in r.DastSqliFindings)
            {
                var riskText = f.Risk ?? "Unknown";
                var riskClass = riskText.Contains("High", StringComparison.OrdinalIgnoreCase) ? "badge-high" : "badge-medium";
                sb.AppendLine($"<tr><td><code>{Truncate(f.Url, 55)}</code></td><td>{f.Method}</td>" +
                              $"<td><code>{f.Parameter}</code></td>" +
                              $"<td><span class='badge {riskClass}'>{riskText}</span></td></tr>");
            }
            sb.AppendLine("</table>");
        }
        sb.AppendLine("<p class='note'>DAST šalje stvarne HTTP zahtjeve. Propušta second-order jer testira <code>/store-profile</code> i <code>/second-order-attack</code> kao zasebne endpointe i ne može povezati taj napadački lanac.</p>");
        sb.AppendLine("</div>");

        // Svi DAST nalazi
        if (r.AllDastAlerts.Count > 0)
        {
            sb.AppendLine("<div class='card full'>");
            sb.AppendLine("<h2>Svi DAST nalazi</h2>");
            sb.AppendLine("<table><tr><th>Plugin</th><th>Naziv</th><th>Risk</th><th>URL</th><th>Parametar</th></tr>");
            foreach (var a in r.AllDastAlerts.OrderByDescending(x => x.RiskCode ?? "0"))
            {
                var riskClass = a.RiskCode == "3" ? "badge-high" : a.RiskCode == "2" ? "badge-medium" : "";
                sb.AppendLine($"<tr><td><code>{a.PluginId}</code></td><td>{a.Name}</td>" +
                              $"<td>{(string.IsNullOrEmpty(riskClass) ? a.RiskDescription ?? "-" : $"<span class='badge {riskClass}'>{a.RiskDescription ?? "-"}</span>")}</td>" +
                              $"<td><code>{Truncate(a.Url, 50)}</code></td><td>{a.Parameter}</td></tr>");
            }
            sb.AppendLine("</table>");
            sb.AppendLine("</div>");
        }

        sb.AppendLine("</div></div></body></html>");
        return sb.ToString();
    }

    private static void AddStat(StringBuilder sb, string value, string label)
    {
        sb.AppendLine($"<div class='stat-box'><div class='stat-num'>{value}</div><div class='stat-lbl'>{label}</div></div>");
    }

    private static string Truncate(string s, int max) =>
        s.Length <= max ? s : s[..max] + "…";
}
