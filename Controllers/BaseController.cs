using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Wlniao;
using Wlniao.XServer;

namespace Template.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    public partial class BaseController : XCoreController
    {
        protected String _UserId = "";
        protected String _OrganId = "";
        protected String _RightFor = "sys";
        protected Models.MyContext db = new Models.MyContext();
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (Request.Cookies.ContainsKey("uid"))
            {
                _UserId = Encryptor.AesDecrypt(Request.Cookies["uid"].ToString(), SourceConfig.CookieSafe);
            }
            if (_UserId.IsNullOrEmpty() && filterContext.ActionDescriptor.RouteValues["action"].ToLower() != "index")
            {
                if (string.IsNullOrEmpty(method))
                {
                    errorMsg = "您暂未登录，请先登录!";
                }
                else
                {
                    filterContext.Result = new JsonResult(new { success = false, message = "您暂未登录，请先登录!" });
                }
            }
            else
            {
                ViewBag.Assets = SourceConfig.AssetsPath;
                ViewBag.ResPath = SourceConfig.ResPath;
                ViewBag.SystemName = Models.Setting.Get("SystemName", SourceConfig.SystemName);
                ViewBag.ResVersion = (DateTools.GetUtcUnix() / 600).ToString();
                base.OnActionExecuting(filterContext);
            }
        }
        /// <summary>
        /// 返回无权限
        /// </summary>
        /// <returns></returns>
        public ActionResult NoRight()
        {
            var result = new ContentResult();
            if (string.IsNullOrEmpty(method))
            {
                errorMsg = "访问出错，请确认您是否有权执行此操作";
            }
            else
            {
                result.Content = Wlniao.Json.ToString(new { success = false, message = "操作失败，请确认您是否有权执行此操作" });
                result.ContentType = "text/json";
            }
            return result;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="RightCode"></param>
        /// <returns></returns>
        public bool Right(String RightCode)
        {
            return Models.Right.Check(_UserId, RightCode, _RightFor);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="RightCode"></param>
        /// <param name="RightFor"></param>
        /// <returns></returns>
        public bool Right(String RightCode, String RightFor)
        {
            return Models.Right.Check(_UserId, RightCode, RightFor);
        }
    }
}
