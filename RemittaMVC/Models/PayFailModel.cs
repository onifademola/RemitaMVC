using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RemittaMVC.Models
{
    public class PayFailModel
    {
        public string merchantId { get; set; }
        public string returnedURL { get; set; }
        public string remitaRetrivalReference { get; set; }
        public string rrrPaymenthash_string { get; set; }
        public string rrrPaymentSHA { get; set; }
        public string rrrGatewayPaymentURL { get; set; }
    }
}