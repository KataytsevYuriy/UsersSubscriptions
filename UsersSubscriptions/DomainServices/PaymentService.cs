using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UsersSubscriptions.Data;
using UsersSubscriptions.Models;

namespace UsersSubscriptions.DomainServices
{
    public class PaymentService : IPaymentService
    {
        ApplicationDbContext _context;
        public PaymentService(ApplicationDbContext context)
        {
            _context = context;
        }
        public IdentityResult UpdateCoursePaymentTypes(string schoolId, string courseId, List<string> pTypes)
        {
            if (string.IsNullOrEmpty(schoolId) || string.IsNullOrEmpty(courseId))
            {
                return IdentityResult.Failed(new IdentityError { Description = "Помилка школи або курса" });
            }
            School school = _context.Schools.FirstOrDefault(sch => sch.Id == schoolId);
            if (school == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Школа не знайдена" });
            }
            if (!school.Courses.Any(cour => cour.Id == courseId))
            {
                return IdentityResult.Failed(new IdentityError
                { Description = "Цей курс не відноситься до цієї школи" });
            }
            List<string> schoolPTypes = school.PaymentTypes.Select(pt => pt.Id).ToList();
            if (schoolPTypes.Count() == 0)
            {
                return IdentityResult.Failed(new IdentityError
                { Description = "Варіанти сплати відсутні" });
            }
            if (pTypes.Except(schoolPTypes).ToList().Count() > 0)
            {
                return IdentityResult.Failed(new IdentityError
                { Description = "Варіанти сплати не відносяться до школи" });
            }
            List<string> coursePTypes = _context.CoursePaymentTypes
                .Where(cpt => cpt.CourseId == courseId)
                .Select(cpt => cpt.PaymentTypeId).ToList();
            if (pTypes.Count() > 0)
            {
                List<string> cptToAdd = pTypes.Except(coursePTypes).ToList();
                if (cptToAdd.Count() > 0)
                {
                    foreach (string ctpId in cptToAdd)
                    {
                        AddCoursePaymentType(courseId, ctpId);
                    }
                }
            }
            if (coursePTypes.Count() > 0)
            {
                List<string> cptToRemove = coursePTypes.Except(pTypes).ToList();
                if (cptToRemove.Count() > 0)
                {
                    foreach (string cptId in cptToRemove)
                    {
                        RemoveCoursePaymentType(courseId, cptId);
                    }
                }
            }
            return IdentityResult.Success;
        }

        private void AddCoursePaymentType(string courseId, string pTypeId)
        {
            _context.CoursePaymentTypes.Add(new CoursePaymentType
            {
                CourseId = courseId,
                PaymentTypeId = pTypeId,
            });
            _context.SaveChanges();
        }
        private void RemoveCoursePaymentType(string courseId, string pTypeId)
        {
            CoursePaymentType coursePT = _context.CoursePaymentTypes
                 .FirstOrDefault(cpt => cpt.CourseId == courseId && cpt.PaymentTypeId == pTypeId);
            if (coursePT != null)
            {
                _context.CoursePaymentTypes.Remove(coursePT);
                _context.SaveChanges();
            }
        }

        public void AddDefaultPaymentTypesToSchool(string schoolId)
        {
            SeedData.CreatePaymentTypesToSchool(_context, schoolId);
        }

        public School GetSchoolFinance(string schoolId)
        {
            School school = _context.Schools
                .Include(sch => sch.Courses)
                .FirstOrDefault(sch => sch.Id == schoolId);
            school.SchoolTransactions = _context.SchoolTransactions
                .Where(st => st.SchoolId == schoolId)
                .OrderByDescending(st => st.PayedDateTime)
                .ToList();
            return school;
        }

        public IdentityResult UpdateSchoolFinance(School school)
        {
            if (school == null) return IdentityResult.Failed(new IdentityError { Description = "Помилка форми" });
            if (school == null || string.IsNullOrEmpty(school.Id)) return IdentityResult.Failed(new IdentityError { Description = "Помтлка форми" });
            School dbSchool = _context.Schools.FirstOrDefault(sch => sch.Id == school.Id);
            if (dbSchool == null) return IdentityResult.Failed(new IdentityError { Description = "Школа не знайдена" });
            dbSchool.Price = school.Price;
            dbSchool.AllowTestUntil = school.AllowTestUntil;
            _context.SaveChanges();
            return IdentityResult.Success;
        }

        public IdentityResult AddSchoolTransaction(SchoolTransaction transaction)
        {
            if (_context.SchoolTransactions.Add(transaction).State == EntityState.Added)
            {
                School school = _context.Schools.FirstOrDefault(sch => sch.Id == transaction.SchoolId);
                if (school != null)
                {
                    school.Balance = transaction.NewBalance;
                    _context.SaveChanges();
                    return IdentityResult.Success;
                }
            }
            return IdentityResult.Failed(new IdentityError { Description = "Сплата не зарахована" });
        }

        public IEnumerable<PaymentType> GetSchoolPaymentTyapes(string schoolId)
        {
            IEnumerable<PaymentType> paymentTypes = _context.PaymentTypes
                .Where(pt => pt.SchoolId == schoolId).OrderBy(pt => pt.Priority).ToList();
            return paymentTypes;
        }

        public IdentityResult RemoveLastSchoolTransaction(string schoolId)
        {
            School school = _context.Schools.Include(sch => sch.SchoolTransactions).FirstOrDefault(sch => sch.Id == schoolId);
            IEnumerable<SchoolTransaction> schoolTransactions = _context.SchoolTransactions.Where(sctr => sctr.SchoolId == schoolId)
                .OrderByDescending(sctr => sctr.PayedDateTime).ToList();
            SchoolTransaction transaction = school.SchoolTransactions.OrderByDescending(sch=>sch.PayedDateTime).FirstOrDefault();
            if (transaction == null) return IdentityResult.Failed(new IdentityError { Description = "Транзакція не знайдена" });
            if (_context.SchoolTransactions.Remove(transaction).State == EntityState.Deleted)
            {
                school.Balance = transaction.OldBalance;
                if (transaction.Payed < 0)
                {
                    school.PayedMonth = new DateTime();
                    school.IsPayed = false;
                }
                _context.SaveChanges();
                return IdentityResult.Success;
            }
            return IdentityResult.Failed(new IdentityError { Description = "Транзакція не видалена" });
        }
    }
}
