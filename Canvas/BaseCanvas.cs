using AutoScanFQCTest;
using AutoScanFQCTest;
using AutoScanFQCTest.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Point = System.Drawing.Point;

namespace AutoScanFQCTest.Canvas
{
    public enum scene_type
    {
        scene_undefined = -1,
        scene_camera1,                                   // 导航图像
        scene_camera2,                                   // 细节图像
        scene_panel,                                        // PCB画布
    }

    public struct Point2d
    {
        public Point2d(double x, double y)
        {
            this.x = x;
            this.y = y;
        }
        public void set(double x, double y)
        {
            this.x = x;
            this.y = y;
        }
        public double x;
        public double y;
    }
    public struct Point2i
    {
        public Point2i(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
        public void set(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
        public int x;
        public int y;
    }

    public enum RECT_UTILITY_TYPE
    {
        NONE = -1,
        TEMPLATE = 0,
        NCC_TEMPLATE_SEARCH_AREA1,
        NCC_TEMPLATE_SEARCH_AREA2,
        NCC_TEMPLATE_SEARCH_AREA3,
        NCC_TEMPLATE_SEARCH_AREA4,
        NCC_TEMPLATE1,
        NCC_TEMPLATE2,
        NCC_TEMPLATE3,
        NCC_TEMPLATE4,
        DEFECT_AREA1,
        DEFECT_AREA1_AS_ROTATED,
        OCR_AREA1,
        PASTE_ROI_TO_IMAGE
    }

    // 模板识别结果
    public class CFindingResult
    {
        public bool m_bFound = false;                           // 是否找到
        public bool m_bIsRotatedRect = false;              // 是否是旋转矩形

        public Rect m_rect = new Rect();                       // 定位到的目标矩形（非旋转）

        public Point2d[] m_rotated_rect_corners = new Point2d[4];           // 定位到的目标矩形（旋转）

        public double m_dbSimilarity = 0;                        // 相似度
        public double m_dbRotationAngle = 0;                // 旋转角度

        public CFindingResult()
        {
            for (int i = 0; i < 4; i++)
                m_rotated_rect_corners[i] = new Point2d(0, 0);

            m_rect.X = 0;
            m_rect.Y = 0;
            m_rect.Width = 0;
            m_rect.Height = 0;
        }

        public CFindingResult cloneClass()
        {
            return (CFindingResult)this.MemberwiseClone();
        }
    }

    public class SearchRect
    {
        public RECT_UTILITY_TYPE m_rect_utility_type;
        public Point2d m_lefttop;
        public Point2d m_rightbottom;
        public Point2d m_select_start_pt = new Point2d(0, 0);
        public Point2d m_select_end_pt = new Point2d(0, 0);

        public const double DEFAULT_ROTATED_RECT_WH_RATIO = 2.0;

        public double m_rotated_rect_wh_ratio = DEFAULT_ROTATED_RECT_WH_RATIO;

        public Point2d[] m_rotated_rect_corners = new Point2d[4];

        public bool m_bRotatedRectIsReady = false; // 旋转矩形是否已经准备好

        public bool m_bIsSelecting;                // 是否正在框选
        public bool m_bIsShaped;                   // 是否已具备形状/成形

        public SearchRect(RECT_UTILITY_TYPE type)
        {
            init();

            m_rect_utility_type = type;
        }

        public SearchRect(Point2d lefttop, Point2d rightbottom)
        {
            m_lefttop = lefttop;
            m_rightbottom = rightbottom;
        }

        public void clear()
        {
            m_lefttop = new Point2d(0, 0);
            m_rightbottom = new Point2d(0, 0);
        }

        public void init()
        {
            m_rect_utility_type = RECT_UTILITY_TYPE.TEMPLATE;

            m_bIsSelecting = false;
            m_bIsShaped = false;

            m_lefttop = new Point2d(0, 0);
            m_rightbottom = new Point2d(0, 0);
            m_select_start_pt = new Point2d(0, 0);
            m_select_end_pt = new Point2d(0, 0);
        }

        public SearchRect cloneClass()
        {
            return (SearchRect)this.MemberwiseClone();
        }
    }

    public class TemplateRect
    {
        public bool m_bFoundTemplate;                        // 是否找到芯片模板

        public double m_template_width;
        public double m_template_height;
        public double m_score;
        public double m_angle;

        public Point2d m_lefttop;
        public Point2d m_rightbottom;
        public Point2d m_center;

        public List<Point2d> m_list_contour_pts = new List<Point2d>();

        public double m_object_height = 0;                   // 物体高度

        public bool m_bIsOK = true;                              // 是否OK，false表示NG

        public int m_nDefectType = 0;                           // 缺陷类型，1为孔大，2为孔小

        public TemplateRect()
        {
            init();
        }

        public TemplateRect(Point2d lefttop, Point2d rightbottom)
        {
            m_lefttop = lefttop;
            m_rightbottom = rightbottom;
        }

        public void init()
        {
            m_bFoundTemplate = false;

            m_template_width = 0;
            m_template_height = 0;
            m_score = 0;
            m_angle = 0;

            m_lefttop = new Point2d(0, 0);
            m_rightbottom = new Point2d(0, 0);
        }
    }

    public class YOLOObject
    {
        public YOLOObject(int nClass, string strName, Point lefttop, Point rightbottom)
        {
            m_nClass = nClass;
            m_strName = strName;

            m_lefttop = lefttop;
            m_rightbottom = rightbottom;
        }

        public int m_nClass;

        public string m_strName;

        public Point m_lefttop;
        public Point m_rightbottom;
    }

    public abstract class BaseCanvas<T> where T : BaseCanvas<T>
    {
        public MainWindow m_parent;

        public System.Windows.Controls.Image m_control_image;
        public Bitmap m_bitmap = null;

        public int m_nControlWidth = 0;
        public int m_nControlHeight = 0;

        public const float INITIAL_ZOOM_RATIO = 0.80f;

        public int m_nLastClickTime = 0;
        public int m_nMouseDownTime = 0;
        public bool m_bIsEditable = true;
        public bool m_bIsThumbnail = true;
        public float m_fZoomRatio = INITIAL_ZOOM_RATIO;
        public float m_fPrevZoomRatio = INITIAL_ZOOM_RATIO;
        public float m_fPrevBoardLeft = 0;
        public float m_fPrevBoardTop = 0;
        public Point2d m_mouse_move_pt = new Point2d(0, 0);
        public Point2d m_mouse_move_pt_on_image = new Point2d(0, 0);
        public Point2d m_mouse_down_pt = new Point2d(0, 0);
        public Point2d m_mouse_up_pt = new Point2d(0, 0);
        public Point2d m_view_drag_pt = new Point2d(0, 0);
        public Point2d m_view_zoom_anchor = new Point2d(0, 0);
        public PointF m_view_mouse_move_pt = new PointF(0, 0);
        public Point2d m_mouse_click_pt = new Point2d(0, 0);

        public static string m_strCurrentImageFilePath = "";          // 当前打开的图片文件路径

        public bool m_bIsMenuShown = false;
        public bool m_bForceRedraw = false;
        public bool m_bFreezeImage = true;
        public bool m_bHasValidImage = false;

        public Bitmap m_origin_bitmap = null;
        public Bitmap m_stretched_bitmap = null;
        public Bitmap m_bitmap_FOV = null;

        protected int m_nOriginImageWidth = 0;
        protected int m_nOriginImageHeight = 0;

        protected const double INITIAL_ZOOM_RATIO_X = 1;
        protected const double INITIAL_ZOOM_RATIO_Y = 1;

        protected double m_dbPrevFOVLeft = 0;
        protected double m_dbPrevFOVTop = 0;
        protected double m_dbPrevFOVWidth = 0;
        protected double m_dbPrevFOVHeight = 0;

        protected double m_dbPrevBoardLeft = 0;
        protected double m_dbPrevBoardTop = 0;
        protected double m_dbZoomRatioX = INITIAL_ZOOM_RATIO_X;
        protected double m_dbZoomRatioY = INITIAL_ZOOM_RATIO_Y;
        protected double m_dbPrevZoomRatioX = INITIAL_ZOOM_RATIO_X;
        protected double m_dbPrevZoomRatioY = INITIAL_ZOOM_RATIO_Y;

        public delegate void callback_delegate(object[] parameters);

        public scene_type m_scene_type = scene_type.scene_camera1;

        protected Color[] m_colors_for_yolo_object;

        public BaseCanvas(MainWindow parent, System.Windows.Controls.Image image, int nControlWidth, int nControlHeight,
            bool bIsEditable, bool bIsThumbnail = false)
        {
            m_parent = parent;

            m_control_image = image;

            m_nControlWidth = nControlWidth;
            m_nControlHeight = nControlHeight;

            m_bIsEditable = bIsEditable;
            m_bIsThumbnail = bIsThumbnail;

            if (true)
            {
                m_colors_for_yolo_object = new Color[]
                {
                    Color.FromArgb(255, 0, 0),       // 红色
                    Color.FromArgb(255, 0, 0),       // 红色
                    Color.FromArgb(155, 255, 155),       // 深绿色
                    Color.FromArgb(0, 0, 255),       // 蓝色
                    Color.FromArgb(255, 255, 0),     // 黄色
                    Color.FromArgb(0, 255, 255),     // 青色
                    Color.FromArgb(255, 0, 255),     // 品红色
                    Color.FromArgb(255, 165, 0),     // 橙色
                    Color.FromArgb(128, 0, 128),     // 紫色
                    Color.FromArgb(128, 128, 0),     // 橄榄色
                    Color.FromArgb(0, 128, 128),     // 暗青色
                    Color.FromArgb(0, 0, 128),       // 深蓝色
                    Color.FromArgb(128, 0, 0),       // 栗色
                    Color.FromArgb(139, 69, 19),     // 马鞍棕色
                    Color.FromArgb(255, 192, 203),   // 粉红色
                    Color.FromArgb(255, 105, 180),   // 热粉红色
                    Color.FromArgb(244, 164, 96),    // 沙棕色
                    Color.FromArgb(75, 0, 130),      // 靛青
                    Color.FromArgb(255, 255, 240),   // 乳白
                    Color.FromArgb(240, 248, 255),   // 爱丽丝蓝
                    Color.FromArgb(0, 206, 209),     // 深绿松石
                    Color.FromArgb(218, 165, 32),    // 金色
                    Color.FromArgb(135, 206, 235),   // 天蓝色
                    Color.FromArgb(152, 251, 152),   // 苍白绿
                };
            }
        }

        // 功能：打开一个图片文件，并显示在BIV画布上
        public static void OpenImageFileAndShowOnBIVCanvas(string strImagePath, int nRotationAngle, T canvas)
        {
            char[] path = strImagePath.ToCharArray();

            int[] pIntParams = new int[10];
            int[] pRetInts = new int[10];

            int nDestWidth = (int)canvas.m_nControlWidth;
            int nDestHeight = (int)canvas.m_nControlHeight;

            pIntParams[0] = (int)(canvas.m_scene_type);
            pIntParams[1] = nDestWidth;
            pIntParams[2] = nDestHeight;
            pIntParams[3] = nRotationAngle;          // 图像旋转角度，0为不旋转，1为顺时针旋转90度，2为顺时针旋转180度，3为顺时针旋转270度

            pRetInts[0] = 0;
            DllImport.PostVisionDLL_load_image_from_file2(path, pIntParams, pRetInts);

            if (1 == pRetInts[0])
            {
                m_strCurrentImageFilePath = strImagePath;

                int nSrcImageWidth = pRetInts[1];
                int nSrcImageHeight = pRetInts[2];

                Debugger.Log(0, null, string.Format("222222 OpenImageFileAndShowOnBIVCanvas() nSrcImageWidth = [{0},{1}]", nSrcImageWidth, nSrcImageHeight));

                canvas.set_origin_image_size(nSrcImageWidth, nSrcImageHeight);

                canvas.m_bHasValidImage = true;
                canvas.m_bForceRedraw = true;
            }
        }



        // 设置原图大小
        public void set_origin_image_size(int nWidth, int nHeight)
        {
            m_nOriginImageWidth = nWidth;
            m_nOriginImageHeight = nHeight;
        }

        // 鼠标事件：鼠标滚轮在画板上滚动
        public abstract void OnMouseWheel(object sender, MouseWheelEventArgs e);

        // 鼠标事件：鼠标在画板上弹起
        public abstract void OnMouseUp(object sender, MouseButtonEventArgs e);

        // 鼠标事件：鼠标在画板上按下
        public abstract void OnMouseDown(object sender, MouseButtonEventArgs e);

        // 鼠标事件：鼠标在画板上拖动
        public abstract void OnMouseMove(object sender, System.Windows.Input.MouseEventArgs e);
    }
}
