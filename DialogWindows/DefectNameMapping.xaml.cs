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

namespace AutoScanFQCTest.DialogWindows
{
    public partial class DefectNameMapping : Window
    {
        MainWindow m_parent;

        public bool m_bResultOK = false;

        public DefectNameMapping(MainWindow parent)
        {
            InitializeComponent();

            m_parent = parent;

            // 设置窗口事件
            this.Loaded += Window_Loaded;
        }

        // 窗口：加载
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // 构造列表gridview_DefectNameMapping
            gridview_DefectNameMapping.RowHeadersVisible = false;
            gridview_DefectNameMapping.ReadOnly = true;
            gridview_DefectNameMapping.ColumnCount = 3;
            gridview_DefectNameMapping.ColumnHeadersHeight = 30;
            gridview_DefectNameMapping.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;

            gridview_DefectNameMapping.EnableHeadersVisualStyles = false;
            gridview_DefectNameMapping.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(236, 230, 239);

            gridview_DefectNameMapping.ColumnHeadersVisible = true;
            gridview_DefectNameMapping.RowsDefaultCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;

            gridview_DefectNameMapping.ColumnHeadersDefaultCellStyle.SelectionBackColor = System.Drawing.Color.FromArgb(236, 230, 239);

            gridview_DefectNameMapping.AllowUserToResizeRows = false;

            double DPI_ratio = Graphics.FromHwnd(new WindowInteropHelper(m_parent).Handle).DpiX / 96;

            gridview_DefectNameMapping.ColumnHeadersDefaultCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            //gridview_CalibResult.Columns[0].Width = 70;
            //gridview_CalibResult.Columns[1].Width = 155;
            //gridview_CalibResult.Columns[2].Width = 165;

            gridview_DefectNameMapping.Columns[0].Width = ((int)(gridview_DefectNameMapping.Width * DPI_ratio) * 10) / 100;
            gridview_DefectNameMapping.Columns[1].Width = ((int)(gridview_DefectNameMapping.Width * DPI_ratio) * 45) / 100;
            gridview_DefectNameMapping.Columns[2].Width = ((int)(gridview_DefectNameMapping.Width * DPI_ratio) * 45) / 100;
            gridview_DefectNameMapping.Columns[0].Name = "序号";
            gridview_DefectNameMapping.Columns[1].Name = "原缺陷名称";
            gridview_DefectNameMapping.Columns[2].Name = "新名称";

            for (int n = 0; n < gridview_DefectNameMapping.ColumnCount; n++)
                gridview_DefectNameMapping.Columns[n].SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;

            // 根据m_parent.m_global.m_dict_old_defect_names_and_new_defect_names_mapping，填充gridview_DefectNameMapping
            for (int i = 0; i < m_parent.m_global.m_dict_old_defect_names_and_new_defect_names_mapping.Count; i++)
            {
                gridview_DefectNameMapping.Rows.Add(i + 1, 
                    m_parent.m_global.m_dict_old_defect_names_and_new_defect_names_mapping.Keys.ElementAt(i), m_parent.m_global.m_dict_old_defect_names_and_new_defect_names_mapping.Values.ElementAt(i));
            }
        }

        // 按钮：修改
        private void BtnModify_Click(object sender, RoutedEventArgs e)
        {
            if (gridview_DefectNameMapping.SelectedRows.Count == 0)
            {
                MessageBox.Show("请选择要修改的行！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            int nSelectedIndex = gridview_DefectNameMapping.SelectedRows[0].Index;

            if (gridview_DefectNameMapping.Rows[nSelectedIndex].Cells[1].Value == null)
                return;

            string strOldDefectName = textbox_OldName.Text;
            string strNewDefectName = textbox_NewName.Text;

            if (strOldDefectName == "" || strNewDefectName == "")
            {
                MessageBox.Show("请输入缺陷名称！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            m_parent.m_global.m_dict_old_defect_names_and_new_defect_names_mapping.Remove(strOldDefectName);
            m_parent.m_global.m_dict_old_defect_names_and_new_defect_names_mapping.Add(strOldDefectName, strNewDefectName);

            gridview_DefectNameMapping.Rows[nSelectedIndex].Cells[2].Value = strNewDefectName;
        }

        // 按钮：添加
        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            string strOldDefectName = textbox_OldName.Text;
            string strNewDefectName = textbox_NewName.Text;

            if (strOldDefectName == "" || strNewDefectName == "")
            {
                MessageBox.Show("请输入缺陷名称！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (m_parent.m_global.m_dict_old_defect_names_and_new_defect_names_mapping.ContainsKey(strOldDefectName))
            {
                MessageBox.Show("已存在相同的原缺陷名称！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            m_parent.m_global.m_dict_old_defect_names_and_new_defect_names_mapping.Add(strOldDefectName, strNewDefectName);

            gridview_DefectNameMapping.Rows.Add(gridview_DefectNameMapping.Rows.Count, strOldDefectName, strNewDefectName);
        }

        // 按钮：删除
        private void BtnRemove_Click(object sender, RoutedEventArgs e)
        {
            if (gridview_DefectNameMapping.SelectedRows.Count == 0)
            {
                MessageBox.Show("请选择要删除的行！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            int nSelectedIndex = gridview_DefectNameMapping.SelectedRows[0].Index;

            if (gridview_DefectNameMapping.Rows[nSelectedIndex].Cells[1].Value == null)
                return;

            string strOldDefectName = gridview_DefectNameMapping.Rows[nSelectedIndex].Cells[1].Value.ToString();

            m_parent.m_global.m_dict_old_defect_names_and_new_defect_names_mapping.Remove(strOldDefectName);

            gridview_DefectNameMapping.Rows.RemoveAt(nSelectedIndex);
        }

        // 按钮：确定
        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            m_bResultOK = true;

            if (true == m_parent.m_global.SaveDefectNameMappingTable("defect_name_mapping.ini"))
            {
                MessageBox.Show("保存成功！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {

            }

            this.Close();
        }

        // 按钮：取消
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void gridview_DefectNameMapping_CellMouseClick(object sender, System.Windows.Forms.DataGridViewCellMouseEventArgs e)
        {
            string strOldDefectName = gridview_DefectNameMapping.Rows[e.RowIndex].Cells[1].Value.ToString();
            string strNewDefectName = gridview_DefectNameMapping.Rows[e.RowIndex].Cells[2].Value.ToString();

            if (strOldDefectName == "" || strNewDefectName == "")
                return;

            textbox_OldName.Text = strOldDefectName;
            textbox_NewName.Text = strNewDefectName;
        }
    }
}
