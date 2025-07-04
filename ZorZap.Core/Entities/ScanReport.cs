// Fichier : ZorZap.Core/Entities/ScanReport.cs
namespace ZorZap.Core.Entities;

public class ScanReport
{
    public int Id { get; set; }
    public required string ReportName { get; set; }
    public required string Status { get; set; }
}
