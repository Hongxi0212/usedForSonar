 using AutoScanFQCTest.DataModels;
using LiveCharts.Wpf;
using LiveCharts;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.SqlServer.Server;
using AutoScanFQCTest.Utilities;
using System.Threading;
using System.ComponentModel;
using MySql.Data.MySqlClient;
using System.Data.Common;

namespace AutoScanFQCTest.DialogWindows
{
	/// <summary>
	/// DefectStatisticsQueryWindow.xaml 的交互逻辑
	/// </summary>
	public partial class DefectStatisticsQueryWindow : Window
	{
		MainWindow m_parent;

		private int totalDefectCount = 0;
		private int totalTrayCount = 0;
		private int totalProductCount = 0;
		private int primaryOKCount = 0;
		private int primaryNGCount = 0;
		private double primaryYield = 0;
		private int recheckOKCount = 0;
		private int recheckNGCount = 0;
		private double recheckYield = 0;

		private string timeDateCondition_Start = "";
		private string timeDateCondition_End = "";
		private string timeHourCondition_Start = "";
		private string timeHourCondition_End = "";
		private string timeMinCondition_Start = "";
		private string timeMinCondition_End = "";
		private string machineIdCondition = "";
		private string productIdCondition = "";
		private string chartCondition = "";

		private List<DefectStatistics> currentDefectRecords = new List<DefectStatistics>();
		// 根据缺陷记录处理得出的产品记录, 字典查询效率高？
		//private Dictionary<string, ProductRecord> handledProductRecord = new Dictionary<string, ProductRecord>();
		//private Dictionary<string, TrayRecord> handledTrayRecord = new Dictionary<string, TrayRecord>();
		private Dictionary<string, List<int>> defectTypeAndCount = new Dictionary<string, List<int>>();
		// bool值为1则该productOK，0则NG
		private Dictionary<string, Dictionary<string, bool>> trayAndProductWithRecheckResult = new Dictionary<string, Dictionary<string, bool>>();

		private Thread synchronizeUIThread;
		private CancellationTokenSource synchronizeUIThreadCTS;
		//private TaskCompletionSource<bool> taskCompletionSource;
		private AutoResetEvent synchronizeUITrigger = new AutoResetEvent(false);

		public DefectStatisticsQueryWindow(MainWindow parent)
		{
			InitializeComponent();
			m_parent = parent;

			initialize();

			this.Closing += CloseThis;

			if (m_parent.m_global.m_strProductType == "dock" && m_parent.m_global.m_strProductSubType != "glue_check")
			{
				synchronizeUIThreadCTS = new CancellationTokenSource();
				synchronizeUIThread = new Thread(() => synchronizeUI(synchronizeUIThreadCTS.Token));
				synchronizeUIThread.Start();
			}
		}

		~DefectStatisticsQueryWindow()
		{
			if (m_parent.m_global.m_strProductType != "nova" && m_parent.m_global.m_strProductSubType != "glue_check")
			{
				// synchronizeUIThreadCTS.Cancel();
				// 唤醒，确保不会阻塞在WaitOne
				synchronizeUITrigger.Set();
				synchronizeUIThread.Join();
			}
		}

		private void CloseThis(object sender, CancelEventArgs eventArgs)
		{
			eventArgs.Cancel = true;

			this.Hide();
		}

		/// <summary>
		/// 初始化，包含初始化复选框和Top10缺陷Grid
		/// </summary>
		private void initialize()
		{
			if (m_parent.m_global.m_strProductType == "nova")
			{
				TextBlockPrimaryNGCount.Text = "当前产品不适用";
				TextBlockPrimaryOKCount.Text = "当前产品不适用";
				TextBlockPrimaryYield.Text = "当前产品不适用";
			}

			initializeComboBoxs();
			initializeTopDefectDataGridView();
		}

