using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Dcn.SqlClient;
using Dcn.SqlClient.ValidateAttribute;
using Newtonsoft.Json;
using Dcn.Util;
using DcnWeb.Filters;
using DcnWeb.Filters.Front.BorrowLendAnyUseOpen;
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
    public class BorrowLendAnyUseOpenController : BaseController
    {
        private readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private List<String> message = new List<String>();

        #region -- Index 客戶資料 --
        [DcnSsoLoginFilter]
        public ActionResult Index()
        {
            BorrowLendAnyUseOpenViewModels model = null;

            if (BorrowLendAnyUseOpenUtil.IndexModel != null)
                model = BorrowLendAnyUseOpenUtil.IndexModel;
            

            //修改這邊

            DdscSDefaultInfo(model);

            model = Index_Page_Load(model);

            return View(model);
        }

        public void DdscSDefaultInfo(BorrowLendAnyUseOpenViewModels model)
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
                    model.ResidentAddress = fun703d.item.ret.cust_data.naddr;
                    model.Address = fun703d.item.ret.cust_data.iaddr;
                }
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(BorrowLendAnyUseOpenViewModels model)
        {
            if (ToValidateIndex(model, ModelState))
            {
                BorrowLendAnyUseOpenUtil.IndexModel = model;
                BorrowLendAnyUseOpenUtil.IndexModel.CaseTypeCode = CaseTypeEnum.BorrowLendAnyUseOpen;

                return RedirectToAction("Investigate");
            }

            Index_Page_Load(model);

            TempData["message"] = ModelState.GetErrorJson();

            return View(model);
        }

        private Boolean ToValidateIndex(BorrowLendAnyUseOpenViewModels model, ModelStateDictionary ModelState)
        {
            Boolean result = true;
            try
            {
                if (ModelState.IsValid)
                {
                    ModelState.Clear();

                    if (OpenUtil.ToValidateOccupation(model.OccupationCode, ModelState))
                    {
                        model.CompanyName = String.Empty;
                        model.CompanyTelePhone = String.Empty;
                        model.CompanyAddress = String.Empty;
                    }

                    Boolean exist_case = DaoService.Instance.GetDao<BorrowLendAnyUseOpenDao>().GetExistPidInCase(model.Pid.ToUpper(), new List<String>(new String[] { "0", "8" }));
                    if (exist_case)
                        ModelState.AddModelError("Pid", "資料送出失敗，您目前有不限用途款項借貸案件正在辦理中，尚未結案!");
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
        private BorrowLendAnyUseOpenViewModels Index_Page_Load(BorrowLendAnyUseOpenViewModels model)
        {
            String open_type_code_value = String.Empty;
            String branch_value = String.Empty;
            String stock_account_value = String.Empty;
            String name = String.Empty;
            bool sameresident_value = false;
            String occupation_value = String.Empty;
            String jobtitle_value = String.Empty;
            String bankstatementsend_code_value = String.Empty;
            String extension_time_code_value = String.Empty;

            String city_value = String.Empty;
            String town_value = String.Empty;
            String resident_city_value = String.Empty;
            String resident_town_value = String.Empty;

            String company_city_value = String.Empty;
            String company_town_value = String.Empty;

            if (model != null)
            {
                open_type_code_value = model.OpenTypeCode;
                branch_value = model.BranchCode;
                stock_account_value = model.CustomerAccount;
                name = model.Name;
                bankstatementsend_code_value = model.BankStatementSendCode;
                sameresident_value = model.SameResident;
                occupation_value = model.OccupationCode;
                jobtitle_value = model.JobTitleCode;
                extension_time_code_value = model.ExtensionTimeCode;

                if (model.SameResident)
                {
                    model.CityCode = model.ResidentCityCode;
                    model.TownCode = model.ResidentTownCode;
                    model.Address = model.ResidentAddress;
                }

                resident_city_value = model.ResidentCityCode;
                resident_town_value = model.ResidentTownCode;
                city_value = model.CityCode;
                town_value = model.TownCode;

                company_city_value = model.CompanyCityCode;
                company_town_value = model.CompanyTownCode;
            }
            else
            {
                if (!String.IsNullOrEmpty(DcnSsoUtil.OpenType))
                {
                    open_type_code_value = DcnSsoUtil.OpenType;
                }
            }

            List<SelectListItem> OpenTypeCode = DaoService.Instance.GetDao<CodeTreeDao>().SelectListItems<ViewCodeBorrowLendAnyUseOpenTypeCode>(null, open_type_code_value);
            ViewBag.OpenTypeCodeList = OpenTypeCode;

            List<SelectListItem> company_city = DaoService.Instance.GetDao<CodeTreeDao>().SelectListItems
                <City>("CityName", "CityCode", "請選擇縣市", company_city_value, "Sort");
            ViewBag.CompanyCityList = company_city;

            List<SelectListItem> company_town = DaoService.Instance.GetDao<TownDao>().SelectListItemList(company_city_value, company_town_value);
            ViewBag.CompanyTownList = company_town;

            List<SelectListItem> branch = DaoService.Instance.GetDao<CodeTreeDao>().SelectListItems<ViewCodeBranchStock>(null, branch_value);
            ViewBag.BranchStockList = branch;

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

            List<SelectListItem> BankStatementSendCode = DaoService.Instance.GetDao<CodeTreeDao>().SelectListItems<ViewCodeBankStatementSendCode>(null, bankstatementsend_code_value);
            ViewBag.BankStatementSendCodeList = BankStatementSendCode;

            List<SelectListItem> OccupationList = DaoService.Instance.GetDao<CodeTreeDao>().SelectListItems<ViewCodeOccupation>(null, occupation_value);
            ViewBag.OccupationCodeList = OccupationList;

            List<SelectListItem> JobTitleCode = DaoService.Instance.GetDao<CodeTreeDao>().SelectListItems<ViewCodeJobTitle>(null, jobtitle_value);
            ViewBag.JobTitleCodeList = JobTitleCode;

            List<SelectListItem> ExtensionTimeCodeList = DaoService.Instance.GetDao<CodeTreeDao>().SelectListItems<ViewCodeExtensionTimeCode>(null, extension_time_code_value);
            ViewBag.ExtensionTimeCodeList = ExtensionTimeCodeList;

            ViewBag.SameResident = sameresident_value;

            return model;
        }

        #endregion

        #region -- BorrowLendAnyUseInvestigate 徵信 --
        public ActionResult Investigate()
        {
            BorrowLendAnyUseInvestigateViewModels model = null;

            if (BorrowLendAnyUseOpenUtil.InvestigateModel != null)
                model = BorrowLendAnyUseOpenUtil.InvestigateModel;

            Investigate_Page_Load(model);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Investigate(BorrowLendAnyUseInvestigateViewModels model)
        {
            if (ToValidateInvestigate(model, ModelState))
            {
                BorrowLendAnyUseOpenUtil.InvestigateModel = model;

                return RedirectToAction("ApplySign");
            }

            Investigate_Page_Load(model);

            TempData["message"] = ModelState.GetErrorJson();

            return View(model);
        }

        private Boolean ToValidateInvestigate(BorrowLendAnyUseInvestigateViewModels model, ModelStateDictionary ModelState)
        {
            Boolean result = true;
            try
            {
                if (ModelState.IsValid)
                {
                    ModelState.Clear();

                    if (model.OtherStockAccount.Equals("1"))
                    {
                        if (model.OpenAccountCount == 0 || model.OpenAccountCount == null)
                            ModelState.AddModelError("OpenAccountCount", "請輸入「開戶家數」");

                        if (String.IsNullOrEmpty(model.OtherStockAccountName))
                            ModelState.AddModelError("OtherStockAccountName", "請輸入「往來證券商名稱」");
                    }

                    if (model.TheOpenRealEstate.Equals("3"))
                    {
                        if (String.IsNullOrEmpty(model.PriceSetting))
                            ModelState.AddModelError("PriceSetting", "請輸入「設定金額」");
                    }

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

        private void Investigate_Page_Load(BorrowLendAnyUseInvestigateViewModels model)
        {
            String OpenAccountForThreeMonth_value = String.Empty;
            String HaveRefundRecord_value = String.Empty;
            String BreachOfContract_value = String.Empty;
            String OtherStockAccount_value = String.Empty;
            int? OpenAccountCount_value;
            String OtherStockAccountName_value = String.Empty;
            String PersonalInComeYear_value = String.Empty;
            String TotalAsset_value = String.Empty;
            String InvestmentExperience_value = String.Empty;
            String InvestmentDuration_value = String.Empty;
            String TradingFrequency_value = String.Empty;
            String MovableAndRealEstate_value = String.Empty;
            String TheOpenRealEstate_value = String.Empty;
            String PriceSetting_value = String.Empty;
            String ProofOfProperty_value = String.Empty;

            if (model != null)
            {
                OpenAccountForThreeMonth_value = model.OpenAccountForThreeMonth;
                HaveRefundRecord_value = model.HaveRefundRecord;
                BreachOfContract_value = model.BreachOfContract;

                OtherStockAccount_value = model.OtherStockAccount;

                OpenAccountCount_value = model.OpenAccountCount;
                OtherStockAccountName_value = model.OtherStockAccountName;

                PersonalInComeYear_value = model.PersonalInComeYear;
                TotalAsset_value = model.TotalAsset;
                InvestmentExperience_value = model.InvestmentExperience;
                InvestmentDuration_value = model.InvestmentDuration;
                TradingFrequency_value = model.TradingFrequency;
                MovableAndRealEstate_value = model.MovableAndRealEstate;
                TheOpenRealEstate_value = model.TheOpenRealEstate;

                PriceSetting_value = model.PriceSetting;

                ProofOfProperty_value = model.ProofOfProperty;
            }

            List<SelectListItem> OpenAccountForThreeMonth = DaoService.Instance.GetDao<CodeTreeDao>().SelectListItems<ViewCodeYesNo>(null, OpenAccountForThreeMonth_value);
            ViewBag.OpenAccountForThreeMonthList = OpenAccountForThreeMonth;

            List<SelectListItem> RefundRecord = DaoService.Instance.GetDao<CodeTreeDao>().SelectListItems<ViewCodeYesNo>(null, HaveRefundRecord_value, "CodeValue");
            ViewBag.RefundRecordList = RefundRecord;

            List<SelectListItem> BreachOfContract = DaoService.Instance.GetDao<CodeTreeDao>().SelectListItems<ViewCodeYesNo>(null, BreachOfContract_value);
            ViewBag.BreachOfContractList = BreachOfContract;

            List<SelectListItem> OtherStockAccount = DaoService.Instance.GetDao<CodeTreeDao>().SelectListItems<ViewCodeHaveEmpty>(null, OtherStockAccount_value);
            ViewBag.OtherStockAccountList = OtherStockAccount;

            List<SelectListItem> PersonalInComeYear = DaoService.Instance.GetDao<CodeTreeDao>().SelectListItems<ViewCodeInComeYear>(null, PersonalInComeYear_value);
            ViewBag.PersonalInComeYearList = PersonalInComeYear;

            List<SelectListItem> TotalAsset = DaoService.Instance.GetDao<CodeTreeDao>().SelectListItems<ViewCodeBorrowLendAnyUseInvestigateTotalAssetsValue>(null, TotalAsset_value);
            ViewBag.TotalAssetList = TotalAsset;

            List<SelectListItem> InvestmentExperience = DaoService.Instance.GetDao<CodeTreeDao>().SelectListItems<ViewCodeBorrowLendAnyUseInvestigateInvestmentExperience>(null, InvestmentExperience_value);
            ViewBag.InvestmentExperienceList = InvestmentExperience;

            List<SelectListItem> InvestmentDuration = DaoService.Instance.GetDao<CodeTreeDao>().SelectListItems<ViewCodeBorrowLendAnyUseInvestigateInvestmentDuration>(null, InvestmentDuration_value);
            ViewBag.InvestmentDurationList = InvestmentDuration;

            List<SelectListItem> TradingFrequency = DaoService.Instance.GetDao<CodeTreeDao>().SelectListItems<ViewCodeBorrowLendAnyUseInvestigateTradingFrequency>(null, TradingFrequency_value);
            ViewBag.TradingFrequencyList = TradingFrequency;

            List<SelectListItem> MovableAndRealEstate = DaoService.Instance.GetDao<CodeTreeDao>().SelectListItems<ViewCodeBorrowLendAnyUseInvestigateMovableAndRealEstate>(null, MovableAndRealEstate_value);
            ViewBag.MovableAndRealEstateList = MovableAndRealEstate;

            List<SelectListItem> TheOpenRealEstate = DaoService.Instance.GetDao<CodeTreeDao>().SelectListItems<ViewCodeBorrowLendAnyUseInvestigateTheOpenRealEstate>(null, TheOpenRealEstate_value);
            ViewBag.TheOpenRealEstateList = TheOpenRealEstate;

            List<SelectListItem> ProofOfProperty = DaoService.Instance.GetDao<CodeTreeDao>().SelectListItems<ViewCodeBorrowLendAnyUseInvestigateProofOfProperty>(null, ProofOfProperty_value);
            ViewBag.ProofOfPropertyList = ProofOfProperty;
        }
        #endregion

        #region -- BorrowLendAnyUseRelation 關係人資料表(2019/04 拿掉流程) --
        //public ActionResult Relation()
        //{
        //    BorrowLendAnyUseRelationViewModels model = null;

        //    if (BorrowLendAnyUseOpenUtil.RelationModel != null)
        //        model = BorrowLendAnyUseOpenUtil.RelationModel;

        //    Relation_Page_Load(model);

        //    return View(model);
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Relation(BorrowLendAnyUseRelationViewModels model)
        //{
        //    if (ToValidateRelation(model, ModelState))
        //    {
        //        BorrowLendAnyUseOpenUtil.RelationModel = model;
        //        return RedirectToAction("ApplySign");
        //    }

        //    Relation_Page_Load(model);

        //    TempData["message"] = ModelState.GetErrorJson();

        //    return View(model);
        //}
        //private Boolean ToValidateRelation(BorrowLendAnyUseRelationViewModels model, ModelStateDictionary ModelState)
        //{
        //    Boolean result = true;
        //    try
        //    {
        //        if (ModelState.IsValid)
        //        {
        //            ModelState.Clear();

        //            if (!String.IsNullOrEmpty(model.SpouseRelationName1) ||
        //                !String.IsNullOrEmpty(model.SpouseRelation1) ||
        //                !String.IsNullOrEmpty(model.SpouseRelationPid1))
        //            {
        //                if (String.IsNullOrEmpty(model.SpouseRelationName1))
        //                    ModelState.AddModelError("SpouseRelationName1", "請輸入配偶姓名");
        //                if (String.IsNullOrEmpty(model.SpouseRelation1))
        //                    ModelState.AddModelError("SpouseRelation1", "請輸入配偶稱謂");
        //                if (String.IsNullOrEmpty(model.SpouseRelationPid1))
        //                    ModelState.AddModelError("SpouseRelationPid1", "請輸入配偶身分證");
        //            }

        //            if (!String.IsNullOrEmpty(model.RelativeRelationName1) ||
        //                !String.IsNullOrEmpty(model.RelativeRelation1) ||
        //                !String.IsNullOrEmpty(model.RelativeRelationPid1))
        //            {
        //                if (String.IsNullOrEmpty(model.RelativeRelationName1))
        //                    ModelState.AddModelError("RelativeRelationName1", "請輸入二等親姓名");
        //                if (String.IsNullOrEmpty(model.RelativeRelation1))
        //                    ModelState.AddModelError("RelativeRelation1", "請輸入二等親稱謂");
        //                if (String.IsNullOrEmpty(model.RelativeRelationPid1))
        //                    ModelState.AddModelError("RelativeRelationPid1", "請輸入二等親身分證");
        //            }
        //            if (!String.IsNullOrEmpty(model.RelativeRelationName2) ||
        //                !String.IsNullOrEmpty(model.RelativeRelation2) ||
        //                !String.IsNullOrEmpty(model.RelativeRelationPid2))
        //            {
        //                if (String.IsNullOrEmpty(model.RelativeRelationName2))
        //                    ModelState.AddModelError("RelativeRelationName2", "請輸入二等親姓名");
        //                if (String.IsNullOrEmpty(model.RelativeRelation2))
        //                    ModelState.AddModelError("RelativeRelation2", "請輸入二等親稱謂");
        //                if (String.IsNullOrEmpty(model.RelativeRelationPid2))
        //                    ModelState.AddModelError("RelativeRelationPid2", "請輸入二等親身分證");
        //            }
        //            if (!String.IsNullOrEmpty(model.RelativeRelationName3) ||
        //                !String.IsNullOrEmpty(model.RelativeRelation3) ||
        //                !String.IsNullOrEmpty(model.RelativeRelationPid3))
        //            {
        //                if (String.IsNullOrEmpty(model.RelativeRelationName3))
        //                    ModelState.AddModelError("RelativeRelationName3", "請輸入二等親姓名");
        //                if (String.IsNullOrEmpty(model.RelativeRelation3))
        //                    ModelState.AddModelError("RelativeRelation3", "請輸入二等親稱謂");
        //                if (String.IsNullOrEmpty(model.RelativeRelationPid3))
        //                    ModelState.AddModelError("RelativeRelationPid3", "請輸入二等親身分證");
        //            }

        //            if (!String.IsNullOrEmpty(model.RelativeRelationName4) ||
        //                !String.IsNullOrEmpty(model.RelativeRelation4) ||
        //                !String.IsNullOrEmpty(model.RelativeRelationPid4))
        //            {
        //                if (String.IsNullOrEmpty(model.RelativeRelationName4))
        //                    ModelState.AddModelError("RelativeRelationName4", "請輸入二等親姓名");
        //                if (String.IsNullOrEmpty(model.RelativeRelation4))
        //                    ModelState.AddModelError("RelativeRelation4", "請輸入二等親稱謂");
        //                if (String.IsNullOrEmpty(model.RelativeRelationPid4))
        //                    ModelState.AddModelError("RelativeRelationPid4", "請輸入二等親身分證");
        //            }

        //            if (!String.IsNullOrEmpty(model.CompanyName1) ||
        //                !String.IsNullOrEmpty(model.UniformNumber1) ||
        //                !String.IsNullOrEmpty(model.Job1))
        //            {
        //                if (String.IsNullOrEmpty(model.CompanyName1))
        //                    ModelState.AddModelError("CompanyName1", "請輸入本人企業名稱");
        //                if (String.IsNullOrEmpty(model.UniformNumber1))
        //                    ModelState.AddModelError("UniformNumber1", "請輸入本人統一編號");
        //                if (String.IsNullOrEmpty(model.Job1))
        //                    ModelState.AddModelError("Job1", "請選擇本人職務");
        //            }

        //            if (!String.IsNullOrEmpty(model.CompanyName2) ||
        //                !String.IsNullOrEmpty(model.UniformNumber2) ||
        //                !String.IsNullOrEmpty(model.Job2))
        //            {
        //                if (String.IsNullOrEmpty(model.CompanyName2))
        //                    ModelState.AddModelError("CompanyName2", "請輸入本人企業名稱");
        //                if (String.IsNullOrEmpty(model.UniformNumber2))
        //                    ModelState.AddModelError("UniformNumber2", "請輸入本人統一編號");
        //                if (String.IsNullOrEmpty(model.Job2))
        //                    ModelState.AddModelError("Job2", "請選擇職務");
        //            }

        //            if (!String.IsNullOrEmpty(model.SpouseCompanyName1) ||
        //                !String.IsNullOrEmpty(model.SpouseUniformNumber1) ||
        //                !String.IsNullOrEmpty(model.SpouseJob1))
        //            {
        //                if (String.IsNullOrEmpty(model.SpouseCompanyName1))
        //                    ModelState.AddModelError("SpouseCompanyName1", "請輸入配偶企業名稱");
        //                if (String.IsNullOrEmpty(model.SpouseUniformNumber1))
        //                    ModelState.AddModelError("SpouseUniformNumber1", "請輸入配偶統一編號");
        //                if (String.IsNullOrEmpty(model.SpouseJob1))
        //                    ModelState.AddModelError("SpouseJob1", "請選擇配偶職務");
        //            }

        //            if (!String.IsNullOrEmpty(model.SpouseCompanyName2) ||
        //                !String.IsNullOrEmpty(model.SpouseUniformNumber2) ||
        //                !String.IsNullOrEmpty(model.SpouseJob2))
        //            {
        //                if (String.IsNullOrEmpty(model.SpouseCompanyName2))
        //                    ModelState.AddModelError("SpouseCompanyName2", "請輸入配偶企業名稱");
        //                if (String.IsNullOrEmpty(model.SpouseUniformNumber2))
        //                    ModelState.AddModelError("SpouseUniformNumber2", "請輸入配偶統一編號");
        //                if (String.IsNullOrEmpty(model.SpouseJob2))
        //                    ModelState.AddModelError("SpouseJob2", "請選擇配偶職務");
        //            }
        //        }

        //        if (ModelState.IsValid)
        //            result = true;
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Info(ex.Message);
        //        logger.Info(ex.StackTrace);
        //    }
        //    return result;
        //}

        //private void Relation_Page_Load(BorrowLendAnyUseRelationViewModels model)
        //{
        //    String Job1_value = String.Empty;
        //    String Job2_value = String.Empty;
        //    String SpouseJob1_value = String.Empty;
        //    String SpouseJob2_value = String.Empty;

        //    if (model != null)
        //    {
        //        Job1_value = model.Job1;
        //        Job2_value = model.Job2;
        //        SpouseJob1_value = model.SpouseJob1;
        //        SpouseJob2_value = model.SpouseJob2;
        //    }

        //    List<SelectListItem> Job1 = DaoService.Instance.GetDao<CodeTreeDao>().SelectListItems<ViewCodeJobTitle>(Job1_value);
        //    ViewBag.Job1List = Job1;

        //    List<SelectListItem> Job2 = DaoService.Instance.GetDao<CodeTreeDao>().SelectListItems<ViewCodeJobTitle>(Job2_value);
        //    ViewBag.Job2List = Job2;

        //    List<SelectListItem> SpouseJob1 = DaoService.Instance.GetDao<CodeTreeDao>().SelectListItems<ViewCodeJobTitle>(SpouseJob1_value);
        //    ViewBag.SpouseJob1List = SpouseJob1;

        //    List<SelectListItem> SpouseJob2 = DaoService.Instance.GetDao<CodeTreeDao>().SelectListItems<ViewCodeJobTitle>(SpouseJob2_value);
        //    ViewBag.SpouseJob2List = SpouseJob2;

        //}
        #endregion

        #region --申請憑證簽章--
        public ActionResult ApplySign()
        {
            FrontOpenApplySignViewModels model = null;

            if (BorrowLendAnyUseOpenUtil.ApplySignModel != null)
                model = BorrowLendAnyUseOpenUtil.ApplySignModel;

            ApplySign_Page_Load(model);

            return View(model);
        }

        [HttpPost]
        public ActionResult ApplySign(FrontOpenApplySignViewModels model)
        {
            if (ToValidateApplySign(BorrowLendAnyUseOpenUtil.ApplySignModel, ModelState))
                return RedirectToAction("Agree");

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

            if (BorrowLendAnyUseOpenUtil.IndexModel != null)
                pid = BorrowLendAnyUseOpenUtil.IndexModel.Pid;

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

        #region -- BorrowLendAnyUseAgree 同意書 --
        public ActionResult Agree()
        {
            FrontOpenAgreeViewModels model = null;

            if (BorrowLendAnyUseOpenUtil.AgreeModel != null)
                model = BorrowLendAnyUseOpenUtil.AgreeModel;

            Agree_Page_Load(model);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Agree(FrontOpenAgreeViewModels model)
        {
            if (ModelState.IsValid)
            {
                BorrowLendAnyUseOpenUtil.AgreeModel = model;

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

        #region -- BorrowLendAnyUseContract 契約書 --
        public ActionResult Contract()
        {
            FrontOpenContractViewModels model = null;

            if (BorrowLendAnyUseOpenUtil.ContractModel != null)
                model = BorrowLendAnyUseOpenUtil.ContractModel;

            Contract_Page_Load(model);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Contract(FrontOpenContractViewModels model)
        {
            if (ModelState.IsValid)
            {
                BorrowLendAnyUseOpenUtil.ContractModel = model;

                return RedirectToAction("ContractDeadline");
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

        #region 契約(第五條-融通期限)
        public ActionResult ContractDeadline()
        {
            FrontOpenContractViewModels model = null;

            if (BorrowLendAnyUseOpenUtil.ContractDeadlineModel != null)
                model = BorrowLendAnyUseOpenUtil.ContractDeadlineModel;

            ContractDeadline_Page_Load(model);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ContractDeadline(FrontOpenContractViewModels model)
        {
            if (ModelState.IsValid)
            {
                BorrowLendAnyUseOpenUtil.ContractDeadlineModel = model;

                return RedirectToAction("IdentityConfirmUpload");
            }

            ContractDeadline_Page_Load(model);

            TempData["message"] = ModelState.GetErrorJson();

            return View(model);
        }

        private void ContractDeadline_Page_Load(FrontOpenContractViewModels model)
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
            BorrowLendAnyUseOpenIdentityConfirmUploadViewModels model = null;

            if (BorrowLendAnyUseOpenUtil.IdentityUploadConfrimFrontModels != null)
            {
                model = BorrowLendAnyUseOpenUtil.IdentityUploadConfrimFrontModels;
            }

            IdentityConfirm_Page_Load(model);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult IdentityConfirmUpload(BorrowLendAnyUseOpenIdentityConfirmUploadViewModels model)
        {
            if (BorrowLendAnyUseOpenUtil.IdentityUploadConfrimFrontModels != null)
                model = BorrowLendAnyUseOpenUtil.IdentityUploadConfrimFrontModels;

            if (ToValidateIdentityConfirm(model, ModelState))
            {
                BorrowLendAnyUseOpenUtil.IdentityUploadConfrimFrontModels = model;
                return RedirectToAction("IdentityUpload");
            }

            IdentityConfirm_Page_Load(model);

            TempData["message"] = ModelState.GetErrorJson();

            return View();
        }

        private void IdentityConfirm_Page_Load(BorrowLendAnyUseOpenIdentityConfirmUploadViewModels model)
        {
            if (model != null)
            {
                model.IdentityConfirm_Url = model.IdentityConfirm_Url;
            }
        }

        private Boolean ToValidateIdentityConfirm(BorrowLendAnyUseOpenIdentityConfirmUploadViewModels model, ModelStateDictionary ModelState)
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

                        if (BorrowLendAnyUseOpenUtil.IdentityUploadFrontModels != null)
                        {
                            virtual_dir_path = BorrowLendAnyUseOpenUtil.IdentityUploadFrontModels.WebDirPath;
                        }
                        else
                        {
                            if (BorrowLendAnyUseOpenUtil.IdentityUploadConfrimFrontModels != null)
                                virtual_dir_path = BorrowLendAnyUseOpenUtil.IdentityUploadConfrimFrontModels.WebDirPath;
                            else
                                virtual_dir_path = String.Format("{0}/{1}/{2}", ConstantUtil.TEMP_UPLOAD_BORROWLENDANYUSE_PATH, DateTime.Now.ToString("yyyyMMdd"), Guid.NewGuid());
                        }

                        file_name = String.Format("{0}-{1}-{2}", "BorrowLendAnyUseOpen", DateTime.Now.ToString("yyyyMMddHHmm"), Guid.NewGuid());

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
            BorrowLendAnyUseOpenIdentityConfirmUploadViewModels model = null;

            try
            {
                if (BorrowLendAnyUseOpenUtil.IdentityUploadConfrimFrontModels != null)
                    model = BorrowLendAnyUseOpenUtil.IdentityUploadConfrimFrontModels;
                else
                    model = new BorrowLendAnyUseOpenIdentityConfirmUploadViewModels();

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
                BorrowLendAnyUseOpenUtil.IdentityUploadConfrimFrontModels = model;
            }
            catch (Exception ex)
            {
                result.result = false;
                result.error_message = String.Format("SetUploadModel_StackTrace：{0}、Message：{1}", ex.StackTrace, ex.Message);
            }

            return result;
        }
        #endregion

        #region -- BorrowLendAnyUseIdentityUpload 上傳檔案 --
        public ActionResult IdentityUpload()
        {
            FrontOpenIdentityUploadViewModels model = null;

            if (BorrowLendAnyUseOpenUtil.IdentityUploadFrontModels != null)
            {
                model = BorrowLendAnyUseOpenUtil.IdentityUploadFrontModels;
            }

            Upload_Page_Load(model);

            return View(model);
        }

        [HttpPost]
        public ActionResult IdentityUpload(FrontOpenIdentityUploadViewModels model)
        {
            if (BorrowLendAnyUseOpenUtil.IdentityUploadFrontModels != null)
                model = BorrowLendAnyUseOpenUtil.IdentityUploadFrontModels;

            if (ToValidateUpload(model, ModelState))
            {
                return RedirectToAction("Confirm");
            }

            Upload_Page_Load(model);

            TempData["message"] = ModelState.GetErrorJson();

            return View(model);
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

                String ProofOfProperty = BorrowLendAnyUseOpenUtil.InvestigateModel.ProofOfProperty as String;
                if (ProofOfProperty.Equals("2"))
                {
                    if (model.Identity1_Agent == false)
                        ModelState.AddModelError("Identity1_Agent", "請上傳：代理人身份證「正面」");

                    if (model.Identity2_Agent == false)
                        ModelState.AddModelError("Identity2_Agent", "請上傳：代理人身份證「反面」");
                }

                if (model.RealEstate1 == false)
                {
                    String MovableAndRealEstate = BorrowLendAnyUseOpenUtil.InvestigateModel.MovableAndRealEstate as String;
                    if (MovableAndRealEstate.Equals("1") || MovableAndRealEstate.Equals("2"))
                        ModelState.AddModelError("RealEstate1", "請上傳：「財力證明-限不動產權狀或當年度稅單」");
                }

                if (model.Bank1 == false)
                {
                    ModelState.AddModelError("Bank1", "請上傳：「銀行存褶封面 (需與證券交割帳號相符)」");
                }

                if (ModelState.IsValid)
                {
                    result = true;

                    model.IdentityConfirm1 = BorrowLendAnyUseOpenUtil.IdentityUploadConfrimFrontModels.IdentityConfirm;
                    model.IdentityConfirm1_Name = BorrowLendAnyUseOpenUtil.IdentityUploadConfrimFrontModels.IdentityConfirm_Name;
                    model.IdentityConfirm1_Url = BorrowLendAnyUseOpenUtil.IdentityUploadConfrimFrontModels.IdentityConfirm_Url;
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

            if (BorrowLendAnyUseOpenUtil.IdentityUploadFrontModels != null)
            {
                model.Identity1_Agent_Url = BorrowLendAnyUseOpenUtil.IdentityUploadFrontModels.Identity1_Agent_Url;
                model.Identity2_Agent_Url = BorrowLendAnyUseOpenUtil.IdentityUploadFrontModels.Identity2_Agent_Url;

                model.RealEstate1_Url = BorrowLendAnyUseOpenUtil.IdentityUploadFrontModels.RealEstate1_Url;
                model.RealEstate2_Url = BorrowLendAnyUseOpenUtil.IdentityUploadFrontModels.RealEstate2_Url;
                model.RealEstate3_Url = BorrowLendAnyUseOpenUtil.IdentityUploadFrontModels.RealEstate3_Url;

                model.Bank1_Url = BorrowLendAnyUseOpenUtil.IdentityUploadFrontModels.Bank1_Url;
            }

            //財產證明 選項：1.本人、2.非本人　
            //說明：2.非本人，必需上傳代理人身份證圖檔
            String ProofOfProperty = String.Empty;
            if (BorrowLendAnyUseOpenUtil.InvestigateModel != null)
                ProofOfProperty = BorrowLendAnyUseOpenUtil.InvestigateModel.ProofOfProperty as String;

            switch (ProofOfProperty)
            {
                case "1":
                    ViewBag.ProofOfProperty = false;
                    break;
                case "2":
                    ViewBag.ProofOfProperty = true;
                    break;
                default:
                    ViewBag.ProofOfProperty = false;
                    break;
            }

            //動產、不動產 選項：1.土地、2.建物　
            //說明：需上傳（財力證明-限不動產權狀或當年度稅單)
            String RealEstate = String.Empty;
            if (BorrowLendAnyUseOpenUtil.InvestigateModel != null)
                RealEstate = BorrowLendAnyUseOpenUtil.InvestigateModel.MovableAndRealEstate as String;

            switch (RealEstate)
            {
                case "1":
                case "2":
                    ViewBag.RealEstate = true;
                    break;
                default:
                    ViewBag.RealEstate = false;
                    break;
            }
        }

        [HttpPost]
        public JsonResult Upload()
        {
            FileSaveResult result = null;
            if (System.Web.HttpContext.Current.Request.Files.AllKeys.Any())
            {
                foreach (String file_id in System.Web.HttpContext.Current.Request.Files)
                {
                    HttpPostedFile file = System.Web.HttpContext.Current.Request.Files[file_id];

                    if (file != null)
                    {
                        String virtual_dir_path = String.Empty;
                        String file_name = String.Empty;

                        if (BorrowLendAnyUseOpenUtil.IdentityUploadConfrimFrontModels != null)
                            virtual_dir_path = BorrowLendAnyUseOpenUtil.IdentityUploadConfrimFrontModels.WebDirPath;
                        else
                            virtual_dir_path = String.Format("{0}/{1}/{2}", ConstantUtil.TEMP_UPLOAD_BORROWLENDANYUSE_PATH, DateTime.Now.ToString("yyyyMMdd"), Guid.NewGuid());

                        file_name = String.Format("{0}-{1}-{2}", "BorrowLendAnyUseOpen", DateTime.Now.ToString("yyyyMMddHHmm"), Guid.NewGuid());

                        result = FileUtil.HttpPostedFileSave(file, virtual_dir_path, file_name, FileType.Image);
                        if (result.result)
                        {
                            logger.Info(String.Format("doing： {0}.Upload file_id：{1} 、virtual_dir_path：{2}、file_name：{3}", BorrowLendAnyUseOpenUtil.IndexModel.Name, file_id, virtual_dir_path, file_name));
                            result.file_id = file_id;
                            result = SetUploadModel(result);
                        }
                    }
                };
            }
            else
            {
                result = new FileSaveResult();
                result.result = false;
                result.error_message = "找不到上傳檔案！";
            }

            return Json(result, "text/html", JsonRequestBehavior.AllowGet);
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
                if (BorrowLendAnyUseOpenUtil.IdentityUploadFrontModels != null)
                    model = BorrowLendAnyUseOpenUtil.IdentityUploadFrontModels;
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
                    case "Identity1_Agent":
                        model.Identity1_Agent = file.result;
                        model.Identity1_AgentName = file.file_name;
                        model.Identity1_Agent_Url = file.web_file_path;
                        break;
                    case "Identity2_Agent":
                        model.Identity2_Agent = file.result;
                        model.Identity2_AgentName = file.file_name;
                        model.Identity2_Agent_Url = file.web_file_path;
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
                    case "Bank1":
                        model.Bank1 = file.result;
                        model.Bank1_Name = file.file_name;
                        model.Bank1_Url = file.web_file_path;
                        break;
                }

                model.WebDirPath = file.web_dir_path;

                file.file_byte = null;
                result = file;
                BorrowLendAnyUseOpenUtil.IdentityUploadFrontModels = model;
            }
            catch (Exception ex)
            {
                result.result = false;
                result.error_message = String.Format("SetUploadModel_StackTrace：{0}、Message：{1}", ex.StackTrace, ex.Message);
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
            if (BorrowLendAnyUseOpenUtil.IdentityUploadFrontModels != null && file != null && !String.IsNullOrEmpty(file.file_id))
                UploadOpenUtil.DeleteSession(BorrowLendAnyUseOpenUtil.IdentityUploadFrontModels, file);
            else
                file = FileUtil.ErrorRestart();

            return Json(file, "text/html", JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region -- Confirm 送出前 確認項目頁 --
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
                    BorrowLendAnyUseOpenUtil.FileMove(AreaEnum.Front);

                    return RedirectToAction("Done");
                }
            }

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
            BorrowLendAnyUseCustomer cases = null;
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
                            upload.CaseId = cases.BorrowLendAnyUseCustomerId;
                            result = DaoService.Instance.Insert(context, upload);
                            if (!result)
                                ModelState.AddModelError("ImageUpload", ErrorTypeEnum.ImageUpload.GetDescription());
                        }
                        else
                            ModelState.AddModelError("Customer", ErrorTypeEnum.Customer.GetDescription());

                        if (result)
                        {
                            tran.Commit();

                            BorrowLendAnyUseOpenUtil.CaseModel = new CustomerCaseViewModels()
                            {
                                CustomerCaseId = MemberUtil.Case != null ? MemberUtil.Case.CustomerCaseId : String.Empty,
                                CaseTypeCode = BorrowLendAnyUseOpenUtil.IndexModel.CaseTypeCode.GetValueString(),
                                CaseTypeEnum = BorrowLendAnyUseOpenUtil.IndexModel.CaseTypeCode,
                                CaseId = upload.CaseId,
                                CustomerId = cases.CustomerId,
                                ImageUploadId = upload.ImageUploadId,
                                CaseCreateDate = upload.CreateDate,
                                CreateUser = BorrowLendAnyUseOpenUtil.IndexModel.Pid
                            };

                            BorrowLendAnyUseOpenUtil.CaseModel.CaseIdSpilt.Add(CaseTypeEnum.BorrowLendAnyUseOpen.GetValueString(), cases.BorrowLendAnyUseCustomerId);

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
        private BorrowLendAnyUseCustomer SetCase()
        {
            BorrowLendAnyUseCustomer result = null;
            try
            {
                BorrowLendAnyUseOpenViewModels IndexModel = BorrowLendAnyUseOpenUtil.IndexModel;
                BorrowLendAnyUseInvestigateViewModels InvestigateModel = BorrowLendAnyUseOpenUtil.InvestigateModel;
                BorrowLendAnyUseRelationViewModels RelationModel = BorrowLendAnyUseOpenUtil.RelationModel;
                FrontOpenApplySignViewModels ApplySignModel = BorrowLendAnyUseOpenUtil.ApplySignModel;

                Customer customer = new Customer();
                CustomerCertification certification = new CustomerCertification();
                CustomerAccount customer_account = new CustomerAccount();
                CustomerBank customer_bank = new CustomerBank();

                BorrowLendAnyUseCustomer cases = new BorrowLendAnyUseCustomer();
                BorrowLendAnyUseInvestigate investigate = new BorrowLendAnyUseInvestigate();
                BorrowLendAnyUseRelation relation = new BorrowLendAnyUseRelation();

                cases.OpenTypeCode = IndexModel.OpenTypeCode;
                cases.BranchCode = IndexModel.BranchCode;

                cases.UserType = DcnSsoUtil.UserType;
                cases.SendToMail = "0";
                cases.StatusCode = "0";

                cases.BankStatementSendCode = IndexModel.BankStatementSendCode;
                cases.CreditLine = IndexModel.CreditLine;
                cases.ExtensionTimeCode = IndexModel.ExtensionTimeCode;
                cases.Descriptions = String.Empty;
                cases.Deleted = false;

                #region 客服資料
                customer.CaseTypeCode = BorrowLendAnyUseOpenUtil.IndexModel.CaseTypeCode.GetValueString();
                customer.Name = IndexModel.Name;
                customer.Pid = IndexModel.Pid.ToUpper();
                customer.ResidentCityCode = IndexModel.ResidentCityCode;
                customer.ResidentTownCode = IndexModel.ResidentTownCode;
                customer.ResidentAddress = IndexModel.ResidentAddress;
                customer.CityCode = IndexModel.CityCode;
                customer.TownCode = IndexModel.TownCode;
                customer.Address = IndexModel.Address;
                customer.Email = IndexModel.Email;
                customer.OccupationCode = IndexModel.OccupationCode;
                customer.JobTitleCode = IndexModel.JobTitleCode;
                customer.CompanyName = IndexModel.CompanyName;
                customer.CompanyTelePhone = IndexModel.CompanyTelePhone;
                customer.CompanyCity = IndexModel.CompanyCityCode;
                customer.CompanyTown = IndexModel.CompanyTownCode;
                customer.CompanyAddress = IndexModel.CompanyAddress;
                customer.EmergencyName = IndexModel.EmergencyName;
                customer.EmergencyContactPhone = IndexModel.EmergencyContactPhone;

                customer.TelePhone = IndexModel.TelePhone;
                customer.ContactPhone = IndexModel.ContactPhone;
                customer.MobilePhone = IndexModel.MobilePhone;
                customer.Fax = String.Empty;
                customer.UpdateHistory = String.Empty;
                customer.UpdateHistory = String.Empty;

                customer.CreateDate = IndexModel.CreateDate;
                customer.CreateUser = "User";
                customer.UserHostAddress = MvcUtil.WebTraceHostAddress;
                customer.Deleted = false;

                if (IndexModel.SameResident)
                {
                    customer.CityCode = IndexModel.ResidentCityCode;
                    customer.TownCode = IndexModel.ResidentTownCode;
                    customer.Address = IndexModel.ResidentAddress;
                }

                customer_account.AccountId = IndexModel.CustomerAccount;

                customer_bank.BankBranchCode = IndexModel.BranchCode;
                customer_bank.BankName = IndexModel.BankName;
                customer_bank.BankBranchName = IndexModel.BankBranchName;
                customer_bank.BankAccountName = IndexModel.Name;
                customer_bank.BankAccountId = IndexModel.BankAccountId;
                #endregion

                #region 憑證資訊
                if (ApplySignModel != null)
                {
                    certification.SerialNumber = ApplySignModel.SerialNumber;
                    certification.NotAfter = ApplySignModel.NotAfter;
                    certification.NotBefore = ApplySignModel.NotBefore;
                    certification.Cser = ApplySignModel.CertContent;
                    certification.Csr = ApplySignModel.Csr;
                }
                #endregion

                #region 徵信
                investigate.OpenAccountForThreeMonth = InvestigateModel.OpenAccountForThreeMonth;
                investigate.HaveRefundRecord = InvestigateModel.HaveRefundRecord;
                investigate.BreachOfContract = InvestigateModel.BreachOfContract;
                investigate.OtherStockAccount = InvestigateModel.OtherStockAccount;
                investigate.OpenAccountCount = InvestigateModel.OpenAccountCount;
                investigate.OtherStockAccountName = InvestigateModel.OtherStockAccountName;
                investigate.PersonalInComeYear = InvestigateModel.PersonalInComeYear;
                investigate.TotalAsset = InvestigateModel.TotalAsset;
                investigate.InvestmentExperience = InvestigateModel.InvestmentExperience;
                investigate.InvestmentDuration = InvestigateModel.InvestmentDuration;
                investigate.TradingFrequency = InvestigateModel.TradingFrequency;
                investigate.MovableAndRealEstate = InvestigateModel.MovableAndRealEstate;
                investigate.TheOpenRealEstate = InvestigateModel.TheOpenRealEstate;
                investigate.PriceSetting = InvestigateModel.PriceSetting;
                investigate.ProofOfProperty = InvestigateModel.ProofOfProperty;

                investigate.CreateUser = InvestigateModel.CreateUser;
                investigate.CreateDate = InvestigateModel.CreateDate;
                #endregion

                /*
                #region 關係人 2019/04 拿掉流程
                relation.SpouseRelationName1 = RelationModel.SpouseRelationName1;
                relation.SpouseRelation1 = RelationModel.SpouseRelation1;
                relation.SpouseRelationPid1 = RelationModel.SpouseRelationPid1;
                relation.RelativeRelationName1 = RelationModel.RelativeRelationName1;
                relation.RelativeRelationPid1 = RelationModel.RelativeRelationPid1;
                relation.RelativeRelation1 = RelationModel.RelativeRelation1;
                relation.RelativeRelationName2 = RelationModel.RelativeRelationName2;
                relation.RelativeRelationPid2 = RelationModel.RelativeRelationPid2;
                relation.RelativeRelation2 = RelationModel.RelativeRelation2;
                relation.RelativeRelationName3 = RelationModel.RelativeRelationName3;
                relation.RelativeRelationPid3 = RelationModel.RelativeRelationPid3;
                relation.RelativeRelation3 = RelationModel.RelativeRelation3;
                relation.RelativeRelationName4 = RelationModel.RelativeRelationName4;
                relation.RelativeRelationPid4 = RelationModel.RelativeRelationPid4;
                relation.RelativeRelation4 = RelationModel.RelativeRelation4;
                relation.CompanyName1 = RelationModel.CompanyName1;
                relation.UniformNumber1 = RelationModel.UniformNumber1;
                relation.Job1 = RelationModel.Job1;
                relation.CompanyName2 = RelationModel.CompanyName2;
                relation.UniformNumber2 = RelationModel.UniformNumber2;
                relation.Job2 = RelationModel.Job2;
                relation.SpouseCompanyName1 = RelationModel.SpouseCompanyName1;
                relation.SpouseUniformNumber1 = RelationModel.SpouseUniformNumber1;
                relation.SpouseJob1 = RelationModel.SpouseJob1;
                relation.SpouseCompanyName2 = RelationModel.SpouseCompanyName2;
                relation.SpouseUniformNumber2 = RelationModel.SpouseUniformNumber2;
                relation.SpouseJob2 = RelationModel.SpouseJob2;
                relation.CreateUser = RelationModel.CreateUser;
                relation.CreateDate = RelationModel.CreateDate;
                #endregion
                // cases.BorrowLendAnyUseRelation = relation;

                */

                cases.BorrowLendAnyUseInvestigate = investigate;
                cases.Customer = customer;
                cases.Customer.CustomerAccount.Add(customer_account);
                cases.Customer.CustomerBank.Add(customer_bank);
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
        private ImageUpload SetUpload()
        {

            ImageUpload result = new ImageUpload();
            FrontOpenIdentityUploadViewModels upload = null;
            BorrowLendAnyUseOpenIdentityConfirmUploadViewModels identity_confirm_model = null;

            try
            {
                upload = BorrowLendAnyUseOpenUtil.IdentityUploadFrontModels;
                identity_confirm_model = BorrowLendAnyUseOpenUtil.IdentityUploadConfrimFrontModels;

                result.CaseTypeCode = BorrowLendAnyUseOpenUtil.IndexModel.CaseTypeCode.GetValueString();

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

                result.BankName1 = upload.Bank1_Name;

                if (upload.RealEstate1)
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
