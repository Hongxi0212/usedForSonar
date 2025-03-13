using AutoScanFQCTest.DataModels;
using AutoScanFQCTest.Utilities;
using AutoScanFQCTest;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Diagnostics;
using Newtonsoft.Json;
using AutoScanFQCTest.Canvas;
using AutoScanFQCTest.DataModels;
using AutoScanFQCTest.Utilities;
using AutoScanFQCTest;

namespace AutoScanFQCTest.Canvas
{
    // 目标检测评估结果
    public class YoloEvaluationResult
    {
        [JsonProperty("image_path")]
        public string ImagePath { get; set; }

        [JsonProperty("predicted_class")]
        public int PredictedClass { get; set; }

        [JsonProperty("label_class")]
        public int LabelClass { get; set; }

        [JsonProperty("label_class")]
        public string strClass { get; set; }

        [JsonProperty("confidence")]
        public double Confidence { get; set; }

        [JsonProperty("x1")]
        public double X1 { get; set; }

        [JsonProperty("y1")]
        public double Y1 { get; set; }

        [JsonProperty("x2")]
        public double X2 { get; set; }

        [JsonProperty("y2")]
        public double Y2 { get; set; }

        public Point2d[] m_rotated_rect_corners = new Point2d[4];

        public YoloEvaluationResult cloneClass()
        {
            return (YoloEvaluationResult)this.MemberwiseClone();
        }
    }

    public class CameraCanvas : BaseCanvas<CameraCanvas>
    {
        [DllImport("kernel32.dll", CallingConvention = CallingConvention.Winapi)]
        extern static int GetTickCount();

