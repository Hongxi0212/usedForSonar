using AutoScanFQCTest.Canvas;
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
    public partial class Home : Window
    {
        public MainWindow m_parent;

        public CameraWindow[] m_camera_windows = new CameraWindow[10];                            // 相机窗口

        public CameraWindow[] m_derived_camera_windows = new CameraWindow[10];              // 衍生相机窗口

        private bool m_bIsWindowInited = false;

        public Home(MainWindow parent)
        {
            InitializeComponent();

            m_parent = parent;

            // 获取内容元素
            var contentElement = this.Content as FrameworkElement;
            if (contentElement != null)
            {
                // 设置内容元素的事件
                contentElement.Loaded += Window_Loaded;
                contentElement.SizeChanged += Window_SizeChanged;
            }
        }

        // 窗口加载
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (null == m_parent)
                return;
        }

        // 窗口大小改变
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (null == m_parent)
                return;

            if (false == m_bIsWindowInited)
            {
                m_bIsWindowInited = true;

                // 初始化相机窗口
                InitCameraWindow();
            }
        }

        // 获取窗口内容
        public UIElement GetWindowContent()
        {
            return this.Content as UIElement;
        }

        // 显示相机窗口
        private void InitCameraWindow()
        {
            // 相机窗口
            if (true)
            {
                int nNumOfCameras = m_parent.m_global.m_list_image_canvases.Count;

                for (int nCameraIdx = 0; nCameraIdx < nNumOfCameras; nCameraIdx++)
                {
                    // 单个相机窗口
                    m_camera_windows[nCameraIdx] = new CameraWindow(m_parent, nCameraIdx)
                    {
                        Owner = m_parent,
                        WindowStartupLocation = WindowStartupLocation.Manual,
                        ShowInTaskbar = false
                    };

                    // 将窗口初始位置设置在ContentGrid内
                    var contentGridPosition = ContentGrid.PointToScreen(new System.Windows.Point(0, 0));
                    m_camera_windows[nCameraIdx].Left = m_parent.m_global.m_list_image_canvases[nCameraIdx].m_lefttop_on_UI.x;
                    m_camera_windows[nCameraIdx].Top = m_parent.m_global.m_list_image_canvases[nCameraIdx].m_lefttop_on_UI.y;

                    // 设置窗口大小
                    m_camera_windows[nCameraIdx].Width = m_parent.m_global.m_list_image_canvases[nCameraIdx].m_size.Width;
                    m_camera_windows[nCameraIdx].Height = m_parent.m_global.m_list_image_canvases[nCameraIdx].m_size.Height;

                    // 设置窗口标题
                    m_camera_windows[nCameraIdx].Title = string.Format("Camera {0}", nCameraIdx + 1);

                    // 监听窗口位置变化事件
                    m_camera_windows[nCameraIdx].LocationChanged += CameraWindow_LocationChanged;
                    m_camera_windows[nCameraIdx].SizeChanged += CameraWindow_SizeChanged;

                    m_camera_windows[nCameraIdx].Show();
                }
            }

            // 衍生相机窗口
            if (true)
            {
                for (int nCameraIdx = 0; nCameraIdx < m_parent.m_global.m_list_derived_canvases.Count; nCameraIdx++)
                {
                    DerivedCanvas derivedCanvas = m_parent.m_global.m_list_derived_canvases[nCameraIdx];

                    // 单个衍生相机窗口
                    m_derived_camera_windows[nCameraIdx] = new CameraWindow(m_parent, nCameraIdx + (int)scene_type.scene_ROI_canvas1)
                    {
                        Owner = m_parent,
                        WindowStartupLocation = WindowStartupLocation.Manual,
                        ShowInTaskbar = false
                    };

                    // 将窗口初始位置设置在ContentGrid内
                    var contentGridPosition = ContentGrid.PointToScreen(new System.Windows.Point(0, 0));
                    m_derived_camera_windows[nCameraIdx].Left = derivedCanvas.m_lefttop_on_UI.x;
                    m_derived_camera_windows[nCameraIdx].Top = derivedCanvas.m_lefttop_on_UI.y;

                    // 设置窗口大小
                    m_derived_camera_windows[nCameraIdx].Width = derivedCanvas.m_size.Width;
                    m_derived_camera_windows[nCameraIdx].Height = derivedCanvas.m_size.Height;

                    // 设置窗口标题
                    m_derived_camera_windows[nCameraIdx].Title = string.Format("Derived Camera {0}", nCameraIdx + 1);

                    // 监听窗口位置变化事件
                    m_derived_camera_windows[nCameraIdx].LocationChanged += DerivedCameraWindow_LocationChanged;
                    m_derived_camera_windows[nCameraIdx].SizeChanged += DerivedCameraWindow_SizeChanged;

                    m_derived_camera_windows[nCameraIdx].Show();
                }
            }
        }

        // 相机窗口位置变化事件
        private void CameraWindow_LocationChanged(object sender, EventArgs e)
        {
            CameraWindow source = sender as CameraWindow;

            // 获取ContentGrid的屏幕位置和大小
            var contentGridPosition = ContentGrid.PointToScreen(new System.Windows.Point(0, 0));
            var contentGridWidth = ContentGrid.ActualWidth;
            var contentGridHeight = ContentGrid.ActualHeight;

            if (m_camera_windows[source.m_nCameraIndex] == null)
                return;
            
            // 获取窗口的大小
            var windowWidth = m_camera_windows[source.m_nCameraIndex].ActualWidth;
            var windowHeight = m_camera_windows[source.m_nCameraIndex].ActualHeight;

            // 计算窗口新的位置，确保其在ContentGrid内
            double newLeft = m_camera_windows[source.m_nCameraIndex].Left;
            double newTop = m_camera_windows[source.m_nCameraIndex].Top;

            if (newLeft < contentGridPosition.X - 6)
                newLeft = contentGridPosition.X - 6;

            if (newTop < contentGridPosition.Y)
                newTop = contentGridPosition.Y;

            if (newLeft + windowWidth > contentGridPosition.X + contentGridWidth)
                newLeft = contentGridPosition.X + contentGridWidth - windowWidth;

            if (newTop + windowHeight > contentGridPosition.Y + contentGridHeight)
                newTop = contentGridPosition.Y + contentGridHeight - windowHeight;

            // 更新窗口位置
            m_camera_windows[source.m_nCameraIndex].Left = newLeft;
            m_camera_windows[source.m_nCameraIndex].Top = newTop;

            Canvas.Point2i lefttop = new Canvas.Point2i((int)m_camera_windows[source.m_nCameraIndex].Left, (int)m_camera_windows[source.m_nCameraIndex].Top);

            m_parent.m_global.m_list_image_canvases[source.m_nCameraIndex].m_lefttop_on_UI = lefttop;
        }

        // 相机窗口大小变化事件
        private void CameraWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            CameraWindow source = sender as CameraWindow;

            // 获取ContentGrid的屏幕位置和大小
            var contentGridPosition = ContentGrid.PointToScreen(new System.Windows.Point(0, 0));
            var contentGridWidth = ContentGrid.ActualWidth;
            var contentGridHeight = ContentGrid.ActualHeight;

            if (m_camera_windows[source.m_nCameraIndex] == null)
                return;

            // 获取窗口的大小
            var windowWidth = m_camera_windows[source.m_nCameraIndex].ActualWidth;
            var windowHeight = m_camera_windows[source.m_nCameraIndex].ActualHeight;

            Size size = new Size(windowWidth, windowHeight);
            Canvas.Point2i lefttop = new Canvas.Point2i((int)m_camera_windows[source.m_nCameraIndex].Left, (int)m_camera_windows[source.m_nCameraIndex].Top);

            m_parent.m_global.m_list_image_canvases[source.m_nCameraIndex].m_size = new System.Drawing.Size((int)size.Width, (int)size.Height);
            m_parent.m_global.m_list_image_canvases[source.m_nCameraIndex].m_lefttop_on_UI = lefttop;
        }

        // 衍生相机窗口位置变化事件
        private void DerivedCameraWindow_LocationChanged(object sender, EventArgs e)
        {
            CameraWindow source = sender as CameraWindow;

            // 获取ContentGrid的屏幕位置和大小
            var contentGridPosition = ContentGrid.PointToScreen(new System.Windows.Point(0, 0));
            var contentGridWidth = ContentGrid.ActualWidth;
            var contentGridHeight = ContentGrid.ActualHeight;

            int nCameraIdx = source.m_nCameraIndex - (int)scene_type.scene_ROI_canvas1;
            if (m_derived_camera_windows[nCameraIdx] == null)
                return;

            // 获取窗口的大小
            var windowWidth = m_derived_camera_windows[nCameraIdx].ActualWidth;
            var windowHeight = m_derived_camera_windows[nCameraIdx].ActualHeight;

            // 计算窗口新的位置，确保其在ContentGrid内
            double newLeft = m_derived_camera_windows[nCameraIdx].Left;
            double newTop = m_derived_camera_windows[nCameraIdx].Top;

            if (newLeft < contentGridPosition.X)
                newLeft = contentGridPosition.X;

            if (newTop < contentGridPosition.Y)
                newTop = contentGridPosition.Y;

            if (newLeft + windowWidth > contentGridPosition.X + contentGridWidth)
                newLeft = contentGridPosition.X + contentGridWidth - windowWidth;

            if (newTop + windowHeight > contentGridPosition.Y + contentGridHeight)
                newTop = contentGridPosition.Y + contentGridHeight - windowHeight;

            // 更新窗口位置
            m_derived_camera_windows[nCameraIdx].Left = newLeft;
            m_derived_camera_windows[nCameraIdx].Top = newTop;

            Canvas.Point2i lefttop = new Canvas.Point2i((int)m_derived_camera_windows[nCameraIdx].Left, (int)m_derived_camera_windows[nCameraIdx].Top);

            m_parent.m_global.m_list_derived_canvases[nCameraIdx].m_lefttop_on_UI = lefttop;
        }

        // 衍生相机窗口大小变化事件
        private void DerivedCameraWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            CameraWindow source = sender as CameraWindow;

            // 获取ContentGrid的屏幕位置和大小
            var contentGridPosition = ContentGrid.PointToScreen(new System.Windows.Point(0, 0));
            var contentGridWidth = ContentGrid.ActualWidth;
            var contentGridHeight = ContentGrid.ActualHeight;

            int nCameraIdx = source.m_nCameraIndex - (int)scene_type.scene_ROI_canvas1;

            if (m_derived_camera_windows[nCameraIdx] == null)
                return;

            // 获取窗口的大小
            var windowWidth = m_derived_camera_windows[nCameraIdx].ActualWidth;
            var windowHeight = m_derived_camera_windows[nCameraIdx].ActualHeight;

            Size size = new Size(windowWidth, windowHeight);
            Canvas.Point2i lefttop = new Canvas.Point2i((int)m_derived_camera_windows[nCameraIdx].Left, (int)m_derived_camera_windows[nCameraIdx].Top);

            m_parent.m_global.m_list_derived_canvases[nCameraIdx].m_size = new System.Drawing.Size((int)size.Width, (int)size.Height);
            m_parent.m_global.m_list_derived_canvases[nCameraIdx].m_lefttop_on_UI = lefttop;
        }
    }
}
