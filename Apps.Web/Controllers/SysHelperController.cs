using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using Apps.Common;
using Microsoft.Practices.Unity;
using Apps.IBLL;
using Apps.Models.Sys;
using Apps.Web.Core;

namespace Apps.Web.Controllers
{
    public class SysHelperController : Controller
    {
        //
        // GET: /SysHelper/
        [Dependency]
        public ISysStructBLL structBLL { get; set; }
        [Dependency]
        public ISysUserBLL sysUserBLL { get; set; }
        [Dependency]
        public ISysPositionBLL sysPosBLL { get; set; }

        public ActionResult Index()
        {
            return View();
        }
        #region 上传图片
        //上传图片
        public ActionResult UpLoadImg(string id="1")
        {
            ViewBag.Dif = id;
            return View();
        }
        [AcceptVerbs(HttpVerbs.Post)]
        public JsonResult Upload(HttpPostedFileBase fileData)
        {
            if (fileData != null)
            {
                try
                {
                    string fileName = Path.GetFileName(fileData.FileName);// 原始文件名称
                    string fileExtension = Path.GetExtension(fileName)?.ToLowerInvariant();  //获取文件扩展名
                    var allowedExtensions = new HashSet<string> { ".jpg", ".jpeg", ".png", ".gif", ".pdf" }; // 通过白名单限制上传
                    if (string.IsNullOrEmpty(fileExtension) || !allowedExtensions.Contains(fileExtension))
                    {
                        return Json(new { Success = false, Message = "不允许的文件类型！" }, JsonRequestBehavior.AllowGet);
                    }

                    //  防止双重扩展名攻击
                    if (fileName.Count(c => c == '.') > 1)
                    {
                        return Json(new { Success = false, Message = "文件名不能包含多个点号！" },
                            JsonRequestBehavior.AllowGet);
                    }

                    // 文件上传后的保存路径
                    string filePath = Server.MapPath("~/Uploads/");
                    if (!Directory.Exists(filePath))
                    {
                        Directory.CreateDirectory(filePath);
                    }
                    string saveName = ResultHelper.NewId + fileExtension; // 保存文件名称

                    fileData.SaveAs(filePath + saveName);

                    return Json(new { Success = true, FileName = fileName, SaveName = saveName, FilePath = "/Uploads/" + saveName }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    return Json(new { Success = false, Message = ex.Message }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {

                return Json(new { Success = false, Message = "请选择要上传的文件！" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion
        //导出时候读取报表
        public ActionResult ReportControl()
        {
            return View();
        }
        //万能查询
        public ActionResult Query()
        {
            return View();
        }



        #region 获取人员选择表
        public ActionResult UserLookUp()
        {
            CommonHelper commonHelper = new CommonHelper();
            ViewBag.StructTree = commonHelper.GetStructTree(true);
            return View();
        }
        public JsonResult GetUserListByDep(GridPager pager, string depId, string queryStr)
        {
            if (string.IsNullOrWhiteSpace(depId))
                return Json(0);
            var userList = sysUserBLL.GetUserByDepId(ref pager, depId, queryStr);

            var jsonData = new
            {
                total = pager.totalRows,
                rows = (
                    from r in userList
                    select new SysUserModel()
                    {
                        Id = r.Id,
                        UserName = r.UserName,
                        TrueName = r.TrueName,
                        DepName = structBLL.GetById(r.DepId).Name,
                        PosName =sysPosBLL.GetById(r.PosId).Name,
                        Flag = "<input type='checkbox' id='cb_" + r.Id + "' onclick='SetValue(\"" + r.Id + "\",\"" + r.TrueName + "\")'>",
                    }
                ).ToArray()
            };
            return Json(jsonData);
        }
        #endregion

        #region 获取部门选择表多选
        public ActionResult DepMulLookUp()
        {
            CommonHelper commonHelper = new CommonHelper();
            ViewBag.StructMulTree = commonHelper.GetStructMulTree();
            return View();
        }
        #endregion

        #region 获取部门单选
        public ActionResult DepLookUp()
        {
            CommonHelper commonHelper = new CommonHelper();
            ViewBag.StructTree = commonHelper.GetStructTree(false);
            return View();
        }
        #endregion

        #region 获取职位选择表
        public ActionResult PosMulLookUp()
        {
            CommonHelper commonHelper = new CommonHelper();
            ViewBag.StructTree = commonHelper.GetStructTree(false);
            return View();
        }
        public JsonResult GetPosListByDep(GridPager pager, string depId)
        {
            var userList = sysPosBLL.GetPosListByDepId(ref pager, depId);

            var jsonData = new
            {
                total = pager.totalRows,
                rows = (
                    from r in userList
                    select new SysPositionModel()
                    {
                        Id = r.Id,
                        Name = r.Name,
                        DepName = r.DepName,
                        Flag = "<input type='checkbox' id='cb_" + r.Id + "' onclick='SetValue(\"" + r.Id + "\",\"" + r.Name + "\")'>",
                    }
                ).ToArray()
            };
            return Json(jsonData);
        }
        #endregion

        #region 获取职位选择表
        public ActionResult PosLookUp()
        {
            CommonHelper commonHelper = new CommonHelper();
            ViewBag.StructTree = commonHelper.GetStructTree(false);
            return View();
        }
     
        #endregion
    }
}
