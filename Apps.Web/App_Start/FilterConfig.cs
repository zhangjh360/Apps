using System.Web;
using System.Web.Mvc;

namespace Apps.Web
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            // 添加全局输入清理过滤器，拦截可疑 GET 参数
            filters.Add(new Apps.Web.Core.InputSanitizerFilter());
        }
    }
}
