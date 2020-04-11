using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UsersSubscriptions.Models;

namespace UsersSubscriptions.Areas.Teacher.Models.ViewModels
{
    public class StudentInfoViewModel
    {
        public AppUser Student { get; set; }
        public IEnumerable<Subscription> Subscriptions { get; set; }
        public DateTime Month { get; set; }
    }
}
