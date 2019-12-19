using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Himall.Core;
using System.Net.Http;

namespace Himall.API
{
    /// <summary>
    /// APP端授权
    /// </summary>
    public class WebApiAuthAttribute : AuthorizationFilterAttribute
    {
        public override void OnAuthorization(HttpActionContext actionContext)
        {
            var siteSettingsInfo = Application.SiteSettingApplication.GetSiteSettings();
            if (siteSettingsInfo != null && !siteSettingsInfo.IsOpenApp)
            {
                HttpResponseMessage result = new HttpResponseMessage();
                string jsonstr = "{\"IsOpenApp\":\"{0}\"}";
                jsonstr = jsonstr.Replace("\"{0}\"", "false");
                result.Content = new StringContent(jsonstr, Encoding.GetEncoding("UTF-8"), "application/json");
                actionContext.Response = result;
            }
        }
    }
}
