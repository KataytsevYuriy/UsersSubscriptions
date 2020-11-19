using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UsersSubscriptions.Models;

namespace UsersSubscriptions.DomainServices
{
    public class RoleService : IRoleService
    {
        private RoleManager<IdentityRole> _roleManager;
        private UserManager<AppUser> _userManager;
        public RoleService(RoleManager<IdentityRole> roleManager, UserManager<AppUser> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }
        public List<IdentityRole> GetAllRoles()
        {
            return _roleManager.Roles.ToList();
        }

        public async Task<IList<string>> GetUserRolesAsync(string id)
        {
            return await _userManager.GetRolesAsync(new AppUser { Id = id });
        }

        public async Task CreateRoleAsync(IdentityRole role)
        {
            if (!(await _roleManager.RoleExistsAsync(role.Name)))
            {
                await _roleManager.CreateAsync(role);
            }
        }

        public async Task UpdateRole(IdentityRole model)
        {
            IdentityRole role = await GetRoleAsync(model.Id);
            if (role != null)
            {
                role.Name = model.Name;
                await _roleManager.UpdateAsync(role);
            }
        }

        public async Task<IdentityRole> GetRoleAsync(string id)
        {
            return await _roleManager.FindByIdAsync(id);
        }

        public async Task<IList<AppUser>> GetRoleUsersAsync(string roleName)
        {
            return await _userManager.GetUsersInRoleAsync(roleName);
        }

        public async Task DeleteRoleAsync(string id)
        {
            IdentityRole role = await GetRoleAsync(id);
            if (role != null)
            {
                await _roleManager.DeleteAsync(role);
            }
        }

    }
}
