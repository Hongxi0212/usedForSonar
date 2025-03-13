using FupanBackgroundService.DataModels;
using FupanBackgroundService.Helpers;
using Mysqlx.Crud;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using static FupanBackgroundService.DataModels.AVIProductInfo;

namespace FupanBackgroundService
{
    public partial class App : Application
    {
        private static Mutex _mutex;

        protected override void OnStartup(StartupEventArgs e)
        {
            bool createNew;
            _mutex = new Mutex(true, "FupanBackgroundService", out createNew);
            if (!createNew)
            {
                MessageBox.Show("检测程序 FupanBackgroundService.exe 已经有一个运行实例，请先关闭 (或在任务管理器中终止实例)，再重新打开。", "程序冲突提示");
                System.Environment.Exit(1);
            }

            base.OnStartup(e);

            // 获取Global.m_instance.m_strCurrentDirectory的上一级目录
            //DirectoryInfo parentDir = (new DirectoryInfo(Global.m_instance.m_strCurrentDirectory)).Parent;

            //Global.m_instance.LoadConfigData(parentDir.FullName + "\\config.ini");
            Global.m_instance.LoadConfigData(Global.m_instance.m_strCurrentDirectory + "config.ini");

            // 设置webapi端口
            if (Global.m_instance.m_strProductSubType == "glue_check")
            {
                Global.m_instance.m_AVI_data_collector_service_set_id = "http://*:8888/vml/dcs/set_id/";
                Global.m_instance.m_AVI_data_collector_service_report_data = "http://*:8888/vml/dcs/report_data/";
                Global.m_instance.m_AVI_data_collector_service_recheck_station_result = "http://*:8888/fqc/recheck_result_submitted/";
            }
            else
            {
                Global.m_instance.m_AVI_data_collector_service_set_id = string.Format("http://*:{0}/vml/dcs/set_id/", Global.m_instance.m_nWebapiPort);
                Global.m_instance.m_AVI_data_collector_service_report_data = string.Format("http://*:{0}/vml/dcs/report_data/", Global.m_instance.m_nWebapiPort);
                Global.m_instance.m_AVI_data_collector_service_single_side_report_data_forAI = string.Format("http://*:{0}/vml/dcs/report_data/ai/", Global.m_instance.m_nWebapiPort);
                Global.m_instance.m_AVI_data_collector_service_avi_finish = string.Format("http://*:{0}/vml/dcs/avi/finish/", Global.m_instance.m_nWebapiPort);
                Global.m_instance.m_AVI_data_collector_service_recheck_station_result = string.Format("http://*:{0}/fqc/recheck_result_submitted/", Global.m_instance.m_nWebapiPort);
                Global.m_instance.m_AVI_data_sender_service_merge_side_request_data_forAI = string.Format("http://*:{0}/vml/dcs/request_data/ai/", Global.m_instance.m_nWebapiPort);
                Global.m_instance.m_transfer_data_collector_service_report_data = string.Format("http://*:{0}/vml/dcs/report_data/transfer/", Global.m_instance.m_nWebapiPort);
			}

            if (Global.m_instance.m_station_type == "fqc")
            {
                Global.m_instance.m_recheck_server_report_url = Global.m_instance.m_recheck_server_report_url.Replace("8180", "8280");
                Global.m_instance.m_recheck_server_heartbeat_url = Global.m_instance.m_recheck_server_heartbeat_url.Replace("8180", "8280");
            }
            if (Global.m_instance.m_station_type == "transfer")
            {
                Global.m_instance.m_mes_service.singleSideAIInterfaceUrl = Global.m_instance.m_strAISingleSideInfoUploadAddress;
                Global.m_instance.m_mes_service.mergeSideAIInterfaceUrl = Global.m_instance.m_strAIMergeInfoUploadAddress;
            }

            // 启动配置文件监视器
            (new Thread(thread_monitor_config_file_change)).Start();

            //(new Thread(Global.m_instance.m_AVI_communication.thread_avi_data_wait_send_to_recheck_folder_monitor)).Start();

            // 初始化数据库连接
            init_database();

            // 初始化Http服务
            init_http_service();

            // 调用后台服务
            StartBackgroundService();

            // 可以考虑此处添加系统托盘图标逻辑，如果需要的话

            // 设置开机自启动
            if (true)
            {
                Microsoft.Win32.RegistryKey rk = Microsoft.Win32.Registry.CurrentUser;

                rk.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run");
                rk = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);

                //string strExePath = Application.ExecutablePath.ToString();

                //rk.SetValue("FupanBackgroundService", strExePath);
            }
        }

