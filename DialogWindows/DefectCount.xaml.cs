using AutoScanFQCTest.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MessageBox = System.Windows.MessageBox;

namespace AutoScanFQCTest.DialogWindows
{
    /// <summary>
    /// DefectCount.xaml 的交互逻辑
    /// </summary>
    public partial class DefectCount : Window
    {
        MainWindow m_parent;
        public DefectCount(MainWindow parent)
        {
            InitializeComponent();
            m_parent = parent;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadDGV();
        }

        private void LoadDGV()
        {
            try
            {
                string sql = "select * from defectcount;";
                var data = MySqlHelpers.GetDataTable(m_parent.m_global.ConnsectionString, CommandType.Text, sql);
                if (data == null || data?.Rows.Count > 0)
                {
                    MessageBox.Show("未能查询到缺陷统计！");
                    return;
                }
                var list = ListHelper.DataTableToList<DataModels.DefectCount>(data);
                gridview_QueryResults.AutoGenerateColumns = false;
                gridview_QueryResults.DataSource = null;
                gridview_QueryResults.Columns.Clear();
                DataGridViewTextBoxColumn DefectName = new DataGridViewTextBoxColumn();
                DefectName.HeaderText = "缺陷名称";
                DefectName.DataPropertyName = "DefectName"; // 关联到实体类的Id属性
                gridview_QueryResults.Columns.Add(DefectName);
                DataGridViewTextBoxColumn Count = new DataGridViewTextBoxColumn();
                Count.HeaderText = "数量";
                Count.DataPropertyName = "Count"; // 关联到实体类的Id属性
                gridview_QueryResults.Columns.Add(Count);
                gridview_QueryResults.ColumnHeadersHeight = 40;
                double DPI_ratio = 1.0;
                gridview_QueryResults.Columns[0].Width = ((int)(gridview_QueryResults.Width * DPI_ratio) * 50) / 100;
                gridview_QueryResults.Columns[1].Width = ((int)(gridview_QueryResults.Width * DPI_ratio) * 50) / 100;
                gridview_QueryResults.DataSource = list;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                m_parent.m_global.m_log_presenter.Log(ex.Message);
            }
        }
    }
}