		/// <summary>
		/// 图表显示项目复选框切换的响应函数
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public void ComboBoxChartCondition_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			chartCondition = ComboBoxChartCondition.SelectedItem.ToString();
			switch (chartCondition)
			{
				case "全部缺陷":
					generateChartForDefectCount(defectTypeAndCount.Count);
					break;
				case "TOP10缺陷":
					generateChartForDefectCount(10);
					break;
				case "TOP5缺陷":
					generateChartForDefectCount(5);
					break;
			}
		}

		/// <summary>
		/// 导出excel报表按钮的响应函数
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public void BtnExport_Click(object sender, EventArgs e)
		{
			var exportPath = @"D:\fupanExportData";
			CSVHelper.ExportDefectStatisticsRecord(exportPath, currentDefectRecords);
		}

		/// <summary>
		/// 查询按钮的响应函数
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public void BtnQuery_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				timeDateCondition_Start = DatePickerStart.SelectedDate.ToString();
				timeDateCondition_End = DatePickerEnd.SelectedDate.ToString();
				timeHourCondition_Start = ComboBoxHourPickerStart.SelectedItem == null ? "00" : ComboBoxHourPickerStart.SelectedItem.ToString();
				timeHourCondition_End = ComboBoxHourPickerEnd.SelectedItem == null ? "00" : ComboBoxHourPickerEnd.SelectedItem.ToString();
				timeMinCondition_Start = ComboBoxMinPickerStart.SelectedItem == null ? "00" : ComboBoxMinPickerStart.SelectedItem.ToString();
				timeMinCondition_End = ComboBoxMinPickerEnd.SelectedItem == null ? "00" : ComboBoxMinPickerEnd.SelectedItem.ToString();
				machineIdCondition = ComboBoxMachineId.SelectedItem?.ToString();
				productIdCondition = TextBoxProductId.Text;

				clearStatistics();
				clearUI();

				if (machineIdCondition == null && timeDateCondition_Start == "" && timeDateCondition_End == "")
				{
					MessageBox.Show("请选择查询条件！");
					return;
				}

				List<string> trayTableNames = new List<string>();
				if (machineIdCondition != "" && machineIdCondition != null && machineIdCondition != "全部机台")
				{
					trayTableNames.Add(machineIdCondition);
				}
				else
				{
					foreach (var item in ComboBoxMachineId.Items)
					{
						
						
						
						if (item.ToString() != "" && item.ToString() != "全部机台")
						{
							trayTableNames.Add(item.ToString());
						}
					}
				}

				List<string> sqlQuerys = combineQuerySQL(ref trayTableNames);
				TimeSpan ts = new TimeSpan(0, 0, 0, 0);

				foreach (var sql in sqlQuerys)
				{
					string queryResult = "";
					// 表名不影响查询
					m_parent.m_global.m_mysql_ops.QueryTableData(trayTableNames[0], sql, ref queryResult, ref ts);

                    List<DefectStatistics> temp = new List<DefectStatistics>();
                    m_parent.m_global.m_database_service.ProcessDefectStatisticsQueryResult(queryResult, ref temp);

					currentDefectRecords.AddRange(temp);
				}

