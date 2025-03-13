using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.NetworkInformation;
using FupanBackgroundService.DataModels;
using System.IO;
using System.Diagnostics;
using static FupanBackgroundService.DataModels.AVIProductInfo;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using FupanBackgroundService.Helpers;
using AutoScanFQCTest.DataModels;
using System.Windows.Interop;
using System.Text.Json;
using System.ComponentModel;
using MySql.Data.MySqlClient;
using System.Data;
using FupanBackgroundService.Utilities;
using System.Runtime.Remoting.Messaging;
using System.Windows;

namespace FupanBackgroundService.Logics
{
    // 产品信息序列化和反序列化帮助类
    public class FqcJsonSerializerHelper
    {
        // 序列化产品信息
        public static string SerializeProductInfo(FqcTrayInfo productInfo)
        {
            return System.Text.Json.JsonSerializer.Serialize(productInfo, new JsonSerializerOptions { WriteIndented = true });
        }

        // 反序列化产品信息
        public static FqcTrayInfo DeserializeProductInfo(string json)
        {
            return System.Text.Json.JsonSerializer.Deserialize<FqcTrayInfo>(json);
        }
    }

    public class FqcTrayInfo
    {
        [JsonPropertyName("set_id")]
        public string SetId { get; set; }

        /// <summary>
        /// Panel号
        /// </summary>
        [Description("[Panel号]")]
        [JsonPropertyName("panel")]
        public string panel { get; set; }

        /// <summary>
        /// 线体，无线体时线体为机台
        /// </summary>
        [Description("[线体]")]
        [JsonPropertyName("resource")]
        public string resource { get; set; }

        /// <summary>
        /// 位置，苏州：SMT，盐城：YCSMT
        /// </summary>
        [Description("[位置]")]
        [JsonPropertyName("site")]
        public string site { get; set; }

        /// <summary>
        /// 机台MAC地址
        /// </summary>
        [Description("[机台MAC地址]")]
        [JsonPropertyName("mac")]
        public string mac { get; set; }

        /// <summary>
        /// 程序名
        /// </summary>
        [Description("[程序名]")]
        [JsonPropertyName("programName")]
        public string programName { get; set; }

        /// <summary>
        /// 机台
        /// </summary>
        [Description("[机台]")]
        [JsonPropertyName("machine")]
        public string machine { get; set; }

        /// <summary>
        /// 产品
        /// </summary>
        [Description("[产品]")]
        [JsonPropertyName("product")]
        public string product { get; set; }

        /// <summary>
        /// 工位
        /// </summary>
        [Description("[工位]")]
        [JsonPropertyName("workArea")]
        public string workArea { get; set; }

        /// <summary>
        /// 测试类型
        /// </summary>
        [Description("[测试类型]")]
        [JsonPropertyName("testType")]
        public string testType { get; set; }

        /// <summary>
        /// 测试时间
        /// </summary>
        [Description("[测试时间]")]
        [JsonPropertyName("testTime")]
        public string testTime { get; set; }

        /// <summary>
        /// 操作员
        /// </summary>
        [Description("[操作员]")]
        [JsonPropertyName("operatorName")]
        public string operatorName { get; set; }

        /// <summary>
        /// 操作员
        /// </summary>
        [Description("[操作员类别]")]
        [JsonPropertyName("operatorType")]
        public string operatorType { get; set; }

        /// <summary>
        /// 测试模式
        /// </summary>
        [Description("[测试模式]")]
        [JsonPropertyName("testMode")]
        public string testMode { get; set; }

        /// <summary>
        /// 轨道L Left Track, R Right Track, S Single Track
        /// </summary>
        [Description("[轨道]")]
        [JsonPropertyName("trackType")]
        public string trackType { get; set; }

        /// <summary>
        /// 是否检测数据
        /// </summary>
        [Description("[是否检测数据]")]
        [JsonPropertyName("checkPcsDataForAVI")]
        public bool checkPcsDataForAVI { get; set; }

        /// <summary>
        /// UUID
        /// </summary>
        [Description("[UUID]")]
        [JsonPropertyName("uuid")]
        public string uuid { get; set; }

        /// <summary>
        /// 检测细节
        /// </summary>
        [Description("[检测细节]")]
        [JsonPropertyName("checkDetail")]
        public string checkDetail { get; set; }

        /// <summary>
        /// 检测细节
        /// </summary>
        [Description("[检测细节]")]
        [JsonPropertyName("tableInfor")]
        public string tableInfor { get; set; }

        /// <summary>
        /// 检测细节
        /// </summary>
        [Description("[检测细节]")]
        [JsonPropertyName("totalCol")]
        public int totalCol { get; set; }

        /// <summary>
        /// 检测细节
        /// </summary>
        [Description("[检测细节]")]
        [JsonPropertyName("totalRow")]
        public int totalRow { get; set; }

        /// <summary>
        /// 机台ip地址
        /// </summary>
        [Description("[单片汇总]")]
        [JsonPropertyName("pcsSummarys")]
        public FqcProductSummary[] pcsSummarys { get; set; }

        public FqcTrayInfo cloneClass()
        {
            return (FqcTrayInfo)this.MemberwiseClone();
        }
    }

    public class FqcProductSummary
    {
        public List<string> m_list_local_imageA_paths;
        public List<string> m_list_local_imageB_paths;
        public List<string> m_list_local_imageC_paths;
        public List<string> m_list_remote_imageA_paths;
        public List<string> m_list_remote_imageB_paths;
        public List<string> m_list_remote_image_paths;

        [JsonPropertyName("set_id")]
        public string SetId { get; set; }

        /// <summary>
        /// panel号
        /// </summary>
        [Description("[panel号]")]
        [JsonPropertyName("panelId")]
        public string panelId { get; set; }

        /// <summary>
        /// 检测细节
        /// </summary>
        [Description("[检测细节]")]
        [JsonPropertyName("row")]
        public int pos_row { get; set; }

        /// <summary>
        /// 检测细节
        /// </summary>
        [Description("[检测细节]")]
        [JsonPropertyName("col")]
        public int pos_col { get; set; }


        /// <summary>
        /// Pcs位置
        /// </summary>
        [Description("[Pcs位置]")]
        [JsonPropertyName("pcsSeq")]
        public string pcsSeq { get; set; }

        /// <summary>
        /// 测试人员
        /// </summary>
        [Description("[测试人员]")]
        [JsonPropertyName("operatorName")]
        public string operatorName { get; set; }

        /// <summary>
        /// 测试时间
        /// </summary>
        [Description("[测试时间]")]
        [JsonPropertyName("operatorTime")]
        public string operatorTime { get; set; }

        /// <summary>
        /// 复判结果
        /// </summary>
        [Description("[复判结果]")]
        [JsonPropertyName("verifyResult")]
        public string verifyResult { get; set; }

        /// <summary>
        /// 复判人员
        /// </summary>
        [Description("[复判人员]")]
        [JsonPropertyName("verifyOperatorName")]
        public string verifyOperatorName { get; set; }

        /// <summary>
        /// 复判时间
        /// </summary>
        [Description("[复判时间]")]
        [JsonPropertyName("verifyTime")]
        public string verifyTime { get; set; }

        /// <summary>
        /// 测试结果
        /// </summary>
        [Description("[测试结果]")]
        [JsonPropertyName("testResult")]
        public string testResult { get; set; }

        /// <summary>
        /// 测试结果
        /// </summary>
        [Description("[测试结果]")]
        [JsonPropertyName("pcsDetails")]
        public FqcProductDetail[] pcsDetails { get; set; }

    }

    public class FqcProductDetail
    {
        [JsonPropertyName("set_id")]
        public string SetId { get; set; }

        /// <summary>
        /// panel号
        /// </summary>
        [Description("[panel号]")]
        [JsonPropertyName("panelId")]
        public string panelId { get; set; }

        /// <summary>
        /// barcode
        /// </summary>
        [Description("[barcode]")]
        [JsonPropertyName("pcsBarCode")]
        public string pcsBarCode { get; set; }

        /// <summary>
        /// 测试类型
        /// </summary>
        [Description("[测试类型]")]
        [JsonPropertyName("testType")]
        public string testType { get; set; }

        /// <summary>
        /// pcs位置
        /// </summary>
        [Description("[pcs位置]")]
        [JsonPropertyName("pcsSeq")]
        public string pcsSeq { get; set; }

        /// <summary>
        /// 元件位置
        /// </summary>
        [Description("[元件位置]")]
        [JsonPropertyName("partSeq")]
        public string partSeq { get; set; }

        /// <summary>
        /// pin脚位置
        /// </summary>
        [Description("[pin脚位置]")]
        [JsonPropertyName("pinSeq")]
        public string pinSeq { get; set; }

        /// <summary>
        /// 测试结果
        /// </summary>
        [Description("[测试结果]")]
        [JsonPropertyName("testResult")]
        public string testResult { get; set; }

        /// <summary>
        /// 测试人员
        /// </summary>
        [Description("[测试人员]")]
        [JsonPropertyName("operatorName")]
        public string operatorName { get; set; }

        /// <summary>
        /// 复判结果
        /// </summary>
        [Description("[复判结果]")]
        [JsonPropertyName("verifyResult")]
        public string verifyResult { get; set; }

        /// <summary>
        /// 复判人员
        /// </summary>
        [Description("[复判人员]")]
        [JsonPropertyName("verifyOperatorName")]
        public string verifyOperatorName { get; set; }

        /// <summary>
        /// 复判时间
        /// </summary>
        [Description("[复判时间]")]
        [JsonPropertyName("verifyTime")]
        public string verifyTime { get; set; }

