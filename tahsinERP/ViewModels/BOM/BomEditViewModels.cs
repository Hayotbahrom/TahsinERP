using System.Collections.Generic;
using tahsinERP.Models;

namespace tahsinERP.ViewModels.BOM
{
    public class BomEditViewModels
    {
        public string ProductPNo { get; set; }
        public string PartPno { get; set; }
        public SLITTING_NORMS SLITTING_NORMS { get; set; }
        public int SlittingID { get; set; }
        public int Slitting_After_ID { get; set; }
        public int Slitting_Before_ID { get; set; }
        public STAMPING_NORMS STAMPING_NORMS { get; set; }
        public int StampingID { get; set; }
        public int Stamping_After_ID { get; set; }
        public int Stamping_Before_ID { get; set; }
        public BLANKING_NORMS BLANKING_NORMS { get; set; }
        public int BlankingID { get; set; }
        public int Blanking_After_ID { get; set; }
        public int Blanking_Before_ID { get; set; }
        public List<WeldingParts> WeldingPart { get; set; }
        public List<string> ProccessList { get; set; }

        public BomEditViewModels()
        {
            WeldingPart = new List<WeldingParts>();
        }
    }
    public class WeldingParts
    {
        public int Welding_PartID { get; set; }
        public string PNo { get; set; }
        public double WeldingQuantity { get; set; }
    }
}