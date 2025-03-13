using AutoScanFQCTest.DataModels;
using AutoScanFQCTest.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static System.Windows.Forms.LinkLabel;
using MessageBox = System.Windows.MessageBox;

namespace AutoScanFQCTest.DialogWindows
{
    public partial class QueryDatabase : Window
    {
        public MainWindow m_parent;

        private List<ProductResult> lastSearch;
        public QueryDatabase(MainWindow parent)
        {
            InitializeComponent();

            m_parent = parent;

            // 设置窗口事件
            this.Loaded += Window_Loaded;
        }

        // 窗口加载
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // 设置 DatePicker 控件的初始日期为当前日期
            if (true)
            {
                // 设置 DatePicker 控件的初始日期为当前日期
                datePicker_StartTime.SelectedDate = DateTime.Now;
                datePicker_EndTime.SelectedDate = DateTime.Now;

                // 添加小时数到 comboBox_StartHour
                for (int i = 0; i < 24; i++)
                {
                    comboBox_StartHour.Items.Add(i.ToString("00"));
                    comboBox_EndHour.Items.Add(i.ToString("00"));
                }

                // 添加分钟数到 comboBox_StartMinute
                for (int i = 0; i < 60; i++)
                {
                    comboBox_StartMinute.Items.Add(i.ToString("00"));
                    comboBox_EndMinute.Items.Add(i.ToString("00"));
                }

                // 添加秒数到 comboBox_StartSecond
                for (int i = 0; i < 60; i++)
                {
                    comboBox_StartSecond.Items.Add(i.ToString("00"));
                    comboBox_EndSecond.Items.Add(i.ToString("00"));
                }

                // 设置 comboBox_StartHour, comboBox_StartMinute, comboBox_StartSecond 的初始值
                comboBox_StartHour.SelectedIndex = 0;
                comboBox_StartMinute.SelectedIndex = 0;
                comboBox_StartSecond.SelectedIndex = 0;

                // 设置 comboBox_EndHour, comboBox_EndMinute, comboBox_EndSecond 的初始值
                comboBox_EndHour.SelectedIndex = 23;
                comboBox_EndMinute.SelectedIndex = 59;
                comboBox_EndSecond.SelectedIndex = 59;

                // 设置 comboBox_All 的初始值
                comboBox_All.Items.Add("全部");
                comboBox_All.SelectedIndex = 0;
            }

            #region Old
            // 构造查询结果列表gridview
            //if (true)
            //{
            //    gridview_QueryResults.RowHeadersVisible = false;
            //    gridview_QueryResults.ReadOnly = true;
            //    gridview_QueryResults.ColumnCount = 9;
            //    gridview_QueryResults.ColumnHeadersHeight = 40;
            //    gridview_QueryResults.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;

            //    gridview_QueryResults.EnableHeadersVisualStyles = false;
            //    gridview_QueryResults.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(236, 230, 239);

            //    gridview_QueryResults.ColumnHeadersVisible = true;
            //    gridview_QueryResults.RowsDefaultCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;

            //    gridview_QueryResults.ColumnHeadersDefaultCellStyle.SelectionBackColor = System.Drawing.Color.FromArgb(236, 230, 239);

            //    gridview_QueryResults.AllowUserToResizeRows = false;

            //    double DPI_ratio = Graphics.FromHwnd(new WindowInteropHelper(m_parent).Handle).DpiX / 96;

            //    gridview_QueryResults.ColumnHeadersDefaultCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            //    //gridview_CalibResult.Columns[0].Width = 70;
            //    //gridview_CalibResult.Columns[1].Width = 155;
            //    //gridview_CalibResult.Columns[2].Width = 165;

            //    DPI_ratio = 1.0;

