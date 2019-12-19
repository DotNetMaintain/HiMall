using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Himall.IServices;
using Himall.Web.Framework;

namespace Himall.Web.Areas.Admin.Controllers
{
    public class OrderRefundsController : BaseWebController
    {
        private IRefundService _iRefundService;
        public OrderRefundsController(IRefundService iRefundService)
        {
            _iRefundService = iRefundService;
        }
        // GET: SellerAdmin/OrderRefunds
        public ActionResult Index()
        {
            return View();
        }
        //[HttpPost]
        public JsonResult ConfirmRefunds(long refundId, string managerRemark, string name, double returnmoney = 0)
        {
            decimal returnmoneys = new decimal(returnmoney);
            Core.Log.Debug("refundId11========" + refundId + "managerRemark=====" + managerRemark + "returnmoney=====" + returnmoney);
            string webRoot = CurrentUrlHelper.CurrentUrlNoPort();
            //获取异步通知地址
            string notifyurl = webRoot + "/Pay/RefundNotify/{0}";

            string refundurl = _iRefundService.ConfirmRefund(refundId, managerRemark, name, notifyurl, returnmoneys);

            return Json("true", JsonRequestBehavior.AllowGet);
        }
    }
}