using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;


namespace tahsinERP.ViewModels
{
    public class SOrderViewModel
    {
        public int Id { get; set; }
        public string OrderNo { get; set; }
        public int CustomerID { get; set; }
        public int ContractID { get; set; }
        public string Currency { get; set; }
        public string Description { get; set; }


    }

    public class SOrderProduct
    {
        public int Id { get; set; }
        public int ProductID { get; set; }
        public float Price { get; set; }
        public float Amount { get; set; }
        public float MOQ { get; set; }
        public int UnitID { get; set; }
    }
}