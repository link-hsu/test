using Dcn.DdscUtil.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml.Linq;
using Dcn.Util;


namespace Dcn.DdscUtil
{
    /// <summary>
    /// 五、001查詢客戶資料
    /// </summary>
    public class DdscS703 : BaseDao
    {
        private readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public static readonly String DDSC_STOCK_IP = ConfigurationManager.AppSettings["DdscStockSocket"];
        
        private string _function_code = "703";
        private int _port = 9703;


        // 修改這邊
        public DdscS703(string cust_id)
        {
            this.function_code = _function_code;
            string branch_id = "6460";
            this.branch_id = branch_id.PadLeft(4, ' ');
            this.cust_id = cust_id.PadLeft(7, ' ');
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="branch_id">分公司</param>
        /// <param name="cust_id">客戶帳號(含檢查碼)  EX: 1234567</param>
        public DdscS703(string branch_id, string cust_id)
        {
            this.function_code = _function_code;
            this.branch_id = branch_id.PadLeft(4, ' ');
            this.cust_id = cust_id.PadLeft(7, ' ');
        }

        /// <summary>
        /// 分公司
        /// </summary>
        public string branch_id { get; set; }

        /// <summary>
        /// 客戶帳號(含檢查碼)  EX: 1234567
        /// </summary>
        public string cust_id { get; set; }

        public DdscS703d data { get; set; }

        /// <summary>
        /// (公開)取得資料
        /// </summary>
        /// <returns></returns>
        public DdscS703d Get()
        {

            try
            {
                Send();
            }
            catch (Exception ex)
            {
                logger.Info(ex.Message);
                logger.Info(ex.StackTrace);
            }

            return data;
        }

        /// <summary>
        /// 1.送出
        /// </summary>
        /// <returns></returns>
        private Boolean Send()
        {
            try
            {
                Sockets socket = new Sockets(DDSC_STOCK_IP, _port);
                socket.SendData = SetSend();

                SocketUitl.Socket(socket);

                if (socket.Connected)
                {
                    GetReceive(socket.ReceiveContent);
                    GetBody();
                }

                socket = null;
            }
            catch (Exception ex)
            {
                logger.Info(ex.Message);
                logger.Info(ex.StackTrace);
            }

            return data != null ? true : false; ;
        }

        /// <summary>
        /// 1.送出內容
        /// </summary>
        /// <returns></returns>
        private byte[] SetSend()
        {
            byte[] result = null;

            try
            {
                body_temp = SetBody();

                result = head_check_stock.Concat(body_temp).Concat(end_check_stock).ToArray();

            }
            catch (Exception ex)
            {
                logger.Info(ex.Message);
                logger.Info(ex.StackTrace);
            }

            return result;
        }

        private byte[] SetBody()
        {
            byte[] result = null;
            String path = String.Empty;
            try
            {
                XElement req = new XElement("req");

                XAttribute sys_code = new XAttribute("sys_code", "001");
                XAttribute request_no = new XAttribute("request_no", seq_stock);
                XAttribute gate_func = new XAttribute("gate_func", this.function_code);
                XAttribute branch_id = new XAttribute("branch_id", this.branch_id);
                XAttribute cust_id = new XAttribute("cust_id", this.cust_id);

                req.Add(sys_code, request_no, gate_func, branch_id, cust_id);


                result = Encoder.GetBytes(req.ToString().Replace(@"""", "'"));
                //String A = req.ToString().Replace(@"""", "'");
                //logger.Info(A);

            }
            catch (Exception ex)
            {
                logger.Info(ex.Message);
                logger.Info(ex.StackTrace);
            }

            return result;
        }

        /// <summary>
        /// 取得 內容
        /// </summary>
        private void GetBody()
        {
            DdscS703d d703 = new DdscS703d();
            String context = String.Empty;
            apgw obj = null;

            try
            {
                if (ReceiveContent.Count > body_temp.Length)
                {
                    context = Encoding.Default.GetString(ReceiveContent.GetRange(1, ReceiveContent.Count-4).ToArray());

                    obj = XmlUtil.Deserialize<apgw>(context);

                    if(obj != null)
                    {
                        d703.item = obj;
                        d703.return_code = obj.ret.retcode;
                        d703.return_code_name = obj.ret.retmsg;
                        logger.Info($"ok：{d703.item.ret.cust_data.idno}.{d703.item.ret.cust_data.cname}");

                    }

                }
            }
            catch (Exception ex)
            {
                d703.return_code = "-999999";
                d703.return_code = "系統異常";

                logger.Info($"資料錯誤：{branch_id}.{cust_id}");
                logger.Info(ex.Message);
                logger.Info(ex.StackTrace);
            }

            data = d703 != null ? d703 : null;
        }
    }

    /// <summary>
    /// 703 客戶資料
    /// </summary>
    public class DdscS703d
    {
        /// <summary>
        /// 回應碼
        /// </summary>
        public string return_code { get; set; }


        /// <summary>
        /// 回應碼(中文說明)
        /// </summary>
        public string return_code_name { get; set; }


        /// <summary>
        /// 回傳內容
        /// </summary>
        public apgw item { get; set; }

    }
}
