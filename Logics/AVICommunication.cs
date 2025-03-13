using AutoScanFQCTest.DataModels;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using AutoScanFQCTest.Canvas;
using System.Windows;
using MySql.Data.MySqlClient;
using System.Text.Json.Serialization;
using System.Net.NetworkInformation;
using MySqlX.XDevAPI.Relational;
using System.Runtime.InteropServices;
using AutoScanFQCTest.Utilities;
using System.Runtime.Remoting.Contexts;

namespace AutoScanFQCTest.Logics
{
    public class AVICommunication
    {
        [DllImport("kernel32.dll", CallingConvention = CallingConvention.Winapi)]
        extern static int GetTickCount();

        public MainWindow m_parent;

        private TcpListener m_TCP_listener;                              // 服务器监听器
        private TcpClient m_connected_client;                          // 已连接的客户端
        private Thread m_listener_thread;                                  // 监听线程
        private string m_strIP = "";                                             // 服务器IP地址
        private int m_nPort = 0;                                                  // 服务器端口
        private bool m_bIsTCPServerInited = false;                  // TCP服务器是否已初始化

        private Task m_task;
        private CancellationTokenSource m_cancellationToken;
        private Queue<string> m_querys_queue = new Queue<string>();

        private HttpListener m_webservice_listener;                 // WebService监听器
        private CancellationTokenSource m_cts;                         // 取消令牌
        private bool m_bIsWebServiceInited = false;                 // WebService是否已初始化

        int m_nBackgroundServiceHeartBeatCount = 0;          // 后台服务心跳计数

        // 构造函数
        public AVICommunication(MainWindow parent)
        {
            m_parent = parent;

            m_cancellationToken = new CancellationTokenSource();

            m_task = Task.Run(thread_handle_request_queue);
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
                m_parent.m_global.m_log_presenter.Log(string.Format("IP地址或端口号不合法。它们是：{0}:{1}", strIP, nPort));

                return;
            }

            // 创建WebService监听器
            m_webservice_listener = new HttpListener();

            // 添加监听地址
            m_webservice_listener.Prefixes.Add(m_parent.m_global.m_http_service_url_of_set_id);
            m_webservice_listener.Prefixes.Add(m_parent.m_global.m_http_service_url_of_background_heartbeat);
            m_webservice_listener.Prefixes.Add(m_parent.m_global.m_http_service_url_of_tray_recheck_data_mes_submit_event);

            // 启动WebService监听器
            m_webservice_listener.Start();

            m_cts = new CancellationTokenSource();

            m_bIsWebServiceInited = true;

            m_parent.m_global.m_log_presenter.Log(string.Format("HTTP WebService已启动"));
            m_parent.m_global.m_log_presenter.Log(string.Format("URL1: {0}", m_parent.m_global.m_http_service_url_of_set_id));
            m_parent.m_global.m_log_presenter.Log(string.Format("URL2: {0}", m_parent.m_global.m_http_service_url_of_background_heartbeat));
            m_parent.m_global.m_log_presenter.Log(string.Format("URL4: {0}", m_parent.m_global.m_http_service_url_of_tray_recheck_data_mes_submit_event));

            // 异步开始监听请求
            //if (m_parent.m_global.m_strProductType == "nova")
            //    Task.Run(() => ListenLoopForNova(m_cts.Token), m_cts.Token);
            //else if (m_parent.m_global.m_strProductType == "dock" || m_parent.m_global.m_strProductType == "cl5")
            Task.Run(() => ListenLoopForDock(m_cts.Token), m_cts.Token);
        }

