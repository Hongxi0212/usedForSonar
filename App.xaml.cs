using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace AutoScanFQCTest
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        private static Mutex _mutex;

        protected override void OnStartup(StartupEventArgs e)
        {
            bool createNew;
            _mutex = new Mutex(true, "AutoScanFQCTest", out createNew);
            if (!createNew)
            {
                MessageBox.Show("检测程序 AutoScanFQCTest.exe 已经有一个运行实例，请先关闭 (或在任务管理器中终止实例)，再重新打开。", "程序冲突提示");
                System.Environment.Exit(1);
            }

            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _mutex.ReleaseMutex();
            base.OnExit(e);
        }
    }
}
