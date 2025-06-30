using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace tahsinERP.ViewModels.Home
{
    public class StatisticsVM
    {
        public int NoOfParentBOMs{ get; set; }
        public int NoOfProducts { get; set; }
        public int NoOfParts { get; set; }
        public int NoOfUsers { get; set; }
        public int NoOfRoles { get; set; }
    }
}