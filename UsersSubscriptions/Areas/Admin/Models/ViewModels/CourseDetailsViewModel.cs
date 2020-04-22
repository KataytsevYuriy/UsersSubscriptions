using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UsersSubscriptions.Models;

namespace UsersSubscriptions.Areas.Admin.Models.ViewModels
{
    public class CourseDetailsViewModel:Course
    {
        public IList<string> TeachersId { get; set; }
        public CourseDetailsViewModel()
        {
            TeachersId = new List<string>();
        }
    }
}