        // 监控后台服务是否在正常运行，如果不正常，重启后台服务
        public void thread_monitor_background_service()
        {
            int nWatchDogCount = 0;                                                     // 看门狗计数
            int nPreviousBackgroundServiceHeartBeatCount = 0;       // 上一次后台服务心跳计数

            // 判断后台服务进程是否在运行
            bool bIsBackgroundServiceProcessRunning = false;
            if (true)
            {
                Process[] processes = Process.GetProcessesByName("FupanBackgroundService");
                foreach (Process process in processes)
                {
                    bIsBackgroundServiceProcessRunning = true;
                    break;
                }
            }

            while (true)
            {
                if (true == m_parent.m_global.m_bExitProgram)
                    break;

                // 如果后台服务心跳计数没有增加，说明后台服务已经停止运行
                if (m_nBackgroundServiceHeartBeatCount <= nPreviousBackgroundServiceHeartBeatCount)
                {
                    nWatchDogCount++;
                }
                else
                {
                    nWatchDogCount = 0;
                }

                // 如果看门狗计数超过10，或者达到重启时间间隔（每隔10000秒重启一次后台服务），重启后台服务
                int nResetTimeInterval = 80000;
                if (nWatchDogCount >= 200 || m_nBackgroundServiceHeartBeatCount > nResetTimeInterval || false == bIsBackgroundServiceProcessRunning)
                {
                    // 重启后台服务
                    if (nWatchDogCount >= 5)
                        m_parent.m_global.m_log_presenter.Log("后台服务异常，重启后台服务");
                    else if (m_nBackgroundServiceHeartBeatCount > nResetTimeInterval)
                        m_parent.m_global.m_log_presenter.Log(string.Format("后台服务心跳计数超过{0}，重启后台服务", nResetTimeInterval));

                    // 重启后台服务
                    RestartBackgroundService();

                    bIsBackgroundServiceProcessRunning = true;
                    m_nBackgroundServiceHeartBeatCount = 0;
                    nWatchDogCount = 0;
                }

                nPreviousBackgroundServiceHeartBeatCount = m_nBackgroundServiceHeartBeatCount;

                Thread.Sleep(1000);
            }
        }

        //
        /*private void test()
        {
            string json = "";
            string barcode = "";

            if (true)
            {
                // D盘根目录下创建一个名为fupandata的文件夹
                if (false == Directory.Exists(@"D:\fupandata"))
                    Directory.CreateDirectory(@"D:\fupandata");

                // 创建一个以barcode命名的文本文件，如果文件已经存在，覆盖掉
                string strFileName = string.Format(@"D:\fupandata\{0}.txt", barcode);

                Debugger.Log(0, null, string.Format("666666 二维码 {0} 内容将要保存到文件 {1}", barcode, strFileName));

                File.WriteAllText(strFileName, json);

                Debugger.Log(0, null, string.Format("666666 二维码 {0} 内容已经保存到文件 {1}", barcode, strFileName));
            }

            // 遍历D:\fupandata目录下的所有txt文件
            if (true)
            {
                string[] files = Directory.GetFiles(@"D:\fupandata", "*.txt");

                for (int n = 0; n < files.Length; n++)
                {
                    if (n > 5)
                        break;

                    string file = files[n];

                    // 读取文件内容
                    string content = File.ReadAllText(file);

                    // 判断内容是否为json格式
                    if (true)
                    {
                        bool bIsJson = false;

                        try
                        {
                            JObject json_object = JObject.Parse(content);

                            // 获取json内容中的barcode
                            string strBarcode = json_object["barcode"].ToString();

                            // 如果barcode不为空，说明是json格式
                            if (false == string.IsNullOrEmpty(strBarcode))
                            {
                                bIsJson = true;
                            }
                        }
                        catch (Exception ex)
                        {
                            bIsJson = false;
                        }

                        Debugger.Log(0, null, string.Format("666666 n {0}: 文件 {1} 是否为json格式 bIsJson = {2}", n, file, bIsJson));

                        // 如果是json格式，发送content给复判软件
                        if (bIsJson)
                        {
                            Debugger.Log(0, null, string.Format("666666 n {0}: 将文件 {1} 的内容 发送给复判软件", n, file));

                            // 发送content给复判软件
                            bool bResult = SendDataToServer("");

                            Debugger.Log(0, null, string.Format("666666 n {0}: 文件 {1} 的内容 发送给复判软件 结果 bResult = {2}", n, file, bResult));

                            // 如果发送成功，删除文件
                            if (bResult)
                            {
                                File.Delete(file);
                            }
                        }
                        else
                        {
                            // 如果不是json格式，删除文件
                            File.Delete(file);
                        }
                    }
                }
            }
        }*/

        // 处理消息队列
        private async void thread_handle_request_queue()
        {
            while (!m_cancellationToken.IsCancellationRequested)
            {
                if (m_parent.m_global != null && m_parent.m_global.m_bExitProgram)
                    break;

                if (m_querys_queue?.Count > 0)
                {
                    string info = m_querys_queue.Dequeue();

                    handle_incoming_data(info);

                    await Task.Delay(100);
                }
                else
                {
                    await Task.Delay(100);
                }
            }
        }

