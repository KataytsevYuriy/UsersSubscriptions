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
    }
}
