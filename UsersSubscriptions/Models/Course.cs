using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace UsersSubscriptions.Models
{
    public class Course
    {
        [StringLength(64)]
        public string Id { get; set; }
        [StringLength(50)]
        public string Name { get; set; }
        public bool IsActive { get; set; }
    }

    public class CourseTaechers
    {
        [StringLength(64)]
        public string Id { get; set; }

        [StringLength(64)]
        public string CourseId { get; set; }
        public Course Course { get; set; }

        [StringLength(64)]
        public string UserId { get; set; }
        public AppUser Teachers { get; set; }
    }
}
