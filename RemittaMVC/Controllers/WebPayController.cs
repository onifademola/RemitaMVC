using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using RemittaMVC.Models;
using System.Net;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;

namespace RemittaMVC.Controllers
{
    public class WebPayController : Controller
    {
        // GET: WebPay
        public string merchant_id =  RemittaConfig.MERCHANTID;
        public string serviceType_id = RemittaConfig.SERVICETYPEID;
        public string apiKey = RemittaConfig.APIKEY;
        public string gatewayURL = RemittaConfig.GATEWAYURL;
        public string returnedURLReceiptPage = RemittaConfig.RESPONSEURL;
        public string rrrGatewayPaymentURL = RemittaConfig.RRRGATEWAYPAYMENTURL;
        public string order_Id;
        public string totalAmount;
        public string name;
        public string phone;
        public string email;
        public string status;
        public string remitaRetrivalReference;
        public string statuscode;
        public string returnedURL;
        public string testUrl;

        public ActionResult Index()
        {            
            return View();
        }

        [HttpPost]
        public ActionResult Index(PaymentModel payMod)
        {
            returnedURL = HttpContext.Request.Url.Scheme + "://" + HttpContext.Request.Url.Authority + HttpContext.Request.ApplicationPath + returnedURLReceiptPage;

            WebProxy webProxy = new WebProxy("192.9.200.10", 3128);
            webProxy.BypassProxyOnLocal = false;
            name = payMod.name;
            email = payMod.email;
            phone = payMod.phone;
            totalAmount = "150";
            long milliseconds = DateTime.Now.Ticks;
            order_Id = milliseconds.ToString();
            string hash_string = merchant_id + serviceType_id + order_Id + totalAmount + returnedURL + apiKey;
            string hashed = SHA512(hash_string);
            string jsondata = "";
            string json = "{\"merchantId\":\"" + merchant_id + "\",\"serviceTypeId\":\"" + serviceType_id + "\",\"totalAmount\":\"" + totalAmount + "\",\"hash\":\"" + hashed + "\",\"orderId\":\"" + order_Id + "\",\"responseurl\":\"" + returnedURL + "\",\"payerName\":\"" + name + "\",\"payerEmail\":\"" + email + "\",\"payerPhone\":\"" + phone + "\",\"lineItems\":[ {\"lineItemsId\":\"881004944703\",\"beneficiaryName\":\"Department of Software Development\",\"beneficiaryAccount\":\"0230084841038\",\"bankCode\":\"000\",\"beneficiaryAmount\":\"150\",\"deductFeeFrom\":\"1\"}]}";
            using (var client = new WebClient())
            {
                client.Headers[HttpRequestHeader.Accept] = "application/json";
                client.Headers[HttpRequestHeader.ContentType] = "application/json";

                //Setup the WebClient to route packets through the proxy instead of directly
                client.Proxy = webProxy;
                try
                {

                    jsondata = client.UploadString(gatewayURL, "POST", json);

                }
                catch (Exception ex)
                {

                    throw ex;
                }
            }
            jsondata = jsondata.Replace("jsonp(", "");
            jsondata = jsondata.Replace(")", "");
            SplitResponseVO result = JsonConvert.DeserializeObject<SplitResponseVO>(jsondata);
            if (result != null)
            {
                status = result.status;
                remitaRetrivalReference = result.RRR.Trim();
                statuscode = result.statuscode;

                var model = new PaySuccessModel 
                {
                    status = status,
                    remitaRetrivalReference = remitaRetrivalReference,
                    statuscode = statuscode
                };

                return View("PaymentSuccess", model);
            }


            if (statuscode != null && statuscode.Equals("025"))
            {
                var model = new PayFailModel
                {
                    merchantId = merchant_id,
                    returnedURL = returnedURL,
                    remitaRetrivalReference = remitaRetrivalReference,
                    rrrPaymenthash_string = merchant_id + remitaRetrivalReference + apiKey
                    //rrrPaymentSHA = SHA512(rrrPaymenthash_string),
                    // rrr = rrrPaymentSHA
                    //form1.Action = rrrGatewayPaymentURL;
                };

                return View("PaymentFail", model);
            }

            return View();
        }

        public ActionResult PaymentSuccess()
        {
            return View();
        }

        public ActionResult PaymentFail()
        {
            return View();
        }

