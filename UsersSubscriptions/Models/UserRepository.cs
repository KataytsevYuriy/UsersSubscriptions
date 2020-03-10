using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UsersSubscriptions.Data;
using UsersSubscriptions.Models;

namespace UsersSubscriptions.Models
{

    public class UserRepository :IUserRepository
    {
        private ApplicationDbContext _context;
        private UserManager<AppUser> _userManager;
        private RoleManager<IdentityRole> _roleManager;
        public UserRepository(ApplicationDbContext ctx,
                                UserManager<AppUser> manager,
                                RoleManager<IdentityRole> roleManager)
        {
            _userManager = manager;
            _roleManager = roleManager;
            _context = ctx;
        }
        
        public async Task<AppUser> GetCurentUser(HttpContext context)
        {
            AppUser user = await _userManager.GetUserAsync(context.User);
            return user;
        }

        public IEnumerable<Course> GetAllCourses()
        {
            return _context.Courses.Include(c => c.CourseAppUsers).ThenInclude(u=>u.AppUser).ToList();
        }

        public async Task<Course> GetCourse(string id)
        {
            return (await _context.Courses.FindAsync(id));
        }

        public AppUser GetUserCourses(string id)
        {
            AppUser user = _context.Users.Include(s => s.Subscriptions)
                                    .ThenInclude(c => c.Course).ThenInclude(cu=>cu.CourseAppUsers)
                                    .ThenInclude(au=>au.AppUser)
                                .Include(sub=>sub.Subscriptions).ThenInclude(conf=>conf.ConfirmedByTeacher)
                                    .FirstOrDefault(i => i.Id == id);
            return (user);
        }

        public async Task CreateSubscription(Subscription subscription)
        {
            if ((subscription.DayStart.Year == DateTime.Now.Year
                        && subscription.DayStart.Month >= DateTime.Now.Month)
                 || (subscription.DayStart.Year > DateTime.Now.Year))
            {
                if (!IsSubscriptionExist(subscription))
                {
                    await _context.Subscriptions.AddAsync(subscription);
                    await _context.SaveChangesAsync();
                } else
                {
                    // exception Subscription to this month is alreday exist
                }
            } else
            {
                //exception Date out of range
            }
        }

        private bool IsSubscriptionExist(Subscription subscription)
        {
            if (_context.Subscriptions.Where(dbSubscription =>

                  dbSubscription.CourseId == subscription.CourseId &&
                  dbSubscription.DayStart.Year == subscription.DayStart.Year &&
                  dbSubscription.DayStart.Month == subscription.DayStart.Month)
                  .Count()==0)
            {
                return false;
            }
            return true;
        }

        public async Task<Subscription> GetSubscriptionAsync(string id)
        {
            return await _context.Subscriptions.Include(subs => subs.Course)
                .FirstOrDefaultAsync(subs => subs.Id == id);
        }

        public async Task DeleteSubscription(string id)
        {
            Subscription subscription = await _context.Subscriptions
                    .FirstOrDefaultAsync(subs => subs.Id == id);
           if( subscription != null)
            {
                _context.Subscriptions.Remove(subscription);
                await _context.SaveChangesAsync();
            } else
            {
                //Subscription not Found
            }
        }
    }
}
