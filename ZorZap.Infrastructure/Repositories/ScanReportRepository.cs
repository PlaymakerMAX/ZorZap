// Fichier : ZorZap.Infrastructure/Repositories/ScanReportRepository.cs
using Microsoft.EntityFrameworkCore;
using ZorZap.Core.Entities;
using ZorZap.Core.Interfaces;
using ZorZap.Infrastructure.Persistence;

namespace ZorZap.Infrastructure.Repositories;

public class ScanReportRepository(ZorZapDbContext context) : IScanReportRepository
{
    public async Task<IEnumerable<ScanReport>> GetAllAsync()
    {
        return await context.ScanReports.ToListAsync();
    }
}
// This code defines a repository for managing scan reports in a database using Entity Framework Core.
// The `ScanReportRepository` class implements the `IScanReportRepository` interface and provides