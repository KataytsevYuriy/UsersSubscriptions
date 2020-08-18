using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UsersSubscriptions.Models;

namespace UsersSubscriptions.Models.ViewModels
{
    public class TeacherCoursesViewModel
    {
        public IEnumerable<Student> Students { get; set; }
        public DateTime Month { get; set; }
        public Course CurrentCourse { get; set; }
    }
    public class Student
    {
        public string StudentName { get; set; }
        public string Phone { get; set; }
        public int Price { get; set; }
        public string PaymentName { get; set; }
    }
}