            //    gridview_QueryResults.Columns[0].Width = ((int)(gridview_QueryResults.Width * DPI_ratio) * 14) / 100;
            //    gridview_QueryResults.Columns[1].Width = ((int)(gridview_QueryResults.Width * DPI_ratio) * 8) / 100;
            //    gridview_QueryResults.Columns[2].Width = ((int)(gridview_QueryResults.Width * DPI_ratio) * 22) / 100;
            //    gridview_QueryResults.Columns[3].Width = ((int)(gridview_QueryResults.Width * DPI_ratio) * 12) / 100;
            //    gridview_QueryResults.Columns[4].Width = ((int)(gridview_QueryResults.Width * DPI_ratio) * 8) / 100;
            //    gridview_QueryResults.Columns[5].Width = ((int)(gridview_QueryResults.Width * DPI_ratio) * 8) / 100;
            //    gridview_QueryResults.Columns[6].Width = ((int)(gridview_QueryResults.Width * DPI_ratio) * 10) / 100;
            //    gridview_QueryResults.Columns[7].Width = ((int)(gridview_QueryResults.Width * DPI_ratio) * 9) / 100;
            //    gridview_QueryResults.Columns[8].Width = ((int)(gridview_QueryResults.Width * DPI_ratio) * 9) / 100;
            //    gridview_QueryResults.Columns[0].Name = "机台";
            //    gridview_QueryResults.Columns[1].Name = "索引";
            //    gridview_QueryResults.Columns[2].Name = "二维码";
            //    gridview_QueryResults.Columns[3].Name = "测试类型";
            //    gridview_QueryResults.Columns[4].Name = "pcsSeq";
            //    gridview_QueryResults.Columns[5].Name = "测试结果";
            //    gridview_QueryResults.Columns[6].Name = "测试人员";
            //    gridview_QueryResults.Columns[7].Name = "复判结果";
            //    gridview_QueryResults.Columns[8].Name = "复判人员";
            //    for (int n = 0; n < gridview_QueryResults.ColumnCount; n++)
            //        gridview_QueryResults.Columns[n].SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;

            //    foreach (System.Windows.Forms.DataGridViewRow row in gridview_QueryResults.Rows)
            //    {
            //        //row.DefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(247, 245, 249);
            //    }

            //    //gridview_FoundDefectsInTotal.Height = (int)grid_Test.ActualHeight;
            //    //gridview_FoundDefectsInTotal.Update();

            //    //RefreshCalibResult(gridview_CalibResult);
            //}
            #endregion

        }

