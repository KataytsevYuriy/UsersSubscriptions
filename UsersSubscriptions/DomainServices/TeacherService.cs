using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UsersSubscriptions.Data;
using UsersSubscriptions.Models;

namespace UsersSubscriptions.DomainServices
{
    public class TeacherService:ITeacherService
    {
        private readonly ApplicationDbContext _context;
        private UserManager<AppUser> _userManager;
        public TeacherService(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task AddTeacherToCourse(string userId, string courseId)
        {
            if (string.IsNullOrEmpty(userId) && _context.Users.FirstOrDefault(ap => ap.Id == userId) == null)
            {
                return;
            }
            if (string.IsNullOrEmpty(courseId) && _context.Courses.FirstOrDefault(cour => cour.Id == courseId) == null)
            {
                return;
            }
            if ((await _context.CourseAppUsers.FirstOrDefaultAsync(capu => capu.AppUserId == userId && capu.CourseId == courseId)) != null)
            {
                return;
            }
             _context.CourseAppUsers.Add(new CourseAppUser { AppUserId = userId, CourseId = courseId });
             _context.SaveChanges();
            AppUser user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                if (!(await _userManager.IsInRoleAsync(user, Common.UsersConstants.teacher)))
                {
                    await _userManager.AddToRoleAsync(user, Common.UsersConstants.teacher);
                }
            }
        }

        public async Task RemoveTeacherFromCourse(string userId, string courseId)
        {
            if (string.IsNullOrEmpty(courseId))
            {
                return;
            }
            CourseAppUser courseAppUser = _context.CourseAppUsers.FirstOrDefault(capu => capu.AppUserId == userId && capu.CourseId == courseId);
            if (courseAppUser == null)
            {
                return;
            }
            _context.CourseAppUsers.Remove(courseAppUser);
            _context.SaveChanges();
            if (string.IsNullOrEmpty(userId)) return;
            AppUser user = await _userManager.FindByIdAsync(userId);
            if (user != null && _context.CourseAppUsers.FirstOrDefault(cap => cap.AppUserId == userId) == null)
            {
                if (await _userManager.IsInRoleAsync(user, Common.UsersConstants.teacher))
                {
                    await _userManager.RemoveFromRoleAsync(user, Common.UsersConstants.teacher);
                }
            }
        }

        public bool IsItThisSchoolOwner(string schoolId, string ownerId)
        {
            School school = _context.Schools.Include(sch => sch.Owner)
                .FirstOrDefault(sch => sch.Id == schoolId && sch.OwnerId == ownerId);
            return school == null ? false : true;
        }

        public bool IsItThisSchoolTeacher(string schoolId, string teacherId)
        {
            Course course = _context.Courses
                .Include(cour => cour.CourseAppUsers)
                .Include(cour => cour.School)
                .FirstOrDefault(cour =>
                cour.SchoolId == schoolId
                && cour.IsActive
                && cour.CourseAppUsers.Any(cau => cau.AppUserId == teacherId));
            return course == null ? false : true;
        }

        public AppUser GetCurrentOwner(string userId)
        {
            AppUser currentOwner = _context.Users
                .Include(usr => usr.Schools)
                .FirstOrDefault(usr => usr.Id == userId);
            return currentOwner;
        }

        public IEnumerable<School> GetCurrentTeacherSchools(string userId)
        {
            List<School> curTeacherSchools = _context.Schools
                .Include(sch => sch.Courses).ThenInclude(cour => cour.CourseAppUsers)
                .Where(sch => sch.OwnerId == userId || sch.Courses.Any(cour => cour.CourseAppUsers.Any(cap => cap.AppUserId == userId)))
                .ToList();
            return curTeacherSchools;
        }
    }
}
