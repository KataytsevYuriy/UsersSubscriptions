using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UsersSubscriptions.Models;

namespace UsersSubscriptions.Models.ViewModels
{
    public class AddSubscriptionViewModel
    {
        public AppUser Student { get; set; }
        public IEnumerable<Course> TeacherCourses { get; set; }
        public DateTime Month { get; set; }
        public Course SelectedCours { get; set; }
        public string SchoolId { get; set; }
        public IEnumerable<PaymentType> PaymentTypes { get; set; }
        public string SelectedPaymentType { get; set; }
        public int Payed { get; set; }
    }
}