        // 按钮点击事件：查询（指定时间段）
        private void BtnSearchForSpecifiedTime_Click(object sender, RoutedEventArgs e)
        {
            #region Old
            //// 查询数据库
            //string strQueryResult = "";
            //string strTableName = "Product";

            //string strProductID = textbox_ProductID.Text.Trim();

            //// 检查产品ID是否为空
            //if (string.IsNullOrEmpty(strProductID))
            //{
            //    MessageBox.Show("请输入产品ID");
            //    return;
            //}

            //// 查询产品信息
            //if (false)
            //{
            //    strTableName = "BatchInfo";

            //    string strQuery = "SELECT * FROM " + strTableName + " WHERE batch_id = '" + strProductID + "';";

            //    // 查询耗时
            //    TimeSpan ts = new TimeSpan(0, 0, 0);

            //    // 查询数据库
            //    m_parent.m_global.m_mysql_ops.QueryTableData(strTableName, strQuery, ref strQueryResult, ref ts);

            //    // 解析查询结果
            //    List<AVITrayInfo> list_batch_info = new List<AVITrayInfo>();
            //    string[] lines = strQueryResult.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);

            //    // 如果查询结果为空
            //    if (lines.Length == 0)
            //    {
            //        MessageBox.Show("未查询到表格 {0} 中条件符合的数据", strTableName);
            //        return;
            //    }

            //    for (int i = 0; i < lines.Length; i++)
            //    {
            //        string[] parts = lines[i].Trim().Split(new char[] { ' ' }, StringSplitOptions.None);

            //        if (parts.Length >= 15) // 确保有足够的数据来创建AVIProductInfo对象
            //        {
            //            AVITrayInfo batch_info = new AVITrayInfo
            //            {
            //                // 假设数据顺序为：batch_id, row, col, front, mid, operator, operator_id, product_id, resource, scan_code_mode_, set_id, total_pcs, uuid, work_area, full_status
            //                // 并且batch_id是我们不需要的一个值
            //                BatchId = parts[0],
            //                Row = int.Parse(parts[1]),
            //                Col = int.Parse(parts[2]),
            //                Front = bool.Parse(parts[3]),
            //                Mid = parts[4],
            //                Operator = parts[5],
            //                OperatorId = parts[6],
            //                ProductId = parts[7],
            //                Resource = parts[8],
            //                ScanCodeMode = int.Parse(parts[9]),
            //                SetId = parts[10],
            //                TotalPcs = int.Parse(parts[11]),
            //                Uuid = parts[12],
            //                WorkArea = parts[13],
            //                Fullstatus = parts[14]
            //            };

            //            list_batch_info.Add(batch_info);
            //        }
            //    }

            //    // 将查询结果显示在gridview中
            //    gridview_QueryResults.Rows.Clear();

            //    for (int i = 0; i < list_batch_info.Count; i++)
            //    {
            //        AVITrayInfo batch_info = list_batch_info[i];

            //        string[] row = new string[9];
            //        row[0] = batch_info.Resource;
            //        row[1] = (i + 1).ToString();
            //        row[2] = batch_info.BatchId;
            //        row[3] = batch_info.WorkArea;
            //        row[4] = "";
            //        row[5] = "";
            //        row[6] = batch_info.Operator;
            //        row[7] = "";
            //        row[8] = "";

            //        gridview_QueryResults.Columns[0].Name = "机台";
            //        gridview_QueryResults.Columns[1].Name = "索引";
            //        gridview_QueryResults.Columns[2].Name = "二维码";
            //        gridview_QueryResults.Columns[3].Name = "测试类型";
            //        gridview_QueryResults.Columns[4].Name = "pcsSeq";
            //        gridview_QueryResults.Columns[5].Name = "测试结果";
            //        gridview_QueryResults.Columns[6].Name = "测试人员";
            //        gridview_QueryResults.Columns[7].Name = "复判结果";
            //        gridview_QueryResults.Columns[8].Name = "复判人员";

            //        gridview_QueryResults.Rows.Add(row);
            //    }
            //// 查询产品数据
            //if (true)
            //{
            //    // 得到 datePicker_StartTime 的时间
            //    string startTime = datePicker_StartTime.SelectedDate.Value.ToString("yyyy-MM-dd");
            //    string startHour = comboBox_StartHour.SelectedItem.ToString();
            //    string startMinute = comboBox_StartMinute.SelectedItem.ToString();
            //    string startSecond = comboBox_StartSecond.SelectedItem.ToString();
            //    startTime += " " + startHour + ":" + startMinute + ":" + startSecond;

            //    // 得到 datePicker_EndTime 的时间
            //    string endTime = datePicker_EndTime.SelectedDate.Value.ToString("yyyy-MM-dd");
            //    string endHour = comboBox_EndHour.SelectedItem.ToString();
            //    string endMinute = comboBox_EndMinute.SelectedItem.ToString();
            //    string endSecond = comboBox_EndSecond.SelectedItem.ToString();
            //    endTime += " " + endHour + ":" + endMinute + ":" + endSecond;

            //    string strQuery = "SELECT * FROM " + strTableName + " WHERE panel_id = '" + strProductID
            //        + "' AND create_time BETWEEN '" + startTime + "' AND '" + endTime + "';";

            //    // 查询耗时
            //    TimeSpan ts = new TimeSpan(0, 0, 0);

            //    // 查询数据库
            //    m_parent.m_global.m_mysql_ops.QueryTableData(strTableName, strQuery, ref strQueryResult, ref ts);

            //    // 显示查询耗时
            //    textblock_QueryTime.Text = "查询耗时：" + ts.TotalSeconds.ToString("0.000") + "秒";

            //    // 解析查询结果
            //    List<Product> list_products = new List<Product>();
            //    string[] lines = strQueryResult.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);

            //    // 如果查询结果为空
            //    if (lines.Length == 0)
            //    {
            //        MessageBox.Show("未查询到表格 {0} 中条件符合的数据", strTableName);
            //        return;
            //    }

            //    for (int i = 0; i < lines.Length; i++)
            //    {
            //        string[] parts = lines[i].Trim().Split(new char[] { ' ' }, StringSplitOptions.None);

            //        if (parts.Length >= 5) // 确保有足够的数据来创建Product对象
            //        {
            //            Product product = new Product
            //            {
            //                // 假设数据顺序为：id, panel_id, bar_code, image, image_a, image_b, is_null, pos_col, pos_row, shareimg_path
            //                // 并且id是我们不需要的一个值
            //                BarCode = parts[1],
            //                Image = parts[2],
            //                ImageA = parts[3],
            //                ImageB = parts[4],
            //                IsNull = parts[5],
            //                PanelId = parts[6],
            //                PosCol = int.Parse(parts[7]),
            //                PosRow = int.Parse(parts[8]),
            //                ShareimgPath = parts[9]
            //            };

            //            list_products.Add(product);
            //        }
            //    }

            //    // 将查询结果显示在gridview中
            //    gridview_QueryResults.Rows.Clear();
            //    for (int i = 0; i < list_products.Count; i++)
            //    {
            //        Product product = list_products[i];

            //        string[] row = new string[10];
            //        row[0] = "";
            //        row[1] = (i + 1).ToString();
            //        row[2] = product.BarCode;
            //        row[3] = "";
            //        row[4] = "";
            //        row[5] = "";
            //        row[6] = "";
            //        row[7] = "";
            //        row[8] = "";
            //        row[9] = "";

            //        gridview_QueryResults.Rows.Add(row);
            //    }

            //    // 更新总数
            //    textbox_TotalCount.Text = list_products.Count.ToString() + "条";
            //}

            //// 查询产品缺陷数据
            //if (false)
            //{
            //    string strQuery = "SELECT * FROM " + strTableName + " WHERE panel_id = '" + strProductID + "';";

            //    // 查询耗时
            //    TimeSpan ts = new TimeSpan(0, 0, 0);

            //    // 查询数据库
            //    m_parent.m_global.m_mysql_ops.QueryTableData(strTableName, strQuery, ref strQueryResult, ref ts);

            //    // 显示查询耗时
            //    textblock_QueryTime.Text = "查询耗时：" + ts.TotalSeconds.ToString("0.000") + "秒";

            //    // 解析查询结果
            //    List<Defect> defects = new List<Defect>();
            //    string[] lines = strQueryResult.Split(new string[] { "\n" }, StringSplitOptions.None);

            //    // 如果查询结果为空
            //    if (lines.Length == 0)
            //    {
            //        MessageBox.Show("未查询到表格 {0} 中条件符合的数据", strTableName);
            //        return;
            //    }

            //    for (int i = 0; i < lines.Length; i++)
            //    {
            //        string[] parts = lines[i].Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            //        if (parts.Length >= 9) // 确保有足够的数据来创建Defect对象
            //        {
            //            Defect defect = new Defect
            //            {
            //                // 假设数据顺序为：id, sn, type, height, width, x, y, channel, channelNum
            //                // 并且id是我们不需要的一个值
            //                Sn = int.Parse(parts[2]),
            //                Type = parts[3],
            //                Height = double.Parse(parts[4]),
            //                Width = double.Parse(parts[5]),
            //                X = double.Parse(parts[6]),
            //                Y = double.Parse(parts[7]),
            //                Channel = int.Parse(parts[8]),
            //                ChannelNum = int.Parse(parts[9])
            //            };

            //            defects.Add(defect);
            //        }
            //    }
            //}
            //}
            #endregion
            string barcode = textbox_ProductID.Text;
            if (string.IsNullOrEmpty(barcode))
            {
                MessageBox.Show("请输入二维码！");
                return;
            }
            Stopwatch sw = new Stopwatch();
            sw.Start();
            // 得到 datePicker_StartTime 的时间
            string startTime = datePicker_StartTime.SelectedDate.Value.ToString("yyyy-MM-dd");
            string startHour = comboBox_StartHour.SelectedItem.ToString();
            string startMinute = comboBox_StartMinute.SelectedItem.ToString();
            string startSecond = comboBox_StartSecond.SelectedItem.ToString();
            startTime += " " + startHour + ":" + startMinute + ":" + startSecond;

            // 得到 datePicker_EndTime 的时间
            string endTime = datePicker_EndTime.SelectedDate.Value.ToString("yyyy-MM-dd");
            string endHour = comboBox_EndHour.SelectedItem.ToString();
            string endMinute = comboBox_EndMinute.SelectedItem.ToString();
            string endSecond = comboBox_EndSecond.SelectedItem.ToString();
            endTime += " " + endHour + ":" + endMinute + ":" + endSecond;

            //获取setid
            string sql = $@"SELECT SetId  FROM productresult WHERE BarCode='{barcode}' LIMIT 0,1;";
            object obj = MySqlHelpers.ExecuteScalar(m_parent.m_global.ConnsectionString, CommandType.Text, sql);
            if (obj == null)
            {
                MessageBox.Show("未能查询当该盘信息！");
                return;
            }
            string setid = obj.ToString();
            sql = $@"SELECT *  FROM productresult WHERE SetId='{setid}' and STR_TO_DATE(TestDate, '%Y-%m-%d %H:%i:%s') BETWEEN '{startTime}' AND '{endTime}';";
            var list = GetResult(sql);
            lastSearch = list;
            BindDGV(list);
            sw.Stop();
            // 更新总数
            textbox_TotalCount.Text = list?.Count > 0 ? list.Count.ToString() + "条" : "0条";
            // 显示查询耗时
            textblock_QueryTime.Text = "查询耗时：" + sw.Elapsed.TotalSeconds.ToString("F3") + "秒";
        }

