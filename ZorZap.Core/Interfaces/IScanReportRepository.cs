using ZorZap.Core.Entities;

namespace ZorZap.Core.Interfaces;

public interface IScanReportRepository
{
    Task<IEnumerable<ScanReport>> GetAllAsync();
    Task AddAsync(ScanReport report); // <-- AJOUTÉ
}
