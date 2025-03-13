using AutoScanFQCTest.DataModels;
using AutoScanFQCTest.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AutoScanFQCTest.Logics
{
    public class DatabaseService
    {
        private static DatabaseService m_instance;
        private static readonly object _lock = new object();

        public MainWindow m_parent;

        public object DatabaseHelper { get; private set; }

        // 构造函数
        public DatabaseService(MainWindow parent)
        {
            m_parent = parent;
        }

        // 静态方法获取实例
        //public static DatabaseService GetInstance()
        //{
        //    if (m_instance == null)
        //    {
        //        lock (_lock)
        //        {
        //            if (m_instance == null)
        //            {
        //                NewInstance();
        //            }
        //        }
        //    }

        //    return m_instance;
        //}

        private void NewInstance()
        {
            m_instance = new DatabaseService(m_parent);
        }

        // 功能：删除相同set_id的tray数据
        public void DeleteTrayDataIfExist(string set_id)
        {
            string deleteQuery = $"DELETE FROM Trays WHERE set_id = '{set_id}'";

            // 执行删除操作
            string strError = "";
            m_parent.m_global.m_mysql_ops.ExecuteQueryWithoutNeedingResult(deleteQuery, ref strError);
        }

        // 功能：删除相同set_id的tray数据
        public void DeleteTrayDataIfExist(string strTrayTable, string set_id)
        {
            string deleteQuery = $"DELETE FROM " + strTrayTable + $" WHERE set_id = '{set_id}'";

            // 执行删除操作
            string strError = "";
            m_parent.m_global.m_mysql_ops.ExecuteQueryWithoutNeedingResult(deleteQuery, ref strError);
        }

        // 功能：删除相同set_id的product数据
        public void DeleteProductDataIfExist(string strBarcode)
        {
            string deleteQuery = $"DELETE FROM Products WHERE bar_code = '{strBarcode}'";

            // 执行删除操作
            string strError = "";
            m_parent.m_global.m_mysql_ops.ExecuteQueryWithoutNeedingResult(deleteQuery, ref strError);
        }

        // 功能：删除相同set_id的product数据
        public void DeleteProductDataIfExist(string strProductTable, string strBarcode)
        {
            string deleteQuery = $"DELETE FROM " + strProductTable + $" WHERE bar_code = '{strBarcode}'";

            // 执行删除操作
            string strError = "";
            m_parent.m_global.m_mysql_ops.ExecuteQueryWithoutNeedingResult(deleteQuery, ref strError);
        }

        // 功能：删除相同set_id的defect数据
        public void DeleteDefectDataIfExist(string product_id)
        {
            string deleteQuery = $"DELETE FROM Defects WHERE product_id = '{product_id}'";

            // 执行删除操作
            string strError = "";
            m_parent.m_global.m_mysql_ops.ExecuteQueryWithoutNeedingResult(deleteQuery, ref strError);
        }

        public void DeleteRecheckRecordIfExist(string setID)
        {
            string strDeleteSQL = $"DELETE FROM RecheckRecord WHERE SetID = '{setID}'";
            string strError = "";

            m_parent.m_global.m_mysql_ops.ExecuteQueryWithoutNeedingResult(strDeleteSQL, ref strError);
        }

        // 功能：删除相同set_id的defect数据
        public void DeleteDefectDataIfExist(string strDefectTable, string product_id)
        {
            string deleteQuery = $"DELETE FROM " + strDefectTable + $" WHERE product_id = '{product_id}'";

            // 执行删除操作
            string strError = "";
            m_parent.m_global.m_mysql_ops.ExecuteQueryWithoutNeedingResult(deleteQuery, ref strError);
        }

        // 功能：处理料盘数据查询结果
        public void ProcessTrayDataQueryResult(string strQueryResult, ref List<AVITrayInfo> list_trays)
        {
            // 解析查询结果
            string[] lines = strQueryResult.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);

            // 如果查询结果为空
            if (lines.Length == 0)
            {
                MessageBox.Show(m_parent, "未查询到数据库中条件符合的记录", "提示");
                return;
            }

            for (int i = 0; i < lines.Length; i++)
            {
                string[] parts = lines[i].Trim().Split(new char[] { '@' }, StringSplitOptions.None);

                if (parts.Length >= 5) // 确保有足够的数据来创建Product对象
                {
                    AVITrayInfo tray = new AVITrayInfo
                    {
                        BatchId = parts[0],
                        Row = int.Parse(parts[1]),
                        Col = int.Parse(parts[2]),
                        Front = bool.Parse(parts[3]),
                        Mid = parts[4],
                        Operator = parts[5],
                        OperatorId = parts[6],
                        ProductId = parts[7],
                        Resource = parts[8],
                        ScanCodeMode = int.Parse(parts[9]),
                        SetId = parts[10],
                        TotalPcs = int.Parse(parts[11]),
                        Uuid = parts[12],
                        WorkArea = parts[13],
                        Fullstatus = parts[14],
                        r1 = parts[15],
                    };

                    list_trays.Add(tray);
                }
            }
        }

        // 功能：处理料盘数据查询结果
        public void ProcessTrayDataQueryResult(string strQueryResult, ref List<TrayInfo> list_trays)
        {
            // 解析查询结果
            string[] lines = strQueryResult.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);

            // 如果查询结果为空
            if (lines.Length == 0)
            {
                MessageBox.Show(m_parent, "未查询到数据库中条件符合的记录", "提示");
                return;
            }

            for (int i = 0; i < lines.Length; i++)
            {
                string[] parts = lines[i].Trim().Split(new char[] { '@' }, StringSplitOptions.None);

                if (parts.Length >= 5) // 确保有足够的数据来创建Product对象
                {
                    TrayInfo tray = new TrayInfo
                    {
                        set_id = parts[10],
                        total_rows = int.Parse(parts[1]),
                        total_columns = int.Parse(parts[2]),
                        machine_id = parts[0],
                        Operator = parts[5],
                        operator_id = parts[6],
                        site = parts[14],
                        r1 = parts[15],
                    };

                    list_trays.Add(tray);
                }
            }
        }

        // 功能：处理料盘数据查询结果
        public void ProcessTrayDataQueryResult(string strQueryResult, ref List<TrayInfo> list_trays, bool bShowErrorMessage = false)
        {
            // 解析查询结果
            string[] lines = strQueryResult.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);

            // 如果查询结果为空
            if (lines.Length == 0)
            {
                if (true == bShowErrorMessage)
                    MessageBox.Show(m_parent, "未查询到数据库中条件符合的记录", "提示");
                return;
            }

            for (int i = 0; i < lines.Length; i++)
            {
                string[] parts = lines[i].Trim().Split(new char[] { '@' }, StringSplitOptions.None);

                if (parts.Length >= 5) // 确保有足够的数据来创建Product对象
                {
                    TrayInfo tray = new TrayInfo
                    {
                        set_id = parts[10],
                        total_rows = int.Parse(parts[1]),
                        total_columns = int.Parse(parts[2]),
                        machine_id = parts[0],
                        Operator = parts[5],
                        operator_id = parts[6],
                        site = parts[14],
                        r1 = parts[15],
                        r2 = parts[16],
                        r3 = parts[17],
                        r4 = parts[18],
                        r5 = parts[19],
                        r6 = parts[20],
                        r7 = parts[21],
                        r8 = parts[22],
                        r9 = parts[23],
                        r10 = parts[24],
                    };

                    list_trays.Add(tray);
                }
            }
        }

        // 功能：处理产品数据查询结果
        public void ProcessProductDataQueryResult(string strQueryResult, ref List<Product> list_products)
        {
            // 解析查询结果
            string[] lines = strQueryResult.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);

            // 如果查询结果为空
            if (lines.Length == 0)
            {
                MessageBox.Show(m_parent, "未查询到数据库中条件符合的记录", "Product");
                return;
            }

            for (int i = 0; i < lines.Length; i++)
            {
                string[] parts = lines[i].Trim().Split(new char[] { '@' }, StringSplitOptions.None);

                if (parts.Length >= 5) // 确保有足够的数据来创建Product对象
                {
                    Product product = new Product
                    {
                        SetId = parts[0],
                        Uuid = parts[1],
                        BarCode = parts[3],
                        Image = parts[4],
                        ImageA = parts[5],
                        ImageB = parts[6],
                        IsNull = parts[7],
                        PanelId = parts[8],
                        PosCol = int.Parse(parts[9]),
                        PosRow = int.Parse(parts[10]),
                        ShareimgPath = parts[11],
                        aiResult = parts[12],
                        aiTime = parts[13],
                        r1 = parts[14],
                        r2 = parts[15],
                        r3 = parts[16],
                    };

                    list_products.Add(product);
                }
            }
        }

        // 功能：处理产品数据查询结果
        public void ProcessProductDataQueryResult(string strQueryResult, ref List<ProductInfo> list_products, bool bShowErrorMessage = false)
        {
            // 解析查询结果
            string[] lines = strQueryResult.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);

            // 如果查询结果为空
            if (lines.Length == 0)
            {
                if (true == bShowErrorMessage)
                    MessageBox.Show(m_parent, "未查询到数据库中条件符合的记录", "ProductInfo");
                return;
            }

            for (int i = 0; i < lines.Length; i++)
            {
                string[] parts = lines[i].Trim().Split(new char[] { '@' }, StringSplitOptions.None);

                if (parts.Length >= 5) // 确保有足够的数据来创建Product对象
                {
                    if (String.IsNullOrEmpty(parts[8]) || String.IsNullOrEmpty(parts[10]))
                    {
                        parts[8] = "false";
                        parts[10] = "false";
                    }

                    ProductInfo product = new ProductInfo {
                        set_id = parts[0],
                        machine_id = parts[2],
                        barcode = parts[3],
                        column = int.Parse(parts[6]),
                        row = int.Parse(parts[7]),
                        bET = bool.Parse(parts[8]),
                        MES_failure_msg = parts[9],
                        is_ok_product = bool.Parse(parts[10]),
                        inspect_date_time = parts[11],
                        region_area = parts[12],
                        mac_address = parts[13],
                        ip_address = parts[14],
                        sideA_image_path = parts[15],
                        sideB_image_path = parts[16],
                        sideC_image_path = parts[17],
                        sideD_image_path = parts[18],
                        sideE_image_path = parts[19],
                        sideF_image_path = parts[20],
                        sideG_image_path = parts[21],
                        sideH_image_path = parts[22],
                        all_image_paths = parts[23],
                        r1 = parts[24],
                        r2 = parts[25],
                        r3 = parts[26],
                        r4 = parts[27],
                        r5 = parts[28],
                        r6 = parts[29],
                        r7 = parts[30],
                        r8 = parts[31],
                        r9 = parts[32],
                        r10 = parts[33],
                    };

                    list_products.Add(product);
                }
            }
        }

        // 功能：处理缺陷数据查询结果
        public void ProcessDefectDataQueryResult(string strQueryResult, ref List<Defect> list_defects)
        {
            // 解析查询结果
            string[] lines = strQueryResult.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);

            // 如果查询结果为空
            if (lines.Length == 0)
            {
                MessageBox.Show(m_parent, "未查询到数据库中条件符合的记录", "Defect");
                return;
            }

            for (int i = 0; i < lines.Length; i++)
            {
                string[] parts = lines[i].Trim().Split(new char[] { '@' }, StringSplitOptions.None);

                if (parts.Length >= 5) // 确保有足够的数据来创建Defect对象
                {
                    Defect defect = new Defect
                    {
                        SetId = parts[1],
                        Uuid = parts[2],
                        product_id = parts[3],
                        Sn = int.Parse(parts[4]),
                        Type = parts[5],
                        Width = double.Parse(parts[6]),
                        Height = double.Parse(parts[7]),
                        X = double.Parse(parts[8]),
                        Y = double.Parse(parts[9]),
                        Channel = int.Parse(parts[10]),
                        ChannelNum = int.Parse(parts[11]),
                        Area = double.Parse(parts[12]),
                        um_per_pixel = double.Parse(parts[13]),
                        aiResult = parts[14],
                        aiDefectCode = parts[15].Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList(),
                        aiGuid = parts[16]
                    };

                    list_defects.Add(defect);
                }
            }
        }

        // 功能：处理缺陷数据查询结果
        public void ProcessDefectDataQueryResult(string strQueryResult, ref List<DefectInfo> list_defects, bool bShowErrorMessage = false)
        {
            // 解析查询结果
            string[] lines = strQueryResult.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);

            // 如果查询结果为空
            if (lines.Length == 0)
            {
                if (true == bShowErrorMessage)
                    MessageBox.Show(m_parent, "根据料片SN查询到的产品无缺陷数据，有可能是MES校验NG产品", "Defect");
                return;
            }

            for (int i = 0; i < lines.Length; i++)
            {
                string[] parts = lines[i].Trim().Split(new char[] { '@' }, StringSplitOptions.None);

                if (parts.Length >= 5) // 确保有足够的数据来创建Defect对象
                {
                    if (String.IsNullOrEmpty(parts[7]) ||
                        String.IsNullOrEmpty(parts[12]) ||
                        String.IsNullOrEmpty(parts[13]) ||
                        String.IsNullOrEmpty(parts[14]))
                    {
                        parts[7] = "0";
                        parts[12] = "0";
                        parts[13] = "0";
                        parts[14] = "0";
                    }

                    DefectInfo defect = new DefectInfo
                    {
                        ID = int.Parse(parts[0]),
                        set_id = parts[1],
                        product_id = parts[3],
                        type = parts[4],
                        width = double.Parse(parts[5]),
                        height = double.Parse(parts[6]),
                        area = double.Parse(parts[7]),
                        center_x = double.Parse(parts[8]),
                        center_y = double.Parse(parts[9]),
                        side = int.Parse(parts[10]),
                        light_channel = int.Parse(parts[11]),
                        image_path= parts[12],
                        aiCam = int.Parse(parts[13]),
                        aiPos = int.Parse(parts[14]),
                        aiImageIndex = int.Parse(parts[15]),
                        r1 = parts[16],
                        r2 = parts[17],
                        r3 = parts[18],
                        r4 = parts[19],
                        r5 = parts[10],
                        r6 = parts[21],
                        r7 = parts[22],
                        r8 = parts[23],
                        r9 = parts[24],
                        r10 = parts[25],
                    };

                    list_defects.Add(defect);
                }
            }
        }

        // 处理复判记录查询结果
        public void ProcessRecheckRecordQueryResult(string strQueryResult, ref MESTrayInfo mesTrayInfo)
        {
            // 解析查询结果
            string[] lines = strQueryResult.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);

            // 如果查询结果为空
            if (lines.Length == 0)
            {
                mesTrayInfo = new MESTrayInfo();
                return;
            }
            string[] parts = lines[0].Trim().Split(new char[] { '@' }, StringSplitOptions.None);
            mesTrayInfo = JsonConvert.DeserializeObject<MESTrayInfo>(parts[1]);
        }

        // 处理缺陷数据统计查询结果
        public void ProcessDefectStatisticsQueryResult(string strQueryResult, ref List<DefectStatistics> list_defectRecords)
        {
            // 解析查询结果
            string[] lines = strQueryResult.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);

            // 如果查询结果为空
            if (lines.Length == 0)
            {
                list_defectRecords = new List<DefectStatistics>();
                return;
            }

            foreach (var line in lines)
            {
                string[] parts = line.Trim().Split(new char[] { '@' }, StringSplitOptions.None);

                if (parts.Length >= 5) // 确保有足够的数据来创建对象
                {
                    DefectStatistics record = new DefectStatistics
                    {
                        MachineId = parts[0],
                        ProductId = parts[1],
                        TrayId = parts[2],
                        Barcode = parts[3],
                        DefectType = parts[4],
                        TimeBlock = Convert.ToDateTime( parts[5]),
                        RecheckNGCount =Convert.ToInt32( parts[6]),
                        RecheckOKCount= Convert.ToInt32(parts[7])
                    };

                    list_defectRecords.Add(record);
                }
            }
        }

        // 处理中转站AI数据查询结果
        // 有没有可能做成一次查询批量结果，减少连接
        public void ProcessTransferAIInfoQueryResult(string strQueryResult, ref string aiResult)
        {
            // 解析查询结果
            string[] lines = strQueryResult.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);

            // 如果查询结果为空
            if (lines.Length == 0)
            {
                aiResult = "";
                return;
            }

            foreach (var line in lines)
            {
                string[] parts = line.Trim().Split(new char[] { '@' }, StringSplitOptions.None);

                if (parts.Length >= 1)
                {
                    aiResult = parts[4];
                }
            }

        }
    }
}