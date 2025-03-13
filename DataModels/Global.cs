using AutoScanFQCTest.DialogWindows;
using AutoScanFQCTest.Logics;
using AutoScanFQCTest.Utilities;
using AutoScanFQCTest.Views;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using User = AutoScanFQCTest.DialogWindows.User;

namespace AutoScanFQCTest.DataModels
{
    // 复判数据查询模式
    public enum RecheckDataQueryMode
    {
        // 通过条码查询复判数据
        ByBarcode = 0,

        // 通过是否已复判标志查询复判数据
        ByQueryUncheckedFlag = 1,
    }

    public class DetectionResultEntry
    {
        public int m_nID = 0;
        public int m_nIndex = 0;
        public int m_nMachineID = 0;
        public int m_nResultFlag = 1;          // 1: OK, 2: NG
        public int m_nDefectType = 0;

        public string m_strBarcode = "";
        public string m_strResult = "";
        public string m_strTime = "";

        public double m_dbDefectArea = 0.0;
    }

    // 复判统计信息
    public class RecheckStatistics
    {
        private MainWindow m_parent;
        private string m_strCurrentDirectory = System.Environment.CurrentDirectory + "\\";     // 当前目录

        public int m_nTotalPanels = 0;                              // 总盘数
        public int m_nTotalRecheckedProducts = 0;         // 总复判产品数
        public int m_nTotalOKProducts = 0;                     // 总OK产品数
        public int m_nTotalNGProducts = 0;                     // 总NG产品数
        public int m_nTotalRecheckedOKProducts = 0;   // 总复判OK产品数
        public int m_nTotalRecheckedNGProducts = 0;   // 总复判NG产品数
        public int m_nRecheckedOKs = 0;                        // 复判OK点数
        public int m_nRecheckedNGs = 0;                       // 复判NG点数
        public int m_nTotalDefects = 0;                          // 总缺陷点数

        public int m_okpanel = 0; //ok盘数
        public int m_ngpanel = 0; //ng盘数
        public int m_okproduct = 0;//ok产品数
        public int m_ngproduct = 0;//ng产品数
        public int m_alldefects = 0;//总缺陷点数

        public int m_confirmOKs = 0; // 复判OK数
        public int m_confirmNGs = 0; //复判NG数

        public RecheckStatistics(MainWindow parent)
        {
            m_parent = parent;
        }

