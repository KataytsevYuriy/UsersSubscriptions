using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UsersSubscriptions.Models;

namespace UsersSubscriptions.Areas.Teacher.Models
{
    public interface ITeacherRepository
    {
        Task<AppUser> GetCurrentUserAsync(HttpContext context);
        Task<IEnumerable<Course>> GetTeacherCoursesAsync(AppUser teacher);
        Task<Course> GetCoursInfoAsync(string id);
        Task<Subscription> GetSubscriptionAsync(string id);
        Task ConfirmSubscriptionAsync(AppUser teacher, string id);
        Task ConfirmPayedSubscriptionAsync(AppUser teacher, string id);
    }
}
