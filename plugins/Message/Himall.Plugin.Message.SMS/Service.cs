using Himall.Core.Plugins.Message;
using Himall.MessagePlugin;
using Himall.Plugin.Message.SMS;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Serialization;

namespace Himall.Plugin.Message.SMS
{
    public class Service : ISMSPlugin
    {
        MessageStatus messageStatus;
        Dictionary<MessageTypeEnum, StatusEnum> dic = new Dictionary<MessageTypeEnum, StatusEnum>();
        public Service()
        {
            if (!string.IsNullOrEmpty(SMSCore.WorkDirectory))
            {
                InitMessageStatus();
            }
        }

        void InitMessageStatus()
        {
            //DirectoryInfo dir = new DirectoryInfo(SMSCore.WorkDirectory);
            ////查找该目录下的
            //var configFile = dir.GetFiles("Data/config.xml").FirstOrDefault();
            //if (configFile != null)
            //{
            //    using (FileStream fs = new FileStream(configFile.FullName, FileMode.Open))
            //    {
            //        XmlSerializer xs = new XmlSerializer(typeof(MessageStatus));
            //        messageStatus = (MessageStatus)xs.Deserialize(fs);
            //        dic.Clear();
            //        dic.Add(MessageTypeEnum.FindPassWord, (StatusEnum)messageStatus.FindPassWord);
            //        dic.Add(MessageTypeEnum.OrderCreated, (StatusEnum)messageStatus.OrderCreated);
            //        dic.Add(MessageTypeEnum.OrderPay, (StatusEnum)messageStatus.OrderPay);
            //        dic.Add(MessageTypeEnum.OrderRefund, (StatusEnum)messageStatus.OrderRefund);
            //        dic.Add(MessageTypeEnum.OrderShipping, (StatusEnum)messageStatus.OrderShipping);
            //        dic.Add(MessageTypeEnum.ShopAudited, (StatusEnum)messageStatus.ShopAudited);
            //        //2.4删除
            //        //  dic.Add(MessageTypeEnum.ShopSuccess, (StatusEnum)messageStatus.ShopSuccess);去掉
            //        dic.Add(MessageTypeEnum.ShopHaveNewOrder, (StatusEnum)messageStatus.ShopHaveNewOrder);
            //        dic.Add(MessageTypeEnum.ReceiveBonus, (StatusEnum)messageStatus.ReceiveBonus);
            //        dic.Add(MessageTypeEnum.LimitTimeBuy, (StatusEnum)messageStatus.LimitTimeBuy);
            //        dic.Add(MessageTypeEnum.SubscribeLimitTimeBuy, (StatusEnum)messageStatus.SubscribeLimitTimeBuy);
            //        dic.Add(MessageTypeEnum.RefundDeliver, (StatusEnum)messageStatus.RefundDeliver);

            //        #region 拼团
            //        dic.Add(MessageTypeEnum.FightGroupOpenSuccess, (StatusEnum)messageStatus.FightGroupOpenSuccess);
            //        dic.Add(MessageTypeEnum.FightGroupJoinSuccess, (StatusEnum)messageStatus.FightGroupJoinSuccess);
            //        dic.Add(MessageTypeEnum.FightGroupNewJoin, (StatusEnum)messageStatus.FightGroupNewJoin);
            //        dic.Add(MessageTypeEnum.FightGroupFailed, (StatusEnum)messageStatus.FightGroupFailed);
            //        dic.Add(MessageTypeEnum.FightGroupSuccess, (StatusEnum)messageStatus.FightGroupSuccess);
            //        #endregion
            //        //发送优惠券
            //        dic.Add(MessageTypeEnum.SendCouponSuccess, (StatusEnum)messageStatus.SendCouponSuccess);
            //    }
            //}

            DirectoryInfo dir = new DirectoryInfo(SMSCore.WorkDirectory);
            //查找该目录下的
            messageStatus = SMSCore.GetMessageStatus();

            if (messageStatus != null)
            {
                dic.Clear();
                dic.Add(MessageTypeEnum.FindPassWord, (StatusEnum)messageStatus.FindPassWord);
                dic.Add(MessageTypeEnum.OrderCreated, (StatusEnum)messageStatus.OrderCreated);
                dic.Add(MessageTypeEnum.OrderPay, (StatusEnum)messageStatus.OrderPay);
                dic.Add(MessageTypeEnum.OrderRefund, (StatusEnum)messageStatus.OrderRefund);
                dic.Add(MessageTypeEnum.OrderShipping, (StatusEnum)messageStatus.OrderShipping);
                dic.Add(MessageTypeEnum.ShopAudited, (StatusEnum)messageStatus.ShopAudited);
                //2.4删除
                //  dic.Add(MessageTypeEnum.ShopSuccess, (StatusEnum)messageStatus.ShopSuccess);去掉
                dic.Add(MessageTypeEnum.ShopHaveNewOrder, (StatusEnum)messageStatus.ShopHaveNewOrder);
                dic.Add(MessageTypeEnum.ReceiveBonus, (StatusEnum)messageStatus.ReceiveBonus);
                dic.Add(MessageTypeEnum.LimitTimeBuy, (StatusEnum)messageStatus.LimitTimeBuy);
                dic.Add(MessageTypeEnum.SubscribeLimitTimeBuy, (StatusEnum)messageStatus.SubscribeLimitTimeBuy);
                dic.Add(MessageTypeEnum.RefundDeliver, (StatusEnum)messageStatus.RefundDeliver);

                #region 拼团
                dic.Add(MessageTypeEnum.FightGroupOpenSuccess, (StatusEnum)messageStatus.FightGroupOpenSuccess);
                dic.Add(MessageTypeEnum.FightGroupJoinSuccess, (StatusEnum)messageStatus.FightGroupJoinSuccess);
                dic.Add(MessageTypeEnum.FightGroupNewJoin, (StatusEnum)messageStatus.FightGroupNewJoin);
                dic.Add(MessageTypeEnum.FightGroupFailed, (StatusEnum)messageStatus.FightGroupFailed);
                dic.Add(MessageTypeEnum.FightGroupSuccess, (StatusEnum)messageStatus.FightGroupSuccess);
                #endregion
                //发送优惠券
                dic.Add(MessageTypeEnum.SendCouponSuccess, (StatusEnum)messageStatus.SendCouponSuccess);
            }
        }

