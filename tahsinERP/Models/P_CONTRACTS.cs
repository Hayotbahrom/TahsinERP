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
    
    public partial class P_CONTRACTS
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public P_CONTRACTS()
        {
            this.P_CONTRACT_DOCS = new HashSet<P_CONTRACT_DOCS>();
            this.P_CONTRACT_PARTS = new HashSet<P_CONTRACT_PARTS>();
            this.P_ORDERS = new HashSet<P_ORDERS>();
        }
    
        public int ID { get; set; }
        public string ContractNo { get; set; }
        public System.DateTime IssuedDate { get; set; }
        public int CompanyID { get; set; }
        public int SupplierID { get; set; }
        public string Currency { get; set; }
        public Nullable<double> Amount { get; set; }
        public string Incoterms { get; set; }
        public string PaymentTerms { get; set; }
        public System.DateTime DueDate { get; set; }
        public Nullable<bool> IsDeleted { get; set; }
        public string IDN { get; set; }
    
        public virtual COMPANy COMPANy { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<P_CONTRACT_DOCS> P_CONTRACT_DOCS { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<P_CONTRACT_PARTS> P_CONTRACT_PARTS { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<P_ORDERS> P_ORDERS { get; set; }
        public virtual SUPPLIER SUPPLIER { get; set; }
    }
}