        // 监听HTTP请求
        private async Task ListenLoopForNova(CancellationToken ct)
        {
            try
            {
                while (!ct.IsCancellationRequested)
                {
                    // 等待传入的请求
                    var context = m_webservice_listener.GetContext();

                    if (true == m_parent.m_global.m_bExitProgram)
                        break;

                    // 获取请求信息
                    var request = context.Request;

                    //m_parent.m_global.m_log_presenter.Log(string.Format("HTTP请求来自 {0}", request.HttpMethod, request.Url.AbsolutePath, request.RemoteEndPoint.Address.ToString()));
                    m_parent.m_global.m_log_presenter.Log(string.Format("HTTP请求来自 {0}", request.RemoteEndPoint.Address.ToString()));

                    //return;

                    try
                    {
                        // 获取端口号后面部分的字符串
                        string strHttpPrefix1 = m_parent.m_global.m_http_service_url_of_set_id.Substring(m_parent.m_global.m_http_service_url_of_set_id.LastIndexOf(':') + 1);
                        strHttpPrefix1 = strHttpPrefix1.Substring(strHttpPrefix1.IndexOf('/') + 1);

                        string strHttpPrefix2 = m_parent.m_global.m_http_service_url_of_background_heartbeat.Substring(m_parent.m_global.m_http_service_url_of_background_heartbeat.LastIndexOf(':') + 1);
                        strHttpPrefix2 = strHttpPrefix2.Substring(strHttpPrefix2.IndexOf('/') + 1);

                        // 检查URL和HTTP方法，以决定如何处理请求
                        if (request.Url.AbsolutePath.EndsWith(string.Format("{0}", strHttpPrefix2)) && request.HttpMethod == "POST")
                        {
                            // 读取并反序列化请求中的JSON数据，对应类为ProductInfo
                            using (var reader = new System.IO.StreamReader(context.Request.InputStream))
                            {
                                string info = reader.ReadToEnd();
                                m_querys_queue.Enqueue(info);

                                // 返回数据
                                if (true)
                                {
                                    SendResponseToClient(context);
                                }
                            }
                        }
                        else if (request.Url.AbsolutePath.EndsWith(string.Format("{0}", strHttpPrefix1)) && request.HttpMethod == "POST")
                        {
                            // 返回数据
                            if (true)
                            {
                                ResponseFromRecheck response = new ResponseFromRecheck();

                                response.error_code = 0;
                                response.msg = "OK";

                                // 序列化Person对象为JSON字符串
                                string jsonResponse = System.Text.Json.JsonSerializer.Serialize(response);

                                SendResponseToClient(context, jsonResponse);
                            }
                        }
                        else if (request.Url.AbsolutePath.EndsWith(string.Format("{0}", strHttpPrefix1)) && request.HttpMethod == "GET")
                        {
                            // 返回数据
                            if (true)
                            {
                                AquireData aquireData = new AquireData();

                                // aquireData成员变量赋值
                                aquireData.uuid = Guid.NewGuid().ToString();
                                aquireData.product_id = "";
                                aquireData.set_id = m_parent.m_global.m_strCurrentMachineID + "_" + DateTime.Now.ToString("yyyyMMddHHmmss");

                                MESAquireSetId mESAquireSetId = new MESAquireSetId();

                                mESAquireSetId.data = aquireData;
                                mESAquireSetId.err_code = "0";

                                // 序列化Person对象为JSON字符串
                                string jsonResponse = System.Text.Json.JsonSerializer.Serialize(mESAquireSetId);

                                SendResponseToClient(context, jsonResponse);
                            }
                        }
                        else
                        {
                            // 处理其他请求
                        }
                    }
                    catch (Exception ex)
                    {
                        m_parent.m_global.m_log_presenter.Log(string.Format("处理请求失败。错误信息：{0}", ex.Message));
                    }
                }
            }
            catch (ObjectDisposedException)
            {
                // HttpListener被关闭时会抛出此异常
                // 无需处理，只需退出循环即可
            }
        }