        public string WorkDirectory
        {
            set { SMSCore.WorkDirectory = value; }
        }

        public void CheckCanEnable()
        {
            MessageSMSConfig config = SMSCore.GetConfig();
            if (string.IsNullOrWhiteSpace(config.accountSid))
                throw new Himall.Core.PluginConfigException("未设置accountSid");

            if (string.IsNullOrWhiteSpace(config.authToken))
                throw new Himall.Core.PluginConfigException("未设置authToken");
        }


        public Core.Plugins.FormData GetFormData()
        {

            var config = SMSCore.GetConfig();

            var formData = new Core.Plugins.FormData()
            {
                Items = new Core.Plugins.FormData.FormItem[] { 
                   //AppKey
                   new  Core.Plugins.FormData.FormItem(){
                     DisplayName = "accountSid",
                     Name = "accountSid",
                     IsRequired = true,
                      Type= Core.Plugins.FormData.FormItemType.text,
                      Value=config.accountSid
                   },
                     new  Core.Plugins.FormData.FormItem(){
                     DisplayName = "authToken",
                     Name = "authToken",
                     IsRequired = true,
                       Type= Core.Plugins.FormData.FormItemType.text,
                       Value=config.authToken
                   }
                }
            };
            return formData;
        }



        public void SetFormValues(IEnumerable<KeyValuePair<string, string>> values)
        {
            var appKeyItem = values.FirstOrDefault(item => item.Key == "accountSid");
            if (string.IsNullOrWhiteSpace(appKeyItem.Value))
                throw new Himall.Core.PluginConfigException("accountSid不能为空");
            var appSecretItem = values.FirstOrDefault(item => item.Key == "authToken");
            if (string.IsNullOrWhiteSpace(appSecretItem.Value))
                throw new Himall.Core.PluginConfigException("authToken不能为空");
            MessageSMSConfig oldConfig = SMSCore.GetConfig();
            oldConfig.accountSid = appKeyItem.Value;
            oldConfig.authToken = appSecretItem.Value;
            SMSCore.SaveConfig(oldConfig);
        }

