//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace tahsinERP.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class PARTPACK
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public PARTPACK()
        {
            this.PART1738DOCS = new HashSet<PART1738DOCS>();
            this.PARTS = new HashSet<PART>();
        }
    
        public int ID { get; set; }
        public Nullable<int> PartID { get; set; }
        public string PrPackMaterial { get; set; }
        public string Securement { get; set; }
        public string Dunnage { get; set; }
        public Nullable<double> PrLength { get; set; }
        public Nullable<double> PrWidth { get; set; }
        public Nullable<double> PrHeight { get; set; }
        public Nullable<double> PrWeight { get; set; }
        public Nullable<double> PrPackQty { get; set; }
        public string ScPackMaterial { get; set; }
        public Nullable<double> ScLength { get; set; }
        public Nullable<double> ScWidth { get; set; }
        public Nullable<double> ScHeight { get; set; }
        public Nullable<double> ScWeight { get; set; }
        public string PalletType { get; set; }
        public Nullable<double> PltLength { get; set; }
        public Nullable<double> PltWidth { get; set; }
        public Nullable<double> PltHeight { get; set; }
        public Nullable<double> PltWeight { get; set; }
        public Nullable<double> ScPackQty { get; set; }
        public Nullable<bool> IsActive { get; set; }
        public Nullable<System.DateTime> RegDate { get; set; }
        public Nullable<bool> IsDeleted { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PART1738DOCS> PART1738DOCS { get; set; }
        public virtual PART PART { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PART> PARTS { get; set; }
    }
}
