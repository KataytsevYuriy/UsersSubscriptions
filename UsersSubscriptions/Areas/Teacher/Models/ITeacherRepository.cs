using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UsersSubscriptions.Models;
using UsersSubscriptions.Areas.Teacher.Models.ViewModels;
using Microsoft.AspNetCore.Identity;

namespace UsersSubscriptions.Areas.Teacher.Models
{
    public interface ITeacherRepository
    {
        Task<AppUser> GetCurrentUserAsync(HttpContext context);
        Task<IEnumerable<Subscription>> GetUserSubscriptionsAsync(string userId, DateTime month);
        Task<AppUser> GetUserAsync(string id);
        Task<IEnumerable<Course>> GetTeacherCoursesAsync(AppUser teacher);
        Task<IdentityResult> CreateSubscriptionAsync(Subscription subscription);
        Task<IEnumerable<Student>> GetTeacherMonthStudentsAsync(string courseId, DateTime month);

        Task<Course> GetCoursInfoAsync(string id);
        IEnumerable<School> GetUsersSchools(string userId);
        Task<School> GetSchoolAsync(string schoolId);
        Task<IdentityResult> AddCourseAsync(Course course);
    }
}
