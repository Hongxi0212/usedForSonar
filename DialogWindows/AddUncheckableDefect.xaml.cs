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
    public partial class AddUncheckableDefect : Window
    {
        MainWindow m_parent;

        public bool m_bResultOK = false;

        public string m_strUncheckableDefectName = "";

        public AddUncheckableDefect(MainWindow parent, string strInfo)
        {
            InitializeComponent();

            m_parent = parent;

            textblock_Info.Text = strInfo;
        }

        // 按钮：确定
        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            if (textbox_Defect.Text == "")
            {
                MessageBox.Show("输入名称为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            m_bResultOK = true;
            m_strUncheckableDefectName = textbox_Defect.Text;

            this.Close();
        }

        // 按钮：取消
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
