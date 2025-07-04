namespace ZorZap.Core.Interfaces;

public interface IZapScanService
{
    Task StartFullScanAsync(string targetUrl);
}
