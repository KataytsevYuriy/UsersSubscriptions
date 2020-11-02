using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using UsersSubscriptions.Models.ViewModels;
using UsersSubscriptions.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
//using UsersSubscriptions.Data;

namespace UsersSubscriptions.Models
{
    public class TeacherRepository : ITeacherRepository
    {
        private ApplicationDbContext _context;
        private UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public TeacherRepository(ApplicationDbContext ctx,
                                UserManager<AppUser> userMng,
                                RoleManager<IdentityRole> roleMng)
        {
            _context = ctx;
            _userManager = userMng;
            _roleManager = roleMng;
        }

        //user

        public async Task<AppUser> GetUserAsync(string id)
        {
            return await _userManager.FindByIdAsync(id);
        }

        public AppUser GetUserByPhone(string phone)
        {
            return _context.Users.FirstOrDefault(us => Regex.Replace(us.PhoneNumber, @"[^\d]+", "") == (phone));
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
                .Where(sch => sch.OwnerId==userId || sch.Courses.Any(cour => cour.CourseAppUsers.Any(cap => cap.AppUserId == userId)))
                .ToList();
            return curTeacherSchools;
        }


        public IEnumerable<AppUser> FindUserByName(string name)
        {
            name = name.ToLower();
            List<AppUser> appUsers = _context.Users.Where(user => user.FullName.ToLower().Contains(name)).OrderBy(usr => usr.FullName).ToList();
            return appUsers;
        }

        //course

        public Course GetCourse(string id)
        {
            return _context.Courses
                .Include(cour => cour.School).ThenInclude(sch => sch.Owner)
                .Include(cour => cour.CourseAppUsers).ThenInclude(appu => appu.AppUser)
                .Include(cour => cour.CoursePaymentTypes)
                .FirstOrDefault(cour => cour.Id == id); ;
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
                AllPaymentTypes = GetSchoolPaymentTyapes(course.SchoolId),
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
                    await AddTeacherToCourse(teacherId, course.Id);
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
                await AddTeacherToCourse(teacher, dbCourse.Id);
            }
            foreach (string teacher in removedTeachers)
            {
                await RemoveTeacherFromCourse(teacher, dbCourse.Id);
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

        public IEnumerable<Course> GetTeacherCourses(string teacherId, string schoolId, bool onlyActive)
        {

            IEnumerable<Course> courses = _context.Courses
                .Include(cour => cour.School)
                .Include(cu => cu.CourseAppUsers)
                .Include(cu => cu.CoursePaymentTypes)
                .ThenInclude(cpt => cpt.PaymentType)
                    .Where(cour =>
                        //cour.CourseAppUsers.Any(dd => dd.AppUserId == teacherId)&& 
                        cour.SchoolId == schoolId
                    ).ToList();
            if(!IsItThisSchoolOwner(schoolId, teacherId))
            {
                courses = courses.Where(cour => cour.CourseAppUsers.Any(cap => cap.AppUserId == teacherId)).ToList();
            }
            if (onlyActive)
            {
                courses = courses.Where(cour => cour.IsActive == true).ToList();
            }
            return courses;
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
            await _context.CourseAppUsers.AddAsync(new CourseAppUser { AppUserId = userId, CourseId = courseId });
            await _context.SaveChangesAsync();
            AppUser user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                if (!(await _userManager.IsInRoleAsync(user, Common.UsersConstants.teacher)))
                {
                    await _userManager.AddToRoleAsync(user, Common.UsersConstants.teacher);
                }
            }
        }

