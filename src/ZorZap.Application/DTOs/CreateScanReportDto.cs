namespace ZorZap.Application.DTOs;

public class CreateScanReportDto
{
    public required string ReportName { get; set; }
    public required string TargetUrl { get; set; }
}
