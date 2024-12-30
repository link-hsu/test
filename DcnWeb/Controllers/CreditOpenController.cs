using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Dcn.SqlClient.ValidateAttribute;
using Dcn.SqlClient;
using DcnWeb.Filters;
using DcnWeb.Filters.Front.CreditOpen;
using Dcn.Util;
using Dcn.DdscUtil;

using Newtonsoft.Json;
using Dcn.SqlClient.ViewModels.Front;
using DcnWeb.Filters.Front.Member;
using DcnWeb.Filters.Front.Open;

namespace DcnWeb.Controllers
{
    [NoCache]
    [SessionFilter]
    [MemberFilter]
    [OpenFilter]
    public class CreditOpenController : BaseController
    {
        private readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [DcnSsoLoginFilter]
        public ActionResult Index()
        {
            CreditOpenViewModels model = null;

            if (CreditOpenUtil.IndexModel != null)
                model = CreditOpenUtil.IndexModel;

            //修改這邊

            DdscSDefaultInfo(model);

            model = Index_Page_Load(model);
            //Index_Page_Load(model);

            return View(model);
        }

        public void DdscSDefaultInfo(CreditOpenViewModels model)
        {
            string referrer = HttpContext.Request.UrlReferrer?.ToString();
            string allowedReferrer = "https://customer.dcn.com.tw/DcnSsoBirthdayValidate/Select";
            if (referrer == allowedReferrer)
            {
                DdscS703d fun703d = new DdscS703(model.Pid).Get();
                if (fun703d.return_code.Contains("000000"))
                {
                    model.Name = fun703d.item.ret.cust_data.cname;

                    string bdate = fun703d.item.ret.cust_data.bdate;
                    model.Birthday = DateTime.ParseExact(bdate, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
                    model.ContactPhone = fun703d.item.ret.cust_data.ttel;
                    model.ResidentAddress = fun703d.item.ret.cust_data.naddr;
                    model.Address = fun703d.item.ret.cust_data.iaddr;
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(CreditOpenViewModels model)
        {
            if (ToValidateIndex(model, ModelState))
            {
                CreditOpenUtil.IndexModel = model;
                CreditOpenUtil.IndexModel.CaseTypeCode = CaseTypeEnum.CreditOpen;

                return RedirectToAction("ApplySign");
            }

            Index_Page_Load(model);

            TempData["message"] = ModelState.GetErrorJson();

            return View(model);
        }

        private bool ToValidateIndex(CreditOpenViewModels model, ModelStateDictionary modelState)
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
                    if (String.IsNullOrEmpty(model.ProofOfProperty))
                        ModelState.AddModelError("ProofOfProperty", DisplayNameUtil.GetDisplayName<CreditOpenViewModels, String>(o => o.ProofOfProperty));

                    if (String.IsNullOrEmpty(model.ExtensionTimeCode))
                        ModelState.AddModelError("ExtensionTimeCode", DisplayNameUtil.GetDisplayName<CreditOpenViewModels, String>(o => o.ExtensionTimeCode));

                    Boolean exist_case = DaoService.Instance.GetDao<CreditOpenDao>().GetExistPidInCase(model.Pid.ToUpper(), new List<String>(new String[] { "0", "8" }));
                    if (exist_case)
                        ModelState.AddModelError("Pid", "您目前尚有信用案件正在辦理中，無法重覆申請！");
                }

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

        private CreditOpenViewModels Index_Page_Load(CreditOpenViewModels model)
        {
            CompanyTypeEnum company_type = (CompanyTypeEnum)ViewBag.LayoutEnum;
            String open_type_code_value = String.Empty;
            String pid_value = String.Empty;
            String branch_value = String.Empty;
            String customer_account_value = String.Empty;
            String name_value = String.Empty;
            String contact_phone_value = String.Empty;
            String mobile_phone_value = String.Empty;
            String extension_time_code_value = "3";

            int margin_trading_short_selling_value = 0;
            String proof_of_property_value = String.Empty;
            String agree_value = String.Empty;

            String resident_city_value = String.Empty;
            String resident_town_value = String.Empty;

            String city_value = String.Empty;
            String town_value = String.Empty;

            String occupation_value = String.Empty;
            String jobtitle_value = String.Empty;
            String company_city_value = String.Empty;
            String company_town_value = String.Empty;
            List<SelectListItem> branch = null;

            if (model != null)
            {
                open_type_code_value = model.OpenTypeCode;
                pid_value = model.Pid;
                branch_value = model.BranchCode;
                customer_account_value = model.CustomerAccount;
                name_value = model.Name;
                contact_phone_value = model.ContactPhone;
                mobile_phone_value = model.MobilePhone;
                extension_time_code_value = model.ExtensionTimeCode;
                margin_trading_short_selling_value = model.MarginTradingShortSelling;
                proof_of_property_value = model.ProofOfProperty;

                city_value = model.CityCode;
                town_value = model.TownCode;
                resident_city_value = model.ResidentCityCode;
                resident_town_value = model.ResidentTownCode;

                occupation_value = model.OccupationCode;
                jobtitle_value = model.JobTitleCode;

                company_city_value = model.CompanyCityCode;
                company_town_value = model.CompanyTownCode;
            }
            else if (!String.IsNullOrEmpty(DcnSsoUtil.OpenType))
            {
                open_type_code_value = DcnSsoUtil.OpenType;
            }

            branch = DaoService.Instance.GetDao<BranchDao>().List(company_type.GetValueString(), branch_value, false);
            ViewBag.BranchStockList = branch;

            List<SelectListItem> company_city = DaoService.Instance.GetDao<CodeTreeDao>().SelectListItems
                <City>("CityName", "CityCode", "請選擇縣市", company_city_value, "Sort");
            ViewBag.CompanyCityList = company_city;

            List<SelectListItem> company_town = DaoService.Instance.GetDao<TownDao>().SelectListItemList(company_city_value, company_town_value);
            ViewBag.CompanyTownList = company_town;

            List<SelectListItem> resident_city = DaoService.Instance.GetDao<CodeTreeDao>().SelectListItems
                <City>("CityName", "CityCode", "請選擇縣市", resident_city_value, "Sort");
            ViewBag.ResidentCityList = resident_city;

            List<SelectListItem> resident_town = DaoService.Instance.GetDao<TownDao>().SelectListItemList(resident_city_value, resident_town_value);
            ViewBag.ResidentTownList = resident_town;

            List<SelectListItem> city = DaoService.Instance.GetDao<CodeTreeDao>().SelectListItems
                <City>("CityName", "CityCode", "請選擇縣市", city_value, "Sort");
            ViewBag.CityList = city;

            List<SelectListItem> town = DaoService.Instance.GetDao<TownDao>().SelectListItemList(city_value, town_value);
            ViewBag.TownList = town;

            List<SelectListItem> opentypecode = DaoService.Instance.GetDao<CodeTreeDao>().SelectListItems<ViewCodeCreditOpenTypeCodeFront>(null, open_type_code_value);
            ViewBag.OpenTypeCodeList = opentypecode;

            List<SelectListItem> extension_time_code = DaoService.Instance.GetDao<CodeTreeDao>().SelectListItems<ViewCodeExtensionTimeCode>(null, extension_time_code_value);
            ViewBag.ExtensionTimeCodeList = extension_time_code;

            List<SelectListItem> proof_of_property = DaoService.Instance.GetDao<CodeTreeDao>().SelectListItems<ViewCodeCreditOpenProofOfProperty>(null, proof_of_property_value);
            ViewBag.ProofOfPropertyList = proof_of_property;

            List<SelectListItem> agree = DaoService.Instance.GetDao<CodeTreeDao>().SelectListItems<ViewCodeAgree>(null, agree_value);
            ViewBag.AgreeList = agree;

            List<SelectListItem> OccupationList = DaoService.Instance.GetDao<CodeTreeDao>().SelectListItems<ViewCodeOccupation>(null, occupation_value);
            ViewBag.OccupationCodeList = OccupationList;

            List<SelectListItem> JobTitleCode = DaoService.Instance.GetDao<CodeTreeDao>().SelectListItems<ViewCodeJobTitle>(null, jobtitle_value);
            ViewBag.JobTitleCodeList = JobTitleCode;

            return model;
        }

        #region -- ApplySign 申請憑證簽章 --
        public ActionResult ApplySign()
        {
            FrontOpenApplySignViewModels model = null;

            if (CreditOpenUtil.ApplySignModel != null)
                model = CreditOpenUtil.ApplySignModel;

            ApplySign_Page_Load(model);

            return View(model);
        }

        [HttpPost]
        public ActionResult ApplySign(FrontOpenApplySignViewModels model)
        {
            if (ToValidateApplySign(CreditOpenUtil.ApplySignModel, ModelState))
                return RedirectToAction("CRS");

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

            if (CreditOpenUtil.IndexModel != null)
                pid = CreditOpenUtil.IndexModel.Pid;

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

        #region -- CRS 自我證明表-個人 --
        public ActionResult CRS()
        {
            FrontOpenAgreeViewModels model = null;

            if (CreditOpenUtil.CRSModel != null)
                model = CreditOpenUtil.CRSModel;

            CRS_Page_Load(model);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CRS(FrontOpenAgreeViewModels model)
        {
            if (ModelState.IsValid)
            {
                CreditOpenUtil.CRSModel = model;

                return RedirectToAction("Agree");
            }

            CRS_Page_Load(model);

            TempData["message"] = ModelState.GetErrorJson();

            return View(model);
        }

        private void CRS_Page_Load(FrontOpenAgreeViewModels model)
        {
            String agree_value = String.Empty;

            if (model != null)
                agree_value = model.Agree;

            List<SelectListItem> agree = DaoService.Instance.GetDao<CodeTreeDao>().SelectListItems<ViewCodeYesNo>(null, agree_value);
            ViewBag.AgreeList = agree;

            string resident_address_full = DaoService.Instance.GetDao<TownDao>().GetString(CreditOpenUtil.IndexModel.ResidentTownCode, CreditOpenUtil.IndexModel.ResidentAddress);
            ViewBag.CurrentAddressFull = resident_address_full;

            if (!CreditOpenUtil.IndexModel.SameResident)
            {
                string address_full = DaoService.Instance.GetDao<TownDao>().GetString(CreditOpenUtil.IndexModel.TownCode, CreditOpenUtil.IndexModel.Address);
                ViewBag.AddressFull = address_full;
            }
            else
                ViewBag.AddressFull = "同上";
        }

        #endregion

        #region -- Agree 同意簽署 --
        public ActionResult Agree()
        {
            FrontOpenAgreeViewModels model = null;

            if (CreditOpenUtil.AgreeModel != null)
                model = CreditOpenUtil.AgreeModel;

            Agree_Page_Load(model);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Agree(FrontOpenAgreeViewModels model)
        {
            if (ModelState.IsValid)
            {
                CreditOpenUtil.AgreeModel = model;

                return RedirectToAction("Contract");
            }

            Agree_Page_Load(model);

            TempData["message"] = ModelState.GetErrorJson();

            return View(model);
        }

        private void Agree_Page_Load(FrontOpenAgreeViewModels model)
        {
            String agree_value = String.Empty;

            if (model != null)
                agree_value = model.Agree;

            List<SelectListItem> agree = DaoService.Instance.GetDao<CodeTreeDao>().SelectListItems<ViewCodeAgree>(null, agree_value);
            ViewBag.AgreeList = agree;
        }
        #endregion

        #region -- Contract 契約書 --
        public ActionResult Contract()
        {
            FrontOpenContractViewModels model = null;

            if (CreditOpenUtil.ContractModel != null)
                model = CreditOpenUtil.ContractModel;

            Contract_Page_Load(model);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Contract(FrontOpenContractViewModels model)
        {
            if (ModelState.IsValid)
            {
                CreditOpenUtil.ContractModel = model;
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

            switch ((CompanyTypeEnum)ViewBag.LayoutEnum)
            {
                case CompanyTypeEnum.Dcnf:
                    ViewBag.TemplatePartial = "CreditOpen/_ContractFuturePartial";
                    break;
                case CompanyTypeEnum.Dcn:
                default:
                    ViewBag.TemplatePartial = "CreditOpen/_ContractStockPartial";
                    break;
            }

            List<SelectListItem> agree = DaoService.Instance.GetDao<CodeTreeDao>().SelectListItems<ViewCodeAgree>(null, agree_value);
            ViewBag.AgreeList = agree;
        }
        #endregion

        #region --上傳確認身分檔案--
        public ActionResult IdentityConfirmUpload()
        {
            CreditOpenIdentityConfirmUploadViewModels model = null;

            if (CreditOpenUtil.IdentityUploadConfrimFrontModels != null)
            {
                model = CreditOpenUtil.IdentityUploadConfrimFrontModels;
            }

            IdentityConfirm_Page_Load(model);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult IdentityConfirmUpload(CreditOpenIdentityConfirmUploadViewModels model)
        {
            if (CreditOpenUtil.IdentityUploadConfrimFrontModels != null)
                model = CreditOpenUtil.IdentityUploadConfrimFrontModels;

            if (ToValidateIdentityConfirm(model, ModelState))
            {
                CreditOpenUtil.IdentityUploadConfrimFrontModels = model;
                return RedirectToAction("IdentityUpload");
            }

            IdentityConfirm_Page_Load(model);

            TempData["message"] = ModelState.GetErrorJson();

            return View();
        }

        private void IdentityConfirm_Page_Load(CreditOpenIdentityConfirmUploadViewModels model)
        {
            if (model != null)
            {
                model.IdentityConfirm_Url = model.IdentityConfirm_Url;
            }
        }

        private Boolean ToValidateIdentityConfirm(CreditOpenIdentityConfirmUploadViewModels model, ModelStateDictionary ModelState)
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

                        if (CreditOpenUtil.IdentityUploadFrontModels != null)
                        {
                            virtual_dir_path = CreditOpenUtil.IdentityUploadFrontModels.WebDirPath;
                        } else
                        {
                            if (CreditOpenUtil.IdentityUploadConfrimFrontModels != null)
                                virtual_dir_path = CreditOpenUtil.IdentityUploadConfrimFrontModels.WebDirPath;
                            else
                                virtual_dir_path = String.Format("{0}/{1}/{2}", ConstantUtil.TEMP_UPLOAD_CREDITOLD_PATH, DateTime.Now.ToString("yyyyMMdd"), Guid.NewGuid());
                        }

                        file_name = String.Format("{0}-{1}-{2}", "CreditOldOpen", DateTime.Now.ToString("yyyyMMddHHmm"), Guid.NewGuid());

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
            CreditOpenIdentityConfirmUploadViewModels model = null;

            try
            {
                if (CreditOpenUtil.IdentityUploadConfrimFrontModels != null)
                    model = CreditOpenUtil.IdentityUploadConfrimFrontModels;
                else
                    model = new CreditOpenIdentityConfirmUploadViewModels();

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
                CreditOpenUtil.IdentityUploadConfrimFrontModels = model;
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

            if (CreditOpenUtil.IdentityUploadFrontModels != null)
            {
                model = CreditOpenUtil.IdentityUploadFrontModels;
            }

            Upload_Page_Load(model);

            return View(model);
        }

        private void Upload_Page_Load(FrontOpenIdentityUploadViewModels model)
        {
            if (model != null)
            {
                model.Identity1_Url = model.Identity1_Url;
                model.Identity2_Url = model.Identity2_Url;

                model.RealEstate1_Url = model.RealEstate1_Url;
                model.RealEstate2_Url = model.RealEstate2_Url;
                model.RealEstate3_Url = model.RealEstate3_Url;
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult IdentityUpload(FrontOpenIdentityUploadViewModels model)
        {
            if (CreditOpenUtil.IdentityUploadFrontModels != null)
                model = CreditOpenUtil.IdentityUploadFrontModels;
            //else
            //{
            //    CreditOpenUtil.IdentityUploadFrontModels = new FrontOpenIdentityUploadViewModels();
            //    CreditOpenUtil.IdentityUploadFrontModels.WebDirPath = String.Format("{0}/{1}/{2}", ConstantUtil.TEMP_UPLOAD_CREDITOLD_PATH, DateTime.Now.ToString("yyyyMMdd"), Guid.NewGuid());
            //}

            if (ToValidateUpload(model, ModelState))
            {
                return RedirectToAction("Confirm");
            }

            Upload_Page_Load(model);

            TempData["message"] = ModelState.GetErrorJson();

            return View(model);
        }

        private bool ToValidateUpload(FrontOpenIdentityUploadViewModels model, ModelStateDictionary modelState)
        {
            bool result = false;
            try
            {
                ModelState.Clear();

                if (model != null)
                {
                    switch (CreditOpenUtil.IndexModel.ProofOfProperty)
                    {
                        case "0"://不動產
                        case "3"://其它
                            if (model.RealEstate1 == false)
                            {
                                ModelState.AddModelError("RealEstate1", "請上傳：「財力證明(附件1)(您選擇財力證明：不動產資料.其它)」");
                            }
                            break;
                        case "1"://集保餘額

                            break;
                        case "2"://免財力證明
                        default:
                            if (CreditOpenUtil.IndexModel.MarginTradingShortSelling > 50)
                            {
                                if (model.RealEstate1 == false)
                                {
                                    ModelState.AddModelError("RealEstate1", "請上傳：「財力證明(附件1)」，(免財力證明，但申請信用額度大於50萬)");
                                }
                            }
                            break;
                    }
                }

                if (model.Identity1 == false)
                    ModelState.AddModelError("Identity1", "請上傳：身分證「正面」");

                if (model.Identity2 == false)
                    ModelState.AddModelError("Identity2", "請上傳：身分證「反面」");

                if (ModelState.IsValid)
                {
                    result = true;
                    model.IdentityConfirm1 = CreditOpenUtil.IdentityUploadConfrimFrontModels.IdentityConfirm;
                    model.IdentityConfirm1_Name = CreditOpenUtil.IdentityUploadConfrimFrontModels.IdentityConfirm_Name;
                    model.IdentityConfirm1_Url = CreditOpenUtil.IdentityUploadConfrimFrontModels.IdentityConfirm_Url;
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

                        if (CreditOpenUtil.IdentityUploadConfrimFrontModels != null)
                            virtual_dir_path = CreditOpenUtil.IdentityUploadConfrimFrontModels.WebDirPath;
                        else
                            virtual_dir_path = String.Format("{0}/{1}/{2}", ConstantUtil.TEMP_UPLOAD_FUTURE_PATH, DateTime.Now.ToString("yyyyMMdd"), Guid.NewGuid());

                        file_name = String.Format("{0}-{1}-{2}", "CreditOldOpen", DateTime.Now.ToString("yyyyMMddHHmm"), Guid.NewGuid());

                        file_save = FileUtil.HttpPostedFileSave(file, virtual_dir_path, file_name, FileType.Image);




                        if (file_save.result)
                        {
                            logger.Info(String.Format("doing： {0}.Upload file_id：{1} 、virtual_dir_path：{2}、file_name：{3}", CreditOpenUtil.IndexModel.Name, file_id, virtual_dir_path, file_name));

                            file_save.file_id = file_id;
                            file_save = SetUploadModel(file_save);
                        }

                        //if (CreditOpenUtil.IdentityUploadFrontModels != null)
                        //    virtual_dir_path = CreditOpenUtil.IdentityUploadFrontModels.WebDirPath;
                        //else
                        //{
                        //    CreditOpenUtil.IdentityUploadFrontModels = new FrontOpenIdentityUploadViewModels();
                        //    virtual_dir_path = String.Format("{0}/{1}/{2}", ConstantUtil.TEMP_UPLOAD_CREDITOLD_PATH, DateTime.Now.ToString("yyyyMMdd"), Guid.NewGuid());
                        //    CreditOpenUtil.IdentityUploadFrontModels.WebDirPath = virtual_dir_path;
                        //}

                        //if (!String.IsNullOrEmpty(virtual_dir_path))
                        //{
                        //    file_name = String.Format("{0}-{1}-{2}", "CreditOpen", DateTime.Now.ToString("yyyyMMddHHmm"), Guid.NewGuid());
                        //    file_save = FileUtil.HttpPostedFileSave(file, virtual_dir_path, file_name, FileType.Image);
                        //    if (file_save.result)
                        //    {
                        //        file_save.file_id = file_id;
                        //        UploadOpenUtil.SetSession(CreditOpenUtil.IdentityUploadFrontModels, file_save);
                        //    }
                        //}
                        //else
                        //    file_save = FileUtil.ErrorRestart();
                    }
                };
            }
            logger.Info("Upload test 3");
            return Json(file_save, "text/html", JsonRequestBehavior.AllowGet);
        }

        private FileSaveResult SetUploadModel(FileSaveResult file)
        {
            FileSaveResult result = null;
            FrontOpenIdentityUploadViewModels model = null;

            try
            {
                if (CreditOpenUtil.IdentityUploadFrontModels != null)
                    model = CreditOpenUtil.IdentityUploadFrontModels;
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
                CreditOpenUtil.IdentityUploadFrontModels = model;
            }
            catch (Exception ex)
            {
                result.result = false;
                result.error_message = String.Format("SetUploadModel_StackTrace：{0}、Message：{1}", ex.StackTrace, ex.Message);
            }

            return result;
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

        /// <summary>
        /// (刪除)證件檔案 DeleteUpload
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult UploadDelete(FileSaveResult file)
        {
            if (CreditOpenUtil.IdentityUploadFrontModels != null && file != null && !String.IsNullOrEmpty(file.file_id))
                UploadOpenUtil.DeleteSession(CreditOpenUtil.IdentityUploadFrontModels, file);
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
                    CreditOpenUtil.FileMove(AreaEnum.Front);

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
        /// <returns></returns>
        private Boolean SaveObj()
        {
            Boolean result = false;
            CreditCustomer cases = null;
            ImageUpload upload = null;

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
                            if (upload != null)
                            {
                                upload.CaseId = cases.CreditCustomerId;
                                result = DaoService.Instance.Insert(context, upload);
                                if (!result)
                                {
                                    ModelState.AddModelError("ImageUpload", ErrorTypeEnum.ImageUpload.GetDescription());
                                }
                            }
                        }
                        else
                        {
                            ModelState.AddModelError("Customer", ErrorTypeEnum.Customer.GetDescription());
                        }

                        if (result)
                        {
                            tran.Commit();

                            CreditOpenUtil.CaseModel = new CustomerCaseViewModels()
                            {
                                CustomerCaseId = MemberUtil.Case != null ? MemberUtil.Case.CustomerCaseId : String.Empty,
                                CaseTypeCode = CreditOpenUtil.IndexModel.CaseTypeCode.GetValueString(),
                                CaseTypeEnum = CreditOpenUtil.IndexModel.CaseTypeCode,
                                CustomerId = cases.CustomerId,
                                CaseId = cases != null ? cases.CreditCustomerId : (long?)null,
                                ImageUploadId = upload != null ? upload.ImageUploadId : (long?)null,
                                CaseCreateDate = upload != null ? upload.CreateDate : DateTime.Now,
                                CreateUser = CreditOpenUtil.IndexModel.Pid
                            };

                            CreditOpenUtil.CaseModel.CaseIdSpilt.Add(CaseTypeEnum.CreditOpen.GetValueString(), cases.CreditCustomerId);
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
        /// <returns></returns>
        private CreditCustomer SetCase()
        {
            CreditCustomer result = null;
            try
            {
                CreditOpenViewModels IndexModel = CreditOpenUtil.IndexModel;
                FrontOpenApplySignViewModels ApplySignModel = CreditOpenUtil.ApplySignModel;

                CreditCustomer cases = new CreditCustomer();

                Customer customer = new Customer();
                CustomerAccount cases_account = new CustomerAccount();
                CustomerCertification certification = new CustomerCertification();

                customer.CaseTypeCode = CreditOpenUtil.IndexModel.CaseTypeCode.GetValueString();
                customer.Pid = IndexModel.Pid.ToUpper();
                customer.Name = IndexModel.Name;
                customer.ContactPhone = IndexModel.ContactPhone;
                customer.MobilePhone = IndexModel.MobilePhone;

                customer.ResidentCityCode = IndexModel.ResidentCityCode;
                customer.ResidentTownCode = IndexModel.ResidentTownCode;
                customer.ResidentAddress = IndexModel.ResidentAddress;

                customer.CityCode = IndexModel.CityCode;
                customer.TownCode = IndexModel.TownCode;
                customer.Address = IndexModel.Address;

                customer.OccupationCode = IndexModel.OccupationCode;
                customer.JobTitleCode = IndexModel.JobTitleCode;
                customer.CompanyName = IndexModel.CompanyName;
                customer.CompanyTelePhone = IndexModel.CompanyTelePhone;
                customer.CompanyCity = IndexModel.CompanyCityCode;
                customer.CompanyTown = IndexModel.CompanyTownCode;
                customer.CompanyAddress = IndexModel.CompanyTownCode;
                customer.CompanyAddress = IndexModel.CompanyAddress;
                customer.Email = UserUtil.Users.Email;

                customer.Deleted = false;
                customer.CreateDate = DateTime.Now;
                customer.CreateUser = "User";

                cases_account.AccountId = IndexModel.CustomerAccount;

                cases.BranchCode = IndexModel.BranchCode;
                cases.OpenTypeCode = IndexModel.OpenTypeCode;
                cases.ExtensionTimeCode = IndexModel.CheckExtensionTime();
                cases.MarginTradingShortSelling = IndexModel.MarginTradingShortSelling;
                cases.ProofOfProperty = IndexModel.ProofOfProperty;

                cases.UserType = DcnSsoUtil.UserType;
                cases.SendToMail = "0";
                cases.StatusCode = "0";//新建

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
                customer.CustomerAccount.Add(cases_account);
                cases.Customer.CustomerCertification = certification;

                result = cases;
            }
            catch (Exception ex)
            {
                result = null;

                logger.Info(ex.Message);
                logger.Info(ex.StackTrace);
            }

            return result;
        }

        /// <summary>
        /// 存檔：2.證件設定
        /// </summary>
        /// <returns></returns>
        private ImageUpload SetUpload()
        {
            ImageUpload result = new ImageUpload();
            FrontOpenIdentityUploadViewModels model = null;
            CreditOpenIdentityConfirmUploadViewModels identity_confirm_model = null;
            try
            {
                model = CreditOpenUtil.IdentityUploadFrontModels;
                identity_confirm_model = CreditOpenUtil.IdentityUploadConfrimFrontModels;

                if (model != null && identity_confirm_model != null)
                {
                    result.CaseTypeCode = CreditOpenUtil.IndexModel.CaseTypeCode.GetValueString();

                    result.IdentityConfirmName1 = identity_confirm_model.IdentityConfirm_Name;

                    if (model.Identity1)
                        result.IdentityName1 = model.Identity1_Name;

                    if (model.Identity2)
                        result.IdentityName2 = model.Identity2_Name;

                    if (model.RealEstate1)
                        result.RealEstateName1 = model.RealEstate1_Name;

                    if (model.RealEstate2)
                        result.RealEstateName2 = model.RealEstate2_Name;
                    else
                        result.RealEstateName2 = null;

                    if (model.RealEstate3)
                        result.RealEstateName3 = model.RealEstate3_Name;
                    else
                        result.RealEstateName3 = null;

                    result.CreateUser = UserUtil.Users.UserId;
                    result.CreateDate = DateTime.Now;
                }
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
