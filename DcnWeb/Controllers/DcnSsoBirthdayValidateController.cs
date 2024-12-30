using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using CaptchaMvc.HtmlHelpers;
using System.Net;

using Dcn.SqlClient;
using DcnWeb.Areas.Member.Controllers;
using DcnWeb.Filters.Front;
using Microsoft.AspNet.Identity;
using DcnWeb.Areas.Back.Models;
using Elmah;
using log4net.Repository.Hierarchy;
using System.Web.Caching;
using System.Security.Cryptography;
using Dcn.SqlClient.ViewModels.Front;
using Dcn.DdscUtil;
using System.ServiceModel.Channels;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Dcn.SqlClient.ValidateAttribute;
using DcnWeb.Filters.Front.Open;

// 檔案命名 DdscCustomerinfoSyncController.cs

namespace DcnWeb.Controllers
{

    public class DcnSsoBirthdayValidateController : BaseController
    {
        private readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// version 1.0.0
        /// </summary>
        /// <param name="Pid"></param>
        /// <returns></returns>
        /// 

        // Get DcnBirthdayValidate
        public ActionResult Index()
        {
            try
            {
                DdscS703d fun703d = new DdscS703("R123845486").Get();
                if (fun703d != null)
                {
                    logger.Info("fun703d is not null");
                }
            }
            catch (Exception ex)
            {
                logger.Info(ex.Message);
                logger.Info(ex.StackTrace);
            }

            //if (!fun703d.return_code.Contains("000000"))
            //{
            //    logger.Info("return_code: " + fun703d.return_code);
            //}
            //else
            //{
            //    logger.Info("Get fun703d success");
            //    logger.Info("idno: " + fun703d.item.ret.cust_data.idno);
            //    logger.Info("cname: " + fun703d.item.ret.cust_data.cname);
            //    logger.Info("bdate: " + fun703d.item.ret.cust_data.bdate);
            //    logger.Info("ttel: " + fun703d.item.ret.cust_data.ttel);
            //    logger.Info("naddr: " + fun703d.item.ret.cust_data.naddr);
            //    logger.Info("iaddr: " + fun703d.item.ret.cust_data.iaddr);
            //}
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Index(DcnSsoBirthdayValidateViewModels model)
        {
            this.IsCaptchaValid("「驗證碼」輸入錯誤");
            ApplicationUser user = null;
            if (ToValidate(ref user, model))
            {
                DdscS703d fun703d = new DdscS703(model.Account).Get();
                if (!fun703d.return_code.Contains("000000"))
                {
                    switch (fun703d.return_code)
                    {
                        case "900001":
                            ModelState.AddModelError("Account", "無交易會員資料");
                            break;
                        case "900003":
                            ModelState.AddModelError("Account", "交易會員資料不可空白");
                            break;
                        default:
                            ModelState.AddModelError("Account", "讀取交易會員資料異常");
                            break;
                    }
                    return RedirectToAction("Login", "Account", new { Area = "Member" });
                }
                else
                {
                    DcnSsoBirthdayValidateUtil.UserType = "1";
                    logger.Info($"登入成功：{model.Account}、UserType：{DcnSsoUtil.UserType}、FrontMemberSSO：{SecurityUtil.AuthenticationManager.User.IsInRole("FrontMemberSSO")}、IP：{MvcUtil.WebTraceHostAddress}");

                    if (model.Account == fun703d.item.ret.cust_data.idno &&
                    model.Birthday?.ToString("yyyy/MM/dd").Replace("/", "") == fun703d.item.ret.cust_data.bdate)
                    {
                        DcnSsoBirthdayValidateUtil.Pid = model.Account;
                        DcnSsoBirthdayValidateUtil.Birthday = model.Birthday?.ToString("yyyy/MM/dd");
                        try
                        {
                            ApplicationUser user_1 = GetUser(model.Account);
                            UserUtil.Set(user_1);


                            //var user = await SecurityUtil.UserManager.FindByNameAsync(model.Account);
                            //if (user != null && user.IsEnable)
                            //{
                            //    await SecurityUtil.SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                            //    var identity = SecurityUtil.AuthenticationManager.AuthenticationResponseGrant?.Identity;
                            //    if (identity != null && identity.IsAuthenticated)
                            //    {
                            //        UserUtil.Set(user);
                            //        return RedirectToAction("Select");
                            //    }
                            //    else
                            //    {
                            //        return RedirectToAction("Login", "Account", new { Area = "Member" });
                            //    }
                            //}
                        }
                        catch (Exception ex)
                        {
                            logger.Info(ex.Message);
                            logger.Info(ex.StackTrace);
                        }

                        //if (UserUtil.IsFrontMember
                        //    && model.Account == SecurityUtil.AuthenticationManager.User.Identity.Name
                        //    && UserUtil.Users != null
                        //    )
                        //{
                        //    var result_role = SecurityUtil.UserManager.AddToRoles(UserUtil.Users.AspNetUsersId, new string[] { "FrontMemberSSO" });
                        //    if (result_role.Succeeded)
                        //    {
                        //        UserUtil.Users.RolesName.Add("FrontMemberSSO");
                        //    }
                        //    else
                        //    {
                        //        logger.Info("error");
                        //    }
                        //}
                    }
                }
            }
            return RedirectToAction("Login", "Account", new { Area = "Member" });
        }

        private ApplicationUser GetUser(string account)
        {
            ApplicationUser user = SecurityUtil.UserManager.FindByName(account);
            return user;
        }

        private Boolean ToValidate(ref ApplicationUser user, DcnSsoBirthdayValidateViewModels model)
        {
            Boolean result = false;
            try
            {
                String error = "請確認帳號是否正確！";
                if (ModelState.IsValid)
                {
                    user = SecurityUtil.UserManager.FindByName(model.Account);
                    if (user != null && user.IsEnable)
                    {
                        if (user.IsEnable && user.UserType != "10" && user.UserType != "20")
                        {
                            if (!user.IsPublicLogin)
                            {

                            }
                        }

                        if (user.UserType != "20")
                        {

                        }

                    } else
                    {
                        ModelState.AddModelError("", error);
                    }
                }
                if (ModelState.IsValid)
                    result = true;
                




                //    if (model.Account != SecurityUtil.AuthenticationManager.User.Identity.Name)
                //        ModelState.AddModelError("Account", "您的會員帳號與交易帳號不相同，無法驗證成功");
                //}

                //if (!ModelState.IsValid)
                //    result = false;
                //else
                //    result = true;
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"系統發生錯誤，請稍後再試!");
                logger.Info(ex.Message);
                logger.Info(ex.StackTrace);
            }
            return result;
        }


