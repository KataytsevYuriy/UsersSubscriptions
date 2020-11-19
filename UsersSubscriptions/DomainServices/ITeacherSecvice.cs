using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UsersSubscriptions.Models;

namespace UsersSubscriptions.DomainServices
{
    public interface ITeacherService
    {
        Task AddTeacherToCourse(string userId, string courseId);
        Task RemoveTeacherFromCourse(string userId, string courseId);
        bool IsItThisSchoolOwner(string schoolId, string ownerId);
        bool IsItThisSchoolTeacher(string schoolId, string teacherId);
        AppUser GetCurrentOwner(string userId);
        IEnumerable<School> GetCurrentTeacherSchools(string userId);
    }
}
