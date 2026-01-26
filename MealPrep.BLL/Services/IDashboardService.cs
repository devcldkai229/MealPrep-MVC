using MealPrep.BLL.DTOs;
using System;
using System.Threading.Tasks;

namespace MealPrep.BLL.Services
{
    public interface IDashboardService
    {
        Task<DashboardDto> GetDashboardDataAsync(Guid userId);
    }
}
