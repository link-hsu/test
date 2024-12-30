using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Dcn.SqlClient;
using Dcn.Util;
using DcnWeb.Filters;
using DcnWeb.Filters.Front.BorrowLendOpen;
using Dcn.SqlClient.ValidateAttribute;
using Dcn.SqlClient.ViewModels.Front;
using DcnWeb.Filters.Front.Member;
using DcnWeb.Filters.Front.Open;
using Dcn.SqlClient.ViewModels;
using Dcn.DdscUtil;

namespace DcnWeb.Controllers
{
    [NoCache]
    [SessionFilter]
    [MemberFilter]
    [OpenFilter]
    public class BorrowLendOpenController : BaseController
    {
        private readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
       
        [DcnSsoLoginFilter]
        public ActionResult Index()
        {
            BorrowLendOpenViewModels model = null;

            if (BorrowLendOpenUtil.IndexModel != null)
                model = BorrowLendOpenUtil.IndexModel;

            // 修改這邊

            DdscSDefaultInfo(model);

            model = Index_Page_Load(model);

            return View(model);
        }

        public void DdscSDefaultInfo(BorrowLendOpenViewModels model)
        {
            string referrer = HttpContext.Request.UrlReferrer?.ToString();
            string allowedReferrer = "https://customer.dcn.com.tw/DcnSsoBirthdayValidate/Select";
            if (referrer == allowedReferrer)
            {
                DdscS703d fun703d = new DdscS703(model.Pid).Get();
                if (fun703d.return_code.Contains("000000"))
                {
                    model.CustomerAccount = fun703d.item.ret.cust_data.cseq;
                    model.Name = fun703d.item.ret.cust_data.cname;

                    string bdate = fun703d.item.ret.cust_data.bdate;
                    model.Birthday = DateTime.ParseExact(bdate, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
                    model.ContactPhone = fun703d.item.ret.cust_data.ttel;
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(BorrowLendOpenViewModels model)
        {
            if (ToValidateIndex(model, ModelState))
            {
                BorrowLendOpenUtil.IndexModel = model;
                BorrowLendOpenUtil.IndexModel.CaseTypeCode = CaseTypeEnum.BorrowLendOpen;

                return RedirectToAction("ApplySign");
            }

            Index_Page_Load(model);

            TempData["message"] = ModelState.GetErrorJson();

            return View(model);
        }

        private bool ToValidateIndex(BorrowLendOpenViewModels model, ModelStateDictionary modelState)
        {
            Boolean result = false;

            try
            {
                if (OpenUtil.ToValidateOccupation(model.OccupationCode, ModelState))
                {
                    model.CompanyName = String.Empty;
                    model.CompanyTelePhone = String.Empty;
                    model.CompanyAddress = String.Empty;
                }

                if (ModelState.IsValid)
                {
                    Boolean exist_case = DaoService.Instance.GetDao<BorrowLendOpenDao>().GetExistPidInCase(model.Pid.ToUpper(), new List<String>(new String[] { "0", "8" }));
                    if (exist_case)
                        ModelState.AddModelError("Pid", "目前尚有案件正在辦理中，無法重覆申請！");
                }

                result = ModelState.IsValid;
            }
            catch (Exception ex)
            {
                logger.Info(ex.Message);
                logger.Info(ex.StackTrace);
            }

            return result;
        }

        private BorrowLendOpenViewModels Index_Page_Load(BorrowLendOpenViewModels model)
        {
            String open_type_code_value = "2";
            String pid_value = String.Empty;
            String branch_value = String.Empty;
            String customer_account_value = String.Empty;
            String name_value = String.Empty;
            String contact_phone_value = String.Empty;
            String mobile_phone_value = String.Empty;
            String agree_value = String.Empty;
            int borrowlendmoney_value = 0;
            String occupation_value = String.Empty;
            String jobtitle_value = String.Empty;
            String company_city_value = String.Empty;
            String company_town_value = String.Empty;

            if (model != null)
            {
                pid_value = model.Pid;
                branch_value = model.BranchCode;
                customer_account_value = model.CustomerAccount;
                name_value = model.Name;
                contact_phone_value = model.ContactPhone;
                mobile_phone_value = model.MobilePhone;
                borrowlendmoney_value = model.BorrowLendMoney;
                occupation_value = model.OccupationCode;
                jobtitle_value = model.JobTitleCode;

                open_type_code_value = model.OpenTypeCode;

                company_city_value = model.CompanyCityCode;
                company_town_value = model.CompanyTownCode;
            }
            List<SelectListItem> branch = DaoService.Instance.GetDao<CodeTreeDao>().SelectListItems<ViewCodeBranchStock>(null, branch_value);
            ViewBag.BranchStockList = branch;

            List<SelectListItem> company_city = DaoService.Instance.GetDao<CodeTreeDao>().SelectListItems
                <City>("CityName", "CityCode", "請選擇縣市", company_city_value, "Sort");
            ViewBag.CompanyCityList = company_city;

            List<SelectListItem> company_town = DaoService.Instance.GetDao<TownDao>().SelectListItemList(company_city_value, company_town_value);
            ViewBag.CompanyTownList = company_town;

            List<SelectListItem> OccupationList = DaoService.Instance.GetDao<CodeTreeDao>().SelectListItems<ViewCodeOccupation>(null, occupation_value);
            ViewBag.OccupationCodeList = OccupationList;

            List<SelectListItem> JobTitleCode = DaoService.Instance.GetDao<CodeTreeDao>().SelectListItems<ViewCodeJobTitle>(null, jobtitle_value);
            ViewBag.JobTitleCodeList = JobTitleCode;

            List<SelectListItem> opentypecode = DaoService.Instance.GetDao<CodeTreeDao>().SelectListItems<ViewCodeCreditOpenTypeCodeFront>(null, open_type_code_value);
            ViewBag.OpenTypeCodeList = opentypecode;

            return model;
        }

        #region -- ApplySign 申請憑證簽章 --
        public ActionResult ApplySign()
        {
            FrontOpenApplySignViewModels model = null;

            if (BorrowLendOpenUtil.ApplySignModel != null)
                model = BorrowLendOpenUtil.ApplySignModel;

            ApplySign_Page_Load(model);

            return View(model);
        }

        [HttpPost]
        public ActionResult ApplySign(FrontOpenApplySignViewModels model)
        {
            if (ToValidateApplySign(BorrowLendOpenUtil.ApplySignModel, ModelState))
                return RedirectToAction("Contract");

            ApplySign_Page_Load(model);

            TempData["message"] = ModelState.GetErrorJson();

            return View(model);
        }

        private void ApplySign_Page_Load(FrontOpenApplySignViewModels model)
        {
            String cert = String.Empty;
            String csr = String.Empty;
            String public_key_pem = String.Empty;
            String private_key_pem = String.Empty;
            String sign = String.Empty;
            String serial_number = String.Empty;
            String pid = String.Empty;

            if (model != null)
            {
                csr = model.Csr;
                public_key_pem = model.CsrPublicKeyToPem;
                private_key_pem = model.CsrPirvateKeyToPem;
                cert = model.CertContent;
                sign = model.Sign.ToString().ToLower();
                serial_number = model.SerialNumber;
            }

            if (BorrowLendOpenUtil.IndexModel != null)
                pid = BorrowLendOpenUtil.IndexModel.Pid;

            ViewBag.csr = csr;
            ViewBag.csrpublic_key_pem = public_key_pem;
            ViewBag.csrprivate_key_pem = private_key_pem;
            ViewBag.sign = sign;
            ViewBag.cert = cert;
            ViewBag.pid = pid;
        }

        private Boolean ToValidateApplySign(FrontOpenApplySignViewModels model, ModelStateDictionary ModelState)
        {
            Boolean result = false;
            try
            {
                if (model != null)
                {
                    if (model.Cert != true)
                    {
                        ModelState.AddModelError("Cert", "您尚未申請憑證，請點選「申請憑證」");
                        return false;
                    }

                    if (model.Sign != true)
                    {
                        ModelState.AddModelError("Sign", "請點選「確認簽章」！");
                        return false;
                    }

                    result = true;
                }
            }
            catch (Exception ex)
            {
                logger.Info(ex.Message);
                logger.Info(ex.StackTrace);
            }
            return result;
        }
        #endregion

        #region -- Contract 契約書 --
        public ActionResult Contract()
        {
            FrontOpenContractViewModels model = null;

            if (BorrowLendOpenUtil.ContractModel != null)
                model = BorrowLendOpenUtil.ContractModel;

            Contract_Page_Load(model);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Contract(FrontOpenContractViewModels model)
        {
            if (ModelState.IsValid)
            {
                BorrowLendOpenUtil.ContractModel = model;

                return RedirectToAction("IdentityConfirmUpload");
            }

            Contract_Page_Load(model);

            TempData["message"] = ModelState.GetErrorJson();

            return View(model);
        }

        private void Contract_Page_Load(FrontOpenContractViewModels model)
        {
            String agree_value = String.Empty;

            if (model != null)
                agree_value = model.Agree;

            List<SelectListItem> agree = DaoService.Instance.GetDao<CodeTreeDao>().SelectListItems<ViewCodeAgree>(null, agree_value);
            ViewBag.AgreeList = agree;
        }
        #endregion

        #region -- 上傳確認身分檔案 --
        public ActionResult IdentityConfirmUpload()
        {
            BorrowLendOpenIdentityConfirmUploadViewModels model = null;

            if (BorrowLendOpenUtil.IdentityUploadConfrimFrontModels != null)
            {
                model = BorrowLendOpenUtil.IdentityUploadConfrimFrontModels;
            }

            IdentityConfirm_Page_Load(model);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult IdentityConfirmUpload(BorrowLendOpenIdentityConfirmUploadViewModels model)
        {
            if (BorrowLendOpenUtil.IdentityUploadConfrimFrontModels != null)
                model = BorrowLendOpenUtil.IdentityUploadConfrimFrontModels;

            if (ToValidateIdentityConfirm(model, ModelState))
            {
                BorrowLendOpenUtil.IdentityUploadConfrimFrontModels = model;
                return RedirectToAction("IdentityUpload");
            }

            IdentityConfirm_Page_Load(model);

            TempData["message"] = ModelState.GetErrorJson();

            return View();
        }

        private void IdentityConfirm_Page_Load(BorrowLendOpenIdentityConfirmUploadViewModels model)
        {
            if (model != null)
            {
                model.IdentityConfirm_Url = model.IdentityConfirm_Url;
            }
        }

        private Boolean ToValidateIdentityConfirm(BorrowLendOpenIdentityConfirmUploadViewModels model, ModelStateDictionary ModelState)
        {
            Boolean result = false;
            try
            {
                ModelState.Clear();

                if (model.IdentityConfirm == false)
                    ModelState.AddModelError("IdentityConfirm", "請上傳：手持身分證自拍照");

                if (ModelState.IsValid)
                    result = true;
            }
            catch (Exception ex)
            {
                logger.Info(ex.Message);
                logger.Info(ex.StackTrace);
            }

            return result;
        }

        [HttpPost]
        public JsonResult UploadIdentityConfirm()
        {
            FileSaveResult file_save = null;
            if (System.Web.HttpContext.Current.Request.Files.AllKeys.Any())
            {
                foreach (String file_id in System.Web.HttpContext.Current.Request.Files)
                {
                    HttpPostedFile file = System.Web.HttpContext.Current.Request.Files[file_id];

                    if (file != null)
                    {
                        String virtual_dir_path = String.Empty;
                        String file_name = String.Empty;

                        if (BorrowLendOpenUtil.IdentityUploadFrontModels != null)
                        {
                            virtual_dir_path = BorrowLendOpenUtil.IdentityUploadFrontModels.WebDirPath;
                        }
                        else
                        {
                            if (BorrowLendOpenUtil.IdentityUploadConfrimFrontModels != null)
                                virtual_dir_path = BorrowLendOpenUtil.IdentityUploadConfrimFrontModels.WebDirPath;
                            else
                                virtual_dir_path = String.Format("{0}/{1}/{2}", ConstantUtil.TEMP_UPLOAD_BORROWLEND_PATH, DateTime.Now.ToString("yyyyMMdd"), Guid.NewGuid());
                        }

                        file_name = String.Format("{0}-{1}-{2}", "BorrowLendOpen", DateTime.Now.ToString("yyyyMMddHHmm"), Guid.NewGuid());

                        file_save = FileUtil.HttpPostedFileSave(file, virtual_dir_path, file_name, FileType.Image);

                        if (file_save.result)
                        {
                            logger.Info(String.Format("UploadIdentityConfirm Upload OK - virtual_dir_path：{0}、file_name：{1}、IP：{2}", virtual_dir_path, file_name, MvcUtil.WebTraceHostAddress));

                            file_save.file_id = file_id;
                            file_save = ConfirmSetUploadModel(file_save);
                        }
                    }
                };
            }

            return Json(file_save, "text/html", JsonRequestBehavior.AllowGet);
        }

        private FileSaveResult ConfirmSetUploadModel(FileSaveResult file)
        {
            FileSaveResult result = null;
            BorrowLendOpenIdentityConfirmUploadViewModels model = null;

            try
            {
                if (BorrowLendOpenUtil.IdentityUploadConfrimFrontModels != null)
                    model = BorrowLendOpenUtil.IdentityUploadConfrimFrontModels;
                else
                    model = new BorrowLendOpenIdentityConfirmUploadViewModels();

                file.web_dir_path = Url.Content(file.web_dir_path);
                file.web_file_path = Url.Content(file.web_file_path);

                switch (file.file_id)
                {
                    case "IdentityConfirm":
                        model.IdentityConfirm = file.result;
                        model.IdentityConfirm_Name = file.file_name;
                        model.IdentityConfirm_Url = file.web_file_path;
                        break;
                }

                model.WebDirPath = file.web_dir_path;

                file.file_byte = null;
                result = file;
                BorrowLendOpenUtil.IdentityUploadConfrimFrontModels = model;
            }
            catch (Exception ex)
            {
                result.result = false;
                result.error_message = String.Format("SetUploadModel_StackTrace：{0}、Message：{1}", ex.StackTrace, ex.Message);
            }

            return result;
        }
        #endregion




        #region -- IdentityUpload 上傳檔案 --
        public ActionResult IdentityUpload()
        {
            FrontOpenIdentityUploadViewModels model = null;

            if (BorrowLendOpenUtil.IdentityUploadFrontModels != null)
            {
                model = BorrowLendOpenUtil.IdentityUploadFrontModels;
            }

            Upload_Page_Load(model);

            return View(model);
        }

        [HttpPost]
        public ActionResult IdentityUpload(FrontOpenIdentityUploadViewModels model)
        {
            if (BorrowLendOpenUtil.IdentityUploadFrontModels != null)
                model = BorrowLendOpenUtil.IdentityUploadFrontModels;

            if (ToValidateUpload(model, ModelState))
            {
                return RedirectToAction("Confirm");
            }

            Upload_Page_Load(model);

            TempData["message"] = ModelState.GetErrorJson();

            return View(model);
        }

        private void Upload_Page_Load(FrontOpenIdentityUploadViewModels model)
        {
            if (model != null)
            {
                model.Identity1_Url = model.Identity1_Url;
                model.Identity2_Url = model.Identity2_Url;

                model.DriverLicence1_Url = model.DriverLicence1_Url;
                model.DriverLicence2_Url = model.DriverLicence2_Url;

                model.HealthInsurance1_Url = model.HealthInsurance1_Url;

                model.RealEstate1_Url = model.RealEstate1_Url;
                model.RealEstate2_Url = model.RealEstate2_Url;
                model.RealEstate3_Url = model.RealEstate3_Url;
            }
        }

        private Boolean ToValidateUpload(FrontOpenIdentityUploadViewModels model, ModelStateDictionary ModelState)
        {
            Boolean result = true;

            try
            {
                ModelState.Clear();

                if (!model.Identity1)
                    ModelState.AddModelError("Identity1", "請上傳：身分證「正面」");

                if (!model.Identity2)
                    ModelState.AddModelError("Identity2", "請上傳：身分證「反面」");

                if (model.DriverLicence1 && !model.DriverLicence2)
                    ModelState.AddModelError("DriverLicence2", "請上傳：駕照「反面」");

                if (!model.DriverLicence1 && model.DriverLicence2)
                    ModelState.AddModelError("DriverLicence1", "請上傳：駕照「正面」");

                if (!model.DriverLicence1 && !model.HealthInsurance1)
                {
                    ModelState.AddModelError("DriverLicence1", "【「駕照」或「健保卡」擇一上傳】");
                    ModelState.AddModelError("DriverLicence1", "請上傳：駕照「正面」");
                    ModelState.AddModelError("DriverLicence2", "請上傳：駕照「反面」");
                    ModelState.AddModelError("HealthInsurance1", "請上傳：「健保卡」");
                }

                if (model == null || model.RealEstate1 == false)
                {
                    ModelState.AddModelError("IdentityUpload.RealEstate1", "請上傳：「財力證明」必須上傳至少1個財力證明");
                }

                if (ModelState.IsValid)
                {
                    result = true;

                    model.IdentityConfirm1 = BorrowLendOpenUtil.IdentityUploadConfrimFrontModels.IdentityConfirm;
                    model.IdentityConfirm1_Name = BorrowLendOpenUtil.IdentityUploadConfrimFrontModels.IdentityConfirm_Name;
                    model.IdentityConfirm1_Url = BorrowLendOpenUtil.IdentityUploadConfrimFrontModels.IdentityConfirm_Url;
                }
            }
            catch (Exception ex)
            {
                result = false;
                logger.Info(ex.Message);
                logger.Info(ex.StackTrace);
            }

            return result;
        }
        /// <summary>
        /// 上傳圖片檔案
        /// </summary>
        [HttpPost]
        public JsonResult Upload()
        {
            FileSaveResult file_save = null;

            if (System.Web.HttpContext.Current.Request.Files.AllKeys.Any())
            {
                foreach (String file_id in System.Web.HttpContext.Current.Request.Files)
                {
                    HttpPostedFile file = System.Web.HttpContext.Current.Request.Files[file_id];

                    if (file != null)
                    {
                        String virtual_dir_path = String.Empty;
                        String file_name = String.Empty;

                        if (BorrowLendOpenUtil.IdentityUploadConfrimFrontModels != null)
                            virtual_dir_path = BorrowLendOpenUtil.IdentityUploadConfrimFrontModels.WebDirPath;
                        else
                            virtual_dir_path = String.Format("{0}/{1}/{2}", ConstantUtil.TEMP_UPLOAD_BORROWLEND_PATH, DateTime.Now.ToString("yyyyMMdd"), Guid.NewGuid());

                        file_name = String.Format("{0}-{1}-{2}", "BorrowLendOpen", DateTime.Now.ToString("yyyyMMddHHmm"), Guid.NewGuid());

                        file_save = FileUtil.HttpPostedFileSave(file, virtual_dir_path, file_name, FileType.Image);

                        if (file_save.result)
                        {
                            logger.Info(String.Format("doing： Upload file_id：{0} 、virtual_dir_path：{1}、file_name：{2}", file_id, virtual_dir_path, file_name));

                            file_save.file_id = file_id;
                            file_save = SetUploadModel(file_save);
                        }
                    }
                };
            }

            return Json(file_save, "text/html", JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult GetImg()
        {
            String post_url = System.Web.HttpContext.Current.Request.Form["post_url"];
            logger.Info(String.Format("Get_img (post_url：{0}", post_url));

            String r = FileUtil.HttpFileToBase64(post_url);
            // logger.Info("HttpFileToBase64：" + r);

            var jsonResult = Json(r, "text/html", JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = 10000000;
            return jsonResult;
        }

        private FileSaveResult SetUploadModel(FileSaveResult file)
        {
            FileSaveResult result = null;
            FrontOpenIdentityUploadViewModels model = null;

            try
            {
                if (BorrowLendOpenUtil.IdentityUploadFrontModels != null)
                    model = BorrowLendOpenUtil.IdentityUploadFrontModels;
                else
                    model = new FrontOpenIdentityUploadViewModels();

                file.web_dir_path = Url.Content(file.web_dir_path);
                file.web_file_path = Url.Content(file.web_file_path);

                switch (file.file_id)
                {
                    case "Identity1":
                        model.Identity1 = file.result;
                        model.Identity1_Name = file.file_name;
                        model.Identity1_Url = file.web_file_path;
                        break;
                    case "Identity2":
                        model.Identity2 = file.result;
                        model.Identity2_Name = file.file_name;
                        model.Identity2_Url = file.web_file_path;
                        break;
                    case "DriverLicence1":
                        model.DriverLicence1 = file.result;
                        model.DriverLicence1_Name = file.file_name;
                        model.DriverLicence1_Url = file.web_file_path;
                        break;
                    case "DriverLicence2":
                        model.DriverLicence2 = file.result;
                        model.DriverLicence2_Name = file.file_name;
                        model.DriverLicence2_Url = file.web_file_path;
                        break;
                    case "HealthInsurance1":
                        model.HealthInsurance1 = file.result;
                        model.HealthInsurance1_Name = file.file_name;
                        model.HealthInsurance1_Url = file.web_file_path;
                        break;
                    case "RealEstate1":
                        model.RealEstate1 = file.result;
                        model.RealEstate1_Name = file.file_name;
                        model.RealEstate1_Url = file.web_file_path;
                        break;
                    case "RealEstate2":
                        model.RealEstate2 = file.result;
                        model.RealEstate2_Name = file.file_name;
                        model.RealEstate2_Url = file.web_file_path;
                        break;
                    case "RealEstate3":
                        model.RealEstate3 = file.result;
                        model.RealEstate3_Name = file.file_name;
                        model.RealEstate3_Url = file.web_file_path;
                        break;

                }

                model.WebDirPath = file.web_dir_path;

                file.file_byte = null;
                result = file;
                BorrowLendOpenUtil.IdentityUploadFrontModels = model;
            }
            catch (Exception ex)
            {
                result.result = false;
                result.error_message = String.Format("BorrowLendOpen_SetUploadModel_StackTrace：{0}、Message：{1}", ex.StackTrace, ex.Message);
            }

            return result;
        }

        /// <summary>
        /// (刪除)證件檔案 DeleteUpload
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult UploadDelete(FileSaveResult file)
        {
            if (BorrowLendOpenUtil.IdentityUploadFrontModels != null && file != null && !String.IsNullOrEmpty(file.file_id))
                UploadOpenUtil.DeleteSession(BorrowLendOpenUtil.IdentityUploadFrontModels, file);
            else
                file = FileUtil.ErrorRestart();

            return Json(file, "text/html", JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region -- Confirm 確認 填寫內容 --
        public ActionResult Confirm()
        {
            Confirm_Page_Load(null);

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Confirm(FrontOpenConfirmViewModels model)
        {
            Boolean result = false;
            if (ModelState.IsValid)
            {
                result = SaveObj();
                if (result)
                {
                    BorrowLendOpenUtil.FileMove(AreaEnum.Front);

                    return RedirectToAction("Done");
                }
            }

            TempData["message"] = ModelState.GetErrorJson();

            Confirm_Page_Load(model);

            return View(model);
        }

        private void Confirm_Page_Load(FrontOpenConfirmViewModels model)
        {
            String agree_value = String.Empty;

            if (model != null)
                agree_value = model.Agree;

            List<SelectListItem> agree = DaoService.Instance.GetDao<CodeTreeDao>().SelectListItems<ViewCodeAgree>(null, agree_value);
            ViewBag.AgreeList = agree;
        }

        /// <summary>
        /// 0.存檔
        /// </summary>
        private Boolean SaveObj()
        {
            BorrowLendCustomer cases = null;
            ImageUpload upload = null;
            Boolean result = false;

            try
            {
                cases = SetCase();
                upload = SetUpload();

                using (var context = new DcnEntity())
                {
                    using (var tran = context.Database.BeginTransaction())
                    {
                        if (cases != null)
                            result = DaoService.Instance.Insert(context, cases);

                        if (result)
                        {
                            upload.CaseId = cases.BorrowLendCustomerId;
                            result = DaoService.Instance.Insert(context, upload);
                            if (!result)
                                ModelState.AddModelError("ImageUpload", ErrorTypeEnum.ImageUpload.GetDescription());
                        }
                        else
                        {
                            ModelState.AddModelError("Customer", ErrorTypeEnum.Customer.GetDescription());
                        }

                        if (result)
                        {
                            tran.Commit();

                            BorrowLendOpenUtil.CaseModel = new CustomerCaseViewModels()
                            {
                                CustomerCaseId = MemberUtil.Case != null ? MemberUtil.Case.CustomerCaseId : String.Empty,
                                CaseTypeCode = BorrowLendOpenUtil.IndexModel.CaseTypeCode.GetValueString(),
                                CaseTypeEnum = BorrowLendOpenUtil.IndexModel.CaseTypeCode,
                                CaseId = upload.CaseId,
                                CustomerId = cases.CustomerId,
                                ImageUploadId = upload.ImageUploadId,
                                CaseCreateDate = upload.CreateDate,
                                CreateUser = BorrowLendOpenUtil.IndexModel.Pid
                            };

                            BorrowLendOpenUtil.CaseModel.CaseIdSpilt.Add(CaseTypeEnum.BorrowLendOpen.GetValueString(), cases.BorrowLendCustomerId);
                        }
                        else
                        {
                            tran.Rollback();
                            ModelState.AddModelError("Case", ErrorTypeEnum.SaveCommit.GetDescription());
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                result = false;
                ModelState.AddModelError("SaveObj", ErrorTypeEnum.SaveCommit.GetDescription());

                logger.Info(ex.Message);
                logger.Info(ex.StackTrace);
            }

            return result;
        }

        /// <summary>
        /// 存檔：1.案件設定
        /// </summary>
        private BorrowLendCustomer SetCase()
        {
            BorrowLendCustomer cases = null;
            try
            {
                BorrowLendOpenViewModels IndexModel = BorrowLendOpenUtil.IndexModel;
                FrontOpenApplySignViewModels ApplySignModel = BorrowLendOpenUtil.ApplySignModel;

                cases = new BorrowLendCustomer();
                CustomerAccount account = new CustomerAccount();
                Customer customer = new Customer();
                CustomerCertification certification = new CustomerCertification();

                customer.CaseTypeCode = BorrowLendOpenUtil.IndexModel.CaseTypeCode.GetValueString();
                customer.Pid = IndexModel.Pid.ToUpper();
                customer.Name = IndexModel.Name;
                customer.ContactPhone = IndexModel.ContactPhone;
                customer.MobilePhone = IndexModel.MobilePhone;

                customer.OccupationCode = IndexModel.OccupationCode;
                customer.JobTitleCode = IndexModel.JobTitleCode;
                customer.CompanyName = IndexModel.CompanyName;
                customer.CompanyTelePhone = IndexModel.CompanyTelePhone;
                customer.CompanyCity = IndexModel.CompanyCityCode;
                customer.CompanyTown = IndexModel.CompanyTownCode;
                customer.CompanyAddress = IndexModel.CompanyAddress;
                customer.Email = IndexModel.Email;

                customer.Deleted = false;
                customer.CreateDate = DateTime.Now;
                customer.CreateUser = "User";

                account.AccountId = IndexModel.CustomerAccount;

                cases.OpenTypeCode = IndexModel.OpenTypeCode;
                cases.BorrowLendMoney = IndexModel.BorrowLendMoney;
                cases.BranchCode = IndexModel.BranchCode;

                cases.UserType = DcnSsoUtil.UserType;
                cases.SendToMail = "0";
                cases.StatusCode = "0";

                cases.Descriptions = String.Empty;
                cases.Deleted = false;

                #region 憑證資訊
                certification.SerialNumber = ApplySignModel.SerialNumber;
                certification.NotAfter = ApplySignModel.NotAfter;
                certification.NotBefore = ApplySignModel.NotBefore;
                certification.Cser = ApplySignModel.CertContent;
                certification.Csr = ApplySignModel.Csr;
                #endregion

                cases.Customer = customer;
                customer.CustomerAccount.Add(account);
                cases.Customer.CustomerCertification = certification;
            }
            catch (Exception ex)
            {
                cases = null;

                logger.Info(ex.Message);
                logger.Info(ex.StackTrace);
            }

            return cases;
        }

        /// <summary>
        /// 存檔：2.證件設定(必填)
        /// </summary>
        private ImageUpload SetUpload()
        {
            ImageUpload result = new ImageUpload();
            FrontOpenIdentityUploadViewModels upload = null;
            BorrowLendOpenIdentityConfirmUploadViewModels identity_confirm_model = null;

            try
            {
                upload = BorrowLendOpenUtil.IdentityUploadFrontModels;
                identity_confirm_model = BorrowLendOpenUtil.IdentityUploadConfrimFrontModels;

                result.CaseTypeCode = BorrowLendOpenUtil.IndexModel.CaseTypeCode.GetValueString();

                result.IdentityConfirmName1 = identity_confirm_model.IdentityConfirm_Name;

                result.IdentityName1 = upload.Identity1_Name;
                result.IdentityName2 = upload.Identity2_Name;

                if (upload.DriverLicence1)
                    result.DriverLicenceName1 = upload.DriverLicence1_Name;
                else
                    result.DriverLicenceName1 = null;

                if (upload.DriverLicence2)
                    result.DriverLicenceName2 = upload.DriverLicence2_Name;
                else
                    result.DriverLicenceName2 = null;

                if (upload.HealthInsurance1)
                    result.HealthInsuranceName1 = upload.HealthInsurance1_Name;
                else
                    result.HealthInsuranceName1 = null;

                result.RealEstateName1 = upload.RealEstate1_Name;

                if (upload.RealEstate2)
                    result.RealEstateName2 = upload.RealEstate2_Name;
                else
                    result.RealEstateName2 = null;

                if (upload.RealEstate3)
                    result.RealEstateName3 = upload.RealEstate3_Name;
                else
                    result.RealEstateName3 = null;

                result.CreateUser = "User";
                result.CreateDate = DateTime.Now;

            }
            catch (Exception ex)
            {
                result = null;

                logger.Info(ex.Message);
                logger.Info(ex.StackTrace);
            }

            return result;
        }
        #endregion

        public ActionResult Done()
        {
            return View();
        }
    }
}
