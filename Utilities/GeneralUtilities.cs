using AutoScanFQCTest.Canvas;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Resources;
using System.Windows;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;

namespace AutoScanFQCTest.Utilities
{
    public class GeneralUtilities
    {
        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        public static ImageSource ChangeBitmapToImageSource(Bitmap bitmap)
        {
            IntPtr hBitmap = bitmap.GetHbitmap();
            ImageSource wpfBitmap = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                hBitmap,
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            if (!DeleteObject(hBitmap))
            {
                throw new System.ComponentModel.Win32Exception();
            }
            return wpfBitmap;
        }

        public static Stream GetCursorFromICO(Uri uri, byte hotspotx, byte hotspoty)
        {
            StreamResourceInfo sri = System.Windows.Application.GetResourceStream(uri);

            Stream s = sri.Stream;

            byte[] buffer = new byte[s.Length];

            s.Read(buffer, 0, (int)s.Length);

            MemoryStream ms = new MemoryStream();

            buffer[2] = 2; // change to CUR file type
            buffer[10] = hotspotx;
            buffer[12] = hotspoty;

            ms.Write(buffer, 0, (int)s.Length);

            ms.Position = 0;

            s.Close();
            s.Dispose();

            return ms;
        }

        public static double get_distance(Point2d pt1, Point2d pt2)
        {
            return Math.Sqrt((pt1.x - pt2.x) * (pt1.x - pt2.x) + (pt1.y - pt2.y) * (pt1.y - pt2.y));
        }
        public static double get_distance(Point2d pt1, Point2d pt2, double pixelmm_x, double pixelmm_y)
        {
            return Math.Sqrt(Math.Pow(pt1.x - pt2.x, 2) * Math.Pow(pixelmm_x, 2) + Math.Pow(pt1.y - pt2.y, 2) * Math.Pow(pixelmm_y, 2));
        }
        public static double get_distance(Point2i pt1, Point2i pt2)
        {
            return Math.Sqrt((pt1.x - pt2.x) * (pt1.x - pt2.x) + (pt1.y - pt2.y) * (pt1.y - pt2.y));
        }
        public static double get_distance(System.Drawing.Point pt1, System.Drawing.Point pt2)
        {
            return Math.Sqrt((pt1.X - pt2.X) * (pt1.X - pt2.X) + (pt1.Y - pt2.Y) * (pt1.Y - pt2.Y));
        }

        public static bool get_angle(Point2d start_pt, Point2d end_pt, ref double angle)
        {
            Point2d start = start_pt;
            Point2d end = end_pt;

            double kp_θ0 = 0;
            double x0 = end.x - start.x;
            double y0 = end.y - start.y;
            if ((0 == x0) && (y0 > 0))
                kp_θ0 = 90;
            else if ((0 == x0) && (y0 < 0))
                kp_θ0 = 270;
            else if ((0 == x0) && (0 == y0))
            {
                return false;
            }
            else if ((0 == y0) && (x0 > 0))
                kp_θ0 = 0;
            else if ((0 == y0) && (x0 < 0))
                kp_θ0 = 0 + 180;
            else
                kp_θ0 = (Math.Atan(y0 / x0) * 180.0) / 3.1415926535;

            if ((x0 > 0) && (y0 < 0))
                kp_θ0 = kp_θ0 + 360;
            else if ((x0 < 0) && (y0 > 0))
                kp_θ0 = kp_θ0 + 180;
            else if ((x0 < 0) && (y0 < 0))
                kp_θ0 = kp_θ0 + 180;

            angle = kp_θ0;

            //sprintf(msg + 7, "θ = %0.0f, [%0.0f,%0.0f]", kp_θ0, y0, x0);
            //OutputDebugStringA(msg);

            return true;
        }

