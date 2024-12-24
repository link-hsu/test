//controller


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
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Index(DcnSsoBirthdayValidateViewModels model)
        {


            this.IsCaptchaValid("「驗證碼」輸入錯誤");
            string redirect_url = Url.Content("~/");
            if (ToValidate(model))
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
                }
                else
                {
                    DcnSsoBirthdayValidateUtil.UserType = "1";

                    logger.Info($"登入成功：{model.Account}、UserType：{DcnSsoUtil.UserType}、FrontMemberSSO：{SecurityUtil.AuthenticationManager.User.IsInRole("FrontMemberSSO")}、IP：{MvcUtil.WebTraceHostAddress}");


                    if (model.Account != null && model.Birthday != null)
                    {
                        DcnSsoBirthdayValidateUtil.Birthday = model.Birthday;
                        DcnSsoBirthdayValidateUtil.Pid = model.Account;
                        Boolean checkLoginProcess = true;
                        if (checkLoginProcess)
                        {
                            try
                            {
                                var user = await SecurityUtil.UserManager.FindByNameAsync(model.Account);
                                if (user != null && user.IsEnable)
                                {
                                    await SecurityUtil.SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                                    var identity = SecurityUtil.AuthenticationManager.AuthenticationResponseGrant?.Identity;
                                    if (identity != null && identity.IsAuthenticated)
                                    {
                                        UserUtil.Set(user);
                                        return RedirectToAction("Index", "Manage", new { Area = "Member" });
                                    }
                                    else
                                    {
                                        return RedirectToAction("Index", "Home", new { Area = "" });
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                logger.Info(ex.Message);
                                logger.Info(ex.StackTrace);
                            }

                            if (UserUtil.IsFrontMember
                                && model.Account == SecurityUtil.AuthenticationManager.User.Identity.Name
                                && UserUtil.Users != null
                                )
                            {
                                var result_role = SecurityUtil.UserManager.AddToRoles(UserUtil.Users.AspNetUsersId, new string[] { "FrontMemberSSO" });
                                if (result_role.Succeeded)
                                {
                                    UserUtil.Users.RolesName.Add("FrontMemberSSO");
                                }
                                else
                                {
                                    logger.Info("error");
                                }
                            }
                        }
                    }
                }
            }
            return View(model);
        }

        private Boolean ToValidate(DcnSsoBirthdayValidateViewModels model)
        {
            Boolean result = true;
            try
            {
                if (ModelState.IsValid)
                {
                    if (model.Account != SecurityUtil.AuthenticationManager.User.Identity.Name)
                        ModelState.AddModelError("Account", "您的會員帳號與交易帳號不相同，無法驗證成功");
                }

                if (!ModelState.IsValid)
                    result = false;
                else
                    result = true;
            }
            catch (Exception ex)
            {
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





// index.cshtml

@*@using Dcn.SqlClient.ViewModels.Front
    @using CaptchaMvc.HtmlHelpers;

    @model DcnSsoBirthdayValidateViewModels
    @{
        ViewBag.Title = "線上開戶 - 客戶交易帳號驗證";
        ViewBag.PageDescription = "大昌證券客戶驗證，請輸入帳號及密碼和驗證碼，";
        ViewBag.PageKeywords = "客戶交易帳號驗證,";
        ViewBag.MasterPagePanelHeadTitle = "線上開戶";
    }

    @using (Html.BeginForm("Index", "DcnSsoPassLogin", FormMethod.Post, new { @id = "MyForm", @AutoComplete = "Off", @role = "form" }))
    {
        @Html.AntiForgeryToken()
        <div class="form-horizontal">
            <div class="form-group">
                <div class="alert alert-danger col-md-offset-2 col-md-8 col-sm-offset-2 col-sm-8 col-xs-offset-1 col-xs-10 text-center">
                    <h2></h2>
                </div>
            </div>
            <div class="form-group">
                <div class="col-md-offset-2 col-md-8 text-center">
                    <div title="證券 HTML5 版">
                        <span class="badge">網路下單密碼，非線上開戶密碼</span>
                    </div>
                    <hr class="form-control-hr-space">
                </div>
            </div>
            @{Html.RenderPartial("FormMessage/_RequiredTextPartial");}
            <div class="form-group">
                @Html.Label("出生日期", new { @class = "col-md-3 col-sm-3 col-xs-12 control-label", @for = "Account" })
                <div class="col-md-7 col-sm-7 col-xs-12 ">
                    @Html.TextBoxFor(model => model.Birthday, new { @class = "form-control text-uppercase", @placeholder = "yyyy/mm/dd", @title = "請輸入「出生日期」(範例：yyyy/mm/dd)", @onclick = "WdatePicker({dateFmt:'yyyy/MM/dd'})" })
                    @Html.ValidationMessageFor(model => model.Birthday, "", new { @class = "text-danger" })
                </div>
            </div>
            <div class="form-group">
                @Html.Label("驗證碼", new { @class = "col-md-3 col-sm-3 col-xs-12 control-label", @for = "CaptchaInputText" })
                <div class="col-md-7 col-sm-7 col-xs-12 ">
                    @{
                        CaptchaMvc.Models.ParameterModel[] catchaParams = new CaptchaMvc.Models.ParameterModel[] {
                            new CaptchaMvc.Models.ParameterModel(CaptchaMvc.Infrastructure.DefaultCaptchaManager.InputTextAttribute , "請輸入驗證碼"),
                            new CaptchaMvc.Models.ParameterModel(CaptchaMvc.Infrastructure.DefaultCaptchaManager.PartialViewNameAttribute, "_Captcha"),
                            new CaptchaMvc.Models.ParameterModel(CaptchaMvc.Infrastructure.DefaultCaptchaManager.CaptchaNotValidViewDataKey, true),
                            new CaptchaMvc.Models.ParameterModel(CaptchaMvc.Infrastructure.DefaultCaptchaManager.RefreshTextAttribute, "點我更換圖片"),
                            new CaptchaMvc.Models.ParameterModel(CaptchaMvc.Infrastructure.DefaultCaptchaManager.ErrorAttribute, "輸入錯誤"),
                            new CaptchaMvc.Models.ParameterModel(CaptchaMvc.Infrastructure.DefaultCaptchaManager.IsRequiredAttribute, true),
                            new CaptchaMvc.Models.ParameterModel(CaptchaMvc.Infrastructure.DefaultCaptchaManager.RequiredMessageAttribute, "驗證碼 欄位(必填)")
                        };
                    }
                    @Html.Captcha(4, catchaParams)
                </div>
            </div>
            <div class="form-group">
                <div class="col-md-offset-2 col-md-8">
                    <hr class="form-control-hr-space">
                </div>
            </div>
            <div class="form-group">
                <div class="col-md-offset-2 col-md-4 col-xs-6">
                    <input type="submit" title="確定登入" class="btn btn-danger btn-block" value="確定" />
                </div>
                <div class="col-md-4 col-xs-6">
                    @Html.ActionLink("返回首頁", "index", "default", null, new { @class = "btn btn-primary btn-block", @title = "返回首頁" })
                </div>
            </div>
            <div class="form-group">
                <div class="col-md-offset-2 col-md-8">
                    <hr class="form-control-hr-space">
                </div>
            </div>
            <div class="form-group">
                <div class="col-md-offset-2 col-md-8 alert alert-warning">
                    <div class="h4">
                        出生年月日身分驗證
                    </div>
                </div>
            </div>
        </div>
    }

    @section Scripts{
        @Scripts.Render("~/Scripts/jqueryval")

        <script type="text/javascript">
            Dcn.Utility.SetInputMaxLength();
            $(function () {
                $("#MyForm").submit(function (event) {
                    Dcn.Message.FormSubmit();
                });
            })
        </script>

        @if (TempData["message"] != null)
        {
            <script type="text/javascript">
                var message = @Html.Raw(TempData["message"]);
                Dcn.Message.array("訊息",　message);
            </script>
        }
        else
        {
            <script type="text/javascript">
                $(document).ready(function () {
                    Dcn.Utility.CheckBorwser();
                });
            </script>
        }

        @{Html.RenderPartial("_FrontMemberInfoPartial");}
    }*@



@*----------------------------------------------------------------*@

@using Dcn.SqlClient.ViewModels.Front
@using CaptchaMvc.HtmlHelpers;

@model DcnSsoBirthdayValidateViewModels
@{
    ViewBag.Title = "線上開戶 - 客戶開戶帳號生日驗證";
    ViewBag.PageDescription = "大昌證券客戶帳號生日驗證，請輸入帳號及生日和驗證碼，";
    ViewBag.PageKeywords = "客戶開戶帳號驗證,";
    ViewBag.MasterPagePanelHeadTitle = "線上開戶";
}

@using (Html.BeginForm("Index", "DcnSsoBirthdayValidate", FormMethod.Post, new { @id = "MyForm", @AutoComplete = "Off", @role = "form" }))
{
    @Html.AntiForgeryToken()

    <div class="form-horizontal">
        <div class="form-group">
            <div class="alert alert-danger col-md-offset-2 col-md-8 col-sm-offset-2 col-sm-8 col-xs-offset-1 col-xs-10 text-center">
                <h2>客戶開戶帳號驗證</h2>
            </div>
        </div>
        <div class="form-group">
            <div class="col-md-offset-2 col-md-8 text-center">
                <div title="證券 HTML5 版">
                    <span class="badge">開戶帳號及生日，非網路下單</span>
                </div>
                <hr class="form-control-hr-space">
            </div>
        </div>
        @{Html.RenderPartial("FormMessage/_RequiredTextPartial");}
        <div class="form-group">
            @Html.Label("身分證字號", new { @class = "col-md-3 col-sm-3 col-xs-12 control-label", @for = "Account" })
            <div class="col-md-7 col-sm-7 col-xs-12 ">
                @Html.TextBoxFor(model => model.Account, new { @class = "form-control text-uppercase", @placeholder = "請輸入「身份證字號」", @title = "請輸入「身份證字號」", @MaxLength = "10", @AutoComplete = "Off" })
                @Html.ValidationMessageFor(model => model.Account, "", new { @class = "text-danger" })
            </div>
        </div>
        <div class="form-group">
            @Html.Label("出生日期", new { @class = "col-md-3 col-sm-3 col-xs-12 control-label", @for = "Birthday" })
            <div class="col-md-7 col-sm-7 col-xs-12 ">
                @Html.TextBoxFor(model => model.Birthday, new { @class = "form-control", @placeholder = "yyyy/mm/dd", @title = "請輸入「出生日期」(範例：yyyy/mm/dd)", @onclick = "WdatePicker({dateFmt:'yyyy/MM/dd'})" })
                @Html.ValidationMessageFor(model => model.Birthday, "", new { @class = "text-danger" })
            </div>
        </div>
        <div class="form-group">
            @Html.Label("驗證碼", new { @class = "col-md-3 col-sm-3 col-xs-12 control-label", @for = "CaptchaInputText" })
            <div class="col-md-7 col-sm-7 col-xs-12 ">
                @{
                    CaptchaMvc.Models.ParameterModel[] catchaParams = new CaptchaMvc.Models.ParameterModel[] {
                        new CaptchaMvc.Models.ParameterModel(CaptchaMvc.Infrastructure.DefaultCaptchaManager.InputTextAttribute , "請輸入驗證碼"),
                        new CaptchaMvc.Models.ParameterModel(CaptchaMvc.Infrastructure.DefaultCaptchaManager.PartialViewNameAttribute, "_Captcha"),
                        new CaptchaMvc.Models.ParameterModel(CaptchaMvc.Infrastructure.DefaultCaptchaManager.CaptchaNotValidViewDataKey, true),
                        new CaptchaMvc.Models.ParameterModel(CaptchaMvc.Infrastructure.DefaultCaptchaManager.RefreshTextAttribute, "點我更換圖片"),
                        new CaptchaMvc.Models.ParameterModel(CaptchaMvc.Infrastructure.DefaultCaptchaManager.ErrorAttribute, "輸入錯誤"),
                        new CaptchaMvc.Models.ParameterModel(CaptchaMvc.Infrastructure.DefaultCaptchaManager.IsRequiredAttribute, true),
                        new CaptchaMvc.Models.ParameterModel(CaptchaMvc.Infrastructure.DefaultCaptchaManager.RequiredMessageAttribute, "驗證碼 欄位(必填)")
                    };
                }
                @Html.Captcha(4, catchaParams)
            </div>
        </div>
        <div class="form-group">
            <div class="col-md-offset-2 col-md-8">
                <hr class="form-control-hr-space">
            </div>
        </div>
        <div class="form-group">
            <div class="col-md-offset-2 col-md-4 col-xs-6">
                <input type="submit" title="確定登入" class="btn btn-danger btn-block" value="確定" />
            </div>
            <div class="col-md-4 col-xs-6">
                @Html.ActionLink("返回首頁", "index", "default", null, new { @class = "btn btn-primary btn-block", @title = "返回首頁" })
            </div>
        </div>
        <div class="form-group">
            <div class="col-md-offset-2 col-md-8">
                <hr class="form-control-hr-space">
            </div>
        </div>
    </div>
}

@section Scripts{
    @Scripts.Render("~/Scripts/jqueryval")

    <script type="text/javascript">
        Dcn.Utility.SetInputMaxLength();
        $(function () {
            $("#MyForm").submit(function (event) {
                Dcn.Message.FormSubmit();
            });
        })
    </script>

    @if (TempData["message"] != null)
    {
        <script type="text/javascript">
            var message = @Html.Raw(TempData["message"]);
            Dcn.Message.array("訊息",　message);
        </script>
    }
    else
    {
        <script type="text/javascript">
            $(document).ready(function () {
                Dcn.Utility.CheckBorwser();
            });
        </script>
    }

    @{Html.RenderPartial("_FrontMemberInfoPartial");}
}



// select.cshtml

@{
    ViewBag.Title = "舊戶加開信用.款項借貸.不限用途款項借貸 線上開戶";
    ViewBag.PageDescription = "舊戶加開信用.款項借貸.不限用途款項借貸線上開戶";
    ViewBag.PageKeywords = "舊戶加開信用,款項借貸,不限用途款項借貸";
    ViewBag.MasterPagePanelHeadTitle = "線上開戶";
}

@Html.AntiForgeryToken()

@{ Html.RenderPartial(@"_DcnSsoStockPartial"); }

@section Scripts {
    <script>
    $(document).ready(function () {
      Dcn.Utility.CheckBorwser();
    });
    </script>

    <!-- Meta Pixel Code -->
    <script>
    !function (f, b, e, v, n, t, s) {
      if (f.fbq) return; n = f.fbq = function () {
        n.callMethod ?
        n.callMethod.apply(n, arguments) : n.queue.push(arguments)
      };
      if (!f._fbq) f._fbq = n; n.push = n; n.loaded = !0; n.version = '2.0';
      n.queue = []; t = b.createElement(e); t.async = !0;
      t.src = v; s = b.getElementsByTagName(e)[0];
      s.parentNode.insertBefore(t, s)
    }(window, document, 'script',
      'https://connect.facebook.net/en_US/fbevents.js');
    fbq('init', '1904340723284181');
    fbq('track', 'PageView');
    </script>
    <noscript>
        <img height="1" width="1" style="display:none"
             src="https://www.facebook.com/tr?id=1904340723284181&ev=PageView&noscript=1" />
    </noscript>
    <!-- End Meta Pixel Code -->

}

// partial.cshtml

<div class="row">
    <div class="col-md-12 col-xs-12">
        <div class="h3 text-red">※ 證券[舊客戶]加開：請點選欲加開之類別</div>
    </div>
    <div class="col-md-3 col-xs-6">
        @Html.ImageActionLink("~/Content/Image/Open/Customer-Credit-Add.jpg", "index", "CreditOpen", null, new { @class = "img-responsive", @alt = "舊戶加開「信用」", @title = "舊戶加開「信用」" })
    </div>
    <div class="col-md-3 col-xs-6">
        @Html.ImageActionLink("~/Content/Image/Open/Customer-BorrowLend.jpg", "index", "BorrowLendOpen", null, new { @class = "img-responsive", @alt = "舊戶加開「款項借貸」", @title = "舊戶加開「款項借貸」" })
    </div>
    <div class="col-md-3 col-xs-6">
        @Html.ImageActionLink("~/Content/Image/Open/Customer-BorrowLendAnyUse.jpg", "index", "BorrowLendAnyUseOpen", null, new { @class = "img-responsive", @alt = "舊戶加開「不限用途款項借貸」", @title = "舊戶加開「不限用途款項借貸」" })
    </div>
    <div class="col-md-3 col-xs-6">
        @Html.ImageActionLink("~/Content/Image/Open/Open-Subbrokerage.jpg", "index", "SubBrokerage", null, new { @class = "img-responsive", @alt = "舊戶加開「複委託」", @title = "舊戶加開「複委託」" })
    </div>
</div>

<div class="row">
    <div class="col-md-12 col-xs-12">
        <div class="h3 text-red">※ 預約開戶</div>
    </div>
    <div class="col-md-3 col-xs-6">
        @Html.ImageActionLink("~/Content/Image/Open/Reserve.jpg", "Index", "Reserve", null, new { @class = "img-responsive", @alt = "預約開戶", @title = "預約開戶" })
    </div>
</div>
