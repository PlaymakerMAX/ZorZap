// Fichier : ZorZap.Api/Controllers/ScansController.cs
using Hangfire;
using Microsoft.AspNetCore.Mvc;
using ZorZap.Core.DTOs;
using ZorZap.Core.Entities;
using ZorZap.Core.Interfaces;

namespace ZorZap.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ScansController(
    IScanReportRepository repository,
    IBackgroundJobClient backgroundJobClient) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateScan([FromBody] CreateScanReportDto createDto)
    {
        var newReport = new ScanReport
        {
            ReportName = createDto.ReportName,
            TargetUrl = createDto.TargetUrl,
            Status = "Queued", // Le statut est "En file d'attente"
            CreatedAt = DateTime.UtcNow
        };

        await repository.AddAsync(newReport);

        // On met le vrai scan en file d'attente pour qu'il s'exécute en arrière-plan
        backgroundJobClient.Enqueue<IZapScanService>(service =>
            service.StartFullScanAsync(newReport.TargetUrl));

        // On utilise 202 Accepted pour indiquer que la tâche est acceptée mais pas encore terminée
        return Accepted(newReport);
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var reports = await repository.GetAllAsync();
        return Ok(reports);
    }
}
// Ce contrôleur gère les requêtes liées aux scans.
// Il permet de créer un nouveau scan et de récupérer la liste des scans existants.
