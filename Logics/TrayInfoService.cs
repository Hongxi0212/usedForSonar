using AutoScanFQCTest.DataModels;
using AutoScanFQCTest.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrackBar;

namespace AutoScanFQCTest.Logics
{
    public class TrayInfoService
    {
        public MainWindow m_parent;

        public AutoResetEvent CopyImagesEvent;

        // 构造函数
        public TrayInfoService(MainWindow parent)
        {
            m_parent = parent;

            CopyImagesEvent = new AutoResetEvent(false);

            m_parent.m_global.m_bIsImagesCloningThreadInProcess = new bool[66];
            for (int n = 0; n < m_parent.m_global.m_bIsImagesCloningThreadInProcess.Length; n++)
                m_parent.m_global.m_bIsImagesCloningThreadInProcess[n] = false;
        }

        // 处理来自AVI系统的托盘信息，用于Nova，主要处理数据库和图片路径存储
        public bool HandleTrayInfoFromAVIForNova_DatabaseAndImagePathStorage(AVITrayInfo tray_info, bool bInsertToDatabase = true)
        {
            bool bSuccess = true;

            string strMachineName = tray_info.Mid;
            string strMachineIP = "";
            if (string.IsNullOrEmpty(strMachineName))
            {
                m_parent.m_global.m_log_presenter.Log(string.Format("错误信息：机器编号为空！"));
                return false;
            }
            if (false == m_parent.m_global.m_dict_machine_names_and_IPs.Keys.Contains(strMachineName))
            {
                m_parent.m_global.m_log_presenter.Log(string.Format("错误信息：机器编号{0}不在清单上！", strMachineName));
                return false;
            }

            string strTrayTable = "trays_" + strMachineName;
            string strProductTable = "products_" + strMachineName;
            string strDefectTable = "details_" + strMachineName;

            strTrayTable = strTrayTable.Replace("-", "_");
            strProductTable = strProductTable.Replace("-", "_");
            strDefectTable = strDefectTable.Replace("-", "_");

            strMachineIP = m_parent.m_global.m_dict_machine_names_and_IPs[strMachineName];

            m_parent.m_global.m_strCurrentClientMachineName = strMachineName;
            m_parent.m_global.m_nCurrentTrayRows = tray_info.Row;
            m_parent.m_global.m_nCurrentTrayColumns = tray_info.Col;

            List<string> list_pingable_IPs = new List<string>();               // 可以ping通的IP地址列表
            List<string> list_unpingable_IPs = new List<string>();           // 无法ping通的IP地址列表

            // 将料盘信息TrayInfo插入数据库
            if (true == bInsertToDatabase)
            {
                // 删除相同set_id的tray数据
                m_parent.m_global.m_database_service.DeleteTrayDataIfExist(strTrayTable, tray_info.SetId);

                // 插入数据的SQL语句
                string insertOrUpdateSql = @"
                    INSERT INTO " + strTrayTable + @" (batch_id, number_of_rows, number_of_cols, front, mid, operator, operator_id, product_id, resource, scan_code_mode_, set_id, total_pcs, uuid, work_area, full_status)
                    VALUES (@BatchId, @Row, @Col, @Front, @Mid, @Operator, @OperatorId, @ProductId, @Resource, @ScanCodeMode, @SetId, @TotalPcs, @Uuid, @WorkArea, @Fullstatus);";

                // 将产品信息插入数据库
                if (false == m_parent.m_global.m_mysql_ops.AddTrayInfoToTable(strTrayTable, insertOrUpdateSql, tray_info))
                {
                    //m_parent.m_global.m_log_presenter.Log(string.Format("插入托盘信息失败，可能与原有数据重复。批次号：{0}", tray_info.BatchId));
                    return bSuccess;
                }
            }

            // 从tray_info中获取产品信息
            for (int i = 0; i < tray_info.Products.Count; i++)
            {
                Product product = tray_info.Products[i];

                // 移除缺陷信息为空的产品
                if (product.Defects == null)
                {
                    tray_info.Products.RemoveAt(i);
                    i--;
                    continue;
                }

                // 从product中获取其他信息
                string strSharedImgDir = product.ShareimgPath;

                m_parent.m_global.m_log_presenter.Log(string.Format("共享图片目录：{0}", string.IsNullOrEmpty(strSharedImgDir) ? "无" : strSharedImgDir));

                int nImageWidth = 0;
                int nImageHeight = 0;

                // 将产品数据插入数据库
                if (true == bInsertToDatabase)
                {
                    // 删除相同bar_code的product数据
                    m_parent.m_global.m_database_service.DeleteProductDataIfExist(strProductTable, product.BarCode);

                    // 判断产品是否有缺陷数据
                    if (product.Defects.Count <= 0)
                    {
                        // m_parent.m_global.m_log_presenter.Log(string.Format("产品{0} 没有缺陷数据", product.BarCode));
                        continue;
                    }

                    // 插入数据的SQL语句
                    string insertSql = @"
                        INSERT INTO " + strProductTable + @" (set_id, uuid, batch_id, bar_code, image, image_a, image_b, is_null, panel_id, pos_col, pos_row, shareimg_path, r1)
                        VALUES (@SetId, @Uuid, @BatchId, @BarCode, @Image, @ImageA, @ImageB, @IsNull, @PanelId, @PosCol, @PosRow, @ShareimgPath, @r1);";

                    product.SetId = tray_info.SetId;
                    product.Uuid = tray_info.Uuid;

                    // 将产品数据插入数据库
                    m_parent.m_global.m_mysql_ops.AddProductToTable("Products", insertSql, product, tray_info.BatchId);

                    // 删除相同product_id的defect数据
                    m_parent.m_global.m_database_service.DeleteDefectDataIfExist(strDefectTable, product.BarCode);

                    // 将缺陷信息插入数据库
                    for (int j = 0; j < product.Defects.Count; j++)
                    {
                        Defect defect = product.Defects[j];

                        //m_parent.m_global.m_log_presenter.Log(string.Format("缺陷信息：{0}", defect.Type));

                        string insertQuery = @"
                                            INSERT INTO " + strDefectTable + @" (id, set_id, uuid, product_id, height, sn, type, width, x, y, channel_, channelNum_, area, um_per_pixel, aiResult, aiDefectCode, aiGuid)
                                            VALUES (@id, @set_id, @uuid, @product_id, @height, @sn, @type, @width, @x, @y, @channel_, @channelNum_, @area, @um_per_pixel, @aiResult, @aiDefectCode, @aiGuid);";

                        defect.id = j;
                        defect.SetId = tray_info.SetId;
                        defect.Uuid = tray_info.Uuid;

                        // 将缺陷信息插入数据库
                        m_parent.m_global.m_mysql_ops.AddDefectToTable(strDefectTable, insertQuery, defect, product.PanelId);
                    }
                }

                // 如果strShareImgPath为有效图片路径，将图片下载到本地
                if (false == string.IsNullOrEmpty(strSharedImgDir))
                {
                    try
                    {
                        List<string> imageFiles = new List<string>();

                        // 定义支持的图片格式
                        string[] extensions = new string[] { ".jpg", ".jpeg", ".png" };

                        // 判断strSharedImgDir是否是远程路径
                        if (strSharedImgDir.StartsWith("\\\\"))
                        {
                            // 下载图片
                            string strIP = strSharedImgDir.Split('\\')[2];

                            // 检查是否在ping通的IP地址列表中
                            if (list_pingable_IPs.Contains(strIP))
                            {
                                //m_parent.m_global.m_log_presenter.Log(string.Format("IP地址：{0}已经ping通", strIP));
                            }
                            else
                            {
                                // 检查是否在无法ping通的IP地址列表中
                                if (list_unpingable_IPs.Contains(strIP))
                                {
                                    //m_parent.m_global.m_log_presenter.Log(string.Format("无法ping通IP地址：{0}", strIP));
                                    continue;
                                }

                                // 判断strIP是否是有效IP地址
                                if (false == AVICommunication.ValidateIPAddress(strIP))
                                {
                                    m_parent.m_global.m_log_presenter.Log(string.Format("无效的IP地址：{0}", strIP));
                                    return bSuccess;
                                }

                                // 判断能否ping通strIP
                                if (false == AVICommunication.PingAddress(strIP))
                                {
                                    list_unpingable_IPs.Add(strIP);

                                    m_parent.m_global.m_log_presenter.Log(string.Format("无法ping通IP地址：{0}", strIP));

                                    continue;
                                }
                                else
                                {
                                    list_pingable_IPs.Add(strIP);
                                }
                            }
                        }

                        // 算出正确的远程路径
                        int nIPEndIndex = GeneralUtilities.FindIPEndIndex(strSharedImgDir);
                        if (nIPEndIndex > 0)
                        {
                            // 视觉那边只传了A目录，所以这里要循环两次，第一次是A，第二次改为B
                            // 苏州因为有C面要循环三次
                            string strSharedImgDirBackup = product.ShareimgPath;
                            for (int iteration = 0; iteration < 3; iteration++)
                            {
                                // 第二次循环时，如果strSharedImgDir末字母是A，则改为B，否则退出
                                if (iteration == 1)
                                {
                                    if (strSharedImgDirBackup.EndsWith("A"))
                                    {
                                        strSharedImgDir = strSharedImgDirBackup.Substring(0, strSharedImgDirBackup.Length - 1) + "B";

                                        strSharedImgDirBackup = strSharedImgDir;
                                    }
                                    else
                                        break;
                                }
                                else if (iteration == 2)
                                {
                                    if (m_parent.m_global.m_strSiteCity == "盐城" || m_parent.m_global.m_strProductSubType == "yuehu")
                                    {
                                        continue;
                                    }

                                    if (strSharedImgDirBackup.EndsWith("B"))
                                    {
                                        strSharedImgDir = strSharedImgDirBackup.Substring(0, strSharedImgDirBackup.Length - 1) + "C";

                                        strSharedImgDirBackup = strSharedImgDir;
                                    }
                                    else
                                        break;
                                }

                                // 提取IP地址部分
                                string ip = strSharedImgDir.Substring(0, nIPEndIndex);

                                // 查找 "outdatas" 子字符串的位置
                                int outdatasIndex = strSharedImgDir.IndexOf("outdatas");

                                if (outdatasIndex >= 0)
                                {
                                    // 提取包含 "outdatas" 及其右边部分的子字符串
                                    string outdatasIncludingRight = strSharedImgDir.Substring(outdatasIndex);

                                    strSharedImgDir = ip + "\\" + outdatasIncludingRight;

                                    if (!Directory.Exists(strSharedImgDir))
                                    {
                                        break;
                                    }

                                    // 遍历所有支持的图片格式并获取匹配的文件
                                    foreach (string extension in extensions)
                                    {
                                        imageFiles.AddRange(Directory.GetFiles(strSharedImgDir, $"*{extension}", SearchOption.AllDirectories));
                                    }

                                    // 去重
                                    imageFiles = imageFiles.Distinct().ToList();

                                    // 定义保存图片的本地路径
                                    string strLocalPath = string.Format("D:\\avi_images");
                                    if (false == Directory.Exists(strLocalPath))
                                    {
                                        Directory.CreateDirectory(strLocalPath);

                                        if (false == Directory.Exists(strLocalPath))
                                        {
                                            m_parent.m_global.m_log_presenter.Log(string.Format("创建本地目录失败：{0}", strLocalPath));
                                            return bSuccess;
                                        }
                                    }

                                    if (true == string.IsNullOrEmpty(tray_info.Mid))
                                    {
                                        m_parent.m_global.m_log_presenter.Log(string.Format("机器编号为空，无法创建本地目录"));
                                        return bSuccess;
                                    }

                                    strLocalPath = Path.Combine(strLocalPath, tray_info.Mid);
                                    if (false == Directory.Exists(strLocalPath))
                                    {
                                        Directory.CreateDirectory(strLocalPath);

                                        if (false == Directory.Exists(strLocalPath))
                                        {
                                            m_parent.m_global.m_log_presenter.Log(string.Format("创建本地目录失败：{0}", strLocalPath));
                                            return bSuccess;
                                        }
                                    }

                                    // 获取今日日期，创建本地目录
                                    string strToday = DateTime.Now.ToString("yyyy_MM_dd");

                                    strLocalPath = Path.Combine(strLocalPath, strToday);
                                    if (false == Directory.Exists(strLocalPath))
                                    {
                                        Directory.CreateDirectory(strLocalPath);

                                        if (false == Directory.Exists(strLocalPath))
                                        {
                                            m_parent.m_global.m_log_presenter.Log(string.Format("创建本地目录失败：{0}", strLocalPath));
                                            return bSuccess;
                                        }
                                    }

                                    // 创建二维码命名的目录
                                    strLocalPath = Path.Combine(strLocalPath, product.BarCode);
                                    if (false == Directory.Exists(strLocalPath))
                                    {
                                        Directory.CreateDirectory(strLocalPath);

                                        if (false == Directory.Exists(strLocalPath))
                                        {
                                            m_parent.m_global.m_log_presenter.Log(string.Format("创建本地目录失败：{0}", strLocalPath));
                                            return bSuccess;
                                        }
                                    }

                                    // 创建正反面目录
                                    string strSideDir = strLocalPath + "\\" + strSharedImgDirBackup.Substring(strSharedImgDirBackup.Length - 1);
                                    if (false == Directory.Exists(strSideDir))
                                    {
                                        Directory.CreateDirectory(strSideDir);

                                        string strSideDir2 = strSideDir.Substring(0, strSideDir.Length - 1) + "B";

                                        if (false == Directory.Exists(strSideDir2))
                                        {
                                            Directory.CreateDirectory(strSideDir2);
                                        }
                                    }

                                    // 拷贝文件到本地路径
                                    for (int j = 0; j < imageFiles.Count; j++)
                                    {
                                        string filePath = imageFiles[j];
                                        string fileName = Path.GetFileName(imageFiles[j]);

                                        string destFile = Path.Combine(strSideDir, fileName);

                                        //File.Copy(filePath, destFile, true);

                                        if (strSideDir.EndsWith("A"))
                                        {
                                            if (null == product.m_list_local_imageA_paths_for_channel1)
                                                product.m_list_local_imageA_paths_for_channel1 = new List<string>();
                                            if (null == product.m_list_remote_imageA_paths_for_channel1)
                                                product.m_list_remote_imageA_paths_for_channel1 = new List<string>();
                                            if (null == product.m_list_local_imageA_paths_for_channel2)
                                                product.m_list_local_imageA_paths_for_channel2 = new List<string>();
                                            if (null == product.m_list_remote_imageA_paths_for_channel2)
                                                product.m_list_remote_imageA_paths_for_channel2 = new List<string>();

                                            string[] parts = fileName.Split('_');

                                            // 检查数组长度以确保文件名格式正确
                                            if (parts.Length == 3)
                                            {
                                                // 取出中间的数字部分（即数组的第二个元素）
                                                string middleNumber = parts[1];

                                                // 判断中间的数字是1还是2
                                                //if (middleNumber == "1")
                                                {
                                                    // 中间的数字是1
                                                    product.m_list_local_imageA_paths_for_channel1.Add(destFile);
                                                    product.m_list_remote_imageA_paths_for_channel1.Add(filePath);
                                                }
                                                //else if (middleNumber == "2")
                                                //{
                                                //    // 中间的数字是2
                                                //    product.m_list_local_imageA_paths_for_channel2.Add(destFile);
                                                //    product.m_list_remote_imageA_paths_for_channel2.Add(filePath);
                                                //}
                                                //else
                                                //{
                                                //    // 文件名不符合预期格式
                                                //    m_parent.m_global.m_log_presenter.Log(string.Format("文件名不符合预期格式：{0}", fileName));
                                                //    continue;
                                                //}

                                                // 将destFile最后一个字母改为B
                                                string strNewDestFile = strSideDir.Substring(0, strSideDir.Length - 1) + "B";
                                                if (false == Directory.Exists(strNewDestFile))
                                                {
                                                    Directory.CreateDirectory(strNewDestFile);
                                                }

                                                if (null == product.m_list_local_imageB_paths_for_channel1)
                                                    product.m_list_local_imageB_paths_for_channel1 = new List<string>();
                                                if (null == product.m_list_remote_imageB_paths_for_channel1)
                                                    product.m_list_remote_imageB_paths_for_channel1 = new List<string>();
                                                if (null == product.m_list_local_imageB_paths_for_channel2)
                                                    product.m_list_local_imageB_paths_for_channel2 = new List<string>();
                                                if (null == product.m_list_remote_imageB_paths_for_channel2)
                                                    product.m_list_remote_imageB_paths_for_channel2 = new List<string>();

                                                // 得到filePath的文件名，去掉目录
                                                string strFileName = Path.GetFileName(filePath);

                                                string strNewPath = Path.Combine(strNewDestFile, strFileName);

                                                if (false == product.m_list_local_imageB_paths_for_channel1.Contains(strNewPath))
                                                {
                                                    try
                                                    {
                                                        // 获取filePath的目录
                                                        string strSourceDir = Path.GetDirectoryName(filePath);

                                                        // 将最后一个字母改为B
                                                        strSourceDir = strSourceDir.Substring(0, strSourceDir.Length - 1) + "B";

                                                        string strSourceFile = Path.Combine(strSourceDir, strFileName);

                                                        product.m_list_local_imageB_paths_for_channel1.Add(strNewPath);
                                                        product.m_list_remote_imageB_paths_for_channel1.Add(strSourceFile);
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        m_parent.m_global.m_log_presenter.Log(string.Format("拷贝文件失败。错误信息：{0}", ex.Message));
                                                    }
                                                }

                                                if (null == product.m_list_local_imageC_paths_for_channel1)
                                                    product.m_list_local_imageC_paths_for_channel1 = new List<string>();
                                                if (null == product.m_list_remote_imageC_paths_for_channel1)
                                                    product.m_list_remote_imageC_paths_for_channel1 = new List<string>();
                                                if (null == product.m_list_local_imageC_paths_for_channel2)
                                                    product.m_list_local_imageC_paths_for_channel2 = new List<string>();
                                                if (null == product.m_list_remote_imageC_paths_for_channel2)
                                                    product.m_list_remote_imageC_paths_for_channel2 = new List<string>();

                                                string strNewDestFileForSideC = strSideDir.Substring(0, strSideDir.Length - 1) + "C";
                                                if (false == Directory.Exists(strNewDestFileForSideC))
                                                {
                                                    Directory.CreateDirectory(strNewDestFileForSideC);
                                                }

                                                strNewPath = Path.Combine(strNewDestFileForSideC, strFileName);

                                                if (false == product.m_list_local_imageC_paths_for_channel1.Contains(strNewPath))
                                                {
                                                    try
                                                    {
                                                        // 获取filePath的目录
                                                        string strSourceDir = Path.GetDirectoryName(filePath);

                                                        // 将最后一个字母改为B
                                                        strSourceDir = strSourceDir.Substring(0, strSourceDir.Length - 1) + "C";

                                                        string strSourceFile = Path.Combine(strSourceDir, strFileName);

                                                        product.m_list_local_imageC_paths_for_channel1.Add(strNewPath);
                                                        product.m_list_remote_imageC_paths_for_channel1.Add(strSourceFile);
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        m_parent.m_global.m_log_presenter.Log(string.Format("拷贝文件失败。错误信息：{0}", ex.Message));
                                                    }
                                                }

                                            }
                                        }
                                        else if (strSideDir.EndsWith("B"))
                                        {
                                            if (null == product.m_list_local_imageB_paths_for_channel1)
                                                product.m_list_local_imageB_paths_for_channel1 = new List<string>();
                                            if (null == product.m_list_remote_imageB_paths_for_channel1)
                                                product.m_list_remote_imageB_paths_for_channel1 = new List<string>();
                                            if (null == product.m_list_local_imageB_paths_for_channel2)
                                                product.m_list_local_imageB_paths_for_channel2 = new List<string>();
                                            if (null == product.m_list_remote_imageB_paths_for_channel2)
                                                product.m_list_remote_imageB_paths_for_channel2 = new List<string>();

                                            string[] parts = fileName.Split('_');

                                            // 检查数组长度以确保文件名格式正确
                                            if (parts.Length == 3)
                                            {
                                                // 取出最后的数字部分（即数组的第三个元素）
                                                string lastNumber = parts[2].Split('.').Count() > 0 ? parts[2].Split('.')[0] : "";

                                                try
                                                {
                                                    int nLastNumber = Convert.ToInt32(lastNumber);

                                                    for (int d = 0; d < product.m_list_local_imageB_paths_for_channel1.Count; d++)
                                                    {
                                                        string fileName2 = Path.GetFileNameWithoutExtension(imageFiles[j]);

                                                        string[] parts2 = fileName2.Split('_');

                                                        int nLastNumber2 = Convert.ToInt32(parts2[2]);

                                                        if (nLastNumber == nLastNumber2)
                                                        {
                                                            // 如果文件名相同，从product.m_list_local_imageB_paths_for_channel1中删除
                                                            // 如果出现光源图片缺少，尝试注释掉这两行
                                                            product.m_list_local_imageB_paths_for_channel1.RemoveAt(d);
                                                            product.m_list_remote_imageB_paths_for_channel1.RemoveAt(d);

                                                            break;
                                                        }
                                                    }
                                                }
                                                catch (Exception ex)
                                                {
                                                    m_parent.m_global.m_log_presenter.Log(string.Format("处理AVI产品信息失败。错误信息：{0}", ex.Message));

                                                    bSuccess = false;
                                                }

                                                // 中间的数字是1
                                                if (product.m_list_local_imageB_paths_for_channel1.Contains(destFile))
                                                {
                                                    continue;
                                                }
                                                else
                                                {
                                                    product.m_list_local_imageB_paths_for_channel1.Add(destFile);
                                                    product.m_list_remote_imageB_paths_for_channel1.Add(filePath);
                                                }
                                            }
                                        }
                                        else if (strSideDir.EndsWith("C"))
                                        {
                                            if (null == product.m_list_local_imageC_paths_for_channel1)
                                                product.m_list_local_imageC_paths_for_channel1 = new List<string>();
                                            if (null == product.m_list_remote_imageC_paths_for_channel1)
                                                product.m_list_remote_imageC_paths_for_channel1 = new List<string>();
                                            if (null == product.m_list_local_imageC_paths_for_channel2)
                                                product.m_list_local_imageC_paths_for_channel2 = new List<string>();
                                            if (null == product.m_list_remote_imageC_paths_for_channel2)
                                                product.m_list_remote_imageC_paths_for_channel2 = new List<string>();

                                            string[] parts = fileName.Split('_');

                                            // 检查数组长度以确保文件名格式正确
                                            if (parts.Length == 3)
                                            {
                                                // 取出中间的数字部分（即数组的第二个元素）
                                                string middleNumber = parts[1];

                                                // 判断中间的数字是1还是2
                                                //if (middleNumber == "1")
                                                {
                                                    // 中间的数字是1
                                                    if (product.m_list_local_imageC_paths_for_channel1.Contains(destFile))
                                                    {
                                                        continue;
                                                    }
                                                    else
                                                    {
                                                        product.m_list_local_imageC_paths_for_channel1.Add(destFile);
                                                        product.m_list_remote_imageC_paths_for_channel1.Add(filePath);
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    if (imageFiles.Count > 0)
                                    {
                                        bSuccess = true;
                                        m_parent.m_global.m_log_presenter.Log(string.Format("共检索到{0}张图片", imageFiles.Count));
                                    }

                                    // 可能不是有效图片文件扩展名，需要判断
                                    if (false)
                                    {
                                        if (strSharedImgDir.EndsWith(".jpg") || strSharedImgDir.EndsWith(".jpeg"))
                                        {
                                            // 下载图片
                                            //string strLocalPath = string.Format("D:\\{0}.jpg", DateTime.Now.ToString("yyyyMMddHHmmss"));

                                            //WebClient webClient = new WebClient();
                                            //webClient.DownloadFile(strSharedImgDir, strLocalPath);

                                            //m_parent.m_global.m_log_presenter.Log(string.Format("图片已下载到：{0}", strLocalPath));

                                            //// 释放资源
                                            //webClient.Dispose();
                                        }

                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        m_parent.m_global.m_log_presenter.Log(string.Format("处理AVI产品信息失败。错误信息：{0}", ex.Message));

                        bSuccess = false;
                    }

                }

            }

            // 后台线程拷贝图片到本地
            if (false == m_parent.m_global.m_bIsImagesCloningThreadInProcess[0] && false == bInsertToDatabase)
            {
                m_parent.m_global.m_bIsImagesCloningThreadInProcess[0] = true;

                // 开启线程，将图片拷贝到本地
                new Thread(thread_clone_images_for_Nova).Start();
            }

            return bSuccess;
        }

        // 处理来自AVI系统的托盘信息，用于Dock和CL5，主要处理数据库和图片路径存储
        public bool HandleTrayInfoFromAVIForDockOrCL5_DatabaseAndImagePathStorage(TrayInfo tray_info, bool bInsertToDatabase = true)
        {
            bool bSuccess = true;

            string strMachineName = tray_info.machine_id;
            string strMachineIP = "";
            if (string.IsNullOrEmpty(strMachineName))
            {
                m_parent.m_global.m_log_presenter.Log(string.Format("错误信息：机器编号为空！"));
                return false;
            }
            if (false == m_parent.m_global.m_dict_machine_names_and_IPs.Keys.Contains(strMachineName))
            {
                m_parent.m_global.m_log_presenter.Log(string.Format("错误信息：机器编号{0}不在清单上！", strMachineName));
                return false;
            }

            strMachineIP = m_parent.m_global.m_dict_machine_names_and_IPs[strMachineName];

            int nMachineIndex = m_parent.m_global.m_dict_machine_names_and_IPs.Keys.ToList().IndexOf(strMachineName);

            string strTrayTable = "trays_" + strMachineName;
            string strProductTable = "products_" + strMachineName;
            string strDefectTable = "details_" + strMachineName;

            m_parent.m_global.m_strCurrentClientMachineName = strMachineName;
            m_parent.m_global.m_nCurrentTrayRows = tray_info.total_rows;
            m_parent.m_global.m_nCurrentTrayColumns = tray_info.total_columns;

            strTrayTable = strTrayTable.Replace("-", "_");
            strProductTable = strProductTable.Replace("-", "_");
            strDefectTable = strDefectTable.Replace("-", "_");

            List<string> list_pingable_IPs = new List<string>();               // 可以ping通的IP地址列表
            List<string> list_unpingable_IPs = new List<string>();           // 无法ping通的IP地址列表

            // 将料盘信息TrayInfo插入数据库
            if (true == bInsertToDatabase)
            {
                // 删除相同set_id的tray数据
                m_parent.m_global.m_database_service.DeleteTrayDataIfExist(strTrayTable, tray_info.set_id);

                // 插入数据的SQL语句
                string insertOrUpdateSql = @"
                    INSERT INTO " + strTrayTable + @" (number_of_rows, number_of_cols, operator, operator_id, resource, set_id, machine_id, work_area, site)
                    VALUES (@Row, @Col, @Operator, @OperatorId, @Resource, @SetId, @MachineId, @WorkArea, @Site);";

                // 将料盘信息插入数据库
                if (false == m_parent.m_global.m_mysql_ops.AddTrayInfoToTable(strTrayTable, insertOrUpdateSql, tray_info))
                {
                    //m_parent.m_global.m_log_presenter.Log(string.Format("插入托盘信息失败，可能与原有数据重复。批次号：{0}", tray_info.BatchId));
                    return bSuccess;
                }
            }

            // 从tray_info中获取产品信息
            for (int i = 0; i < tray_info.products.Count; i++)
            {
                ProductInfo product = tray_info.products[i];

                bool[] bValidSides = new bool[6] { false, false, false, false, false, false };

                // 判断是否只获取有缺陷的一面的图片
                if (false == m_parent.m_global.m_bRetrieveOnlyImagesOfDefectedSide)
                {
                    bValidSides = new bool[6] { true, true, true, true, true, true };
                }

                // 移除缺陷信息为空的产品
                //if (product.defects == null)
                //{
                //    tray_info.products.RemoveAt(i);
                //    i--;
                //    continue;
                //}

                // 从product中获取其他信息
                //string strSharedImgDir = product.ShareimgPath;

                //m_parent.m_global.m_log_presenter.Log(string.Format("共享图片目录：{0}", string.IsNullOrEmpty(strSharedImgDir) ? "无" : strSharedImgDir));

                int nImageWidth = 0;
                int nImageHeight = 0;

                // 获取机台IP（同一个料盘产品机台号可能不一样）
                strMachineName = product.machine_id;
                if (string.IsNullOrEmpty(strMachineName))
                {
                    m_parent.m_global.m_log_presenter.Log(string.Format("错误信息：机器编号为空！"));
                    //return false;
                }
                if (false == m_parent.m_global.m_dict_machine_names_and_IPs.Keys.Contains(strMachineName))
                {
                    m_parent.m_global.m_log_presenter.Log(string.Format("错误信息：机器编号{0}不在清单上！", strMachineName));
                    //return false;
                }
                strMachineIP = m_parent.m_global.m_dict_machine_names_and_IPs[strMachineName];

                for (int j = 0; j < product.defects.Count; j++)
                {
                    DefectInfo defect = product.defects[j];

                    if (defect.side >= 0 && defect.side < bValidSides.Length)
                        bValidSides[defect.side] = true;
                }

                // 将产品数据插入数据库
                if (true == bInsertToDatabase)
                {
                    // 删除相同bar_code的product数据
                    m_parent.m_global.m_database_service.DeleteProductDataIfExist(strProductTable, product.barcode);

                    // 插入数据的SQL语句
                    string insertSql = @"
                        INSERT INTO " + strProductTable + @" (set_id, machine_id, bar_code, pos_col, pos_row, bET, sideA_image_path, sideB_image_path, sideC_image_path, sideD_image_path, sideE_image_path, sideF_image_path, sideG_image_path, sideH_image_path, all_image_paths, r1, r2, r3, r4, r5, r6, r7, r8, r9, r10) 
                        VALUES (@SetId, @MachineId, @BarCode, @PosCol, @PosRow, @bET, @sideA_image_path, @sideB_image_path, @sideC_image_path, @sideD_image_path,  
                        @sideE_image_path, @sideF_image_path, @sideG_image_path, @sideH_image_path, @all_image_paths, @r1, @r2, @r3, @r4, @r5, @r6, @r7, @r8, @r9, @r10);";
                    /*string insertSql = @"
                        INSERT INTO " + strProductTable + @" (set_id, machine_id, bar_code, pos_col, pos_row, bET, sideA_image_path, sideB_image_path, sideC_image_path, sideD_image_path, sideE_image_path, sideF_image_path, sideG_image_path, sideH_image_path, image_path_for_MES_xiaochengxu_chatu, r1, r2, r3, r4, r5, r6, r7, r8, r9, r10) 
                        VALUES (@SetId, @MachineId, @BarCode, @PosCol, @PosRow, @bET, @sideA_image_path, @sideB_image_path, @sideC_image_path, @sideD_image_path,  
                        @sideE_image_path, @sideF_image_path, @sideG_image_path, @sideH_image_path, @image_path_for_MES_xiaochengxu_chatu, @r1, @r2, @r3, @r4, @r5, @r6, @r7, @r8, @r9, @r10);";*/

                    product.set_id = tray_info.set_id;

                    // 将产品数据插入数据库
                    m_parent.m_global.m_mysql_ops.AddProductToTable(strProductTable, insertSql, product);

                    // 删除相同product_id的defect数据
                    m_parent.m_global.m_database_service.DeleteDefectDataIfExist(strDefectTable, product.barcode);

                    // 判断产品是否有缺陷数据
                    if (product.defects == null || product.defects.Count <= 0)
                    {
                        // m_parent.m_global.m_log_presenter.Log(string.Format("产品{0} 没有缺陷数据", product.barcode));
                        continue;
                    }

                    // 将缺陷信息插入数据库
                    for (int j = 0; j < product.defects.Count; j++)
                    {
                        DefectInfo defect = product.defects[j];

                        //m_parent.m_global.m_log_presenter.Log(string.Format("缺陷信息：{0}", defect.Type));

                        string insertQuery = @"
                                            INSERT INTO " + strDefectTable + @" (id, set_id, product_id, height, type, width, area, center_x, center_y, side, light_channel, image_path)
                                            VALUES (@id, @set_id, @product_id, @height, @type, @width, @area, @center_x, @center_y, @side, @light_channel, @image_path);";

                        defect.ID = j;
                        defect.set_id = tray_info.set_id;
                        defect.product_id = product.barcode;

                        // 将缺陷信息插入数据库
                        m_parent.m_global.m_mysql_ops.AddDefectToTable(strDefectTable, insertQuery, defect);
                    }
                }

                List<string> list_side_paths = new List<string>();

                if (false == string.IsNullOrEmpty(product.sideA_image_path))
                    list_side_paths.Add(product.sideA_image_path);
                if (false == string.IsNullOrEmpty(product.sideB_image_path))
                    list_side_paths.Add(product.sideB_image_path);
                if (false == string.IsNullOrEmpty(product.sideC_image_path))
                    list_side_paths.Add(product.sideC_image_path);

                // 如果是检胶，无缺陷的穴位不需要下载图片
                List<int> list_light_channels_without_defect = new List<int>();
                if (m_parent.m_global.m_strProductSubType == "glue_check")
                {
                    for (int j = 0; j < product.defects_with_NULL_or_empty_type_name.Count; j++)
                    {
                        list_light_channels_without_defect.Add(product.defects_with_NULL_or_empty_type_name[j].light_channel);
                    }
                }

                int nNumOfAccessedImages = 0;
                for (int k = 0; k < list_side_paths.Count; k++)
                {
                    string strSidePath = list_side_paths[k];

                    // 如果strSidePath为有效图片路径，将图片下载到本地
                    if (false == string.IsNullOrEmpty(strSidePath))
                    {
                        string strSharedImgDir = strSidePath;

                        try
                        {
                            List<string> imageFiles = new List<string>();

                            // 定义支持的图片格式
                            string[] extensions = new string[] { ".jpg", ".jpeg", ".png", ".bmp" };

                            if (strSidePath.IndexOf("E:") >= 0)
                                strSharedImgDir = strSidePath.Replace("E:", "\\\\" + strMachineIP + "\\e");
                            else if (strSidePath.IndexOf("D:") >= 0)
                                strSharedImgDir = strSidePath.Replace("D:", "\\\\" + strMachineIP + "\\d");
                                else if (strSidePath.IndexOf("F:") >= 0)
                                    strSharedImgDir = strSidePath.Replace("F:", "\\\\" + strMachineIP);
                            else if (false == strSidePath.Contains("."))
                                strSharedImgDir = "\\\\" + strMachineIP + "\\" + strSidePath;

                            // 判断strSharedImgDir是否是远程路径
                            if (strSharedImgDir.StartsWith("\\\\"))
                            {
                                // 下载图片
                                string strIP = strSharedImgDir.Split('\\')[2];

                                // 检查是否在ping通的IP地址列表中
                                if (list_pingable_IPs.Contains(strIP))
                                {
                                    //m_parent.m_global.m_log_presenter.Log(string.Format("IP地址：{0}已经ping通", strIP));
                                }
                                else
                                {
                                    // 检查是否在无法ping通的IP地址列表中
                                    if (list_unpingable_IPs.Contains(strIP))
                                    {
                                        //m_parent.m_global.m_log_presenter.Log(string.Format("无法ping通IP地址：{0}", strIP));
                                        continue;
                                    }

                                    // 判断strIP是否是有效IP地址
                                    if (false == AVICommunication.ValidateIPAddress(strIP))
                                    {
                                        m_parent.m_global.m_log_presenter.Log(string.Format("无效的IP地址：{0}", strIP));
                                        return bSuccess;
                                    }

                                    // 判断能否ping通strIP
                                    if (false == AVICommunication.PingAddress(strIP))
                                    {
                                        list_unpingable_IPs.Add(strIP);

                                        m_parent.m_global.m_log_presenter.Log(string.Format("无法ping通IP地址：{0}", strIP));

                                        continue;
                                    }
                                    else
                                    {
                                        list_pingable_IPs.Add(strIP);
                                    }
                                }

                            }

                            bool bUnaccessable = false;

                            // 遍历所有支持的图片格式并获取匹配的文件
                            try
                            {
                                foreach (string extension in extensions)
                                {
                                    imageFiles.AddRange(Directory.GetFiles(strSharedImgDir, $"*{extension}", SearchOption.AllDirectories));
                                }
                            }
                            catch (Exception ex)
                            {
                                if (ex.Message.Contains("用户名或密码") || ex.Message.Contains("找不到网络名") || ex.Message.Contains("未能找到"))
                                {
                                    bUnaccessable = true;
                                }
                            }

                            if (true == bUnaccessable)
                            {
                                if (strSidePath.IndexOf("E:") >= 0)
                                    strSharedImgDir = strSidePath.Replace("E:", "\\\\" + strMachineIP);
                                else if (strSidePath.IndexOf("D:") >= 0)
                                    strSharedImgDir = strSidePath.Replace("D:", "\\\\" + strMachineIP);
                                else if (strSidePath.IndexOf("F:") >= 0)
                                    strSharedImgDir = strSidePath.Replace("F:", "\\\\" + strMachineIP);
                            else if (false == strSidePath.Contains("."))
                                strSharedImgDir = "\\\\" + strMachineIP + "\\" + strSidePath;

                                foreach (string extension in extensions)
                                {
                                    imageFiles.AddRange(Directory.GetFiles(strSharedImgDir, $"*{extension}", SearchOption.AllDirectories));
                                }
                            }

                            // 去重
                            imageFiles = imageFiles.Distinct().ToList();

                            // 如果是检胶，无缺陷的穴位不需要下载图片，根据list_light_channels_without_defect以及文件末尾的数字判断（带下划线的数字），只下载有缺陷的穴位图片
                            if (m_parent.m_global.m_strProductSubType == "glue_check")
                            {
                                List<string> list_image_files_with_defect = new List<string>();

                                for (int j = 0; j < imageFiles.Count; j++)
                                {
                                    string filePath = imageFiles[j];
                                    string fileName = Path.GetFileName(imageFiles[j]);

                                    // 找到最后一个下划线的位置
                                    int lastUnderscoreIndex = fileName.LastIndexOf('_');

                                    if (lastUnderscoreIndex > 0)
                                    {
                                        // fileName去掉扩展名
                                        fileName = Path.GetFileNameWithoutExtension(fileName);

                                        // 提取光源编号
                                        string lightSourceStr = fileName.Substring(lastUnderscoreIndex + 1);

                                        if (int.TryParse(lightSourceStr, out int lightSource))
                                        {
                                            if (list_light_channels_without_defect.Contains(lightSource))
                                            {
                                                continue;
                                            }
                                        }
                                    }

                                    list_image_files_with_defect.Add(filePath);
                                }

                                imageFiles = list_image_files_with_defect;
                            }

                            if (true)
                            {
                                try
                                {
                                    // 用来存储每个光源的最新成像文件
                                    var latestImages = new Dictionary<int, (DateTime timestamp, string filePath)>();

                                    foreach (var file in imageFiles)
                                    {
                                        // 获取文件名
                                        string fileName = Path.GetFileNameWithoutExtension(file);
                                        string extension = Path.GetExtension(file).ToLower();

                                        Debugger.Log(0, null, string.Format("222222 fileName {0}", fileName));

                                        // 检查文件扩展名
                                        if (extension == ".jpg" || extension == ".jpeg" || extension == ".bmp" || extension == ".png")
                                        {
                                            // 找到最后两个下划线的位置
                                            int lastUnderscoreIndex = fileName.LastIndexOf('_');
                                            int secondLastUnderscoreIndex = fileName.LastIndexOf('_', lastUnderscoreIndex - 1);

                                            if (lastUnderscoreIndex > 0 && secondLastUnderscoreIndex > 0)
                                            {
                                                // 提取时间戳和光源编号
                                                string timestampStr = fileName.Substring(secondLastUnderscoreIndex + 1, lastUnderscoreIndex - secondLastUnderscoreIndex - 1);
                                                string lightSourceStr = fileName.Substring(lastUnderscoreIndex + 1);

                                                // 只取时间戳的前17个字符
                                                if (timestampStr.Length >= 17)
                                                {
                                                    timestampStr = timestampStr.Substring(0, 17);
                                                }

                                                if (DateTime.TryParseExact(timestampStr, "yyyyMMddHHmmssfff", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime timestamp) &&
                                                    int.TryParse(lightSourceStr, out int lightSource))
                                                {
                                                    if (!latestImages.ContainsKey(lightSource) || latestImages[lightSource].timestamp < timestamp)
                                                    {
                                                        latestImages[lightSource] = (timestamp, file);
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    if (latestImages.Count > 0)
                                    {
                                        List<string> latestImagePaths = latestImages.OrderBy(y => y.Key).Select(kvp => $"{kvp.Value.filePath}").ToList();

                                        imageFiles = latestImagePaths;
                                    }
                                }
                                catch (Exception ex)
                                {
                                }
                            }

                            // 定义保存图片的本地路径
                            string strLocalPath = string.Format("D:\\avi_images");
                            if (false == Directory.Exists(strLocalPath))
                            {
                                Directory.CreateDirectory(strLocalPath);

                                if (false == Directory.Exists(strLocalPath))
                                {
                                    m_parent.m_global.m_log_presenter.Log(string.Format("创建本地目录失败：{0}", strLocalPath));
                                    return bSuccess;
                                }
                            }

                            if (true == string.IsNullOrEmpty(tray_info.machine_id))
                            {
                                m_parent.m_global.m_log_presenter.Log(string.Format("机器编号为空，无法创建本地目录"));
                                return bSuccess;
                            }

                            strLocalPath = Path.Combine(strLocalPath, tray_info.machine_id);
                            if (false == Directory.Exists(strLocalPath))
                            {
                                Directory.CreateDirectory(strLocalPath);

                                if (false == Directory.Exists(strLocalPath))
                                {
                                    m_parent.m_global.m_log_presenter.Log(string.Format("创建本地目录失败：{0}", strLocalPath));
                                    return bSuccess;
                                }
                            }

                            // 获取今日日期，创建本地目录
                            string strToday = DateTime.Now.ToString("yyyy_MM_dd");

                            strLocalPath = Path.Combine(strLocalPath, strToday);
                            if (false == Directory.Exists(strLocalPath))
                            {
                                Directory.CreateDirectory(strLocalPath);

                                if (false == Directory.Exists(strLocalPath))
                                {
                                    m_parent.m_global.m_log_presenter.Log(string.Format("创建本地目录失败：{0}", strLocalPath));
                                    return bSuccess;
                                }
                            }

                            // 创建二维码命名的目录
                            strLocalPath = Path.Combine(strLocalPath, product.barcode);

                            if (false == Directory.Exists(strLocalPath))
                            {
                                Directory.CreateDirectory(strLocalPath);

                                if (false == Directory.Exists(strLocalPath))
                                {
                                    m_parent.m_global.m_log_presenter.Log(string.Format("创建本地目录失败：{0}", strLocalPath));
                                    return bSuccess;
                                }
                            }

                            string strLocalSideDir = strLocalPath + "\\" + strSidePath.Substring(strSidePath.Length - 1);

                            for (int n = 0; n < 2; n++)
                            {
                                if (n==1&&strLocalSideDir.EndsWith("A"))
                                {
                                    strLocalSideDir = strLocalSideDir.Substring(0, strLocalSideDir.Length - 1) + "B";
                                }

                                // 创建正反面目录
                                if (false == Directory.Exists(strLocalSideDir))
                                {
                                        Directory.CreateDirectory(strLocalSideDir);
                                }

                                // 拷贝文件到本地路径
                                for (int j = 0; j < imageFiles.Count; j++)
                                {
                                    string filePath = imageFiles[j];

                                    if (m_parent.m_global.m_strProductSubType == "glue_check")
                                    {
                                        if (strLocalSideDir.EndsWith("B"))
                                        {
                                            filePath = filePath.Replace("sourceA", "sourceB");
                                            filePath = filePath.Replace("ProA", "ProB");
                                        }
                                    }

                                    string fileName = Path.GetFileName(imageFiles[j]);

                                    string destFile = Path.Combine(strLocalSideDir, fileName);

                                    //File.Copy(filePath, destFile, true);

                                    if (null == product.m_list_local_imageA_paths_for_channel1)
                                        product.m_list_local_imageA_paths_for_channel1 = new List<string>();
                                    if (null == product.m_list_remote_imageA_paths_for_channel1)
                                        product.m_list_remote_imageA_paths_for_channel1 = new List<string>();
                                    if (null == product.m_list_local_imageA_paths_for_channel2)
                                        product.m_list_local_imageA_paths_for_channel2 = new List<string>();
                                    if (null == product.m_list_remote_imageA_paths_for_channel2)
                                        product.m_list_remote_imageA_paths_for_channel2 = new List<string>();

                                    if (null == product.m_list_local_imageB_paths_for_channel1)
                                        product.m_list_local_imageB_paths_for_channel1 = new List<string>();
                                    if (null == product.m_list_remote_imageB_paths_for_channel1)
                                        product.m_list_remote_imageB_paths_for_channel1 = new List<string>();
                                    if (null == product.m_list_local_imageB_paths_for_channel2)
                                        product.m_list_local_imageB_paths_for_channel2 = new List<string>();
                                    if (null == product.m_list_remote_imageB_paths_for_channel2)
                                        product.m_list_remote_imageB_paths_for_channel2 = new List<string>();

                                    if (null == product.m_list_local_imageC_paths_for_channel1)
                                        product.m_list_local_imageC_paths_for_channel1 = new List<string>();
                                    if (null == product.m_list_remote_imageC_paths_for_channel1)
                                        product.m_list_remote_imageC_paths_for_channel1 = new List<string>();
                                    if (null == product.m_list_local_imageC_paths_for_channel2)
                                        product.m_list_local_imageC_paths_for_channel2 = new List<string>();
                                    if (null == product.m_list_remote_imageC_paths_for_channel2)
                                        product.m_list_remote_imageC_paths_for_channel2 = new List<string>();

                                    if (null == product.m_list_local_imageD_paths_for_channel1)
                                        product.m_list_local_imageD_paths_for_channel1 = new List<string>();
                                    if (null == product.m_list_remote_imageD_paths_for_channel1)
                                        product.m_list_remote_imageD_paths_for_channel1 = new List<string>();
                                    if (null == product.m_list_local_imageD_paths_for_channel2)
                                        product.m_list_local_imageD_paths_for_channel2 = new List<string>();
                                    if (null == product.m_list_remote_imageD_paths_for_channel2)
                                        product.m_list_remote_imageD_paths_for_channel2 = new List<string>();

                                    if (m_parent.m_global.m_strProductType == "dock")
                                    {
                                        if (strLocalSideDir.EndsWith("A") && bValidSides[0] == true)
                                        {
                                            product.m_list_local_imageA_paths_for_channel1.Add(destFile);
                                            product.m_list_remote_imageA_paths_for_channel1.Add(filePath);
                                        }
                                        else if (strLocalSideDir.EndsWith("B") && bValidSides[1] == true)
                                        {
                                            product.m_list_local_imageB_paths_for_channel1.Add(destFile);
                                            product.m_list_remote_imageB_paths_for_channel1.Add(filePath);
                                        }
                                        else if (strLocalSideDir.EndsWith("C") && bValidSides[2] == true)
                                        {
                                            product.m_list_local_imageC_paths_for_channel1.Add(destFile);
                                            product.m_list_remote_imageC_paths_for_channel1.Add(filePath);
                                        }
                                        else if (strLocalSideDir.EndsWith("D") && bValidSides[3] == true)
                                        {
                                            product.m_list_local_imageD_paths_for_channel1.Add(destFile);
                                            product.m_list_remote_imageD_paths_for_channel1.Add(filePath);
                                        }
                                    }
                                }
                            }

                            if (imageFiles.Count > 0)
                            {
                                bSuccess = true;

                                nNumOfAccessedImages += imageFiles.Count;
                            }
                        }
                        catch (Exception ex)
                        {
                            m_parent.m_global.m_log_presenter.Log(string.Format("处理AVI产品信息失败。图片路径：{0}。错误信息：{1}", strSharedImgDir, ex.Message));

                            bSuccess = false;
                        }
                    }
                }

                m_parent.m_global.m_log_presenter.Log(string.Format("共检索到{0}张图片", nNumOfAccessedImages));
            }

            // 后台线程拷贝图片到本地
            if (false == m_parent.m_global.m_bIsImagesCloningThreadInProcess[0] && false == bInsertToDatabase)
            {
                m_parent.m_global.m_bIsImagesCloningThreadInProcess[0] = true;
                m_parent.m_global.m_bIsDeletingImages = true;

                // 开启线程，将图片拷贝到本地
                new Thread(thread_clone_images_for_Dock).Start(0);

                Thread.Sleep(300);
            }

            return true;
        }

        // 处理来自AVI系统的托盘信息，用于Nova，主要处理状态数组和显示图片
        public void HandleTrayInfoFromAVIForNova_InitFlagsAndShowImage(AVITrayInfo tray_info)
        {
            // 拷贝m_current_AVI_product_info到m_current_MES_product_info
            m_parent.m_global.m_current_MES_tray_info = new MESTrayInfo();
            m_parent.m_global.m_current_MES_tray_info.init_data(tray_info);

            int nTrayRows = m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Nova.Row;
            int nTrayCols = m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Nova.Col;

            m_parent.m_global.m_panel_recheck_result = new PanelRecheckResult();
            m_parent.m_global.m_panel_recheck_result.summarylist = new List<SummaryList>();
            m_parent.m_global.m_panel_recheck_result.detaillist = new List<DetailList>();

            // 更新复判结果状态数组
            int nNumOfDefectedProducts = 0;
            List<int> list_indices_of_defected_products = new List<int>();
            for (int n = 0; n < m_parent.m_global.m_current_tray_info_for_Nova.Products.Count; n++)
            {
                int nRow = m_parent.m_global.m_current_tray_info_for_Nova.Products[n].PosRow - 1;
                int nCol = m_parent.m_global.m_current_tray_info_for_Nova.Products[n].PosCol - 1;

                if (m_parent.m_global.m_current_tray_info_for_Nova.Products[n].Defects != null)
                {
                    if (m_parent.m_global.m_current_tray_info_for_Nova.Products[n].Defects.Count > 0 && ((nCol + nRow * nTrayCols) < nTrayRows * nTrayCols))
                    {
                        nNumOfDefectedProducts++;
                        list_indices_of_defected_products.Add(nCol + nRow * nTrayCols);

                        RecheckResult[] recheck_flags = new RecheckResult[m_parent.m_global.m_current_tray_info_for_Nova.Products[n].Defects.Count];

						// 处理复判pass项flag
						for (int i = 0; i < m_parent.m_global.m_current_tray_info_for_Nova.Products[n].Defects.Count; i++)
						{
							bool bUncheckable = false;
							for (int j = 0; j < m_parent.m_global.m_uncheckable_pass_types.Count; j++)
							{
								if (m_parent.m_global.m_current_tray_info_for_Nova.Products[n].Defects[i].Type == m_parent.m_global.m_uncheckable_pass_types[j]
									&& m_parent.m_global.m_uncheckable_pass_enable_flags[j] == true)
								{
									bUncheckable = true;
								}
							}

							if (bUncheckable)
							{
								recheck_flags[i] = RecheckResult.OK;
							}
							else
                            {
                                recheck_flags[i] = RecheckResult.NotChecked;
                            }
						}

                        m_parent.m_global.m_current_MES_tray_info.m_list_recheck_flags_by_defect.Add(recheck_flags);

                        m_parent.m_global.m_current_MES_tray_info.m_check_flags_by_product[nCol + nRow * nTrayCols] = RecheckResult.NotChecked;
                    }
                }
            }

            // 不可复判项
            for (int n = 0; n < m_parent.m_global.m_current_tray_info_for_Nova.products_with_unrecheckable_defect.Count; n++)
            {
                int nRow = m_parent.m_global.m_current_tray_info_for_Nova.products_with_unrecheckable_defect[n].PosRow - 1;
                int nCol = m_parent.m_global.m_current_tray_info_for_Nova.products_with_unrecheckable_defect[n].PosCol - 1;

                m_parent.m_global.m_current_MES_tray_info.m_check_flags_by_product[nCol + nRow * nTrayCols] = RecheckResult.Unrecheckable;
            }

            // 不可复判OK项，不需要整片料判ok
            for (int n = 0; n < m_parent.m_global.m_current_tray_info_for_Nova.products_with_unrecheckable_pass.Count; n++)
            {
                int nRow = m_parent.m_global.m_current_tray_info_for_Nova.products_with_unrecheckable_pass[n].PosRow - 1;
                int nCol = m_parent.m_global.m_current_tray_info_for_Nova.products_with_unrecheckable_pass[n].PosCol - 1;

                int defectCount = 0;
                var defects = m_parent.m_global.m_current_tray_info_for_Nova.products_with_unrecheckable_pass[n].Defects;
                for (int i = 0; i < defects.Count; i++)
                {
                    for (int j = 0; j < m_parent.m_global.m_uncheckable_pass_types.Count; j++)
                    {
                        if (m_parent.m_global.m_uncheckable_pass_enable_flags[j] == true && defects[i].Type == m_parent.m_global.m_uncheckable_pass_types[j])
                        {
                            defectCount++;
                        }
                    }

                }
                if (m_parent.m_global.m_current_tray_info_for_Nova.products_with_unrecheckable_pass[n].Defects.Count == defectCount)
                    m_parent.m_global.m_current_MES_tray_info.m_check_flags_by_product[nCol + nRow * nTrayCols] = RecheckResult.OK;
            }

            // MES过站失败产品信息
            for (int k = 0; k < m_parent.m_global.m_current_tray_info_for_Nova.MES_failed_products.Count; k++)
            {
                int nRow = m_parent.m_global.m_current_tray_info_for_Nova.MES_failed_products[k].PosRow - 1;
                int nCol = m_parent.m_global.m_current_tray_info_for_Nova.MES_failed_products[k].PosCol - 1;

                m_parent.m_global.m_current_MES_tray_info.m_check_flags_by_product[nCol + nRow * nTrayCols] = RecheckResult.MES_NG;
            }

            // ET产品信息
            for (int k = 0; k < m_parent.m_global.m_current_tray_info_for_Nova.ET_products.Count; k++)
            {
                int nRow = m_parent.m_global.m_current_tray_info_for_Nova.ET_products[k].PosRow - 1;
                int nCol = m_parent.m_global.m_current_tray_info_for_Nova.ET_products[k].PosCol - 1;

                m_parent.m_global.m_current_MES_tray_info.m_check_flags_by_product[nCol + nRow * nTrayCols] = RecheckResult.ET;
            }

            // NotRecieved产品，Defect为空产品
            for (int k = 0; k < m_parent.m_global.m_current_tray_info_for_Nova.NotRecievedPorducts.Count; k++)
            {
                int nRow = m_parent.m_global.m_current_tray_info_for_Nova.NotRecievedPorducts[k].PosRow - 1;
                int nCol = m_parent.m_global.m_current_tray_info_for_Nova.NotRecievedPorducts[k].PosCol - 1;

                m_parent.m_global.m_current_MES_tray_info.m_check_flags_by_product[nCol + nRow * nTrayCols] = RecheckResult.NotReceived;
            }

            // 将未接收到的穴位显示为红色
            if (true == m_parent.m_global.m_bShowUnreceivedDataAsRedSlot)
            {
                for (int m = 0; m < nTrayRows; m++)
                {
                    for (int n = 0; n < nTrayCols; n++)
                    {
                        bool bReceived = false;

                        // 不可复判项
                        for (int k = 0; k < m_parent.m_global.m_current_tray_info_for_Nova.products_with_unrecheckable_defect.Count; k++)
                        {
                            int nRow = m_parent.m_global.m_current_tray_info_for_Nova.products_with_unrecheckable_defect[k].PosRow - 1;
                            int nCol = m_parent.m_global.m_current_tray_info_for_Nova.products_with_unrecheckable_defect[k].PosCol - 1;

                            if (n == nCol && m == nRow)
                            {
                                bReceived = true;
                                break;
                            }
                        }

                        // 不可复判OK项
                        for(int k = 0;k<m_parent.m_global.m_current_tray_info_for_Nova.products_with_unrecheckable_pass.Count; k++)
						{
							int nRow = m_parent.m_global.m_current_tray_info_for_Nova.products_with_unrecheckable_pass[k].PosRow - 1;
							int nCol = m_parent.m_global.m_current_tray_info_for_Nova.products_with_unrecheckable_pass[k].PosCol - 1;

                            if(n==nCol && m == nRow)
                            {
                                bReceived = true;
                                break;
                            }
						}

                        // MES过站失败产品信息
                        for (int k = 0; k < m_parent.m_global.m_current_tray_info_for_Nova.MES_failed_products.Count; k++)
                        {
                            int nRow = m_parent.m_global.m_current_tray_info_for_Nova.MES_failed_products[k].PosRow - 1;
                            int nCol = m_parent.m_global.m_current_tray_info_for_Nova.MES_failed_products[k].PosCol - 1;

                            if (n == nCol && m == nRow)
                            {
                                bReceived = true;
                                break;
                            }
                        }

                        // ET产品信息
                        for (int k = 0; k < m_parent.m_global.m_current_tray_info_for_Nova.ET_products.Count; k++)
                        {
                            int nRow = m_parent.m_global.m_current_tray_info_for_Nova.ET_products[k].PosRow - 1;
                            int nCol = m_parent.m_global.m_current_tray_info_for_Nova.ET_products[k].PosCol - 1;

                            if (n == nCol && m == nRow)
                            {
                                bReceived = true;
                                break;
                            }
                        }

                        if (false == bReceived && m_parent.m_global.m_current_MES_tray_info.m_check_flags_by_product[n + m * nTrayCols] != RecheckResult.NotChecked)
                        {
                            m_parent.m_global.m_current_MES_tray_info.m_check_flags_by_product[n + m * nTrayCols] = RecheckResult.NotReceived;
                        }
                    }
                }
            }

            m_parent.m_global.m_current_MES_tray_info.m_array_indices_of_defected_products = new int[nNumOfDefectedProducts];
            m_parent.m_global.m_current_MES_tray_info.m_array_barcodes_of_defected_products = new string[nNumOfDefectedProducts];

            // 保存有缺陷的产品的索引
            for (int n = 0; n < list_indices_of_defected_products.Count; n++)
            {
                m_parent.m_global.m_current_MES_tray_info.m_array_indices_of_defected_products[n] = list_indices_of_defected_products[n];
                m_parent.m_global.m_current_MES_tray_info.m_array_barcodes_of_defected_products[n] = m_parent.m_global.m_current_tray_info_for_Nova.Products[n].BarCode;
            }

            // 更新Canvas复判结果状态数组
            m_parent.page_HomeView.m_panel_canvas.clone_check_results(m_parent.m_global.m_current_MES_tray_info.m_check_flags_by_product);

            // 更新UI
            Application.Current.Dispatcher.Invoke(() =>
            {
                // 更新画布
                if (true)
                {
                    m_parent.page_HomeView.m_panel_canvas.set_panel_rows_and_columns(nTrayRows, nTrayCols);

                    m_parent.page_HomeView.m_panel_canvas.m_bForceRedraw = true;
                    m_parent.page_HomeView.m_panel_canvas.OnWindowSizeChanged(new object(), new EventArgs());
                }

                // 创建按钮
                m_parent.page_HomeView.GenerateCircularButtonsInGridContainer(m_parent.page_HomeView.grid_CircularButtonContainer,
                    nTrayRows, nTrayCols, m_parent.m_global.m_current_MES_tray_info.m_check_flags_by_product);

                // 更新产品信息
                m_parent.page_HomeView.textblock_MachineID.Text = m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Nova.Mid;
                m_parent.page_HomeView.textblock_ProductID.Text = m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Nova.ProductId;
                //m_parent.page_HomeView.textblock_ProductID.Text = tray_info.ProductId;
                m_parent.page_HomeView.textblock_OperatorID.Text = m_parent.m_global.m_strCurrentOperatorID;

                //m_parent.m_global.m_recheck_statistics.m_nTotalPanels++;
                m_parent.m_global.m_recheck_statistics.Save();

                m_parent.page_HomeView.textblock_TotalPanels.Text = m_parent.m_global.m_recheck_statistics.m_nTotalPanels.ToString();

                // 更新复判列表
                for (int i = 0; i < m_parent.m_global.m_current_MES_tray_info.m_array_indices_of_defected_products.Length; i++)
                {
                    int nID = m_parent.m_global.m_current_MES_tray_info.m_array_indices_of_defected_products[i];

                    m_parent.page_HomeView.gridview_RecheckItemNo.Rows.Add((nID + 1).ToString());
                    m_parent.page_HomeView.gridview_RecheckItemBarcode.Rows.Add(m_parent.m_global.m_current_MES_tray_info.m_array_barcodes_of_defected_products[i]);
                }

                // 选中第一个有缺陷的产品
                if (m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Nova.Products.Count > 0)
                {
                    m_parent.page_HomeView.SelectDefectedProductBarcode(0);
                }

                // 选中第一个有缺陷的产品
                if (m_parent.m_global.m_current_MES_tray_info.m_array_indices_of_defected_products.Length > 0)
                    m_parent.page_HomeView.SelectProduct(m_parent.m_global.m_current_MES_tray_info.m_array_indices_of_defected_products[0]);



            });
        }

        // 处理来自AVI系统的托盘信息，用于Dock或CL5，主要处理状态数组和显示图片
        public void HandleTrayInfoFromAVIForDockOrCL5_InitFlagsAndShowImage(TrayInfo tray_info, string strBarcodeForSinglePieceMode)
        {
            // 拷贝m_current_AVI_product_info到m_current_MES_product_info
            m_parent.m_global.m_current_MES_tray_info = new MESTrayInfo();
            m_parent.m_global.m_current_MES_tray_info.init_data(tray_info);

            int nTrayRows = m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Dock.total_rows;
            int nTrayCols = m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Dock.total_columns;

            m_parent.m_global.m_panel_recheck_result = new PanelRecheckResult();
            m_parent.m_global.m_panel_recheck_result.summarylist = new List<SummaryList>();
            m_parent.m_global.m_panel_recheck_result.detaillist = new List<DetailList>();

            // 更新复判结果状态数组
            int nNumOfDefectedProducts = 0;
            int nNumOfETProducts = 0;
            List<int> list_indices_of_defected_products = new List<int>();
            List<int> list_indices_of_ET_products = new List<int>();
            for (int n = 0; n < m_parent.m_global.m_current_tray_info_for_Dock.products.Count; n++)
            {
                int nRow = m_parent.m_global.m_current_tray_info_for_Dock.products[n].row - 1;
                int nCol = m_parent.m_global.m_current_tray_info_for_Dock.products[n].column - 1;

                if (m_parent.m_global.m_current_tray_info_for_Dock.products[n].defects != null)
                {
                    if (m_parent.m_global.m_current_tray_info_for_Dock.products[n].defects.Count > 0 && ((nCol + nRow * nTrayCols) < nTrayRows * nTrayCols))
                    {
                        list_indices_of_defected_products.Add(nCol + nRow * nTrayCols);

                        nNumOfDefectedProducts++;

                        RecheckResult[] recheck_flags = new RecheckResult[m_parent.m_global.m_current_tray_info_for_Dock.products[n].defects.Count];

                        // 处理不可复判项flag
                        for (int i = 0; i < m_parent.m_global.m_current_tray_info_for_Dock.products[n].defects.Count; i++)
                        {
                            bool bUncheckable = false;
                            for (int j = 0; j < m_parent.m_global.m_uncheckable_defect_types.Count; j++)
                            {
                                if (m_parent.m_global.m_current_tray_info_for_Dock.products[n].defects[i].type == m_parent.m_global.m_uncheckable_defect_types[j]
                                    && m_parent.m_global.m_uncheckable_defect_enable_flags[j] == true)
                                {
                                    bUncheckable = true;
                                }
                            }

                            if (bUncheckable)
                            {
                                recheck_flags[i] = RecheckResult.Unrecheckable;
                            }
                            else
                            {
                                recheck_flags[i] = RecheckResult.NotChecked;
                            }
                        }

                        m_parent.m_global.m_current_MES_tray_info.m_list_recheck_flags_by_defect.Add(recheck_flags);

                        m_parent.m_global.m_current_MES_tray_info.m_check_flags_by_product[nCol + nRow * nTrayCols] = RecheckResult.NotChecked;

                        // 判断是否无码
                        //if (m_parent.m_global.m_current_tray_info_for_Dock.products[n].barcode.IndexOf("nocode", StringComparison.OrdinalIgnoreCase) >= 0)
                        //{
                        //    m_parent.m_global.m_current_MES_tray_info.m_check_flags_by_product[nCol + nRow * nTrayCols] = RecheckResult.NoCode;
                        //}

                        if (m_parent.m_global.m_current_tray_info_for_Dock.products[n].r3.Contains("PASS") || m_parent.m_global.m_current_tray_info_for_Dock.products[n].is_ok_product == true)
                        {
                            m_parent.m_global.m_current_MES_tray_info.m_check_flags_by_product[nCol + nRow * nTrayCols] = RecheckResult.OK;
                        }

                        if (m_parent.m_global.m_bShowAIResultFromTransferStation)
                        {
                        if (m_parent.m_global.m_current_tray_info_for_Dock.products[n].r3.Contains("FAIL"))
                        {
                            m_parent.m_global.m_current_MES_tray_info.m_check_flags_by_product[nCol + nRow * nTrayCols] = RecheckResult.NG;
                        }
                        }

                        //else
                        //{
                        //    // 判断是否是不可复判项
                        //    for (int j = 0; j < m_parent.m_global.m_current_tray_info_for_Dock.products[n].defects.Count; j++)
                        //    {
                        //        for (int k = 0; k < m_parent.m_global.m_uncheckable_defect_types.Count; k++)
                        //        {
                        //            if (m_parent.m_global.m_uncheckable_defect_enable_flags[k] == true)
                        //            {
                        //                if (m_parent.m_global.m_current_tray_info_for_Dock.products[n].defects[j].type == m_parent.m_global.m_uncheckable_defect_types[k])
                        //                {
                        //                    m_parent.m_global.m_current_MES_tray_info.m_check_flags_by_product[nCol + nRow * nTrayCols] = RecheckResult.Unrecheckable;
                        //                    break;
                        //                }
                        //            }
                        //        }
                        //    }
                        //}
                    }
                }
            }

            // 不可复判项
            for (int n = 0; n < m_parent.m_global.m_current_tray_info_for_Dock.products_with_unrecheckable_defect.Count; n++)
            {
                int nRow = m_parent.m_global.m_current_tray_info_for_Dock.products_with_unrecheckable_defect[n].row - 1;
                int nCol = m_parent.m_global.m_current_tray_info_for_Dock.products_with_unrecheckable_defect[n].column - 1;

                m_parent.m_global.m_current_MES_tray_info.m_check_flags_by_product[nCol + nRow * nTrayCols] = RecheckResult.Unrecheckable;
            }

            // 定位失败的产品
            for (int k = 0; k < m_parent.m_global.m_current_tray_info_for_Dock.products_with_failed_positioning.Count; k++)
            {
                int nRow = m_parent.m_global.m_current_tray_info_for_Dock.products_with_failed_positioning[k].row - 1;
                int nCol = m_parent.m_global.m_current_tray_info_for_Dock.products_with_failed_positioning[k].column - 1;

                m_parent.m_global.m_current_MES_tray_info.m_check_flags_by_product[nCol + nRow * nTrayCols] = RecheckResult.FailedPositioning;
            }

            // 空穴
            for (int k = 0; k < m_parent.m_global.m_current_tray_info_for_Dock.empty_slots.Count; k++)
            {
                int nRow = m_parent.m_global.m_current_tray_info_for_Dock.empty_slots[k].row - 1;
                int nCol = m_parent.m_global.m_current_tray_info_for_Dock.empty_slots[k].column - 1;

                m_parent.m_global.m_current_MES_tray_info.m_check_flags_by_product[nCol + nRow * nTrayCols] = RecheckResult.EmptySlot;
            }

            // 无二维码nocode
            for (int k = 0; k < m_parent.m_global.m_current_tray_info_for_Dock.nocode_products.Count; k++)
            {
                int nRow = m_parent.m_global.m_current_tray_info_for_Dock.nocode_products[k].row - 1;
                int nCol = m_parent.m_global.m_current_tray_info_for_Dock.nocode_products[k].column - 1;

                m_parent.m_global.m_current_MES_tray_info.m_check_flags_by_product[nCol + nRow * nTrayCols] = RecheckResult.NoCode;
            }

            for (int k = 0; k < m_parent.m_global.m_current_tray_info_for_Dock.NotRecievedPorducts.Count; k++)
            {
                int nRow = m_parent.m_global.m_current_tray_info_for_Dock.NotRecievedPorducts[k].row - 1;
                int nCol = m_parent.m_global.m_current_tray_info_for_Dock.NotRecievedPorducts[k].column - 1;

                m_parent.m_global.m_current_MES_tray_info.m_check_flags_by_product[nCol + nRow * nTrayCols] = RecheckResult.NotReceived;
            }

            // OK产品
            for (int k = 0; k < m_parent.m_global.m_current_tray_info_for_Dock.OK_products.Count; k++)
            {
                int nRow = m_parent.m_global.m_current_tray_info_for_Dock.OK_products[k].row - 1;
                int nCol = m_parent.m_global.m_current_tray_info_for_Dock.OK_products[k].column - 1;

                m_parent.m_global.m_current_MES_tray_info.m_check_flags_by_product[nCol + nRow * nTrayCols] = RecheckResult.OK;
            }

            // MES过站失败产品信息
            for (int k = 0; k < m_parent.m_global.m_current_tray_info_for_Dock.MES_failed_products.Count; k++)
            {
                int nRow = m_parent.m_global.m_current_tray_info_for_Dock.MES_failed_products[k].row - 1;
                int nCol = m_parent.m_global.m_current_tray_info_for_Dock.MES_failed_products[k].column - 1;

                m_parent.m_global.m_current_MES_tray_info.m_check_flags_by_product[nCol + nRow * nTrayCols] = RecheckResult.MES_NG;
            }

            // ET产品
            for(int k = 0; k < m_parent.m_global.m_current_tray_info_for_Dock.ET_products.Count; k++)
            {
                int nRow = m_parent.m_global.m_current_tray_info_for_Dock.ET_products[k].row - 1;
                int nCol = m_parent.m_global.m_current_tray_info_for_Dock.ET_products[k].column - 1;

                m_parent.m_global.m_current_MES_tray_info.m_check_flags_by_product[nCol + nRow * nTrayCols] = RecheckResult.ET;
            }

            // 将未接收到的穴位显示为红色
            if (true == m_parent.m_global.m_bShowUnreceivedDataAsRedSlot)
            {
                for (int m = 0; m < nTrayRows; m++)
                {
                    for (int n = 0; n < nTrayCols; n++)
                    {
                        bool bReceived = false;

                        // 有缺陷的产品
                        for (int k = 0; k < m_parent.m_global.m_current_tray_info_for_Dock.products.Count; k++)
                        {
                            int nRow = m_parent.m_global.m_current_tray_info_for_Dock.products[k].row - 1;
                            int nCol = m_parent.m_global.m_current_tray_info_for_Dock.products[k].column - 1;

                            if (n == nCol && m == nRow)
                            {
                                bReceived = true;
                                break;
                            }
                        }

                        // 空穴
                        for (int k = 0; k < m_parent.m_global.m_current_tray_info_for_Dock.empty_slots.Count; k++)
                        {
                            int nRow = m_parent.m_global.m_current_tray_info_for_Dock.empty_slots[k].row - 1;
                            int nCol = m_parent.m_global.m_current_tray_info_for_Dock.empty_slots[k].column - 1;

                            if (n == nCol && m == nRow)
                            {
                                bReceived = true;
                                break;
                            }
                        }

                        // 无二维码nocode
                        for (int k = 0; k < m_parent.m_global.m_current_tray_info_for_Dock.nocode_products.Count; k++)
                        {
                            int nRow = m_parent.m_global.m_current_tray_info_for_Dock.nocode_products[k].row - 1;
                            int nCol = m_parent.m_global.m_current_tray_info_for_Dock.nocode_products[k].column - 1;

                            if (n == nCol && m == nRow)
                            {
                                bReceived = true;
                                break;
                            }
                        }

                        // 定位失败的产品
                        for (int k = 0; k < m_parent.m_global.m_current_tray_info_for_Dock.products_with_failed_positioning.Count; k++)
                        {
                            int nRow = m_parent.m_global.m_current_tray_info_for_Dock.products_with_failed_positioning[k].row - 1;
                            int nCol = m_parent.m_global.m_current_tray_info_for_Dock.products_with_failed_positioning[k].column - 1;

                            if (n == nCol && m == nRow)
                            {
                                bReceived = true;
                                break;
                            }
                        }

                        // ET产品
                        for (int k = 0; k < m_parent.m_global.m_current_tray_info_for_Dock.ET_products.Count; k++)
                        {
                            int nRow = m_parent.m_global.m_current_tray_info_for_Dock.ET_products[k].row - 1;
                            int nCol = m_parent.m_global.m_current_tray_info_for_Dock.ET_products[k].column - 1;

                            if (n == nCol && m == nRow)
                            {
                                bReceived = true;
                                break;
                            }
                        }

                        // OK产品
                        for (int k = 0; k < m_parent.m_global.m_current_tray_info_for_Dock.OK_products.Count; k++)
                        {
                            int nRow = m_parent.m_global.m_current_tray_info_for_Dock.OK_products[k].row - 1;
                            int nCol = m_parent.m_global.m_current_tray_info_for_Dock.OK_products[k].column - 1;

                            if (n == nCol && m == nRow)
                            {
                                bReceived = true;
                                break;
                            }
                        }

                        // 不可复判项
                        for (int k = 0; k < m_parent.m_global.m_current_tray_info_for_Dock.products_with_unrecheckable_defect.Count; k++)
                        {
                            int nRow = m_parent.m_global.m_current_tray_info_for_Dock.products_with_unrecheckable_defect[k].row - 1;
                            int nCol = m_parent.m_global.m_current_tray_info_for_Dock.products_with_unrecheckable_defect[k].column - 1;

                            if (n == nCol && m == nRow)
                            {
                                bReceived = true;
                                break;
                            }
                        }

                        // MES过站失败产品信息
                        for (int k = 0; k < m_parent.m_global.m_current_tray_info_for_Dock.MES_failed_products.Count; k++)
                        {
                            int nRow = m_parent.m_global.m_current_tray_info_for_Dock.MES_failed_products[k].row - 1;
                            int nCol = m_parent.m_global.m_current_tray_info_for_Dock.MES_failed_products[k].column - 1;

                            if (n == nCol && m == nRow)
                            {
                                bReceived = true;
                                break;
                            }
                        }

                        if (false == bReceived)
                        {
                            m_parent.m_global.m_current_MES_tray_info.m_check_flags_by_product[n + m * nTrayCols] = RecheckResult.NotReceived;
                        }
                    }
                }
            }

            m_parent.m_global.m_current_MES_tray_info.m_array_indices_of_ET_products = new int[nNumOfETProducts];
            m_parent.m_global.m_current_MES_tray_info.m_array_indices_of_defected_products = new int[nNumOfDefectedProducts];
            m_parent.m_global.m_current_MES_tray_info.m_array_barcodes_of_defected_products = new string[nNumOfDefectedProducts];

            // 保存有缺陷的产品的索引
            for (int n = 0; n < list_indices_of_defected_products.Count; n++)
            {
                m_parent.m_global.m_current_MES_tray_info.m_array_indices_of_defected_products[n] = list_indices_of_defected_products[n];
                m_parent.m_global.m_current_MES_tray_info.m_array_barcodes_of_defected_products[n] = m_parent.m_global.m_current_tray_info_for_Dock.products[n].barcode;
            }

            // 保存ET产品的索引
            for (int n = 0; n < list_indices_of_ET_products.Count; n++)
            {
                m_parent.m_global.m_current_MES_tray_info.m_array_indices_of_ET_products[n] = list_indices_of_ET_products[n];
            }

            // 更新Canvas复判结果状态数组
            m_parent.page_HomeView.m_panel_canvas.clone_check_results(m_parent.m_global.m_current_MES_tray_info.m_check_flags_by_product);

            // 更新UI
            Application.Current.Dispatcher.Invoke(() =>
            {
                // 更新画布
                if (true)
                {
                    m_parent.m_global.m_strLastOpenImagePath = "";

                    m_parent.page_HomeView.m_panel_canvas.set_panel_rows_and_columns(nTrayRows, nTrayCols);

                    m_parent.page_HomeView.m_panel_canvas.m_bForceRedraw = true;
                    m_parent.page_HomeView.m_panel_canvas.OnWindowSizeChanged(new object(), new EventArgs());
                }

                // 创建按钮
                m_parent.page_HomeView.GenerateCircularButtonsInGridContainer(m_parent.page_HomeView.grid_CircularButtonContainer,
                    nTrayRows, nTrayCols, m_parent.m_global.m_current_MES_tray_info.m_check_flags_by_product);

                // 更新产品信息
                m_parent.page_HomeView.textblock_MachineID.Text = m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Dock.machine_id;
                //m_parent.page_HomeView.textblock_ProductID.Text = m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Nova.ProductId;
                //m_parent.page_HomeView.textblock_ProductID.Text = tray_info.ProductId;
                m_parent.page_HomeView.textblock_OperatorID.Text = m_parent.m_global.m_strCurrentOperatorID;

                //m_parent.m_global.m_recheck_statistics.m_nTotalPanels++;
                m_parent.m_global.m_recheck_statistics.Save();

                m_parent.page_HomeView.textblock_TotalPanels.Text = m_parent.m_global.m_recheck_statistics.m_nTotalPanels.ToString();

                // 更新复判列表
                for (int i = 0; i < m_parent.m_global.m_current_MES_tray_info.m_array_indices_of_defected_products.Length; i++)
                {
                    int nID = m_parent.m_global.m_current_MES_tray_info.m_array_indices_of_defected_products[i];

                    m_parent.page_HomeView.gridview_RecheckItemNo.Rows.Add((nID + 1).ToString());
                    m_parent.page_HomeView.gridview_RecheckItemBarcode.Rows.Add(m_parent.m_global.m_current_MES_tray_info.m_array_barcodes_of_defected_products[i]);
                }

                // 选中第一个有缺陷的产品
                if (m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Dock.products.Count > 0)
                {
                    m_parent.page_HomeView.SelectDefectedProductBarcode(0);
                }

                m_parent.page_HomeView.render_tray_button_color();

                // 选中第一个有缺陷的产品
                if (m_parent.m_global.m_current_MES_tray_info.m_array_indices_of_defected_products.Length > 0)
                {
                    if (m_parent.m_global.m_nRecheckDisplayMode == 0)
                        m_parent.page_HomeView.SelectProduct(m_parent.m_global.m_current_MES_tray_info.m_array_indices_of_defected_products[0]);
                    else
                        m_parent.page_HomeView.SelectProductInSinglePieceMode(strBarcodeForSinglePieceMode);
                }
            });
        }

        // AI复判产品信息入队列
        public void EnqueueProductInfosWaitSubmittoAI(TrayInfo trayInfo)
        {
            var queue = m_parent.m_global.m_MES_service.m_productInfos_waitSubmitToAI_queue;
            queue.Clear();

            if (m_parent.m_global.m_bSubmitOKProductsToAI)
            {
                foreach(var p in trayInfo.OK_products)
                {
                    queue.Enqueue(p);
                }
            }

            foreach(var p in trayInfo.products)
            {
                queue.Enqueue(p);
            }

            // 不可复判项应该不用向AI提交复判
            //foreach(var p in trayInfo.products_with_unrecheckable_defect)
            //{
            //    queue.Enqueue(p);
            //}
        }

        // 后台线程：拷贝图片到本地
        public void thread_clone_images_for_Nova()
        {
            //Thread.Sleep(500);

            // 删除图片
            try
            {
                var products = m_parent.m_global.m_current_tray_info_for_Nova.Products;
                for (int i = 0; i < products.Count; i++)
                {
                    var product = products[i];

                    // 删除图片
                    DeleteImages(product.m_list_local_imageA_paths_for_channel1);
                    DeleteImages(product.m_list_local_imageA_paths_for_channel2);
                    DeleteImages(product.m_list_local_imageB_paths_for_channel1);
                    DeleteImages(product.m_list_local_imageB_paths_for_channel2);
                    DeleteImages(product.m_list_local_imageC_paths_for_channel1);
                    DeleteImages(product.m_list_local_imageC_paths_for_channel2);

                    //m_parent.m_global.m_log_presenter.Log(string.Format("后台线程：捞取第{0}个产品的图片到本地", i + 1));
                }
            }
            catch (Exception ex)
            {
                //m_parent.m_global.m_log_presenter.Log(string.Format("删除图片失败失败。错误信息：{0}", ex.Message));
            }

            if (false)
            {
                // 从tray_info中获取产品信息
                for (int i = 0; i < m_parent.m_global.m_current_tray_info_for_Nova.Products.Count; i++)
                {
                    try
                    {
                        // 遍历product.m_list_local_imageA_paths，拷贝图片到本地
                        if (null != m_parent.m_global.m_current_tray_info_for_Nova.Products[i].m_list_local_imageA_paths_for_channel1
                            && null != m_parent.m_global.m_current_tray_info_for_Nova.Products[i].m_list_remote_imageA_paths_for_channel1)
                        {
                            for (int j = 0; j < m_parent.m_global.m_current_tray_info_for_Nova.Products[i].m_list_local_imageA_paths_for_channel1.Count; j++)
                            {
                                if (j >= m_parent.m_global.m_current_tray_info_for_Nova.Products[i].m_list_remote_imageA_paths_for_channel1.Count)
                                {
                                    break;
                                }
                                if (m_parent.m_global.m_current_tray_info_for_Nova.Products[i].m_list_remote_imageA_paths_for_channel1[j].Length == 0)
                                {
                                    continue;
                                }

                                string strSourceFile = m_parent.m_global.m_current_tray_info_for_Nova.Products[i].m_list_remote_imageA_paths_for_channel1[j];
                                string strNewPath = m_parent.m_global.m_current_tray_info_for_Nova.Products[i].m_list_local_imageA_paths_for_channel1[j];

                                if (File.Exists(strNewPath))
                                    File.Delete(strNewPath);
                            }
                        }

                        if (null != m_parent.m_global.m_current_tray_info_for_Nova.Products[i].m_list_local_imageA_paths_for_channel2
                            && null != m_parent.m_global.m_current_tray_info_for_Nova.Products[i].m_list_remote_imageA_paths_for_channel2)
                        {
                            for (int j = 0; j < m_parent.m_global.m_current_tray_info_for_Nova.Products[i].m_list_local_imageA_paths_for_channel2.Count; j++)
                            {
                                if (j >= m_parent.m_global.m_current_tray_info_for_Nova.Products[i].m_list_remote_imageA_paths_for_channel2.Count)
                                {
                                    break;
                                }
                                if (m_parent.m_global.m_current_tray_info_for_Nova.Products[i].m_list_remote_imageA_paths_for_channel2[j].Length == 0)
                                {
                                    continue;
                                }

                                string strSourceFile = m_parent.m_global.m_current_tray_info_for_Nova.Products[i].m_list_remote_imageA_paths_for_channel2[j];
                                string strNewPath = m_parent.m_global.m_current_tray_info_for_Nova.Products[i].m_list_local_imageA_paths_for_channel2[j];

                                if (File.Exists(strNewPath))
                                    File.Delete(strNewPath);
                            }
                        }

                        // 遍历product.m_list_local_imageB_paths，拷贝图片到本地
                        if (null != m_parent.m_global.m_current_tray_info_for_Nova.Products[i].m_list_local_imageB_paths_for_channel1
                            && null != m_parent.m_global.m_current_tray_info_for_Nova.Products[i].m_list_remote_imageB_paths_for_channel1)
                        {
                            for (int j = 0; j < m_parent.m_global.m_current_tray_info_for_Nova.Products[i].m_list_local_imageB_paths_for_channel1.Count; j++)
                            {
                                if (j >= m_parent.m_global.m_current_tray_info_for_Nova.Products[i].m_list_remote_imageB_paths_for_channel1.Count)
                                {
                                    break;
                                }
                                if (m_parent.m_global.m_current_tray_info_for_Nova.Products[i].m_list_remote_imageB_paths_for_channel1[j].Length == 0)
                                {
                                    continue;
                                }

                                string strSourceFile = m_parent.m_global.m_current_tray_info_for_Nova.Products[i].m_list_remote_imageB_paths_for_channel1[j];
                                string strNewPath = m_parent.m_global.m_current_tray_info_for_Nova.Products[i].m_list_local_imageB_paths_for_channel1[j];

                                if (File.Exists(strNewPath))
                                    File.Delete(strNewPath);
                            }
                        }

                        if (null != m_parent.m_global.m_current_tray_info_for_Nova.Products[i].m_list_local_imageB_paths_for_channel2
                            && null != m_parent.m_global.m_current_tray_info_for_Nova.Products[i].m_list_remote_imageB_paths_for_channel2)
                        {
                            for (int j = 0; j < m_parent.m_global.m_current_tray_info_for_Nova.Products[i].m_list_local_imageB_paths_for_channel2.Count; j++)
                            {
                                if (j >= m_parent.m_global.m_current_tray_info_for_Nova.Products[i].m_list_remote_imageB_paths_for_channel2.Count)
                                {
                                    break;
                                }
                                if (m_parent.m_global.m_current_tray_info_for_Nova.Products[i].m_list_remote_imageB_paths_for_channel2[j].Length == 0)
                                {
                                    continue;
                                }

                                string strSourceFile = m_parent.m_global.m_current_tray_info_for_Nova.Products[i].m_list_remote_imageB_paths_for_channel2[j];
                                string strNewPath = m_parent.m_global.m_current_tray_info_for_Nova.Products[i].m_list_local_imageB_paths_for_channel2[j];

                                if (File.Exists(strNewPath))
                                    File.Delete(strNewPath);
                            }
                        }

                        //m_parent.m_global.m_log_presenter.Log(string.Format("后台线程：捞取第{0}个产品的图片到本地", i + 1));
                    }
                    catch (Exception ex)
                    {
                        //m_parent.m_global.m_log_presenter.Log(string.Format("删除图片失败失败。错误信息：{0}", ex.Message));
                    }
                }
            }

            // 拷贝图片到本地
            try
            {
                var products = m_parent.m_global.m_current_tray_info_for_Nova.Products;
                for (int i = 0; i < products.Count; i++)
                {
                    var product = products[i];

                    // Copy images for channel 1 and channel 2 for image types A, B, and C
                    CopyImages(product.m_list_local_imageA_paths_for_channel1, product.m_list_remote_imageA_paths_for_channel1);
                    CopyImages(product.m_list_local_imageA_paths_for_channel2, product.m_list_remote_imageA_paths_for_channel2);
                    CopyImages(product.m_list_local_imageB_paths_for_channel1, product.m_list_remote_imageB_paths_for_channel1);
                    CopyImages(product.m_list_local_imageB_paths_for_channel2, product.m_list_remote_imageB_paths_for_channel2);
                    CopyImages(product.m_list_local_imageC_paths_for_channel1, product.m_list_remote_imageC_paths_for_channel1);
                    CopyImages(product.m_list_local_imageC_paths_for_channel2, product.m_list_remote_imageC_paths_for_channel2);

                    m_parent.m_global.m_log_presenter.Log(string.Format("后台线程：捞取第{0}个产品的图片到本地", i + 1));
                }
            }
            catch (Exception ex)
            {
                m_parent.m_global.m_log_presenter.Log(string.Format("拷贝图片到本地失败。错误信息：{0}", ex.Message));
            }

            if (false)
            {
                // 从tray_info中获取产品信息
                for (int i = 0; i < m_parent.m_global.m_current_tray_info_for_Nova.Products.Count; i++)
                {
                    try
                    {
                        // 遍历product.m_list_local_imageA_paths，拷贝图片到本地
                        if (null != m_parent.m_global.m_current_tray_info_for_Nova.Products[i].m_list_local_imageA_paths_for_channel1
                            && null != m_parent.m_global.m_current_tray_info_for_Nova.Products[i].m_list_remote_imageA_paths_for_channel1)
                        {
                            for (int j = 0; j < m_parent.m_global.m_current_tray_info_for_Nova.Products[i].m_list_local_imageA_paths_for_channel1.Count; j++)
                            {
                                if (j >= m_parent.m_global.m_current_tray_info_for_Nova.Products[i].m_list_remote_imageA_paths_for_channel1.Count)
                                {
                                    break;
                                }
                                if (m_parent.m_global.m_current_tray_info_for_Nova.Products[i].m_list_remote_imageA_paths_for_channel1[j].Length == 0)
                                {
                                    continue;
                                }

                                string strSourceFile = m_parent.m_global.m_current_tray_info_for_Nova.Products[i].m_list_remote_imageA_paths_for_channel1[j];
                                string strNewPath = m_parent.m_global.m_current_tray_info_for_Nova.Products[i].m_list_local_imageA_paths_for_channel1[j];

                                if (File.Exists(strSourceFile))
                                    File.Copy(strSourceFile, strNewPath, true);
                            }
                        }

                        if (null != m_parent.m_global.m_current_tray_info_for_Nova.Products[i].m_list_local_imageA_paths_for_channel2
                            && null != m_parent.m_global.m_current_tray_info_for_Nova.Products[i].m_list_remote_imageA_paths_for_channel2)
                        {
                            for (int j = 0; j < m_parent.m_global.m_current_tray_info_for_Nova.Products[i].m_list_local_imageA_paths_for_channel2.Count; j++)
                            {
                                if (j >= m_parent.m_global.m_current_tray_info_for_Nova.Products[i].m_list_remote_imageA_paths_for_channel2.Count)
                                {
                                    break;
                                }
                                if (m_parent.m_global.m_current_tray_info_for_Nova.Products[i].m_list_remote_imageA_paths_for_channel2[j].Length == 0)
                                {
                                    continue;
                                }

                                string strSourceFile = m_parent.m_global.m_current_tray_info_for_Nova.Products[i].m_list_remote_imageA_paths_for_channel2[j];
                                string strNewPath = m_parent.m_global.m_current_tray_info_for_Nova.Products[i].m_list_local_imageA_paths_for_channel2[j];

                                if (File.Exists(strSourceFile))
                                    File.Copy(strSourceFile, strNewPath, true);
                            }
                        }

                        // 遍历product.m_list_local_imageB_paths，拷贝图片到本地
                        if (null != m_parent.m_global.m_current_tray_info_for_Nova.Products[i].m_list_local_imageB_paths_for_channel1
                            && null != m_parent.m_global.m_current_tray_info_for_Nova.Products[i].m_list_remote_imageB_paths_for_channel1)
                        {
                            for (int j = 0; j < m_parent.m_global.m_current_tray_info_for_Nova.Products[i].m_list_local_imageB_paths_for_channel1.Count; j++)
                            {
                                if (j >= m_parent.m_global.m_current_tray_info_for_Nova.Products[i].m_list_remote_imageB_paths_for_channel1.Count)
                                {
                                    break;
                                }
                                if (m_parent.m_global.m_current_tray_info_for_Nova.Products[i].m_list_remote_imageB_paths_for_channel1[j].Length == 0)
                                {
                                    continue;
                                }

                                string strSourceFile = m_parent.m_global.m_current_tray_info_for_Nova.Products[i].m_list_remote_imageB_paths_for_channel1[j];
                                string strNewPath = m_parent.m_global.m_current_tray_info_for_Nova.Products[i].m_list_local_imageB_paths_for_channel1[j];

                                if (File.Exists(strSourceFile))
                                    File.Copy(strSourceFile, strNewPath, true);
                            }
                        }

                        if (null != m_parent.m_global.m_current_tray_info_for_Nova.Products[i].m_list_local_imageB_paths_for_channel2
                            && null != m_parent.m_global.m_current_tray_info_for_Nova.Products[i].m_list_remote_imageB_paths_for_channel2)
                        {
                            for (int j = 0; j < m_parent.m_global.m_current_tray_info_for_Nova.Products[i].m_list_local_imageB_paths_for_channel2.Count; j++)
                            {
                                if (j >= m_parent.m_global.m_current_tray_info_for_Nova.Products[i].m_list_remote_imageB_paths_for_channel2.Count)
                                {
                                    break;
                                }
                                if (m_parent.m_global.m_current_tray_info_for_Nova.Products[i].m_list_remote_imageB_paths_for_channel2[j].Length == 0)
                                {
                                    continue;
                                }

                                string strSourceFile = m_parent.m_global.m_current_tray_info_for_Nova.Products[i].m_list_remote_imageB_paths_for_channel2[j];
                                string strNewPath = m_parent.m_global.m_current_tray_info_for_Nova.Products[i].m_list_local_imageB_paths_for_channel2[j];

                                if (File.Exists(strSourceFile))
                                    File.Copy(strSourceFile, strNewPath, true);
                            }
                        }

                        // 遍历product.m_list_local_imageC_paths，拷贝图片到本地
                        if (null != m_parent.m_global.m_current_tray_info_for_Nova.Products[i].m_list_local_imageC_paths_for_channel1
                            && null != m_parent.m_global.m_current_tray_info_for_Nova.Products[i].m_list_remote_imageC_paths_for_channel1)
                        {
                            for (int j = 0; j < m_parent.m_global.m_current_tray_info_for_Nova.Products[i].m_list_local_imageC_paths_for_channel1.Count; j++)
                            {
                                if (j >= m_parent.m_global.m_current_tray_info_for_Nova.Products[i].m_list_remote_imageC_paths_for_channel1.Count)
                                {
                                    break;
                                }
                                if (m_parent.m_global.m_current_tray_info_for_Nova.Products[i].m_list_remote_imageC_paths_for_channel1[j].Length == 0)
                                {
                                    continue;
                                }

                                string strSourceFile = m_parent.m_global.m_current_tray_info_for_Nova.Products[i].m_list_remote_imageC_paths_for_channel1[j];
                                string strNewPath = m_parent.m_global.m_current_tray_info_for_Nova.Products[i].m_list_local_imageC_paths_for_channel1[j];

                                if (File.Exists(strSourceFile))
                                    File.Copy(strSourceFile, strNewPath, true);
                            }
                        }

                        //m_parent.m_global.m_log_presenter.Log(string.Format("后台线程：捞取第{0}个产品的图片到本地", i + 1));
                    }
                    catch (Exception ex)
                    {
                        //m_parent.m_global.m_log_presenter.Log(string.Format("拷贝图片到本地失败。错误信息：{0}", ex.Message));
                    }
                }
            }

            m_parent.m_global.m_bIsImagesCloningThreadInProcess[0] = false;
        }

        // 后台线程：拷贝图片到本地
        public void thread_clone_images_for_Dock(object obj)
        {
            int nMachineIndex = (int)obj;

            //Thread.Sleep(500);

            //TrayInfo tray_info = m_parent.m_global.m_background_tray_info_for_Dock;
            TrayInfo tray_info = m_parent.m_global.m_current_tray_info_for_Dock;

            // 删除图片
            try
            {
                var products = tray_info.products;
                for (int i = 0; i < products.Count; i++)
                {
                    var product = products[i];

                    // 删除图片
                    DeleteImages(product.m_list_local_imageA_paths_for_channel1);
                    DeleteImages(product.m_list_local_imageA_paths_for_channel2);
                    DeleteImages(product.m_list_local_imageB_paths_for_channel1);
                    DeleteImages(product.m_list_local_imageB_paths_for_channel2);
                    DeleteImages(product.m_list_local_imageC_paths_for_channel1);
                    DeleteImages(product.m_list_local_imageC_paths_for_channel2);
                    DeleteImages(product.m_list_local_imageD_paths_for_channel1);
                    DeleteImages(product.m_list_local_imageD_paths_for_channel2);

                    //m_parent.m_global.m_log_presenter.Log(string.Format("后台线程：删除第{0}个产品的图片", i + 1));
                }
            }
            catch (Exception ex)
            {
                //m_parent.m_global.m_log_presenter.Log(string.Format("删除图片失败失败。错误信息：{0}", ex.Message));
            }

            m_parent.m_global.m_bIsDeletingImages = false;

            // 拷贝图片到本地
            for (int i = 0; i < tray_info.products.Count; i++)
            {
                try
                {
                    var product = tray_info.products[i];

                    // 去重
                    product.m_list_local_imageA_paths_for_channel1 = product.m_list_local_imageA_paths_for_channel1.Distinct().ToList();
                    product.m_list_local_imageA_paths_for_channel2 = product.m_list_local_imageA_paths_for_channel2.Distinct().ToList();
                    product.m_list_local_imageB_paths_for_channel1 = product.m_list_local_imageB_paths_for_channel1.Distinct().ToList();
                    product.m_list_local_imageB_paths_for_channel2 = product.m_list_local_imageB_paths_for_channel2.Distinct().ToList();
                    product.m_list_local_imageC_paths_for_channel1 = product.m_list_local_imageC_paths_for_channel1.Distinct().ToList();
                    product.m_list_local_imageC_paths_for_channel2 = product.m_list_local_imageC_paths_for_channel2.Distinct().ToList();
                    product.m_list_local_imageD_paths_for_channel1 = product.m_list_local_imageD_paths_for_channel1.Distinct().ToList();
                    product.m_list_local_imageD_paths_for_channel2 = product.m_list_local_imageD_paths_for_channel2.Distinct().ToList();

                    // 去重
                    product.m_list_remote_imageA_paths_for_channel1 = product.m_list_remote_imageA_paths_for_channel1.Distinct().ToList();
                    product.m_list_remote_imageA_paths_for_channel2 = product.m_list_remote_imageA_paths_for_channel2.Distinct().ToList();
                    product.m_list_remote_imageB_paths_for_channel1 = product.m_list_remote_imageB_paths_for_channel1.Distinct().ToList();
                    product.m_list_remote_imageB_paths_for_channel2 = product.m_list_remote_imageB_paths_for_channel2.Distinct().ToList();
                    product.m_list_remote_imageC_paths_for_channel1 = product.m_list_remote_imageC_paths_for_channel1.Distinct().ToList();
                    product.m_list_remote_imageC_paths_for_channel2 = product.m_list_remote_imageC_paths_for_channel2.Distinct().ToList();
                    product.m_list_remote_imageD_paths_for_channel1 = product.m_list_remote_imageD_paths_for_channel1.Distinct().ToList();
                    product.m_list_remote_imageD_paths_for_channel2 = product.m_list_remote_imageD_paths_for_channel2.Distinct().ToList();

                    // 拷贝图片到本地
                    CopyImages(product.m_list_local_imageA_paths_for_channel1, product.m_list_remote_imageA_paths_for_channel1);
                    CopyImages(product.m_list_local_imageA_paths_for_channel2, product.m_list_remote_imageA_paths_for_channel2);
                    CopyImages(product.m_list_local_imageB_paths_for_channel1, product.m_list_remote_imageB_paths_for_channel1);
                    CopyImages(product.m_list_local_imageB_paths_for_channel2, product.m_list_remote_imageB_paths_for_channel2);
                    CopyImages(product.m_list_local_imageC_paths_for_channel1, product.m_list_remote_imageC_paths_for_channel1);
                    CopyImages(product.m_list_local_imageC_paths_for_channel2, product.m_list_remote_imageC_paths_for_channel2);
                    CopyImages(product.m_list_local_imageD_paths_for_channel1, product.m_list_remote_imageD_paths_for_channel1);
                    CopyImages(product.m_list_local_imageD_paths_for_channel2, product.m_list_remote_imageD_paths_for_channel2);

                    // 从本地拷贝图片到本地，用于AI复判
                    if (true)
                    {
                        if (m_parent.m_global.m_nRecheckModeWithAISystem == 3)
                        {
                            CloneDirectoriesAndCopyFiles(product.m_list_local_imageA_paths_for_channel1);
                            CloneDirectoriesAndCopyFiles(product.m_list_local_imageA_paths_for_channel2);
                            CloneDirectoriesAndCopyFiles(product.m_list_local_imageB_paths_for_channel1);
                            CloneDirectoriesAndCopyFiles(product.m_list_local_imageB_paths_for_channel2);
                            CloneDirectoriesAndCopyFiles(product.m_list_local_imageC_paths_for_channel1);
                            CloneDirectoriesAndCopyFiles(product.m_list_local_imageC_paths_for_channel2);
                            CloneDirectoriesAndCopyFiles(product.m_list_local_imageD_paths_for_channel1);
                            CloneDirectoriesAndCopyFiles(product.m_list_local_imageD_paths_for_channel2);
                        }
                    }

                    //m_parent.m_global.m_log_presenter.Log(string.Format("后台线程：捞取第{0}个产品的图片到本地", i + 1));
                }
                catch (Exception ex)
                {
                    //m_parent.m_global.m_log_presenter.Log(string.Format("拷贝图片到本地失败。错误信息：{0}", ex.Message));
                }
            }

            if (false)
            {
                // 从tray_info中获取产品信息
                for (int i = 0; i < tray_info.products.Count; i++)
                {
                    try
                    {
                        // 遍历product.m_list_local_imageA_paths，拷贝图片到本地
                        if (null != tray_info.products[i].m_list_local_imageA_paths_for_channel1 && null != tray_info.products[i].m_list_remote_imageA_paths_for_channel1)
                        {
                            for (int j = 0; j < tray_info.products[i].m_list_local_imageA_paths_for_channel1.Count; j++)
                            {
                                if (j >= tray_info.products[i].m_list_remote_imageA_paths_for_channel1.Count)
                                    break;
                                if (j >= tray_info.products[i].m_list_remote_imageA_paths_for_channel1.Count)
                                    break;

                                string strSourceFile = tray_info.products[i].m_list_remote_imageA_paths_for_channel1[j];
                                string strNewPath = tray_info.products[i].m_list_local_imageA_paths_for_channel1[j];

                                if (File.Exists(strSourceFile))
                                    File.Copy(strSourceFile, strNewPath, true);
                            }
                        }

                        if (null != tray_info.products[i].m_list_local_imageA_paths_for_channel2 && null != tray_info.products[i].m_list_remote_imageA_paths_for_channel2)
                        {
                            for (int j = 0; j < tray_info.products[i].m_list_local_imageA_paths_for_channel2.Count; j++)
                            {
                                if (j >= tray_info.products[i].m_list_remote_imageA_paths_for_channel2.Count)
                                    break;
                                if (j >= tray_info.products[i].m_list_remote_imageA_paths_for_channel2.Count)
                                    break;

                                string strSourceFile = tray_info.products[i].m_list_remote_imageA_paths_for_channel2[j];
                                string strNewPath = tray_info.products[i].m_list_local_imageA_paths_for_channel2[j];

                                if (File.Exists(strSourceFile))
                                    File.Copy(strSourceFile, strNewPath, true);
                            }
                        }

                        // 遍历product.m_list_local_imageB_paths，拷贝图片到本地
                        if (null != tray_info.products[i].m_list_local_imageB_paths_for_channel1 && null != tray_info.products[i].m_list_remote_imageB_paths_for_channel1)
                        {
                            for (int j = 0; j < tray_info.products[i].m_list_local_imageB_paths_for_channel1.Count; j++)
                            {
                                if (j >= tray_info.products[i].m_list_remote_imageB_paths_for_channel1.Count)
                                    break;
                                if (j >= tray_info.products[i].m_list_remote_imageB_paths_for_channel1.Count)
                                    break;

                                string strSourceFile = tray_info.products[i].m_list_remote_imageB_paths_for_channel1[j];
                                string strNewPath = tray_info.products[i].m_list_local_imageB_paths_for_channel1[j];

                                if (File.Exists(strSourceFile))
                                    File.Copy(strSourceFile, strNewPath, true);
                            }
                        }

                        if (null != tray_info.products[i].m_list_local_imageB_paths_for_channel2 && null != tray_info.products[i].m_list_remote_imageB_paths_for_channel2)
                        {
                            for (int j = 0; j < tray_info.products[i].m_list_local_imageB_paths_for_channel2.Count; j++)
                            {
                                if (j >= tray_info.products[i].m_list_remote_imageB_paths_for_channel2.Count)
                                    break;
                                if (j >= tray_info.products[i].m_list_remote_imageB_paths_for_channel2.Count)
                                    break;

                                string strSourceFile = tray_info.products[i].m_list_remote_imageB_paths_for_channel2[j];
                                string strNewPath = tray_info.products[i].m_list_local_imageB_paths_for_channel2[j];

                                if (File.Exists(strSourceFile))
                                    File.Copy(strSourceFile, strNewPath, true);
                            }
                        }

                        // 遍历product.m_list_local_imageC_paths，拷贝图片到本地
                        if (null != tray_info.products[i].m_list_local_imageC_paths_for_channel1 && null != tray_info.products[i].m_list_remote_imageC_paths_for_channel1)
                        {
                            for (int j = 0; j < tray_info.products[i].m_list_local_imageC_paths_for_channel1.Count; j++)
                            {
                                if (j >= tray_info.products[i].m_list_remote_imageC_paths_for_channel1.Count)
                                    break;
                                if (j >= tray_info.products[i].m_list_remote_imageC_paths_for_channel1.Count)
                                    break;

                                string strSourceFile = tray_info.products[i].m_list_remote_imageC_paths_for_channel1[j];
                                string strNewPath = tray_info.products[i].m_list_local_imageC_paths_for_channel1[j];

                                if (File.Exists(strSourceFile))
                                    File.Copy(strSourceFile, strNewPath, true);
                            }
                        }

                        if (null != tray_info.products[i].m_list_local_imageC_paths_for_channel2 && null != tray_info.products[i].m_list_remote_imageC_paths_for_channel2)
                        {
                            for (int j = 0; j < tray_info.products[i].m_list_local_imageC_paths_for_channel2.Count; j++)
                            {
                                if (j >= tray_info.products[i].m_list_remote_imageC_paths_for_channel2.Count)
                                    break;
                                if (j >= tray_info.products[i].m_list_remote_imageC_paths_for_channel2.Count)
                                    break;

                                string strSourceFile = tray_info.products[i].m_list_remote_imageC_paths_for_channel2[j];
                                string strNewPath = tray_info.products[i].m_list_local_imageC_paths_for_channel2[j];

                                if (File.Exists(strSourceFile))
                                    File.Copy(strSourceFile, strNewPath, true);
                            }
                        }

                        // 遍历product.m_list_local_imageD_paths，拷贝图片到本地
                        if (null != tray_info.products[i].m_list_local_imageD_paths_for_channel1 && null != tray_info.products[i].m_list_remote_imageD_paths_for_channel1)
                        {
                            for (int j = 0; j < tray_info.products[i].m_list_local_imageD_paths_for_channel1.Count; j++)
                            {
                                if (j >= tray_info.products[i].m_list_remote_imageD_paths_for_channel1.Count)
                                    break;
                                if (j >= tray_info.products[i].m_list_remote_imageD_paths_for_channel1.Count)
                                    break;

                                string strSourceFile = tray_info.products[i].m_list_remote_imageD_paths_for_channel1[j];
                                string strNewPath = tray_info.products[i].m_list_local_imageD_paths_for_channel1[j];

                                if (File.Exists(strSourceFile))
                                    File.Copy(strSourceFile, strNewPath, true);
                            }
                        }

                        if (null != tray_info.products[i].m_list_local_imageD_paths_for_channel2 && null != tray_info.products[i].m_list_remote_imageD_paths_for_channel2)
                        {
                            for (int j = 0; j < tray_info.products[i].m_list_local_imageD_paths_for_channel2.Count; j++)
                            {
                                if (j >= tray_info.products[i].m_list_remote_imageD_paths_for_channel2.Count)
                                    break;
                                if (j >= tray_info.products[i].m_list_remote_imageD_paths_for_channel2.Count)
                                    break;

                                string strSourceFile = tray_info.products[i].m_list_remote_imageD_paths_for_channel2[j];
                                string strNewPath = tray_info.products[i].m_list_local_imageD_paths_for_channel2[j];

                                if (File.Exists(strSourceFile))
                                    File.Copy(strSourceFile, strNewPath, true);
                            }
                        }

                        m_parent.m_global.m_log_presenter.Log(string.Format("后台线程：捞取第{0}个产品的图片到本地", i + 1));
                    }
                    catch (Exception ex)
                    {
                        m_parent.m_global.m_log_presenter.Log(string.Format("拷贝图片到本地失败。错误信息：{0}", ex.Message));
                    }
                }
            }

            m_parent.m_global.m_bIsImagesCloningThreadInProcess[0] = false;

            CopyImagesEvent.Set();
        }

        // 拷贝远程图片到本地
        private void CopyImages(List<string> localPaths, List<string> remotePaths)
        {
            if (localPaths != null && remotePaths != null)
            {
                int nCount = Math.Min(localPaths.Count, remotePaths.Count);

                // 创建一个 AutoResetEvent 数组，用于等待所有线程完成
                AutoResetEvent[] doneEvents = new AutoResetEvent[nCount];

                for (int j = 0; j < nCount; j++)
                {
                    string strSourceFile = remotePaths[j];
                    string strNewPath = localPaths[j];

                    if (File.Exists(strSourceFile))
                    {
                        // 初始化 AutoResetEvent
                        doneEvents[j] = new AutoResetEvent(false);

                        // 捕获当前索引的副本
                        int index = j;

                        // 创建一个 AutoResetEvent 用于线程启动同步
                        AutoResetEvent threadStarted = new AutoResetEvent(false);

                        // 创建一个线程执行复制操作
                        Thread thread = new Thread((object state) =>
                        {
                            int idx = (int)state;
                            try
                            {
                                // 通知主线程线程已经启动
                                threadStarted.Set();

                                var localPath = Path.GetDirectoryName(localPaths[idx]);
                                if (!Directory.Exists(localPath)){
                                    Directory.CreateDirectory(localPath);
                                }

                                File.Copy(remotePaths[idx], localPaths[idx], true);
                            }
                            catch (Exception ex)
                            {
                                m_parent.m_global.m_log_presenter.Log(string.Format("拷贝图片到本地失败。错误信息：{0}", ex.Message));
                            }
                            finally
                            {
                                // 设置事件状态为终止，表示线程完成
                                doneEvents[idx].Set();
                            }
                        });

                        thread.Start(index);

                        // 等待线程启动完成
                        threadStarted.WaitOne();
                    }
                    else
                    {
                        // 如果文件不存在，直接设置事件状态为终止
                        doneEvents[j] = new AutoResetEvent(true);
                    }
                }

                // 等待所有线程完成
                for (int j = 0; j < nCount; j++)
                {
                    doneEvents[j].WaitOne();
                }
            }
        }

        // 拷贝目录和文件到目标目录
        private void CloneDirectoriesAndCopyFiles(List<string> paths)
        {
            List<string> list_already_created_directories = new List<string>();

            foreach (var path in paths)
            {
                try
                {
                    // 如果不是图片文件，直接跳过
                    if (false == path.EndsWith(".jpg") && false == path.EndsWith(".jpeg") && false == path.EndsWith(".png") && false == path.EndsWith(".bmp"))
                    {
                        continue;
                    }

                    // 如果文件不存在，直接跳过
                    if (false == File.Exists(path))
                    {
                        continue;
                    }

                    // 解析文件路径
                    var fileInfo = new FileInfo(path);
                    var directoryInfo = fileInfo.Directory;
                    if (directoryInfo != null)
                    {
                        // 获取源目录的根路径
                        var root = directoryInfo.Root;

                        // 获取一级目录
                        var firstFolder = directoryInfo.FullName.Substring(root.FullName.Length).Split(System.IO.Path.DirectorySeparatorChar)[0];

                        // 新的一级目录名称
                        var firstFolderForAI = firstFolder + "_forAI";

                        // 创建新的一级目录路径
                        var newFirstFolderPath = System.IO.Path.Combine(root.FullName, firstFolderForAI);

                        // 创建从二级目录开始的路径
                        var subPath = directoryInfo.FullName.Substring(System.IO.Path.Combine(root.FullName, firstFolder).Length);

                        // 最终目标目录
                        var targetDirectory = newFirstFolderPath + subPath;

                        // 创建目标目录
                        if (false == list_already_created_directories.Contains(targetDirectory))
                        {
                            if (false == Directory.Exists(targetDirectory))
                            {
                                Directory.CreateDirectory(targetDirectory);                                        // 无需检查存在性，CreateDirectory 检查这一点

                                list_already_created_directories.Add(targetDirectory);
                            }
                        }

                        // 目标文件路径
                        var targetFilePath = System.IO.Path.Combine(targetDirectory, fileInfo.Name);

                        // 拷贝文件到目标目录
                        File.Copy(path, targetFilePath, true);

                        //m_parent.m_global.m_log_presenter.Log("Copied file to: " + targetFilePath);
                    }
                }
                catch (Exception ex)
                {
                    m_parent.m_global.m_log_presenter.LogError("拷贝文件 " + path + " 失败，异常信息: " + ex.Message);
                }
            }
        }

        // 删除本地图片
        private void DeleteImages(List<string> localPaths)
        {
            if (localPaths != null)
            {
                for (int j = 0; j < localPaths.Count; j++)
                {
                    string strNewPath = localPaths[j];

                    if (File.Exists(strNewPath))
                        File.Delete(strNewPath);
                }
            }
        }
    }
}
