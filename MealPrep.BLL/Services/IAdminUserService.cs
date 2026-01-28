using BusinessObjects.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MealPrep.BLL.Services
{
    public interface IAdminUserService
    {
        Task<List<AppUser>> GetUsersAsync(string? search, int page, int pageSize);
        Task<int> GetUsersCountAsync(string? search);
        Task<AppUser?> GetUserDetailsAsync(Guid id);
        Task DeactivateUserAsync(Guid id);
        Task ActivateUserAsync(Guid id);
    }
}
