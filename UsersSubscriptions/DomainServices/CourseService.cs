using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UsersSubscriptions.Data;
using UsersSubscriptions.Models;
using UsersSubscriptions.Models.ViewModels;

namespace UsersSubscriptions.DomainServices
{
    public class CourseService : ICourseService
    {
        ApplicationDbContext _context;
        UserManager<AppUser> _userManager;
        IUserService _userService;
        ITeacherService _teacherService;
        IPaymentService _paymentService;
        ISubscriptionsService _subscriptionsService;
        public CourseService(ApplicationDbContext context, IUserService userService,
                UserManager<AppUser> userManager, IPaymentService paymentService,
                ITeacherService teacherService, ISubscriptionsService subscriptionsService)
        {
            _context = context;
            _userService = userService;
            _userManager = userManager;
            _paymentService = paymentService;
            _teacherService = teacherService;
            _subscriptionsService = subscriptionsService;
        }

        public Course GetCourse(string courseId)
        {
            return _context.Courses
                .Include(cour => cour.School).ThenInclude(sch => sch.Owner)
                .Include(cour => cour.CourseAppUsers).ThenInclude(appu => appu.AppUser)
                .Include(cour => cour.CoursePaymentTypes)
                .FirstOrDefault(cour => cour.Id == courseId); ;
        }

        public CourseViewModel GetCourseViewModel(string id)
        {
            Course course = _context.Courses
                            .Include(cour => cour.School).ThenInclude(sch => sch.Owner)
                            .Include(cour => cour.CourseAppUsers).ThenInclude(appu => appu.AppUser)
                            .Include(cour => cour.CoursePaymentTypes)
                            .FirstOrDefault(cour => cour.Id == id); ;
            CourseViewModel model = new CourseViewModel
            {
                Id = course.Id,
                Name = course.Name,
                Description = course.Description,
                IsActive = course.IsActive,
                Price = course.Price,
                CourseAppUsers = course.CourseAppUsers,
                SchoolId = course.SchoolId,
                AllowOneTimePrice = course.AllowOneTimePrice,
                OneTimePrice = course.OneTimePrice,
                IsCreatingNew = false,
                AllPaymentTypes = _paymentService.GetSchoolPaymentTyapes(course.SchoolId),
                ListPaymentTypes = course.CoursePaymentTypes.Select(cpt => cpt.PaymentType).ToList(),
            };
            return model;
        }
        public CourseViewModel GetCourseViewModelByName(string name, string schoolId)
        {
            Course course = _context.Courses.FirstOrDefault(cour => cour.Name == name && cour.SchoolId == schoolId);
            if (course == null)
            {
                return new CourseViewModel();
            }
            return GetCourseViewModel(course.Id);
        }

