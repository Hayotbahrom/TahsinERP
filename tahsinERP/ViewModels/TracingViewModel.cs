using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using tahsinERP.Models;

namespace tahsinERP.ViewModels
{
    public class TracingViewModel
    {
        public int ID { get; set; }
        public string TransportNo { get; set; }
        public DateTime LastIssueDateTime { get; set; }
        public TRACING LastTracing { get; set; }
        public List<TRACING> Tracings { get; set; }
    }
}