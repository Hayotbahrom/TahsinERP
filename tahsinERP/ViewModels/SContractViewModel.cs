using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace tahsinERP.ViewModels
{
    // General ViewModel
    public class SContractViewModel
    {
        public int ID { get; set; }

        [Required]
        public string ContractNo { get; set; }

        [DataType(DataType.Date)]
        public DateTime IssuedDate { get; set; }

        [Required]
        public int CompanyID { get; set; }

        [Required]
        public int CustomerID { get; set; }

        public string Currency { get; set; }

        public int Amount { get; set; }

        public string Incoterms { get; set; }

        public string PaymentTerms { get; set; }

        [DataType(DataType.Date)]
        public DateTime DueDate { get; set; }

        public bool IsDeleted { get; set; }

        public List<SContractProductViewModel> Products { get; set; }

        public SContractViewModel()
        {
            Products = new List<SContractProductViewModel>();
        }
    }

    // Product ViewModel
    public class SContractProductViewModel
    {
        public int ID { get; set; }

        [Required]
        public int ContractID { get; set; }

        [Required]
        public int ProductID { get; set; }

        [Required]
        public double PiecePrice { get; set; }

        public string Unit { get; set; }

        [Required]
        public double Amount { get; set; }
    }
}
