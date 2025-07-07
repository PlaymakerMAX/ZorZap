using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using ZorZap.Core.Interfaces;

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

    public async Task StartFullScanAsync(string targetUrl)
    {
        Console.WriteLine($"--- Début du Full Scan sur {targetUrl} ---");

        // Étape 1 : Lancer le Spider pour découvrir les URL
        var spiderScanId = await StartSpider(targetUrl);
        await WaitForTaskCompletion($"{_zapApiBaseUrl}/spider/view/status/?scanId={spiderScanId}", "Spider");

        // Étape 2 : Lancer le Scan Actif sur les URL découvertes
        var activeScanId = await StartActiveScan(targetUrl);
        await WaitForTaskCompletion($"{_zapApiBaseUrl}/ascan/view/status/?scanId={activeScanId}", "Active Scan");

        // Étape 3 : Générer le rapport final
        await GenerateReport(targetUrl);
        
        Console.WriteLine($"--- Full Scan sur {targetUrl} terminé ! ---");
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
            // On attend 10 secondes entre chaque vérification pour ne pas surcharger l'API
            await Task.Delay(10000); 
            var statusResponse = await _httpClient.GetFromJsonAsync<ZapStatusResponse>($"{statusUrl}&apikey={_apiKey}");
            progress = int.Parse(statusResponse?.status ?? "0");
            Console.WriteLine($"{taskName} - Progression : {progress}%");
        }
        Console.WriteLine($"{taskName} terminé.");
    }

    // Fichier : ZapScanService.cs

    private async Task GenerateReport(string targetUrl)
    {
        // On crée un nom de fichier simple et unique, sans l'URL
        var fileName = $"ZAP_Report_{DateTime.UtcNow:yyyyMMdd_HHmmss}.html";
    
        // Le dossier de destination est simplement /zap/wrk/
        var reportDir = "/zap/wrk/";

        // On utilise le bon endpoint avec les bons paramètres
        var url = $"{_zapApiBaseUrl}/reports/action/generate/?apikey={_apiKey}&title=ZorZap+Scan+Report&template=traditional-html&reportDir={reportDir}&fileName={fileName}";
    
        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode(); 

        Console.WriteLine($"Demande de génération de rapport envoyée à ZAP. Il sera sauvegardé dans : {reportDir}{fileName}");
    }
}

// Classes pour désérialiser les réponses JSON de ZAP, nécessaires pour que GetFromJsonAsync fonctionne
public class ZapScanResponse { public string? Scan { get; set; } }
public class ZapStatusResponse { public string? status { get; set; } }