        public ActionResult Select()
        {
            return View();
        }




        //-----------------------------------------------------------------------
        //public ActionResult Index(string Pid)
        //{
        //    this.IsCaptchaValid("「驗證碼」輸入錯誤");

        //    String redirect_url = Url.Content("~/");

        //    if (string.IsNullOrEmpty(Pid))
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    else
        //    {

        //        var _user = await SecurityUtil.UserManager.FindByName(model.Account);
        //        if (user != null && user.IsEnable)
        //        {
        //            if (user.IsEnable && user.UserType != "10" && user.UserType != "20")
        //            {
        //                if (!user.IsPublicLogin)
        //                {
        //                    if (!MvcUtil.WebTraceHostAddress.Contains("192.168"))
        //                    {
        //                        string error = $"登入失敗！原因：後台外部網路不允許登入(您的IP.{MvcUtil.WebTraceHostAddress})，請洽管理員!";
        //                        ModelState.AddModelError("", error);
        //                        logger.Info(error);
        //                    }
        //                }
        //            }


        //        }

        //        UserUtil.Set(user);
        //    }

        //    ViewBag.Pid = Pid;
        //    return View();
        //}


        ///// <summary>
        ///// version 2
        ///// </summary>

        //public ActionResult Index(string Pid)
        //{
        //    ViewBag.Pid = Pid;
        //    return View();
        //}





        //[HttpPost]
        //public ActionResult Index(DcnSsoBirthdayValidateViewModels model)
        //{
        //    if (ToValidateIndex(model, ModelState))
        //    {

        //    }

        //    string Pid = ViewBag.Pid;
        //    if (string.IsNullOrEmpty(Pid))
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    else
        //    {
        //        ApplicationUser user = null;
        //        user = SecurityUtil.UserManager.FindByName(Pid);
        //        if (user != null && user.IsEnable)
        //        {
        //            if (user.IsEnable && user.UserType != "10" && user.UserType != "20")
        //            {
        //                if (!user.IsPublicLogin)
        //                {
        //                    if (!MvcUtil.WebTraceHostAddress.Contains("192.168"))
        //                    {
        //                        string error = $"登入失敗！原因：後台外部網路不允許登入(您的IP.{MvcUtil.WebTraceHostAddress})，請洽管理員!";
        //                        ModelState.AddModelError("", error);
        //                        logger.Info(error);
        //                    }
        //                }
        //            }
        //        }
        //        UserUtil.Set(user);
        //    }
        //    return View();
        //}

        //public Boolean ToValidate(DcnSsoBirthdayValidateViewModels model)
        //{
        //    Boolean result = true;
        //    try
        //    {
        //        if (ModelState.IsValid)
        //        {
        //            if (model.Account != SecurityUtil.AuthenticationManager.User.Identity.Name)
        //            {
        //                ModelState.AddModelError("Account", "您的會員帳號與交易帳號不相同，無法驗證成功");
        //            }
        //        }

        //        if (!ModelState.IsValid)
        //        {
        //            result = false;
        //        }
        //        else
        //        {
        //            result = true;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Info(ex.Message);
        //        logger.Info(ex.StackTrace);
        //    }

        //}
    }
}

