using DisasterApp.Application.DTOs;

namespace DisasterApp.Application.Services.Interfaces;

public interface IUserStatsHubService
{
    Task SendUserStatsUpdateAsync();
    Task SendUserStatsUpdateAsync(UserStatsUpdate update, ChartDataUpdate chartData);
}