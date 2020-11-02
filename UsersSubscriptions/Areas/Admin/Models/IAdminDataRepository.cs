using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UsersSubscriptions.Models;
using UsersSubscriptions.Areas.Admin.Models.ViewModels;

namespace UsersSubscriptions.Areas.Admin.Models
{
    public interface IAdminDataRepository
    {
        //Users
        IEnumerable<AppUser> GetAllUsers();
        Task UpdateUserAsync(AppUser user, IList<string> newUserRoles);
        Task<IdentityResult> DeleteUserAsync(string id);

        //Roles
        List<IdentityRole> GetAllRoles();
        Task<IList<string>> GetUserRolesAsync(string id);
        Task CreateRoleAsync(IdentityRole role);
        Task UpdateRole(IdentityRole role);
        Task<IdentityRole> GetRoleAsync(string id);
        Task<IList<AppUser>> GetRoleUsersAsync(string roleName);
        Task DeleteRoleAsync(string id);

        //Subscriptions
        IEnumerable<Subscription> GetFilteredSubscriptions(string schoolId, string courseId, DateTime month, string searchByName);
        Task<Subscription> GetSubscription(string id);

        //Schools
        IEnumerable<School> GetAllSchools();
        Task<IdentityResult> CreateSchoolAsync(School school);
        Task<IdentityResult> UpdateSchoolAsync(School school);
        Task<IdentityResult> RemoveScoolAsync(string Id);

        //Finance
        School GetSchoolFinance(string schoolId);
        IdentityResult UpdateSchoolFinance(School school);
        IdentityResult AddSchoolTransaction(SchoolTransaction transaction);

        //Json
        Task<IdentityResult> ChengeOwnerAsync(string newOwnerId, string schoolId);
    }
}
