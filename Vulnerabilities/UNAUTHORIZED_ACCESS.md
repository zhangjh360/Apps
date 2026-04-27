# 未授权访问漏洞清单

此文档用于记录仓库中发现的“未授权（接口可任意访问）”相关漏洞位置与简要说明，后续可在此文件中追加更多漏洞记录。

## 概要
发现多个鉴权/权限校验逻辑存在缺陷，可能导致未授权访问或权限绕过。已记录并在代码中修复部分关键问题。

---

## 漏洞记录

1. 文件: Apps.Web\Core\SupportFilter.cs
   - 问题: ValiddatePermission 方法原本在结尾始终返回 true，导致鉴权失效。
   - 风险: 未登录或无权限用户可访问受保护的界面/接口。
   - 状态: 已修复（返回实际校验结果，并改用基于用户的会话键缓存权限）。

2. 文件: Apps.WebApi\Core\SupportFilter.cs
   - 问题: 从 QueryString 获取 token 并使用 Session 验证，但未将用户身份注入 HttpContext，且 token 放 URL 易泄露。
   - 风险: token 泄露、鉴权绕过、后续基于 User 的鉴权失效。
   - 状态: 建议修复（尚未修改） — 建议改为从 Authorization Header 读取 token，验证后设置 Principal。

3. 文件: Apps.Core\LoginUserManage.cs
   - 问题: ValidateIsLogined 在 account 为 null 时仍返回 true；ValidateTicket 仅依赖 Session 比对，没有过期检查。
   - 风险: 未登录用户被当作已登录处理；token 重放/会话固定风险。
   - 状态: 建议修复（尚未修改） — 建议对方法返回逻辑进行修正，并使用更安全的 token 管理策略。
   - 变更: 已修复 ValidateIsLogined 返回逻辑（返回 false 当未登录）。

4. 文件: Apps.Web\Controllers\SysUserController.cs
   - 问题: 用户创建时使用弱默认密码（"123456"）。
   - 风险: 攻击者可利用默认口令登录并获取用户权限。
   - 建议: 强制首次登录修改默认口令、实施密码复杂度策略、增加验证码及限流。
   - 状态: 已修复 — 移除弱默认密码；新增密码强度校验（至少8位且需包含数字/大小写/特殊字符中至少3类）。

---

## 使用说明
- 后续新漏洞请在本文件末尾追加编号记录，并说明文件、问题、风险与修复状态。
- 若需要，我可以继续对 Apps.WebApi\Core\SupportFilter.cs 和 Apps.Core\LoginUserManage.cs 执行修复补丁。

---

生成时间: 自动
