using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UsersSubscriptions.Models;

namespace UsersSubscriptions.Areas.Teacher.Models.ViewModels
{
    public class AddSubscriptionViewModel
    {
        public AppUser Student { get; set; }
        public IEnumerable<Course> TeacherCourses { get; set; }
        public DateTime Month { get; set; }
        public Course SelectedCours { get; set; }
    }
}