				handleDefectRecord();
				updateStatisticsOnUI();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}

		/// <summary>
		/// 将查询到的缺陷记录计数并处理为以产品、料盘为单位的数据
		/// </summary>
		private void handleDefectRecord(List<DefectStatistics> defectStatistics = null)
		{
			if (defectStatistics != null && currentDefectRecords.Count == 0)
			{
				currentDefectRecords = defectStatistics;
			}

			foreach (var d in currentDefectRecords)
			{
				if (defectTypeAndCount.ContainsKey(d.DefectType))
				{
					// defectTypeCount键值对的值为List<int>，List<int>的第三位表示该缺陷总数
					defectTypeAndCount[d.DefectType][2] += d.RecheckOKCount + d.RecheckNGCount;
				}
				else
				{
					defectTypeAndCount.Add(d.DefectType, new List<int> { 0, 0, d.RecheckNGCount + d.RecheckOKCount });
				}
				// 第二位表示二次OK缺陷数
				defectTypeAndCount[d.DefectType][1] += d.RecheckOKCount;
				// 第一位表示二次NG缺陷数
				defectTypeAndCount[d.DefectType][0] += d.RecheckNGCount;


				if (d.Barcode != "OKProduct")
				{
					if (trayAndProductWithRecheckResult.ContainsKey(d.TrayId))
					{
						if (trayAndProductWithRecheckResult[d.TrayId].ContainsKey(d.Barcode))
						{
							if (d.RecheckNGCount > 0)
							{
								trayAndProductWithRecheckResult[d.TrayId][d.Barcode] = false;
							}
							else
							{
								if (false != trayAndProductWithRecheckResult[d.TrayId][d.Barcode])
								{
									trayAndProductWithRecheckResult[d.TrayId][d.Barcode] = true;
								}
							}
						}
						else
						{
							trayAndProductWithRecheckResult[d.TrayId].Add(d.Barcode, d.RecheckNGCount > 0 ? false : true);

						}
					}
					else
					{
						trayAndProductWithRecheckResult.Add(d.TrayId, new Dictionary<string, bool>());
						trayAndProductWithRecheckResult[d.TrayId].Add(d.Barcode, d.RecheckNGCount > 0 ? false : true);
					}
				}
				else
				{
					if (!trayAndProductWithRecheckResult.ContainsKey(d.TrayId))
					{
						trayAndProductWithRecheckResult.Add(d.TrayId, new Dictionary<string, bool>());
					}
					//trayAndProductWithRecheckResult[d.TrayId].Add(d.Barcode, true); 
				}
			}

			foreach (var d in defectTypeAndCount)
			{
				if (d.Key != "OKProductHasNoDefects")
				{
					totalDefectCount += d.Value[2];
				}
			}

			if (defectTypeAndCount.Count() > 0)
			{
				var productCount = 0;
				foreach (var p in trayAndProductWithRecheckResult.Values)
				{
					productCount += p.Values.Count;
				}
				primaryNGCount = productCount;

				if (m_parent.m_global.m_strProductType == "dock")
				{
					if (defectTypeAndCount.Keys.Contains("OKProductHasNoDefects"))
					{
						primaryOKCount = defectTypeAndCount["OKProductHasNoDefects"][2];
					}
				}

				var count = 0;
				foreach (var t in trayAndProductWithRecheckResult)
				{
					count += t.Value.Values.Where(p => p == false).Count();
				}
				recheckNGCount = count;

				defectTypeAndCount = defectTypeAndCount.OrderByDescending(d => d.Value[2]).ToDictionary(d => d.Key, d => d.Value);

				totalProductCount = primaryNGCount + primaryOKCount;
				totalTrayCount = trayAndProductWithRecheckResult.Count();
			}

			if (m_parent.m_global.m_strProductSubType == "glue_check")
			{
				// 总盘数，OK数，总数
				var infos = new List<int>() { 0, 0, 0 };
				if (ComboBoxMachineId.SelectedItem == null)
				{
					return;

				}
				else
				{
					var machineName = ComboBoxMachineId.SelectedItem.ToString();
					var machineIP = "";
					m_parent.m_global.m_dict_machine_names_and_IPs.TryGetValue(machineName, out machineIP);
					CSVHelper.ReadGlueCheckResultTxtFile($@"\\{machineIP}\d\ResultImage\Result\Count\fupan.txt", ref infos);
				}
				updateGlueCheckData(infos);
			}

			recheckOKCount = primaryOKCount + primaryNGCount - recheckNGCount;
			primaryYield = 1.0 * primaryOKCount / (primaryOKCount + primaryNGCount);
			recheckYield = 1.0 * recheckOKCount / (primaryOKCount + primaryNGCount);
		}

		/// <summary>
		/// 根据限定的前n项数量生成缺陷报表
		/// </summary>
		/// <param name="topCount">需要生成图标的前topCount项</param>
		private void generateChartForDefectCount(int topCount)
		{
			MyChart.LegendLocation = LegendLocation.Top;

			MyChart.AxisX[0].Labels = new List<string>();
			MyChart.AxisX[0].FontSize = 14;
			MyChart.AxisX[0].Separator = new LiveCharts.Wpf.Separator() { Step = 1 };
			MyChart.AxisX[0].MinValue = 0;
			if (m_parent.m_global.m_strProductSubType == "glue_check")
			{
				MyChart.AxisX[0].LabelsRotation = 0;
			}
			else
			{
				MyChart.AxisX[0].LabelsRotation = 80;
			}

			if (MyChart.AxisY.Count < 2)
			{
				var percentageAxis = new Axis
				{
					Title = "误报率 (%)",
					Position = AxisPosition.RightTop,
					MinValue = 0,
					MaxValue = 100,
					LabelFormatter = value => value.ToString("N0") + "%"
				};

				MyChart.AxisY.Add(percentageAxis);
			}

			if (topCount > defectTypeAndCount.Count)
			{
				topCount = defectTypeAndCount.Count;
			}

			var primaryNGSeries = new ColumnSeries { Title = "一次NG", Values = new ChartValues<int>(), DataLabels = true, LabelPoint = p => p.Y.ToString() };
			var recheckNGSeries = new ColumnSeries { Title = "二次NG", Values = new ChartValues<int>(), DataLabels = true, LabelPoint = p => p.Y.ToString() };
			var recheckOKSeries = new ColumnSeries { Title = "二次OK", Values = new ChartValues<int>(), DataLabels = true, LabelPoint = p => p.Y.ToString() };
			var falseAlarmRateSeries = new LineSeries
			{
				Title = "误报率",
				Values = new ChartValues<double>(),
				DataLabels = true,
				LabelPoint = point => point.Y.ToString() + "%",
				LineSmoothness = 0,
				PointGeometry = DefaultGeometries.Circle,
				PointGeometrySize = 10,
				PointForeground = System.Windows.Media.Brushes.Brown,
				StrokeThickness = 0,
				Fill = System.Windows.Media.Brushes.Transparent,
				ScalesYAt = 1  // 绑定到第二个Y轴
			};

			for (int i = 0; i < topCount; i++)
			{
				if (defectTypeAndCount.Count <= i)
				{
					break;
				}

				if (defectTypeAndCount.ElementAt(i).Key == "OKProductHasNoDefects")
				{
					// 当选择top5或top10时，剔除OKProductHasNoDefects需要对展示数量加一
					// 当选择全部缺陷时，topCount远大于11，且如果仍然加一会导致越界
					if (topCount < 11)
					{
						topCount++;
					}
					continue;
				}

				MyChart.AxisX[0].Labels.Add(defectTypeAndCount.ElementAt(i).Key);

				var defectValues = defectTypeAndCount.ElementAt(i).Value;
				var falseAlarmRate = 1.0 * defectValues[1] / defectValues[2];

				// 向每个系列中添加对应的值
				primaryNGSeries.Values.Add(defectValues[2]);
				recheckNGSeries.Values.Add(defectValues[0]);
				recheckOKSeries.Values.Add(defectValues[1]);
				falseAlarmRateSeries.Values.Add(Math.Round(falseAlarmRate, 4) * 100);
			}

			// 将所有系列添加到图表的 SeriesCollection 中
			MyChart.Series = new SeriesCollection { primaryNGSeries, recheckNGSeries, recheckOKSeries, falseAlarmRateSeries };
		}

		/// <summary>
		/// 在处理数据后调用，更新局部变量和UI
		/// </summary>
		private void updateStatisticsOnUI()
		{
			if (currentDefectRecords.Count() > 0)
			{
				TextCurrentProductType.Text = currentDefectRecords[0].ProductId;
				TextBlockTotalDefectCount.Text = totalDefectCount.ToString();
				TextBlockTotalProductCount.Text = totalProductCount.ToString();
				TextBlockTotalTrayCount.Text = totalTrayCount.ToString();

				TextBlockPrimaryNGCount.Text = primaryNGCount.ToString();
				TextBlockPrimaryOKCount.Text = primaryOKCount.ToString();
				TextBlockPrimaryYield.Text = (primaryYield * 100).ToString("0.00") + "%";

				TextBlockRecheckNGCount.Text = recheckNGCount.ToString();
				TextBlockRecheckOKCount.Text = recheckOKCount.ToString();
				TextBlockRecheckYield.Text = (recheckYield * 100).ToString("0.00") + "%";

				DataGridViewTopNG.ColumnHeadersVisible = true;

				var rows = 10;
				for (int i = 0; i < rows; i++)
				{
					if (defectTypeAndCount.Count <= i)
					{
						break;
					}

					if (defectTypeAndCount.ElementAt(i).Key == "OKProductHasNoDefects")
					{
						rows++;
						continue;
					}

					var kv = defectTypeAndCount.ElementAt(i);
					DataGridViewTopNG.Rows.Add(i, kv.Key, kv.Value[2]);
				}

				generateChartForDefectCount(defectTypeAndCount.Count);
			}
			else
			{
                if (m_parent.m_global.m_strProductSubType == "glue_check")
                {
					if (totalProductCount == primaryOKCount)
					{
						TextBlockPrimaryOKCount.Text = primaryOKCount.ToString();
						TextBlockPrimaryYield.Text = (primaryYield * 100).ToString("0.00") + "%";

						TextBlockRecheckOKCount.Text = recheckOKCount.ToString();
						TextBlockRecheckYield.Text = (recheckYield * 100).ToString("0.00") + "%";
						return;
                    }
                }

				MessageBox.Show("未查询到缺陷和产品信息，请确认查询条件！");
			}

			if (m_parent.m_global.m_strProductType == "nova")
			{
				TextBlockPrimaryNGCount.Text = "当前产品不适用";
				TextBlockPrimaryOKCount.Text = "当前产品不适用";
				TextBlockPrimaryYield.Text = "当前产品不适用";
			}
		}

		private void updateStatisticsOnHomeView()
		{
			var statistics = m_parent.m_global.m_recheck_statistics;

			statistics.m_nTotalDefects = totalDefectCount;
			statistics.m_nTotalRecheckedProducts = primaryNGCount + primaryOKCount;
			statistics.m_nTotalPanels = trayAndProductWithRecheckResult.Count();

			statistics.m_nTotalNGProducts = primaryNGCount;
			statistics.m_nTotalOKProducts = primaryOKCount;

			statistics.m_nTotalRecheckedNGProducts = recheckNGCount;
			statistics.m_nTotalRecheckedOKProducts = recheckOKCount;

			Application.Current.Dispatcher.Invoke(new Action(() =>
			{
				// 更新统计数据
				m_parent.page_HomeView.textblock_TotalPanels.Text = m_parent.m_global.m_recheck_statistics.m_nTotalPanels.ToString();                                   //总盘数
				m_parent.page_HomeView.txtblock_ProductCount.Text = m_parent.m_global.m_recheck_statistics.m_nTotalRecheckedProducts.ToString();            //总产品数量
				m_parent.page_HomeView.textblock_TotalDefects.Text = m_parent.m_global.m_recheck_statistics.m_nTotalDefects.ToString();                               //总缺陷点数

				m_parent.page_HomeView.txt_FirstOks.Text = m_parent.m_global.m_recheck_statistics.m_nTotalOKProducts.ToString();                                           //一次ok数
				m_parent.page_HomeView.txt_FirstNGs.Text = m_parent.m_global.m_recheck_statistics.m_nTotalNGProducts.ToString();                                           //一次ng数

				m_parent.page_HomeView.textblock_TotalConfirmedOKs.Text = m_parent.m_global.m_recheck_statistics.m_nTotalRecheckedOKProducts.ToString();                       //二次ok数
				m_parent.page_HomeView.textblock_TotalConfirmedNGs.Text = m_parent.m_global.m_recheck_statistics.m_nTotalRecheckedNGProducts.ToString();                       //二次ng数

				//textblock_RecheckedOKs.Text = m_parent.m_global.m_recheck_statistics.m_nRecheckedOKs.ToString();                                       //复判ok点数
				//textblock_RecheckedNGs.Text = m_parent.m_global.m_recheck_statistics.m_nRecheckedNGs.ToString();                                       //复判ng点数

				if (m_parent.m_global.m_recheck_statistics.m_nTotalRecheckedProducts == 0)
				{
					m_parent.page_HomeView.textblock_FirstOKRate.Text = "0%";
					m_parent.page_HomeView.textblock_SecondOKRate.Text = "0%";
				}
				else
				{
					double dFirstOKRate = ((double)m_parent.m_global.m_recheck_statistics.m_nTotalOKProducts / (double)m_parent.m_global.m_recheck_statistics.m_nTotalRecheckedProducts) * 100;
					double dSecondOKRate = ((double)m_parent.m_global.m_recheck_statistics.m_nTotalRecheckedOKProducts / (double)m_parent.m_global.m_recheck_statistics.m_nTotalRecheckedProducts) * 100;

					m_parent.page_HomeView.textblock_FirstOKRate.Text = dFirstOKRate.ToString("F2") + "%";
					m_parent.page_HomeView.textblock_SecondOKRate.Text = dSecondOKRate.ToString("F2") + "%";
				}

			}));
		}

		/// <summary>
		/// 每次查询前调用，清理上次查询的残留数据和UI
		/// </summary>
		private void clearStatistics()
		{
			totalDefectCount = 0;
			primaryOKCount = 0;
			primaryNGCount = 0;
			primaryYield = 0;
			recheckOKCount = 0;
			recheckNGCount = 0;
			recheckYield = 0;

			currentDefectRecords.Clear();
			defectTypeAndCount.Clear();
			trayAndProductWithRecheckResult.Clear();
		}

		private void clearUI()
		{
			TextCurrentProductType.Text = "";
			TextBlockTotalDefectCount.Text = "0";
			TextBlockTotalProductCount.Text = "0";
			TextBlockTotalTrayCount.Text = "0";
			TextBlockPrimaryNGCount.Text = "0";
			TextBlockPrimaryOKCount.Text = "0";
			TextBlockPrimaryYield.Text = "0";
			TextBlockRecheckOKCount.Text = "0";
			TextBlockRecheckNGCount.Text = "0";
			TextBlockRecheckYield.Text = "0";

			DataGridViewTopNG.ColumnHeadersVisible = false;
			DataGridViewTopNG.Rows.Clear();

			MyChart.AxisX[0].Labels?.Clear();
			MyChart.AxisY[0].Labels?.Clear();
			MyChart.Series = new SeriesCollection();
		}

		/// <summary>
		/// 根据界面选择的条件组合查询语句
		/// </summary>
		/// <param name="trayTableNames">需要查询的所有表名</param>
		/// <returns></returns>
		private List<string> combineQuerySQL(ref List<string> trayTableNames)
		{
			List<string> sqlQuery = new List<string>();

			for (int i = 0; i < trayTableNames.Count; i++)
			{
				sqlQuery.Add("SELECT * FROM ");

				trayTableNames[i] = "defects_statistics_" + trayTableNames[i];
				trayTableNames[i] = trayTableNames[i].Replace("-", "_");
				sqlQuery[i] += trayTableNames[i];

				if (timeDateCondition_Start != "" || timeDateCondition_End != "")
				{
					if (productIdCondition == "" || productIdCondition == null)
					{
						sqlQuery[i] += " WHERE ";
					}
					else
					{
						sqlQuery[i] += $" WHERE product_id = \"{productIdCondition}\"";
					}
				}

				if (timeDateCondition_Start != "")
				{
					if (!sqlQuery[i].EndsWith("WHERE "))
					{
						sqlQuery[i] += " and ";
					}
					var datePart = timeDateCondition_Start.Split(' ')[0];
					sqlQuery[i] += "time_block >= \"" + datePart + $" {timeHourCondition_Start}:{timeMinCondition_Start}:00" + "\"";
				}
				if (timeDateCondition_End != "")
				{
					if (!sqlQuery[i].EndsWith("WHERE "))
					{
						sqlQuery[i] += " and ";
					}
					var datePart = timeDateCondition_End.Split(' ')[0];
					sqlQuery[i] += "time_block < \"" + datePart + $" {timeHourCondition_End}:{timeMinCondition_End}:00" + "\"";
				}

				sqlQuery[i] += ";";
			}

			return sqlQuery;
		}

		/// <summary>
		/// 初始化左下角Top缺陷项数据表
		/// </summary>
		private void initializeTopDefectDataGridView()
		{
			DataGridViewTopNG.ColumnCount = 3;
			DataGridViewTopNG.Columns[0].HeaderCell.Value = "序号";
			DataGridViewTopNG.Columns[1].HeaderCell.Value = "缺陷名称";
			DataGridViewTopNG.Columns[2].HeaderCell.Value = "数量";
			DataGridViewTopNG.ReadOnly = true;
			DataGridViewTopNG.RowHeadersVisible = false;
			DataGridViewTopNG.ColumnHeadersVisible = false;
			DataGridViewTopNG.EnableHeadersVisualStyles = false;
			DataGridViewTopNG.AllowUserToResizeRows = false;

			DataGridViewTopNG.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.Single;
			DataGridViewTopNG.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			DataGridViewTopNG.RowsDefaultCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
			DataGridViewTopNG.ColumnHeadersDefaultCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;

			DataGridViewTopNG.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(236, 230, 239);
			DataGridViewTopNG.ColumnHeadersDefaultCellStyle.SelectionBackColor = System.Drawing.Color.FromArgb(236, 230, 239);
			DataGridViewTopNG.DefaultCellStyle.SelectionForeColor = System.Drawing.Color.Black;

			double DPI_ratio = Graphics.FromHwnd(new WindowInteropHelper(m_parent).Handle).DpiX / 180;
			DataGridViewTopNG.Columns[0].Width = ((int)(DataGridViewTopNG.Width * DPI_ratio) * 30) / 100;
			DataGridViewTopNG.Columns[1].Width = ((int)(DataGridViewTopNG.Width * DPI_ratio) * 120) / 100;
			DataGridViewTopNG.Columns[2].Width = ((int)(DataGridViewTopNG.Width * DPI_ratio) * 55) / 100;

			for (int n = 0; n < DataGridViewTopNG.ColumnCount; n++)
				DataGridViewTopNG.Columns[n].SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;

		}

		/// <summary>
		/// 初始化机台选择列表和图标生成选择列表
		/// </summary>
		private void initializeComboBoxs()
		{
			ComboBoxMachineId.Items.Add("全部机台");
			for (int n = 0; n < m_parent.m_global.m_dict_machine_names_and_IPs.Keys.Count; n++)
			{
				ComboBoxMachineId.Items.Add(m_parent.m_global.m_dict_machine_names_and_IPs.Keys.ElementAt(n));
			}

			ComboBoxChartCondition.Items.Add("全部缺陷");
			ComboBoxChartCondition.Items.Add("TOP5缺陷");
			ComboBoxChartCondition.Items.Add("TOP10缺陷");
			ComboBoxChartCondition.SelectedItem = ComboBoxChartCondition.Items[0];

			for (int i = 0; i < 24; i++)
			{
				ComboBoxHourPickerStart.Items.Add(i.ToString("00"));
				ComboBoxHourPickerEnd.Items.Add(i.ToString("00"));
			}

			ComboBoxMinPickerStart.Items.Add("00");
			ComboBoxMinPickerStart.Items.Add("15");
			ComboBoxMinPickerStart.Items.Add("30");
			ComboBoxMinPickerStart.Items.Add("45");
			ComboBoxMinPickerEnd.Items.Add("00");
			ComboBoxMinPickerEnd.Items.Add("15");
			ComboBoxMinPickerEnd.Items.Add("30");
			ComboBoxMinPickerEnd.Items.Add("45");

		}

		private void synchronizeUI(CancellationToken token)
		{
			try
			{
				while (!token.IsCancellationRequested)
				{
					synchronizeUITrigger.WaitOne();

					if (token.IsCancellationRequested)
					{
						break;
					}

					List<string> trayTableNames = new List<string>();
					var mid = m_parent.m_global.m_current_tray_info_for_Dock?.machine_id;
					if (mid != null)
					{
						trayTableNames.Add(mid);
					}
					else
					{
						continue;
					}

					List<string> sqlQuerys = new List<string>();
					for (int i = 0; i < trayTableNames.Count; i++)
					{
						sqlQuerys.Add("SELECT * FROM ");

						trayTableNames[i] = "defects_statistics_" + trayTableNames[i];
						trayTableNames[i] = trayTableNames[i].Replace("-", "_");

						sqlQuerys[i] += trayTableNames[i];
						sqlQuerys[i] += " WHERE ";

						if (DateTime.Now.Hour > 8 && DateTime.Now.Hour < 20)
						{
							sqlQuerys[i] += "time_block >= \"" + DateTime.Now.Date.ToString().Split(' ')[0] + $" 8:00:00" + "\"";
						}
						else if (DateTime.Now.Hour >= 20)
						{
							sqlQuerys[i] += "time_block >= \"" + DateTime.Now.Date.ToString().Split(' ')[0] + $" 20:00:00" + "\"";
						}
						else if (DateTime.Now.Hour < 8)
						{
							sqlQuerys[i] += "time_block >= \"" + DateTime.Now.AddDays(-1).ToString().Split(' ')[0] + $" 20:00:00" + "\"";
						}

						sqlQuerys[i] += " and ";
						sqlQuerys[i] += "time_block < \"" + DateTime.Now.ToString() + "\"";
						sqlQuerys[i] += ";";
					}

					List<DefectStatistics> defectStatistics = new List<DefectStatistics>();

					string connStr = string.Format($"server=localhost;user=root;port={m_parent.m_global.m_nMysqlPort};password=123456;SslMode=None;allowPublicKeyRetrieval=true;");
					var m_connection = new MySqlConnection(connStr);
					m_connection.Open();

					string useSQL = "USE autoscanfqctest";
					MySqlCommand cmd = new MySqlCommand(useSQL, m_connection);
					cmd.ExecuteNonQuery();

					foreach (var sql in sqlQuerys)
					{
						string queryResult = "";

						cmd = new MySqlCommand(sql, m_connection);
						MySqlDataReader rdr = cmd.ExecuteReader();

						while (rdr.Read())
						{
							for (int i = 0; i < rdr.FieldCount; i++)
							{
								queryResult += rdr[i].ToString() + "@";
							}
							queryResult += "\n";
						}

						List<DefectStatistics> temp = new List<DefectStatistics>();
						m_parent.m_global.m_database_service.ProcessDefectStatisticsQueryResult(queryResult, ref temp);

						defectStatistics.AddRange(temp);
					}

					clearStatistics();

					handleDefectRecord(defectStatistics);

					updateStatisticsOnHomeView();

					//taskCompletionSource.SetResult(true);
				}
			}
			catch (Exception ex)
			{
				//MessageBox.Show($"数据同步遇到意外：{ex.Message} 主界面数据不准，请在缺陷统计系统中查询");
			}
		}

        public void TriggerSynchronizeUI()
		{
			//taskCompletionSource = tcs;
			synchronizeUITrigger.Set();
		}

		public void updateGlueCheckData(List<int> data)
		{
			//检胶读取的数据第一位为总盘数
			totalTrayCount = data[0];
			// 第三位为产品总数
			totalProductCount = data[2];
			// 第二位为一次OK数
			primaryOKCount = data[1];
			primaryNGCount = totalProductCount - primaryOKCount;
		}
	}
}