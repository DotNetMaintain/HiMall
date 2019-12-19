﻿using Himall.Core.Plugins.Payment;
using Himall.PaymentPlugin;
using Himall.WeixinPaymentBase;
using System;
using System.Xml.Linq;

namespace Himall.Plugin.Payment.WeiXinPay_O2OSmallProg
{
    public class Service : ServiceBase, IPaymentPlugin
    {

        public string GetRequestUrl(string returnUrl, string notifyUrl, string orderId, decimal totalFee, string productInfo, string openId = null)
        {
            string timeStamp = "";
            string nonceStr = "";
            string paySign = "";

            string sp_billno = orderId;
            //当前时间 yyyyMMdd
            string date = DateTime.Now.ToString("yyyyMMdd");

            //创建支付应答对象
            RequestHandler packageReqHandler = new RequestHandler();
            //初始化
            packageReqHandler.Init();
            //packageReqHandler.SetKey(""/*TenPayV3Info.Key*/);

            timeStamp = TenPayUtil.GetTimestamp();
            nonceStr = TenPayUtil.GetNoncestr();

            Config config = Utility<Config>.GetConfig(WorkDirectory);

            //设置package订单参数
            packageReqHandler.SetParameter("appid", config.AppId);		  //公众账号ID
            packageReqHandler.SetParameter("mch_id", config.MCHID);		  //商户号
            packageReqHandler.SetParameter("nonce_str", nonceStr);                    //随机字符串
            packageReqHandler.SetParameter("body", productInfo);
            packageReqHandler.SetParameter("out_trade_no", sp_billno);		//商家订单号
            packageReqHandler.SetParameter("total_fee", ((int)(totalFee * 100)).ToString());			        //商品金额,以分为单位(money * 100).ToString()
            packageReqHandler.SetParameter("spbill_create_ip", "222.240.184.122");   //用户的公网ip，不是商户服务器IP
            packageReqHandler.SetParameter("notify_url", notifyUrl);		    //接收财付通通知的URL
            packageReqHandler.SetParameter("trade_type", "JSAPI");                      //交易类型

            //如果包含了参数Main_Mch_ID和Main_AppId则为服务商模式
            if (!string.IsNullOrEmpty(config.Main_Mch_ID) && !string.IsNullOrEmpty(config.Main_AppId))
            {
                packageReqHandler.SetParameter("sub_mch_id", config.Main_Mch_ID);//服务商商户ID
                packageReqHandler.SetParameter("sub_appid", config.Main_AppId);//服务商AppID
                packageReqHandler.SetParameter("sub_openid", string.IsNullOrWhiteSpace(openId) ? "" : openId);//用户的openId
            }
            else
            {
                packageReqHandler.SetParameter("openid", string.IsNullOrWhiteSpace(openId) ? "" : openId);//用户的openId
            }
            string sign = packageReqHandler.CreateMd5Sign("key", config.Key);
            packageReqHandler.SetParameter("sign", sign);	                    //签名

            string data = packageReqHandler.ParseXML();
            Core.Log.Debug("data=" + data);

            var result = TenPayV3.Unifiedorder(data);
            //Core.Log.Debug("result=" + result);
            var res = XDocument.Parse(result);
            if (res == null)
                throw new ApplicationException("调用统一支付出错：请求内容：" + data);
            string returnCode = res.Element("xml").Element("return_code").Value;
            if (returnCode == "FAIL")//失败
                throw new ApplicationException("预支付失败：" + res.Element("xml").Element("return_msg").Value);

            string resultCode = res.Element("xml").Element("result_code").Value;
            if (resultCode == "FAIL")
                throw new ApplicationException("预支付失败：" + res.Element("xml").Element("err_code_des").Value);

            string prepayId = res.Element("xml").Element("prepay_id").Value;


            //设置支付参数
            RequestHandler paySignReqHandler = new RequestHandler();
            //签名参数
            paySignReqHandler.SetParameter("appId", config.AppId);
            paySignReqHandler.SetParameter("nonceStr", nonceStr);
            paySignReqHandler.SetParameter("timeStamp", timeStamp);
            paySignReqHandler.SetParameter("package", "prepay_id=" + prepayId);
            paySignReqHandler.SetParameter("signType", "MD5");
            //参数END
            paySign = paySignReqHandler.CreateMd5Sign("key", config.Key);
            //设置返回参数，不参与签名
            paySignReqHandler.SetParameter("sign", paySign);
            paySignReqHandler.SetParameter("prepayId", prepayId);

            var hashtable = paySignReqHandler.GetAllParameters();
            System.Text.StringBuilder strBuilder = new System.Text.StringBuilder();
            foreach (var p in hashtable.Keys)
            {
                strBuilder.Append(p + ":" + hashtable[p]);
                strBuilder.Append(",");
            }
            string resultStr = strBuilder.ToString().TrimEnd(',');
            return resultStr;
        }


        public UrlType RequestUrlType
        {
            get { return UrlType.Page; }
        }


        public string HelpImage
        {
            get { throw new NotImplementedException(); }
        }
    }
}