        // 按钮点击事件：查询（所有时间）
        private void BtnSearchForAllTime_Click(object sender, RoutedEventArgs e)
        {
            #region Old
            //// 查询数据库
            //string strQueryResult = "";
            //string strTableName = "Product";

            //string strProductID = textbox_ProductID.Text.Trim();

            //// 检查产品ID是否为空
            //if (string.IsNullOrEmpty(strProductID))
            //{
            //    MessageBox.Show("请输入产品ID");
            //    return;
            //}

            //// 清除
            //gridview_QueryResults.Rows.Clear();

            //// 查询产品数据
            //if (true)
            //{
            //    // 得到 datePicker_StartTime 的时间
            //    string startTime = datePicker_StartTime.SelectedDate.Value.ToString("yyyy-MM-dd");
            //    string startHour = comboBox_StartHour.SelectedItem.ToString();
            //    string startMinute = comboBox_StartMinute.SelectedItem.ToString();
            //    string startSecond = comboBox_StartSecond.SelectedItem.ToString();
            //    startTime += " " + startHour + ":" + startMinute + ":" + startSecond;

            //    // 得到 datePicker_EndTime 的时间
            //    string endTime = datePicker_EndTime.SelectedDate.Value.ToString("yyyy-MM-dd");
            //    string endHour = comboBox_EndHour.SelectedItem.ToString();
            //    string endMinute = comboBox_EndMinute.SelectedItem.ToString();
            //    string endSecond = comboBox_EndSecond.SelectedItem.ToString();
            //    endTime += " " + endHour + ":" + endMinute + ":" + endSecond;

            //    string strQuery = "SELECT * FROM " + strTableName + " WHERE panel_id = '" + strProductID + "';";

            //    // 查询耗时
            //    TimeSpan ts = new TimeSpan(0, 0, 0);

            //    // 查询数据库
            //    m_parent.m_global.m_mysql_ops.QueryTableData(strTableName, strQuery, ref strQueryResult, ref ts);

            //    // 显示查询耗时
            //    textblock_QueryTime.Text = "查询耗时：" + ts.TotalSeconds.ToString("0.000") + "秒";

            //    List<AVITrayInfo> list_trays = new List<AVITrayInfo>();

            //    // 处理查询结果
            //    m_parent.m_global.m_database_service.ProcessTrayDataQueryResult(strQueryResult, ref list_trays);

            //    if (list_trays.Count == 0)
            //    {
            //        MessageBox.Show("未查询到表格 {0} 中条件符合的数据", strTableName);
            //        return;
            //    }

            //    // 将查询结果显示在gridview中
            //    for (int i = 0; i < list_trays.Count; i++)
            //    {
            //        AVITrayInfo tray = list_trays[i];

            //        string[] row = new string[10];
            //        row[0] = "";
            //        row[1] = (i + 1).ToString();

            //        gridview_QueryResults.Rows.Add(row);
            //    }

            //    // 更新总数
            //    textbox_TotalCount.Text = list_trays.Count.ToString() + "条";
            //}
            #endregion
            string barcode = textbox_ProductID.Text;
            if (string.IsNullOrEmpty(barcode))
            {
                MessageBox.Show("请输入二维码！");
                return;
            }
            Stopwatch sw = new Stopwatch();
            sw.Start();
            //获取setid
            string sql = $@"SELECT SetId  FROM productresult WHERE BarCode='{barcode}' LIMIT 0,1;";
            object obj = MySqlHelpers.ExecuteScalar(m_parent.m_global.ConnsectionString, CommandType.Text, sql);
            if (obj == null)
            {
                MessageBox.Show("未能查询当该盘信息！");
                return;
            }
            string setid = obj.ToString();
            sql = $@"SELECT *  FROM productresult WHERE SetId='{setid}';";
            var list = GetResult(sql);
            lastSearch = list;
            BindDGV(list);
            sw.Stop();
            // 更新总数
            textbox_TotalCount.Text = list?.Count > 0 ? list.Count.ToString() + "条" : "0条";
            // 显示查询耗时
            textblock_QueryTime.Text = "查询耗时：" + sw.Elapsed.TotalSeconds.ToString("F3") + "秒";
        }

