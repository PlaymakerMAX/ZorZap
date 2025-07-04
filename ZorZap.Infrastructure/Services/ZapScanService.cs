// Fichier : ZorZap.Infrastructure/Services/ZapScanService.cs

using System.Net.Http.Json; // Important pour communiquer en JSON
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
        _zapApiBaseUrl = "http://localhost:8080/JSON"; // On cible directement l'API JSON

        _httpClient = new HttpClient();
    }

    public async Task StartFullScanAsync(string targetUrl)
    {
        // On construit l'URL pour vérifier la version, avec les paramètres
        var versionUrl = $"{_zapApiBaseUrl}/core/view/version/?apikey={_apiKey}";
        
        // On fait un appel GET pour tester la connexion
        var response = await _httpClient.GetAsync(versionUrl);
        response.EnsureSuccessStatusCode(); // Lève une exception si l'appel échoue

        var versionInfo = await response.Content.ReadFromJsonAsync<ZapVersionInfo>();

        Console.WriteLine($"Connecté à ZAP version : {versionInfo?.Version}. Lancement du scan sur {targetUrl}");
        
        // TODO : Ajouter la logique pour le Spider, l'Active Scan, etc.
    }
}

// Classe simple pour "désérialiser" la réponse JSON de ZAP
public class ZapVersionInfo
{
    public string? Version { get; set; }
}
