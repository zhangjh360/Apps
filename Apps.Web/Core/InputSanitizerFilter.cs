using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace Apps.Web.Core
{
    // 全局过滤器：对 QueryString 中的参数进行基本白名单校验或简单清理
    public class InputSanitizerFilter : ActionFilterAttribute
    {
        // 使用逐字字符串常量，避免编译器对 \- 等转义解释错误
        private const string Pattern = @"^[\w@.\u4e00-\u9fa5\- ]{0,1000}$";

        // 简单示例白名单：允许的字符（可根据参数名定制更严格规则）
        private static readonly Regex DefaultAllow = new Regex(Pattern, RegexOptions.Compiled);

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var request = filterContext.HttpContext.Request;
            // 遍历 QueryString 进行校验
            foreach (string key in request.QueryString.Keys)
            {
                if (string.IsNullOrEmpty(key)) continue;
                var vals = request.QueryString.GetValues(key);
                if (vals == null) continue;
                foreach (var v in vals)
                {
                    if (string.IsNullOrEmpty(v)) continue;
                    // 如果包含 <script> 或 常见 XSS 特征，视为可疑
                    if (v.IndexOf("<script", StringComparison.OrdinalIgnoreCase) >= 0 || v.IndexOf("javascript:", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        // 记录并拒绝请求
                        filterContext.Result = new HttpStatusCodeResult(400, "Bad Request");
                        return;
                    }
                    // 使用默认白名单检查，失败则继续（可记录告警）
                    if (!DefaultAllow.IsMatch(v))
                    {
                        filterContext.Result = new HttpStatusCodeResult(400, "Bad Request");
                        return;
                    }
                }
            }

            base.OnActionExecuting(filterContext);
        }
    }
}
