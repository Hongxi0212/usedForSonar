using AutoScanFQCTest.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security;
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
    // 用户权限
    public enum UserPermission
    {
        CreateTask = 0,
        LoadTask = 1,
    }

    // 用户组
    public class UserGroup
    {
        public int m_nGroupID = -1;                                                                                                                                 // 用户组ID

        public string m_strGroupName = "";                                                                                                                    // 用户组名称

        public bool[] m_bUserPermissions = new bool[Enum.GetNames(typeof(UserPermission)).Length];                // 用户权限

        // 构造函数
        public UserGroup(int nID, string strName)
        {
            m_nGroupID = nID;
            m_strGroupName = strName;

            if ((m_strGroupName == "super admin" && m_nGroupID == 0) || (m_strGroupName == "admin" && m_nGroupID == 1))
            {
                for (int i = 0; i < m_bUserPermissions.Length; i++)
                {
                    m_bUserPermissions[i] = true;
                }
            }
            else
            {
                for (int i = 0; i < m_bUserPermissions.Length; i++)
                {
                    m_bUserPermissions[i] = false;
                }
            }
        }

        public UserGroup()
        {
            m_nGroupID = -1;
            m_strGroupName = "";

            for (int i = 0; i < m_bUserPermissions.Length; i++)
            {
                m_bUserPermissions[i] = false;
            }
        }
    }

    // 用户
    public class User
    {
        //public const string SUPER_ADMIN = "SuperAdmin";
        //public const string SUPER_ADMIN_PASSWORD = "SuperAdmin";
        public const string SUPER_ADMIN = "a";
        public const string SUPER_ADMIN_PASSWORD = "a";

        public const string ADMIN = "admin";
        public const string ADMIN_PASSWORD = "admin";

        public int m_nUserId;
        public int m_nUserGroupId;

        public string m_strUserName;
        public string m_strPassword;

        public User(int nUserId, int nUserGroupId, string strUserName, string strPassword)
        {
            m_nUserId = nUserId;
            m_nUserGroupId = nUserGroupId;
            m_strUserName = strUserName;
            m_strPassword = strPassword;
        }

        public User()
        {
            m_nUserId = -1;
            m_nUserGroupId = -1;
            m_strUserName = "";
            m_strPassword = "";
        }
    }

    public partial class UserPermissionManagement : Window
    {
        public MainWindow m_parent;

        private bool m_bIsCreatingNewUserGroup = false;                                  // 是否正在创建新用户组
        private bool m_bIsCreatingNewUser = false;                                             // 是否正在创建新用户

        private SecureString m_strPassword = new SecureString();

        private int m_nSelectedUserGroupIndex = -1;                                        // 选中的用户组索引

        private System.Timers.Timer m_timer = new System.Timers.Timer(60000); // 设置间隔为5分钟

        public UserPermissionManagement(MainWindow parent)
        {
            InitializeComponent();

            m_parent = parent;

            grid_UserGroups.Visibility = Visibility.Hidden;
            grid_UserOverview.Visibility = Visibility.Hidden;
            grid_CreateNewUserGroup.Visibility = Visibility.Hidden;

            BtnSaveNewUserGroup.IsEnabled = false;
            BtnSaveNewUser.IsEnabled = false;

            this.Loaded += Window_Loaded;
        }

        // 窗口加载
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            m_nSelectedUserGroupIndex = 0;

            // 导入用户组数据，构造用户组列表
            if (true == ImportUserGroupData("user_group.ini", ref m_parent.m_global.m_user_groups))
            {
                for (int n = 2; n < m_parent.m_global.m_user_groups.Count; n++)
                {
                    combobox_UserGroup.Items.Add(m_parent.m_global.m_user_groups[n].m_strGroupName);
                }

                if (combobox_UserGroup.Items.Count > 0)
                {
                    combobox_UserGroup.SelectedIndex = 0;
                }
            }

            // 导入用户数据，构造用户列表
            if (true == ImportUserData("user.ini", ref m_parent.m_global.m_users))
            {

            }

            // 构造用户组列表gridview
            if (true)
            {
                gridview_UserGroups.RowHeadersVisible = false;
                gridview_UserGroups.ReadOnly = true;
                gridview_UserGroups.ColumnCount = 2;
                gridview_UserGroups.ColumnHeadersHeight = 30;
                gridview_UserGroups.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;

                gridview_UserGroups.EnableHeadersVisualStyles = false;
                gridview_UserGroups.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(236, 230, 239);

                gridview_UserGroups.ColumnHeadersVisible = true;
                gridview_UserGroups.RowsDefaultCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;

                gridview_UserGroups.ColumnHeadersDefaultCellStyle.SelectionBackColor = System.Drawing.Color.FromArgb(236, 230, 239);

                gridview_UserGroups.AllowUserToResizeRows = false;

                double DPI_ratio = Graphics.FromHwnd(new WindowInteropHelper(m_parent).Handle).DpiX / 96;

                gridview_UserGroups.ColumnHeadersDefaultCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
                gridview_UserGroups.Columns[0].Width = ((int)(grid_UserGroups.ActualWidth * DPI_ratio) * 50) / 100;
                gridview_UserGroups.Columns[1].Width = ((int)(grid_UserGroups.ActualWidth * DPI_ratio) * 50) / 100;
                gridview_UserGroups.Columns[0].Name = "用户组";
                gridview_UserGroups.Columns[1].Name = "人数";
                for (int n = 0; n < gridview_UserGroups.ColumnCount; n++)
                    gridview_UserGroups.Columns[n].SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;

                foreach (System.Windows.Forms.DataGridViewRow row in gridview_UserGroups.Rows)
                {
                    //row.DefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(247, 245, 249);
                }

                if (gridview_UserGroups.Rows.Count > 0)
                {
                    gridview_UserGroups.Rows[0].Selected = true;
                }
            }

            // 构造用户列表gridview
            if (true)
            {
                gridview_Users.RowHeadersVisible = false;
                gridview_Users.ReadOnly = true;
                gridview_Users.ColumnCount = 2;
                gridview_Users.ColumnHeadersHeight = 30;
                gridview_Users.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;

                gridview_Users.EnableHeadersVisualStyles = false;
                gridview_Users.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(236, 230, 239);

                gridview_Users.ColumnHeadersVisible = true;
                gridview_Users.RowsDefaultCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;

                gridview_Users.ColumnHeadersDefaultCellStyle.SelectionBackColor = System.Drawing.Color.FromArgb(236, 230, 239);

                gridview_Users.AllowUserToResizeRows = false;

                double DPI_ratio = Graphics.FromHwnd(new WindowInteropHelper(m_parent).Handle).DpiX / 96;

                gridview_Users.ColumnHeadersDefaultCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
                gridview_Users.Columns[0].Width = ((int)(grid_Users.ActualWidth * DPI_ratio) * 50) / 100;
                gridview_Users.Columns[1].Width = ((int)(grid_Users.ActualWidth * DPI_ratio) * 50) / 100;
                gridview_Users.Columns[0].Name = "用户名";
                gridview_Users.Columns[1].Name = "密码";
                for (int n = 0; n < gridview_Users.ColumnCount; n++)
                    gridview_Users.Columns[n].SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;

                foreach (System.Windows.Forms.DataGridViewRow row in gridview_Users.Rows)
                {
                    //row.DefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(247, 245, 249);
                }

                if (gridview_Users.Rows.Count > 0)
                {
                    gridview_Users.Rows[0].Selected = true;
                }
            }
        }

        // 定时器
        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                m_parent.m_global.m_log_presenter.Log("工程师权限默认时间已过，退出工程师身份");

                m_parent.m_global.m_current_user = m_parent.m_global.m_users[10];

                m_parent.refresh_UI_controls_by_user_level(m_parent.m_global.m_current_user);

                Login(m_parent.m_global.m_current_user.m_strUserName, m_parent.m_global.m_current_user.m_strPassword);
            }));
        }

        // 功能：更新用户组列表
        public void UpdateUserGroupList(List<UserGroup> user_groups, List<User> users)
        {
            // 清空列表
            gridview_UserGroups.Rows.Clear();

            // 更新列表
            if (true)
            {
                // 超级管理员
                System.Windows.Forms.DataGridViewRow row = new System.Windows.Forms.DataGridViewRow();
                row.CreateCells(gridview_UserGroups);
                row.Cells[0].Value = "超级管理员";
                row.Cells[1].Value = "1";
                row.Height = 30;
                gridview_UserGroups.Rows.Add(row);

                // 管理员
                row = new System.Windows.Forms.DataGridViewRow();
                row.CreateCells(gridview_UserGroups);
                row.Cells[0].Value = "管理员";
                row.Cells[1].Value = "1";
                row.Height = 30;
                gridview_UserGroups.Rows.Add(row);

                // 普通用户组
                for (int n = 2; n < user_groups.Count; n++)
                {
                    int nNumOfUsers = 0;

                    for (int k = 0; k < users.Count; k++)
                    {
                        if (users[k].m_nUserGroupId == user_groups[n].m_nGroupID)
                        {
                            nNumOfUsers++;
                        }
                    }

                    row = new System.Windows.Forms.DataGridViewRow();
                    row.CreateCells(gridview_UserGroups);
                    row.Cells[0].Value = user_groups[n].m_strGroupName;
                    row.Cells[1].Value = nNumOfUsers.ToString();
                    row.Height = 30;
                    gridview_UserGroups.Rows.Add(row);
                }

                // 更新用户组下拉列表
                combobox_UserGroup.Items.Clear();
                for (int n = 2; n < user_groups.Count; n++)
                {
                    combobox_UserGroup.Items.Add(m_parent.m_global.m_user_groups[n].m_strGroupName);
                }
                if (combobox_UserGroup.Items.Count > 0)
                    combobox_UserGroup.SelectedIndex = 0;
            }
        }

        // 功能：更新用户列表---显示除了超级管理员和管理员之外的所有用户
        public void UpdateUserList(List<User> users)
        {
            // 清空列表
            gridview_Users.Rows.Clear();

            // 更新列表
            if (true)
            {
                for (int n = 2; n < users.Count; n++)
                {
                    System.Windows.Forms.DataGridViewRow row = new System.Windows.Forms.DataGridViewRow();
                    row.CreateCells(gridview_Users);
                    row.Cells[0].Value = users[n].m_strUserName;
                    row.Cells[1].Value = users[n].m_strPassword;
                    row.Height = 30;
                    gridview_Users.Rows.Add(row);
                }
            }
        }

        // 功能：更新用户列表---显示指定用户组的所有用户
        public void UpdateUserList(List<User> users, int nGroupID)
        {
            // 清空列表
            gridview_Users.Rows.Clear();

            // 更新列表
            if (true)
            {
                for (int n = 2; n < users.Count; n++)
                {
                    if (users[n].m_nUserGroupId == nGroupID)
                    {
                        System.Windows.Forms.DataGridViewRow row = new System.Windows.Forms.DataGridViewRow();
                        row.CreateCells(gridview_Users);
                        row.Cells[0].Value = users[n].m_strUserName;
                        row.Cells[1].Value = users[n].m_strPassword;
                        row.Height = 30;
                        gridview_Users.Rows.Add(row);
                    }
                }
            }
        }

        // 功能：登录
        public void Login(string strUser, string strPassword)
        {
            bool bIsValidUser = false;

            if (strUser.Trim() == User.SUPER_ADMIN && strPassword.Trim() == User.SUPER_ADMIN_PASSWORD)
            {
                // 超级管理员登录
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    grid_UserGroups.Visibility = Visibility.Visible;
                    grid_UserOverview.Visibility = Visibility.Visible;
                    grid_CreateNewUserGroup.Visibility = Visibility.Visible;

                    m_parent.Title = m_parent.m_global.m_strOriginalProgramTitle + " (超级管理员)";

                    m_parent.m_global.m_log_presenter.Log("超级管理员 登录");

                    // 更新用户组列表
                    UpdateUserGroupList(m_parent.m_global.m_user_groups, m_parent.m_global.m_users);

                    // 设置用户组默认选中行
                    if (m_parent.m_global.m_user_groups.Count > 2)
                    {
                        if (gridview_UserGroups.Rows.Count >= 3)
                        {
                            gridview_UserGroups.Rows[2].Selected = true;
                        }
                    }

                    // 更新用户组权限信息
                    RefreshUserGroupPermissionInfo(m_parent.m_global.m_user_groups[0]);
                }));

                bIsValidUser = true;
            }
            else if (strUser.Trim() == User.ADMIN && strPassword.Trim() == User.ADMIN_PASSWORD)
            {
                // 管理员登录
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    grid_UserGroups.Visibility = Visibility.Visible;
                    grid_UserOverview.Visibility = Visibility.Visible;
                    grid_CreateNewUserGroup.Visibility = Visibility.Visible;

                    m_parent.Title = m_parent.m_global.m_strOriginalProgramTitle + " (管理员)";

                    m_parent.m_global.m_log_presenter.Log("管理员 登录");

                    // 更新用户组列表
                    UpdateUserGroupList(m_parent.m_global.m_user_groups, m_parent.m_global.m_users);

                    // 设置用户组默认选中行
                    if (m_parent.m_global.m_user_groups.Count > 2)
                    {
                        if (gridview_UserGroups.Rows.Count >= 3)
                        {
                            gridview_UserGroups.Rows[2].Selected = true;
                        }
                    }

                    // 更新用户组权限信息
                    RefreshUserGroupPermissionInfo(m_parent.m_global.m_user_groups[1]);
                }));

                bIsValidUser = true;
            }
            else if (strUser.Trim() == "操作员")
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    grid_UserGroups.Visibility = Visibility.Hidden;

                    m_parent.Title = m_parent.m_global.m_strOriginalProgramTitle + string.Format(" (操作员)");

                    m_parent.m_global.m_log_presenter.Log(string.Format("{0} 登录", "操作员"));

                    m_parent.m_global.m_current_user = m_parent.m_global.m_users[10];
                }));

                bIsValidUser = true;

                // 如果切换到操作员，停止定时器
                m_timer.Stop();
            }
            else
            {
                // 检查用户名和密码是否正确
                for (int n = 2; n < m_parent.m_global.m_users.Count; n++)
                {
                    if (m_parent.m_global.m_users[n].m_strUserName == strUser && m_parent.m_global.m_users[n].m_strPassword == strPassword)
                    {
                        // 普通用户登录
                        Application.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            grid_UserGroups.Visibility = Visibility.Hidden;

                            // 获取用户组名称
                            string strGroupName = "";

                            for (int k = 0; k < m_parent.m_global.m_user_groups.Count; k++)
                            {
                                if (m_parent.m_global.m_user_groups[k].m_nGroupID == m_parent.m_global.m_users[n].m_nUserGroupId)
                                {
                                    strGroupName = m_parent.m_global.m_user_groups[k].m_strGroupName;
                                    break;
                                }
                            }

                            m_parent.Title = m_parent.m_global.m_strOriginalProgramTitle + string.Format(" ({0} {1})", strGroupName, m_parent.m_global.m_users[n].m_strUserName);

                            m_parent.m_global.m_log_presenter.Log(string.Format("{0} {1} 登录", strGroupName, m_parent.m_global.m_users[n].m_strUserName));

                            m_parent.m_global.m_current_user = m_parent.m_global.m_users[n];
                        }));

                        bIsValidUser = true;

                        Application.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            MessageBox.Show("登陆成功");
                        }));

                        break;
                    }
                }

                // 如果切换到管理员，启动定时器
                if (true == bIsValidUser && m_parent.m_global.m_current_user.m_nUserGroupId == 2)
                {
                    m_timer.Interval = 180000; // 设置间隔为5分钟

                    m_timer.Elapsed += Timer_Elapsed;

                    m_timer.Start();
                }

                // 普通用户登录
                //Application.Current.Dispatcher.Invoke(new Action(() =>
                //{
                //    grid_UserGroups.Visibility = Visibility.Hidden;

                //    m_parent.Title = m_parent.m_global.m_strOriginalProgramTitle + " (普通用户)";

                //    m_parent.m_global.m_log_presenter.Log("普通用户 登录");
                //}));

                //bIsValidUser = true;
            }

            // 登录失败
            if (false == bIsValidUser)
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    m_parent.m_global.m_log_presenter.Log("登录失败");

                    MessageBox.Show("账号或密码错误，登录失败！");
                }));
            }

            m_strPassword.Clear();

            textbox_Password.Text = "";
        }

        // 功能：保存用户组数据到文件
        public bool SaveUserGroupDataToFile(string strFileName, List<UserGroup> user_groups, bool bDeleteBeforeSave = false)
        {
            bool bSuccess = false;

            try
            {
                string filename = m_parent.m_global.m_strCurrentDirectory + strFileName;

                // 如果需要删除原来的文件
                if (true == bDeleteBeforeSave)
                {
                    if (File.Exists(filename))
                    {
                        File.Delete(filename);
                    }
                }

                INIFileParser parser = new INIFileParser(filename);

                parser.WritePrivateProfileString("一般", "用户组数目", user_groups.Count.ToString());

                string[] strPermissionNames = Enum.GetNames(typeof(UserPermission));

                for (int n = 2; n < user_groups.Count; n++)
                {
                    string strSection = "用户组" + n.ToString();

                    parser.WritePrivateProfileString(strSection, "用户组ID", user_groups[n].m_nGroupID.ToString());
                    parser.WritePrivateProfileString(strSection, "用户组名称", user_groups[n].m_strGroupName);

                    for (int k = 0; k < Enum.GetNames(typeof(UserPermission)).Length; k++)
                    {
                        parser.WritePrivateProfileString(strSection, strPermissionNames[k], Convert.ToInt32(user_groups[n].m_bUserPermissions[k]).ToString());
                    }
                }

                parser.Save();

                bSuccess = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            return bSuccess;
        }

        // 功能：保存用户数据到文件
        public bool SaveUserDataToFile(string strFileName, List<User> users, bool bDeleteBeforeSave = false)
        {
            bool bSuccess = false;

            try
            {
                string filename = m_parent.m_global.m_strCurrentDirectory + strFileName;

                // 如果需要删除原来的文件
                if (true == bDeleteBeforeSave)
                {
                    if (File.Exists(filename))
                    {
                        File.Delete(filename);
                    }
                }

                INIFileParser parser = new INIFileParser(filename);

                parser.WritePrivateProfileString("一般", "用户数目", (users.Count - 2).ToString());

                for (int n = 2; n < users.Count; n++)
                {
                    string strSection = "用户" + (n - 2).ToString();

                    parser.WritePrivateProfileString(strSection, "用户组ID", users[n].m_nUserGroupId.ToString());
                    parser.WritePrivateProfileString(strSection, "用户名", users[n].m_strUserName);
                    parser.WritePrivateProfileString(strSection, "密码", users[n].m_strPassword);
                }

                parser.Save();

                bSuccess = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            return bSuccess;
        }

        // 功能：导入用户组数据
        public bool ImportUserGroupData(string strFileName, ref List<UserGroup> user_groups)
        {
            bool bSuccess = false;

            try
            {
                if (user_groups.Count > 2)
                {
                    user_groups.RemoveRange(2, user_groups.Count - 2);
                }

                string filename = m_parent.m_global.m_strCurrentDirectory + strFileName;

                INIFileParser parser = new INIFileParser(filename);

                int nNumOfUserGroups = Convert.ToInt32(parser.GetPrivateProfileString("一般", "用户组数目", "0"));

                for (int n = 2; n < nNumOfUserGroups; n++)
                {
                    string strSection = "用户组" + n.ToString();

                    UserGroup user_group = new UserGroup();

                    user_group.m_nGroupID = Convert.ToInt32(parser.GetPrivateProfileString(strSection, "用户组ID", "-1"));
                    user_group.m_strGroupName = parser.GetPrivateProfileString(strSection, "用户组名称", "");

                    string[] strPermissionNames = Enum.GetNames(typeof(UserPermission));

                    for (int k = 0; k < Enum.GetNames(typeof(UserPermission)).Length; k++)
                    {
                        user_group.m_bUserPermissions[k] = Convert.ToBoolean(Convert.ToInt32(parser.GetPrivateProfileString(strSection, strPermissionNames[k], "0")));
                    }

                    user_groups.Add(user_group);
                }

                bSuccess = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            return bSuccess;
        }

        // 功能：导入用户数据
        public bool ImportUserData(string strFileName, ref List<User> users)
        {
            bool bSuccess = false;

            try
            {
                if (users.Count > 2)
                {
                    users.RemoveRange(2, users.Count - 2);
                }

                string filename = m_parent.m_global.m_strCurrentDirectory + strFileName;

                INIFileParser parser = new INIFileParser(filename);

                int nNumOfUsers = Convert.ToInt32(parser.GetPrivateProfileString("一般", "用户数目", "0"));

                for (int n = 0; n < nNumOfUsers; n++)
                {
                    string strSection = "用户" + n.ToString();

                    User user = new User();

                    user.m_nUserGroupId = Convert.ToInt32(parser.GetPrivateProfileString(strSection, "用户组ID", "-1"));
                    user.m_strUserName = parser.GetPrivateProfileString(strSection, "用户名", "");
                    user.m_strPassword = parser.GetPrivateProfileString(strSection, "密码", "");

                    users.Add(user);
                }

                // 初始化当前用户
                for (int n = 2 + nNumOfUsers; n < 20; n++)
                {
                    User user = new User(n, 3, "操作员", "操作员");

                    users.Add(user);
                }

                bSuccess = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            return bSuccess;
        }

        // 功能：刷新用户组权限信息
        private void RefreshUserGroupPermissionInfo(UserGroup user_group)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                CheckBox_CreateTask.IsChecked = user_group.m_bUserPermissions[(int)UserPermission.CreateTask];
                CheckBox_LoadTask.IsChecked = user_group.m_bUserPermissions[(int)UserPermission.LoadTask];
            }));
        }

        // 功能：刷新用户列表
        private void RefreshUserList(UserGroup user_group, List<User> users, int nGroupID)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                List<User> users_in_group = new List<User>();

                for (int n = 2; n < users.Count; n++)
                {
                    if (users[n].m_nUserGroupId == nGroupID)
                    {
                        users_in_group.Add(users[n]);
                    }
                }


            }));
        }

        // 线程：5分钟后切换回操作员身份
        public void thread_poll_MES_json_file()
        {

        }

        // 按钮：登录
        private void BtnSignIn_Click(object sender, RoutedEventArgs e)
        {
            string strUser = textbox_User.Text;
            string strPassword = GeneralUtilities.ConvertSecureStringToString(m_strPassword);

            Login(strUser, strPassword);

            m_parent.refresh_UI_controls_by_user_level(m_parent.m_global.m_current_user);
        }

        // 按钮：创建新用户组
        private void BtnCreateNewUserGroup_Click(object sender, RoutedEventArgs e)
        {
            m_bIsCreatingNewUserGroup = true;

            BtnCreateNewUserGroup.IsEnabled = false;
            BtnSaveNewUserGroup.IsEnabled = true;
        }

        // 按钮：保存新用户组
        private void BtnSaveNewUserGroup_Click(object sender, RoutedEventArgs e)
        {
            string strGroupName = textbox_NameOfNewUserGroup.Text;

            if (true == m_bIsCreatingNewUserGroup)
            {
                if (strGroupName.Trim().Length > 0)
                {
                    if (MessageBox.Show(string.Format("请确认是否创建并保存新用户组 {0}？", strGroupName), "提示", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        // 创建新用户组
                        if (true)
                        {
                            int nNewUserGroupID = m_parent.m_global.m_user_groups.Count;

                            UserGroup new_group = new UserGroup(nNewUserGroupID, strGroupName);

                            new_group.m_bUserPermissions[Convert.ToInt32(UserPermission.CreateTask)] = CheckBox_CreateTask.IsChecked.Value;
                            new_group.m_bUserPermissions[Convert.ToInt32(UserPermission.LoadTask)] = CheckBox_LoadTask.IsChecked.Value;

                            m_parent.m_global.m_user_groups.Add(new_group);

                            // 更新列表
                            UpdateUserGroupList(m_parent.m_global.m_user_groups, m_parent.m_global.m_users);

                            gridview_UserGroups.Rows[nNewUserGroupID].Selected = true;

                            // 保存到文件
                            if (true == SaveUserGroupDataToFile("user_group.ini", m_parent.m_global.m_user_groups, true))
                            {
                                // 提示保存成功
                                MessageBox.Show("保存成功！");
                            }
                            // 保存失败
                            else
                            {
                                // 提示保存失败
                                MessageBox.Show("保存失败！");
                            }
                        }
                    }

                    m_bIsCreatingNewUserGroup = false;

                    BtnCreateNewUserGroup.IsEnabled = true;
                    BtnSaveNewUserGroup.IsEnabled = false;
                }
                else
                {
                    MessageBox.Show("请输入新用户组名称！");
                }
            }
        }

        // 按钮：删除用户组
        private void BtnRemoveUserGroup_Click(object sender, RoutedEventArgs e)
        {
            if (0 == m_nSelectedUserGroupIndex || 1 == m_nSelectedUserGroupIndex)
            {
                MessageBox.Show("不能删除系统默认用户组！");
                return;
            }
            if (m_nSelectedUserGroupIndex < 0)
            {
                MessageBox.Show("请选择要删除的用户组！");
                return;
            }
            if (m_nSelectedUserGroupIndex >= m_parent.m_global.m_user_groups.Count)
            {
                MessageBox.Show("请选择要删除的用户组！");
                return;
            }

            if (MessageBox.Show(string.Format("请确认是否删除用户组: {0}？", m_parent.m_global.m_user_groups[m_nSelectedUserGroupIndex].m_strGroupName),
                "提示", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                // 删除用户组
                if (true)
                {
                    m_parent.m_global.m_user_groups.RemoveAt(m_nSelectedUserGroupIndex);

                    // 更新列表
                    UpdateUserGroupList(m_parent.m_global.m_user_groups, m_parent.m_global.m_users);

                    // 保存到文件
                    if (true == SaveUserGroupDataToFile("user_group.ini", m_parent.m_global.m_user_groups, true))
                    {
                        // 提示保存成功
                        MessageBox.Show("保存成功！");
                    }
                    // 保存失败
                    else
                    {
                        // 提示保存失败
                        MessageBox.Show("保存失败！");
                    }
                }
            }
        }

        // 按钮：创建新用户
        private void BtnCreateNewUser_Click(object sender, RoutedEventArgs e)
        {
            if (combobox_UserGroup.Items.Count <= 0)
            {
                MessageBox.Show("请先创建非管理员的用户组！");
                return;
            }

            m_bIsCreatingNewUser = true;

            BtnCreateNewUser.IsEnabled = false;
            BtnSaveNewUser.IsEnabled = true;
        }

        // 按钮：保存新用户
        private void BtnSaveNewUser_Click(object sender, RoutedEventArgs e)
        {
            if (combobox_UserGroup.Items.Count <= 0)
            {
                MessageBox.Show("请先创建非管理员的用户组！");
                return;
            }

            string strUserGroup = combobox_UserGroup.SelectedItem.ToString();
            string strUserName = textbox_NewUserName.Text;
            string strPassword = textbox_NewUserPassword.Text;
            string strPasswordToConfirm = textbox_NewUserPasswordForConfirm.Text;

            if (true == m_bIsCreatingNewUser)
            {
                if (strUserGroup.Trim().Length > 0 && strUserName.Trim().Length > 0 && strPassword.Trim().Length > 0 && strPasswordToConfirm.Trim().Length > 0)
                {
                    // 检查用户名是否已存在
                    if (true)
                    {
                        for (int i = 0; i < m_parent.m_global.m_users.Count; i++)
                        {
                            if (m_parent.m_global.m_users[i].m_strUserName == strUserName)
                            {
                                MessageBox.Show("该用户名已存在！");
                                return;
                            }
                        }
                    }

                    if (strPassword != strPasswordToConfirm)
                    {
                        MessageBox.Show("两次输入的密码不一致！");
                        return;
                    }

                    if (MessageBox.Show(string.Format("请确认是否创建并保存新用户 {0}？", strUserName), "提示", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        // 创建新用户
                        if (true)
                        {
                            int nGroupID = combobox_UserGroup.SelectedIndex + 2;

                            int nNewUserID = m_parent.m_global.m_users.Count;

                            User new_user = new User(nNewUserID, nGroupID, strUserName, strPassword);

                            m_parent.m_global.m_users.Add(new_user);

                            // 更新用户列表
                            UpdateUserList(m_parent.m_global.m_users);

                            // 更新用户组列表
                            UpdateUserGroupList(m_parent.m_global.m_user_groups, m_parent.m_global.m_users);

                            gridview_UserGroups.Rows[nGroupID].Selected = true;

                            if (gridview_Users.Rows.Count >= 2)
                                gridview_Users.Rows[gridview_Users.Rows.Count - 2].Selected = true;

                            // 保存到文件
                            if (true == SaveUserDataToFile("user.ini", m_parent.m_global.m_users, true))
                            {
                                // 提示保存成功
                                MessageBox.Show("保存成功！");
                            }
                            // 保存失败
                            else
                            {
                                // 提示保存失败
                                MessageBox.Show("保存失败！");
                            }
                        }
                    }
                }
                else
                {
                    MessageBox.Show("请输入完整的新用户信息！");
                }
            }
        }

        // 按钮：删除用户
        private void BtnDeleteUser_Click(object sender, RoutedEventArgs e)
        {
            string strUserName = "";

            if (gridview_Users.SelectedRows.Count > 0)
            {
                strUserName = gridview_Users.SelectedRows[0].Cells[0].Value.ToString();
            }
            else
            {
                MessageBox.Show("请选择要删除的用户！");
                return;
            }

            if (MessageBox.Show(string.Format("请确认是否删除用户: {0}？", strUserName), "提示", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                // 删除用户
                if (true)
                {
                    int nSelectedUserID = gridview_Users.SelectedRows[0].Index;

                    m_parent.m_global.m_users.RemoveAt(nSelectedUserID);

                    // 更新列表
                    UpdateUserList(m_parent.m_global.m_users);

                    // 更新用户组列表
                    UpdateUserGroupList(m_parent.m_global.m_user_groups, m_parent.m_global.m_users);

                    // 保存到文件
                    if (true == SaveUserDataToFile("user.ini", m_parent.m_global.m_users, true))
                    {
                        // 提示保存成功
                        MessageBox.Show("保存成功！");
                    }
                    // 保存失败
                    else
                    {
                        // 提示保存失败
                        MessageBox.Show("保存失败！");
                    }
                }
            }
        }

        // 按钮：取消
        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {

        }

        // 文本框：密码
        private void textbox_Password_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;
            textBox.Text += new string('*', e.Text.Length);
            textBox.CaretIndex = textBox.Text.Length;

            // Add the actual input to the password
            foreach (char c in e.Text)
            {
                m_strPassword.AppendChar(c);
            }

            e.Handled = true;
        }

        // 文本框：密码
        private void textbox_Password_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                var textBox = sender as TextBox;
                textBox.Text += "*";
                textBox.CaretIndex = textBox.Text.Length;

                // Add the space to the password
                m_strPassword.AppendChar(' ');

                e.Handled = true;
            }
            else if (e.Key == Key.Back)
            {
                var textBox = sender as TextBox;
                if (textBox.Text.Length > 0)
                {
                    textBox.Text = textBox.Text.Substring(0, textBox.Text.Length - 1);
                    textBox.CaretIndex = textBox.Text.Length;

                    // Remove the last character from the password
                    if (m_strPassword.Length > 0)
                    {
                        m_strPassword.RemoveAt(m_strPassword.Length - 1);
                    }
                }
                e.Handled = true;
            }
        }

        // 文本框：密码
        private void textbox_Password_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(String)))
            {
                String text = (String)e.DataObject.GetData(typeof(String));
                textbox_Password.Text += new string('*', text.Length);

                // Append the actual input to the password
                foreach (char c in text)
                {
                    m_strPassword.AppendChar(c);
                }

                e.CancelCommand();
            }
            else
            {
                e.CancelCommand();
            }

        }

        // 用户组列表框点击事件
        private void gridview_UserGroups_CellMouseClick(object sender, System.Windows.Forms.DataGridViewCellMouseEventArgs e)
        {
            if (gridview_UserGroups.RowCount <= 1)
                return;
            if (e.RowIndex == (gridview_UserGroups.RowCount - 1))
                return;

            int idx = e.RowIndex;

            if (idx < m_parent.m_global.m_user_groups.Count)
            {
                m_nSelectedUserGroupIndex = idx;

                RefreshUserGroupPermissionInfo(m_parent.m_global.m_user_groups[m_nSelectedUserGroupIndex]);

                // 更新用户列表
                UpdateUserList(m_parent.m_global.m_users, m_nSelectedUserGroupIndex);

                // 更新用户组组合框
                if (m_nSelectedUserGroupIndex >= 2 && (m_nSelectedUserGroupIndex - 2) < combobox_UserGroup.Items.Count)
                {
                    combobox_UserGroup.SelectedIndex = m_nSelectedUserGroupIndex - 2;
                }
            }
        }

        // 用户列表框点击事件
        private void gridview_Users_CellMouseClick(object sender, System.Windows.Forms.DataGridViewCellMouseEventArgs e)
        {

        }

        // 组合框选择变更：用户组
        private void combobox_UserGroup_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (combobox_UserGroup.Items.Count <= 0)
            {
                return;
            }

            int nSelectedIndex = combobox_UserGroup.SelectedIndex;

            // 更新用户列表
            UpdateUserList(m_parent.m_global.m_users, nSelectedIndex + 2);
        }
    }
}
