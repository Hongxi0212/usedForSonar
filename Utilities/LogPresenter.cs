using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace AutoScanFQCTest.Utilities
{
    // 日志呈现类
    public class LogPresenter
    {
        private TextBox m_textbox_object;

        private TextBox m_error_textbox_object;

        private bool m_bShowTime = true;

        private MainWindow m_parent;

        private object m_lock = new object();

        private List<string> m_log_buffer = new List<string>();
        private List<string> m_error_log_buffer = new List<string>();

        public LogPresenter(MainWindow window, bool bShowTime = true)
        {
            m_parent = window;

            m_bShowTime = bShowTime;
        }

        public void set_textbox_object(TextBox textBox, TextBox errorTextBox)
        {
            m_textbox_object = textBox;
            m_error_textbox_object = errorTextBox;
        }

        public void Log(string message)
        {
            if (m_textbox_object != null)
            {
                //string time = DateTime.Now.Month.ToString() + "-" + DateTime.Now.Day.ToString() + " " + DateTime.Now.ToLongTimeString().ToString();
                string time = DateTime.Now.ToLongTimeString().ToString();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    //m_textbox_object.Text += time + "   " + message + "\n";

                    // 如果超过5000行，清除前面的3000行
                    if (m_textbox_object.LineCount > 1000)
                    {
                        int index = m_textbox_object.GetCharacterIndexFromLineIndex(300);

                        m_textbox_object.Select(0, index);
                        m_textbox_object.SelectedText = "";
                    }

                    if (m_bShowTime)
                        m_textbox_object.AppendText(time + "   " + message + "\n");
                    else
                        m_textbox_object.AppendText(message + "\n");

                    m_textbox_object.ScrollToEnd();
				});

				// 保存日志到文件
                lock (m_lock)
                {
               SaveLogToFile(time, message);
                }
            }
        }

        public void LogError(string message)
        {
            if (m_error_textbox_object != null)
            {
                //string time = DateTime.Now.Month.ToString() + "-" + DateTime.Now.Day.ToString() + " " + DateTime.Now.ToLongTimeString().ToString();
                string time = DateTime.Now.ToLongTimeString().ToString();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    //m_error_textbox_object.Text += time + "   " + message + "\n";

                    // 如果超过5000行，清除前面的3000行
                    if (m_error_textbox_object.LineCount > 1000)
                    {
                        int index = m_error_textbox_object.GetCharacterIndexFromLineIndex(300);

                        m_error_textbox_object.Select(0, index);
                        m_error_textbox_object.SelectedText = "";
                    }

                    if (m_bShowTime)
                        m_error_textbox_object.AppendText(time + "   " + message + "\n");
                    else
                        m_error_textbox_object.AppendText(message + "\n");

                    m_error_textbox_object.ScrollToEnd();
                });

                // 保存日志到文件
                lock (m_lock)
                {
                    m_error_log_buffer.Add(time + "   " + message);

                    if (m_error_log_buffer.Count > 30)
                    {
                        // 判断当前目录下是否存在log文件夹，不存在则创建
                        string logPath = m_parent.m_global.m_strCurrentDirectory + "\\recheck_error_logs";
                        if (!System.IO.Directory.Exists(logPath))
                        {
                            System.IO.Directory.CreateDirectory(logPath);
                        }

                        // 创建以日期为名的文件，包含年_月_日
                        string logFile = logPath + "\\" + DateTime.Now.Year.ToString() + "_" + DateTime.Now.Month.ToString() + "_" + DateTime.Now.Day.ToString() + ".txt";

                        // 如果文件不存在，则创建文件，如果存在，则追加
                        if (!System.IO.File.Exists(logFile))
                        {
                            System.IO.FileStream fs = System.IO.File.Create(logFile);
                            fs.Close();
                        }

                        // 写入日志
                        System.IO.StreamWriter sw = new System.IO.StreamWriter(logFile, true);

                        foreach (string log in m_error_log_buffer)
                        {
                            sw.WriteLine(log);
                        }

                        sw.Close();

                        m_error_log_buffer.Clear();
                    }
                }
            }
        }

        public void Clear()
        {
            if (m_textbox_object != null)
            {
                string time = DateTime.Now.ToLongTimeString().ToString();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    m_textbox_object.Text = time + "   " + "清除信息" + "\n";
                    m_error_textbox_object.Text = time + "   " + "清除信息" + "\n";

                    m_textbox_object.ScrollToEnd();
                    m_error_textbox_object.ScrollToEnd();
                });
            }
        }

      public void SaveLogToFile(string time, string message, bool saveImmediately=false) {

			m_log_buffer.Add(time + "   " + message);

			if (saveImmediately || m_log_buffer.Count > 30) {
				// 判断当前目录下是否存在log文件夹，不存在则创建
				string logPath = m_parent.m_global.m_strCurrentDirectory + "\\recheck_logs";
				if (!System.IO.Directory.Exists(logPath)) {
					System.IO.Directory.CreateDirectory(logPath);
				}

				// 创建以日期为名的文件，包含年_月_日
				string logFile = logPath + "\\" + DateTime.Now.Year.ToString() + "_" + DateTime.Now.Month.ToString() + "_" + DateTime.Now.Day.ToString() + ".txt";

				// 如果文件不存在，则创建文件，如果存在，则追加
				if (!System.IO.File.Exists(logFile)) {
					System.IO.FileStream fs = System.IO.File.Create(logFile);
					fs.Close();
				}

				// 写入日志
				System.IO.StreamWriter sw = new System.IO.StreamWriter(logFile, true);

				foreach (string log in m_log_buffer) {
					sw.WriteLine(log);
				}

				sw.Close();

				m_log_buffer.Clear();
			}
		}
    }
}
