using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutoScanFQCTest
{
    public class MFlex
    {
        /// <summary>
        /// MFlex Mes Address
        /// </summary>
        public enum MesAddress
        {
            SZ,
            YC
        }

        public static readonly string SZ = "http://ot-messmtaviapi.mflex.com.cn/";
        public static readonly string YC = "http://mycmessmtaviapi.mflex.com.cn/";

        /// <summary>
        /// 根据IC卡号获得员工ID账号
        /// </summary>
        /// <param name="address">Mes地址</param>
        /// <param name="IC">IC卡号</param>
        /// <param name="MachineName">机台编号</param>
        /// <param name="EmployeeID">返回员工ID</param>
        /// <returns>标准ResponseInfo返回值</returns>
        public static ResponseInfo GetUserAccountByIC(MesAddress address, string IC, string MachineName,out string EmployeeID)
        {
            ResponseInfo sp = new ResponseInfo();
            sp.StartTime = DateTime.Now;
            //Dictionary<string, dynamic> dict = new Dictionary<string, dynamic>();
            EmployeeID = string.Empty;
            string temp = string.Empty;
            try
            {
                string URL = string.Empty;
                switch(address)
                {
                    case MesAddress.SZ:
                        {
                            URL = SZ;
                        }; break;
                    case MesAddress.YC:
                        {
                            URL = YC;
                        }; break;
                }
                sp = HttpHelper.Get(URL + "Api/SmtInfo/UserTrainingsAsync", 
                    string.Format("EmployeeName={0}&MachineName={1}", IC, MachineName),out temp);

                Dictionary<string, dynamic> dict = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(temp);

                if (sp.ret > 0 && dict.Count > 0 && dict["barcode"] != null && dict["isPass"] != false)
                {
                    EmployeeID = dict["barcode"] == null ? string.Empty : dict["barcode"];
                }
                else
                {
                    MessageBox.Show( temp, "刷卡错误");
                }
            }
            catch (Exception ex)
            {
                sp.Exception = ex;
            }
            sp.EndTime = DateTime.Now;
            return sp;
        }



    }
}
