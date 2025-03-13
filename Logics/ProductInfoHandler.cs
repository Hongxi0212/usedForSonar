using AutoScanFQCTest.DataModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AutoScanFQCTest.Logics
{
    public class ProductInfoHandler
    {
        public MainWindow m_parent;

        // 构造函数
        public ProductInfoHandler(MainWindow parent)
        {
            m_parent = parent;
        }

        // 处理AVI产品信息
        public void HandleAVIProductInfo(AVIProductInfo product_info)
        {
            // 将产品信息BatchInfo插入数据库
            if (false)
            {
                // 插入数据的SQL语句
                string insertSql = @"
                    INSERT INTO " + "BatchInfo" + @" (batch_id, number_of_rows, number_of_cols, front, mid, operator, operator_id, product_id, resource, scan_code_mode_, set_id, total_pcs, uuid, work_area, full_status)
                    VALUES (@BatchId, @Row, @Col, @Front, @Mid, @Operator, @OperatorId, @ProductId, @Resource, @ScanCodeMode, @SetId, @TotalPcs, @Uuid, @WorkArea, @Fullstatus);";
                
                // 将产品信息插入数据库
                m_parent.m_global.m_mysql_ops.AddProductInfoToTable("BatchInfo", insertSql, product_info);
            }

            // 从productInfo中获取产品信息
            for (int i = 0; i < product_info.Products.Count; i++)
            {
                Product product = product_info.Products[i];

                // 从product中获取其他信息
                string strSharedImgDir = product.ShareimgPath;

                m_parent.m_global.m_log_presenter.Log(string.Format("共享图片目录：{0}", string.IsNullOrEmpty(strSharedImgDir) ? "无" : strSharedImgDir));

                int nImageWidth = 0;
                int nImageHeight = 0;

                // 将产品数据插入数据库
                if (false)
                {
                    // 插入数据的SQL语句
                    string insertSql = @"
                        INSERT INTO " + "Product" + @" (batch_id, bar_code, image, image_a, image_b, is_null, panel_id, pos_col, pos_row, shareimg_path)
                        VALUES (@BatchId, @BarCode, @Image, @ImageA, @ImageB, @IsNull, @PanelId, @PosCol, @PosRow, @ShareimgPath);";

                    // 将产品数据插入数据库
                    m_parent.m_global.m_mysql_ops.AddProductToTable("Product", insertSql, product, product_info.BatchId);

                    // 从product中获取其他信息
                    for (int j = 0; j < product.Defects.Count; j++)
                    {
                        Defect defect = product.Defects[j];

                        m_parent.m_global.m_log_presenter.Log(string.Format("缺陷信息：{0}", defect.ToString()));

                        string insertQuery = @"
                                            INSERT INTO Defect (product_id, height, sn, type, width, x, y, channel_, channelNum_)
                                            VALUES (@product_id, @height, @sn, @type, @width, @x, @y, @channel_, @channelNum_);";

                        // 将缺陷信息插入数据库
                        m_parent.m_global.m_mysql_ops.AddDefectToTable("Defect", insertQuery, defect, product.PanelId);
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

                            // 判断strIP是否是有效IP地址
                            if (false == AVICommunication.ValidateIPAddress(strIP))
                            {
                                m_parent.m_global.m_log_presenter.Log(string.Format("无效的IP地址：{0}", strIP));
                                return;
                            }

                            // 判断能否ping通strIP
                            if (false == AVICommunication.PingAddress(strIP))
                            {
                                m_parent.m_global.m_log_presenter.Log(string.Format("无法ping通IP地址：{0}", strIP));
                                return;
                            }
                        }

                        // 遍历所有支持的图片格式并获取匹配的文件
                        foreach (string extension in extensions)
                        {
                            imageFiles.AddRange(Directory.GetFiles(strSharedImgDir, $"*{extension}", SearchOption.AllDirectories));
                        }

                        // 去重
                        imageFiles = imageFiles.Distinct().ToList();

                        // 定义保存图片的本地路径
                        string strLocalPath = string.Format("D:\\local");
                        if (false == Directory.Exists(strLocalPath))
                        {
                            Directory.CreateDirectory(strLocalPath);
                        }

                        // 拷贝文件到本地路径
                        for (int j = 0; j < imageFiles.Count; j++)
                        {
                            string filePath = imageFiles[j];
                            string fileName = Path.GetFileName(imageFiles[j]);

                            string destFile = Path.Combine(strLocalPath, fileName);

                            File.Copy(filePath, destFile, true);

                            m_parent.m_global.m_list_current_batch_image_paths.Add(destFile);
                        }

                        if (imageFiles.Count > 0)
                        {
                            m_parent.m_global.m_log_presenter.Log(string.Format("共捞取到{0}张图片", imageFiles.Count));
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
                    catch (Exception ex)
                    {
                        m_parent.m_global.m_log_presenter.Log(string.Format("处理AVI产品信息失败。错误信息：{0}", ex.Message));
                    }
                    
                }

            }
        }
    }
}
