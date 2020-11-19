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
    public class SubscriptionService : ISubscriptionsService
    {
        ApplicationDbContext _context;
        public SubscriptionService(ApplicationDbContext context)
        {
            _context = context;
        }
        public IEnumerable<Subscription> GetUserSubscriptions(string userId, string schoolId, DateTime month)
        {
            return _context.Subscriptions
                .Include(cour => cour.Course).ThenInclude(us => us.CourseAppUsers).ThenInclude(use => use.AppUser)
                .Include(sub => sub.Payments)
                .Include(subs => subs.Course).ThenInclude(cour => cour.School)
                        .Where(sub =>
                            sub.AppUserId == userId
                            && sub.Period.Year == month.Year
                            && sub.Period.Month == month.Month
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

        public IEnumerable<Subscription> GetFilteredSubscriptions(string schoolId, string courseId, DateTime month, string searchByName)
        {
            IEnumerable<Subscription> subscriptions = _context.Subscriptions
                                        .Include(cour => cour.Course).ThenInclude(cou => cou.School)
                                        .Include(user => user.AppUser)
                                        .Include(pay => pay.Payments).ThenInclude(paymen => paymen.PaymentType)
                                        .ToList();
            if (!string.IsNullOrEmpty(schoolId))
            {
                subscriptions = subscriptions.Where(sub => sub.Course.SchoolId == schoolId);
            }
            if (!string.IsNullOrEmpty(courseId))
            {
                subscriptions = subscriptions.Where(sub => sub.CourseId == courseId);
            }
            if (month.Year > 2010)
            {
                subscriptions = subscriptions.Where(sub => sub.Period.Year == month.Year
                  && sub.Period.Month == month.Month);
            }
            if (!string.IsNullOrEmpty(searchByName))
            {
                subscriptions = subscriptions
                    .Where(sub =>
                    (sub.AppUser != null && sub.AppUser.FullName.ToLower().Contains(searchByName.ToLower()))
                    || (sub.AppUser == null && !string.IsNullOrEmpty(sub.FullName) && sub.FullName.ToLower().Contains(searchByName.ToLower())));

            }
            return subscriptions;
        }

        public async Task<IdentityResult> CreateSubscriptionAsync(Subscription subscription)
        {
            if (subscription.MonthSubscription
                && (_context.Subscriptions.FirstOrDefault(sub =>
                sub.AppUserId == subscription.AppUserId
                && sub.CourseId == subscription.CourseId
                && sub.Period.Year == subscription.Period.Year && sub.Period.Month == subscription.Period.Month
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
                    RemovePayment(removedPaymentId, true);
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
                if (saveChanges) _context.SaveChanges();
            }
        }

        public IEnumerable<string> GetLastMonthUserCourseIds(string userId)
        {
            DateTime prewMonth = DateTime.Now.AddMonths(-1);
            IEnumerable<Subscription> userSubscriptions = _context.Subscriptions
                .Where(sub => sub.AppUserId == userId && sub.Period.Year == prewMonth.Year&& sub.Period.Month == prewMonth.Month)
                .ToList();
            if (userSubscriptions == null || userSubscriptions.Count() == 0) return new List<string>();
            return userSubscriptions.Select(usub => usub.CourseId).ToList();
        }
    }
}
