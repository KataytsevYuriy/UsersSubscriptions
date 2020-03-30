using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UsersSubscriptions.Models;

namespace UsersSubscriptions.Areas.Teacher.Models.ViewModels
{
    public class SubscriptionTeatcherViewModel
    {
        public Subscription Subscription { get; set; }
        public AppUser Student { get; set; }
        public Course Course { get; set; }
    }
}