        // 利用叉乘，判断给定坐标点是否位于凸多边形之内
        public static bool is_point_inside_convex_polygon(Point2d input_pt, Point2d[] polygon)
        {
            if (polygon.Length < 3)
                return false;

            Point2d center = new Point2d(0, 0);

            for (int n = 0; n < polygon.Length; n++)
            {
                center.x += polygon[n].x;
                center.y += polygon[n].y;
            }
            center.x /= (double)(polygon.Length);
            center.y /= (double)(polygon.Length);

            for (int n = 0; n < polygon.Length; n++)
            {
                Point2d pt1 = polygon[n];
                Point2d pt2 = polygon[(n + 1) % polygon.Length];

                double cross_multiply1 = (pt1.x - center.x) * (pt2.y - center.y) - (pt2.x - center.x) * (pt1.y - center.y);
                double cross_multiply2 = (pt1.x - input_pt.x) * (pt2.y - input_pt.y) - (pt2.x - input_pt.x) * (pt1.y - input_pt.y);

                if ((cross_multiply1 * cross_multiply2) < 0)
                    return false;
            }

            return true;
        }

        // 旋转坐标，旋转原点为[0,0]
        public static bool rotate_crd(Point2d old_crd, ref Point2d new_crd, double rotate_angle)
        {
            double dist = Math.Sqrt(old_crd.x * old_crd.x + old_crd.y * old_crd.y);
            double old_angle = 0;

            get_angle(new Point2d(0, 0), old_crd, ref old_angle);

            double angle_sum = old_angle + rotate_angle;
            double rad_angle = ((angle_sum) * 3.1415926535) / 180.0;

            new_crd.x = dist * Math.Cos(rad_angle);
            new_crd.y = dist * Math.Sin(rad_angle);

            return true;
        }

        // 获取点集左上角点及其索引号
        public static Point2d get_point_collection_lefttop_corner_pt(List<Point2d> vec_pts, ref int nPointIndex)
        {
            Point2d lefttop = new Point2d(100000000, 100000000);

            for (int n = 0; n < vec_pts.Count; n++)
            {
                if (vec_pts[n].x < lefttop.x)
                    lefttop.x = vec_pts[n].x;
                if (vec_pts[n].y < lefttop.y)
                    lefttop.y = vec_pts[n].y;
            }

            double dbMinDistance = 100000000;
            int nMinDistanceIdx = 0;
            Point2d corner_pt = new Point2d(0, 0);
            for (int n = 0; n < vec_pts.Count; n++)
            {
                double dist = GeneralUtilities.get_distance(lefttop, vec_pts[n]);
                if (dist < dbMinDistance)
                {
                    dbMinDistance = dist;
                    nMinDistanceIdx = n;
                    corner_pt = vec_pts[n];
                }
            }

            nPointIndex = nMinDistanceIdx;

            return corner_pt;
        }

