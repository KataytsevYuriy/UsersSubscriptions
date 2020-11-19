using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UsersSubscriptions.Models;

namespace UsersSubscriptions.DomainServices
{
    public interface IRoleService
    {
        List<IdentityRole> GetAllRoles();
        Task<IList<string>> GetUserRolesAsync(string id);
        Task CreateRoleAsync(IdentityRole role);
        Task UpdateRole(IdentityRole role);
        Task<IdentityRole> GetRoleAsync(string id);
        Task<IList<AppUser>> GetRoleUsersAsync(string roleName);
        Task DeleteRoleAsync(string id);
    }
}
