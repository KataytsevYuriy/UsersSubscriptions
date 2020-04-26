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
        Task<AppUser> GetUserAsync(string id);
        Task<IList<string>> GetUserRolesAsync(string id);
        Task UpdateUserAsync(AppUser user, IList<string> newUserRoles);
        Task DeleteUseAsyncr(string id);
        //Roles
        List<IdentityRole> GetAllRoles();
        Task CreateRoleAsync(IdentityRole role);
        Task UpdateRole(IdentityRole role);
        Task<IdentityRole> GetRoleAsync(string id);
        Task<IList<AppUser>> GetRoleUsersAsync(string roleName);
        Task DeleteRoleAsync(string id);
        //Courses
        IEnumerable<Course> GetAllCourses();
        Task<IdentityResult> CreateCourseAsync(Course model);
        Task<Course> GetCourseAsync(string id);
        Task<IdentityResult> UpdateCourseAsync(CourseDetailsViewModel course);
        Task<IdentityResult> DeleteCourse(string Id);
        Task<bool> CourseHasSubscriptions(string id);
        //Subscriptions
        IEnumerable<Subscription> GetAllSubscriptions();
        Task RemoveSubscriptionAsync(string id);
        Task<Subscription> GetSubscription(string id);
        //Schools
        IEnumerable<School> GetAllSchools();
        Task<IdentityResult> CreateSchoolAsync(School school);
        Task<School> GetSchoolAsync(string id);
        Task<IdentityResult> UpdateSchoolAsync(School school);
        Task<IdentityResult> DeleteScoolAsync(string Id);

        //Json
        Task<IdentityResult> ChengeOwnerAsync(string newOwnerId, string schoolId);
    }
}