        // 把 Image 转成 bytes
        static public byte[] convert_bitmap_to_bytes(Bitmap bmp, ref int nStride)
        {
            Rectangle rect = new Rectangle(new System.Drawing.Point(0, 0), bmp.Size);
            BitmapData bmp_data = bmp.LockBits(rect, ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            byte[] buf = new byte[bmp_data.Stride * bmp_data.Height];

            System.Runtime.InteropServices.Marshal.Copy(bmp_data.Scan0, buf, 0, buf.Length);

            bmp.UnlockBits(bmp_data);

            nStride = bmp_data.Stride;

            return buf;
        }

        static public List<string> convert_images_to_base64(List<string> imgPath)
        {
            var b64s = new List<string>();
            for (int i = 0; i < imgPath.Count; i++)
            {
                if (String.IsNullOrEmpty(imgPath[i]))
                {
                    continue;
                }

                var bytes = File.ReadAllBytes(imgPath[i]);
                b64s.Add(Convert.ToBase64String(bytes));
            }

            return b64s;
        }

        // 非独占方式读取一个图片文件
        static public Bitmap read_bitmap_file(string strFileName)
        {
            Bitmap bmp = null;

            try
            {
                //using (FileStream stream = new FileStream(strFileName, FileMode.Open, FileAccess.Read))
                //{
                //    bmp = (Bitmap)System.Drawing.Image.FromStream(stream);
                //}

                FileStream fs = new FileStream(strFileName, FileMode.Open, FileAccess.Read);
                int byteLength = (int)fs.Length;
                byte[] fileBytes = new byte[byteLength];
                fs.Read(fileBytes, 0, byteLength);
                fs.Close();

                bmp = (Bitmap)(System.Drawing.Image.FromStream(new MemoryStream(fileBytes)));

                return bmp;
            }
            catch (Exception ex)
            {
                Debugger.Log(0, null, string.Format("222222 read_bitmap_file() 读取图片文件 {0} 异常：{1}", strFileName, ex.Message));
            }

            return null;
        }

        // 生成一张纯色图片
        public static Bitmap generate_single_color_bitmap(int nWidth, int nHeight, System.Drawing.Color color)
        {
            Bitmap bmp = new Bitmap(nWidth, nHeight);
            Graphics g = Graphics.FromImage(bmp);
            SolidBrush b = new SolidBrush(color);

            g.FillRectangle(b, 0, 0, bmp.Width, bmp.Height);

            return bmp;
        }

        // 以迭代遍历的方式，删除指定目录下所有子目录和文件
        public static void RemoveAllFilesAndDirectoriesUnderParentDirectory(string strParentDirectory)
        {
            try
            {
                foreach (var file in Directory.GetFiles(strParentDirectory))
                {
                    File.SetAttributes(file, FileAttributes.Normal);
                    File.Delete(file);
                }

                foreach (var dir in Directory.GetDirectories(strParentDirectory))
                {
                    RemoveAllFilesAndDirectoriesUnderParentDirectory(dir);
                }

                Directory.Delete(strParentDirectory, false);
            }
            catch (Exception ex)
            {
                Debugger.Log(0, null, string.Format("222222 RemoveAllFilesAndDirectoriesUnderParentDirectory() 异常：{0}", ex.Message));
            }
        }

        // 遍历目录中的子目录，找到以纯数字命名的子目录，并获取其中的图片文件的绝对路径
        public static List<(string imagePath, int dirNumber)> GetImagePathsFromNumericSubdirectories(string parentDirectory)
        {
            List<(string imagePath, int dirNumber)> imagePaths = new List<(string imagePath, int dirNumber)>();
            Regex numericRegex = new Regex("^[0-9]+$");

            // 获取父目录下的所有子目录
            string[] subdirectories = Directory.GetDirectories(parentDirectory);

            foreach (string subdir in subdirectories)
            {
                DirectoryInfo dirInfo = new DirectoryInfo(subdir);

                // 检查子目录名称是否为纯数字
                if (numericRegex.IsMatch(dirInfo.Name))
                {
                    int dirNumber = int.Parse(dirInfo.Name);

                    // 获取子目录中的所有图片文件（支持jpg, jpeg, png, gif, bmp等格式）
                    string[] images = Directory.GetFiles(subdir, "*.*", SearchOption.TopDirectoryOnly)
                        .Where(file => file.ToLower().EndsWith(".jpg") || file.ToLower().EndsWith(".jpeg") ||
                                        file.ToLower().EndsWith(".png") || file.ToLower().EndsWith(".gif") ||
                                        file.ToLower().EndsWith(".bmp")).ToArray();

                    // 将图片文件的绝对路径和目录名的数字添加到列表中
                    foreach (string imagePath in images)
                    {
                        imagePaths.Add((imagePath, dirNumber));
                    }
                }
            }

            return imagePaths;
        }

        // 获取图片的尺寸
        public static System.Drawing.Size GetImageSize(string imagePath)
        {
            using (var stream = new FileStream(imagePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var decoder = BitmapDecoder.Create(stream, BitmapCreateOptions.DelayCreation, BitmapCacheOption.None);
                if (decoder.Frames.Count > 0)
                {
                    var frame = decoder.Frames[0];
                    return new System.Drawing.Size(frame.PixelWidth, frame.PixelHeight);
                }
            }
            return System.Drawing.Size.Empty;
        }

        // 将安全字符串转换为字符串
        public static string ConvertSecureStringToString(SecureString securePassword)
        {
            if (securePassword == null)
                throw new ArgumentNullException("securePassword");

            IntPtr unmanagedString = IntPtr.Zero;
            try
            {
                unmanagedString = Marshal.SecureStringToGlobalAllocUnicode(securePassword);
                return Marshal.PtrToStringUni(unmanagedString);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(unmanagedString);
            }
        }

        // 提取IP地址部分
        public static int FindIPEndIndex(string input)
        {
            // Ignoring leading slashes
            int startIndex = 0;
            while (startIndex < input.Length && input[startIndex] == '\\')
            {
                startIndex++;
            }

            // IP地址部分的模式是数字和点
            int dotCount = 0;
            for (int i = startIndex; i < input.Length; i++)
            {
                char c = input[i];

                // 如果遇到的是点，增加点的计数
                if (c == '.')
                {
                    dotCount++;
                    // 如果点计数到达3，那么下一个非数字字符就是IP地址的结束
                    if (dotCount == 3)
                    {
                        // Look ahead for the next non-digit character
                        int j = i + 1;
                        while (j < input.Length && char.IsDigit(input[j]))
                        {
                            j++;
                        }

                        // Return the index of the non-digit character or the end of the string
                        return j < input.Length ? j : -1;
                    }
                }
                // 如果当前字符不是数字也不是点，则不是一个有效的IP地址
                else if (!char.IsDigit(c))
                {
                    break;
                }
            }
            return -1; // 没有找到有效的IP地址结束位置
        }

        // 判断某个进程是否正在运行，如果没有运行则尝试启动进程
        public static bool CheckAndStartProcess(string processName, string executablePath)
        {
            // 检查进程是否已经在运行
            Process[] processes = Process.GetProcessesByName(processName);

            if (processes.Length == 0)
            {
                // 进程没有运行，尝试启动进程
                try
                {
                    Process.Start(executablePath);
                }
                catch (Exception ex)
                {
                    // 处理错误（例如，文件未找到，没有执行权限等）
                    MessageBox.Show($"Could not start the process: {ex.Message}");
                }
            }
            else
            {
                // 进程已经在运行
                //MessageBox.Show($"{processName} is already running.");
            }

            processes = Process.GetProcessesByName(processName);

            return processes.Length > 0;
        }

        // 获取指定目录下的所有文件，支持超时设置
        public static async Task<string[]> GetFilesWithTimeout(string path, string searchPattern, SearchOption searchOption, int timeout)
        {
            using (var cts = new CancellationTokenSource())
            {
                var task = Task.Run(() => Directory.GetFiles(path, searchPattern, searchOption), cts.Token);
                if (await Task.WhenAny(task, Task.Delay(timeout, cts.Token)) == task)
                {
                    cts.Cancel(); // 取消延迟任务
                    return await task; // 返回任务结果
                }
                else
                {
                    throw new TimeoutException();
                }
            }
        }

        // 读取CSV文件，返回用户名和密码的字典
        public static Dictionary<string, string> ReadUserAndPasswordCsvFile(string strFilePath)
        {
            char delimiter = ',';

            Dictionary<string, string> data = new Dictionary<string, string>();

            if (File.Exists(strFilePath))
            {
                using (StreamReader reader = new StreamReader(strFilePath))
                {
                    // 读取并忽略第一行(列标题)
                    reader.ReadLine();

                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        string[] values = line.Split(delimiter);
                        if (values.Length == 2)
                        {
                            string username = values[0].Trim();
                            string password = values[1].Trim();
                            data[username] = password;
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("CSV file does not exist.");
            }

            return data;
        }
    }
}
