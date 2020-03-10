using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UsersSubscriptions.Models;

namespace UsersSubscriptions.Areas.Admin.Models
{
    public interface IAdminDataRepository
    {
        //Users
        IEnumerable<AppUser> GetAllUsers();
        Task<AppUser> GetUserAsync(string id);
        Task<IList<string>> GetUserRolesAsync(string id);
        //Task<IEnumerable<AppUser>> GetTeachersInCourse(string courseId);
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
        Task CreateCourseAsync(CourseViewModel model);
        Task<Course> GetCourse(string id);
        Task UpdateCourseAsync(Course course);
        Task DeleteCourse(string Id);
        //Subscriptions
        IEnumerable<Subscription> GetAllSubscriptions();
        Task RemoveSubscriptionAsync(string id);
        Task<Subscription> GetSubscription(string id);
    }
}
