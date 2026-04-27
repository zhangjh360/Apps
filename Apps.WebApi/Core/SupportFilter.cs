using Apps.BLL;
using Apps.Common;
using Apps.Core;
using Apps.DAL;
using Apps.Models;
using Apps.Models.Sys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Routing;
using System.Web.Security;

namespace Apps.WebApi.Core
{
    public class SupportFilter : AuthorizeAttribute
    {
        //重写基类的验证方式，加入我们自定义的Ticket验证
        public override void OnAuthorization(System.Web.Http.Controllers.HttpActionContext actionContext)
        {
            // 首先尝试从 Authorization header 获取 token（优先），回退到 QueryString（兼容旧客户端）
            var content = actionContext.Request.Properties[ConfigPara.MS_HttpContext] as HttpContextBase;
            string token = null;
            if (actionContext.Request.Headers.Authorization != null && !string.IsNullOrEmpty(actionContext.Request.Headers.Authorization.Parameter))
            {
                token = actionContext.Request.Headers.Authorization.Parameter;
            }
            else
            {
                token = content.Request.QueryString[ConfigPara.Token];
            }

            if (!string.IsNullOrEmpty(token))
            {
                //解密用户ticket,并校验用户名密码是否匹配

                //读取请求上下文中的Controller,Action,Id
                var routes = new RouteCollection();
                RouteConfig.RegisterRoutes(routes);
                RouteData routeData = routes.GetRouteData(content);
                //取出区域的控制器Action,id
                string controller = actionContext.ActionDescriptor.ControllerDescriptor.ControllerName;
                string action = actionContext.ActionDescriptor.ActionName;
                //URL路径
                string filePath = HttpContext.Current.Request.FilePath;
                if (LoginUserManage.ValidateTicket(token) && ValiddatePermission(token, controller, action, filePath))
                {
                    // 验证成功后，应将当前用户身份注入 HttpContext，以便后续授权逻辑依赖 HttpContext.User
                    try
                    {
                        var userName = LoginUserManage.DecryptToken(token.Trim());
                        var identity = new System.Security.Principal.GenericIdentity(userName);
                        var principal = new System.Security.Principal.GenericPrincipal(identity, roles: null);
                        HttpContext.Current.User = principal;
                        System.Threading.Thread.CurrentPrincipal = principal;
                    }
                    catch
                    {
                        // 忽略注入失败，仍然认为已授权，由于 ValidateTicket 已通过
                    }
                    base.IsAuthorized(actionContext);
                }
                else
                {
                    HandleUnauthorizedRequest(actionContext);
                }
            }
            //如果取不到身份验证信息，并且不允许匿名访问，则返回未验证401
            else
            {
                var attributes = actionContext.ActionDescriptor.GetCustomAttributes<AllowAnonymousAttribute>().OfType<AllowAnonymousAttribute>();
                bool isAnonymous = attributes.Any(a => a is AllowAnonymousAttribute);
                if (isAnonymous) base.OnAuthorization(actionContext);
                else HandleUnauthorizedRequest(actionContext);
            }
        }




        public bool ValiddatePermission(string token, string controller, string action, string filePath)
        {
            bool bResult = false;

            List<permModel> perm = null;

            perm = (List<permModel>)HttpContext.Current.Session[filePath];
            if (perm == null)
            {
                SysUserBLL userBLL = new SysUserBLL()
                {
                    m_Rep = new SysUserRepository(new DBContainer()),
                    sysRightRep = new SysRightRepository(new DBContainer())
                };
                {
                    var userName = LoginUserManage.DecryptToken(token.Trim());
                    perm = userBLL.GetPermission(userName, controller);//获取当前用户的权限列表
                    HttpContext.Current.Session[filePath] = perm;//获取的劝降放入会话由Controller调用
                }
            }
                //查询当前Action 是否有操作权限，大于0表示有，否则没有
                int count = perm.Where(a => a.KeyCode.ToLower() == action.ToLower()).Count();
                if (count > 0)
                {
                    bResult = true;
                }
                else
                {
                    bResult = false;
                    LoginUserManage.RedirectUrlFor401();
                }

            
            return bResult;

        }
    }
}