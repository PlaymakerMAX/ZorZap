// Fichier : ZorZap.Core/Interfaces/IScanReportRepository.cs
using ZorZap.Core.Entities;

namespace ZorZap.Core.Interfaces;

public interface IScanReportRepository
{
    Task<IEnumerable<ScanReport>> GetAllAsync();
}
