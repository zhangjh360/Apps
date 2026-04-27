# 跨站脚本攻击（XSS）漏洞清单

此文档用于记录仓库中发现的 XSS 相关漏洞位置、修复建议与当前状态，后续可追加记录。

## 概要
XSS 产生于未对用户输入或数据库内容进行正确的上下文编码或过滤，攻击者可向页面注入脚本，盗取 Cookie 或执行未授权操作。

## 已发现与修复的位置

1. 文件: Apps.Web\Core\ExtendMvcHtml.cs
   - 问题: 在生成 HTML 时直接使用未编码的 id、icon、text 等值，存在反射型/存储型 XSS 风险。
   - 风险: 恶意用户或数据导致页面注入脚本。
   - 状态: 已修复 — 在生成 HTML 时对变量使用 HttpUtility.HtmlEncode。

## 建议
- 在所有输出到 HTML 的位置使用适当的编码（HtmlEncode、UrlEncode 等），不要使用 Html.Raw 或 MvcHtmlString 包裹未验证的输入。
- 对富文本编辑器输入采用白名单清洗（例如只允许基本标签和属性），可使用 Microsoft AntiXSS 或第三方库如 HtmlSanitizer。
- 对上传或外部数据做严格校验与输出编码。
- 可在全局引入 MVC 过滤器或 HttpModule，对响应进行后置扫描替换但这仅作为补充，不能代替上下文编码。

## 是否可通过中间件（过滤器）拦截？
- 可以在 ASP.NET MVC 中实现 ActionFilter 或在 WebApi 中实现 DelegatingHandler 来对入站参数进行统一的输入清理或输出编码的辅助检查，但最可靠的方法仍是在每个输出点进行上下文编码与对富文本进行白名单清洗。
- 我可以为你实现一个简单的 MVC ActionFilter（或 WebApi DelegatingHandler）来对字符串参数进行基础的清理（如去除 script 标签），并将其添加到仓库中供进一步迭代。

---

生成时间: 自动
