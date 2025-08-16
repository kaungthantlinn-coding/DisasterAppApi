using System.ComponentModel.DataAnnotations;

namespace DisasterApp.Application.DTOs;

public class UserStatsUpdate
{
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int SuspendedUsers { get; set; }
    public int NewJoins { get; set; }
    public string Timestamp { get; set; } = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
}

public class ChartDataUpdate
{
    public List<MonthlyData> MonthlyData { get; set; } = new();
    public RoleDistribution RoleDistribution { get; set; } = new();
}

public class MonthlyData
{
    public string Month { get; set; } = string.Empty;
    public int ActiveUsers { get; set; }
    public int SuspendedUsers { get; set; }
    public int NewJoins { get; set; }
}

public class RoleDistribution
{
    public int Admin { get; set; }
    public int Cj { get; set; }
    public int User { get; set; }
}