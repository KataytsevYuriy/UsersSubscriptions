using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UsersSubscriptions.Models;
using UsersSubscriptions.DomainServices;

namespace UsersSubscriptions.Services
{
    public class CheckSchool
    {
        private ISchoolService _schoolService;
        private readonly ISession session;

        public CheckSchool(ISchoolService schoolService, ISession sessio)
        {
            session = sessio;
            _schoolService = schoolService;
        }

        public bool IsSchoolAllowed(string schoolId)
        {
            bool result = false;
            string allowedSchoolId = session.GetString("allowedSchool");
            if (string.IsNullOrEmpty(allowedSchoolId) || !allowedSchoolId.Equals(schoolId))
            {
                result = CheckSchoolPayInDB(schoolId);
                session.SetString("allowedSchool", result ? schoolId : "");
            }
            else result = true;
            return result;
        }
        private bool CheckSchoolPayInDB(string schoolId)
        {
            School school = _schoolService.GetSchool(schoolId);
            if (school == null) return false;
            if (school.Enable == false) return false;
            if (school.AllowTestUntil != null && school.AllowTestUntil > DateTime.Now) return true;
            if (!IsMonthLost(school.PayedMonth))
            {
                return true;
            }
            else
            {
                if (PayForSchool(schoolId)) return true;
            }
            return false;
        }

        private bool PayForSchool(string schoolId)
        {
            IdentityResult result = _schoolService.PayForSchool(schoolId, "");
            if (result.Succeeded) return true;
            return false;
        }

        private bool IsMonthLost(DateTime date)
        {
            DateTime now = DateTime.Now;
            if (date.Year == now.Year && date.Month >= now.Month) return false;
            if (date.Year > now.Year) return false;
            return true;
        }

        public string GetSchoolId_From_Context(string schoolUrl = "", string schoolId = "")
        {
            School school = new School();
            if (string.IsNullOrEmpty(schoolId))
            {
                string url = session.GetString("schoolUrl");
                string id = session.GetString("schoolId");
                if (string.IsNullOrEmpty(schoolUrl)) return string.IsNullOrEmpty(id) ? "" : id;
                if (url.ToLower().Equals(schoolUrl.ToLower())) return id;
                school = _schoolService.GetSchoolByUrl(schoolUrl);
            }
            else school = _schoolService.GetSchool(schoolId);
            if (school == null) return "";
            session.SetString("schoolUrl", school.UrlName);
            session.SetString("schoolId", school.Id);
            return school.Id;
        }
    }
}
