﻿using AutoScanFQCTest.DataModels;
using AutoScanFQCTest.Logics;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AutoScanFQCTest.Utilities
{
    // 复判软件和数据收集软件之间的通信数据
    public class ReportData
    {
        [JsonPropertyName("error_code")]
        public int error_code { get; set; }                              // 错误码

        [JsonPropertyName("message")]
        public string message { get; set; }                             // 消息
    }

    // 缺陷信息
    public class DefectInfo
    {
        [JsonPropertyName("ID")]
        public int ID { get; set; }                                   // 序号

        [JsonPropertyName("set_id")]
        public string set_id { get; set; }                             // 虚拟盘号，基于时间戳生成

        [JsonPropertyName("product_id")]
        public string product_id { get; set; }                     // 产品ID，即二维码

        [JsonPropertyName("type")]
        public string type { get; set; }                              // 缺陷类型

        [JsonPropertyName("center_x")]
        public double center_x { get; set; }                                 // 缺陷中心x坐标

        [JsonPropertyName("center_y")]
        public double center_y { get; set; }                                 // 缺陷中心y坐标

        [JsonPropertyName("width")]
        public double width { get; set; }                                 // 缺陷宽度

        [JsonPropertyName("height")]
        public double height { get; set; }                                 // 缺陷高度

        [JsonPropertyName("side")]
        public int side { get; set; }                                           // 缺陷所在的面，0为正面（A面），1为背面（B面），2为侧面1，3为侧面2

        [JsonPropertyName("light_channel")]
        public int light_channel { get; set; }                           // 缺陷所在的光源通道，0为光源1，1为光源2

        [JsonPropertyName("r1")]
        public string r1 { get; set; }                           // 预留字段1

        [JsonPropertyName("r2")]
        public string r2 { get; set; }                           // 预留字段2

        [JsonPropertyName("r3")]
        public string r3 { get; set; }                           // 预留字段3

        [JsonPropertyName("r4")]
        public string r4 { get; set; }                           // 预留字段4

        [JsonPropertyName("r5")]
        public string r5 { get; set; }                           // 预留字段5

        [JsonPropertyName("r6")]
        public string r6 { get; set; }                           // 预留字段6

        [JsonPropertyName("r7")]
        public string r7 { get; set; }                           // 预留字段7

        [JsonPropertyName("r8")]
        public string r8 { get; set; }                           // 预留字段8

        [JsonPropertyName("r9")]
        public string r9 { get; set; }                           // 预留字段9

        [JsonPropertyName("r10")]
        public string r10 { get; set; }                           // 预留字段10
    }

    // 产品信息
    public class ProductInfo
    {
        public List<string> m_list_local_imageA_paths_for_channel1;
        public List<string> m_list_local_imageB_paths_for_channel1;
        public List<string> m_list_local_imageC_paths_for_channel1;
        public List<string> m_list_local_imageD_paths_for_channel1;
        public List<string> m_list_local_imageA_paths_for_channel2;
        public List<string> m_list_local_imageB_paths_for_channel2;
        public List<string> m_list_local_imageC_paths_for_channel2;
        public List<string> m_list_local_imageD_paths_for_channel2;
        public List<string> m_list_remote_imageA_paths_for_channel1;
        public List<string> m_list_remote_imageB_paths_for_channel1;
        public List<string> m_list_remote_imageC_paths_for_channel1;
        public List<string> m_list_remote_imageD_paths_for_channel1;
        public List<string> m_list_remote_imageA_paths_for_channel2;
        public List<string> m_list_remote_imageB_paths_for_channel2;
        public List<string> m_list_remote_imageC_paths_for_channel2;
        public List<string> m_list_remote_imageD_paths_for_channel2;

        [JsonPropertyName("set_id")]
        public string set_id { get; set; }                             // 虚拟盘号，基于时间戳生成

        [JsonPropertyName("machine_id")]
        public string machine_id { get; set; }                     // 机台ID

        [JsonPropertyName("barcode")]
        public string barcode { get; set; }                            // 二维码

        [JsonPropertyName("column")]
        public int column { get; set; }                                 // 产品所在列号

        [JsonPropertyName("row")]
        public int row { get; set; }                                      // 产品所在行号

        [JsonPropertyName("bET")]
        public bool bET { get; set; }                                      // 是否是ET产品

        [JsonPropertyName("defects")]
        public List<DefectInfo> defects { get; set; }                  // 缺陷列表信息

        [JsonPropertyName("defects_with_NULL_or_empty_type_name")]
        public List<DefectInfo> defects_with_NULL_or_empty_type_name { get; set; }                   // 用于保存缺陷类型为空的缺陷（检胶项目专用，用于保存OK位置）

        [JsonPropertyName("sideA_image_path")]
        public string sideA_image_path { get; set; }             // A面图像路径

        [JsonPropertyName("sideB_image_path")]
        public string sideB_image_path { get; set; }             // B面图像路径

        [JsonPropertyName("sideC_image_path")]
        public string sideC_image_path { get; set; }             // 侧面1图像路径

        [JsonPropertyName("sideD_image_path")]
        public string sideD_image_path { get; set; }             // 侧面2图像路径

        [JsonPropertyName("sideE_image_path")]
        public string sideE_image_path { get; set; }             // 侧面3图像路径

        [JsonPropertyName("sideF_image_path")]
        public string sideF_image_path { get; set; }             // 侧面4图像路径

        [JsonPropertyName("sideG_image_path")]
        public string sideG_image_path { get; set; }             // 侧面5图像路径

        [JsonPropertyName("sideH_image_path")]
        public string sideH_image_path { get; set; }             // 侧面6图像路径

        [JsonPropertyName("r1")]
        public string r1 { get; set; }                           // 预留字段1，检测时间

        [JsonPropertyName("r2")]
        public string r2 { get; set; }                           // 预留字段2，RegionArea

        [JsonPropertyName("r3")]
        public string r3 { get; set; }                           // 预留字段3

        [JsonPropertyName("r4")]
        public string r4 { get; set; }                           // 预留字段4

        [JsonPropertyName("r5")]
        public string r5 { get; set; }                           // 预留字段5

        [JsonPropertyName("r6")]
        public string r6 { get; set; }                           // 预留字段6

        [JsonPropertyName("r7")]
        public string r7 { get; set; }                           // 预留字段7

        [JsonPropertyName("r8")]
        public string r8 { get; set; }                           // 预留字段8

        [JsonPropertyName("r9")]
        public string r9 { get; set; }                           // 预留字段9

        [JsonPropertyName("r10")]
        public string r10 { get; set; }                           // 预留字段10
    }

    // 料盘信息
    public class TrayInfo
    {
        [JsonPropertyName("set_id")]
        public string set_id { get; set; }                             // 虚拟盘号，基于时间戳生成

        [JsonPropertyName("machine_id")]
        public string machine_id { get; set; }                     // 机台ID

        [JsonPropertyName("total_columns")]
        public int total_columns { get; set; }                        // 总列数

        [JsonPropertyName("total_rows")]
        public int total_rows { get; set; }                           // 总行数

        [JsonPropertyName("operator")]
        public string Operator { get; set; }                         // 操作员

        [JsonPropertyName("operator_id")]
        public string operator_id { get; set; }                     // 操作员ID

        [JsonPropertyName("site")]
        public string site { get; set; }                                  // 厂区

        [JsonPropertyName("products")]
        public List<ProductInfo> products { get; set; }              // 只包含带有缺陷的产品信息

        [JsonPropertyName("ET_products")]
        public List<ProductInfo> ET_products { get; set; }              // 只包含ET产品信息

        [JsonPropertyName("OK_products")]
        public List<ProductInfo> OK_products { get; set; }              // 只包含OK产品信息

        [JsonPropertyName("nocode_products")]
        public List<ProductInfo> nocode_products { get; set; }              // 只包含无二维码产品信息

        [JsonPropertyName("MES_failed_products")]
        public List<ProductInfo> MES_failed_products { get; set; }              // MES过站失败产品信息

        [JsonPropertyName("products_with_unrecheckable_defect")]
        public List<ProductInfo> products_with_unrecheckable_defect { get; set; }              // 只包含不可复判缺陷的产品信息

        [JsonPropertyName("products_with_failed_positioning")]
        public List<ProductInfo> products_with_failed_positioning { get; set; }              // 定位失败的产品信息

        [JsonPropertyName("empty_slots")]
        public List<ProductInfo> empty_slots { get; set; }              // 空穴信息

        [JsonPropertyName("num_of_OK_trays")]
        public int num_of_OK_trays { get; set; }                      // OK料盘数量

        [JsonPropertyName("num_of_NG_trays")]
        public int num_of_NG_trays { get; set; }                     // NG料盘数量

        [JsonPropertyName("num_of_OK_products")]
        public int num_of_OK_products { get; set; }                 // OK产品数量

        [JsonPropertyName("num_of_NG_products")]
        public int num_of_NG_products { get; set; }                // NG产品数量

        [JsonPropertyName("r1")]
        public string r1 { get; set; }                           // r1为布尔值，表示是否已经提交MES

        [JsonPropertyName("r2")]
        public string r2 { get; set; }                           // 预留字段2，RegionArea

        [JsonPropertyName("r3")]
        public string r3 { get; set; }                           // 预留字段3

        [JsonPropertyName("r4")]
        public string r4 { get; set; }                           // 预留字段4

        [JsonPropertyName("r5")]
        public string r5 { get; set; }                           // 预留字段5

        [JsonPropertyName("r6")]
        public string r6 { get; set; }                           // 预留字段6

        [JsonPropertyName("r7")]
        public string r7 { get; set; }                           // 预留字段7

        [JsonPropertyName("r8")]
        public string r8 { get; set; }                           // 预留字段8

        [JsonPropertyName("r9")]
        public string r9 { get; set; }                           // 预留字段9

        [JsonPropertyName("r10")]
        public string r10 { get; set; }                           // 预留字段10
    }

    // 回复数据
    public class ResponseFromRecheck
    {
        [JsonPropertyName("error_code")]
        public int error_code { get; set; }

        [JsonPropertyName("version")]
        public string version { get; set; }

        [JsonPropertyName("msg")]
        public string msg { get; set; }
    }

    public class Test
    {
        public static string m_strRecheckURL = "http://127.0.0.1:8080/vml/dcs/report_data/";        // 复判软件URL

        // 发送数据到复判软件
        public static bool SendHttpServiceRequest(string strURL, string strDataToSend, ref string strResponse, int nGetOrPostFlag = 1)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(strURL);

                request.Method = "POST";
                request.ContentType = "application/json";

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
                Debugger.Log(0, null, string.Format("222222 MES请求异常，可能是连接失败，异常信息：{0}", ex.Message));
                return false;
            }
        }

        public static void Test1()
        {
            // 生成托盘信息
            TrayInfo tray_info = new TrayInfo();

            if (true)
            {
                // set_id为时间戳
                tray_info.set_id = DateTime.Now.ToString("yyyyMMdd_HHmmssfff");
                tray_info.total_columns = 10;
                tray_info.total_rows = 10;
                tray_info.num_of_OK_products = 0;
                tray_info.num_of_NG_products = 0;
                tray_info.site = "SZ";

                for (int i = 0; i < tray_info.total_columns; i++)
                {
                    for (int j = 0; j < tray_info.total_rows; j++)
                    {
                        ProductInfo product_info = new ProductInfo();

                        product_info.set_id = tray_info.set_id;
                        product_info.row = i;
                        product_info.column = j;
                        product_info.barcode = "1234567890";                                  // 改为实际的二维码
                        product_info.sideA_image_path = "D:\\images\\1.png";         // 改为实际的图像路径
                        product_info.sideB_image_path = "D:\\images\\2.png";        // 改为实际的图像路径

                        // 生成缺陷信息，按照实际情况生成
                        for (int k = 0; k < 3; k++)
                        {
                            DefectInfo defect_info = new DefectInfo();

                            defect_info.set_id = tray_info.set_id;
                            defect_info.type = "scratch";
                            defect_info.width = 20;
                            defect_info.height = 20;
                            defect_info.center_x = 100.0;
                            defect_info.center_y = 100.0;
                            defect_info.side = 0;

                            if (null == product_info.defects)
                                product_info.defects = new List<DefectInfo>();

                            product_info.defects.Add(defect_info);
                        }

                        if (null == tray_info.products)
                            tray_info.products = new List<ProductInfo>();

                        tray_info.products.Add(product_info);
                    }
                }
            }

            // 序列化为JSON字符串
            string strJson = JsonConvert.SerializeObject(tray_info);

            string strMesServerResponse = "";

            bool bRet = SendHttpServiceRequest(m_strRecheckURL, strJson, ref strMesServerResponse, 1);

            if (true == bRet)
            {
                ResponseFromRecheck response = JsonConvert.DeserializeObject<ResponseFromRecheck>(strMesServerResponse);

                if (response.error_code == 0)
                {
                    // 复判软件成功接收数据
                }
                else
                {
                    // 复判软件接收数据异常，需要显示和记录异常信息
                }
            }
            else
            {
                // 复判软件接收数据异常
            }
        }
    }
}
