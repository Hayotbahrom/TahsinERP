﻿using System.Collections.Generic;
using System;

namespace tahsinERP.ViewModels
{
    public class SContractViewModel
    {
        public int ID { get; set; }
        public string ContractNo { get; set; }
        public int CustomerID { get; set; }
        public DateTime IssuedDate { get; set; }
        public DateTime DueDate { get; set; }
        public string Incoterms { get; set; }
        public string PaymentTerms { get; set; }
        public string Currency { get; set; }
        public decimal Amount { get; set; }
        public List<SContractProductViewModel> ProductList { get; set; }
    }

    public class SContractProductViewModel
    {
        public int ID { get; set; }
        public int SContractID { get; set; }
        public int ProductID { get; set; }
        public string ProductPNo { get; set; }
        public double PiecePrice { get; set; }
        public int UnitID { get; set; }
        public int Amount { get; set; }
        public ProductViewModel PRODUCT { get; set; }
    }

    public class ProductViewModel
    {
        public int ID { get; set; }
        public string PNo { get; set; }
        public string Name { get; set; }
    }
}
