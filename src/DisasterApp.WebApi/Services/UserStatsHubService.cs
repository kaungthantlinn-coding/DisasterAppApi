using DisasterApp.Application.DTOs;
using DisasterApp.Application.Services.Interfaces;
using DisasterApp.WebApi.Hubs;
using Microsoft.AspNetCore.SignalR;
using System.Linq;

namespace DisasterApp.WebApi.Services;

public class UserStatsHubService : IUserStatsHubService
{
    private readonly IHubContext<UserStatsHub> _hubContext;
    private readonly IUserManagementService _userManagementService;
    private readonly ILogger<UserStatsHubService> _logger;

    public UserStatsHubService(
        IHubContext<UserStatsHub> hubContext,
        IUserManagementService userManagementService,
        ILogger<UserStatsHubService> logger)
    {
        _hubContext = hubContext;
        _userManagementService = userManagementService;
        _logger = logger;
    }

    public async Task SendUserStatsUpdateAsync()
    {
        try
        {
            var stats = await _userManagementService.GetUserStatisticsAsync();
            var activityTrends = await _userManagementService.GetUserActivityTrendsAsync();
            var roleDistribution = await _userManagementService.GetRoleDistributionAsync();
            
            var chartData = new ChartDataUpdate
            {
                MonthlyData = activityTrends.Data.Select(d => new MonthlyData
                {
                    Month = d.Month,
                    ActiveUsers = d.ActiveUsers,
                    SuspendedUsers = d.SuspendedUsers,
                    NewJoins = d.NewUsers
                }).ToList(),
                RoleDistribution = new RoleDistribution
                {
                    Admin = roleDistribution.Roles.FirstOrDefault(r => r.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase))?.Count ?? 0,
                    Cj = roleDistribution.Roles.FirstOrDefault(r => r.Role.Equals("CJ", StringComparison.OrdinalIgnoreCase))?.Count ?? 0,
                    User = roleDistribution.Roles.FirstOrDefault(r => r.Role.Equals("User", StringComparison.OrdinalIgnoreCase))?.Count ?? 0
                }
            };

            var update = new UserStatsUpdate
            {
                TotalUsers = stats.TotalUsers,
                ActiveUsers = stats.ActiveUsers,
                SuspendedUsers = stats.SuspendedUsers,
                NewJoins = stats.NewUsersThisMonth
            };

            await _hubContext.Clients.Group("UserManagement").SendAsync("UserStatsUpdated", update);
            await _hubContext.Clients.Group("UserManagement").SendAsync("ChartDataUpdated", chartData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending SignalR user stats update");
        }
    }

    public async Task SendUserStatsUpdateAsync(UserStatsUpdate update, ChartDataUpdate chartData)
    {
        try
        {
            await _hubContext.Clients.Group("UserManagement").SendAsync("UserStatsUpdated", update);
            await _hubContext.Clients.Group("UserManagement").SendAsync("ChartDataUpdated", chartData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending SignalR user stats update");
        }
    }
}