        public void Disable(MessageTypeEnum e)
        {
            CheckCanEnable();
            if (dic.Where(a => a.Key == e).FirstOrDefault().Value == Himall.Core.Plugins.Message.StatusEnum.Disable)
            {
                throw new Himall.Core.HimallException("该功能已被禁止，不能进行设置");
            }
            SetMessageStatus(e, StatusEnum.Close);
            SMSCore.SaveMessageStatus(messageStatus);

        }
        void SetMessageStatus(MessageTypeEnum e, StatusEnum s)
        {
            switch (e)
            {
                case MessageTypeEnum.OrderCreated:
                    messageStatus.OrderCreated = (int)s;
                    break;
                case MessageTypeEnum.FindPassWord:
                    messageStatus.FindPassWord = (int)s;
                    break;
                case MessageTypeEnum.OrderPay:
                    messageStatus.OrderPay = (int)s;
                    break;
                case MessageTypeEnum.OrderRefund:
                    messageStatus.OrderRefund = (int)s;
                    break;
                case MessageTypeEnum.OrderShipping:
                    messageStatus.OrderShipping = (int)s;
                    break;
                case MessageTypeEnum.ShopAudited:
                    messageStatus.ShopAudited = (int)s;
                    break;
                //2.4删除
                //case MessageTypeEnum.ShopSuccess:
                //    messageStatus.ShopSuccess = (int)s;
                //    break;
                case MessageTypeEnum.RefundDeliver:
                    messageStatus.RefundDeliver = (int)s;
                    break;
                case MessageTypeEnum.FightGroupOpenSuccess:
                    messageStatus.FightGroupOpenSuccess = (int)s;
                    break;
                case MessageTypeEnum.FightGroupJoinSuccess:
                    messageStatus.FightGroupJoinSuccess = (int)s;
                    break;
                case MessageTypeEnum.FightGroupNewJoin:
                    messageStatus.FightGroupNewJoin = (int)s;
                    break;
                case MessageTypeEnum.FightGroupFailed:
                    messageStatus.FightGroupFailed = (int)s;
                    break;
                case MessageTypeEnum.FightGroupSuccess:
                    messageStatus.FightGroupSuccess = (int)s;
                    break;
                case MessageTypeEnum.SendCouponSuccess://发送优惠券
                    messageStatus.SendCouponSuccess = (int)s;
                    break;
            }
        }
        public void Enable(MessageTypeEnum e)
        {
            CheckCanEnable();
            if (dic.Where(a => a.Key == e).FirstOrDefault().Value == Himall.Core.Plugins.Message.StatusEnum.Disable)
            {
                throw new Himall.Core.HimallException("该功能已被禁止，不能进行设置");
            }
            SetMessageStatus(e, StatusEnum.Open);
            SMSCore.SaveMessageStatus(messageStatus);
        }

        public StatusEnum GetStatus(MessageTypeEnum e)
        {
            InitMessageStatus();
            return dic.FirstOrDefault(a => a.Key == e).Value;
        }

        public string Logo
        {
            get
            {
                if (string.IsNullOrWhiteSpace(SMSCore.WorkDirectory))
                    throw new MissingFieldException("没有设置插件工作目录");
                return SMSCore.WorkDirectory + "/Data/logo.png";
            }
        }

        public Dictionary<MessageTypeEnum, StatusEnum> GetAllStatus()
        {
            InitMessageStatus();
            return dic;
        }

        public string SendMessageCode(string destination, MessageUserInfo info)
        {
            var config = SMSCore.GetMessageContentConfig();
            //var text = config.Bind.Replace("#userName#", info.UserName).Replace("#checkCode#", info.CheckCode);
            return SendMessage(destination, info.CheckCode, "170141");
        }