        /// <summary>
        /// 班次更换 保存上一班次的数据
        /// </summary>
        /// <returns></returns>
        public bool Backup()
        {
            try
            {
                string strFilePath = m_strCurrentDirectory + "recheck_statistics.dat";
                // 如果存在D盘，将strFilePath文件拷贝一份到D盘
                if (File.Exists(strFilePath))
                {
                    // 在D盘创建“复判备份”文件夹
                    string strDestDir = $"D:\\班次更换\\{m_parent.m_global.m_strCurrentOperatorID}";
                    if (!Directory.Exists(strDestDir))
                        Directory.CreateDirectory(strDestDir);

                    // 以时间戳为文件名，将strFilePath文件拷贝到D盘
                    string strDestFilePath = strDestDir + "\\" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".dat";
                    File.Copy(strFilePath, strDestFilePath, true);
                }
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }

        // 函数：将复判统计信息保存到文件
        public bool Save()
        {
            string strFilePath = m_strCurrentDirectory + "recheck_statistics.dat";

            try
            {
                INIFileParser parser = new INIFileParser(strFilePath);

                parser.WritePrivateProfileString("复判统计", "总盘数", m_nTotalPanels.ToString());
                parser.WritePrivateProfileString("复判统计", "总复判产品数", m_nTotalRecheckedProducts.ToString());
                parser.WritePrivateProfileString("复判统计", "复判OK点数", m_nRecheckedOKs.ToString());
                parser.WritePrivateProfileString("复判统计", "复判NG点数", m_nRecheckedNGs.ToString());


                parser.WritePrivateProfileString("复判统计", "总缺陷点数", m_nTotalDefects.ToString());
                parser.WritePrivateProfileString("复判统计", "一次OK数", m_nTotalOKProducts.ToString());
                parser.WritePrivateProfileString("复判统计", "一次NG数", m_nTotalNGProducts.ToString());

                //parser.WritePrivateProfileString("统计", "ok盘数", m_okpanel.ToString());
                //parser.WritePrivateProfileString("统计", "ng盘数", m_ngpanel.ToString());
                //parser.WritePrivateProfileString("统计", "ok产品数", m_okproduct.ToString());
                //parser.WritePrivateProfileString("统计", "ng产品数", m_ngproduct.ToString());
                //parser.WritePrivateProfileString("统计", "复判OK数", m_confirmOKs.ToString());
                //parser.WritePrivateProfileString("统计", "复判NG数", m_confirmNGs.ToString());

                parser.Save();

                //// 如果存在D盘，将strFilePath文件拷贝一份到D盘
                //if (File.Exists(strFilePath))
                //{
                //    // 在D盘创建“复判备份”文件夹
                //    string strDestDir = "D:\\复判备份";
                //    if (!Directory.Exists(strDestDir))
                //        Directory.CreateDirectory(strDestDir);

                //    // 以时间戳为文件名，将strFilePath文件拷贝到D盘
                //    string strDestFilePath = strDestDir + "\\" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".dat";
                //    File.Copy(strFilePath, strDestFilePath, true);
                //}
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }

        // 函数：从文件中加载复判统计信息
        public bool Load()
        {
            string strFilePath = m_strCurrentDirectory + "recheck_statistics.dat";

            try
            {
                INIFileParser parser = new INIFileParser(strFilePath);

                string strValue = "";

                strValue = parser.GetPrivateProfileString("复判统计", "总盘数", "0");
                if (strValue.Length > 0)
                    m_nTotalPanels = Convert.ToInt32(strValue);

                strValue = parser.GetPrivateProfileString("复判统计", "总复判产品数", "0");
                if (strValue.Length > 0)
                    m_nTotalRecheckedProducts = Convert.ToInt32(strValue);

                strValue = parser.GetPrivateProfileString("复判统计", "复判OK点数", "0");
                if (strValue.Length > 0)
                    m_nRecheckedOKs = Convert.ToInt32(strValue);

                strValue = parser.GetPrivateProfileString("复判统计", "复判NG点数", "0");
                if (strValue.Length > 0)
                    m_nRecheckedNGs = Convert.ToInt32(strValue);

                strValue = parser.GetPrivateProfileString("复判统计", "总缺陷点数", "0");
                if (strValue.Length > 0)
                    m_nTotalDefects = Convert.ToInt32(strValue);

                strValue = parser.GetPrivateProfileString("复判统计", "一次OK数", "0");
                if (strValue.Length > 0)
                    m_nTotalOKProducts = Convert.ToInt32(strValue);

                strValue = parser.GetPrivateProfileString("复判统计", "一次NG数", "0");
                if (strValue.Length > 0)
                    m_nTotalNGProducts = Convert.ToInt32(strValue);

                //strValue = parser.GetPrivateProfileString("统计", "ok盘数", "0");
                //if (strValue.Length > 0)
                //    m_okpanel = Convert.ToInt32(strValue);

                //strValue = parser.GetPrivateProfileString("统计", "ng盘数", "0");
                //if (strValue.Length > 0)
                //    m_ngpanel = Convert.ToInt32(strValue);

                //strValue = parser.GetPrivateProfileString("统计", "ok产品数", "0");
                //if (strValue.Length > 0)
                //    m_okproduct = Convert.ToInt32(strValue);

                //strValue = parser.GetPrivateProfileString("统计", "ng产品数", "0");
                //if (strValue.Length > 0)
                //    m_ngproduct = Convert.ToInt32(strValue);

                //strValue = parser.GetPrivateProfileString("统计", "总缺陷点数", "0");
                //if (strValue.Length > 0)
                //    m_alldefects = Convert.ToInt32(strValue);

                //strValue = parser.GetPrivateProfileString("统计", "复判OK数", "0");
                //if (strValue.Length > 0)
                //    m_confirmOKs = Convert.ToInt32(strValue);

                //strValue = parser.GetPrivateProfileString("统计", "复判NG数", "0");
                //if (strValue.Length > 0)
                //    m_confirmNGs = Convert.ToInt32(strValue);
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }

        // 函数：清空复判统计信息
        public void Clear()
        {
            m_nTotalPanels = 0;
            m_nTotalRecheckedProducts = 0;
            m_nTotalOKProducts = 0;
            m_nTotalNGProducts = 0;
            m_nTotalRecheckedOKProducts = 0;
            m_nTotalRecheckedNGProducts = 0;
            m_nRecheckedOKs = 0;
            m_nRecheckedNGs = 0;
            m_nTotalDefects = 0;

            m_okpanel = 0;
            m_ngpanel = 0;
            m_okproduct = 0;
            m_ngproduct = 0;
            m_alldefects = 0;

            m_confirmOKs = 0;
            m_confirmNGs = 0;
        }
    }

    public struct StationIPAndPort
    {
        public string m_strIP;
        public int m_nPort;
    }

    public class Global
    {
        private MainWindow m_parent = null;

        public string m_strCurrentDirectory = System.Environment.CurrentDirectory + "\\";     // 当前目录

        public bool m_bOfflineMode = false;                                                                            // 离线模式
        public bool m_bIsMainWindowInited = false;                                                               // 主窗口是否初始化完成
        public bool m_bExitProgram = false;                                                                            // 退出程序标志位

        public bool m_bIsCtrlKeyPressed = false;                                             // Ctrl键是否按下

        public static bool m_bIsAppInited = false;                                            // 程序是否已经初始化

        public LogPresenter m_log_presenter;                                                  // 日志呈现类对象

        public string m_strRootPWD = "chenling";
        public string m_strManagerPWD = "330010";
        public string m_strSZManagerPWD = "CvGY@;)g@9563217＊#rty";
        public bool m_bHasSuperRight = false;

        public static string m_strHardwareConfigFileName = "hardware.ini";

        public string m_http_service_url_of_set_id = "http://*:8180/vml/dcs/set_id/";
        public string m_http_service_url_of_background_heartbeat = "http://*:8180/heartbeat/";

        public string m_strCurrentMachineID = "SMT-AVI-888";         // 当前机器ID

        public string[] m_strLastOpenedImageFiles = new string[3];     // 最近打开的图片文件
        public string m_strLastOpenImagePath = "";                              // 最近打开的图片路径

        public string m_strSaveImageBrowseDir = "";                           // 保存图片浏览目录
        public String m_strOpenImageBrowseDir = "";                          // 打开图片浏览目录

        public string m_strAVIImageRemoteDir = "";                             // AVI图片远程目录

        public int m_nScannedBarcodeQuantity = 0;
        public int m_nOKQuantity = 0;
        public int m_nNGQuantity = 0;

        public bool m_bTurnOffMESUpload = false;                                                                 // 是否屏蔽MES上传

        public int m_nScannerGunComPortNum = 1;                                                             // 扫码枪串口号
        public int m_nScannerGunBaudRate = 9600;                                                             // 扫码枪波特率
        public int m_nScannerGunWorkMode = 1;                                                                 // 扫码枪工作模式，1为串口模式，2为焦点模式

        public double m_dbScreenDPIScale = 1;                                                                   // 屏幕DPI缩放比例

        // 用户管理
        public List<UserGroup> m_user_groups = new List<UserGroup>();                                                                           // 用户组
        public List<User> m_users = new List<User>();                                                                                                  // 用户
        public User m_current_user = new User();                                                                                                 // 当前用户

        // 数据库
        public MysqlOps m_mysql_ops;

        public string m_strOriginalProgramTitle = "";                                                                                                        // 程序标题

        // AVI通信对象
        public AVICommunication m_AVI_communication;

        // 料盘信息服务
        public TrayInfoService m_tray_info_service;

        // MES处理对象
        public MESService m_MES_service;

        // 数据库处理对象
        public DatabaseService m_database_service;

        public DefectStatisticsQueryWindow m_defectStatisticsQuery;

        public ThreeLevelUserPermission ThreeLevelUserPermission;

        // 后台料盘信息
        public AVITrayInfo m_background_tray_info_for_Nova = new AVITrayInfo();
        public TrayInfo m_background_tray_info_for_Dock = new TrayInfo();

        // 当前料盘信息
        public AVITrayInfo m_current_tray_info_for_Nova = new AVITrayInfo();
        public TrayInfo m_current_tray_info_for_Dock = new TrayInfo();

        public MESTrayInfo m_current_MES_tray_info = new MESTrayInfo();

        public PanelRecheckResult m_panel_recheck_result = new PanelRecheckResult();

        // 复判统计信息
        public RecheckStatistics m_recheck_statistics;

        public bool m_bIsMesConnected = false;                                                                                                     // MES是否连接成功

        public string m_strProductType = "";                                                                                                                    // 产品型号
        public string m_strProductSubType = "";                                                                                                              // 产品子型号
        public bool m_bUseProductTypeFromAVIData = false;

        public string m_strProductNameShownOnUI = "";                                                                                              // 界面显示产品名称

        // 图像旋转角度，0为不旋转，1为顺时针旋转90度，2为顺时针旋转180度，3为顺时针旋转270度
        public int m_nImageRotationAngle = 0;

        // 用dict来表示<机器名，机器IP>
        public Dictionary<string, string> m_dict_machine_names_and_IPs = new Dictionary<string, string>();
        public Dictionary<string, bool> m_dict_machine_names_and_image_cloning_flags = new Dictionary<string, bool>();                                // 用dict来表示<机器名，是否克隆图片>
        public Dictionary<string, bool> m_dict_machine_names_and_flags_of_reverse_order_for_even_rows = new Dictionary<string, bool>();   // 用dict来表示<机器名，偶数行是否逆序>
        public Dictionary<string, bool> m_dict_machine_names_and_flags_of_treating_defect_center_as_lefttop = new Dictionary<string, bool>();   // 用dict来表示<机器名，是否将缺陷中心坐标视为左上角坐标>
        public Dictionary<string, int> m_dict_machine_names_and_image_storage_method = new Dictionary<string, int>();                                   // 用dict来表示<机器名，图片存储方式>，0为本地存储，1为远程存储

        // FQC看图电脑的IP
        public Dictionary<string, StationIPAndPort> m_dict_FQC_station_names_and_IPs = new Dictionary<string, StationIPAndPort>();
        public bool m_bSendRecheckResultToFQCStations = false;                                                                                   // 是否将复判结果发送给FQC看图电脑

        // 复判站名和IP
        public Dictionary<string, string> m_dict_recheck_station_names_and_IPs = new Dictionary<string, string>();
        public string m_strRecheckStationName = "";                                                                                                      // 本机复判站名

        // 复判数据查询模式
        public RecheckDataQueryMode m_recheck_data_query_mode = RecheckDataQueryMode.ByBarcode;

        // 当一盘料被复判完成并成功上传到MES后，将触发此事件，以便通知其他复判站，实现集中复判的功能
        public string m_http_service_url_of_tray_recheck_data_mes_submit_event = "http://*:8180/tray_recheck_data_mes_submit_event/";

        // 用dict来表示<机器名，连接状态>，0为ping通和共享目录都正常，1为ping通但是共享目录不正常，2为ping不通，3为IP地址无效
        public Dictionary<string, int> m_dict_machine_names_and_connect_status = new Dictionary<string, int>(); 

        public bool m_bUncheckableDefectModifiable = true;                                                                                        // 不可复判项是否可修改
        public bool m_bShowPictureOfUncheckableDefect = false;                                                                                // 是否显示不可复判项的图片
        public List<string> m_uncheckable_defect_types = new List<string>();                                                             // 不可复判项
        public List<string> m_uncheckable_pass_types = new List<string>();
        public List<bool> m_uncheckable_defect_enable_flags = new List<bool>();                                                     // 不可复判项是否启用
        public List<bool> m_uncheckable_pass_enable_flags = new List<bool>();

        public string m_strCurrentClientMachineName = "";                                                                                                 // 当前机器名
        public int m_nCurrentTrayRows = 0;                                                                                                                     // 当前料盘行数
        public int m_nCurrentTrayColumns = 0;                                                                                                                 // 当前料盘列数
        
        public int m_nSpecialTrayCornerMode = 0;                                                                                                           // 料盘缺角方向，0为无（不适用），1为左上角，2为右上角
        
        public bool m_bSelectByMachine = false;                                                                                                             // 是否按机台号查询数据库
        public int m_nSelectedMachineIndexForDatabaseQuery = 0;                                                                               // 查询数据库时所选的机台号

        public bool m_bCombineTwoMachinesAsOne = false;                                                                                         // 是否将两台机器合并为一组

        public bool m_bIsInTestingMode = false;                                                                                                                // 是否为测试模式
        
        public int m_nRecheckInterval = 300;                                                                                                                     // 复判时间间隔

        //public int m_nWaitTimeForImageAcquisitionAndRendering = 50;                                                                       // 等待捞图的时间，单位为毫秒
        //public int m_nStartTimeForImageAcquisitionAndRendering = 0;                                                                         // 开始捞图的时间

        public int m_nDataStorageDuration = 10;                                                               // 清空表数据间隔天数

        public int m_nRecheckDisplayMode = 0;                                                                                                                  // 复判显示模式，0为盘检模式，1为单Pcs模式
        
        public bool m_bShowUnreceivedDataAsRedSlot = false;                                                                                       // 是否将未接收到的穴位显示为红色
        
        public bool m_bIgnoreOtherDefectsIfOneUnrecheckableItemExists = true;                                                          // 只要有一个不可复判项，该产品其它缺陷都自动不可复判
        
        public bool[] m_bIsImagesCloningThreadInProcess;

        public bool m_bIsDeletingImages = false;

        public List<List<string>> m_list_list_unchecked_trays = new List<List<string>>();                                         // 未复判的料盘列表

        public List<DefectCorrection> m_list_DefectCorrectionRecord = new List<DefectCorrection>();

        // 刷卡器对象
        public bool m_bIsCardReaderEnabled = false;                                                                                                     // 是否使用读卡器
        public ScanCard m_card_reader;
        public ScanCard ThreeLevelCardReader;                                                                                                       // 三级权限界面刷卡读取ID

        public string m_station_type = "";                                                                                                          // 当前站别，recheck复判或是transfer中转

        public int m_nMysqlPort = 3306;                                                                                                                          // mysql端口

        public bool m_bIsConnectedToDatabase = false;                                                                                                 // 是否连接到数据库

        public bool m_bDownloadImagesUponReceivingData = false;                                                                            // 是否在接收数据后立刻下载图片

        public bool m_bIsQueryingDatabaseAndCloningImages = false;                                                                         // 是否正在查询数据库并克隆图片

        public string m_strCurrentTrayTableName = "";                                                                                                    // 当前料盘表名

        public string m_strCurrentPC_AnyAVIName = "";

        public Action m_ChangedCount;

        public int m_nCustomerID = 0;                                                                                                                           // 客户ID，0为维信，1为华通

        public string m_strSiteCity = "苏州";                                                                                                                 // 厂区所在城市，苏州或者盐城
        public string m_strProductName = "";                                                                                                                 // 提交MES的产品料号名
        public string m_strProgramName = "";                                                                                                                // 提交MES的程序名

        public Dictionary<string, string> m_dictionary_users_and_passwords = new Dictionary<string, string>();  // 用户名和密码
        public bool m_bNeedToCheckPasswordWhenLogin = false;                                                                             // 登录工号时是否需要检查密码

        public string m_strTCP_IP_address_for_communication_with_card_reader = "169.254.80.183";                  // 读码器通讯本机服务端IP
        public int m_nTCP_port_for_communication_with_card_reader = 8600;                                                        // 读码器通讯本机服务端端口

        public int m_nRecheckModeWithAISystem = 1;                                                                                                      // AI复判模式，1纯人工，2人工+AI复判，3纯AI复判不带人工

        public bool m_bEnableAIAutoSubmit = true;                                                                               // 在纯AI模式下，是否允许AI自动提交

        public bool m_bUseAIRecheckResult;                                                                                                                 // 是否采用AI复判结果
        public bool m_bSubmitOKProductsToAI;                                                                                                             // AI是否提交OK数据
        public bool m_bShowAIResultFromTransferStation;                                                                         // 在复判站显示中转的AI结果
        public string m_strMesAIDataUploadUrl = "";                                                                                                     // MES接收AI复判数据接口地址

        public bool m_bSecondConfirmNGProductsBeforeSubmittingToMES = false;                                                  // 是否启用NG二次确认功能

        public bool m_bEnableOriginalNGDataUploadToMES = false;                                                                          // 是否启用一次NG数据上传到MES

        public bool m_bEnableMESValidationForOriginalNGProducts = false;                                                             // 扫码查询时对NG产品进行MES校验

        public bool m_bRetrieveOnlyImagesOfDefectedSide = true;                                                                            // 只捞取有缺陷一面的图片

        public bool m_bDisableRecheckIfImageIsNotExisting = false;                                                                        // 如果图片不存在则不允许复判

        public Dictionary<string, string> m_dict_old_defect_names_and_new_defect_names_mapping = new Dictionary<string, string>();     // 缺陷名称映射
        public bool m_bEnableDefectNameMapping = false;                                                                                               // 是否启用缺陷名称映射

        public bool m_bSendInspectionResultToRecheckStation = false;                                                                 // 是否作为中转站将检测结果发送给复判站

        // MES服务器端口和IP
        public string m_strMesIP = "";
        public int m_nMesPort = 8080;

        // MES接收复判数据接口地址
        public string m_strMesDataUploadUrl = "";

        public string m_strMesValidationUrl = "";                                                                                                           // MES验证接口地址

        // 是否提交MES前先检查已经跑过样品板
        public bool m_bCheckSampleProductStatusBeforeSubmittingRecheckResultsToMES = false;

        // 是否启用样品板卡控功能
        public bool m_bEnableSampleProductControl = false;

        // MES接收卡控数据接口地址
        public string m_strMesCheckSampleProductUrl = "";

        // 当前操作员工号
        public string m_strCurrentOperatorID = "";

        public string m_strVersion = "2.09.05";                                                                                                                           // 版本号

      // 三级权限管理
      public List<FupanUser> FupanUsers = new List<FupanUser>();        // 工程师用户名的身份和密码

        public UserType CurrentLoginType;                               // 三级权限下当前登录的用户类型

		/// <summary>
		/// 变更操作人事件
		/// </summary>
		public Action<string> UserChanged;
        /// <summary>
        /// 更换班次
        /// </summary>
        public Action m_WorkShiftChanged;
        /// <summary>
        /// 数据库连接字符串
        /// </summary>
        public string ConnsectionString = "server=localhost;user=root;port=3306;password=123456;SslMode=None;database=autoscanfqctest;allowPublicKeyRetrieval=true;";

        // 构造函数
        public Global(MainWindow parent)
        {
            m_parent = parent;

            m_parent.m_global = this;

            // 初始化日志呈现类对象
            m_log_presenter = new LogPresenter(m_parent);

            // 初始化数据模型
            //m_general_data_model = new GeneralDataModel(m_parent);

            // 初始化AVI通信对象
            m_AVI_communication = new AVICommunication(parent);

            // 初始化料盘信息服务
            m_tray_info_service = new TrayInfoService(parent);

            // 初始化MES处理对象
            m_MES_service = new MESService(parent);

            // 初始化数据库处理对象
            m_database_service = new DatabaseService(parent);

            m_recheck_statistics = new RecheckStatistics(m_parent);
            m_recheck_statistics.Load();

            // 初始化超级管理员和管理员两个用户组
            m_user_groups.Add(new UserGroup(0, "super admin"));
            m_user_groups.Add(new UserGroup(1, "admin"));

            // 初始化超级管理员和管理员两个用户
            m_users.Add(new User(0, 0, "super admin", "super admin"));
            m_users.Add(new User(1, 1, "admin", "admin"));

            // 初始化当前用户
            for (int n = 2; n < 20; n++)
            {
                m_users.Add(new User(n, 3, "", ""));
            }

            m_strOriginalProgramTitle = m_parent.Title;

            m_current_user = new User(10, 3, "操作员", "操作员");
            //m_current_user = m_users[2];

            m_mysql_ops = new MysqlOps(m_parent);


        }

        // 初始化数据模型
        public void InitDataModels()
        {
            //m_camera_and_light_data_model = new CameraAndLightDataModel(m_parent);
            //m_camera_and_light_data_model.init(m_strCurrentDirectory + "config.ini", m_strCurrentDirectory + "hardware.ini");
        }

        // 加载缺陷名称映射表
        public void LoadDefectNameMappingTable(string strFileName)
        {
            if (File.Exists(strFileName))
            {
                INIFileParser parser = new INIFileParser(strFileName);

                string strValue = "";

                strValue = parser.GetPrivateProfileString("一般", "映射数量", "0");
                if (strValue.Length > 0)
                {
                    int nCount = Convert.ToInt32(strValue);

                    for (int i = 1; i <= nCount; i++)
                    {
                        string strKey = "映射" + i.ToString();
                        
                        string strOldDefectName = parser.GetPrivateProfileString(strKey, "原缺陷名", "");
                        string strNewDefectName = parser.GetPrivateProfileString(strKey, "新缺陷名", "");

                        if (strOldDefectName.Length > 0 && strNewDefectName.Length > 0)
                        {
                            m_dict_old_defect_names_and_new_defect_names_mapping[strOldDefectName] = strNewDefectName;
                        }
                    }
                }
            }
        }

        // 保存缺陷名称映射表
        public bool SaveDefectNameMappingTable(string strFileName)
        {
            try
            {
                INIFileParser parser = new INIFileParser(strFileName);

                int nCount = m_dict_old_defect_names_and_new_defect_names_mapping.Count;

                parser.WritePrivateProfileString("一般", "映射数量", nCount.ToString());

                int nIndex = 1;

                foreach (KeyValuePair<string, string> kvp in m_dict_old_defect_names_and_new_defect_names_mapping)
                {
                    string strKey = "映射" + nIndex.ToString();

                    parser.WritePrivateProfileString(strKey, "原缺陷名", kvp.Key);
                    parser.WritePrivateProfileString(strKey, "新缺陷名", kvp.Value);

                    nIndex++;
                }

                parser.Save();

                return true;
            }
            catch (Exception ex)
            {
                // 弹出错误提示
                MessageBox.Show("保存缺陷名称映射表失败！异常信息：" + ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);

                return false;
            }
        }

        // 加载配置信息
        public void LoadConfigData(string strFileName)
        {
            if (File.Exists(strFileName))
            {
                INIFileParser parser = new INIFileParser(strFileName);

                string strValue = "";

                strValue = parser.GetPrivateProfileString("一般", "站别类型", "recheck");
                if (strValue.Length > 0)
                    m_station_type = strValue;

                strValue = parser.GetPrivateProfileString("一般", "离线模式", "False");
                if (strValue.Length > 0)
                    m_bOfflineMode = Convert.ToBoolean(strValue.ToString());

                strValue = parser.GetPrivateProfileString("一般", "客户ID", "0");
                if (strValue.Length > 0)
                    m_nCustomerID = Convert.ToInt32(strValue);

                strValue = parser.GetPrivateProfileString("一般", "厂区所在城市", "苏州");
                if (strValue.Length > 0)
                    m_strSiteCity = strValue;

                strValue = parser.GetPrivateProfileString("一般", "提交MES的产品料号名", "");
                if (strValue.Length > 0)
                    m_strProductName = strValue;

                strValue = parser.GetPrivateProfileString("一般", "提交MES的程序名", "");
                if (strValue.Length > 0)
                    m_strProgramName = strValue;

                strValue = parser.GetPrivateProfileString("一般", "屏蔽MES上传", "False");
                if (strValue.Length > 0)
                    m_bTurnOffMESUpload = Convert.ToBoolean(strValue.ToString());

                strValue = parser.GetPrivateProfileString("一般", "扫码枪串口号", "1");
                if (strValue.Length > 0)
                    m_nScannerGunComPortNum = Convert.ToInt32(strValue.ToString());
                strValue = parser.GetPrivateProfileString("一般", "扫码枪波特率", "9600");
                if (strValue.Length > 0)
                    m_nScannerGunBaudRate = Convert.ToInt32(strValue.ToString());
                strValue = parser.GetPrivateProfileString("一般", "扫码枪工作模式", "1");
                if (strValue.Length > 0)
                    m_nScannerGunWorkMode = Convert.ToInt32(strValue.ToString());

                // mysql端口
                strValue = parser.GetPrivateProfileString("一般", "mysql端口", "3306");
                if (strValue.Length > 0)
                    m_nMysqlPort = Convert.ToInt32(strValue);

                // 管理密码
                strValue = parser.GetPrivateProfileString("一般", "管理密码", "330010");
                if (strValue.Length > 0)
                    m_strManagerPWD = strValue;

                // 是否在接收数据后立刻下载图片
                strValue = parser.GetPrivateProfileString("一般", "是否在接收数据后立刻下载图片", "False");
                if (strValue.Length > 0)
                    m_bDownloadImagesUponReceivingData = Convert.ToBoolean(strValue.ToString());

                // 界面显示产品名称
                strValue = parser.GetPrivateProfileString("一般", "界面显示产品名称", "");
                if (strValue.Length > 0)
                    m_strProductNameShownOnUI = strValue;

                strValue = parser.GetPrivateProfileString("一般", "产品型号", "nova");
                if (strValue.Length > 0)
                    m_strProductType = strValue;
                strValue = parser.GetPrivateProfileString("一般", "产品子型号", "");
                if (strValue.Length > 0)
                    m_strProductSubType = strValue;

                strValue = parser.GetPrivateProfileString("一般", "MES服务器IP", "");
                if (strValue.Length > 0)
                    m_strMesIP = strValue;
                strValue = parser.GetPrivateProfileString("一般", "MES服务器端口", "8080");
                if (strValue.Length > 0)
                    m_nMesPort = Convert.ToInt32(strValue.ToString());
                strValue = parser.GetPrivateProfileString("一般", "MES接收复判数据接口地址", "");
                if (strValue.Length > 0)
                    m_strMesDataUploadUrl = strValue;
                strValue = parser.GetPrivateProfileString("一般", "MES验证接口地址", "");
                if (strValue.Length > 0)
                    m_strMesValidationUrl = strValue;

                strValue = parser.GetPrivateProfileString("一般", "显示中转的AI结果", "false");
                if (strValue.Length > 0)
                    m_bShowAIResultFromTransferStation = Convert.ToBoolean(strValue.ToString());

                strValue = parser.GetPrivateProfileString("一般", "扫码查询时对NG产品进行MES校验", "False");
                if (strValue.Length > 0)
                    m_bEnableMESValidationForOriginalNGProducts = Convert.ToBoolean(strValue.ToString());
                strValue = parser.GetPrivateProfileString("一般", "是否提交MES前先检查已经跑过样品板", "True");
                if (strValue.Length > 0)
                    m_bCheckSampleProductStatusBeforeSubmittingRecheckResultsToMES = Convert.ToBoolean(strValue.ToString());
                strValue = parser.GetPrivateProfileString("一般", "MES接收卡控数据接口地址", "");
                if (strValue.Length > 0)
                    m_strMesCheckSampleProductUrl = strValue;

                strValue = parser.GetPrivateProfileString("一般", "只捞取有缺陷一面的图片", "True");
                if (strValue.Length > 0)
                    m_bRetrieveOnlyImagesOfDefectedSide = Convert.ToBoolean(strValue.ToString());

                strValue = parser.GetPrivateProfileString("一般", "如果图片不存在则不允许复判", "False");
                if (strValue.Length > 0)
                    m_bDisableRecheckIfImageIsNotExisting = Convert.ToBoolean(strValue.ToString());

                strValue = parser.GetPrivateProfileString("一般", "是否采用AI结果", "False");
                if (strValue.Length > 0)
                    m_bUseAIRecheckResult = Convert.ToBoolean(strValue.ToString());
                strValue = parser.GetPrivateProfileString("一般", "AI是否提交OK数据", "False");
                if (strValue.Length > 0)
                    m_bSubmitOKProductsToAI = Convert.ToBoolean(strValue.ToString());
                strValue = parser.GetPrivateProfileString("一般", "MES接收AI复判数据接口地址", "");
                if (strValue.Length > 0)
                    m_strMesAIDataUploadUrl = strValue;
                strValue = parser.GetPrivateProfileString("一般", "AI复判模式", "1");
                if (strValue.Length > 0)
                    m_nRecheckModeWithAISystem = Convert.ToInt32(strValue.ToString());
                strValue = parser.GetPrivateProfileString("一般", "开启AI自动提交", "True");
                if (strValue.Length > 0)
                    m_bEnableAIAutoSubmit = Convert.ToBoolean(strValue.ToString());

                strValue = parser.GetPrivateProfileString("一般", "NG二次确认", "False");
                if (strValue.Length > 0)
                    m_bSecondConfirmNGProductsBeforeSubmittingToMES = Convert.ToBoolean(strValue.ToString());

                strValue = parser.GetPrivateProfileString("一般", "启用一次NG数据上传", "False");
                if (strValue.Length > 0)
                    m_bEnableOriginalNGDataUploadToMES = Convert.ToBoolean(strValue.ToString());

                strValue = parser.GetPrivateProfileString("一般", "启用样品板卡控功能", "False"); 
                if (strValue.Length > 0)
                    m_bEnableSampleProductControl = Convert.ToBoolean(strValue.ToString());

                strValue = parser.GetPrivateProfileString("一般", "是否启用缺陷名称映射", "False");
                if (strValue.Length > 0)
                    m_bEnableDefectNameMapping = Convert.ToBoolean(strValue.ToString());

                strValue = parser.GetPrivateProfileString("一般", "登录工号时是否需要检查密码", "False");
                if (strValue.Length > 0)
                    m_bNeedToCheckPasswordWhenLogin = Convert.ToBoolean(strValue.ToString());

                strValue = parser.GetPrivateProfileString("一般", "读码器通讯本机服务端IP", "169.254.80.183");
                if (strValue.Length > 0)
                    m_strTCP_IP_address_for_communication_with_card_reader = strValue;
                strValue = parser.GetPrivateProfileString("一般", "读码器通讯本机服务端端口", "8600");
                if (strValue.Length > 0)
                    m_nTCP_port_for_communication_with_card_reader = Convert.ToInt32(strValue);

                strValue = parser.GetPrivateProfileString("一般", "操作员工号", "");
                if (strValue.Length > 0)
                    m_strCurrentOperatorID = strValue;

                strValue = parser.GetPrivateProfileString("一般", "是否按机台号查询数据库", "False");
                if (strValue.Length > 0)
                    m_bSelectByMachine = Convert.ToBoolean(strValue.ToString());

                strValue = parser.GetPrivateProfileString("一般", "查询数据库时所选的机台号", "0");
                if (strValue.Length > 0)
                    m_nSelectedMachineIndexForDatabaseQuery = Convert.ToInt32(strValue.ToString());

                strValue = parser.GetPrivateProfileString("一般", "图像旋转角度", "0");
                if (strValue.Length > 0)
                    m_nImageRotationAngle = Convert.ToInt32(strValue.ToString());

                strValue = parser.GetPrivateProfileString("一般", "复判时间间隔", "300");
                if (strValue.Length > 0)
                    m_nRecheckInterval = Convert.ToInt32(strValue.ToString());

                strValue = parser.GetPrivateProfileString("一般", "删除数据间隔天数", "10");
                if (strValue.Length > 0)
                    m_nDataStorageDuration = Convert.ToInt32(strValue.ToString());

                strValue = parser.GetPrivateProfileString("一般", "料盘缺角方向", "0");
                if (strValue.Length > 0)
                    m_nSpecialTrayCornerMode = Convert.ToInt32(strValue.ToString());

                strValue = parser.GetPrivateProfileString("一般", "是否将两台机器合并为一组", "False");
                if (strValue.Length > 0)
                    m_bCombineTwoMachinesAsOne = Convert.ToBoolean(strValue.ToString());

                strValue = parser.GetPrivateProfileString("一般", "是否为测试模式", "False");
                if (strValue.Length > 0)
                    m_bIsInTestingMode = Convert.ToBoolean(strValue.ToString());

                strValue = parser.GetPrivateProfileString("一般", "是否使用读卡器", "False");
                if (strValue.Length > 0)
                    m_bIsCardReaderEnabled = Convert.ToBoolean(strValue.ToString());

                strValue = parser.GetPrivateProfileString("一般", "当前PC的AVI机台号（任意一个）", "");
                if (strValue.Length > 0)
                    m_strCurrentPC_AnyAVIName = strValue;

                strValue = parser.GetPrivateProfileString("一般", "复判显示模式", "0");
                if (strValue.Length > 0)
                    m_nRecheckDisplayMode = Convert.ToInt32(strValue.ToString());

                // 复判数据查询模式
                strValue = parser.GetPrivateProfileString("一般", "复判数据查询模式", "0");
                if (strValue.Length > 0)
                    m_recheck_data_query_mode = (RecheckDataQueryMode)Convert.ToInt32(strValue);

                // 将未接收到的穴位显示为红色
                strValue = parser.GetPrivateProfileString("一般", "将未接收到的穴位显示为红色", "False");
                if (strValue.Length > 0)
                    m_bShowUnreceivedDataAsRedSlot = Convert.ToBoolean(strValue.ToString());

                for (int n = 0; n < m_strLastOpenedImageFiles.Length; n++)
                {
                    strValue = parser.GetPrivateProfileString("一般", string.Format("上一次打开的图片文件{0}", n + 1), "");
                    if (strValue.Length > 0)
                        m_strLastOpenedImageFiles[n] = strValue;
                }

                strValue = parser.GetPrivateProfileString("一般", "最近打开的图片路径", "");
                if (strValue.Length > 0)
                    m_strLastOpenImagePath = strValue;

                strValue = parser.GetPrivateProfileString("一般", "保存图片浏览目录", "");
                if (strValue.Length > 0)
                    m_strSaveImageBrowseDir = strValue;

                strValue = parser.GetPrivateProfileString("一般", "打开图片浏览目录", "");
                if (strValue.Length > 0)
                    m_strOpenImageBrowseDir = strValue;

                strValue = parser.GetPrivateProfileString("一般", "AVI图片远程目录", "");
                if (strValue.Length > 0)
                    m_strAVIImageRemoteDir = strValue;

                int nNumOfMachines = 33;
                if (m_strProductType == "dock")
                    nNumOfMachines = 66;

                // 初始化数组
                m_bIsImagesCloningThreadInProcess = new bool[nNumOfMachines];
                for (int n = 0; n < nNumOfMachines; n++)
                    m_bIsImagesCloningThreadInProcess[n] = false;

                strValue = parser.GetPrivateProfileString("不可复判项", "不可复判项可修改", "True");
                if (strValue.Length > 0)
                    m_bUncheckableDefectModifiable = Convert.ToBoolean(strValue);

                strValue = parser.GetPrivateProfileString("不可复判项", "是否显示不可复判项的图片", "False");
                if (strValue.Length > 0)
                    m_bShowPictureOfUncheckableDefect = Convert.ToBoolean(strValue);

                // 不可复判项
                strValue = parser.GetPrivateProfileString("不可复判项", "不可复判项数量", "0");
                if (strValue.Length > 0)
                {
                    int nNumOfUncheckableDefectTypes = Convert.ToInt32(strValue);

                    // 只要有一个不可复判项，该产品其它缺陷都自动不可复判
                    strValue = parser.GetPrivateProfileString("不可复判项", "只要有一个不可复判项，该产品其它缺陷都自动不可复判", "True");
                    if (strValue.Length > 0)
                        m_bIgnoreOtherDefectsIfOneUnrecheckableItemExists = Convert.ToBoolean(strValue);

                    for (int n = 0; n < nNumOfUncheckableDefectTypes; n++)
                    {
                        strValue = parser.GetPrivateProfileString("不可复判项", string.Format("不可复判项{0}", n + 1), "");
                        if (strValue.Length > 0)
                        {
                            m_uncheckable_defect_types.Add(strValue);

                            strValue = parser.GetPrivateProfileString("不可复判项", string.Format("不可复判项{0}是否启用", n + 1), "False");
                            if (strValue.Length > 0)
                                m_uncheckable_defect_enable_flags.Add(Convert.ToBoolean(strValue));
                        }
                    }
                }

                strValue = parser.GetPrivateProfileString("不可复判项", "跳过复判项数量", "0");
                if (strValue.Length > 0)
                {
                    int nUncheckablePassTypesNum = Convert.ToInt32(strValue);

                    for (int n = 0; n < nUncheckablePassTypesNum; n++)
                    {
                        strValue = parser.GetPrivateProfileString("不可复判项", string.Format("跳过复判项{0}", n + 1), "");
                        if (strValue.Length > 0)
                        {
                            m_uncheckable_pass_types.Add(strValue);

                            strValue = parser.GetPrivateProfileString("不可复判项", string.Format("跳过复判项{0}是否启用", n + 1), "False");
                            if (strValue.Length > 0)
                            {
                                m_uncheckable_pass_enable_flags.Add(Convert.ToBoolean(strValue));
                            }
                        }
                    }
                }

                // FQC看图电脑
                strValue = parser.GetPrivateProfileString("FQC看图电脑", string.Format("是否将复判结果发送给FQC看图电脑"), "False");
                if (strValue.Length > 0)
                    m_bSendRecheckResultToFQCStations = Convert.ToBoolean(strValue);
                for (int n = 0; n < nNumOfMachines; n++)
                {
                    strValue = parser.GetPrivateProfileString("FQC看图电脑", string.Format("看图站{0}_名称", n + 1), "");
                    if (strValue.Length > 0)
                    {
                        string strFQCStationName = strValue;

                        strValue = parser.GetPrivateProfileString("FQC看图电脑", string.Format("看图站{0}_IP", n + 1), "");
                        if (strValue.Length > 0)
                        {
                            string strFQCStationIP = strValue;

                            int nFQCStationPort = 0;
                            strValue = parser.GetPrivateProfileString("FQC看图电脑", string.Format("看图站{0}_端口", n + 1), "0");
                            if (strValue.Length > 0)
                                nFQCStationPort = Convert.ToInt32(strValue);

                            StationIPAndPort station_ip_and_port = new StationIPAndPort();
                            station_ip_and_port.m_strIP = strFQCStationIP;
                            station_ip_and_port.m_nPort = nFQCStationPort;

                            m_dict_FQC_station_names_and_IPs.Add(strFQCStationName, station_ip_and_port);
                        }
                    }
                }

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
                            m_dict_machine_names_and_connect_status.Add(strMachineName, 2);
                        }
                    }
                }
                
                // 复判站名和IP
                for (int n = 0; n < 50; n++)
                {
                    strValue = parser.GetPrivateProfileString("复判站", string.Format("复判站{0}_名称", n + 1), "");
                    if (strValue.Length > 0)
                    {
                        string strRecheckStationName = strValue;

                        strValue = parser.GetPrivateProfileString("复判站", string.Format("复判站{0}_IP", n + 1), "");
                        if (strValue.Length > 0)
                        {
                            string strRecheckStationIP = strValue;

                            m_dict_recheck_station_names_and_IPs.Add(strRecheckStationName, strRecheckStationIP);
                        }
                    }
                }

                // 是否作为中转站将检测结果发送给复判站
                strValue = parser.GetPrivateProfileString("一般", "是否作为中转站将检测结果发送给复判站", "False");
                if (strValue.Length > 0)
                    m_bSendInspectionResultToRecheckStation = Convert.ToBoolean(strValue);
            }
        }

