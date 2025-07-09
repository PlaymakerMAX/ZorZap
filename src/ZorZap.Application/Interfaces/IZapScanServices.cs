namespace ZorZap.Application.Interfaces;

public interface IZapScanService
{
    /// Démarre un scan complet sur l'URL cible et génère un rapport.
    Task StartFullScanAsync(string targetUrl, string reportName);

}