        // 线程：配置文件监视
        private void thread_monitor_config_file_change()
        {
            string strConfigFile = Global.m_instance.m_strCurrentDirectory + "config.ini";

            // 获取strConfigFile最后一次修改的时间
            DateTime dtLastWriteTime = File.GetLastWriteTime(strConfigFile);

            while (true)
            {
                Thread.Sleep(1000);

                // 判断strConfigFile是否被修改，记录时间戳，重新加载配置文件
                if (dtLastWriteTime != File.GetLastWriteTime(strConfigFile))
                {
                    dtLastWriteTime = File.GetLastWriteTime(strConfigFile);

                    Debugger.Log(0, null, string.Format("222222 配置文件 {0} 已被修改，重新加载配置文件", strConfigFile));

                    string msg = string.Format("配置文件 {0} 已被修改，数据服务进程 重新加载配置文件", strConfigFile);

                    Global.m_instance.m_AVI_communication.SendLogToRecheckServer(msg);

                    // 重新加载配置文件
                    Global.m_instance.LoadConfigData(strConfigFile);

                    // 重新初始化数据库连接
                    init_database();
                }
            }
        }

        // 初始化数据库连接
        private void init_database()
        {
            // 连接MySQL数据库
            if (true == Global.m_instance.m_mysql_ops.IsMySQLServiceInstalled())
            {
                if (true == Global.m_instance.m_mysql_ops.ConnectToMySQL("root", "123456", Global.m_instance.m_nMysqlPort))
                {
                    Debugger.Log(0, null, string.Format("222222 成功连接到MySQL数据库"));

                    // 判断数据库是否存在
                    string strDatabase = "AutoScanFQCTest";
                    bool bIsDatabaseExist = false;
                    if (true)
                    {
                        if (false == Global.m_instance.m_mysql_ops.IsDatabaseExist(strDatabase))
                        {
                            Global.m_instance.m_mysql_ops.CreateDatabase(strDatabase);

                            // 判断是否创建成功
                            if (false == Global.m_instance.m_mysql_ops.IsDatabaseExist(strDatabase))
                            {
                                Debugger.Log(0, null, string.Format("222222 创建数据库失败"));
                            }
                            else
                            {
                                Debugger.Log(0, null, string.Format("222222 创建数据库 {0} 成功", strDatabase));

                                bIsDatabaseExist = true;
                            }
                        }
                        else
                        {
                            Debugger.Log(0, null, string.Format("222222 数据库 {0} 已存在", strDatabase));

                            bIsDatabaseExist = true;
                        }
                    }

                    // 连接到数据库
                    bool bIsConnectedToDatabase = false;
                    if (true == bIsDatabaseExist)
                    {
                        if (false == Global.m_instance.m_mysql_ops.ConnectToDatabase(strDatabase))
                        {
                            Debugger.Log(0, null, string.Format("222222 连接到数据库 {0} 失败", strDatabase));

                            bIsConnectedToDatabase = false;
                        }
                        else
                        {
                            Debugger.Log(0, null, string.Format("222222 成功连接到数据库 {0}", strDatabase));

                            bIsConnectedToDatabase = true;
                        }
                    }

                    // 如果数据库存在，判断表格是否存在
                    if (true == bIsConnectedToDatabase)
                    {
                        // 判断表格recheck_station_result是否存在
                        if (Global.m_instance.m_station_type == "fqc")
                        {
                            if (false == Global.m_instance.m_mysql_ops.IsTableExist(strDatabase, "recheck_station_result"))
                            {
                                string strSqlCommand = @"
                                CREATE TABLE IF NOT EXISTS recheck_station_result (
                                    set_id VARCHAR(255),
                                    barcode VARCHAR(255),
                                    recheck_result VARCHAR(255),
                                    r1 VARCHAR(255),
                                    r2 VARCHAR(255),
                                    r3 VARCHAR(255),
                                    r4 VARCHAR(255),
                                    r5 VARCHAR(255),
                                    r6 VARCHAR(255),
                                    r7 VARCHAR(255),
                                    r8 VARCHAR(255),
                                    r9 VARCHAR(255),
                                    r10 VARCHAR(255),
                                    create_time TIMESTAMP DEFAULT CURRENT_TIMESTAMP
                                );";

                                Global.m_instance.m_mysql_ops.CreateTable(strDatabase, "recheck_station_result", strSqlCommand);

                                // 判断是否创建成功
                                if (false == Global.m_instance.m_mysql_ops.IsTableExist(strDatabase, "recheck_station_result"))
                                {
                                    Debugger.Log(0, null, string.Format("222222 创建表格 recheck_station_result 失败"));
                                }
                                else
                                {
                                    Debugger.Log(0, null, string.Format("222222 创建表格 recheck_station_result 成功"));
                                }
                            }
                            else
                            {
                                Debugger.Log(0, null, string.Format("222222 表格 recheck_station_result 已存在"));
                            }
                        }

                        // 遍历m_global.m_dict_machine_names_and_IPs的keys，创建表格
                        for (int t = 0; t < Global.m_instance.m_dict_machine_names_and_IPs.Count; t++)
                        {
                            string strMachineName = Global.m_instance.m_dict_machine_names_and_IPs.Keys.ElementAt(t);

                            // 如果strMachineName不为空，创建表格
                            if (true == string.IsNullOrEmpty(strMachineName))
                                continue;

                            string strTrayTable = "trays_" + strMachineName;
                            string strProductTable = "products_" + strMachineName;
                            string strDefectTable = "details_" + strMachineName;
                            string strSingleSideTable = "transfer_aiInfo_" + strMachineName;

                            string[] strTables = new string[4] { strTrayTable, strProductTable, strDefectTable, strSingleSideTable };

                            string[] strSqlCommands = new string[5];

                            // 如果strTables文本包含"-"，替换为"_"
                            for (int i = 0; i < strTables.Length; i++)
                            {
                                if (true == strTables[i].Contains("-"))
                                {
                                    strTables[i] = strTables[i].Replace("-", "_");
                                }
                            }

                            if (Global.m_instance.m_station_type == "fqc")
                            {
                                strSqlCommands[0] = @"
                                    CREATE TABLE IF NOT EXISTS " + strTables[0] + @" (
                                        set_id VARCHAR(255) PRIMARY KEY,
                                        panel VARCHAR(255),
                                        resource VARCHAR(255),
                                        site VARCHAR(255),
                                        mac VARCHAR(255),
                                        programName VARCHAR(255),
                                        machine VARCHAR(255),
                                        product VARCHAR(255),
                                        workArea VARCHAR(255),
                                        testType VARCHAR(255),
                                        testTime VARCHAR(255),
                                        operatorName VARCHAR(255),
                                        operatorType VARCHAR(255),
                                        testMode VARCHAR(255),
                                        trackType VARCHAR(255),
                                        checkPcsDataForAVI BOOLEAN,
                                        uuid VARCHAR(255),
                                        checkDetail TEXT,
                                        tableInfor TEXT,
                                        totalCol INT,
                                        totalRow INT,
                                        create_time TIMESTAMP DEFAULT CURRENT_TIMESTAMP
                                    );";

                                strSqlCommands[1] = @"
                                    CREATE TABLE IF NOT EXISTS " + strTables[1] + @" (
                                        set_id VARCHAR(255),
                                        panelId VARCHAR(255),
                                        pos_row INT,
                                        pos_col INT,
                                        pcsSeq VARCHAR(255),
                                        operatorName VARCHAR(255),
                                        operatorTime VARCHAR(255),
                                        verifyResult VARCHAR(255),
                                        verifyOperatorName VARCHAR(255),
                                        verifyTime VARCHAR(255),
                                        testResult VARCHAR(255),
                                        create_time TIMESTAMP DEFAULT CURRENT_TIMESTAMP
                                    );";

                                strSqlCommands[2] = @"
                                    CREATE TABLE IF NOT EXISTS " + strTables[2] + @" (
                                        set_id VARCHAR(255),
                                        panelId VARCHAR(255),
                                        pcsBarCode VARCHAR(255),
                                        testType VARCHAR(255),
                                        pcsSeq VARCHAR(255),
                                        partSeq VARCHAR(255),
                                        pinSeq VARCHAR(255),
                                        testResult VARCHAR(255),
                                        operatorName VARCHAR(255),
                                        verifyResult VARCHAR(255),
                                        verifyOperatorName VARCHAR(255),
                                        verifyTime VARCHAR(255),
                                        defectCode VARCHAR(255),
                                        bubbleValue VARCHAR(255),
                                        testFile VARCHAR(255),
                                        imagePath VARCHAR(255),
                                        create_time TIMESTAMP DEFAULT CURRENT_TIMESTAMP
                                    );";
                            }
                            else
                            {
                                switch (Global.m_instance.m_strProductType)
                                {
                                    case "nova":
                                        if (true)
                                        {
                                            strSqlCommands[0] = @"
                                                CREATE TABLE IF NOT EXISTS " + strTables[0] + @" (
                                                    batch_id VARCHAR(255),
                                                    number_of_rows INT,
                                                    number_of_cols INT,
                                                    front BOOLEAN,
                                                    mid VARCHAR(255),
                                                    operator VARCHAR(255),
                                                    operator_id VARCHAR(50),
                                                    product_id VARCHAR(255),
                                                    resource VARCHAR(255),
                                                    scan_code_mode INT,
                                                    set_id VARCHAR(255) PRIMARY KEY,
                                                    total_pcs INT,
                                                    uuid VARCHAR(255),
                                                    work_area VARCHAR(255),
                                                    full_status VARCHAR(255),
                                                    r1 VARCHAR(255),
                                                    r2 VARCHAR(255),
                                                    r3 VARCHAR(255),
                                                    r4 VARCHAR(255),
                                                    r5 VARCHAR(255),
                                                    r6 VARCHAR(255),
                                                    r7 VARCHAR(255),
                                                    r8 VARCHAR(255),
                                                    r9 VARCHAR(255),
                                                    r10 VARCHAR(255),
                                                    create_time TIMESTAMP DEFAULT CURRENT_TIMESTAMP
                                                );";

                                            strSqlCommands[1] = @"
                                            CREATE TABLE IF NOT EXISTS " + strTables[1] + @" (
                                                    set_id VARCHAR(255),
                                                    uuid VARCHAR(255),
                                                    batch_id VARCHAR(255),
                                                    bar_code VARCHAR(255),
                                                    image VARCHAR(255),
                                                    image_a VARCHAR(255),
                                                    image_b VARCHAR(255),
                                                    is_null VARCHAR(255),
                                                    panel_id VARCHAR(255),
                                                    pos_col INT,
                                                    pos_row INT,
                                                    shareimg_path VARCHAR(255),
                                                    r1 VARCHAR(255),
                                                    r2 VARCHAR(255),
                                                    r3 VARCHAR(255),
                                                    r4 VARCHAR(255),
                                                    r5 VARCHAR(255),
                                                    r6 VARCHAR(255),
                                                    r7 VARCHAR(255),
                                                    r8 VARCHAR(255),
                                                    r9 VARCHAR(255),
                                                    r10 VARCHAR(255),
                                                    create_time TIMESTAMP DEFAULT CURRENT_TIMESTAMP
                                                );";

                                            strSqlCommands[2] = @"
                                            CREATE TABLE IF NOT EXISTS " + strTables[2] + @" (
                                                    id INT,
                                                    set_id VARCHAR(255),
                                                    uuid VARCHAR(255),
                                                    product_id VARCHAR(255),
                                                    sn INT,
                                                    type VARCHAR(255),
                                                    width DECIMAL(10, 2),
                                                    height DECIMAL(10, 2),
                                                    x DECIMAL(10, 2),
                                                    y DECIMAL(10, 2),
                                                    channel_ INT,
                                                    channelNum_ INT,
                                                    area DECIMAL(10, 2),
                                                    um_per_pixel DECIMAL(10, 2),
                                                    aiResult VARCHAR(255),
                                                    aiDefectCode VARCHAR(255),
                                                    aiGuid VARCHAR(255),
                                                    r1 VARCHAR(255),
                                                    r2 VARCHAR(255),
                                                    r3 VARCHAR(255),
                                                    r4 VARCHAR(255),
                                                    r5 VARCHAR(255),
                                                    r6 VARCHAR(255),
                                                    r7 VARCHAR(255),
                                                    r8 VARCHAR(255),
                                                    r9 VARCHAR(255),
                                                    r10 VARCHAR(255),
                                                    create_time TIMESTAMP DEFAULT CURRENT_TIMESTAMP
                                                );";
                                        }
                                        break;

                                    case "dock":
                                    case "cl5":
                                        if (true)
                                        {
                                            strSqlCommands[0] = @"
                                                CREATE TABLE IF NOT EXISTS " + strTables[0] + @" (
                                                machine_id VARCHAR(255),
                                                number_of_rows INT,
                                                number_of_cols INT,
                                                front BOOLEAN,
                                                mid VARCHAR(255),
                                                operator VARCHAR(255),
                                                operator_id VARCHAR(50),
                                                product_id VARCHAR(255),
                                                resource VARCHAR(255),
                                                scan_code_mode_ INT,
                                                set_id VARCHAR(255) PRIMARY KEY,
                                                total_pcs INT,
                                                uuid VARCHAR(255),
                                                work_area VARCHAR(255),
                                                site VARCHAR(255),
                                                inspect_date_time VARCHAR(255),
                                                region_area VARCHAR(255),
                                                mac_address VARCHAR(255),
                                                ip_address VARCHAR(255),
                                                MES_failure_msg VARCHAR(255),
                                                r1 VARCHAR(255),
                                                r2 VARCHAR(255),
                                                r3 VARCHAR(255),
                                                r4 VARCHAR(255),
                                                r5 VARCHAR(255),
                                                r6 VARCHAR(255),
                                                r7 VARCHAR(255),
                                                r8 VARCHAR(255),
                                                r9 VARCHAR(255),
                                                r10 VARCHAR(255),
                                                create_time TIMESTAMP DEFAULT CURRENT_TIMESTAMP
                                            );";

                                            strSqlCommands[1] = @"
                                                CREATE TABLE IF NOT EXISTS " + strTables[1] + @" (
                                                    set_id VARCHAR(255),
                                                    uuid VARCHAR(255),
                                                    machine_id VARCHAR(255),
                                                    bar_code VARCHAR(255),
                                                    is_null VARCHAR(255),
                                                    panel_id VARCHAR(255),
                                                    pos_col INT,
                                                    pos_row INT,
                                                    bET BOOL,
                                                    MES_failure_msg VARCHAR(255),
                                                    is_ok_product BOOL,
                                                    inspect_date_time VARCHAR(255),
                                                    region_area VARCHAR(255),
                                                    mac_address VARCHAR(255),
                                                    ip_address VARCHAR(255),
                                                    sideA_image_path VARCHAR(255),
                                                    sideB_image_path VARCHAR(255),
                                                    sideC_image_path VARCHAR(255),
                                                    sideD_image_path VARCHAR(255),
                                                    sideE_image_path VARCHAR(255),
                                                    sideF_image_path VARCHAR(255),
                                                    sideG_image_path VARCHAR(255),
                                                    sideH_image_path VARCHAR(255),
                                                    image_path1_for_MES_to_fetch VARCHAR(255),
                                                    image_path2_for_MES_to_fetch VARCHAR(255),
                                                    image_path3_for_MES_to_fetch VARCHAR(255),
                                                    image_path4_for_MES_to_fetch VARCHAR(255),
                                                    image_path5_for_MES_to_fetch VARCHAR(255),
                                                    image_path6_for_MES_to_fetch VARCHAR(255),
                                                    image_path7_for_MES_to_fetch VARCHAR(255),
                                                    image_path8_for_MES_to_fetch VARCHAR(255),
                                                    image_path9_for_MES_to_fetch VARCHAR(255),
                                                    image_path10_for_MES_to_fetch VARCHAR(255),
                                                    image_path11_for_MES_to_fetch VARCHAR(255),
                                                    image_path12_for_MES_to_fetch VARCHAR(255),
                                                    image_path13_for_MES_to_fetch VARCHAR(255),
                                                    image_path14_for_MES_to_fetch VARCHAR(255),
                                                    image_path15_for_MES_to_fetch VARCHAR(255),
                                                    image_path16_for_MES_to_fetch VARCHAR(255),
                                                    image_path17_for_MES_to_fetch VARCHAR(255),
                                                    image_path18_for_MES_to_fetch VARCHAR(255),
                                                    image_path19_for_MES_to_fetch VARCHAR(255),
                                                    image_path20_for_MES_to_fetch VARCHAR(255),
                                                    image_path_for_MES_xiaochengxu_chatu TEXT,
                                                    r1 VARCHAR(255),
                                                    r2 VARCHAR(255),
                                                    r3 VARCHAR(255),
                                                    r4 VARCHAR(255),
                                                    r5 VARCHAR(255),
                                                    r6 VARCHAR(255),
                                                    r7 VARCHAR(255),
                                                    r8 VARCHAR(255),
                                                    r9 VARCHAR(255),
                                                    r10 VARCHAR(255),
                                                    create_time TIMESTAMP DEFAULT CURRENT_TIMESTAMP
                                                );";

                                            strSqlCommands[2] = @"
                                                CREATE TABLE IF NOT EXISTS " + strTables[2] + @" (
                                                    id INT,
                                                    set_id VARCHAR(255),
                                                    uuid VARCHAR(255),
                                                    product_id VARCHAR(255),
                                                    type VARCHAR(255),
                                                    width DECIMAL(10, 2),
                                                    height DECIMAL(10, 2),
                                                    area DECIMAL(10, 2),
                                                    center_x DECIMAL(10, 2),
                                                    center_y DECIMAL(10, 2),
                                                    side INT,
                                                    light_channel INT,
                                                    aiCam INT,
                                                    aiPos INT,
                                                    aiImageIndex INT,
                                                    r1 VARCHAR(255),
                                                    r2 VARCHAR(255),
                                                    r3 VARCHAR(255),
                                                    r4 VARCHAR(255),
                                                    r5 VARCHAR(255),
                                                    r6 VARCHAR(255),
                                                    r7 VARCHAR(255),
                                                    r8 VARCHAR(255),
                                                    r9 VARCHAR(255),
                                                    r10 VARCHAR(255),
                                                    create_time TIMESTAMP DEFAULT CURRENT_TIMESTAMP
                                                );";

                                            // transfer_aiInfo表在中转站即AVI机台电脑上，用于存储和AVI机台交互的单面AI信息
                                            //                  在复判站电脑上时，用于存储从中转发来的汇总AI信息
                                            strSqlCommands[3] = @"
                                                CREATE TABLE IF NOT EXISTS " + strTables[3] + @" (
                                                    side VARCHAR(10),
                                                    machine VARCHAR(100),
                                                    uuid VARCHAR(100),
                                                    barcode VARCHAR(100),
                                                    aiResult VARCHAR(10),
                                                    pointInfoJson TEXT,
                                                    createTime TIMESTAMP DEFAULT CURRENT_TIMESTAMP
                                                );";
                                        }
                                        break;
                                }
                            }

                            for (int n = 0; n < strTables.Length; n++)
                            {
                                if (false == Global.m_instance.m_mysql_ops.IsTableExist(strDatabase, strTables[n]))
                                {
                                    Global.m_instance.m_mysql_ops.CreateTable(strDatabase, strTables[n], strSqlCommands[n]);

                                    // 判断是否创建成功
                                    if (false == Global.m_instance.m_mysql_ops.IsTableExist(strDatabase, strTables[n]))
                                    {
                                        Debugger.Log(0, null, string.Format("222222 创建表格 {0} 失败", strTables[n]));
                                    }
                                    else
                                    {
                                        Debugger.Log(0, null, string.Format("222222 创建表格 {0} 成功", strTables[n]));
                                    }
                                }
                                else
                                {
                                    Debugger.Log(0, null, string.Format("222222 表格 {0} 已存在", strTables[n]));
                                }
                            }
                        }
                    }
                }
            }
        }