        // 保存配置信息
        public void SaveConfigData(string strFileName)
        {
            string filename = m_strCurrentDirectory + strFileName;

            INIFileParser parser = new INIFileParser(filename);
            
            //parser.WritePrivateProfileString("面阵相机参数", "曝光", m_area_camera_config.m_dbExposure.ToString());
            //parser.WritePrivateProfileString("面阵相机参数", "增益", m_area_camera_config.m_dbGain.ToString());

            parser.WritePrivateProfileString("一般", "屏蔽MES上传", m_bTurnOffMESUpload.ToString());
            parser.WritePrivateProfileString("一般", "扫码枪串口号", m_nScannerGunComPortNum.ToString());
            parser.WritePrivateProfileString("一般", "扫码枪波特率", m_nScannerGunBaudRate.ToString());
            parser.WritePrivateProfileString("一般", "扫码枪工作模式", m_nScannerGunWorkMode.ToString());
            //parser.WritePrivateProfileString("一般", "产品型号", m_strProductType.ToString());

            parser.WritePrivateProfileString("一般", "厂区所在城市", m_strSiteCity);
            parser.WritePrivateProfileString("一般", "提交MES的产品料号名", m_strProductName);
            parser.WritePrivateProfileString("一般", "提交MES的程序名", m_strProgramName);
            parser.WritePrivateProfileString("一般", "MES接收复判数据接口地址", m_strMesDataUploadUrl);
            parser.WritePrivateProfileString("一般", "MES验证接口地址", m_strMesValidationUrl);

            parser.WritePrivateProfileString("一般", "显示中转的AI结果", m_bShowAIResultFromTransferStation.ToString());
            parser.WritePrivateProfileString("一般", "扫码查询时对NG产品进行MES校验", m_bEnableMESValidationForOriginalNGProducts.ToString());
            parser.WritePrivateProfileString("一般", "MES接收卡控数据接口地址", m_strMesCheckSampleProductUrl);
            parser.WritePrivateProfileString("一般", "MES接收AI复判数据接口地址", m_strMesAIDataUploadUrl);
            parser.WritePrivateProfileString("一般", "AI复判模式", m_nRecheckModeWithAISystem.ToString());
            parser.WritePrivateProfileString("一般", "AI是否提交OK数据", m_bSubmitOKProductsToAI.ToString());

            parser.WritePrivateProfileString("一般", "只捞取有缺陷一面的图片", m_bRetrieveOnlyImagesOfDefectedSide.ToString());

            parser.WritePrivateProfileString("一般", "如果图片不存在则不允许复判", m_bDisableRecheckIfImageIsNotExisting.ToString());

            parser.WritePrivateProfileString("一般", "NG二次确认", m_bSecondConfirmNGProductsBeforeSubmittingToMES.ToString());

            parser.WritePrivateProfileString("一般", "启用一次NG数据上传", m_bEnableOriginalNGDataUploadToMES.ToString());

            parser.WritePrivateProfileString("一般", "是否提交MES前先检查已经跑过样品板", m_bCheckSampleProductStatusBeforeSubmittingRecheckResultsToMES.ToString());

            parser.WritePrivateProfileString("一般", "启用样品板卡控功能", m_bEnableSampleProductControl.ToString());

            parser.WritePrivateProfileString("一般", "是否启用缺陷名称映射", m_bEnableDefectNameMapping.ToString());

            parser.WritePrivateProfileString("一般", "登录工号时是否需要检查密码", m_bNeedToCheckPasswordWhenLogin.ToString());

            parser.WritePrivateProfileString("一般", "读码器通讯本机服务端IP", m_strTCP_IP_address_for_communication_with_card_reader);
            parser.WritePrivateProfileString("一般", "读码器通讯本机服务端端口", m_nTCP_port_for_communication_with_card_reader.ToString());

            parser.WritePrivateProfileString("一般", "产品型号", m_strProductType);

            parser.WritePrivateProfileString("一般", "复判显示模式", m_nRecheckDisplayMode.ToString());

            parser.WritePrivateProfileString("一般", "复判数据查询模式", ((int)m_recheck_data_query_mode).ToString());

            parser.WritePrivateProfileString("一般", "是否按机台号查询数据库", m_bSelectByMachine.ToString());
            parser.WritePrivateProfileString("一般", "查询数据库时所选的机台号", m_nSelectedMachineIndexForDatabaseQuery.ToString());

            parser.WritePrivateProfileString("一般", "料盘缺角方向", m_nSpecialTrayCornerMode.ToString());

            parser.WritePrivateProfileString("一般", "操作员工号", m_strCurrentOperatorID);

            parser.WritePrivateProfileString("一般", "复判时间间隔", m_nRecheckInterval.ToString());

            parser.WritePrivateProfileString("一般", "是否为测试模式", m_bIsInTestingMode.ToString());

            parser.WritePrivateProfileString("一般", "是否使用读卡器", m_bIsCardReaderEnabled.ToString());

            parser.WritePrivateProfileString("一般", "图像旋转角度", m_nImageRotationAngle.ToString());

            parser.WritePrivateProfileString("一般", "删除数据间隔天数", m_nDataStorageDuration.ToString());

            for (int n = 0; n < m_strLastOpenedImageFiles.Length; n++)
            {
                if (null != m_strLastOpenedImageFiles[n])
                    parser.WritePrivateProfileString("一般", string.Format("上一次打开的图片文件{0}", n + 1), m_strLastOpenedImageFiles[n].ToString());
            }

            parser.WritePrivateProfileString("一般", "最近打开的图片路径", m_strLastOpenImagePath.ToString());
            parser.WritePrivateProfileString("一般", "保存图片浏览目录", m_strSaveImageBrowseDir.ToString());
            parser.WritePrivateProfileString("一般", "打开图片浏览目录", m_strOpenImageBrowseDir.ToString());

            parser.WritePrivateProfileString("一般", "AVI图片远程目录", m_strAVIImageRemoteDir.ToString());

            // 不可复判项
            parser.WritePrivateProfileString("不可复判项", "是否显示不可复判项的图片", m_bShowPictureOfUncheckableDefect.ToString());

            parser.WritePrivateProfileString("不可复判项", "不可复判项数量", m_uncheckable_defect_types.Count.ToString());
            parser.WritePrivateProfileString("不可复判项", "只要有一个不可复判项，该产品其它缺陷都自动不可复判", m_bIgnoreOtherDefectsIfOneUnrecheckableItemExists.ToString());
            for (int n = 0; n < m_uncheckable_defect_types.Count; n++)
            {
                parser.WritePrivateProfileString("不可复判项", string.Format("不可复判项{0}", n + 1), m_uncheckable_defect_types[n]);
                parser.WritePrivateProfileString("不可复判项", string.Format("不可复判项{0}是否启用", n + 1), m_uncheckable_defect_enable_flags[n].ToString());
            }

            parser.WritePrivateProfileString("不可复判项", "跳过复判项数量", m_uncheckable_pass_types.Count.ToString());
            for(int n = 0; n < m_uncheckable_pass_types.Count; n++)
            {
                parser.WritePrivateProfileString("不可复判项", string.Format("跳过复判项{0}", n+1), m_uncheckable_pass_types[n]);
                parser.WritePrivateProfileString("不可复判项", string.Format("跳过复判项{0}是否启用", n + 1), m_uncheckable_pass_enable_flags[n].ToString());
            }

            parser.Save();

            // 保存各个数据模型的配置信息
            //m_camera_and_light_data_model.save(m_strCurrentDirectory + "config.ini", "hardware.ini");
        }

