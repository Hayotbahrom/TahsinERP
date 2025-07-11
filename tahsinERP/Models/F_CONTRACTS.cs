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
    
    public partial class F_CONTRACTS
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public F_CONTRACTS()
        {
            this.F_CONTRACT_DOCS = new HashSet<F_CONTRACT_DOCS>();
            this.F_WAYBILLS = new HashSet<F_WAYBILLS>();
        }
    
        public int ID { get; set; }
        public string ContractNo { get; set; }
        public int CompanyID { get; set; }
        public int ForwarderID { get; set; }
        public System.DateTime IssueDate { get; set; }
        public System.DateTime DueDate { get; set; }
        public double Amount { get; set; }
        public string Currency { get; set; }
        public string Description { get; set; }
        public bool IsDeleted { get; set; }
    
        public virtual COMPANy COMPANy { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<F_CONTRACT_DOCS> F_CONTRACT_DOCS { get; set; }
        public virtual FORWARDER FORWARDER { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<F_WAYBILLS> F_WAYBILLS { get; set; }
    }
}
