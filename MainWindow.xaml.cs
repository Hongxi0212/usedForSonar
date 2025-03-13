using AutoScanFQCTest.Canvas;
using AutoScanFQCTest.DataModels;
using AutoScanFQCTest.DialogWindows;
using AutoScanFQCTest.Logics;
using AutoScanFQCTest.Utilities;
using AutoScanFQCTest.Views;
using Microsoft.Win32;
using MySqlX.XDevAPI.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Reflection;

using System.Runtime.InteropServices;

using System.Runtime.InteropServices;

using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WindowsFirewallHelper;
using static Mysqlx.Datatypes.Scalar.Types;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace AutoScanFQCTest
{
	public partial class MainWindow: Window
	{
        [DllImport("netapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern uint NetFwIsOn(out bool isOn);

        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr PostMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        [DllImport("user32.dll")]
        private static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        private static uint _currentProcessId = (uint)Process.GetCurrentProcess().Id;

        private const UInt32 WM_CLOSE = 0x0010;
        private const int MaxTitleLength = 255;

		public Global m_global = null;

		private int m_nDataSendingStatusFlag = 0;      // 0:数据重发线程未启动或者已经结束，1:数据重发线程正在运行

		public MainWindow()
		{
			InitializeComponent();

			//this.Title = "苏粤次元世纪---汽车骨架焊接检测";

			// 初始化全局数据模型
			m_global = new Global(this);

			// 用户登录
			if (false)
			{
				LoginWindow dialog = new LoginWindow(this);

				dialog.ShowDialog();

				if (false == dialog.m_bIsValidUser)
				{
					// 提示用户登录失败
					MessageBox.Show("登录失败，程序即将退出", "提示", MessageBoxButton.OK, MessageBoxImage.Information);

					this.Close();
				}
			}

			page_HomeView.m_parent = this;

			// 设置训练日志呈现类对象的输出控件
			m_global.m_log_presenter.set_textbox_object(page_HomeView.textbox_RunningInfo, page_HomeView.textbox_ErrorInfo);
			m_global.m_log_presenter.Log("程序启动");

			//m_global.m_log_presenter.LogError("程序启动");

			// 加载配置信息
			m_global.LoadConfigData("config.ini");

			// 加载三级用户权限信息
			m_global.LoadUserData("user.ini");

			// 加载缺陷名称映射表
			m_global.LoadDefectNameMappingTable("defect_name_mapping.ini");

			//this.Title = this.Title + " " + m_global.m_strProductType + " 1.21";
			this.statusbar_task_info.Text = "操作员信息: " + m_global.m_strCurrentOperatorID;

			//m_global.m_bIsInTestingMode = true;

			// 隐藏主界面
			if (false == m_global.m_bIsInTestingMode && true == m_global.m_bIsCardReaderEnabled)
				page_HomeView.Visibility = Visibility.Hidden;

			page_HomeView.UpdateStatistics();

			if (false)
			{
				string currentDirectory = Directory.GetCurrentDirectory();
				string filePath = System.IO.Path.Combine(currentDirectory, "user.csv");
				string delimiter = ",";

				string[][] data = new string[][]
				{
				new string[] { "Username", "Password" },
				new string[] { "john.doe", "password123" },
				new string[] { "jane.smith", "qwerty456" }
				};

				int length = data.GetLength(0);

				using (StreamWriter writer = new StreamWriter(filePath))
				{
					for (int index = 0; index < length; index++)
					{
						string line = string.Join(delimiter, data[index]);
						writer.WriteLine(line);
					}
				}
			}

			if (false)
			{
				string currentDirectory = Directory.GetCurrentDirectory();
				string filePath = System.IO.Path.Combine(currentDirectory, "user.csv");
				char delimiter = ',';

				Dictionary<string, string> data = new Dictionary<string, string>();

				if (File.Exists(filePath))
				{
					using (StreamReader reader = new StreamReader(filePath))
					{
						// 读取并忽略第一行(列标题)
						reader.ReadLine();

						string line;
						while ((line = reader.ReadLine()) != null)
						{
							string[] values = line.Split(delimiter);
							if (values.Length == 2)
							{
								string username = values[0].Trim();
								string password = values[1].Trim();
								data[username] = password;
							}
						}
					}
				}
				else
				{
					MessageBox.Show("CSV file does not exist.");
				}

				// 用m_global.m_log_presenter.Log输出data的内容
				foreach (var pair in data)
				{
					m_global.m_log_presenter.Log(pair.Key + " " + pair.Value);
				}
			}

			//this.Title = this.Title + " " + m_global.m_strProductType + " " + m_global.m_strVersion;

			// 设置窗口事件
			this.Loaded += Window_Loaded;
			this.SizeChanged += Window_SizeChanged;
			this.Closing += Window_Closing;
			this.ContentRendered += Window_ContentRendered;
		}

		// 窗口加载
		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			// 创建一个json字段，保存到文件
			if (false)
			{
				string strJson = "{\"name\":\"弟弟\",\"age\":30,\"city\":\"New York\"}";

				// 将json字符串写入文件
				string strFilePath = "test.json";
				File.WriteAllText(strFilePath, strJson);

				// 读取json文件
				string strJsonFromFile = File.ReadAllText(strFilePath);

				// 解析json字符串
			}

			// 设置开机自启动
			if (false)
			{
				Microsoft.Win32.RegistryKey rk = Microsoft.Win32.Registry.CurrentUser;

				rk.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run");
				rk = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);

				string strExePath = System.Windows.Forms.Application.ExecutablePath.ToString();

				rk.SetValue("AutoScanFQCTest", strExePath);
			}

			// 设置窗口位置
			this.Left = SystemParameters.WorkArea.Left;
			this.Top = SystemParameters.WorkArea.Top;
			this.Width = SystemParameters.WorkArea.Width;
			this.Height = SystemParameters.WorkArea.Height;

			// 获取屏幕DPI比例
			if (true)
			{
				PresentationSource presentationSource = PresentationSource.FromVisual(System.Windows.Application.Current.MainWindow);

				double scaleX = 1;
				double scaleY = 1;

				// 获取屏幕DPI缩放比例，并根据缩放比例调整控件大小
				if (presentationSource != null)
				{
					Matrix m = presentationSource.CompositionTarget.TransformToDevice;
					double dpiX = m.M11 * 96.0;
					double dpiY = m.M22 * 96.0;

					m_global.m_dbScreenDPIScale = dpiX / 96.0;
				}
			}

			// 导入上一次选择的图片目录
			if (true)
			{
				for (int n = 0; n < 2; n++)
				{
					if (page_HomeView.m_camera_canvases[n] != null)
					{
						//if (true == File.Exists(m_global.m_strLastOpenImagePath))
						//{
						//    BaseCanvas<CameraCanvas>.OpenImageFileAndShowOnBIVCanvas(m_global.m_strLastOpenImagePath, m_global.m_nImageRotationAngle, page_HomeView.m_camera_canvases[n]);

						//    page_HomeView.m_camera_canvases[n].show_whole_image();
						//}
						//else
						{
							if (0 == n)
								BaseCanvas<CameraCanvas>.OpenImageFileAndShowOnBIVCanvas("1.png", m_global.m_nImageRotationAngle, page_HomeView.m_camera_canvases[n]);
							else if (1 == n)
								BaseCanvas<CameraCanvas>.OpenImageFileAndShowOnBIVCanvas("2.png", m_global.m_nImageRotationAngle, page_HomeView.m_camera_canvases[n]);

							m_global.m_strLastOpenImagePath = "1.png";

							page_HomeView.m_camera_canvases[n].show_whole_image();
						}

						page_HomeView.m_camera_canvases[n].show_whole_image();
					}
				}
			}

			// 连接MySQL数据库
			if (true == m_global.m_mysql_ops.IsMySQLServiceInstalled())
			{
				if (true == m_global.m_mysql_ops.ConnectToMySQL("root", "123456", m_global.m_nMysqlPort))
				{
					m_global.m_log_presenter.Log("成功连接到MySQL数据库");

					// 判断数据库是否存在
					string strDatabase = "AutoScanFQCTest";
					bool bIsDatabaseExist = false;
					if (true)
					{
						if (false == m_global.m_mysql_ops.IsDatabaseExist(strDatabase))
						{
							m_global.m_mysql_ops.CreateDatabase(strDatabase);

							// 判断是否创建成功
							if (false == m_global.m_mysql_ops.IsDatabaseExist(strDatabase))
							{
								m_global.m_log_presenter.Log("创建数据库失败");
							}
							else
							{
								m_global.m_log_presenter.Log("创建数据库 " + strDatabase + " 成功");

								bIsDatabaseExist = true;
							}
						}
						else
						{
							m_global.m_log_presenter.Log("数据库 " + strDatabase + " 已存在");

							bIsDatabaseExist = true;
						}
					}

					// 连接到数据库
					if (true == bIsDatabaseExist)
					{
						if (false == m_global.m_mysql_ops.ConnectToDatabase(strDatabase))
						{
							m_global.m_log_presenter.Log("连接到数据库 " + strDatabase + " 失败");

							m_global.m_bIsConnectedToDatabase = false;
						}
						else
						{
							m_global.m_log_presenter.Log("成功连接到数据库 " + strDatabase);

							m_global.m_bIsConnectedToDatabase = true;
						}
					}

					// 如果数据库存在，判断表格是否存在
					if (true == m_global.m_bIsConnectedToDatabase)
					{
						// 遍历m_global.m_dict_machine_names_and_IPs的keys，创建表格
						for (int t = 0; t < m_global.m_dict_machine_names_and_IPs.Count; t++)
						{
							string strMachineName = m_global.m_dict_machine_names_and_IPs.Keys.ElementAt(t);

							// 如果strMachineName不为空，创建表格
							if (true == string.IsNullOrEmpty(strMachineName))
								continue;

							string strTrayTable = "trays_" + strMachineName;
							string strProductTable = "products_" + strMachineName;
							string strDefectTable = "details_" + strMachineName;
							string strTableOfStatisticsByDefects = "defects_statistics_" + strMachineName;

							string[] strTables = new string[8] { strTrayTable, strProductTable, strDefectTable, "ProductResult", "DefectCount", "Statistics", "RecheckRecord", strTableOfStatisticsByDefects };

							string[] strSqlCommands = new string[8];

							// 如果strTables文本包含"-"，替换为"_"
							for (int i = 0; i < strTables.Length; i++)
							{
								if (true == strTables[i].Contains("-"))
								{
									strTables[i] = strTables[i].Replace("-", "_");
								}
							}

							if (true)
							{
								switch (m_global.m_strProductType)
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
                                                recheck_result VARCHAR(255),
                                                ai_result VARCHAR(255),
                                                ai_time VARCHAR(255),
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

										strSqlCommands[3] = @"
                                            CREATE TABLE IF NOT EXISTS " + strTables[3] + @" (
                                                SetId VARCHAR(255),
                                                BarCode VARCHAR(255),
                                                Result VARCHAR(255),
                                                TestDate VARCHAR(255),
                                                Tester VARCHAR(255)
                                            );";

										strSqlCommands[4] = @"
                                            CREATE TABLE IF NOT EXISTS " + strTables[4] + @" (
                                                DefectName VARCHAR(255),
                                                Count int default 0
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
                                                all_image_paths LONGTEXT,
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
												image_path VARCHAR(255),
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

										strSqlCommands[3] = @"
                                            CREATE TABLE IF NOT EXISTS " + strTables[3] + @" (
                                                SetId VARCHAR(255),
                                                BarCode VARCHAR(255),
                                                Result VARCHAR(255),
                                                TestDate VARCHAR(255),
                                                Tester VARCHAR(255)
                                            );";

										strSqlCommands[4] = @"
                                            CREATE TABLE IF NOT EXISTS " + strTables[4] + @" (
                                                DefectName VARCHAR(255),
                                                Count int default 0
                                            );";
									}
									break;
								}

								strSqlCommands[5] = @"
                                    CREATE TABLE IF NOT EXISTS " + strTables[5] + @" (
                                        total_trays INT,
                                        total_products INT,
                                        total_defects INT,
                                        total_OK_products INT,
                                        total_NG_products INT,
                                        r1 VARCHAR(255),
                                        r2 VARCHAR(255),
                                        r3 VARCHAR(255),
                                        r4 VARCHAR(255),
                                        r5 VARCHAR(255),
                                        r6 VARCHAR(255),
                                        r7 VARCHAR(255),
                                        r8 VARCHAR(255),
                                        r9 VARCHAR(255),
                                        r10 VARCHAR(255)
                                    );";

								strSqlCommands[6] = @"
                                    CREATE TABLE IF NOT EXISTS " + strTables[6] + @" (
                                        SetID VARCHAR(255) PRIMARY KEY,
                                        JsonData JSON DEFAULT NULL,
                                        CreateTime TIMESTAMP DEFAULT CURRENT_TIMESTAMP
                                    );";

								// 机台缺陷统计表
								// 联合主键, tray_id和product_id是否要联合
								strSqlCommands[7] = @"
                                    CREATE TABLE IF NOT EXISTS " + strTables[7] + @" (
                                        machine_id VARCHAR(20) NOT NULL,
                                        product_id VARCHAR(20),
                                        tray_id VARCHAR(50) NOT NULL,
                                        barcode VARCHAR(50) NOT NULL,
                                        defect_type VARCHAR(50) NOT NULL,
                                        time_block DATETIME NOT NULL,
                                        ng_count INT DEFAULT 0,
                                        ok_count INT DEFAULT 0
                                    );

                                    ALTER TABLE " + strTables[7] +
									@" ADD UNIQUE INDEX idx_full_mulkey (
                                        machine_id, tray_id, barcode, defect_type, time_block
                                    );";

								for (int n = 0; n < strTables.Length; n++)
								{
									if (false == m_global.m_mysql_ops.IsTableExist(strDatabase, strTables[n]))
									{
										m_global.m_mysql_ops.CreateTable(strDatabase, strTables[n], strSqlCommands[n]);

										// 判断是否创建成功
										if (false == m_global.m_mysql_ops.IsTableExist(strDatabase, strTables[n]))
										{
											m_global.m_log_presenter.Log("创建表格 " + strTables[n] + " 失败");
										}
										else
										{
											m_global.m_log_presenter.Log("创建表格 " + strTables[n] + " 成功");
										}
									}
									else
									{
										m_global.m_log_presenter.Log("表格 " + strTables[n] + " 已存在");
									}
								}

								// 查询统计数据
								if (true == m_global.m_mysql_ops.IsTableExist(strDatabase, strTables[5]))
								{
								}
							}
						}
					}
				}
				else
				{
					m_global.m_log_presenter.Log("无法连接到MySQL数据库");
				}
			}
			else
			{
				m_global.m_log_presenter.Log("警告：MySQL数据库服务未安装");
			}

			// 启动后台线程
			if (true)
			{
				(new Thread(m_global.m_AVI_communication.thread_monitor_background_service)).Start();
			}

			// 启动AVI通信对象的TCP服务器
			if (true)
			{
				string strIP = "*";
				int nPort = 8080;

				//if (m_global.m_strProductType == "dock")
				if (true)
				{
					nPort = 8180;

					m_global.m_http_service_url_of_set_id = "http://*:8180/data_collect_message/";
					m_global.m_http_service_url_of_background_heartbeat = "http://*:8180/heartbeat/";
				}

				if (false == m_global.m_AVI_communication.IsPrefixInUse(m_global.m_http_service_url_of_set_id) &&
					false == m_global.m_AVI_communication.IsPrefixInUse(m_global.m_http_service_url_of_background_heartbeat))
				{
					m_global.m_AVI_communication.InitTCPServer(m_global.m_strTCP_IP_address_for_communication_with_card_reader, m_global.m_nTCP_port_for_communication_with_card_reader);

					//m_global.m_AVI_communication.InitTCPServer(strIP, nPort);
					m_global.m_AVI_communication.InitWebService(strIP, nPort);
				}
				else
				{
					m_global.m_log_presenter.Log("警告：Http Webservice 端口 " + nPort + " 已被占用，无法启动");

					MessageBox.Show("警告：Http Webservice 端口 " + nPort + " 已被占用，无法启动。请检查原因并解决，否则程序无法正常工作！", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
				}
			}

			// 连接MES
			if (true)
			{
				//m_global.m_MES_handler.Login("72303002", "wY123456");
				//m_global.m_MES_handler.Test();

				//string strIP = "10.1.34.39";
				//string strIP = "10.13.5.254";
				string strIP = m_global.m_strMesIP;
				int nPort = m_global.m_nMesPort;

				if (true == Logics.MESService.IsPortOpen(strIP, nPort, 1500))
				{
					m_global.m_bIsMesConnected = true;

					page_HomeView.textblock_MesConnectStatus.Text = "已连接";

					m_global.m_log_presenter.Log("IP端口为 " + strIP + ":" + nPort + " 的MES系统已成功连接");
				}
				else
				{
					m_global.m_bIsMesConnected = false;

					page_HomeView.textblock_MesConnectStatus.Text = "未连接";

					m_global.m_log_presenter.Log("IP端口为 " + strIP + ":" + nPort + " 的MES系统连接失败");
				}

				m_global.m_log_presenter.Log("MES接收复判数据接口地址为： " + m_global.m_strMesDataUploadUrl);
			}

			// 当前产品类型
			m_global.m_log_presenter.Log("本机复判产品类型：" + m_global.m_strProductType);

			if (m_global.m_strProductSubType == "glue_check")
			{
				page_HomeView.textblock_UncheckedTrays.Visibility = Visibility.Visible;
				page_HomeView.combo_SelectUncheckedTrays.Visibility = Visibility.Visible;
				page_HomeView.BtnClearCountButtons.Visibility = Visibility.Collapsed;
				page_HomeView.BtnClearProductButtons.Visibility = Visibility.Collapsed;
				page_HomeView.BtnRequestData.Visibility = Visibility.Collapsed;
				page_HomeView.grid_ProductInfo.Visibility = Visibility.Collapsed;
				page_HomeView.panel_RotationSelection.Visibility = Visibility.Collapsed;
				page_HomeView.grid_borders_currentPieceInfo.Visibility = Visibility.Collapsed;
				page_HomeView.grid_productsOfTray.Visibility = Visibility.Collapsed;

				page_HomeView.BtnConfirmOK.Background = new SolidColorBrush(Colors.Green);
				page_HomeView.BtnConfirmNG.Background = new SolidColorBrush(Colors.Red);

				page_HomeView.grid_ColorIllustration.Visibility = Visibility.Collapsed;

				var columnDefinitions = page_HomeView.SubmitMesRow.ColumnDefinitions;
				columnDefinitions[0].Width = new GridLength(0, GridUnitType.Star);
				columnDefinitions[1].Width = new GridLength(1, GridUnitType.Star);
				var mesBtnParent = VisualTreeHelper.GetParent(page_HomeView.BtnSubmit) as Grid;
				mesBtnParent.Margin = new Thickness(0, 0, 0, 0);

				page_HomeView.textblock_TrayBarcodeText.Text = "料号";
				page_HomeView.textblock_TrayBarcode.Text = "";
			}

			if (m_global.m_strSiteCity == "盐城")
			{
				page_HomeView.UnrecheckableColor.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(128, 0, 0));
                page_HomeView.FailedPositioningColor.Background = System.Windows.Media.Brushes.Purple;

            }

			// 复判数据查询模式
			if (m_global.m_recheck_data_query_mode == RecheckDataQueryMode.ByQueryUncheckedFlag)
			{
				page_HomeView.textblock_UncheckedTrays.Visibility = Visibility.Visible;
				page_HomeView.combo_SelectUncheckedTrays.Visibility = Visibility.Visible;
			}

			// 是否可以修改不可复判项
			if (!m_global.m_bUncheckableDefectModifiable)
			{
				menuitem_UnRecheckableItemSetting.Visibility = Visibility.Collapsed;
			}

			// 在UI上显示不可复判项启用的数量
			var uncheckableItemsEnableCount = 0;
			for (int i = 0; i < m_global.m_uncheckable_defect_types.Count; i++)
			{
				if (true == m_global.m_uncheckable_defect_enable_flags[i])
				{
					uncheckableItemsEnableCount++;
				}
			}
			page_HomeView.textBlock_uncheckableItemsCount.Text = uncheckableItemsEnableCount.ToString();

			if (m_global.m_defectStatisticsQuery == null)
			{
				m_global.m_defectStatisticsQuery = new DefectStatisticsQueryWindow(this);
			}
			m_global.m_defectStatisticsQuery.Hide();

			//var tcs = new TaskCompletionSource<bool>();
			//m_global.m_defectStatisticsQuery.TriggerSynchronizeUI(tcs);

			// 启动未复判料盘查询线程
			if (m_global.m_strProductSubType == "glue_check" || m_global.m_recheck_data_query_mode == RecheckDataQueryMode.ByQueryUncheckedFlag)
				(new Thread(thread_detect_unchecked_trays)).Start();

			// 启动早上8点和晚上8点换班，切换班别的线程
			(new Thread(thread_monitor_shift_change)).Start();

			// 如果作为中转站将检测结果发送给复判站，则启动无数据重发监控线程
			if (m_global.m_bSendInspectionResultToRecheckStation == true)
				(new Thread(thread_monitor_recheck_data_sending_status)).Start();

			// 启动检测消息框线程
			if (m_global.m_nRecheckModeWithAISystem == 3)
				(new Thread(thread_detect_message_box)).Start();

			// 启动图片目录清除线程
			(new Thread(thread_recycle_old_images)).Start();

			// 启动AVI机台联网状态查询线程
			(new Thread(thread_detect_AVIMachineConnectStatus)).Start();

			// 检测win10的系统防火墙是否处于开启或者关闭状态
			(new Thread(thread_detect_windows_firewall_status)).Start();

			// 启动AI提交线程
			if (m_global.m_nRecheckModeWithAISystem == 2)
			{
				(new Thread(m_global.m_MES_service.thread_handle_productInfo_toAI_queue)).Start();
			}

			// 纯AI复判模式下，启动AI等待复判结果监控线程
			if (m_global.m_nRecheckModeWithAISystem == 3)
			{
				(new Thread(page_HomeView.thread_AI_wait_to_recheck_monitor)).Start();
			}

			// 读卡器
			if (true == m_global.m_bIsCardReaderEnabled)
			{
				try
				{
					m_global.m_card_reader = new ScanCard(ScanCard.Brand.YW60x);

					m_global.m_card_reader.Open();

					m_global.m_card_reader.OnScanCardEvent += ScanCard_OnScanCardEvent;

					//               if (m_global.ThreeLevelUserPermission == null)
					//               {
					//                   m_global.ThreeLevelUserPermission = new ThreeLevelUserPermission(this);
					//               }

					//               m_global.ThreeLevelUserPermission.Hide();
					//               //m_global.ThreeLevelUserPermission.UnregisterThreeLevelPageScanCardEvent();
					//m_global.ThreeLevelCardReader.Close();

					m_global.ThreeLevelCardReader = new ScanCard(ScanCard.Brand.YW60x);

					m_global.ThreeLevelCardReader.Open();

					if (m_global.ThreeLevelUserPermission == null)
					{
						m_global.ThreeLevelUserPermission = new ThreeLevelUserPermission(this);
					}

					//m_global.ThreeLevelCardReader.OnScanCardEvent += m_global.ThreeLevelUserPermission.FillTreeLevelUserName_OnScanCardEvent;
                }
                catch (Exception ex)
				{
					m_global.m_log_presenter.Log("读卡器初始化失败：" + ex.Message);

					m_global.m_bIsCardReaderEnabled = false;
				}
			}

            // 打开应用时更新操作权限
            page_HomeView.UpdateOperatorRight();
		}

		// 窗口大小改变
		private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			// 当用户尝试改变窗口大小时，重新将窗口状态设置为最大化
			if (this.WindowState != WindowState.Maximized)
			{
				this.WindowState = WindowState.Maximized;

				e.Handled = true;
			}
		}

		// 窗口内容渲染完成
		private void Window_ContentRendered(object sender, EventArgs e)
		{
			m_global.m_bIsMainWindowInited = true;

			refresh_UI_controls_by_user_level(m_global.m_current_user);
		}

		// 窗口关闭
		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (true == m_global.m_bSendInspectionResultToRecheckStation)
			{
				// 弹出警告框
				MessageBox.Show(this, "本程序作为数据重发软件，负责发送数据给复判站，不得关闭本软件！", "警告", MessageBoxButton.YesNo);
				e.Cancel = true;

				return;
			}

			m_global.m_bExitProgram = true;

			// 保存配置信息
			m_global.SaveConfigData("config.ini");

			// 退出AVI通信对象
			m_global.m_AVI_communication.Exit();

			// 退出MySQL数据库
			m_global.m_mysql_ops.DisconnectFromMySQL();

			// 延迟500ms
			System.Threading.Thread.Sleep(500);

			System.Environment.Exit(0);
		}

		// 窗口按键预览事件
		private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			if (e.OriginalSource is System.Windows.Controls.ComboBox)
			{
				e.Handled = true;
				return;
			}

			// 如果是enter键，标记为已处理，不再传递，避免触发默认按钮，导致界面切换，并返回
			if (e.Key == Key.Enter)
			{
				e.Handled = true;

				return;
			}

			if (e.Key == Key.Up || e.Key == Key.NumPad1 || e.Key == Key.End)
			{
				page_HomeView.BtnPreviousDefect_Click(sender, e);
			}
			else if (e.Key == Key.Down || e.Key == Key.NumPad2)
			{
				page_HomeView.BtnNextDefect_Click(sender, e);
			}
			else if (e.Key == Key.Left)
			{
				page_HomeView.BtnConfirmOK_Click(sender, e);
			}
			else if (e.Key == Key.Right)
			{
				page_HomeView.BtnConfirmNG_Click(sender, e);
			}
			else if (e.Key == Key.Delete || e.Key == Key.NumPad0)
			{
				// 如果是del键，清空产品按钮
				page_HomeView.BtnClearProductButtons_Click(sender, e);
			}
		}

		// 窗口按键事件
		private void Window_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.OriginalSource is System.Windows.Controls.TextBox)
			{
				// 判断原控件名称是否为textbox_ScannedBarcode，如果是，返回
				if ((e.OriginalSource as System.Windows.Controls.TextBox).Name == "textbox_ScannedBarcode")
					return;
			}

			// 处理键盘按下事件
			if (e.Key == Key.D1 || e.Key == Key.D2 || e.Key == Key.D3 || e.Key == Key.D4 || e.Key == Key.D5 || e.Key == Key.D6 || e.Key == Key.D7 || e.Key == Key.D8 || e.Key == Key.D9 || e.Key == Key.D0)
			{
				int nLightChannel = (int)e.Key - 34;

				if (nLightChannel >= 0 && nLightChannel < 10)
				{
					if (m_global.m_strProductType != "nova")
					{
						if (true)
						{
							TrayInfo tray_info = m_global.m_current_tray_info_for_Dock;

							if (null != tray_info.products[page_HomeView.m_nCurrentDefectedProductIndex].defects
								&& page_HomeView.m_nCurrentDefectIndex < tray_info.products[page_HomeView.m_nCurrentDefectedProductIndex].defects.Count)
							{
								DefectInfo defect = tray_info.products[page_HomeView.m_nCurrentDefectedProductIndex].defects[page_HomeView.m_nCurrentDefectIndex];

								if (defect != null)
								{
									int nReferenceChannel = defect.light_channel;

									int nBaseChannel = (nReferenceChannel / 10) * 10;

									nLightChannel = nBaseChannel + nLightChannel;
								}
							}
						}
						else
							nLightChannel = (page_HomeView.m_nCurrentSideAorB) * 10 + nLightChannel;
					}

					// 切换光源通道
					if (m_global.m_strProductType == "nova")
						page_HomeView.SwitchLightChannelForNova(nLightChannel);
					else if (m_global.m_strProductType == "dock")
					{
						bool bIsImageExisting = false;

						page_HomeView.SwitchLightChannelForDock(nLightChannel, ref bIsImageExisting);
					}

					int nImageWidth = page_HomeView.m_camera_canvases[0].get_origin_image_width();
					int nImageHeight = page_HomeView.m_camera_canvases[0].get_origin_image_height();

					// 显示缺陷
					if (nImageWidth > 0 && nImageHeight > 0)
					{
						if (m_global.m_strProductType == "nova")
						{
							AVITrayInfo tray_info = m_global.m_current_tray_info_for_Nova;

							if (null != tray_info.Products[page_HomeView.m_nCurrentDefectedProductIndex].Defects)
							{
								page_HomeView.ShowDefectOnCanvasForNova(page_HomeView.m_nCurrentDefectedProductIndex, page_HomeView.m_nCurrentDefectIndex, nImageWidth, nImageHeight);
							}
						}
						else if (m_global.m_strProductType == "dock")
						{
							TrayInfo tray_info = m_global.m_current_tray_info_for_Dock;

							if (null != tray_info.products[page_HomeView.m_nCurrentDefectedProductIndex].defects)
							{
								page_HomeView.ShowDefectOnCanvasForDock(page_HomeView.m_nCurrentDefectedProductIndex, page_HomeView.m_nCurrentDefectIndex, nImageWidth, nImageHeight);
							}
						}
					}

					m_global.m_log_presenter.Log("当前光源通道：" + nLightChannel);
				}
			}
		}

		// 窗口按键释放事件
		private void Window_KeyUp(object sender, KeyEventArgs e)
		{
			// 处理键盘释放事件
		}

		// 根据用户级别刷新界面控件
		public void refresh_UI_controls_by_user_level(User user)
		{
			int nGroupID = user.m_nUserGroupId;

			for (int k = 0; k < m_global.m_user_groups.Count; k++)
			{
				UserGroup user_group = m_global.m_user_groups[k];

				if (user_group.m_nGroupID == nGroupID)
				{
					for (int i = 0; i < user_group.m_bUserPermissions.Length; i++)
					{
						if (true == user_group.m_bUserPermissions[i])
						{
							switch (i)
							{
							case (int)UserPermission.CreateTask:
							{
								btnCreateNewTask.IsEnabled = true;
								//page_HomeView.BtnCreateTask.IsEnabled = true;
								break;
							}
							case (int)UserPermission.LoadTask:
							{
								btnLoadTask.IsEnabled = true;
								//page_HomeView.BtnLoadTask.IsEnabled = true;
								break;
							}
							}
						}
						else
						{
							switch (i)
							{
							case (int)UserPermission.CreateTask:
							{
								btnCreateNewTask.IsEnabled = false;
								//page_HomeView.BtnCreateTask.IsEnabled = false;
								break;
							}
							case (int)UserPermission.LoadTask:
							{
								btnLoadTask.IsEnabled = false;
								//page_HomeView.BtnLoadTask.IsEnabled = false;
								break;
							}
							}
						}
					}
				}
			}
		}

		// 菜单项点击事件：查询数据库
		private void menuitem_QueryDatabase_Click(object sender, RoutedEventArgs e)
		{
			QueryDatabase dialog = new QueryDatabase(this);

			dialog.ShowDialog();
		}

		// 菜单项点击事件：选择目录
		private void menuitem_SelectFolders_Click(object sender, RoutedEventArgs e)
		{
			SelectFolders dialog = new SelectFolders(this);

			dialog.ShowDialog();
		}

		// 菜单项点击事件：用户权限管理
		private void menuitem_UserPermissionManagement_Click(object sender, RoutedEventArgs e)
		{
			//UserPermissionManagement dialog = new UserPermissionManagement(this);
			//ThreeLevelUserPermission dialog = new ThreeLevelUserPermission(this);

			//dialog.ShowDialog();

			if (m_global.ThreeLevelUserPermission == null)
			{
				m_global.ThreeLevelUserPermission = new ThreeLevelUserPermission(this);
			}

            m_global.ThreeLevelUserPermission.Show();
		}

		// 菜单项点击事件：不可复判项设置
		private void menuitem_UnRecheckableItemSetting_Click(object sender, RoutedEventArgs e)
		{
			PasswordValidation passwordValidation = new PasswordValidation(this, "不可复判项设置");

			//passwordValidation.ShowDialog();

			if (true == passwordValidation.m_bResultOK)
			{
				UncheckableDefectSetting dialog = new UncheckableDefectSetting(this);

				dialog.ShowDialog();
			}
		}

		// 菜单项点击事件：缺陷名称映射设置
		private void menuitem_DefectNameMapping_Click(object sender, RoutedEventArgs e)
		{
			//PasswordValidation passwordValidation = new PasswordValidation(this, "请输入管理密码：");

			//passwordValidation.ShowDialog();

			//if (true == passwordValidation.m_bResultOK)
			{
				DefectNameMapping defectNameMapping = new DefectNameMapping(this);

				defectNameMapping.ShowDialog();
			}
		}

		// 菜单项点击事件：通用设置
		private void menuitem_GeneralSetting_Click(object sender, RoutedEventArgs e)
		{
			PasswordValidation passwordValidation = new PasswordValidation(this, "一般设置");

			//passwordValidation.ShowDialog();

			if (true == passwordValidation.m_bResultOK)
			{
				GeneralSetting dialog = new GeneralSetting(this);

				dialog.ShowDialog();
			}
		}

		// 菜单项点击事件：手动扫码提交OK数据
		private void menuitem_ManuallyScanBarcodeAndSubmit_Click(object sender, RoutedEventArgs e)
		{
			PasswordValidation passwordValidation = new PasswordValidation(this, "测试");

			passwordValidation.ShowDialog();

			if (true == passwordValidation.m_bResultOK)
			{
				ManualScanBarcodeAndSubmitMES dialog = new ManualScanBarcodeAndSubmitMES(this);

				dialog.ShowDialog();
			}
		}

        // 菜单项点击事件：更换登录人
        private void btnChangeUser_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow login = new LoginWindow(this);
            login.ShowDialog();
        }

        // 菜单项点击事件：打开图片
        private void menuitem_OpenImage1_Click(object sender, RoutedEventArgs e)
        {
            OpenImageAndShowOnCanvas(0);
        }

        // 功能：打开图片并显示在画布上
        public void OpenImageAndShowOnCanvas(int nSceneType)
        {
            bool bHasDefaultDir = false;
            if (null != m_global.m_strOpenImageBrowseDir)
            {
                if ((m_global.m_strOpenImageBrowseDir.Length > 0) && (Directory.Exists(m_global.m_strOpenImageBrowseDir)))
                    bHasDefaultDir = true;
            }

            System.Windows.Forms.OpenFileDialog dialog = new System.Windows.Forms.OpenFileDialog();

            if (bHasDefaultDir)
                dialog.InitialDirectory = m_global.m_strOpenImageBrowseDir;
            else
                dialog.InitialDirectory = ".";

            dialog.Title = "选择图片文件";
            dialog.Filter = "图片文件|*.bmp;*.jpg;*.jpeg;*.png;*.jfif;*.webp";

            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.Cancel)
                return;
            else
            {
                m_global.m_strOpenImageBrowseDir = dialog.FileName.Substring(0, dialog.FileName.LastIndexOf('\\'));

                page_HomeView.OpenAndShowImageOnCanvas(dialog.FileName, nSceneType);

                m_global.m_strLastOpenImagePath = dialog.FileName;
            }
        }

		// 刷卡器扫描委托
		private void ScanCard_OnScanCardEvent(ScanCard.State state, string IC)
		{
			try
			{
				if (state == ScanCard.State.Online)
				{
					this.Dispatcher.BeginInvoke(new Action(() => {
						string Result = string.Empty;

						m_global.m_log_presenter.Log(string.Format("在主页面触发读卡器，内容：{0}", IC));

						if (m_global.m_strSiteCity == "苏州")
							MFlex.GetUserAccountByIC(MFlex.MesAddress.SZ, IC, m_global.m_strCurrentPC_AnyAVIName, out Result);
						else if (m_global.m_strSiteCity == "盐城")
							MFlex.GetUserAccountByIC(MFlex.MesAddress.YC, IC, m_global.m_strCurrentPC_AnyAVIName, out Result);

						if (!string.IsNullOrEmpty(Result))
						{
							try
							{

								//if (m_global.ThreeLevelUserPermission.IsFocused)
								if (m_global.ThreeLevelUserPermission.IsActive)
								{
									m_global.ThreeLevelUserPermission.textbox_EngineerName.Text = Result;

									// 记录登录信息和时间
									m_global.m_log_presenter.Log("操作员 " + m_global.m_strCurrentOperatorID + " 于 " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " 在三级权限刷入ID " + m_global.m_strCurrentPC_AnyAVIName);
								}
								else
								{
									m_global.m_strCurrentOperatorID = Result;

									page_HomeView.textblock_OperatorID.Text = Result;
									//page_HomeView.TextBlock_UserType_Name.Text = Result;

									var users = m_global.FupanUsers.Where(user => user.UserName == Result).ToList();
									if (users.Count() > 0)
                                    {
                                        page_HomeView.TextBlock_UserType_Name.Text = m_global.FupanUsers
                                        .Where(user => user.UserName == Result).ToList().FirstOrDefault().UserType.ToString();
                                    }
                                    else
                                    {
										page_HomeView.TextBlock_UserType_Name.Text = UserType.操作员.ToString();
                                    }

									page_HomeView.Visibility = Visibility.Visible;

									this.statusbar_task_info.Text = "操作员信息: " + m_global.m_strCurrentOperatorID;

									// 记录登录信息和时间
									m_global.m_log_presenter.Log("操作员 " + m_global.m_strCurrentOperatorID + " 于 " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " 登录机台复判站 " + m_global.m_strCurrentPC_AnyAVIName);

								}
							}
							catch (Exception e)
							{
								m_global.m_log_presenter.LogError("刷卡登录出现异常：" + e.Message);
							}
						}
					}));
				}
				else
				{
					if (false == m_global.m_bIsInTestingMode)
					{
						this.Dispatcher.BeginInvoke(new Action(() => {
							page_HomeView.Visibility = Visibility.Hidden;

							this.statusbar_task_info.Text = "操作员信息: 未登录";

							// 记录退出登录信息和时间
							m_global.m_log_presenter.Log("操作员 " + m_global.m_strCurrentOperatorID + " 于 " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " 退出登录");
						}));
					}
				}
			}
			catch (Exception ex)
			{
			}
		}

		// 按钮点击事件：测试
		private void BtnTest_Click(object sender, RoutedEventArgs e)
		{
			if (false)
			{
				string networkPath = @"\\192.168.31.31\test";
				string username = "Everyone";
				string domain = "WIN-E3RVJF34OGD"; // 如果是本地账户，domain可以是机器名或者留空
				string password = "";

				try
				{
					NetworkShareAccesser.ListFiles(networkPath, username, domain, password);
				}
				catch (Exception ex)
				{
					Console.WriteLine("An error occurred: " + ex.Message);
				}

				return;
			}
		}

		// 按钮点击事件：清除信息
		private void btnClearInfo_Click(object sender, RoutedEventArgs e)
		{
			m_global.m_log_presenter.Clear();
		}

		// 按钮点击事件：修改当前缺陷名称
		private void btnModifyCurrentDefectName_Click(object sender, RoutedEventArgs e)
		{
			page_HomeView.ModifyCurrentDefectName();
		}

		// 按钮点击事件：样品板卡控
		private void btnSampleProductControl_Click(object sender, RoutedEventArgs e)
		{
		}

		// 按钮点击事件：查询机台联网状态
		private void btnCheckAVIMachineConnectStatus_Click(object sender, RoutedEventArgs e)
		{
			AVIMachineConnectStatus window = new AVIMachineConnectStatus(this);

			window.Show();
		}

		// 线程：早上8点和晚上8点换班，切换班别
		public void thread_monitor_shift_change()
		{
			int nPreviousShift = 1;         // 1:白班，2:晚班

			int nDayShiftStartHour = 8;
			int nNightShiftStartHour = 20;

			if (m_global.m_strSiteCity == "苏州")
			{
				nDayShiftStartHour = 7;
				nNightShiftStartHour = 19;
			}

			if (DateTime.Now.Hour >= nDayShiftStartHour && DateTime.Now.Hour < nNightShiftStartHour)
			{
				nPreviousShift = 1;
			}
			else
			{
				nPreviousShift = 2;
			}

			// 更新界面
			this.Dispatcher.BeginInvoke(new Action(() => {
				page_HomeView.combo_WorkShift.SelectedIndex = nPreviousShift - 1;
			}));

			while (true)
			{
				// 获取当前时间
				DateTime currentTime = DateTime.Now;

				// 如果当前时间是8点，切换班别
				if (currentTime.Hour == nDayShiftStartHour || currentTime.Hour == nNightShiftStartHour)
				{
					// 切换班别
					if (currentTime.Hour == nDayShiftStartHour && nPreviousShift == 2)
					{
						nPreviousShift = 1;

						// 更新界面
						this.Dispatcher.BeginInvoke(new Action(() => {
							page_HomeView.combo_WorkShift.SelectedIndex = nPreviousShift - 1;
						}));
					}
					else if (currentTime.Hour == nNightShiftStartHour && nPreviousShift == 1)
					{
						nPreviousShift = 2;

						// 更新界面
						this.Dispatcher.BeginInvoke(new Action(() => {
							page_HomeView.combo_WorkShift.SelectedIndex = nPreviousShift - 1;
						}));
					}
				}

				Thread.Sleep(1000);
			}
		}

		// 线程：无数据重发监控线程
		private void thread_monitor_recheck_data_sending_status()
		{
			m_nDataSendingStatusFlag = 0;

			while (true)
			{
				// 0代表发送线程未启动或者发送线程已经结束
				if (0 == m_nDataSendingStatusFlag)
				{
					// 启动无数据重发线程
					(new Thread(thread_send_data_to_recheck_station)).Start();

					Thread.Sleep(3000);

					// 等待m_nDataSendingStatusFlag变为1
					while (true)
					{
						if (1 == m_nDataSendingStatusFlag)
							break;

						if (m_global.m_bExitProgram == true)
							return;
						Thread.Sleep(100);
					}
				}

				if (m_global.m_bExitProgram == true)
					return;

				Thread.Sleep(500);
			}
		}

		// 线程：无数据重发线程
		private void thread_send_data_to_recheck_station()
		{
			Thread.Sleep(3000);

			m_nDataSendingStatusFlag = 1;

			for (int n = 0; n < 50; n++)
			{
				Thread.Sleep(100);

				if (m_global.m_bExitProgram == true)
					return;
			}

			//while (true)
			{
				m_global.m_log_presenter.Log("触发一次 数据重发线程");

				// 遍历m_global.m_dict_recheck_station_names_and_IPs
				for (int i = 0; i < m_global.m_dict_recheck_station_names_and_IPs.Count; i++)
				{
					string strRecheckStationName = m_global.m_dict_recheck_station_names_and_IPs.Keys.ElementAt(i);
					string strRecheckStationIP = m_global.m_dict_recheck_station_names_and_IPs.Values.ElementAt(i);

					// 如果strRecheckStationName不为空，发送数据
					if (false == string.IsNullOrEmpty(strRecheckStationName))
					{
						try
						{
							// 判断IP是否合法
							if (true == AVICommunication.ValidateIPAddress(strRecheckStationIP))
							{
								// 验证IP地址是否能ping通
								if (true == AVICommunication.PingAddress(strRecheckStationIP))
								{
									// 构造webapi地址
									string strWebAPIAddress = "http://" + strRecheckStationIP + ":8080/vml/dcs/report_data/";

									// 生成日志目录
									string logpath = @"D:\复判数据重发日志\" + DateTime.Now.ToString("yyyyMMdd") + "\\fupandata" + (i + 1).ToString();
									if (false == Directory.Exists(logpath))
										Directory.CreateDirectory(logpath);

									string[] files = Directory.GetFiles(@"D:\fupandata" + (i + 1).ToString(), "*.txt");

									for (int n = 0; n < files.Length; n++)
									{
										if (n > 10)
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
												//File.WriteAllText(strLogName, info1);
												m_global.m_log_presenter.Log(info1);

												using (StreamWriter sw = new StreamWriter(strLogName, true))
												{
													// 写入当前时间，精确到毫秒
													sw.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + ": " + info1);
													sw.Close();
												}

												// 发送content给复判软件
												string strMesServerResponse = "";
												bool bRet = m_global.m_MES_service.SendMESForDataResend(strWebAPIAddress, content, ref strMesServerResponse, 100, 1);
												string info2 = "";

												if (true == bRet)
												{
													ResponseFromRecheck response = JsonConvert.DeserializeObject<ResponseFromRecheck>(strMesServerResponse);

													Debugger.Log(0, null, string.Format("666666 n {0}: 复判软件1接收数据返回 response.msg = {1}", n, response.msg));

													if (response.error_code == 0 && response.msg == "OK")
													{
														// 如果发送成功，删除文件
														{
															File.Delete(file);
														}

														info2 = info1 + "-------发送成功-----------";
														//File.WriteAllText(strLogName, info2);
														m_global.m_log_presenter.Log(info2);

														using (StreamWriter sw = new StreamWriter(strLogName, true))
														{
															// 写入当前时间，精确到毫秒
															sw.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + ": " + info2);
															sw.Close();
														}
													}
													else
													{
														info2 = info1 + "-------发送异常-------";
														//File.WriteAllText(strLogName, info2);
														m_global.m_log_presenter.Log(info2);

														using (StreamWriter sw = new StreamWriter(strLogName, true))
														{
															// 写入当前时间，精确到毫秒
															sw.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + ": " + info2);
															sw.Close();
														}
													}
												}
												else
												{
													info2 = info1 + "-------连接异常-------";
													//File.WriteAllText(strLogName, info2);
													m_global.m_log_presenter.Log(info2);

													using (StreamWriter sw = new StreamWriter(strLogName, true))
													{
														// 写入当前时间，精确到毫秒
														sw.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + ": " + info2);
														sw.Close();
													}
												}

												Debugger.Log(0, null, string.Format("重发机制n {0}: 文件 {1} 的内容 发送给复判软件 结果 bRet_1 = {2}", n, file, bRet));
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
						}
						catch (Exception ex)
						{
							m_global.m_log_presenter.Log("数据发送失败，异常描述：" + ex.Message);
						}
					}
				}

				if (m_global.m_bExitProgram == true)
					return;

				//Thread.Sleep(10000);
			}

			for (int n = 0; n < 50; n++)
			{
				Thread.Sleep(100);

				if (m_global.m_bExitProgram == true)
					return;
			}

			m_nDataSendingStatusFlag = 0;
		}

		// 枚举窗口
		private bool EnumTheWindows(IntPtr hWnd, IntPtr lParam)
		{
			uint processId;

			GetWindowThreadProcessId(hWnd, out processId);

			if (processId == _currentProcessId) // 确保窗口属于当前进程
			{
				StringBuilder title = new StringBuilder(MaxTitleLength);

				GetWindowText(hWnd, title, title.Capacity + 1);

				string windowTitle = title.ToString();

				// 根据需要处理所有消息框或特定标题的消息框
				//if (!string.IsNullOrEmpty(windowTitle))
				{
					// 获取类名
					StringBuilder className = new StringBuilder(MaxTitleLength);

					GetClassName(hWnd, className, className.Capacity);

					string windowClass = className.ToString();

					// 如果是MessageBox
					if (windowClass.Contains("#32770"))
					{
						Debugger.Log(0, null, string.Format("222222 检测到MessageBox: 标题='{0}', 类名='{1}'", windowTitle, windowClass));

						// 尝试在消息框中找到消息文本
						IntPtr hText = FindWindowEx(hWnd, IntPtr.Zero, "Static", null);

						if (hText != IntPtr.Zero)
						{
							StringBuilder message = new StringBuilder(MaxTitleLength);

							GetWindowText(hText, message, message.Capacity + 1);

							string messageText = message.ToString();

							Debugger.Log(0, null, string.Format("222222 检测到消息框: 标题='{0}', 消息='{1}'", windowTitle, messageText));

							// 如果消息文本包含"索引"、"index"、"Index"、"非法"、"illegal"、"Illegal"，则关闭消息框
							//if (messageText.Contains("索引") || messageText.Contains("index") || messageText.Contains("Index")
							//    || messageText.Contains("非法") || messageText.Contains("illegal") || messageText.Contains("关闭"))
							{
								Debugger.Log(0, null, string.Format("222222 关闭消息框 111: 标题='{0}', 消息='{1}'", windowTitle, messageText));

								for (int i = 0; i < 50; i++)
								{
									Thread.Sleep(100);

									if (m_global.m_bExitProgram == true)
										return false;
								}

								// 此处你可以根据内容决定是否关闭消息框，用postmessage
								PostMessage(hWnd, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);

								Thread.Sleep(1000);

								m_global.m_log_presenter.LogError(string.Format("检测到消息框: 标题='{0}', 消息='{1}', 对其进行强制关闭", windowTitle, messageText));

								Debugger.Log(0, null, string.Format("222222 关闭消息框 222: 标题='{0}', 消息='{1}'", windowTitle, messageText));
							}
						}
					}
				}
			}

			return true;             // 继续枚举
		}

		// 线程：检测消息框线程
		public void thread_detect_message_box()
		{
			Thread.Sleep(6000);

			m_global.m_log_presenter.Log("检测消息框线程已启动");

			while (true)
			{
				EnumWindows(new EnumWindowsProc(EnumTheWindows), IntPtr.Zero);

				Thread.Sleep(1000);

				if (m_global.m_bExitProgram == true)
					return;
			}
		}

		// 线程：图片目录清除线程
		public void thread_recycle_old_images()
		{
			Thread.Sleep(3000);

			while (true)
			{
				// 递归遍历D:\avi_images目录下所有子目录，删除超过2天的所有图片文件，包含jpg、jpeg、png、bmp
				// 设置根目录和文件扩展名集合
				string[] rootDirectories = new string[5];

				rootDirectories[0] = @"D:\avi_images";
				rootDirectories[1] = @"D:\avi_images" + "_forAI";
				rootDirectories[2] = @"D:\fupandata";
				rootDirectories[3] = @"D:\fupandata_temp";
				rootDirectories[4] = @"D:\aiRecheckSubmitData";

				string[] imageExtensions = { ".jpg", ".jpeg", ".png", ".bmp" };

				// 获取当前时间
				DateTime currentTime = DateTime.Now;

				// 设置文件过期时间（7天）
				TimeSpan expireTime = TimeSpan.FromDays(m_global.m_nDataStorageDuration);

				// 递归遍历目录并删除符合条件的文件
				try
				{
					for (int i = 0; i < 2; i++)
					{
						string rootDirectory = rootDirectories[i];

						// 判断目录是否存在
						if (false == Directory.Exists(rootDirectory))
							continue;

						CleanOldImages(rootDirectory, imageExtensions, currentTime, expireTime);
					}

					for (int i = 2; i < 5; i++)
					{
						string rootDirectory = rootDirectories[i];

						// 判断目录是否存在
						if (false == Directory.Exists(rootDirectory))
							continue;

						// txt文件过期时间为一个月
						TimeSpan expireTimeForTxt = TimeSpan.FromDays(30);

						// 删除txt文件
						CleanOldTXTAndJsonFiles(rootDirectory, currentTime, expireTimeForTxt);
					}

					m_global.m_log_presenter.Log("执行一次清除txt文件");
				}
				catch (Exception ex)
				{
					m_global.m_log_presenter.Log("清除图片文件失败：" + ex.Message);
				}

				// 删除数据库两天之前的数据
				if (true == m_global.m_bIsConnectedToDatabase)
				{
					// 遍历m_global.m_dict_machine_names_and_IPs的keys，创建表格
					for (int t = 0; t < m_global.m_dict_machine_names_and_IPs.Count; t++)
					{
						string strMachineName = m_global.m_dict_machine_names_and_IPs.Keys.ElementAt(t);

						// 如果strMachineName不为空，创建表格
						if (true == string.IsNullOrEmpty(strMachineName))
							continue;

						string strTrayTable = "trays_" + strMachineName;
						string strProductTable = "products_" + strMachineName;
						string strDefectTable = "details_" + strMachineName;

						string[] strTables = new string[3] { strTrayTable, strProductTable, strDefectTable };

						string[] strSqlCommands = new string[3];

						// 如果strTables文本包含"-"，替换为"_"
						for (int i = 0; i < strTables.Length; i++)
						{
							if (true == strTables[i].Contains("-"))
							{
								strTables[i] = strTables[i].Replace("-", "_");
							}
						}

						if (true)
						{
							if (!m_global.m_bSendInspectionResultToRecheckStation)
							{
								for (int n = 0; n < strTables.Length; n++)
								{
									// 先查询表格是否存在
									if (false == m_global.m_mysql_ops.IsTableExist("AutoScanFQCTest", strTables[n]))
									{
										continue;
									}

									if (true == m_global.m_mysql_ops.DeleteTableData(strTables[n], m_global.m_nDataStorageDuration))
									{
										m_global.m_log_presenter.Log("删除表格 " + strTables[n] + $" {m_global.m_nDataStorageDuration}天之前的数据成功");
									}
									else
									{
										m_global.m_log_presenter.Log("删除表格 " + strTables[n] + $" {m_global.m_nDataStorageDuration}天之前的数据失败");
									}
								}
							}
						}
					}

					if (m_global.m_bSendInspectionResultToRecheckStation)
					{
						for (int t = 0; t < m_global.m_dict_machine_names_and_IPs.Count; t++)
						{
							string strMachineName = m_global.m_dict_machine_names_and_IPs.Keys.ElementAt(t);

							// 如果strMachineName不为空，创建表格
							if (true == string.IsNullOrEmpty(strMachineName))
								continue;

							string tableName = "transfer_aiinfo_" + strMachineName;
							tableName = tableName.Replace('-', '_');

							// 先查询表格是否存在
							if (false == m_global.m_mysql_ops.IsTableExist("AutoScanFQCTest", tableName))
							{
								continue;
							}

							if (true == m_global.m_mysql_ops.DeleteTransferTableData(tableName, 2))
							{
								m_global.m_log_presenter.Log("删除表格 " + tableName + $" 2天之前的数据成功");
							}
							else
							{
								m_global.m_log_presenter.Log("删除表格 " + tableName + $" 2天之前的数据失败");
							}
						}
					}
				}

				for (int n = 0; n < 1000; n++)
				{
					Thread.Sleep(5000);

					if (m_global.m_bExitProgram == true)
						return;
				}
			}
		}

		// 线程：检测未复判料盘
		public void thread_detect_unchecked_trays()
		{
			if (false == m_global.m_mysql_ops.IsMySQLServiceInstalled())
				return;

			Thread.Sleep(1000);

			m_global.m_list_list_unchecked_trays = new List<List<string>>();
			for (int n = 0; n < m_global.m_dict_machine_names_and_IPs.Count; n++)
				m_global.m_list_list_unchecked_trays.Add(new List<string>());

			List<TrayInfo>[] list_list_trays = new List<TrayInfo>[m_global.m_dict_machine_names_and_IPs.Count];
			List<TrayInfo> prev_list_trays_as_summed_up = new List<TrayInfo>();

			while (true)
			{
				for (int n = 0; n < 20; n++)
				{
					Thread.Sleep(100);

					if (true == m_global.m_bExitProgram)
						return;
				}

				// 待复判料盘汇总
				List<TrayInfo> list_trays_as_summed_up = new List<TrayInfo>();

				for (int n = 0; n < m_global.m_dict_machine_names_and_IPs.Count; n++)
				{
					// 获取机器名称
					string strMachineName = m_global.m_dict_machine_names_and_IPs.Keys.ElementAt(n);

					// 如果strMachineName为空，则跳过
					if (string.IsNullOrEmpty(strMachineName))
						continue;

					// 替换机器名称中的“-”为“_”
					strMachineName = strMachineName.Replace("-", "_");

					// 生成表名
					string strTableName = "trays_" + strMachineName;
					strTableName = strTableName.Replace("-", "_");

					// 构建查询语句
					string strQuery = $"SELECT * FROM {strTableName} WHERE r1 = 'to_be_checked';";

					// 查询耗时
					TimeSpan ts = new TimeSpan(0, 0, 0);

					// 查询数据库，获取托盘数据
					string strQueryResult = "";
					m_global.m_mysql_ops.QueryTableData(strTableName, strQuery, ref strQueryResult, ref ts);

					// 如果查询结果不为空，处理查询结果
					if (strQueryResult.Length > 0)
					{
						List<TrayInfo> list_trays = new List<TrayInfo>();
						m_global.m_database_service.ProcessTrayDataQueryResult(strQueryResult, ref list_trays, !m_global.m_bCombineTwoMachinesAsOne);

						// 如果托盘列表不为空，将其添加到结果集合中
						if (list_trays.Count > 0)
						{
							list_list_trays[n] = list_trays;

							for (int i = 0; i < list_trays.Count; i++)
							{
								list_trays_as_summed_up.Add(list_trays[i]);

								if (list_trays_as_summed_up[i].set_id.Contains('_'))
								{
									// 构建查询语句
									string strProductTableName = "products_" + strMachineName;
									strProductTableName = strProductTableName.Replace("-", "_");
									// TrayInfo中无barcode来查询（5号楼set_id为时间戳，无法查询）,通过set_id查找到所有product
									strQuery = $"SELECT * FROM {strProductTableName} WHERE set_id = '{list_trays_as_summed_up[i].set_id}';";

									strQueryResult = "";
									m_global.m_mysql_ops.QueryTableData(strTableName, strQuery, ref strQueryResult, ref ts);

									// 解析查询结果
									if (strQueryResult.Length > 0)
									{
										List<ProductInfo> list_products = new List<ProductInfo>();
										m_global.m_database_service.ProcessProductDataQueryResult(strQueryResult, ref list_products);

										for (int j = 0; j < list_products.Count; j++)
										{
											// 找到barcode不为NoCode的任意product，赋值给tray的r8（未使用的备用字段
											if (!list_products[j].barcode.Contains("NoCode"))
											{
												list_trays_as_summed_up[i].r8 = list_products[j].barcode;
												break;
											}
											else
												continue;
										}
									}
								}
							}
						}
					}
				}

				bool bSame = true;

				// 检查新的托盘列表与之前的是否相同
				if (list_trays_as_summed_up.Count != prev_list_trays_as_summed_up.Count)
				{
					bSame = false;
				}
				else
				{
					for (int i = 0; i < list_trays_as_summed_up.Count; i++)
					{
						if (list_trays_as_summed_up[i].set_id != prev_list_trays_as_summed_up[i].set_id)
						{
							bSame = false;
							break;
						}
					}
				}

				// 如果托盘列表不同，更新界面
				if (!bSame)
				{
					this.Dispatcher.BeginInvoke(new Action(() => {
						// 清空下拉框
						page_HomeView.combo_SelectUncheckedTrays.Items.Clear();

						// 添加新的托盘ID到下拉框
						for (int i = 0; i < list_trays_as_summed_up.Count; i++)
						{
							// 如果是5号楼料盘，则set_id为时间戳，含有"_"
							if (list_trays_as_summed_up[i].set_id.Contains('_'))
							{
								page_HomeView.combo_SelectUncheckedTrays.Items.Add(list_trays_as_summed_up[i].r8);
							}
							else
							{
								// 检胶
								if (!page_HomeView.combo_SelectUncheckedTrays.Items.Contains(list_trays_as_summed_up[i].set_id))
								{
									page_HomeView.combo_SelectUncheckedTrays.Items.Add(list_trays_as_summed_up[i].set_id);
								}
							}
						}

						// 更新待复判料盘的数量
						page_HomeView.textblock_UncheckedTrays.Text = string.Format("待复判料盘 {0}个：", page_HomeView.combo_SelectUncheckedTrays.Items.Count);

						if (page_HomeView.combo_SelectUncheckedTrays.Items.Count > 0)
						{
							// 设置默认选中项
							page_HomeView.combo_SelectUncheckedTrays.SelectedIndex = 0;
						}

						// 如果有托盘，更新扫描条码的文本框
						if (list_trays_as_summed_up.Count > 0 && m_global.m_recheck_data_query_mode == RecheckDataQueryMode.ByBarcode)
						{
							page_HomeView.textbox_ScannedBarcode.Text = list_trays_as_summed_up[0].set_id;
						}
					}));

					// 更新之前的托盘列表
					prev_list_trays_as_summed_up.Clear();
					for (int i = 0; i < list_trays_as_summed_up.Count; i++)
					{
						prev_list_trays_as_summed_up.Add(list_trays_as_summed_up[i]);
					}
				}
			}
		}

		// 线程：检测AVI机台联网状态
		public void thread_detect_AVIMachineConnectStatus()
		{
			bool bFirst = true;

			while (true)
			{
				if (true == bFirst)
				{
					bFirst = false;
				}
				else
				{
					for (int n = 0; n < 80; n++)
					{
						Thread.Sleep(1000);

						if (true == m_global.m_bExitProgram)
							return;
					}
				}

				for (int n = 0; n < m_global.m_dict_machine_names_and_IPs.Count; n++)
				{
					string strMachineName = m_global.m_dict_machine_names_and_IPs.Keys.ElementAt(n);

					// 如果strMachineName为空，则跳过
					if (true == string.IsNullOrEmpty(strMachineName))
					{
						Debugger.Log(0, null, string.Format("222222 机台 {0} 名称为空", n + 1));

						continue;
					}

					string strMachineIP = m_global.m_dict_machine_names_and_IPs.Values.ElementAt(n);

					// 如果strMachineIP为空，则跳过
					if (true == string.IsNullOrEmpty(strMachineIP))
					{
						m_global.m_dict_machine_names_and_connect_status[strMachineName] = 3;

						Debugger.Log(0, null, string.Format("222222 机台 {0} IP地址为空", strMachineName));

						m_global.m_log_presenter.Log(string.Format("机台 {0} IP地址为空", strMachineName));

						continue;
					}

					// 判断strIP是否是有效IP地址
					if (false == AVICommunication.ValidateIPAddress(strMachineIP))
					{
						m_global.m_dict_machine_names_and_connect_status[strMachineName] = 3;

						Debugger.Log(0, null, string.Format("222222 机台 {0} IP地址 {1} 无效", strMachineName, strMachineIP));

						m_global.m_log_presenter.Log(string.Format("机台 {0} IP地址 {1} 无效", strMachineName, strMachineIP));

						continue;
					}

					// 判断能否ping通strMachineIP
					if (false == AVICommunication.PingAddress(strMachineIP) && !m_global.m_bSendInspectionResultToRecheckStation)
					{
						m_global.m_dict_machine_names_and_connect_status[strMachineName] = 2;

						Debugger.Log(0, null, string.Format("222222 机台 {0} IP地址 {1} 无法ping通", strMachineName, strMachineIP));

						m_global.m_log_presenter.Log(string.Format("机台 {0} IP地址 {1} 无法ping通", strMachineName, strMachineIP));

						continue;
					}

					string[] strSharedDirs = { "AVIData", "ResultImage", "D\\AVIData", "E\\AVIData", "D\\outdatas", "E\\outdatas", "outdatas" };
					string[] strSharedDirsWithIP = new string[strSharedDirs.Length];

					// 定义支持的图片格式
					string[] extensions = new string[] { ".jpg", ".jpeg" };

					int timeout = 200; // 超时时间（毫秒）

					for (int i = 0; i < strSharedDirs.Length; i++)
					{
						strSharedDirsWithIP[i] = "\\\\" + strMachineIP + "\\" + strSharedDirs[i];

						// 判断能否访问strSharedDirsWithIP[i]
						try
						{
							foreach (string extension in extensions)
							{
								try
								{
									Directory.GetFiles(strSharedDirsWithIP[i], $"*{extension}", SearchOption.TopDirectoryOnly);

									m_global.m_dict_machine_names_and_connect_status[strMachineName] = 0;

									break;
								}
								catch (Exception ex)
								{
									m_global.m_dict_machine_names_and_connect_status[strMachineName] = 1;

									//string strError = string.Format("机台 {0} IP地址 {1} 无法访问共享目录 {2}，错误信息：{3}", strMachineName, strMachineIP, strSharedDirsWithIP[i], ex.Message);

									//m_global.m_log_presenter.Log(strError);

									//Debugger.Log(0, null, string.Format("222222 {0}", strError));
								}
							}

							if (m_global.m_dict_machine_names_and_connect_status[strMachineName] == 0)
								break;
						}
						catch (Exception ex)
						{
							m_global.m_dict_machine_names_and_connect_status[strMachineName] = 1;

							//string strError = string.Format("机台 {0} IP地址 {1} 无法访问共享目录 {2}，错误信息：{3}", strMachineName, strMachineIP, strSharedDirsWithIP[i], ex.Message);

							//m_global.m_log_presenter.Log(strError);

							//Debugger.Log(0, null, string.Format("222222 {0}", strError));
						}
					}

					if (1 == m_global.m_dict_machine_names_and_connect_status[strMachineName] && !m_global.m_bSendInspectionResultToRecheckStation)
					{
						m_global.m_log_presenter.Log(string.Format("机台 {0} IP地址 {1} 无法访问共享目录", strMachineName, strMachineIP));

						Debugger.Log(0, null, string.Format("222222 机台 {0} IP地址 {1} 无法访问共享目录", strMachineName, strMachineIP));
					}
				}
			}
		}

		// 线程：检测win10的系统防火墙是否处于开启或者关闭状态
		public void thread_detect_windows_firewall_status()
		{
			int counter = 0;

			Thread.Sleep(5000);

			while (true)
			{
				if (true == m_global.m_bExitProgram)
					return;

				counter++;

				try
				{
					// 获取防火墙管理实例
					bool isFirewallOn = false;

					for (int i = 0; i < FirewallManager.Instance.Profiles.Count; i++)
					{
						var profile = FirewallManager.Instance.Profiles[i];

						// 检查防火墙是否启用
						if (profile.Enable)
						{
							isFirewallOn = true;
						}
					}

					if (isFirewallOn)
					{
						// 防火墙已开启
						if (counter % 10 == 0)
						{
							this.Dispatcher.BeginInvoke(new Action(() => {
								m_global.m_log_presenter.Log("Windows 防火墙已开启，复判软件无法正常接收数据，请关闭防火墙！");

								// 作为中转向复判站发送数据时不弹窗
								if (!m_global.m_bSendInspectionResultToRecheckStation)
									MessageBox.Show("Windows 防火墙已开启，复判软件无法正常接收数据，请关闭防火墙！");
							}));
						}
					}
					else
					{
						// 防火墙已关闭
						if (counter % 10 == 0)
							m_global.m_log_presenter.Log("Windows 防火墙已关闭");
					}
				}
				catch (Exception ex)
				{
					m_global.m_log_presenter.Log("检查防火墙状态时出错：" + ex.Message);
				}

				Thread.Sleep(5000);
			}
		}

		// 功能：清除过期的图片文件
		private void CleanOldImages(string directory, string[] imageExtensions, DateTime currentTime, TimeSpan expireTime)
		{
			try
			{
				// 处理当前目录下的文件
				foreach (var file in Directory.GetFiles(directory))
				{
					// 获取文件的扩展名并转换为小写
					string fileExtension = System.IO.Path.GetExtension(file).ToLower();

					// 检查文件是否为图片文件
					if (imageExtensions.Contains(fileExtension))
					{
						// 获取文件的创建时间
						DateTime creationTime = File.GetCreationTime(file);

						// 判断文件是否超过设定的过期时间
						if ((currentTime - creationTime) > expireTime)
						{
							// 删除文件
							try
							{
								File.Delete(file);

								//m_global.m_log_presenter.Log("删除文件：" + file);
							}
							catch (Exception ex)
							{
								//m_global.m_log_presenter.Log("删除文件失败：" + file + "，错误信息：" + ex.Message);
							}
						}
					}
				}

				// 递归遍历子目录
				foreach (var subDirectory in Directory.GetDirectories(directory))
				{
					CleanOldImages(subDirectory, imageExtensions, currentTime, expireTime);

					// 删除空目录
					if (Directory.GetFiles(subDirectory).Length == 0 && Directory.GetDirectories(subDirectory).Length == 0)
					{
						try
						{
							Directory.Delete(subDirectory);
							m_global.m_log_presenter.Log("删除空目录：" + subDirectory);
						}
						catch (Exception ex)
						{
							m_global.m_log_presenter.Log("删除空目录失败：" + subDirectory + "，错误信息：" + ex.Message);
						}
					}
				}
			}
			catch (Exception ex)
			{
				m_global.m_log_presenter.LogError("清除图片文件失败：" + ex.Message);
			}
		}

		// 功能：清除过期的TXT文件
		private void CleanOldTXTAndJsonFiles(string directory, DateTime currentTime, TimeSpan expireTime)
		{
			try
			{
				// 处理当前目录下的文件
				foreach (var file in Directory.GetFiles(directory))
				{
					// 获取文件的扩展名并转换为小写
					string fileExtension = System.IO.Path.GetExtension(file).ToLower();

					// 检查文件是否为TXT文件
					if (fileExtension == ".txt" || fileExtension == ".json")
					{
						// 获取文件的创建时间
						DateTime creationTime = File.GetCreationTime(file);

						// 判断文件是否超过设定的过期时间
						if ((currentTime - creationTime) > expireTime)
						{
							// 删除文件
							try
							{
								File.Delete(file);
								//m_global.m_log_presenter.Log("删除文件：" + file);
							}
							catch (Exception ex)
							{
								//m_global.m_log_presenter.Log("删除文件失败：" + file + "，错误信息：" + ex.Message);
							}
						}
					}
				}

				// 递归遍历子目录
				foreach (var subDirectory in Directory.GetDirectories(directory))
				{
					CleanOldTXTAndJsonFiles(subDirectory, currentTime, expireTime);

					// 删除空目录
					if (Directory.GetFiles(subDirectory).Length == 0 && Directory.GetDirectories(subDirectory).Length == 0)
					{
						try
						{
							Directory.Delete(subDirectory);
							m_global.m_log_presenter.Log("删除空目录：" + subDirectory);
						}
						catch (Exception ex)
						{
							m_global.m_log_presenter.Log("删除空目录失败：" + subDirectory + "，错误信息：" + ex.Message);
						}
					}
				}
			}
			catch (Exception ex)
			{
				m_global.m_log_presenter.LogError("清除TXT文件失败：" + ex.Message);
			}
		}

		private void btnQueryDB_Click(object sender, RoutedEventArgs e)
		{
			QueryDatabase qd = new QueryDatabase(this);
			qd.Show();
		}

		private void btnDefects_Click(object sender, RoutedEventArgs e)
		{
			//DialogWindows.DefectCount dc = new DialogWindows.DefectCount(this);
			if (m_global.m_defectStatisticsQuery == null)
			{
				m_global.m_defectStatisticsQuery = new DefectStatisticsQueryWindow(this);
			}

			m_global.m_defectStatisticsQuery.Show();
		}

		private void BtnExportDefectCorrectionData_Click(object sender, RoutedEventArgs e)
		{
			var exportPath = @"D:\fupanExportData";
			CSVHelper.ExportDefectCorrectionRecord(exportPath, m_global.m_list_DefectCorrectionRecord);
		}

		private void LoginBtn_Click(object sender,RoutedEventArgs e)
		{
			LoginWindow login = new LoginWindow(this);
			login.ShowDialog();
		}

		private void LogoutBtn_Click(object sender, RoutedEventArgs eventArgs)
        {
			m_global.CurrentLoginType = UserType.操作员;
			page_HomeView.TextBlock_UserType_Name.Text = "";

			m_global.m_strCurrentOperatorID = "";

			page_HomeView. textblock_OperatorID.Text = m_global.m_strCurrentOperatorID;

			page_HomeView.Visibility = Visibility.Hidden;

			statusbar_task_info.Text = "操作员信息: 未登录";

			m_global.m_log_presenter.Log("操作员 " + m_global.m_strCurrentOperatorID + " 于 " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " 退出登录");
		}
	}
}