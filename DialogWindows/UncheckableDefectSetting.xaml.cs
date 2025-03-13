using AutoScanFQCTest.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
using System.Xml.Linq;

namespace AutoScanFQCTest.DialogWindows
{
    public partial class UncheckableDefectSetting : Window
    {
        MainWindow m_parent;

        private CheckBox[] m_checkboxs;
        private CheckBox[] m_passboxes;

        private Button[] m_buttons;

        public UncheckableDefectSetting(MainWindow parent)
        {
            InitializeComponent();

            m_parent = parent;

            GenerateUncheckableItemsGridContainer(grid_UncheckableItems, m_parent.m_global.m_uncheckable_defect_types.Count);
            GenerateUncheckableItemsGridContainer(grid_UncheckablePasses, m_parent.m_global.m_uncheckable_pass_types.Count, true);

            // 设置复选框的状态
            if (m_parent.m_global.m_bIgnoreOtherDefectsIfOneUnrecheckableItemExists)
            {
                CheckBox_IgnoreOtherDefectsIfOneUnrecheckableItemExists.IsChecked = true;
            }
            else
            {
                CheckBox_IgnoreOtherDefectsIfOneUnrecheckableItemExists.IsChecked = false;
            }
        }

        // 功能：在grid_container生成N行的文本框textblock
        public void GenerateUncheckableItemsGridContainer(Grid grid_container, int nRows, bool uncheckablePass = false)
        {
            Grid grid = grid_container;

            grid.Children.Clear();

            // 清除之前的定义
            grid.ColumnDefinitions.Clear();
            grid.RowDefinitions.Clear();

            //if (m_parent.m_global.m_strProductType == "dock")
            {
                // 为Grid定义需要的行数
                for (int rowIndex = 0; rowIndex < nRows; rowIndex++)
                {
                    grid.RowDefinitions.Add(new RowDefinition()
                    {
                        Height = GridLength.Auto // 设置行高为自动，根据内容调整
                    });
                }
            }

            if (uncheckablePass)
			{
				m_passboxes = new CheckBox[nRows];
			}
            else
			{
				m_checkboxs = new CheckBox[nRows];
			}

            // 创建并添加多个文本框
            for (int i = 0; i < nRows; i++)
            {
                if (uncheckablePass)
				{
					// 创建按钮实例
					m_passboxes[i] = new CheckBox();
					m_passboxes[i].FontSize = 16;
					m_passboxes[i].Height = 32;

					//m_textblocks[i].Background = new SolidColorBrush(Colors.Green);

					// 将按钮添加到 Grid，并设置其行和列
					grid.Children.Add(m_passboxes[i]);
					Grid.SetRow(m_passboxes[i], i);
					Grid.SetColumn(m_passboxes[i], 0);

					m_passboxes[i].Content = m_parent.m_global.m_uncheckable_pass_types[i];
					m_passboxes[i].VerticalAlignment = VerticalAlignment.Center;
					m_passboxes[i].IsChecked = m_parent.m_global.m_uncheckable_pass_enable_flags[i];
                }
                else
				{
					// 创建按钮实例
					m_checkboxs[i] = new CheckBox();
					m_checkboxs[i].FontSize = 16;
					m_checkboxs[i].Height = 32;

					//m_textblocks[i].Background = new SolidColorBrush(Colors.Green);

					// 将按钮添加到 Grid，并设置其行和列
					grid.Children.Add(m_checkboxs[i]);
					Grid.SetRow(m_checkboxs[i], i);
					Grid.SetColumn(m_checkboxs[i], 0);

					m_checkboxs[i].Content = m_parent.m_global.m_uncheckable_defect_types[i];
					m_checkboxs[i].VerticalContentAlignment = VerticalAlignment.Center;
					m_checkboxs[i].IsChecked = m_parent.m_global.m_uncheckable_defect_enable_flags[i];
				}
            }

            m_buttons = new Button[nRows];

            // 创建并添加多个文本框
            for (int i = 0; i < nRows; i++)
            {
                // 创建按钮实例
                m_buttons[i] = new Button();
                m_buttons[i].FontSize = 16;
                m_buttons[i].Height = 32;
                m_buttons[i].Width = 80;
                m_buttons[i].Content = "删除";

                //m_textblocks[i].Background = new SolidColorBrush(Colors.Green);

                // 将按钮添加到 Grid，并设置其行和列
                grid.Children.Add(m_buttons[i]);
                Grid.SetRow(m_buttons[i], i);
                Grid.SetColumn(m_buttons[i], 1);

                m_buttons[i].Click += BtnDeleteIndexClick;
            }
        }

