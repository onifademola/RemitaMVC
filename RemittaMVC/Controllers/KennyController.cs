using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RemittaMVC.Controllers
{
    public class KennyController : Controller
    {
        // GET: Kenny
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Pay(string refId, string token)
        {
            //var param = new PaymentForm1M();
            //var param = new PaymentForm2M();
            var param = new PaymentPostModelM();
            //Todo: Pls lets allow the payment to be done through no data, session data or Json request based on the login cookie

            ////==================WebPay===============================
            //param.Amount = 5000;
            //param.AuditTime = DateTime.Now;
            //param.AuditUser = "MeMeMe";
            //param.CategorySubscriber = "SCHL";
            //param.Description = "The School Fee for Primary 5";
            //param.Email = "odeyinkao@yahoo.com";
            //short cty=156;
            //param.IpAddress = Help.Help.GetEssentials(out cty);
            //param.CountryReqFrom = cty;
            //param.pay_item_id = 101;
            //param.product_id = 4220;
            //param.TransactionId = 2222;
            //param.PostTransId = 27;
            //param.ProviderId = (short) Provider.Type.WebPay;
            //param.PhoneNo = 07065518389 + "";
            //param.ProductCode = "Sch-Fee";
            //param.RefIdFromSubscriber = "Rf454666";
            //param.SubscriberId = "234";
            //param.NotifyUrl = "http://localhost:3962/MPOPay/PayresultWebPay";
            ////====================================================================

            //==================VoguePay MockData===============================
            param.EffectiveAmount = 5000;
            param.AuditTime = DateTime.Now;
            param.AuditUser = 1;
            param.CategorySubscriber = "SCHL";
            param.Description = "The School Fee for Primary 5";
            param.Email = "odeyinkao@yahoo.com";
            short cty = 156;
            param.IpAddress = Help.Help.GetEssentials(out cty);
            param.CountryReqFrom = cty;

            param.TransactionId = 2222;
            param.PostTransId = 2;
            param.ProviderId = (short)Provider.Type.VoguePay;
            param.PhoneNo = 07065518389 + "";
            param.ProductCode = "Sch-Fee";
            param.RefIdFromSubscriber = "Rf454666";
            param.SubscriberId = 234;
            param.NotifyUrl = "http://localhost:3962/MPOPay/NotificationVogue";
            param.SuccessUrl = "http://localhost:3962/MPOPay/PaysuccessVogue";
            param.FailUrl = "http://localhost:3962/MPOPay/PayfailureVogue";
            //Increase PostTransaction Id
            param.PostTransId = param.PostTransId + 1;
            //====================================================================

            ViewData["Provider"] = new SelectList(_codeRepo.GetProviders(), "CodeId", "name", param.ProviderId);

            _payRepo.MakeIncoReqLogOnTrasTable(param);
            if (param.Equals(null))
            {
                //Log Recipient of Transaction
                _payRepo.MakeLog(new LoggingM
                {
                    LogType = LoggingTypes.InCominReq,
                    Amount = param.EffectiveAmount,
                    AuditUser = param.AuditUser,
                    IpAddress = param.IpAddress,
                    CountryReqFrom = param.CountryReqFrom,
                    Status = 1,
                    TransactionId = param.TransactionId,
                    PostTransactionId = param.PostTransId
                });
            }

            return View(param);
        }

        [HttpPost]
        public ActionResult Pay(PaymentPostModelM param, string transaction_id/*This id is used to resolve vogue pay bug*/)
        {
            //Todo: To be removed when issues are solved with voguepay with thier redirect bug
            if (!string.IsNullOrEmpty(transaction_id))
            {
                return RedirectToAction("NotificationVogue", "MPOPay", new { transaction_id = transaction_id });
            }

            //Todo=========================================================

            // ActionResult actresult = View(param);
            if (ModelState.IsValid)
            {
                //Todo Update Transaction with the outgoing request time
                _payRepo.MakeOutgReqLogOnTrasTable(param);
                //LoggingM the sending of the Transaction to third party
                _payRepo.MakeLog(new LoggingM
                {
                    LogType = LoggingTypes.OutGoingReq,
                    Amount = param.EffectiveAmount,

                    AuditUser = param.AuditUser,
                    IpAddress = param.IpAddress,
                    CountryReqFrom = param.CountryReqFrom,
                    Status = 1,
                    TransactionId = param.TransactionId,
                    PostTransactionId = param.PostTransId
                });

                //Todo: create the type of data to send here.

                switch (param.ProviderId)
                {
                    case (short)Provider.Type.WebPay:
                        const string macString = "199F6031F20C63C18E2DC6F9CBA7689137661A05ADD4114ED10F5AFB64BE625B6A9993A634F590B64887EEB93FCFECB513EF9DE1C0B53FA33D287221D75643AB";

                        var WPay = new WebPaySendSM
                        {
                            Amount = param.EffectiveAmount,
                            Hash = SendHttpAuto.GetWebPayHash(param, macString),
                            Currency = 556,
                            PayItemId = param.pay_item_id,
                            ProductId = param.product_id,
                            SiteRedirectUrl = param.NotifyUrl,
                            TxnRef = param.CategorySubscriber + "|" + param.SubscriberId + "|" + param.RefIdFromSubscriber + "|" + param.ProductCode + "|" + param.ProviderId + "|" + param.TransactionId + "|" + param.PostTransId,
                            ActionUrl = "https://stageserv.interswitchng.com/test_paydirect/pay"
                        };

                        return View("WebPaySFormat", WPay);
                        break;

                    case (short)Provider.Type.VoguePay:
                        var VPay = new VoguePaySendSM
                        {
                            FailUrl = param.FailUrl,
                            Memo = param.Description,
                            MerchantRef = param.CategorySubscriber + "|" + param.SubscriberId + "|" + param.RefIdFromSubscriber + "|" + param.ProductCode + "|" + param.ProviderId + "|" + param.TransactionId + "|" + param.PostTransId,
                            NotifyUrl = param.NotifyUrl,
                            SuccessUrl = param.SuccessUrl,
                            Total = param.EffectiveAmount,
                            VMerchantId = "Demo"
                        };
                        return View("VoguePaySFormat", VPay);
                        break;
                }

                // RedirectToRoute("");
            }

            ViewData["Provider"] = new SelectList(_codeRepo.GetProviders(), "CodeId", "name", param.ProviderId);
            //increament the posttransaction id
            param.PostTransId = param.PostTransId + 1;

            _payRepo.MakeIncoReqLogOnTrasTable(param);
            //Log on Transaction
            _payRepo.MakeLog(new LoggingM
            {
                LogType = LoggingTypes.InCominReq,
                Amount = param.EffectiveAmount,

                AuditUser = param.AuditUser,
                IpAddress = param.IpAddress,
                CountryReqFrom = param.CountryReqFrom,
                Status = 1,
                TransactionId = param.TransactionId,
                PostTransactionId = param.PostTransId
            });

            return View(param);
        }
    }
}