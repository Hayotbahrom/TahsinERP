using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace tahsinERP.Models
{
    public class DataTableModel
    {
        public List<string> Columns { get; set; }
        public List<List<string>> Rows { get; set; }
    }
}