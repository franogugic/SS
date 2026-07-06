using System.Text.Json;

namespace SqlInjectionPresenter.ZapScanner;

public sealed class ZapClient(string zapProxyUrl)
{
    // mapira JSON u C#
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    // HttpClient konfiguriran da koristi ZAP kao proxy
    private readonly HttpClient http = new(new HttpClientHandler
    {
        Proxy = new System.Net.WebProxy(zapProxyUrl),
        UseProxy = true
    })
    // timeout jer neki pozivi mogu trajati duže
    { Timeout = TimeSpan.FromSeconds(60) };

    // ZAP API se poziva kroz proxy s hostom "zap"
    private const string ZapApiHost = "http://zap";
    
    // pokreće active scan — ova faza testira ranjivosti
    public async Task<string> StartActiveScanAsync(string targetUrl)
    {
        var url = $"{ZapApiHost}/JSON/ascan/action/scan/?url={Uri.EscapeDataString(targetUrl)}&recurse=true&inScopeOnly=false";
        var json = await GetJsonAsync(url);
        var result = JsonSerializer.Deserialize<ZapScanStarted>(json, JsonOpts);
        return result?.ScanId ?? "0";
    }

    public async Task WaitForActiveScanAsync(string scanId)
    {
        while (true)
        {
            var url = $"{ZapApiHost}/JSON/ascan/view/status/?scanId={scanId}";
            var json = await GetJsonAsync(url);
            var result = JsonSerializer.Deserialize<ZapScanStatus>(json, JsonOpts);
            if (int.TryParse(result?.Status, out var pct) && pct >= 100) break;
            await Task.Delay(4000);
        }
    }
    
    // dohvaća sve rezultate nakon scana
    public async Task<List<ZapAlert>> GetAlertsAsync(string baseUrl)
    {
        var url = $"{ZapApiHost}/JSON/core/view/alerts/?baseurl={Uri.EscapeDataString(baseUrl)}&start=0&count=500";
        var json = await GetJsonAsync(url);
        var result = JsonSerializer.Deserialize<ZapAlertsResponse>(json, JsonOpts);
        return result?.Alerts ?? [];
    }


    // stavlja taj url u zap
    public async Task AccessUrlAsync(string targetUrl)
    {
        var url = $"{ZapApiHost}/JSON/core/action/accessUrl/?url={Uri.EscapeDataString(targetUrl)}";
        await GetJsonAsync(url);
    }

    //dohvaca odgovore od ZAP-a
    public async Task WaitForZapAsync()
    {
        Console.WriteLine("Čekam na ZAP daemon...");
        while (true)
        {
            try
            {
                var resp = await http.GetAsync($"{ZapApiHost}/JSON/core/view/version/");
                if (resp.IsSuccessStatusCode)
                {
                    Console.WriteLine("ZAP je spreman.");
                    return;
                }
            }
            catch { /* ZAP još nije pokrenut */ }
            await Task.Delay(3000);
        }
    }

    private async Task<string> GetJsonAsync(string url)
    {
        var response = await http.GetAsync(url);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }
}