        /// <summary>
        /// 缺陷代码
        /// </summary>
        [Description("[缺陷代码]")]
        [JsonPropertyName("defectCode")]
        public string defectCode { get; set; }

        /// <summary>
        /// MIC气泡的百分比
        /// </summary>
        [Description("[MIC气泡的百分比]")]
        [JsonPropertyName("bubbleValue")]
        public string bubbleValue { get; set; }

        /// <summary>
        /// 文件位置
        /// </summary>
        [Description("[文件位置]")]
        [JsonPropertyName("testFile")]
        public string testFile { get; set; }

        /// <summary>
        /// 图片地址
        /// </summary>
        [Description("[图片地址]")]
        [JsonPropertyName("imagePath")]
        public string imagePath { get; set; }

        public FqcTrayInfo cloneClass()
        {
            return (FqcTrayInfo)this.MemberwiseClone();
        }
    }

    // 复判结果
    public class RecheckResult
    {
        [JsonPropertyName("set_id")]
        public string set_id { get; set; }                             // 料盘ID

        [JsonPropertyName("barcode")]
        public string barcode { get; set; }                           // 产品ID

        [JsonPropertyName("recheck_result")]
        public string recheck_result { get; set; }               // 复判结果

        [JsonPropertyName("r1")]
        public string r1 { get; set; }                           // 预留字段1，检测时间

        [JsonPropertyName("r2")]
        public string r2 { get; set; }                           // 预留字段2，RegionArea

        [JsonPropertyName("r3")]
        public string r3 { get; set; }                           // 预留字段3

        [JsonPropertyName("r4")]
        public string r4 { get; set; }                           // 预留字段4

        [JsonPropertyName("r5")]
        public string r5 { get; set; }                           // 预留字段5

        [JsonPropertyName("r6")]
        public string r6 { get; set; }                           // 预留字段6

        [JsonPropertyName("r7")]
        public string r7 { get; set; }                           // 预留字段7

        [JsonPropertyName("r8")]
        public string r8 { get; set; }                           // 预留字段8

        [JsonPropertyName("r9")]
        public string r9 { get; set; }                           // 预留字段9

        [JsonPropertyName("r10")]
        public string r10 { get; set; }                           // 预留字段10
    }

    public class AVICommunication
    {
        [DllImport("kernel32.dll", CallingConvention = CallingConvention.Winapi)]
        extern static int GetTickCount();

        private TcpListener m_TCP_listener;                              // 服务器监听器
        private TcpClient m_connected_client;                          // 已连接的客户端
        private Thread m_listener_thread;                                  // 监听线程
        private string m_strIP = "";                                             // 服务器IP地址
        private int m_nPort = 0;                                                  // 服务器端口
        private bool m_bIsTCPServerInited = false;                  // TCP服务器是否已初始化

        private Task m_task1;
        private Task m_task2;
        private Task m_task3;
        private Task m_task4;
        private Task m_task5;
        private Task m_task6;
        private CancellationTokenSource m_cancellationToken1;
        private CancellationTokenSource m_cancellationToken2;
        private CancellationTokenSource m_cancellationToken3;
        private CancellationTokenSource m_cancellationToken4;
        private CancellationTokenSource m_cancellationToken5;
        private CancellationTokenSource m_cancellationToken6;
        private Queue<string> m_queue_of_reported_data = new Queue<string>();
        private Queue<string> m_queue_of_single_side_reported_data_for_AI = new Queue<string>();
        private Queue<KeyValuePair<HttpListenerContext, string>> m_queue_of_merge_info_request_for_AI = new Queue<KeyValuePair<HttpListenerContext, string>>();
        private Queue<string> m_queue_of_merge_reported_data_wait_send_to_recheck_station = new Queue<string>();
        private Queue<string> m_queue_of_transfer_reported_data_for_AI = new Queue<string>();
        private Queue<string> m_queue_of_recheck_station_results = new Queue<string>();

        public HttpListener m_webservice_listener;                 // WebService监听器
        private CancellationTokenSource m_cts;                         // 取消令牌
        private bool m_bIsWebServiceInited = false;                 // WebService是否已初始化

        // 插入数据最长耗时
        private int m_nMaxInsertDataTime = 0;

        // 插入数据最长耗时的时间点
        private string m_strMaxInsertDataTime = "";

        // FupandataTemp目录名，用于比对，减少查询目录是否存在的次数
        string m_strDirOfFupandataTemp = "";
        bool m_bInsertDataTimeExceeds = false;                                          // 插入数据耗时是否超过阈值

        // 构造函数
        public AVICommunication()
        {
            m_cancellationToken1 = new CancellationTokenSource();
            m_cancellationToken2 = new CancellationTokenSource();
            m_cancellationToken3 = new CancellationTokenSource();
            m_cancellationToken4 = new CancellationTokenSource();
            m_cancellationToken5 = new CancellationTokenSource();
            m_cancellationToken6 = new CancellationTokenSource();

            m_task1 = Task.Run(thread_handle_queue_of_reported_data);
            m_task2 = Task.Run(thread_handle_queue_of_single_side_reported_data_for_AI);
            m_task3 = Task.Run(thread_handle_queue_of_transfer_station_data_for_AI);
            m_task4 = Task.Run(thread_handle_queue_of_recheck_station_results);
            m_task5 = Task.Run(thread_handle_queue_of_merge_info_request_for_AI);
            m_task6 = Task.Run(thread_handle_queue_of_merge_data_wait_send_to_recheck_station);
        }

        // 主窗口关闭
        public void Exit()
        {
            // 关闭TCP服务器
            if (true == m_bIsTCPServerInited)
            {
                m_bIsTCPServerInited = false;

                // 如果有客户端连接，关闭连接
                if (m_connected_client != null)
                {
                    m_connected_client.Close();
                    m_connected_client = null;
                }

                // 如果m_TCP_listener正在监听，停止监听
                if (m_TCP_listener != null)
                {
                    m_TCP_listener.Stop();
                }
            }

            // 关闭WebService
            if (true == m_bIsWebServiceInited)
            {
                m_bIsWebServiceInited = false;

                // 取消监听
                m_cts.Cancel();

                // 停止监听
                m_webservice_listener.Stop();
                m_webservice_listener.Close();
            }
        }

        // 验证IP地址是否合法
        public static bool ValidateIPAddress(string ipAddress)
        {
            // Validate IP address
            IPAddress ip;
            bool isValidIP = IPAddress.TryParse(ipAddress, out ip);

            if (false == isValidIP)
            {
                // 判断是否是通配符地址
                if (ipAddress == "*" || ipAddress == "+")
                    isValidIP = true;
            }

            return isValidIP;
        }

        // 验证IP地址是否能ping通
        public static bool PingAddress(string ipAddress)
        {
            using (Ping pinger = new Ping())
            {
                try
                {
                    PingReply reply = pinger.Send(ipAddress, 100);
                    return reply.Status == IPStatus.Success;
                }
                catch (PingException)
                {
                    // Ping请求失败，通常是由于网络问题或IP地址错误
                    return false;
                }
            }
        }

        // 验证端口号是否合法
        public static bool ValidatePort(string portValue)
        {
            // Validate port
            int port;
            bool isValidPort = int.TryParse(portValue, out port) && port > 0 && port <= 65535;
            return isValidPort;
        }

        // 判断端口是否被占用
        public bool IsPortInUse(int port)
        {
            var listener = new HttpListener();
            try
            {
                // 尝试添加前缀并启动监听
                listener.Prefixes.Add($"http://*:{port}/");
                listener.Start();
                return false; // 如果没有异常发生，表示端口当前没有被监听
            }
            catch (HttpListenerException ex)
            {
                if (ex.ErrorCode == 183) // 183代表ERROR_ALREADY_EXISTS(端口已经被占用)
                {
                    return true; // 端口已经被占用
                }
                throw; // 其他类型的异常重新抛出
            }
            finally
            {
                // 停止监听并释放资源
                listener.Close();
            }
        }

        // 判断前缀是否被占用
        public bool IsPrefixInUse(string prefix)
        {
            var listener = new HttpListener();
            try
            {
                // 尝试添加前缀并启动监听
                listener.Prefixes.Add(prefix);
                listener.Start();
                return false; // 如果没有异常发生，表示前缀当前没有被监听
            }
            catch (HttpListenerException ex)
            {
                // 根据异常类型判断前缀是否被占用
                // 通常错误码 5 表示拒绝访问，可能是权限问题
                // 错误码 183 表示项已存在，可能是前缀已被占用
                // 具体错误码可能会根据操作系统不同而有所差异
                return true; // 任何异常都假设前缀被占用（或者你可以根据需要处理不同的错误码）
            }
            finally
            {
                // 停止监听并释放资源
                listener.Close();
            }
        }