        async Task RemoveTeacherFromCourse(string userId, string courseId)
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
                    await RemoveTeacherFromCourse(teacherId, course.Id);
                }
            }
            if (course.Subscriptions.Count() > 0)
            {
                List<string> subsId = course.Subscriptions.Select(sub => sub.Id).ToList();
                foreach (string sub in subsId)
                {
                    RemoveSubscription(sub);
                    //if (_context.Subscriptions.Remove(sub).State != EntityState.Deleted)
                    //{
                    //    return IdentityResult.Failed(new IdentityError
                    //    { Description = "Неможливо видалити абонемент" });
                    //}
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
                        AppUser deletingTeacher = await GetUserAsync(deletingTeacherId);
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

        public bool CourseHasSubscriptions(string id)
        {
            if ((_context.Courses
                .Include(cour => cour.Subscriptions)
                .FirstOrDefault(cour => cour.Id == id))
                .Subscriptions.Count() > 0)
            {
                return true;
            }
            return false;
        }

        public IEnumerable<Payment> GetCoursePayment(string Id)
        {
            throw new NotImplementedException();
        }


        //Subscriprions

        public IEnumerable<Subscription> GetUserSubscriptions(string userId, string schoolId, DateTime month)
        {
            return _context.Subscriptions
                .Include(cour => cour.Course).ThenInclude(us => us.CourseAppUsers).ThenInclude(use => use.AppUser)
                .Include(sub => sub.Payments)
                .Include(subs => subs.Course).ThenInclude(cour => cour.School)
                        .Where(sub =>
                            sub.AppUserId == userId
                            && sub.Month.Year == month.Year
                            && sub.Month.Month == month.Month
                            && sub.Course.SchoolId == schoolId
                        )
                        .ToList();
        }

        public Subscription GetSubscription(string Id)
        {
            return _context.Subscriptions
                .Include(cour => cour.Course).ThenInclude(cau => cau.CourseAppUsers)
                .Include(cour => cour.Course).ThenInclude(cau => cau.CoursePaymentTypes).ThenInclude(cpt => cpt.PaymentType)
                .Include(sub => sub.AppUser)
                .Include(pmnt => pmnt.Payments).ThenInclude(pt => pt.PaymentType)
                .Include(pmnt => pmnt.Payments).ThenInclude(pt => pt.PayedTo)
                .FirstOrDefault(sub => sub.Id == Id);
        }

        public async Task<IdentityResult> CreateSubscriptionAsync(Subscription subscription)
        {
            if (subscription.MonthSubscription
                && (_context.Subscriptions.FirstOrDefault(sub =>
                sub.AppUserId == subscription.AppUserId
                && sub.CourseId == subscription.CourseId
                && sub.Month.Year == subscription.Month.Year && sub.Month.Month == subscription.Month.Month
            )) != null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Такий абонемент вже існує" });
            }
            var state = await _context.Subscriptions.AddAsync(subscription);
            if (state.State != EntityState.Added)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Абонемент не доданий" });
            }
            await _context.SaveChangesAsync();

            return IdentityResult.Success;
        }

        public IdentityResult UpdateSubscription(Subscription subscription, string teacherId)
        {
            Subscription dbSubscription = GetSubscription(subscription.Id);
            if (dbSubscription == null) return IdentityResult.Failed(new IdentityError { Description = "Абонемент незнайдений" });
            if (dbSubscription.PayedToId != null) dbSubscription.PayedToId = null;
            if (dbSubscription.PaymentTypeId != null) dbSubscription.PaymentTypeId = null;
            List<string> dbPaymentsId = dbSubscription.Payments.Select(pay => pay.Id).ToList();
            if (subscription.Payments != null)
            {
                foreach (Payment formPayment in subscription.Payments)
                {
                    if (formPayment.Id != null && !string.IsNullOrEmpty(formPayment.Id.ToString()))
                    {
                        dbPaymentsId.Remove(formPayment.Id);
                        AddUpdatePayment(formPayment);
                    }
                    else
                    {
                        if (formPayment.Price > 0)
                        {
                            formPayment.SubscriptionId = subscription.Id;
                            formPayment.PayedToId = teacherId;
                            AddUpdatePayment(formPayment);
                        }
                    }
                }
            }
            if (dbPaymentsId.Count() > 0)
            {
                foreach (string removedPaymentId in dbPaymentsId)
                {
                    RemovePayment(removedPaymentId,true);
                }
            }
            return IdentityResult.Success;
        }

        public void RemoveSubscription(string id)
        {
            Subscription subscription = _context.Subscriptions
                .Include(sub => sub.Payments)
                .FirstOrDefault(sub => sub.Id == id);
            if (subscription != null)
            {
                IEnumerable<Payment> payments = subscription.Payments.ToList();
                if (payments != null && payments.Count() > 0)
                {
                    foreach (Payment payment in payments)
                    {
                        RemovePayment(payment.Id, true);
                    }
                }
                _context.Subscriptions.Remove(subscription);
                _context.SaveChanges();
            }
        }


        //School
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
                        if (_context.Subscriptions.FirstOrDefault(sub => sub.PaymentTypeId == pTId) != null)
                        {
                            return IdentityResult.Failed(new IdentityError
                            { Description = "Неможливо видалити спосіб оплати доки існують абенементи сплачені цим способом" });
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
                .Include(cour => cour.Subscriptions).ThenInclude(entry=>entry.PaymentType)
                .Include(cour => cour.CourseAppUsers).ThenInclude(capu => capu.AppUser)
                .Include(cour => cour.Subscriptions).ThenInclude(sub => sub.AppUser)
                .Include(cour => cour.Subscriptions).ThenInclude(sub => sub.Payments).ThenInclude(pay=>pay.PaymentType)
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
                     .Where(sub => sub.Month.Month == month.Month && sub.Month.Year == month.Year)
                     .Select(sub => sub.Payments.Select(pay=>pay.Price).Sum()).Sum(),
                }).ToList();
            }

            var sumByPaymentType = new Dictionary<string, int>();
            var subs = courses.SelectMany(entry => entry.Subscriptions)
                .Where(sub => sub.Month.Month == month.Month && sub.Month.Year == month.Year).ToList();
            foreach(var paymentType in subs.Select(entry => entry.PaymentType).Distinct())
            {
                sumByPaymentType.Add(paymentType.Name, subs.Where(entry => entry.PaymentTypeId == paymentType.Id).Sum(entry => entry.Price));
            }

            List<Subscription> subscriptions = new List<Subscription>();
            if (!string.IsNullOrEmpty(courseId) && courses.FirstOrDefault(cour => cour.Id == courseId) != null)
            {
                subscriptions = courses.FirstOrDefault(cour => cour.Id == courseId)
                    .Subscriptions
                    .Where(sub => sub.Month.Month == month.Month && sub.Month.Year == month.Year)
                    /*.OrderBy(sub => sub.AppUser.FullName)*/.ToList();
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
                    .Where(sub => sub.Month.Month == month.Month && sub.Month.Year == month.Year)
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
            //IEnumerable<Subscription> subscriptions =
            return _context.Subscriptions
                .Include(us => us.PaymentType)
                .Include(us => us.AppUser)
                .Include(su => su.Payments).ThenInclude(pay => pay.PaymentType)
                .Where(cour => cour.CourseId == courseId
                    && cour.Month.Year == month.Year
                    && cour.Month.Month == month.Month)
                .ToList();
            //IList<Student> students = new List<Student>();
            //foreach (var subscr in TeacherSubscriptions)
            //{
            //    students.Add(new Student
            //    {
            //        StudentName = subscr.AppUser == null ? subscr.FullName : subscr.AppUser.FullName,
            //        Phone = subscr.AppUser == null ? subscr.Phone : subscr.AppUser.PhoneNumber,
            //        Price = subscr.Price,
            //        PaymentName = subscr.PaymentType?.Name

            //    });
            //}
            //return subscriptions;
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

        public IEnumerable<PaymentType> GetSchoolPaymentTyapes(string schoolId)
        {
            IEnumerable<PaymentType> paymentTypes = _context.PaymentTypes
                .Where(pt => pt.SchoolId == schoolId).OrderBy(pt => pt.Priority).ToList();
            //;
            return paymentTypes;
        }

        //PaymentType

        public IdentityResult UpdateCoursePaymentTypes(string schoolId, string courseId, List<string> pTypes)
        {
            if (string.IsNullOrEmpty(schoolId) || string.IsNullOrEmpty(courseId))
            {
                return IdentityResult.Failed(new IdentityError { Description = "Помилка школи або курса" });
            }
            School school = GetSchool(schoolId);
            if (school == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Школа не знайдена" });
            }
            if (!school.Courses.Any(cour => cour.Id == courseId))
            {
                return IdentityResult.Failed(new IdentityError
                { Description = "Цей курс не відноситься до цієї школи" });
            }
            List<string> schoolPTypes = school.PaymentTypes.Select(pt => pt.Id).ToList();
            if (schoolPTypes.Count() == 0)
            {
                return IdentityResult.Failed(new IdentityError
                { Description = "Варіанти сплати відсутні" });
            }
            if (pTypes.Except(schoolPTypes).ToList().Count() > 0)
            {
                return IdentityResult.Failed(new IdentityError
                { Description = "Варіанти сплати не відносяться до школи" });
            }
            List<string> coursePTypes = _context.CoursePaymentTypes
                .Where(cpt => cpt.CourseId == courseId)
                .Select(cpt => cpt.PaymentTypeId).ToList();
            if (pTypes.Count() > 0)
            {
                List<string> cptToAdd = pTypes.Except(coursePTypes).ToList();
                if (cptToAdd.Count() > 0)
                {
                    foreach (string ctpId in cptToAdd)
                    {
                        AddCoursePaymentType(courseId, ctpId);
                    }
                }
            }
            if (coursePTypes.Count() > 0)
            {
                List<string> cptToRemove = coursePTypes.Except(pTypes).ToList();
                if (cptToRemove.Count() > 0)
                {
                    foreach (string cptId in cptToRemove)
                    {
                        RemoveCoursePaymentType(courseId, cptId);
                    }
                }
            }
            return IdentityResult.Success;
        }

        private void AddCoursePaymentType(string courseId, string pTypeId)
        {
            _context.CoursePaymentTypes.Add(new CoursePaymentType
            {
                CourseId = courseId,
                PaymentTypeId = pTypeId,
            });
            _context.SaveChanges();
        }
        private void RemoveCoursePaymentType(string courseId, string pTypeId)
        {
            CoursePaymentType coursePT = _context.CoursePaymentTypes
                 .FirstOrDefault(cpt => cpt.CourseId == courseId && cpt.PaymentTypeId == pTypeId);
            if (coursePT != null)
            {
                _context.CoursePaymentTypes.Remove(coursePT);
                _context.SaveChanges();
            }
        }

        public void AddDefaultPaymentTypesToSchool(string schoolId)
        {
            SeedData.CreatePaymentTypesToSchool(_context, schoolId);
        }

        private void AddUpdatePayment(Payment payment)
        {
            if (payment.Id == null || string.IsNullOrEmpty(payment.Id.ToString()))
            {
                Payment newPayment = new Payment
                {
                    DateTime = DateTime.Now,
                    PayedToId = payment.PayedToId,
                    PaymentTypeId = payment.PaymentTypeId,
                    Price = payment.Price,
                    SubscriptionId = payment.SubscriptionId
                };
                _context.Payments.Add(newPayment);
                _context.SaveChanges();
            }
            else
            {
                Payment dbPayment = _context.Payments.FirstOrDefault(pay => pay.Id == payment.Id);
                if (dbPayment != null)
                {
                    dbPayment.PaymentTypeId = payment.PaymentTypeId;
                    dbPayment.Price = payment.Price;
                    _context.SaveChanges();
                }
            }
        }

        private void RemovePayment(string paymentId, bool saveChanges)
        {
            Payment paymentToRemove = _context.Payments.FirstOrDefault(pay => pay.Id == paymentId);
            if (paymentToRemove != null)
            {
                _context.Payments.Remove(paymentToRemove);
                if(saveChanges) _context.SaveChanges();
            }
        }

    }
}
