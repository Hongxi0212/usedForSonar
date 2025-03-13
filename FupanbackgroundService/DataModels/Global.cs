using AutoScanFQCTest.DataModels;
using FupanBackgroundService.Logics;
using FupanBackgroundService.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static FupanBackgroundService.DataModels.AVIProductInfo;

namespace FupanBackgroundService.DataModels
{
    public class Global
    {
        public string m_strCurrentDirectory = System.Environment.CurrentDirectory + "\\";     // 当前目录

        public static Global m_instance = new Global(null);  // 全局唯一实例

        public string m_strProductType = "";                                                                                                           // 产品型号

        public bool m_bExitProgram = false;                                                                            // 退出程序标志位

        // 用dict来表示<机器名，机器IP>
        public Dictionary<string, string> m_dict_machine_names_and_IPs = new Dictionary<string, string>();
        public Dictionary<string, string> m_dict_recheckPC_names_and_IPs = new Dictionary<string, string>();
        public Dictionary<string, bool> m_dict_machine_names_and_image_cloning_flags = new Dictionary<string, bool>();                                // 用dict来表示<机器名，是否克隆图片>
        public Dictionary<string, bool> m_dict_machine_names_and_flags_of_reverse_order_for_even_rows = new Dictionary<string, bool>();   // 用dict来表示<机器名，偶数行是否逆序>
        public Dictionary<string, bool> m_dict_machine_names_and_flags_of_treating_defect_center_as_lefttop = new Dictionary<string, bool>();   // 用dict来表示<机器名，是否将缺陷中心坐标视为左上角坐标>
        public Dictionary<string, int> m_dict_machine_names_and_image_storage_method = new Dictionary<string, int>();                                   // 用dict来表示<机器名，图片存储方式>，0为本地存储，1为远程存储

        public string m_strVersion = "8.27";                                                                                                                                  // 版本号

        // 数据库
        public MysqlOps m_mysql_ops;

        // AVI通信对象
        public AVICommunication m_AVI_communication;

        // 料盘信息服务
        public TrayInfoService m_tray_info_service;

        // 数据库处理对象
        public DatabaseService m_database_service;

        public MESService m_mes_service;

        // 后台料盘信息
        public AVITrayInfo m_background_tray_info_for_Nova = new AVITrayInfo();
        public TrayInfo m_background_tray_info_for_Dock = new TrayInfo();

        public string m_AVI_data_collector_service_set_id = "http://*:8080/vml/dcs/set_id/";
        public string m_AVI_data_collector_service_report_data = "http://*:8080/vml/dcs/report_data/";
        public string m_AVI_data_collector_service_recheck_station_result = "http://*:8080/fqc/recheck_result_submitted/";
        public string m_AVI_data_collector_service_avi_finish = "http://*:8080/vml/dcs/avi/finish/";
        public string m_AVI_data_collector_service_single_side_report_data_forAI = "http://*:8080/vml/dcs/report_data/ai/";
        public string m_AVI_data_sender_service_merge_side_request_data_forAI = "http://*:8080/vml/dcs/request_data/ai/";
        public string m_transfer_data_collector_service_report_data = "http://*:8080/vml/dcs/report_data/transfer/";

        public string m_recheck_server_report_url = "http://127.0.0.1:8180/data_collect_message/";
        public string m_recheck_server_heartbeat_url = "http://127.0.0.1:8180/heartbeat/";

        // 站别类型
        public string m_station_type = "";

        public int m_nMysqlPort = 3306;                                                                                                                          // mysql端口

        public int m_nWebapiPort = 8080;                                                                                                                        // webapi端口

        public bool m_bDownloadImagesUponReceivingData = false;                                                                            // 是否在接收数据后立刻下载图片

        public bool m_bEnableOriginalNGDataUploadToMES = false;                                                                          // 是否启用一次NG数据上传到MES

        public string m_strProductSubType = "";                                                                                                              // 产品子型号

        public string m_strSiteCity = "苏州";                                                                                                                 // 厂区所在城市，苏州或者盐城
        public string m_strProductName = "";                                                                                                                 // 提交MES的产品料号名
        public string m_strProgramName = "";                                                                                                                // 提交MES的程序名

