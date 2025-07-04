using Microsoft.AspNetCore.Mvc;
using ZorZap.Core.Interfaces;

namespace ZorZap.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ScansController(IScanReportRepository repository) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var reports = await repository.GetAllAsync();
        return Ok(reports);
    }
}
