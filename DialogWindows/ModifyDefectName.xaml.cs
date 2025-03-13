using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AutoScanFQCTest.DialogWindows
{
    public partial class ModifyDefectName : Window
    {
        public MainWindow m_parent;

        public bool m_bConfirm = false;

        public string m_strCurrentDefectName = "";
        public string m_strNewDefectName = "";

        public ModifyDefectName(MainWindow parent, string strCurrentDefectName)
        {
            InitializeComponent();

            m_parent = parent;

            m_strCurrentDefectName = strCurrentDefectName;

            // 设置窗口事件
            this.Loaded += Window_Loaded;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            textbox_DefectName.Text = m_strCurrentDefectName;

            textbox_DefectName.Focus();
            textbox_DefectName.CaretIndex = textbox_DefectName.Text.Length;

            // textbox_DefectName全选，类似记事本选择所有文本的效果
            textbox_DefectName.SelectAll();
        }

        // 确认按钮
        private void button_Confirm_Click(object sender, RoutedEventArgs e)
        {
            // 弹出确认框
            MessageBoxResult result = MessageBox.Show("确定修改吗？", "提示", MessageBoxButton.OKCancel, MessageBoxImage.Question);
            if (result == MessageBoxResult.OK)
            {
                m_bConfirm = true;

                m_strNewDefectName = textbox_DefectName.Text;

                this.Close();
            }
        }

        // 取消按钮
        private void button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
