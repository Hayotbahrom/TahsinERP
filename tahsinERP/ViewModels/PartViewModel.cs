using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace tahsinERP.ViewModels
{
    public class PartViewModel
    {
        public int ID { get; set; }
        [Required]
        public string PNo { get; set; }
        public string PName { get; set; }
        public double PWeight { get; set; }
        public double PLength { get; set; }
        public double PWidth { get; set; }
        public double PHeight { get; set; }
        public string Unit { get; set; }
        [Display(Name = "Selected Part Type")]
        public string Type { get; set; }
        [DataType(DataType.Text)]
        public string Description { get; set; }
        public bool IsDeleted { get; set; }
        public int PackID { get; set; }
        public int ShopID { get; set; }
        public string ShopName { get; set; }
        public bool IsInHouse { get; set; }
        public string Marka { get; set; }
        public string Standart { get; set; }
        public string Coating { get; set; }
        public string Grade { get; set; }
        public double Gauge { get; set; }
        public double Thickness { get; set; }
        public double Pitch { get; set; }
        [DataType(DataType.Upload)]
        public HttpPostedFileBase File;
        public List<string> PartTypes { get; set; }
    }
}