        // 初始化Http服务
        private void init_http_service()
        {
            string strIP = "*";
            int nPort = 8080;

            if (false == Global.m_instance.m_AVI_communication.IsPrefixInUse(Global.m_instance.m_AVI_data_collector_service_set_id) &&
                false == Global.m_instance.m_AVI_communication.IsPrefixInUse(Global.m_instance.m_AVI_data_collector_service_report_data) &&
				false == Global.m_instance.m_AVI_communication.IsPrefixInUse(Global.m_instance.m_AVI_data_collector_service_recheck_station_result) &&
				false == Global.m_instance.m_AVI_communication.IsPrefixInUse(Global.m_instance.m_AVI_data_collector_service_avi_finish) &&
                false == Global.m_instance.m_AVI_communication.IsPrefixInUse(Global.m_instance.m_AVI_data_sender_service_merge_side_request_data_forAI) &&
				false == Global.m_instance.m_AVI_communication.IsPrefixInUse(Global.m_instance.m_AVI_data_collector_service_single_side_report_data_forAI) &&
				false == Global.m_instance.m_AVI_communication.IsPrefixInUse(Global.m_instance.m_transfer_data_collector_service_report_data))
            {
                Global.m_instance.m_AVI_communication.InitWebService(strIP, nPort);
            }
        }

