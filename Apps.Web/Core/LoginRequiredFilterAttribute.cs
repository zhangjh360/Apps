using System.Web.Mvc;

namespace Apps.Web.Core
{
    /// <summary>
    /// 在 Action 执行前校验当前请求是否需要登录。
    /// 未登录且不在白名单中的请求会被重定向到登录页。
    /// </summary>
    public class LoginRequiredFilterAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// 在目标 Action 执行前进行登录拦截。
        /// </summary>
        /// <param name="filterContext">当前请求的过滤器上下文。</param>
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // 过滤器上下文为空，或者当前请求是子 Action 时，直接放行。
            if (filterContext == null || filterContext.IsChildAction)
            {
                base.OnActionExecuting(filterContext);
                return;
            }

            // Session 中存在 Account，说明用户已登录，直接放行。
            if (filterContext.HttpContext.Session?["Account"] != null)
            {
                base.OnActionExecuting(filterContext);
                return;
            }

            // 读取当前请求对应的控制器和 Action，用于白名单判断。
            string controller = (string)filterContext.RouteData.Values["controller"] ?? string.Empty;
            string action = (string)filterContext.RouteData.Values["action"] ?? string.Empty;

            // 登录页、登录提交和验证码接口本身不能被继续拦截，否则会形成循环跳转。
            if (IsInWhitelist(controller, action))
            {
                base.OnActionExecuting(filterContext);
                return;
            }

            // 未登录且不在白名单中时，重定向到登录页，并携带原始地址用于登录后回跳。
            filterContext.Result = new RedirectToRouteResult(
                new System.Web.Routing.RouteValueDictionary(
                    new
                    {
                        controller = "Account",
                        action = "Index",
                        url = filterContext.HttpContext.Request.RawUrl
                    }));
        }

        /// <summary>
        /// 判断当前请求是否属于登录白名单。
        /// </summary>
        /// <param name="controller">控制器名称。</param>
        /// <param name="action">动作名称。</param>
        /// <returns>属于白名单返回 true，否则返回 false。</returns>
        private static bool IsInWhitelist(string controller, string action)
        {
            if (!controller.Equals("Account", System.StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return action.Equals("Index", System.StringComparison.OrdinalIgnoreCase)
                || action.Equals("Login", System.StringComparison.OrdinalIgnoreCase)
                || action.Equals("ValidateCode", System.StringComparison.OrdinalIgnoreCase);
        }
    }
}
