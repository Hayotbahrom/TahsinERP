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
    
    public partial class F_WAYBILLS
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public F_WAYBILLS()
        {
            this.F_WAYBILL_DOCS = new HashSet<F_WAYBILL_DOCS>();
            this.PART_WRHS_INCOMES = new HashSet<PART_WRHS_INCOMES>();
        }
    
        public int ID { get; set; }
        public string WaybillNo { get; set; }
        public Nullable<int> ContractID { get; set; }
        public Nullable<int> TransportTypeID { get; set; }
        public Nullable<int> InvoiceID { get; set; }
        public Nullable<double> CBM { get; set; }
        public Nullable<double> GrWeight { get; set; }
        public string Description { get; set; }
        public Nullable<bool> IsDeleted { get; set; }
        public Nullable<double> Amount { get; set; }
        public Nullable<System.DateTime> IssueDate { get; set; }
    
        public virtual F_CONTRACTS F_CONTRACTS { get; set; }
        public virtual F_TRANSPORT_TYPES F_TRANSPORT_TYPES { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<F_WAYBILL_DOCS> F_WAYBILL_DOCS { get; set; }
        public virtual P_INVOICES P_INVOICES { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PART_WRHS_INCOMES> PART_WRHS_INCOMES { get; set; }
    }
}