        // 初始化WebService服务端
        public void InitWebService(string strIP, int nPort)
        {
            if (true == m_bIsWebServiceInited)
            {
                return;
            }

            // 判断IP地址是否合法，端口号是否合法，如果不合法，返回
            if (false == ValidateIPAddress(strIP) || false == ValidatePort(nPort.ToString()))
            {
                m_bIsWebServiceInited = false;

                // 显示错误信息
                Debugger.Log(0, null, string.Format("222222 IP地址或端口号不合法。它们是：{0}:{1}", strIP, nPort));

                return;
            }

            // 创建WebService监听器
            m_webservice_listener = new HttpListener();

            // 添加监听地址
            if (Global.m_instance.m_station_type == "recheck")
            {
                m_webservice_listener.Prefixes.Add(Global.m_instance.m_AVI_data_collector_service_report_data);
                m_webservice_listener.Prefixes.Add(Global.m_instance.m_transfer_data_collector_service_report_data);
            }
            else if (Global.m_instance.m_station_type == "transfer")
            {
                m_webservice_listener.Prefixes.Add(Global.m_instance.m_AVI_data_collector_service_single_side_report_data_forAI);
                m_webservice_listener.Prefixes.Add(Global.m_instance.m_AVI_data_sender_service_merge_side_request_data_forAI);
            }
            m_webservice_listener.Prefixes.Add(Global.m_instance.m_AVI_data_collector_service_set_id);
            m_webservice_listener.Prefixes.Add(Global.m_instance.m_AVI_data_collector_service_avi_finish);
            m_webservice_listener.Prefixes.Add(Global.m_instance.m_AVI_data_collector_service_recheck_station_result);

            // 启动WebService监听器
            m_webservice_listener.Start();

            m_cts = new CancellationTokenSource();

            m_bIsWebServiceInited = true;

            Debugger.Log(0, null, string.Format("222222 HTTP WebService已启动"));
            Debugger.Log(0, null, string.Format("222222 URL1: {0}", Global.m_instance.m_AVI_data_collector_service_set_id));
            Debugger.Log(0, null, string.Format("222222 URL2: {0}", Global.m_instance.m_AVI_data_collector_service_report_data));
            Debugger.Log(0, null, string.Format("222222 URL3: {0}", Global.m_instance.m_AVI_data_collector_service_avi_finish));
            Debugger.Log(0, null, string.Format("222222 URL4: {0}", Global.m_instance.m_AVI_data_collector_service_recheck_station_result));

            // 异步开始监听请求
            Task.Run(() => ListenLoop(m_cts.Token), m_cts.Token);
        }

        // 发送日志到复判软件
        public void SendLogToRecheckServer(string strLog)
        {
            if (true)
            {
                ResponseFromRecheck reportData = new ResponseFromRecheck();

                reportData.error_code = 0;
                reportData.msg = strLog;

                string strDataToSend = System.Text.Json.JsonSerializer.Serialize(reportData);

                string strResponse = "";

                MesHelper.SendMES2(Global.m_instance.m_recheck_server_report_url, strDataToSend, ref strResponse, 1, 100);
            }
        }

