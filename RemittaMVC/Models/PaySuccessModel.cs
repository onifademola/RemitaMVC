using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RemittaMVC.Models
{
    public class PaySuccessModel
    {
        //status = result.status;
        //        remitaRetrivalReference = result.RRR.Trim();
        //        statuscode = result.statuscode;

        public string status { get; set; }
        public string remitaRetrivalReference { get; set; }
        public string statuscode { get; set; }
    }
}