using AutoScanFQCTest.Canvas;
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
using System.Windows.Threading;

namespace AutoScanFQCTest.DialogWindows
{
    public partial class AVIMachineConnectStatus : Window
    {
        public MainWindow m_parent;

        private DispatcherTimer m_Timer;
        private int m_TickCount;

        public AVIMachineConnectStatus(MainWindow parent)
        {
            InitializeComponent();

            m_parent = parent;

            // 为 Grid 定义m_dict_machine_names_and_IPs数量一样的行
            GenerateMachineInfoControls(grid_MachineInfo, m_parent.m_global.m_dict_machine_names_and_IPs.Count);

            m_Timer = new DispatcherTimer();
            m_Timer.Interval = TimeSpan.FromMilliseconds(1000); // 设置定时间隔为1秒
            m_Timer.Tick += Timer_Tick;

            m_Timer.Start();

            this.Closing += AVIMachineConnectStatus_Closing;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            m_TickCount++;

            // 为 Grid 定义m_dict_machine_names_and_IPs数量一样的行
            Application.Current.Dispatcher.Invoke(() =>
            {
                GenerateMachineInfoControls(grid_MachineInfo, m_parent.m_global.m_dict_machine_names_and_IPs.Count);
            });
        }

        private void AVIMachineConnectStatus_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            m_Timer.Stop();
        }

        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // 功能：在grid_container生成N行的文本框textblock
        public void GenerateMachineInfoControls(Grid grid_container, int nRows)
        {
            Grid grid = grid_container;

            grid.Children.Clear();

            // 清除之前的定义
            grid.ColumnDefinitions.Clear();
            grid.RowDefinitions.Clear();

            for (int rowIndex = 0; rowIndex < nRows; rowIndex++)
            {
                grid.RowDefinitions.Add(new RowDefinition()
                {
                    Height = new GridLength(28)
                });

                // 加入四列column，宽度为100个像素
                for (int i = 0; i < 5; i++)
                {
                    if (i == 3)
                    {
                        grid.ColumnDefinitions.Add(new ColumnDefinition()
                        {
                            Width = new GridLength(350)
                        });
                    }
                    else if (i == 4)
                    {
                        grid.ColumnDefinitions.Add(new ColumnDefinition()
                        {
                            Width = new GridLength(100)
                        });
                    }
                    else
                    {
                        grid.ColumnDefinitions.Add(new ColumnDefinition()
                        {
                            Width = new GridLength(200)
                        });
                    }
                }

                // 加入两列文本框，1列显示机器名，1列显示IP
               TextBlock tb_machine_name = new TextBlock()
               {
                   Text = m_parent.m_global.m_dict_machine_names_and_IPs.Keys.ElementAt(rowIndex),
                   VerticalAlignment = VerticalAlignment.Center,
                   HorizontalAlignment = HorizontalAlignment.Left,
                   FontSize = 15,
                   FontWeight = FontWeights.Bold
               };

                TextBlock tb_machine_IP = new TextBlock()
                {
                    Text = m_parent.m_global.m_dict_machine_names_and_IPs.Values.ElementAt(rowIndex),
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    FontSize = 15,
                    FontWeight = FontWeights.Bold
                };

                grid.Children.Add(tb_machine_name);
                Grid.SetRow(tb_machine_name, rowIndex);
                Grid.SetColumn(tb_machine_name, 0);

                grid.Children.Add(tb_machine_IP);
                Grid.SetRow(tb_machine_IP, rowIndex);
                Grid.SetColumn(tb_machine_IP, 1);

                // 如果m_parent.m_global.m_dict_machine_names_and_connect_status[strMachineName]为0，表示连接正常，第三列显示绿色，否则显示红色
                if (m_parent.m_global.m_dict_machine_names_and_connect_status[m_parent.m_global.m_dict_machine_names_and_IPs.Keys.ElementAt(rowIndex)] == 0)
                {
                    TextBlock tb_status = new TextBlock()
                    {
                        Text = "连接正常",
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        FontSize = 15,
                        FontWeight = FontWeights.Bold,
                        Foreground = Brushes.White,
                        Background = Brushes.Green
                    };

                    grid.Children.Add(tb_status);
                    Grid.SetRow(tb_status, rowIndex);
                    Grid.SetColumn(tb_status, 2);
                }
                else
                {
                    TextBlock tb_status = new TextBlock()
                    {
                        Text = "连接异常",
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        FontSize = 15,
                        FontWeight = FontWeights.Bold,
                        Foreground = Brushes.Black,
                        Background = Brushes.Red
                    };

                    grid.Children.Add(tb_status);
                    Grid.SetRow(tb_status, rowIndex);
                    Grid.SetColumn(tb_status, 2);

                    // 第四列解释连接失败的原因
                    switch (m_parent.m_global.m_dict_machine_names_and_connect_status[m_parent.m_global.m_dict_machine_names_and_IPs.Keys.ElementAt(rowIndex)])
                    {
                        case 1:
                            TextBlock tb_reason = new TextBlock()
                            {
                                Text = "ping通但是共享目录不正常，请检查视觉电脑的网络共享设置或目录情况",
                                VerticalAlignment = VerticalAlignment.Center,
                                HorizontalAlignment = HorizontalAlignment.Center,
                                FontSize = 15,
                                FontWeight = FontWeights.Bold
                            };

                            grid.Children.Add(tb_reason);
                            Grid.SetRow(tb_reason, rowIndex);
                            Grid.SetColumn(tb_reason, 3);
                            break;

                        case 2:
                            TextBlock tb_reason2 = new TextBlock()
                            {
                                Text = "ping不通，请检查视觉电脑的网络状态",
                                VerticalAlignment = VerticalAlignment.Center,
                                HorizontalAlignment = HorizontalAlignment.Center,
                                FontSize = 15,
                                FontWeight = FontWeights.Bold
                            };

                            grid.Children.Add(tb_reason2);
                            Grid.SetRow(tb_reason2, rowIndex);
                            Grid.SetColumn(tb_reason2, 3);
                            break;

                        case 3:
                            TextBlock tb_reason3 = new TextBlock()
                            {
                                Text = "IP地址无效，请检查配置文件",
                                VerticalAlignment = VerticalAlignment.Center,
                                HorizontalAlignment = HorizontalAlignment.Center,
                                FontSize = 15,
                                FontWeight = FontWeights.Bold
                            };

                            grid.Children.Add(tb_reason3);
                            Grid.SetRow(tb_reason3, rowIndex);
                            Grid.SetColumn(tb_reason3, 3);
                            break;
                    }
                }

                // 在每一行的第五列加入一个按钮,文本为"停机"
                Button btn_stop_machine = new Button()
                {
                    Content = "停机",
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    FontSize = 15,
                    Padding = new Thickness(10, 5, 10, 5)
                };

                // 为按钮添加点击事件处理程序
                btn_stop_machine.Click += (sender, e) =>
                {
                    int idx = Grid.GetRow((Button)sender);

                    // 获取当前行对应的机器名和IP
                    string machineName = m_parent.m_global.m_dict_machine_names_and_IPs.Keys.ElementAt(idx);
                    string machineIP = m_parent.m_global.m_dict_machine_names_and_IPs.Values.ElementAt(idx);

                    // 调用停机函数,并传入机器名和IP作为参数
                    StopMachine(machineName, machineIP);
                };

                grid.Children.Add(btn_stop_machine);
                Grid.SetRow(btn_stop_machine, rowIndex);
                Grid.SetColumn(btn_stop_machine, 4);
            }
        }

