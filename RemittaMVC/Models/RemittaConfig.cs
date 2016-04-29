using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RemittaMVC.Models
{
    public class RemittaConfig
    {
        public static string MERCHANTID = "2547916";
        public static string SERVICETYPEID = "4430731";
        public static string APIKEY = "1946";
        public static string GATEWAYURL = "http://www.remitademo.net/remita/ecomm/split/init.reg";
        public static string CHECKSTATUSURL = "http://www.remitademo.net/remita/ecomm";
        public static string RESPONSEURL = "http://localhost:16652/WebPay/Reciept";
        public static string RRRGATEWAYPAYMENTURL = "http://www.remitademo.net/remita/ecomm/finalize.reg";
    }
}