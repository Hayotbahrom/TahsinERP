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
    
    public partial class CUSTOMER
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public CUSTOMER()
        {
            this.S_CONTRACTS = new HashSet<S_CONTRACTS>();
            this.S_ORDERS = new HashSet<S_ORDERS>();
        }
    
        public int ID { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Country { get; set; }
        public string Telephone { get; set; }
        public string Email { get; set; }
        public string ContactPersonName { get; set; }
        public string DirectorName { get; set; }
        public Nullable<bool> IsDeleted { get; set; }
        public string DUNS { get; set; }
        public string Type { get; set; }
        public string City { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<S_CONTRACTS> S_CONTRACTS { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<S_ORDERS> S_ORDERS { get; set; }
    }
}
