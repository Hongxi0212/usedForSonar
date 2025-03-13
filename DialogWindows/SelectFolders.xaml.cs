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
    public partial class SelectFolders : Window
    {
        public MainWindow m_parent;

        public bool m_bResultOK = false;

        // 构造函数
        public SelectFolders(MainWindow parent)
        {
            InitializeComponent();

            m_parent = parent;

            this.Topmost = true;

            textbox_AVIImageRemoteDir.Text = m_parent.m_global.m_strAVIImageRemoteDir;

            this.Loaded += Window_Loaded;
            this.ContentRendered += Window_ContentRendered;
        }

        // 窗口加载
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
        }

        // 窗口内容渲染
        private void Window_ContentRendered(object sender, EventArgs e)
        {
            textbox_AVIImageRemoteDir.Focus();
            textbox_AVIImageRemoteDir.CaretIndex = textbox_AVIImageRemoteDir.Text.Length;
        }

        // 按钮点击事件：选择远程AVI图片目录
        private void BtnSelectAVIImageRemoteDir_Click(object sender, RoutedEventArgs e)
        {
            m_parent.m_global.m_strAVIImageRemoteDir = textbox_AVIImageRemoteDir.Text;
        }

        // 按钮：确定
        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            m_parent.m_global.m_strAVIImageRemoteDir = textbox_AVIImageRemoteDir.Text;

            m_bResultOK = true;

            // 提示保存成功，但需要强制退出程序，重启后才能生效
            MessageBox.Show(this, "保存成功，但需要重启软件，方能生效。\r\n请重启软件。", "提示", MessageBoxButton.OK);

            this.Close();
            m_parent.Close();
            Environment.Exit(0);

            this.Close();
        }

        // 按钮：取消
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
