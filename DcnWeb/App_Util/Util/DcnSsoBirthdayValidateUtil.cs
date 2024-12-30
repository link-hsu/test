using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Dcn.Util;
using System.Net.Sockets;
using System.Configuration;

[Serializable]
public class DcnSsoBirthdayValidateUtil
{
    private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

    private const String SSO_LOGINS = "session.dcn.sso.logins";
    private const String SSO_USER_TYPE = "session.dcn.sso.usertype";
    private const String SSO_OPEN_TYPE = "session.dcn.sso.opentype";
    private const String SSO_USER_PID = "session.dcn.sso.account";
    private const String SSO_USER_BIRTHDAY = "session.dcn.sso.birthday";
    private const String SSO_REDIRECT_URL = "session.dcn.sso.redirect.url";

    /// <summary>
    /// 帳號驗證-是否登入 (預設：0-無、1-有)
    /// </summary>
    public static Boolean Logins
    {
        get
        {
            Boolean result = false;
            if (HttpContext.Current.Session[SSO_LOGINS] == null)
                HttpContext.Current.Session[SSO_LOGINS] = false;

            result = Boolean.TryParse(HttpContext.Current.Session[SSO_LOGINS].ToString(), out result);

            if (result)
                result = Convert.ToBoolean(HttpContext.Current.Session[SSO_LOGINS]);

            return result;
        }
        set { HttpContext.Current.Session[SSO_LOGINS] = value; }
    }

    /// <summary>
    /// 來源參數：1.WebLogin  2.SorderWeb 5.會員已綁定下單帳號 9.本機測試
    /// </summary>
    public static String UserType
    {
        get
        {
            if (HttpContext.Current.Session[SSO_USER_TYPE] == null)
                HttpContext.Current.Session[SSO_USER_TYPE] = "1";
            return HttpContext.Current.Session[SSO_USER_TYPE] as String;
        }
        set { HttpContext.Current.Session[SSO_USER_TYPE] = value; }
    }

    /// <summary>
    /// 信用開戶類別(1.雙開預設、2.加開、3.續約、4.額度)
    /// </summary>
    public static String OpenType
    {
        get
        {
            return HttpContext.Current.Session[SSO_OPEN_TYPE] as String;
        }
        set { HttpContext.Current.Session[SSO_OPEN_TYPE] = value; }
    }

    /// <summary>
    /// 身分證
    /// </summary>
    public static String Pid
    {
        get
        {
            if (HttpContext.Current.Session[SSO_USER_PID] == null)
                HttpContext.Current.Session[SSO_USER_PID] = String.Empty;
            return HttpContext.Current.Session[SSO_USER_PID] as String;
        }
        set { HttpContext.Current.Session[SSO_USER_PID] = value; }
    }

    /// <summary>
    /// 生日
    /// </summary>
    public static String Birthday
    {
        get
        {
            if (HttpContext.Current.Session[SSO_USER_BIRTHDAY] == null)
                HttpContext.Current.Session[SSO_USER_BIRTHDAY] = String.Empty;
            return HttpContext.Current.Session[SSO_USER_BIRTHDAY] as String;
        }
        set { HttpContext.Current.Session[SSO_USER_BIRTHDAY] = value; }
    }

    /// <summary>
    /// 線上開戶 登入帳號頁，記錄導向舊戶加開項目
    /// </summary>
    public static String RedirectUrl
    {
        get
        {
            if (HttpContext.Current.Session[SSO_REDIRECT_URL] == null)
                HttpContext.Current.Session[SSO_REDIRECT_URL] = "";
            return HttpContext.Current.Session[SSO_REDIRECT_URL] as String;
        }
        set { HttpContext.Current.Session[SSO_REDIRECT_URL] = value; }
    }
}
