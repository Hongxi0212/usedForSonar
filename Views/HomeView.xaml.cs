using AutoScanFQCTest.Canvas;
using AutoScanFQCTest.DataModels;
using AutoScanFQCTest.DialogWindows;
using AutoScanFQCTest.Logics;
using AutoScanFQCTest.Utilities;
using Google.Protobuf;
using Mysqlx.Crud;
using Mysqlx.Resultset;
using MySqlX.XDevAPI.Relational;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.Cms;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Printing;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Contexts;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static Google.Protobuf.Reflection.UninterpretedOption.Types;
using Binding = System.Windows.Data.Binding;
using Brush = System.Drawing.Brush;
using Color = System.Drawing.Color;
using Path = System.IO.Path;
using Pen = System.Drawing.Pen;
using Point = System.Drawing.Point;

namespace AutoScanFQCTest.Views
{
    public class TabWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // 获取TabControl的宽度
            double tabControlWidth = (double)value;

            // 这里可以获取TabControl中的TabItem数量，这个例子中假设有5个TabItem
            int tabItemCount = 4;

            // 计算每个TabItem的宽度，这里减去一些边距值，根据需要调整
            double tabItemWidth = (tabControlWidth / tabItemCount) - 1;

            return tabItemWidth > 0 ? tabItemWidth : 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public partial class HomeView : UserControl
    {
        [DllImport("kernel32.dll", CallingConvention = CallingConvention.Winapi)]
        extern static int GetTickCount();

        [DllImport("AICore.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool PostVisionDLL_copy_ROI_content_from_image_to_image(int[] pIntParams, int[] pRetInts);

        public MainWindow m_parent;

        public PanelCanvas m_panel_canvas;                                                                      // PCB画布

        public CameraCanvas[] m_camera_canvases = new CameraCanvas[2];                 // 图像画布

        private Button[] m_product_buttons;
        private int m_nCurrentProductIndex = 0;                                                               // 当前产品索引

        public int m_nCurrentDefectIndex = 0;                                                                // 当前缺陷索引
        public int m_nCurrentDefectedProductIndex = 0;                                                // 当前缺陷产品索引
    
        public int m_nCurrentSideAorB = 0;                                                                    // 1: A面  2: B面

        private int m_nLastRecheckTime = 0;

        private string m_strCurrentBarcode = "";

        private bool m_bHasClearRecheckData = false;

        private bool m_bIsValidatingWithMES = false;                                                   // 是否正在进行MES校验

        private bool m_bDisableRecheckCurrentProduct = false;                                      // 是否禁止复判当前产品

        public int m_bIsAIRechecking = 1;                                                         // AI 是否正在复判，0为复判正常结束，1为正在复判，-1为提交异常

        private bool m_bIsAIRecheckAndSubmissionThreadRunning = false;          // AI复判和提交线程是否正在运行

        //AutoResetEvent m_reset_event = new AutoResetEvent(false);

        public HomeView()
        {
            InitializeComponent();

            // 设置窗口事件
            this.Loaded += Window_Loaded;
            this.SizeChanged += Window_SizeChanged;
        }

        // 窗口加载
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (null == m_parent)
                return;

            // 获取屏幕DPI缩放比例，并根据缩放比例调整控件大小
            if (true)
            {
                double width = grid_ProductInfo.ActualWidth;
                double height = grid_ProductInfo.ActualHeight;

                tabitem_ParameterSetting.Width = (width / 1) * 23 / 100;
                //tabitem_HistoryRecordQuery.Width = (width / 1) * 25 / 100;
                //tabitem_ClearProductInfo.Width = (width / 1) * 25 / 100;
                //tabitem_QueryYield.Width = (width / 1) * 23 / 100;

                width = grid_StatisticsBar.ActualWidth;

                tabitem_ItemsPendingRecheck.Width = (width / 1) * 16 / 100;
                tabitem_DefectBarChart.Width = (width / 1) * 16 / 100;
                YieldRateLineChart.Width = (width / 1) * 16 / 100;
                ShiftOutputChart.Width = (width / 1) * 18 / 100;
                ProductRoadmap.Width = (width / 1) * 16 / 100;
                //ProductInfo.Width = (width / 1) * 16 / 100;

                width = grid_ItemsPendingRecheck.ActualWidth;
                height = grid_ItemsPendingRecheck.ActualHeight;

                gridview_RecheckItemNo.Width = (int)(width * (12f / 100f));
                gridview_RecheckItemBarcode.Width = (int)(width * (71f / 100f));

                gridview_RecheckItemNo.Height = (int)(height * 0.9);
                gridview_RecheckItemBarcode.Height = (int)(height * 0.9);

                width = grid_DefectsWaitingConfirm.ActualWidth;
                height = grid_DefectsWaitingConfirm.ActualHeight;

                gridview_DefectsWaitingConfirm.Width = (int)(width * (68f / 100f));
                gridview_DefectsWaitingConfirm.Height = (int)(height * 0.9);
            }

            // 构造缺陷列表gridview
            if (true)
            {
                gridview_RecheckItemNo.RowHeadersVisible = false;
                gridview_RecheckItemNo.ColumnHeadersVisible = false;
                gridview_RecheckItemNo.EnableHeadersVisualStyles = false;
                gridview_RecheckItemNo.ReadOnly = true;
                gridview_RecheckItemNo.ColumnCount = 1;
                gridview_RecheckItemNo.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
                gridview_RecheckItemNo.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;

                gridview_RecheckItemNo.EnableHeadersVisualStyles = false;
                gridview_RecheckItemNo.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(236, 230, 239);

                gridview_RecheckItemNo.RowsDefaultCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;

                gridview_RecheckItemNo.ColumnHeadersDefaultCellStyle.SelectionBackColor = System.Drawing.Color.FromArgb(236, 230, 239);

                gridview_RecheckItemNo.AllowUserToResizeRows = false;

                double DPI_ratio = Graphics.FromHwnd(new WindowInteropHelper(m_parent).Handle).DpiX / 96;

                gridview_RecheckItemNo.ColumnHeadersDefaultCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;

                gridview_RecheckItemNo.Columns[0].Width = ((int)(gridview_RecheckItemNo.Width * DPI_ratio) * 100) / 100;
                for (int n = 0; n < gridview_RecheckItemNo.ColumnCount; n++)
                    gridview_RecheckItemNo.Columns[n].SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;

                //foreach (System.Windows.Forms.DataGridViewRow row1 in gridview_RecheckItemNo.Rows)
                //{
                //    row1.DefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(247, 245, 249);
                //}

                gridview_RecheckItemNo.DefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(0xE3, 0xED, 0xFF);

                if (false)
                {
                    for (int n = 0; n < 10; n++)
                    {
                        string[] row = new string[] { string.Format("{0}", n + 1) };

                        gridview_RecheckItemNo.Rows.Add(row);
                    }
                }
            }

            // gridview_RecheckItemBarcode
            if (true)
            {
                gridview_RecheckItemBarcode.RowHeadersVisible = false;
                gridview_RecheckItemBarcode.ColumnHeadersVisible = false;
                gridview_RecheckItemBarcode.EnableHeadersVisualStyles = false;
                gridview_RecheckItemBarcode.ReadOnly = true;
                gridview_RecheckItemBarcode.ColumnCount = 1;
                gridview_RecheckItemBarcode.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;

                gridview_RecheckItemBarcode.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.Single;

                gridview_RecheckItemBarcode.EnableHeadersVisualStyles = false;
                gridview_RecheckItemBarcode.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(236, 230, 239);

                gridview_RecheckItemBarcode.RowsDefaultCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;

                gridview_RecheckItemBarcode.ColumnHeadersDefaultCellStyle.SelectionBackColor = System.Drawing.Color.FromArgb(236, 230, 239);

                gridview_RecheckItemBarcode.AllowUserToResizeRows = false;

                double DPI_ratio = Graphics.FromHwnd(new WindowInteropHelper(m_parent).Handle).DpiX / 96;

                gridview_RecheckItemBarcode.ColumnHeadersDefaultCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;

                gridview_RecheckItemBarcode.Columns[0].Width = ((int)(gridview_RecheckItemBarcode.Width * DPI_ratio) * 100) / 100;
                for (int n = 0; n < gridview_RecheckItemBarcode.ColumnCount; n++)
                    gridview_RecheckItemBarcode.Columns[n].SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;

                //foreach (System.Windows.Forms.DataGridViewRow row1 in gridview_RecheckItemBarcode.Rows)
                //{
                //    row1.DefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(247, 245, 249);
                //}

                //gridview_RecheckItemBarcode.DefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(0xE3, 0xED, 0xFF);
                gridview_RecheckItemBarcode.DefaultCellStyle.SelectionForeColor = System.Drawing.Color.Black;

                if (false)
                {
                    for (int n = 0; n < 10; n++)
                    {
                        string[] row = new string[] { string.Format("C66666666666666{0}", n + 1) };

                        gridview_RecheckItemBarcode.Rows.Add(row);

                        if (n < 4)
                            gridview_RecheckItemBarcode.Rows[n].DefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(0x00, 0xC0, 0x00);
                        else
                            gridview_RecheckItemBarcode.Rows[n].DefaultCellStyle.BackColor = System.Drawing.Color.Yellow;
                    }

                    if (gridview_RecheckItemBarcode.Rows.Count > 0)
                    {
                        // 选择第一行
                        gridview_RecheckItemBarcode.Rows[0].Selected = true;

                        // 创建一个新的DataGridViewCellEventArgs实例
                        System.Windows.Forms.DataGridViewCellEventArgs e2 = new System.Windows.Forms.DataGridViewCellEventArgs(0, 2);

                        // 调用CellClick事件处理器
                        gridview_RecheckItemBarcode_CellClick(gridview_RecheckItemBarcode, e2);
                    }
                }
            }

            // gridview_DefectsWaitingConfirm
            if (true)
            {
                gridview_DefectsWaitingConfirm.RowHeadersVisible = false;
                gridview_DefectsWaitingConfirm.ColumnHeadersVisible = false;
                gridview_DefectsWaitingConfirm.EnableHeadersVisualStyles = false;
                gridview_DefectsWaitingConfirm.ReadOnly = true;
                gridview_DefectsWaitingConfirm.ColumnCount = 1;

                gridview_DefectsWaitingConfirm.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;

                gridview_DefectsWaitingConfirm.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.Single;

                gridview_DefectsWaitingConfirm.EnableHeadersVisualStyles = false;
                gridview_DefectsWaitingConfirm.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(236, 230, 239);

                gridview_DefectsWaitingConfirm.RowsDefaultCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;

                gridview_DefectsWaitingConfirm.ColumnHeadersDefaultCellStyle.SelectionBackColor = System.Drawing.Color.FromArgb(236, 230, 239);

                gridview_DefectsWaitingConfirm.AllowUserToResizeRows = false;

                double DPI_ratio = Graphics.FromHwnd(new WindowInteropHelper(m_parent).Handle).DpiX / 96;

                gridview_DefectsWaitingConfirm.ColumnHeadersDefaultCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;

                gridview_DefectsWaitingConfirm.Columns[0].Width = ((int)(gridview_DefectsWaitingConfirm.Width * DPI_ratio) * 100) / 100;
                for (int n = 0; n < gridview_DefectsWaitingConfirm.ColumnCount; n++)
                {
                    gridview_DefectsWaitingConfirm.Columns[n].SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;

                    System.Windows.Forms.DataGridViewCellStyle cellStyle = new System.Windows.Forms.DataGridViewCellStyle();
                    cellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;


                    // 将样式应用到DataGrid
                    gridview_DefectsWaitingConfirm.Columns[n].DefaultCellStyle = cellStyle;
                }

                //foreach (System.Windows.Forms.DataGridViewRow row1 in gridview_RecheckItemBarcode.Rows)
                //{
                //    row1.DefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(247, 245, 249);
                //}

                //gridview_RecheckItemBarcode.DefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(0xE3, 0xED, 0xFF);
                gridview_DefectsWaitingConfirm.DefaultCellStyle.SelectionForeColor = System.Drawing.Color.Black;
            }

            //GenerateCircularButtonsInGridContainer(grid_CircularButtonContainer, 3, 5);

            // 加载良率统计
            m_parent.m_global.m_ChangedCount += changedcount;
            changedcount();

            combo_RecheckDisplayMode.SelectedIndex = m_parent.m_global.m_nRecheckDisplayMode;

            textblock_OperatorID.Text = m_parent.m_global.m_strCurrentOperatorID;
            m_parent.m_global.UserChanged += userchanged;

            m_parent.m_global.m_WorkShiftChanged += ClearCount;

            // 按机台查询
            for (int n = 0; n < m_parent.m_global.m_dict_machine_names_and_IPs.Keys.Count; n++)
            {
                combo_SelectMachine.Items.Add(m_parent.m_global.m_dict_machine_names_and_IPs.Keys.ElementAt(n));
            }
            combo_SelectMachine.SelectedIndex = combo_SelectMachine.Items.Count - 1;

            CheckBox_QueryByMachine.IsChecked = m_parent.m_global.m_bSelectByMachine;
            if (m_parent.m_global.m_bSelectByMachine)
            {
                if (m_parent.m_global.m_nSelectedMachineIndexForDatabaseQuery < combo_SelectMachine.Items.Count)
                    combo_SelectMachine.SelectedIndex = m_parent.m_global.m_nSelectedMachineIndexForDatabaseQuery;
            }

            combo_SelectMachine.SelectedIndex = 0;

            generateContextMenuForBtnConfirmNG();
        }

        // 功能：更新统计数据
        public void UpdateStatistics(bool bConsiderCurrentTray = true)
        {
            if (null == m_parent)
                return;

            if (true)
            {
                if (m_parent.m_global.m_strProductType == "nova")
                {
                    if (m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Nova == null)
                        return;

                    List<Product> products = m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Nova.Products;

                    m_parent.m_global.m_recheck_statistics.m_nTotalRecheckedProducts += products.Count;

                    for (int i = 0; i < products.Count; i++)
                    {
                        m_parent.m_global.m_recheck_statistics.m_nTotalDefects += products[i].Defects.Count;

                        int nColumns = m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Nova.Col;
                        int nRows = m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Nova.Row;

                        int col = products[i].PosCol;
                        int row = products[i].PosRow;

                        int nIndex = row * nColumns + col;
                        if (nIndex >= m_parent.m_global.m_current_MES_tray_info.m_check_flags_by_product.Length)
                            continue;

                        if (m_parent.m_global.m_current_MES_tray_info.m_check_flags_by_product[nIndex] == RecheckResult.OK)
                        {
                            m_parent.m_global.m_recheck_statistics.m_nRecheckedOKs++;
                        }
                        else if (m_parent.m_global.m_current_MES_tray_info.m_check_flags_by_product[nIndex] == RecheckResult.NG)
                        {
                            m_parent.m_global.m_recheck_statistics.m_nRecheckedNGs++;
                        }
                    }
                }
                //else if (m_parent.m_global.m_strProductType == "dock")
                //{
                //    if (m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Dock != null && bConsiderCurrentTray == true)
                //    {
                //        List<ProductInfo> products = m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Dock.products;

                //        m_parent.m_global.m_recheck_statistics.m_nTotalOKProducts += m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Dock.OK_products.Count;
                //        m_parent.m_global.m_recheck_statistics.m_nTotalNGProducts += products.Count;
                //        m_parent.m_global.m_recheck_statistics.m_nTotalNGProducts += m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Dock.products_with_unrecheckable_defect.Count;

                //        for (int i = 0; i < products.Count; i++)
                //        {
                //            m_parent.m_global.m_recheck_statistics.m_nTotalDefects += products[i].defects.Count;

                //            int nColumns = m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Dock.total_columns;
                //            int nRows = m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Dock.total_rows;

                //            int col = products[i].column - 1;
                //            int row = products[i].row - 1;

                //            int nIndex = row * nColumns + col;
                //            if (nIndex >= m_parent.m_global.m_current_MES_tray_info.m_check_flags_by_product.Length)
                //                continue;

                //            if (m_parent.m_global.m_current_MES_tray_info.m_check_flags_by_product[nIndex] == RecheckResult.OK)
                //            {
                //                m_parent.m_global.m_recheck_statistics.m_nRecheckedOKs++;
                //            }
                //            else if (m_parent.m_global.m_current_MES_tray_info.m_check_flags_by_product[nIndex] == RecheckResult.NG)
                //            {
                //                m_parent.m_global.m_recheck_statistics.m_nRecheckedNGs++;
                //            }
                //        }

                //        products = m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Dock.products_with_unrecheckable_defect;
                //        for (int i=0;i< products.Count;i++)
                //        {
                //            m_parent.m_global.m_recheck_statistics.m_nTotalDefects += products[i].defects.Count;
                //            m_parent.m_global.m_recheck_statistics.m_nRecheckedNGs++;
                //        }
                //    }

                //    m_parent.m_global.m_recheck_statistics.m_nTotalRecheckedOKProducts = (m_parent.m_global.m_recheck_statistics.m_nTotalOKProducts
                //        + m_parent.m_global.m_recheck_statistics.m_nRecheckedOKs);
                //    m_parent.m_global.m_recheck_statistics.m_nTotalRecheckedNGProducts = m_parent.m_global.m_recheck_statistics.m_nRecheckedNGs;

                //    m_parent.m_global.m_recheck_statistics.m_nTotalRecheckedProducts = m_parent.m_global.m_recheck_statistics.m_nTotalRecheckedOKProducts
                //        + m_parent.m_global.m_recheck_statistics.m_nTotalRecheckedNGProducts;
                //}
            }

            // 更新统计数据
            textblock_TotalPanels.Text = m_parent.m_global.m_recheck_statistics.m_nTotalPanels.ToString();                                   //总盘数
            txtblock_ProductCount.Text = m_parent.m_global.m_recheck_statistics.m_nTotalRecheckedProducts.ToString();            //总产品数量
            textblock_TotalDefects.Text = m_parent.m_global.m_recheck_statistics.m_nTotalDefects.ToString();                               //总缺陷点数

            txt_FirstOks.Text = m_parent.m_global.m_recheck_statistics.m_nTotalOKProducts.ToString();                                           //一次ok数
            txt_FirstNGs.Text = m_parent.m_global.m_recheck_statistics.m_nTotalNGProducts.ToString();                                           //一次ng数

            textblock_TotalConfirmedOKs.Text = m_parent.m_global.m_recheck_statistics.m_nTotalRecheckedOKProducts.ToString();                       //二次ok数
            textblock_TotalConfirmedNGs.Text = m_parent.m_global.m_recheck_statistics.m_nTotalRecheckedNGProducts.ToString();                       //二次ng数

            //textblock_RecheckedOKs.Text = m_parent.m_global.m_recheck_statistics.m_nRecheckedOKs.ToString();                                       //复判ok点数
            //textblock_RecheckedNGs.Text = m_parent.m_global.m_recheck_statistics.m_nRecheckedNGs.ToString();                                       //复判ng点数

            if (m_parent.m_global.m_recheck_statistics.m_nTotalRecheckedProducts == 0)
            {
                textblock_FirstOKRate.Text = "0%";
                textblock_SecondOKRate.Text = "0%";
            }
            else
            {
                double dFirstOKRate = ((double)m_parent.m_global.m_recheck_statistics.m_nTotalOKProducts / (double)m_parent.m_global.m_recheck_statistics.m_nTotalRecheckedProducts) * 100;
                double dSecondOKRate = ((double)m_parent.m_global.m_recheck_statistics.m_nTotalRecheckedOKProducts / (double)m_parent.m_global.m_recheck_statistics.m_nTotalRecheckedProducts) * 100;

                textblock_FirstOKRate.Text = dFirstOKRate.ToString("F2") + "%";
                textblock_SecondOKRate.Text = dSecondOKRate.ToString("F2") + "%";
            }

            // 保存统计数据
            m_parent.m_global.m_recheck_statistics.Save();

            if (m_parent.m_global.m_strProductType == "nova")
            {
                if (m_parent.m_global.m_strProductSubType == "yuehu")
                {
                    BtnRequestData.Content = "待清洁";
                }
            }
        }

        private void userchanged(string username)
        {
            m_parent.m_global.m_strCurrentOperatorID = username;
            textblock_OperatorID.Text = username;
        }

        private void changedcount()
        {
            return;
            Dispatcher.Invoke(() =>
            {
                var recheck = m_parent.m_global.m_recheck_statistics;
                int all = (recheck.m_ngproduct + recheck.m_okproduct);
                //textblock_TotalPanels.Text = (recheck.m_okpanel + recheck.m_ngpanel).ToString();//总盘数
                //txtblock_productCount.Text = all.ToString();//总产品数量
                textblock_TotalDefects.Text = recheck.m_alldefects.ToString();//总缺陷点数
                txt_FirstOks.Text = recheck.m_okproduct.ToString();//一次ok数
                txt_FirstNGs.Text = recheck.m_ngproduct.ToString();//一次ng数
                double yicilianglv = (((double)recheck.m_okproduct / (double)(recheck.m_ngproduct + recheck.m_okproduct)) * 100);
                textblock_FirstOKRate.Text = yicilianglv.ToString("F2") + "%";//一次良率
                textblock_RecheckedNGs.Text = m_parent.m_global.m_recheck_statistics.m_nRecheckedNGs.ToString();//复判ng点数
                textblock_RecheckedOKs.Text = m_parent.m_global.m_recheck_statistics.m_nRecheckedOKs.ToString();//复判ok点数
                textblock_TotalConfirmedNGs.Text = m_parent.m_global.m_recheck_statistics.m_confirmNGs.ToString();//二次ng数
                textblock_TotalConfirmedOKs.Text = m_parent.m_global.m_recheck_statistics.m_confirmOKs.ToString();//二次ok数                                                    

                double wupanlv = 100 - yicilianglv;//误报率
                double erci = ((double)m_parent.m_global.m_recheck_statistics.m_confirmNGs / (double)all) * 100;//二次ng率
                double ratio = wupanlv - erci;//误判率
                //double ratio = (double)m_parent.m_global.m_recheck_statistics.m_confirmOKs / (double)(m_parent.m_global.m_recheck_statistics.m_confirmNGs + m_parent.m_global.m_recheck_statistics.m_nRecheckedNGs);
                textblock_WrongCheckRatio.Text = string.Format("{0:F2}%", ratio);
                //二次良品率
                textblock_SecondOKRate.Text = ((((double)all - (double)m_parent.m_global.m_recheck_statistics.m_confirmNGs) / (double)all) * 100).ToString("F2") + "%";
            });
            m_parent.m_global.m_recheck_statistics.Save();
        }

        // 窗口大小改变
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (null == m_parent)
                return;

            //textblock_CurrentProductType.Text = m_parent.m_global.m_strProductType;
            textblock_CurrentProductType.Text = m_parent.m_global.m_strProductNameShownOnUI;

            combo_RotationAngle.SelectedIndex = m_parent.m_global.m_nImageRotationAngle;

            combo_SpecialTrayCornerMode.SelectedIndex = m_parent.m_global.m_nSpecialTrayCornerMode;

            // 动态调整导航图像画布大小，以适应不同分辨率的显示器
            if (true)
            {
                int nWidth = (int)(grid_NavigationImageCanvas1.ActualWidth);
                if (nWidth % 4 != 0)
                    grid_NavigationImageCanvas1.Width = (int)(nWidth / 4) * 4;
                else
                    grid_NavigationImageCanvas1.Width = nWidth;

                grid_NavigationImageCanvas1.UpdateLayout();

                if (null == m_camera_canvases[0])
                {
                    m_camera_canvases[0] = new CameraCanvas(m_parent, ctrl_NavigationImageCanvas,
                        (int)grid_NavigationImageCanvas1.Width, (int)grid_NavigationImageCanvas1.ActualHeight, scene_type.scene_camera1, false);

                    m_camera_canvases[0].m_bForceRedraw = true;
                    m_camera_canvases[0].OnWindowSizeChanged(sender, e);
                }
            }

            // 动态调整细节图像画布大小，以适应不同分辨率的显示器
            if (true)
            {
                int nWidth = (int)(grid_ROIImageCanvas.ActualWidth);
                if (nWidth % 4 != 0)
                    grid_ROIImageCanvas.Width = (int)(nWidth / 4) * 4;
                else
                    grid_ROIImageCanvas.Width = nWidth;

                grid_ROIImageCanvas.UpdateLayout();

                if (null == m_camera_canvases[1])
                {
                    m_camera_canvases[1] = new CameraCanvas(m_parent, ctrl_ROIImageCanvas1,
                                               (int)grid_ROIImageCanvas.Width, (int)grid_ROIImageCanvas.ActualHeight, scene_type.scene_camera2, false);

                    m_camera_canvases[1].m_bForceRedraw = true;
                    m_camera_canvases[1].OnWindowSizeChanged(sender, e);
                }
            }

            // 动态调整PCB画布大小，以适应不同分辨率的显示器
            if (true)
            {
                int nWidth = (int)(grid_PanelCanvas.ActualWidth);
                if (nWidth % 4 != 0)
                    grid_PanelCanvas.Width = (int)(nWidth / 4) * 4;
                else
                    grid_PanelCanvas.Width = nWidth;

                grid_PanelCanvas.UpdateLayout();

                if (null == m_panel_canvas)
                {
                    m_panel_canvas = new PanelCanvas(m_parent, ctrl_PanelCanvas,
                                               (int)grid_PanelCanvas.Width, (int)grid_PanelCanvas.ActualHeight, scene_type.scene_panel, false);

                    m_panel_canvas.m_bForceRedraw = true;
                    m_panel_canvas.OnWindowSizeChanged(sender, e);
                }
            }
        }

        // gridview_RecheckItemNo选择改变事件
        private void gridview_RecheckItemNo_SelectionChanged(object sender, EventArgs e)
        {
            gridview_RecheckItemNo.ClearSelection();
        }

        // 旋转角度选择改变事件
        private void combo_RotationAngle_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (null != m_parent)
            {
                if (m_parent.m_global.m_nImageRotationAngle != combo_RotationAngle.SelectedIndex)
                {
                    // 弹出确认对话框
                    //if (MessageBox.Show("确定要改变图像旋转角度吗？如选择确定，程序将会重启，重启后生效", "再次确认", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        m_parent.m_global.m_nImageRotationAngle = combo_RotationAngle.SelectedIndex;

                        // 判断m_parent.m_global.m_strLastOpenImagePath是否存在
                        if (File.Exists(m_parent.m_global.m_strLastOpenImagePath))
                        {
                            BaseCanvas<CameraCanvas>.OpenImageFileAndShowOnBIVCanvas(m_parent.m_global.m_strLastOpenImagePath,
                                m_parent.m_global.m_nImageRotationAngle, m_parent.page_HomeView.m_camera_canvases[0]);

                            m_parent.page_HomeView.m_camera_canvases[0].show_whole_image();
                        }

                        //m_parent.Close();
                        //Environment.Exit(0);
                    }
                    //else
                    //{
                    //    combo_RotationAngle.SelectedIndex = m_parent.m_global.m_nImageRotationAngle;
                    //}
                }
            }
        }

        // 滚动条滚动事件：待确认产品二维码
        private void gridview_RecheckItemBarcode_Scroll(object sender, System.Windows.Forms.ScrollEventArgs e)
        {
            if (e.ScrollOrientation == System.Windows.Forms.ScrollOrientation.VerticalScroll)
            {
                // 将A的滚动位置设定为B的滚动位置
                gridview_RecheckItemNo.FirstDisplayedScrollingRowIndex = gridview_RecheckItemBarcode.FirstDisplayedScrollingRowIndex;
            }
        }

        // 滚动条单元格绘制事件：待确认产品二维码
        private void gridview_RecheckItemBarcode_CellPainting(object sender, System.Windows.Forms.DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                // 检查当前行是否是选中的行
                if (gridview_RecheckItemBarcode.Rows[e.RowIndex].Selected)
                {
                    // 使用自定义的边框颜色和宽度
                    Color borderColor = Color.Blue; // 边框颜色
                    int borderWidth = 2; // 边框宽度

                    using (Brush borderBrush = new SolidBrush(borderColor))
                    {
                        using (Pen borderPen = new Pen(borderBrush, borderWidth))
                        {
                            // 绘制原有的单元格内容
                            System.Windows.Forms.DataGridViewPaintParts parts = ~System.Windows.Forms.DataGridViewPaintParts.SelectionBackground
                                & System.Windows.Forms.DataGridViewPaintParts.All;

                            e.Paint(e.CellBounds, parts);

                            // 绘制自定义的边框
                            e.Graphics.DrawRectangle(borderPen, e.CellBounds.Left + 1, e.CellBounds.Top + 1, e.CellBounds.Width - 2, e.CellBounds.Height - 2);

                            // 告诉DataGridView我们已经自己处理了绘制
                            e.Handled = true;
                        }
                    }
                }
                else
                {
                    // 绘制单元格边框
                    // 设置你想要的边框颜色
                    Color borderColor = System.Drawing.Color.FromArgb(0x22, 0x22, 0x22);

                    // 重绘背景，确保背景颜色不会丢失
                    e.PaintBackground(e.CellBounds, true);

                    // 重绘单元格的内容
                    e.PaintContent(e.CellBounds);

                    // 绘制自定义边框
                    using (Pen gridLinePen = new Pen(borderColor, 0.6f))
                    {
                        e.Graphics.DrawRectangle(gridLinePen, e.CellBounds.Left, e.CellBounds.Top,
                            e.CellBounds.Width - 1, e.CellBounds.Height - 0);
                    }

                    // 告诉 DataGridView 我们已经自己处理了绘制
                    e.Handled = true;
                }
            }
        }

        // 单元格点击事件：待确认产品二维码
        private void gridview_RecheckItemBarcode_CellClick(object sender, System.Windows.Forms.DataGridViewCellEventArgs e)
        {
            int rowIndex = e.RowIndex;

            // 检查是否是有效行（防止点击列标题行）
            // 遇到一次rowIndex为7259越界
            if (m_parent.m_global.m_current_MES_tray_info.m_array_indices_of_defected_products != null &&
                rowIndex >= 0 && rowIndex < m_parent.m_global.m_current_MES_tray_info.m_array_indices_of_defected_products.Length)
            {
                SelectProduct(m_parent.m_global.m_current_MES_tray_info.m_array_indices_of_defected_products[rowIndex]);

                SelectDefectedProductBarcode(rowIndex);
            }
        }

        // 滚动条单元格绘制事件：待确认NG集合
        private void gridview_DefectsWaitingConfirm_CellPainting(object sender, System.Windows.Forms.DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                // 检查当前行是否是选中的行
                if (gridview_DefectsWaitingConfirm.Rows[e.RowIndex].Selected)
                {
                    // 使用自定义的边框颜色和宽度
                    Color borderColor = Color.Blue; // 边框颜色
                    int borderWidth = 2; // 边框宽度

                    using (Brush borderBrush = new SolidBrush(borderColor))
                    {
                        using (Pen borderPen = new Pen(borderBrush, borderWidth))
                        {
                            // 绘制原有的单元格内容
                            System.Windows.Forms.DataGridViewPaintParts parts = ~System.Windows.Forms.DataGridViewPaintParts.SelectionBackground
                                & System.Windows.Forms.DataGridViewPaintParts.All;

                            e.Paint(e.CellBounds, parts);

                            // 绘制自定义的边框
                            e.Graphics.DrawRectangle(borderPen, e.CellBounds.Left + 1, e.CellBounds.Top + 1, e.CellBounds.Width - 2, e.CellBounds.Height - 2);

                            // 告诉DataGridView我们已经自己处理了绘制
                            e.Handled = true;
                        }
                    }
                }
                else
                {
                    // 绘制单元格边框
                    // 设置你想要的边框颜色
                    Color borderColor = System.Drawing.Color.FromArgb(0x22, 0x22, 0x22);

                    // 重绘背景，确保背景颜色不会丢失
                    e.PaintBackground(e.CellBounds, true);

                    // 重绘单元格的内容
                    e.PaintContent(e.CellBounds);

                    // 绘制自定义边框
                    using (Pen gridLinePen = new Pen(borderColor, 0.6f))
                    {
                        e.Graphics.DrawRectangle(gridLinePen, e.CellBounds.Left, e.CellBounds.Top,
                            e.CellBounds.Width - 1, e.CellBounds.Height - 0);
                    }

                    // 告诉 DataGridView 我们已经自己处理了绘制
                    e.Handled = true;
                }
            }
        }

        // 获取下一个有效的缺陷索引（检胶项目专用）
        private bool get_next_valid_defect_index_for_GLUE_CHECK(int nStartIndex, ref int nNextValidIndex)
        {
            // 检查当前托盘中产品的缺陷列表是否为空，如果为空则返回 false
            if (m_parent.m_global.m_current_tray_info_for_Dock.products[m_nCurrentDefectedProductIndex].defects.Count == 0)
                return false;

            if (nStartIndex < 0)
                nStartIndex = 0;
            if (nStartIndex >= m_parent.m_global.m_current_tray_info_for_Dock.products[m_nCurrentDefectedProductIndex].defects.Count)
                nStartIndex = m_parent.m_global.m_current_tray_info_for_Dock.products[m_nCurrentDefectedProductIndex].defects.Count - 1;

            // 从指定的起始索引开始遍历缺陷列表
            for (int n = nStartIndex; n < m_parent.m_global.m_current_tray_info_for_Dock.products[m_nCurrentDefectedProductIndex].defects.Count; n++)
            {
                // 检查缺陷类型是否为空，如果为空则跳过当前缺陷
                if (string.IsNullOrEmpty(m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Nova.Products[m_nCurrentDefectedProductIndex].Defects[n].Type))
                    continue;

                // 如果找到有效缺陷，则更新下一个有效索引并返回 true
                nNextValidIndex = n;
                return true;
            }

            // 如果没有找到有效缺陷，则返回 false
            return false;
        }

        // 获取上一个有效的缺陷索引（检胶项目专用）
        private bool get_previous_valid_defect_index_for_GLUE_CHECK(int nStartIndex, ref int nPreviousValidIndex)
        {
            // 检查当前托盘中产品的缺陷列表是否为空，如果为空则返回 false
            if (m_parent.m_global.m_current_tray_info_for_Dock.products[m_nCurrentDefectedProductIndex].defects.Count == 0)
                return false;

            if (nStartIndex < 0)
                nStartIndex = 0;
            if (nStartIndex >= m_parent.m_global.m_current_tray_info_for_Dock.products[m_nCurrentDefectedProductIndex].defects.Count)
                nStartIndex = m_parent.m_global.m_current_tray_info_for_Dock.products[m_nCurrentDefectedProductIndex].defects.Count - 1;

            // 从指定的起始索引开始遍历缺陷列表
            for (int n = nStartIndex; n >= 0; n--)
            {
                // 检查缺陷类型是否为空，如果为空则跳过当前缺陷
                if (string.IsNullOrEmpty(m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Nova.Products[m_nCurrentDefectedProductIndex].Defects[n].Type))
                    continue;

                // 如果找到有效缺陷，则更新上一个有效索引并返回 true
                nPreviousValidIndex = n;
                return true;
            }

            // 如果没有找到有效缺陷，则返回 false
            return false;
        }

        // 单元格点击事件：待确认NG集合
        private void gridview_DefectsWaitingConfirm_CellClick(object sender, System.Windows.Forms.DataGridViewCellEventArgs e)
        {
            int rowIndex = e.RowIndex;

            // 检查是否是有效行（防止点击列标题行）
            if (rowIndex >= 0)
            {
                m_nCurrentDefectIndex = rowIndex;

                if (false)
                {
                    if (m_parent.m_global.m_strProductType == "dock" && m_parent.m_global.m_strProductSubType == "glue_check")
                    {
                        // 初始化一个计数器用于跟踪缺陷索引
                        int counter = 0;

                        // 遍历当前托盘中产品的所有缺陷
                        for (int n = 0; n < m_parent.m_global.m_current_tray_info_for_Dock.products[m_nCurrentDefectedProductIndex].defects.Count; n++)
                        {
                            // 检查缺陷类型是否为空，如果为空则跳过当前缺陷
                            if (string.IsNullOrEmpty(m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Nova.Products[m_nCurrentDefectedProductIndex].Defects[n].Type))
                                continue;

                            // 如果计数器与当前缺陷索引相同，则更新当前缺陷索引并跳出循环
                            if (counter == m_nCurrentDefectIndex)
                            {
                                m_nCurrentDefectIndex = n;
                                break;
                            }

                            // 增加计数器
                            counter++;
                        }
                    }
                }

                SelectProductDefect(m_nCurrentDefectIndex);
            }
        }

        // 鼠标按下事件：画布
        private void ctrl_ROIImageCanvas1_MouseDown(object sender, MouseButtonEventArgs e)
        {
            m_camera_canvases[1].OnMouseDown(sender, e);
        }

        // 鼠标移动事件：画布
        private void ctrl_ROIImageCanvas1_MouseMove(object sender, MouseEventArgs e)
        {
            m_camera_canvases[1].OnMouseMove(sender, e);
        }

        // 鼠标抬起事件：画布
        private void ctrl_ROIImageCanvas1_MouseUp(object sender, MouseButtonEventArgs e)
        {
            m_camera_canvases[1].OnMouseUp(sender, e);
        }

        // 鼠标滚轮事件：画布
        private void ctrl_ROIImageCanvas1_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            m_camera_canvases[1].OnMouseWheel(sender, e);
        }

        // 鼠标双击事件：画布
        private void ctrl_NavigationImageCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            m_camera_canvases[0].OnMouseDown(sender, e);
        }

        // 鼠标移动事件：画布
        private void ctrl_NavigationImageCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            m_camera_canvases[0].OnMouseMove(sender, e);
        }

        // 鼠标抬起事件：画布
        private void ctrl_NavigationImageCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            m_camera_canvases[0].OnMouseUp(sender, e);
        }

        // 鼠标滚轮事件：画布
        private void ctrl_NavigationImageCanvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            m_camera_canvases[0].OnMouseWheel(sender, e);
        }

        // 按钮点击事件：清空“总产品数”
        private void BtnClearProductCount_Click(object sender, RoutedEventArgs e)
        {
            // 弹出确认对话框
            //if (MessageBox.Show("确定要清空总产品数吗？清空后不可撤回，请谨慎决定！", "再次确认", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
            //    return;

            m_parent.m_global.m_recheck_statistics.Clear();

            m_parent.m_global.m_recheck_statistics.Save();

            UpdateStatistics(false);

            m_parent.m_global.m_log_presenter.Log("已清空所有统计信息！");
        }

        // 按钮点击事件：清除工号
        private void BtnClearOperatorID_Click(object sender, RoutedEventArgs e)
        {
            m_parent.m_global.m_strCurrentOperatorID = "";

            textblock_OperatorID.Text = m_parent.m_global.m_strCurrentOperatorID;

            m_parent.statusbar_task_info.Text = "操作员信息: ";
        }

        // 按钮点击事件：一个盘中的产品号被点击
        private void BtnProductIndexClick(object sender, RoutedEventArgs e)
        {
            //if (m_parent.m_global.m_bIsUIOperationInProcess == true)
            //{
            //    // 提示正在进行其他操作，请稍后再试
            //    MessageBox.Show("正在进行其他UI操作，请稍后再试", "提示", MessageBoxButton.OK, MessageBoxImage.Information);

            //    return;
            //}

            m_bDisableRecheckCurrentProduct = false;

            Button button = sender as Button;

            if (button != null)
            {
                int index = (int)button.Tag;

                if (m_parent.m_global.m_strProductType == "nova")
                {
                    if (index >= m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Nova.TotalPcs)
                    {
                        m_parent.m_global.m_log_presenter.Log("产品索引超出范围");
                        return;
                    }
                }
                if (m_parent.m_global.m_strProductType == "dock")
                {
                    int nTotalPcs = m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Dock.total_columns * m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Dock.total_rows;

                    if (index >= nTotalPcs)
                    {
                        m_parent.m_global.m_log_presenter.Log("产品索引超出范围");
                        return;
                    }
                }

                // 检查index是否在m_parent.m_global.m_current_MES_tray_info.m_array_indices_of_defected_products里面，如果不在，不允许点击
                bool bFound = false;
                for (int i = 0; i < m_parent.m_global.m_current_MES_tray_info.m_array_indices_of_defected_products.Length; i++)
                {
                    if (m_parent.m_global.m_current_MES_tray_info.m_array_indices_of_defected_products[i] == index)
                    {
                        // 判断是否无码
                        if (m_parent.m_global.m_strProductType == "dock")
                        {
                            if (m_parent.m_global.m_current_tray_info_for_Dock.products[i].barcode.IndexOf("nocode", StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                m_parent.m_global.m_log_presenter.Log(string.Format("第{0}个产品无码或空穴，不需要复判", index + 1));
                                MessageBox.Show(string.Format("第{0}个产品无码或空穴，不需要复判", index + 1), "提示", MessageBoxButton.OK, MessageBoxImage.Information);

                                return;
                            }
                        }

                        bFound = true;

                        if (m_parent.m_global.m_strProductType == "nova")
                        {
                            if (m_parent.m_global.m_strProductSubType == "yuehu")
                            {
                                Product product = m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Nova.Products[i];

                                int nColumns = m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Nova.Col;
                                int nRows = m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Nova.Row;

                                int col = product.PosCol - 1;
                                int row = product.PosRow - 1;

                                int nIndex = row * nColumns + col;

                                RecheckResult result = m_parent.m_global.m_current_MES_tray_info.m_check_flags_by_product[nIndex];

                                if (result == RecheckResult.NEED_CLEAN)
                                {
                                    bFound = false;
                                }
                            }
                        }

                        break;
                    }
                }
                if (false == bFound && m_parent.m_global.m_strProductType == "dock")
                {
                    TrayInfo tray_info = m_parent.m_global.m_current_tray_info_for_Dock;

                    RecheckResult result = RecheckResult.NotChecked;
                    string strProductBarcode = "";
                    bool bFound2 = false;

                    int nColumns = m_parent.m_global.m_current_tray_info_for_Dock.total_columns;
                    int nRows = m_parent.m_global.m_current_tray_info_for_Dock.total_rows;

                    try
                    {
                        for (int i = 0; i < m_parent.m_global.m_current_tray_info_for_Dock.ET_products.Count; i++)
                        {
                            ProductInfo product = m_parent.m_global.m_current_tray_info_for_Dock.ET_products[i];

                            int col = product.column - 1;
                            int row = product.row - 1;

                            int nIndex = row * nColumns + col;

                            if (nIndex == index)
                            {
                                result = m_parent.m_global.m_current_MES_tray_info.m_check_flags_by_product[nIndex];

                                strProductBarcode = product.barcode;
                                bFound2 = true;

                                break;
                            }
                        }

                        if (false == bFound2)
                        {
                            for (int i = 0; i < m_parent.m_global.m_current_tray_info_for_Dock.OK_products.Count; i++)
                            {
                                ProductInfo product = m_parent.m_global.m_current_tray_info_for_Dock.OK_products[i];

                                int col = product.column - 1;
                                int row = product.row - 1;

                                int nIndex = row * nColumns + col;

                                if (nIndex == index)
                                {
                                    result = m_parent.m_global.m_current_MES_tray_info.m_check_flags_by_product[nIndex];

                                    strProductBarcode = product.barcode;
                                    bFound2 = true;

                                    break;
                                }
                            }
                        }

                        if (false == bFound2)
                        {
                            for (int i = 0; i < m_parent.m_global.m_current_tray_info_for_Dock.products_with_unrecheckable_defect.Count; i++)
                            {
                                ProductInfo product = m_parent.m_global.m_current_tray_info_for_Dock.products_with_unrecheckable_defect[i];

                                int col = product.column - 1;
                                int row = product.row - 1;

                                int nIndex = row * nColumns + col;

                                if (nIndex == index)
                                {
                                    result = m_parent.m_global.m_current_MES_tray_info.m_check_flags_by_product[nIndex];

                                    strProductBarcode = product.barcode;
                                    bFound2 = true;

                                    break;
                                }
                            }
                        }

                        if (false == bFound2)
                        {
                            for (int i = 0; i < m_parent.m_global.m_current_tray_info_for_Dock.products_with_failed_positioning.Count; i++)
                            {
                                ProductInfo product = m_parent.m_global.m_current_tray_info_for_Dock.products_with_failed_positioning[i];

                                int col = product.column - 1;
                                int row = product.row - 1;

                                int nIndex = row * nColumns + col;

                                if (nIndex == index)
                                {
                                    result = m_parent.m_global.m_current_MES_tray_info.m_check_flags_by_product[nIndex];

                                    strProductBarcode = product.barcode;
                                    bFound2 = true;

                                    break;
                                }
                            }
                        }

                        if (false == bFound2)
                        {
                            for (int i = 0; i < m_parent.m_global.m_current_tray_info_for_Dock.MES_failed_products.Count; i++)
                            {
                                ProductInfo product = m_parent.m_global.m_current_tray_info_for_Dock.MES_failed_products[i];

                                int col = product.column - 1;
                                int row = product.row - 1;

                                int nIndex = row * nColumns + col;

                                if (nIndex == index)
                                {
                                    result = m_parent.m_global.m_current_MES_tray_info.m_check_flags_by_product[nIndex];

                                    strProductBarcode = product.barcode;
                                    bFound2 = true;

                                    break;
                                }
                            }
                        }

                        if (false == bFound2)
                            result = m_parent.m_global.m_current_MES_tray_info.m_check_flags_by_product[index];
                    }
                    catch (Exception ex)
                    {
                        // 提示错误信息
                        m_parent.m_global.m_log_presenter.Log(string.Format("获取产品信息失败，错误信息：{0}", ex.Message));

                        MessageBox.Show(string.Format("获取产品信息失败，错误信息：{0}", ex.Message), "错误", MessageBoxButton.OK, MessageBoxImage.Error);

                        return;
                    }

                    if (result == RecheckResult.EMPTY_DEFECT)
                    {
                        m_parent.m_global.m_log_presenter.Log(string.Format("第{0}个产品缺陷信息为空，二维码 {1}，不需要复判", index + 1, strProductBarcode));

                        MessageBox.Show(string.Format("第{0}个产品缺陷信息为空，二维码 {1}，不需要复判", index + 1, strProductBarcode), "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else if (result == RecheckResult.ET)
                    {
                        m_parent.m_global.m_log_presenter.Log(string.Format("第{0}个产品为ET产品，二维码 {1}，不需要复判", index + 1, strProductBarcode));

                        MessageBox.Show(string.Format("第{0}个产品为ET产品，二维码 {1}，不需要复判", index + 1, strProductBarcode), "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else if (result == RecheckResult.OK)
                    {
                        m_parent.m_global.m_log_presenter.Log(string.Format("第{0}个产品为OK产品，二维码 {1}，不需要复判", index + 1, strProductBarcode));

                        MessageBox.Show(string.Format("第{0}个产品为OK产品，二维码 {1}，不需要复判", index + 1, strProductBarcode), "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else if (result == RecheckResult.NEED_CLEAN)
                    {
                        m_parent.m_global.m_log_presenter.Log(string.Format("第{0}个产品为待清洁产品，二维码 {1}，不需要复判", index + 1, strProductBarcode));

                        MessageBox.Show(string.Format("第{0}个产品为待清洁产品，二维码 {1}，不需要复判", index + 1, strProductBarcode), "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else if (result == RecheckResult.Unrecheckable)
                    {
                        m_parent.m_global.m_log_presenter.Log(string.Format("第{0}个产品为不可复判项的产品，二维码 {1}，不需要复判", index + 1, strProductBarcode));

                        MessageBox.Show(string.Format("第{0}个产品为不可复判项的产品，二维码 {1}，不需要复判", index + 1, strProductBarcode), "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else if (result == RecheckResult.NotReceived)
                    {
                        m_parent.m_global.m_log_presenter.Log(string.Format("第{0}个产品为视觉未发数据的产品，无法复判，请重新检测", index + 1));

                        MessageBox.Show(string.Format("第{0}个产品为视觉未发数据的产品，无法复判，请重新检测", index + 1), "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else if (result == RecheckResult.FailedPositioning)
                    {
                        m_parent.m_global.m_log_presenter.Log(string.Format("第{0}个产品为定位失败的产品，请将产品拿出", index + 1));

                        MessageBox.Show(string.Format("第{0}个产品为定位失败的产品，请将产品拿出", index + 1), "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else if (result == RecheckResult.EmptySlot)
                    {
                        m_parent.m_global.m_log_presenter.Log(string.Format("第{0}个产品空穴，不需要复判", index + 1));

                        MessageBox.Show(string.Format("第{0}个产品空穴，不需要复判", index + 1), "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else if (result == RecheckResult.NoCode)
                    {
                        m_parent.m_global.m_log_presenter.Log(string.Format("第{0}个产品为无码（视觉发过来带nocode字样），不需要复判", index + 1));

                        MessageBox.Show(string.Format("第{0}个产品为无码（视觉发过来带nocode字样），不需要复判。\r\nno code的可能原因有：1.空穴（无料）；2.二维码视觉识别失败；3.二维码丢失（有料）", index + 1), "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else if (result == RecheckResult.MES_NG)
                    {
                        m_parent.m_global.m_log_presenter.Log(string.Format("第{0}个产品为MES提交过站失败的产品，二维码 {1}，不需要复判", index + 1, strProductBarcode));

                        MessageBox.Show(string.Format("第{0}个产品为MES提交过站失败的产品，二维码 {1}，不需要复判", index + 1, strProductBarcode), "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        m_parent.m_global.m_log_presenter.Log(string.Format("第{0}个产品不需要复判，二维码{1}", index + 1, strProductBarcode));
                        MessageBox.Show(string.Format("第{0}个产品不需要复判，二维码{1}", index + 1, strProductBarcode), "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    }

                    return;
                }
                else if (false == bFound && m_parent.m_global.m_strProductType == "nova")
                {
                    AVITrayInfo tray_info = m_parent.m_global.m_current_tray_info_for_Nova;

                    RecheckResult result = RecheckResult.NotChecked;
                    var resultInfo = "";
                    string strProductBarcode = "";
                    bool bFound2 = false;

                    int nColumns = m_parent.m_global.m_current_tray_info_for_Nova.Col;
                    int nRows = m_parent.m_global.m_current_tray_info_for_Nova.Row;

                    try
                    {
                        if (false == bFound2)
                        {
                            for (int i = 0; i < m_parent.m_global.m_current_tray_info_for_Nova.products_with_unrecheckable_defect.Count; i++)
                            {
                                Product product = m_parent.m_global.m_current_tray_info_for_Nova.products_with_unrecheckable_defect[i];

                                int col = product.PosCol - 1;
                                int row = product.PosRow - 1;

                                int nIndex = row * nColumns + col;

                                if (nIndex == index)
                                {
                                    result = m_parent.m_global.m_current_MES_tray_info.m_check_flags_by_product[nIndex];

                                    var allType = new List<string>();
                                    foreach (var defect in product.Defects)
                                    {
                                        allType.Add(defect.Type);
                                    }
                                    resultInfo = string.Join(", ", allType);

                                    strProductBarcode = product.BarCode;
                                    bFound2 = true;

                                    break;
                                }
                            }
                        }

                        if (false == bFound2)
                        {
                            for (int i = 0; i < m_parent.m_global.m_current_tray_info_for_Nova.NotRecievedPorducts.Count; i++)
                            {
                                Product product = m_parent.m_global.m_current_tray_info_for_Nova.NotRecievedPorducts[i];

                                int col = product.PosCol - 1;
                                int row = product.PosRow - 1;

                                int nIndex = row * nColumns + col;

                                if (nIndex == index)
                                {
                                    strProductBarcode = product.BarCode;
                                    break;
                                }
                            }
                        }

                        if (false == bFound2)
                            result = m_parent.m_global.m_current_MES_tray_info.m_check_flags_by_product[index];
                    }
                    catch (Exception ex)
                    {
                        // 提示错误信息
                        m_parent.m_global.m_log_presenter.Log(string.Format("获取产品信息失败，错误信息：{0}", ex.Message));

                        MessageBox.Show(string.Format("获取产品信息失败，错误信息：{0}", ex.Message), "错误", MessageBoxButton.OK, MessageBoxImage.Error);

                        return;
                    }

                    if (result == RecheckResult.Unrecheckable)
                    {
                        m_parent.m_global.m_log_presenter.Log(string.Format("第{0}个产品为不可复判项的产品，二维码 {1}，不需要复判", index + 1, strProductBarcode));

                        MessageBox.Show(string.Format("第{0}个产品为不可复判项的产品，二维码 {1}，缺陷为{2}", index + 1, strProductBarcode, resultInfo), "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        m_parent.m_global.m_log_presenter.Log(string.Format("第{0}个产品不需要复判，二维码{1}", index + 1, strProductBarcode));
                        MessageBox.Show(string.Format("第{0}个产品不需要复判，二维码{1}", index + 1, strProductBarcode), "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    }

                    return;
                }

                m_nCurrentProductIndex = index;

                render_tray_button_color();

                // 选中的才高亮
                m_product_buttons[index].Background = System.Windows.Media.Brushes.Blue;
                m_product_buttons[index].Foreground = System.Windows.Media.Brushes.White;

                // 在缺陷产品索引数组中查找当前产品索引，找到则记录其位置
                m_nCurrentDefectedProductIndex = -1;
                for (int i = 0; i < m_parent.m_global.m_current_MES_tray_info.m_array_indices_of_defected_products.Length; i++)
                {
                    if (m_parent.m_global.m_current_MES_tray_info.m_array_indices_of_defected_products[i] == m_nCurrentProductIndex)
                    {
                        m_nCurrentDefectedProductIndex = i;
                        m_nCurrentDefectIndex = 0;

                        break;
                    }
                }

                if (m_nCurrentDefectedProductIndex >= 0)
                {
                    // 更新 gridview_DefectsWaitingConfirm 和 gridview_RecheckItemBarcode 的选中行
                    if (true)
                    {
                        if (m_nCurrentDefectedProductIndex < gridview_RecheckItemBarcode.Rows.Count)
                        {
                            // 确定GridView可见的行数
                            int visibleRowCount = gridview_DefectsWaitingConfirm.DisplayedRowCount(false);

                            // 计算首个显示的行的索引，确保选中行为GridView的可见区域中的最后一行
                            int firstRowIndex = m_nCurrentDefectIndex - visibleRowCount + 1;
                            if (firstRowIndex < 0)
                            {
                                firstRowIndex = 0;  // 确保索引不会变成负数
                            }

                            if (firstRowIndex < gridview_DefectsWaitingConfirm.Rows.Count)
                            {
                                // 设置首个显示的行的索引，使选中的行为GridView的可见区域中的最后一行
                                gridview_DefectsWaitingConfirm.FirstDisplayedScrollingRowIndex = firstRowIndex;

                                SelectDefectedProductBarcode(m_nCurrentDefectedProductIndex);
                            }
                        }
                    }

                    if (m_parent.m_global.m_strProductType == "nova")
                    {
                        AVITrayInfo tray_info = m_parent.m_global.m_current_tray_info_for_Nova;

                        textblock_TrayBarcode.Text = tray_info.Products[m_nCurrentDefectedProductIndex].BarCode;

                        // 根据产品信息更新UI
                        m_parent.page_HomeView.UpdateUIWithProductInfoForNova(tray_info, m_nCurrentDefectedProductIndex);

                        m_nCurrentSideAorB = 0;

                        // 检查是否需要切换显示A面或B面的图片
                        SwitchAorBSideImageIfNeededForNova(m_nCurrentDefectIndex);

                        int nImageWidth = m_parent.page_HomeView.m_camera_canvases[0].get_origin_image_width();
                        int nImageHeight = m_parent.page_HomeView.m_camera_canvases[0].get_origin_image_height();

                        if (nImageWidth > 0 && nImageHeight > 0)
                        {
                            // 从product中获取缺陷信息
                            if (null != tray_info.Products[m_nCurrentDefectedProductIndex].Defects)
                            {
                                for (int k = 0; k < tray_info.Products[m_nCurrentDefectedProductIndex].Defects.Count; k++)
                                {
                                    ShowDefectOnCanvasForNova(m_nCurrentDefectedProductIndex, k, nImageWidth, nImageHeight);

                                    break;
                                }
                            }
                        }
                    }
                    else if (m_parent.m_global.m_strProductType == "dock")
                    {
                        TrayInfo tray_info = m_parent.m_global.m_current_tray_info_for_Dock;

                        textblock_TrayBarcode.Text = tray_info.products[m_nCurrentDefectedProductIndex].barcode;
                        if (m_parent.m_global.m_strProductSubType == "glue_check")
                        {
                            textblock_TrayBarcode.Text = tray_info.r7.Split('_')[0];
                        }

                        // 根据产品信息更新UI
                        m_parent.page_HomeView.UpdateUIWithProductInfoForDock(tray_info, m_nCurrentDefectedProductIndex);

                        m_nCurrentSideAorB = 0;

                        // 检查是否需要切换显示A面或B面的图片
                        bool bIsImageExisting = false;
                        SwitchAorBSideImageIfNeededForDock(m_nCurrentDefectIndex, ref bIsImageExisting);

                        int nImageWidth = m_parent.page_HomeView.m_camera_canvases[0].get_origin_image_width();
                        int nImageHeight = m_parent.page_HomeView.m_camera_canvases[0].get_origin_image_height();

                        if (nImageWidth > 0 && nImageHeight > 0)
                        {
                            // 从product中获取缺陷信息
                            if (null != tray_info.products[m_nCurrentDefectedProductIndex].defects)
                            {
                                for (int k = 0; k < tray_info.products[m_nCurrentDefectedProductIndex].defects.Count; k++)
                                {
                                    if (false)
                                    {
                                        if (m_parent.m_global.m_strProductSubType == "glue_check")
                                        {
                                            if (string.IsNullOrEmpty(m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Nova.Products[m_nCurrentDefectedProductIndex].Defects[k].Type))
                                                continue;
                                        }
                                    }

                                    ShowDefectOnCanvasForDock(m_nCurrentDefectedProductIndex, k, nImageWidth, nImageHeight);

                                    break;
                                }
                            }
                        }

                        // 如果图片不存在则不允许复判
                        if (true == m_parent.m_global.m_bDisableRecheckIfImageIsNotExisting)
                        {
                            if (false == bIsImageExisting)
                            {
                                m_bDisableRecheckCurrentProduct = true;

                                MessageBox.Show("当前产品缺陷没有对应图片，可能视觉电脑没保存，或者网络异常捞图失败，这将导致该产品缺陷无法复判。请查找原因。");

                                m_parent.m_global.m_log_presenter.Log("当前产品缺陷没有对应图片，可能视觉电脑没保存，或者网络异常捞图失败，这将导致该产品缺陷无法复判。请查找原因。");
                            }
                        }
                    }
                }
            }
        }

        // 按钮点击事件：确认NG
        public void BtnConfirmNG_Click(object sender, RoutedEventArgs e)
        {
            if (m_parent.m_global.m_bIsQueryingDatabaseAndCloningImages == true)
            {
				// 提示正在进行其他操作，请稍后再试
                if (m_parent.m_global.m_nRecheckModeWithAISystem != 3)
                {
                    MessageBox.Show("正在进行其他UI操作，请稍后再试", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    m_parent.m_global.m_log_presenter.LogError("正在进行其他UI操作，请稍后再试");
                }

                return;
            }

            if (m_parent.m_global.m_bEnableMESValidationForOriginalNGProducts == true && m_bIsValidatingWithMES == true)
            {
                // 提示正在进行MES校验，请稍后再试
                MessageBox.Show("正在进行MES校验，请稍后再试", "提示", MessageBoxButton.OK, MessageBoxImage.Information);

                return;
            }

            if (true == m_parent.m_global.m_bDisableRecheckIfImageIsNotExisting)
            {
                if (m_bDisableRecheckCurrentProduct == true)
                {
                    // 提示当前产品无法复判
                    MessageBox.Show("当前产品缺陷图片不存在，无法复判。请查找原因。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);

                    return;
                }
            }

            // 如果当前正在进行图像采集和渲染，并且时间未超过等待时间，则不允许操作
            //if ((GetTickCount() - m_parent.m_global.m_nStartTimeForImageAcquisitionAndRendering < m_parent.m_global.m_nWaitTimeForImageAcquisitionAndRendering))
            //{
            //    if (true == m_parent.m_global.m_bIsImagesCloningThreadInProcess[0])
            //    {
            //        // 提示正在克隆图片，请稍后再试
            //        MessageBox.Show("正在克隆图片，请稍后再试", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            //        return;
            //    }
            //}

            if (m_parent.m_global.m_strProductType == "nova")
            {
                if (m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Nova == null || m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Nova.Products.Count == 0)
                {
                    MessageBox.Show("没有需要复判的缺陷和产品！");
                    return;
                }

                if (m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Nova == null)
                {
                    return;
                }
                int nCurrentTime = GetTickCount();

                if (nCurrentTime - m_nLastRecheckTime < m_parent.m_global.m_nRecheckInterval)
                {
                    m_nLastRecheckTime = nCurrentTime;

                    MessageBox.Show(m_parent, "复判间隔太短，无法确认", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                m_nLastRecheckTime = nCurrentTime;

                if (m_nCurrentDefectedProductIndex >= 0 && m_nCurrentDefectedProductIndex < m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Nova.TotalPcs
                    //还要考虑料盘没跑完的情况，这时TotalPcs为料盘的最大料数，而list_flags_by_defect的数量才是实际料数
                    && m_nCurrentDefectedProductIndex <= m_parent.m_global.m_current_MES_tray_info.m_list_recheck_flags_by_defect.Count - 1)
                {
                    // 再次确认
                    //if (MessageBox.Show("确定要判定为NG吗？", "再次确认", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        m_parent.m_global.m_log_presenter.Log(string.Format("第{0}个缺陷 复判为 NG", m_nCurrentDefectIndex + 1));

                        // 设置MES缺陷信息的检测结果为NG
                        m_parent.m_global.m_current_MES_tray_info.m_list_recheck_flags_by_defect[m_nCurrentDefectedProductIndex][m_nCurrentDefectIndex] = RecheckResult.NG;

                        // 设置MES产品信息的检测结果为NG
                        m_parent.m_global.m_current_MES_tray_info.m_check_flags_by_product[m_nCurrentProductIndex] = RecheckResult.NG;

                        // 设置画布上的检测结果
                        m_panel_canvas.set_check_result(m_nCurrentProductIndex, RecheckResult.NG);

                        // 刷新PanelCanvas
                        m_panel_canvas.m_bForceRedraw = true;
                        m_panel_canvas.OnWindowSizeChanged(new object(), new EventArgs());

                        // 更新统计信息
                        //m_parent.m_global.m_recheck_statistics.m_nRecheckedNGs++;
                        //m_parent.m_global.m_recheck_statistics.Save();

                        //textblock_RecheckedNGs.Text = m_parent.m_global.m_recheck_statistics.m_nRecheckedNGs.ToString();

                        //// 计算复判率
                        //double ratio = (double)m_parent.m_global.m_recheck_statistics.m_nRecheckedOKs / (double)(m_parent.m_global.m_recheck_statistics.m_nRecheckedOKs + m_parent.m_global.m_recheck_statistics.m_nRecheckedNGs);
                        //textblock_WrongCheckRatio.Text = string.Format("{0:F2}%", ratio * 100);

                        BtnNextDefect_Click(sender, e);
                    }
                }
            }
            else if (m_parent.m_global.m_strProductType == "dock")
            {
                if (m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Dock == null || m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Dock.products.Count == 0)
                {
                    MessageBox.Show("没有需要复判的缺陷和产品！");
                    return;
                }

                if (m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Dock == null)
                {
                    return;
                }
                int nCurrentTime = GetTickCount();

                if (nCurrentTime - m_nLastRecheckTime < m_parent.m_global.m_nRecheckInterval)
                {
                    m_nLastRecheckTime = nCurrentTime;

                    MessageBox.Show(m_parent, "复判间隔太短，无法确认", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                m_nLastRecheckTime = nCurrentTime;

                int nTotalPcs = m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Dock.total_columns * m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Dock.total_rows;

                if (m_nCurrentDefectedProductIndex >= 0 && m_nCurrentDefectedProductIndex < nTotalPcs)
                {
                    // 再次确认
                    //if (MessageBox.Show("确定要判定为NG吗？", "再次确认", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        m_parent.m_global.m_log_presenter.Log(string.Format("第{0}个缺陷 复判为 NG", m_nCurrentDefectIndex + 1));

                        // 设置MES缺陷信息的检测结果为NG
                        m_parent.m_global.m_current_MES_tray_info.m_list_recheck_flags_by_defect[m_nCurrentDefectedProductIndex][m_nCurrentDefectIndex] = RecheckResult.NG;

                        if (m_parent.m_global.m_current_MES_tray_info.m_list_recheck_flags_by_defect.Count <= m_nCurrentDefectedProductIndex)
                            return;

                        // 设置复判标志位
                        bool bAllChecked = true;
                        bool bNG = false;
                        for (int n = 0; n < m_parent.m_global.m_current_MES_tray_info.m_list_recheck_flags_by_defect[m_nCurrentDefectedProductIndex].Length; n++)
                        {
                            if (m_parent.m_global.m_current_MES_tray_info.m_list_recheck_flags_by_defect[m_nCurrentDefectedProductIndex][n] == RecheckResult.NotChecked)
                            {
                                bAllChecked = false;
                            }

                            if (m_parent.m_global.m_current_MES_tray_info.m_list_recheck_flags_by_defect[m_nCurrentDefectedProductIndex][n] == RecheckResult.NG)
                            {
                                bNG = true;
                            }
                        }
                        if (true == bAllChecked)
                        {
                            if (true == bNG)
                            {
                                m_parent.m_global.m_current_MES_tray_info.m_check_flags_by_product[m_nCurrentProductIndex] = RecheckResult.NG;

                                // 设置画布上的检测结果
                                m_panel_canvas.set_check_result(m_nCurrentProductIndex, RecheckResult.NG);
                            }
                            else
                                m_parent.m_global.m_current_MES_tray_info.m_check_flags_by_product[m_nCurrentProductIndex] = RecheckResult.OK;
                        }

                        // 刷新PanelCanvas
                        m_panel_canvas.m_bForceRedraw = true;
                        m_panel_canvas.OnWindowSizeChanged(new object(), new EventArgs());

                        BtnNextDefect_Click(sender, e);
                    }
                }
            }
        }

        // 按钮点击事件：确认OK
        public void BtnConfirmOK_Click(object sender, RoutedEventArgs e)
        {
            if (m_parent.m_global.m_bIsQueryingDatabaseAndCloningImages == true)
            {
				// 提示正在进行其他操作，请稍后再试
				if (m_parent.m_global.m_nRecheckModeWithAISystem != 3)
				{
					MessageBox.Show("正在进行其他UI操作，请稍后再试", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
				}
				else
				{
					m_parent.m_global.m_log_presenter.LogError("正在进行其他UI操作，请稍后再试");
				}

                return;
            }

            if (m_parent.m_global.m_bEnableMESValidationForOriginalNGProducts == true && m_bIsValidatingWithMES == true)
            {
                // 提示正在进行MES校验，请稍后再试
                MessageBox.Show("正在进行MES校验，请稍后再试", "提示", MessageBoxButton.OK, MessageBoxImage.Information);

                return;
            }

            if (true == m_parent.m_global.m_bDisableRecheckIfImageIsNotExisting)
            {
                if (m_bDisableRecheckCurrentProduct == true)
                {
                    // 提示当前产品无法复判
                    MessageBox.Show("当前产品缺陷图片不存在，无法复判。请查找原因。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);

                    return;
                }
            }

            if (m_parent.m_global.m_strProductType == "nova")
            {
                if (m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Nova == null || m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Nova.Products.Count == 0)
                {
                    MessageBox.Show("没有需要复判的缺陷和产品！");
                    return;
                }

                if (m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Nova == null)
                {
                    return;
                }
                int nCurrentTime = GetTickCount();

                if (nCurrentTime - m_nLastRecheckTime < m_parent.m_global.m_nRecheckInterval)
                {
                    m_nLastRecheckTime = nCurrentTime;

                    MessageBox.Show(m_parent, "复判间隔太短，无法确认", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                m_nLastRecheckTime = nCurrentTime;

                if (m_nCurrentDefectedProductIndex >= 0 && m_nCurrentDefectedProductIndex < m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Nova.TotalPcs
                    //还要考虑料盘没跑完的情况，这时TotalPcs为料盘的最大料数，而list_flags_by_defect的数量才是实际料数
                    && m_nCurrentDefectedProductIndex <= m_parent.m_global.m_current_MES_tray_info.m_list_recheck_flags_by_defect.Count - 1)
                {
                    // 如果该产品有不可复判项，则不允许判定为OK
                    if (true == m_parent.m_global.m_bShowPictureOfUncheckableDefect)
                    {
                        if (m_parent.m_global.m_current_MES_tray_info.m_list_recheck_flags_by_defect[m_nCurrentDefectedProductIndex][m_nCurrentDefectIndex] == RecheckResult.Unrecheckable)
                        {
                            MessageBox.Show("该产品已包含不可复判项，无法判定为OK！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                            return;
                        }
                    }

                    m_parent.m_global.m_log_presenter.Log(string.Format("第{0}个缺陷 复判为 OK", m_nCurrentDefectIndex + 1));

                    // 设置MES产品信息的检测结果为OK
                    m_parent.m_global.m_current_MES_tray_info.m_list_recheck_flags_by_defect[m_nCurrentDefectedProductIndex][m_nCurrentDefectIndex] = RecheckResult.OK;

                    // 更新统计信息
                    //m_parent.m_global.m_recheck_statistics.m_nRecheckedOKs++;
                    //m_parent.m_global.m_recheck_statistics.Save();

                    //textblock_RecheckedOKs.Text = m_parent.m_global.m_recheck_statistics.m_nRecheckedOKs.ToString();

                    //// 计算复判率
                    //double ratio = (double)m_parent.m_global.m_recheck_statistics.m_nRecheckedOKs / (double)(m_parent.m_global.m_recheck_statistics.m_nRecheckedOKs + m_parent.m_global.m_recheck_statistics.m_nRecheckedNGs);
                    //textblock_WrongCheckRatio.Text = string.Format("{0:F2}%", ratio * 100);

                    BtnNextDefect_Click(sender, e);

                    return;
                }
            }
            else if (m_parent.m_global.m_strProductType == "dock")
            {
                if (m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Dock == null || m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Dock.products.Count == 0)
                {
                    MessageBox.Show("没有需要复判的缺陷和产品！");
                    return;
                }

                if (m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Dock == null)
                {
                    return;
                }
                int nCurrentTime = GetTickCount();

                if (nCurrentTime - m_nLastRecheckTime < m_parent.m_global.m_nRecheckInterval)
                {
                    m_nLastRecheckTime = nCurrentTime;

                    MessageBox.Show(m_parent, "复判间隔太短，无法确认", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                m_nLastRecheckTime = nCurrentTime;

                int nTotalPcs = m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Dock.total_columns * m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Dock.total_rows;

                if (m_nCurrentDefectedProductIndex >= 0 && m_nCurrentDefectedProductIndex < nTotalPcs)
                {
                    if (m_parent.m_global.m_current_MES_tray_info.m_list_recheck_flags_by_defect[m_nCurrentDefectedProductIndex][m_nCurrentDefectIndex] != RecheckResult.Unrecheckable)
                    {
                        m_parent.m_global.m_log_presenter.Log(string.Format("第{0}个缺陷 复判为 OK", m_nCurrentDefectIndex + 1));

                        if (m_parent.m_global.m_current_MES_tray_info.m_list_recheck_flags_by_defect.Count <= m_nCurrentDefectedProductIndex)
                            return;

                        // 设置MES产品信息的检测结果为OK
                        m_parent.m_global.m_current_MES_tray_info.m_list_recheck_flags_by_defect[m_nCurrentDefectedProductIndex][m_nCurrentDefectIndex] = RecheckResult.OK;
                    }
                    else
                    {
                        m_parent.m_global.m_log_presenter.Log(string.Format("第{0}个缺陷为不可复判项，无需复判！", m_nCurrentDefectIndex + 1));
                    }

                    // 设置复判标志位
                    bool bAllChecked = true;
                    bool bOK = true;
                    for (int n = 0; n < m_parent.m_global.m_current_MES_tray_info.m_list_recheck_flags_by_defect[m_nCurrentDefectedProductIndex].Length; n++)
                    {
                        if (m_parent.m_global.m_current_MES_tray_info.m_list_recheck_flags_by_defect[m_nCurrentDefectedProductIndex][n] == RecheckResult.Unrecheckable)
                        {
                            bOK = false;
                            bAllChecked = true;
                            break;
                        }

                        if (m_parent.m_global.m_current_MES_tray_info.m_list_recheck_flags_by_defect[m_nCurrentDefectedProductIndex][n] == RecheckResult.NotChecked)
                        {
                            bAllChecked = false;
                        }

                        if (m_parent.m_global.m_current_MES_tray_info.m_list_recheck_flags_by_defect[m_nCurrentDefectedProductIndex][n] == RecheckResult.NG)
                        {
                            bOK = false;
                        }
                    }
                    if (true == bAllChecked)
                    {
                        if (true == bOK)
                            m_parent.m_global.m_current_MES_tray_info.m_check_flags_by_product[m_nCurrentProductIndex] = RecheckResult.OK;
                        else
                            m_parent.m_global.m_current_MES_tray_info.m_check_flags_by_product[m_nCurrentProductIndex] = RecheckResult.NG;
                    }

                    BtnNextDefect_Click(sender, e);

                    return;
                }
            }
        }

        private void generateContextMenuForBtnConfirmNG()
        {
            foreach (var d in m_parent.m_global.m_uncheckable_defect_types)
            {
                var item = new MenuItem();
                item.Header = d;
                item.Click += BtnConfirmNGContextMenuItem_Click;

                ContextMenuDefectCorrection.Items.Add(item);
            }
        }

        private void BtnConfirmNGContextMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var item = sender as MenuItem;

            if (item == null)
            {
                return;
            }

            var defectCorrection = new DefectCorrection();

            defectCorrection.CorrectedType = item.Header.ToString();
            defectCorrection.CorrectTime = DateTime.Now;
            defectCorrection.OperatorID = m_parent.m_global.m_strCurrentOperatorID;

            if (m_parent.m_global.m_strProductType == "nova")
            {
                var products = m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Nova.Products;
                if (m_nCurrentDefectedProductIndex < products.Count &&
                    m_nCurrentDefectIndex < products[m_nCurrentDefectedProductIndex].Defects.Count)
                {
                    var product = products[m_nCurrentDefectedProductIndex];

                    defectCorrection.OriginalType = product.Defects[m_nCurrentDefectIndex].Type;
                    defectCorrection.ProductSN = product.BarCode;
                    defectCorrection.ProductID = m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Nova.BatchId;
                    defectCorrection.ImagePath = product.ShareimgPath;

                    product.Defects[m_nCurrentDefectIndex].Type = item.Header.ToString();
                }
            }
            else if (m_parent.m_global.m_strProductType == "dock")
            {
                var products = m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Dock.products;
                if (m_nCurrentDefectedProductIndex < products.Count &&
                    m_nCurrentDefectIndex < products[m_nCurrentDefectedProductIndex].defects.Count)
                {
                    var product = products[m_nCurrentDefectedProductIndex];

                    defectCorrection.OriginalType = product.defects[m_nCurrentDefectIndex].type;
                    defectCorrection.ProductSN = product.barcode;
                    defectCorrection.ProductID = m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Nova.BatchId;
                    defectCorrection.ImagePath = product.sideA_image_path;

                    product.defects[m_nCurrentDefectIndex].type = item.Header.ToString();
                }
            }

            m_parent.m_global.m_list_DefectCorrectionRecord.Add(defectCorrection);
            m_parent.page_HomeView.BtnConfirmNG.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        }

        // 按钮点击事件：待确认OK
        private void BtnPendingConfirmOK_Click(object sender, RoutedEventArgs e)
        {

        }

        // 按钮点击事件：上一个缺陷
        public void BtnPreviousDefect_Click(object sender, RoutedEventArgs e)
        {
            if (m_parent.m_global.m_bIsQueryingDatabaseAndCloningImages == true)
            {
				// 提示正在进行其他操作，请稍后再试
				if (m_parent.m_global.m_nRecheckModeWithAISystem != 3)
				{
					MessageBox.Show("正在进行其他UI操作，请稍后再试", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
				}
				else
				{
					m_parent.m_global.m_log_presenter.LogError("正在进行其他UI操作，请稍后再试");
				}

                return;
            }

            if (m_parent.m_global.m_strProductType == "nova")
            {
                if (m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Nova == null || m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Nova.Products.Count == 0)
                {
                    MessageBox.Show("没有需要复判的缺陷和产品！");
                    return;
                }

                if (null == m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Nova)
                {
                    MessageBox.Show("产品信息为空，无法进行上一个缺陷的复判！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            else if (m_parent.m_global.m_strProductType == "dock")
            {
                if (m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Dock == null || m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Dock.products.Count == 0)
                {
                    MessageBox.Show("没有需要复判的缺陷和产品！");
                    return;
                }

                if (null == m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Dock)
                {
                    MessageBox.Show("产品信息为空，无法进行上一个缺陷的复判！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            if (m_nCurrentDefectIndex > 0)
            {
                if (m_nCurrentDefectIndex >= gridview_DefectsWaitingConfirm.Rows.Count)
                {
                    return;
                }

                m_nCurrentDefectIndex--;
                if (m_parent.m_global.m_strProductType == "nova")
                {
                    if (m_nCurrentDefectedProductIndex >= m_parent.m_global.m_current_tray_info_for_Nova.Products.Count)
                    {
                        m_nCurrentDefectedProductIndex--;
                    }
                }
                else if (m_parent.m_global.m_strProductType == "dock")
                {
                    if (m_nCurrentDefectedProductIndex >= m_parent.m_global.m_current_tray_info_for_Dock.products.Count)
                    {
                        m_nCurrentDefectedProductIndex--;
                    }
                }

                // 检查是否需要切换显示A面或B面的图片
                if (m_parent.m_global.m_strProductType == "nova")
                    SwitchAorBSideImageIfNeededForNova(m_nCurrentDefectIndex);
                else if (m_parent.m_global.m_strProductType == "dock")
                {
                    bool bIsImageExisting = false;

                    SwitchAorBSideImageIfNeededForDock(m_nCurrentDefectIndex, ref bIsImageExisting);
                }

                int nImageWidth = m_parent.page_HomeView.m_camera_canvases[0].get_origin_image_width();
                int nImageHeight = m_parent.page_HomeView.m_camera_canvases[0].get_origin_image_height();

                gridview_DefectsWaitingConfirm.Rows[m_nCurrentDefectIndex].Selected = true;

                // 确定GridView可见的行数
                int visibleRowCount = gridview_DefectsWaitingConfirm.DisplayedRowCount(false);

                // 计算首个显示的行的索引，确保选中行为GridView的可见区域中的最后一行
                int firstRowIndex = m_nCurrentDefectIndex - visibleRowCount + 1;
                if (firstRowIndex < 0)
                {
                    firstRowIndex = 0;  // 确保索引不会变成负数
                }

                // 设置首个显示的行的索引，使选中的行为GridView的可见区域中的最后一行
                gridview_DefectsWaitingConfirm.FirstDisplayedScrollingRowIndex = firstRowIndex;

                if (m_parent.m_global.m_strProductType == "nova")
                    ShowDefectOnCanvasForNova(m_nCurrentDefectedProductIndex, m_nCurrentDefectIndex, nImageWidth, nImageHeight);
                else if (m_parent.m_global.m_strProductType == "dock")
                    ShowDefectOnCanvasForDock(m_nCurrentDefectedProductIndex, m_nCurrentDefectIndex, nImageWidth, nImageHeight);
            }
            else
            {
                // 提示已经是第一个缺陷，没有上一个
                //MessageBox.Show("已经是第一个缺陷，没有上一个", "提示", MessageBoxButton.OK, MessageBoxImage.Information);

                if (m_nCurrentDefectedProductIndex > 0)
                {
                    bool bFound = false;
                    int nTempIndex = m_nCurrentDefectedProductIndex - 1;
                    for (int i = m_parent.m_global.m_current_MES_tray_info.m_array_indices_of_defected_products.Length - 1; i >= 0; i--)
                    {
                        if (i == nTempIndex)
                        {
                            // 判断是否无码
                            if (m_parent.m_global.m_strProductType == "dock")
                            {
                                if (m_parent.m_global.m_current_tray_info_for_Dock.products[i].barcode.IndexOf("nocode", StringComparison.OrdinalIgnoreCase) >= 0)
                                {
                                    nTempIndex++;
                                    continue;
                                }
                            }
                            if (m_parent.m_global.m_strProductType == "nova")
                            {
                                if (m_parent.m_global.m_current_tray_info_for_Nova.Products[i].BarCode.IndexOf("nocode", StringComparison.OrdinalIgnoreCase) >= 0)
                                {
                                    nTempIndex++;
                                    continue;
                                }
                            }

                            bFound = true;
                            break;
                        }
                    }

                    if (false == bFound)
                    {
                        // 提示已经是最后一个产品，没有下一个
                        MessageBox.Show("已经是第一个产品，没有上一个", "提示", MessageBoxButton.OK, MessageBoxImage.Information);

                        return;
                    }

                    m_nCurrentDefectedProductIndex = nTempIndex;

                    SelectDefectedProductBarcode(m_nCurrentDefectedProductIndex);

                    if (m_nCurrentDefectedProductIndex >= 0)
                        SelectProduct(m_parent.m_global.m_current_MES_tray_info.m_array_indices_of_defected_products[m_nCurrentDefectedProductIndex]);
                }
            }
        }

        // 按钮点击事件：下一个缺陷
        public void BtnNextDefect_Click(object sender, RoutedEventArgs e)
        {
            if (m_parent.m_global.m_bIsQueryingDatabaseAndCloningImages == true)
            {
				// 提示正在进行其他操作，请稍后再试
				if (m_parent.m_global.m_nRecheckModeWithAISystem != 3)
				{
					MessageBox.Show("正在进行其他UI操作，请稍后再试", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
				}
				else
				{
					m_parent.m_global.m_log_presenter.LogError("正在进行其他UI操作，请稍后再试");
				}

                return;
            }

            if (m_parent.m_global.m_bEnableMESValidationForOriginalNGProducts == true && m_bIsValidatingWithMES == true)
            {
                // 提示正在进行MES校验，请稍后再试
                MessageBox.Show("正在进行MES校验，请稍后再试", "提示", MessageBoxButton.OK, MessageBoxImage.Information);

                return;
            }

            if (m_parent.m_global.m_strProductType == "nova")
            {
                if (m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Nova == null || m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Nova.Products.Count == 0)
                {
                    MessageBox.Show("没有需要复判的缺陷和产品！");
                    return;
                }

                if (null == m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Nova)
                {
                    MessageBox.Show("产品信息为空，无法进行下一个缺陷的复判！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (m_nCurrentDefectedProductIndex >= m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Nova.Products.Count)
                {
                    MessageBox.Show("已经是最后一个产品，没有下一个", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                if (m_nCurrentDefectIndex < m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Nova.Products[m_nCurrentDefectedProductIndex].Defects.Count - 1)
                {
                    m_nCurrentDefectIndex++;

                    for (int i = 0; i < m_parent.m_global.m_uncheckable_pass_types.Count; i++)
                    {
                        if (m_parent.m_global.m_uncheckable_pass_enable_flags[i] == true &&
                        m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Nova.Products[m_nCurrentDefectedProductIndex].Defects[m_nCurrentDefectIndex].Type == m_parent.m_global.m_uncheckable_pass_types[i])
                        {
                            if (m_nCurrentDefectIndex + 1 < gridview_DefectsWaitingConfirm.Rows.Count)
                            {
                                gridview_DefectsWaitingConfirm.Rows[m_nCurrentDefectIndex + 1].Selected = true;
                            }

                            m_parent.page_HomeView.BtnConfirmOK.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                        }
                    }

                    // 检查是否需要切换显示A面或B面的图片
                    SwitchAorBSideImageIfNeededForNova(m_nCurrentDefectIndex);

                    int nImageWidth = m_parent.page_HomeView.m_camera_canvases[0].get_origin_image_width();
                    int nImageHeight = m_parent.page_HomeView.m_camera_canvases[0].get_origin_image_height();

                    gridview_DefectsWaitingConfirm.Rows[m_nCurrentDefectIndex].Selected = true;

                    // 确定GridView可见的行数
                    int visibleRowCount = gridview_DefectsWaitingConfirm.DisplayedRowCount(false);

                    // 计算首个显示的行的索引，确保选中行为GridView的可见区域中的最后一行
                    int firstRowIndex = m_nCurrentDefectIndex - visibleRowCount + 1;
                    if (firstRowIndex < 0)
                    {
                        firstRowIndex = 0;  // 确保索引不会变成负数
                    }

                    // 设置首个显示的行的索引，使选中的行为GridView的可见区域中的最后一行
                    gridview_DefectsWaitingConfirm.FirstDisplayedScrollingRowIndex = firstRowIndex;

                    ShowDefectOnCanvasForNova(m_nCurrentDefectedProductIndex, m_nCurrentDefectIndex, nImageWidth, nImageHeight);
                }
                else
                {
                    // 提示已经是最后一个缺陷，没有下一个
                    //MessageBox.Show("已经是最后一个缺陷，没有下一个", "提示", MessageBoxButton.OK, MessageBoxImage.Information);

                    if (m_nCurrentDefectedProductIndex <= m_parent.m_global.m_current_tray_info_for_Nova.Products.Count - 1
                        && m_nCurrentDefectedProductIndex <= gridview_RecheckItemBarcode.Rows.Count - 1)
                    {
                        // 判断是否全为OK
                        bool bAllOK = true;
                        for (int n = 0; n < m_parent.m_global.m_current_MES_tray_info.m_list_recheck_flags_by_defect[m_nCurrentDefectedProductIndex].Length; n++)
                        {
                            if (m_parent.m_global.m_current_MES_tray_info.m_list_recheck_flags_by_defect[m_nCurrentDefectedProductIndex][n] != RecheckResult.OK)
                            {
                                bAllOK = false;
                                break;
                            }
                        }

                        // 如果全为OK，则刷新PanelCanvas
                        if (true == bAllOK)
                        {
                            m_parent.m_global.m_current_MES_tray_info.m_check_flags_by_product[m_nCurrentProductIndex] = RecheckResult.OK;

                            // 设置画布上的检测结果
                            m_panel_canvas.set_check_result(m_nCurrentProductIndex, RecheckResult.OK);

                            // 刷新PanelCanvas
                            m_panel_canvas.m_bForceRedraw = true;
                            m_panel_canvas.OnWindowSizeChanged(new object(), new EventArgs());

                            //m_parent.m_global.m_recheck_statistics.m_confirmOKs++;
                            //textblock_TotalConfirmedOKs.Text = m_parent.m_global.m_recheck_statistics.m_confirmOKs.ToString();
                            //m_parent.m_global.m_recheck_statistics.Save();
                        }
                        //else
                        //{
                        //    m_parent.m_global.m_recheck_statistics.m_confirmNGs++;
                        //    textblock_TotalConfirmedOKs.Text = m_parent.m_global.m_recheck_statistics.m_confirmNGs.ToString();
                        //    m_parent.m_global.m_recheck_statistics.Save();
                        //}

                        m_nCurrentDefectedProductIndex++;
                        m_nCurrentProductIndex = m_nCurrentDefectedProductIndex;

                        SelectDefectedProductBarcode(m_nCurrentDefectedProductIndex);

                        if (m_nCurrentDefectedProductIndex < m_parent.m_global.m_current_MES_tray_info.m_array_indices_of_defected_products.Length)
                            SelectProduct(m_parent.m_global.m_current_MES_tray_info.m_array_indices_of_defected_products[m_nCurrentDefectedProductIndex]);
                    }

                    if (m_nCurrentDefectIndex != 0 && m_nCurrentDefectedProductIndex > m_parent.m_global.m_current_tray_info_for_Nova.Products.Count - 1)
                    {
                        // 提示已经是最后一个产品，没有下一个
                        MessageBox.Show("已经是最后一个产品，没有下一个", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            else if (m_parent.m_global.m_strProductType == "dock")
            {
                if (m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Dock == null || m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Dock.products.Count == 0)
                {
                    MessageBox.Show("没有需要复判的缺陷和产品！");
                    return;
                }

                if (null == m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Dock)
                {
                    MessageBox.Show("产品信息为空，无法进行下一个缺陷的复判！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                bool hasCheckAll = false;
                //if (m_parent.m_global.m_strProductSubType == "glue_check")
                //{
                //    hasCheckAll = m_nCurrentDefectIndex < gridview_DefectsWaitingConfirm.Rows.Count - 1;
                //}
                //else
                //{
                hasCheckAll = m_nCurrentDefectIndex < m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Dock.products[m_nCurrentDefectedProductIndex].defects.Count - 1;
                //}
                //if (m_nCurrentDefectIndex < m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Dock.products[m_nCurrentDefectedProductIndex].defects.Count - 1)
                if (hasCheckAll)
                {
                    m_nCurrentDefectIndex++;

                    // 检查是否需要切换显示A面或B面的图片
                    bool bIsImageExisting = false;
                    SwitchAorBSideImageIfNeededForDock(m_nCurrentDefectIndex, ref bIsImageExisting);

                    int nImageWidth = m_parent.page_HomeView.m_camera_canvases[0].get_origin_image_width();
                    int nImageHeight = m_parent.page_HomeView.m_camera_canvases[0].get_origin_image_height();

                    gridview_DefectsWaitingConfirm.Rows[m_nCurrentDefectIndex].Selected = true;

                    // 确定GridView可见的行数
                    int visibleRowCount = gridview_DefectsWaitingConfirm.DisplayedRowCount(false);

                    // 计算首个显示的行的索引，确保选中行为GridView的可见区域中的最后一行
                    int firstRowIndex = m_nCurrentDefectIndex - visibleRowCount + 1;
                    if (firstRowIndex < 0)
                    {
                        firstRowIndex = 0;  // 确保索引不会变成负数
                    }

                    // 设置首个显示的行的索引，使选中的行为GridView的可见区域中的最后一行
                    gridview_DefectsWaitingConfirm.FirstDisplayedScrollingRowIndex = firstRowIndex;

                    ShowDefectOnCanvasForDock(m_nCurrentDefectedProductIndex, m_nCurrentDefectIndex, nImageWidth, nImageHeight);
                }
                else
                {
                    // 提示已经是最后一个缺陷，没有下一个
                    //MessageBox.Show("已经是最后一个缺陷，没有下一个", "提示", MessageBoxButton.OK, MessageBoxImage.Information);

                    if ((m_nCurrentDefectedProductIndex + 1) < m_parent.m_global.m_current_tray_info_for_Dock.products.Count
                        && (m_nCurrentDefectedProductIndex + 1) < gridview_RecheckItemBarcode.Rows.Count)
                    {
                        // 判断是否全为OK
                        bool bAllOK = true;
                        for (int n = 0; n < m_parent.m_global.m_current_MES_tray_info.m_list_recheck_flags_by_defect[m_nCurrentDefectedProductIndex].Length; n++)
                        {
                            if (m_parent.m_global.m_current_MES_tray_info.m_list_recheck_flags_by_defect[m_nCurrentDefectedProductIndex][n] != RecheckResult.OK)
                            {
                                bAllOK = false;
                                break;
                            }
                        }

                        // 如果全为OK，则刷新PanelCanvas
                        if (true == bAllOK)
                        {
                            m_parent.m_global.m_current_MES_tray_info.m_check_flags_by_product[m_nCurrentProductIndex] = RecheckResult.OK;

                            // 设置画布上的检测结果
                            m_panel_canvas.set_check_result(m_nCurrentProductIndex, RecheckResult.OK);

                            // 刷新PanelCanvas
                            m_panel_canvas.m_bForceRedraw = true;
                            m_panel_canvas.OnWindowSizeChanged(new object(), new EventArgs());
                        }

                        bool bFound = false;
                        int nTempIndex = m_nCurrentDefectedProductIndex + 1;
                        for (int i = 0; i < m_parent.m_global.m_current_MES_tray_info.m_array_indices_of_defected_products.Length; i++)
                        {
                            if (i == nTempIndex)
                            {
                                // 判断是否无码
                                if (m_parent.m_global.m_current_tray_info_for_Dock.products[i].barcode.IndexOf("nocode", StringComparison.OrdinalIgnoreCase) >= 0)
                                {
                                    nTempIndex++;
                                    continue;
                                }

                                bFound = true;
                                break;
                            }
                        }

                        if (false == bFound)
                        {
                            // 提示已经是最后一个产品，没有下一个
                            MessageBox.Show("已经是最后一个产品，没有下一个", "提示", MessageBoxButton.OK, MessageBoxImage.Information);

                            return;
                        }

                        m_nCurrentDefectedProductIndex = nTempIndex;

                        SelectDefectedProductBarcode(m_nCurrentDefectedProductIndex);

                        if (m_nCurrentDefectedProductIndex < m_parent.m_global.m_current_MES_tray_info.m_array_indices_of_defected_products.Length)
                            SelectProduct(m_parent.m_global.m_current_MES_tray_info.m_array_indices_of_defected_products[m_nCurrentDefectedProductIndex]);
                    }
                    else
                    {
                        // 提示已经是最后一个产品，没有下一个
                        MessageBox.Show("已经是最后一个产品，没有下一个", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
        }

        // 按钮点击事件：查询（从数据库提取数据）
        private void BtnRequestData_Click(object sender, RoutedEventArgs e)
        {
            if (m_parent.m_global.m_bIsQueryingDatabaseAndCloningImages == true)
            {
				// 提示正在进行其他操作，请稍后再试
				if (m_parent.m_global.m_nRecheckModeWithAISystem != 3)
				{
					MessageBox.Show("正在进行其他UI操作，请稍后再试", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
				}
				else
				{
					m_parent.m_global.m_log_presenter.LogError("正在进行其他UI操作，请稍后再试");
				}

                return;
            }

            m_parent.m_global.m_bIsQueryingDatabaseAndCloningImages = true;

            string strBarcode = textbox_ScannedBarcode.Text.Trim();

            // 检查产品ID是否合法
            if (strBarcode.Length <= 0)
            {
                MessageBox.Show("扫码内容不能为空", "错误", MessageBoxButton.OK, MessageBoxImage.Error);

                m_parent.m_global.m_bIsQueryingDatabaseAndCloningImages = false;
                return;
            }

            if (m_parent.m_global.m_bIsImagesCloningThreadInProcess[0] == true)
            {
                // 提示正在克隆图片，请稍后再试
                MessageBox.Show("上一次查询正在拷贝图片，请稍后再试", "提示", MessageBoxButton.OK, MessageBoxImage.Information);

                m_parent.m_global.m_bIsQueryingDatabaseAndCloningImages = false;
                return;
            }

            m_parent.m_global.m_MES_service.m_productInfos_waitSubmitToAI_queue.Clear();
            ClearProductInfo();

            // 每天换班时间自动清空UI上的统计数据
            //if (DateTime.Now.Hour == 8 || DateTime.Now.Hour == 20)
            //{
            //    if (!m_bHasClearRecheckData)
            //    {
            //        m_parent.page_HomeView.BtnClearProductCount.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            //        m_bHasClearRecheckData = true;
            //    }
            //}
            //else
            //{
            //    m_bHasClearRecheckData = false;
            //}

            // 等待200ms，以便让UI更新
            //await Task.Delay(200);

            //try
            //{
            //    m_parent.progressBar_ImageClone.Value = 0;

            //    m_parent.textblock_ImageCloneProgress.Text = "0%";
            //}
            //catch (Exception ex)
            //{
            //    m_parent.m_global.m_log_presenter.Log(string.Format("更新进度条失败。错误信息：{0}", ex.Message));
            //}

            //m_parent.m_global.m_log_presenter.Log("正在查询数据...请稍后...");

            //BtnQueryData.IsEnabled = false;

            // 查询耗时
            TimeSpan ts = new TimeSpan(0, 0, 0);

            string strMachineName1 = "";
            string strMachineName2 = "";

            string strQueryResult = "";

            int nMachineIndex = 0;

            // 获取数据存储的表并且获得set_id
            bool bFound = false;
            if (true == m_parent.m_global.m_bSelectByMachine)
            {
                if (0 == m_parent.m_global.m_nSelectedMachineIndexForDatabaseQuery)
                {
                    List<string> list_MachineNames1 = new List<string>();
                    List<string> list_MachineNames2 = new List<string>();
                    List<DateTime> list_DateTimes = new List<DateTime>();

                    for (int n = 0; n < m_parent.m_global.m_dict_machine_names_and_IPs.Count; n++)
                    {
                        strMachineName1 = m_parent.m_global.m_dict_machine_names_and_IPs.Keys.ElementAt(n);

                        // 如果strMachineName为空，则跳过
                        if (true == string.IsNullOrEmpty(strMachineName1))
                            continue;

                        strMachineName1 = strMachineName1.Replace("-", "_");

                        string strProductTable = "products_" + strMachineName1;

                        string sql = "SELECT * FROM " + strProductTable + " WHERE bar_code = '" + strBarcode + "';";

                        // 查询数据库，获取托盘数据
                        m_parent.m_global.m_mysql_ops.QueryTableData(strProductTable, sql, ref strQueryResult, ref ts);

                        if (strQueryResult.Length > 0)
                        {
                            nMachineIndex = n;

                            if (nMachineIndex % 2 == 0)
                            {
                                if (nMachineIndex + 1 < m_parent.m_global.m_dict_machine_names_and_IPs.Count)
                                    strMachineName2 = m_parent.m_global.m_dict_machine_names_and_IPs.Keys.ElementAt(nMachineIndex + 1);
                            }
                            else
                            {
                                if (nMachineIndex - 1 >= 0)
                                    strMachineName2 = m_parent.m_global.m_dict_machine_names_and_IPs.Keys.ElementAt(nMachineIndex - 1);
                            }

                            // 解析查询结果
                            string[] lines = strQueryResult.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
                            if (lines.Length > 0)
                            {
                                for (int i = 0; i < lines.Length; i++)
                                {
                                    string[] parts = lines[i].Trim().Split(new char[] { '@' }, StringSplitOptions.None);

                                    // 最后一个字段是时间戳
                                    if (parts.Length < 2)
                                        continue;

                                    string strDateTime = parts[parts.Length - 2];

                                    try
                                    {
                                        DateTime dt = DateTime.Parse(strDateTime);

                                        list_MachineNames1.Add(strMachineName1);
                                        list_MachineNames2.Add(strMachineName2);
                                        list_DateTimes.Add(dt);
                                    }
                                    catch (Exception ex)
                                    {
                                        m_parent.m_global.m_log_presenter.Log(string.Format("解析时间戳失败。错误信息：{0}", ex.Message));
                                    }
                                }
                            }

                            strMachineName2 = strMachineName2.Replace("-", "_");

                            bFound = true;

                            //break;
                        }
                    }

                    // 如果list_DateTimes有多个时间戳，则选择最新的时间戳
                    if (list_DateTimes.Count > 0)
                    {
                        DateTime dtLatest = list_DateTimes.Max();
                        int nIndex = list_DateTimes.IndexOf(dtLatest);

                        strMachineName1 = list_MachineNames1[nIndex];
                        strMachineName2 = list_MachineNames2[nIndex];

                        bFound = true;
                    }
                }
                else
                {
                    nMachineIndex = m_parent.m_global.m_nSelectedMachineIndexForDatabaseQuery - 1;

                    strMachineName1 = m_parent.m_global.m_dict_machine_names_and_IPs.Keys.ElementAt(nMachineIndex);

                    strMachineName1 = strMachineName1.Replace("-", "_");

                    if (true == m_parent.m_global.m_bCombineTwoMachinesAsOne)
                    {
                        if (nMachineIndex % 2 == 0)
                        {
                            if (nMachineIndex + 1 < m_parent.m_global.m_dict_machine_names_and_IPs.Count)
                                strMachineName2 = m_parent.m_global.m_dict_machine_names_and_IPs.Keys.ElementAt(nMachineIndex + 1);
                        }
                        else
                        {
                            if (nMachineIndex - 1 >= 0)
                                strMachineName2 = m_parent.m_global.m_dict_machine_names_and_IPs.Keys.ElementAt(nMachineIndex - 1);
                        }

                        strMachineName2 = strMachineName2.Replace("-", "_");
                    }

                    string strProductTable = "products_" + strMachineName1;

                    string sql = "SELECT * FROM " + strProductTable + " WHERE bar_code = '" + strBarcode + "';";

                    // 查询数据库，获取托盘数据
                    m_parent.m_global.m_mysql_ops.QueryTableData(strProductTable, sql, ref strQueryResult, ref ts);

                    if (strQueryResult.Length > 0)
                    {
                        bFound = true;
                    }
                    else
                    {
                        strProductTable = "products_" + strMachineName2;

                        sql = "SELECT * FROM " + strProductTable + " WHERE bar_code = '" + strBarcode + "';";

                        // 查询数据库，获取托盘数据
                        m_parent.m_global.m_mysql_ops.QueryTableData(strProductTable, sql, ref strQueryResult, ref ts);

                        if (strQueryResult.Length > 0)
                        {
                            bFound = true;
                        }
                    }
                }
            }
            else
            {
                for (int n = 0; n < m_parent.m_global.m_dict_machine_names_and_IPs.Count; n++)
                {
                    strMachineName1 = m_parent.m_global.m_dict_machine_names_and_IPs.Keys.ElementAt(n);

                    // 如果strMachineName为空，则跳过
                    if (true == string.IsNullOrEmpty(strMachineName1))
                        continue;

                    strMachineName1 = strMachineName1.Replace("-", "_");

                    string strProductTable = "products_" + strMachineName1;

                    string sql = "SELECT * FROM " + strProductTable + " WHERE bar_code = '" + strBarcode + "';";

                    // 查询数据库，获取托盘数据
                    m_parent.m_global.m_mysql_ops.QueryTableData(strProductTable, sql, ref strQueryResult, ref ts);

                    if (strQueryResult.Length > 0)
                    {
                        bFound = true;
                        break;
                    }
                }
            }
            if (false == bFound)
            {
                m_bIsAIRechecking = -1;

                string strMessage = string.Format("在数据库中未查询到产品ID为 {0} 的记录", strBarcode);

                if (true == m_parent.m_global.m_bSelectByMachine)
                {
                    strMessage = string.Format("在数据库 ({0}) 中未查询到产品ID为 {1} 的记录", combo_SelectMachine.Text, strBarcode);
                }

                m_parent.m_global.m_log_presenter.Log(strMessage);

                if (m_parent.m_global.m_nRecheckModeWithAISystem != 3)
                {
                    MessageBox.Show(strMessage, "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    m_parent.m_global.m_log_presenter.LogError(strMessage);
                }

                m_parent.m_global.m_bIsQueryingDatabaseAndCloningImages = false;

                return;
            }

            // 查询数据库
            string strTableName = "products_" + strMachineName1;
            strTableName = strTableName.Replace("-", "_");

            string strQuery = "SELECT * FROM " + strTableName + " WHERE bar_code = '" + strBarcode + "';";

            // 查询数据库，获取托盘数据
            strQueryResult = "";
            m_parent.m_global.m_mysql_ops.QueryTableData(strTableName, strQuery, ref strQueryResult, ref ts);

            if (true)
            {
                if (m_parent.m_global.m_strProductType == "nova")
                {
                    // 处理查询结果
                    List<Product> list_products = new List<Product>();
                    m_parent.m_global.m_database_service.ProcessProductDataQueryResult(strQueryResult, ref list_products);

                    if (list_products.Count > 0)
                    {
                        // 从数据库获取托盘数据
                        strQueryResult = "";
                        strTableName = "trays_" + strMachineName1;
                        strTableName = strTableName.Replace("-", "_");
                        strQuery = "SELECT * FROM " + strTableName + " WHERE set_id = '" + list_products[0].SetId + "';";

                        // 查询数据库，获取托盘数据
                        m_parent.m_global.m_mysql_ops.QueryTableData(strTableName, strQuery, ref strQueryResult, ref ts);

                        // 处理查询结果
                        List<AVITrayInfo> list_trays2 = new List<AVITrayInfo>();
                        m_parent.m_global.m_database_service.ProcessTrayDataQueryResult(strQueryResult, ref list_trays2);

                        if (list_trays2.Count > 0)
                        {
                            // 从数据库获取产品数据
                            strQueryResult = "";
                            strTableName = "products_" + strMachineName1;

                            strTableName = strTableName.Replace("-", "_");

                            strQuery = "SELECT * FROM " + strTableName + " WHERE set_id = '" + list_products[0].SetId + "';";

                            // 查询数据库，获取产品数据
                            strQueryResult = "";
                            m_parent.m_global.m_mysql_ops.QueryTableData(strTableName, strQuery, ref strQueryResult, ref ts);

                            // 处理查询结果
                            List<Product> list_products2 = new List<Product>();
                            m_parent.m_global.m_database_service.ProcessProductDataQueryResult(strQueryResult, ref list_products2);

                            if (list_products2.Count > 0)
                            {
                                gridview_RecheckItemNo.Rows.Clear();
                                gridview_RecheckItemBarcode.Rows.Clear();
                                gridview_DefectsWaitingConfirm.Rows.Clear();

                                // 得到每一个产品的数据，然后显示在UI上
                                if (true)
                                {
                                    try
                                    {
                                        // 构建查询的条件部分
                                        var barcodes = list_products2
                                            .Where(p => !string.IsNullOrEmpty(p.BarCode))
                                            .Select(p => $"'{p.BarCode}'")
                                            .Distinct()
                                            .ToList();

                                        if (barcodes.Count == 0)
                                        {
                                            // 如果没有条形码，直接返回
                                            m_parent.m_global.m_log_presenter.Log("产品没有条形码，无法查询缺陷数据。");

                                            m_parent.m_global.m_bIsQueryingDatabaseAndCloningImages = false;
                                            return;
                                        }

                                        string barcodeList = string.Join(", ", barcodes);

                                        strQueryResult = "";
                                        //strTableName = "Defects";

                                        // 查询第一个机器的数据
                                        string strTableName1 = $"details_{strMachineName1}".Replace("-", "_");
                                        string query1 = $"SELECT * FROM {strTableName1} WHERE product_id IN ({barcodeList}) and set_id = '{list_products2[0].SetId}';";

                                        // 执行查询，并处理结果
                                        Dictionary<string, List<Defect>> defectDataDict = new Dictionary<string, List<Defect>>();

                                        void ProcessQueryResult(string query, string tableName)
                                        {
                                            string queryResult = "";
                                            m_parent.m_global.m_mysql_ops.QueryTableData(tableName, query, ref queryResult, ref ts);

                                            List<Defect> list_defects = new List<Defect>();
                                            m_parent.m_global.m_database_service.ProcessDefectDataQueryResult(queryResult, ref list_defects);

                                            foreach (var defect in list_defects)
                                            {
                                                if (!defectDataDict.ContainsKey(defect.product_id))
                                                {
                                                    defectDataDict[defect.product_id] = new List<Defect>();
                                                }
                                                defectDataDict[defect.product_id].Add(defect);
                                            }
                                        }

                                        ProcessQueryResult(query1, strTableName1);

                                        // 处理每个产品
                                        foreach (var product in list_products2)
                                        {
                                            if (defectDataDict.TryGetValue(product.BarCode, out var defects))
                                            {
                                                product.Defects = defects;
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        m_parent.m_global.m_log_presenter.Log(string.Format("处理产品数据失败。错误信息：{0}", ex.Message));
                                    }
                                }
                                else
                                {
                                    for (int i = 0; i < list_products2.Count; i++)
                                    {
                                        Product product = list_products2[i];

                                        if (string.IsNullOrEmpty(product.BarCode))
                                            continue;

                                        strQueryResult = "";
                                        strTableName = "details_" + strMachineName1;

                                        strTableName = strTableName.Replace("-", "_");

                                        strQuery = "SELECT * FROM " + strTableName + " WHERE product_id = '" + product.BarCode + "' and set_id = '" + product.SetId + "';";

                                        // 查询数据库，获取缺陷数据
                                        m_parent.m_global.m_mysql_ops.QueryTableData(strTableName, strQuery, ref strQueryResult, ref ts);

                                        // 处理查询结果
                                        List<Defect> list_defects = new List<Defect>();
                                        m_parent.m_global.m_database_service.ProcessDefectDataQueryResult(strQueryResult, ref list_defects);

                                        if (list_defects.Count > 0)
                                        {
                                            // 保存缺陷数据
                                            product.Defects = list_defects;
                                        }
                                    }
                                }
                            }

                            // 确保产品按照行列号排列顺序
                            if (true)
                            {
                                List<Product> list_temp = new List<Product>();

                                int nRows = list_trays2[0].Row;
                                int nColumns = list_trays2[0].Col;

                                if (m_parent.m_global.m_strProductType == "yuehu")
                                {
                                    for (int n = 0; n < list_temp.Count; n++)
                                    {
                                        list_temp[n].PosRow = (nRows + 1) - list_temp[n].PosRow;
                                    }
                                }

                                for (int m = 0; m < nRows; m++)
                                {
                                    for (int n = 0; n < nColumns; n++)
                                    {
                                        int nTargetRow = m + 1;
                                        int nTargetColumn = n + 1;

                                        for (int i = 0; i < list_products2.Count; i++)
                                        {
                                            if (list_products2[i].PosCol == nTargetColumn && list_products2[i].PosRow == nTargetRow)
                                            {
                                                list_temp.Add(list_products2[i]);

                                                break;
                                            }
                                        }
                                    }
                                }

                                list_products2 = list_temp;
                            }

                            list_trays2[0].Products = list_products2;

                            m_parent.m_global.m_current_tray_info_for_Nova = list_trays2[0];

                            // 处理AVI产品信息
                            try
                            {
                                // 如果是单Pce模式，则只需要处理对应二维码的产品
                                if (m_parent.m_global.m_nRecheckDisplayMode == 1)
                                {
                                    List<Product> list_temp = new List<Product>();

                                    for (int i = 0; i < m_parent.m_global.m_current_tray_info_for_Nova.Products.Count; i++)
                                    {
                                        if (m_parent.m_global.m_current_tray_info_for_Nova.Products[i].BarCode == strBarcode)
                                        {
                                            list_temp.Add(m_parent.m_global.m_current_tray_info_for_Nova.Products[i]);
                                            break;
                                        }
                                    }

                                    m_parent.m_global.m_current_tray_info_for_Nova.Products = list_temp;
                                }

                                // 处理不可复判项
                                if (true)
                                {
                                    // 初始化不可复判缺陷产品列表
                                    m_parent.m_global.m_current_tray_info_for_Nova.products_with_unrecheckable_defect = new List<Product>();
                                    m_parent.m_global.m_current_tray_info_for_Nova.products_with_unrecheckable_pass = new List<Product>();

                                    // 遍历当前托盘中的所有产品
                                    for (int i = 0; i < m_parent.m_global.m_current_tray_info_for_Nova.Products.Count; i++)
                                    {
                                        Product product = m_parent.m_global.m_current_tray_info_for_Nova.Products[i];

                                        if (product.Defects == null)
                                            continue;

                                        // 遍历产品的所有缺陷
                                        for (int j = 0; j < product.Defects.Count; j++)
                                        {
                                            product.Defects[j].IsUncheckable = false;

                                            bool bIsUnrecheckable = false;

                                            // 遍历不可复判缺陷类型列表
                                            for (int k = 0; k < m_parent.m_global.m_uncheckable_defect_types.Count; k++)
                                            {
                                                // 如果不可复判缺陷类型是启用的
                                                if (m_parent.m_global.m_uncheckable_defect_enable_flags[k] == true)
                                                {
                                                    // 如果产品的缺陷类型与当前不可复判缺陷类型匹配
                                                    if (product.Defects[j].Type == m_parent.m_global.m_uncheckable_defect_types[k])
                                                    {
                                                        if (true == m_parent.m_global.m_bShowPictureOfUncheckableDefect)
                                                        {
                                                            product.Defects[j].IsUncheckable = true;
                                                        }
                                                        else
                                                        {
                                                            // 将带有不可复判缺陷的产品添加到列表中
                                                            m_parent.m_global.m_current_tray_info_for_Nova.products_with_unrecheckable_defect.Add(product);

                                                            // 从当前托盘中移除该产品
                                                            m_parent.m_global.m_current_tray_info_for_Nova.Products.RemoveAt(i);

                                                            // 调整索引，防止跳过下一个产品
                                                            i--;
                                                        }

                                                        // 标记为不可复判
                                                        bIsUnrecheckable = true;

                                                        break;
                                                    }
                                                }
                                            }

                                            // 如果产品带有不可复判缺陷，跳出内层循环
                                            if (true == bIsUnrecheckable)
                                                break;
                                        }

                                        // 逻辑同上，处理不可复判OK产品
                                        for (int j = 0; j < product.Defects.Count; j++)
                                        {
                                            bool isUnrecheckable = false;
                                            for (int k = 0; k < m_parent.m_global.m_uncheckable_pass_types.Count; k++)
                                            {
                                                if (m_parent.m_global.m_uncheckable_pass_enable_flags[k] == true)
                                                {
                                                    if (product.Defects[j].Type == m_parent.m_global.m_uncheckable_pass_types[k])
                                                    {
                                                        // 不可复判OK产品还需要点击查看其他缺陷，在此无需从Products中移除
                                                        m_parent.m_global.m_current_tray_info_for_Nova.products_with_unrecheckable_pass.Add(product);
                                                        //m_parent.m_global.m_current_tray_info_for_Nova.Products.RemoveAt(i);

                                                        //i--;

                                                        break;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }

                                int nStartTime = GetTickCount();

                                // 处理MES过站失败产品信息
                                if (true)
                                {
                                    m_parent.m_global.m_current_tray_info_for_Nova.MES_failed_products = new List<Product>();

                                    for (int j = 0; j < m_parent.m_global.m_current_tray_info_for_Nova.Products.Count; j++)
                                    {
                                        Product prod = m_parent.m_global.m_current_tray_info_for_Nova.Products[j];

                                        if (null != prod.r10 && prod.r10.Contains("MES_NG"))
                                        {
                                            m_parent.m_global.m_current_tray_info_for_Nova.MES_failed_products.Add(prod);

                                            m_parent.m_global.m_current_tray_info_for_Nova.Products.RemoveAt(j);

                                            j--;

                                            continue;
                                        }
                                    }
                                }

                                // 处理ET产品信息
                                if (true)
                                {
                                    m_parent.m_global.m_current_tray_info_for_Nova.ET_products = new List<Product>();

                                    for (int j = 0; j < m_parent.m_global.m_current_tray_info_for_Nova.Products.Count; j++)
                                    {
                                        Product prod = m_parent.m_global.m_current_tray_info_for_Nova.Products[j];

                                        if (null != prod.r10 && prod.r10.Contains("ET"))
                                        {
                                            m_parent.m_global.m_current_tray_info_for_Nova.ET_products.Add(prod);

                                            m_parent.m_global.m_current_tray_info_for_Nova.Products.RemoveAt(j);

                                            j--;

                                            continue;
                                        }
                                    }
                                }

                                // 处理NotRecieved或Defects为空产品信息
                                if (true)
                                {
                                    m_parent.m_global.m_current_tray_info_for_Nova.NotRecievedPorducts = new List<Product>();

                                    for (int j = 0; j < m_parent.m_global.m_current_tray_info_for_Nova.Products.Count; j++)
                                    {
                                        Product prod = m_parent.m_global.m_current_tray_info_for_Nova.Products[j];

                                        if (prod.Defects == null || prod.Defects.Count == 0)
                                        {
                                            m_parent.m_global.m_current_tray_info_for_Nova.NotRecievedPorducts.Add(prod);

                                            m_parent.m_global.m_current_tray_info_for_Nova.Products.RemoveAt(j);

                                            j--;

                                            continue;
                                        }
                                    }
                                }

                                // 处理来自AVI系统的托盘信息，用于Nova，主要处理数据库和图片路径存储
                                if (true == m_parent.m_global.m_tray_info_service.HandleTrayInfoFromAVIForNova_DatabaseAndImagePathStorage(m_parent.m_global.m_current_tray_info_for_Nova, false))
                                {
                                    //await Task.Delay(500);
                                    Thread.Sleep(1500);

                                    // 处理来自AVI系统的托盘信息，用于Nova，主要处理初始化标志和显示图片
                                    m_parent.m_global.m_tray_info_service.HandleTrayInfoFromAVIForNova_InitFlagsAndShowImage(m_parent.m_global.m_current_tray_info_for_Nova);

                                    // 处理不可复判的产品标识
                                    if (true == m_parent.m_global.m_bShowPictureOfUncheckableDefect)
                                    {
                                        int nTrayCols = m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Nova.Col;

                                        // 判断是否有不可复判项的产品，如果有，则更新 m_parent.m_global.m_current_MES_tray_info.m_check_flags_by_product
                                        for (int i = 0; i < m_parent.m_global.m_current_tray_info_for_Nova.Products.Count; i++)
                                        {
                                            Product product = m_parent.m_global.m_current_tray_info_for_Nova.Products[i];

                                            if (product.Defects == null)
                                                continue;

                                            int nRow = product.PosRow - 1;
                                            int nCol = product.PosCol - 1;

                                            // 遍历产品的所有缺陷
                                            for (int j = 0; j < product.Defects.Count; j++)
                                            {
                                                if (product.Defects[j].IsUncheckable == true)
                                                {
                                                    m_parent.m_global.m_current_MES_tray_info.m_check_flags_by_product[nCol + nRow * nTrayCols] = RecheckResult.Unrecheckable;

                                                    // 遍历m_parent.m_global.m_current_MES_tray_info.m_list_recheck_flags_by_defect[i]，将所有缺陷的复判结果设置为不可复判
                                                    for (int k = 0; k < m_parent.m_global.m_current_MES_tray_info.m_list_recheck_flags_by_defect[i].Length; k++)
                                                    {
                                                        m_parent.m_global.m_current_MES_tray_info.m_list_recheck_flags_by_defect[i][k] = RecheckResult.Unrecheckable;
                                                    }
                                                }
                                            }
                                        }

                                        // 更新按钮颜色
                                        render_tray_button_color();
                                    }
                                }

                                int nEndTime = GetTickCount();

                                m_parent.m_global.m_log_presenter.Log(string.Format("接收处理AVI数据耗时：{0:0.000}秒", (double)(nEndTime - nStartTime) / 1000.0f));

                                // MES校验
                                if (m_parent.m_global.m_bEnableMESValidationForOriginalNGProducts == true)
                                {
                                    (new Thread(thread_validate_original_NG_product_data_with_MES)).Start(strBarcode);
                                }
                            }
                            catch (Exception ex)
                            {
                                m_parent.m_global.m_log_presenter.Log(string.Format("处理AVI产品信息失败。错误信息：{0}", ex.Message));
                            }

                        }
                    }
                }

                if (m_parent.m_global.m_strProductType == "dock")
                {
                    // 处理查询结果
                    List<ProductInfo> list_products = new List<ProductInfo>();
                    m_parent.m_global.m_database_service.ProcessProductDataQueryResult(strQueryResult, ref list_products, !m_parent.m_global.m_bCombineTwoMachinesAsOne);

                    // 查询第二个机器的数据
                    if (true == m_parent.m_global.m_bCombineTwoMachinesAsOne && strMachineName2.Length > 0)
                    {
                        strTableName = "products_" + strMachineName2;
                        strTableName = strTableName.Replace("-", "_");
                        strQuery = "SELECT * FROM " + strTableName + " WHERE bar_code = '" + strBarcode + "';";

                        // 查询数据库，获取托盘数据
                        strQueryResult = "";
                        m_parent.m_global.m_mysql_ops.QueryTableData(strTableName, strQuery, ref strQueryResult, ref ts);

                        // 处理查询结果
                        m_parent.m_global.m_database_service.ProcessProductDataQueryResult(strQueryResult, ref list_products, !m_parent.m_global.m_bCombineTwoMachinesAsOne);
                    }

                    if (list_products.Count > 0)
                    {
                        // 从数据库获取托盘数据
                        strQueryResult = "";
                        strTableName = "trays_" + strMachineName1;

                        strTableName = strTableName.Replace("-", "_");

                        m_parent.m_global.m_strCurrentTrayTableName = strTableName;

                        strQuery = "SELECT * FROM " + strTableName + " WHERE set_id = '" + list_products[0].set_id + "';";

                        // 查询数据库，获取托盘数据
                        m_parent.m_global.m_mysql_ops.QueryTableData(strTableName, strQuery, ref strQueryResult, ref ts);

                        // 处理查询结果
                        List<TrayInfo> list_trays2 = new List<TrayInfo>();
                        m_parent.m_global.m_database_service.ProcessTrayDataQueryResult(strQueryResult, ref list_trays2, !m_parent.m_global.m_bCombineTwoMachinesAsOne);

                        // 查询第二个机器的数据
                        if (true == m_parent.m_global.m_bCombineTwoMachinesAsOne && strMachineName2.Length > 0)
                        {
                            strTableName = "trays_" + strMachineName2;

                            strTableName = strTableName.Replace("-", "_");

                            strQuery = "SELECT * FROM " + strTableName + " WHERE set_id = '" + list_products[0].set_id + "';";

                            // 查询数据库，获取托盘数据
                            strQueryResult = "";
                            m_parent.m_global.m_mysql_ops.QueryTableData(strTableName, strQuery, ref strQueryResult, ref ts);

                            // 处理查询结果
                            List<TrayInfo> list_trays3 = new List<TrayInfo>();
                            m_parent.m_global.m_database_service.ProcessTrayDataQueryResult(strQueryResult, ref list_trays3, !m_parent.m_global.m_bCombineTwoMachinesAsOne);

                            if (list_trays3.Count > 0)
                            {
                                list_trays2.AddRange(list_trays3);
                            }
                        }

                        if (list_trays2.Count > 0)
                        {
                            // 设置界面左上角的料号
                            m_parent.m_global.m_strProductNameShownOnUI = list_trays2[0].r4;
                            m_parent.page_HomeView.textblock_CurrentProductType.Text = list_trays2[0].r4;
                            if (m_parent.m_global.m_strProductSubType == "glue_check")
                            {
                                m_parent.page_HomeView.textblock_CurrentProductType.Text = list_trays2[0].r1;
                            }

                            // 从数据库获取产品数据
                            strQueryResult = "";
                            strTableName = "products_" + strMachineName1;

                            strTableName = strTableName.Replace("-", "_");

                            strQuery = "SELECT * FROM " + strTableName + " WHERE set_id = '" + list_products[0].set_id + "';";

                            // 查询数据库，获取产品数据
                            strQueryResult = "";
                            m_parent.m_global.m_mysql_ops.QueryTableData(strTableName, strQuery, ref strQueryResult, ref ts);

                            // 处理查询结果
                            List<ProductInfo> list_products2 = new List<ProductInfo>();
                            m_parent.m_global.m_database_service.ProcessProductDataQueryResult(strQueryResult, ref list_products2, !m_parent.m_global.m_bCombineTwoMachinesAsOne);

                            // 查询第二个机器的数据
                            if (true == m_parent.m_global.m_bCombineTwoMachinesAsOne && strMachineName2.Length > 0)
                            {
                                strTableName = "products_" + strMachineName2;

                                strTableName = strTableName.Replace("-", "_");

                                strQuery = "SELECT * FROM " + strTableName + " WHERE set_id = '" + list_products[0].set_id + "';";

                                // 查询数据库，获取产品数据
                                strQueryResult = "";
                                m_parent.m_global.m_mysql_ops.QueryTableData(strTableName, strQuery, ref strQueryResult, ref ts);

                                // 处理查询结果
                                m_parent.m_global.m_database_service.ProcessProductDataQueryResult(strQueryResult, ref list_products2, !m_parent.m_global.m_bCombineTwoMachinesAsOne);
                            }

                            if (list_products2.Count > 0)
                            {
                                gridview_RecheckItemNo.Rows.Clear();
                                gridview_RecheckItemBarcode.Rows.Clear();
                                gridview_DefectsWaitingConfirm.Rows.Clear();

                                // 确保产品按照行列号排列顺序
                                if (true)
                                {
                                    List<ProductInfo> list_temp = new List<ProductInfo>();
                                    List<ProductInfo> list_temp2 = new List<ProductInfo>();

                                    int nRows = list_trays2[0].total_rows;
                                    int nColumns = list_trays2[0].total_columns;

                                    if (222 == m_parent.m_global.m_nSpecialTrayCornerMode)
                                    {
                                        for (int n = 0; n < nColumns; n++)
                                        {
                                            for (int m = 0; m < nRows; m++)
                                            {
                                                int nTargetRow = m + 1;
                                                int nTargetColumn = n + 1;

                                                for (int i = 0; i < list_products2.Count; i++)
                                                {
                                                    if (list_products2[i].column == nTargetColumn && list_products2[i].row == nTargetRow)
                                                    {
                                                        list_temp.Add(list_products2[i]);

                                                        break;
                                                    }
                                                }
                                            }
                                        }

                                        for (int n = 0; n < list_temp.Count; n++)
                                        {
                                            int idx = (list_temp[n].column - 1) + nColumns * (list_temp[n].row - 1);
                                            int nCurrentRow = idx / nColumns; // 整数除法确定行索引
                                            int nCurrentCol = idx % nColumns; // 取余数确定列索引

                                            list_temp[n].column = nCurrentRow + 1;
                                            list_temp[n].row = nCurrentCol + 1;

                                            idx = (list_temp[n].column - 1) + nRows * (list_temp[n].row - 1);

                                            list_temp[n].column = idx % nColumns;
                                            list_temp[n].row = idx / nColumns;

                                            list_temp[n].column += 1;
                                            list_temp[n].row += 1;
                                        }

                                        for (int n = 0; n < nColumns; n++)
                                        {
                                            for (int m = 0; m < nRows; m++)
                                            {
                                                int nTargetRow = m + 1;
                                                int nTargetColumn = n + 1;

                                                for (int i = 0; i < list_temp.Count; i++)
                                                {
                                                    if (list_temp[i].column == nTargetColumn && list_temp[i].row == nTargetRow)
                                                    {
                                                        list_temp2.Add(list_temp[i]);

                                                        break;
                                                    }
                                                }
                                            }
                                        }

                                        list_temp = list_temp2;
                                    }
                                    else
                                    {
                                        for (int m = 0; m < nRows; m++)
                                        {
                                            for (int n = 0; n < nColumns; n++)
                                            {
                                                int nTargetRow = m + 1;
                                                int nTargetColumn = n + 1;

                                                for (int i = 0; i < list_products2.Count; i++)
                                                {
                                                    if (list_products2[i].column == nTargetColumn && list_products2[i].row == nTargetRow)
                                                    {
                                                        list_temp.Add(list_products2[i]);

                                                        break;
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    list_products2 = list_temp;
                                }

                                // 得到每一个产品的数据，然后显示在UI上
                                if (true)
                                {
                                    try
                                    {
                                        // 构建查询的条件部分
                                        var barcodes = list_products2
                                            .Where(p => !string.IsNullOrEmpty(p.barcode))
                                            .Select(p => $"'{p.barcode}'")
                                            .Distinct()
                                            .ToList();

                                        if (barcodes.Count == 0)
                                        {
                                            // 如果没有条形码，直接返回
                                            m_parent.m_global.m_log_presenter.Log("产品没有条形码，无法查询缺陷数据。");

                                            m_parent.m_global.m_bIsQueryingDatabaseAndCloningImages = false;
                                            return;
                                        }

                                        string barcodeList = string.Join(", ", barcodes);

                                        // 查询第一个机器的数据
                                        string strTableName1 = $"details_{strMachineName1}".Replace("-", "_");
                                        string query1 = $"SELECT * FROM {strTableName1} WHERE product_id IN ({barcodeList});";

                                        // 如果需要合并两个机器的数据，构建第二个查询
                                        string query2 = "";
                                        if (m_parent.m_global.m_bCombineTwoMachinesAsOne && !string.IsNullOrEmpty(strMachineName2))
                                        {
                                            string strTableName2 = $"details_{strMachineName2}".Replace("-", "_");
                                            query2 = $"SELECT * FROM {strTableName2} WHERE product_id IN ({barcodeList});";
                                        }

                                        // 执行查询，并处理结果
                                        Dictionary<string, List<DefectInfo>> defectDataDict = new Dictionary<string, List<DefectInfo>>();

                                        void ProcessQueryResult(string query, string tableName)
                                        {
                                            string queryResult = "";
                                            m_parent.m_global.m_mysql_ops.QueryTableData(tableName, query, ref queryResult, ref ts);

                                            List<DefectInfo> tempDefects = new List<DefectInfo>();
                                            m_parent.m_global.m_database_service.ProcessDefectDataQueryResult(queryResult, ref tempDefects, !m_parent.m_global.m_bCombineTwoMachinesAsOne);

                                            foreach (var defect in tempDefects)
                                            {
                                                if (!defectDataDict.ContainsKey(defect.product_id))
                                                {
                                                    defectDataDict[defect.product_id] = new List<DefectInfo>();
                                                }
                                                defectDataDict[defect.product_id].Add(defect);
                                            }
                                        }

                                        ProcessQueryResult(query1, strTableName1);

                                        if (!string.IsNullOrEmpty(query2))
                                        {
                                            ProcessQueryResult(query2, $"details_{strMachineName2}".Replace("-", "_"));
                                        }

                                        // 处理每个产品
                                        foreach (var product in list_products2)
                                        {
                                            if (defectDataDict.TryGetValue(product.barcode, out var defects))
                                            {
                                                product.defects = defects;
                                            }
                                            else
                                            {
                                                product.defects = new List<DefectInfo>();
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        m_parent.m_global.m_log_presenter.Log(string.Format("处理产品数据失败。错误信息：{0}", ex.Message));
                                    }
                                }
                                else
                                {
                                    for (int i = 0; i < list_products2.Count; i++)
                                    {
                                        ProductInfo product = list_products2[i];

                                        if (string.IsNullOrEmpty(product.barcode))
                                            continue;

                                        if (product.r3.Contains("PASS") || product.is_ok_product == true)
                                        {
                                            continue;
                                        }

                                        strQueryResult = "";
                                        strTableName = "details_" + strMachineName1;

                                        strTableName = strTableName.Replace("-", "_");

                                        strQuery = "SELECT * FROM " + strTableName + " WHERE product_id = '" + product.barcode + "' and set_id = '" + product.set_id + "';";

                                        // 查询数据库，获取缺陷数据
                                        m_parent.m_global.m_mysql_ops.QueryTableData(strTableName, strQuery, ref strQueryResult, ref ts);

                                        // 处理查询结果
                                        List<DefectInfo> list_defects = new List<DefectInfo>();
                                        m_parent.m_global.m_database_service.ProcessDefectDataQueryResult(strQueryResult, ref list_defects, !m_parent.m_global.m_bCombineTwoMachinesAsOne);

                                        // 查询第二个机器的数据
                                        if (true == m_parent.m_global.m_bCombineTwoMachinesAsOne && strMachineName2.Length > 0)
                                        {
                                            strTableName = "details_" + strMachineName2;

                                            strTableName = strTableName.Replace("-", "_");

                                            strQuery = "SELECT * FROM " + strTableName + " WHERE product_id = '" + product.barcode + "' and set_id = '" + product.set_id + "';";

                                            // 查询数据库，获取缺陷数据
                                            strQueryResult = "";
                                            m_parent.m_global.m_mysql_ops.QueryTableData(strTableName, strQuery, ref strQueryResult, ref ts);

                                            // 处理查询结果
                                            List<DefectInfo> list_defects2 = new List<DefectInfo>();
                                            m_parent.m_global.m_database_service.ProcessDefectDataQueryResult(strQueryResult, ref list_defects2, !m_parent.m_global.m_bCombineTwoMachinesAsOne);

                                            if (list_defects2.Count > 0)
                                            {
                                                list_defects.AddRange(list_defects2);
                                            }
                                        }

                                        if (list_defects.Count > 0)
                                        {
                                            // 保存缺陷数据
                                            product.defects = list_defects;
                                        }
                                    }
                                }
                            }

                            list_trays2[0].products = list_products2;

                            m_parent.m_global.m_current_tray_info_for_Dock = list_trays2[0];

                            if(m_parent.m_global.m_bShowAIResultFromTransferStation)
                            {
                                foreach (var product in m_parent.m_global.m_current_tray_info_for_Dock.products)
                                {
                                    var barcode = product.barcode;
                                    strTableName = "transfer_aiInfo_" + strMachineName1;
                                    strTableName = strTableName.Replace("-", "_");

                                    strQuery = "SELECT * FROM " + strTableName + " WHERE barcode = '" + barcode + "';";
                                    strQueryResult = "";
                                    m_parent.m_global.m_mysql_ops.QueryTableData(strTableName, strQuery, ref strQueryResult, ref ts);

                                    var aiResult = "";
                                    m_parent.m_global.m_database_service.ProcessTransferAIInfoQueryResult(strQueryResult, ref aiResult);

                                    if (aiResult == "PASS")
                                    {
                                        product.r3 = "PASS";
                                    }
                                    else if (aiResult == "FAIL")
                                    {
                                        product.r3 = "FAIL";
                                    }
                                    else
                                    {
                                        product.r3 = "";
                                    }

                                    // 查询第二个机器的数据
                                    if (true == m_parent.m_global.m_bCombineTwoMachinesAsOne && strMachineName2.Length > 0)
                                    {
                                        strTableName = "transfer_aiInfo_" + strMachineName2;
                                        strTableName = strTableName.Replace("-", "_");

                                        strQuery = "SELECT * FROM " + strTableName + " WHERE barcode = '" + barcode + "';";
                                        strQueryResult = "";
                                        m_parent.m_global.m_mysql_ops.QueryTableData(strTableName, strQuery, ref strQueryResult, ref ts);

                                        m_parent.m_global.m_database_service.ProcessTransferAIInfoQueryResult(strQueryResult, ref aiResult);

                                        if (aiResult == "PASS")
                                        {
                                            product.r3 = "PASS";
                                        }
                                        else if (aiResult == "FAIL")
                                        {
                                            product.r3 = "FAIL";
                                        }
                                        else
                                        {
                                            product.r3 = "";
                                        }
                                    }
                                }

                            }
                            
                            // 篡改运控行列数据
                            bool bSpecialMode = false;
                            if (true == bSpecialMode)
                            {
                                if (10 == m_parent.m_global.m_current_tray_info_for_Dock.total_rows)
                                {
                                    // 把总行数改为8
                                    m_parent.m_global.m_current_tray_info_for_Dock.total_rows = 8;

                                    // 把第9行的行数改为第8行
                                    for (int i = 0; i < m_parent.m_global.m_current_tray_info_for_Dock.products.Count; i++)
                                    {
                                        if (m_parent.m_global.m_current_tray_info_for_Dock.products[i].row == 9)
                                        {
                                            m_parent.m_global.m_current_tray_info_for_Dock.products[i].row = 8;
                                        }
                                    }
                                }
                            }

                            // 处理AVI产品信息
                            try
                            {
                                int nStartTime = GetTickCount();

                                var originalProductCount = m_parent.m_global.m_current_tray_info_for_Dock.products.Count;

                                // 如果是单Pce模式，则只需要处理对应二维码的产品
                                if (m_parent.m_global.m_nRecheckDisplayMode == 1)
                                {
                                    List<ProductInfo> list_temp = new List<ProductInfo>();

                                    for (int i = 0; i < m_parent.m_global.m_current_tray_info_for_Dock.products.Count; i++)
                                    {
                                        if (m_parent.m_global.m_current_tray_info_for_Dock.products[i].barcode == strBarcode)
                                        {
                                            list_temp.Add(m_parent.m_global.m_current_tray_info_for_Dock.products[i]);
                                            break;
                                        }
                                    }

                                    m_parent.m_global.m_current_tray_info_for_Dock.products = list_temp;
                                }

                                // 处理定位失败的产品，和空穴
                                if (true)
                                {
                                    m_parent.m_global.m_current_tray_info_for_Dock.products_with_failed_positioning = new List<ProductInfo>();
                                    m_parent.m_global.m_current_tray_info_for_Dock.empty_slots = new List<ProductInfo>();

                                    for (int i = 0; i < m_parent.m_global.m_current_tray_info_for_Dock.products.Count; i++)
                                    {
                                        ProductInfo product = m_parent.m_global.m_current_tray_info_for_Dock.products[i];

                                        if (product.r10.Contains("empty_slot"))
                                        {
                                            m_parent.m_global.m_current_tray_info_for_Dock.empty_slots.Add(product);

                                            m_parent.m_global.m_current_tray_info_for_Dock.products.RemoveAt(i);

                                            i--;

                                            continue;
                                        }

                                        if (product.defects != null)
                                        {
                                            bool bHasAtLeastOneNull = false;
                                            bool bHasAtLeastOneNotNull = false;
                                            int nNullCount = 0;

                                            for (int j = 0; j < product.defects.Count; j++)
                                            {
                                                if (product.defects[j].type == "NULL")
                                                {
                                                    bHasAtLeastOneNull = true;
                                                    nNullCount++;
                                                }
                                                else
                                                {
                                                    bHasAtLeastOneNotNull = true;
                                                }
                                            }

                                            // 只要有一个type为NULL，且有一个type不为NULL，则认为定位失败
                                            if (bHasAtLeastOneNull && bHasAtLeastOneNotNull)
                                            {
                                                m_parent.m_global.m_current_tray_info_for_Dock.products_with_failed_positioning.Add(product);

                                                m_parent.m_global.m_current_tray_info_for_Dock.products.RemoveAt(i);

                                                i--;
                                            }
                                            else if (nNullCount >= 6)
                                            {
                                                // 如果nNullCount大于等于6，则认为是空穴
                                                m_parent.m_global.m_current_tray_info_for_Dock.empty_slots.Add(product);

                                                m_parent.m_global.m_current_tray_info_for_Dock.products.RemoveAt(i);

                                                i--;
                                            }
                                            else if (bHasAtLeastOneNull)
                                            {
                                                m_parent.m_global.m_current_tray_info_for_Dock.products_with_failed_positioning.Add(product);

                                                m_parent.m_global.m_current_tray_info_for_Dock.products.RemoveAt(i);

                                                i--;
                                            }
                                        }
                                    }
                                }

                                // 处理nocode产品
                                if (true)
                                {
                                    m_parent.m_global.m_current_tray_info_for_Dock.nocode_products = new List<ProductInfo>();

                                    for (int i = 0; i < m_parent.m_global.m_current_tray_info_for_Dock.products.Count; i++)
                                    {
                                        ProductInfo product = m_parent.m_global.m_current_tray_info_for_Dock.products[i];

                                        if (product.r10.Contains("nocode"))
                                        {
                                            m_parent.m_global.m_current_tray_info_for_Dock.nocode_products.Add(product);

                                            m_parent.m_global.m_current_tray_info_for_Dock.products.RemoveAt(i);

                                            i--;

                                            continue;
                                        }

                                        if (product.barcode.IndexOf("nocode", StringComparison.OrdinalIgnoreCase) >= 0)
                                        {
                                            m_parent.m_global.m_current_tray_info_for_Dock.nocode_products.Add(product);

                                            m_parent.m_global.m_current_tray_info_for_Dock.products.RemoveAt(i);

                                            i--;
                                        }
                                    }
                                }

                                // 处理缺陷为空的产品信息
                                //if (m_parent.m_global.m_strSiteCity=="苏州")
                                if (true)
                                {
                                    m_parent.m_global.m_current_tray_info_for_Dock.NotRecievedPorducts = new List<ProductInfo>();
                                    for(int i = 0; i < m_parent.m_global.m_current_tray_info_for_Dock.products.Count; i++)
                                    {
                                        ProductInfo product = m_parent.m_global.m_current_tray_info_for_Dock.products[i];

                                        if (!product.r3.Contains("PASS") && (product.defects == null || product.defects.Count == 0))
                                        {
                                            m_parent.m_global.m_current_tray_info_for_Dock.NotRecievedPorducts.Add(product);
                                            m_parent.m_global.m_current_tray_info_for_Dock.products.RemoveAt(i);

                                            i--;
                                        }
                                    }
                                }

                                // 处理MES过站失败产品信息
                                if (true)
                                {
                                    m_parent.m_global.m_current_tray_info_for_Dock.MES_failed_products = new List<ProductInfo>();

                                    for (int i = 0; i < m_parent.m_global.m_current_tray_info_for_Dock.products.Count; i++)
                                    {
                                        ProductInfo product = m_parent.m_global.m_current_tray_info_for_Dock.products[i];

                                        if (product.r10.Contains("MES_NG") || product.MES_failure_msg.Contains("MES_NG") || product.r10.Contains("AVI检测NG"))
                                        {
                                            m_parent.m_global.m_current_tray_info_for_Dock.MES_failed_products.Add(product);

                                            m_parent.m_global.m_current_tray_info_for_Dock.products.RemoveAt(i);

                                            i--;

                                            continue;
                                        }
                                    }

                                    for( int i=0;i<m_parent.m_global.m_current_tray_info_for_Dock.NotRecievedPorducts.Count;i++)
                                    {
                                        ProductInfo product = m_parent.m_global.m_current_tray_info_for_Dock.NotRecievedPorducts[i];

                                        if (product.r10.Contains("MES_NG") || product.MES_failure_msg.Contains("MES_NG") || product.r10.Contains("AVI检测NG"))
                                        {
                                            m_parent.m_global.m_current_tray_info_for_Dock.MES_failed_products.Add(product);

                                            m_parent.m_global.m_current_tray_info_for_Dock.NotRecievedPorducts.RemoveAt(i);

                                            i--;

                                            continue;
                                        }
                                    }
                                }

                                // 处理ET产品显示
                                if (true)
                                {
                                    m_parent.m_global.m_current_tray_info_for_Dock.ET_products = new List<ProductInfo>();

                                    for (int i = 0; i < m_parent.m_global.m_current_tray_info_for_Dock.products.Count; i++)
                                    {
                                        ProductInfo product = m_parent.m_global.m_current_tray_info_for_Dock.products[i];

                                        //if (product.defects == null || product.defects.Count == 0 || product.bET == true)
                                        if (product.bET == true)
                                        {
                                            if (false == product.r3.Contains("PASS"))
                                            {
                                                m_parent.m_global.m_current_tray_info_for_Dock.ET_products.Add(product);

                                                m_parent.m_global.m_current_tray_info_for_Dock.products.RemoveAt(i);

                                                i--;
                                            }
                                            else
                                            {

                                            }
                                        }
                                    }
                                }

                                var badProductCount =
                                    m_parent.m_global.m_current_tray_info_for_Dock.products_with_failed_positioning.Count +
                                    m_parent.m_global.m_current_tray_info_for_Dock.empty_slots.Count +
                                    m_parent.m_global.m_current_tray_info_for_Dock.nocode_products.Count +
                                    m_parent.m_global.m_current_tray_info_for_Dock.MES_failed_products.Count +
                                    m_parent.m_global.m_current_tray_info_for_Dock.ET_products.Count;
                                var messageHasShown = false;

                                if (m_parent.m_global.m_current_tray_info_for_Dock.MES_failed_products.Count == originalProductCount)
                                {
                                    messageHasShown = true;
                                    MessageBox.Show(m_parent, "所有产品均为MES校验异常产品，本盘产品无需提交MES，请联系产线技术员排查MES校验异常", "处理产品信息");
                                }

                                if (!messageHasShown && badProductCount == originalProductCount)
                                {
                                    MessageBox.Show(m_parent, "所有产品均为异常（定位失败、无码、空穴、MES校验失败）产品，请联系AVI视觉工程师排查异常", "处理产品信息");
                                }

                                // 处理OK产品显示
                                if (true)
                                {
                                    m_parent.m_global.m_current_tray_info_for_Dock.OK_products = new List<ProductInfo>();

                                    for (int i = 0; i < m_parent.m_global.m_current_tray_info_for_Dock.products.Count; i++)
                                    {
                                        ProductInfo product = m_parent.m_global.m_current_tray_info_for_Dock.products[i];

                                        if (true == product.r3.Contains("PASS") || product.is_ok_product == true)
                                        {
                                            m_parent.m_global.m_current_tray_info_for_Dock.OK_products.Add(product);

                                            m_parent.m_global.m_current_tray_info_for_Dock.products.RemoveAt(i);

                                            i--;
                                        }
                                        else
                                        {

                                        }
                                    }
                                }

                                // 处理不可复判项
                                if (true)
                                {
                                    // 初始化不可复判缺陷产品列表
                                    m_parent.m_global.m_current_tray_info_for_Dock.products_with_unrecheckable_defect = new List<ProductInfo>();

                                    // 遍历当前托盘中的所有产品
                                    for (int i = 0; i < m_parent.m_global.m_current_tray_info_for_Dock.products.Count; i++)
                                    {
                                        ProductInfo product = m_parent.m_global.m_current_tray_info_for_Dock.products[i];

                                        // 遍历产品的所有缺陷
                                        for (int j = 0; j < product.defects.Count; j++)
                                        {
                                            bool bIsUnrecheckable = false;

                                            // 遍历不可复判缺陷类型列表
                                            for (int k = 0; k < m_parent.m_global.m_uncheckable_defect_types.Count; k++)
                                            {
                                                // 如果不可复判缺陷类型是启用的
                                                if (m_parent.m_global.m_uncheckable_defect_enable_flags[k] == true)
                                                {
                                                    // 如果产品的缺陷类型与当前不可复判缺陷类型匹配
                                                    if (product.defects[j].type == m_parent.m_global.m_uncheckable_defect_types[k])
                                                    {
                                                        // 如果产品子类型为 "glue_check" 并且设置为不忽略其它缺陷
                                                        if (m_parent.m_global.m_strProductSubType == "glue_check" && m_parent.m_global.m_bIgnoreOtherDefectsIfOneUnrecheckableItemExists == false)
                                                        {
                                                            // 仅移除不可复判项
                                                            product.defects.RemoveAt(j);

                                                            // 调整索引，防止跳过下一个缺陷
                                                            j--;
                                                        }
                                                        else
                                                        {
                                                            // 将带有不可复判缺陷的产品添加到列表中
                                                            m_parent.m_global.m_current_tray_info_for_Dock.products_with_unrecheckable_defect.Add(product);

                                                            // 因不可复判项需要点击，在此不从products中去除
                                                            // 从当前托盘中移除该产品
                                                            //m_parent.m_global.m_current_tray_info_for_Dock.products.RemoveAt(i);

                                                            // 调整索引，防止跳过下一个产品
                                                            //i--;

                                                            // 标记为不可复判
                                                            bIsUnrecheckable = true;

                                                            break;
                                                        }
                                                    }
                                                }
                                            }

                                            // 如果产品带有不可复判缺陷，跳出内层循环
                                            if (true == bIsUnrecheckable)
                                                break;
                                        }
                                    }
                                }

                                // 处理检胶项目的OK位置
                                if (m_parent.m_global.m_strProductSubType == "glue_check")
                                {
                                    for (int i = 0; i < m_parent.m_global.m_current_tray_info_for_Dock.products.Count; i++)
                                    {
                                        ProductInfo product = m_parent.m_global.m_current_tray_info_for_Dock.products[i];

                                        // 遍历缺陷列表
                                        if (product.defects == null)
                                        {
                                            // 提示框
                                            MessageBox.Show("产品缺陷列表为空，无法处理检胶项目。", "错误", MessageBoxButton.OK, MessageBoxImage.Error);

                                            continue;
                                        }

                                        var ngLightChannelList = new List<int>();

                                        for (int j = 0; j < product.defects.Count; j++)
                                        {
                                            // 如果缺陷类型为空白字符串，则将其添加到检胶项目的OK位置列表中
                                            if (true == string.IsNullOrEmpty(product.defects[j].type))
                                            //if (true == string.IsNullOrEmpty(product.defects[j].type) && !ngLightChannelList.Contains(product.defects[j].light_channel))
                                            {
                                                if (null == product.defects_with_NULL_or_empty_type_name)
                                                    product.defects_with_NULL_or_empty_type_name = new List<DefectInfo>();

                                                // 将缺陷添加到检胶项目的OK位置列表中
                                                product.defects_with_NULL_or_empty_type_name.Add(product.defects[j]);

                                                // 从当前产品的缺陷列表中移除该缺陷
                                                product.defects.RemoveAt(j);

                                                // 调整索引，防止跳过下一个缺陷
                                                j--;
                                            }
                                            else
                                            {
                                                ngLightChannelList.Add(product.defects[j].light_channel);
                                            }
                                        }

                                        for (int n = 0; n < product.defects_with_NULL_or_empty_type_name.Count; n++)
                                        {
                                            foreach (var c in ngLightChannelList)
                                            {
                                                if (product.defects_with_NULL_or_empty_type_name[n].light_channel == c)
                                                {
                                                    product.defects_with_NULL_or_empty_type_name.RemoveAt(n);
                                                    n--;

                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    // 复判记录反查功能
                                    if (false)
                                    {
                                        // 检查提交记录中是否有该盘记录
                                        strQueryResult = "";
                                        strQuery = "SELECT * FROM RecheckRecord WHERE SetID = '" + list_products[0].set_id + "';";

                                        m_parent.m_global.m_mysql_ops.QueryTableData(strTableName, strQuery, ref strQueryResult, ref ts);
                                        m_parent.m_global.m_database_service.ProcessRecheckRecordQueryResult(strQueryResult, ref m_parent.m_global.m_current_MES_tray_info);

                                        if (m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Nova != null ||
                                            m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Dock != null)
                                        {
                                            if (m_parent.m_global.m_strProductType == "dock")
                                            {
                                                m_parent.m_global.m_log_presenter.Log($"查询到SetID为{m_parent.m_global.m_background_tray_info_for_Dock.set_id}的复判记录，上次复判结果已通过按钮颜色显示");
                                                MessageBox.Show($"查询到SetID为{m_parent.m_global.m_background_tray_info_for_Dock.set_id}的复判记录，上次复判结果已通过按钮颜色显示");
                                                m_parent.m_global.m_strCurrentClientMachineName = m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Dock.machine_id;

                                                // 查询后更新UI，恢复上次复判结果的按钮颜色
                                                int nTrayRows = m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Dock.total_rows;
                                                int nTrayCols = m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Dock.total_columns;

                                                m_parent.m_global.m_strLastOpenImagePath = "";
                                                m_parent.page_HomeView.m_panel_canvas.set_panel_rows_and_columns(nTrayRows, nTrayCols);
                                                m_parent.page_HomeView.m_panel_canvas.m_bForceRedraw = true;
                                                m_parent.page_HomeView.m_panel_canvas.OnWindowSizeChanged(new object(), new EventArgs());

                                                // 创建按钮
                                                m_parent.page_HomeView.GenerateCircularButtonsInGridContainer(m_parent.page_HomeView.grid_CircularButtonContainer,
                                                    nTrayRows, nTrayCols, m_parent.m_global.m_current_MES_tray_info.m_check_flags_by_product);

                                                // 更新产品信息
                                                m_parent.page_HomeView.textblock_MachineID.Text = m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Dock.machine_id;
                                                m_parent.page_HomeView.textblock_OperatorID.Text = m_parent.m_global.m_strCurrentOperatorID;
                                                m_parent.m_global.m_recheck_statistics.Save();
                                                m_parent.page_HomeView.textblock_TotalPanels.Text = m_parent.m_global.m_recheck_statistics.m_nTotalPanels.ToString();

                                                m_parent.page_HomeView.render_tray_button_color();

                                                m_parent.m_global.m_bIsQueryingDatabaseAndCloningImages = false;
                                            }

                                            return;
                                        }
                                        else
                                        {
                                            m_parent.m_global.m_log_presenter.Log("未查询到过往复判记录");
                                        }
                                    }
                                }

                                // 清空二维码输入框
                                textbox_ScannedBarcode.Text = "";

                                // 处理AVI数据
                                if (true == m_parent.m_global.m_tray_info_service.HandleTrayInfoFromAVIForDockOrCL5_DatabaseAndImagePathStorage(m_parent.m_global.m_current_tray_info_for_Dock, false))
                                {
                                    //await Task.Delay(500);
                                    //Thread.Sleep(1500);

                                    for (int i = 0; i < 50; i++)
                                    {
                                        if (m_parent.m_global.m_bIsDeletingImages == false)
                                            break;

                                        Thread.Sleep(100);
                                    }

                                    m_parent.m_global.m_tray_info_service.HandleTrayInfoFromAVIForDockOrCL5_InitFlagsAndShowImage(m_parent.m_global.m_current_tray_info_for_Dock, strBarcode);
                                }

                                int nEndTime = GetTickCount();

                                m_parent.m_global.m_log_presenter.Log(string.Format("接收处理AVI数据耗时：{0:0.000}秒", (double)(nEndTime - nStartTime) / 1000.0f));

                                // 处理ET产品显示
                                if (true)
                                {
                                    for (int i = 0; i < m_parent.m_global.m_current_tray_info_for_Dock.ET_products.Count; i++)
                                    {
                                        ProductInfo product = m_parent.m_global.m_current_tray_info_for_Dock.ET_products[i];

                                        int nColumns = m_parent.m_global.m_current_tray_info_for_Dock.total_columns;
                                        int nRows = m_parent.m_global.m_current_tray_info_for_Dock.total_rows;

                                        int col = product.column - 1;
                                        int row = product.row - 1;

                                        int nIndex = row * nColumns + col;

                                        if (nIndex >= m_parent.m_global.m_current_MES_tray_info.m_check_flags_by_product.Length || nIndex >= m_product_buttons.Length)
                                            continue;

                                        if (product.defects == null || product.defects.Count == 0)
                                        {
                                            m_parent.m_global.m_current_MES_tray_info.m_check_flags_by_product[nIndex] = RecheckResult.EMPTY_DEFECT;

                                            m_product_buttons[nIndex].Background = System.Windows.Media.Brushes.Black;
                                            m_product_buttons[nIndex].Foreground = System.Windows.Media.Brushes.White;
                                            m_product_buttons[nIndex].Content = "空";
                                        }

                                        if (product.bET == true)
                                        {
                                            m_parent.m_global.m_current_MES_tray_info.m_check_flags_by_product[nIndex] = RecheckResult.ET;

                                            m_product_buttons[nIndex].Background = System.Windows.Media.Brushes.Black;
                                            m_product_buttons[nIndex].Foreground = System.Windows.Media.Brushes.White;
                                            m_product_buttons[nIndex].Content = "ET";
                                        }
                                    }
                                }

                                // 将需要提交AI的产品加入queue
                                if (m_parent.m_global.m_nRecheckModeWithAISystem != 1)
                                {
                                    m_parent.m_global.m_tray_info_service.EnqueueProductInfosWaitSubmittoAI(m_parent.m_global.m_current_tray_info_for_Dock);
                                }

                                m_bIsValidatingWithMES = true;

                                // MES校验
                                if (m_parent.m_global.m_bEnableMESValidationForOriginalNGProducts == true)
                                {
                                    (new Thread(thread_validate_original_NG_product_data_with_MES)).Start(strBarcode);
                                }

                                // 判断AI复判模式，如果是纯AI复判模式，则直接提交到AI系统
                                if (m_parent.m_global.m_nRecheckModeWithAISystem == 3)
                                {
                                    (new Thread(thread_AI_recheck_submit)).Start();
                                }
                            }
                            catch (Exception ex)
                            {
                                m_parent.m_global.m_log_presenter.Log(string.Format("处理AVI产品信息失败。错误信息：{0}", ex.Message));
                            }
                        }
                    }
                }
            }

            m_parent.m_global.m_bIsQueryingDatabaseAndCloningImages = false;
        }

        // 按钮点击事件：提交MES
        private void BtnSubmit_Click(object sender, RoutedEventArgs e)
        {
            if (m_parent.m_global.m_bIsQueryingDatabaseAndCloningImages == true)
            {
				// 提示正在进行其他操作，请稍后再试
				if (m_parent.m_global.m_nRecheckModeWithAISystem != 3)
				{
					MessageBox.Show("正在进行其他UI操作，请稍后再试", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
				}
				else
				{
					m_parent.m_global.m_log_presenter.LogError("正在进行其他UI操作，请稍后再试");
				}

                m_bIsAIRechecking = -1;

                return;
            }

            if (m_parent.m_global.m_strProductType == "dock")
            {
                if (null == m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Dock.set_id)
                {
                    if (m_parent.m_global.m_nRecheckModeWithAISystem != 3)
                    {
                        MessageBox.Show("产品信息为空，无法提交！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else
                    {
                        m_parent.m_global.m_log_presenter.LogError("产品信息为空，无法提交！");
                    }

                    m_bIsAIRechecking = -1;

                    return;
                }
            }

            // 检查登录信息，如果未登录，则提示登录
            if (string.IsNullOrEmpty(m_parent.m_global.m_strCurrentOperatorID))
            {
                MessageBox.Show("请先登录，否则无法提交！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // NG二次确认
            if (m_parent.m_global.m_bSecondConfirmNGProductsBeforeSubmittingToMES == true)
            {
                SecondConfirmNGProducts second_confirm_window = new SecondConfirmNGProducts(m_parent);

                // 构建待确认的NG产品二维码列表
                List<string> list_NG_barcodes = new List<string>();

                if (m_parent.m_global.m_strProductType == "nova" && m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Nova != null)
                {
                    for (int i = 0; i < m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Nova.Products.Count; i++)
                    {
                        if (m_parent.m_global.m_current_MES_tray_info.m_check_flags_by_product[i] == RecheckResult.NG)
                        {
                            list_NG_barcodes.Add(m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Nova.Products[i].BarCode);
                        }
                    }
                }
                else if (m_parent.m_global.m_strProductType == "dock" && m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Dock != null)
                {
                    for (int i = 0; i < m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Dock.products.Count; i++)
                    {
                        int nColumns = m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Dock.total_columns;
                        int nRows = m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Dock.total_rows;

                        int col = m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Dock.products[i].column - 1;
                        int row = m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Dock.products[i].row - 1;

                        int nIndex = row * nColumns + col;
                        if (nIndex >= m_parent.m_global.m_current_MES_tray_info.m_check_flags_by_product.Length)
                            continue;

                        if (m_parent.m_global.m_current_MES_tray_info.m_check_flags_by_product[nIndex] == RecheckResult.NG)
                        {
                            list_NG_barcodes.Add(m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Dock.products[i].barcode);
                        }
                    }
                }

                // 如果有NG产品，弹出二次确认窗口
                if (list_NG_barcodes.Count > 0)
                {
                    second_confirm_window.m_list_NG_barcodes = list_NG_barcodes;

                    second_confirm_window.ShowDialog();

                    // 如果未全部成功确认，则弹出提示框，不允许提交
                    if (second_confirm_window.m_bIsAllNGProductsConfirmed == false)
                    {
                        m_parent.m_global.m_log_presenter.LogError("NG产品未全部成功确认，无法提交！请检查原因。");

                        MessageBox.Show("NG产品未全部成功确认，无法提交！请检查原因。\r\n如果无法解决该问题，但一定要提交，可以在设置中更改二次确认选项开关。", "错误", MessageBoxButton.OK, MessageBoxImage.Error);

                        return;
                    }
                }
            }

            try
            {
                bool bIsAllChecked = true;

                if (m_parent.m_global.m_strProductType == "nova")
                {
                    if (null == m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Nova)
                    {
                        m_parent.m_global.m_log_presenter.LogError("产品信息为空，无法提交！");

                        MessageBox.Show("产品信息为空，无法提交！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // 检查是否所有产品都已经检查
                    for (int i = 0; i < m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Nova.Products.Count; i++)
                    {
                        if (m_parent.m_global.m_current_MES_tray_info.m_check_flags_by_product[i] == RecheckResult.NotChecked)
                        {
                            bIsAllChecked = false;
                            break;
                        }
                    }
                }
                else if (m_parent.m_global.m_strProductType == "dock")
                {
                    if (null == m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Dock)
                    {
						if (m_parent.m_global.m_nRecheckModeWithAISystem != 3)
						{
                            m_parent.m_global.m_log_presenter.LogError("产品信息为空，无法提交！");

							MessageBox.Show("产品信息为空，无法提交！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
						}
						else
						{
							m_parent.m_global.m_log_presenter.LogError("产品信息为空，无法提交！");
						}

                        m_bIsAIRechecking = -1;

                        return;
                    }

                    // 检查是否所有产品都已经检查
                    for (int i = 0; i < m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Dock.products.Count; i++)
                    {
                        int nColumns = m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Dock.total_columns;
                        int nRows = m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Dock.total_rows;

                        int col = m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Dock.products[i].column - 1;
                        int row = m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Dock.products[i].row - 1;

                        int nIndex = row * nColumns + col;
                        if (nIndex >= m_parent.m_global.m_current_MES_tray_info.m_check_flags_by_product.Length)
                            continue;

                        if (m_parent.m_global.m_current_MES_tray_info.m_check_flags_by_product[nIndex] == RecheckResult.NotChecked)
                        {
                            if (m_parent.m_global.m_current_MES_tray_info.m_check_flags_by_product[nIndex] != RecheckResult.NoCode)
                            {
                                bIsAllChecked = false;
                                //break;
                            }
                        }
                    }
                }

                if (false == bIsAllChecked)
                {
                    m_bIsAIRechecking = -1;

                    m_parent.m_global.m_log_presenter.LogError("还有产品未复判确认，无法提交！");

                    if (m_parent.m_global.m_nRecheckModeWithAISystem != 3)
                    {
                        MessageBox.Show("还有产品未复判确认，无法提交！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    }

                    return;
                }

                // 提交数据到MES
                MesResponse response = new MesResponse();
                List<string> list_barcodes_to_ignore = new List<string>();

                // 缺陷误报统计
                List<DataModels.DefectCount> defectCounts = GetDefectCounts();

                if (true == m_parent.m_global.m_MES_service.SubmitPanelRecheckResults(m_parent.m_global.m_current_MES_tray_info, ref response, list_barcodes_to_ignore, defectCounts))
                {
                    m_parent.m_global.m_log_presenter.Log("提交MES成功，服务器返回内容：" + JsonConvert.SerializeObject(response));
                    m_parent.m_global.m_log_presenter.Log("提交MES成功，服务器返回内容：" + JsonConvert.SerializeObject(response.Message));

                    ClearProductInfo();

                    GenerateCircularButtonsInGridContainer(grid_CircularButtonContainer, 0, 0, null);

                    // 更新统计信息
                    var listpro = m_parent.m_global.m_current_MES_tray_info.m_check_flags_by_product.ToList();
                    m_parent.m_global.m_recheck_statistics.m_confirmOKs += listpro.Where(x => x == RecheckResult.OK).Count();
                    m_parent.m_global.m_recheck_statistics.m_confirmNGs += listpro.Where(x => x == RecheckResult.NG).Count();

                    //var listdefects = m_parent.m_global.m_current_MES_tray_info.m_list_recheck_flags_by_defect;

                    //foreach (var defects in listdefects)
                    //{
                    //    var list = defects.ToList();
                    //    m_parent.m_global.m_recheck_statistics.m_nRecheckedOKs += list.Where(x => x == RecheckResult.OK).Count();
                    //    m_parent.m_global.m_recheck_statistics.m_nRecheckedNGs += list.Where(x => x == RecheckResult.NG).Count();
                    //}

                    //changedcount();

                    m_parent.m_global.m_recheck_statistics.Save();

                    m_parent.m_global.m_recheck_statistics.m_nTotalPanels++;

                    if (m_parent.m_global.m_strProductType != "nova")
                    {
                        //var task = AsyncUpdateStatistics();
                        m_parent.m_global.m_defectStatisticsQuery.TriggerSynchronizeUI();
                    }
                    UpdateStatistics();

                    // 提交成功后，更新数据表的是否已经复判的字段
                    if (m_parent.m_global.m_strProductSubType == "glue_check" || m_parent.m_global.m_recheck_data_query_mode == RecheckDataQueryMode.ByQueryUncheckedFlag)
                    {
                        // 查询耗时
                        TimeSpan ts = new TimeSpan(0, 0, 0);
                        for (int i = 0; i < m_parent.m_global.m_dict_machine_names_and_IPs.Count; i++)
                        {
                            string strMachineName = m_parent.m_global.m_dict_machine_names_and_IPs.Keys.ElementAt(i);
                            string strTrayTable = "trays_" + strMachineName;
                            strTrayTable = strTrayTable.Replace("-", "_");

                            string strSetID = m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Dock.set_id;
                            string strQuery = string.Format("UPDATE {0} SET r1 = 'been_checked' WHERE set_id = '{1}';", strTrayTable, strSetID);

                            string strQueryResult = "";
                            m_parent.m_global.m_mysql_ops.QueryTableData(strTrayTable, strQuery, ref strQueryResult, ref ts);
                        }
                    }

                    // 提交成功后，给所有复判站发送料盘已复判完成并且提交MES成功的消息
                    if (m_parent.m_global.m_strProductSubType == "glue_check")
                    {
                        TrayRecheckedAndSubmittedEventTelegram message = new TrayRecheckedAndSubmittedEventTelegram();

                        message.set_id = m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Dock.set_id;
                        message.table_name = m_parent.m_global.m_strCurrentTrayTableName;

                        string json = JsonConvert.SerializeObject(message);

                        for (int i = 0; i < m_parent.m_global.m_dict_recheck_station_names_and_IPs.Count; i++)
                        {
                            string strStation = m_parent.m_global.m_dict_recheck_station_names_and_IPs.ElementAt(i).Key;
                            string strIP = m_parent.m_global.m_dict_recheck_station_names_and_IPs.ElementAt(i).Value;

                            // 把通配符改为具体的IP地址
                            string strURL = m_parent.m_global.m_http_service_url_of_tray_recheck_data_mes_submit_event.Replace("*", strIP);

                            string strResponse = "";
                            if (true == m_parent.m_global.m_MES_service.SendMES(strURL, json, ref strResponse, 1, 5))
                            {
                                m_parent.m_global.m_log_presenter.Log(string.Format("向复判站{0}发送料盘已复判完成并且提交MES成功的消息成功，接收IP地址：{1}。", strStation, strIP));
                            }
                            else
                            {
                                m_parent.m_global.m_log_presenter.LogError(string.Format("向复判站{0}发送料盘已复判完成的消息失败，接收IP地址：{1}。", strStation, strIP));
                            }
                        }
                    }

                    // 是否将复判结果发送给FQC看图电脑
                    if (m_parent.m_global.m_bSendRecheckResultToFQCStations && m_parent.m_global.m_strProductType == "nova")
                    {
                        // 发送复判结果给FQC看图电脑
                        for (int i = 0; i < m_parent.m_global.m_dict_FQC_station_names_and_IPs.Count; i++)
                        {
                            StationIPAndPort station_info = m_parent.m_global.m_dict_FQC_station_names_and_IPs.ElementAt(i).Value;

                            string strIP = station_info.m_strIP;
                            int nPort = station_info.m_nPort;

                            string strURL = string.Format("http://{0}:{1}/fqc/recheck_result_submitted/", strIP, nPort);

                            string json = JsonConvert.SerializeObject(m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Nova);

                            string strResponse = "";
                            if (true == m_parent.m_global.m_MES_service.SendMES(strURL, json, ref strResponse, 1, 5))
                            {
                                m_parent.m_global.m_log_presenter.Log(string.Format("向FQC看图电脑{0}发送复判结果成功，接收地址：{1}。", m_parent.m_global.m_dict_FQC_station_names_and_IPs.ElementAt(i).Key, strURL));
                            }
                            else
                            {
                                m_parent.m_global.m_log_presenter.Log(string.Format("向FQC看图电脑{0}发送复判结果失败，接收地址：{1}。", m_parent.m_global.m_dict_FQC_station_names_and_IPs.ElementAt(i).Key, strURL));
                            }
                        }
                    }

                    if (m_parent.m_global.m_strProductType == "nova")
                    {
                        MessageBox.Show("提交MES成功，服务器返回内容：" + JsonConvert.SerializeObject(response.Message), "提示", MessageBoxButton.OK);
                    }

                    textbox_ScannedBarcode.Focus();
                    textbox_ScannedBarcode.CaretIndex = textbox_ScannedBarcode.Text.Length;

                    m_bIsAIRechecking = 0;
                }
                else
                {
                    m_parent.m_global.m_log_presenter.Log("提交MES失败：" + JsonConvert.SerializeObject(response));

                    if (m_parent.m_global.m_nRecheckModeWithAISystem != 3)
                    {
                        MessageBox.Show("提交MES失败！" + JsonConvert.SerializeObject(response.Message), "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
					else
					{
						m_parent.m_global.m_log_presenter.LogError("提交MES失败！" + JsonConvert.SerializeObject(response.Message));
					}

                    if (response.Result == "-2" || response.Result == "-1")
                    {
                        List<string> ETProducts = new List<string>();
                        List<string> OverpassProducts = new List<string>();

                        string strInfo = string.Format("MES返回异常错误信息，请确认是否忽略带错误信息的二维码产品，再次提交？\r\nMES返回错误信息为：{0}", JsonConvert.SerializeObject(response.Message));

                        for (int i = 0; i < response.Message.Count; i++)
                        {
                            if (response.Message[i].Contains("ET") ||
                               (response.Message[i].Contains("对应区域为") && !response.Message[i].Contains("FQC")))
                            {
                                string[] parts = response.Message[i].Split('*');
                                if (parts.Length > 0)
                                {
                                    ETProducts.Add(parts[0]);
                                }
                            }

                            if (response.Message[i].Contains("FQC") || response.Message[i].Contains("PACKING"))
                            {
                                string[] parts = response.Message[i].Split('*');
                                if (parts.Length > 0)
                                {
                                    OverpassProducts.Add(parts[0]);
                                }
                            }
                        }

                        // 提示用户是否忽略这些产品
                        if (m_parent.m_global.m_nRecheckModeWithAISystem == 3 || (ETProducts.Count > 0 && MessageBox.Show(strInfo, "提示", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes))
                        {
                            // 忽略ET产品
                            foreach (var barcode in ETProducts)
                            {
                                list_barcodes_to_ignore.Add(barcode);

                                if (m_parent.m_global.m_strProductType == "nova")
                                {
                                    for (int i = 0; i < m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Nova.Products.Count; i++)
                                    {
                                        int nColumns = m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Nova.Col;
                                        int nRows = m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Nova.Row;

                                        int col = m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Nova.Products[i].PosCol - 1;
                                        int row = m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Nova.Products[i].PosRow - 1;

                                        int nIndex = row * nColumns + col;

                                        if (m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Nova.Products[i].BarCode == barcode)
                                        {
                                            m_parent.m_global.m_current_MES_tray_info.m_check_flags_by_product[nIndex] = RecheckResult.ET;

                                            m_product_buttons[nIndex].Background = System.Windows.Media.Brushes.Black;
                                            m_product_buttons[nIndex].Foreground = System.Windows.Media.Brushes.White;
                                            m_product_buttons[nIndex].Content = "ET";
                                            break;
                                        }
                                    }
                                }
                                else if (m_parent.m_global.m_strProductType == "dock")
                                {
                                    for (int i = 0; i < m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Dock.products.Count; i++)
                                    {
                                        int nColumns = m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Dock.total_columns;
                                        int nRows = m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Dock.total_rows;

                                        int col = m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Dock.products[i].column - 1;
                                        int row = m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Dock.products[i].row - 1;

                                        int nIndex = row * nColumns + col;

                                        if (nIndex >= m_parent.m_global.m_current_MES_tray_info.m_check_flags_by_product.Length)
                                            continue;

                                        if (m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Dock.products[i].barcode == barcode)
                                        {
                                            m_parent.m_global.m_current_MES_tray_info.m_check_flags_by_product[nIndex] = RecheckResult.ET;

                                            m_product_buttons[nIndex].Background = System.Windows.Media.Brushes.Black;
                                            m_product_buttons[nIndex].Foreground = System.Windows.Media.Brushes.White;
                                            m_product_buttons[nIndex].Content = "ET";
                                            break;
                                        }
                                    }

                                    for (int i = 0; i < m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Dock.OK_products.Count; i++)
                                    {
                                        int nColumns = m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Dock.total_columns;
                                        int nRows = m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Dock.total_rows;

                                        int col = m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Dock.OK_products[i].column - 1;
                                        int row = m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Dock.OK_products[i].row - 1;

                                        int nIndex = row * nColumns + col;

                                        if (nIndex >= m_parent.m_global.m_current_MES_tray_info.m_check_flags_by_product.Length)
                                            continue;

                                        if (m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Dock.OK_products[i].barcode == barcode)
                                        {
                                            m_parent.m_global.m_current_MES_tray_info.m_check_flags_by_product[nIndex] = RecheckResult.ET;

                                            m_product_buttons[nIndex].Background = System.Windows.Media.Brushes.Black;
                                            m_product_buttons[nIndex].Foreground = System.Windows.Media.Brushes.White;
                                            m_product_buttons[nIndex].Content = "ET";
                                            break;
                                        }
                                    }
                                }
                            }

                            foreach (var barcode in OverpassProducts)
                            {
                                list_barcodes_to_ignore.Add(barcode);

                                if (m_parent.m_global.m_strProductType == "nova")
                                {
                                    for (int i = 0; i < m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Nova.Products.Count; i++)
                                    {
                                        int nColumns = m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Nova.Col;
                                        int nRows = m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Nova.Row;

                                        int col = m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Nova.Products[i].PosCol - 1;
                                        int row = m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Nova.Products[i].PosRow - 1;

                                        int nIndex = row * nColumns + col;

                                        if (m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Nova.Products[i].BarCode == barcode)
                                        {
                                            m_product_buttons[i].Background = System.Windows.Media.Brushes.DarkOrange;
                                            m_product_buttons[nIndex].Content = "已过站";
                                            break;
                                        }
                                    }
                                }
                            }

                            // 再次提交
                            if (true == m_parent.m_global.m_MES_service.SubmitPanelRecheckResults(m_parent.m_global.m_current_MES_tray_info, ref response, list_barcodes_to_ignore, defectCounts))
                            {
                                m_parent.m_global.m_log_presenter.Log("再次提交MES成功，服务器返回内容：" + JsonConvert.SerializeObject(response));

                                if (m_parent.m_global.m_nRecheckModeWithAISystem != 3)
                                {
                                    MessageBox.Show("提交MES成功，服务器返回内容：" + JsonConvert.SerializeObject(response.Message), "提示", MessageBoxButton.OK);
                                }
								else
								{
									m_parent.m_global.m_log_presenter.LogError("提交MES成功，服务器返回内容：" + JsonConvert.SerializeObject(response.Message));
								}

                                ClearProductInfo();

                                // 更新统计信息
                                var listpro = m_parent.m_global.m_current_MES_tray_info.m_check_flags_by_product.ToList();
                                m_parent.m_global.m_recheck_statistics.m_confirmOKs += listpro.Where(x => x == RecheckResult.OK).Count();
                                m_parent.m_global.m_recheck_statistics.m_confirmNGs += listpro.Where(x => x == RecheckResult.NG).Count();

                                //var listdefects = m_parent.m_global.m_current_MES_tray_info.m_list_recheck_flags_by_defect;
                                //foreach (var defects in listdefects)
                                //{
                                //    var list = defects.ToList();
                                //    m_parent.m_global.m_recheck_statistics.m_nRecheckedOKs += list.Where(x => x == RecheckResult.OK).Count();
                                //    m_parent.m_global.m_recheck_statistics.m_nRecheckedNGs += list.Where(x => x == RecheckResult.NG).Count();
                                //}

                                //changedcount();

                                m_parent.m_global.m_recheck_statistics.Save();

                                m_parent.m_global.m_recheck_statistics.m_nTotalPanels++;

                                if (m_parent.m_global.m_strProductType != "nova")
                                {
                                    m_parent.m_global.m_defectStatisticsQuery.TriggerSynchronizeUI();
                                    //var task = AsyncUpdateStatistics();
                                }
                                UpdateStatistics();

                                // 提交成功后，更新数据表的是否已经复判的字段
                                if (m_parent.m_global.m_strProductSubType == "glue_check" || m_parent.m_global.m_recheck_data_query_mode == RecheckDataQueryMode.ByQueryUncheckedFlag)
                                {
                                    // 查询耗时
                                    TimeSpan ts = new TimeSpan(0, 0, 0);

                                    string strSetID = m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Dock.set_id;
                                    string strQuery = string.Format("UPDATE {0} SET r1 = 'been_checked' WHERE set_id = '{1}';", m_parent.m_global.m_strCurrentTrayTableName, strSetID);

                                    string strQueryResult = "";
                                    m_parent.m_global.m_mysql_ops.QueryTableData(m_parent.m_global.m_strCurrentTrayTableName, strQuery, ref strQueryResult, ref ts);
                                }

                                // 提交成功后，给所有复判站发送料盘已复判完成并且提交MES成功的消息
                                if (m_parent.m_global.m_strProductSubType == "glue_check")
                                {
                                    TrayRecheckedAndSubmittedEventTelegram message = new TrayRecheckedAndSubmittedEventTelegram();

                                    message.set_id = m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Dock.set_id;
                                    message.table_name = m_parent.m_global.m_strCurrentTrayTableName;

                                    string json = JsonConvert.SerializeObject(message);

                                    for (int i = 0; i < m_parent.m_global.m_dict_recheck_station_names_and_IPs.Count; i++)
                                    {
                                        string strStation = m_parent.m_global.m_dict_recheck_station_names_and_IPs.ElementAt(i).Key;
                                        string strIP = m_parent.m_global.m_dict_recheck_station_names_and_IPs.ElementAt(i).Value;

                                        // 把通配符改为具体的IP地址
                                        string strURL = m_parent.m_global.m_http_service_url_of_tray_recheck_data_mes_submit_event.Replace("*", strIP);

                                        string strResponse = "";
                                        if (true == m_parent.m_global.m_MES_service.SendMES(strURL, json, ref strResponse, 1, 5))
                                        {
                                            m_parent.m_global.m_log_presenter.Log(string.Format("向复判站{0}发送料盘已复判完成并且提交MES成功的消息成功，接收IP地址：{1}。", strStation, strIP));
                                        }
                                        else
                                        {
                                            m_parent.m_global.m_log_presenter.Log(string.Format("向复判站{0}发送料盘已复判完成的消息失败，接收IP地址：{1}。", strStation, strIP));
                                        }
                                    }
                                }

                                // 是否将复判结果发送给FQC看图电脑
                                if (m_parent.m_global.m_bSendRecheckResultToFQCStations && m_parent.m_global.m_strProductType == "nova")
                                {
                                    // 发送复判结果给FQC看图电脑
                                    for (int i = 0; i < m_parent.m_global.m_dict_FQC_station_names_and_IPs.Count; i++)
                                    {
                                        StationIPAndPort station_info = m_parent.m_global.m_dict_FQC_station_names_and_IPs.ElementAt(i).Value;

                                        string strIP = station_info.m_strIP;
                                        int nPort = station_info.m_nPort;

                                        string strURL = string.Format("http://{0}:{1}/fqc/recheck_result_submitted/", strIP, nPort);

                                        string json = JsonConvert.SerializeObject(m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Nova);

                                        string strResponse = "";
                                        if (true == m_parent.m_global.m_MES_service.SendMES(strURL, json, ref strResponse, 1, 5))
                                        {
                                            m_parent.m_global.m_log_presenter.Log(string.Format("向FQC看图电脑{0}发送复判结果成功，接收地址：{1}。", m_parent.m_global.m_dict_FQC_station_names_and_IPs.ElementAt(i).Key, strURL));
                                        }
                                        else
                                        {
                                            m_parent.m_global.m_log_presenter.Log(string.Format("向FQC看图电脑{0}发送复判结果失败，接收地址：{1}。", m_parent.m_global.m_dict_FQC_station_names_and_IPs.ElementAt(i).Key, strURL));
                                        }
                                    }
                                }

                                textbox_ScannedBarcode.Focus();
                                textbox_ScannedBarcode.CaretIndex = textbox_ScannedBarcode.Text.Length;

                                m_bIsAIRechecking = 0;
                            }
                            else
                            {
                                m_parent.m_global.m_log_presenter.Log("再次提交MES失败：" + JsonConvert.SerializeObject(response));

                                if (response.Message != null && response.Message[0].Contains("汇总信息为空"))
                                {
                                    m_bIsAIRechecking = 0;
                                }
                                else
                                {
                                    m_bIsAIRechecking = -1;
                                }
                            }
                        }
                        else
                        {
                            m_bIsAIRechecking = 0;
                        }
                    }
                    else
                    {
                        // 如果料盘含有不可复叛项，则提交时会从products中去除，如果不重新扫码提交会导致products越界
                        if (m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Dock?.products_with_unrecheckable_defect.Count > 0
                            || m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Nova?.products_with_unrecheckable_defect.Count > 0)
                        {
                            MessageBox.Show("请重新扫码并复判提交！");
                            ClearProductInfo();

                            GenerateCircularButtonsInGridContainer(grid_CircularButtonContainer, 0, 0, null);

                            m_bIsAIRechecking = -1;
                        }

                        if (response.Message != null && response.Message[0].Contains("汇总信息为空"))
                        {
                            m_bIsAIRechecking = 0;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);

                m_parent.m_global.m_log_presenter.Log("提交MES异常：" + ex.Message);
            }
        }

        // 按钮点击事件：待清洁
        private void BtnSetThirdState_Click(object sender, RoutedEventArgs e)
        {
            if (m_parent.m_global.m_strProductType == "nova")
            {
                if (null == m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Nova)
                {
                    MessageBox.Show("产品信息为空，无法选择待清洁！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                else
                {
                    try
                    {
                        for (int n = 0; n < m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Nova.Products[m_nCurrentDefectedProductIndex].Defects.Count; n++)
                        {
                            Defect defect = m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Nova.Products[m_nCurrentDefectedProductIndex].Defects[n];

                            if (defect != null)
                            {
                                //if (defect.Type.Contains("MC") || defect.Type.Contains("DC"))
                                //{
                                //    MessageBox.Show("该产品有MC或DC缺陷，无法选择待清洁！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                                //    return;
                                //}
                            }
                        }

                        // 提示用户是否选择产品为待清洁
                        if (MessageBox.Show("请选择是否将产品设为待清洁？", "提示", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                        {
                            m_parent.m_global.m_current_MES_tray_info.m_check_flags_by_product[m_nCurrentDefectedProductIndex] = RecheckResult.NEED_CLEAN;

                            //if (true)
                            //{
                            //    if (m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Nova.products_to_clean == null)
                            //    {
                            //        m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Nova.products_to_clean = new List<Product> { };
                            //    }

                            //    m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Nova.Products

                            //    for (int i = 0; i < m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Nova.Products.Count; i++)
                            //    {
                            //        Product product = m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Nova.Products[i];

                            //        if (product.defects == null || product.defects.Count == 0 || product.bET == true)
                            //        {
                            //            m_parent.m_global.m_current_tray_info_for_Dock.ET_products.Add(product);

                            //            m_parent.m_global.m_current_tray_info_for_Dock.products.RemoveAt(i);

                            //            i--;
                            //        }
                            //    }
                            //}

                            if ((m_nCurrentDefectedProductIndex + 1) < m_parent.m_global.m_current_tray_info_for_Nova.Products.Count
                                && (m_nCurrentDefectedProductIndex + 1) < gridview_RecheckItemBarcode.Rows.Count)
                            {
                                m_nCurrentDefectedProductIndex++;

                                SelectDefectedProductBarcode(m_nCurrentDefectedProductIndex);

                                if (m_nCurrentDefectedProductIndex < m_parent.m_global.m_current_MES_tray_info.m_array_indices_of_defected_products.Length)
                                    SelectProduct(m_parent.m_global.m_current_MES_tray_info.m_array_indices_of_defected_products[m_nCurrentDefectedProductIndex]);
                            }
                            else
                            {
                                // 提示已经是最后一个产品，没有下一个
                                MessageBox.Show("已经是最后一个产品，没有下一个", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        m_parent.m_global.m_log_presenter.Log(string.Format("待清洁状态设置失败，错误信息：{0}", ex.Message));
                    }
                }
            }
        }

        // 插入缺陷统计
        private void InsertDefectCount(List<DataModels.DefectCount> defectCounts)
        {
            try
            {
                string sql = string.Empty;
                foreach (var item in defectCounts)
                {
                    sql = $@"select count(1) from defectcount where defectname='{item.DefectName}';";
                    int count = (int)MySqlHelpers.ExecuteScalar(m_parent.m_global.ConnsectionString, CommandType.Text, sql);
                    if (count > 0)
                    {
                        sql = $@"UPDATE defectcount set  `COUNT`={item.Count} where DefectName='{item.DefectName}';";
                    }
                    else
                    {
                        sql = $@"INSERT into defectcount ( DefectName,`COUNT`) VALUES ('{item.DefectName}',{item.Count}) ;";
                    }
                    MySqlHelpers.ExecuteNonQuery(m_parent.m_global.ConnsectionString, CommandType.Text, sql);
                }
            }
            catch (Exception ex)
            {
                m_parent.m_global.m_log_presenter.Log("插入缺陷统计异常：" + ex.Message);
            }
        }

        // 获取缺陷统计
        private List<DataModels.DefectCount> GetDefectCounts()
        {
            List<DataModels.DefectCount> defectCounts = new List<DataModels.DefectCount>();
            try
            {
                string sql = "select * from defectcount;";
                var data = MySqlHelpers.GetDataTable(m_parent.m_global.ConnsectionString, CommandType.Text, sql);
                if (data != null && data?.Rows.Count > 0)
                {
                    defectCounts = ListHelper.DataTableToList<DataModels.DefectCount>(data);
                }
            }
            catch (Exception ex)
            {
                m_parent.m_global.m_log_presenter.Log("获取缺陷统计异常：" + ex.Message);
            }
            return defectCounts;
        }

        // 按钮点击事件：清空产品按钮
        public void BtnClearProductButtons_Click(object sender, RoutedEventArgs e)
        {
            // 弹出确认对话框
            //if (MessageBox.Show(m_parent, "确定要清空所有产品按钮吗？", "再次确认", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                ClearProductInfo();

                // 清空产品信息
                GenerateCircularButtonsInGridContainer(grid_CircularButtonContainer, 0, 0, null);
            }
        }

        // 文本框内容改变事件：扫描条码
        private void textbox_ScannedBarcode_TextChanged(object sender, TextChangedEventArgs e)
        {
            string strTemp = textbox_ScannedBarcode.Text.Trim();

            if (strTemp.Length > 5)
            {
                // 启动配置文件监视器
                (new Thread(thread_execute_query_with_delay)).Start();
            }
        }

        // 功能：为料盘按钮上色
        public void render_tray_button_color()
        {
            try
            {
                // 没选中的都不高亮
                for (int i = 0; i < m_product_buttons.Length; i++)
                {
                    if (null != m_parent.m_global.m_current_MES_tray_info.m_check_flags_by_product)
                    {
                        if (m_parent.m_global.m_current_MES_tray_info.m_check_flags_by_product[i] == RecheckResult.OK)
                            m_product_buttons[i].Background = System.Windows.Media.Brushes.MediumSeaGreen;
                        else if (m_parent.m_global.m_current_MES_tray_info.m_check_flags_by_product[i] == RecheckResult.NG)
                            m_product_buttons[i].Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(228, 0, 0));
                        else if (m_parent.m_global.m_current_MES_tray_info.m_check_flags_by_product[i] == RecheckResult.DoNotNeedRecheck)
                            m_product_buttons[i].Background = System.Windows.Media.Brushes.Gray;
                        //m_product_buttons[i].Background = System.Windows.Media.Brushes.Green;
                        else if (m_parent.m_global.m_current_MES_tray_info.m_check_flags_by_product[i] == RecheckResult.ET)
                        {
                            m_product_buttons[i].Background = System.Windows.Media.Brushes.Black;
                            m_product_buttons[i].Foreground = System.Windows.Media.Brushes.White;
                            m_product_buttons[i].Content = "ET";
                        }
                        else if (m_parent.m_global.m_current_MES_tray_info.m_check_flags_by_product[i] == RecheckResult.EMPTY_DEFECT)
                        {
                            m_product_buttons[i].Background = System.Windows.Media.Brushes.Black;
                            m_product_buttons[i].Foreground = System.Windows.Media.Brushes.White;
                            m_product_buttons[i].Content = "";
                        }
                        else if (m_parent.m_global.m_current_MES_tray_info.m_check_flags_by_product[i] == RecheckResult.NoCode)
                        {
                            //m_product_buttons[i].Background = System.Windows.Media.Brushes.Black;
                            m_product_buttons[i].Background = System.Windows.Media.Brushes.Gray;
                            m_product_buttons[i].Foreground = System.Windows.Media.Brushes.White;
                            //m_product_buttons[i].Content = "空";
                        }
                        else if (m_parent.m_global.m_current_MES_tray_info.m_check_flags_by_product[i] == RecheckResult.Unrecheckable)
                        {
                            // 紫色
                            if (m_parent.m_global.m_strSiteCity == "苏州")
                            {
                                m_product_buttons[i].Background = System.Windows.Media.Brushes.Purple;
                            }
                            else if (m_parent.m_global.m_strSiteCity == "盐城")
                            {
                                m_product_buttons[i].Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(128, 0, 0));
                            }
                        }
                        else if (m_parent.m_global.m_current_MES_tray_info.m_check_flags_by_product[i] == RecheckResult.NotReceived)
                        {
                            // 浅红色，用RGB表示
                            m_product_buttons[i].Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 122, 122));
                        }
                        else if (m_parent.m_global.m_current_MES_tray_info.m_check_flags_by_product[i] == RecheckResult.FailedPositioning)
                        {
                            // 浅红色，用RGB表示

                            if (m_parent.m_global.m_strSiteCity == "苏州")
                            {
                                m_product_buttons[i].Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(128, 0, 255));
                            }
                            else if (m_parent.m_global.m_strSiteCity == "盐城")
                            {
                                m_product_buttons[i].Background = System.Windows.Media.Brushes.Purple;
                            }
                        }
                        else if (m_parent.m_global.m_current_MES_tray_info.m_check_flags_by_product[i] == RecheckResult.EmptySlot)
                        {
                            // 浅灰色
                            m_product_buttons[i].Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(200, 200, 200));
                        }
                        else if (m_parent.m_global.m_current_MES_tray_info.m_check_flags_by_product[i] == RecheckResult.MES_NG)
                        {
                            m_product_buttons[i].Background = System.Windows.Media.Brushes.DarkOrange;
                        }
                        else
                            m_product_buttons[i].Background = System.Windows.Media.Brushes.Yellow;
                    }

                    if (m_parent.m_global.m_current_MES_tray_info.m_check_flags_by_product[i] != RecheckResult.ET && m_parent.m_global.m_current_MES_tray_info.m_check_flags_by_product[i] != RecheckResult.NoCode)
                        m_product_buttons[i].Foreground = System.Windows.Media.Brushes.Black;
                }
            }
            catch (Exception ex)
            {

            }
        }

        // 功能：清空图像
        public void ClearImageControlContent()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                m_camera_canvases[0] = new CameraCanvas(m_parent, ctrl_NavigationImageCanvas,
                            (int)grid_NavigationImageCanvas1.Width, (int)grid_NavigationImageCanvas1.ActualHeight, scene_type.scene_camera1, false);

                m_camera_canvases[0].m_bForceRedraw = true;
                m_camera_canvases[0].OnWindowSizeChanged(null, null);

                m_camera_canvases[1] = new CameraCanvas(m_parent, ctrl_ROIImageCanvas1,
                        (int)grid_ROIImageCanvas.Width, (int)grid_ROIImageCanvas.ActualHeight, scene_type.scene_camera2, false);

                m_camera_canvases[1].m_bForceRedraw = true;
                m_camera_canvases[1].OnWindowSizeChanged(null, null);
            });
        }

        // 功能：清空界面上的产品信息
        public void ClearProductInfo()
        {
            // 更新UI
            Application.Current.Dispatcher.Invoke(() =>
            {
                //GenerateCircularButtonsInGridContainer(grid_CircularButtonContainer, 0, 0, null);

                m_parent.page_HomeView.gridview_RecheckItemNo.Rows.Clear();
                m_parent.page_HomeView.gridview_RecheckItemBarcode.Rows.Clear();
                m_parent.page_HomeView.gridview_DefectsWaitingConfirm.Rows.Clear();

                textblock_TrayBarcode.Text = "";
                textblock_FrontOrBack.Text = "";
                textblock_CurrentBarcode.Text = "";
                textblock_TotalPieces.Text = "";
                textblock_TotalDefectNum.Text = "";
                textblock_CurrentProductIndex.Text = "";
                textblock_NumOfDefectsOfCurrentPiece.Text = "";
                textblock_InspectDateTime.Text = "";
                textblock_AIResult.Text = "";

                // 清空图像
                ClearImageControlContent();
            });
        }

        // 功能：把图片1的ROI区域内容复制到图片2
        public void CopyROIContentFromSrcImageToDstImage(int nSourceImageIndex, int nDestImageIndex,
            int nROILeft, int nROITop, int nROIWidth, int nROIHeight)
        {
            if (null == m_camera_canvases[nSourceImageIndex] || null == m_camera_canvases[nDestImageIndex])
                return;

            int[] pIntParams = new int[10];
            int[] pRetInts = new int[10];

            pIntParams[0] = nSourceImageIndex;
            pIntParams[1] = nDestImageIndex;
            pIntParams[2] = nROILeft;
            pIntParams[3] = nROITop;
            pIntParams[4] = nROIWidth;
            pIntParams[5] = nROIHeight;

            if (nROIWidth <= 0 || nROIHeight <= 0)
                return;

            pRetInts[0] = 0;

            PostVisionDLL_copy_ROI_content_from_image_to_image(pIntParams, pRetInts);

            if (1 == pRetInts[0])
            {
                int nSrcImageWidth = pRetInts[1];
                int nSrcImageHeight = pRetInts[2];

                m_camera_canvases[nDestImageIndex].set_origin_image_size(nSrcImageWidth, nSrcImageHeight);

                m_camera_canvases[nDestImageIndex].m_bHasValidImage = true;
                m_camera_canvases[nDestImageIndex].m_bForceRedraw = true;
            }
        }

        // 功能：在grid_CircularButtonContainer生成M行N列的按钮
        public void GenerateCircularButtonsInGridContainer(Grid grid_container, int nRows, int nColumns, RecheckResult[] recheck_flags)
        {
            if (nRows <= 0 || nColumns <= 0)
            {
                grid_container.Children.Clear();

                return;
            }
            if (grid_container == null)
                return;
            if (recheck_flags.Length != nRows * nColumns)
                return;

            Grid grid = grid_container;

            grid.Children.Clear();

            // 清除之前的定义
            grid.ColumnDefinitions.Clear();
            grid.RowDefinitions.Clear();

            if (m_parent.m_global.m_strProductType == "nova")
            {
                // 为 Grid 定义三行
                for (int rowIndex = 0; rowIndex < nRows; rowIndex++)
                {
                    grid.RowDefinitions.Add(new RowDefinition()
                    {
                        Height = GridLength.Auto // 设置行高为自动，根据内容调整
                    });
                }

                // 为 Grid 定义五列
                for (int columnIndex = 0; columnIndex < nColumns; columnIndex++)
                {
                    grid.ColumnDefinitions.Add(new ColumnDefinition()
                    {
                        Width = GridLength.Auto // 设置列宽为自动，根据内容调整
                    });
                }
            }
            else if (m_parent.m_global.m_strProductType == "dock")
            {
                if (3 == m_parent.m_global.m_nSpecialTrayCornerMode)
                {
                    // 为 Grid 定义三行
                    for (int rowIndex = 0; rowIndex < nColumns; rowIndex++)
                    {
                        grid.RowDefinitions.Add(new RowDefinition()
                        {
                            Height = GridLength.Auto // 设置行高为自动，根据内容调整
                        });
                    }

                    // 为 Grid 定义五列
                    for (int columnIndex = 0; columnIndex < nRows; columnIndex++)
                    {
                        grid.ColumnDefinitions.Add(new ColumnDefinition()
                        {
                            Width = GridLength.Auto // 设置列宽为自动，根据内容调整
                        });
                    }
                }
                else
                {
                    // 为 Grid 定义三行
                    for (int rowIndex = 0; rowIndex < nRows; rowIndex++)
                    {
                        grid.RowDefinitions.Add(new RowDefinition()
                        {
                            Height = GridLength.Auto // 设置行高为自动，根据内容调整
                        });
                    }

                    // 为 Grid 定义五列
                    for (int columnIndex = 0; columnIndex < nColumns; columnIndex++)
                    {
                        grid.ColumnDefinitions.Add(new ColumnDefinition()
                        {
                            Width = GridLength.Auto // 设置列宽为自动，根据内容调整
                        });
                    }
                }
            }

            m_product_buttons = new Button[nRows * nColumns];

            // 偶数行是否逆序
            bool bReverseOrderForEvenRow = false;
            if (false == m_parent.m_global.m_dict_machine_names_and_IPs.Keys.Contains(m_parent.m_global.m_strCurrentClientMachineName))
            {
                m_parent.m_global.m_log_presenter.Log(string.Format("错误信息：机器编号{0}不在清单上！", m_parent.m_global.m_strCurrentClientMachineName));
                return;
            }
            bReverseOrderForEvenRow = m_parent.m_global.m_dict_machine_names_and_flags_of_reverse_order_for_even_rows[m_parent.m_global.m_strCurrentClientMachineName];

            // 创建并添加 15 个按钮
            for (int i = 0; i < nRows * nColumns; i++)
            {
                // 计算行和列的索引
                int nCurrentRow = i / nColumns; // 整数除法确定行索引
                int nCurrentCol = i % nColumns; // 取余数确定列索引

                // 创建按钮实例
                m_product_buttons[i] = new Button();
                m_product_buttons[i].FontSize = 16;

                // 标识按钮的索引
                if (m_parent.m_global.m_strProductType == "nova")
                {
                    if (true == bReverseOrderForEvenRow)
                    {
                        if (1 == nCurrentRow % 2)
                        {
                            m_product_buttons[i].Tag = nCurrentRow * nColumns + (i % nColumns);
                            m_product_buttons[i].Content = $"{nCurrentRow * nColumns + (i % nColumns) + 1}";
                        }
                        else
                        {
                            m_product_buttons[i].Tag = i;
                            m_product_buttons[i].Content = $"{i + 1}";
                        }
                    }
                    else
                    {
                        m_product_buttons[i].Tag = i;
                        m_product_buttons[i].Content = $"{i + 1}";
                    }
                }
                else if (m_parent.m_global.m_strProductType == "dock")
                {
                    if (1 == m_parent.m_global.m_nSpecialTrayCornerMode)
                    {

                    }
                    else if (222 == m_parent.m_global.m_nSpecialTrayCornerMode)
                    {
                        nCurrentRow = i % nRows; // 整数除法确定行索引
                        nCurrentCol = i / nRows; // 取余数确定列索引

                        //m_product_buttons[i].Tag = nCurrentRow * nColumns + (i % nColumns);
                        //m_product_buttons[i].Content = $"{nCurrentRow * nColumns + (i % nColumns) + 1}";
                        m_product_buttons[i].Tag = i;
                        m_product_buttons[i].Content = $"{i + 1}";
                    }
                    else if (3 == m_parent.m_global.m_nSpecialTrayCornerMode)
                    {
                        m_product_buttons[i].Tag = i;
                        m_product_buttons[i].Content = $"{i + 1}";
                    }
                    else
                    {
                        m_product_buttons[i].Tag = i;
                        m_product_buttons[i].Content = $"{i + 1}";
                    }
                }

                // 设置按钮背景颜色
                //button.Background = new SolidColorBrush(i < 5 ? Colors.Green : Colors.Yellow);

                switch (recheck_flags[i])
                {
                    case RecheckResult.DoNotNeedRecheck:
                        m_product_buttons[i].Background = new SolidColorBrush(Colors.Gray);
                        break;
                    //case RecheckResult.NotChecked:
                    //    m_product_buttons[i].Background = new SolidColorBrush(Colors.Yellow);
                    //    break;
                    case RecheckResult.ET:
                        m_product_buttons[i].Background = new SolidColorBrush(Colors.Black);
                        m_product_buttons[i].Foreground = System.Windows.Media.Brushes.White;
                        m_product_buttons[i].Content = "ET";
                        break;
                    case RecheckResult.EmptySlot:
                        m_product_buttons[i].Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(200, 200, 200));
                        break;
                    case RecheckResult.NoCode:
                        m_product_buttons[i].Background = new SolidColorBrush(Colors.Gray);
                        break;
                    case RecheckResult.MES_NG:
                        m_product_buttons[i].Background = new SolidColorBrush(Colors.DarkOrange);
                        break;
                    case RecheckResult.Unrecheckable:
                        m_product_buttons[i].Background = new SolidColorBrush(Colors.Purple);
                        break;
                }

                // 应用样式
                if (Application.Current.Resources["CircularButtonStyle"] is Style style)
                {
                    m_product_buttons[i].Style = style;
                }

                if (m_parent.m_global.m_strProductType == "dock")
                {
                    // 创建直角的两条线段
                    Line line1 = new Line();
                    line1.X1 = -0;
                    line1.Y1 = -0;
                    line1.X2 = line1.X1 + 23;
                    line1.Y2 = line1.Y1;
                    line1.Stroke = System.Windows.Media.Brushes.Black;
                    line1.StrokeThickness = 2;

                    Line line2 = new Line();
                    line2.X1 = line1.X2;
                    line2.Y1 = line1.Y2;
                    line2.X2 = line2.X1;
                    line2.Y2 = line2.Y1 + 23;
                    line2.Stroke = System.Windows.Media.Brushes.Black;
                    line2.StrokeThickness = 2;

                    Line line3 = new Line();
                    line3.X1 = line1.X1;
                    line3.Y1 = line1.Y1;
                    line3.X2 = line3.X1;
                    line3.Y2 = line3.Y1 + 23;
                    line3.Stroke = System.Windows.Media.Brushes.Black;
                    line3.StrokeThickness = 2;

                    Line line4 = new Line();
                    line4.X1 = line3.X2;
                    line4.Y1 = line3.Y2;
                    line4.X2 = line4.X1 + 23;
                    line4.Y2 = line4.Y1;
                    line4.Stroke = System.Windows.Media.Brushes.Black;
                    line4.StrokeThickness = 2;

                    // 创建一个 Grid 作为 Button 的内容
                    Grid grid2 = new Grid();

                    // 创建一个 TextBlock 用于显示按钮文本
                    TextBlock textBlock = new TextBlock();
                    textBlock.Text = m_product_buttons[i].Content.ToString();
                    textBlock.HorizontalAlignment = HorizontalAlignment.Center;
                    textBlock.VerticalAlignment = VerticalAlignment.Center;

                    // 将文本和直角添加到 Grid 中
                    grid2.Children.Add(textBlock);
                    if (3 == m_parent.m_global.m_nSpecialTrayCornerMode)
                    {
                        if (nCurrentRow % 2 == 1)
                        {
                            grid2.Children.Add(line1);
                            grid2.Children.Add(line2);
                        }
                        else
                        {
                            grid2.Children.Add(line3);
                            grid2.Children.Add(line4);
                        }
                    }

                    m_product_buttons[i].Content = grid2;
                }

                // 添加 Click 事件处理器
                m_product_buttons[i].Click += BtnProductIndexClick;

                m_product_buttons[i].Width = 35;
                m_product_buttons[i].Height = 35;

                double spacing = (grid.ActualWidth - m_product_buttons[i].Width * nColumns - 50) / nColumns;
                //spacing /= m_parent.m_global.m_dbScreenDPIScale;

                if (m_parent.m_global.m_strProductType == "dock" && 3 == m_parent.m_global.m_nSpecialTrayCornerMode)
                {
                    spacing = (grid.ActualWidth - m_product_buttons[i].Width * nRows - 50) / nRows;
                }

                // 设置按钮间距，为所有按钮添加相同的间距
                if (0 == nCurrentCol)
                    m_product_buttons[i].Margin = new Thickness(spacing, 3, 3, 0);
                else if ((nColumns - 1) == nCurrentCol)
                    m_product_buttons[i].Margin = new Thickness(spacing, 3, 3, 0);
                else
                    m_product_buttons[i].Margin = new Thickness(spacing, 3, 3, 0);

                // 将按钮添加到 Grid，并设置其行和列
                grid.Children.Add(m_product_buttons[i]);
                Grid.SetRow(m_product_buttons[i], nCurrentRow);

                if (m_parent.m_global.m_strProductType == "nova")
                {
                    if (true == bReverseOrderForEvenRow)
                    {
                        if (1 == nCurrentRow % 2)
                            Grid.SetColumn(m_product_buttons[i], nColumns - nCurrentCol - 1);
                        else
                            Grid.SetColumn(m_product_buttons[i], nCurrentCol);
                    }
                    else
                        Grid.SetColumn(m_product_buttons[i], nCurrentCol);
                }
                else if (m_parent.m_global.m_strProductType == "dock")
                {
                    if (1 == m_parent.m_global.m_nSpecialTrayCornerMode)
                    {

                    }
                    else if (222 == m_parent.m_global.m_nSpecialTrayCornerMode)
                    {
                        //Grid.SetColumn(m_product_buttons[i], nColumns - nCurrentCol - 1);
                        Grid.SetColumn(m_product_buttons[i], nCurrentCol);
                    }
                    else if (3 == m_parent.m_global.m_nSpecialTrayCornerMode)
                    {
                        Grid.SetRow(m_product_buttons[i], nCurrentCol);
                        Grid.SetColumn(m_product_buttons[i], nCurrentRow);
                    }
                    else
                        Grid.SetColumn(m_product_buttons[i], nCurrentCol);
                }
            }
        }

        // 功能：根据产品信息更新UI
        public void UpdateUIWithProductInfoForNova(AVITrayInfo productInfo, int nCurrentProductIndex)
        {
            if (null == productInfo)
                return;

            // 更新产品信息
            if (true)
            {
                // 更新产品信息
                if (null != productInfo.Products && productInfo.Products.Count > 0)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        // 更新一些文本框
                        textblock_FrontOrBack.Text = productInfo.Front == true ? "A面" : "B面";

                        textblock_TotalPieces.Text = productInfo.TotalPcs.ToString();

                        var totalDefectCount = 0;
                        foreach (var product in productInfo.Products)
                        {
                            totalDefectCount += product.Defects.Count;
                        }
                        textblock_TotalDefectNum.Text = totalDefectCount.ToString();

                        if (productInfo.Products.Count > 0)
                        {
                            textblock_CurrentBarcode.Text = productInfo.Products[nCurrentProductIndex].BarCode;
                            textblock_InspectDateTime.Text = productInfo.Products[nCurrentProductIndex].r1;
                            textblock_AIResult.Text = productInfo.r3;

                            if (null == productInfo.Products[nCurrentProductIndex].Defects)
                                textblock_NumOfDefectsOfCurrentPiece.Text = "0";
                            else
                                textblock_NumOfDefectsOfCurrentPiece.Text = productInfo.Products[nCurrentProductIndex].Defects.Count.ToString();
                        }

                        // 获取当前产品索引
                        string datePart = "";
                        string timePart = "";
                        string strDate = "";
                        string strTime = "";

                        string[] parts = productInfo.SetId.Split('_');
                        if (parts.Length > 1)
                        {
                            datePart = parts[1].Substring(0, 8);
                            timePart = parts[1].Substring(8, 6);
                        }

                        #region 旧setid处理方式，新setid格式不一样
                        //if (true)
                        //{
                        //    string[] parts = productInfo.SetId.Split('-');
                        //    if (parts.Length > 2)
                        //    {
                        //        //nCurrentProductIndex = int.Parse(parts[parts.Length - 1]);

                        //        // 提取倒数第三部分作为日期
                        //        datePart = parts[parts.Length - 3];
                        //        // 提取倒数第二部分作为时间
                        //        timePart = parts[parts.Length - 2];
                        //    }
                        //    else
                        //    {
                        //        string[] parts1 = productInfo.Products[nCurrentProductIndex].r1.Split('_');
                        //        if (parts1.Length > 5)
                        //        {
                        //            // 提取第一二三部分作为日期
                        //            datePart = parts1[0] + parts1[1] + parts1[2];
                        //            // 提取第四五六部分作为时间
                        //            timePart = parts1[3] + parts1[4] + parts1[5];
                        //        }
                        //    }
                        //}

                        #endregion

                        // 尝试解析日期和时间
                        if (DateTime.TryParseExact(datePart, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date) &&
                            DateTime.TryParseExact(timePart, "HHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime time))
                        {
                            // 使用标准的日期和时间格式化
                            strDate = date.ToString("yyyy-MM-dd");
                            strTime = time.ToString("HH:mm:ss");
                        }

                        textblock_CurrentProductIndex.Text = (nCurrentProductIndex + 1).ToString();
                        textblock_InspectDateTime.Text = string.Format("{0} {1}", strDate, strTime);
                    });
                }
            }
        }

        // 功能：根据产品信息更新UI
        public void UpdateUIWithProductInfoForDock(TrayInfo tray_info, int nCurrentProductIndex)
        {
            if (null == tray_info)
                return;

            // 更新产品信息
            if (true)
            {
                // 更新产品信息
                if (null != tray_info.products && tray_info.products.Count > 0)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        int nTotalPcs = tray_info.total_rows * tray_info.total_columns;

                        // 更新一些文本框
                        string[] strSides = { "A面", "B面", "C面", "D面" };
                        textblock_FrontOrBack.Text = (m_nCurrentSideAorB >= 0 && m_nCurrentSideAorB < strSides.Length) ? strSides[m_nCurrentSideAorB] : "未知";

                        textblock_TotalPieces.Text = nTotalPcs.ToString();

                        var totalDefectCount = 0;
                        foreach (var product in tray_info.products)
                        {
                            totalDefectCount += product.defects.Count;
                        }
                        textblock_TotalDefectNum.Text = totalDefectCount.ToString();

                        textblock_MachineID.Text = tray_info.products[nCurrentProductIndex].machine_id;

                        if (tray_info.products.Count > 0)
                        {
                            textblock_CurrentBarcode.Text = tray_info.products[nCurrentProductIndex].barcode;
                            textbox_VirtualSetID.Text = tray_info.products[nCurrentProductIndex].set_id;
                            textblock_AIResult.Text = tray_info.products[nCurrentProductIndex].r3;

                            if (null == tray_info.products[nCurrentProductIndex].defects)
                                textblock_NumOfDefectsOfCurrentPiece.Text = "0";
                            else
                                textblock_NumOfDefectsOfCurrentPiece.Text = tray_info.products[nCurrentProductIndex].defects.Count.ToString();
                        }

                        // 获取当前产品索引
                        string strDate = "";
                        string strTime = "";
                        if (true)
                        {
                            string[] parts = tray_info.set_id.Split('-');
                            if (parts.Length >= 3)
                            {
                                //nCurrentProductIndex = int.Parse(parts[parts.Length - 1]);

                                // 提取倒数第三部分作为日期
                                string datePart = parts[parts.Length - 3];
                                // 提取倒数第二部分作为时间
                                string timePart = parts[parts.Length - 2];

                                // 尝试解析日期和时间
                                if (DateTime.TryParseExact(datePart, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date) &&
                                    DateTime.TryParseExact(timePart, "HHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime time))
                                {
                                    // 使用标准的日期和时间格式化
                                    strDate = date.ToString("yyyy-MM-dd");
                                    strTime = time.ToString("HH:mm:ss");
                                }
                            }
                        }

                        textblock_CurrentProductIndex.Text = (nCurrentProductIndex + 1).ToString();

                        if (strDate.Length > 0)
                        {
                            textblock_InspectDateTime.Text = string.Format("{0} {1}", strDate, strTime);
                        }
                        else if (false == string.IsNullOrEmpty(tray_info.products[nCurrentProductIndex].r1))
                        {
                            textblock_InspectDateTime.Text = string.Format("{0}", tray_info.products[nCurrentProductIndex].r1);
                        }
                        else if (false == string.IsNullOrEmpty(tray_info.products[nCurrentProductIndex].inspect_date_time))
                        {
                            textblock_InspectDateTime.Text = string.Format("{0}", tray_info.products[nCurrentProductIndex].inspect_date_time);
                        }
                    });
                }
            }
        }

        // 功能：指定选中某个产品
        public void SelectProduct(int nIndex)
        {
            if (nIndex >= 0 && nIndex < m_product_buttons.Length)
            {
                m_product_buttons[nIndex].RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            }
        }

        // 功能：单Pcs模式下，显示指定二维码的产品
        public void SelectProductInSinglePieceMode(string strBarcode)
        {
            for (int n = 0; n < m_parent.m_global.m_current_MES_tray_info.m_array_barcodes_of_defected_products.Length; n++)
            {
                if (m_parent.m_global.m_current_tray_info_for_Dock.products[n].barcode == strBarcode)
                {
                    int nIndex = m_parent.m_global.m_current_MES_tray_info.m_array_indices_of_defected_products[n];

                    if (nIndex >= 0 && nIndex < m_product_buttons.Length)
                    {
                        // 隐藏其它按钮，只显示当前按钮
                        for (int i = 0; i < m_product_buttons.Length; i++)
                        {
                            if (i == nIndex)
                                m_product_buttons[i].Visibility = Visibility.Visible;
                            else
                                m_product_buttons[i].Visibility = Visibility.Hidden;
                        }

                        m_product_buttons[nIndex].RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    }

                    break;
                }
            }
        }

        // 功能：显示指定索引号缺陷的图片
        public void ShowDefectOnCanvasForNova(int nDefectedProductIndex, int nDefectIndex, int nImageWidth, int nImageHeight)
        {
            if (nDefectedProductIndex < 0 || nDefectedProductIndex >= m_parent.m_global.m_current_tray_info_for_Nova.Products.Count)
            {
                m_parent.m_global.m_log_presenter.Log("产品索引超出范围");
                return;
            }
            if (nDefectIndex < 0 || nDefectIndex >= m_parent.m_global.m_current_tray_info_for_Nova.Products[nDefectedProductIndex].Defects.Count)
            {
                m_parent.m_global.m_log_presenter.Log("缺陷索引超出范围");
                return;
            }
            if (nImageWidth <= 0 || nImageHeight <= 0)
            {
                if (m_nCurrentDefectedProductIndex != 0)
                {
                    m_parent.m_global.m_log_presenter.Log("图像尺寸错误，为0");

                    MessageBox.Show("图像尺寸错误，为0");
                }
                return;
            }

            Defect defect = m_parent.m_global.m_current_tray_info_for_Nova.Products[nDefectedProductIndex].Defects[nDefectIndex];

            double x = defect.X - defect.Width / 2;
            double y = defect.Y - defect.Height / 2;
            double width = defect.Width;
            double height = defect.Height;

            Point2d lefttop = new Point2d(x, y);
            Point2d rightbottom = new Point2d(x + width, y + height);

            m_parent.m_global.m_log_presenter.Log(string.Format("第[{0},{1}]个缺陷 坐标：({2:0.}, {3:0.}), 宽高 [{4:0.},{5:0.}]", nDefectedProductIndex + 1, nDefectIndex + 1, x, y, width, height));

            double extension = 300;

            //x -= extension;
            //y -= extension;
            //width += extension * 2;
            //height += extension * 2;

            int nControlSize = m_camera_canvases[1].m_nControlWidth < m_camera_canvases[1].m_nControlHeight ? m_camera_canvases[1].m_nControlWidth : m_camera_canvases[1].m_nControlHeight;

            int nDefectSize = (int)(defect.Width > defect.Height ? defect.Width : defect.Height);

            if (nControlSize > nDefectSize * 2)
            {
                x -= nControlSize / 2;
                y -= nControlSize / 2;
                width = nControlSize;
                height = nControlSize;
            }
            else
            {
                //x -= extension;
                //y -= extension;
                //width += extension * 2;
                //height += extension * 2;
                x = 0;
                y = 0;
                width = nImageWidth;
                height = nImageHeight;
            }

            if (x + width > nImageWidth)
            {
                x = nImageWidth - width;
            }
            if (y + height > nImageHeight)
            {
                y = nImageHeight - height;
            }
            if (x < 0)
            {
                x = 0;
                if (width > nImageWidth - 1)
                    width = nImageWidth - 1;
            }
            if (y < 0)
            {
                y = 0;
                if (height > nImageHeight - 1)
                    height = nImageHeight - 1;
            }

            m_parent.page_HomeView.CopyROIContentFromSrcImageToDstImage(0, 1, (int)x, (int)y, (int)width, (int)height);

            Application.Current.Dispatcher.Invoke(() =>
            {
                double extension2 = 10;

                Point2d new_lefttop = new Point2d(lefttop.x - x - extension2, lefttop.y - y - extension2);
                Point2d new_rightbottom = new Point2d(rightbottom.x - x + extension2, rightbottom.y - y + extension2);

                string strAorB = defect.ChannelNum == 1 ? "A面" : "B面";

                string info = string.Format(" ({0} 光{1})", strAorB, defect.Channel);

                if (m_parent.m_global.m_strProductSubType == "yuehu")
                {
                    double um_per_pixel = 3.75;
                    info = string.Format(" ({0}  长宽 [{1:0.},{2:0.}]um)", strAorB, defect.Height * um_per_pixel, defect.Width * um_per_pixel);

                    m_parent.page_HomeView.m_camera_canvases[1].m_yolo_object = new YOLOObject(1, defect.Type + info,
                        new Point((int)new_lefttop.x, (int)new_lefttop.y), new Point((int)new_rightbottom.x, (int)new_rightbottom.y));
                }
                else
                {
                    var area = (defect.Width * defect.Height).ToString("0.00") + "um²";
                    if (defect.Area != null && defect.Area != 0)
                    {
                        area = defect.Area.ToString("0.00") + "um²";
                    }

                    info = string.Format($" \n面积：{area}");
                    var info2 = $"\n长：{defect.Height * defect.um_per_pixel}um，宽：{defect.Width * defect.um_per_pixel}um";

                    info += info2;

                    string strDefectType = defect.Type;

                    // 是否启用缺陷名称映射
                    if (m_parent.m_global.m_bEnableDefectNameMapping == true)
                    {
                        for (int i = 0; i < m_parent.m_global.m_dict_old_defect_names_and_new_defect_names_mapping.Count; i++)
                        {
                            if (m_parent.m_global.m_dict_old_defect_names_and_new_defect_names_mapping.ElementAt(i).Key == defect.Type)
                            {
                                strDefectType = m_parent.m_global.m_dict_old_defect_names_and_new_defect_names_mapping.ElementAt(i).Value;
                                break;
                            }
                        }
                    }

                    m_parent.page_HomeView.m_camera_canvases[1].m_yolo_object = new YOLOObject(1, strDefectType + info,
                    new Point((int)new_lefttop.x, (int)new_lefttop.y), new Point((int)new_rightbottom.x, (int)new_rightbottom.y));
                }

                m_parent.page_HomeView.m_camera_canvases[1].show_whole_image();

                double x2 = defect.X - defect.Width / 2;
                double y2 = defect.Y - defect.Height / 2;
                double width2 = defect.Width;
                double height2 = defect.Height;

                lefttop = new Point2d(x2, y2);
                rightbottom = new Point2d(x2 + width2, y2 + height2);

                new_lefttop = new Point2d(lefttop.x - extension2, lefttop.y - extension2);
                new_rightbottom = new Point2d(rightbottom.x + extension2, rightbottom.y + extension2);

                m_parent.page_HomeView.m_camera_canvases[0].m_yolo_object = new YOLOObject(1, "", new Point((int)new_lefttop.x, (int)new_lefttop.y), new Point((int)new_rightbottom.x, (int)new_rightbottom.y));

                m_parent.page_HomeView.m_camera_canvases[0].refresh(true);
            });
        }

        // 功能：显示指定索引号缺陷的图片
        public void ShowDefectOnCanvasForDock(int nDefectedProductIndex, int nDefectIndex, int nImageWidth, int nImageHeight)
        {
            if (nDefectedProductIndex < 0 || nDefectedProductIndex >= m_parent.m_global.m_current_tray_info_for_Dock.products.Count)
            {
                m_parent.m_global.m_log_presenter.Log("产品索引超出范围");
                return;
            }
            if (nDefectIndex < 0 || nDefectIndex >= m_parent.m_global.m_current_tray_info_for_Dock.products[nDefectedProductIndex].defects.Count)
            {
                m_parent.m_global.m_log_presenter.Log("缺陷索引超出范围");
                return;
            }
            if (nImageWidth <= 0 || nImageHeight <= 0)
            {
                m_parent.m_global.m_log_presenter.Log("图像尺寸错误，为0");

                MessageBox.Show("图像尺寸错误，为0");
                return;
            }

            DefectInfo defect = m_parent.m_global.m_current_tray_info_for_Dock.products[nDefectedProductIndex].defects[nDefectIndex];

            double x = defect.center_x - defect.width / 2;
            double y = defect.center_y - defect.height / 2;
            double width = defect.width;
            double height = defect.height;

            // 是否将缺陷中心坐标视为左上角坐标
            bool bTreatDefectCenterAsLefttop = false;
            if (false == m_parent.m_global.m_dict_machine_names_and_IPs.Keys.Contains(m_parent.m_global.m_strCurrentClientMachineName))
            {
                m_parent.m_global.m_log_presenter.Log(string.Format("错误信息：机器编号{0}不在清单上！", m_parent.m_global.m_strCurrentClientMachineName));
                return;
            }
            bTreatDefectCenterAsLefttop = m_parent.m_global.m_dict_machine_names_and_flags_of_treating_defect_center_as_lefttop[m_parent.m_global.m_strCurrentClientMachineName];

            if (true == bTreatDefectCenterAsLefttop)
            {
                x = defect.center_x - defect.width;
                y = defect.center_y - defect.height;
            }

            Point2d lefttop = new Point2d(x, y);
            Point2d rightbottom = new Point2d(x + width, y + height);

            m_parent.m_global.m_log_presenter.Log(string.Format("第[{0},{1}]个缺陷 坐标：({2:0.}, {3:0.}), 宽高 [{4:0.},{5:0.}]", nDefectedProductIndex + 1, nDefectIndex + 1, x, y, width, height));

            double extension = 300;

            //x -= extension;
            //y -= extension;
            //width += extension * 2;
            //height += extension * 2;

            int nControlSize = m_camera_canvases[1].m_nControlWidth < m_camera_canvases[1].m_nControlHeight ? m_camera_canvases[1].m_nControlWidth : m_camera_canvases[1].m_nControlHeight;

            nControlSize = (nControlSize * 35) / 10;

            int nDefectSize = (int)(defect.width > defect.height ? defect.width : defect.height);

            // 从原图上截图的区域左上角起始坐标，以及宽高
            if (nControlSize > nDefectSize * 2)
            {
                x -= nControlSize / 2;
                y -= nControlSize / 2;
                width = nControlSize;
                height = nControlSize;
            }
            else
            {
                //x -= extension;
                //y -= extension;
                //width += extension * 2;
                //height += extension * 2;
                x = 0;
                y = 0;
                width = nImageWidth;
                height = nImageHeight;
            }

            if (x + width > nImageWidth)
            {
                x = nImageWidth - width;
            }
            if (y + height > nImageHeight)
            {
                y = nImageHeight - height;
            }
            if (x < 0)
            {
                x = 0;
                if (width > nImageWidth - 1)
                    width = nImageWidth - 1;
            }
            if (y < 0)
            {
                y = 0;
                if (height > nImageHeight - 1)
                    height = nImageHeight - 1;
            }
            if (width > nImageWidth - 1)
                width = nImageWidth - 1;
            if (height > nImageHeight - 1)
                height = nImageHeight - 1;

            m_parent.page_HomeView.CopyROIContentFromSrcImageToDstImage(0, 1, (int)x, (int)y, (int)width, (int)height);

            Application.Current.Dispatcher.Invoke(() =>
            {
                double extension2 = 100;

                if (m_parent.m_global.m_strProductSubType == "yuehu")
                    extension2 = 10;

                Point2d new_lefttop = new Point2d(lefttop.x - x - extension2, lefttop.y - y - extension2);
                Point2d new_rightbottom = new Point2d(rightbottom.x - x + extension2, rightbottom.y - y + extension2);

                string strAorB = defect.side == 0 ? "A面" : "B面";

                switch (defect.side)
                {
                    case 0:
                        strAorB = "A面";
                        break;
                    case 1:
                        strAorB = "B面";
                        break;
                    case 2:
                        strAorB = "C面";
                        break;
                    case 3:
                        strAorB = "D面";
                        break;
                }

                string info = string.Format(" ({0} 光{1})", strAorB, defect.light_channel);

                info = "";

                m_parent.page_HomeView.m_camera_canvases[1].m_yolo_object = new YOLOObject(1, defect.type + info,
                    new Point((int)new_lefttop.x, (int)new_lefttop.y), new Point((int)new_rightbottom.x, (int)new_rightbottom.y));

                m_parent.page_HomeView.m_camera_canvases[1].show_whole_image();

                m_parent.page_HomeView.m_camera_canvases[0].m_yolo_object = new YOLOObject(1, "", new Point((int)x, (int)y), new Point((int)(x + width), (int)(y + height)));

                m_parent.page_HomeView.m_camera_canvases[0].refresh(true);
            });
        }

        // 功能：选择缺陷产品二维码
        public void SelectDefectedProductBarcode(int nProductIndex)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (m_parent.m_global.m_strProductType == "nova")
                {
                    if (nProductIndex >= 0 && nProductIndex < m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Nova.Products.Count)
                    {
                        // 确定GridView可见的行数
                        int visibleRowCount = gridview_RecheckItemBarcode.DisplayedRowCount(false);

                        // 计算首个显示的行的索引，确保选中行为GridView的可见区域中的最后一行
                        int firstRowIndex = nProductIndex - visibleRowCount + 1;
                        if (firstRowIndex < 0)
                        {
                            firstRowIndex = 0;  // 确保索引不会变成负数
                        }

                        // 检查缺陷产品二维码的行数是否大于0
                        if (gridview_RecheckItemBarcode.Rows.Count <= 0)
                            return;

                        // 设置首个显示的行的索引，使选中的行为GridView的可见区域中的最后一行
                        gridview_RecheckItemBarcode.FirstDisplayedScrollingRowIndex = firstRowIndex;

                        gridview_RecheckItemBarcode.Rows[nProductIndex].Selected = true;

                        gridview_DefectsWaitingConfirm.Rows.Clear();

                        if (null != m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Nova.Products[nProductIndex].Defects)
                        {
                            for (int i = 0; i < m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Nova.Products[nProductIndex].Defects.Count; i++)
                            {
                                string strAorB = m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Nova.Products[nProductIndex].Defects[i].ChannelNum == 1 ? "A面" : "B面";

                                string info = string.Format("{0}. {1} ({2} 光{3})", i + 1, m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Nova.Products[nProductIndex].Defects[i].Type,
                                    strAorB, m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Nova.Products[nProductIndex].Defects[i].Channel);

                                gridview_DefectsWaitingConfirm.Rows.Add(info);
                            }
                        }

                        for (int i = 0; i < m_parent.m_global.m_uncheckable_pass_types.Count; i++)
                        {
                            if (m_parent.m_global.m_uncheckable_pass_enable_flags[i] == true &&
                                m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Nova.Products[nProductIndex].Defects[0].Type == m_parent.m_global.m_uncheckable_pass_types[i])
                            {
                                if (m_nCurrentDefectIndex + 1 < gridview_DefectsWaitingConfirm.Rows.Count)
                                {
                                    gridview_DefectsWaitingConfirm.Rows[1].Selected = true;
                                }
                                else
                                {
                                    m_nCurrentDefectIndex = 0;
                                }

                                m_parent.page_HomeView.BtnConfirmOK.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                            }
                        }
                    }
                }

                if (m_parent.m_global.m_strProductType == "dock")
                {
                    if (nProductIndex >= 0 && nProductIndex < m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Dock.products.Count)
                    {
                        // 确定GridView可见的行数
                        int visibleRowCount = gridview_RecheckItemBarcode.DisplayedRowCount(false);

                        // 计算首个显示的行的索引，确保选中行为GridView的可见区域中的最后一行
                        int firstRowIndex = nProductIndex - visibleRowCount + 1;
                        if (firstRowIndex < 0)
                        {
                            firstRowIndex = 0;  // 确保索引不会变成负数
                        }

                        // 检查缺陷产品二维码的行数是否大于0
                        if (gridview_RecheckItemBarcode.Rows.Count <= 0)
                            return;

                        // 设置首个显示的行的索引，使选中的行为GridView的可见区域中的最后一行
                        gridview_RecheckItemBarcode.FirstDisplayedScrollingRowIndex = firstRowIndex;

                        gridview_RecheckItemBarcode.Rows[nProductIndex].Selected = true;

                        gridview_DefectsWaitingConfirm.Rows.Clear();

                        if (null != m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Dock.products[nProductIndex].defects)
                        {
                            for (int i = 0; i < m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Dock.products[nProductIndex].defects.Count; i++)
                            {
                                var defect = m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Dock.products[nProductIndex].defects[i];
                                if (true)
                                {
                                    if (m_parent.m_global.m_strProductSubType == "glue_check")
                                    {
                                        if (string.IsNullOrEmpty(defect.type))
                                            continue;
                                    }
                                }

                                string strAorB = defect.side == 0 ? "A面" : "B面";

                                switch (defect.side)
                                {
                                    case 0:
                                        strAorB = "A面";
                                        break;
                                    case 1:
                                        strAorB = "B面";
                                        break;
                                    case 2:
                                        strAorB = "C面";
                                        break;
                                    case 3:
                                        strAorB = "D面";
                                        break;
                                }

                                string info = string.Format("{0}. {1} ({2} 光{3})", i + 1, defect.type, strAorB, defect.light_channel);

                                gridview_DefectsWaitingConfirm.Rows.Add(info);
                            }
                        }
                    }
                }
            });
        }

        // 功能：选择缺陷
        public void SelectProductDefect(int nDefectIndex)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (m_parent.m_global.m_strProductType == "nova")
                {
                    SwitchAorBSideImageIfNeededForNova(nDefectIndex);

                    int nImageWidth = m_parent.page_HomeView.m_camera_canvases[0].get_origin_image_width();
                    int nImageHeight = m_parent.page_HomeView.m_camera_canvases[0].get_origin_image_height();

                    ShowDefectOnCanvasForNova(m_nCurrentDefectedProductIndex, nDefectIndex, nImageWidth, nImageHeight);
                }
                else if (m_parent.m_global.m_strProductType == "dock")
                {
                    // 检查是否需要切换显示A面或B面的图片
                    bool bIsImageExisting = false;
                    SwitchAorBSideImageIfNeededForDock(nDefectIndex, ref bIsImageExisting);

                    int nImageWidth = m_parent.page_HomeView.m_camera_canvases[0].get_origin_image_width();
                    int nImageHeight = m_parent.page_HomeView.m_camera_canvases[0].get_origin_image_height();

                    ShowDefectOnCanvasForDock(m_nCurrentDefectedProductIndex, nDefectIndex, nImageWidth, nImageHeight);

                    // 如果图片不存在则不允许复判
                    if (true == m_parent.m_global.m_bDisableRecheckIfImageIsNotExisting)
                    {
                        if (false == bIsImageExisting)
                        {
                            m_bDisableRecheckCurrentProduct = true;

                            MessageBox.Show("当前产品缺陷没有对应图片，可能视觉电脑没保存，或者网络异常捞图失败，这将导致该产品缺陷无法复判。请查找原因。");

                            m_parent.m_global.m_log_presenter.Log("当前产品缺陷没有对应图片，可能视觉电脑没保存，或者网络异常捞图失败，这将导致该产品缺陷无法复判。请查找原因。");
                        }
                    }
                }
            });
        }

        // 功能：检查是否需要切换显示A面或B面的图片
        public void SwitchAorBSideImageIfNeededForNova(int nCurrentDefectIndex)
        {
            AVITrayInfo tray_info = m_parent.m_global.m_current_tray_info_for_Nova;

            if (tray_info.Products.Count <= 0)
                return;
            if (null == tray_info.Products[m_nCurrentDefectedProductIndex].Defects || tray_info.Products[m_nCurrentDefectedProductIndex].Defects.Count <= 0)
                return;
            if (nCurrentDefectIndex < 0 || nCurrentDefectIndex >= tray_info.Products[m_nCurrentDefectedProductIndex].Defects.Count)
                return;

            int nAorBorC = 0;

            if (tray_info.Products[m_nCurrentDefectedProductIndex].Defects.Count > 0)
            {
                if (tray_info.Products[m_nCurrentDefectedProductIndex].Defects[nCurrentDefectIndex].ChannelNum == 1)
                    nAorBorC = 1;
                else if (tray_info.Products[m_nCurrentDefectedProductIndex].Defects[nCurrentDefectIndex].ChannelNum == 2)
                    nAorBorC = 2;
                else if (tray_info.Products[m_nCurrentDefectedProductIndex].Defects[nCurrentDefectIndex].ChannelNum == 3)
                    nAorBorC = 3;
            }
            else
                return;

            int nChannel = tray_info.Products[m_nCurrentDefectedProductIndex].Defects[nCurrentDefectIndex].Channel;
            if (nChannel <= 0 || nChannel >= 1000)
                return;

            // 文件名以_nChannel结尾，表示第nChannel个光源的图像
            string strChannel = string.Format("_{0}", nChannel);
            if (m_nCurrentSideAorB > 0)
            {
                // 1表示B面，2表示C面，3表示D面，B面光源从11开始，C面光源从21开始，D面光源从31开始
                //if (m_nCurrentSideAorB == 1)
                //    strChannel = string.Format("_{0}", 10 + nChannel);
                //else if (m_nCurrentSideAorB == 2)
                //    strChannel = string.Format("_{0}", 20 + nChannel);
                //else if (m_nCurrentSideAorB == 3)
                //    strChannel = string.Format("_{0}", 30 + nChannel);
            }

            if (nAorBorC != m_nCurrentSideAorB)
            {
                m_nCurrentSideAorB = nAorBorC;
            }

            SwitchLightChannelForNova(nChannel);
        }

        // 功能：检查是否需要切换显示A面或B面的图片
        public void SwitchAorBSideImageIfNeededForDock(int nCurrentDefectIndex, ref bool bIsImageExisting)
        {
            TrayInfo tray_info = m_parent.m_global.m_current_tray_info_for_Dock;

            if (tray_info.products.Count <= 0)
                return;
            if (null == tray_info.products[m_nCurrentDefectedProductIndex].defects || tray_info.products[m_nCurrentDefectedProductIndex].defects.Count <= 0)
                return;
            if (nCurrentDefectIndex < 0 || nCurrentDefectIndex >= tray_info.products[m_nCurrentDefectedProductIndex].defects.Count)
                return;

            int nAorB = 0;

            if (tray_info.products[m_nCurrentDefectedProductIndex].defects.Count > 0)
            {
                nAorB = tray_info.products[m_nCurrentDefectedProductIndex].defects[nCurrentDefectIndex].side;
            }
            else
                return;

            if (nAorB != m_nCurrentSideAorB)
            {
                m_nCurrentSideAorB = nAorB;
            }

            // 更新一些文本框
            string[] strSides = { "A面", "B面", "C面", "D面" };
            textblock_FrontOrBack.Text = (m_nCurrentSideAorB >= 0 && m_nCurrentSideAorB < strSides.Length) ? strSides[m_nCurrentSideAorB] : "未知";

            int nChannel = tray_info.products[m_nCurrentDefectedProductIndex].defects[nCurrentDefectIndex].light_channel;
            //nChannel = 1;

            if (nChannel <= 0 || nChannel >= 1000)
                return;

            SwitchLightChannelForDock(nChannel, ref bIsImageExisting);
        }

        // 功能：切换光源图片
        public void SwitchLightChannelForNova(int nLightChannel)
        {
            AVITrayInfo tray_info = m_parent.m_global.m_current_tray_info_for_Nova;

            // 文件名以_nChannel结尾，表示第nChannel个光源的图像
            string strChannel = string.Format("_{0}", nLightChannel);

            List<string> list_image_paths = null;
            try
            {
            switch (m_nCurrentSideAorB)
            {
                case 0:
                    //if (1 == nChannel)
                    {
                        if (tray_info.Products[m_nCurrentDefectedProductIndex].m_list_local_imageA_paths_for_channel1 != null
                            && tray_info.Products[m_nCurrentDefectedProductIndex].m_list_local_imageA_paths_for_channel1.Count > 0)
                            list_image_paths = tray_info.Products[m_nCurrentDefectedProductIndex].m_list_local_imageA_paths_for_channel1;
                    }
                    //else if (2 == nChannel)
                    //{
                    //    if (tray_info.Products[m_nCurrentDefectedProductIndex].m_list_local_imageA_paths_for_channel2 != null
                    //                                   && tray_info.Products[m_nCurrentDefectedProductIndex].m_list_local_imageA_paths_for_channel2.Count > 0)
                    //        list_image_paths = tray_info.Products[m_nCurrentDefectedProductIndex].m_list_local_imageA_paths_for_channel2;
                    //}
                    break;

                case 1:
                    //if (1 == nChannel)
                    {
                        if (tray_info.Products[m_nCurrentDefectedProductIndex].m_list_local_imageA_paths_for_channel1 != null
                            && tray_info.Products[m_nCurrentDefectedProductIndex].m_list_local_imageA_paths_for_channel1.Count > 0)
                            list_image_paths = tray_info.Products[m_nCurrentDefectedProductIndex].m_list_local_imageA_paths_for_channel1;
                    }
                    //else if (2 == nChannel)
                    //{
                    //    if (tray_info.Products[m_nCurrentDefectedProductIndex].m_list_local_imageA_paths_for_channel2 != null
                    //        && tray_info.Products[m_nCurrentDefectedProductIndex].m_list_local_imageA_paths_for_channel2.Count > 0)
                    //        list_image_paths = tray_info.Products[m_nCurrentDefectedProductIndex].m_list_local_imageA_paths_for_channel2;
                    //}
                    break;

                case 2:
                    //if (1 == nChannel)
                    {
                        if (tray_info.Products[m_nCurrentDefectedProductIndex].m_list_local_imageB_paths_for_channel1 != null
                            && tray_info.Products[m_nCurrentDefectedProductIndex].m_list_local_imageB_paths_for_channel1.Count > 0)
                            list_image_paths = tray_info.Products[m_nCurrentDefectedProductIndex].m_list_local_imageB_paths_for_channel1;
                    }
                    //else if (2 == nChannel)
                    //{
                    //    if (tray_info.Products[m_nCurrentDefectedProductIndex].m_list_local_imageB_paths_for_channel2 != null
                    //        && tray_info.Products[m_nCurrentDefectedProductIndex].m_list_local_imageB_paths_for_channel2.Count > 0)
                    //        list_image_paths = tray_info.Products[m_nCurrentDefectedProductIndex].m_list_local_imageB_paths_for_channel2;
                    //}
                    break;

                case 3:
                    if (tray_info.Products[m_nCurrentDefectedProductIndex].m_list_local_imageC_paths_for_channel1 != null
                            && tray_info.Products[m_nCurrentDefectedProductIndex].m_list_local_imageC_paths_for_channel1.Count > 0)
                        list_image_paths = tray_info.Products[m_nCurrentDefectedProductIndex].m_list_local_imageC_paths_for_channel1;
                    break;
            }
            }
            catch (Exception ex)
            {
                m_parent.m_global.m_log_presenter.Log("切换光源发生异常：" + ex.Message);
            }

            // 更换显示的图片
            if (null != list_image_paths && list_image_paths.Count > 0)
            {
                //string strImagePath = list_image_paths[0];
                string strImagePath = "";

                // 寻找list_image_paths中去掉扩展名后的文件名中以strChannel结束的文件名
                foreach (string str in list_image_paths)
                {
                    // 取出文件名，去掉扩展名
                    string strFileName = System.IO.Path.GetFileNameWithoutExtension(str);

                    // 判断文件名是否以strChannel结束
                    if (strFileName.EndsWith(strChannel))
                    {
                        strImagePath = str;
                        break;
                    }
                }

                // 判断是否找到了文件，如果没有找到，则报警退出
                if (string.IsNullOrEmpty(strImagePath))
                {
                    m_parent.m_global.m_log_presenter.Log(string.Format("没有找到对应 {0} 光源的图像文件", strChannel));
                    return;
                }

                int nImageWidth = 0;
                int nImageHeight = 0;

                // 如果是扫码
                if (true == m_parent.m_global.m_bIsQueryingDatabaseAndCloningImages)
                {
                    for (int i = 0; i < 100; i++)
                    {
                        if (File.Exists(strImagePath))
                            break;

                        Thread.Sleep(20);
                    }
                }
                else
                {
                    if (false == File.Exists(strImagePath))
                    {
                        m_parent.m_global.m_log_presenter.Log(string.Format("文件{0}不存在", strImagePath));
                        return;
                    }
                }

                // 等待文件生成
                for (int i = 0; i < 100; i++)
                {
                    if (File.Exists(strImagePath))
                    {
                        if (true == m_parent.m_global.m_bIsQueryingDatabaseAndCloningImages)
                        {
                            try
                            {
                                {
                                    Bitmap bmp = new Bitmap(strImagePath);
                                    if (bmp == null || bmp.Width == 0)
                                        continue;
                                }
                            }
                            catch (Exception ex)
                            {
                            }
                        }

                        break;
                    }

                    //await Task.Delay(100);
                    Thread.Sleep(100);
                }

                // 导航图画布切换显示的图片
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (m_parent.m_global.m_strLastOpenImagePath == strImagePath)
                    {
                        return;
                    }

                    BaseCanvas<CameraCanvas>.OpenImageFileAndShowOnBIVCanvas(strImagePath, m_parent.m_global.m_nImageRotationAngle, m_parent.page_HomeView.m_camera_canvases[0]);

                    m_parent.m_global.m_strLastOpenImagePath = strImagePath;

                    nImageWidth = m_parent.page_HomeView.m_camera_canvases[0].get_origin_image_width();
                    nImageHeight = m_parent.page_HomeView.m_camera_canvases[0].get_origin_image_height();

                    m_parent.page_HomeView.m_camera_canvases[0].show_whole_image();
                });
            }
        }

        // 功能：切换光源图片
        public void SwitchLightChannelForDock(int nLightChannel, ref bool bIsImageExisting)
        {
            TrayInfo tray_info = m_parent.m_global.m_current_tray_info_for_Dock;

            if (tray_info.products == null)
                return;
            if (nLightChannel <= 0 || nLightChannel >= 1000)
                return;

            // 文件名以_nChannel结尾，表示第nChannel个光源的图像
            string strChannel = string.Format("_{0}", nLightChannel);
            if (m_nCurrentSideAorB > 0)
            {
                // 1表示B面，2表示C面，3表示D面，B面光源从11开始，C面光源从21开始，D面光源从31开始
                //if (m_nCurrentSideAorB == 1)
                //    strChannel = string.Format("_{0}", 10 + nChannel);
                //else if (m_nCurrentSideAorB == 2)
                //    strChannel = string.Format("_{0}", 20 + nChannel);
                //else if (m_nCurrentSideAorB == 3)
                //    strChannel = string.Format("_{0}", 30 + nChannel);
            }

            List<string> list_image_paths = null;
            switch (m_nCurrentSideAorB)
            {
                case 0:
                    //if (1 == nChannel)
                    {
                        if (tray_info.products[m_nCurrentDefectedProductIndex].m_list_local_imageA_paths_for_channel1 != null
                            && tray_info.products[m_nCurrentDefectedProductIndex].m_list_local_imageA_paths_for_channel1.Count > 0)
                            list_image_paths = tray_info.products[m_nCurrentDefectedProductIndex].m_list_local_imageA_paths_for_channel1;
                    }
                    break;

                case 1:
                    //if (1 == nChannel)
                    {
                        if (tray_info.products[m_nCurrentDefectedProductIndex].m_list_local_imageB_paths_for_channel1 != null
                            && tray_info.products[m_nCurrentDefectedProductIndex].m_list_local_imageB_paths_for_channel1.Count > 0)
                            list_image_paths = tray_info.products[m_nCurrentDefectedProductIndex].m_list_local_imageB_paths_for_channel1;
                    }
                    break;

                case 2:
                    //if (1 == nChannel)
                    {
                        if (tray_info.products[m_nCurrentDefectedProductIndex].m_list_local_imageC_paths_for_channel1 != null
                            && tray_info.products[m_nCurrentDefectedProductIndex].m_list_local_imageC_paths_for_channel1.Count > 0)
                            list_image_paths = tray_info.products[m_nCurrentDefectedProductIndex].m_list_local_imageC_paths_for_channel1;
                    }
                    break;

                case 3:
                    //if (1 == nChannel)
                    {
                        if (tray_info.products[m_nCurrentDefectedProductIndex].m_list_local_imageD_paths_for_channel1 != null
                            && tray_info.products[m_nCurrentDefectedProductIndex].m_list_local_imageD_paths_for_channel1.Count > 0)
                            list_image_paths = tray_info.products[m_nCurrentDefectedProductIndex].m_list_local_imageD_paths_for_channel1;
                    }
                    break;
            }

            // 更换显示的图片
            if (null != list_image_paths && list_image_paths.Count > 0)
            {
                string strImagePath = "";

                // 寻找list_image_paths中去掉扩展名后的文件名中以strChannel结束的文件名
                foreach (string str in list_image_paths)
                {
                    // 取出文件名，去掉扩展名
                    string strFileName = System.IO.Path.GetFileNameWithoutExtension(str);

                    // 判断文件名是否以strChannel结束
                    if (strFileName.EndsWith(strChannel))
                    {
                        strImagePath = str;
                        break;
                    }
                }

                // 判断是否找到了文件，如果没有找到，则报警退出
                if (string.IsNullOrEmpty(strImagePath))
                {
                    ClearImageControlContent();

                    m_parent.m_global.m_log_presenter.Log(string.Format("没有找到对应 {0} 光源的图像文件", strChannel));

                    return;
                }

                int nImageWidth = 0;
                int nImageHeight = 0;

                //if (strImagePath != m_parent.m_global.m_strLastOpenImagePath)
                {
                    int nWaitIterations = 100;
                    if (m_parent.m_global.m_strProductSubType == "glue_check")
                    {
                        nWaitIterations = 10;
                    }

                    // 等待文件生成
                    for (int i = 0; i < nWaitIterations; i++)
                    {
                        if (File.Exists(strImagePath))
                        {
                            if (true == m_parent.m_global.m_bIsQueryingDatabaseAndCloningImages)
                            {
                                try
                                {
                                    Bitmap bmp = new Bitmap(strImagePath);
                                    if (bmp == null || bmp.Width == 0)
                                        continue;
                                }
                                catch (Exception ex)
                                {
                                }
                            }

                            break;
                        }

                        //await Task.Delay(100);
                        Thread.Sleep(100);
                    }

                    // 判断是否找到了文件，如果没有找到，则报警退出
                    if (false == File.Exists(strImagePath))
                    {
                        strImagePath = strImagePath.Replace(@"\B\", @"\A\");

                        if (false == File.Exists(strImagePath))
                        {
                            ClearImageControlContent();

                            m_parent.m_global.m_log_presenter.Log(string.Format("文件名为{0}的图像文件不存在", strImagePath));

                            bIsImageExisting = false;

                            return;
                        }
                    }

                    bIsImageExisting = true;

                    // 导航图画布切换显示的图片
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        BaseCanvas<CameraCanvas>.OpenImageFileAndShowOnBIVCanvas(strImagePath, m_parent.m_global.m_nImageRotationAngle, m_parent.page_HomeView.m_camera_canvases[0]);

                        m_parent.m_global.m_strLastOpenImagePath = strImagePath;

                        nImageWidth = m_parent.page_HomeView.m_camera_canvases[0].get_origin_image_width();
                        nImageHeight = m_parent.page_HomeView.m_camera_canvases[0].get_origin_image_height();

                        m_parent.page_HomeView.m_camera_canvases[0].show_whole_image();
                    });
                }
            }
        }

        // 功能：根据权限状态更新操作权限
        public void UpdateOperatorRight()
        {
            //if (m_parent.m_global.m_bHasSuperRight)
            {
                combo_RecheckDisplayMode.IsEnabled = true;
                combo_TurnOnMesUpload.IsEnabled = true;
                btn_ClearAllRecheckData.IsEnabled = true;
                BtnClearOperatorID.IsEnabled = true;
                combo_SpecialTrayCornerMode.IsEnabled = true;
                BtnClearCountButtons.IsEnabled = true;
                BtnClearProductButtons.IsEnabled = true;
                combo_SelectMachine.IsEnabled = true;
                CheckBox_QueryByMachine.IsEnabled = true;
                combo_RotationAngle.IsEnabled = true;
                combo_QueryByShortDate.IsEnabled = true;
                combo_QueryTime.IsEnabled = true;
                combo_TurnOnNonreevaluableItem.IsEnabled = true;
                BtnClearProductCount.IsEnabled = true;

                m_parent.page_HomeView.Visibility = Visibility.Visible;
                m_parent.toolBar_MainWindow.Visibility = Visibility.Visible;
                m_parent.statusBar_MainWindow.Visibility = Visibility.Visible;
            }
            /*
            else
            {
                //combo_RecheckDisplayMode.IsEnabled = false;
                combo_TurnOnMesUpload.IsEnabled = false;
                btn_ClearAllRecheckData.IsEnabled = false;
                BtnClearOperatorID.IsEnabled = false;
                combo_SpecialTrayCornerMode.IsEnabled = false;
                BtnClearCountButtons.IsEnabled = false;
                BtnClearProductButtons.IsEnabled = false;
                combo_RotationAngle.IsEnabled = false;
                combo_QueryByShortDate.IsEnabled = false;
                combo_QueryTime.IsEnabled = false;
                combo_TurnOnNonreevaluableItem.IsEnabled = false;

                if (m_parent.m_global.m_strProductSubType != "glue_check")
                {
                    //combo_SelectMachine.IsEnabled = false;
                    //CheckBox_QueryByMachine.IsEnabled = false;
                }

                if (m_parent.m_global.m_strProductType != "nova")
                {
                    BtnClearProductCount.IsEnabled = false;
                }

                if (m_parent.m_global.m_station_type == "transfer")
                {
                    m_parent.page_HomeView.Visibility = Visibility.Hidden;
                    m_parent.toolBar_MainWindow.Visibility = Visibility.Hidden;
                    m_parent.statusBar_MainWindow.Visibility = Visibility.Hidden;
                }
            }
            */
        }

        // 功能：修改当前缺陷名称
        public void ModifyCurrentDefectName()
        {
            int nProductIndex = m_nCurrentDefectedProductIndex;
            int nDefectIndex = m_nCurrentDefectIndex;

            if (m_parent.m_global.m_strProductType == "nova")
            {
                if (m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Nova != null)
                {
                    if (nProductIndex >= 0 && nProductIndex < m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Nova.Products.Count)
                    {
                        if (m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Nova.Products[nProductIndex].Defects != null)
                        {
                            if (nDefectIndex >= 0 && nDefectIndex < m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Nova.Products[nProductIndex].Defects.Count)
                            {
                                // 获取当前缺陷名称
                                string strDefectName = m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Nova.Products[nProductIndex].Defects[nDefectIndex].Type;

                                // 弹出修改缺陷名称的窗口
                                ModifyDefectName window = new ModifyDefectName(m_parent, strDefectName);

                                window.ShowDialog();

                                if (window.m_bConfirm == true)
                                {
                                    // 修改缺陷名称
                                    m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Nova.Products[nProductIndex].Defects[nDefectIndex].Type = window.m_strNewDefectName;

                                    // 更新显示
                                    gridview_DefectsWaitingConfirm.Rows.Clear();

                                    for (int i = 0; i < m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Nova.Products[nProductIndex].Defects.Count; i++)
                                    {
                                        string strAorB = m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Nova.Products[nProductIndex].Defects[i].ChannelNum == 1 ? "A面" : "B面";

                                        string info = string.Format("{0}. {1} ({2} 光{3})", i + 1, m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Nova.Products[nProductIndex].Defects[i].Type,
                                                                                       strAorB, m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Nova.Products[nProductIndex].Defects[i].Channel);

                                        gridview_DefectsWaitingConfirm.Rows.Add(info);
                                    }

                                    SelectProductDefect(nDefectIndex);
                                }
                            }
                        }
                    }
                }
            }
        }

        // 功能：打开图片，并在画布上显示
        public void OpenAndShowImageOnCanvas(string strImagePath, int nCanvasIndex)
        {
            BaseCanvas<CameraCanvas>.OpenImageFileAndShowOnBIVCanvas(strImagePath, 0, m_camera_canvases[nCanvasIndex]);

            m_camera_canvases[nCanvasIndex].clear_all_search_rects();

            Application.Current.Dispatcher.Invoke(() =>
            {
                m_camera_canvases[nCanvasIndex].show_whole_image();
            });
        }

        public void FillBarcodeFromAVIForAIMode3(string barcode)
        {
            textbox_ScannedBarcode.Text = barcode;

            BtnQueryData.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        }

        private void BtnClearCountButtons_Click(object sender, RoutedEventArgs e)
        {
            var dlg = MessageBox.Show("是否清空所有统计？", "提示", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (dlg == MessageBoxResult.Yes)
            {
                ClearCount();
            }
        }

        private void ClearCount()
        {
            m_parent.m_global.m_recheck_statistics.Backup();//备份上一次的数据
            m_parent.m_global.m_recheck_statistics.m_nTotalPanels = 0;                              // 总盘数
            m_parent.m_global.m_recheck_statistics.m_nTotalRecheckedProducts = 0;         // 总复判产品数
            m_parent.m_global.m_recheck_statistics.m_nRecheckedOKs = 0;                        // 复判OK点数
            m_parent.m_global.m_recheck_statistics.m_nRecheckedNGs = 0;                       // 复判NG点数
            m_parent.m_global.m_recheck_statistics.m_okpanel = 0; //ok盘数
            m_parent.m_global.m_recheck_statistics.m_ngpanel = 0; //ng盘数
            m_parent.m_global.m_recheck_statistics.m_okproduct = 0;//ok产品数
            m_parent.m_global.m_recheck_statistics.m_ngproduct = 0;//ng产品数
            m_parent.m_global.m_recheck_statistics.m_alldefects = 0;//总缺陷点数
            m_parent.m_global.m_recheck_statistics.m_confirmOKs = 0; // 复判OK数
            m_parent.m_global.m_recheck_statistics.m_confirmNGs = 0; //复判NG数
            changedcount();
        }

        private void combo_SpecialTrayCornerMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (m_parent == null)
                return;
            //if (m_parent.m_global.m_bIsMainWindowInited == false)
            //    return;

            m_parent.m_global.m_nSpecialTrayCornerMode = combo_SpecialTrayCornerMode.SelectedIndex;
            grid_TrayCornerLefttop.Children.Clear();
            grid_TrayCornerRighttop.Children.Clear();

            switch (m_parent.m_global.m_nSpecialTrayCornerMode)
            {
                case 0:
                    grid_TrayCornerLefttop.Children.Clear();
                    grid_TrayCornerRighttop.Children.Clear();
                    break;

                case 1:
                case 3:
                    if (true)
                    {
                        // 在grid_TrayCornerLefttop画一个斜三角形
                        var path = new System.Windows.Shapes.Path
                        {
                            Stroke = System.Windows.Media.Brushes.Black,
                            Fill = System.Windows.Media.Brushes.SkyBlue,
                            StrokeThickness = 2
                        };

                        var pathGeometry = new PathGeometry();
                        var pathFigure = new PathFigure
                        {
                            StartPoint = new System.Windows.Point((int)grid_TrayCornerLefttop.ActualWidth, 0)  // 左上角
                        };

                        // 添加线到右下角
                        pathFigure.Segments.Add(new LineSegment(new System.Windows.Point((int)grid_TrayCornerLefttop.ActualWidth, (int)grid_TrayCornerLefttop.ActualHeight), true));
                        // 添加线到左下角
                        pathFigure.Segments.Add(new LineSegment(new System.Windows.Point(0, (int)grid_TrayCornerLefttop.ActualHeight), true));
                        // 关闭路径
                        pathFigure.IsClosed = true;

                        pathGeometry.Figures.Add(pathFigure);
                        path.Data = pathGeometry;

                        grid_TrayCornerLefttop.Children.Add(path);

                        grid_TrayCornerRighttop.Children.Clear();
                    }
                    break;

                case 2:
                    if (true)
                    {
                        // 在grid_TrayCornerRighttop画一个斜三角形
                        var path = new System.Windows.Shapes.Path
                        {
                            Stroke = System.Windows.Media.Brushes.Black,
                            Fill = System.Windows.Media.Brushes.SkyBlue,
                            StrokeThickness = 2
                        };

                        var pathGeometry = new PathGeometry();
                        var pathFigure = new PathFigure
                        {
                            StartPoint = new System.Windows.Point((int)0, 0)  // 左上角
                        };

                        // 添加线到右下角
                        pathFigure.Segments.Add(new LineSegment(new System.Windows.Point((int)grid_TrayCornerRighttop.ActualWidth, (int)grid_TrayCornerRighttop.ActualHeight), true));
                        // 添加线到左下角
                        pathFigure.Segments.Add(new LineSegment(new System.Windows.Point(0, (int)grid_TrayCornerRighttop.ActualHeight), true));
                        // 关闭路径
                        pathFigure.IsClosed = true;

                        pathGeometry.Figures.Add(pathFigure);
                        path.Data = pathGeometry;

                        grid_TrayCornerRighttop.Children.Add(path);

                        grid_TrayCornerLefttop.Children.Clear();
                    }
                    break;

                case 4:
                    if (true)
                    {
                        // 在grid_TrayCornerLefttop画一个斜三角形
                        var path = new System.Windows.Shapes.Path
                        {
                            Stroke = System.Windows.Media.Brushes.Black,
                            Fill = System.Windows.Media.Brushes.SkyBlue,
                            StrokeThickness = 2
                        };

                        var pathGeometry = new PathGeometry();
                        var pathFigure = new PathFigure
                        {
                            StartPoint = new System.Windows.Point(0, 0)  // 左上角
                            //StartPoint = new System.Windows.Point((int)grid_TrayCornerLefttop.ActualWidth, 0)  // 左上角
                        };

                        // 添加线到右下角
                        pathFigure.Segments.Add(new LineSegment(new System.Windows.Point((int)grid_TrayCornerLefttop.ActualWidth, 0), true));
                        //pathFigure.Segments.Add(new LineSegment(new System.Windows.Point((int)grid_TrayCornerLefttop.ActualWidth, (int)grid_TrayCornerLefttop.ActualHeight), true));
                        // 添加线到左下角
                        pathFigure.Segments.Add(new LineSegment(new System.Windows.Point((int)grid_TrayCornerLefttop.ActualWidth, (int)grid_TrayCornerLefttop.ActualHeight), true));
                        //pathFigure.Segments.Add(new LineSegment(new System.Windows.Point(0, (int)grid_TrayCornerLefttop.ActualHeight), true));
                        // 关闭路径
                        pathFigure.IsClosed = true;

                        pathGeometry.Figures.Add(pathFigure);
                        path.Data = pathGeometry;

                        grid_TrayCornerLefttop.Children.Add(path);

                        grid_TrayCornerRighttop.Children.Clear();
                    }
                    break;

                case 5:
                    if (true)
                    {
                        // 在grid_TrayCornerRighttop画一个斜三角形
                        var path = new System.Windows.Shapes.Path
                        {
                            Stroke = System.Windows.Media.Brushes.Black,
                            Fill = System.Windows.Media.Brushes.SkyBlue,
                            StrokeThickness = 2
                        };

                        var pathGeometry = new PathGeometry();
                        var pathFigure = new PathFigure
                        {
                            StartPoint = new System.Windows.Point((int)0, 0)  // 左上角
                        };

                        // 添加线到右下角
                        pathFigure.Segments.Add(new LineSegment(new System.Windows.Point((int)grid_TrayCornerRighttop.ActualWidth, (int)grid_TrayCornerRighttop.ActualHeight), true));
                        // 添加线到左下角
                        pathFigure.Segments.Add(new LineSegment(new System.Windows.Point(0, (int)grid_TrayCornerRighttop.ActualHeight), true));
                        // 关闭路径
                        pathFigure.IsClosed = true;

                        pathGeometry.Figures.Add(pathFigure);
                        path.Data = pathGeometry;

                        grid_TrayCornerRighttop.Children.Add(path);

                        grid_TrayCornerLefttop.Children.Clear();
                    }
                    break;
            }
        }

        // 复选框：勾选按机台查询
        private void CheckBox_QueryByMachine_Checked(object sender, RoutedEventArgs e)
        {
            m_parent.m_global.m_bSelectByMachine = true;
        }

        // 复选框：取消勾选按机台查询
        private void CheckBox_QueryByMachine_Unchecked(object sender, RoutedEventArgs e)
        {
            m_parent.m_global.m_bSelectByMachine = false;
        }

        // 组合框：选择机台
        private void combo_SelectMachine_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (m_parent == null)
                return;
            //if (m_parent.m_global.m_bIsMainWindowInited == false)
            //    return;

            if (combo_SelectMachine.SelectedIndex >= 0)
            {
                m_parent.m_global.m_nSelectedMachineIndexForDatabaseQuery = combo_SelectMachine.SelectedIndex;
            }
        }

        private void BtnClearAllData_Click(object sender, RoutedEventArgs e)
        {
            if (true == m_parent.m_global.m_bIsConnectedToDatabase)
            {
                int nTruncateSuccessfulCount = 0;

                // 弹出确认对话框
                if (MessageBox.Show("确定要清空所有数据？清空后不可撤回，敏感操作请联系管理人员确认！", "再次确认", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                    return;

                // 遍历m_global.m_dict_machine_names_and_IPs的keys，创建表格
                for (int t = 0; t < m_parent.m_global.m_dict_machine_names_and_IPs.Count; t++)
                {
                    string strMachineName = m_parent.m_global.m_dict_machine_names_and_IPs.Keys.ElementAt(t);

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
                        for (int n = 0; n < strTables.Length; n++)
                        {
                            // 先查询表格是否存在
                            if (false == m_parent.m_global.m_mysql_ops.IsTableExist("AutoScanFQCTest", strTables[n]))
                            {
                                continue;
                            }

                            if (true == m_parent.m_global.m_mysql_ops.TruncateTableData(strTables[n]))
                            {
                                nTruncateSuccessfulCount++;
                            }
                        }
                    }
                }

                if (nTruncateSuccessfulCount == 6)
                {
                    MessageBox.Show("清空复判数据成功");
                }
                else if (nTruncateSuccessfulCount == 0)
                {
                    MessageBox.Show("清空复判数据失败！");
                }
                else
                {
                    MessageBox.Show("复判数据未完全清除！");
                }

                ClearProductInfo();

                GenerateCircularButtonsInGridContainer(grid_CircularButtonContainer, 0, 0, null);
            }
        }

        // 组合框：未检料盘
        private void combo_SelectUncheckedTrays_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (m_parent == null)
                return;

            if (combo_SelectUncheckedTrays.SelectedIndex >= 0)
            {
                string strSelectedText = "";
                if (combo_SelectUncheckedTrays.SelectedItem == null)
                {
                    return;
                }
                else
                {
                    // 得到选择项目的文本
                    strSelectedText = combo_SelectUncheckedTrays.SelectedItem.ToString();
                }

                // 扫码
                textbox_ScannedBarcode.Text = strSelectedText;

                // 处理扫描到的条码
                BtnRequestData_Click(new object(), new RoutedEventArgs());
            }
        }

        // 组合框：未检料盘
        private void combo_SelectUncheckedTrays_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        // 组合框：未检料盘
        private void combo_SelectUncheckedTrays_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        // 组合框：选择复判显示模式
        private void combo_RecheckDisplayMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (m_parent == null)
                return;

            m_parent.m_global.m_nRecheckDisplayMode = combo_RecheckDisplayMode.SelectedIndex;
        }

        // 线程：配置文件监视
        private void thread_execute_query_with_delay()
        {
         if (m_parent.m_global.m_nRecheckModeWithAISystem == 3) {
            return;
         }

            Thread.Sleep(300);

            Application.Current.Dispatcher.Invoke(() =>
            {
                string strTemp = textbox_ScannedBarcode.Text.Trim();

                if (strTemp.Length > 5)
                {
                    if (m_strCurrentBarcode != strTemp)
                    {
                        m_strCurrentBarcode = strTemp;

                        // 处理扫描到的条码
                        //if (m_parent.m_global.m_recheck_data_query_mode != RecheckDataQueryMode.ByQueryUncheckedFlag)
                        if (m_parent.m_global.m_strProductSubType != "glue_check")
                        {
                            BtnRequestData_Click(new object(), new RoutedEventArgs());
                        }

                        // 使文本框失去焦点
                        Keyboard.ClearFocus();

                        // 恢复键盘焦点
                        Keyboard.Focus(textbox_RunningInfo);
                    }
                }
            });
        }

        // 线程：AI复判提交，纯AI复判模式
        private void thread_AI_recheck_submit()
        {
            //m_reset_event.Set();

            m_bIsAIRecheckAndSubmissionThreadRunning = true;

            // 打印当前线程ID
            Debugger.Log(0, "", string.Format("111111 AI复判提交线程ID：{0}, 111", Thread.CurrentThread.ManagedThreadId));

            if (m_parent.m_global.m_bEnableMESValidationForOriginalNGProducts)
            {
                while (m_bIsValidatingWithMES)
                {
                    Thread.Sleep(3000);
                }
            }

            while (m_parent.m_global.m_bIsImagesCloningThreadInProcess[0] == true)
            {
                Thread.Sleep(3000);
            }

            // 打印当前线程ID
            Debugger.Log(0, "", string.Format("111111 AI复判提交线程ID：{0}, 222", Thread.CurrentThread.ManagedThreadId));

            for (int i = 0; i < m_parent.m_global.m_current_MES_tray_info.m_array_indices_of_defected_products.Length; i++)
            {
                m_nCurrentProductIndex = m_parent.m_global.m_current_MES_tray_info.m_array_indices_of_defected_products[i];

                ProductInfo product = m_parent.m_global.m_current_tray_info_for_Dock.products[i];

                // 打印当前线程ID
                Debugger.Log(0, "", string.Format("111111 AI复判提交线程ID：{0}, i = {1}, aaa", Thread.CurrentThread.ManagedThreadId, i));

                // 选中产品
                Application.Current.Dispatcher.Invoke(() =>
                {
                    SelectProduct(m_nCurrentProductIndex);
                });

                // 打印当前线程ID
                Debugger.Log(0, "", string.Format("111111 AI复判提交线程ID：{0}, i = {1}, bbb", Thread.CurrentThread.ManagedThreadId, i));

                // 请求该产品的AI复判结果
                RecheckResult[] check_results = new RecheckResult[product.defects.Count];
                if (true)
                {
                    var defectResults = new List<AIDefect>();

                    if (m_parent.m_global.m_strSiteCity == "苏州")
                    {
                        var response = new MesAIProductInfoSZ();

                        if (!m_parent.m_global.m_MES_service.SubmitUnrecheckProductInfoToAI(product, ref response))
                        {
                            m_parent.m_global.m_log_presenter.Log($"向提交AI复判提交第{m_nCurrentProductIndex}个NG产品的信息失败，失败信息：{response.finalRes}。可无视信息继续人工复判");
                        }
                        else
                        {
                            if (response != null)
                            {
                                m_parent.m_global.m_current_MES_tray_info.m_check_flags_by_product[m_nCurrentProductIndex] = RecheckResult.OK;

                                if (!m_parent.m_global.m_current_MES_tray_info.m_dictionary_barcode_with_mesAIProductInfoSZ.ContainsKey(response.barCode))
                                {
                                    m_parent.m_global.m_current_MES_tray_info.m_dictionary_barcode_with_mesAIProductInfoSZ.Add(response.barCode, response);
                                }

                                m_parent.m_global.m_log_presenter.Log($"后台线程：已提交第{m_nCurrentProductIndex}个NG产品SN：{product.barcode}信息到AI复判");

                                if (response.pointPosInfo.side_A != null)
                                {
                                    response.pointPosInfo.side_A.ForEach(cp => defectResults.AddRange(cp.defects));
                                }
                                if (response.pointPosInfo.side_B != null)
                                {
                                    response.pointPosInfo.side_B.ForEach(cp => defectResults.AddRange(cp.defects));
                                }
                                if (response.pointPosInfo.side_C != null)
                                {
                                    response.pointPosInfo.side_C.ForEach(cp => defectResults.AddRange(cp.defects));
                                }

                                for (int j = 0; j < defectResults.Count(); j++)
                                {
                                    check_results[j] = defectResults[j].aiResult == "OK" ? RecheckResult.OK : RecheckResult.NG;

                                    if (check_results[j] == RecheckResult.NG)
                                    {
                                        m_parent.m_global.m_current_MES_tray_info.m_check_flags_by_product[m_nCurrentProductIndex] = RecheckResult.NG;
                                    }
                                }
                            }
                        }
                    }
                    else if (m_parent.m_global.m_strSiteCity == "盐城")
                    {
                        var response = new MesAIProductInfoYC();
                        if (!m_parent.m_global.m_MES_service.SubmitUnrecheckProductInfoToAI(product, ref response))
                        {
                            m_parent.m_global.m_log_presenter.Log($"向提交AI复判提交第{m_nCurrentProductIndex}个NG产品的信息失败，失败信息：{response.finalRes}。可无视信息继续人工复判");

                            response.aiFinalRes = "";

                            if (response.pointPosInfo != null)
                            {
                                foreach (var defect in response.pointPosInfo.side_A.cam0Pos0.defects)
                                {
                                    defect.aiResult = "";
                                    defect.aiGuid = "";
                                }

                                foreach (var defect in response.pointPosInfo.side_B.cam0Pos0.defects)
                                {
                                    defect.aiResult = "";
                                    defect.aiGuid = "";
                                }

                                if (!m_parent.m_global.m_current_MES_tray_info.m_dictionary_barcode_with_mesAIProductInfoYC.ContainsKey(response.barCode))
                                {
                                    m_parent.m_global.m_current_MES_tray_info.m_dictionary_barcode_with_mesAIProductInfoYC.Add(response.barCode, response);
                                }

                                for (int j = 0; i < check_results.Length; i++)
                                {
                                    check_results[j] = RecheckResult.NG;
                                }

                                m_parent.m_global.m_current_MES_tray_info.m_check_flags_by_product[m_nCurrentProductIndex] = RecheckResult.NG;
                            }
                        }
                        else
                        {
                            if (response != null)
                            {
                                m_parent.m_global.m_current_MES_tray_info.m_check_flags_by_product[m_nCurrentProductIndex] = RecheckResult.OK;

                                if (!m_parent.m_global.m_current_MES_tray_info.m_dictionary_barcode_with_mesAIProductInfoYC.ContainsKey(response.barCode))
                                {
                                    m_parent.m_global.m_current_MES_tray_info.m_dictionary_barcode_with_mesAIProductInfoYC.Add(response.barCode, response);
                                }

                                m_parent.m_global.m_log_presenter.Log($"后台线程：已提交第{m_nCurrentProductIndex}个NG产品SN：{product.barcode}信息到AI复判");

                                if (response.pointPosInfo.side_A != null)
                                {
                                    defectResults.AddRange(response.pointPosInfo.side_A.cam0Pos0.defects);
                                }
                                if (response.pointPosInfo.side_B != null)
                                {
                                    defectResults.AddRange(response.pointPosInfo.side_B.cam0Pos0.defects);
                                }

                                for (int j = 0; j < defectResults.Count(); j++)
                                {
                                    if (defectResults[j].aiResult == "OK" || defectResults[j].aiResult == "PASS")
                                    {
                                        check_results[j] = RecheckResult.OK;
                                    }
                                    else if (defectResults[j].aiResult == "NG" || defectResults[j].aiResult == "FAIL")
                                    {
                                        check_results[j] = RecheckResult.NG;
                                    }
                                    else
                                    {
                                        check_results[j] = RecheckResult.NG;
                                    }
                                }

                                if (response.aiFinalRes == "OK" || response.aiFinalRes == "PASS")
                                {
                                    m_parent.m_global.m_current_MES_tray_info.m_check_flags_by_product[m_nCurrentProductIndex] = RecheckResult.OK;
                                }
                                else if (response.aiFinalRes == "NG" || response.aiFinalRes == "FAIL")
                                {
                                    m_parent.m_global.m_current_MES_tray_info.m_check_flags_by_product[m_nCurrentProductIndex] = RecheckResult.NG;
                                }
                                else
                                {
                                    m_parent.m_global.m_current_MES_tray_info.m_check_flags_by_product[m_nCurrentProductIndex] = RecheckResult.NG;
                                }
                            }
                        }

                        try
                        {
                            string strJsonFolderPath = "D:\\aiRecheckSubmitData\\Responses";

                            strJsonFolderPath = Path.Combine(strJsonFolderPath, m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Dock.set_id);
                            if (!Directory.Exists(strJsonFolderPath))
                                Directory.CreateDirectory(strJsonFolderPath);

                            string strJsonFileName = "";
                            strJsonFileName = $"{product.barcode}.json";
                            string strJsonPath = Path.Combine(strJsonFolderPath, strJsonFileName);

                            if (!File.Exists(strJsonPath))
                            {
                                File.Create(strJsonPath).Close();
                            }
                            else
                            {
                                File.Delete(strJsonPath);
                                File.Create(strJsonPath).Close();
                            }

                            File.WriteAllText(strJsonPath, JsonConvert.SerializeObject(response));
                        }
                        catch (Exception e)
                        {
                            m_parent.m_global.m_log_presenter.Log("保存AI提交返回结果异常：" + e.Message);
                        }

                    }
                }

                // 打印当前线程ID
                Debugger.Log(0, "", string.Format("111111 AI复判提交线程ID：{0}, i = {1}, ccc", Thread.CurrentThread.ManagedThreadId, i));

                // 根据AI复判结果，自动填入缺陷复判结果
                if (i < m_parent.m_global.m_current_MES_tray_info.m_list_recheck_flags_by_defect.Count())
                {
					m_parent.m_global.m_current_MES_tray_info.m_list_recheck_flags_by_defect[i] = check_results;
				}

                // 打印当前线程ID
                Debugger.Log(0, "", string.Format("111111 AI复判提交线程ID：{0}, i = {1}, ddd", Thread.CurrentThread.ManagedThreadId, i));

				// 更新UI
                Application.Current.Dispatcher.Invoke(() =>
                {
                    UpdateUIWithProductInfoForDock(m_parent.m_global.m_current_tray_info_for_Dock, i);

                    render_tray_button_color();
                });

                // 打印当前线程ID
                Debugger.Log(0, "", string.Format("111111 AI复判提交线程ID：{0}, i = {1}, eee", Thread.CurrentThread.ManagedThreadId, i));

                // 等待一段时间
                Thread.Sleep(900);
            }

            Thread.Sleep(1000);

            if (m_parent.m_global.m_bEnableAIAutoSubmit)
            {
                for (int n = 0; n < 4; n++)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        BtnSubmit.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    });

                    if (3 == n)
                        break;
                    if (0 == m_bIsAIRechecking)
                        break;

                    Thread.Sleep(10000);
                }
            }

            // 打印当前线程ID
            Debugger.Log(0, "", string.Format("111111 AI复判提交线程ID：{0}, 333", Thread.CurrentThread.ManagedThreadId));

            m_bIsAIRecheckAndSubmissionThreadRunning = false;
        }

        public void thread_AI_wait_to_recheck_monitor()
        {
            var waitToRecheckFilesDirectory = @"D:\fupanAutoAIData\WaitToRecheck";
            var aiSubmitMESUnsuccessfulDirectory = @"D:\fupanAutoAIData\SubmitUnsuccessful";

            // 文件名和提交次数
            Dictionary<string, int> dict_file_names_and_submit_counts_for_successful_recheck = new Dictionary<string, int>();
            Dictionary<string, int> dict_file_names_and_submit_counts_for_unsuccessful_recheck = new Dictionary<string, int>();

            while (true)
            {
                m_bIsAIRechecking = 1;

                if (!Directory.Exists(waitToRecheckFilesDirectory))
                {
                    Directory.CreateDirectory(waitToRecheckFilesDirectory);
                }

				var barcodeFiles = Directory.GetFiles(waitToRecheckFilesDirectory, $"*.bcd", SearchOption.TopDirectoryOnly);

				if (barcodeFiles.Count() > 0)
                {
                    var barcode = System.IO.Path.GetFileNameWithoutExtension(barcodeFiles.First());

                    // 如果字典元素超过100个，则清空
                    if (dict_file_names_and_submit_counts_for_successful_recheck.Count > 100)
                    {
                        dict_file_names_and_submit_counts_for_successful_recheck.Clear();
                    }

                    // 统计文件名和提交次数
                    string strFileName = System.IO.Path.GetFileName(barcodeFiles.First());
                    if (dict_file_names_and_submit_counts_for_successful_recheck.ContainsKey(strFileName))
                    {
                        dict_file_names_and_submit_counts_for_successful_recheck[strFileName]++;

                        // 如果提交次数超过3次，则把文件移动到D:\\fupanSubmitTooManyTimes文件夹
                        if (dict_file_names_and_submit_counts_for_successful_recheck[strFileName] > 3)
                        {
                            var tooManyTimesDirectory = @"D:\fupanSubmitTooManyTimes";
                            if (!Directory.Exists(tooManyTimesDirectory))
                            {
                                Directory.CreateDirectory(tooManyTimesDirectory);
                            }

                            // 移动文件，如果文件已经存在，则删除
                            var tooManyTimesPath = System.IO.Path.Combine(tooManyTimesDirectory, strFileName);
                            if (false == File.Exists(tooManyTimesPath))
                            {
                                File.Move(barcodeFiles.First(), tooManyTimesPath);
                            }
                            else
                            {
                                File.Delete(barcodeFiles.First());
                            }

                            // 删除字典中的记录
                            dict_file_names_and_submit_counts_for_successful_recheck.Remove(strFileName);

                            continue;
                        }
                    }
                    else
                    {
                        dict_file_names_and_submit_counts_for_successful_recheck.Add(strFileName, 1);
                    }

                    Debugger.Log(0, "", string.Format("111111 启动AI复判提交线程，条码：{0}", barcode));

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        m_parent.page_HomeView.FillBarcodeFromAVIForAIMode3(Convert.ToString(barcode));
                    });

                    //m_reset_event.Reset();
                    //if (false == m_reset_event.WaitOne(15000))
                    //    continue;

                    while (m_bIsAIRechecking == 1 || m_bIsAIRecheckAndSubmissionThreadRunning == true)
                    {
                        Thread.Sleep(2000);
                    }

                    Debugger.Log(0, "", string.Format("111111 AI复判线程，条码：{0}，运行完毕", barcode));

                    var originalPath = System.IO.Path.Combine(waitToRecheckFilesDirectory, $"{barcode}.bcd");
                    if (m_bIsAIRechecking == -1)
                    {
                        var unsuccessfulPath = System.IO.Path.Combine(aiSubmitMESUnsuccessfulDirectory, $"{barcode}.bcd");

                        if (!File.Exists(unsuccessfulPath))
                        {
                            File.Copy(originalPath, unsuccessfulPath);
                            File.Delete(originalPath);
                        }
                    }
                    else if (m_bIsAIRechecking == 0)
                    {
                        File.Delete(originalPath);
                    }
                }

                if (barcodeFiles.Count() == 0)
                {
                    m_bIsAIRechecking = 1;

                    if (!Directory.Exists(aiSubmitMESUnsuccessfulDirectory))
                    {
                        Directory.CreateDirectory(aiSubmitMESUnsuccessfulDirectory);
                    }

                    var unsuccessfuleFiles = Directory.GetFiles(aiSubmitMESUnsuccessfulDirectory, $"*.bcd", SearchOption.TopDirectoryOnly);

                    if (unsuccessfuleFiles.Count() > 0)
                    {
                        var barcode = System.IO.Path.GetFileNameWithoutExtension(unsuccessfuleFiles.First());

                        // 如果字典元素超过100个，则清空
                        if (dict_file_names_and_submit_counts_for_unsuccessful_recheck.Count > 100)
                        {
                            dict_file_names_and_submit_counts_for_unsuccessful_recheck.Clear();
                        }

                        // 统计文件名和提交次数
                        string strFileName = System.IO.Path.GetFileName(unsuccessfuleFiles.First());
                        if (dict_file_names_and_submit_counts_for_unsuccessful_recheck.ContainsKey(strFileName))
                        {
                            dict_file_names_and_submit_counts_for_unsuccessful_recheck[strFileName]++;

                            // 如果提交次数超过3次，则把文件移动到D:\\fupanSubmitTooManyTimes文件夹
                            if (dict_file_names_and_submit_counts_for_unsuccessful_recheck[strFileName] > 3)
                            {
                                var tooManyTimesDirectory = @"D:\fupanSubmitTooManyTimes";
                                if (!Directory.Exists(tooManyTimesDirectory))
                                {
                                    Directory.CreateDirectory(tooManyTimesDirectory);
                                }

                                // 移动文件，如果文件已经存在，则删除
                                var tooManyTimesPath = System.IO.Path.Combine(tooManyTimesDirectory, strFileName);
                                if (false == File.Exists(tooManyTimesPath))
                                {
                                    File.Move(unsuccessfuleFiles.First(), tooManyTimesPath);
                                }
                                else
                                {
                                    File.Delete(unsuccessfuleFiles.First());
                                }

                                // 删除字典中的记录
                                dict_file_names_and_submit_counts_for_unsuccessful_recheck.Remove(strFileName);
                                continue;
                            }
                        }
                        else
                        {
                            dict_file_names_and_submit_counts_for_unsuccessful_recheck.Add(strFileName, 1);
                        }

                        Thread.Sleep(3000);

                        Debugger.Log(0, "", string.Format("111111 启动AI复判提交线程unsuccess，条码：{0}", barcode));

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            m_parent.page_HomeView.FillBarcodeFromAVIForAIMode3(Convert.ToString(barcode));
                        });

                        //m_reset_event.Reset();
                        //if (false == m_reset_event.WaitOne(15000))
                        //    continue;

                        while (m_bIsAIRechecking == 1 || m_bIsAIRecheckAndSubmissionThreadRunning == true)
                        {
                            Thread.Sleep(2000);
                        }

                        Debugger.Log(0, "", string.Format("111111 AI复判线程unsuccess，条码：{0}，运行完毕", barcode));

                        var unsuccessfulPath = System.IO.Path.Combine(aiSubmitMESUnsuccessfulDirectory, $"{barcode}.bcd");
                        if (m_bIsAIRechecking == 0)
                        {
                            File.Delete(unsuccessfulPath);
                        }
                    }
                }

				Thread.Sleep(3000);

			}
		}

        // 线程：对一次NG数据进行MES校验
        private void thread_validate_original_NG_product_data_with_MES(object obj)
        {
            m_parent.m_global.m_tray_info_service.CopyImagesEvent.WaitOne();

            string strBarcodeForSinglePieceMode = (string)obj;

            m_bIsValidatingWithMES = true;

            bool bIsDataChangedAndNeedRefreshUI = false;

            if (m_parent.m_global.m_strProductType == "nova")
            {
                // try catch block
                try
                {
                    for (int i = 0; i < m_parent.m_global.m_current_tray_info_for_Nova.NotRecievedPorducts.Count; i++)
                    {
                        var product = m_parent.m_global.m_current_tray_info_for_Nova.NotRecievedPorducts[i];
                        var trayInfo = m_parent.m_global.m_current_tray_info_for_Nova;
                        string strBarcode = product.BarCode;
                        string strMesServerResponse = "";
                        string json = orgnizeValidateInfoForMES(trayInfo, product);

                        // 将产品barcode提交到MES进行校验
                        bool bResult = m_parent.m_global.m_MES_service.SendMES(m_parent.m_global.m_strMesValidationUrl, json, ref strMesServerResponse, 1, 5);

                        // 如果校验成功
                        if (true == bResult)
                        {
                            MesResponse response = JsonConvert.DeserializeObject<MesResponse>(strMesServerResponse);

                            if (response.Result == "0")
                            {
                                // 校验成功
                                m_parent.m_global.m_log_presenter.Log(string.Format("未收到缺陷产品 {0} MES校验成功", strBarcode));

                                // 更新产品信息，判断产品是否为特殊报警或异常产品，如果是，则标记为对应的特殊报警或异常产品
                                if (true)
                                {
                                    // 标记为特殊报警或异常产品
                                    //m_parent.m_global.m_current_MES_tray_info.m_check_flags_by_product[i] = RecheckResult.;
                                }
                            }
                            else
                            {
                                if (response.Message.Count > 0)
                                {
                                    if (response.Message[0].Contains("样品板"))
                                    {
                                        MessageBox.Show($"MES返回样品报错，请检查本班次是否提交过样品版或样品版结果与维护结果是否相符。报错信息：{response.Message[0]}");

                                        // 更新UI
                                        //Application.Current.Dispatcher.Invoke(() =>
                                        //{
                                        //    ClearProductInfo();

                                        //    GenerateCircularButtonsInGridContainer(grid_CircularButtonContainer, 0, 0, null);
                                        //});

                                        break;
                                    }

                                    if (response.ResPanels.Count > 0)
                                    {
                                        var temp = JsonConvert.DeserializeObject<MesResponsePanel>(response.ResPanels[0].ToString());
                                        if (!temp.IsSuccess)
                                        {
                                            int nColumns = m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Nova.Col;
                                            int col = product.PosCol - 1;
                                            int row = product.PosRow - 1;

                                            int nIndex = row * nColumns + col;

                                            if (response.Message[0].Contains("FQC") ||
                                                response.Message[0].Contains("PACKING") ||
                                                response.Message[0].Contains("包装"))
                                            {
                                                m_parent.m_global.m_log_presenter.Log($"MES返回料片过站信息:{response.Message[0]}，过站料号：" + temp.Panel);
                                                m_parent.m_global.m_current_MES_tray_info.m_check_flags_by_product[nIndex] = RecheckResult.MES_NG;

                                                // 把过站料号加入到已过站料号列表

                                                // 在m_parent.m_global.m_current_tray_info_for_Nova.products中查找过站料号，找到后将其移动到m_parent.m_global.m_current_tray_info_for_Nova.MES_failed_products中
                                                for (int j = 0; j < m_parent.m_global.m_current_tray_info_for_Nova.NotRecievedPorducts.Count; j++)
                                                {
                                                    if (m_parent.m_global.m_current_tray_info_for_Nova.NotRecievedPorducts[j].BarCode == temp.Panel)
                                                    {
                                                        m_parent.m_global.m_current_tray_info_for_Nova.MES_failed_products.Add(m_parent.m_global.m_current_tray_info_for_Nova.NotRecievedPorducts[j]);

                                                        m_parent.m_global.m_current_tray_info_for_Nova.NotRecievedPorducts.RemoveAt(j);

                                                        bIsDataChangedAndNeedRefreshUI = true;

                                                        break;
                                                    }
                                                }
                                            }
                                            else if (
                                                response.Message[0].Contains("ET") ||
                                                response.Message[0].Contains("RF") ||
                                                response.Message[0].Contains("FCT") ||
                                                response.Message[0].Contains("ICT") ||
                                                response.Message[0].Contains("WARPAGEA"))
                                            {
                                                m_parent.m_global.m_log_presenter.Log($"MES返回料片ET信息:{response.Message[0]}，ET料号：" + temp.Panel);

                                                m_parent.m_global.m_current_MES_tray_info.m_check_flags_by_product[nIndex] = RecheckResult.ET;

                                                // 把ET料号加入到ET列表

                                                // 在m_parent.m_global.m_current_tray_info_for_Nova.products中查找ET料号，找到后将其移动到m_parent.m_global.m_current_tray_info_for_Nova.ET_products中
                                                for (int j = 0; j < m_parent.m_global.m_current_tray_info_for_Nova.NotRecievedPorducts.Count; j++)
                                                {
                                                    if (m_parent.m_global.m_current_tray_info_for_Nova.NotRecievedPorducts[j].BarCode == temp.Panel)
                                                    {
                                                        m_parent.m_global.m_current_tray_info_for_Nova.ET_products.Add(m_parent.m_global.m_current_tray_info_for_Nova.NotRecievedPorducts[j]);

                                                        m_parent.m_global.m_current_tray_info_for_Nova.NotRecievedPorducts.RemoveAt(j);

                                                        bIsDataChangedAndNeedRefreshUI = true;

                                                        break;
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                m_parent.m_global.m_log_presenter.Log($"MES返回料片其他异常信息:{response.Message[0]}，异常料号：" + temp.Panel);
                                                m_parent.m_global.m_current_MES_tray_info.m_check_flags_by_product[nIndex] = RecheckResult.NG;

                                                // 把过站料号加入到已过站料号列表

                                                // 在m_parent.m_global.m_current_tray_info_for_Nova.products中查找过站料号，找到后将其移动到m_parent.m_global.m_current_tray_info_for_Nova.MES_failed_products中
                                                for (int j = 0; j < m_parent.m_global.m_current_tray_info_for_Nova.NotRecievedPorducts.Count; j++)
                                                {
                                                    if (m_parent.m_global.m_current_tray_info_for_Nova.NotRecievedPorducts[j].BarCode == temp.Panel)
                                                    {
                                                        m_parent.m_global.m_current_tray_info_for_Nova.MES_failed_products.Add(m_parent.m_global.m_current_tray_info_for_Nova.NotRecievedPorducts[j]);

                                                        m_parent.m_global.m_current_tray_info_for_Nova.NotRecievedPorducts.RemoveAt(j);

                                                        bIsDataChangedAndNeedRefreshUI = true;

                                                        break;
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    // 校验失败
                                    m_parent.m_global.m_log_presenter.Log(string.Format("产品{0}MES校验失败，失败信息：{1}", strBarcode, response.Message[0]));
                                }

                                // 更新UI
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    if (true == bIsDataChangedAndNeedRefreshUI)
                                    {
                                        m_parent.m_global.m_tray_info_service.HandleTrayInfoFromAVIForNova_InitFlagsAndShowImage(m_parent.m_global.m_current_tray_info_for_Nova);
                                    }

                                    UpdateUIWithProductInfoForNova(m_parent.m_global.m_current_tray_info_for_Nova, i);

                                    render_tray_button_color();
                                });

                                i--;
                            }
                        }
                    }

                    for (int i = 0; i < m_parent.m_global.m_current_MES_tray_info.m_array_indices_of_defected_products.Length; i++)
                    {
                        var product = m_parent.m_global.m_current_tray_info_for_Nova.Products[i];
                        var trayInfo = m_parent.m_global.m_current_tray_info_for_Nova;
                        string strBarcode = product.BarCode;
                        string strMesServerResponse = "";
                        string json = orgnizeValidateInfoForMES(trayInfo, product);

                        // 将产品barcode提交到MES进行校验
                        bool bResult = m_parent.m_global.m_MES_service.SendMES(m_parent.m_global.m_strMesValidationUrl, json, ref strMesServerResponse, 1, 5);

                        // 如果校验成功
                        if (true == bResult)
                        {
                            MesResponse response = JsonConvert.DeserializeObject<MesResponse>(strMesServerResponse);

                            if (response.Result == "0")
                            {
                                // 校验成功
                                m_parent.m_global.m_log_presenter.Log(string.Format("产品 {0} MES校验成功", strBarcode));

                                // 更新产品信息，判断产品是否为特殊报警或异常产品，如果是，则标记为对应的特殊报警或异常产品
                                if (true)
                                {
                                    // 标记为特殊报警或异常产品
                                    //m_parent.m_global.m_current_MES_tray_info.m_check_flags_by_product[i] = RecheckResult.;
                                }
                            }
                            else
                            {
                                if (response.Message.Count > 0)
                                {
                                    if (response.Message[0].Contains("样品板"))
                                    {
                                        MessageBox.Show($"MES返回样品报错，请检查本班次是否提交过样品版或样品版结果与维护结果是否相符。报错信息：{response.Message[0]}");

                                        // 更新UI
                                        //Application.Current.Dispatcher.Invoke(() =>
                                        //{
                                        //    ClearProductInfo();

                                        //    GenerateCircularButtonsInGridContainer(grid_CircularButtonContainer, 0, 0, null);
                                        //});

                                        break;
                                    }

                                    if (response.ResPanels.Count > 0)
                                    {
                                        var temp = JsonConvert.DeserializeObject<MesResponsePanel>(response.ResPanels[0].ToString());
                                        if (!temp.IsSuccess)
                                        {
                                            if (response.Message[0].Contains("FQC") ||
                                                response.Message[0].Contains("PACKING") ||
                                                response.Message[0].Contains("包装"))
                                            {
                                                m_parent.m_global.m_log_presenter.Log($"MES返回料片过站信息:{response.Message[0]}，过站料号：" + temp.Panel);
                                                m_parent.m_global.m_current_MES_tray_info.m_check_flags_by_product[i] = RecheckResult.MES_NG;

                                                // 把过站料号加入到已过站料号列表

                                                // 在m_parent.m_global.m_current_tray_info_for_Nova.products中查找过站料号，找到后将其移动到m_parent.m_global.m_current_tray_info_for_Nova.MES_failed_products中
                                                for (int j = 0; j < m_parent.m_global.m_current_tray_info_for_Nova.Products.Count; j++)
                                                {
                                                    if (m_parent.m_global.m_current_tray_info_for_Nova.Products[j].BarCode == temp.Panel)
                                                    {
                                                        m_parent.m_global.m_current_tray_info_for_Nova.MES_failed_products.Add(m_parent.m_global.m_current_tray_info_for_Nova.Products[j]);

                                                        m_parent.m_global.m_current_tray_info_for_Nova.Products.RemoveAt(j);

                                                        bIsDataChangedAndNeedRefreshUI = true;

                                                        break;
                                                    }
                                                }
                                            }
                                            else if (
                                                response.Message[0].Contains("ET") ||
                                                response.Message[0].Contains("RF") ||
                                                response.Message[0].Contains("FCT") ||
                                                response.Message[0].Contains("ICT") ||
                                                response.Message[0].Contains("WARPAGEA"))
                                            {
                                                m_parent.m_global.m_log_presenter.Log($"MES返回料片ET信息:{response.Message[0]}，ET料号：" + temp.Panel);

                                                m_parent.m_global.m_current_MES_tray_info.m_check_flags_by_product[i] = RecheckResult.ET;

                                                // 把ET料号加入到ET列表

                                                // 在m_parent.m_global.m_current_tray_info_for_Nova.products中查找ET料号，找到后将其移动到m_parent.m_global.m_current_tray_info_for_Nova.ET_products中
                                                for (int j = 0; j < m_parent.m_global.m_current_tray_info_for_Nova.Products.Count; j++)
                                                {
                                                    if (m_parent.m_global.m_current_tray_info_for_Nova.Products[j].BarCode == temp.Panel)
                                                    {
                                                        m_parent.m_global.m_current_tray_info_for_Nova.ET_products.Add(m_parent.m_global.m_current_tray_info_for_Nova.Products[j]);

                                                        m_parent.m_global.m_current_tray_info_for_Nova.Products.RemoveAt(j);

                                                        bIsDataChangedAndNeedRefreshUI = true;

                                                        break;
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                m_parent.m_global.m_log_presenter.Log($"MES返回料片其他异常信息:{response.Message[0]}，异常料号：" + temp.Panel);
                                                m_parent.m_global.m_current_MES_tray_info.m_check_flags_by_product[i] = RecheckResult.NG;

                                                // 把过站料号加入到已过站料号列表

                                                // 在m_parent.m_global.m_current_tray_info_for_Nova.products中查找过站料号，找到后将其移动到m_parent.m_global.m_current_tray_info_for_Nova.MES_failed_products中
                                                for (int j = 0; j < m_parent.m_global.m_current_tray_info_for_Nova.Products.Count; j++)
                                                {
                                                    if (m_parent.m_global.m_current_tray_info_for_Nova.Products[j].BarCode == temp.Panel)
                                                    {
                                                        m_parent.m_global.m_current_tray_info_for_Nova.MES_failed_products.Add(m_parent.m_global.m_current_tray_info_for_Nova.Products[j]);

                                                        m_parent.m_global.m_current_tray_info_for_Nova.Products.RemoveAt(j);

                                                        bIsDataChangedAndNeedRefreshUI = true;

                                                        break;
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    // 校验失败
                                    m_parent.m_global.m_log_presenter.Log(string.Format("产品{0}MES校验失败，失败信息：{1}", strBarcode, response.Message[0]));
                                }

                                // 更新UI
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    if (true == bIsDataChangedAndNeedRefreshUI)
                                    {
                                        m_parent.m_global.m_tray_info_service.HandleTrayInfoFromAVIForNova_InitFlagsAndShowImage(m_parent.m_global.m_current_tray_info_for_Nova);
                                    }

                                    // 在移除products中最后一个产品时，再以i更新UI会超出
                                    if (i == m_parent.m_global.m_current_tray_info_for_Nova.Products.Count)
                                    {
                                        UpdateUIWithProductInfoForNova(m_parent.m_global.m_current_tray_info_for_Nova, i - 1);
                                    }
                                    else
                                    {
                                        UpdateUIWithProductInfoForNova(m_parent.m_global.m_current_tray_info_for_Nova, i);
                                    }

                                    render_tray_button_color();
                                });

                                i--;
                            }
                        }
                    }

                    for (int i = 0; i < m_parent.m_global.m_current_tray_info_for_Nova.Products.Count; i++)
                    {
                        Product product = m_parent.m_global.m_current_tray_info_for_Nova.Products[i];

                        if (product.Defects == null)
                            continue;

                        // 遍历产品的所有缺陷
                        for (int j = 0; j < product.Defects.Count; j++)
                        {
                            product.Defects[j].IsUncheckable = false;

                            bool bIsUnrecheckable = false;

                            // 遍历不可复判缺陷类型列表
                            for (int k = 0; k < m_parent.m_global.m_uncheckable_defect_types.Count; k++)
                            {
                                // 如果不可复判缺陷类型是启用的
                                if (m_parent.m_global.m_uncheckable_defect_enable_flags[k] == true)
                                {
                                    // 如果产品的缺陷类型与当前不可复判缺陷类型匹配
                                    if (product.Defects[j].Type == m_parent.m_global.m_uncheckable_defect_types[k])
                                    {
                                            // 将带有不可复判缺陷的产品添加到列表中
                                            m_parent.m_global.m_current_tray_info_for_Nova.products_with_unrecheckable_defect.Add(product);

                                            // 从当前托盘中移除该产品
                                            m_parent.m_global.m_current_tray_info_for_Nova.Products.RemoveAt(i);

                                            // 调整索引，防止跳过下一个产品
                                            i--;

                                        // 标记为不可复判
                                        bIsUnrecheckable = true;

                                        break;
                                    }
                                }
                            }

                            // 如果产品带有不可复判缺陷，跳出内层循环
                            if (true == bIsUnrecheckable)
                                break;
                        }
                    }

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        m_parent.m_global.m_tray_info_service.HandleTrayInfoFromAVIForNova_InitFlagsAndShowImage(m_parent.m_global.m_current_tray_info_for_Nova);
                    });
                }
                catch (Exception ex)
                {
                    m_parent.m_global.m_log_presenter.Log("线程：对一次NG数据进行MES校验异常：" + ex.Message);
                }
            }
            else if (m_parent.m_global.m_strProductType == "dock")
            {
                // try catch block
                try
                {
                    for (int i = 0; i < m_parent.m_global.m_current_tray_info_for_Dock.OK_products.Count; i++)
                    {
                        var trayInfo = m_parent.m_global.m_current_tray_info_for_Dock;
                        var product = trayInfo.OK_products[i];
                        string strBarcode = product.barcode;
                        string strMesServerResponse = "";
                        string json = orgnizeValidateInfoForMES(trayInfo, product);

                        bool bResult = m_parent.m_global.m_MES_service.SendMES(m_parent.m_global.m_strMesValidationUrl, json, ref strMesServerResponse, 1, 5);

                        if (true == bResult)
                        {
                            MesResponse response = JsonConvert.DeserializeObject<MesResponse>(strMesServerResponse);

                            if (response.Result == "0")
                            {
                                // 校验成功
                                m_parent.m_global.m_log_presenter.Log(string.Format("产品 {0} MES校验成功", strBarcode));

                                // 更新产品信息，判断产品是否为特殊报警或异常产品，如果是，则标记为对应的特殊报警或异常产品
                                if (true)
                                {
                                    // 标记为特殊报警或异常产品
                                    //m_parent.m_global.m_current_MES_tray_info.m_check_flags_by_product[i] = RecheckResult.;
                                }
                            }
                            else
                            {
                                if (response.Message.Count > 0)
                                {
                                    if (response.Message[0].Contains("样品板"))
                                    {
                                        MessageBox.Show($"MES返回样品报错，请检查本班次是否提交过样品版或样品版结果与维护结果是否相符。报错信息：{response.Message[0]}");

                                        // 更新UI
                                        //Application.Current.Dispatcher.Invoke(() =>
                                        //{
                                        //    ClearProductInfo();

                                        //    GenerateCircularButtonsInGridContainer(grid_CircularButtonContainer, 0, 0, null);
                                        //});

                                        break;
                                    }

                                    if (response.ResPanels.Count > 0)
                                    {
                                        var temp = JsonConvert.DeserializeObject<MesResponsePanel>(response.ResPanels[0].ToString());
                                        if (!temp.IsSuccess)
                                        {
                                            if (response.Message[0].Contains("FQC") ||
                                                response.Message[0].Contains("PACKING") ||
                                                response.Message[0].Contains("包装"))
                                            {
                                                m_parent.m_global.m_log_presenter.Log($"MES返回料片过站信息:{response.Message[0]}，过站料号：" + temp.Panel);
                                                m_parent.m_global.m_current_MES_tray_info.m_check_flags_by_product[i] = RecheckResult.MES_NG;

                                                // 把过站料号加入到已过站料号列表

                                                // 在m_parent.m_global.m_current_tray_info_for_Nova.products中查找过站料号，找到后将其移动到m_parent.m_global.m_current_tray_info_for_Nova.MES_failed_products中
                                                for (int j = 0; j < m_parent.m_global.m_current_tray_info_for_Dock.OK_products.Count; j++)
                                                {
                                                    if (m_parent.m_global.m_current_tray_info_for_Dock.OK_products[j].barcode == temp.Panel)
                                                    {
                                                        m_parent.m_global.m_current_tray_info_for_Dock.MES_failed_products.Add(m_parent.m_global.m_current_tray_info_for_Dock.OK_products[j]);

                                                        m_parent.m_global.m_current_tray_info_for_Dock.OK_products.RemoveAt(j);

                                                        bIsDataChangedAndNeedRefreshUI = true;

                                                        break;
                                                    }
                                                }
                                            }
                                            else if (response.Message[0].Contains("ET") ||
                                                response.Message[0].Contains("RF") ||
                                                response.Message[0].Contains("FCT") ||
                                                response.Message[0].Contains("ICT"))
                                            {
                                                m_parent.m_global.m_log_presenter.Log($"MES返回料片ET信息:{response.Message[0]}，ET料号：" + temp.Panel);

                                                m_parent.m_global.m_current_MES_tray_info.m_check_flags_by_product[i] = RecheckResult.ET;

                                                // 把ET料号加入到ET列表

                                                // 在m_parent.m_global.m_current_tray_info_for_Nova.products中查找ET料号，找到后将其移动到m_parent.m_global.m_current_tray_info_for_Nova.ET_products中
                                                for (int j = 0; j < m_parent.m_global.m_current_tray_info_for_Dock.OK_products.Count; j++)
                                                {
                                                    if (m_parent.m_global.m_current_tray_info_for_Dock.OK_products[j].barcode == temp.Panel)
                                                    {
                                                        m_parent.m_global.m_current_tray_info_for_Dock.ET_products.Add(m_parent.m_global.m_current_tray_info_for_Dock.OK_products[j]);

                                                        m_parent.m_global.m_current_tray_info_for_Dock.OK_products.RemoveAt(j);

                                                        bIsDataChangedAndNeedRefreshUI = true;

                                                        break;
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                m_parent.m_global.m_log_presenter.Log($"MES返回料片其他异常信息:{response.Message[0]}，异常料号：" + temp.Panel);
                                                m_parent.m_global.m_current_MES_tray_info.m_check_flags_by_product[i] = RecheckResult.NG;

                                                // 把过站料号加入到已过站料号列表

                                                // 在m_parent.m_global.m_current_tray_info_for_Nova.products中查找过站料号，找到后将其移动到m_parent.m_global.m_current_tray_info_for_Nova.MES_failed_products中
                                                for (int j = 0; j < m_parent.m_global.m_current_tray_info_for_Dock.OK_products.Count; j++)
                                                {
                                                    if (m_parent.m_global.m_current_tray_info_for_Dock.OK_products[j].barcode == temp.Panel)
                                                    {
                                                        m_parent.m_global.m_current_tray_info_for_Dock.MES_failed_products.Add(m_parent.m_global.m_current_tray_info_for_Dock.OK_products[j]);

                                                        m_parent.m_global.m_current_tray_info_for_Dock.OK_products.RemoveAt(j);

                                                        bIsDataChangedAndNeedRefreshUI = true;

                                                        break;
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    // 校验失败
                                    m_parent.m_global.m_log_presenter.Log(string.Format("产品{0}MES校验失败，失败信息：{1}", strBarcode, response.Message[0]));
                                }

                                // 更新UI
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    if (true == bIsDataChangedAndNeedRefreshUI)
                                    {
                                        m_parent.m_global.m_tray_info_service.HandleTrayInfoFromAVIForDockOrCL5_InitFlagsAndShowImage(m_parent.m_global.m_current_tray_info_for_Dock, strBarcodeForSinglePieceMode);
                                    }

                                    UpdateUIWithProductInfoForDock(m_parent.m_global.m_current_tray_info_for_Dock, i);

                                    render_tray_button_color();
                                });

                                i--;
                            }
                        }
                    }

                    for(int i = 0; i < m_parent.m_global.m_current_tray_info_for_Dock.NotRecievedPorducts.Count; i++)
                    {
                        var trayInfo = m_parent.m_global.m_current_tray_info_for_Dock;
                        var product = trayInfo.NotRecievedPorducts[i];
                        string strBarcode = product.barcode;
                        string strMesServerResponse = "";
                        string json = orgnizeValidateInfoForMES(trayInfo, product);

                        bool bResult = m_parent.m_global.m_MES_service.SendMES(m_parent.m_global.m_strMesValidationUrl, json, ref strMesServerResponse, 1, 5);

                        if (true == bResult)
                        {
                            MesResponse response = JsonConvert.DeserializeObject<MesResponse>(strMesServerResponse);

                            if (response.Result == "0")
                            {
                                // 校验成功
                                m_parent.m_global.m_log_presenter.Log(string.Format("产品 {0} MES校验成功", strBarcode));

                                // 更新产品信息，判断产品是否为特殊报警或异常产品，如果是，则标记为对应的特殊报警或异常产品
                                if (true)
                                {
                                    // 标记为特殊报警或异常产品
                                    //m_parent.m_global.m_current_MES_tray_info.m_check_flags_by_product[i] = RecheckResult.;
                                }
                            }
                            else
                            {
                                if (response.Message.Count > 0)
                                {
                                    if (response.Message[0].Contains("样品板"))
                                    {
                                        MessageBox.Show($"MES返回样品报错，请检查本班次是否提交过样品版或样品版结果与维护结果是否相符。报错信息：{response.Message[0]}");

                                        // 更新UI
                                        //Application.Current.Dispatcher.Invoke(() =>
                                        //{
                                        //    ClearProductInfo();

                                        //    GenerateCircularButtonsInGridContainer(grid_CircularButtonContainer, 0, 0, null);
                                        //});

                                        break;
                                    }

                                    if (response.ResPanels.Count > 0)
                                    {
                                        var temp = JsonConvert.DeserializeObject<MesResponsePanel>(response.ResPanels[0].ToString());
                                        if (!temp.IsSuccess)
                                        {
                                            if (response.Message[0].Contains("FQC") ||
                                                response.Message[0].Contains("PACKING") ||
                                                response.Message[0].Contains("包装"))
                                            {
                                                m_parent.m_global.m_log_presenter.Log($"MES返回料片过站信息:{response.Message[0]}，过站料号：" + temp.Panel);
                                                m_parent.m_global.m_current_MES_tray_info.m_check_flags_by_product[i] = RecheckResult.MES_NG;

                                                // 把过站料号加入到已过站料号列表

                                                // 在m_parent.m_global.m_current_tray_info_for_Nova.products中查找过站料号，找到后将其移动到m_parent.m_global.m_current_tray_info_for_Nova.MES_failed_products中
                                                for (int j = 0; j < m_parent.m_global.m_current_tray_info_for_Dock.NotRecievedPorducts.Count; j++)
                                                {
                                                    if (m_parent.m_global.m_current_tray_info_for_Dock.NotRecievedPorducts[j].barcode == temp.Panel)
                                                    {
                                                        m_parent.m_global.m_current_tray_info_for_Dock.MES_failed_products.Add(m_parent.m_global.m_current_tray_info_for_Dock.NotRecievedPorducts[j]);

                                                        m_parent.m_global.m_current_tray_info_for_Dock.NotRecievedPorducts.RemoveAt(j);

                                                        bIsDataChangedAndNeedRefreshUI = true;

                                                        break;
                                                    }
                                                }
                                            }
                                            else if (response.Message[0].Contains("ET") ||
                                                response.Message[0].Contains("RF") ||
                                                response.Message[0].Contains("FCT") ||
                                                response.Message[0].Contains("ICT"))
                                            {
                                                m_parent.m_global.m_log_presenter.Log($"MES返回料片ET信息:{response.Message[0]}，ET料号：" + temp.Panel);

                                                m_parent.m_global.m_current_MES_tray_info.m_check_flags_by_product[i] = RecheckResult.ET;

                                                // 把ET料号加入到ET列表

                                                // 在m_parent.m_global.m_current_tray_info_for_Nova.products中查找ET料号，找到后将其移动到m_parent.m_global.m_current_tray_info_for_Nova.ET_products中
                                                for (int j = 0; j < m_parent.m_global.m_current_tray_info_for_Dock.NotRecievedPorducts.Count; j++)
                                                {
                                                    if (m_parent.m_global.m_current_tray_info_for_Dock.NotRecievedPorducts[j].barcode == temp.Panel)
                                                    {
                                                        m_parent.m_global.m_current_tray_info_for_Dock.ET_products.Add(m_parent.m_global.m_current_tray_info_for_Dock.NotRecievedPorducts[j]);

                                                        m_parent.m_global.m_current_tray_info_for_Dock.NotRecievedPorducts.RemoveAt(j);

                                                        bIsDataChangedAndNeedRefreshUI = true;

                                                        break;
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                m_parent.m_global.m_log_presenter.Log($"MES返回料片其他异常信息:{response.Message[0]}，异常料号：" + temp.Panel);
                                                m_parent.m_global.m_current_MES_tray_info.m_check_flags_by_product[i] = RecheckResult.NG;

                                                // 把过站料号加入到已过站料号列表

                                                // 在m_parent.m_global.m_current_tray_info_for_Nova.products中查找过站料号，找到后将其移动到m_parent.m_global.m_current_tray_info_for_Nova.MES_failed_products中
                                                for (int j = 0; j < m_parent.m_global.m_current_tray_info_for_Dock.NotRecievedPorducts.Count; j++)
                                                {
                                                    if (m_parent.m_global.m_current_tray_info_for_Dock.NotRecievedPorducts[j].barcode == temp.Panel)
                                                    {
                                                        m_parent.m_global.m_current_tray_info_for_Dock.MES_failed_products.Add(m_parent.m_global.m_current_tray_info_for_Dock.NotRecievedPorducts[j]);

                                                        m_parent.m_global.m_current_tray_info_for_Dock.NotRecievedPorducts.RemoveAt(j);

                                                        bIsDataChangedAndNeedRefreshUI = true;

                                                        break;
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    // 校验失败
                                    m_parent.m_global.m_log_presenter.Log(string.Format("产品{0}MES校验失败，失败信息：{1}", strBarcode, response.Message[0]));
                                }

                                // 更新UI
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    if (true == bIsDataChangedAndNeedRefreshUI)
                                    {
                                        m_parent.m_global.m_tray_info_service.HandleTrayInfoFromAVIForDockOrCL5_InitFlagsAndShowImage(m_parent.m_global.m_current_tray_info_for_Dock, strBarcodeForSinglePieceMode);
                                    }

                                    UpdateUIWithProductInfoForDock(m_parent.m_global.m_current_tray_info_for_Dock, i);

                                    render_tray_button_color();
                                });

                                i--;
                            }
                        }
                    }

                    for (int i = 0; i < m_parent.m_global.m_current_MES_tray_info.m_array_indices_of_defected_products.Length; i++)
                    {
                        var trayInfo = m_parent.m_global.m_current_tray_info_for_Dock;
                        var product = trayInfo.products[i];
                        string strBarcode = product.barcode;
                        string strMesServerResponse = "";
                        string json = orgnizeValidateInfoForMES(trayInfo, product);

                        // 将产品barcode提交到MES进行校验
                        bool bResult = m_parent.m_global.m_MES_service.SendMES(m_parent.m_global.m_strMesValidationUrl, json, ref strMesServerResponse, 1, 5);

                        // 如果校验成功
                        if (true == bResult)
                        {
                            MesResponse response = JsonConvert.DeserializeObject<MesResponse>(strMesServerResponse);

                            if (response.Result == "0")
                            {
                                // 校验成功
                                m_parent.m_global.m_log_presenter.Log(string.Format("产品 {0} MES校验成功", strBarcode));

                                // 更新产品信息，判断产品是否为特殊报警或异常产品，如果是，则标记为对应的特殊报警或异常产品
                                if (true)
                                {
                                    // 标记为特殊报警或异常产品
                                    //m_parent.m_global.m_current_MES_tray_info.m_check_flags_by_product[i] = RecheckResult.;
                                }
                            }
                            else
                            {
                                if (response.Message.Count > 0)
                                {
                                    if (response.Message[0].Contains("样品板"))
                                    {
                                        MessageBox.Show($"MES返回样品报错，请检查本班次是否提交过样品版或样品版结果与维护结果是否相符。报错信息：{response.Message[0]}");

                                        // 更新UI
                                        //Application.Current.Dispatcher.Invoke(() =>
                                        //{
                                        //    ClearProductInfo();

                                        //    GenerateCircularButtonsInGridContainer(grid_CircularButtonContainer, 0, 0, null);
                                        //});

                                        break;
                                    }

                                    if (response.ResPanels.Count > 0)
                                    {
                                        var temp = JsonConvert.DeserializeObject<MesResponsePanel>(response.ResPanels[0].ToString());
                                        if (!temp.IsSuccess)
                                        {
                                            if (response.Message[0].Contains("FQC") ||
                                                response.Message[0].Contains("PACKING") ||
                                                response.Message[0].Contains("包装"))
                                            {
                                                m_parent.m_global.m_log_presenter.Log($"MES返回料片过站信息:{response.Message[0]}，过站料号：" + temp.Panel);
                                                m_parent.m_global.m_current_MES_tray_info.m_check_flags_by_product[i] = RecheckResult.MES_NG;

                                                // 把过站料号加入到已过站料号列表

                                                // 在m_parent.m_global.m_current_tray_info_for_Nova.products中查找过站料号，找到后将其移动到m_parent.m_global.m_current_tray_info_for_Nova.MES_failed_products中
                                                for (int j = 0; j < m_parent.m_global.m_current_tray_info_for_Dock.products.Count; j++)
                                                {
                                                    if (m_parent.m_global.m_current_tray_info_for_Dock.products[j].barcode == temp.Panel)
                                                    {
                                                        m_parent.m_global.m_current_tray_info_for_Dock.MES_failed_products.Add(m_parent.m_global.m_current_tray_info_for_Dock.products[j]);

                                                        m_parent.m_global.m_current_tray_info_for_Dock.products.RemoveAt(j);

                                                        bIsDataChangedAndNeedRefreshUI = true;

                                                        break;
                                                    }
                                                }
                                            }
                                            else if (response.Message[0].Contains("ET") ||
                                                response.Message[0].Contains("RF") ||
                                                response.Message[0].Contains("FCT") ||
                                                response.Message[0].Contains("ICT"))
                                            {
                                                m_parent.m_global.m_log_presenter.Log($"MES返回料片ET信息:{response.Message[0]}，ET料号：" + temp.Panel);

                                                m_parent.m_global.m_current_MES_tray_info.m_check_flags_by_product[i] = RecheckResult.ET;

                                                // 把ET料号加入到ET列表

                                                // 在m_parent.m_global.m_current_tray_info_for_Nova.products中查找ET料号，找到后将其移动到m_parent.m_global.m_current_tray_info_for_Nova.ET_products中
                                                for (int j = 0; j < m_parent.m_global.m_current_tray_info_for_Dock.products.Count; j++)
                                                {
                                                    if (m_parent.m_global.m_current_tray_info_for_Dock.products[j].barcode == temp.Panel)
                                                    {
                                                        m_parent.m_global.m_current_tray_info_for_Dock.ET_products.Add(m_parent.m_global.m_current_tray_info_for_Dock.products[j]);

                                                        m_parent.m_global.m_current_tray_info_for_Dock.products.RemoveAt(j);

                                                        bIsDataChangedAndNeedRefreshUI = true;

                                                        break;
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                m_parent.m_global.m_log_presenter.Log($"MES返回料片其他异常信息:{response.Message[0]}，异常料号：" + temp.Panel);
                                                m_parent.m_global.m_current_MES_tray_info.m_check_flags_by_product[i] = RecheckResult.NG;

                                                // 把过站料号加入到已过站料号列表

                                                // 在m_parent.m_global.m_current_tray_info_for_Nova.products中查找过站料号，找到后将其移动到m_parent.m_global.m_current_tray_info_for_Nova.MES_failed_products中
                                                for (int j = 0; j < m_parent.m_global.m_current_tray_info_for_Dock.products.Count; j++)
                                                {
                                                    if (m_parent.m_global.m_current_tray_info_for_Dock.products[j].barcode == temp.Panel)
                                                    {
                                                        m_parent.m_global.m_current_tray_info_for_Dock.MES_failed_products.Add(m_parent.m_global.m_current_tray_info_for_Dock.products[j]);

                                                        m_parent.m_global.m_current_tray_info_for_Dock.products.RemoveAt(j);

                                                        bIsDataChangedAndNeedRefreshUI = true;

                                                        break;
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            m_parent.m_global.m_current_MES_tray_info.m_check_flags_by_product[i] = RecheckResult.NG;
                                            for (int j = 0; j < m_parent.m_global.m_current_tray_info_for_Dock.products.Count; j++)
                                            {
                                                if (m_parent.m_global.m_current_tray_info_for_Dock.products[j].barcode == temp.Panel)
                                                {
                                                    m_parent.m_global.m_current_tray_info_for_Dock.MES_failed_products.Add(m_parent.m_global.m_current_tray_info_for_Dock.products[j]);

                                                    m_parent.m_global.m_current_tray_info_for_Dock.products.RemoveAt(j);

                                                    bIsDataChangedAndNeedRefreshUI = true;

                                                    break;
                                                }
                                            }
                                        }
                                    }

                                    // 校验失败
                                    m_parent.m_global.m_log_presenter.Log(string.Format("产品{0}MES校验失败，失败信息：{1}", strBarcode, response.Message[0]));
                                }

                                // 更新UI
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    if (true == bIsDataChangedAndNeedRefreshUI)
                                    {
                                        m_parent.m_global.m_tray_info_service.HandleTrayInfoFromAVIForDockOrCL5_InitFlagsAndShowImage(m_parent.m_global.m_current_tray_info_for_Dock, strBarcodeForSinglePieceMode);
                                    }

                                    // 在移除products中最后一个产品时，再以i更新UI会超出
                                    if (i == m_parent.m_global.m_current_tray_info_for_Dock.products.Count)
                                    {
                                        UpdateUIWithProductInfoForDock(m_parent.m_global.m_current_tray_info_for_Dock, i - 1);

                                    }
                                    else
                                    {
                                        UpdateUIWithProductInfoForDock(m_parent.m_global.m_current_tray_info_for_Dock, i);
                                    }

                                    render_tray_button_color();
                                });

                                i--;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    m_parent.m_global.m_log_presenter.Log("线程：对一次NG数据进行MES校验异常：" + ex.Message);
                }
            }

            m_bIsValidatingWithMES = false;
        }

        private string orgnizeValidateInfoForMES(AVITrayInfo trayInfo, Product product)
        {
            var uploadInfo = new
            {
                Panel = product.BarCode,
                Resource = trayInfo.Mid,
                Site = m_parent.m_global.m_strSiteCity == "苏州" ? "SZSMT" : "YCSMT",
                Mac = "F8-16-54-CD-AF-82",
                ProgramName = m_parent.m_global.m_strProgramName,
                Machine = trayInfo.Mid,
                Product = m_parent.m_global.m_strProductName,
                WorkArea = "SMT-FAVI",
                TestType = "AVI-FQC",
                TestTimt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                OperatorName = m_parent.m_global.m_strCurrentOperatorID,
                OperatorType = "Operator",
                TestMode = "PANEL",
                TrackType = "0",
                CheckPcsDataForAVI = true,
                JdeProductName = m_parent.m_global.m_strProductName,
                Uuid = trayInfo.SetId,
                VersionFlag = "0",
                CheckDetail = "1"
            };

            return JsonConvert.SerializeObject(uploadInfo);

        }

        private string orgnizeValidateInfoForMES(TrayInfo trayInfo, ProductInfo product)
        {
            var uploadInfo = new
            {
                Panel = product.barcode,
                Resource = trayInfo.machine_id,
                Site = m_parent.m_global.m_strSiteCity == "苏州" ? "SZSMT" : "YCSMT",
                Mac = "F8-16-54-CD-AF-82",
                ProgramName = m_parent.m_global.m_strProgramName,
                Machine = trayInfo.machine_id,
                Product = m_parent.m_global.m_strProductName,
                WorkArea = "SMT-FAVI",
                TestType = "AVI-FQC",
                TestTimt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                OperatorName = m_parent.m_global.m_strCurrentOperatorID,
                OperatorType = "Operator",
                TestMode = "PANEL",
                TrackType = "0",
                CheckPcsDataForAVI = true,
                JdeProductName = m_parent.m_global.m_strProductName,
                Uuid = trayInfo.set_id,
                VersionFlag = "0",
                CheckDetail = "1"
            };

            return JsonConvert.SerializeObject(uploadInfo);
        }
    }
}