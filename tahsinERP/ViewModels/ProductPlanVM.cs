using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using tahsinERP.Models;

namespace tahsinERP.ViewModels
{
    public class ProductPlanVM
    {
        public int ID { get; set; }
        public int ProductID { get; set; }
        public double Amount { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime DueDate { get; set; }
        public bool IsTwoShiftPlan { get; set; }
        public virtual ICollection<PRODUCTPLANS_DAILY> PRODUCTPLANS_DAILY { get; set; }
        public virtual PRODUCT PRODUCT { get; set; }
    }
}