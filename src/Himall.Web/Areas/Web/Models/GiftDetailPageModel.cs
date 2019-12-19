﻿using System.Collections.Generic;
using Himall.Model;
using Himall.DTO;

namespace Himall.Web.Areas.Web.Models
{
    public class GiftDetailPageModel
    {
        /// <summary>
        /// 礼品信息
        /// </summary>
        public GiftInfo GiftData { get; set; }
        /// <summary>
        /// 热门礼品
        /// </summary>
        public List<GiftModel> HotGifts { get; set; }
        /// <summary>
        /// 是否可兑
        /// </summary>
        public bool GiftCanBuy { get; set; }
    }
}