using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace tahsinERP.ViewModels
{
    public class PackingListViewModel
    {
        public int ID { get; set; }
        public int InvoiceID { get; set; }
        public int TransportTypeID { get; set; }
        public string TransportNo { get; set; }
        public string PackingListNo { get; set; }
        public string SealNo { get; set; }
        public string Comment { get; set; }
        public bool IsDeleted { get; set; }
        public Nullable<bool> InTransit { get; set; }
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
        public int ID { get; set; }
        public int PartID { get; set; }
        public Nullable<double> PieceWeight { get; set; }
        public Nullable<double> PrLength { get; set; }
        public Nullable<double> PrWidth { get; set; }
        public Nullable<double> PrHeight { get; set; }
        public Nullable<double> PrCBM { get; set; }
        public Nullable<double> PrAmount { get; set; }
        public Nullable<double> PrNetWeight { get; set; }
        public Nullable<double> PrGrWeight { get; set; }
        public string PrPackMaterial { get; set; }
        public string ScPackMaterial { get; set; }
        public Nullable<double> ScLength { get; set; }
        public Nullable<double> ScWidth { get; set; }
        public Nullable<double> ScHeight { get; set; }
        public Nullable<double> ScCBM { get; set; }
        public Nullable<double> NoOfBoxes { get; set; }
        public Nullable<double> SchedPack { get; set; }
        public Nullable<double> ScNetWeight { get; set; }
        public Nullable<double> ScGrWeight { get; set; }
        public string PalletType { get; set; }
        public int PackingListID { get; set; }
    }
}