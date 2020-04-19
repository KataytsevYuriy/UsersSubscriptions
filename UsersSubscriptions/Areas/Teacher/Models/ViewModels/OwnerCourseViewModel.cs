using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UsersSubscriptions.Models;

namespace UsersSubscriptions.Areas.Teacher.Models.ViewModels
{
    public class OwnerCourseViewModel : Course
    {
        public IList<string> Teachers { get; set; }
        public OwnerCourseViewModel()
        {
            Teachers = new List<string>();
        }
    }
}