        // 监听HTTP请求
        private async Task ListenLoopForDock(CancellationToken ct)
        {
            try
            {
                while (!ct.IsCancellationRequested)
                {
                    // 等待传入的请求
                    var context = m_webservice_listener.GetContext();

                    if (true == m_parent.m_global.m_bExitProgram)
                        break;

                    // 获取请求信息
                    var request = context.Request;

                    //m_parent.m_global.m_log_presenter.Log(string.Format("HTTP请求来自 {0}", request.HttpMethod, request.Url.AbsolutePath, request.RemoteEndPoint.Address.ToString()));
                    //m_parent.m_global.m_log_presenter.Log(string.Format("HTTP请求来自 {0}", request.RemoteEndPoint.Address.ToString()));

                    //return;

                    try
                    {
                        // 获取端口号后面部分的字符串
                        string strHttpPrefixForBackgroundMsg = m_parent.m_global.m_http_service_url_of_set_id.Substring(m_parent.m_global.m_http_service_url_of_set_id.LastIndexOf(':') + 1);
                        strHttpPrefixForBackgroundMsg = strHttpPrefixForBackgroundMsg.Substring(strHttpPrefixForBackgroundMsg.IndexOf('/') + 1);

                        string strHttpPrefixForBackgroundHeartBeat = m_parent.m_global.m_http_service_url_of_background_heartbeat.Substring(m_parent.m_global.m_http_service_url_of_background_heartbeat.LastIndexOf(':') + 1);
                        strHttpPrefixForBackgroundHeartBeat = strHttpPrefixForBackgroundHeartBeat.Substring(strHttpPrefixForBackgroundHeartBeat.IndexOf('/') + 1);

                        string strHttpPrefixForSyncRecheckStationResult = m_parent.m_global.m_http_service_url_of_tray_recheck_data_mes_submit_event.Substring(m_parent.m_global.m_http_service_url_of_tray_recheck_data_mes_submit_event.LastIndexOf(':') + 1);
                        strHttpPrefixForSyncRecheckStationResult = strHttpPrefixForSyncRecheckStationResult.Substring(strHttpPrefixForSyncRecheckStationResult.IndexOf('/') + 1);

                        // 检查URL和HTTP方法，以决定如何处理请求
                        if (request.Url.AbsolutePath.EndsWith(string.Format("{0}", strHttpPrefixForBackgroundHeartBeat)) && request.HttpMethod == "POST")
                        {
                            // 读取并反序列化请求中的JSON数据，对应类为ProductInfo
                            using (var reader = new System.IO.StreamReader(context.Request.InputStream))
                            {
                                // 反序列化JSON数据
                                string info = reader.ReadToEnd();

                                ResponseFromRecheck response = System.Text.Json.JsonSerializer.Deserialize<ResponseFromRecheck>(info);

                                // 返回数据
                                if (true)
                                {
                                    SendResponseToClient(context);
                                }

                                if (0 == m_nBackgroundServiceHeartBeatCount)
                                {
                                    Application.Current.Dispatcher.Invoke(new Action(() =>
                                    {
                                        //m_parent.Title = m_parent.m_global.m_strOriginalProgramTitle 
                                        //    + string.Format(" (复判主版本 {0}, 后台服务版本 {1} 端口 {2}, )", m_parent.m_global.m_strVersion, response.version, response.data_port);
                                        m_parent.Title = m_parent.m_global.m_strOriginalProgramTitle + string.Format(" (复判主版本 v{0})", m_parent.m_global.m_strVersion);
                                    }));
                                }

                                if (response.error_code == 0 && response.msg == "alive")
                                {
                                    if (m_nBackgroundServiceHeartBeatCount % 25 == 0)
                                    {
                                        m_parent.m_global.m_log_presenter.Log(string.Format("后台数据服务进程 心跳计数：{0}", m_nBackgroundServiceHeartBeatCount));

                                        // 读取程序目录下面的wx.cert文件，将内容写入一个字符串
                                        string strCert = "";
                                        if (true)
                                        {
                                            string strCertFile = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName), "wx.cert");
                                            if (true == System.IO.File.Exists(strCertFile))
                                            {
                                                strCert = System.IO.File.ReadAllText(strCertFile);
                                            }
                                        }

                                        Application.Current.Dispatcher.Invoke(new Action(() =>
                                        {
                                            //m_parent.Title = m_parent.m_global.m_strOriginalProgramTitle
                                            //    + string.Format(" (复判主版本 {0}, 后台服务版本 {1} 端口 {2}, {3})", m_parent.m_global.m_strVersion, response.version, response.data_port, strCert);
                                            m_parent.Title = m_parent.m_global.m_strOriginalProgramTitle + string.Format(" (复判主版本 v{0})", m_parent.m_global.m_strVersion);
                                        }));
                                    }

                                    m_nBackgroundServiceHeartBeatCount++;
                                }
                            }
                        }
                        else if (request.Url.AbsolutePath.EndsWith(string.Format("{0}", strHttpPrefixForSyncRecheckStationResult)) && request.HttpMethod == "POST")
                        {
                            // 读取并反序列化请求中的JSON数据，对应类为TrayRecheckedAndSubmittedEventTelegram
                            using (var reader = new System.IO.StreamReader(context.Request.InputStream))
                            {
                                // 反序列化JSON数据
                                string info = reader.ReadToEnd();

                                TrayRecheckedAndSubmittedEventTelegram message = System.Text.Json.JsonSerializer.Deserialize<TrayRecheckedAndSubmittedEventTelegram>(info);

                                // 返回数据
                                if (true)
                                {
                                    SendResponseToClient(context);
                                }

                                string strDatabase = "AutoScanFQCTest";
                                if (true == m_parent.m_global.m_mysql_ops.IsTableExist(strDatabase, message.table_name))
                                {
                                    // 查询耗时
                                    TimeSpan ts = new TimeSpan(0, 0, 0);

                                    string strSetID = message.set_id;
                                    string strQuery = string.Format("UPDATE {0} SET r1 = 'been_checked' WHERE set_id = '{1}';", message.table_name, strSetID);

                                    string strQueryResult = "";
                                    m_parent.m_global.m_mysql_ops.QueryTableData(message.table_name, strQuery, ref strQueryResult, ref ts);

                                    m_parent.m_global.m_log_presenter.Log(string.Format("料盘 {0} 已经被复判并提交MES成功", message.set_id));

                                    // 收到已复判消息后清空当前界面以免复判相同料盘
                                    m_parent.page_HomeView.ClearProductInfo();

                                    m_parent.page_HomeView.GenerateCircularButtonsInGridContainer(m_parent.page_HomeView.grid_CircularButtonContainer, 0, 0, null);
                                }
                                else
                                {
                                    m_parent.m_global.m_log_presenter.Log(string.Format("数据表 {0} 不存在", message.table_name));
                                }
                            }
                        }
                        else if (request.Url.AbsolutePath.EndsWith(string.Format("{0}", strHttpPrefixForBackgroundMsg)) && request.HttpMethod == "POST")
                        {
                            // 读取并反序列化请求中的JSON数据，对应类为ProductInfo
                            using (var reader = new System.IO.StreamReader(context.Request.InputStream))
                            {
                                // 反序列化JSON数据
                                string info = reader.ReadToEnd();

                                ResponseFromRecheck response = System.Text.Json.JsonSerializer.Deserialize<ResponseFromRecheck>(info);

                                // 返回数据
                                if (true)
                                {
                                    SendResponseToClient(context);
                                }

                                m_parent.m_global.m_log_presenter.Log(string.Format("{0}", response.msg));
                            }
                        }
                        else if (request.Url.AbsolutePath.EndsWith(string.Format("{0}", strHttpPrefixForBackgroundMsg)) && request.HttpMethod == "GET")
                        {
                            // 返回数据
                            if (true)
                            {
                                AquireData aquireData = new AquireData();

                                // aquireData成员变量赋值
                                aquireData.uuid = Guid.NewGuid().ToString();
                                aquireData.product_id = "";
                                aquireData.set_id = m_parent.m_global.m_strCurrentMachineID + "_" + DateTime.Now.ToString("yyyyMMddHHmmss");

                                MESAquireSetId mESAquireSetId = new MESAquireSetId();

                                mESAquireSetId.data = aquireData;
                                mESAquireSetId.err_code = "0";

                                // 序列化Person对象为JSON字符串
                                string jsonResponse = System.Text.Json.JsonSerializer.Serialize(mESAquireSetId);

                                SendResponseToClient(context, jsonResponse);
                            }
                        }
                        else
                        {
                            // 处理其他请求
                        }
                    }
                    catch (Exception ex)
                    {
                        m_parent.m_global.m_log_presenter.Log(string.Format("处理请求失败。错误信息：{0}", ex.Message));
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

        // 初始化TCP服务器
        public void InitTCPServer(string strIP, int nPort)
        {
            if (true == m_bIsTCPServerInited)
            {
                return;
            }

            // 判断IP地址是否合法，端口号是否合法，如果不合法，返回
            if (false == ValidateIPAddress(strIP) || false == ValidatePort(nPort.ToString()))
            {
                m_bIsTCPServerInited = false;

                // 显示错误信息
                m_parent.m_global.m_log_presenter.Log(string.Format("IP地址或端口号不合法。它们是：{0}:{1}", strIP, nPort));

                return;
            }

            m_bIsTCPServerInited = true;

            // 创建监听线程，strIP和nPort作为参数传递给监听线程
            m_listener_thread = new Thread(new ParameterizedThreadStart(ListenForClients));
            m_listener_thread.Start(new string[] { strIP, nPort.ToString() });
        }

        // 监听客户端连接
        private void ListenForClients(object obj)
        {
            string[] strParams = (string[])obj;

            m_strIP = strParams[0];
            m_nPort = int.Parse(strParams[1]);

            try
            {
                m_TCP_listener = new TcpListener(IPAddress.Parse(m_strIP), m_nPort);

                m_TCP_listener.Start();

                m_parent.m_global.m_log_presenter.Log(string.Format("TCP服务器已启动, IP地址: {0}, 端口: {1}", m_strIP, m_nPort));

                while (true)
                {
                    // 阻塞直到一个客户端连接到服务器
                    TcpClient client = m_TCP_listener.AcceptTcpClient();

                    if (true == m_parent.m_global.m_bExitProgram)
                        break;

                    // 检查是否已有客户端连接
                    if (m_connected_client == null)
                    {
                        m_connected_client = client;
                        NetworkStream clientStream = m_connected_client.GetStream();

                        byte[] message = new byte[4096];
                        int bytesRead;

                        // 尝试从客户端读取消息（阻塞直到有数据可读）
                        //bytesRead = clientStream.Read(message, 0, 4096);

                        // 成功接收到消息
                        UTF8Encoding encoder = new UTF8Encoding();

                        //Debugger.Log(0, null, string.Format("222222 encoder.GetString(message) = {0}", encoder.GetString(message)));

                        while ((bytesRead = clientStream.Read(message, 0, message.Length)) != 0)
                        {
                            if (true == m_parent.m_global.m_bExitProgram)
                                break;

                            string info = encoder.GetString(message);

                            Debugger.Log(0, null, string.Format("222222 info = {0}", info));

                            AVITrayInfo productInfo;

                            try
                            {
                                // 移除字符串中的NUL字节
                                info = info.Replace("\0", string.Empty);

                                productInfo = JsonSerializerHelper.DeserializeProductInfo(info);
                                //json_object = JsonConvert.DeserializeObject<JObject>(info);

                                Product product = new Product();

                                product.Defects = productInfo.Products[0].Defects;

                                Defect defect = product.Defects[0];
                            }
                            catch (Exception ex)
                            {
                                m_parent.m_global.m_log_presenter.Log(string.Format("解析JSON数据失败。错误信息：{0}", ex.Message));

                                return;
                            }

                            // 将消息回显给客户端

                            //clientStream.Write(message, 0, bytesRead);
                        }

                        // 关闭客户端连接并重置connectedClient
                        m_connected_client.Close();
                        m_connected_client = null;
                    }
                    else
                    {
                        // 可选，通知客户端服务器已连接到另一个客户端
                        using (NetworkStream ns = client.GetStream())
                        {
                            byte[] rejectMsg = Encoding.ASCII.GetBytes("服务器已经连接到一个客户端。");
                            ns.Write(rejectMsg, 0, rejectMsg.Length);
                        }

                        client.Close();
                    }
                }
            }
            catch (Exception e)
            {
            }
            finally
            {
                // 停止监听新的客户端
                m_TCP_listener.Stop();
            }
        }

        // 模拟发送数据给服务端m_strIP和m_nPort
        public void SendDataToServer(string strIP, int nPort, string strData)
        {
            try
            {
                TcpClient client = new TcpClient(strIP, nPort);

                NetworkStream clientStream = client.GetStream();

                byte[] message = Encoding.UTF8.GetBytes(strData);

                clientStream.Write(message, 0, message.Length);
                clientStream.Flush();

                client.Close();
            }
            catch (Exception e)
            {
            }
        }

        // 模拟发送数据给HTTP Webservice服务端
        public void SendJsonStringToWebService(string strHttpURL, string strData)
        {
            try
            {
                // 创建一个HTTP请求
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(strHttpURL);

                // 设置请求的方法
                request.Method = "POST";

                // 设置请求的内容类型
                request.ContentType = "application/json";

                // 将字符串转换为字节数组
                byte[] byteArray = Encoding.UTF8.GetBytes(strData);

                // 设置请求的内容长度
                request.ContentLength = byteArray.Length;

                try
                {
                    // 获取请求的输出流，并写入内容
                    using (Stream dataStream = request.GetRequestStream())
                    {
                        dataStream.Write(byteArray, 0, byteArray.Length);
                    }

                    // 发送请求并获取响应
                    using (WebResponse response = request.GetResponse())
                    {
                        // 获取HTTP响应
                        if (true)
                        {
                            {
                                // 获取响应的流
                                using (Stream dataStream = response.GetResponseStream())
                                {
                                    // 从流中读取响应内容
                                    using (StreamReader reader = new StreamReader(dataStream))
                                    {
                                        string responseFromServer = reader.ReadToEnd();

                                        // 显示响应内容
                                        //m_parent.m_global.m_log_presenter.Log(string.Format("HTTP响应内容：{0}", responseFromServer));
                                    }
                                }
                            }
                        }
                    }
                }
                catch (WebException ex)
                {
                    // 异常处理
                }

                Thread.Sleep(100);
            }
            catch (Exception e)
            {
            }
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

        // 发送响应给客户端
        private bool SendResponseToClient(HttpListenerContext http_context)
        {
            try
            {
                ResponseFromRecheck result = new ResponseFromRecheck();

                result.error_code = 0;

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

        // 处理传入的数据
        public void handle_incoming_data(object obj)
        {
            string info = (string)obj;
            
            try
            {
                // 如果D:\fupandata目录不存在，创建它
                if (false == Directory.Exists(@"D:\fupandata"))
                    Directory.CreateDirectory(@"D:\fupandata");

                // 序列化JSON数据
                switch (m_parent.m_global.m_strProductType)
                {
                    case "nova":
                        if (true)
                        {
                            // 将info保存到文件
                            string filepath = $@"D:\fupandata\incoming_data_{DateTime.Now:yyyyMMddHHmmss}.txt";
                            File.WriteAllText(filepath, info);

                            var model = JsonSerializerHelper.DeserializeProductInfo(info);

                            m_parent.m_global.m_background_tray_info_for_Nova = model;

                            // 更新UI
                            //m_parent.page_HomeView.ClearProductInfo();

                            // 如果m_parent.m_global.m_background_tray_info.product数量超过m_parent.m_global.m_background_tray_info.total_pcs，说明有问题，需要把多出来的product删除
                            if (m_parent.m_global.m_background_tray_info_for_Nova?.Products?.Count > m_parent.m_global.m_background_tray_info_for_Nova.TotalPcs)
                            {
                                m_parent.m_global.m_background_tray_info_for_Nova.Products.RemoveRange(m_parent.m_global.m_background_tray_info_for_Nova.TotalPcs,
                                    m_parent.m_global.m_background_tray_info_for_Nova.Products.Count - m_parent.m_global.m_background_tray_info_for_Nova.TotalPcs);
                            }

                            // 处理AVI产品信息
                            try
                            {
                                int nStartTime = GetTickCount();

                                if (true == m_parent.m_global.m_tray_info_service.HandleTrayInfoFromAVIForNova_DatabaseAndImagePathStorage(m_parent.m_global.m_background_tray_info_for_Nova))
                                    ;

                                //if (true == m_parent.m_global.m_tray_info_service.HandleTrayInfoFromAVI(m_parent.m_global.m_background_tray_info))
                                //    m_parent.m_global.m_tray_info_service.HandleTrayInfoFromAVI2(m_parent.m_global.m_background_tray_info);

                                int nEndTime = GetTickCount();

                                //m_parent.m_global.m_log_presenter.Log(string.Format("接收处理AVI数据耗时：{0:0.000}秒", (double)(nEndTime - nStartTime) / 1000.0f));
                                //m_parent.m_global.m_recheck_statistics.m_nTotalPanels++;
                                m_parent.m_global.m_ChangedCount?.Invoke();
                            }
                            catch (Exception ex)
                            {
                                m_parent.m_global.m_log_presenter.Log(string.Format("处理AVI产品信息失败。错误信息：{0}", ex.Message));
                            }
                        }
                        break;

                    case "dock":
                    case "cl5":
                        if (true)
                        {
                            TrayInfo tray_info = JsonConvert.DeserializeObject<TrayInfo>(info);

                            m_parent.m_global.m_background_tray_info_for_Dock = tray_info;

                            // 更新UI
                            //m_parent.page_HomeView.ClearProductInfo();

                            int nTotalPcs = m_parent.m_global.m_background_tray_info_for_Dock.total_columns * m_parent.m_global.m_background_tray_info_for_Dock.total_rows;

                            // 如果m_parent.m_global.m_background_tray_info.product数量超过m_parent.m_global.m_background_tray_info.total_pcs，说明有问题，需要把多出来的product删除
                            if (m_parent.m_global.m_background_tray_info_for_Dock?.products?.Count > nTotalPcs)
                            {
                                m_parent.m_global.m_background_tray_info_for_Dock.products.RemoveRange(nTotalPcs, m_parent.m_global.m_background_tray_info_for_Dock.products.Count - nTotalPcs);
                            }

                            // 处理AVI产品信息
                            try
                            {
                                // 将info保存到文件
                                string filepath = $@"D:\fupandata\incoming_data_{DateTime.Now:yyyyMMdd}";

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
                                    filepath = $@"{dir}\incoming_data_{DateTime.Now:yyyyMMdd}";
                                }

                                // 加上毫秒
                                filepath += DateTime.Now.ToString("HHmmssfff");

                                // 加上一个随机八位数
                                Random random = new Random();
                                filepath += random.Next(10000000, 99999999).ToString();

                                filepath += ".txt";

                                File.WriteAllText(filepath, info);

                                int nStartTime = GetTickCount();

                                if (true == m_parent.m_global.m_tray_info_service.HandleTrayInfoFromAVIForDockOrCL5_DatabaseAndImagePathStorage(m_parent.m_global.m_background_tray_info_for_Dock))
                                    ;

                                int nEndTime = GetTickCount();

                                //m_parent.m_global.m_log_presenter.Log(string.Format("接收处理AVI数据耗时：{0:0.000}秒", (double)(nEndTime - nStartTime) / 1000.0f));
                                //m_parent.m_global.m_recheck_statistics.m_nTotalPanels++;
                                m_parent.m_global.m_ChangedCount?.Invoke();
                            }
                            catch (Exception ex)
                            {
                                m_parent.m_global.m_log_presenter.Log(string.Format("处理AVI产品信息失败。错误信息：{0}", ex.Message));
                            }
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                m_parent.m_global.m_log_presenter.Log(string.Format("处理传入的数据失败。错误信息：{0}", ex.Message));
            }
        }

        // 重启后台服务
        public void RestartBackgroundService()
        {
            // 重启后台服务
            if (true)
            {
                // 先杀死后台服务进程
                Process[] processes = Process.GetProcessesByName("FupanBackgroundService");
                foreach (Process process in processes)
                {
                    process.Kill();
                }
                Thread.Sleep(1000);

                // 启动后台服务进程
                if (true)
                {
                    string strSourceFile = m_parent.m_global.m_strCurrentDirectory + "config.ini";
                    string strDestFile = m_parent.m_global.m_strCurrentDirectory + "FupanBackgroundService\\" + "config.ini";

                    // 复制配置文件
                    File.Copy(strSourceFile, strDestFile, true);

                    // 判断数据库进程“FupanBackgroundService.exe”有无启动
                    if (false == GeneralUtilities.CheckAndStartProcess("FupanBackgroundService", m_parent.m_global.m_strCurrentDirectory + "FupanBackgroundService\\" + "FupanBackgroundService.exe"))
                    {
                        m_parent.m_global.m_log_presenter.Log("警告：数据接收服务进程 FupanBackgroundService.exe 未启动");
                    }
                    else
                    {
                        m_parent.m_global.m_log_presenter.Log("数据接收服务进程 FupanBackgroundService.exe 关闭重启后正在运行中");
                    }
                }
            }
        }

        private class AVIFinishInfo
        {
            public string barcode;
            public bool isFinished;
        }
    }
}
