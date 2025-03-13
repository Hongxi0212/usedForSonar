using AutoScanFQCTest.DataModels;
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
   public partial class PasswordValidation: Window
   {
	  private MainWindow m_parent;

	  public bool m_bResultOK = false;
	  private string requestOperation;

		public PasswordValidation(MainWindow parent, string calledFunc)
		{
			InitializeComponent();

			m_parent = parent;
			requestOperation = calledFunc;

			//username_Info.Text = "请输入用户名：          ";
			//password_Info.Text = "请输入管理密码：";

			username_Input.Focus();
			//textbox_Password.CaretIndex = textbox_Password.Text.Length;

			switch (m_parent.m_global.CurrentLoginType)
			{
			case UserType.工程师:
				break;

			case UserType.技术员:
				if (requestOperation == "一般设置")
				{
					MessageBox.Show("用户无权限！");
					return;
				}
				break;

			case UserType.操作员:
				MessageBox.Show("用户无权限！");
				return;

			default:
				MessageBox.Show("不存在该用户身份！");
				return;
			}

			m_bResultOK = true;

			this.Close();
		}

	  // 按钮：确定
	  private void BtnOK_Click(object sender, RoutedEventArgs e)
	  {
		 var foundUser = m_parent.m_global.FupanUsers.Where(user => user.UserName == username_Input.Text).ToList();

		 if (foundUser.Count <= 0)
		 {
			MessageBox.Show("用户不存在！");
			return;
		 }

		 if (foundUser.First().Password == textbox_Password.Password)
		 {
			switch (foundUser.First().UserType)
			{
			case UserType.工程师:
			   break;

			case UserType.技术员:
			   if (requestOperation == "一般设置")
			   {
				  MessageBox.Show("用户无权限！");
				  return;
			   }
			   break;

			case UserType.操作员:
			   MessageBox.Show("用户无权限！");
			   return;

			default:
			   MessageBox.Show("不存在该用户身份！");
			   return;
			}
		 }
		 else
		 {
			MessageBox.Show("密码错误!");
			return;
		 }

		 m_bResultOK = true;

		 this.Close();
	  }

	  // 按钮：取消
	  private void BtnCancel_Click(object sender, RoutedEventArgs e)
	  {
		 this.Close();
	  }
   }
}