        // 加载用户信息
        public void LoadUserData(string strFileName)
        {
            if (File.Exists(strFileName))
            {
                INIFileParser parser = new INIFileParser(strFileName);
                string strValue = "";

                try
                {
                    strValue = parser.GetPrivateProfileString("工程师", "数量", "0");
                    if (strValue.Length > 0)
                    {
                        int nCount = Convert.ToInt32(strValue);

                        for (int i = 1; i <= nCount; i++)
                        {
                            string strKey = "工程师" + i.ToString() + "账户名";

                            strValue = parser.GetPrivateProfileString("工程师", strKey, "");
                            if (strValue.Length > 0)
                            {
                                string strAccountName = strValue;

                                strKey = "工程师" + i.ToString() + "密码";
                                strValue = parser.GetPrivateProfileString("工程师", strKey, "");

                                if (strValue.Length > 0)
                                {
                                    FupanUsers.Add(new FupanUser() { UserName = strAccountName, Password = strValue, UserType = (UserType)1 });
                                }
                            }
                        }
                    }

                    strValue = parser.GetPrivateProfileString("技术员", "数量", "0");
                    if (strValue.Length > 0)
                    {
                        int nCount = Convert.ToInt32(strValue);

                        for (int i = 1; i <= nCount; i++)
                        {
                            string strKey = "技术员" + i.ToString() + "账户名";

                            strValue = parser.GetPrivateProfileString("技术员", strKey, "");
                            if (strValue.Length > 0)
                            {
                                string strAccountName = strValue;

                                strKey = "技术员" + i.ToString() + "密码";
                                strValue = parser.GetPrivateProfileString("技术员", strKey, "");

                                if (strValue.Length > 0) {
									FupanUsers.Add(new FupanUser() { UserName = strAccountName, Password = strValue, UserType = (UserType)2 });
								}
							}
                        }
                    }


					strValue = parser.GetPrivateProfileString("操作员", "数量", "0");
					if (strValue.Length > 0) {
						int nCount = Convert.ToInt32(strValue);

						for (int i = 1; i <= nCount; i++) {
							string strKey = "操作员" + i.ToString() + "账户名";

							strValue = parser.GetPrivateProfileString("操作员", strKey, "");
							if (strValue.Length > 0) {
								string strAccountName = strValue;

								strKey = "操作员" + i.ToString() + "密码";
								strValue = parser.GetPrivateProfileString("操作员", strKey, "");

								if (strValue.Length > 0) {
									FupanUsers.Add(new FupanUser() { UserName = strAccountName, Password = strValue, UserType = (UserType)3 });
								}
							}
						}
					}
				}
                catch (Exception ex)
                {
                    // 弹出错误提示
                    MessageBox.Show("加载用户信息失败！异常信息：" + ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // 保存用户信息
        public void SaveUserData(string strFileName)
        {
            try
            {
                INIFileParser parser = new INIFileParser(strFileName);

                var users = FupanUsers.Where(user => user.UserType == UserType.工程师).ToList();
				int nCount = users.Count;

                parser.WritePrivateProfileString("工程师", "数量", nCount.ToString());

                int nIndex = 1;

                foreach (var user in users)
                {
                    string strKey = "工程师" + nIndex.ToString() + "账户名";

                    parser.WritePrivateProfileString("工程师", strKey, user.UserName);

                    strKey = "工程师" + nIndex.ToString() + "密码";
                    parser.WritePrivateProfileString("工程师", strKey, user.Password);

                    //strKey = "工程师" + nIndex.ToString() + "身份";
                    //var temp = user.UserType.GetTypeCode();
                    //parser.WritePrivateProfileString("工程师",strKey, user.UserType.GetTypeCode().ToString());

					nIndex++;
                }

				users = FupanUsers.Where(user=>user.UserType == UserType.技术员).ToList();
				nCount = users.Count;

                parser.WritePrivateProfileString("技术员", "数量", nCount.ToString());

                nIndex = 1;
                foreach (var user in users)
                {
                    string strKey = "技术员" + nIndex.ToString() + "账户名";
                    parser.WritePrivateProfileString("技术员", strKey, user.UserName);

                    strKey = "技术员" + nIndex.ToString() + "密码";
                    parser.WritePrivateProfileString("技术员", strKey, user.Password);

					//strKey = "技术员" + nIndex.ToString() + "身份";
					//parser.WritePrivateProfileString("技术员", strKey, user.UserType.GetTypeCode().ToString());

					nIndex++;
                }

                users = FupanUsers.Where(user => user.UserType == UserType.操作员).ToList();
				nCount = users.Count;

				parser.WritePrivateProfileString("操作员", "数量", nCount.ToString());

				nIndex = 1;
				foreach (var user in users) {
					string strKey = "操作员" + nIndex.ToString() + "账户名";
					parser.WritePrivateProfileString("操作员", strKey, user.UserName);

					strKey = "操作员" + nIndex.ToString() + "密码";
					parser.WritePrivateProfileString("操作员", strKey, user.Password);

					//strKey = "操作员" + nIndex.ToString() + "身份";
					//parser.WritePrivateProfileString("操作员", strKey, user.UserType.GetTypeCode().ToString());

					nIndex++;
				}

				parser.Save();
            }
            catch (Exception ex)
            {
                // 弹出错误提示
                MessageBox.Show("保存用户信息失败！异常信息：" + ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