        // MES接收卡控数据接口地址
        public string m_strMesCheckSampleProductUrl = "";

        // MES接收复判数据接口地址
        public string m_strMesDataUploadUrl = "";

        // 是否提交MES前先检查已经跑过样品板
        public bool m_bCheckSampleProductStatusBeforeSubmittingRecheckResultsToMES = false;

        public MESTrayInfo m_current_MES_tray_info = new MESTrayInfo();

        // 当前操作员工号
        public string m_strCurrentOperatorID = "";

        public int m_nRecheckModeWithAISystem = 1;                                                          // AI复判模式，1纯人工，2人工+AI复判，3纯AI复判不带人工

        public bool m_bSubmitOKProductsToAI = false;                                                        // AI模式下，OK产品是否也向AI复判接口提交

        public string m_strAISingleSideInfoUploadAddress;                                                   // AI接收单面数据的接口地址
        public string m_strAIMergeInfoUploadAddress;                                                        // AI接收合并数据的接口地址

        // 构造函数
        public Global(MainWindow parent)
        {
            m_mysql_ops = new MysqlOps();

            // 初始化料盘信息服务
            m_tray_info_service = new TrayInfoService();

            // 初始化数据库处理对象
            m_database_service = new DatabaseService();

            // 初始化AVI通信对象
            m_AVI_communication = new AVICommunication();

            m_mes_service = new MESService();
        }

