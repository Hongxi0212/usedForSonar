using AutoScanFQCTest.DataModels;
using AutoScanFQCTest.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
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
    // 登录窗口
    public partial class LoginWindow : Window
    {
        public MainWindow m_parent;

        public bool m_bIsValidUser = false;

        // 构造函数
        public LoginWindow(MainWindow parent)
        {
            InitializeComponent();

            m_parent = parent;

            // 设置窗口事件
            this.Loaded += Window_Loaded;
            this.ContentRendered += Window_ContentRendered;
        }

        // 窗口加载
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // 读取用户和密码
            string currentDirectory = Directory.GetCurrentDirectory();
            string filePath = System.IO.Path.Combine(currentDirectory, "user.csv");

            m_parent.m_global.m_dictionary_users_and_passwords = GeneralUtilities.ReadUserAndPasswordCsvFile(filePath);

            // 如果需要检查密码，则显示密码输入框
            if (m_parent.m_global.m_bNeedToCheckPasswordWhenLogin == true)
            {
                grid_Password.Visibility = Visibility.Visible;
                passwordbox_Password.Visibility = Visibility.Visible;
            }
        }

        // 窗口内容渲染
        private void Window_ContentRendered(object sender, EventArgs e)
        {
            textbox_UserName.Focus();
        }

        // 登录按钮
        private void button_Login_Click(object sender, RoutedEventArgs e)
        {
            string strUserName = textbox_UserName.Text;

            if (string.IsNullOrEmpty(strUserName))
            {
                MessageBox.Show("请输入用户名！");
                return;
            }

            var userList = m_parent.m_global.FupanUsers.Where(user => user.UserName == strUserName).ToList();

            // 如果需要检查密码，则检查密码
            if (m_parent.m_global.m_bNeedToCheckPasswordWhenLogin == true)
            {

                if (userList.Count() < 1)
                {
                    MessageBox.Show("用户名不存在！");
                    return;
                }

                string strPassword = passwordbox_Password.Password;

                if(userList.First().Password != strPassword)
                {
					MessageBox.Show("密码错误！");
					return;
				}

                //if (m_parent.m_global.m_dictionary_users_and_passwords.ContainsKey(strUserName) == false)
                //{
                //    MessageBox.Show("用户名不存在！");
                //    return;
                //}

                //if (m_parent.m_global.m_dictionary_users_and_passwords[strUserName] != strPassword)
                //{
                //    MessageBox.Show("密码错误！");
                //    return;
                //}
            }

            var dlg = MessageBox.Show($"是否切换为{strUserName}？", "提示", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (dlg == MessageBoxResult.Yes)
            {
                m_parent.m_global.CurrentLoginType = userList.Count > 0 ? userList.First().UserType : UserType.操作员;
                m_parent.page_HomeView.TextBlock_UserType_Name.Text = m_parent.m_global.CurrentLoginType.ToString();
                //m_parent.page_HomeView.TextBlock_UserType_Name.Text = m_parent.m_global.FupanUsers.Where(user => user.UserName == strUserName).ToList().FirstOrDefault().UserType.ToString();

                m_parent.m_global.m_strCurrentOperatorID = strUserName;

                m_parent.statusbar_task_info.Text = "操作员信息: " + m_parent.m_global.m_strCurrentOperatorID;

                m_parent.m_global.UserChanged?.Invoke(strUserName);

                m_parent.page_HomeView.Visibility = Visibility.Visible;
                m_parent.toolBar_MainWindow.Visibility = Visibility.Visible;
                m_parent.statusBar_MainWindow.Visibility = Visibility.Visible;

                Close();
            }

            //string strPassword = passwordbox_Password.Password;

            //if (strUserName == "admin" && strPassword == "admin")
            //{
            //    m_bIsValidUser = true;
            //    this.Close();
            //}
            //else
            //{
            //    MessageBox.Show("用户名或密码错误，请重新输入！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            //}
        }

        // 取消按钮
        private void button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
