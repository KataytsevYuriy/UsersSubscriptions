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
        CourseViewModel GetCourseViewModelByName(string name, string schoolId);
        Task<IdentityResult> CreateCourseAsync(CourseViewModel course);
        Task<IdentityResult> UpdateCourseAsync(CourseViewModel course);
        IEnumerable<Course> GetTeacherCourses(string teacherId, string schoolId, bool onlyActive);
        Task AddTeacherToCourse(string userId, string courseId);
        Task<IdentityResult> RemoveCourseAsync(string Id);
        bool CourseHasSubscriptions(string id);
        IEnumerable<Payment> GetCoursePayment(string Id);

        //Subscription
        Task<IdentityResult> CreateSubscriptionAsync(Subscription subscription);
        IEnumerable<Subscription> GetUserSubscriptions(string userId, string schoolId, DateTime month);
        Subscription GetSubscription(string Id);
        IdentityResult UpdateSubscription(Subscription subscription, string teacherId);
        void RemoveSubscription(string id);

        //school
        School GetSchool(string schoolId);
        IdentityResult UpdateSchoolOptions(School school);
        SchoolCalculationsViewModel GetSchoolDetail(string schoolId, string courseId, DateTime month, string selectedNavId, string selectedTeacherId);
        School GetSchoolByUrl(string url);
        IEnumerable<School> GetUsersSchools(string userId);
        IEnumerable<Subscription> GetTeacherMonthSubscriptions(string courseId, DateTime month);
        IEnumerable<PaymentType> GetSchoolPaymentTyapes(string schoolId);
        bool IsItThisSchoolOwner(string schoolId, string ownerId);
        bool IsItThisSchoolTeacher(string schoolId, string teacherId);
        bool IsSchoolAllowed(string schoolId);
        IdentityResult PayForSchool(string schoolId, string description);

        //PaymentType
        IdentityResult UpdateCoursePaymentTypes(string schoolId, string courseId, List<string> pTypes);
        void AddDefaultPaymentTypesToSchool(string schoolId);
    }
}
