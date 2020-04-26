using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UsersSubscriptions.Models;
using UsersSubscriptions.Models.ViewModels;
using Microsoft.AspNetCore.Identity;

namespace UsersSubscriptions.Models
{
    public interface ITeacherRepository
    {
        Task<AppUser> GetCurrentUserAsync(HttpContext context);
        Task<AppUser> GetCurrentOwnerAsync(string userId);
        Task<IEnumerable<School>> GetCurrentTeacherSchools(string userId);
        Task<IEnumerable<Subscription>> GetUserSubscriptionsAsync(string userId, DateTime month);
        Task<AppUser> GetUserAsync(string id);
        Task<IEnumerable<Course>> GetTeacherCoursesAsync(AppUser teacher);
        Task<IdentityResult> CreateSubscriptionAsync(Subscription subscription);
        Task<IEnumerable<Student>> GetTeacherMonthStudentsAsync(string courseId, DateTime month);

        Task<Course> GetCoursInfoAsync(string id);
        IEnumerable<School> GetUsersSchools(string userId);
        Task<School> GetSchoolAsync(string schoolId);
        Task<IdentityResult> AddCourseAsync(Course course);
        Task<IdentityResult> UpdateCourseAsync(Course course, IList<string> TeachersId);
        Task<AppUser> GetUserByPhone(string phone);

        Task AddTeacherToCourse(string userId, string courseId);
    }
}