        //private string SendMessage(string destination, string text, string speed = "0")
        //{
        //    if (!string.IsNullOrWhiteSpace(destination))
        //    {
        //        var config = SMSCore.GetConfig();
        //        SortedDictionary<string, string> tmpParas = new SortedDictionary<string, string>();
        //        tmpParas.Add("mobiles", destination);
        //        tmpParas.Add("text", text);
        //        tmpParas.Add("appkey", config.AppKey);
        //        tmpParas.Add("sendtime", DateTime.Now.ToString());
        //        tmpParas.Add("speed", speed);
        //        Dictionary<string, string> paras = SMSAPiHelper.Parameterfilter(tmpParas);
        //        string sign = SMSAPiHelper.BuildSign(paras, config.AppSecret, "MD5", "utf-8");
        //        paras.Add("sign", sign);
        //        paras.Add("sign_type", "MD5");
        //        string postdata = SMSAPiHelper.CreateLinkstring(paras);
        //        return SMSAPiHelper.PostData("http://sms.kuaidiantong.cn/SendMsg.aspx", postdata);
        //    }
        //    return "发送目标不能为空！";
        //}


        //private void BatchSendMessage(string[] destination, string text)
        //{
        //    if (destination.Length > 0)
        //    {
        //        var config = SMSCore.GetConfig();
        //        SortedDictionary<string, string> tmpParas = new SortedDictionary<string, string>();
        //        tmpParas.Add("mobiles", string.Join(",", destination));
        //        tmpParas.Add("text", text);
        //        tmpParas.Add("appkey", config.AppKey);
        //        tmpParas.Add("sendtime", DateTime.Now.ToString());
        //        tmpParas.Add("speed", "1");
        //        Dictionary<string, string> paras = SMSAPiHelper.Parameterfilter(tmpParas);
        //        string sign = SMSAPiHelper.BuildSign(paras, config.AppSecret, "MD5", "utf-8");
        //        paras.Add("sign", sign);
        //        paras.Add("sign_type", "MD5");
        //        string postdata = SMSAPiHelper.CreateLinkstring(paras);
        //        SMSAPiHelper.PostData("http://sms.kuaidiantong.cn/SendMsg.aspx", postdata);
        //    }
        //}


        public string SendMessageOnFindPassWord(string destination, MessageUserInfo info)
        {
            var config = SMSCore.GetMessageContentConfig();
            var text = config.FindPassWord.Replace("#userName#", info.UserName).Replace("#checkCode#", info.CheckCode).Replace("#siteName#", info.SiteName);
            SendMessage(destination, text, "2");
            return text;
        }

        public string SendMessageOnOrderCreate(string destination, MessageOrderInfo info)
        {
            var config = SMSCore.GetMessageContentConfig();
            var text = config.OrderCreated.Replace("#userName#", info.UserName).Replace("#orderId#", info.OrderId).Replace("#siteName#", info.SiteName);
            SendMessage(destination, text, "170141");
            return text;
        }

        public string SendMessageOnOrderPay(string destination, MessageOrderInfo info)
        {
            var config = SMSCore.GetMessageContentConfig();
            var text = config.OrderPay.Replace("#userName#", info.UserName).Replace("#orderId#", info.OrderId).Replace("#siteName#", info.SiteName).Replace("#Total#", info.TotalMoney.ToString("F2"));
            SendMessage(destination, text, "170141");
            return text;
        }

        public string SendMessageOnOrderRefund(string destination, MessageOrderInfo info)
        {
            var config = SMSCore.GetMessageContentConfig();
            var text = config.OrderRefund.Replace("#userName#", info.UserName).Replace("#orderId#", info.OrderId).Replace("#siteName#", info.SiteName).Replace("#RefundMoney#", info.RefundMoney.ToString("F2"));
            SendMessage(destination, text, "170141");
            return text;
        }
        public string SendMessageOnRefundDeliver(string destination, MessageOrderInfo info)
        {
            var config = SMSCore.GetMessageContentConfig();
            var text = config.RefundDeliver.Replace("#userName#", info.UserName).Replace("#orderId#", info.OrderId).Replace("#siteName#", info.SiteName).Replace("#RefundMoney#", info.RefundMoney.ToString("F2"));
            SendMessage(destination, text, "170141");
            return text;
        }

