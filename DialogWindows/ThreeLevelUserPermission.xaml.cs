using AutoScanFQCTest.DataModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Tracing;
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
	public partial class ThreeLevelUserPermission: Window
	{
		public MainWindow m_parent;

		private bool m_bAdminConfirmed = false;

		// 构造函数
		public ThreeLevelUserPermission(MainWindow parent)
		{
			InitializeComponent();

			m_parent = parent;

			SetUIEnableStatus(m_bAdminConfirmed);

            //if (true == m_parent.m_global.m_bIsCardReaderEnabled)
            //{
            //    try
            //    {
     //               m_parent.m_global.ThreeLevelCardReader = new ScanCard(ScanCard.Brand.YW60x);
					//m_parent.m_global.ThreeLevelCardReader.Open();

     //               m_parent.m_global.ThreeLevelCardReader.OnScanCardEvent += FillTreeLevelUserName_OnScanCardEvent;
                //}
                //catch (Exception ex)
                //{
                //    m_parent.m_global.m_log_presenter.Log("读卡器初始化失败：" + ex.Message);

                //    m_parent.m_global.m_bIsCardReaderEnabled = false;
                //}
            //}

            // 设置窗口事件
            this.Loaded += Window_Loaded;
			this.Closing += Window_Close;

        }

		// 窗口加载事件
		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			if (true)
			{
				gridview_Engineer.RowHeadersVisible = false;
				gridview_Engineer.ReadOnly = true;
				gridview_Engineer.ColumnCount = 3;
				gridview_Engineer.ColumnHeadersHeight = 30;
				gridview_Engineer.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;

				gridview_Engineer.EnableHeadersVisualStyles = false;
				gridview_Engineer.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(236, 230, 239);

				gridview_Engineer.ColumnHeadersVisible = true;
				gridview_Engineer.RowsDefaultCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;

				gridview_Engineer.ColumnHeadersDefaultCellStyle.SelectionBackColor = System.Drawing.Color.FromArgb(236, 230, 239);

				gridview_Engineer.AllowUserToResizeRows = false;

				double DPI_ratio = Graphics.FromHwnd(new WindowInteropHelper(m_parent).Handle).DpiX / 96;

				gridview_Engineer.ColumnHeadersDefaultCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
				//gridview_Engineer.Columns[0].Width = ((int)(grid_UserGroups.ActualWidth * DPI_ratio) * 50) / 100;
				//gridview_Engineer.Columns[1].Width = ((int)(grid_UserGroups.ActualWidth * DPI_ratio) * 50) / 100;
				gridview_Engineer.Columns[0].Width = 149;
				gridview_Engineer.Columns[1].Width = 149;
				gridview_Engineer.Columns[2].Width = 149;
				gridview_Engineer.Columns[0].Name = "账户名";
				gridview_Engineer.Columns[1].Name = "密码";
				gridview_Engineer.Columns[2].Name = "身份";
				for (int n = 0; n < gridview_Engineer.ColumnCount; n++)
					gridview_Engineer.Columns[n].SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;

				try
				{
					//m_parent.UnregisterMainWindowScanCardEvent();
                    //m_parent.m_global.ThreeLevelCardReader.OnScanCardEvent += FillTreeLevelUserName_OnScanCardEvent;
					//m_parent.m_global.ThreeLevelCardReader.Open();
                    //m_parent.m_global.m_card_reader.Close();

                }
				catch (Exception ex)
				{
					m_parent.m_global.m_log_presenter.Log("三级权限管理窗口初始化失败：" + ex.Message);
				}
			}
		}

		private void Window_Close(object sender, CancelEventArgs e)
		{
			e.Cancel = true;

			////m_parent.m_global.m_card_reader.Open();
			//m_parent.RegisterMainWindowScanCardEvent();
   //         //UnregisterThreeLevelPageScanCardEvent();
   //         m_parent.m_global.ThreeLevelCardReader.Close();

            this.Hide();
		}

		public void UnregisterThreeLevelPageScanCardEvent()
		{
            //m_parent.m_global.ThreeLevelCardReader.OnScanCardEvent -= FillTreeLevelUserName_OnScanCardEvent;
        }

        // 确认按钮
        private void BtnConfirm_Click(object sender, RoutedEventArgs e)
		{
			// 管理员账户名为admin，密码为m_parent.m_global.m_strManagerPWD
			string strInputAdminName = textbox_AdminName.Text.Trim();
			string strInputAdminPWD = textbox_AdminPassword.Password.Trim();

			if (strInputAdminName == "admin" && strInputAdminPWD == m_parent.m_global.m_strManagerPWD)
			{
				m_bAdminConfirmed = true;

				SetUIEnableStatus(m_bAdminConfirmed);

				// 清除gridview_Engineer
				gridview_Engineer.Rows.Clear();

				showUsersOnGridView();

				MessageBox.Show("管理员账户验证成功！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
			}
			else
			{
				MessageBox.Show("管理员账户名或密码错误！", "提示", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		// 按钮点击事件：添加工程师
		private void BtnAddUser_Click(object sender, RoutedEventArgs e)
		{
			string strInputName = textbox_EngineerName.Text.Trim();
			string strInputPWD = textbox_EngineerPassword.Text.Trim();
			string strSelectedIdentity = comboBox_Identity.Text.Trim();

			// 判断有效性
			if (strInputName == "" || strInputPWD == "")
			{
				MessageBox.Show("账户名或密码不能为空！", "提示", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}

			if (String.IsNullOrEmpty(strSelectedIdentity))
			{
				MessageBox.Show("请选择要添加账户的身份！", "提示", MessageBoxButton.OK, MessageBoxImage.Error);

				return;
			}

			// 判断账户名在m_parent.m_global.m_dict_engineers_and_passwords中是否已经存在
			if (m_parent.m_global.FupanUsers.Where(user => user.UserName == strInputName).Count() > 0)
			{
				MessageBox.Show("账户名已经存在！", "提示", MessageBoxButton.OK, MessageBoxImage.Error);

				return;
			}

			// 添加工程师
			switch (comboBox_Identity.SelectedIndex)
			{
			case 0:
				m_parent.m_global.FupanUsers.Add(new FupanUser { UserName = strInputName, Password = strInputPWD, UserType = UserType.工程师 });
				break;

			case 1:
				m_parent.m_global.FupanUsers.Add(new FupanUser { UserName = strInputName, Password = strInputPWD, UserType = UserType.技术员 });
				break;

			case 2:
				m_parent.m_global.FupanUsers.Add(new FupanUser { UserName = strInputName, Password = strInputPWD, UserType = UserType.操作员 });
				break;

			default:
				MessageBox.Show("选择账户身份时出现错误！", "提示", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}

			// 保存到配置文件
			m_parent.m_global.SaveUserData("user.ini");

			// 将工程师账户名和密码显示到gridview_Engineer
			gridview_Engineer.Rows.Add(strInputName, strInputPWD, strSelectedIdentity);

			// 选中最后一行
			gridview_Engineer.Rows[gridview_Engineer.Rows.Count - 1].Selected = true;
		}

		// 按钮点击事件：移除工程师
		private void BtnRemoveUser_Click(object sender, RoutedEventArgs e)
		{
			string strInputName = textbox_EngineerName.Text.Trim();

			// 判断有效性
			if (strInputName == "")
			{
				MessageBox.Show("账户名不能为空！", "提示", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}

			if (m_parent.m_global.FupanUsers.Where(user => user.UserName == strInputName).Count() == 0)
			{
				MessageBox.Show("账户名不存在！", "提示", MessageBoxButton.OK, MessageBoxImage.Error);

				return;
			}

		 m_parent.m_global.FupanUsers.RemoveAll(user => user.UserName == strInputName);

		 // 保存到配置文件
		 m_parent.m_global.SaveUserData("user.ini");

			// 清除gridview_Engineer
			gridview_Engineer.Rows.Clear();

			showUsersOnGridView();

			// 清空textbox_EngineerName和textbox_EngineerPassword
			textbox_EngineerName.Text = "";
			textbox_EngineerPassword.Text = "";
		}

		// 工程师列表点击事件
		private void gridview_User_CellMouseClick(object sender, System.Windows.Forms.DataGridViewCellMouseEventArgs e)
		{
			// 获取选中的行，将账户名和密码显示到textbox_EngineerName和textbox_EngineerPassword
			if (e.RowIndex >= 0)
			{
				textbox_EngineerName.Text = gridview_Engineer.Rows[e.RowIndex].Cells[0].Value.ToString();
				textbox_EngineerPassword.Text = gridview_Engineer.Rows[e.RowIndex].Cells[1].Value.ToString();
			}
		}

		// 功能：根据管理员是否已经确认，设置界面的可用性
		public void SetUIEnableStatus(bool bAdminConfirmed)
		{
			if (bAdminConfirmed)
			{
				BtnAddEngineer.IsEnabled = true;
				BtnRemoveEngineer.IsEnabled = true;
				textbox_EngineerName.IsEnabled = true;
				textbox_EngineerPassword.IsEnabled = true;

				gridview_Engineer.Enabled = true;
			}
			else
			{
				BtnAddEngineer.IsEnabled = false;
				BtnRemoveEngineer.IsEnabled = false;
				textbox_EngineerName.IsEnabled = false;
				textbox_EngineerPassword.IsEnabled = false;

				gridview_Engineer.Enabled = false;
			}
		}

		private void showUsersOnGridView()
		{
			foreach (var user in m_parent.m_global.FupanUsers)
			{
				gridview_Engineer.Rows.Add(user.UserName, user.Password, user.UserType.ToString());
			}
		}

        public void FillTreeLevelUserName_OnScanCardEvent(ScanCard.State state, string IC)
        {
            var isVisible = false;

            try
            {
                //Application.Current.Dispatcher.Invoke(() =>
                //{
                //    isVisible = this.IsVisible;
                //});

                //if (!isVisible)
                //{
                //    return;
                //}

                if (state == ScanCard.State.Online)
                {
                    if (m_parent.m_global.ThreeLevelUserPermission == null)
                    {
                        return;
                    }

                    this.Dispatcher.BeginInvoke(new Action(() => {
                        string Result = string.Empty;

                        m_parent.m_global.m_log_presenter.Log(string.Format("在三级权限界面触发读卡器，内容：{0}", IC));

                        if (m_parent.m_global.m_strSiteCity == "苏州")
                            MFlex.GetUserAccountByIC(MFlex.MesAddress.SZ, IC, m_parent.m_global.m_strCurrentPC_AnyAVIName, out Result);
                        else if (m_parent.m_global.m_strSiteCity == "盐城")
                            MFlex.GetUserAccountByIC(MFlex.MesAddress.YC, IC, m_parent.m_global.m_strCurrentPC_AnyAVIName, out Result);

                        if (!string.IsNullOrEmpty(Result))
                        {
                            m_parent.m_global.ThreeLevelUserPermission.textbox_EngineerName.Text = Result;
                            // 记录登录信息和时间
                            m_parent.m_global.m_log_presenter.Log("操作员 " + m_parent.m_global.m_strCurrentOperatorID + " 于 " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " 在三级权限刷入ID " + m_parent.m_global.m_strCurrentPC_AnyAVIName);
                        }
                    }));
                }
                else
                {
                    if (false == m_parent.m_global.m_bIsInTestingMode)
                    {
                        this.Dispatcher.BeginInvoke(new Action(() => {
                            m_parent.m_global.ThreeLevelUserPermission.textbox_EngineerName.Text = "";
                        }));
                    }
                }
            }
            catch (Exception ex)
            {
            }

        }

    }
}