        // 监听HTTP请求
        private async Task ListenLoop(CancellationToken ct)
        {
            try
            {
                while (!ct.IsCancellationRequested)
                {
                    Debugger.Log(0, null, string.Format("7777777 监听到HTTP请求"));

                    // 等待传入的请求
                    var context = m_webservice_listener.GetContext();

                    if (true == Global.m_instance.m_bExitProgram)
                        break;

                    // 获取请求信息
                    var request = context.Request;

                    string strIP = request.RemoteEndPoint.Address.ToString();

                    //Debugger.Log(0, null, string.Format("222222 111 HTTP请求来自IP {0}", strIP));

                    string msg = "";
                    if (Global.m_instance.m_station_type == "recheck")
                    {
                        if (false == Global.m_instance.m_dict_machine_names_and_IPs.Values.Contains(strIP))
                            msg = string.Format("HTTP请求来自IP {0}，不在机台清单上", request.RemoteEndPoint.Address.ToString());
                        else
                        {
                            string strMachineName = Global.m_instance.m_dict_machine_names_and_IPs.FirstOrDefault(x => x.Value == strIP).Key;

                            msg = string.Format("HTTP请求来自IP {0}，对应机台号 {1}", request.RemoteEndPoint.Address.ToString(), strMachineName);
                        }

                        SendLogToRecheckServer(msg);
                    }

                    Debugger.Log(0, null, string.Format("222222 {0}", msg));

                    //return;

                    try
                    {
                        // 获取端口号后面部分的字符串
                        string strHttpPrefixForGetSetId = Global.m_instance.m_AVI_data_collector_service_set_id.Substring(Global.m_instance.m_AVI_data_collector_service_set_id.LastIndexOf(':') + 1);
                        strHttpPrefixForGetSetId = strHttpPrefixForGetSetId.Substring(strHttpPrefixForGetSetId.IndexOf('/') + 1);

                        string strHttpPrefixForListenAVIData = Global.m_instance.m_AVI_data_collector_service_report_data.Substring(Global.m_instance.m_AVI_data_collector_service_report_data.LastIndexOf(':') + 1);
                        strHttpPrefixForListenAVIData = strHttpPrefixForListenAVIData.Substring(strHttpPrefixForListenAVIData.IndexOf('/') + 1);

                        string strHttpPrefixForListenAVIAIData = Global.m_instance.m_AVI_data_collector_service_single_side_report_data_forAI.Substring(Global.m_instance.m_AVI_data_collector_service_single_side_report_data_forAI.LastIndexOf(':') + 1);
                        strHttpPrefixForListenAVIAIData = strHttpPrefixForListenAVIAIData.Substring(strHttpPrefixForListenAVIAIData.IndexOf('/') + 1);

                        string strHttpPrefixForListenAVIFinishSingnal = Global.m_instance.m_AVI_data_collector_service_avi_finish.Substring(Global.m_instance.m_AVI_data_collector_service_avi_finish.LastIndexOf(":") + 1);
                        strHttpPrefixForListenAVIFinishSingnal = strHttpPrefixForListenAVIFinishSingnal.Substring(strHttpPrefixForListenAVIFinishSingnal.IndexOf("/") + 1);

                        string strHttpPrefixForListenRecheckResult = Global.m_instance.m_AVI_data_collector_service_recheck_station_result.Substring(Global.m_instance.m_AVI_data_collector_service_recheck_station_result.LastIndexOf(':') + 1);
                        strHttpPrefixForListenRecheckResult = strHttpPrefixForListenRecheckResult.Substring(strHttpPrefixForListenRecheckResult.IndexOf('/') + 1);

                        string strHttpPrefixForAVIRequestAIMergeData = Global.m_instance.m_AVI_data_sender_service_merge_side_request_data_forAI.Substring(Global.m_instance.m_AVI_data_sender_service_merge_side_request_data_forAI.LastIndexOf(':') + 1);
                        strHttpPrefixForAVIRequestAIMergeData = strHttpPrefixForAVIRequestAIMergeData.Substring(strHttpPrefixForAVIRequestAIMergeData.IndexOf('/') + 1);

                        string strHttpPrefixForListenTransferStationAIResult = Global.m_instance.m_transfer_data_collector_service_report_data.Substring(Global.m_instance.m_transfer_data_collector_service_report_data.LastIndexOf(':') + 1);
                        strHttpPrefixForListenTransferStationAIResult = strHttpPrefixForListenTransferStationAIResult.Substring(strHttpPrefixForListenTransferStationAIResult.IndexOf("/") + 1);

                        // 检查URL和HTTP方法，以决定如何处理请求
                        if (request.Url.AbsolutePath.EndsWith(string.Format("{0}", strHttpPrefixForGetSetId)) && request.HttpMethod == "GET")
                        {
                            // 返回数据
                            if (true)
                            {
                                AquireData aquireData = new AquireData();

                                // aquireData成员变量赋值
                                aquireData.uuid = Guid.NewGuid().ToString();
                                aquireData.product_id = "";
                                aquireData.set_id = DateTime.Now.ToString("yyyyMMddHHmmss");

                                MESAquireSetId mESAquireSetId = new MESAquireSetId();

                                mESAquireSetId.data = aquireData;
                                mESAquireSetId.err_code = "0";

                                // 序列化Person对象为JSON字符串
                                string jsonResponse = System.Text.Json.JsonSerializer.Serialize(mESAquireSetId);

                                SendResponseToClient(context, jsonResponse);
                            }
                        }
                        else if (request.Url.AbsolutePath.EndsWith(string.Format("{0}", strHttpPrefixForAVIRequestAIMergeData)) && request.HttpMethod == "POST")
                        {
                            using (var reader = new StreamReader(context.Request.InputStream))
                            {
                                string info = reader.ReadToEnd();
                                Debugger.Log(0, null, string.Format("88888888 读取AVI的请求信息：") + info);

                                m_queue_of_merge_info_request_for_AI.Enqueue(new KeyValuePair<HttpListenerContext, string>(context, info));
                            }
                        }
                        else if (request.Url.AbsolutePath.EndsWith(string.Format("{0}", strHttpPrefixForListenAVIData)) && request.HttpMethod == "POST")
                        {
                            // 读取并反序列化请求中的JSON数据，对应类为ProductInfo
                            using (var reader = new System.IO.StreamReader(context.Request.InputStream))
                            {
                                Debugger.Log(0, null, string.Format("7777777 读取HTTP请求内容"));

                                string info = reader.ReadToEnd();

                                Debugger.Log(0, null, string.Format("7777777 HTTP请求内容开始入队"));

                                if (info.Length > 50)
                                    m_queue_of_reported_data.Enqueue(info);

                                Debugger.Log(0, null, string.Format("7777777 HTTP请求内容结束入队"));

                                // 返回数据
                                if (true)
                                {
                                    if (info.Length > 50)
                                        SendResponseToClient(context);
                                    else
                                        SendResponseToClient(context, false);

                                    Debugger.Log(0, null, string.Format("7777777 返回HTTP结果"));
                                }

                                // 保存请求中的JSON数据到log文件夹下
                                if (true)
                                {
                                    // 在当前目录下创建log文件夹
                                    if (false == Directory.Exists(Global.m_instance.m_strCurrentDirectory + "log"))
                                        Directory.CreateDirectory(Global.m_instance.m_strCurrentDirectory + "log");

                                    // 创建以年_月_日命名的文本文件，在log文件夹下，如果存在则追加，不存在则创建
                                    string strFileName = string.Format("{0}log\\{1}.txt", Global.m_instance.m_strCurrentDirectory, DateTime.Now.ToString("yyyy_MM_dd"));

                                    using (StreamWriter sw = new StreamWriter(strFileName, true))
                                    {
                                        // 写入当前时间，精确到毫秒
                                        sw.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));

                                        sw.WriteLine(info);

                                        // 写入空行
                                        sw.WriteLine();
                                        sw.WriteLine();
                                        sw.Close();
                                    }
                                }
                            }
                        }
                        else if (request.Url.AbsolutePath.EndsWith(string.Format("{0}", strHttpPrefixForListenAVIAIData)) && request.HttpMethod == "POST")
                        {
                            // 读取并反序列化请求中的JSON数据，对应类为ProductInfo
                            using (var reader = new System.IO.StreamReader(context.Request.InputStream))
                            {
                                string info = reader.ReadToEnd();
                                m_queue_of_single_side_reported_data_for_AI.Enqueue(info);

                                // 返回数据
                                SendResponseToClient(context);
                            }
                        }
                        else if (request.Url.AbsolutePath.EndsWith(string.Format("{0}", strHttpPrefixForListenAVIFinishSingnal)) && request.HttpMethod == "POST")
                        {
                            using (var reader = new System.IO.StreamReader(context.Request.InputStream))
                            {
                                string info = reader.ReadToEnd();

                                var temp = JsonConvert.DeserializeObject<AVIFinishInfo>(info);

                                if (Global.m_instance.m_nRecheckModeWithAISystem == 3 && Convert.ToBoolean(temp.isFinished))
                                {
                                    var dictionary = @"D:\fupanAutoAIData\WaitToRecheck";
                                    if (!Directory.Exists(dictionary))
                                        Directory.CreateDirectory(dictionary);

                                    var fileName = $"{temp.barcode}.bcd";

                                    string fullPath = Path.Combine(dictionary, fileName);
                                    if (!File.Exists(fullPath))
                                    {
                                        File.Create(fullPath).Close();
                                    }
                                    else
                                    {
                                        File.Delete(fullPath);
                                        File.Create(fullPath).Close();
                                    }
                                    File.WriteAllText(fullPath, fileName);
                                }

                                // 返回数据
                                if (true)
                                {
                                    SendResponseToClient(context);
                                }
                            }

                        }
                        else if (request.Url.AbsolutePath.EndsWith(string.Format("{0}", strHttpPrefixForListenRecheckResult)) && request.HttpMethod == "POST")
                        {
                            // 读取并反序列化请求中的JSON数据，对应类为RecheckStationResult
                            using (var reader = new System.IO.StreamReader(context.Request.InputStream))
                            {
                                string info = reader.ReadToEnd();

                                m_queue_of_recheck_station_results.Enqueue(info);

                                // 返回数据
                                if (true)
                                {
                                    SendResponseToClient(context);
                                }
                            }
                        }
                        else if (request.Url.AbsolutePath.EndsWith(string.Format("{0}", strHttpPrefixForListenTransferStationAIResult)) && request.HttpMethod == "POST")
                        {
                            // 读取并反序列化请求中的JSON数据，对应类为ProductInfo
                            using (var reader = new System.IO.StreamReader(context.Request.InputStream))
                            {
                                string info = reader.ReadToEnd();
                                // 解析中转站发来的json数据，应该用MergeAIInfo
                                m_queue_of_transfer_reported_data_for_AI.Enqueue(info);

                                // 返回数据
                                if (true)
                                {
                                    SendResponseToClient(context);
                                }
                            }
                        }
                        else
                        {
                            // 处理其他请求
                        }
                    }
                    catch (Exception ex)
                    {
                        Debugger.Log(0, null, string.Format("222222 处理请求失败。错误信息：{0}", ex.Message));

                        if (true)
                        {
                            msg = string.Format("处理请求失败。错误信息：{0}", ex.Message);

                            SendLogToRecheckServer(msg);
                        }
                    }
                }
            }
            catch (ObjectDisposedException)
            {
                // HttpListener被关闭时会抛出此异常
                // 无需处理，只需退出循环即可
            }
        }

        // 发送响应给客户端
        private bool SendResponseToClient(HttpListenerContext http_context, string jsonResponse)
        {
            try
            {
                // 设置响应头为application/json表示我们发送的是JSON格式的数据
                var response = http_context.Response;
                response.ContentType = "application/json";
                response.ContentEncoding = Encoding.UTF8;
                response.StatusCode = (int)HttpStatusCode.OK; // 设置状态码为200

                // 将JSON字符串转换为字节流
                byte[] buffer = Encoding.UTF8.GetBytes(jsonResponse);

                // 设置响应的内容长度
                response.ContentLength64 = buffer.Length;

                // 写入响应流
                using (Stream output = response.OutputStream)
                {
                    output.Write(buffer, 0, buffer.Length);
                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        // 发送响应给客户端
        private bool SendResponseToClient(HttpListenerContext http_context, bool bIsValidData = true)
        {
            try
            {
                ResponseFromRecheck result = new ResponseFromRecheck();

                result.error_code = 0;

                if (true == bIsValidData)
                    result.msg = "OK";
                else
                    result.msg = "Error";

                // 序列化response对象为JSON字符串
                string jsonResponse = System.Text.Json.JsonSerializer.Serialize(result);

                // 设置响应头为application/json表示我们发送的是JSON格式的数据
                var response = http_context.Response;
                response.ContentType = "application/json";
                response.ContentEncoding = Encoding.UTF8;
                response.StatusCode = (int)HttpStatusCode.OK; // 设置状态码为200

                // 将JSON字符串转换为字节流
                byte[] buffer = Encoding.UTF8.GetBytes(jsonResponse);

                // 设置响应的内容长度
                response.ContentLength64 = buffer.Length;

                // 写入响应流
                using (Stream output = response.OutputStream)
                {
                    output.Write(buffer, 0, buffer.Length);
                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        // 处理avi检测结果消息队列
        private void thread_handle_queue_of_reported_data()
        {
            while (!m_cancellationToken1.IsCancellationRequested)
            {
                if (Global.m_instance.m_bExitProgram)
                    break;

                if (m_queue_of_reported_data?.Count > 0)
                {
                    Debugger.Log(0, null, string.Format("7777777 HTTP请求内容开始出队"));
                    string info = m_queue_of_reported_data.Dequeue();
                    Debugger.Log(0, null, string.Format("7777777 HTTP请求内容结束出队"));

                    Debugger.Log(0, null, string.Format("7777777 HTTP请求内容结束出队, info.Length = {0}", info.Length));

                    if (Global.m_instance.m_station_type == "recheck")
                    {
                        handle_reported_data_for_recheck_station(info);
                        GC.Collect();
                    }
                    else
                        handle_reported_data_for_FQC_station(info);

                    Thread.Sleep(100);
                }
                else
                {
                    Thread.Sleep(100);
                }
            }
        }

        // 处理提交到AI的单面信息队列
        private void thread_handle_queue_of_single_side_reported_data_for_AI()
        {
            while (!m_cancellationToken2.IsCancellationRequested)
            {
                if (Global.m_instance.m_bExitProgram)
                    break;

                if (m_queue_of_single_side_reported_data_for_AI?.Count > 0)
                {
                    string info = m_queue_of_single_side_reported_data_for_AI.Dequeue();

                    handle_single_side_reported_data_for_AI(info);

                    Thread.Sleep(100);
                }
                else
                {
                    Thread.Sleep(100);
                }
            }
        }

        private void thread_handle_queue_of_transfer_station_data_for_AI()
        {
            while (!m_cancellationToken3.IsCancellationRequested)
            {
                if (Global.m_instance.m_bExitProgram)
                    return;

                if (m_queue_of_transfer_reported_data_for_AI?.Count > 0)
                {
                    string info = m_queue_of_transfer_reported_data_for_AI.Dequeue();

                    handle_transfer_station_data_for_AI(info);

                    Thread.Sleep(100);
                }
                else
                {
                    Thread.Sleep(100);
                }
            }
        }

        public void thread_avi_data_wait_send_to_recheck_folder_monitor()
        {
            try
            {
                while (true)
                {
                    Debugger.Log(0, null, "数据重发机制循环运行");

                    var fupanAddress = new List<string>(){
                    "http://10.104.77.37:8080/vml/dcs/report_data/",
                    "http://10.104.76.113:8080/vml/dcs/report_data/",
                    "http://10.104.77.45:8080/vml/dcs/report_data/",
                    "http://10.104.77.66:8080/vml/dcs/report_data/",
                    "http://10.104.76.52:8080/vml/dcs/report_data/",
                    "http://10.104.76.219:8080/vml/dcs/report_data/"};

                    for (int i = 0; i < fupanAddress.Count; i++)
                    {
                        // 遍历D:\fupandata目录下的所有txt文件
                        if (true)
                        {
                            string logpath = @"C:\算法\复判数据日志\" + DateTime.Now.ToString("yyyyMMdd") + "\\fupandata" + i + 1;
                            if (false == Directory.Exists(logpath))
                                Directory.CreateDirectory(logpath);

                            string[] files = { };

                            if (i == 0) files = Directory.GetFiles(@"D:\fupandata1", "*.txt");
                            else if (i == 1) files = Directory.GetFiles(@"D:\fupandata2", "*.txt");
                            else if (i == 2) files = Directory.GetFiles(@"D:\fupandata3", "*.txt");
                            else if (i == 3) files = Directory.GetFiles(@"D:\fupandata4", "*.txt");
                            else if (i == 4) files = Directory.GetFiles(@"D:\fupandata5", "*.txt");
                            else if (i == 5) files = Directory.GetFiles(@"D:\fupandata6", "*.txt");

                            for (int n = 0; n < files.Length; n++)
                            {
                                if (n > 20)
                                    break;

                                string file = files[n];
                                string strLogName = logpath + "\\" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + "_复判Json_读取日志.txt";

                                // 读取文件内容
                                string content = File.ReadAllText(file);

                                // 判断内容是否为json格式
                                if (true)
                                {
                                    bool bIsJson = false;

                                    try
                                    {
                                        if (content.StartsWith("{\"set_id\""))
                                            bIsJson = true;
                                    }
                                    catch
                                    {
                                        bIsJson = false;
                                    }

                                    Debugger.Log(0, null, string.Format("666666 n {0}: 文件 {1} 是否为json格式 bIsJson = {2}", n, file, bIsJson));

                                    // 如果是json格式，发送content给复判软件
                                    if (bIsJson)
                                    {
                                        Debugger.Log(0, null, string.Format("666666 n {0}: 将文件 {1} 的内容 发送给复判软件, bIsJson = {2}", n, file, content));

                                        string info1 = string.Format("{3} fupandata {0}: 将文件 {1} 的内容 发送给复判软件, bIsJson = {2}", n + 1, file, content, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                                        File.WriteAllText(strLogName, info1);
                                        // 发送content给复判软件
                                        string strMesServerResponse = "";
                                        string strRecheckURL_1 = fupanAddress[i];
                                        bool bRet_1 = MesHelper.SendMES(strRecheckURL_1, content, ref strMesServerResponse, 1);
                                        string info2 = "";

                                        if (true == bRet_1)
                                        {
                                            ResponseFromRecheck response_1 = JsonConvert.DeserializeObject<ResponseFromRecheck>(strMesServerResponse);

                                            Debugger.Log(0, null, string.Format("666666 n {0}: 复判软件1接收数据返回 response_1.msg = {1}", n, response_1.msg));

                                            if (response_1.error_code == 0 && response_1.msg == "OK")
                                            {
                                                // 如果发送成功，删除文件
                                                {
                                                    File.Delete(file);
                                                }
                                                info2 = info1 + "\r\n" + "-------发送成功-----------";
                                                File.WriteAllText(strLogName, info2);

                                            }
                                            else
                                            {
                                                info2 = info1 + "\r\n" + "-------发送异常-------";
                                                File.WriteAllText(strLogName, info2);
                                            }
                                        }
                                        else
                                        {
                                            info2 = info1 + "\r\n" + "-------连接异常-------";
                                            File.WriteAllText(strLogName, info2);
                                        }

                                        Debugger.Log(0, null, string.Format("重发机制n {0}: 文件 {1} 的内容 发送给复判软件 结果 bRet_1 = {2}", n, file, bRet_1));
                                    }
                                    else
                                    {
                                        // 如果不是json格式，删除文件
                                        File.Delete(file);
                                    }
                                }
                            }
                        }
                    }

                    Thread.Sleep(10000);
                }
            }
            catch (Exception ex)
            {
                Debugger.Log(0, null, "数据重发机制运行异常：" + ex.Message);
            }
        }

        // 处理复判结果消息队列
        private async void thread_handle_queue_of_recheck_station_results()
        {
            while (!m_cancellationToken4.IsCancellationRequested)
            {
                if (Global.m_instance.m_bExitProgram)
                    break;

                if (m_queue_of_recheck_station_results?.Count > 0)
                {
                    string info = m_queue_of_recheck_station_results.Dequeue();

                    handle_recheck_station_result(info);

                    Thread.Sleep(100);
                }
                else
                {
                    Thread.Sleep(100);
                }
            }
        }

        private void thread_handle_queue_of_merge_info_request_for_AI()
        {
            while (!m_cancellationToken5.IsCancellationRequested)
            {
                if (Global.m_instance.m_bExitProgram)
                    break;

                if (m_queue_of_merge_info_request_for_AI?.Count > 0)
                {
                    Debugger.Log(0, null, string.Format("88888888 Merge信息队列不为空，触发出队"));
                    var kvp = m_queue_of_merge_info_request_for_AI.Dequeue();

                    handle_merge_info_request_for_AI(kvp);

                    Thread.Sleep(100);
                }
                else
                {
                    Thread.Sleep(100);
                }
            }
        }

        private void thread_handle_queue_of_merge_data_wait_send_to_recheck_station()
        {
            while (!m_cancellationToken6.IsCancellationRequested)
            {
                if (Global.m_instance.m_bExitProgram)
                    break;

                if (m_queue_of_merge_reported_data_wait_send_to_recheck_station?.Count > 0)
                {
                    Debugger.Log(0, null, string.Format("88888888 待发送给复判站的汇总结果信息队列不为空，触发出队"));
                    var data = m_queue_of_merge_reported_data_wait_send_to_recheck_station.Dequeue();

                    TimeSpan ts = new TimeSpan(0, 0, 0);
                    Stopwatch stopwatch = new Stopwatch();

                    // 开始计时
                    stopwatch.Start();
                    SendAIInfoToRecheckStation(JsonConvert.DeserializeObject<MesAIMergeAIInfo>(data));
                    stopwatch.Stop();
                    // 获取运行时间
                    ts = stopwatch.Elapsed;
                    Debugger.Log(0, null, string.Format("88888888 向所有复判站发送汇总结果结束，总耗时：") + ts);

                    Thread.Sleep(100);
                }
                else
                {
                    Thread.Sleep(100);
                }
            }
        }

        // 处理传入FQC站的数据
        public void handle_reported_data_for_FQC_station(object obj)
        {
            string info = (string)obj;

            try
            {
                // 如果D:\fupandata目录不存在，创建它
                if (false == Directory.Exists(@"D:\fqcdata"))
                    Directory.CreateDirectory(@"D:\fqcdata");

                string strFileName = string.Format("D:\\fqcdata\\incoming_data_{0}.txt", DateTime.Now.ToString("yyyyMMddHHmmss"));
                File.WriteAllText(strFileName, info);

                // 序列化JSON数据
                var trayInfo = FqcJsonSerializerHelper.DeserializeProductInfo(info);

                int nStartTime = GetTickCount();

                InsertTrayInfo(trayInfo);

                int nEndTime = GetTickCount();

                string msg = string.Format("接收处理AVI数据耗时：{0:0.000}秒", (double)(nEndTime - nStartTime) / 1000.0f);

                SendLogToRecheckServer(msg);
            }
            catch (Exception ex)
            {
                Debugger.Log(0, null, string.Format("处理传入FQC站的数据失败。错误信息：{0}", ex.Message));

                if (true)
                {
                    string msg = string.Format("处理传入FQC站的数据失败。错误信息：{0}", ex.Message);

                    SendLogToRecheckServer(msg);
                }
            }
        }

        // 处理传入复判站的数据
        public void handle_reported_data_for_recheck_station(object obj)
        {
            Debugger.Log(0, null, string.Format("7777777 开始处理队列中的AVI信息"));

            string info = (string)obj;

            int nStartTime = GetTickCount();

            try
            {
                Debugger.Log(0, null, string.Format("7777777 开始处理队列中的AVI信息 111"));

                // 如果D:\fupandata目录不存在，创建它
                if (false == Directory.Exists(@"D:\fupandata"))
                    Directory.CreateDirectory(@"D:\fupandata");
                if (false == Directory.Exists(@"D:\fupandata_temp"))
                    Directory.CreateDirectory(@"D:\fupandata_temp");

                Debugger.Log(0, null, string.Format("7777777 开始处理队列中的AVI信息 222"));

                // 保存一份副本，以免序列化失败
                if (true)
                {
                    // 创建年月日的目录
                    string strDir = $@"D:\fupandata_temp\{DateTime.Now:yyyyMMdd}";

                    Debugger.Log(0, null, string.Format("7777777 开始处理队列中的AVI信息 222 aaa"));

                    // 如果目录和之前成功创建的目录不一样，说明目录已经被删除，需要重新创建
                    if (m_strDirOfFupandataTemp != strDir)
                    {
                        if (false == Directory.Exists(strDir))
                            Directory.CreateDirectory(strDir);

                        Debugger.Log(0, null, string.Format("7777777 开始处理队列中的AVI信息 222 bbb"));

                        if (true == Directory.Exists(strDir))
                        {
                            m_strDirOfFupandataTemp = strDir;
                        }
                    }

                    Debugger.Log(0, null, string.Format("7777777 开始处理队列中的AVI信息 333"));

                    // 将info保存到文件，时间戳为年月日时分秒毫秒
                    string filepath = $@"{strDir}\incoming_data_{DateTime.Now:yyyyMMddHHmmssfff}.txt";

                    Debugger.Log(0, null, string.Format("7777777 开始处理队列中的AVI信息 555 filepath = {0}, length = {1}", filepath, info.Length));

                    try
                    {
                        File.WriteAllText(filepath, info);
                    }
                    catch (Exception ex)
                    {
                        Debugger.Log(0, null, string.Format("7777777 写入文件失败。错误信息：{0}", ex.Message));

                        Directory.CreateDirectory(strDir);

                        if (true == Directory.Exists(strDir))
                        {
                            m_strDirOfFupandataTemp = strDir;
                        }

                        File.WriteAllText(filepath, info);
                    }

                    Debugger.Log(0, null, string.Format("7777777 创建并保存AVI信息副本"));
                }

                // 序列化JSON数据
                switch (Global.m_instance.m_strProductType)
                {
                    case "nova":
                        if (true)
                        {
                            // 将info保存到文件
                            string filepath = $@"D:\fupandata\incoming_data_{DateTime.Now:yyyyMMddHHmmss}.txt";
                            File.WriteAllText(filepath, info);

                            AVITrayInfo tray_info = JsonConvert.DeserializeObject<AVITrayInfo>(info);

                            var model = JsonSerializerHelper.DeserializeProductInfo(info);

                            Global.m_instance.m_background_tray_info_for_Nova = model;

                            // 处理AVI产品信息
                            try
                            {
                                if (true == Global.m_instance.m_tray_info_service.HandleTrayInfoFromAVIForNova_DatabaseAndImagePathStorage(Global.m_instance.m_background_tray_info_for_Nova))
                                    ;

                                string msg = "";

                                // 一次NG数据上传
                                if (Global.m_instance.m_bEnableOriginalNGDataUploadToMES == true)
                                {
                                    Debugger.Log(0, null, string.Format("222222 进行一次NG数据上传"));

                                    msg = string.Format("进行一次NG数据上传");
                                    SendLogToRecheckServer(msg);

                                    if (true == Global.m_instance.m_tray_info_service.HandleTrayInfoFromAVIForNova_SubmitOriginalNGProductsDataToMES(Global.m_instance.m_background_tray_info_for_Nova))
                                    {
                                        Debugger.Log(0, null, string.Format("222222 一次NG数据上传成功"));

                                        msg = string.Format("一次NG数据上传成功");
                                        SendLogToRecheckServer(msg);
                                    }
                                    else
                                    {
                                        Debugger.Log(0, null, string.Format("222222 一次NG数据上传失败"));

                                        msg = string.Format("一次NG数据上传失败");
                                        SendLogToRecheckServer(msg);
                                    }
                                }

                                //if (true == m_parent.m_global.m_tray_info_service.HandleTrayInfoFromAVI(m_parent.m_global.m_background_tray_info))
                                //    m_parent.m_global.m_tray_info_service.HandleTrayInfoFromAVI2(m_parent.m_global.m_background_tray_info);
                            }
                            catch (Exception ex)
                            {
                                Debugger.Log(0, null, string.Format("7777777 后台处理AVI产品信息失败。错误信息：{0}", ex.Message));

                                if (true)
                                {
                                    string msg = string.Format("后台处理AVI产品信息失败。错误信息：{0}", ex.Message);

                                    SendLogToRecheckServer(msg);
                                }
                            }
                        }
                        break;

                    case "dock":
                    case "cl5":
                        if (true)
                        {
                            TrayInfo tray_info = JsonConvert.DeserializeObject<TrayInfo>(info);
                            Debugger.Log(0, null, string.Format("7777777 反序列化AVI信息为实体类"));

                            Global.m_instance.m_background_tray_info_for_Dock = tray_info;

                            int nTotalPcs = Global.m_instance.m_background_tray_info_for_Dock.total_columns * Global.m_instance.m_background_tray_info_for_Dock.total_rows;

                            // 如果m_parent.m_global.m_background_tray_info.product数量超过m_parent.m_global.m_background_tray_info.total_pcs，说明有问题，需要把多出来的product删除
                            if (Global.m_instance.m_background_tray_info_for_Dock?.products?.Count > nTotalPcs)
                            {
                                Global.m_instance.m_background_tray_info_for_Dock.products.RemoveRange(nTotalPcs, Global.m_instance.m_background_tray_info_for_Dock.products.Count - nTotalPcs);
                            }

                            // 处理AVI产品信息
                            try
                            {
                                // 将info保存到文件
                                string filepath = $@"D:\fupandata\incoming_data_{DateTime.Now:yyyyMMddHHmmss}.txt";

                                if (false == string.IsNullOrEmpty(tray_info.machine_id))
                                {
                                    string strMachineID = tray_info.machine_id.Replace("-", "_");
                                    //string strBarcode = tray_info.products[0].Replace("-", "_");

                                    // 目录为"D:\fupandata"加上机器ID
                                    filepath = $@"D:\fupandata\{strMachineID}";

                                    if (false == Directory.Exists(filepath))
                                        Directory.CreateDirectory(filepath);

                                    string dir = $@"D:\fupandata\{strMachineID}\{tray_info.set_id}";

                                    if (false == Directory.Exists(dir))
                                        Directory.CreateDirectory(dir);

                                    //filepath = $@"{filepath}\incoming_data_{DateTime.Now:yyyyMMddHHmmss}.txt";
                                    filepath = $@"{dir}\incoming_data_{DateTime.Now:yyyyMMddHHmmss}.txt";
                                }

                                File.WriteAllText(filepath, info);
                                Debugger.Log(0, null, string.Format("7777777 创建并保存AVI信息到incomingdata"));

                                if (true == Global.m_instance.m_tray_info_service.HandleTrayInfoFromAVIForDockOrCL5_DatabaseAndImagePathStorage(Global.m_instance.m_background_tray_info_for_Dock, filepath))
                                    ;

                            }
                            catch (Exception ex)
                            {
                                Debugger.Log(0, null, string.Format("7777777 后台处理AVI产品信息失败。错误信息：{0}", ex.Message));

                                if (true)
                                {
                                    string msg = string.Format("后台处理AVI产品信息失败。错误信息：{0}", ex.Message);

                                    SendLogToRecheckServer(msg);
                                }
                            }
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                Debugger.Log(0, null, string.Format("处理传入复判站的数据失败。错误信息：{0}", ex.Message));

                if (true)
                {
                    string msg = string.Format("处理传入复判站的数据失败。错误信息：{0}", ex.Message);

                    SendLogToRecheckServer(msg);
                }
            }

            int nEndTime = GetTickCount();

            int nTime = nEndTime - nStartTime;

            if (nTime > m_nMaxInsertDataTime)
            {
                m_nMaxInsertDataTime = nTime;
                m_strMaxInsertDataTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                Debugger.Log(0, null, string.Format("7777777 插入数据最长耗时：{0}ms，时间点：{1}", nTime, m_strMaxInsertDataTime));

                // 如果超过2秒，发送重启后台的命令到复判软件
                if (nTime > 2000)
                {
                    m_bInsertDataTimeExceeds = true;

                    string msg = string.Format("插入数据最长耗时：{0}ms，时间点：{1}", nTime, m_strMaxInsertDataTime);

                    SendLogToRecheckServer(msg);
                }
            }

            if (true == m_bInsertDataTimeExceeds)
            {
                if (nTime < 600 && m_queue_of_reported_data?.Count <= 1)
                {
                    string msg = string.Format("插入数据最长耗时恢复正常：{0}ms，时间点：{1}", nTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                    SendLogToRecheckServer(msg);
                }
            }
        }

        // 处理要提交到AI的单面信息
        public void handle_single_side_reported_data_for_AI(object obj)
        {
            string info = (string)obj;

            try
            {
                TrayInfo tray_info = JsonConvert.DeserializeObject<TrayInfo>(info);

                Global.m_instance.m_background_tray_info_for_Dock = tray_info;

                try
                {
                    // 将info保存到文件
                    string filepath = "";

                    if (false == string.IsNullOrEmpty(tray_info.machine_id))
                    {
                        string strMachineID = tray_info.machine_id.Replace("-", "_");
                        string dir = $@"D:\fupanAIdata\{strMachineID}";

                        if (false == Directory.Exists(dir))
                            Directory.CreateDirectory(dir);

                        //filepath = $@"{filepath}\incoming_data_{DateTime.Now:yyyyMMddHHmmss}.txt";
                        filepath = $@"{dir}\incoming_data_{DateTime.Now:yyyyMMddHHmmss}.json";
                    }

                    File.WriteAllText(filepath, info);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("中转软件保存传入的AVI信息文本异常" + ex.Message);
                }

                if (true == Global.m_instance.m_tray_info_service.InferProductImagePath(Global.m_instance.m_background_tray_info_for_Dock))
                    ;

                var response = new MesAISingleSideAIInfo();

                if (true == Global.m_instance.m_mes_service.SubmitSingleSideTrayInfoToAI(Global.m_instance.m_background_tray_info_for_Dock, ref response))
                {
                }
                else
                {
                    SendLogToRecheckServer("与AI接口交互失败，aiFinalRes赋值为空");

                    response.aiFinalRes = "";
                }

                if (response.finalRes != "FAIL" || response.finalRes != "PASS")
                {
                    SendLogToRecheckServer("与AI接口交互的信息有误，交互结果：" + response.finalRes);
                }

                response.pointPosInfo.ForEach(camPos =>
                {
                    camPos.imgData = new List<string>();
                    camPos.pathChannel = new List<string>();
                });

                handleSingleSideAIInterfaceResponse(response);
            }
            catch (Exception ex)
            {
                MessageBox.Show("中转软件处理传入的AVI信息异常：" + ex.Message);
            }
        }

        public void handle_merge_info_request_for_AI(KeyValuePair<HttpListenerContext, string> kvp)
        {
            string info = kvp.Value;

            try
            {
                Debugger.Log(0, null, string.Format("88888888 开始处理Merge队列信息"));
                var singleSideInfo = new MesAISingleSideAIInfo() { pointPosInfo = new List<CamPos>() };
                var mergeInfo = JsonConvert.DeserializeObject<MesAIMergeAIInfo>(info);
                var barcode = mergeInfo.dmCode;
                var tableName = "transfer_aiInfo_" + mergeInfo.machine;
                tableName = tableName.Replace('-', '_');

                // 循环查询数据库中保存的单面数据，直到能查到三条才返回，超过两秒返回失败
                TimeSpan ts = new TimeSpan(0, 0, 0);
                Stopwatch stopwatch = new Stopwatch();

                // 开始计时
                stopwatch.Start();
                while (singleSideInfo.pointPosInfo.Count < 3)
                {
                    Debugger.Log(0, null, string.Format("88888888 开始查询数据库中相关的SingleSide信息") + barcode);
                    singleSideInfo = Global.m_instance.m_database_service.FindSingleSideAIInfoDataWithBarcode(tableName, barcode);
                    Debugger.Log(0, null, string.Format("88888888 结束查询数据库中相关的SingleSide信息") + barcode);

                    // 停止计时
                    stopwatch.Stop();
                    // 获取运行时间
                    ts = stopwatch.Elapsed;

                    stopwatch.Start();

                    if (ts.TotalSeconds > 2)
                    {
                        Debugger.Log(0, null, string.Format("88888888 查询数据库中相关的SingleSide信息超时,用时：") + ts.TotalSeconds);
                        singleSideInfo.aiFinalRes = "";
                        break;
                    }
                }

                // 查询完成后提交合并信息
                mergeInfo.aiFinalRes = singleSideInfo.aiFinalRes;
                if(String.IsNullOrEmpty( mergeInfo.aiFinalRes))
                {
                    Debugger.Log(0, null, string.Format("88888888 汇总信息最后的aiFinalRes为空，出现异常"));
                }

                mergeInfo.pointPosInfo = singleSideInfo.pointPosInfo;
                var result = JsonConvert.SerializeObject(mergeInfo);
                Debugger.Log(0, null, string.Format("88888888 给AVI请求返回汇总结果：") + result);
                SendResponseToClient(kvp.Key, result);

                Debugger.Log(0, null, string.Format("88888888 向AI接口提交汇总结果：") + result);
                Global.m_instance.m_mes_service.SubmitMergeTrayInfoToAI(mergeInfo);

                //Debugger.Log(0, null, string.Format("88888888 向复判站发送汇总结果：") + result);
                //向复判站发送结果是否会出现超时，导致处理队列阻塞？改为单独的队列处理向复判站发送
                //SendAIInfoToRecheckStation(mergeInfo);
                m_queue_of_merge_reported_data_wait_send_to_recheck_station.Enqueue(result);
                Debugger.Log(0, null, string.Format("88888888 结束处理Merge队列信息"));
            }
            catch (Exception ex)
            {
                MessageBox.Show("中转软件处理传入的合并AI请求异常：" + ex.Message);
            }
        }

        public void handle_transfer_station_data_for_AI(object obj)
        {
            string info = (string)obj;

            try
            {
                var aiInfo = JsonConvert.DeserializeObject<MesAIMergeAIInfo>(info);

                try
                {
                    // 将info保存到文件
                    string filePath = "";

                    if (false == String.IsNullOrEmpty(aiInfo.machine) && false == String.IsNullOrEmpty(aiInfo.dmCode))
                    {
                        string strMachineID = aiInfo.machine.Replace("-", "_");
                        string dir = $@"D:\fupanAIdata\mergeInfo\{strMachineID}";
                        filePath = $@"D:\fupanAIdata\mergeInfo\{strMachineID}\{aiInfo.dmCode}.json";

                        if (false == Directory.Exists(dir))
                            Directory.CreateDirectory(dir);
                    }

                    File.WriteAllText(filePath, info);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("中转软件保存传入的合并AI信息文本异常" + ex.Message);
                }

                var tableName = "transfer_aiInfo_" + aiInfo.machine.Replace('-', '_');
                Global.m_instance.m_database_service.InsertMergeSideInfoToAIData(tableName, aiInfo);
            }
            catch (Exception ex)
            {
                MessageBox.Show("中转软件保存传入的合并AI信息异常" + ex.Message);
            }
        }

        private void handleSingleSideAIInterfaceResponse(MesAISingleSideAIInfo response)
        {
            try
            {
                var side = "";
                if (response.pointPosInfo != null && response.pointPosInfo.Count != 0)
                {
                    side = response.pointPosInfo.First().side;
                }
                var tableName = "transfer_aiInfo_" + response.machine.Replace('-', '_');

                //Global.m_instance.m_database_service.DeleteSingleSideAIInfoDataByUUid(tableName, side, response.tray);
                Global.m_instance.m_database_service.InsertSingleSideInfoToAIData(tableName, side, response);
            }
            catch (Exception ex)
            {
                MessageBox.Show("中转软件插入单面接口返回信息到数据库时遇到异常：" + ex.Message + "。返回值为：" + JsonConvert.SerializeObject(response));
            }
        }

        // 处理复判结果
        public void handle_recheck_station_result(string info)
        {
            try
            {
                // 如果D:\fupandata目录不存在，创建它
                if (false == Directory.Exists(@"D:\fupandata"))
                    Directory.CreateDirectory(@"D:\fupandata");

                // 创建recheck_station_result目录
                if (false == Directory.Exists(@"D:\fupandata\recheck_station_result"))
                    Directory.CreateDirectory(@"D:\fupandata\recheck_station_result");

                // 序列化JSON数据
                switch (Global.m_instance.m_strProductType)
                {
                    case "nova":
                        if (true)
                        {
                            // 将info保存到文件
                            string filepath = $@"D:\fupandata\recheck_station_result\recheck_station_result_{DateTime.Now:yyyyMMddHHmmss}.txt";
                            File.WriteAllText(filepath, info);

                            // 处理复判结果信息
                            try
                            {
                                int nStartTime = GetTickCount();

                                AVITrayInfo tray_info = JsonConvert.DeserializeObject<AVITrayInfo>(info);

                                for (int i = 0; i < tray_info.Products.Count; i++)
                                {
                                    Product product = tray_info.Products[i];

                                    string strBarcode = product.BarCode;
                                    string strRecheckResult = product.RecheckResult;

                                    if (false == string.IsNullOrEmpty(strBarcode) && false == string.IsNullOrEmpty(strRecheckResult))
                                    {
                                        RecheckResult recheckResult = new RecheckResult();

                                        recheckResult.set_id = tray_info.SetId;
                                        recheckResult.barcode = strBarcode;
                                        recheckResult.recheck_result = strRecheckResult;

                                        // 保存到复判结果数据库
                                        string strDatabase = "recheck_station_result";

                                        string insertOrUpdateSql = @"
                                            INSERT INTO recheck_station_result (set_id, barcode, recheck_result, r1, r2, r3, r4, r5, r6, r7, r8, r9, r10)
                                            VALUES (@set_id, @barcode, @recheck_result, @r1, @r2, @r3, @r4, @r5, @r6, @r7, @r8, @r9, @r10)
                                            ON DUPLICATE KEY UPDATE
                                            set_id = VALUES(set_id),
                                            recheck_result = VALUES(recheck_result),
                                            r1 = VALUES(r1),
                                            r2 = VALUES(r2),
                                            r3 = VALUES(r3),
                                            r4 = VALUES(r4),
                                            r5 = VALUES(r5),
                                            r6 = VALUES(r6),
                                            r7 = VALUES(r7),
                                            r8 = VALUES(r8),
                                            r9 = VALUES(r9),
                                            r10 = VALUES(r10);
                                            ";

                                        try
                                        {
                                            Global.m_instance.m_mysql_ops.AddRecheckResultToTable(strDatabase, insertOrUpdateSql, recheckResult);
                                        }
                                        catch (Exception ex)
                                        {
                                            Debugger.Log(0, null, string.Format("222222 数据库插入复判结果失败。错误信息：{0}", ex.Message));

                                            if (true)
                                            {
                                                string msg = string.Format("数据库插入复判结果失败。错误信息：{0}", ex.Message);

                                                SendLogToRecheckServer(msg);
                                            }
                                        }
                                    }
                                }

                                int nEndTime = GetTickCount();
                            }
                            catch (Exception ex)
                            {
                                Debugger.Log(0, null, string.Format("222222 处理复判结果失败。错误信息：{0}", ex.Message));

                                if (true)
                                {
                                    string msg = string.Format("处理复判结果失败。错误信息：{0}", ex.Message);

                                    SendLogToRecheckServer(msg);
                                }
                            }
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                Debugger.Log(0, null, string.Format("处理复判结果失败。错误信息：{0}", ex.Message));

                if (true)
                {
                    string msg = string.Format("处理复判结果失败。错误信息：{0}", ex.Message);

                    SendLogToRecheckServer(msg);
                }
            }
        }

        // 当前站别为中转站时，向复判后台发送AI汇总信息
        public bool SendAIInfoToRecheckStation(MesAIMergeAIInfo aiInfo)
        {
            foreach (var ip in Global.m_instance.m_dict_recheckPC_names_and_IPs.Values)
            {
                var url = $@"http://{ip}:8080/vml/dcs/report_data/transfer/";

                string json = JsonConvert.SerializeObject(aiInfo);

                string strResponse = "";
                if (true == MesHelper.SendMES(url, json, ref strResponse, 1, 2))
                {
                    //m_parent.m_global.m_log_presenter.Log(string.Format("向FQC看图电脑{0}发送复判结果成功，接收地址：{1}。", m_parent.m_global.m_dict_FQC_station_names_and_IPs.ElementAt(i).Key, strURL));
                }
                else
                {
                    //m_parent.m_global.m_log_presenter.Log(string.Format("向FQC看图电脑{0}发送复判结果失败，接收地址：{1}。", m_parent.m_global.m_dict_FQC_station_names_and_IPs.ElementAt(i).Key, strURL));
                }
            }

            return false;
        }

        // 插入物料盘信息
        public bool InsertTrayInfo(FqcTrayInfo tray_info)
        {
            try
            {
                string conectionstring = $"server=localhost;user=root;port=3306;password=123456;SslMode=None;database=AutoScanFQCTest;allowPublicKeyRetrieval=true;Pooling=true;Min Pool Size=5;Max Pool Size=50;";
                string strMachineName = tray_info.machine;
                if (string.IsNullOrEmpty(strMachineName))
                {
                    return false;
                }
                string strTrayTable = "trays_" + strMachineName;
                string strProductTable = "products_" + strMachineName;
                string strDefectTable = "details_" + strMachineName;
                strTrayTable = strTrayTable.Replace("-", "_");
                strProductTable = strProductTable.Replace("-", "_");
                strDefectTable = strDefectTable.Replace("-", "_");
                string strSetID = tray_info.machine + "_" + DateTime.Now.ToString("yyyyMMddHHmmss");//生成set_id

                // 加上毫秒
                strSetID += "_" + DateTime.Now.Millisecond.ToString();

                // 加上一个随机八位数
                Random random = new Random();
                strSetID += "_" + random.Next(10000000, 99999999).ToString();

                tray_info.SetId = strSetID;
                string oldSetid = string.Empty;
                int flag;//执行数据库 受影响的行数
                #region 获取旧setid
                string sql = "SELECT set_id FROM " + strProductTable + " WHERE panelId = '" + tray_info.pcsSummarys[0].panelId + "' limit 0,1;";
                DataTable data = MysqlHeplers.GetDataTable(conectionstring, CommandType.Text, sql);
                if (data?.Rows.Count > 0)
                {
                    oldSetid = data.Rows[0][0].ToString();
                }
                #endregion
                #region 物料盘表操作
                //删除物料盘表中重复数据
                if (!string.IsNullOrEmpty(oldSetid))
                {
                    sql = $"DELETE FROM " + strTrayTable + $" WHERE set_id = '{oldSetid}'";
                    flag = MysqlHeplers.ExecuteNonQuery(conectionstring, CommandType.Text, sql);
                    if (flag < 0)
                    {
                        string msg = string.Format($"{strTrayTable}表，数据删除失败！");

                        SendLogToRecheckServer(msg);

                        return false;
                    }
                }
                //插入物料盘数据
                sql = $@"INSERT INTO " + strTrayTable + @" (set_id, panel, resource, site, mac, programName, machine, product, workArea, testType, testTime, operatorName, operatorType, testMode, trackType, checkPcsDataForAVI, uuid, checkDetail, tableInfor, totalCol, totalRow) 
                                        VALUES (@set_id, @panel, @resource, @site, @mac, @programName, @machine, @product, @workArea, @testType, @testTime, @operatorName, @operatorType, @testMode, @trackType, @checkPcsDataForAVI, @uuid, @checkDetail, @tableInfor, @totalCol, @totalRow);";
                List<MySqlParameter> mySqlParameters = new List<MySqlParameter>();
                mySqlParameters.Add(new MySqlParameter("@set_id", tray_info.SetId));
                mySqlParameters.Add(new MySqlParameter("@panel", tray_info.panel));
                mySqlParameters.Add(new MySqlParameter("@resource", tray_info.resource));
                mySqlParameters.Add(new MySqlParameter("@site", tray_info.site));
                mySqlParameters.Add(new MySqlParameter("@mac", tray_info.mac));
                mySqlParameters.Add(new MySqlParameter("@programName", tray_info.programName));
                mySqlParameters.Add(new MySqlParameter("@machine", tray_info.machine));
                mySqlParameters.Add(new MySqlParameter("@product", tray_info.product));
                mySqlParameters.Add(new MySqlParameter("@workArea", tray_info.workArea));
                mySqlParameters.Add(new MySqlParameter("@testType", tray_info.testType));
                mySqlParameters.Add(new MySqlParameter("@testTime", tray_info.testTime));
                mySqlParameters.Add(new MySqlParameter("@operatorName", tray_info.operatorName));
                mySqlParameters.Add(new MySqlParameter("@operatorType", tray_info.operatorType));
                mySqlParameters.Add(new MySqlParameter("@testMode", tray_info.testMode));
                mySqlParameters.Add(new MySqlParameter("@trackType", tray_info.trackType));
                mySqlParameters.Add(new MySqlParameter("@checkPcsDataForAVI", tray_info.checkPcsDataForAVI));
                mySqlParameters.Add(new MySqlParameter("@uuid", tray_info.uuid));
                mySqlParameters.Add(new MySqlParameter("@checkDetail", tray_info.checkDetail));
                mySqlParameters.Add(new MySqlParameter("@tableInfor", tray_info.tableInfor));
                mySqlParameters.Add(new MySqlParameter("@totalCol", tray_info.totalCol));
                mySqlParameters.Add(new MySqlParameter("@totalRow", tray_info.totalRow));
                flag = MysqlHeplers.ExecuteNonQuery(conectionstring, CommandType.Text, sql, mySqlParameters.ToArray());
                if (flag <= 0)
                {
                    string msg = string.Format($"{strTrayTable}表，数据插入失败！");

                    SendLogToRecheckServer(msg);

                    return false;
                }
                #endregion
                #region 产品信息表操作
                //删除旧数据
                if (!string.IsNullOrEmpty(oldSetid))
                {
                    sql = $"DELETE FROM " + strProductTable + $" WHERE set_id = '{oldSetid}';";
                    flag = MysqlHeplers.ExecuteNonQuery(conectionstring, CommandType.Text, sql);
                    if (flag < 0)
                    {
                        string msg = string.Format($"{strProductTable}表，数据删除失败！");

                        SendLogToRecheckServer(msg);

                        return false;
                    }
                }
                //插入新数据
                sql = @"INSERT INTO " + strProductTable + @" (set_id, panelId, pos_row, pos_col, pcsSeq, operatorName, operatorTime, verifyResult, verifyOperatorName, verifyTime, testResult) VALUES ";
                StringBuilder values = new StringBuilder();
                foreach (var item in tray_info?.pcsSummarys)
                {
                    item.SetId = strSetID;
                    values.Append("(");
                    values.Append($"'{item.SetId}',");
                    values.Append($"'{item.panelId}',");
                    values.Append($"{item.pos_row},");
                    values.Append($"{item.pos_col},");
                    values.Append($"'{item.pcsSeq}',");
                    values.Append($"'{item.operatorName}',");
                    values.Append($"'{item.operatorTime}',");
                    values.Append($"'{item.verifyResult}',");
                    values.Append($"'{item.verifyOperatorName}',");
                    values.Append($"'{item.verifyTime}',");
                    values.Append($"'{item.testResult}'");
                    values.Append("),");
                }
                sql += values.ToString();
                sql = sql.Substring(0, sql.Length - 1);
                sql += ";";
                flag = MysqlHeplers.ExecuteNonQuery(conectionstring, CommandType.Text, sql);
                if (flag <= 0)
                {
                    string msg = string.Format($"{strProductTable}表，数据插入失败！");

                    SendLogToRecheckServer(msg);

                    return false;
                }
                #endregion
                #region 详情表操作
                //删除旧详情
                if (!string.IsNullOrEmpty(oldSetid))
                {
                    sql = $"DELETE FROM " + strDefectTable + $" WHERE set_id = '{oldSetid}';";
                    flag = MysqlHeplers.ExecuteNonQuery(conectionstring, CommandType.Text, sql);
                    if (flag < 0)
                    {
                        string msg = string.Format($"{strDefectTable}表，数据删除失败！");

                        SendLogToRecheckServer(msg);

                        return false;
                    }
                }
                //插入详情数据
                sql = @"INSERT INTO " + strDefectTable + @" (set_id, panelId, pcsBarCode, testType, pcsSeq, partSeq, pinSeq, testResult, operatorName, verifyResult, verifyOperatorName, verifyTime, defectCode, bubbleValue, testFile, imagePath)  VALUES ";
                StringBuilder detailValues = new StringBuilder();
                foreach (var product in tray_info?.pcsSummarys)
                {
                    foreach (var defect in product?.pcsDetails)
                    {
                        defect.SetId = strSetID;
                        detailValues.Append("(");
                        detailValues.Append($"'{defect.SetId}',");
                        detailValues.Append($"'{defect.panelId}',");
                        detailValues.Append($"'{defect.pcsBarCode}',");
                        detailValues.Append($"'{defect.testType}',");
                        detailValues.Append($"'{defect.pcsSeq}',");
                        detailValues.Append($"'{defect.partSeq}',");
                        detailValues.Append($"'{defect.pinSeq}',");
                        detailValues.Append($"'{defect.testResult}',");
                        detailValues.Append($"'{defect.operatorName}',");
                        detailValues.Append($"'{defect.verifyResult}',");
                        detailValues.Append($"'{defect.verifyOperatorName}',");
                        detailValues.Append($"'{defect.verifyTime}',");
                        detailValues.Append($"'{defect.defectCode}',");
                        detailValues.Append($"'{defect.bubbleValue}',");
                        detailValues.Append($"'{defect.testFile}',");
                        detailValues.Append($"'{defect.imagePath}'");
                        detailValues.Append("),");
                    }
                }
                sql += detailValues.ToString();
                sql = sql.Substring(0, sql.Length - 1);
                sql += ";";
                flag = MysqlHeplers.ExecuteNonQuery(conectionstring, CommandType.Text, sql);
                if (flag <= 0)
                {
                    string msg = string.Format($"{strDefectTable}表，数据插入失败！");

                    SendLogToRecheckServer(msg);

                    return false;
                }
                #endregion
            }
            catch (Exception ex)
            {
                string msg = string.Format("插入数据失败。错误信息：{0}", ex.Message);
                SendLogToRecheckServer(msg);

                msg = string.Format("插入数据失败。堆栈信息：{0}", ex.Message);
                SendLogToRecheckServer(msg);

                return false;
            }

            return true;
        }

        private class AVIFinishInfo
        {
            public string barcode;
            public bool isFinished;
        }
    }
}