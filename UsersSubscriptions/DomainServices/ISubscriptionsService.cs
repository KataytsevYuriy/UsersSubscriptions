using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UsersSubscriptions.Models;

namespace UsersSubscriptions.DomainServices
{
    public interface ISubscriptionsService
    {
        Task<IdentityResult> CreateSubscriptionAsync(Subscription subscription);
        IEnumerable<Subscription> GetUserSubscriptions(string userId, string schoolId, DateTime month);
        Subscription GetSubscription(string Id);
        IEnumerable<Subscription> GetFilteredSubscriptions(string schoolId, string courseId, DateTime month, string searchByName);
        IdentityResult UpdateSubscription(Subscription subscription, string teacherId);
        void RemoveSubscription(string id);
        IEnumerable<string> GetLastMonthUserCourseIds(string userId);
    }
}
