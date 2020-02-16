using Microsoft.AspNetCore.Http;
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
        Task CreateSubscription(Subscription subscription);
    }
}
