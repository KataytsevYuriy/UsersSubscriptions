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
        //user
        Task<AppUser> GetCurrentUserAsync(HttpContext context);
        Task<AppUser> GetUserAsync(string id);
        Task<AppUser> GetUserByPhone(string phone);
        Task<AppUser> GetCurrentOwnerAsync(string userId);
        Task<IEnumerable<School>> GetCurrentTeacherSchools(string userId);
       
        //course
        Task<IdentityResult> CreateCourseAsync(OwnerCourseViewModel course);
        Task<Course> GetCoursAsync(string id);
        Task<IEnumerable<Course>> GetTeacherCoursesAsync(AppUser teacher, string schoolId, bool onlyActive);
        Task<IdentityResult> UpdateCourseAsync(Course course, IList<string> TeachersId);
        Task AddTeacherToCourse(string userId, string courseId);
        Task<IdentityResult> DeleteCourse(string Id);
        Task<bool> CourseHasSubscriptions(string id);

        //Subscription
        Task<IdentityResult> CreateSubscriptionAsync(Subscription subscription);
        Task<IEnumerable<Subscription>> GetUserSubscriptionsAsync(string userId,string schoolId, DateTime month);

        //school
        Task<School> GetSchoolAsync(string schoolId);
        School GetSchoolByUrl(string url);
        Task<IEnumerable<Student>> GetTeacherMonthStudentsAsync(string courseId, DateTime month);
        IEnumerable<School> GetUsersSchools(string userId);
        bool IsItThisSchoolOwner(string schoolId, string ownerId);
        bool IsItThisSchoolTeacher(string schoolId, string teacherId);


    }
}
