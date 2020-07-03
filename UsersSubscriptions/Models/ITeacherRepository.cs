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
        Task<AppUser> GetUserAsync(string id);
        AppUser GetUserByPhone(string phone);
        AppUser GetCurrentOwner(string userId);
        IEnumerable<School> GetCurrentTeacherSchools(string userId);
        IEnumerable<AppUser> FindUserByName(string name);

        //course
        Course GetCourse(string id);
        CourseViewModel GetCourseViewModel(string id);
        Task<IdentityResult> CreateCourseAsync(CourseViewModel course);
        Task<IdentityResult> UpdateCourseAsync(CourseViewModel course);
        IEnumerable<Course> GetTeacherCourses(string teacherId, string schoolId, bool onlyActive);
        Task AddTeacherToCourse(string userId, string courseId);
        Task<IdentityResult> DeleteCourseAsync(string Id);
        bool CourseHasSubscriptions(string id);

        //Subscription
        Task<IdentityResult> CreateSubscriptionAsync(Subscription subscription);
        IEnumerable<Subscription> GetUserSubscriptions(string userId, string schoolId, DateTime month);
        void RemoveSubscription(string id);

        //school
        School GetSchool(string schoolId);
        IdentityResult UpdateSchoolOptions(School school);
        SchoolCalculationsViewModel GetSchoolDetail(string schoolId, string courseId, DateTime month, string selectedNavId, string selectedTeacherId);
        School GetSchoolByUrl(string url);
        IEnumerable<School> GetUsersSchools(string userId);
        IEnumerable<Student> GetTeacherMonthStudents(string courseId, DateTime month);
        IEnumerable<PaymentType> GetSchoolPaymentTyapes(string schoolId);
        bool IsItThisSchoolOwner(string schoolId, string ownerId);
        bool IsItThisSchoolTeacher(string schoolId, string teacherId);

        //PaymentType
        IdentityResult UpdateCoursePaymentTypes(string schoolId, string courseId, List<string> pTypes);
    }
}
