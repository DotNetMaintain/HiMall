using Himall.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Entities
{
    public partial class MemberInfo
    {
        /// <summary>
        /// 显示昵称
        /// <para>无昵称则显示用户名</para>
        /// </summary>
        public string ShowNick
        {
            get
            {
                string result = this.Nick;
                if (string.IsNullOrWhiteSpace(result))
                {
                    result = this.UserName;
                }
                return result;
            }
        }

        /// <summary>
        /// 是否是微信用户
        /// </summary>
        public bool IsWeiXinUser
        {
            get
            {
                return !string.IsNullOrEmpty(this.PasswordSalt) && this.PasswordSalt.StartsWith("o");
            }
        }
        /// <summary>
        /// 会员折扣(0.00-1)
        /// </summary>
        public decimal MemberDiscount { get; set; }
    }
}
