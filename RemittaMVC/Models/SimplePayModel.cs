using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RemittaMVC.Models
{
    public class SimplePayModel
    {
        public string name { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public double amount { get; set; }
    }
}