        // 加载配置信息
        public void LoadConfigData(string strFileName)
        {
            if (File.Exists(strFileName))
            {
                m_dict_machine_names_and_flags_of_reverse_order_for_even_rows.Clear();
                m_dict_machine_names_and_flags_of_treating_defect_center_as_lefttop.Clear();
                m_dict_machine_names_and_IPs.Clear();
                m_dict_machine_names_and_image_cloning_flags.Clear();
                m_dict_machine_names_and_image_storage_method.Clear();

                INIFileParser parser = new INIFileParser(strFileName);

                string strValue = "";

                // 站别类型
                strValue = parser.GetPrivateProfileString("一般", "站别类型", "recheck");
                if (strValue.Length > 0)
                    m_station_type = strValue;

                strValue = parser.GetPrivateProfileString("一般", "产品型号", "nova");
                if (strValue.Length > 0)
                    m_strProductType = strValue;

                strValue = parser.GetPrivateProfileString("一般", "产品子型号", "");
                if (strValue.Length > 0)
                    m_strProductSubType = strValue;

                strValue = parser.GetPrivateProfileString("一般", "操作员工号", "");
                if (strValue.Length > 0)
                    m_strCurrentOperatorID = strValue;

                // mysql端口
                strValue = parser.GetPrivateProfileString("一般", "mysql端口", "3306");
                if (strValue.Length > 0)
                    m_nMysqlPort = Convert.ToInt32(strValue);

                // webapi端口
                strValue = parser.GetPrivateProfileString("一般", "webapi端口", "8080");
                if (strValue.Length > 0)
                    m_nWebapiPort = Convert.ToInt32(strValue);

                // 是否在接收数据后立刻下载图片
                strValue = parser.GetPrivateProfileString("一般", "是否在接收数据后立刻下载图片", "False");
                if (strValue.Length > 0)
                    m_bDownloadImagesUponReceivingData = Convert.ToBoolean(strValue);

                // 是否启用一次NG数据上传到MES
                strValue = parser.GetPrivateProfileString("一般", "启用一次NG数据上传", "False");
                if (strValue.Length > 0)
                    m_bEnableOriginalNGDataUploadToMES = Convert.ToBoolean(strValue);

                strValue = parser.GetPrivateProfileString("一般", "厂区所在城市", "苏州");
                if (strValue.Length > 0)
                    m_strSiteCity = strValue;

                strValue = parser.GetPrivateProfileString("一般", "提交MES的产品料号名", "");
                if (strValue.Length > 0)
                    m_strProductName = strValue;

                strValue = parser.GetPrivateProfileString("一般", "提交MES的程序名", "");
                if (strValue.Length > 0)
                    m_strProgramName = strValue;

                strValue = parser.GetPrivateProfileString("一般", "是否提交MES前先检查已经跑过样品板", "True");
                if (strValue.Length > 0)
                    m_bCheckSampleProductStatusBeforeSubmittingRecheckResultsToMES = Convert.ToBoolean(strValue.ToString());

                strValue = parser.GetPrivateProfileString("一般", "MES接收卡控数据接口地址", "");
                if (strValue.Length > 0)
                    m_strMesCheckSampleProductUrl = strValue;

                strValue = parser.GetPrivateProfileString("一般", "MES接收复判数据接口地址", "");
                if (strValue.Length > 0)
                    m_strMesDataUploadUrl = strValue;

                strValue = parser.GetPrivateProfileString("一般", "AI复判模式", "1");
                if (strValue.Length > 0)
                    m_nRecheckModeWithAISystem = Convert.ToInt32(strValue.ToString());

                strValue = parser.GetPrivateProfileString("一般", "AI是否提交OK数据", "False");
                if (strValue.Length > 0)
                    m_bSubmitOKProductsToAI = Convert.ToBoolean(strValue.ToString());

                strValue = parser.GetPrivateProfileString("一般", "AI接收单面数据接口地址", "");
                if (strValue.Length > 0)
                    m_strAISingleSideInfoUploadAddress = strValue;

                strValue = parser.GetPrivateProfileString("一般", "AI接收合并数据接口地址", "");
                if (strValue.Length > 0)
                    m_strAIMergeInfoUploadAddress = strValue;

                int nNumOfMachines = 33;
                if (m_strProductType == "dock")
                    nNumOfMachines = 66;

                // 机器名和IP
                for (int n = 0; n < nNumOfMachines; n++)
                {
                    strValue = parser.GetPrivateProfileString("AVI视觉电脑", string.Format("机器{0}_名称", n + 1), "");
                    if (strValue.Length > 0)
                    {
                        string strMachineName = strValue;

                        strValue = parser.GetPrivateProfileString("AVI视觉电脑", string.Format("机器{0}_IP", n + 1), "");
                        if (strValue.Length > 0)
                        {
                            string strMachineIP = strValue;

                            int nImageStorageMethod = 0;

                            strValue = parser.GetPrivateProfileString("AVI视觉电脑", string.Format("机器{0}_图片存储方式", n + 1), "本地");
                            if (strValue.Length > 0)
                            {
                                if (strValue.Contains("本地"))
                                    nImageStorageMethod = 0;
                                else if (strValue.Contains("远程"))
                                    nImageStorageMethod = 1;
                            }

                            // 偶数行逆序
                            strValue = parser.GetPrivateProfileString("AVI视觉电脑", string.Format("机器{0}_偶数行是否逆序", n + 1), "False");
                            if (strValue.Length > 0)
                                m_dict_machine_names_and_flags_of_reverse_order_for_even_rows.Add(strMachineName, Convert.ToBoolean(strValue));

                            // 是否将缺陷中心坐标视为左上角坐标
                            strValue = parser.GetPrivateProfileString("AVI视觉电脑", string.Format("机器{0}_是否将缺陷中心坐标视为左上角坐标", n + 1), "False");
                            if (strValue.Length > 0)
                                m_dict_machine_names_and_flags_of_treating_defect_center_as_lefttop.Add(strMachineName, Convert.ToBoolean(strValue));

                            m_dict_machine_names_and_IPs.Add(strMachineName, strMachineIP);
                            m_dict_machine_names_and_image_cloning_flags.Add(strMachineName, false);
                            m_dict_machine_names_and_image_storage_method.Add(strMachineName, nImageStorageMethod);
                        }
                    }
                }

                int nNumOfRecheckPC = 6;
                for(int n = 0; n < nNumOfRecheckPC; n++)
                {
                    strValue = parser.GetPrivateProfileString("复判站", $"复判站{n + 1}_名称", "");
                    if (strValue.Length > 0)
                    {
                        var machineName = strValue;

                        strValue = parser.GetPrivateProfileString("复判站", $"复判站{n + 1}_IP", "");
                        if (strValue.Length > 0)
                        {
                            var machineIP = strValue;

                            m_dict_recheckPC_names_and_IPs.Add(machineName, machineIP);
                        }
                    }
                }
            }
        }
    }
}
