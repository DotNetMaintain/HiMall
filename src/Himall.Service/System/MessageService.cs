using Himall.CommonModel;
using Himall.CommonModel.WeiXin;
using Himall.Core;
using Himall.Core.Helper;
using Himall.Core.Plugins.Message;
using Himall.Entity;
using Himall.IServices;
using Himall.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;

namespace Himall.Service
{
    public class MessageService : ServiceBase, IMessageService
    {

        public void UpdateMemberContacts(MemberContactsInfo info)
        {
            var exist = Context.MemberContactsInfo.FirstOrDefault(a => a.ServiceProvider == info.ServiceProvider && a.UserId == info.UserId && a.UserType == info.UserType);
            if (exist != null)
            {
                exist.Contact = info.Contact;
            }
            else
            {
                Context.MemberContactsInfo.Add(info);
            }
            var user = Context.UserMemberInfo.Where(a => a.Id == info.UserId).FirstOrDefault();
            if (user != null)
            {
                if (info.ServiceProvider == "Himall.Plugin.Message.SMS")
                {
                    user.CellPhone = info.Contact;
                }
                else
                {
                    user.Email = info.Contact;
                }
            }
            Context.SaveChanges();
            Core.Cache.Remove(CacheKeyCollection.Member(info.UserId));//移除用户缓存
        }

        public string GetDestination(long userId, string pluginId, MemberContactsInfo.UserTypes type)
        {
            var Destination = Context.MemberContactsInfo.Where(a => a.UserId == userId && a.ServiceProvider == pluginId && a.UserType == type).FirstOrDefault();
            if (Destination != null)
            {
                return Destination.Contact;
            }
            return "";
        }

        public string GetDestination(long userId, string pluginId)
        {
            var Destination = Context.MemberContactsInfo.Where(a => a.UserId == userId && a.ServiceProvider == pluginId).FirstOrDefault();
            if (Destination != null)
            {
                return Destination.Contact;
            }
            return "";
        }
        public MemberContactsInfo GetMemberContactsInfo(string pluginId, string contact, MemberContactsInfo.UserTypes type)
        {
            return Context.MemberContactsInfo.Where(a => a.ServiceProvider == pluginId && a.UserType == type && a.Contact == contact).FirstOrDefault();
        }

        public List<MemberContactsInfo> GetMemberContactsInfo(long UserId)
        {
            return Context.MemberContactsInfo.Where(a => a.UserId == UserId).ToList();
        }

        public string SendMessageCode(string destination, string pluginId, Core.Plugins.Message.MessageUserInfo info)
        {
            var messagePlugin = PluginsManagement.GetPlugin<IMessagePlugin>(pluginId);
            if (string.IsNullOrEmpty(destination) || !messagePlugin.Biz.CheckDestination(destination))
                throw new HimallException(messagePlugin.Biz.ShortName + "错误");
            var content = messagePlugin.Biz.SendMessageCode(destination, info);
            Core.Log.Debug("结果" + content);
            if (messagePlugin.Biz.EnableLog)
            {
                Context.MessageLog.Add(new MessageLog() { SendTime = DateTime.Now, ShopId = 0, MessageContent = content, TypeId = "短信" });
                Context.SaveChanges();
            }
            return content;
        }

