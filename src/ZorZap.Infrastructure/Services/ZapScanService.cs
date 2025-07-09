using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using ZorZap.Application.Interfaces;

namespace ZorZap.Infrastructure.Services;

public class ZapScanService : IZapScanService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _zapApiBaseUrl;

    public ZapScanService(IConfiguration configuration)
    {
        _apiKey = configuration["ZapApiKey"] ?? throw new InvalidOperationException("Clé API ZAP non configurée !");
        _zapApiBaseUrl = "http://localhost:8080/JSON";
        // On augmente le timeout car un scan peut être très long
        _httpClient = new HttpClient { Timeout = TimeSpan.FromHours(24) };
    }

    public async Task StartFullScanAsync(string targetUrl, string reportName)
    {
        Console.WriteLine($"--- Début du Full Scan sur {targetUrl} pour le rapport '{reportName}' ---");

        var spiderScanId = await StartSpider(targetUrl);
        await WaitForTaskCompletion($"{_zapApiBaseUrl}/spider/view/status/?scanId={spiderScanId}", "Spider");

        var activeScanId = await StartActiveScan(targetUrl);
        await WaitForTaskCompletion($"{_zapApiBaseUrl}/ascan/view/status/?scanId={activeScanId}", "Active Scan");

        // On ajoute une courte pause pour s'assurer que ZAP est prêt.
        Console.WriteLine("Scan terminé. Attente de 5 secondes pour la finalisation des rapports ZAP...");
        await Task.Delay(5000);

        await GenerateReports(reportName);
        
        Console.WriteLine($"--- Full Scan pour le rapport '{reportName}' terminé ! ---");
    }

    private async Task<string> StartSpider(string targetUrl)
    {
        var url = $"{_zapApiBaseUrl}/spider/action/scan/?apikey={_apiKey}&url={Uri.EscapeDataString(targetUrl)}";
        var response = await _httpClient.GetFromJsonAsync<ZapScanResponse>(url);
        Console.WriteLine($"Spider lancé avec l'ID : {response?.Scan}");
        return response?.Scan ?? throw new InvalidOperationException("Impossible de démarrer le Spider.");
    }

    private async Task<string> StartActiveScan(string targetUrl)
    {
        var url = $"{_zapApiBaseUrl}/ascan/action/scan/?apikey={_apiKey}&url={Uri.EscapeDataString(targetUrl)}&recurse=true&inScopeOnly=false";
        var response = await _httpClient.GetFromJsonAsync<ZapScanResponse>(url);
        Console.WriteLine($"Active Scan lancé avec l'ID : {response?.Scan}");
        return response?.Scan ?? throw new InvalidOperationException("Impossible de démarrer l'Active Scan.");
    }

    private async Task WaitForTaskCompletion(string statusUrl, string taskName)
    {
        int progress = 0;
        while (progress < 100)
        {
            await Task.Delay(10000); 
            var statusResponse = await _httpClient.GetFromJsonAsync<ZapStatusResponse>($"{statusUrl}&apikey={_apiKey}");
            progress = int.Parse(statusResponse?.status ?? "0");
            Console.WriteLine($"{taskName} - Progression : {progress}%");
        }
        Console.WriteLine($"{taskName} terminé.");
    }

    private async Task GenerateReports(string reportName)
    {
        try
        {
            // --- Génération du Rapport HTML ---
            Console.WriteLine("Génération du rapport HTML...");
            var htmlUrl = $"{_zapApiBaseUrl}/core/other/htmlreport/?apikey={_apiKey}";
            var reportHtml = await _httpClient.GetStringAsync(htmlUrl);
            await SaveReportToFile(reportName, reportHtml, "html");

            // --- MODIFICATION : Génération du Rapport JSON ---
            Console.WriteLine("Génération du rapport JSON...");
            var jsonUrl = $"{_zapApiBaseUrl}/core/other/jsonreport/?apikey={_apiKey}"; // <-- Endpoint changé
            var reportJson = await _httpClient.GetStringAsync(jsonUrl); // <-- Variable renommée
            await SaveReportToFile(reportName, reportJson, "json"); // <-- Extension changée
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERREUR lors de la génération ou sauvegarde des rapports : {ex.Message}");
        }
    }
     
    private async Task SaveReportToFile(string reportName, string content, string extension)
    {
        // Sanitize the report name to avoid invalid characters
        var sanitizedName = string.Join("_", reportName.Split(Path.GetInvalidFileNameChars()));    
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        // Create a unique file name with timestamp
        var fileName = $"{sanitizedName}_{timestamp}.{extension}";
        // Ensure the directory exists    
        var fullPathOnHost = Path.Combine(@"C:\zap-data", fileName);

        await File.WriteAllTextAsync(fullPathOnHost, content);
        // Log the file path
        Console.WriteLine($"Rapport '{fileName}' généré et sauvegardé avec succès dans : C:\\zap-data");
    }
}

// Classes pour désérialiser les réponses JSON de ZAP
public class ZapScanResponse { public string? Scan { get; set; } }
public class ZapStatusResponse { public string? status { get; set; } }
