// Fichier : ZorZap.Infrastructure/Persistence/ZorZapDbContext.cs
using Microsoft.EntityFrameworkCore;
using ZorZap.Core.Entities;

namespace ZorZap.Infrastructure.Persistence;

public class ZorZapDbContext(DbContextOptions<ZorZapDbContext> options) : DbContext(options)
{
    public DbSet<ScanReport> ScanReports { get; set; }
}