        public void SendMessageOnFindPassWord(long userId, Core.Plugins.Message.MessageUserInfo info)
        {
            var message = PluginsManagement.GetPlugins<IMessagePlugin>().ToList();
            foreach (var msg in message)
            {
                if (msg.Biz.GetStatus(MessageTypeEnum.FindPassWord) == StatusEnum.Open)
                {
                    string destination = GetDestination(userId, msg.PluginInfo.PluginId, MemberContactsInfo.UserTypes.General);
                    if (!msg.Biz.CheckDestination(destination))
                        throw new HimallException(msg.Biz.ShortName + "错误");
                    var content = msg.Biz.SendMessageOnFindPassWord(destination, info);
                    if (msg.Biz.EnableLog)
                    {
                        Context.MessageLog.Add(new MessageLog() { SendTime = DateTime.Now, ShopId = 0, MessageContent = content, TypeId = "短信" });
                        Context.SaveChanges();
                    }
                }
            }
        }
        /// <summary>
        /// 创建订单通知
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="info"></param>
        public void SendMessageOnOrderCreate(long userId, MessageOrderInfo info)
        {
            var message = PluginsManagement.GetPlugins<IMessagePlugin>().ToList();

            foreach (var msg in message)
            {
                if (msg.Biz.GetStatus(MessageTypeEnum.OrderCreated) == StatusEnum.Open)
                {
                    string destination = GetDestination(userId, msg.PluginInfo.PluginId, MemberContactsInfo.UserTypes.General);
                    if (!msg.Biz.CheckDestination(destination))
                        throw new HimallException(msg.Biz.ShortName + "错误");
                    //var content = msg.Biz.SendMessageOnOrderCreate(destination, info);
                    var content = "成功";
                    if (msg.Biz.EnableLog)
                    {
                        Context.MessageLog.Add(new MessageLog() { SendTime = DateTime.Now, ShopId = info.ShopId, MessageContent = content, TypeId = "短信" });
                        Context.SaveChanges();
                    }
                }
            }

            //#region 发送模板消息
            //if (info.MsgOrderType == MessageOrderType.Applet || info.MsgOrderType == MessageOrderType.O2OApplet)
            //{
            //    Log.Info((info.MsgOrderType == MessageOrderType.Applet ? "商城" : "o2o") + "小程序创建订单，准备开始发送消息");
            //    var userinfo = Context.UserMemberInfo.FirstOrDefault(d => d.Id == userId);
            //    if (userinfo != null)
            //    {
            //        var msgdata = new WX_MsgTemplateSendDataModel();

            //        msgdata.keyword1.value = info.OrderTime.ToString("yyyy-MM-dd HH:mm:ss");
            //        msgdata.keyword1.color = "#173177";
            //        msgdata.keyword2.value = info.TotalMoney.ToString("F2");
            //        msgdata.keyword2.color = "#ff3300";
            //        msgdata.keyword3.value = info.ProductName + " 等...";
            //        msgdata.keyword3.color = "#173177";
            //        msgdata.keyword4.value = info.OrderId;
            //        msgdata.keyword4.color = "#173177";
            //        msgdata.keyword5.value = "待支付";
            //        msgdata.keyword5.color = "#173177";

            //        //处理url
            //        var _iwxtser = Himall.ServiceProvider.Instance<IWXMsgTemplateService>.Create;
            //        var _iwxmember = Himall.ServiceProvider.Instance<IMemberService>.Create;
            //        string page = _iwxtser.GetWXAppletMessageTemplateShowUrl(MessageTypeEnum.OrderCreated);//小程序跳转地址
            //        page = page.Replace("{id}", info.OrderId.ToString());
            //        Log.Info((info.MsgOrderType == MessageOrderType.Applet ? "商城" : "o2o") + "小程序创建订单跳转地址：" + page);
            //        string openId = "";
            //        try
            //        {
            //            if (info.MsgOrderType == MessageOrderType.Applet)
            //            {
            //                openId = _iwxmember.GetMemberOpenIdInfoByuserIdAndType(userId, "WeiXinSmallProg").OpenId;//登录小程序的OpenId
            //            }
            //            else if (info.MsgOrderType == MessageOrderType.O2OApplet)
            //            {
            //                openId = _iwxmember.GetMemberOpenIdInfoByuserIdAndType(userId, "O2OWeiXinSmallProg").OpenId;//登录小程序的OpenId
            //            }

            //            Log.Info((info.MsgOrderType == MessageOrderType.Applet ? "商城" : "o2o") + "拿到的小程序openid为：" + openId);
            //        }
            //        catch(Exception e)
            //        {
            //            Log.Error((info.MsgOrderType == MessageOrderType.Applet ? "商城" : "o2o") + "小程序获取openid报错：" + e.Message);
            //        }
            //        string formid = "";
            //        var formmodel = _iwxtser.GetWXAppletFromDataById(MessageTypeEnum.OrderCreated, info.OrderId.ToString());
            //        if (formmodel != null)
            //            formid = formmodel.FormId;//根据OrderId获取FormId
            //        Log.Info((info.MsgOrderType == MessageOrderType.Applet ? "商城" : "o2o") + "拿到的小程序formId：" + formid);
            //        if (info.MsgOrderType == MessageOrderType.O2OApplet)
            //            _iwxtser.SendAppletMessageByTemplate(MessageTypeEnum.OrderCreated, userId, msgdata, page, openId, formid, true);
            //        else
            //            _iwxtser.SendAppletMessageByTemplate(MessageTypeEnum.OrderCreated, userId, msgdata, page, openId, formid);
            //    }

            //}
            //else
            //{
            //    var userinfo = Context.UserMemberInfo.FirstOrDefault(d => d.Id == userId);
            //    if (userinfo != null)
            //    {
            //        var msgdata = new WX_MsgTemplateSendDataModel();
            //        msgdata.first.value = "尊敬的（" + userinfo.Nick + "），您的订单信息如下：";
            //        msgdata.first.color = "#000000";
            //        msgdata.keyword1.value = info.OrderId.ToString();
            //        msgdata.keyword1.color = "#FF0000";
            //        msgdata.keyword2.value = info.Quantity.ToString();
            //        msgdata.keyword2.color = "#000000";
            //        msgdata.keyword3.value = info.TotalMoney.ToString();
            //        msgdata.keyword3.color = "#FF0000";
            //        msgdata.remark.value = "感谢您的光顾，支付完成后，我们将火速为您发货~~";
            //        msgdata.remark.color = "#000000";
            //        //处理url
            //        var _iwxtser = Himall.ServiceProvider.Instance<IWXMsgTemplateService>.Create;
            //        string url = _iwxtser.GetMessageTemplateShowUrl(MessageTypeEnum.OrderCreated);
            //        url = url.Replace("{id}", info.OrderId.ToString());
            //        _iwxtser.SendMessageByTemplate(MessageTypeEnum.OrderCreated, userId, msgdata, url);
            //    }
            //}

            //#endregion
        }
        /// <summary>
        /// 支付通知
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="info"></param>
        public void SendMessageOnOrderPay(long userId, MessageOrderInfo info)
        {
            var message = PluginsManagement.GetPlugins<IMessagePlugin>().ToList();
            //foreach (var msg in message)
            //{
            //    if (msg.Biz.GetStatus(MessageTypeEnum.OrderPay) == StatusEnum.Open)
            //    {
            //        string destination = GetDestination(userId, msg.PluginInfo.PluginId, MemberContactsInfo.UserTypes.General);
            //        if (!msg.Biz.CheckDestination(destination))
            //            throw new HimallException(msg.Biz.ShortName + "错误");
            //        var content = msg.Biz.SendMessageOnOrderPay(destination, info);
            //        if (msg.Biz.EnableLog)
            //        {
            //            Context.MessageLog.Add(new MessageLog() { SendTime = DateTime.Now, ShopId = info.ShopId, MessageContent = content, TypeId = "短信" });
            //            Context.SaveChanges();
            //        }
            //    }
            //}

            #region 发送模板消息
            if (info.MsgOrderType == MessageOrderType.Applet || info.MsgOrderType == MessageOrderType.O2OApplet)
            {
                Log.Info((info.MsgOrderType == MessageOrderType.Applet ? "商城" : "o2o") + "小程序支付成功，准备开始发送消息");
                var _iwxtser = Himall.ServiceProvider.Instance<IWXMsgTemplateService>.Create;
                var _iwxmember = Himall.ServiceProvider.Instance<IMemberService>.Create;
                string formid = "";
                var formmodel = _iwxtser.GetWXAppletFromDataById(MessageTypeEnum.OrderPay, info.OrderId.ToString());
                if (formmodel != null)
                {
                    formid = formmodel.FormId;//根据OrderId获取FormId
                }
                else
                {
                    Log.Info((info.MsgOrderType == MessageOrderType.Applet ? "商城" : "o2o") + "小程序订单：" + info.OrderId + " FormId为空");
                }
                Log.Info((info.MsgOrderType == MessageOrderType.Applet ? "商城" : "o2o") + "小程序支付，formid=" + formid);

                var userinfo = Context.UserMemberInfo.FirstOrDefault(d => d.Id == userId);
                if (userinfo != null)
                {
                    Log.Info((info.MsgOrderType == MessageOrderType.Applet ? "商城" : "o2o") + "小程序支付，userId=" + userId);
                    var msgdata = new WX_MsgTemplateSendDataModel();
                    msgdata.keyword1.value = info.TotalMoney.ToString("f2") + "元";
                    msgdata.keyword1.color = "#173177";
                    msgdata.keyword2.value = info.PayTime.ToString("yyyy-MM-dd HH:mm:ss");
                    msgdata.keyword2.color = "#ff3300";
                    msgdata.keyword3.value = info.ProductName.ToString() + "等...";
                    msgdata.keyword3.color = "#173177";
                    msgdata.keyword4.value = info.OrderId.ToString();
                    msgdata.keyword4.color = "#173177";
                    msgdata.keyword5.value = info.OrderTime.ToString("yyyy-MM-dd HH:mm:ss");
                    msgdata.keyword5.color = "#173177";
                    //处理url
                    string page = _iwxtser.GetWXAppletMessageTemplateShowUrl(MessageTypeEnum.OrderPay);//小程序跳转地址
                    page = page.Replace("{id}", info.OrderId.ToString());
                    Log.Info((info.MsgOrderType == MessageOrderType.Applet ? "商城" : "o2o") + "小程序支付跳转url=" + page);
                    string openId = "";
                    try
                    {
                        if (info.MsgOrderType == MessageOrderType.Applet)
                        {
                            openId = _iwxmember.GetMemberOpenIdInfoByuserIdAndType(userId, "WeiXinSmallProg").OpenId;//登录小程序的OpenId
                        }
                        else if (info.MsgOrderType == MessageOrderType.O2OApplet)
                        {
                            openId = _iwxmember.GetMemberOpenIdInfoByuserIdAndType(userId, "O2OWeiXinSmallProg").OpenId;//登录小程序的OpenId
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Error((info.MsgOrderType == MessageOrderType.Applet ? "商城" : "o2o") + "小程序支付，获取openid出错：" + e.Message);
                    }
                    Log.Info((info.MsgOrderType == MessageOrderType.Applet ? "商城" : "o2o") + "小程序openid=" + openId);
                    if (info.MsgOrderType == MessageOrderType.O2OApplet)
                        _iwxtser.SendAppletMessageByTemplate(MessageTypeEnum.OrderPay, userId, msgdata, page, openId, formid, true);
                    else
                        _iwxtser.SendAppletMessageByTemplate(MessageTypeEnum.OrderPay, userId, msgdata, page, openId, formid);
                }
            }
            else
            {
                var _iwxtser = Himall.ServiceProvider.Instance<IWXMsgTemplateService>.Create;
                var userinfo = Context.UserMemberInfo.FirstOrDefault(d => d.Id == userId);
                if (userinfo != null)
                {
                    var msgdata = new WX_MsgTemplateSendDataModel();
                    msgdata.first.value = "您好，您有一笔订单已经支付成功";
                    msgdata.first.color = "#000000";
                    msgdata.keyword1.value = info.ProductName.ToString();
                    msgdata.keyword1.color = "#000000";
                    msgdata.keyword2.value = info.OrderId.ToString();
                    msgdata.keyword2.color = "#000000";
                    msgdata.keyword3.value = info.TotalMoney.ToString();
                    msgdata.keyword3.color = "#FF0000";
                    msgdata.keyword4.value = info.PayTime.ToString();
                    msgdata.keyword4.color = "#000000";
                    msgdata.remark.value = "感谢您的惠顾";
                    msgdata.remark.color = "#000000";
                    string url = _iwxtser.GetMessageTemplateShowUrl(MessageTypeEnum.OrderPay);
                    url = url.Replace("{id}", info.OrderId.ToString());
                    _iwxtser.SendMessageByTemplate(MessageTypeEnum.OrderPay, userId, msgdata, url);
                }
                ////发送商家
                //var shopsend = Context.ShopShippersInfo.FirstOrDefault(d => d.ShopId == info.ShopId && d.IsDefaultSendGoods == true);
                //if (shopsend != null && !string.IsNullOrWhiteSpace(shopsend.WxOpenId))
                //{
                //    var msgdata = new WX_MsgTemplateSendDataModel();
                //    msgdata.first.value = info.SiteName + "中有一个新的订单付款成功，请及时发货！";
                //    msgdata.first.color = "#000000";
                //    msgdata.keyword1.value = info.OrderId.ToString();
                //    msgdata.keyword1.color = "#000000";
                //    msgdata.keyword2.value = info.PayTime.ToString();
                //    msgdata.keyword2.color = "#000000";
                //    msgdata.keyword3.value = info.TotalMoney.ToString();
                //    msgdata.keyword3.color = "#FF0000";
                //    msgdata.keyword4.value = info.PaymentType.ToString();
                //    msgdata.keyword4.color = "#000000";
                //    msgdata.remark.value = info.ProductName + "等...";
                //    msgdata.remark.color = "#000000";
                //    string url = "";
                //    _iwxtser.SendMessageByTemplate(MessageTypeEnum.OrderPay, 0, msgdata, url, shopsend.WxOpenId);
                //}
            }
            #endregion
        }
        /// <summary>
        /// 支付通知
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="info"></param>
        public void SendMessageOnWaitPay(long userId, MessageOrderInfo info)
        {
            var message = PluginsManagement.GetPlugins<IMessagePlugin>().ToList();

            var _iwxtser = Himall.ServiceProvider.Instance<IWXMsgTemplateService>.Create;
            var userinfo = Context.UserMemberInfo.FirstOrDefault(d => d.Id == userId);
            if (userinfo != null)
            {
                var msgdata = new WX_MsgTemplateSendDataModel();
                msgdata.first.value = "您好，您有一笔订单未付款";
                msgdata.first.color = "#000000";
                msgdata.keyword1.value = info.OrderId.ToString();
                msgdata.keyword1.color = "#000000";
                msgdata.keyword2.value = info.TotalMoney.ToString();
                msgdata.keyword2.color = "#000000";
                msgdata.keyword3.value = info.OrderTime.ToString();
                msgdata.keyword3.color = "#FF0000";
                msgdata.remark.value = "24小时内未支付将自动取消该订单";
                msgdata.remark.color = "#000000";
                string url = _iwxtser.GetMessageTemplateShowUrl(MessageTypeEnum.OrderCreated);
                url = url.Replace("{id}", info.OrderId.ToString());
                _iwxtser.SendMessageByTemplate(MessageTypeEnum.OrderCreated, userId, msgdata, url);
                Core.Log.Debug("代付款提醒：url"+url);
            }
        }

        /// <summary>
        /// 店铺有新订单
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="info"></param>
        public void SendMessageOnShopHasNewOrder(long shopId, MessageOrderInfo info)
        {
            #region 发送模板消息
            //卖家收信息
            var shopinfo = Context.ManagerInfo.FirstOrDefault(d => d.ShopId == shopId);
            if (shopinfo != null)
            {
                var sellerinfo = Context.UserMemberInfo.FirstOrDefault(d => d.UserName == shopinfo.UserName);
                if (sellerinfo != null)
                {
                    var msgdata = new WX_MsgTemplateSendDataModel();

#if DEBUG
                    Core.Log.Info("[模板消息]卖家新订单用户编号：" + sellerinfo.Id.ToString());
#endif
                    msgdata = new WX_MsgTemplateSendDataModel();
                    msgdata.first.value = "您的店铺有新的订单生成。";
                    msgdata.first.color = "#000000";
                    msgdata.keyword1.value = info.ShopName;
                    msgdata.keyword1.color = "#000000";
                    msgdata.keyword2.value = info.ProductName.ToString() + "等...";
                    msgdata.keyword2.color = "#000000";
                    msgdata.keyword3.value = info.OrderTime.ToString();
                    msgdata.keyword3.color = "#000000";
                    msgdata.keyword4.value = info.TotalMoney.ToString();
                    msgdata.keyword4.color = "#FF0000";
                    msgdata.keyword5.value = "已付款(" + info.PaymentType + ")";
                    msgdata.keyword5.color = "#000000";
                    msgdata.remark.value = "感谢您的使用,祝您生意兴荣。";
                    msgdata.remark.color = "#000000";

#if DEBUG
                    Core.Log.Info("[模板消息]卖家新订单开始前：" + sellerinfo.Id.ToString() + "_" + info.OrderId.ToString());
#endif
                    var _iwxtser = Himall.ServiceProvider.Instance<IWXMsgTemplateService>.Create;
                    _iwxtser.SendMessageByTemplate(MessageTypeEnum.ShopHaveNewOrder, sellerinfo.Id, msgdata);


#if DEBUG
                    Core.Log.Info("[模板消息]发送商家发货人：" + shopId.ToString() + "_" + info.OrderId.ToString());
#endif
                    //发送商家发货人
                    var shopsend = Context.ShopShippersInfo.FirstOrDefault(d => d.ShopId == shopId && d.IsDefaultSendGoods == true);
                    if (shopsend != null && !string.IsNullOrWhiteSpace(shopsend.WxOpenId))
                    {
                        var sendmsgdata = new WX_MsgTemplateSendDataModel();
                        sendmsgdata.first.value = info.SiteName + "中有一个新的订单付款成功，请及时发货！";
                        sendmsgdata.first.color = "#000000";
                        sendmsgdata.keyword1.value = info.OrderId.ToString();
                        sendmsgdata.keyword1.color = "#000000";
                        sendmsgdata.keyword2.value = info.PayTime.ToString();
                        sendmsgdata.keyword2.color = "#000000";
                        sendmsgdata.keyword3.value = info.TotalMoney.ToString();
                        sendmsgdata.keyword3.color = "#FF0000";
                        sendmsgdata.keyword4.value = info.PaymentType.ToString();
                        sendmsgdata.keyword4.color = "#000000";
                        sendmsgdata.remark.value = info.ProductName + "等...";
                        sendmsgdata.remark.color = "#000000";
                        string url = "";
                        _iwxtser.SendMessageByTemplate(MessageTypeEnum.OrderPay, 0, sendmsgdata, url, shopsend.WxOpenId);
                    }
                }
            }
            #endregion
        }

        /// <summary>
        /// 退款/退货成功通知
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="info"></param>
        /// <param name="refundid"></param>
        public void SendMessageOnOrderRefund(long userId, MessageOrderInfo info, long refundid = 0, string refundPayTypeName = "")
        {
            var message = PluginsManagement.GetPlugins<IMessagePlugin>().ToList();
            //foreach (var msg in message)
            //{
            //    if (msg.Biz.GetStatus(MessageTypeEnum.OrderRefund) == StatusEnum.Open)
            //    {
            //        string destination = GetDestination(userId, msg.PluginInfo.PluginId, MemberContactsInfo.UserTypes.General);
            //        if (!msg.Biz.CheckDestination(destination))
            //            throw new HimallException(msg.Biz.ShortName + "错误");
            //        //var content = msg.Biz.SendMessageOnOrderRefund(destination, info);
            //        if (msg.Biz.EnableLog)
            //        {
            //            Context.MessageLog.Add(new MessageLog() { SendTime = DateTime.Now, ShopId = info.ShopId, MessageContent = "成功", TypeId = "短信" });
            //            Context.SaveChanges();
            //        }
            //    }
            //}

            #region 发送模板消息
            if (info.MsgOrderType == MessageOrderType.Applet || info.MsgOrderType == MessageOrderType.O2OApplet)
            {
                Log.Info((info.MsgOrderType == MessageOrderType.Applet ? "商城" : "o2o") + "小程序退款退货，准备开始发送消息");
                //小程序发送
                string status = "退货/退款成功";
                string remark = "您的订单已经完成退款,¥" + info.RefundMoney.ToString("F2") + "已经退回您的付款账户（或预存款账户），请留意查收.";

                var userinfo = Context.UserMemberInfo.FirstOrDefault(d => d.Id == userId);
                if (userinfo != null)
                {
                    var msgdata = new WX_MsgTemplateSendDataModel();
                    msgdata.keyword1.value = "¥" + info.RefundMoney.ToString("F2") + "元";
                    msgdata.keyword1.color = "#173177";
                    msgdata.keyword2.value = refundPayTypeName;
                    msgdata.keyword2.color = "#173177";
                    msgdata.keyword3.value = status;
                    msgdata.keyword3.color = "#173177";
                    msgdata.keyword4.value = remark;
                    msgdata.keyword4.color = "#173177";
                    msgdata.keyword5.value = DateTime.Now.ToString();
                    msgdata.keyword5.color = "#173177";
                    msgdata.keyword6.value = info.OrderId;
                    msgdata.keyword6.color = "#173177";

                    //处理url
                    var _iwxtser = Himall.ServiceProvider.Instance<IWXMsgTemplateService>.Create;
                    var _iwxmember = Himall.ServiceProvider.Instance<IMemberService>.Create;
                    string page = _iwxtser.GetWXAppletMessageTemplateShowUrl(MessageTypeEnum.OrderRefund);//小程序跳转地址
                    page = page.Replace("{id}", info.OrderId.ToString());
                    if (refundid > 0)
                    {
                        page = page.Replace("{id}", refundid.ToString());
                    }
                    Log.Info((info.MsgOrderType == MessageOrderType.Applet ? "商城" : "o2o") + "小程序退款退货url=" + page);
                    string openId = "";
                    try
                    {
                        if (info.MsgOrderType == MessageOrderType.Applet)
                        {
                            openId = _iwxmember.GetMemberOpenIdInfoByuserIdAndType(userId, "WeiXinSmallProg").OpenId;//登录小程序的OpenId
                        }
                        else if (info.MsgOrderType == MessageOrderType.O2OApplet)
                        {
                            openId = _iwxmember.GetMemberOpenIdInfoByuserIdAndType(userId, "O2OWeiXinSmallProg").OpenId;//登录小程序的OpenId
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Info((info.MsgOrderType == MessageOrderType.Applet ? "商城" : "o2o") + "小程序退款退货获取openId出错：" + e.Message);
                    }
                    string formid = "";
                    var formmodel = _iwxtser.GetWXAppletFromDataById(MessageTypeEnum.OrderRefund, info.OrderId.ToString());
                    if (formmodel != null)
                        formid = formmodel.FormId;//根据OrderId获取FormId
                    Log.Info((info.MsgOrderType == MessageOrderType.Applet ? "商城" : "o2o") + "小程序退款退货formId=" + formid);
                    if (info.MsgOrderType == MessageOrderType.O2OApplet)
                        _iwxtser.SendAppletMessageByTemplate(MessageTypeEnum.OrderRefund, userId, msgdata, page, openId, formid, true);
                    else
                        _iwxtser.SendAppletMessageByTemplate(MessageTypeEnum.OrderRefund, userId, msgdata, page, openId, formid);
                }

            }
            else
            {
                var userinfo = Context.UserMemberInfo.FirstOrDefault(d => d.Id == userId);
                if (userinfo != null)
                {
                    var msgdata = new WX_MsgTemplateSendDataModel();
                    msgdata.first.value = "您的订单已经完成退款，¥" + info.RefundMoney.ToString("F2") + "已经退回您的付款账户，请留意查收。";
                    msgdata.first.color = "#000000";
                    msgdata.keyword1.value = "¥" + info.RefundMoney.ToString("F2");
                    msgdata.keyword1.color = "#FF0000";
                    msgdata.keyword2.value = info.OrderStatus;
                    msgdata.keyword2.color = "#FF0000";
                    msgdata.keyword3.value = info.RefundTime.ToString();
                    msgdata.keyword3.color = "#000000";
                    msgdata.keyword4.value = "原路返回";
                    msgdata.keyword4.color = "#000000";
                    msgdata.remark.value = "感谢您的支持";
                    msgdata.remark.color = "#000000";
                    var _iwxtser = Himall.ServiceProvider.Instance<IWXMsgTemplateService>.Create;
                    if (refundid > 0)
                    {
                        string url = _iwxtser.GetMessageTemplateShowUrl(MessageTypeEnum.OrderRefund);
                        url = url.Replace("{id}", refundid.ToString());
                    }
                    _iwxtser.SendMessageByTemplate(MessageTypeEnum.OrderRefund, userId, msgdata);
                }

            }
            #endregion
        }
        /// <summary>
        /// 售后发货信息提醒
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="info"></param>
        /// <param name="refundid"></param>
        public void SendMessageOnRefundDeliver(long userId, MessageOrderInfo info, long refundid = 0)
        {
            var message = PluginsManagement.GetPlugins<IMessagePlugin>().ToList();
#if DEBUG
            Core.Log.Info("[发送消息]数据：" + Newtonsoft.Json.JsonConvert.SerializeObject(info) + "[售后发货]");
#endif
            foreach (var msg in message)
            {
                if (msg.Biz.GetStatus(MessageTypeEnum.RefundDeliver) == StatusEnum.Open)
                {
                    string destination = GetDestination(userId, msg.PluginInfo.PluginId, MemberContactsInfo.UserTypes.General);
                    if (!msg.Biz.CheckDestination(destination))
                    {
#if DEBUG
                        Core.Log.Info("[发送消息]失败：" + msg.PluginInfo.PluginId + "未发送," + destination + "格式检测未通过[售后发货]");
#endif
                        throw new HimallException(msg.Biz.ShortName + "错误：实例失败。");
                    }
                    try
                    {
                        //var content = msg.Biz.SendMessageOnRefundDeliver(destination, info);
#if DEBUG
                        Core.Log.Info("[发送消息]发送结束：" + destination + " : " + msg.PluginInfo.PluginId + "[售后发货]");
#endif
                        if (msg.Biz.EnableLog)
                        {
                            Context.MessageLog.Add(new MessageLog() { SendTime = DateTime.Now, ShopId = info.ShopId, MessageContent = "", TypeId = "短信" });
                            Context.SaveChanges();
                        }
                    }
                    catch (Exception ex)
                    {
                        Core.Log.Info("[发送消息]发送失败：" + msg.PluginInfo.PluginId + "[售后发货]", ex);
                    }
                }
            }

            #region 发送模板消息
            var userinfo = Context.UserMemberInfo.FirstOrDefault(d => d.Id == userId);
            if (userinfo != null)
            {
                var msgdata = new WX_MsgTemplateSendDataModel();
                msgdata.first.value = "您的订单(" + info.OrderId + ")售后已审核通过，请及时发货。";
                msgdata.first.color = "#000000";
                msgdata.keyword1.value = info.OrderId;
                msgdata.keyword1.color = "#FF0000";
                msgdata.keyword2.value = "请您尽快发货并在订单详情页填写快递单号";
                msgdata.keyword2.color = "#000000";
                var _iwxtser = Himall.ServiceProvider.Instance<IWXMsgTemplateService>.Create;
                if (refundid > 0)
                {
                    string url = _iwxtser.GetMessageTemplateShowUrl(MessageTypeEnum.RefundDeliver);
                    url = url.Replace("{id}", refundid.ToString());
                }
                _iwxtser.SendMessageByTemplate(MessageTypeEnum.RefundDeliver, userId, msgdata);
            }
            #endregion
        }

        /// <summary>
        /// 发货通知
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="info"></param>
        public void SendMessageOnOrderShipping(long userId, MessageOrderInfo info)
        {
            var message = PluginsManagement.GetPlugins<IMessagePlugin>().ToList();
            //foreach (var msg in message)
            //{
            //    if (msg.Biz.GetStatus(MessageTypeEnum.OrderShipping) == StatusEnum.Open)
            //    {
            //        string destination = GetDestination(userId, msg.PluginInfo.PluginId, MemberContactsInfo.UserTypes.General);
            //        if (!msg.Biz.CheckDestination(destination))
            //            throw new HimallException(msg.Biz.ShortName + "错误");
            //        var content = msg.Biz.SendMessageOnOrderShipping(destination, info);
            //        if (msg.Biz.EnableLog)
            //        {
            //            Context.MessageLog.Add(new MessageLog() { SendTime = DateTime.Now, ShopId = info.ShopId, MessageContent = content, TypeId = "短信" });
            //            Context.SaveChanges();
            //        }
            //    }
            //}
            #region 发送模板消息
            if (info.MsgOrderType == MessageOrderType.Applet || info.MsgOrderType == MessageOrderType.O2OApplet)
            {
                Log.Info((info.MsgOrderType == MessageOrderType.Applet ? "商城" : "o2o") + "小程序发货通知，准备开始发送消息");
                //小程序模版
                var userinfo = Context.UserMemberInfo.FirstOrDefault(d => d.Id == userId);
                if (userinfo != null)
                {
                    var msgdata = new WX_MsgTemplateSendDataModel();
                    msgdata.keyword1.value = info.OrderId;
                    msgdata.keyword1.color = "#173177";
                    msgdata.keyword2.value = info.OrderTime.ToString("yyyy-MM-dd HH:mm:ss");
                    msgdata.keyword2.color = "#173177";
                    msgdata.keyword3.value = info.ShippingCompany;//快递公司
                    msgdata.keyword3.color = "#173177";
                    msgdata.keyword4.value = info.ShippingNumber;//快递单号
                    msgdata.keyword4.color = "#173177";
                    msgdata.keyword5.value = info.ProductName + "等...";
                    msgdata.keyword5.color = "#173177";
                    msgdata.remark.value = "";
                    msgdata.remark.color = "#000000";

                    //处理url
                    var _iwxtser = Himall.ServiceProvider.Instance<IWXMsgTemplateService>.Create;
                    var _iwxmember = Himall.ServiceProvider.Instance<IMemberService>.Create;
                    string page = _iwxtser.GetWXAppletMessageTemplateShowUrl(MessageTypeEnum.OrderShipping);//小程序跳转地址
                    page = page.Replace("{id}", info.OrderId.ToString());
                    Log.Info((info.MsgOrderType == MessageOrderType.Applet ? "商城" : "o2o") + "小程序发货通知url=" + page);
                    string openId = "";
                    try
                    {
                        if (info.MsgOrderType == MessageOrderType.Applet)
                        {
                            openId = _iwxmember.GetMemberOpenIdInfoByuserIdAndType(userId, "WeiXinSmallProg").OpenId;//登录小程序的OpenId
                        }
                        else if (info.MsgOrderType == MessageOrderType.O2OApplet)
                        {
                            openId = _iwxmember.GetMemberOpenIdInfoByuserIdAndType(userId, "O2OWeiXinSmallProg").OpenId;//登录小程序的OpenId
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Info((info.MsgOrderType == MessageOrderType.Applet ? "商城" : "o2o") + "小程序发货通知获取openid出错：" + e.Message);
                    }

                    string formid = "";
                    var formmodel = _iwxtser.GetWXAppletFromDataById(MessageTypeEnum.OrderPay, info.OrderId.ToString());
                    if (formmodel != null)
                        formid = formmodel.FormId;//根据OrderId获取FormId

                    Log.Info((info.MsgOrderType == MessageOrderType.Applet ? "商城" : "o2o") + "小程序发货通知获取formId=" + formid);
                    if (info.MsgOrderType == MessageOrderType.O2OApplet)
                        _iwxtser.SendAppletMessageByTemplate(MessageTypeEnum.OrderShipping, userId, msgdata, page, openId, formid, true);
                    else
                        _iwxtser.SendAppletMessageByTemplate(MessageTypeEnum.OrderShipping, userId, msgdata, page, openId, formid);
                }

            }
            #endregion
            #region 
            else
            {
                var userinfo = Context.UserMemberInfo.FirstOrDefault(d => d.Id == userId);
                if (userinfo != null)
                {
                    var msgdata = new WX_MsgTemplateSendDataModel();
                    msgdata.first.value = "您的订单已发货，正加速送到您的手上。";
                    msgdata.first.color = "#000000";
                    msgdata.keyword1.value = info.OrderId;
                    msgdata.keyword1.color = "#000000";
                    msgdata.keyword2.value = info.OrderTime.ToString("yyyy-MM-dd HH:mm:ss");
                    msgdata.keyword2.color = "#000000";
                    msgdata.keyword3.value = info.ShippingCompany.ToString();
                    msgdata.keyword3.color = "#FF0000";
                    msgdata.keyword4.value = info.ShippingNumber;
                    msgdata.keyword4.color = "#000000";
                    msgdata.keyword5.value = "待收货";
                    msgdata.keyword5.color = "#000000";
                    msgdata.remark.value = "请您耐心等候";
                    msgdata.remark.color = "#000000";
                    var _iwxtser = Himall.ServiceProvider.Instance<IWXMsgTemplateService>.Create;
                    string url = _iwxtser.GetMessageTemplateShowUrl(MessageTypeEnum.OrderShipping);
                    url = url.Replace("{id}", info.OrderId.ToString());
                    Core.Log.Debug("url" + url);
                    _iwxtser.SendMessageByTemplate(MessageTypeEnum.OrderShipping, userId, msgdata);
                }
            }
            #endregion
        }

        public void SendMessageOnShopAudited(long userId, MessageShopInfo info)
        {
            var messages = PluginsManagement.GetPlugins<IMessagePlugin>().ToList();
            foreach (var msg in messages)
            {
                if (msg.Biz.GetStatus(MessageTypeEnum.ShopAudited) == StatusEnum.Open)
                {
                    string destination = GetDestination(userId, msg.PluginInfo.PluginId);
                    if (!msg.Biz.CheckDestination(destination))
                        throw new HimallException(msg.Biz.ShortName + "错误");
                    var content = msg.Biz.SendMessageOnShopAudited(destination, info);
                    if (msg.Biz.EnableLog)
                    {
                        Context.MessageLog.Add(new MessageLog() { SendTime = DateTime.Now, ShopId = 0, MessageContent = content, TypeId = "短信" });
                        Context.SaveChanges();
                    }
                }
            }
        }

        /// <summary>
        /// 发送优惠券成功时发送消息
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="info"></param>

        public void SendMessageOnCouponSuccess(long userId, MessageCouponInfo info)
        {
            var message = PluginsManagement.GetPlugins<IMessagePlugin>().ToList();
            foreach (var msg in message)
            {
                if (msg.Biz.GetStatus(MessageTypeEnum.SendCouponSuccess) == StatusEnum.Open)
                {
                    string destination = GetDestination(userId, msg.PluginInfo.PluginId, MemberContactsInfo.UserTypes.General);
                    if (!msg.Biz.CheckDestination(destination))
                        throw new HimallException(msg.Biz.ShortName + "错误");
                    var content = msg.Biz.SendMessageOnCouponSuccess(destination, info);
                    if (msg.Biz.EnableLog)
                    {
                        Context.MessageLog.Add(new MessageLog() { SendTime = DateTime.Now, ShopId = 0, MessageContent = content, TypeId = "短信" });
                        Context.SaveChanges();
                    }
                }
            }
        }


        //2.4去除
        //public void SendMessageOnShopSuccess(long userId, MessageShopInfo info)
        //{
        //    var message = PluginsManagement.GetPlugins<IMessagePlugin>().ToList();
        //    foreach (var msg in message)
        //    {
        //        if (msg.Biz.GetStatus(MessageTypeEnum.ShopSuccess) == StatusEnum.Open)
        //        {
        //            string destination = GetDestination(userId, msg.PluginInfo.PluginId, MemberContactsInfo.UserTypes.ShopManager);
        //            if (!msg.Biz.CheckDestination(destination))
        //                throw new HimallException(msg.Biz.ShortName + "错误");
        //            var content = msg.Biz.SendMessageOnShopSuccess(destination, info);
        //            if (msg.Biz.EnableLog)
        //            {
        //                context.MessageLog.Add(new MessageLog() { SendTime = DateTime.Now, ShopId = 0, MessageContent = content, TypeId = "短信" });
        //                context.SaveChanges();
        //            }
        //        }
        //    }
        //}

        //原结算发送消息 150624
        //public void SendMessageOnShopSettlement(long userId, MessageSettlementInfo info)
        //{
        //    var message = PluginsManagement.GetPlugins<IMessagePlugin>().ToList();
        //    foreach (var msg in message)
        //    {
        //        if (msg.Biz.GetStatus(MessageTypeEnum.ShopSettlement) == StatusEnum.Open)
        //        {
        //            string destination = GetDestination(userId, msg.PluginInfo.PluginId, MemberContactsInfo.UserTypes.ShopManager);
        //            if (!msg.Biz.CheckDestination(destination))
        //                throw new HimallException(msg.Biz.ShortName + "错误");
        //            var content = msg.Biz.SendMessageOnShopSettlement(destination, info);
        //            if (msg.Biz.EnableLog)
        //            {
        //                context.MessageLog.Add(new MessageLog() { SendTime = DateTime.Now, ShopId = 0, MessageContent = content, TypeId = "短信" });
        //                context.SaveChanges();
        //            }
        //        }
        //    }
        //}


        public void AddSendMessageRecord(dynamic model)
        {
            throw new NotImplementedException();
        }

        public QueryPageModel<object> GetSendMessageRecords(object querymodel)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 退款/退货失败通知
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="info"></param>
        /// <param name="refundid"></param>
        public void SendMessageOnOrderRefundFail(long userId, MessageOrderInfo info, long refundid = 0)
        {
            #region 发送模板消息
            if (info.MsgOrderType == MessageOrderType.Applet || info.MsgOrderType == MessageOrderType.O2OApplet)
            {
                //小程序发送
                var userinfo = Context.UserMemberInfo.FirstOrDefault(d => d.Id == userId);
                if (userinfo != null)
                {
                    var msgdata = new WX_MsgTemplateSendDataModel();
                    msgdata.keyword1.value = info.OrderId;
                    msgdata.keyword1.color = "#173177";
                    msgdata.keyword2.value = info.ProductName + "等...";
                    msgdata.keyword2.color = "#ff3300";
                    msgdata.keyword3.value = info.RefundMoney.ToString();
                    msgdata.keyword3.color = "#173177";
                    msgdata.keyword4.value = info.Remark;
                    msgdata.keyword4.color = "#173177";

                    //处理url
                    var _iwxtser = Himall.ServiceProvider.Instance<IWXMsgTemplateService>.Create;
                    var _iwxmember = Himall.ServiceProvider.Instance<IMemberService>.Create;
                    string page = _iwxtser.GetWXAppletMessageTemplateShowUrl(MessageTypeEnum.OrderRefundFail);//小程序跳转地址
                    page = page.Replace("{id}", info.OrderId.ToString());
                    if (refundid > 0)
                    {
                        page = page.Replace("{id}", refundid.ToString());
                    }
                    string openId = _iwxmember.GetMemberOpenIdInfoByuserIdAndType(userId, "WeiXinSmallProg").OpenId;//登录小程序的OpenId
                    if (info.MsgOrderType == MessageOrderType.O2OApplet)
                    {
                        openId = _iwxmember.GetMemberOpenIdInfoByuserIdAndType(userId, "O2OWeiXinSmallProg").OpenId;//登录小程序的OpenId
                    }
                    string formid = "";
                    var formmodel = _iwxtser.GetWXAppletFromDataById(MessageTypeEnum.OrderPay, info.OrderId.ToString());
                    if (formmodel != null)
                        formid = formmodel.FormId;//根据OrderId获取FormId
                    if (info.MsgOrderType == MessageOrderType.O2OApplet)
                        _iwxtser.SendAppletMessageByTemplate(MessageTypeEnum.OrderRefundFail, userId, msgdata, page, openId, formid, true);
                    else
                        _iwxtser.SendAppletMessageByTemplate(MessageTypeEnum.OrderRefundFail, userId, msgdata, page, openId, formid);
                }
            }
            else
            {
                //暂时不支持sms、email
                //var message = PluginsManagement.GetPlugins<IMessagePlugin>().ToList();
                //foreach (var msg in message)
                //{
                //    if (msg.Biz.GetStatus(MessageTypeEnum.OrderRefundFail) == StatusEnum.Open)
                //    {
                //        string destination = GetDestination(userId, msg.PluginInfo.PluginId, MemberContactsInfo.UserTypes.General);
                //        if (!msg.Biz.CheckDestination(destination))
                //            throw new HimallException(msg.Biz.ShortName + "错误");
                //        var content = msg.Biz.SendMessageOnOrderRefund(destination, info);
                //        if (msg.Biz.EnableLog)
                //        {
                //            Context.MessageLog.Add(new MessageLog() { SendTime = DateTime.Now, ShopId = info.ShopId, MessageContent = content, TypeId = "短信" });
                //            Context.SaveChanges();
                //        }
                //    }
                //}

                var userinfo = Context.UserMemberInfo.FirstOrDefault(d => d.Id == userId);
                if (userinfo != null)
                {
                    var msgdata = new WX_MsgTemplateSendDataModel();
                    msgdata.first.value = "非常抱歉，您的订单退款失败";
                    msgdata.first.color = "#000000";
                    msgdata.keyword1.value = info.OrderId.ToString();
                    msgdata.keyword1.color = "#FF0000";
                    msgdata.keyword2.value = info.OrderStatus.ToString();
                    msgdata.keyword2.color = "#FF0000";
                    msgdata.keyword3.value = info.TotalMoney.ToString();
                    msgdata.keyword3.color = "#FF0000";
                    msgdata.keyword4.value = "原路返回";
                    msgdata.keyword4.color = "#FF0000";
                    msgdata.remark.value = "详情请联系客服人员，感谢您的使用";
                    msgdata.remark.color = "#000000";
                    //处理url
                    var _iwxtser = Himall.ServiceProvider.Instance<IWXMsgTemplateService>.Create;
                    _iwxtser.SendMessageByTemplate(MessageTypeEnum.OrderRefundFail, userId, msgdata);
                }
            }
            #endregion
        }
    }
}