        // 按钮点击事件：按时间查询
        private void button_QueryByTime_Click(object sender, RoutedEventArgs e)
        {
            #region Old
            //// 查询数据库
            //string strQueryResult = "";
            //string strTableName = "Product";

            //string strProductID = textbox_ProductID.Text.Trim();

            //// 查询产品数据
            //if (true)
            //{
            //    // 得到 datePicker_StartTime 的时间
            //    string startTime = datePicker_StartTime.SelectedDate.Value.ToString("yyyy-MM-dd");
            //    string startHour = comboBox_StartHour.SelectedItem.ToString();
            //    string startMinute = comboBox_StartMinute.SelectedItem.ToString();
            //    string startSecond = comboBox_StartSecond.SelectedItem.ToString();
            //    startTime += " " + startHour + ":" + startMinute + ":" + startSecond;

            //    // 得到 datePicker_EndTime 的时间
            //    string endTime = datePicker_EndTime.SelectedDate.Value.ToString("yyyy-MM-dd");
            //    string endHour = comboBox_EndHour.SelectedItem.ToString();
            //    string endMinute = comboBox_EndMinute.SelectedItem.ToString();
            //    string endSecond = comboBox_EndSecond.SelectedItem.ToString();
            //    endTime += " " + endHour + ":" + endMinute + ":" + endSecond;

            //    string strQuery = "SELECT * FROM " + strTableName + " WHERE panel_id = '" + strProductID
            //        + "' AND create_time BETWEEN '" + startTime + "' AND '" + endTime + "';";

            //    // 如果strProductID为空，则查询所有产品数据
            //    if (strProductID == "")
            //    {
            //        strQuery = "SELECT * FROM " + strTableName + " WHERE create_time BETWEEN '" + startTime + "' AND '" + endTime + "';";
            //    }

            //    // 查询耗时
            //    TimeSpan ts = new TimeSpan(0, 0, 0);

            //    // 查询数据库
            //    m_parent.m_global.m_mysql_ops.QueryTableData(strTableName, strQuery, ref strQueryResult, ref ts);

            //    // 显示查询耗时
            //    textblock_QueryTime.Text = "查询耗时：" + ts.TotalSeconds.ToString("0.000") + "秒";

            //    // 处理查询结果
            //    List<Product> list_products = new List<Product>();
            //    m_parent.m_global.m_database_service.ProcessProductDataQueryResult(strQueryResult, ref list_products);

            //    if (list_products.Count == 0)
            //    {
            //        MessageBox.Show("未查询到表格 {0} 中条件符合的数据", strTableName);
            //        return;
            //    }

            //    // 将查询结果显示在gridview中
            //    for (int i = 0; i < list_products.Count; i++)
            //    {
            //        Product product = list_products[i];

            //        string[] row = new string[10];
            //        row[0] = "";
            //        row[1] = (i + 1).ToString();
            //        row[2] = product.BarCode;
            //        row[3] = "";
            //        row[4] = "";
            //        row[5] = "";
            //        row[6] = "";
            //        row[7] = "";
            //        row[8] = "";
            //        row[9] = "";

            //        gridview_QueryResults.Rows.Add(row);
            //    }

            //    // 更新总数
            //    textbox_TotalCount.Text = list_products.Count.ToString() + "条";
            //}
            #endregion
            Stopwatch sw = new Stopwatch();
            sw.Start();
            // 得到 datePicker_StartTime 的时间
            string startTime = datePicker_StartTime.SelectedDate.Value.ToString("yyyy-MM-dd");
            string startHour = comboBox_StartHour.SelectedItem.ToString();
            string startMinute = comboBox_StartMinute.SelectedItem.ToString();
            string startSecond = comboBox_StartSecond.SelectedItem.ToString();
            startTime += " " + startHour + ":" + startMinute + ":" + startSecond;

            // 得到 datePicker_EndTime 的时间
            string endTime = datePicker_EndTime.SelectedDate.Value.ToString("yyyy-MM-dd");
            string endHour = comboBox_EndHour.SelectedItem.ToString();
            string endMinute = comboBox_EndMinute.SelectedItem.ToString();
            string endSecond = comboBox_EndSecond.SelectedItem.ToString();
            endTime += " " + endHour + ":" + endMinute + ":" + endSecond;
            string sql = $@"SELECT *  FROM productresult WHERE STR_TO_DATE(TestDate, '%Y-%m-%d %H:%i:%s') BETWEEN '{startTime}' AND '{endTime}';";
            lastSearch = new List<ProductResult>();
            var list = GetResult(sql);
            lastSearch = list;
            BindDGV(list);
            sw.Stop();
            // 更新总数
            textbox_TotalCount.Text = list?.Count > 0 ? list.Count.ToString() + "条" : "0条";
            // 显示查询耗时
            textblock_QueryTime.Text = "查询耗时：" + sw.Elapsed.TotalSeconds.ToString("F3") + "秒";
        }
        /// <summary>
        /// 获取结果表数据
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        private List<ProductResult> GetResult(string sql)
        {
            try
            {
                var data = MySqlHelpers.GetDataTable(m_parent.m_global.ConnsectionString, CommandType.Text, sql);
                if (data == null || data?.Rows.Count <= 0)
                {
                    MessageBox.Show("未查询到条件符合的数据!");
                    return null;
                }
                var list = ListHelper.DataTableToList<ProductResult>(data);
                return list;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                m_parent.m_global.m_log_presenter.Log(ex.Message);
                return null;
            }
        }