        public ActionResult SimplePost()
        {
            return View();
        }

              
        public ActionResult Pay(SimplePayModel payMod)
        {
            string name = payMod.name;
            string email = payMod.email;
            string phone = payMod.phone;
            double amount = payMod.amount;
            // TextBox paymentType = (TextBox)PreviousPage.FindControl("paymentType");
            //payment_type = (DropDownList)PreviousPage.FindControl("paymentType");
            string response_url = returnedURLReceiptPage;
            long milliseconds = DateTime.Now.Ticks;
            order_Id = milliseconds.ToString();
            string hash_string = merchant_id + serviceType_id + order_Id + amount + response_url + apiKey;
            System.Security.Cryptography.SHA512Managed sha512 = new System.Security.Cryptography.SHA512Managed();
            Byte[] EncryptedSHA512 = sha512.ComputeHash(System.Text.Encoding.UTF8.GetBytes(hash_string));
            sha512.Clear();
            string hashed = BitConverter.ToString(EncryptedSHA512).Replace("-", "").ToLower();

            ViewBag.payerName = payMod.name;
            ViewBag.payerEmail = payMod.email;
            ViewBag.payerPhone = payMod.phone;
            ViewBag.orderId = order_Id;
            ViewBag.merchantId = merchant_id;
            ViewBag.serviceTypeId = serviceType_id;
            ViewBag.responseUrl = response_url;
            ViewBag.amt = payMod.amount;
            ViewBag.hash = hashed;

            return View();
        }

        
        public ActionResult Reciept()
        {
            string merchant_id = RemittaConfig.MERCHANTID;
            string apiKey = RemittaConfig.APIKEY;
            string hashed;
            string order_Id;
            string checkstatusurl = RemittaConfig.CHECKSTATUSURL;

            order_Id = Request.QueryString["orderID"].ToString();
            string hash_string = order_Id + apiKey + merchant_id;
            System.Security.Cryptography.SHA512Managed sha512 = new System.Security.Cryptography.SHA512Managed();
            Byte[] EncryptedSHA512 = sha512.ComputeHash(System.Text.Encoding.UTF8.GetBytes(hash_string));
            sha512.Clear();
            hashed = BitConverter.ToString(EncryptedSHA512).Replace("-", "").ToLower();
            string url = checkstatusurl + "/" + merchant_id + "/" + order_Id + "/" + hashed + "/" + "orderstatus.reg";
            string jsondata = new WebClient().DownloadString(url);
            RemittaResponse result = JsonConvert.DeserializeObject<RemittaResponse>(jsondata);
            
            ViewBag.message = result.message;
            ViewBag.rrr = result.RRR;
            ViewBag.statuscode = result.status;
            ViewBag.merchantid = result.merchantId;
            ViewBag.orderid = result.orderId;
            ViewBag.transactionTime = result.transactiontime;

            return View();
        }

        public void MakePayment(PaymentModel payMod)
        {
            returnedURL = HttpContext.Request.Url.Scheme + "://" + HttpContext.Request.Url.Authority + HttpContext.Request.ApplicationPath + returnedURLReceiptPage;

            WebProxy webProxy = new WebProxy("192.9.200.10", 3128);
              webProxy.BypassProxyOnLocal = false;
              name = payMod.name;
              email = payMod.email;
              phone = payMod.phone;
                totalAmount = "150";
                long milliseconds = DateTime.Now.Ticks;
                order_Id = milliseconds.ToString();
                string hash_string = merchant_id + serviceType_id + order_Id + totalAmount + returnedURL + apiKey;
                string hashed = SHA512(hash_string);
                string jsondata = "";
                string json = "{\"merchantId\":\"" + merchant_id + "\",\"serviceTypeId\":\"" + serviceType_id + "\",\"totalAmount\":\"" + totalAmount + "\",\"hash\":\"" + hashed + "\",\"orderId\":\"" + order_Id + "\",\"responseurl\":\"" + returnedURL + "\",\"payerName\":\"" + name + "\",\"payerEmail\":\"" + email + "\",\"payerPhone\":\"" + phone + "\",\"lineItems\":[ {\"lineItemsId\":\"881004944703\",\"beneficiaryName\":\"Department of Software Development\",\"beneficiaryAccount\":\"0230084841038\",\"bankCode\":\"000\",\"beneficiaryAmount\":\"150\",\"deductFeeFrom\":\"1\"}]}";
                using (var client = new WebClient())
                {
                    client.Headers[HttpRequestHeader.Accept] = "application/json";
                    client.Headers[HttpRequestHeader.ContentType] = "application/json";

                    //Setup the WebClient to route packets through the proxy instead of directly
                  client.Proxy = webProxy;
                    try
                    {

                        jsondata = client.UploadString(gatewayURL, "POST", json);

                    }
                    catch (Exception ex)
                    {

                        throw ex;
                    }
                }
                jsondata = jsondata.Replace("jsonp(", "");
                jsondata = jsondata.Replace(")", "");
                SplitResponseVO result = JsonConvert.DeserializeObject<SplitResponseVO>(jsondata);
                if (result != null)
                {
                    status = result.status;
                    remitaRetrivalReference = result.RRR.Trim();
                    statuscode = result.statuscode;
                }
              
            
            //if (statuscode !=null && statuscode.Equals("025"))
            //    {
            //        merchantId.Value = merchant_id;
            //        responseurl.Value = returnedURL;
            //        rrr.Value = remitaRetrivalReference;
            //        string rrrPaymenthash_string = merchant_id + remitaRetrivalReference + apiKey;
            //        string rrrPaymentSHA = SHA512(rrrPaymenthash_string);
            //        hash.Value = rrrPaymentSHA;
            //        form1.Action = rrrGatewayPaymentURL;
            //    }
            //form1.Action = rrrGatewayPaymentURL;
           
        }

        private string SHA512(string hash_string)
        {
            System.Security.Cryptography.SHA512Managed sha512 = new System.Security.Cryptography.SHA512Managed();
            Byte[] EncryptedSHA512 = sha512.ComputeHash(System.Text.Encoding.UTF8.GetBytes(hash_string));
            sha512.Clear();
            string hashed = BitConverter.ToString(EncryptedSHA512).Replace("-", "").ToLower();
            return hashed;
        }
    }
}