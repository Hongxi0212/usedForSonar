using AutoScanFQCTest.DataModels;
using FupanBackgroundService.DataModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static FupanBackgroundService.DataModels.AVIProductInfo;
using static Mysqlx.Datatypes.Scalar.Types;

namespace FupanBackgroundService.Logics
{
    public class TrayInfoService
    {
        public class MesResponse
        {
            [JsonPropertyName("Result")]
            public string Result { get; set; }

            [JsonPropertyName("Message")]
            public List<string> Message { get; set; }

            [JsonPropertyName("BarcodeLists")]
            public List<object> BarcodeLists { get; set; }

            [JsonPropertyName("ResPanels")]
            public List<object> ResPanels { get; set; }
        }

        // 构造函数
        public TrayInfoService()
        {

        }

        // 处理来自AVI系统的托盘信息，用于Dock和CL5，主要处理数据库和图片路径存储
        public bool HandleTrayInfoFromAVIForDockOrCL5_DatabaseAndImagePathStorage(TrayInfo tray_info, string strRecordFilePath, bool bInsertToDatabase = true)
        {
            Debugger.Log(0, null, string.Format("7777777 ...........111......... 开始将AVI信息插入到数据库并存储图片路径"));

            bool bSuccess = true;

            string strMachineName = tray_info.machine_id;
            string strMachineIP = "";
            if (string.IsNullOrEmpty(strMachineName))
            {
                Debugger.Log(0, null, string.Format("222222 错误信息：机器编号为空！"));

                if (true)
                {
                    string msg = string.Format("机器编号为空！");

                    Global.m_instance.m_AVI_communication.SendLogToRecheckServer(msg);
                }

                return false;
            }
            if (false == Global.m_instance.m_dict_machine_names_and_IPs.Keys.Contains(strMachineName))
            {
                Debugger.Log(0, null, string.Format("错误信息：机器编号{0}不在清单上！", strMachineName));

                if (true)
                {
                    string msg = string.Format("机器编号{0}不在清单上！", strMachineName);

                    Global.m_instance.m_AVI_communication.SendLogToRecheckServer(msg);
                }

                //return false;
            }

            strMachineIP = Global.m_instance.m_dict_machine_names_and_IPs[strMachineName];

            int nMachineIndex = Global.m_instance.m_dict_machine_names_and_IPs.Keys.ToList().IndexOf(strMachineName);

            string strTrayTable = "trays_" + strMachineName;
            string strProductTable = "products_" + strMachineName;
            string strDefectTable = "details_" + strMachineName;

            //Global.m_instance.m_strCurrentClientMachineName = strMachineName;
            //Global.m_instance.m_nCurrentTrayRows = tray_info.total_rows;
            //Global.m_instance.m_nCurrentTrayColumns = tray_info.total_columns;

            strTrayTable = strTrayTable.Replace("-", "_");
            strProductTable = strProductTable.Replace("-", "_");
            strDefectTable = strDefectTable.Replace("-", "_");

            List<string> list_pingable_IPs = new List<string>();               // 可以ping通的IP地址列表
            List<string> list_unpingable_IPs = new List<string>();           // 无法ping通的IP地址列表

            // 将料盘信息TrayInfo插入数据库
            if (true == bInsertToDatabase)
            {
                Debugger.Log(0, null, string.Format("7777777 ...........222......... 开始在数据库中删除原有的TrayInfo数据"));
                // 删除相同set_id的tray数据
                Global.m_instance.m_database_service.DeleteTrayDataIfExist(strTrayTable, tray_info.set_id);
                Debugger.Log(0, null, string.Format("7777777 ...........333......... 结束在数据库中删除原有的TrayInfo数据"));

                tray_info.r1 = "to_be_checked";

                // 插入数据的SQL语句
                string insertOrUpdateSql = @"INSERT INTO " + strTrayTable + @" (number_of_rows, number_of_cols, operator, operator_id, resource, set_id, machine_id, work_area, site, r1, r2, r3, r4, r5, r6, r7) VALUES (@Row, @Col, @Operator, @OperatorId, @Resource, @SetId, @MachineId, @WorkArea, @Site, @r1, @r2, @r3, @r4, @r5, @r6, @r7);";

                // 将料盘信息插入数据库
                Debugger.Log(0, null, string.Format("7777777 ...........444......... 开始在数据库中插入当前的TrayInfo数据"));
                if (false == Global.m_instance.m_mysql_ops.AddTrayInfoToTable(strTrayTable, insertOrUpdateSql, tray_info))
                {
                    //m_parent.m_global.m_log_presenter.Log(string.Format("插入托盘信息失败，可能与原有数据重复。批次号：{0}", tray_info.BatchId));
                    Debugger.Log(0, null, string.Format("7777777 ...........555......... 在数据库中插入当前的TrayInfo数据失败"));
                    return bSuccess;
                }
                Debugger.Log(0, null, string.Format("7777777 ...........666......... 结束在数据库中插入当前的TrayInfo数据"));
            }

            // 从tray_info中获取产品信息
            for (int i = 0; i < tray_info.products.Count; i++)
            {
                ProductInfo product = tray_info.products[i];
                Debugger.Log(0, null, string.Format("7777777 ...........777......... 开始遍历TrayInfo的Product信息"));

                int nImageWidth = 0;
                int nImageHeight = 0;

                // 获取机台IP（同一个料盘产品机台号可能不一样）
                strMachineName = product.machine_id;
                if (string.IsNullOrEmpty(strMachineName))
                {
                    Debugger.Log(0, null, string.Format("222222 错误信息：机器编号为空！"));

                    if (true)
                    {
                        string msg = string.Format("错误信息：机器编号为空！");

                        Global.m_instance.m_AVI_communication.SendLogToRecheckServer(msg);
                    }

                    //return false;
                }
                if (false == Global.m_instance.m_dict_machine_names_and_IPs.Keys.Contains(strMachineName))
                {
                    Debugger.Log(0, null, string.Format("222222 错误信息：机器编号{0}不在清单上！", strMachineName));

                    if (true)
                    {
                        string msg = string.Format("机器编号{0}不在清单上！", strMachineName);

                        Global.m_instance.m_AVI_communication.SendLogToRecheckServer(msg);
                    }

                    //return false;
                }
                strMachineIP = Global.m_instance.m_dict_machine_names_and_IPs[strMachineName];

                if (tray_info.products.Count <= 2)
                {
                    // strRecordFilePath是一个.txt文件路径，在它文件名后面加上product.barcode，重命名该文件
                    string strNewRecordFilePath = strRecordFilePath.Replace(".txt", "_" + product.barcode + ".txt");

                    try
                    {
                        Debugger.Log(0, null, string.Format("7777777 ...........888......... 开始重命名incomingdata文件"));
                        File.Move(strRecordFilePath, strNewRecordFilePath);
                    }
                    catch (Exception ex)
                    {
                        Debugger.Log(0, null, string.Format("222222 重命名文件失败。原文件：{0}。新文件：{1}。错误信息：{2}", strRecordFilePath, strNewRecordFilePath, ex.Message));

                        if (true)
                        {
                            string msg = string.Format("重命名文件失败。原文件：{0}。新文件：{1}。错误信息：{2}", strRecordFilePath, strNewRecordFilePath, ex.Message);

                            Global.m_instance.m_AVI_communication.SendLogToRecheckServer(msg);
                        }
                    }
                }

                // 将产品数据插入数据库
                if (true == bInsertToDatabase)
                {
                    // 删除相同bar_code的product数据
                    Debugger.Log(0, null, string.Format("7777777 ...........999......... 开始在数据库中删除原有的ProductInfo数据"));
                    Global.m_instance.m_database_service.DeleteProductDataIfExist(strProductTable, product.barcode);
                    Debugger.Log(0, null, string.Format("7777777 ...........aaa......... 结束在数据库中删除原有的ProductInfo数据"));

                    // 插入数据的SQL语句
                    string insertSql = @"
                        INSERT INTO " + strProductTable + @" (set_id, machine_id, bar_code, pos_col, pos_row, bET, MES_failure_msg, is_ok_product, inspect_date_time, region_area, mac_address, ip_address, sideA_image_path, sideB_image_path, sideC_image_path, sideD_image_path, sideE_image_path, sideF_image_path, sideG_image_path, sideH_image_path, all_image_paths, r1, r2, r3, r4, r5, r6, r7, r8, r9, r10)
                        VALUES (@SetId, @MachineId, @BarCode, @PosCol, @PosRow, @bET, @MES_failure_msg, @is_ok_product, @inspect_date_time, @region_area, @mac_address, @ip_address, @sideA_image_path, @sideB_image_path, @sideC_image_path, @sideD_image_path, @sideE_image_path, @sideF_image_path, @sideG_image_path, @sideH_image_path, @all_image_paths, @r1, @r2, @r3, @r4, @r5, @r6, @r7, @r8, @r9, @r10);";

                    product.set_id = tray_info.set_id;

                    // 将产品数据插入数据库
                    Debugger.Log(0, null, string.Format("7777777 ...........bbb......... 开始在数据库中插入当前的ProductInfo数据"));
                    Global.m_instance.m_mysql_ops.AddProductToTable(strProductTable, insertSql, product);
                    Debugger.Log(0, null, string.Format("7777777 ...........ccc......... 结束在数据库中插入当前的ProductInfo数据"));

                    // 删除相同product_id的defect数据
                    Debugger.Log(0, null, string.Format("7777777 ...........ddd......... 开始在数据库中删除原有的DefectInfo数据"));
                    Global.m_instance.m_database_service.DeleteDefectDataIfExist(strDefectTable, product.barcode);
                    Debugger.Log(0, null, string.Format("7777777 ...........eee......... 结束在数据库中删除原有的DefectInfo数据"));

                    // 判断产品是否有缺陷数据
                    if (product.defects == null || product.defects.Count <= 0)
                    {
                        Debugger.Log(0, null, string.Format("222222 产品{0} 没有缺陷数据", product.barcode));

                        if (true)
                        {
                            string msg = string.Format("产品{0} 没有缺陷数据", product.barcode);

                            Global.m_instance.m_AVI_communication.SendLogToRecheckServer(msg);
                        }

                        continue;
                    }

                    // 将缺陷信息插入数据库
                    for (int j = 0; j < product.defects.Count; j++)
                    {
                        DefectInfo defect = product.defects[j];

                        //m_parent.m_global.m_log_presenter.Log(string.Format("缺陷信息：{0}", defect.Type));

                        string insertQuery = @"
                                            INSERT INTO " + strDefectTable + @" (id, set_id, product_id, height, type, width, area, center_x, center_y, side, light_channel, image_path, aiCam, aiPos, aiImageIndex, r1, r2, r3, r4, r5, r6, r7, r8, r9, r10)
                                            VALUES (@id, @set_id, @product_id, @height, @type, @width, @area, @center_x, @center_y, @side, @light_channel, @image_path, @aiCam, @aiPos, @aiImageIndex, @r1, @r2, @r3, @r4, @r5, @r6, @r7, @r8, @r9, @r10);";

                        defect.ID = j;
                        defect.set_id = tray_info.set_id;
                        defect.product_id = product.barcode;

                        // 将缺陷信息插入数据库
                        Debugger.Log(0, null, string.Format("7777777 ...........fff......... 开始在数据库中插入当前的DefectInfo数据"));
                        Global.m_instance.m_mysql_ops.AddDefectToTable(strDefectTable, insertQuery, defect);
                        Debugger.Log(0, null, string.Format("7777777 ...........ggg......... 结束在数据库中插入当前的DefectInfo数据"));
                    }
                }

                continue;

            }

            Debugger.Log(0, null, string.Format("7777777 ...........hhh......... 结束将AVI信息插入到数据库并存储图片路径"));
            return bSuccess;
        }