        public string SendMessageOnOrderShipping(string destination, MessageOrderInfo info)
        {
            var config = SMSCore.GetMessageContentConfig();
            var text = config.OrderShipping.Replace("#userName#", info.UserName).Replace("#orderId#", info.OrderId).Replace("#siteName#", info.SiteName).Replace("#shippingCompany#", info.ShippingCompany).Replace("#shippingNumber#", info.ShippingNumber);
            SendMessage(destination, text, "170141");
            return text;
        }

        public string SendMessageOnShopAudited(string destination, MessageShopInfo info)
        {
            var config = SMSCore.GetMessageContentConfig();
            var text = config.ShopAudited.Replace("#userName#", info.UserName).Replace("#shopName#", info.ShopName).Replace("#siteName#", info.SiteName);
            SendMessage(destination, text, "170141");
            return text;
        }

        public string SendMessageOnShopSuccess(string destination, MessageShopInfo info)
        {
            var config = SMSCore.GetMessageContentConfig();
            var text = config.ShopSuccess.Replace("#userName#", info.UserName).Replace("#shopName#", info.ShopName).Replace("#siteName#", info.SiteName);
            SendMessage(destination, text, "170141");
            return text;
        }

        public string SendMessageOnCouponSuccess(string destination, MessageCouponInfo info)
        {
            var config = SMSCore.GetMessageContentConfig();
            var text = config.SendCouponSuccess.Replace("#userName#", info.UserName).Replace("#Money#", info.Money.ToString("F2")).Replace("#Url#", info.Url).Replace("#siteName#", info.SiteName);
            SendMessage(destination, text, "170141");
            return text;
        }


        public void SendMessages(string[] destination, string content, string title)
        {
            var test = content;
            BatchSendMessage(destination, test);
        }

        public string SendTestMessage(string destination, string content, string title)
        {
            var test = content;
            return SendMessage(destination, test, "170141");
        }
        public void SetAllStatus(Dictionary<MessageTypeEnum, StatusEnum> dic)
        {
            foreach (var d in dic)
            {
                SetMessageStatus(d.Key, d.Value);
            }
            SMSCore.SaveMessageStatus(messageStatus);
        }
        public string ShortName
        {
            get { return "手机"; }
        }
        public bool EnableLog
        {
            get { return true; }
        }
        //打开购买短信页面
        public string GetBuyLink()
        {
            return "http://www.miaodiyun.com/home";
        }
        //打开短信登录页面
        public string GetLoginLink()
        {
            return "http://www.miaodiyun.com/login.html";
        }
        //获取短信数量
        public string GetSMSAmount()
        {
            var config = SMSCore.GetConfig();
            string postdata = "method=getAmount&appkey=" + config.accountSid;
            return SMSAPiHelper.PostData("http://sms.kuaidiantong.cn/GetAmount.aspx", postdata);
        }


