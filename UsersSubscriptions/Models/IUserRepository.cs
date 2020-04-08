using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UsersSubscriptions.Models
{
    public interface IUserRepository
    {
        Task<AppUser> GetCurentUser(HttpContext context);
        IEnumerable<Course> GetAllCourses();
        Task<Course> GetCourse(string id);
        AppUser GetUserCourses(string id);
        Task<IdentityResult> CreateSubscription(Subscription subscription);
        Task<Subscription> GetSubscriptionAsync(string id);
        Task DeleteSubscription(string id);
    }
}
