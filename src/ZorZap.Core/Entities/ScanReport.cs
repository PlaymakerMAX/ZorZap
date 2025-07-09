namespace ZorZap.Core.Entities;

public class ScanReport
{
    public int Id { get; set; }
    public required string ReportName { get; set; }
    public required string TargetUrl { get; set; } // Ajouté
    public required string Status { get; set; }
    public DateTime CreatedAt { get; set; } // Ajouté
}