        /// <summary>
        /// 绑定datagridview
        /// </summary>
        /// <param name="list"></param>
        private void BindDGV(List<ProductResult> list)
        {
            try
            {
                gridview_QueryResults.AutoGenerateColumns = false;
                gridview_QueryResults.DataSource = null;
                gridview_QueryResults.Columns.Clear();
                DataGridViewTextBoxColumn SetId = new DataGridViewTextBoxColumn();
                SetId.HeaderText = "物料盘id";
                SetId.DataPropertyName = "SetId"; // 关联到实体类的Id属性
                gridview_QueryResults.Columns.Add(SetId);
                DataGridViewTextBoxColumn BarCode = new DataGridViewTextBoxColumn();
                BarCode.HeaderText = "产品id";
                BarCode.DataPropertyName = "BarCode"; // 关联到实体类的Id属性
                gridview_QueryResults.Columns.Add(BarCode);
                DataGridViewTextBoxColumn Result = new DataGridViewTextBoxColumn();
                Result.HeaderText = "复判结果";
                Result.DataPropertyName = "Result"; // 关联到实体类的Id属性
                gridview_QueryResults.Columns.Add(Result);
                DataGridViewTextBoxColumn TestDate = new DataGridViewTextBoxColumn();
                TestDate.HeaderText = "复判时间";
                TestDate.DataPropertyName = "TestDate"; // 关联到实体类的Id属性
                gridview_QueryResults.Columns.Add(TestDate);
                DataGridViewTextBoxColumn Tester = new DataGridViewTextBoxColumn();
                Tester.HeaderText = "复判人";
                Tester.DataPropertyName = "Tester"; // 关联到实体类的Id属性
                gridview_QueryResults.Columns.Add(Tester);
                gridview_QueryResults.ColumnHeadersHeight = 40;
                double DPI_ratio = 1.0;
                gridview_QueryResults.Columns[0].Width = ((int)(gridview_QueryResults.Width * DPI_ratio) * 25) / 100;
                gridview_QueryResults.Columns[1].Width = ((int)(gridview_QueryResults.Width * DPI_ratio) * 25) / 100;
                gridview_QueryResults.Columns[2].Width = ((int)(gridview_QueryResults.Width * DPI_ratio) * 10) / 100;
                gridview_QueryResults.Columns[3].Width = ((int)(gridview_QueryResults.Width * DPI_ratio) * 25) / 100;
                gridview_QueryResults.Columns[4].Width = ((int)(gridview_QueryResults.Width * DPI_ratio) * 15) / 100;
                gridview_QueryResults.DataSource = list;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                m_parent.m_global.m_log_presenter.Log(ex.Message);
            }
        }

        // 按钮点击事件：导出CSV
        private void button_ExportCSV_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (lastSearch == null || lastSearch?.Count <= 0)
                {
                    MessageBox.Show("请先查询要导出的结果！");
                    return;
                }
                FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
                folderBrowserDialog.ShowDialog();
                //获取选择的文件夹的全路径名
                string directoryPath = folderBrowserDialog.SelectedPath;
                CSVHelper.WriteCSV(directoryPath, lastSearch);
                MessageBox.Show("导出成功！");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                m_parent.m_global.m_log_presenter.Log(ex.Message);
            }
        }

        // 按钮点击事件：清空查询结果
        private void BtnClearResults_Click(object sender, RoutedEventArgs e)
        {
            gridview_QueryResults.DataSource = null;
            lastSearch = null;
            textbox_TotalCount.Text = "0条";
            textblock_QueryTime.Text = "查询耗时：";
        }



    }
}