        private void StartBackgroundService()
        {
            Task.Run(() => BackgroundServiceTask());
        }

        private void BackgroundServiceTask()
        {
            while (true)
            {
                string strResponse = "";

                if (true == Global.m_instance.m_AVI_communication.m_webservice_listener.IsListening)
                {
                    //Debugger.Log(0, null, string.Format("222222 Http服务正处于 监听 姿态"));

                    ResponseFromRecheck reportData = new ResponseFromRecheck();

                    reportData.error_code = 0;
                    reportData.version = Global.m_instance.m_strVersion;
                    reportData.msg = "alive";
                    reportData.data_port = Global.m_instance.m_nWebapiPort;

                    string strDataToSend = JsonSerializer.Serialize(reportData);

                    MesHelper.SendMES(Global.m_instance.m_recheck_server_heartbeat_url, strDataToSend, ref strResponse, 1);
                }
                else
                {
                    //Debugger.Log(0, null, string.Format("222222 Http服务没有在监听"));

                    ResponseFromRecheck reportData = new ResponseFromRecheck();

                    reportData.error_code = 1;
                    reportData.msg = "Http服务没有在监听";

                    string strDataToSend = JsonSerializer.Serialize(reportData);

                    MesHelper.SendMES(Global.m_instance.m_recheck_server_report_url, strDataToSend, ref strResponse, 1);
                }

                Task.Delay(1000).Wait();
            }
        }
    }
}