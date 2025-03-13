using AutoScanFQCTest.DataModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AutoScanFQCTest.Logics
{
    public class User
    {
        public string Username { get; set; }
    }

    // MES处理类
    public class MESHandler
    {
        public MainWindow m_parent;

        // 登录信息
        string m_strLoginInfo = "";

        // 服务器地址
        string m_strServerAddress = "http://UCVWMEAP01.mflex.com.cn/wiupload/";
        string m_strLoginURL = "http://UCVWMEAP01.mflex.com.cn/api/SMT/CheckLogin";
        string m_strLoginURL2 = "http://10.13.5.254:8080/api/avi/MES_SaveResultByPcs/";


        // 构造函数
        public MESHandler(MainWindow parent)
        {
            m_parent = parent;
        }

        // 发送数据到MES
        public bool SendMES(string strURL, string strDataToSend, ref string strResponse, int nGetOrPostFlag = 1)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(strURL);

                request.Method = "POST";
                request.ContentType = "application/json";

                if (nGetOrPostFlag == 0)
                {
                    request.Method = "GET";
                }

                Debugger.Log(0, null, string.Format("222222 SendMES url: {0}", strURL));

                // 发送请求数据
                using (StreamWriter writer = new StreamWriter(request.GetRequestStream(), Encoding.UTF8))
                {
                    writer.Write(strDataToSend);
                    writer.Flush();
                    writer.Close();

                    Debugger.Log(0, null, string.Format("222222 MES 数据发送成功！"));
                }

                // 接收响应数据
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    Debugger.Log(0, null, string.Format("222222 SendMES 111"));

                    string responseJson = reader.ReadToEnd();

                    Debugger.Log(0, null, string.Format("222222 接收成功，MES服务器返回数据：{0}", responseJson));

                    strResponse = responseJson;
                }

                return true;
            }
            catch (Exception ex)
            {
                Debugger.Log(0, null, string.Format("222222 MES请求异常，可能是连接失败，异常信息：{0}", ex.Message));
                return false;
            }
        }

        // 获取系统信息
        public void GetSysInfo()
        {
            //AutoScanMain.GetBaseInfo(ref m_sysInfo);
        }

        // 登录
        public void Login(string userName, string password)
        {
            // 登录信息
            string message = "";

            User user = new User();
            user.Username = userName;

            string json = JsonConvert.SerializeObject(user);

            // 登录
            SendMES(m_strLoginURL2, json, ref message, 1);

            //if (true == m_dataProvider.CheckLogin(m_login, ref message))
            //{
            //    m_parent.m_global.m_log_presenter.Log("MES登录成功");

            //    m_strLoginInfo = message;
            //}
            //else
            //{
            //    m_parent.m_global.m_log_presenter.Log("MES登录失败");
            //}
        }

        // 登录
        public void Test()
        {
            // 登录信息
            string message = "";

            List<DetailList> detaillist = new List<DetailList>();
            detaillist.Add(new DetailList());

            List<SummaryList> summaryList = new List<SummaryList>();
            summaryList.Add(new SummaryList());

            List<MicInfoList> micInfoList = new List<MicInfoList>();
            micInfoList.Add(new MicInfoList());

            List<PreussresList> preussresList = new List<PreussresList>();
            preussresList.Add(new PreussresList());

            List<OrionInfoList> orionInfoList = new List<OrionInfoList>();
            orionInfoList.Add(new OrionInfoList());

            PanelRecheckResult result = new PanelRecheckResult();

            result.detaillist = detaillist;
            result.summarylist = summaryList;
            result.micinfolist = micInfoList;
            result.preussresList = preussresList;
            result.orionInfoList = orionInfoList;

            string json = JsonConvert.SerializeObject(result);

            json = json.Replace("null", "\"abc\"");

            // 登录
            SendMES(m_strLoginURL2, json, ref message, 1);
        }

        // 获取缺陷描述列表
        public List<string[]> GetDefectReasonList()
        {
            string msg = "";
            List<string[]> defectReasonList = new List<string[]>();
            List<string> etItemList = new List<string>();

            //AutoScanMain.GetComponentDefectReason(m_login, ref defectReasonList, ref etItemList, ref msg);

            return defectReasonList;
        }

        //// FQC取得元件位置并按照不良次数排序返回
        //public bool GetReferenceDesignatorByOrder(Login login, string strPanelBarcode, ref List<string> orderdDesignator, ref string errMsg)
        //{
        //    return AutoScanMain.getReferenceDesignatorByOrder(login, strPanelBarcode, ref orderdDesignator, ref errMsg);
        //}

        //// 根据FQC条码对应的短料号统计TOP10不良并按顺序返回
        //public bool GetCatalognumberDefectCodeByOrder(Login login, string panel, ref List<List<string>> orderDefect, ref string errMsg)
        //{
        //    return AutoScanMain.getCatalognumberDefectCodeByOrder(login, panel, ref orderDefect, ref errMsg);
        //}

        // 提交OK到MES
        public bool SubmitOKToMES()
        {
            //m_dataProvider.MesFunction(model, m_login);

            return true;
        }

        // 发送数据到MES
        public void SendToMES(string sn, string result)
        {

        }

        // 从MES获取数据
        public string GetFromMES(string sn)
        {
            return "OK";
        }

        // 提交
        public bool SubmitPanelRecheckResults(MESProductInfo MES_product_info, ref string strMesServerResponse)
        {
            List<DetailList> detaillist = new List<DetailList>();
            detaillist.Add(new DetailList());

            List<SummaryList> summaryList = new List<SummaryList>();
            summaryList.Add(new SummaryList());

            List<MicInfoList> micInfoList = new List<MicInfoList>();
            micInfoList.Add(new MicInfoList());

            List<PreussresList> preussresList = new List<PreussresList>();
            preussresList.Add(new PreussresList());

            List<OrionInfoList> orionInfoList = new List<OrionInfoList>();
            orionInfoList.Add(new OrionInfoList());

            for (int i = 0; i < MES_product_info.m_AVI_product_info.Products.Count; i++)
            {
                Product product = MES_product_info.m_AVI_product_info.Products[i];

                if (true)
                {
                    DetailList detail = new DetailList();

                    detail.panelId = product.PanelId;
                    detail.pcsBarCode = product.BarCode;
                    detail.testType = "testType";
                    detail.pcsSeq = "pcsSeq";
                    detail.partSeq = "partSeq";
                    detail.pinSeq = "pinSeq";

                    switch (MES_product_info.m_recheck_flags[i])
                    {
                        case RecheckResult.OK:
                            detail.testResult = "OK";
                            break;
                        case RecheckResult.NG:
                            detail.testResult = "NG";
                            break;
                        case RecheckResult.NotChecked:
                            detail.testResult = "NotChecked";
                            break;
                    }

                    detaillist.Add(detail);
                }

                if (true)
                {
                    SummaryList summary = new SummaryList();

                    summary.panelId = product.PanelId;

                    switch (MES_product_info.m_recheck_flags[i])
                    {
                        case RecheckResult.OK:
                            summary.testResult = "OK";
                            break;
                        case RecheckResult.NG:
                            summary.testResult = "NG";
                            break;
                        case RecheckResult.NotChecked:
                            summary.testResult = "NotChecked";
                            break;
                    }

                    summaryList.Add(summary);
                }
            }

            PanelRecheckResult result = new PanelRecheckResult();

            result.machine = MES_product_info.m_AVI_product_info.Mid;

            result.panel = MES_product_info.m_AVI_product_info.Products[0].PanelId;

            result.detaillist = detaillist;
            result.summarylist = summaryList;
            result.micinfolist = micInfoList;
            result.preussresList = preussresList;
            result.orionInfoList = orionInfoList;

            string json = JsonConvert.SerializeObject(result);

            return SendMES(m_strLoginURL2, json, ref strMesServerResponse, 1);
        }

        // 检测一个特定的IP地址和端口（比如10.13.5.254:8080）是否可以成功连接
        public static bool IsPortOpen(string host, int port, int timeout)
        {
            try
            {
                using (var client = new TcpClient())
                {
                    var result = client.BeginConnect(host, port, null, null);
                    var success = result.AsyncWaitHandle.WaitOne(timeout);
                    if (!success)
                    {
                        return false;
                    }

                    client.EndConnect(result);
                    return true;
                }
            }
            catch
            {
                // Any exception will indicate that the port is closed
                return false;
            }
        }

    }
}
