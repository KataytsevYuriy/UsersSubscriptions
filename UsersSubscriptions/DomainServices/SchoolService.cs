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
    public class SchoolService : ISchoolService
    {
        private ApplicationDbContext _context;
        private UserManager<AppUser> _userManager;
        private ICourseService _courseService;
        public SchoolService(ApplicationDbContext context, UserManager<AppUser> userManager,
            ICourseService courseService)
        {
            _context = context;
            _userManager = userManager;
            _courseService = courseService;
        }

        public School GetSchool(string schoolId)
        {
            return _context.Schools
                .Include(sch => sch.Courses)
                .Include(sch => sch.Owner)
                .Include(sch => sch.PaymentTypes)
                .FirstOrDefault(sch => sch.Id == schoolId);
        }

        public IdentityResult UpdateSchoolOptions(School model)
        {
            if (string.IsNullOrEmpty(model.Id))
            {
                return IdentityResult.Failed(new IdentityError { Description = "Школа вiдсутня" });
            }
            bool modified = false;
            if (model.PaymentTypes != null)
            {
                if (model.PaymentTypes.Count() > 0)
                {
                    foreach (PaymentType payment in model.PaymentTypes)
                    {
                        if ((payment.Name?.Length > 1 && payment.Name?.Length < 3)
                            || (string.IsNullOrEmpty(payment.Name) && !string.IsNullOrEmpty(payment.Id)))
                        {
                            return IdentityResult.Failed(new IdentityError
                            { Description = "Назва способу оплати повинна мати не менше трьох символів" });
                        }
                        if (string.IsNullOrEmpty(payment.Id) && !string.IsNullOrEmpty(payment.Name))
                        {
                            _context.PaymentTypes.Add(new PaymentType
                            {
                                Name = payment.Name,
                                Priority = payment.Priority,
                                SchoolId = model.Id,
                            });
                            modified = true;
                        }
                        else
                        {
                            PaymentType pType = _context.PaymentTypes.FirstOrDefault(pt => pt.Id == payment.Id);
                            if (pType != null
                                    && (string.IsNullOrEmpty(pType.Name) || !pType.Name.Equals(payment.Name)
                                            || pType.Priority != payment.Priority))
                            {
                                pType.Name = payment.Name;
                                pType.Priority = payment.Priority;
                                modified = true;
                            }
                        }

                    }
                }
                List<string> dbPaymentTypes = _context.PaymentTypes.Where(pt => pt.SchoolId == model.Id)
                    .Select(pt => pt.Id).ToList();
                IEnumerable<string> removedPaymentTypes = dbPaymentTypes.Except(model
                                                        .PaymentTypes.Select(pt => pt.Id).ToList());
                if (removedPaymentTypes.Count() > 0)
                {
                    foreach (string pTId in removedPaymentTypes)
                    {
                        List<CoursePaymentType> coursePaymentTypes = _context.CoursePaymentTypes
                                .Where(cpt => cpt.PaymentTypeId == pTId).ToList();
                        if (coursePaymentTypes != null && coursePaymentTypes.Count() > 0)
                        {
                            _context.CoursePaymentTypes.RemoveRange(coursePaymentTypes);
                        }
                        PaymentType paymentToRemove = _context.PaymentTypes.FirstOrDefault(pt => pt.Id == pTId);
                        _context.PaymentTypes.Remove(paymentToRemove);
                    }
                    modified = true;
                }
            }
            if (modified) { _context.SaveChanges(); }

            return IdentityResult.Success;
        }

        public School GetSchoolByUrl(string url)
        {
            return _context.Schools.FirstOrDefault(sch => sch.UrlName.ToLower().Equals(url.ToLower()));
        }

        public SchoolCalculationsViewModel GetSchoolDetail(string schoolId, string courseId,
            DateTime month, string selectedNavId, string selectedTeacherId)
        {
            if (string.IsNullOrEmpty(schoolId)) return new SchoolCalculationsViewModel();
            IEnumerable<Course> courses = _context.Courses
                .Include(cour => cour.School)
                .Include(cour => cour.CourseAppUsers).ThenInclude(capu => capu.AppUser)
                .Include(cour => cour.Subscriptions).ThenInclude(sub => sub.AppUser)
                .Include(cour => cour.Subscriptions).ThenInclude(sub => sub.Payments).ThenInclude(pay => pay.PaymentType)
                .Where(cour => cour.SchoolId == schoolId)
                .OrderBy(cour => cour.Name)
                .ToList();
            List<SchoolCourse> schoolCourses = new List<SchoolCourse>();
            if (courses != null && courses.Count() > 0)
            {
                schoolCourses = courses.Select(course => new SchoolCourse
                {
                    Id = course.Id,
                    Name = course.Name,
                    IsActive = course.IsActive,
                    Sum = course.Subscriptions
                     .Where(sub => sub.Period.Month == month.Month && sub.Period.Year == month.Year)
                     .Select(sub => sub.Payments.Select(pay => pay.Price).Sum()).Sum(),
                }).ToList();
            }

            var sumByPaymentType = new Dictionary<string, int>();
            var subs = courses.SelectMany(entry => entry.Subscriptions)
                .Where(sub => sub.Period.Month == month.Month && sub.Period.Year == month.Year).ToList();
            List<Subscription> subscriptions = new List<Subscription>();
            if (!string.IsNullOrEmpty(courseId) && courses.FirstOrDefault(cour => cour.Id == courseId) != null)
            {
                subscriptions = courses.FirstOrDefault(cour => cour.Id == courseId)
                    .Subscriptions
                    .Where(sub => sub.Period.Month == month.Month && sub.Period.Year == month.Year).ToList();
            }
            IEnumerable<SchoolTeacher> schoolTeachers = courses.SelectMany(cour => cour.CourseAppUsers.Select(capu => capu.AppUser))
                .Distinct()
                .Select(usr => new SchoolTeacher
                {
                    Id = usr.Id,
                    FullName = usr.FullName,
                })
                .ToList();
            IEnumerable<Subscription> teacherSubscriptions = new List<Subscription>();
            if (!string.IsNullOrEmpty(selectedTeacherId))
            {
                IEnumerable<Course> teacherCourses = courses.Where(cour => cour.CourseAppUsers.Any(cap => cap.AppUserId == selectedTeacherId));
                teacherSubscriptions = teacherCourses.SelectMany(cour => cour.Subscriptions)
                    .Where(sub => sub.Period.Month == month.Month && sub.Period.Year == month.Year)
                    .ToList();
            }
            SchoolCalculationsViewModel model = new SchoolCalculationsViewModel
            {
                SchoolId = schoolId,
                SelectedCourseId = courseId,
                SelectedTeacherId = selectedTeacherId,
                Month = month,
                SelectedNavId = selectedNavId,
                SchoolCourses = schoolCourses,
                SchoolCoursesByPaymentTypeSum = sumByPaymentType,
                CourseSubscriptions = subscriptions,
                SchoolTeachers = schoolTeachers,
                TeacherSubscriptions = teacherSubscriptions,
            };
            return model;
        }

        public IEnumerable<School> GetUsersSchools(string userId)
        {
            return _context.Schools.Where(sch => sch.OwnerId == userId).ToList();
        }

        public IEnumerable<Subscription> GetTeacherMonthSubscriptions(string courseId, DateTime month)
        {
            return _context.Subscriptions
                .Include(us => us.AppUser)
                .Include(su => su.Payments).ThenInclude(pay => pay.PaymentType)
                .Where(cour => cour.CourseId == courseId
                    && cour.Period.Year == month.Year
                    && cour.Period.Month == month.Month);
        }

        public bool IsSchoolAllowed(string schoolId)
        {
            School school = _context.Schools.FirstOrDefault(sch => sch.Id == schoolId);
            if (school == null) return false;
            if (school.IsPayed)
            {
                if (!IsDatePassed(school.PayedMonth, false)) return true;
            }
            else
            {
                if (!IsDatePassed(school.AllowTestUntil, true)) return true;
            }
            if (PayForSchool(schoolId, "").Succeeded) return true;
            return false;
        }

        bool IsDatePassed(DateTime date, bool includeDays)
        {
            DateTime dateNow = DateTime.Now;
            if (date.Year == dateNow.Year)
            {
                if (date.Month > dateNow.Month)
                {
                    return false;
                }
                else if (date.Month == dateNow.Month)
                {
                    if (includeDays)
                    {
                        if (date.Day >= dateNow.Day) return false;
                    }
                    else return false;
                }
            }
            else if (date.Year > dateNow.Year) return false;
            return true;
        }

        public IdentityResult PayForSchool(string schoolId, string description)
        {
            School school = _context.Schools.FirstOrDefault(sch => sch.Id == schoolId);
            //if price == 0 don't create transaction but giv access to school
            if (school.Price == 0) return IdentityResult.Success;
            if (IsDatePassed(school.PayedMonth, false))
            {
                if (school.Balance >= school.Price)
                {
                    school.IsPayed = true;
                    school.PayedMonth = DateTime.Now;
                    if (string.IsNullOrEmpty(description)) description = "Автоматична сплата за місяць";
                    SchoolTransaction schoolTransaction = new SchoolTransaction
                    {
                        SchoolId = schoolId,
                        Payed = -school.Price,
                        Description = description,
                        PayedDateTime = DateTime.Now,
                        OldBalance = school.Balance,
                        NewBalance = school.Balance - school.Price,
                    };
                    school.Balance -= school.Price;
                    if (_context.SchoolTransactions.Add(schoolTransaction).State == EntityState.Added)
                    {
                        _context.SaveChanges();
                        return IdentityResult.Success;
                    }
                }
            }
            return IdentityResult.Failed();
        }

        //public IEnumerable<PaymentType> GetSchoolPaymentTyapes(string schoolId)
        //{
        //    IEnumerable<PaymentType> paymentTypes = _context.PaymentTypes
        //        .Where(pt => pt.SchoolId == schoolId).OrderBy(pt => pt.Priority).ToList();
        //    return paymentTypes;
        //}


        public IEnumerable<School> GetAllSchools()
        {
            return _context.Schools
                .Include(usr => usr.Owner)
                .Include(sch => sch.Courses)
                .ToList();
        }

        public async Task<IdentityResult> CreateSchoolAsync(School school)
        {
            if (string.IsNullOrEmpty(school.Name))
            {
                return IdentityResult.Failed(new IdentityError { Description = "Введіть назву школи" });
            }
            if (string.IsNullOrEmpty(school.UrlName))
            {
                return IdentityResult.Failed(new IdentityError { Description = "Введіть Url школи" });
            }
            if (string.IsNullOrEmpty(school.OwnerId))
            {
                return IdentityResult.Failed(new IdentityError { Description = "Додайте власника  школи" });
            }
            AppUser newOwner = await _userManager.FindByIdAsync(school.OwnerId);
            if (newOwner == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Додайте диретора" });
            }
            if (_context.Schools.FirstOrDefault(sc => sc.UrlName == school.UrlName) != null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Школа з такою URL адресою вже існує" });
            }
            school.Balance = 0;
            var state = await _context.Schools.AddAsync(school);
            if (state.State != EntityState.Added)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Школа не додана" });
            }
            await _context.SaveChangesAsync();
            if (!(await _userManager.IsInRoleAsync(newOwner, Common.UsersConstants.schoolOwner)))
            {
                await _userManager.AddToRoleAsync(newOwner, Common.UsersConstants.schoolOwner);
            }
            return IdentityResult.Success;
        }



        public async Task<IdentityResult> UpdateSchoolAsync(School school)
        {
            School dbSchool = await _context.Schools.FirstOrDefaultAsync(sch => sch.Id == school.Id);
            if (dbSchool == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Школа на знайдена" });
            }
            if (string.IsNullOrEmpty(school.Name))
            {
                return IdentityResult.Failed(new IdentityError { Description = "Введіть назву школи" });
            }
            if (string.IsNullOrEmpty(school.UrlName))
            {
                return IdentityResult.Failed(new IdentityError { Description = "Введіть Url школи" });
            }
            string oldOwnerId = dbSchool.OwnerId ?? "";
            if (string.IsNullOrEmpty(school.OwnerId))
            {
                return IdentityResult.Failed(new IdentityError { Description = "Додайте власника  школи" });
            }
            dbSchool.Name = school.Name;
            dbSchool.OwnerId = school.OwnerId;
            dbSchool.UrlName = school.UrlName;
            dbSchool.Enable = school.Enable;
            var state = _context.Schools.Update(dbSchool);
            if (state.State != EntityState.Modified)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Школа не оновлена" });
            }
            await _context.SaveChangesAsync();
            if (!oldOwnerId.Equals(school.OwnerId))
            {
                AppUser newOwner = await _userManager.FindByIdAsync(school.OwnerId);
                if (newOwner != null && !(await _userManager.IsInRoleAsync(newOwner, Common.UsersConstants.schoolOwner)))
                {
                    await _userManager.AddToRoleAsync(newOwner, Common.UsersConstants.schoolOwner);
                }
                await RemoveOwnerRights(oldOwnerId);
            }
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> RemoveScoolAsync(string Id)
        {
            IdentityResult result = IdentityResult.Success;
            School dbSchool = await _context.Schools
                .Include(sch => sch.Courses)
                .Include(sc => sc.PaymentTypes)
                .FirstOrDefaultAsync(sch => sch.Id == Id);
            if (dbSchool == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Школа на знайдена" });
            }
            if (result.Succeeded && dbSchool.Courses.Count() > 0)
            {
                List<string> coursesId = dbSchool.Courses.Select(cour => cour.Id).ToList();
                foreach (string courseId in coursesId)
                {
                    if (result.Succeeded)
                    {
                        result = await _courseService.RemoveCourseAsync(courseId);
                    }
                }
            }
            _context.SaveChanges();
            if (result.Succeeded && dbSchool.PaymentTypes?.Count() > 0)
            {
                foreach (PaymentType paymentType in dbSchool.PaymentTypes)
                {
                    if (result.Succeeded && _context.PaymentTypes.Remove(paymentType).State
                                                                      != EntityState.Deleted)
                    {
                        result = IdentityResult.Failed(new IdentityError { Description = "Тип оплати не видалений" });
                    }
                }
            }
            _context.SaveChanges();
            School schoolToRemove = _context.Schools.FirstOrDefault(sch => sch.Id == dbSchool.Id);
            if (result.Succeeded && _context.Schools.Remove(schoolToRemove).State != EntityState.Deleted)
            {
                result = IdentityResult.Failed(new IdentityError { Description = "Школа не видалена" });

            }
            if (result.Succeeded)
            {
                _context.SaveChanges();
            }
            if (result.Succeeded)
            {
               await  RemoveOwnerRights(dbSchool.OwnerId);
            }
            return result;
        }

        public async Task<IdentityResult> ChengeOwnerAsync(string newOwnerId, string schoolId)
        {
            if (string.IsNullOrEmpty(newOwnerId) || string.IsNullOrEmpty(schoolId))
            {
                return IdentityResult.Failed();
            }
            AppUser newOwner = await _userManager.FindByIdAsync(newOwnerId);
            if (newOwner == null)
            {
                return IdentityResult.Failed();
            }
            School school = await _context.Schools.FirstOrDefaultAsync(scg => scg.Id == schoolId);
            if (school == null)
            {
                return IdentityResult.Failed();
            }
            if (school.OwnerId != null
                && _context.Schools.Where(sch => sch.OwnerId == school.OwnerId).ToList().Count() == 1)
            {
                AppUser oldOwner = await _userManager.FindByIdAsync(school.OwnerId);
                if (oldOwner != null && (_context.Schools
                    .Include(sch => sch.Owner)
                    .Where(sch => sch.OwnerId == oldOwner.Id)
                    .Count() == 1))
                {
                    await _userManager.RemoveFromRoleAsync(oldOwner, Common.UsersConstants.schoolOwner);
                }
            }
            if (!(await _userManager.IsInRoleAsync(newOwner, Common.UsersConstants.schoolOwner)))
            {
                await _userManager.AddToRoleAsync(newOwner, Common.UsersConstants.schoolOwner);
            }
            school.OwnerId = newOwner.Id;
            _context.Schools.Update(school);
            await _context.SaveChangesAsync();
            return IdentityResult.Success;
        }

        private async Task RemoveOwnerRights(string ownerId)
        {
            if (_context.Schools.FirstOrDefault(sch => sch.OwnerId == ownerId) == null)
            {
                AppUser owner = await _userManager.FindByIdAsync(ownerId);
                if (owner != null)
                {
                    await _userManager.RemoveFromRoleAsync(owner, Common.UsersConstants.schoolOwner);
                }
            }

        }
    }
}