        // 复选框：只要有一个不可复判项，该产品其它缺陷都自动不可复判
        private void CheckBox_IgnoreOtherDefectsIfOneUnrecheckableItemExists_Checked(object sender, RoutedEventArgs e)
        {
        }

        // 复选框：只要有一个不可复判项，该产品其它缺陷都自动不可复判
        private void CheckBox_IgnoreOtherDefectsIfOneUnrecheckableItemExists_Unchecked(object sender, RoutedEventArgs e)
        {
        }

        // 按钮：确定
        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < m_checkboxs.Length; i++)
            {
                if (m_checkboxs[i] != null)
                {
					m_parent.m_global.m_uncheckable_defect_enable_flags[i] = m_checkboxs[i].IsChecked.Value;
				}
			}

            for (int i = 0; i < m_passboxes.Length; i++)
            {
                if(m_passboxes[i] != null)
                {
					m_parent.m_global.m_uncheckable_pass_enable_flags[i] = m_passboxes[i].IsChecked.Value;
				}
			}

            m_parent.m_global.m_bIgnoreOtherDefectsIfOneUnrecheckableItemExists = CheckBox_IgnoreOtherDefectsIfOneUnrecheckableItemExists.IsChecked.Value;

            m_parent.m_global.SaveConfigData("config.ini");

            // 弹出提示框
            MessageBox.Show("保存成功！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);

            this.Close();
        }

        // 按钮：取消
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // 菜单项：添加不可复判项
        private void MenuItem_Add_Click(object sender, RoutedEventArgs e)
        {
            AddUncheckableDefect addUncheckableDefect = new AddUncheckableDefect(m_parent, "请输入缺陷名称：");

            addUncheckableDefect.ShowDialog();

            if (true == addUncheckableDefect.m_bResultOK)
            {
                m_parent.m_global.m_uncheckable_defect_types.Add(addUncheckableDefect.m_strUncheckableDefectName);
                m_parent.m_global.m_uncheckable_defect_enable_flags.Add(false);

                // 保存到配置文件
                m_parent.m_global.SaveConfigData("config.ini");

                // 弹出提示框
                MessageBox.Show("保存成功！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);

                // 刷新容器
                GenerateUncheckableItemsGridContainer(grid_UncheckableItems, m_parent.m_global.m_uncheckable_defect_types.Count);
            }
        }

        // 按钮点击事件：删除一个不可复判项被点击
        private void BtnDeleteIndexClick(object sender, RoutedEventArgs e)
        {
            int index = Array.IndexOf(m_buttons, sender);

            if (MessageBox.Show(string.Format("请确认是否删除不可复判项{0}？", index + 1),
                "提示", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                // 删除不可复判项
                if (true)
                {
                    m_parent.m_global.m_uncheckable_defect_types.RemoveAt(index);
                    m_parent.m_global.m_uncheckable_defect_enable_flags.RemoveAt(index);

                    // 保存到配置文件
                    m_parent.m_global.SaveConfigData("config.ini");

                    // 弹出提示框
                    MessageBox.Show("删除成功！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);

                    // 刷新容器
                    GenerateUncheckableItemsGridContainer(grid_UncheckableItems, m_parent.m_global.m_uncheckable_defect_types.Count);
                }
            }
        }
    }
}
