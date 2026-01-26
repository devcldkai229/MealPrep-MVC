using MealPrep.DAL.Entities;
using MealPrep.DAL.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MealPrep.BLL.Services
{
    public interface IAdminPlanService
    {
        Task<List<Plan>> GetAllPlansAsync();
        Task<Plan?> GetPlanDetailsAsync(int id);
        Task CreatePlanAsync(Plan plan);
        Task UpdatePlanAsync(int id, Plan plan);
        Task<bool> CanDeletePlanAsync(int id);
        Task DeletePlanAsync(int id);
    }
}
