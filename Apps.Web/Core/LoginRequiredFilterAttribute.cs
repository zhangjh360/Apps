using System.Web.Mvc;

namespace Apps.Web.Core
{
    public class LoginRequiredFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext == null || filterContext.IsChildAction)
            {
                base.OnActionExecuting(filterContext);
                return;
            }

            if (filterContext.HttpContext.Session?["Account"] != null)
            {
                base.OnActionExecuting(filterContext);
                return;
            }

            string controller = (string)filterContext.RouteData.Values["controller"] ?? string.Empty;
            string action = (string)filterContext.RouteData.Values["action"] ?? string.Empty;

            if (controller.Equals("Account", System.StringComparison.OrdinalIgnoreCase))
            {
                if (action.Equals("Index", System.StringComparison.OrdinalIgnoreCase)
                    || action.Equals("Login", System.StringComparison.OrdinalIgnoreCase)
                    || action.Equals("ValidateCode", System.StringComparison.OrdinalIgnoreCase))
                {
                    base.OnActionExecuting(filterContext);
                    return;
                }
            }

            filterContext.Result = new RedirectToRouteResult(
                new System.Web.Routing.RouteValueDictionary(
                    new
                    {
                        controller = "Account",
                        action = "Index",
                        url = filterContext.HttpContext.Request.RawUrl
                    }));
        }
    }
}
