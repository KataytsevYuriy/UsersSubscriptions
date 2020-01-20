using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UsersSubscriptions.Models
{
    public class Course
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
    }

    public class CourseTaechers
    {
        public string Id { get; set; }
        public string CourseId { get; set; }
        public string UserId { get; set; }
    }
}
