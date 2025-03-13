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
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AutoScanFQCTest.DialogWindows
{
    public partial class CameraWindow : Window
    {
        public MainWindow m_parent;

        public int m_nCameraIndex = 0;                               // 相机索引

        public CameraCanvas m_camera_canvas;                 // 相机画布

        public bool m_bIsWindowInited = false;                 // 窗口是否已经初始化

        public CameraWindow()
        {
            InitializeComponent();
        }

        public CameraWindow(MainWindow parent, int nCameraIndex)
        {
            InitializeComponent();

            m_parent = parent;
            m_nCameraIndex = nCameraIndex;

            this.Loaded += Window_Loaded;
            this.SizeChanged += Window_SizeChanged;
        }

        // 窗口加载
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (null == m_parent)
                return;

            // 动态生成右键菜单
            if (m_nCameraIndex < (int)scene_type.scene_camera18)
            {
                ContextMenu menu = new ContextMenu();

                MenuItem get_current_ROI_data = new MenuItem();
                get_current_ROI_data.Header = "获取当前位置数据";
                get_current_ROI_data.Tag = "get_current_ROI_data";
                get_current_ROI_data.Click += new RoutedEventHandler(MenuClick_get_current_ROI_data);

                MenuItem select_ROI_as_observation_area1 = new MenuItem();
                select_ROI_as_observation_area1.Header = "框选细节观察区域1";
                select_ROI_as_observation_area1.Tag = "select_ROI_as_observation_area1";
                select_ROI_as_observation_area1.Click += new RoutedEventHandler(MenuClick_select_ROI_as_observation_area1);

                MenuItem select_ROI_as_observation_area2 = new MenuItem();
                select_ROI_as_observation_area2.Header = "框选细节观察区域2";
                select_ROI_as_observation_area2.Tag = "select_ROI_as_observation_area2";
                select_ROI_as_observation_area2.Click += new RoutedEventHandler(MenuClick_select_ROI_as_observation_area2);

                MenuItem select_ROI_as_observation_area3 = new MenuItem();
                select_ROI_as_observation_area3.Header = "框选细节观察区域3";
                select_ROI_as_observation_area3.Tag = "select_ROI_as_observation_area3";
                select_ROI_as_observation_area3.Click += new RoutedEventHandler(MenuClick_select_ROI_as_observation_area3);

                MenuItem select_ROI_as_observation_area4 = new MenuItem();
                select_ROI_as_observation_area4.Header = "框选细节观察区域4";
                select_ROI_as_observation_area4.Tag = "select_ROI_as_observation_area4";
                select_ROI_as_observation_area4.Click += new RoutedEventHandler(MenuClick_select_ROI_as_observation_area4);

                menu.Items.Add(get_current_ROI_data);
                menu.Items.Add(select_ROI_as_observation_area1);
                menu.Items.Add(select_ROI_as_observation_area2);
                menu.Items.Add(select_ROI_as_observation_area3);
                menu.Items.Add(select_ROI_as_observation_area4);

                this.ctrl_ImageCanvas.ContextMenu = menu;

                this.ctrl_ImageCanvas.ContextMenu.Opened += ContextMenu_Opened;
                this.ctrl_ImageCanvas.ContextMenu.Closed += ContextMenu_Closed;
            }
        }

        // 窗口大小改变
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (null == m_parent)
                return;

            // 动态调整画布大小，以适应不同分辨率的显示器
            if (true)
            {
                int nWidth = (int)(grid_MotherContainer.ActualWidth);
                if (nWidth % 4 != 0)
                    grid_ImageCanvas.Width = (int)(nWidth / 4) * 4;
                else
                    grid_ImageCanvas.Width = nWidth;

                grid_ImageCanvas.UpdateLayout();

                if (null == m_camera_canvas)
                {
                    m_camera_canvas = new CameraCanvas(m_parent, ctrl_ImageCanvas, (int)grid_ImageCanvas.ActualWidth, (int)grid_ImageCanvas.ActualHeight, scene_type.scene_camera1 + m_nCameraIndex, false);

                    m_camera_canvas.m_bForceRedraw = true;
                    m_camera_canvas.OnWindowSizeChanged(sender, e);

                    //read_shape_and_position_data(m_nCameraIndex);
                }
                else
                {
                    m_camera_canvas.set_control_width_and_height((int)grid_ImageCanvas.ActualWidth, (int)grid_ImageCanvas.ActualHeight);

                    m_camera_canvas.m_bForceRedraw = true;
                    m_camera_canvas.OnWindowSizeChanged(sender, e);
                }
            }

            if (false == m_bIsWindowInited)
            {
                m_bIsWindowInited = true;

                // 初始化数据
                InitData();
            }
        }

        // 菜单弹出
        private void ContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            if (null != m_camera_canvas)
                m_camera_canvas.m_bIsMenuShown = true;
        }

        // 菜单隐藏
        private void ContextMenu_Closed(object sender, RoutedEventArgs e)
        {
            if (null != m_camera_canvas)
                m_camera_canvas.m_bIsMenuShown = false;
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            IntPtr handle = new WindowInteropHelper(this).Handle;
            HwndSource.FromHwnd(handle).AddHook(new HwndSourceHook(WindowProc));
        }

        private IntPtr WindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case 0x0112:
                    int command = ((int)wParam & 0xFFF0);
                    switch (command)
                    {
                        case 0xF020: // SC_MINIMIZE
                            handled = true;
                            break;
                        case 0xF030: // SC_MAXIMIZE
                            handled = true;
                            break;
                        case 0xF060: // SC_CLOSE
                            handled = true;
                            break;
                    }
                    break;
            }
            return IntPtr.Zero;
        }

        // 菜单项点击事件：获取当前位置数据
        private void MenuClick_get_current_ROI_data(object sender, RoutedEventArgs e)
        {
            if (null == m_camera_canvas)
                return;

            m_parent.m_global.m_list_image_canvases[(int)scene_type.scene_camera1 + m_nCameraIndex].m_dbPrevBoardLeft = m_camera_canvas.m_dbPrevBoardLeft;
            m_parent.m_global.m_list_image_canvases[(int)scene_type.scene_camera1 + m_nCameraIndex].m_dbPrevBoardTop = m_camera_canvas.m_dbPrevBoardTop;

            m_parent.m_global.m_list_image_canvases[(int)scene_type.scene_camera1 + m_nCameraIndex].m_dbPrevFOVWidth = m_camera_canvas.m_dbPrevFOVWidth;
            m_parent.m_global.m_list_image_canvases[(int)scene_type.scene_camera1 + m_nCameraIndex].m_dbPrevFOVHeight = m_camera_canvas.m_dbPrevFOVHeight;

            m_parent.m_global.m_list_image_canvases[(int)scene_type.scene_camera1 + m_nCameraIndex].m_dbZoomRatioX = m_camera_canvas.m_dbZoomRatioX;
            m_parent.m_global.m_list_image_canvases[(int)scene_type.scene_camera1 + m_nCameraIndex].m_dbZoomRatioY = m_camera_canvas.m_dbZoomRatioY;
            m_parent.m_global.m_list_image_canvases[(int)scene_type.scene_camera1 + m_nCameraIndex].m_dbPrevZoomRatioX = m_camera_canvas.m_dbPrevZoomRatioX;
            m_parent.m_global.m_list_image_canvases[(int)scene_type.scene_camera1 + m_nCameraIndex].m_dbPrevZoomRatioY = m_camera_canvas.m_dbPrevZoomRatioY;

            m_parent.m_global.m_list_image_canvases[(int)scene_type.scene_camera1 + m_nCameraIndex].m_view_zoom_anchor = m_camera_canvas.m_view_zoom_anchor;
            m_parent.m_global.m_list_image_canvases[(int)scene_type.scene_camera1 + m_nCameraIndex].m_view_drag_pt = m_camera_canvas.m_view_drag_pt;

            //m_camera_canvas.get_current_ROI_data();
        }

        // 菜单项点击事件：框选细节观察区域1
        private void MenuClick_select_ROI_as_observation_area1(object sender, RoutedEventArgs e)
        {
            m_camera_canvas.clear_all_content();
            m_camera_canvas.clear_all_search_rects();
            
            m_camera_canvas.start_selecting_rect(RECT_UTILITY_TYPE.OBSERVATION_AREA1);
        }

        // 菜单项点击事件：框选细节观察区域2
        private void MenuClick_select_ROI_as_observation_area2(object sender, RoutedEventArgs e)
        {
            m_camera_canvas.clear_all_content();
            m_camera_canvas.clear_all_search_rects();

            m_camera_canvas.start_selecting_rect(RECT_UTILITY_TYPE.OBSERVATION_AREA2);
        }

        // 菜单项点击事件：框选细节观察区域3
        private void MenuClick_select_ROI_as_observation_area3(object sender, RoutedEventArgs e)
        {
            m_camera_canvas.clear_all_content();
            m_camera_canvas.clear_all_search_rects();

            m_camera_canvas.start_selecting_rect(RECT_UTILITY_TYPE.OBSERVATION_AREA3);
        }

        // 菜单项点击事件：框选细节观察区域4
        private void MenuClick_select_ROI_as_observation_area4(object sender, RoutedEventArgs e)
        {
            m_camera_canvas.clear_all_content();
            m_camera_canvas.clear_all_search_rects();

            m_camera_canvas.start_selecting_rect(RECT_UTILITY_TYPE.OBSERVATION_AREA4);
        }

        // 回调函数：生成细节观察区域1
        public void GenerateObservationArea1(object[] parameters)
        {
            Point2d lefttop = (Point2d)parameters[0];
            Point2d rightbottom = (Point2d)parameters[1];
            scene_type type = (scene_type)parameters[2];

            if (MessageBoxResult.OK == MessageBox.Show(m_parent, "请确认是否生成此细节观察区域1？", "提示", MessageBoxButton.OKCancel))
            {
                var derivedCanvas = new DerivedCanvas();

                if (m_parent.m_global.m_list_derived_canvases.Count <= (int)type)
                {
                    derivedCanvas.m_size = new System.Drawing.Size(100, 100);
                    derivedCanvas.m_lefttop_on_UI = new Point2i(0, 0);
                    derivedCanvas.m_observation_ROI_lefttop_in_origin_image = new Point2i((int)lefttop.x, (int)lefttop.y);
                    derivedCanvas.m_observation_ROI_rightbottom_in_origin_image = new Point2i((int)rightbottom.x, (int)rightbottom.y);

                    m_parent.m_global.m_list_derived_canvases.Add(derivedCanvas);
                }
                else
                {
                    derivedCanvas = m_parent.m_global.m_list_derived_canvases[(int)type];

                    derivedCanvas.m_observation_ROI_lefttop_in_origin_image = new Point2i((int)lefttop.x, (int)lefttop.y);
                    derivedCanvas.m_observation_ROI_rightbottom_in_origin_image = new Point2i((int)rightbottom.x, (int)rightbottom.y);
                }

                // 把图片1的ROI区域内容复制到ROI画布1
                if (true)
                {
                    int nOriginImageWidth = m_camera_canvas.get_origin_image_width();
                    int nOriginImageHeight = m_camera_canvas.get_origin_image_height();

                    int nLeft = derivedCanvas.m_observation_ROI_lefttop_in_origin_image.x;
                    int nTop = derivedCanvas.m_observation_ROI_lefttop_in_origin_image.y;
                    int nWidth = derivedCanvas.m_observation_ROI_rightbottom_in_origin_image.x - derivedCanvas.m_observation_ROI_lefttop_in_origin_image.x;
                    int nHeight = derivedCanvas.m_observation_ROI_rightbottom_in_origin_image.y - derivedCanvas.m_observation_ROI_lefttop_in_origin_image.y;

                    m_parent.page_HomeView.CopyROIContentFromSrcImageToDstImage((int)type, (int)scene_type.scene_ROI_canvas1 + (int)type, 
                        nLeft, nTop, nWidth, nHeight, nOriginImageWidth, nOriginImageHeight);

                    m_parent.page_HomeView.m_home_view.m_derived_camera_windows[(int)type].m_camera_canvas.m_bForceRedraw = true;
                    m_parent.page_HomeView.m_home_view.m_derived_camera_windows[(int)type].m_camera_canvas.show_whole_image();
                }
            }
        }

        // 回调函数：生成细节观察区域2
        public void GenerateObservationArea2(object[] parameters)
        {
            Point2d lefttop = (Point2d)parameters[0];
            Point2d rightbottom = (Point2d)parameters[1];
            scene_type type = (scene_type)parameters[2];

            if (MessageBoxResult.OK == MessageBox.Show(m_parent, "请确认是否生成此细节观察区域2？", "提示", MessageBoxButton.OKCancel))
            {
                var derivedCanvas = new DerivedCanvas();

                if (m_parent.m_global.m_list_derived_canvases.Count <= (int)type)
                {
                    derivedCanvas.m_size = new System.Drawing.Size(100, 100);
                    derivedCanvas.m_lefttop_on_UI = new Point2i(0, 0);
                    derivedCanvas.m_observation_ROI_lefttop_in_origin_image = new Point2i((int)lefttop.x, (int)lefttop.y);
                    derivedCanvas.m_observation_ROI_rightbottom_in_origin_image = new Point2i((int)rightbottom.x, (int)rightbottom.y);

                    m_parent.m_global.m_list_derived_canvases.Add(derivedCanvas);
                }
                else
                {
                    derivedCanvas = m_parent.m_global.m_list_derived_canvases[(int)type];

                    derivedCanvas.m_observation_ROI_lefttop_in_origin_image = new Point2i((int)lefttop.x, (int)lefttop.y);
                    derivedCanvas.m_observation_ROI_rightbottom_in_origin_image = new Point2i((int)rightbottom.x, (int)rightbottom.y);
                }

                // 把图片2的ROI区域内容复制到ROI画布2
                if (true)
                {
                    int nOriginImageWidth = m_camera_canvas.get_origin_image_width();
                    int nOriginImageHeight = m_camera_canvas.get_origin_image_height();

                    int nLeft = derivedCanvas.m_observation_ROI_lefttop_in_origin_image.x;
                    int nTop = derivedCanvas.m_observation_ROI_lefttop_in_origin_image.y;
                    int nWidth = derivedCanvas.m_observation_ROI_rightbottom_in_origin_image.x - derivedCanvas.m_observation_ROI_lefttop_in_origin_image.x;
                    int nHeight = derivedCanvas.m_observation_ROI_rightbottom_in_origin_image.y - derivedCanvas.m_observation_ROI_lefttop_in_origin_image.y;

                    m_parent.page_HomeView.CopyROIContentFromSrcImageToDstImage((int)type, (int)scene_type.scene_ROI_canvas1 + (int)type,
                        nLeft, nTop, nWidth, nHeight, nOriginImageWidth, nOriginImageHeight);

                    m_parent.page_HomeView.m_home_view.m_derived_camera_windows[(int)type].m_camera_canvas.m_bForceRedraw = true;
                    m_parent.page_HomeView.m_home_view.m_derived_camera_windows[(int)type].m_camera_canvas.show_whole_image();
                }
            }
        }

        // 回调函数：生成细节观察区域3
        public void GenerateObservationArea3(object[] parameters)
        {
            Point2d lefttop = (Point2d)parameters[0];
            Point2d rightbottom = (Point2d)parameters[1];
            scene_type type = (scene_type)parameters[2];

            if (MessageBoxResult.OK == MessageBox.Show(m_parent, "请确认是否生成此细节观察区域3？", "提示", MessageBoxButton.OKCancel))
            {
            }
        }

        // 回调函数：生成细节观察区域4
        public void GenerateObservationArea4(object[] parameters)
        {
            Point2d lefttop = (Point2d)parameters[0];
            Point2d rightbottom = (Point2d)parameters[1];
            scene_type type = (scene_type)parameters[2];

            if (MessageBoxResult.OK == MessageBox.Show(m_parent, "请确认是否生成此细节观察区域4？", "提示", MessageBoxButton.OKCancel))
            {
            }
        }

        // 鼠标事件：鼠标滚轮在画板上滚动
        private void ctrl_ImageCanvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            m_camera_canvas.OnMouseWheel(sender, e);
        }

        // 鼠标事件：鼠标在画板上移动
        private void ctrl_ImageCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            m_camera_canvas.OnMouseMove(sender, e);
        }

        // 鼠标事件：鼠标在画板上按下
        private void ctrl_ImageCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            m_camera_canvas.OnMouseDown(sender, e);
        }

        // 鼠标事件：鼠标在画板上抬起
        private void ctrl_ImageCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            m_camera_canvas.OnMouseUp(sender, e);
        }

        // 功能：初始化数据
        private void InitData()
        {
            if (null == m_parent)
                return;

            // 设置画布回调函数
            set_canvas_callbacks(m_camera_canvas);
        }

        // 功能：设置画布回调函数
        public void set_canvas_callbacks(CameraCanvas canvas)
        {
            canvas.set_utility_rect_callbacks(new BaseCanvas<CameraCanvas>.callback_delegate(GenerateObservationArea1), RECT_UTILITY_TYPE.OBSERVATION_AREA1);
            canvas.set_utility_rect_callbacks(new BaseCanvas<CameraCanvas>.callback_delegate(GenerateObservationArea2), RECT_UTILITY_TYPE.OBSERVATION_AREA2);
            canvas.set_utility_rect_callbacks(new BaseCanvas<CameraCanvas>.callback_delegate(GenerateObservationArea3), RECT_UTILITY_TYPE.OBSERVATION_AREA3);
            canvas.set_utility_rect_callbacks(new BaseCanvas<CameraCanvas>.callback_delegate(GenerateObservationArea4), RECT_UTILITY_TYPE.OBSERVATION_AREA4);
        }

        // 功能：打开图片，并在画布上显示
        public void OpenAndShowImageOnCanvas(string strImagePath, int nCanvasIndex)
        {
            BaseCanvas<CameraCanvas>.OpenImageFileAndShowOnBIVCanvas(strImagePath, m_camera_canvas);

            m_camera_canvas.clear_all_search_rects();

            Application.Current.Dispatcher.Invoke(() =>
            {
                m_camera_canvas.show_whole_image();
            });
        }

    }
}