        public bool InferProductImagePath(TrayInfo tray)
        {
            if (tray.products == null || tray.products.Count < 1)
            {
                return false;
            }

            var product = tray.products[0];

            List<string> list_side_paths = new List<string>();

            if (false == string.IsNullOrEmpty(product.sideA_image_path))
                list_side_paths.Add(product.sideA_image_path);
            if (false == string.IsNullOrEmpty(product.sideB_image_path))
                list_side_paths.Add(product.sideB_image_path);
            if (false == string.IsNullOrEmpty(product.sideC_image_path))
                list_side_paths.Add(product.sideC_image_path);

            var bSuccess = true;

            int nNumOfAccessedImages = 0;
            for (int k = 0; k < list_side_paths.Count; k++)
            {
                string strSidePath = list_side_paths[k];

                // 检查strSidePath为有效图片路径
                if (false == string.IsNullOrEmpty(strSidePath))
                {
                    try
                    {
                        List<string> imageFiles = new List<string>();

                        // 定义支持的图片格式
                        string[] extensions = new string[] { ".jpg", ".jpeg", ".png", ".bmp" };

                        // 遍历所有支持的图片格式并获取匹配的文件
                        foreach (string extension in extensions)
                        {
                            imageFiles.AddRange(Directory.GetFiles(strSidePath, $"*{extension}", SearchOption.AllDirectories));
                        }

                        // 去重
                        imageFiles = imageFiles.Distinct().ToList();

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

                        // 获取文件路径
                        for (int j = 0; j < imageFiles.Count; j++)
                        {
                            string filePath = imageFiles[j];

                            if (Global.m_instance.m_strProductType == "dock")
                            {
                                if (filePath.Contains("Atype"))
                                {
                                    product.m_list_local_imageA_paths_for_channel1.Add(filePath);
                                }
                                else if (filePath.Contains("Btype"))
                                {
                                    product.m_list_local_imageB_paths_for_channel1.Add(filePath);
                                }
                                else if (filePath.Contains("Ctype"))
                                {
                                    product.m_list_local_imageC_paths_for_channel1.Add(filePath);
                                }
                                else if (filePath.Contains("Dtype"))
                                {
                                    product.m_list_local_imageD_paths_for_channel1.Add(filePath);
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
                        string msg = string.Format("处理AVI产品信息失败。图片路径：{0}。错误信息：{1}", strSidePath, ex.Message);
                        // 考虑找不到视觉存图路径处理
                        bSuccess = false;
                    }
                }
            }

            return bSuccess;
        }

        // 处理来自AVI系统的托盘信息，用于Nova，主要处理数据库和图片路径存储
        public bool HandleTrayInfoFromAVIForNova_DatabaseAndImagePathStorage(AVITrayInfo tray_info, bool bInsertToDatabase = true)
        {
            bool bSuccess = true;

            string strMachineName = tray_info.Mid;
            string strMachineIP = "";
            if (string.IsNullOrEmpty(strMachineName))
            {
                Debugger.Log(0, null, string.Format("222222 错误信息：机器编号为空！"));

                if (true)
                {
                    string msg = string.Format("机器编号为空！");

                    Global.m_instance.m_AVI_communication.SendLogToRecheckServer(msg);
                }
                return false;
            }
            if (false == Global.m_instance.m_dict_machine_names_and_IPs.Keys.Contains(strMachineName))
            {
                Debugger.Log(0, null, string.Format("错误信息：机器编号{0}不在清单上！", strMachineName));

                if (true)
                {
                    string msg = string.Format("机器编号{0}不在清单上！", strMachineName);

                    Global.m_instance.m_AVI_communication.SendLogToRecheckServer(msg);
                }
                return false;
            }

            string strTrayTable = "trays_" + strMachineName;
            string strProductTable = "products_" + strMachineName;
            string strDefectTable = "details_" + strMachineName;

            strTrayTable = strTrayTable.Replace("-", "_");
            strProductTable = strProductTable.Replace("-", "_");
            strDefectTable = strDefectTable.Replace("-", "_");

            strMachineIP = Global.m_instance.m_dict_machine_names_and_IPs[strMachineName];

            List<string> list_pingable_IPs = new List<string>();               // 可以ping通的IP地址列表
            List<string> list_unpingable_IPs = new List<string>();           // 无法ping通的IP地址列表

            // 将料盘信息TrayInfo插入数据库
            if (true == bInsertToDatabase)
            {
                // 删除相同set_id的tray数据
                Global.m_instance.m_database_service.DeleteTrayDataIfExist(strTrayTable, tray_info.SetId);

                tray_info.r1 = "to_be_checked";

                // 插入数据的SQL语句
                string insertOrUpdateSql = @"
                    INSERT INTO " + strTrayTable + @" (batch_id, number_of_rows, number_of_cols, front, mid, operator, operator_id, product_id, resource, scan_code_mode, set_id, total_pcs, uuid, work_area, full_status, r1)
                    VALUES (@batch_id, @number_of_rows, @number_of_cols, @front, @mid, @operator, @operator_id, @product_id, @resource, @scan_code_mode, @set_id, @total_pcs, @uuid, @work_area, @full_status, @r1);";

                // 将产品信息插入数据库
                if (false == Global.m_instance.m_mysql_ops.AddTrayInfoToTable(strTrayTable, insertOrUpdateSql, tray_info))
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
                    // continue;
                }

                // 从product中获取其他信息
                string strSharedImgDir = product.ShareimgPath;

                int nImageWidth = 0;
                int nImageHeight = 0;

                // 将产品数据插入数据库
                if (true == bInsertToDatabase)
                {
                    // 删除相同bar_code的product数据
                    Global.m_instance.m_database_service.DeleteProductDataIfExist(strProductTable, product.BarCode);

                    // 判断产品是否有缺陷数据
                    if (product.Defects.Count <= 0)
                    {
                        Debugger.Log(0, null, string.Format("222222 产品{0} 没有缺陷数据", product.BarCode));

                        if (true)
                        {
                            string msg = string.Format("产品{0} 没有缺陷数据", product.BarCode);

                            Global.m_instance.m_AVI_communication.SendLogToRecheckServer(msg);
                        }
                        continue;
                    }

                    // 插入数据的SQL语句
                    string insertSql = @"
                        INSERT INTO " + strProductTable + @" (set_id, uuid, batch_id, bar_code, image, image_a, image_b, is_null, panel_id, pos_col, pos_row, shareimg_path, r1, r2, r3, r4, r5, r6, r7, r8, r9, r10)
                        VALUES (@SetId, @Uuid, @BatchId, @BarCode, @Image, @ImageA, @ImageB, @IsNull, @PanelId, @PosCol, @PosRow, @ShareimgPath, @r1, @r2, @r3, @r4, @r5, @r6, @r7, @r8, @r9, @r10); ";

                    product.SetId = tray_info.SetId;
                    product.Uuid = tray_info.Uuid;

                    // 将产品数据插入数据库
                    Global.m_instance.m_mysql_ops.AddProductToTable("Products", insertSql, product, tray_info.BatchId);

                    // 删除相同product_id的defect数据
                    Global.m_instance.m_database_service.DeleteDefectDataIfExist(strDefectTable, product.BarCode);

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
                        Global.m_instance.m_mysql_ops.AddDefectToTable(strDefectTable, insertQuery, defect, product.PanelId);
                    }
                }

                bSuccess = true;
            }

            return bSuccess;
        }

        // 处理来自AVI系统的托盘信息，用于Nova，提交一次NG数据到MES
        public bool HandleTrayInfoFromAVIForNova_SubmitOriginalNGProductsDataToMES(AVITrayInfo tray_info)
        {
            bool bSuccess = false;

            if (tray_info.Products.Count == 0)
            {
                return false;
            }

            string msg = "";
            string json = "";

            List<PieceSummary> list_summary = new List<PieceSummary>();

            string strOperatorName = Global.m_instance.m_strCurrentOperatorID;
            string testdate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            // 封装主干数据
            try
            {
                for (int i = 0; i < tray_info.Products.Count; i++)
                {
                    Product product = tray_info.Products[i];

                    int nColumns = tray_info.Col;
                    int nRows = tray_info.Row;

                    int col = product.PosCol - 1;
                    int row = product.PosRow - 1;

                    int nIndex = row * nColumns + col;

                    PieceSummary summary = new PieceSummary();

                    List<DefectDetail> list_defects = new List<DefectDetail>();

                    summary.PanelId = product.BarCode;
                    summary.PcsSeq = "1";
                    summary.OperatorName = strOperatorName;
                    summary.OperatorTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    summary.VerifyOperatorName = strOperatorName;
                    summary.VerifyTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    summary.TestResult = "FAIL";
                    summary.VerifyResult = "FAIL";
                    summary.aiResult = "";
                    //summary.aiTime = product.aiTime;

                    for (int n = 0; n < product.Defects.Count; n++)
                    {
                        Defect defect = product.Defects[n];

                        DefectDetail defect_detail = new DefectDetail();

                        defect_detail.PanelId = product.BarCode;
                        defect_detail.PcsBarCode = product.BarCode;
                        defect_detail.PcsSeq = "1";

                        if (1 == defect.ChannelNum)
                            defect_detail.TestType = "AVI-FQCA";
                        else if (2 == defect.ChannelNum)
                            defect_detail.TestType = "AVI-FQCB";
                        else if (3 == defect.ChannelNum)
                            defect_detail.TestType = "AVI-FQCC";
                        else if (4 == defect.ChannelNum)
                            defect_detail.TestType = "AVI-FQCD";
                        else
                            defect_detail.TestType = "AVI-FQC";
                        //defect_detail.TestType = defect.ChannelNum == 1 ? "AVI-FQCA" : "AVI-FQCB";

                        defect_detail.PartSeq = "1";
                        defect_detail.PinSeg = "1";
                        defect_detail.BubbleValue = "";
                        defect_detail.DefectCode = defect.Type;
                        defect_detail.OperatorName = strOperatorName;
                        defect_detail.VerifyOperatorName = "";
                        defect_detail.VerifyTime = "";
                        defect_detail.Description = defect.Type;
                        defect_detail.ImagePath = product.ShareimgPath;
                        defect_detail.TestFile = "testFile";
                        defect_detail.StrValue1 = "";
                        defect_detail.StrValue2 = "";
                        defect_detail.StrValue3 = "";
                        defect_detail.StrValue4 = "";

                        defect_detail.TestResult = "FAIL";
                        defect_detail.VerifyResult = "";

                        summary.TestResult = "FAIL";
                        summary.VerifyResult = "";

                        product.RecheckResult = "";

                        list_defects.Add(defect_detail);
                    }

                    summary.pcsDetails = list_defects;

                    list_summary.Add(summary);
                }
            }
            catch (Exception ex)
            {
                Debugger.Log(0, null, string.Format("222222 封装主干数据失败。错误信息：{0}", ex.Message));

                if (true)
                {
                    msg = string.Format("封装主干数据失败。错误信息：{0}", ex.Message);

                    Global.m_instance.m_AVI_communication.SendLogToRecheckServer(msg);
                }

                return false;
            }

            // 封装头部信息
            MesPanelUploadInfo aviUploadInfo = new MesPanelUploadInfo();
            if (true)
            {
                aviUploadInfo.Panel = tray_info.Products[0].PanelId;
                aviUploadInfo.Resource = tray_info.Mid;
                aviUploadInfo.Machine = tray_info.Mid;
                aviUploadInfo.Uuid = tray_info.Uuid;
                aviUploadInfo.Machine = tray_info.Mid;
                aviUploadInfo.Product = tray_info.BatchId;

                aviUploadInfo.isRepair = "0";

                aviUploadInfo.OperatorName = strOperatorName;
                aviUploadInfo.OperatorType = "operatorType";
                aviUploadInfo.TrackType = "0";
                aviUploadInfo.WorkArea = "SMT-FAVI";
                aviUploadInfo.Mac = "F8-16-54-CD-AF-82";
                aviUploadInfo.VersionFlag = "0";
                aviUploadInfo.CheckDetail = "1";
                aviUploadInfo.CheckPcsDataForAVI = true;
                aviUploadInfo.TestTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                aviUploadInfo.JdeProductName = Global.m_instance.m_strProductName;
                aviUploadInfo.Site = Global.m_instance.m_strSiteCity == "苏州" ? "SZSMT" : "YCSMT";
                aviUploadInfo.TestMode = "PANEL";
                aviUploadInfo.TestType = "AVI-FQC";
                aviUploadInfo.ProgramName = Global.m_instance.m_strProgramName;
                aviUploadInfo.TrackType = "0";
                aviUploadInfo.pcsSummarys = list_summary;

                json = JsonConvert.SerializeObject(aviUploadInfo);
            }

            // 创建json文件夹
            string strJsonFolderPath = "D:\\FupanMESData2";
            if (!Directory.Exists(strJsonFolderPath))
                Directory.CreateDirectory(strJsonFolderPath);

            // 保存json文件
            if (true)
            {
                string strJsonFileName = "";

                strJsonFileName = $"{tray_info.Products[0].BarCode}.json";

                string strJsonPath = Path.Combine(strJsonFolderPath, strJsonFileName);
                if (!File.Exists(strJsonPath))
                {
                    File.Create(strJsonPath).Close();
                }
                else
                {
                    File.Delete(strJsonPath);
                    File.Create(strJsonPath).Close();
                }
                File.WriteAllText(strJsonPath, json);
            }

            string strMesServerResponse = "";
            MesResponse response = new MesResponse();

            // 是否提交MES前先检查已经跑过样品板
            if (false)
            {
                if (Global.m_instance.m_bCheckSampleProductStatusBeforeSubmittingRecheckResultsToMES)
                {
                    bool bHasCheckExamplePanel = SendMES(Global.m_instance.m_strMesCheckSampleProductUrl, json, ref strMesServerResponse, 1);
                    response = JsonConvert.DeserializeObject<MesResponse>(strMesServerResponse);

                    if (strMesServerResponse.Contains("未进行过样品板测试"))
                    {
                        Debugger.Log(0, null, string.Format("222222 机台尚未进行样品板测试！请先跑样品板"));

                        msg = string.Format("机台尚未进行样品板测试！请先跑样品板");
                        Global.m_instance.m_AVI_communication.SendLogToRecheckServer(msg);

                        return false;
                    }
                    else if (response.Result == "-1")
                    {
                        return false;
                    }
                }
            }

            // 提交复判数据
            strMesServerResponse = "";
            bool bRet = SendMES(Global.m_instance.m_strMesDataUploadUrl, json, ref strMesServerResponse, 1);

            if (true == bRet)
            {
                response = JsonConvert.DeserializeObject<MesResponse>(strMesServerResponse);

                Debugger.Log(0, null, string.Format("222222 提交MES一次NG数据 response.Result = {0}", response.Result));

                msg = string.Format("提交MES一次NG数据 response.Result = {0}", response.Result);
                Global.m_instance.m_AVI_communication.SendLogToRecheckServer(msg);

                if (response.Result == "0")
                {
                    bSuccess = true;
                }
                else
                {
                    Debugger.Log(0, null, string.Format("222222 提交MES一次NG数据失败！response.Message = {0}", response.Message));

                    msg = string.Format("提交MES一次NG数据失败！response.Message = {0}", response.Message);
                    Global.m_instance.m_AVI_communication.SendLogToRecheckServer(msg);
                }
            }
            else
            {
                Debugger.Log(0, null, string.Format("222222 提交MES一次NG数据失败！"));

                msg = string.Format("提交MES一次NG数据失败！");
                Global.m_instance.m_AVI_communication.SendLogToRecheckServer(msg);
            }

            return bSuccess;
        }

        // 发送数据到MES
        public bool SendMES(string strURL, string strDataToSend, ref string strResponse, int nGetOrPostFlag = 1, int timeoutInSeconds = 1, int readWriteTimeoutInSeconds = 1)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(strURL);

                request.Method = "POST";
                request.ContentType = "application/json";
                request.Timeout = timeoutInSeconds * 1000;
                request.ReadWriteTimeout = readWriteTimeoutInSeconds * 1000;

                if (nGetOrPostFlag == 0)
                {
                    request.Method = "GET";
                }

                Debugger.Log(0, null, string.Format("222222 SendMES url: {0}", strURL));

                // 发送请求数据
                using (StreamWriter writer = new StreamWriter(request.GetRequestStream(), Encoding.UTF8))
                {
                    writer.Write(strDataToSend);
                    writer.Flush();
                    writer.Close();

                    Debugger.Log(0, null, string.Format("222222 MES 数据发送成功！"));
                }

                // 接收响应数据
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    Debugger.Log(0, null, string.Format("222222 SendMES 111"));

                    string responseJson = reader.ReadToEnd();

                    Debugger.Log(0, null, string.Format("222222 接收成功，MES服务器返回数据：{0}", responseJson));

                    strResponse = responseJson;
                }

                return true;
            }
            catch (Exception ex)
            {
                strResponse = ex.Message;
                Debugger.Log(0, null, string.Format("222222 MES请求异常，可能是连接失败，异常信息：{0}", ex.Message));
                return false;
            }
        }

    }
}