        // 功能：停机函数
        public void StopMachine(string machineName, string machineIP)
        {
            // 弹出确认对话框
            MessageBoxResult result = MessageBox.Show(string.Format("是否停机 {0} ？", machineName), "提示", MessageBoxButton.OKCancel, MessageBoxImage.Question);

            if (result == MessageBoxResult.OK)
            {
                try
                {
                    // 构造远程机器上共享文件的完整路径
                    string remoteFilePath = string.Format("\\\\{0}\\stopmachine.txt", machineIP);

                    // 写入字符串 "true" 到远程文件
                    File.WriteAllText(remoteFilePath, "true");

                    m_parent.m_global.m_log_presenter.Log(string.Format("已成功向机器 {0} ({1}) 发送停机指令。", machineName, machineIP));

                    MessageBox.Show($"已成功向机器 {machineName} ({machineIP}) 发送停机指令。");
                }
                catch (Exception ex)
                {
                    m_parent.m_global.m_log_presenter.Log(string.Format("向机器 {0} ({1}) 发送停机指令时出现错误: {2}", machineName, machineIP, ex.Message));

                    MessageBox.Show($"向机器 {machineName} ({machineIP}) 发送停机指令时出现错误:\n{ex.Message}");
                }

                m_parent.m_global.m_log_presenter.Log(string.Format("停机 {0}", machineName));
            }
        }
    }
}
