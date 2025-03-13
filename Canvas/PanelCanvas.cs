using AutoScanFQCTest.DataModels;
using AutoScanFQCTest.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AutoScanFQCTest.Canvas
{
    public class PanelCanvas : BaseCanvas<CameraCanvas>
    {
        private int m_nPanelRows = 0;
        private int m_nPanelColumns = 0;

        private RecheckResult[] m_recheck_flags = null;

        public PanelCanvas(MainWindow parent, System.Windows.Controls.Image image, int nControlWidth, int nControlHeight,
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

        // 将图形数据绘制输出到 bitmap位图，nActionType 0为拖动，1为缩放
        public void refresh_view(System.Windows.Controls.Image ctrl_image,
            int nControlWidth, int nControlHeight, ref Bitmap bitmap_strip_edit_view, Global global_data, int nActionType,
            double dbZoomRatioX, double dbZoomRatioY, double dbPrevZoomRatioX, double dbPrevZoomRatioY,
            Point2d zoom_anchor, ref Point2d drag_pt, bool bShowSelectionRectOnly = false, bool bShowWholeImage = false)
        {
            if (null == m_origin_bitmap)
                return;

            if (null == bitmap_strip_edit_view || bitmap_strip_edit_view.Width != nControlWidth || bitmap_strip_edit_view.Height != nControlHeight)
                bitmap_strip_edit_view = new System.Drawing.Bitmap(nControlWidth, nControlHeight, System.Drawing.Imaging.PixelFormat.Format32bppRgb);

            // bitmap_strip_edit_view背景色设置为深灰色
            using (Graphics graphics = Graphics.FromImage(bitmap_strip_edit_view))
            {
                graphics.Clear(Color.FromArgb(64, 64, 64));
            }

            // 在bitmap_strip_edit_view图片中画一些五角星
            if (false)
            {
                if (m_nPanelRows > 0 && m_nPanelColumns > 0)
                {
                    // 设置五角星的行数和列数
                    int rows = m_nPanelRows;
                    int cols = m_nPanelColumns;

                    int nStarSize = (rows > cols ? (bitmap_strip_edit_view.Height - 50) / (rows + 0) : (bitmap_strip_edit_view.Width - 50) / (cols + 0));

                    nStarSize -= 5;
                    if (nStarSize < 3)
                        nStarSize = 3;

                    int nTopMargin = 35;
                    int nBottomMargin = 20;
                    int nLeftMargin = 50;
                    int nRightMargin = 20;

                    DrawStarsOnBitmap(bitmap_strip_edit_view, rows, cols, nStarSize, nTopMargin, nBottomMargin, nLeftMargin, nRightMargin, m_recheck_flags);
                }
            }

            // 在bitmap_strip_edit_view的右下角画一个小三角形
            if (true)
            {
                // 设置三角形的大小
                int triangleSize = 20;

                // 创建三角形的点
                Point p1 = new Point(bitmap_strip_edit_view.Width - triangleSize, bitmap_strip_edit_view.Height - triangleSize);
                Point p2 = new Point(bitmap_strip_edit_view.Width - triangleSize, bitmap_strip_edit_view.Height);
                Point p3 = new Point(bitmap_strip_edit_view.Width, bitmap_strip_edit_view.Height - triangleSize);

                DrawTriangleOnBitmap(bitmap_strip_edit_view, p1, p2, p3, triangleSize);
            }

            // 在bitmap_strip_edit_view的顶部最左边画一个红色矩形，后面加上文字NG，向右边再画一个绿色矩形，后面加上文字OK
            if (true)
            {
                Graphics graphics = Graphics.FromImage(bitmap_strip_edit_view);

                // 设置矩形的大小
                int rectWidth = 30;
                int rectHeight = 16;

                // 设置矩形的位置
                Point rectPt = new Point(20, 10);

                // 设置填充颜色
                SolidBrush brush = new SolidBrush(Color.Red);

                // 绘制填充的矩形
                graphics.FillRectangle(brush, rectPt.X, rectPt.Y, rectWidth, rectHeight);

                // 设置文字的颜色
                SolidBrush textBrush = new SolidBrush(Color.White);

                string strText = "NG";

                // 绘制文字
                graphics.DrawString(strText, new Font("Arial", 10), textBrush, rectPt.X + rectWidth + 5, rectPt.Y + 0);

                SizeF size = graphics.MeasureString(strText, new Font("Arial", 10));

                int position = rectPt.X + rectWidth + 5 + (int)size.Width + 10;

                // 设置矩形的位置
                rectPt = new Point(position, 10);

                // 设置填充颜色
                brush = new SolidBrush(Color.FromArgb(0, 192, 0));

                // 绘制填充的矩形
                graphics.FillRectangle(brush, rectPt.X, rectPt.Y, rectWidth, rectHeight);

                strText = "OK";

                // 绘制文字
                graphics.DrawString(strText, new Font("Arial", 10), textBrush, rectPt.X + rectWidth + 5, rectPt.Y + 0);
            }

            ctrl_image.Source = GeneralUtilities.ChangeBitmapToImageSource(bitmap_strip_edit_view);
        }

        public void DrawTriangleOnBitmap(Bitmap bitmap, Point pt1, Point pt2, Point pt3, int triangleSize)
        {
            // 获取Graphics对象用于绘图
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                // 设置绘图的质量
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                // 创建一个路径并添加三角形
                using (System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath())
                {
                    path.AddPolygon(new Point[] { pt1, pt2, pt3 });

                    // 设置填充颜色
                    SolidBrush brush = new SolidBrush(Color.White);

                    // 绘制填充的三角形
                    graphics.FillPath(brush, path);
                }
            }
        }

        // 在位图上绘制五角星
        public void DrawStarsOnBitmap(Bitmap bitmap, int rows, int cols, int nStarSize,
            int nTopMargin, int nBottomMargin, int nLeftMargin, int nRightMargin, RecheckResult[] check_results)
        {
            int bitmapWidth = bitmap.Width;
            int bitmapHeight = bitmap.Height;

            // 计算可用空间，减去边距
            int availableWidth = bitmapWidth - cols - 1;
            int availableHeight = bitmapHeight - rows - 1;

            // 计算间距和五角星的尺寸
            //int spacing = Math.Min((availableWidth - nStarSize * cols) / cols, (availableHeight - nStarSize * rows) / rows);
            float spacingX = (availableWidth - nStarSize * cols - (nLeftMargin + nRightMargin)) / cols;
            float spacingY = (availableHeight - nStarSize * rows - (nTopMargin + nBottomMargin)) / rows;

            // 获取Graphics对象用于绘图
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                // 设置绘图的质量
                graphics.SmoothingMode = SmoothingMode.AntiAlias;

                // 设置五角星的颜色
                Color starColor = Color.Yellow;

                SolidBrush[] brushes = new SolidBrush[2];
                brushes[0] = new SolidBrush(Color.FromArgb(0, 192, 0));
                brushes[1] = new SolidBrush(Color.Yellow);

                string[] strings = new string[rows];

                for (float row = 0; row < rows; row++)
                {
                    for (float col = 0; col < cols; col++)
                    {
                        // 计算每个五角星的起始位置
                        float x = (col + 0.5f) * spacingX + col * nStarSize + nLeftMargin;
                        float y = (row + 0.5f) * spacingY + row * nStarSize + nTopMargin;

                        PointF center = new PointF(x + nStarSize / 2, y + nStarSize / 2);

                        // 画轮廓
                        if (true)
                        {
                            // 轮廓点
                            PointF[] points = new PointF[4];

                            // 计算轮廓的点
                            points[0].X = center.X - nStarSize / 2;
                            points[0].Y = center.Y - nStarSize / 2;
                            points[1].X = points[0].X;
                            points[1].Y = center.Y + nStarSize / 2;
                            points[2].X = center.X + nStarSize / 2;
                            points[2].Y = points[1].Y;
                            points[3].X = points[2].X;
                            points[3].Y = points[0].Y;

                            Pen pen = new Pen(Color.Green, 2);

                            RecheckResult result = check_results != null ? check_results[(int)row * cols + (int)col] : RecheckResult.NotChecked;

                            switch (result)
                            {
                                case RecheckResult.NotChecked:
                                    pen = new Pen(Color.Yellow, 2);
                                    break;
                                case RecheckResult.OK:
                                    pen = new Pen(Color.FromArgb(0, 192, 0), 2);
                                    break;
                                case RecheckResult.NG:
                                    pen = new Pen(Color.Red, 2);
                                    break;
                                case RecheckResult.DoNotNeedRecheck:
                                    pen = new Pen(Color.MediumSeaGreen, 2);
                                    break;
                                default:
                                    pen = new Pen(Color.Green, 2);
                                    break;
                            }

                            // 绘制轮廓
                            for (int i = 0; i < points.Length - 1; i++)
                            {
                                graphics.DrawLine(pen, points[i], points[(i + 1)]);
                            }
                        }

                        // 画十字，以xy为中心
                        if (false)
                        {
                            // 画横线
                            graphics.DrawLine(new Pen(Color.White, 1), center.X - nStarSize / 4, center.Y, center.X + nStarSize / 4, center.Y);

                            // 画竖线
                            graphics.DrawLine(new Pen(Color.White, 1), center.X, center.Y - nStarSize / 4, center.X, center.Y + nStarSize / 4);
                        }
                    }

                    strings[(int)row] = ((int)row + 1).ToString() + "-1";

                    SizeF size = graphics.MeasureString(strings[(int)row], new Font("Arial", 12));

                    // 绘制文字
                    graphics.DrawString(strings[(int)row], new Font("Arial", 12), new SolidBrush(Color.White), (nLeftMargin / 2) - (size.Width / 2),
                        (int)(nTopMargin + row * nStarSize + (row + 0.5f) * spacingY + (nStarSize / 2) - (size.Height / 2) + 2));
                }
            }
        }

        // 创建五角星的路径
        private static GraphicsPath CreateStarPath(float radius, float centerX, float centerY)
        {
            // 创建一个路径来表示五角星
            GraphicsPath path = new GraphicsPath();

            // 定义五角星的角度
            double startAngle = -Math.PI / 2;
            double deltaAngle = Math.PI / 5;

            // 计算五角星的点
            for (int i = 0; i < 5; i++)
            {
                path.AddLine(
                    PolarToCartesian(radius, startAngle + i * 2 * deltaAngle, centerX, centerY),
                    PolarToCartesian(radius, startAngle + (i * 2 + 1) * deltaAngle, centerX, centerY));
            }

            path.CloseFigure();
            return path;
        }

        // 将极坐标转换为笛卡尔坐标
        private static PointF PolarToCartesian(float radius, double angle, float centerX, float centerY)
        {
            return new PointF(
                centerX + radius * (float)Math.Cos(angle),
                centerY + radius * (float)Math.Sin(angle)
            );
        }

        // 设置画板的行数和列数
        public void set_panel_rows_and_columns(int nPanelRows, int nPanelColumns)
        {
            m_nPanelRows = nPanelRows;
            m_nPanelColumns = nPanelColumns;
        }

        // 拷贝检查结果
        public void clone_check_results(RecheckResult[] check_results)
        {
            m_recheck_flags = new RecheckResult[check_results.Length];
            for (int i = 0; i < check_results.Length; i++)
            {
                m_recheck_flags[i] = check_results[i];
            }
        }

        // 设置检查结果
        public void set_check_result(int nIndex, RecheckResult result)
        {
            if (nIndex >= 0 && nIndex < m_recheck_flags.Length)
            {
                m_recheck_flags[nIndex] = result;
            }
        }

        // 鼠标事件：鼠标滚轮在画板上滚动
        public override void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {

        }

        // 鼠标事件：鼠标在画板上按下
        public override void OnMouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        // 鼠标事件：鼠标在画板上移动
        public override void OnMouseMove(object sender, MouseEventArgs e)
        {

        }

        // 鼠标事件：鼠标在画板上抬起
        public override void OnMouseUp(object sender, MouseButtonEventArgs e)
        {

        }
    }
}
