using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using UsersSubscriptions.Models;
using UsersSubscriptions.Data;
using System.Security.Principal;
using Microsoft.EntityFrameworkCore;

namespace UsersSubscriptions.Areas.Teacher.Models
{
    public class TeacherRepository : ITeacherRepository
    {
        private ApplicationDbContext _context;
        private UserManager<AppUser> _userManager;
        private RoleManager<IdentityRole> _roleManager;
        public TeacherRepository(ApplicationDbContext ctx,
                                UserManager<AppUser> userMng,
                                RoleManager<IdentityRole> roleMng)
        {
            _context = ctx;
            _userManager = userMng;
            _roleManager = roleMng;
        }
       
        public async Task<AppUser> GetCurrentUserAsync(HttpContext context)
        {
            AppUser currentUser = await _userManager.GetUserAsync(context.User);
            return currentUser;
        }

        public async Task<AppUser> GetUserAsync(string id)
        {
            return await _userManager.FindByIdAsync(id);
        }

        public async Task<IEnumerable<Course>> GetTeacherCoursesAsync(AppUser teacher)
        {
            IEnumerable<Course> courses = await _context.Courses
                .Include(sub=>sub.Subscriptions)
                .Include(cu => cu.Subscriptions).ThenInclude(cus => cus.ConfirmedBy)
                .Include(cu => cu.Subscriptions).ThenInclude(cus => cus.PayedTo)
                .Include(cu => cu.CourseAppUsers).ThenInclude(te=>te.AppUser)
                    .Where(cap => cap.CourseAppUsers.Any(dd => dd.AppUserId == teacher.Id)).ToListAsync();
            return courses;
        }

        public async Task<Course> GetCoursInfoAsync(string id)
        {
            Course course = await _context.Courses
                .Include(cu => cu.Subscriptions).ThenInclude(cus => cus.AppUser)
                .Include(cu => cu.Subscriptions).ThenInclude(cus => cus.ConfirmedBy)//.ThenInclude(cus => cus.AppUser)
                .Include(cu => cu.Subscriptions).ThenInclude(cus => cus.PayedTo)//.ThenInclude(cus => cus.AppUser)
                .Include(teach=>teach.CourseAppUsers).ThenInclude(teachUs=>teachUs.AppUser)
                .FirstOrDefaultAsync(cour => cour.Id == id);
            return course;
        }

        public async Task<Subscription> GetSubscriptionAsync(string id)
        {
            Subscription subscription = await _context.Subscriptions
                            .Include(co => co.Course)
                            .Include(user=>user.AppUser)
                            .Include(pay => pay.PayedTo)
                            .Include(conf => conf.ConfirmedBy)
                            .FirstOrDefaultAsync(subs => subs.Id == id);
            return (subscription);
        }

        public async Task ConfirmSubscriptionAsync(AppUser teacher, string id)
        {
            Subscription subscription = await _context.Subscriptions
                            .Include(teach => teach.ConfirmedBy)
                            .FirstOrDefaultAsync(subsId => subsId.Id == id);
            if (subscription.ConfirmedBy == null)
            {
                 subscription.ConfirmedById = teacher.Id;
                 subscription.ConfirmedDatetime = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }
        public async Task ConfirmPayedSubscriptionAsync(AppUser teacher, string id, int price)
        {
            Subscription subscription = await _context.Subscriptions
                            .Include(teach => teach.ConfirmedBy)
                            .Include(payed=>payed.PayedTo)
                            .FirstOrDefaultAsync(subsId => subsId.Id == id);
            if (subscription.ConfirmedBy != null && subscription.PayedTo == null)
            {
                subscription.PayedToId = teacher.Id;
                subscription.PayedDatetime = DateTime.Now;
                subscription.WasPayed = true;
                subscription.Price = price;
                await _context.SaveChangesAsync();
            }
        }
        public async Task RemoveSubscriptionAsync(string id)
        {
            Subscription subscription =
                await _context.Subscriptions
                .Include(subs => subs.ConfirmedBy)
                .Include(subsc => subsc.PayedTo)
                .FirstOrDefaultAsync(subs => subs.Id == id);
            if (subscription != null)
            {
                _context.Subscriptions.Remove(subscription);
                await _context.SaveChangesAsync();
            }
        }

        public IEnumerable<Subscription> GetStudentSubscriptionsOfCourse(string userId, string coursId)
        {
            IEnumerable<Subscription> studentSubscriptions = _context.Subscriptions
                .Where(sub => sub.AppUserId == userId && sub.CourseId == coursId).ToList();
            return studentSubscriptions;
        }

        public CourseCalculate CourseCalculateGetSum (string courseId, DateTime month)
        {
            IEnumerable<Subscription> subscriptions = _context.Subscriptions
                .Where(sub => sub.CourseId == courseId && sub.DayStart.Year == month.Year && sub.DayStart.Month == month.Month)
                .ToList();
            int sumPayed = subscriptions.Where(sub => sub.PayedToId != null)
                .Select(sub => sub.Price)
                .Sum();
            int sumNoPayed = subscriptions.Where(sub => sub.PayedToId == null)
                .Select(sub => sub.Price)
                .Sum();
            int quantity = subscriptions.Count();
             CourseCalculate courseCalculate = new CourseCalculate
            {
                PayedSum = sumPayed,
                NoPayedSum = sumNoPayed,
                QuantityStudents = quantity,
            };
            return courseCalculate;
        }
    }
}
