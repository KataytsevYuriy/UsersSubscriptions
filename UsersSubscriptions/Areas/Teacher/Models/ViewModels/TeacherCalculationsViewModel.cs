using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UsersSubscriptions.Models;

namespace UsersSubscriptions.Areas.Teacher.Models.ViewModels
{
    public class TeacherCalculationsViewModel
    {
        public DateTime Month { get; set; }
        public IList<CourseCalculate> Courses { get; set; }
        public int SumPayed { get; set; }
        public int SumNoPayed { get; set; }
    }

    public class CourseCalculate : Course
    {
        public int PayedSum { get; set; }
        public int NoPayedSum { get; set; }
        public int QuantityStudents { get; set; }
    }
}
