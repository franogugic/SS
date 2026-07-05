using System.Text.Json.Serialization;

namespace SqlInjectionPresenter.ZapScanner;

public record ZapScanStatus(
    [property: JsonPropertyName("status")] string Status);

public record ZapScanStarted(
    [property: JsonPropertyName("scan")] string ScanId);

public record ZapAlert(
    [property: JsonPropertyName("pluginId")] string PluginId,
    [property: JsonPropertyName("alert")] string Name,
    [property: JsonPropertyName("riskcode")] string RiskCode,
    [property: JsonPropertyName("riskdesc")] string RiskDescription,
    [property: JsonPropertyName("confidence")] string Confidence,
    [property: JsonPropertyName("url")] string Url,
    [property: JsonPropertyName("method")] string Method,
    [property: JsonPropertyName("param")] string Parameter,
    [property: JsonPropertyName("attack")] string Attack,
    [property: JsonPropertyName("evidence")] string Evidence,
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("solution")] string Solution);

public record ZapAlertsResponse(
    [property: JsonPropertyName("alerts")] List<ZapAlert> Alerts);

public record SastFinding(
    string File,
    string Method,
    string RuleId,
    string Description,
    bool IsSecondOrder);

public record DastFinding(
    string Url,
    string Method,
    string Parameter,
    string Attack,
    string Risk,
    string PluginId);

public record ComparisonReport(
    DateTime GeneratedAt,
    string TargetUrl,
    List<SastFinding> SastFindings,
    List<DastFinding> DastSqliFindings,
    List<ZapAlert> AllDastAlerts,
    ComparisonSummary Summary);

public record ComparisonSummary(
    int SastTotalSqli,
    int SastSecondOrder,
    int DastTotalSqli,
    bool DastDetectedSecondOrder,
    string KeyInsight);
