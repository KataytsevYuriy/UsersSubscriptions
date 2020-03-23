using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UsersSubscriptions.Models;

namespace UsersSubscriptions.Areas.Teacher.Models
{
    public class StudentInfoViewModel
    {
        public AppUser Student { get; set; }
        public IEnumerable<Course> Courses { get; set; }
    }
}