        public async Task<IdentityResult> CreateCourseAsync(CourseViewModel model)
        {
            if (string.IsNullOrEmpty(model.SchoolId)) return IdentityResult.Failed(new IdentityError { Description = "Школа на задана" });
            if (string.IsNullOrEmpty(model.Name)) return IdentityResult.Failed(new IdentityError { Description = "Заповніть назву курсу" });
            Course course = _context.Courses
                .FirstOrDefault(cour => cour.Name == model.Name && cour.SchoolId == model.SchoolId);
            if (course != null) return IdentityResult.Failed(new IdentityError { Description = "Такий курс вже існує" });
            course = new Course
            {
                Name = model.Name,
                IsActive = model.IsActive,
                Price = model.Price,
                SchoolId = model.SchoolId,
                AllowOneTimePrice = model.AllowOneTimePrice,
                OneTimePrice = model.OneTimePrice,
            };
            var state = _context.Courses.Add(course);
            if (state.State != EntityState.Added)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Курс не доданий" });
            }
            _context.SaveChanges();
            course = _context.Courses
                .FirstOrDefault(cour => cour.Name == model.Name && cour.SchoolId == model.SchoolId);
            if (course == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Курс не доданий" });
            }
            if (model.TeachersId.Count() > 0)
            {
                foreach (string teacherId in model.TeachersId)
                {
                    await _teacherService.AddTeacherToCourse(teacherId, course.Id);
                }
                await _context.SaveChangesAsync();
            }
            List<string> paymentTypesId = _context.PaymentTypes.Where(pt => pt.SchoolId == model.SchoolId).Select(pt => pt.Id).ToList();
            if (paymentTypesId != null && paymentTypesId.Count() > 0)
            {
                foreach (string pt in paymentTypesId)
                {
                    _context.CoursePaymentTypes.Add(new CoursePaymentType
                    { CourseId = course.Id, PaymentTypeId = pt });
                }
                _context.SaveChanges();
            }
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> UpdateCourseAsync(CourseViewModel course)
        {
            Course dbCourse = _context.Courses
                .Include(cour => cour.CourseAppUsers)
                .Include(cour => cour.CoursePaymentTypes)
                .FirstOrDefault(cour => cour.Id == course.Id);
            if (dbCourse == null)
                return IdentityResult.Failed(new IdentityError { Description = "Курс не знайдено" });
            IList<string> coursTeachersId = dbCourse.CourseAppUsers.Select(usr => usr.AppUserId).ToList();
            IEnumerable<string> addedTeachers = course.TeachersId.Except(coursTeachersId);
            IEnumerable<string> removedTeachers = coursTeachersId.Except(course.TeachersId);
            IList<string> coursPtypes = dbCourse.CoursePaymentTypes.Select(cpt => cpt.PaymentTypeId).ToList();
            IEnumerable<string> addedPTypes = course.PTIds.Except(coursPtypes);
            IEnumerable<string> removedPTypes = coursPtypes.Except(course.PTIds);
            dbCourse.Name = course.Name;
            dbCourse.Description = course.Description;
            dbCourse.IsActive = course.IsActive;
            dbCourse.Price = course.Price;
            dbCourse.AllowOneTimePrice = course.AllowOneTimePrice;
            dbCourse.OneTimePrice = course.OneTimePrice;
            var state = _context.Courses.Update(dbCourse);
            if (state.State != EntityState.Modified)
                return IdentityResult.Failed(new IdentityError { Description = "Курс не оновлено" });
            await _context.SaveChangesAsync();
            foreach (string teacher in addedTeachers)
            {
                await _teacherService.AddTeacherToCourse(teacher, dbCourse.Id);
            }
            foreach (string teacher in removedTeachers)
            {
                await _teacherService.RemoveTeacherFromCourse(teacher, dbCourse.Id);
            }
            foreach (string pType in addedPTypes)
            {
                _context.CoursePaymentTypes.Add(new CoursePaymentType { CourseId = course.Id, PaymentTypeId = pType });
            }
            foreach (string pType in removedPTypes)
            {
                CoursePaymentType removedCPT = _context.CoursePaymentTypes
                    .FirstOrDefault(cpt => cpt.PaymentTypeId == pType && cpt.CourseId == course.Id);
                if (removedCPT != null)
                {
                    _context.CoursePaymentTypes.Remove(removedCPT);
                }
            }
            await _context.SaveChangesAsync();
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> RemoveCourseAsync(string Id)
        {
            Course course = _context.Courses
                .Include(sub => sub.Subscriptions)
                .Include(teach => teach.CourseAppUsers)
                .Include(cpt => cpt.CoursePaymentTypes)
                .FirstOrDefault(co => co.Id == Id);
            IList<string> deletingTeacherIds = new List<string>();
            if (course.CourseAppUsers.Count() > 0) deletingTeacherIds = course.CourseAppUsers?.Select(cap => cap.AppUserId).ToList();
            if (course.CourseAppUsers.Count() > 0)
            {
                foreach (string teacherId in deletingTeacherIds)
                {
                    await _teacherService.RemoveTeacherFromCourse(teacherId, course.Id);
                }
            }
            if (course.Subscriptions.Count() > 0)
            {
                List<string> subsId = course.Subscriptions.Select(sub => sub.Id).ToList();
                foreach (string subId in subsId)
                {
                    _subscriptionsService.RemoveSubscription(subId);
                }
            }
            if (course.CoursePaymentTypes.Count() > 0)
            {
                foreach (CoursePaymentType cpt in course.CoursePaymentTypes)
                {
                    if (_context.CoursePaymentTypes.Remove(cpt).State
                                                != EntityState.Deleted)
                    {
                        return IdentityResult.Failed(new IdentityError
                        { Description = "Неможливо видалити спосіб оплати" });
                    }
                }
            }
            if (_context.Courses.Remove(course).State != EntityState.Deleted)
            {
                return IdentityResult.Failed(new IdentityError
                { Description = "Неможливо видалити курс" });
            }
            _context.SaveChanges();
            if (deletingTeacherIds.Count() > 0)
            {
                foreach (string deletingTeacherId in deletingTeacherIds)
                {
                    if (_context.CourseAppUsers.FirstOrDefault(cap => cap.AppUserId == deletingTeacherId) == null)
                    {
                        AppUser deletingTeacher = await _userService.GetUserAsync(deletingTeacherId);
                        if (deletingTeacher != null
                        && (await _userManager.IsInRoleAsync(deletingTeacher, Common.UsersConstants.teacher)))
                        {
                            await _userManager.RemoveFromRoleAsync(deletingTeacher, Common.UsersConstants.teacher);
                        }
                    }
                }
            }
            return IdentityResult.Success;
        }

        public IEnumerable<Course> GetTeacherCourses(string teacherId, string schoolId, bool onlyActive)
        {

            IEnumerable<Course> courses = _context.Courses
                .Include(cour => cour.School)
                .Include(cu => cu.CourseAppUsers)
                .Include(cu => cu.CoursePaymentTypes)
                .ThenInclude(cpt => cpt.PaymentType)
                    .Where(cour =>
                        cour.SchoolId == schoolId
                    ).OrderBy(cur=>cur.Name).ToList();
            if (!_teacherService.IsItThisSchoolOwner(schoolId, teacherId))
            {
                courses = courses.Where(cour => cour.CourseAppUsers.Any(cap => cap.AppUserId == teacherId)).ToList();
            }
            if (onlyActive)
            {
                courses = courses.Where(cour => cour.IsActive == true).ToList();
            }
            return courses;
        }

 
    }
}