        public bool IsSettingsValid
        {
            get
            {
                try
                {
                    CheckCanEnable();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        public bool CheckDestination(string destination)
        {
            return Core.Helper.ValidateHelper.IsMobile(destination);
        }

        public static string Md5jm(string str)
        {

            //把字符串转化成字节数组
            byte[] jmq = System.Text.Encoding.Default.GetBytes(str);

            MD5 md5 = new MD5CryptoServiceProvider();

            //通过字节数组转换成加密后的字节数组（hssh编码值）
            byte[] jmBehind = md5.ComputeHash(jmq);

            //加密后烦人字节数组转换成字符串
            string strBehind = BitConverter.ToString(jmBehind).Replace("-", "");

            return strBehind;

        }
        private string BatchSendMessage(string[] destination, string text)
        {
            if (destination.Length > 0)
            {
                var config = SMSCore.GetConfig();
                string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");

                Random rd = new Random();
                int num = rd.Next(245621, 999999);

                string content = "accountSid=" + config.accountSid + "&smsContent=【长生树】" + text + "&to=" + string.Join(",", destination) + "&timestamp=" + timestamp + "&sig=" + Md5jm(config.accountSid + config.authToken + timestamp).ToLower();
                string result = Service.PostData("https://api.miaodiyun.com/20150822/affMarkSMS/sendSMS", content);

                if (result.Contains("00000"))
                {
                    return "发送成功！";
                }
                else if (result.Contains("00008"))
                {
                    return "操作频繁";
                }
                else if (result.Contains("00025"))
                {
                    return "手机格式不对";
                }
                else if (result.Contains("00066"))
                {
                    return "开发者余额已被冻结";
                }
                else if (result.Contains("00111"))
                {
                    return "匹配到黑名单";
                }
                else if (result.Contains("00134"))
                {
                    return "没有匹配的模板";
                }
                else
                {
                    return "请填写正确的accountSid和authToken值";
                }
            }
            return "发送目标不能为空！";
        }
        //public string SendMessage(string phone, string mscontent, string speed = "0")
        //{
        //    var config = SMSCore.GetConfig();
        //    string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");

        //    Random rd = new Random();
        //    int num = rd.Next(245621, 999999);

        //    string content = "accountSid="+ config.accountSid + "&smsContent=【长生树】" + mscontent + "&to=" + phone + "&timestamp=" + timestamp + "&sig=" + Md5jm(config.accountSid + config.authToken + timestamp).ToLower();
        //    string result = Service.PostData("https://api.miaodiyun.com/20150822/affMarkSMS/sendSMS", content);

        //    if (result.Contains("00000"))
        //    {
        //        return "发送成功！";
        //    }
        //    else if (result.Contains("00008"))
        //    {
        //        return "操作频繁";
        //    }
        //    else if (result.Contains("00025"))
        //    {
        //        return "手机格式不对";
        //    }
        //    else if (result.Contains("00066"))
        //    {
        //        return "开发者余额已被冻结";
        //    }
        //    else if (result.Contains("00111"))
        //    {
        //        return "匹配到黑名单";
        //    }
        //    else if (result.Contains("00134"))
        //    {
        //        return "没有匹配的模板";
        //    }
        //    else
        //    {
        //        //return "未知错误，请联系技术客服。";
        //        return result;
        //    }
        //}
        //private string BatchSendYZMessage(string[] destination, string text)
        //{
        //    if (destination.Length > 0)
        //    {
        //        var config = SMSCore.GetConfig();
        //        string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");

        //        Random rd = new Random();
        //        int num = rd.Next(245621, 999999);

        //        string content = "accountSid=" + config.accountSid + "&smsContent=【长生树】" + text + "&to=" + string.Join(",", destination) + "&timestamp=" + timestamp + "&sig=" + Md5jm(config.accountSid + config.authToken + timestamp).ToLower();
        //        string result = Service.PostData("https://api.miaodiyun.com/20150822/industrySMS/sendSMS", content);

        //        if (result.Contains("00000"))
        //        {
        //            return "发送成功！";
        //        }
        //        else if (result.Contains("00008"))
        //        {
        //            return "操作频繁";
        //        }
        //        else if (result.Contains("00025"))
        //        {
        //            return "手机格式不对";
        //        }
        //        else if (result.Contains("00066"))
        //        {
        //            return "开发者余额已被冻结";
        //        }
        //        else if (result.Contains("00111"))
        //        {
        //            return "匹配到黑名单";
        //        }
        //        else if (result.Contains("00134"))
        //        {
        //            return "没有匹配的模板";
        //        }
        //        else
        //        {
        //            return "请填写正确的accountSid和authToken值";
        //        }
        //    }
        //    return "发送目标不能为空！";
        //}

        private string SendMessage(string destination, string text, string tempId)
        {

            if (!string.IsNullOrWhiteSpace(destination))
            {

                string ret = null;
                CCPRestSDK.CCPRestSDK api = new CCPRestSDK.CCPRestSDK();
                bool isInit = api.init("app.cloopen.com", "8883");
                api.setAccount("8a216da859aa5a950159ab24c75400bd", "996a3eb4f7f349609a4efb2cabbde51b");
                api.setAppId("8a216da85b8e6bb1015b9df674670321");
                Random rd = new Random();
                int num = rd.Next(111111, 999999);
                try
                {
                    if (isInit)
                    {
                        Dictionary<string, object> retData = api.SendTemplateSMS(destination, tempId, new string[] { text });
                        ret = getDictionaryData(retData);
                    }
                    else
                    {
                        ret = "初始化失败";
                    }
                }
                catch (Exception exc)
                {
                    ret = exc.Message;
                }
                return ret;
            }
            else
            {
                return "发送目标不能为空！";
            }
        }

        private string getDictionaryData(Dictionary<string, object> data)
        {
            string ret = null;
            foreach (KeyValuePair<string, object> item in data)
            {
                if (item.Value != null && item.Value.GetType() == typeof(Dictionary<string, object>))
                {
                    ret += item.Key.ToString() + "={";
                    ret += getDictionaryData((Dictionary<string, object>)item.Value);
                    ret += "};";
                }
                else
                {
                    ret += item.Key.ToString() + "=" + (item.Value == null ? "null" : item.Value.ToString()) + ";";
                }
            }
            return ret;
        }
        //public string SendYZMessage(string phone, string mscontent, string speed = "0")
        //{
        //    var config = SMSCore.GetConfig();
        //    string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");

        //    Random rd = new Random();
        //    int num = rd.Next(245621, 999999);

        //    string content = "accountSid=" + config.accountSid + "&smsContent=【长生树】" + mscontent + "&to=" + phone + "&timestamp=" + timestamp + "&sig=" + Md5jm(config.accountSid + config.authToken + timestamp).ToLower();
        //    string result = Service.PostData("https://api.miaodiyun.com/20150822/industrySMS/sendSMS", content);

        //    if (result.Contains("00000"))
        //    {
        //        return "发送成功！";
        //    }
        //    else if (result.Contains("00008"))
        //    {
        //        return "操作频繁";
        //    }
        //    else if (result.Contains("00025"))
        //    {
        //        return "手机格式不对";
        //    }
        //    else if (result.Contains("00066"))
        //    {
        //        return "开发者余额已被冻结";
        //    }
        //    else if (result.Contains("00111"))
        //    {
        //        return "匹配到黑名单";
        //    }
        //    else if (result.Contains("00134"))
        //    {
        //        return "没有匹配的模板";
        //    }
        //    else
        //    {
        //        //return "未知错误，请联系技术客服。";
        //        return result;
        //    }
        //}
        public static string PostData(string url, string postData)
        {
            string result = string.Empty;
            try
            {
                Uri uri = new Uri(url);
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
                Encoding encoding = Encoding.UTF8;
                byte[] bytes = encoding.GetBytes(postData);

                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = bytes.Length;
                using (Stream writeStream = request.GetRequestStream())
                {
                    writeStream.Write(bytes, 0, bytes.Length);
                }

                #region 读取服务器返回信息

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    using (Stream responseStream = response.GetResponseStream())
                    {
                        Encoding _encodingResponse = Encoding.UTF8;
                        //if(response)
                        Stream decompress = responseStream;
                        //decompress
                        if (response.ContentEncoding.ToLower() == "gzip")
                        {
                            decompress = new GZipStream(responseStream, CompressionMode.Decompress);
                        }
                        else if (response.ContentEncoding.ToLower() == "deflate")
                        {
                            decompress = new DeflateStream(responseStream, CompressionMode.Decompress);
                        }
                        using (StreamReader readStream = new StreamReader(decompress, _encodingResponse))
                        {
                            result = readStream.ReadToEnd();
                        }
                    }
                }
                #endregion
            }
            catch (Exception e)
            {
                result = string.Format("获取信息错误：{0}", e.Message);

            }

            return result;
        }


    }
}


