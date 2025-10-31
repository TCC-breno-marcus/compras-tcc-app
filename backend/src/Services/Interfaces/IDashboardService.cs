using Models.Dtos;

namespace Services.Interfaces
{
    public interface IDashboardService
    {
        Task<DashResultDto> GetDashboardDataAsync(int ano);
    }
}
