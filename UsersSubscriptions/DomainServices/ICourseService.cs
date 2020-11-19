using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UsersSubscriptions.Models;
using UsersSubscriptions.Models.ViewModels;

namespace UsersSubscriptions.DomainServices
{
    public interface ICourseService
    {
        Course GetCourse(string courseId);
        CourseViewModel GetCourseViewModel(string id);
        CourseViewModel GetCourseViewModelByName(string name, string schoolId);
        Task<IdentityResult> CreateCourseAsync(CourseViewModel model);
        Task<IdentityResult> UpdateCourseAsync(CourseViewModel course);
        Task<IdentityResult> RemoveCourseAsync(string Id);
        IEnumerable<Course> GetTeacherCourses(string teacherId, string schoolId, bool onlyActive);
        //bool CourseHasSubscriptions(string id);
    }
}
