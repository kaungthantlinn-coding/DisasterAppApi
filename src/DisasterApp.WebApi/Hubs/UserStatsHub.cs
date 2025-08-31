using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using DisasterApp.Application.DTOs;
using DisasterApp.Application.Services.Interfaces;
using System.Linq;//

namespace DisasterApp.WebApi.Hubs;

[Authorize]
public class UserStatsHub : Hub
{
    private readonly IUserManagementService _userManagementService;
    private readonly ILogger<UserStatsHub> _logger;

    public UserStatsHub(
        IUserManagementService userManagementService,
        ILogger<UserStatsHub> logger)
    {
        _userManagementService = userManagementService;
        _logger = logger;
    }

    public async Task JoinUserManagementGroup()
    {
        try
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "UserManagement");
            _logger.LogInformation("User {UserId} joined UserManagement group with connection {ConnectionId}", 
                Context.UserIdentifier, Context.ConnectionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding user to UserManagement group");
            throw;
        }
    }

    public async Task RequestDataRefresh()
    {
        try
        {
            // Trigger data refresh and send updated data
            var chartData = await GetLatestChartData();
            await Clients.Caller.SendAsync("ChartDataUpdated", chartData);
            
            _logger.LogInformation("Data refresh requested by user {UserId}", Context.UserIdentifier);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing chart data for user {UserId}", Context.UserIdentifier);
            await Clients.Caller.SendAsync("Error", "Failed to refresh data");
        }
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("User {UserId} connected to UserStatsHub with connection {ConnectionId}", 
            Context.UserIdentifier, Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        try
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "UserManagement");
            _logger.LogInformation("User {UserId} disconnected from UserStatsHub with connection {ConnectionId}", 
                Context.UserIdentifier, Context.ConnectionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing user from UserManagement group on disconnect");
        }
        
        await base.OnDisconnectedAsync(exception);
    }

    private async Task<ChartDataUpdate> GetLatestChartData()
    {
        try
        {
            // Get the latest statistics and role distribution
            var userStats = await _userManagementService.GetUserStatisticsAsync();
            var roleDistribution = await _userManagementService.GetRoleDistributionAsync();
            var trends = await _userManagementService.GetUserActivityTrendsAsync();

            // Convert to the format expected by the frontend
            var monthlyData = trends.Data.Select(t => new MonthlyData
            {
                Month = t.Month,
                ActiveUsers = t.ActiveUsers,
                SuspendedUsers = t.SuspendedUsers,
                NewJoins = t.NewUsers
            }).ToList();

            var roleDistributionData = new RoleDistribution
            {
                Admin = roleDistribution.Roles.FirstOrDefault(r => r.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase))?.Count ?? 0,
                Cj = roleDistribution.Roles.FirstOrDefault(r => r.Role.Equals("CJ", StringComparison.OrdinalIgnoreCase))?.Count ?? 0,
                User = roleDistribution.Roles.FirstOrDefault(r => r.Role.Equals("User", StringComparison.OrdinalIgnoreCase))?.Count ?? 0
            };

            return new ChartDataUpdate
            {
                MonthlyData = monthlyData,
                RoleDistribution = roleDistributionData
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting latest chart data");
            throw;
        }
    }
}