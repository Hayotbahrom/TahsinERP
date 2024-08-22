using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using tahsinERP.Models;

namespace tahsinERP.ViewModels
{
    public class PackingListViewModel
    {
        public int InvoiceID { get; set; }
        [Required]
        public int TransportTypeID { get; set; }
        [Required]
        public string TransportNo { get; set; }
        [Required]
        public string PackingListNo { get; set; }
        public string SealNo { get; set; }
        public string Comment { get; set; }
        public Nullable<double> TotalCBM { get; set; }
        public Nullable<double> TotalGrWeight { get; set; }
        public Nullable<double> TotalNetWeight { get; set; }
        public List<PackingListPart> Parts { get; set; }

        public PackingListViewModel()
        {
            Parts = new List<PackingListPart>();
        }
    }
    public class PackingListPart
    {
        public int PartID { get; set; }
        public PART Part { get; set; }
        [Required]
        public Nullable<double> PieceWeight { get; set; }
        [Required]
        public Nullable<double> PrLength { get; set; }
        [Required]

        public Nullable<double> PrWidth { get; set; }
        [Required]
        public Nullable<double> PrHeight { get; set; }
        public Nullable<double> PrCBM { get; set; }
        [Required]
        public Nullable<double> PrAmount { get; set; }
        [Required]
        public Nullable<double> PrNetWeight { get; set; }
        [Required]
        public Nullable<double> PrGrWeight { get; set; }
        //public string PrPackMaterial { get; set; }
/*      public string ScPackMaterial { get; set; }
        public Nullable<double> ScLength { get; set; }
        public Nullable<double> ScWidth { get; set; }
        public Nullable<double> ScHeight { get; set; }
        public Nullable<double> ScCBM { get; set; }
        public Nullable<double> NoOfBoxes { get; set; }
        public Nullable<double> SchedPack { get; set; }
        public Nullable<double> ScNetWeight { get; set; }
        public Nullable<double> ScGrWeight { get; set; }
        public string PalletType { get; set; }*/
        public int PackingListID { get; set; }
    }
}