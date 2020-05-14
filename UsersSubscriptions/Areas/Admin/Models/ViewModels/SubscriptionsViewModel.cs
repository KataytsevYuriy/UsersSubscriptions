using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UsersSubscriptions.Models;

namespace UsersSubscriptions.Areas.Admin.Models.ViewModels
{
    public class SubscriptionsViewModel
    {
        public string Id { get; set; }
        public IEnumerable<Subscription> _Subscriptions { get; set; }
        public string SelectedSchoolId { get; set; }
        public IEnumerable<School> Schools { get; set; }
        public string SelectedCourseId { get; set; }
        public DateTime Month { get; set; }
        public string MonthStr { get; set; }
        public string SearchName { get; set; }
        public bool ShowFilter { get; set; }
    }
}
