using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RemittaMVC.Models
{
    public class RemittaResponseModels
    {
    }

    public class RemittaResponse
    {
        public string orderId { get; set; }
        public string RRR { get; set; }
        public string status { get; set; }
        public string message { get; set; }
        public string transactiontime { get; set; }
        public string merchantId { get; set; }

    }

    public class SplitResponseVO
    {
        public string orderId { get; set; }
        public string RRR { get; set; }
        public string status { get; set; }
        public string statuscode { get; set; }
    }
}