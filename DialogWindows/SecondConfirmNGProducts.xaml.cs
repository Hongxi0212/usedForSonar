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
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AutoScanFQCTest.DialogWindows
{
    public partial class SecondConfirmNGProducts : Window
    {
        public MainWindow m_parent;

        public List<string> m_list_NG_barcodes = new List<string>();
        private List<bool> m_list_NG_confirm_status = new List<bool>();

        public bool m_bOK = false;

        public bool m_bIsAllNGProductsConfirmed = false;

        public SecondConfirmNGProducts(MainWindow parent)
        {
            InitializeComponent();

            m_parent = parent;

            textbox_Barcode.Focus();
            textbox_Barcode.CaretIndex = textbox_Barcode.Text.Length;

            // 注册事件
            this.Loaded += Window_Loaded;
        }

        // 窗口加载事件
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // 构造产品二维码列表gridview
            if (true)
            {
                gridview_NGProducts.RowHeadersVisible = false;
                gridview_NGProducts.ReadOnly = true;
                gridview_NGProducts.ColumnCount = 4;
                gridview_NGProducts.ColumnHeadersHeight = 35;
                gridview_NGProducts.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;

                gridview_NGProducts.EnableHeadersVisualStyles = false;
                gridview_NGProducts.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(236, 230, 239);

                gridview_NGProducts.ColumnHeadersVisible = true;
                gridview_NGProducts.RowsDefaultCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;

                gridview_NGProducts.ColumnHeadersDefaultCellStyle.SelectionBackColor = System.Drawing.Color.FromArgb(236, 230, 239);

                gridview_NGProducts.AllowUserToResizeRows = false;

                double DPI_ratio = Graphics.FromHwnd(new WindowInteropHelper(m_parent).Handle).DpiX / 96;

                gridview_NGProducts.ColumnHeadersDefaultCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
                //gridview_CalibResult.Columns[0].Width = 70;
                //gridview_CalibResult.Columns[1].Width = 155;
                //gridview_CalibResult.Columns[2].Width = 165;

                gridview_NGProducts.Columns[0].Width = ((int)(gridview_NGProducts.Width * DPI_ratio) * 10) / 100;
                gridview_NGProducts.Columns[1].Width = ((int)(gridview_NGProducts.Width * DPI_ratio) * 50) / 100;
                gridview_NGProducts.Columns[2].Width = ((int)(gridview_NGProducts.Width * DPI_ratio) * 20) / 100;
                gridview_NGProducts.Columns[3].Width = ((int)(gridview_NGProducts.Width * DPI_ratio) * 20) / 100;
                gridview_NGProducts.Columns[0].Name = "序号";
                gridview_NGProducts.Columns[1].Name = "二维码";
                gridview_NGProducts.Columns[2].Name = "确认状态";
                gridview_NGProducts.Columns[3].Name = "预留";

                for (int n = 0; n < gridview_NGProducts.ColumnCount; n++)
                    gridview_NGProducts.Columns[n].SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            }

            // 添加NG产品二维码
            if (m_list_NG_barcodes.Count > 0)
            {
                for (int n = 0; n < m_list_NG_barcodes.Count; n++)
                {
                    gridview_NGProducts.Rows.Add((n + 1).ToString(), m_list_NG_barcodes[n], "未确认", "");

                    // 把第三列设为黄色背景
                    gridview_NGProducts.Rows[n].Cells[2].Style.BackColor = System.Drawing.Color.Yellow;

                    gridview_NGProducts.Rows[n].Height = 30;

                    m_list_NG_confirm_status.Add(false);
                }
            }
        }

        // 确定按钮
        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            // 判断是否所有NG产品都已经确认
            if (false == m_bIsAllNGProductsConfirmed)
            {
                System.Windows.MessageBox.Show("还有NG产品未确认，请确认所有NG产品后再点击确定。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            m_bOK = true;

            this.Close();
        }

        // 取消按钮
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // 文本框：二维码
        private void textbox_Barcode_TextChanged(object sender, TextChangedEventArgs e)
        {
            // 判断文本是否匹配m_list_NG_barcodes，如果匹配，则把对应的行的第三列设为"已确认"，并把背景设为绿色，文本为"√"，二维码文本框清空，且焦点回到二维码文本框
            if (true)
            {
                for (int n = 0; n < m_list_NG_barcodes.Count; n++)
                {
                    if (textbox_Barcode.Text == m_list_NG_barcodes[n])
                    {
                        gridview_NGProducts.Rows[n].Cells[2].Value = "已确认";
                        gridview_NGProducts.Rows[n].Cells[2].Style.BackColor = System.Drawing.Color.FromArgb(0, 255, 0);
                        gridview_NGProducts.Rows[n].Cells[2].Value = "√";

                        textbox_Barcode.Text = "";
                        textbox_Barcode.Focus();
                        textbox_Barcode.CaretIndex = textbox_Barcode.Text.Length;

                        m_list_NG_confirm_status[n] = true;
                    }
                }
            }

            // 判断是否所有NG产品都已经确认
            m_bIsAllNGProductsConfirmed = true;
            for (int n = 0; n < m_list_NG_confirm_status.Count; n++)
            {
                if (m_list_NG_confirm_status[n] == false)
                {
                    m_bIsAllNGProductsConfirmed = false;
                    break;
                }
            }

            // 如果所有NG产品都已经确认，则弹出提示框
            if (m_bIsAllNGProductsConfirmed)
            {
                System.Windows.MessageBox.Show("所有NG产品都已经确认，可以关闭本页面，并将复判结果提交到MES系统。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
