using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace UsersSubscriptions.Common
{
    public class UsersConstants
    {
        //Roles
        public const string user = "Student";
        public const string admin = "Admin";
        public const string teacher = "Teacher";
        public const string schoolOwner = "SchoolOwner";

        //QRCode
        public const int qrCodeImageSize = 10;
        //PaymentTypes
        public const int pTPriorityCount = 11;
        //View Constants
        public static List<string> monthes = new List<string>{"Січень", "Лютий", "Березень", "Квітень", "Травень", "Червень",
"Липень", "Серпень", "Вересень", "Жовтень", "Листопад",  "Грудень"};
        public const int startYear = 2019;

        public const string redirectPayPageController = "Teacher";
        public const string redirectPayPageAction = "PayForSchool";
    }
}