        [DllImport("AICore.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool PostVisionDLL_get_affine_transformed_image2(byte[] buf, int[] pIntParams);

        [DllImport("AICore.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool PostVisionDLL_get_gray_value_of_a_point(int[] pIntParams, int[] pRetInts);

        [DllImport("AICore.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        unsafe extern static bool get_theta(double start_x, double start_y, double end_x, double end_y, double[] out_doubles);

        [DllImport("AICore.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        unsafe extern static bool rotate_crd(double[] in_crds, double[] out_crds, double rotate_angle);

        public int m_nClickCounter = 0;

        public bool m_bShowFoundTemplate = false;
        public bool m_bShowUnetResult = false;
        public bool m_bShowDetectionResult = false;

        public List<YoloEvaluationResult> m_detect_validation_result = new List<YoloEvaluationResult>();                 // 检测模型评估结果

        public List<String> m_OCR_results = new List<String>();                 // OCR识别结果

        public bool m_bIsEditable = false;

        private Point2d m_gauge_2D_starting_pt_for_point_to_point_distance = new Point2d(0, 0);          // 二次元---点到点距离(起点)
        private Point2d m_gauge_2D_end_pt_for_point_to_point_distance = new Point2d(0, 0);                // 二次元---点到点距离(终点)
        private Point2d m_gauge_2D_angle_vertex = new Point2d(0, 0);                                                      // 二次元---角度(顶点)
        private Point2d m_gauge_2D_angle_end_pt1 = new Point2d(0, 0);                                                   // 二次元---角度(端点1)
        private Point2d m_gauge_2D_angle_end_pt2 = new Point2d(0, 0);                                                   // 二次元---角度(端点2)
        public double m_gauge_2D_point_to_point_distance = 0;

        public SearchRect[] m_template_rects = new SearchRect[50];

        public TemplateRect[] m_found_templates = new TemplateRect[50];

        private callback_delegate m_callback_generate_starting_pt_for_point_to_point_distance = null;
        private callback_delegate m_callback_generate_end_pt_for_point_to_point_distance = null;

        private callback_delegate[] m_utility_rect_callbacks = new callback_delegate[50];

        private callback_delegate[] m_tagging_ROI_callbacks = new callback_delegate[50];

        // 引用的YOLO对象（string为图片文件路径）
        public List<YOLOObject> m_referenced_list_yolo_objects = new List<YOLOObject>();

        public List<List<Point2d>> m_list_list_unet_pts = new List<List<Point2d>>();                 // UNet识别结果

        public YOLOObject m_yolo_object = null;

        public CameraCanvas(MainWindow parent, System.Windows.Controls.Image image, int nControlWidth, int nControlHeight,
            scene_type scene_type, bool bIsEditable, bool bIsThumbnail = false) : base(parent, image, nControlWidth, nControlHeight, bIsEditable)
        {
            m_bIsEditable = bIsEditable;

            m_scene_type = scene_type;

            m_origin_bitmap = GeneralUtilities.generate_single_color_bitmap(nControlWidth, nControlHeight, Color.FromArgb(255, 255, 255));
            m_stretched_bitmap = (Bitmap)(m_origin_bitmap.Clone());
        }

        public void OnWindowSizeChanged(object sender, System.EventArgs e)
        {
            if (null == m_origin_bitmap || null == m_parent)
                return;

            refresh_view(m_control_image, m_nControlWidth, m_nControlHeight, ref m_bitmap, m_parent.m_global,
                0, m_dbZoomRatioX, m_dbZoomRatioY, m_dbPrevZoomRatioX, m_dbPrevZoomRatioY,
                m_view_zoom_anchor, ref m_view_drag_pt);
        }

        public void set_callback_generate_starting_pt_for_point_to_point_distance(callback_delegate func)
        {
            m_callback_generate_starting_pt_for_point_to_point_distance = new callback_delegate(func);
        }

        public void set_callback_generate_end_pt_for_point_to_point_distance(callback_delegate func)
        {
            m_callback_generate_end_pt_for_point_to_point_distance = new callback_delegate(func);
        }

        public void set_utility_rect_callbacks(callback_delegate func, RECT_UTILITY_TYPE type)
        {
            m_utility_rect_callbacks[(int)type] = new callback_delegate(func);
        }

        private void invoke_callback_by_rect_utility_type(RECT_UTILITY_TYPE type, SearchRect template_rect, scene_type scene_type)
        {
            object[] parameters = new object[5];

            parameters[0] = template_rect.m_lefttop;
            parameters[1] = template_rect.m_rightbottom;
            parameters[2] = scene_type;

            if (null != m_utility_rect_callbacks[(int)type])
            {
                m_utility_rect_callbacks[(int)type](parameters);
            }
        }

        // 计算合适的倍率
        public void init_proper_zoom_ratio(int nControlWidth, int nControlHeight, int nBmpWidth, int nBmpHeight)
        {
            m_dbZoomRatioX = (double)(nBmpWidth) / (double)(nControlWidth);
            m_dbZoomRatioY = (double)(nBmpHeight) / (double)(nControlHeight);

            m_dbPrevZoomRatioX = m_dbZoomRatioX;
            m_dbPrevZoomRatioY = m_dbZoomRatioY;

            if (false == m_bIsEditable)
                m_view_drag_pt = new Point2d(0, 0);
        }

        // 显示整张图像
        public void show_whole_image()
        {
            m_mouse_click_pt = new Point2d(0, 0);

            if (true == m_bIsEditable)
                m_view_drag_pt = new Point2d(0, 0);

            init_proper_zoom_ratio(m_nControlWidth, m_nControlHeight, m_stretched_bitmap.Width, m_stretched_bitmap.Height);

            refresh_view(m_control_image, m_nControlWidth, m_nControlHeight, ref m_bitmap, m_parent.m_global,
                0, m_dbZoomRatioX, m_dbZoomRatioY, m_dbPrevZoomRatioX, m_dbPrevZoomRatioY,
                m_view_zoom_anchor, ref m_view_drag_pt, false, true);
        }

        // 设置视野中心坐标
        public void set_FOV_center(Point2i center)
        {
            double dbReadjustedImageWidth = (double)(m_nOriginImageWidth);
            double dbReadjustedImageHeight = (double)(m_nOriginImageHeight);

            double image_offset_x = 0;
            double image_offset_y = 0;
            double bitmap_ratio_x = 1;
            double bitmap_ratio_y = 1;
            if ((null != m_origin_bitmap) && (null != m_stretched_bitmap))
            {
                bitmap_ratio_x = (double)(m_nOriginImageWidth) / (double)(m_stretched_bitmap.Width);
                bitmap_ratio_y = (double)(m_nOriginImageHeight) / (double)(m_stretched_bitmap.Height);

                if (bitmap_ratio_x > bitmap_ratio_y)
                {
                    double temp = Math.Round(bitmap_ratio_x * (double)m_nControlHeight - (double)m_nOriginImageHeight);

                    if (((int)temp % 2) != 0)
                        temp += 1;

                    dbReadjustedImageHeight = (double)(m_nOriginImageHeight) + temp;

                    bitmap_ratio_y = dbReadjustedImageHeight / (double)(m_stretched_bitmap.Height);

                    image_offset_y = temp / 2;

                    //if (0 == m_dbPrevBoardTop)
                    //    m_dbPrevBoardTop = 0 - temp / 2;
                }
                else
                {
                    double temp = Math.Round(bitmap_ratio_y * (double)m_nControlWidth - (double)m_nOriginImageWidth);

                    if (((int)temp % 2) != 0)
                        temp += 1;

                    dbReadjustedImageWidth = (double)(m_nOriginImageWidth) + temp;

                    bitmap_ratio_x = dbReadjustedImageWidth / (double)(m_stretched_bitmap.Width);

                    image_offset_x = temp / 2;
                }
            }

            m_dbZoomRatioX = 0.038;
            m_dbZoomRatioY = 0.038;

            m_view_drag_pt.x = center.x - m_nOriginImageWidth / 2;
            m_view_drag_pt.y = center.y - m_nOriginImageHeight / 2;
        }

        // 将图形数据绘制输出到 bitmap位图，nActionType 0为拖动，1为缩放
        public void refresh_view(System.Windows.Controls.Image ctrl_image,
            int nControlWidth, int nControlHeight, ref Bitmap bitmap_camera, Global global_data, int nActionType,
            double dbZoomRatioX, double dbZoomRatioY, double dbPrevZoomRatioX, double dbPrevZoomRatioY,
            Point2d zoom_anchor, ref Point2d drag_pt, bool bShowSelectionRectOnly = false, bool bShowWholeImage = false)
        {
            if (null == m_origin_bitmap)
                return;

            double dbReadjustedImageWidth = (double)(m_nOriginImageWidth);
            double dbReadjustedImageHeight = (double)(m_nOriginImageHeight);

            //Debugger.Log(0, null, string.Format("222222 m_nOriginImageWidth = [{0},{1}]", m_nOriginImageWidth, m_nOriginImageWidth));

            double image_offset_x = 0;
            double image_offset_y = 0;
            double bitmap_ratio_x = 1;
            double bitmap_ratio_y = 1;
            if ((null != m_origin_bitmap) && (null != m_stretched_bitmap))
            {
                bitmap_ratio_x = (double)(m_nOriginImageWidth) / (double)(m_stretched_bitmap.Width);
                bitmap_ratio_y = (double)(m_nOriginImageHeight) / (double)(m_stretched_bitmap.Height);

                //Debugger.Log(0, null, string.Format("222222 m_stretched_bitmap.Width [{0},{1}], bitmap_ratio_x = [{2:0.000},{3:0.000}]",
                //    m_stretched_bitmap.Width, m_stretched_bitmap.Height, bitmap_ratio_x, bitmap_ratio_y));

                if (bitmap_ratio_x > bitmap_ratio_y)
                {
                    double temp = Math.Round(bitmap_ratio_x * (double)nControlHeight - (double)m_nOriginImageHeight);

                    if (((int)temp % 2) != 0)
                        temp += 1;

                    dbReadjustedImageHeight = (double)(m_nOriginImageHeight) + temp;

                    bitmap_ratio_y = dbReadjustedImageHeight / (double)(m_stretched_bitmap.Height);

                    image_offset_y = temp / 2;

                    //if (0 == m_dbPrevBoardTop)
                    //    m_dbPrevBoardTop = 0 - temp / 2;
                }
                else
                {
                    double temp = Math.Round(bitmap_ratio_y * (double)nControlWidth - (double)m_nOriginImageWidth);

                    if (((int)temp % 2) != 0)
                        temp += 1;

                    dbReadjustedImageWidth = (double)(m_nOriginImageWidth) + temp;

                    bitmap_ratio_x = dbReadjustedImageWidth / (double)(m_stretched_bitmap.Width);

                    image_offset_x = temp / 2;

                    //if (0 == m_dbPrevBoardLeft)
                    //     m_dbPrevBoardLeft = 0 - temp / 2;
                }
            }

            // 以原图坐标为单位
            double dbFOVWidth = dbReadjustedImageWidth * dbZoomRatioX;
            double dbFOVHeight = dbReadjustedImageHeight * dbZoomRatioY;
            //Debugger.Log(0, null, string.Format("222222 drag_pt = [{0:0.},{1:0.}]", drag_pt.x, drag_pt.y));

            if (false == bShowWholeImage && m_dbPrevFOVWidth > 0)
            {
                double dbPrevZoomRatio = (double)m_nControlWidth / m_dbPrevFOVWidth;
                double dbZoomRatio = (double)m_nControlWidth / dbFOVWidth;

                if (dbPrevZoomRatio < 1 && dbZoomRatio > 1)
                {
                    dbFOVWidth = m_nControlWidth;
                    dbFOVHeight = m_nControlHeight;

                    dbZoomRatioX = dbFOVWidth / dbReadjustedImageWidth;
                    dbZoomRatioY = dbZoomRatioX;
                }
                else if (dbPrevZoomRatio > 1 && dbZoomRatio < 1)
                {
                    dbFOVWidth = m_nControlWidth;
                    dbFOVHeight = m_nControlHeight;

                    dbZoomRatioX = dbFOVWidth / dbReadjustedImageWidth;
                    dbZoomRatioY = dbZoomRatioX;
                }
                else if (dbPrevZoomRatio < 0.5 && dbZoomRatio > 0.5)
                {
                    dbFOVWidth = m_nControlWidth * 2;
                    dbFOVHeight = m_nControlHeight * 2;

                    dbZoomRatioX = dbFOVWidth / dbReadjustedImageWidth;
                    dbZoomRatioY = dbZoomRatioX;
                }
                else if (dbPrevZoomRatio > 0.5 && dbZoomRatio < 0.5)
                {
                    dbFOVWidth = m_nControlWidth * 2;
                    dbFOVHeight = m_nControlHeight * 2;

                    dbZoomRatioX = dbFOVWidth / dbReadjustedImageWidth;
                    dbZoomRatioY = dbZoomRatioX;
                }
                else if (dbPrevZoomRatio < 0.25 && dbZoomRatio > 0.25)
                {
                    dbFOVWidth = m_nControlWidth * 4;
                    dbFOVHeight = m_nControlHeight * 4;

                    dbZoomRatioX = dbFOVWidth / dbReadjustedImageWidth;
                    dbZoomRatioY = dbZoomRatioX;
                }
                else if (dbPrevZoomRatio > 0.25 && dbZoomRatio < 0.25)
                {
                    dbFOVWidth = m_nControlWidth * 4;
                    dbFOVHeight = m_nControlHeight * 4;

                    dbZoomRatioX = dbFOVWidth / dbReadjustedImageWidth;
                    dbZoomRatioY = dbZoomRatioX;
                }
                else if (dbPrevZoomRatio < 0.125 && dbZoomRatio > 0.125)
                {
                    dbFOVWidth = m_nControlWidth * 8;
                    dbFOVHeight = m_nControlHeight * 8;

                    dbZoomRatioX = dbFOVWidth / dbReadjustedImageWidth;
                    dbZoomRatioY = dbZoomRatioX;
                }
                else if (dbPrevZoomRatio > 0.125 && dbZoomRatio < 0.125)
                {
                    dbFOVWidth = m_nControlWidth * 8;
                    dbFOVHeight = m_nControlHeight * 8;

                    dbZoomRatioX = dbFOVWidth / dbReadjustedImageWidth;
                    dbZoomRatioY = dbZoomRatioX;
                }

                m_dbZoomRatioX = dbZoomRatioX;
                m_dbZoomRatioY = dbZoomRatioY;

                m_dbPrevZoomRatioX = m_dbZoomRatioX;
                m_dbPrevZoomRatioY = m_dbZoomRatioY;
            }

            double dbFOVLeft = ((dbReadjustedImageWidth - dbFOVWidth) / 2) + drag_pt.x;
            double dbFOVTop = ((dbReadjustedImageHeight - dbFOVHeight) / 2) + drag_pt.y;

            if (1 == nActionType)
            {
                double dbPrevFOVLeft = m_dbPrevBoardLeft;
                double dbPrevFOVTop = m_dbPrevBoardTop;

                PointF center1 = new PointF((float)(dbFOVLeft + dbFOVWidth / 2), (float)(dbFOVTop + dbFOVHeight / 2));
                //Debugger.Log(0, null, string.Format("222222 dbFOVLeft [{0:0.},{1:0.}], dbFOVWidth [{2:0.},{3:0.}]",
                //    dbFOVLeft, dbFOVTop, dbFOVWidth, dbFOVHeight));
                //Debugger.Log(0, null, string.Format("222222 center1 [{0:0.},{1:0.}]", center1.X, center1.Y));

                Point2d anchor_in_control_pixel = new Point2d(zoom_anchor.x / (dbPrevZoomRatioX * bitmap_ratio_x),
                    zoom_anchor.y / (dbPrevZoomRatioY * bitmap_ratio_y));

                //Debugger.Log(0, null, string.Format("222222 zoom_anchor [{0:0.},{1:0.}], anchor_in_control_pixel [{2:0.},{3:0.}]", 
                //    zoom_anchor.x, zoom_anchor.y, anchor_in_control_pixel.x, anchor_in_control_pixel.y));

                dbFOVLeft = dbPrevFOVLeft + (double)anchor_in_control_pixel.x * bitmap_ratio_x * dbPrevZoomRatioX
                    - (double)anchor_in_control_pixel.x * bitmap_ratio_x * dbZoomRatioX;
                dbFOVTop = dbPrevFOVTop + (double)anchor_in_control_pixel.y * bitmap_ratio_y * dbPrevZoomRatioY
                    - (double)anchor_in_control_pixel.y * bitmap_ratio_y * dbZoomRatioY;

                dbFOVLeft = Math.Round(dbFOVLeft);
                dbFOVTop = Math.Round(dbFOVTop);

                if (dbFOVLeft < 0)
                    dbFOVLeft = 0;
                if (dbFOVTop < 0)
                    dbFOVTop = 0;

                int nLeft = (int)dbFOVLeft;
                int nTop = (int)dbFOVTop;
                int nWidth = (int)dbFOVWidth;
                int nHeight = (int)dbFOVHeight;

                if ((nLeft + nWidth) >= dbReadjustedImageWidth)
                {
                    dbFOVLeft = dbReadjustedImageWidth - nWidth;
                }
                if ((nTop + nHeight) >= dbReadjustedImageHeight)
                {
                    dbFOVTop = dbReadjustedImageHeight - nHeight;
                }
                //Debugger.Log(0, null, string.Format("222222 333 dbFOVLeft [{0:0.},{1:0.}], dbFOVWidth [{2:0.},{3:0.}]",
                //    dbFOVLeft, dbFOVTop, dbFOVWidth, dbFOVHeight));

                PointF center2 = new PointF((float)(dbFOVLeft + dbFOVWidth / 2), (float)(dbFOVTop + dbFOVHeight / 2));
                drag_pt.x += (center2.X - center1.X);
                drag_pt.y += (center2.Y - center1.Y);
            }

            m_dbPrevBoardLeft = dbFOVLeft;
            m_dbPrevBoardTop = dbFOVTop;

            if (null == m_bitmap_FOV)
                m_bitmap_FOV = new Bitmap(nControlWidth, nControlHeight);

            // 获取当前视野范围内的图像
            if (false == bShowSelectionRectOnly)
            {
                int nLeft = (int)(dbFOVLeft);
                int nTop = (int)(dbFOVTop);
                int nWidth = (int)(dbFOVWidth);
                int nHeight = (int)(dbFOVHeight);

                //Debugger.Log(0, null, string.Format("222222 555 dbFOVLeft [{0:0.},{1:0.}], dbFOVWidth [{2:0.},{3:0.}], dbReadjustedImageWidth [{4:0.},{5:0.}], nWidth [{6:0.},{7:0.}]",
                //    dbFOVLeft, dbFOVTop, dbFOVWidth, dbFOVHeight, dbReadjustedImageWidth, dbReadjustedImageHeight, nWidth, nHeight));

                if ((true == m_bForceRedraw) || ((Math.Abs(m_dbPrevFOVLeft - dbFOVLeft) > 0.1) || (Math.Abs(m_dbPrevFOVTop - dbFOVTop) > 0.1)
                    || (Math.Abs(m_dbPrevFOVWidth - dbFOVWidth) > 0.1) || (Math.Abs(m_dbPrevFOVHeight - dbFOVHeight) > 0.1)))
                {
                    m_bForceRedraw = false;

                    int[] pIntParams = new int[50];
                    byte[] buf = new byte[m_nControlWidth * m_nControlHeight * 3];
                    bool bReadjustPosition = true;

                    for (int n = 0; n < m_nControlWidth * m_nControlHeight * 3; n++)
                        buf[n] = 255;

                    pIntParams[0] = Convert.ToInt32(m_scene_type);
                    pIntParams[1] = Convert.ToInt32(bReadjustPosition);
                    pIntParams[2] = nControlWidth;
                    pIntParams[3] = nControlHeight;
                    pIntParams[10] = nLeft;
                    pIntParams[11] = nTop;
                    pIntParams[12] = nLeft + nWidth;
                    pIntParams[13] = nTop + nHeight;
                    pIntParams[14] = 0;
                    pIntParams[15] = 0;
                    pIntParams[16] = m_nControlWidth;
                    pIntParams[17] = m_nControlHeight;

                    PostVisionDLL_get_affine_transformed_image2(buf, pIntParams);

                    MemoryStream stream = new MemoryStream(buf);

                    m_bitmap_FOV = new Bitmap(m_nControlWidth, m_nControlHeight, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

                    System.Drawing.Rectangle rect = new System.Drawing.Rectangle(new System.Drawing.Point(0, 0), m_bitmap_FOV.Size);
                    System.Drawing.Imaging.BitmapData bmp_data = m_bitmap_FOV.LockBits(rect,
                        System.Drawing.Imaging.ImageLockMode.WriteOnly, m_bitmap_FOV.PixelFormat);
                    System.Runtime.InteropServices.Marshal.Copy(buf, 0, bmp_data.Scan0, buf.Length);
                    m_bitmap_FOV.UnlockBits(bmp_data);
                }

                m_dbPrevFOVLeft = dbFOVLeft;
                m_dbPrevFOVTop = dbFOVTop;
                m_dbPrevFOVWidth = dbFOVWidth;
                m_dbPrevFOVHeight = dbFOVHeight;
            }

            // 输出位图到控件显示
            if (null != m_bitmap_FOV)
            {
                Bitmap bmp = (Bitmap)m_bitmap_FOV.Clone();

                Graphics g = Graphics.FromImage(bmp);

                System.Drawing.Pen green_pen = new System.Drawing.Pen(System.Drawing.Color.FromArgb(0, 255, 0), (float)2);
                System.Drawing.Pen red_pen = new System.Drawing.Pen(System.Drawing.Color.FromArgb(255, 0, 0), (float)2);
                System.Drawing.Pen blue_pen = new System.Drawing.Pen(System.Drawing.Color.FromArgb(0, 0, 255), (float)2);
                System.Drawing.Pen purple_pen = new System.Drawing.Pen(System.Drawing.Color.FromArgb(255, 0, 255), (float)2);
                System.Drawing.Pen yellow_pen = new System.Drawing.Pen(System.Drawing.Color.FromArgb(255, 255, 0), (float)2);
                System.Drawing.Pen undefined_pen = new System.Drawing.Pen(System.Drawing.Color.FromArgb(0, 255, 255), (float)2);

                // 绘制选框
                if (true == m_bFreezeImage)
                {
                    for (int n = 0; n < m_template_rects.Length; n++)
                    {
                        if (null != m_template_rects[n])
                        {
                            if (m_template_rects[n].m_bIsSelecting || m_template_rects[n].m_bIsShaped)
                            {
                                DrawRectToBitmap(ref bmp, m_template_rects[n], new Point2d(dbFOVLeft, dbFOVTop), bitmap_ratio_x, bitmap_ratio_y,
                                    dbZoomRatioX, dbZoomRatioY, new Point2d(image_offset_x, image_offset_y), 0);
                            }
                        }
                    }
                }

                // 绘制检测目标框
                if (null != m_referenced_list_yolo_objects)
                {
                    for (int i = 0; i < m_referenced_list_yolo_objects.Count; i++)
                    {
                        DrawYoloObjectToBitmap(ref bmp, m_referenced_list_yolo_objects[i], new Point2d(dbFOVLeft, dbFOVTop), bitmap_ratio_x, bitmap_ratio_y,
                            dbZoomRatioX, dbZoomRatioY, new Point2d(image_offset_x, image_offset_y));
                    }
                }

                // 绘制矩形框
                if (null != m_yolo_object)
                {
                    List<YOLOObject> list_yolo_objects = new List<YOLOObject>();
                    list_yolo_objects.Add(m_yolo_object);

                    for (int i = 0; i < list_yolo_objects.Count; i++)
                    {
                        DrawYoloObjectToBitmap(ref bmp, list_yolo_objects[i], new Point2d(dbFOVLeft, dbFOVTop), bitmap_ratio_x, bitmap_ratio_y,
                                                       dbZoomRatioX, dbZoomRatioY, new Point2d(image_offset_x, image_offset_y));
                    }
                }

                // 绘制坐标
                if (true == m_bHasValidImage)
                {
                    //System.Drawing.Brush brush1 = new SolidBrush(System.Drawing.Color.FromArgb(150, 160, 160, 160));
                    System.Drawing.Brush brush2 = new SolidBrush(System.Drawing.Color.FromArgb(255, 255, 255));
                    System.Drawing.Brush brush1 = new SolidBrush(System.Drawing.Color.FromArgb(100, 200, 60, 220));
                    //System.Drawing.Brush brush2 = new SolidBrush(System.Drawing.Color.FromArgb(250, 35, 50));
                    Font font = new Font("Arial", 9, System.Drawing.FontStyle.Regular);

                    int[] pIntParams = new int[10];
                    int[] pRetInts = new int[10];

                    pIntParams[0] = (int)m_scene_type;
                    pIntParams[1] = (int)m_mouse_move_pt.x;
                    pIntParams[2] = (int)m_mouse_move_pt.y;

                    pRetInts[0] = 0;
                    PostVisionDLL_get_gray_value_of_a_point(pIntParams, pRetInts);

                    double dbZoomRatio = (double)m_nControlWidth / dbFOVWidth;

                    if (1 == pRetInts[0] && pRetInts[1] >= 0)
                    {
                        int nChannels = pRetInts[1];
                        bool bExceedImageBound = Convert.ToBoolean(pRetInts[2]);

                        if (true)
                        {
                            int nGrayValue1 = pRetInts[3];
                            int nGrayValue2 = pRetInts[4];
                            int nGrayValue3 = pRetInts[5];

                            string strInfo = "";

                            if (1 == nChannels)
                                strInfo = string.Format("[{0:0.},{1:0.}], RGB [{2},{3},{4}]   {5:0.}%", m_mouse_move_pt.x, m_mouse_move_pt.y,
                                    nGrayValue1, nGrayValue1, nGrayValue1, dbZoomRatio * 100);
                            else if (3 == nChannels || 4 == nChannels)
                                strInfo = string.Format("[{0:0.},{1:0.}], RGB [{2},{3},{4}]   {5:0.}%", m_mouse_move_pt.x, m_mouse_move_pt.y,
                                    nGrayValue1, nGrayValue2, nGrayValue3, dbZoomRatio * 100);

                            m_parent.StatusBarItem_ImageCrdInfo.Text = "图像坐标: " + strInfo;
                        }
                        else if (false)
                        {
                            int nGrayValue1 = pRetInts[3];
                            int nGrayValue2 = pRetInts[4];
                            int nGrayValue3 = pRetInts[5];

                            g.FillRectangle(brush1, 0, 0, 200, 30);

                            if (1 == nChannels)
                                g.DrawString(string.Format("[{0:0.},{1:0.}], RGB [{2},{3},{4}]", m_mouse_move_pt.x, m_mouse_move_pt.y,
                                    nGrayValue1, nGrayValue1, nGrayValue1), font, brush2, new PointF(0, 0));
                            else if (3 == nChannels || 4 == nChannels)
                                g.DrawString(string.Format("[{0:0.},{1:0.}], RGB [{2},{3},{4}]", m_mouse_move_pt.x, m_mouse_move_pt.y,
                                    nGrayValue1, nGrayValue2, nGrayValue3), font, brush2, new PointF(0, 0));
                        }
                        else
                        {
                            if (1 == nChannels)
                            {
                                int nGrayValue = pRetInts[3];

                                g.FillRectangle(brush1, m_nControlWidth - 155, m_nControlHeight - 30, 180, 30);

                                g.DrawString(string.Format("[{0:0.},{1:0.}], 灰度 {2}", m_mouse_move_pt.x, m_mouse_move_pt.y, nGrayValue), font, brush2,
                                    new PointF(m_nControlWidth - 155 + 5, m_nControlHeight - 30 + 6));
                            }
                            else if (3 == nChannels || 4 == nChannels)
                            {
                                int nGrayValue1 = pRetInts[3];
                                int nGrayValue2 = pRetInts[4];
                                int nGrayValue3 = pRetInts[5];

                                g.FillRectangle(brush1, m_nControlWidth - 200, m_nControlHeight - 30, 200, 30);

                                g.DrawString(string.Format("[{0:0.},{1:0.}], RGB [{2},{3},{4}]", m_mouse_move_pt.x, m_mouse_move_pt.y, nGrayValue1, nGrayValue2, nGrayValue3), font, brush2,
                                                                           new PointF(m_nControlWidth - 200 + 5, m_nControlHeight - 30 + 6));
                            }
                        }
                    }
                }

                ctrl_image.Source = GeneralUtilities.ChangeBitmapToImageSource(bmp);
            }
        }

        // 在bitmap上绘制矩形
        // nType为0代表基准模板，1代表模板搜索范围ROI，2代表模板，3代表识别到的模板位置，4代表测试寻找标定圆的矩形区域
        // 4代表显示测试寻找到的标定圆
        private void DrawRectToBitmap(ref Bitmap bmp, Point2d start_pt, Point2d end_pt, int nType)
        {
            Graphics g = Graphics.FromImage(bmp);
            System.Drawing.Pen red_pen = new System.Drawing.Pen(System.Drawing.Color.FromArgb(255, 0, 0), (float)2);
            System.Drawing.Pen green_pen = new System.Drawing.Pen(System.Drawing.Color.FromArgb(0, 255, 0), (float)2);
            System.Drawing.Pen yello_pen = new System.Drawing.Pen(System.Drawing.Color.FromArgb(255, 255, 0), (float)2);

            float left = Math.Min((float)start_pt.x, (float)end_pt.x);
            float top = Math.Min((float)start_pt.y, (float)end_pt.y);
            float right = Math.Max((float)start_pt.x, (float)end_pt.x);
            float bottom = Math.Max((float)start_pt.y, (float)end_pt.y);

            //Debugger.Log(0, null, string.Format("222222 m_select_start_pt = [{0:0.000},{1:0.000}], [{2:0.000},{3:0.000}]",
            //    m_select_start_pt.x, m_select_start_pt.y, m_select_end_pt.x, m_select_end_pt.y));

            PointF[] pts = new PointF[4];

            pts[0] = new PointF(left, top);
            pts[2] = new PointF(right, bottom);
            pts[1] = new PointF(pts[2].X, pts[0].Y);
            pts[3] = new PointF(pts[0].X, pts[2].Y);

            switch (nType)
            {
                case 0:
                    for (int n = 0; n < 4; n++)
                        g.DrawLine(yello_pen, pts[n], pts[(n + 1) % 4]);
                    break;
                case 1:
                    for (int n = 0; n < 4; n++)
                        g.DrawLine(red_pen, pts[n], pts[(n + 1) % 4]);
                    break;
            }
        }

        // 在bitmap上绘制矩形
        // nType为0代表基准模板，1代表芯片模板，10代表基准模板搜索范围ROI，11代表Tray盘边缘搜索区域
        private void DrawRectToBitmap(ref Bitmap bmp, SearchRect rect, Point2d FOV_lefttop, double bitmap_ratio_x, double bitmap_ratio_y,
            double dbZoomRatioX, double dbZoomRatioY, Point2d image_offset, int nType)
        {
            Graphics g = Graphics.FromImage(bmp);
            System.Drawing.Pen red_pen = new System.Drawing.Pen(System.Drawing.Color.FromArgb(255, 0, 0), (float)2);
            System.Drawing.Pen green_pen = new System.Drawing.Pen(System.Drawing.Color.FromArgb(0, 255, 0), (float)2);
            System.Drawing.Pen blue_pen = new System.Drawing.Pen(System.Drawing.Color.FromArgb(0, 0, 255), (float)2);
            System.Drawing.Pen yellow_pen = new System.Drawing.Pen(System.Drawing.Color.FromArgb(255, 255, 0), (float)2);
            System.Drawing.Pen purple_pen = new System.Drawing.Pen(System.Drawing.Color.FromArgb(135, 95, 255), (float)2);
            System.Drawing.Pen white_pen = new System.Drawing.Pen(System.Drawing.Color.FromArgb(255, 255, 255), (float)2);

            float left = 0;
            float top = 0;
            float right = 0;
            float bottom = 0;

            if (true == rect.m_bIsShaped)
            {
                left = Math.Min((float)rect.m_lefttop.x, (float)rect.m_rightbottom.x);
                top = Math.Min((float)rect.m_lefttop.y, (float)rect.m_rightbottom.y);
                right = Math.Max((float)rect.m_lefttop.x, (float)rect.m_rightbottom.x);
                bottom = Math.Max((float)rect.m_lefttop.y, (float)rect.m_rightbottom.y);
            }
            else if (true == rect.m_bIsSelecting)
            {
                left = Math.Min((float)rect.m_select_start_pt.x, (float)rect.m_select_end_pt.x);
                top = Math.Min((float)rect.m_select_start_pt.y, (float)rect.m_select_end_pt.y);
                right = Math.Max((float)rect.m_select_start_pt.x, (float)rect.m_select_end_pt.x);
                bottom = Math.Max((float)rect.m_select_start_pt.y, (float)rect.m_select_end_pt.y);
            }

            left += (float)image_offset.x;
            top += (float)image_offset.y;
            right += (float)image_offset.x;
            bottom += (float)image_offset.y;

            left = (float)((((double)left - FOV_lefttop.x) / bitmap_ratio_x) / dbZoomRatioX);
            top = (float)((((double)top - FOV_lefttop.y) / bitmap_ratio_y) / dbZoomRatioY);
            right = (float)((((double)right - FOV_lefttop.x) / bitmap_ratio_x) / dbZoomRatioX);
            bottom = (float)((((double)bottom - FOV_lefttop.y) / bitmap_ratio_y) / dbZoomRatioY);

            PointF[] pts = new PointF[4];

            pts[0] = new PointF(left, top);
            pts[2] = new PointF(right, bottom);
            pts[1] = new PointF(pts[2].X, pts[0].Y);
            pts[3] = new PointF(pts[0].X, pts[2].Y);

            switch (rect.m_rect_utility_type)
            {
                case RECT_UTILITY_TYPE.TEMPLATE:
                    for (int n = 0; n < 4; n++)
                        g.DrawLine(green_pen, pts[n], pts[(n + 1) % 4]);
                    break;

                case RECT_UTILITY_TYPE.DEFECT_AREA1:
                    for (int n = 0; n < 4; n++)
                        g.DrawLine(blue_pen, pts[n], pts[(n + 1) % 4]);
                    break;

                case RECT_UTILITY_TYPE.DEFECT_AREA1_AS_ROTATED:
                case RECT_UTILITY_TYPE.OCR_AREA1:
                case RECT_UTILITY_TYPE.PASTE_ROI_TO_IMAGE:
                    if (true)
                    {
                        PointF[] corner_pts = new PointF[4];
                        for (int n = 0; n < 4; n++)
                        {
                            corner_pts[n].X = (float)rect.m_rotated_rect_corners[n].x;
                            corner_pts[n].Y = (float)rect.m_rotated_rect_corners[n].y;

                            corner_pts[n].X += (float)image_offset.x;
                            corner_pts[n].Y += (float)image_offset.y;

                            corner_pts[n].X = (float)((((double)corner_pts[n].X - FOV_lefttop.x) / bitmap_ratio_x) / dbZoomRatioX);
                            corner_pts[n].Y = (float)((((double)corner_pts[n].Y - FOV_lefttop.y) / bitmap_ratio_y) / dbZoomRatioY);
                        }

                        for (int n = 0; n < 4; n++)
                            g.DrawLine(blue_pen, corner_pts[n], corner_pts[(n + 1) % 4]);
                    }
                    break;
            }
        }

        // 在bitmap上绘制Yolo对象
        private void DrawYoloObjectToBitmap(ref Bitmap bmp, YOLOObject yolo_objects, Point2d FOV_lefttop, double bitmap_ratio_x, double bitmap_ratio_y,
            double dbZoomRatioX, double dbZoomRatioY, Point2d image_offset)
        {
            Graphics g = Graphics.FromImage(bmp);

            float left = yolo_objects.m_lefttop.X;
            float top = yolo_objects.m_lefttop.Y;
            float right = yolo_objects.m_rightbottom.X;
            float bottom = yolo_objects.m_rightbottom.Y;

            left += (float)image_offset.x;
            top += (float)image_offset.y;
            right += (float)image_offset.x;
            bottom += (float)image_offset.y;

            left = (float)((((double)left - FOV_lefttop.x) / bitmap_ratio_x) / dbZoomRatioX);
            top = (float)((((double)top - FOV_lefttop.y) / bitmap_ratio_y) / dbZoomRatioY);
            right = (float)((((double)right - FOV_lefttop.x) / bitmap_ratio_x) / dbZoomRatioX);
            bottom = (float)((((double)bottom - FOV_lefttop.y) / bitmap_ratio_y) / dbZoomRatioY);

            PointF[] pts = new PointF[4];

            pts[0] = new PointF(left, top);
            pts[2] = new PointF(right, bottom);
            pts[1] = new PointF(pts[2].X, pts[0].Y);
            pts[3] = new PointF(pts[0].X, pts[2].Y);

            System.Drawing.Pen pen = new System.Drawing.Pen(m_colors_for_yolo_object[yolo_objects.m_nClass], (float)2);
            System.Drawing.Pen red_pen = new System.Drawing.Pen(System.Drawing.Color.FromArgb(255, 0, 0), (float)2);

            for (int n = 0; n < 4; n++)
                g.DrawLine(red_pen, pts[n], pts[(n + 1) % 4]);

            // 画出类别
            Font font = new Font("Arial", 16.8f / (float)dbZoomRatioX, System.Drawing.FontStyle.Regular);

            string strContent = string.Format("{0}", yolo_objects.m_strName);
            SizeF size = g.MeasureString(strContent, font);

            PointF center = new PointF((pts[0].X + pts[2].X) / 2, (pts[0].Y + pts[2].Y) / 2);

            // 判断center.Y是否小于m_nControlHeight / 2，如果是，则在下方显示，否则在上方显示
            if (center.Y < m_nControlHeight / 2)
            {
                pts[3].Y += size.Height;

                g.DrawString(strContent, font, new SolidBrush(m_colors_for_yolo_object[yolo_objects.m_nClass]), pts[3]);
            }
            else
            {
                pts[0].Y -= size.Height;

                g.DrawString(strContent, font, new SolidBrush(m_colors_for_yolo_object[yolo_objects.m_nClass]), pts[0]);
            }
        }

        // 在bitmap上绘制矩形，nType为0代表识别到的基准模板，1代表识别到的模板，2代表识别到的Tray盘边缘搜索区域
        private void DrawRectToBitmap(ref Bitmap bmp, TemplateRect rect, Point2d FOV_lefttop, double bitmap_ratio_x, double bitmap_ratio_y,
            double dbZoomRatioX, double dbZoomRatioY, Point2d image_offset, int nType)
        {
            Graphics g = Graphics.FromImage(bmp);

            System.Drawing.Pen green_pen = new System.Drawing.Pen(System.Drawing.Color.FromArgb(0, 255, 0), (float)2);
            System.Drawing.Pen purple_pen = new System.Drawing.Pen(System.Drawing.Color.FromArgb(135, 95, 255), (float)2);

            float left = (float)(((rect.m_lefttop.x - FOV_lefttop.x + image_offset.x) / bitmap_ratio_x) / dbZoomRatioX);
            float top = (float)(((rect.m_lefttop.y - FOV_lefttop.y + image_offset.y) / bitmap_ratio_y) / dbZoomRatioY);
            float right = (float)(((rect.m_rightbottom.x - FOV_lefttop.x + image_offset.x) / bitmap_ratio_x) / dbZoomRatioX);
            float bottom = (float)(((rect.m_rightbottom.y - FOV_lefttop.y + image_offset.y) / bitmap_ratio_y) / dbZoomRatioY);

            PointF[] pts = new PointF[4];

            pts[0] = new PointF(left, top);
            pts[2] = new PointF(right, bottom);
            pts[1] = new PointF(pts[2].X, pts[0].Y);
            pts[3] = new PointF(pts[0].X, pts[2].Y);

            switch (nType)
            {
                case 0:
                    for (int n = 0; n < 4; n++)
                        g.DrawLine(green_pen, pts[n], pts[(n + 1) % 4]);
                    break;
                case 1:
                    for (int n = 0; n < 4; n++)
                        g.DrawLine(green_pen, pts[n], pts[(n + 1) % 4]);
                    break;
                case 2:
                    for (int n = 0; n < 4; n++)
                        g.DrawLine(purple_pen, pts[n], pts[(n + 1) % 4]);
                    break;
            }
        }

        // 在bitmap上绘制矩形，nType为0代表识别到的基准模板，1代表识别到的模板，2代表识别到的Tray盘边缘搜索区域
        private void DrawRectToBitmap(ref Bitmap bmp, TemplateRect rect, Point2d FOV_lefttop, double bitmap_ratio_x, double bitmap_ratio_y,
            double dbZoomRatioX, double dbZoomRatioY, Point2d image_offset, bool bOKorNG)
        {
            Graphics g = Graphics.FromImage(bmp);

            System.Drawing.Pen green_pen = new System.Drawing.Pen(System.Drawing.Color.FromArgb(0, 255, 0), (float)2);
            System.Drawing.Pen red_pen = new System.Drawing.Pen(System.Drawing.Color.FromArgb(255, 0, 0), (float)2);

            float left = (float)(((rect.m_lefttop.x - FOV_lefttop.x + image_offset.x) / bitmap_ratio_x) / dbZoomRatioX);
            float top = (float)(((rect.m_lefttop.y - FOV_lefttop.y + image_offset.y) / bitmap_ratio_y) / dbZoomRatioY);
            float right = (float)(((rect.m_rightbottom.x - FOV_lefttop.x + image_offset.x) / bitmap_ratio_x) / dbZoomRatioX);
            float bottom = (float)(((rect.m_rightbottom.y - FOV_lefttop.y + image_offset.y) / bitmap_ratio_y) / dbZoomRatioY);

            PointF[] pts = new PointF[4];

            pts[0] = new PointF(left, top);
            pts[2] = new PointF(right, bottom);
            pts[1] = new PointF(pts[2].X, pts[0].Y);
            pts[3] = new PointF(pts[0].X, pts[2].Y);

            if (true == bOKorNG)
            {
                for (int n = 0; n < 4; n++)
                    g.DrawLine(green_pen, pts[n], pts[(n + 1) % 4]);
            }
            else
            {
                for (int n = 0; n < 4; n++)
                    g.DrawLine(red_pen, pts[n], pts[(n + 1) % 4]);
            }
        }

        // 在bitmap上绘制轮廓
        private void DrawContourToBitmap(ref Bitmap bmp, List<Point> list_contour_pts, Point contour_temp_pt, bool bContourFinished,
            Point2d FOV_lefttop, double bitmap_ratio_x, double bitmap_ratio_y, double dbZoomRatioX, double dbZoomRatioY, Point2d image_offset)
        {
            Graphics g = Graphics.FromImage(bmp);
            System.Drawing.Pen red_pen = new System.Drawing.Pen(System.Drawing.Color.FromArgb(255, 0, 0), (float)2);
            System.Drawing.Pen green_pen = new System.Drawing.Pen(System.Drawing.Color.FromArgb(0, 255, 0), (float)2);

            if (true == bContourFinished)
            {
                for (int n = 0; n < list_contour_pts.Count; n++)
                {
                    PointF pt1 = list_contour_pts[n];
                    PointF pt2 = list_contour_pts[(n + 1) % list_contour_pts.Count];

                    pt1.X = (float)(((pt1.X - FOV_lefttop.x + image_offset.x) / bitmap_ratio_x) / dbZoomRatioX);
                    pt1.Y = (float)(((pt1.Y - FOV_lefttop.y + image_offset.y) / bitmap_ratio_y) / dbZoomRatioY);
                    pt2.X = (float)(((pt2.X - FOV_lefttop.x + image_offset.x) / bitmap_ratio_x) / dbZoomRatioX);
                    pt2.Y = (float)(((pt2.Y - FOV_lefttop.y + image_offset.y) / bitmap_ratio_y) / dbZoomRatioY);

                    g.DrawLine(green_pen, pt1, pt2);
                }
            }
            else
            {
                for (int n = 1; n < list_contour_pts.Count; n++)
                {
                    PointF pt1 = list_contour_pts[n];
                    PointF pt2 = list_contour_pts[(n - 1)];

                    pt1.X = (float)(((pt1.X - FOV_lefttop.x + image_offset.x) / bitmap_ratio_x) / dbZoomRatioX);
                    pt1.Y = (float)(((pt1.Y - FOV_lefttop.y + image_offset.y) / bitmap_ratio_y) / dbZoomRatioY);
                    pt2.X = (float)(((pt2.X - FOV_lefttop.x + image_offset.x) / bitmap_ratio_x) / dbZoomRatioX);
                    pt2.Y = (float)(((pt2.Y - FOV_lefttop.y + image_offset.y) / bitmap_ratio_y) / dbZoomRatioY);

                    g.DrawLine(red_pen, pt1, pt2);
                }

                if (contour_temp_pt.X > 0)
                {
                    PointF pt1 = contour_temp_pt;
                    PointF pt2 = list_contour_pts[list_contour_pts.Count - 1];

                    pt1.X = (float)(((pt1.X - FOV_lefttop.x + image_offset.x) / bitmap_ratio_x) / dbZoomRatioX);
                    pt1.Y = (float)(((pt1.Y - FOV_lefttop.y + image_offset.y) / bitmap_ratio_y) / dbZoomRatioY);
                    pt2.X = (float)(((pt2.X - FOV_lefttop.x + image_offset.x) / bitmap_ratio_x) / dbZoomRatioX);
                    pt2.Y = (float)(((pt2.Y - FOV_lefttop.y + image_offset.y) / bitmap_ratio_y) / dbZoomRatioY);

                    g.DrawLine(green_pen, pt1, pt2);
                }
            }
        }

        // 刷新
        public void refresh(bool bShowSelectionRectOnly = false)
        {
            refresh_view(m_control_image, m_nControlWidth, m_nControlHeight, ref m_bitmap, m_parent.m_global,
                        0, m_dbZoomRatioX, m_dbZoomRatioY, m_dbPrevZoomRatioX, m_dbPrevZoomRatioY,
                        m_view_zoom_anchor, ref m_view_drag_pt, bShowSelectionRectOnly);
        }

        // 回调函数：刷新
        public void on_callback_refresh(object[] parameters)
        {
            bool bShowSelectionRectOnly = (bool)parameters[0];

            refresh_view(m_control_image, m_nControlWidth, m_nControlHeight, ref m_bitmap, m_parent.m_global,
                        0, m_dbZoomRatioX, m_dbZoomRatioY, m_dbPrevZoomRatioX, m_dbPrevZoomRatioY,
                        m_view_zoom_anchor, ref m_view_drag_pt, bShowSelectionRectOnly);
        }

        // 设置位图
        public void set_bitmap(Bitmap bmpOrigin, Bitmap bmpStretched)
        {
            m_origin_bitmap = bmpOrigin;
            m_stretched_bitmap = bmpStretched;
        }

        public int get_origin_image_width()
        {
            return m_nOriginImageWidth;
        }

        public int get_origin_image_height()
        {
            return m_nOriginImageHeight;
        }

        public void clear_all_search_rects()
        {
            for (int n = 0; n < m_template_rects.Length; n++)
            {
                if (null != m_template_rects[n])
                {
                    m_template_rects[n] = new SearchRect((RECT_UTILITY_TYPE)n);
                }
            }
        }

        public void start_selecting_starting_pt_for_point_to_point_distance(Point2d pt)
        {
            m_gauge_2D_starting_pt_for_point_to_point_distance = pt;
        }

        public void start_selecting_end_pt_for_point_to_point_distance(Point2d pt)
        {
            m_gauge_2D_end_pt_for_point_to_point_distance = pt;
        }

        public void start_selecting_angle_vertex(Point2d pt)
        {
            m_gauge_2D_angle_vertex = pt;
        }

        public void start_selecting_angle_end_pt1(Point2d pt)
        {
            m_gauge_2D_angle_end_pt1 = pt;
        }

        public void start_selecting_angle_end_pt2(Point2d pt)
        {
            m_gauge_2D_angle_end_pt2 = pt;
        }

        public void start_selecting_rect(RECT_UTILITY_TYPE rect_type)
        {
            // null all rects
            for (int n = 0; n <= (int)RECT_UTILITY_TYPE.PASTE_ROI_TO_IMAGE; n++)
            {
                //if (null != m_template_rects[n])
                {
                    m_template_rects[n] = new SearchRect((RECT_UTILITY_TYPE)n);
                }
            }

            m_template_rects[(int)rect_type].m_bIsSelecting = true;

            refresh(true);
        }

        public bool get_image_offset(ref Point2d offset)
        {
            double dbReadjustedImageWidth = (double)(m_nOriginImageWidth);
            double dbReadjustedImageHeight = (double)(m_nOriginImageHeight);

            double bitmap_ratio_x = 1;
            double bitmap_ratio_y = 1;

            offset = new Point2d(0, 0);

            if ((null != m_origin_bitmap) && (null != m_stretched_bitmap))
            {
                bitmap_ratio_x = (double)(m_nOriginImageWidth) / (double)(m_stretched_bitmap.Width);
                bitmap_ratio_y = (double)(m_nOriginImageHeight) / (double)(m_stretched_bitmap.Height);

                if (bitmap_ratio_x > bitmap_ratio_y)
                {
                    double temp = Math.Round(bitmap_ratio_x * (double)m_nControlHeight - (double)m_nOriginImageHeight);

                    if (((int)temp % 2) != 0)
                        temp += 1;

                    offset.y = temp / 2;
                }
                else
                {
                    double temp = Math.Round(bitmap_ratio_y * (double)m_nControlWidth - (double)m_nOriginImageWidth);

                    if (((int)temp % 2) != 0)
                        temp += 1;

                    offset.x = temp / 2;
                }

                return true;
            }
            else
                return false;
        }

        public void update_rotated_rect(SearchRect rect)
        {
            Point2d[] pts = new Point2d[4];
            double ratio = rect.m_rotated_rect_wh_ratio;
            double[] in_crds = new double[2];
            double[] out_crds = new double[2];
            in_crds[0] = (rect.m_select_start_pt.x - rect.m_select_end_pt.x) / ratio;
            in_crds[1] = (rect.m_select_start_pt.y - rect.m_select_end_pt.y) / ratio;

            //Debugger.Log(0, null, string.Format("222222 xy = [{0:0.},{1:0.}], rect.m_select_start_pt [{2:0.},{3:0.}], rect.m_select_end_pt [{4:0.},{5:0.}]", 
            //    in_crds[0], in_crds[1], rect.m_select_start_pt.x, rect.m_select_start_pt.y, rect.m_select_end_pt.x, rect.m_select_end_pt.y));

            rotate_crd(in_crds, out_crds, 90);
            rect.m_rotated_rect_corners[0].set(rect.m_select_end_pt.x + out_crds[0], rect.m_select_end_pt.y + out_crds[1]);

            rotate_crd(in_crds, out_crds, -90);
            rect.m_rotated_rect_corners[1].set(rect.m_select_end_pt.x + out_crds[0], rect.m_select_end_pt.y + out_crds[1]);

            in_crds[0] = (rect.m_select_end_pt.x - rect.m_select_start_pt.x) / ratio;
            in_crds[1] = (rect.m_select_end_pt.y - rect.m_select_start_pt.y) / ratio;

            rotate_crd(in_crds, out_crds, 90);
            rect.m_rotated_rect_corners[2].set(rect.m_select_start_pt.x + out_crds[0], rect.m_select_start_pt.y + out_crds[1]);

            rotate_crd(in_crds, out_crds, -90);
            rect.m_rotated_rect_corners[3].set(rect.m_select_start_pt.x + out_crds[0], rect.m_select_start_pt.y + out_crds[1]);
        }

        // 裁剪搜索框，使之不超出实际图像边界
        private void trim_rect(ref SearchRect rect)
        {
            double dbReadjustedImageWidth = (double)(m_nOriginImageWidth);
            double dbReadjustedImageHeight = (double)(m_nOriginImageHeight);

            double dbLeft = 0;
            double dbTop = 0;
            double dbRight = dbReadjustedImageWidth;
            double dbBottom = dbReadjustedImageHeight;

            if (false)
            {
                double bitmap_ratio_x = 1;
                double bitmap_ratio_y = 1;
                if ((null != m_origin_bitmap) && (null != m_stretched_bitmap))
                {
                    bitmap_ratio_x = (double)(m_nOriginImageWidth) / (double)(m_stretched_bitmap.Width);
                    bitmap_ratio_y = (double)(m_nOriginImageHeight) / (double)(m_stretched_bitmap.Height);

                    if (bitmap_ratio_x > bitmap_ratio_y)
                    {
                        double temp = Math.Round(bitmap_ratio_x * (double)m_nControlHeight - (double)m_nOriginImageHeight);

                        if (((int)temp % 2) != 0)
                            temp += 1;

                        dbReadjustedImageHeight = (double)(m_nOriginImageHeight) + temp;

                        bitmap_ratio_y = dbReadjustedImageHeight / (double)(m_stretched_bitmap.Height);

                        dbTop += temp / 2;
                        dbBottom += temp / 2;
                    }
                    else
                    {
                        double temp = Math.Round(bitmap_ratio_y * (double)m_nControlWidth - (double)m_nOriginImageWidth);

                        if (((int)temp % 2) != 0)
                            temp += 1;

                        dbReadjustedImageWidth = (double)(m_nOriginImageWidth) + temp;

                        bitmap_ratio_x = dbReadjustedImageWidth / (double)(m_stretched_bitmap.Width);

                        dbLeft += temp / 2;
                        dbRight += temp / 2;
                    }
                }
            }

            if (rect.m_lefttop.x < dbLeft)
                rect.m_lefttop.x = dbLeft;
            if (rect.m_lefttop.y < dbTop)
                rect.m_lefttop.y = dbTop;
            if (rect.m_rightbottom.x >= dbRight)
                rect.m_rightbottom.x = dbRight - 1;
            if (rect.m_rightbottom.y >= dbBottom)
                rect.m_rightbottom.y = dbBottom - 1;

            //Debugger.Log(0, null, string.Format("222222 trim_rect() dbLeft [{0:0.},{1:0.}], [{2:0.},{3:0.}]", dbLeft, dbTop, dbRight, dbBottom));
        }

        // 更新图像控件的bitmap（执行速度较慢）
        public void UpdateViewWithBitmap(Grid grid_holder, Bitmap bmpOrigin, bool bRenderWithSingleColor = false)
        {
            if ((null == bmpOrigin) || (true == bRenderWithSingleColor))
            {
                Bitmap bmp = GeneralUtilities.generate_single_color_bitmap(this.m_nControlWidth,
                       this.m_nControlHeight, System.Drawing.Color.FromArgb(255, 255, 255));

                this.set_bitmap(bmp, bmp);
            }
            else
            {
                Bitmap bmpSceneStretched = new Bitmap((int)(grid_holder.ActualWidth), (int)(grid_holder.ActualHeight));

                using (Graphics graphics = Graphics.FromImage(bmpSceneStretched))
                {
                    graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighSpeed;

                    graphics.DrawImage(bmpOrigin,
                        new System.Drawing.Rectangle(0, 0, bmpSceneStretched.Width, bmpSceneStretched.Height),
                        new System.Drawing.Rectangle(0, 0, bmpOrigin.Width, bmpOrigin.Height), GraphicsUnit.Pixel);
                }

                this.set_bitmap(bmpOrigin, bmpSceneStretched);
            }
        }

        // 鼠标事件：鼠标滚轮在画板上滚动
        public override void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (true == m_bIsMenuShown)
                return;
            if (0 == m_nOriginImageWidth)
                return;

            double dbReadjustedImageWidth = (double)(m_nOriginImageWidth);
            double dbReadjustedImageHeight = (double)(m_nOriginImageHeight);

            double bitmap_ratio_x = 1;
            double bitmap_ratio_y = 1;
            if ((null != m_origin_bitmap) && (null != m_stretched_bitmap))
            {
                bitmap_ratio_x = (double)(m_nOriginImageWidth) / (double)(m_stretched_bitmap.Width);
                bitmap_ratio_y = (double)(m_nOriginImageHeight) / (double)(m_stretched_bitmap.Height);

                if (bitmap_ratio_x > bitmap_ratio_y)
                {
                    double temp = Math.Round(bitmap_ratio_x * (double)m_nControlHeight - (double)m_nOriginImageHeight);

                    if (((int)temp % 2) != 0)
                        temp += 1;

                    dbReadjustedImageHeight = (double)(m_nOriginImageHeight) + temp;

                    bitmap_ratio_y = dbReadjustedImageHeight / (double)(m_stretched_bitmap.Height);
                }
                else
                {
                    double temp = Math.Round(bitmap_ratio_y * (double)m_nControlWidth - (double)m_nOriginImageWidth);

                    if (((int)temp % 2) != 0)
                        temp += 1;

                    dbReadjustedImageWidth = (double)(m_nOriginImageWidth) + temp;

                    bitmap_ratio_x = dbReadjustedImageWidth / (double)(m_stretched_bitmap.Width);
                }
            }

            //m_view_zoom_anchor = new Point2d(m_nSourceImgWidth / m_dbZoomRatioX, m_nSourceImgHeight / m_dbZoomRatioY);
            m_view_zoom_anchor = new Point2d(e.GetPosition(m_control_image).X, e.GetPosition(m_control_image).Y);
            m_view_zoom_anchor.x *= bitmap_ratio_x;
            m_view_zoom_anchor.y *= bitmap_ratio_y;
            m_view_zoom_anchor.x *= m_dbZoomRatioX;
            m_view_zoom_anchor.y *= m_dbZoomRatioY;

            // 旋转矩形的处理
            if (true)
            {
                bool bExit = false;

                // 模板区域的处理
                for (int n = 0; n < m_template_rects.Length; n++)
                {
                    if (null != m_template_rects[n])
                    {
                        if (true == m_template_rects[n].m_bIsSelecting)
                        {
                            switch (m_template_rects[n].m_rect_utility_type)
                            {
                                case RECT_UTILITY_TYPE.DEFECT_AREA1_AS_ROTATED:
                                case RECT_UTILITY_TYPE.OCR_AREA1:
                                case RECT_UTILITY_TYPE.PASTE_ROI_TO_IMAGE:
                                    if (m_template_rects[n].m_bRotatedRectIsReady == true)
                                    {
                                        bExit = true;

                                        double ratio = m_template_rects[n].m_rotated_rect_wh_ratio;
                                        if (e.Delta < 0)
                                            ratio *= 1.2;
                                        else
                                            ratio /= 1.2;
                                        if (ratio > 15) ratio = 15;
                                        if (ratio < 0.2) ratio = 0.2;
                                        m_template_rects[n].m_rotated_rect_wh_ratio = ratio;

                                        update_rotated_rect(m_template_rects[n]);
                                    }
                                    break;
                            }
                        }
                    }
                }

                if (true == bExit)
                {
                    refresh_view(m_control_image, m_nControlWidth, m_nControlHeight, ref m_bitmap, m_parent.m_global,
                                0, m_dbZoomRatioX, m_dbZoomRatioY, m_dbPrevZoomRatioX, m_dbPrevZoomRatioY, m_view_zoom_anchor,
                                ref m_view_drag_pt, true);

                    return;
                }
            }

            if (e.Delta > 0)
            {
                double dbRatioX = m_dbZoomRatioX * (1 - 0.3);
                double dbRatioY = m_dbZoomRatioY * (1 - 0.3);

                if (dbRatioX >= 0.01)
                {
                    m_dbPrevZoomRatioX = m_dbZoomRatioX;
                    m_dbPrevZoomRatioY = m_dbZoomRatioY;

                    m_dbZoomRatioX = dbRatioX;
                    m_dbZoomRatioY = dbRatioY;

                    //Debugger.Log(0, null, string.Format("222222 m_dbZoomRatioX [{0:0.000},{1:0.000}]", m_dbZoomRatioX, m_dbZoomRatioY));

                    refresh_view(m_control_image, m_nControlWidth, m_nControlHeight, ref m_bitmap, m_parent.m_global,
                        1, m_dbZoomRatioX, m_dbZoomRatioY, m_dbPrevZoomRatioX, m_dbPrevZoomRatioY, m_view_zoom_anchor, ref m_view_drag_pt);
                }
            }
            else if (e.Delta < 0)
            {
                double dbRatioX = m_dbZoomRatioX * (1 + 0.3);
                double dbRatioY = m_dbZoomRatioY * (1 + 0.3);

                //double dbMaxRatioX = (double)m_stretched_bitmap.Width / (double)m_nControlWidth;
                //double dbMaxRatioY = (double)m_stretched_bitmap.Height / (double)m_nControlHeight;
                double dbMaxRatioX = 1;
                double dbMaxRatioY = 1;

                if (dbRatioX > dbMaxRatioX)
                    dbRatioX = dbMaxRatioX;
                if (dbRatioY > dbMaxRatioY)
                    dbRatioY = dbMaxRatioY;

                m_dbPrevZoomRatioX = m_dbZoomRatioX;
                m_dbPrevZoomRatioY = m_dbZoomRatioY;

                m_dbZoomRatioX = dbRatioX;
                m_dbZoomRatioY = dbRatioY;

                //Debugger.Log(0, null, string.Format("222222 m_dbZoomRatioX [{0:0.000},{1:0.000}]", m_dbZoomRatioX, m_dbZoomRatioY));

                refresh_view(m_control_image, m_nControlWidth, m_nControlHeight, ref m_bitmap, m_parent.m_global,
                    1, m_dbZoomRatioX, m_dbZoomRatioY, m_dbPrevZoomRatioX, m_dbPrevZoomRatioY, m_view_zoom_anchor, ref m_view_drag_pt);
            }
        }

        // 鼠标事件：鼠标在画板上弹起
        public override void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            double dbReadjustedImageWidth = (double)(m_nOriginImageWidth);
            double dbReadjustedImageHeight = (double)(m_nOriginImageHeight);

            double image_offset_x = 0;
            double image_offset_y = 0;
            double bitmap_ratio_x = 1;
            double bitmap_ratio_y = 1;
            if ((null != m_origin_bitmap) && (null != m_stretched_bitmap))
            {
                bitmap_ratio_x = (double)(m_nOriginImageWidth) / (double)(m_stretched_bitmap.Width);
                bitmap_ratio_y = (double)(m_nOriginImageHeight) / (double)(m_stretched_bitmap.Height);

                if (bitmap_ratio_x > bitmap_ratio_y)
                {
                    double temp = Math.Round(bitmap_ratio_x * (double)m_nControlHeight - (double)m_nOriginImageHeight);

                    if (((int)temp % 2) != 0)
                        temp += 1;

                    dbReadjustedImageHeight = (double)(m_nOriginImageHeight) + temp;

                    bitmap_ratio_y = dbReadjustedImageHeight / (double)(m_stretched_bitmap.Height);

                    image_offset_y = -temp / 2;
                }
                else
                {
                    double temp = Math.Round(bitmap_ratio_y * (double)m_nControlWidth - (double)m_nOriginImageWidth);

                    if (((int)temp % 2) != 0)
                        temp += 1;

                    dbReadjustedImageWidth = (double)(m_nOriginImageWidth) + temp;

                    bitmap_ratio_x = dbReadjustedImageWidth / (double)(m_stretched_bitmap.Width);

                    image_offset_x = -temp / 2;
                }
            }

            for (int n = 0; n < m_template_rects.Length; n++)
            {
                if (null != m_template_rects[n])
                {
                    if (true == m_template_rects[n].m_bIsSelecting)
                    {
                        Point2d pt = new Point2d(e.GetPosition(m_control_image).X, e.GetPosition(m_control_image).Y);
                        if ((Math.Abs(pt.x - ((m_template_rects[n].m_select_start_pt.x - image_offset_x - m_dbPrevFOVLeft) / m_dbZoomRatioX) / bitmap_ratio_x) > 15)
                            || (Math.Abs(pt.y - ((m_template_rects[n].m_select_start_pt.y - image_offset_y - m_dbPrevFOVTop) / m_dbZoomRatioY) / bitmap_ratio_y) > 15))
                        {
                            m_template_rects[n].m_select_end_pt = pt;

                            m_template_rects[n].m_select_end_pt.x *= bitmap_ratio_x;
                            m_template_rects[n].m_select_end_pt.y *= bitmap_ratio_y;
                            m_template_rects[n].m_select_end_pt.x *= m_dbZoomRatioX;
                            m_template_rects[n].m_select_end_pt.y *= m_dbZoomRatioY;
                            m_template_rects[n].m_select_end_pt.x += m_dbPrevFOVLeft;
                            m_template_rects[n].m_select_end_pt.y += m_dbPrevFOVTop;
                            m_template_rects[n].m_select_end_pt.x += image_offset_x;
                            m_template_rects[n].m_select_end_pt.y += image_offset_y;

                            m_template_rects[n].m_select_end_pt.x = Math.Round(m_template_rects[n].m_select_end_pt.x);
                            m_template_rects[n].m_select_end_pt.y = Math.Round(m_template_rects[n].m_select_end_pt.y);

                            m_template_rects[n].m_lefttop.x = Math.Min(m_template_rects[n].m_select_start_pt.x, m_template_rects[n].m_select_end_pt.x);
                            m_template_rects[n].m_lefttop.y = Math.Min(m_template_rects[n].m_select_start_pt.y, m_template_rects[n].m_select_end_pt.y);
                            m_template_rects[n].m_rightbottom.x = Math.Max(m_template_rects[n].m_select_start_pt.x, m_template_rects[n].m_select_end_pt.x);
                            m_template_rects[n].m_rightbottom.y = Math.Max(m_template_rects[n].m_select_start_pt.y, m_template_rects[n].m_select_end_pt.y);
                            m_template_rects[n].m_bIsShaped = true;

                            trim_rect(ref m_template_rects[n]);

                            refresh_view(m_control_image, m_nControlWidth, m_nControlHeight, ref m_bitmap, m_parent.m_global,
                            0, m_dbZoomRatioX, m_dbZoomRatioY, m_dbPrevZoomRatioX, m_dbPrevZoomRatioY,
                                        m_view_zoom_anchor, ref m_view_drag_pt, true);

                            invoke_callback_by_rect_utility_type(m_template_rects[n].m_rect_utility_type, m_template_rects[n], m_scene_type);

                            m_nClickCounter = 0;

                            m_template_rects[n].m_bIsSelecting = false;
                        }
                    }
                }
            }
        }

        // 鼠标事件：鼠标在画板上按下
        public override void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            double dbReadjustedImageWidth = (double)(m_nOriginImageWidth);
            double dbReadjustedImageHeight = (double)(m_nOriginImageHeight);

            double image_offset_x = 0;
            double image_offset_y = 0;
            double bitmap_ratio_x = 1;
            double bitmap_ratio_y = 1;
            if ((null != m_origin_bitmap) && (null != m_stretched_bitmap))
            {
                bitmap_ratio_x = (double)(m_nOriginImageWidth) / (double)(m_stretched_bitmap.Width);
                bitmap_ratio_y = (double)(m_nOriginImageHeight) / (double)(m_stretched_bitmap.Height);

                if (bitmap_ratio_x > bitmap_ratio_y)
                {
                    double temp = Math.Round(bitmap_ratio_x * (double)m_nControlHeight - (double)m_nOriginImageHeight);

                    if (((int)temp % 2) != 0)
                        temp += 1;

                    dbReadjustedImageHeight = (double)(m_nOriginImageHeight) + temp;

                    bitmap_ratio_y = dbReadjustedImageHeight / (double)(m_stretched_bitmap.Height);

                    image_offset_y = -temp / 2;
                }
                else
                {
                    double temp = Math.Round(bitmap_ratio_y * (double)m_nControlWidth - (double)m_nOriginImageWidth);

                    if (((int)temp % 2) != 0)
                        temp += 1;

                    dbReadjustedImageWidth = (double)(m_nOriginImageWidth) + temp;

                    bitmap_ratio_x = dbReadjustedImageWidth / (double)(m_stretched_bitmap.Width);

                    image_offset_x = -temp / 2;
                }
            }

            bool bIsHandled = false;

            if (MouseButton.Left == e.ChangedButton)
            {
                // 模板区域的处理
                for (int n = 0; n < m_template_rects.Length; n++)
                {
                    if (null != m_template_rects[n])
                    {
                        if (true == m_template_rects[n].m_bIsSelecting)
                        {
                            bIsHandled = true;

                            if (0 == m_nClickCounter)
                            {
                                m_template_rects[n].m_select_start_pt = new Point2d(e.GetPosition(m_control_image).X, e.GetPosition(m_control_image).Y);

                                m_template_rects[n].m_select_start_pt.x *= bitmap_ratio_x;
                                m_template_rects[n].m_select_start_pt.y *= bitmap_ratio_y;
                                m_template_rects[n].m_select_start_pt.x *= m_dbZoomRatioX;
                                m_template_rects[n].m_select_start_pt.y *= m_dbZoomRatioY;
                                m_template_rects[n].m_select_start_pt.x += m_dbPrevFOVLeft;
                                m_template_rects[n].m_select_start_pt.y += m_dbPrevFOVTop;
                                m_template_rects[n].m_select_start_pt.x += image_offset_x;
                                m_template_rects[n].m_select_start_pt.y += image_offset_y;

                                m_template_rects[n].m_select_start_pt.x = Math.Round(m_template_rects[n].m_select_start_pt.x);
                                m_template_rects[n].m_select_start_pt.y = Math.Round(m_template_rects[n].m_select_start_pt.y);

                                m_nClickCounter = 1;
                            }
                            else if (1 == m_nClickCounter)
                            {
                                m_template_rects[n].m_select_end_pt = new Point2d(e.GetPosition(m_control_image).X, e.GetPosition(m_control_image).Y);

                                m_template_rects[n].m_select_end_pt.x *= bitmap_ratio_x;
                                m_template_rects[n].m_select_end_pt.y *= bitmap_ratio_y;
                                m_template_rects[n].m_select_end_pt.x *= m_dbZoomRatioX;
                                m_template_rects[n].m_select_end_pt.y *= m_dbZoomRatioY;
                                m_template_rects[n].m_select_end_pt.x += m_dbPrevFOVLeft;
                                m_template_rects[n].m_select_end_pt.y += m_dbPrevFOVTop;
                                m_template_rects[n].m_select_end_pt.x += image_offset_x;
                                m_template_rects[n].m_select_end_pt.y += image_offset_y;

                                m_template_rects[n].m_select_end_pt.x = Math.Round(m_template_rects[n].m_select_end_pt.x);
                                m_template_rects[n].m_select_end_pt.y = Math.Round(m_template_rects[n].m_select_end_pt.y);

                                m_template_rects[n].m_lefttop.x = Math.Min(m_template_rects[n].m_select_start_pt.x, m_template_rects[n].m_select_end_pt.x);
                                m_template_rects[n].m_lefttop.y = Math.Min(m_template_rects[n].m_select_start_pt.y, m_template_rects[n].m_select_end_pt.y);
                                m_template_rects[n].m_rightbottom.x = Math.Max(m_template_rects[n].m_select_start_pt.x, m_template_rects[n].m_select_end_pt.x);
                                m_template_rects[n].m_rightbottom.y = Math.Max(m_template_rects[n].m_select_start_pt.y, m_template_rects[n].m_select_end_pt.y);
                                m_template_rects[n].m_bIsShaped = true;

                                trim_rect(ref m_template_rects[n]);

                                refresh_view(m_control_image, m_nControlWidth, m_nControlHeight, ref m_bitmap, m_parent.m_global,
                                            0, m_dbZoomRatioX, m_dbZoomRatioY, m_dbPrevZoomRatioX, m_dbPrevZoomRatioY,
                                            m_view_zoom_anchor, ref m_view_drag_pt, true);

                                invoke_callback_by_rect_utility_type(m_template_rects[n].m_rect_utility_type, m_template_rects[n], m_scene_type);

                                m_template_rects[n].m_bIsSelecting = false;

                                m_nClickCounter = 0;
                            }
                        }
                    }
                }
            }

            if (false == bIsHandled)
            {
                if (MouseButton.Left == e.ChangedButton || MouseButton.Right == e.ChangedButton || MouseButton.Middle == e.ChangedButton)
                {
                    // 如果两次点击时间间隔小于设定时间，视为双击
                    if (m_nLastClickTime > 0)
                    {
                        int nTimeGap = GetTickCount() - m_nLastClickTime;

                        if (nTimeGap < 200)
                        {
                            show_whole_image();

                            return;
                        }
                    }
                    m_nLastClickTime = GetTickCount();

                    m_mouse_click_pt.x = e.GetPosition(m_control_image).X;
                    m_mouse_click_pt.y = e.GetPosition(m_control_image).Y;
                    m_mouse_click_pt.x *= bitmap_ratio_x * m_dbZoomRatioX;
                    m_mouse_click_pt.y *= bitmap_ratio_y * m_dbZoomRatioY;
                }
            }
        }

        // 鼠标事件：鼠标在画板上移动
        public override void OnMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            m_mouse_move_pt = new Point2d(e.GetPosition(m_control_image).X, e.GetPosition(m_control_image).Y);

            if (true == m_bIsMenuShown)
                return;

            double dbReadjustedImageWidth = (double)(m_nOriginImageWidth);
            double dbReadjustedImageHeight = (double)(m_nOriginImageHeight);

            double image_offset_x = 0;
            double image_offset_y = 0;
            double bitmap_ratio_x = 1;
            double bitmap_ratio_y = 1;
            if ((null != m_origin_bitmap) && (null != m_stretched_bitmap))
            {
                bitmap_ratio_x = (double)(m_nOriginImageWidth) / (double)(m_stretched_bitmap.Width);
                bitmap_ratio_y = (double)(m_nOriginImageHeight) / (double)(m_stretched_bitmap.Height);

                if (bitmap_ratio_x > bitmap_ratio_y)
                {
                    double temp = Math.Round(bitmap_ratio_x * (double)m_nControlHeight - (double)m_nOriginImageHeight);

                    if (((int)temp % 2) != 0)
                        temp += 1;

                    dbReadjustedImageHeight = (double)(m_nOriginImageHeight) + temp;

                    bitmap_ratio_y = dbReadjustedImageHeight / (double)(m_stretched_bitmap.Height);

                    m_mouse_move_pt.x *= bitmap_ratio_x;
                    m_mouse_move_pt.y *= bitmap_ratio_y;
                    m_mouse_move_pt.x *= m_dbZoomRatioX;
                    m_mouse_move_pt.y *= m_dbZoomRatioY;
                    m_mouse_move_pt.x += m_dbPrevFOVLeft;
                    m_mouse_move_pt.y += m_dbPrevFOVTop;

                    m_mouse_move_pt.y -= temp / 2;

                    image_offset_y = -temp / 2;

                    m_mouse_move_pt_on_image = m_mouse_move_pt;
                }
                else
                {
                    double temp = Math.Round(bitmap_ratio_y * (double)m_nControlWidth - (double)m_nOriginImageWidth);

                    if (((int)temp % 2) != 0)
                        temp += 1;

                    dbReadjustedImageWidth = (double)(m_nOriginImageWidth) + temp;

                    bitmap_ratio_x = dbReadjustedImageWidth / (double)(m_stretched_bitmap.Width);

                    m_mouse_move_pt.x *= bitmap_ratio_x;
                    m_mouse_move_pt.y *= bitmap_ratio_y;
                    m_mouse_move_pt.x *= m_dbZoomRatioX;
                    m_mouse_move_pt.y *= m_dbZoomRatioY;
                    m_mouse_move_pt.x += m_dbPrevFOVLeft;
                    m_mouse_move_pt.y += m_dbPrevFOVTop;

                    m_mouse_move_pt.x -= temp / 2;

                    image_offset_x = -temp / 2;

                    m_mouse_move_pt_on_image = m_mouse_move_pt;
                }
            }

            bool bIsHandled = false;

            for (int n = 0; n < m_template_rects.Length; n++)
            {
                if (null != m_template_rects[n])
                {
                    if (true == m_template_rects[n].m_bIsSelecting)
                    {
                        bIsHandled = true;

                        if ((1 == m_nClickCounter) && (m_template_rects[n].m_select_start_pt.x > 0.1))
                        {
                            m_template_rects[n].m_select_end_pt = new Point2d(e.GetPosition(m_control_image).X, e.GetPosition(m_control_image).Y);

                            m_template_rects[n].m_select_end_pt.x *= bitmap_ratio_x;
                            m_template_rects[n].m_select_end_pt.y *= bitmap_ratio_y;
                            m_template_rects[n].m_select_end_pt.x *= m_dbZoomRatioX;
                            m_template_rects[n].m_select_end_pt.y *= m_dbZoomRatioY;
                            m_template_rects[n].m_select_end_pt.x += m_dbPrevFOVLeft;
                            m_template_rects[n].m_select_end_pt.y += m_dbPrevFOVTop;
                            m_template_rects[n].m_select_end_pt.x += image_offset_x;
                            m_template_rects[n].m_select_end_pt.y += image_offset_y;

                            m_template_rects[n].m_select_end_pt.x = Math.Round(m_template_rects[n].m_select_end_pt.x);
                            m_template_rects[n].m_select_end_pt.y = Math.Round(m_template_rects[n].m_select_end_pt.y);

                            double dist = GeneralUtilities.get_distance(m_template_rects[n].m_select_start_pt, m_template_rects[n].m_select_end_pt);
                            if (dist > 5)
                            {
                                m_template_rects[n].m_bRotatedRectIsReady = true;
                            }

                            update_rotated_rect(m_template_rects[n]);

                            refresh_view(m_control_image, m_nControlWidth, m_nControlHeight, ref m_bitmap, m_parent.m_global,
                                0, m_dbZoomRatioX, m_dbZoomRatioY, m_dbPrevZoomRatioX, m_dbPrevZoomRatioY, m_view_zoom_anchor,
                                ref m_view_drag_pt, true);
                        }
                    }
                }
            }

            if (false == bIsHandled)
            {
                if (MouseButtonState.Pressed == e.LeftButton || MouseButtonState.Pressed == e.MiddleButton)
                {
                    if (null == m_origin_bitmap)
                        return;

                    Point2d current_pos = new Point2d(e.GetPosition(m_control_image).X, e.GetPosition(m_control_image).Y);
                    current_pos.x *= bitmap_ratio_x * m_dbZoomRatioX + 0;
                    current_pos.y *= bitmap_ratio_y * m_dbZoomRatioY + 0;

                    Point2d offset = new Point2d(current_pos.x - m_mouse_click_pt.x, current_pos.y - m_mouse_click_pt.y);

                    double dbFOVWidth = (double)(dbReadjustedImageWidth) * m_dbZoomRatioX;
                    double dbFOVHeight = (double)(dbReadjustedImageHeight) * m_dbZoomRatioY;
                    double dbFOVLeft = (((double)(dbReadjustedImageWidth) - dbFOVWidth) / 2) + m_view_drag_pt.x - offset.x;
                    double dbFOVTop = (((double)(dbReadjustedImageHeight) - dbFOVHeight) / 2) + m_view_drag_pt.y - offset.y;

                    if (dbFOVLeft < 0)
                    {
                        current_pos.x = (((double)dbReadjustedImageWidth - dbFOVWidth) / 2) + m_view_drag_pt.x + m_mouse_click_pt.x;
                    }
                    if (dbFOVTop < 0)
                    {
                        current_pos.y = (((double)dbReadjustedImageHeight - dbFOVHeight) / 2) + m_view_drag_pt.y + m_mouse_click_pt.y;
                    }
                    if ((dbFOVLeft + dbFOVWidth) >= dbReadjustedImageWidth)
                    {
                        current_pos.x = m_mouse_click_pt.x + (-(dbReadjustedImageWidth - dbFOVWidth - (((double)dbReadjustedImageWidth - dbFOVWidth) / 2 + m_view_drag_pt.x)));
                    }
                    if ((dbFOVTop + dbFOVHeight) >= dbReadjustedImageHeight)
                    {
                        current_pos.y = m_mouse_click_pt.y + (-(dbReadjustedImageHeight - dbFOVHeight - (((double)dbReadjustedImageHeight - dbFOVHeight) / 2 + m_view_drag_pt.y)));
                    }

                    offset = new Point2d(current_pos.x - m_mouse_click_pt.x, current_pos.y - m_mouse_click_pt.y);

                    m_view_drag_pt.x -= offset.x;
                    m_view_drag_pt.y -= offset.y;

                    //Debugger.Log(0, null, string.Format("222222 m_graph_drag = [{0},{1}]", m_graph_drag.x, m_graph_drag.y));

                    m_mouse_click_pt = current_pos;

                    refresh_view(m_control_image, m_nControlWidth, m_nControlHeight, ref m_bitmap, m_parent.m_global,
                        0, m_dbZoomRatioX, m_dbZoomRatioY, m_dbPrevZoomRatioX, m_dbPrevZoomRatioY, m_view_zoom_anchor,
                        ref m_view_drag_pt);
                }
                else
                {
                    refresh_view(m_control_image, m_nControlWidth, m_nControlHeight, ref m_bitmap, m_parent.m_global,
                        0, m_dbZoomRatioX, m_dbZoomRatioY, m_dbPrevZoomRatioX, m_dbPrevZoomRatioY, m_view_zoom_anchor,
                        ref m_view_drag_pt, true);
                }
            }
        }
    }
}
