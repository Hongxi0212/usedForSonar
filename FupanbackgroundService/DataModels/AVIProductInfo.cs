using AutoScanFQCTest.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FupanBackgroundService.DataModels
{
    public class AVIProductInfo
    {
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

            [JsonPropertyName("area")]
            public double area { get; set; }                                 // 缺陷面积

            [JsonPropertyName("side")]
            public int side { get; set; }                                           // 缺陷所在的面，0为正面（A面），1为背面（B面），2为侧面1，3为侧面2

            [JsonPropertyName("light_channel")]
            public int light_channel { get; set; }                           // 缺陷所在的光源通道，0为光源1，1为光源2

            [JsonPropertyName("image_path")]
            public string image_path { get; set; }                 // 检出该缺陷所在图片的共享盘路径

            [JsonPropertyName("aiCam")]
            public int aiCam { get; set; }

            [JsonPropertyName("aiPos")]
            public int aiPos { get; set; }

            [JsonPropertyName("aiImageIndex")]
            public int aiImageIndex { get; set; }

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

            [JsonPropertyName("MES_failure_msg")]
            public string MES_failure_msg { get; set; }              // MES过站失败信息

            [JsonPropertyName("is_ok_product")]
            public bool is_ok_product { get; set; }                     // 是否是OK产品

            [JsonPropertyName("inspect_date_time")]
            public string inspect_date_time { get; set; }             // 检测时间

            [JsonPropertyName("region_area")]
            public string region_area { get; set; }                     // 区域

            [JsonPropertyName("mac_address")]
            public string mac_address { get; set; }                     // MAC地址

            [JsonPropertyName("ip_address")]
            public string ip_address { get; set; }                         // IP地址

            [JsonPropertyName("defects")]
            public List<DefectInfo> defects { get; set; }                  // 缺陷列表信息

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

            [JsonPropertyName("all_image_paths")]
            public string all_image_paths { get; set; }             //该料片的所有共享盘图片路径，以分号隔开写入

            [JsonPropertyName("r1")]
            public string r1 { get; set; }                           // 预留字段1，检测时间

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

            [JsonPropertyName("num_of_OK_trays")]
            public int num_of_OK_trays { get; set; }                      // OK料盘数量

            [JsonPropertyName("num_of_NG_trays")]
            public int num_of_NG_trays { get; set; }                     // NG料盘数量

            [JsonPropertyName("num_of_OK_products")]
            public int num_of_OK_products { get; set; }                 // OK产品数量

            [JsonPropertyName("num_of_NG_products")]
            public int num_of_NG_products { get; set; }                // NG产品数量

            [JsonPropertyName("r1")]
            public string r1 { get; set; }                           // 预留字段1，检测时间

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

        // 回复数据
        public class ResponseFromRecheck
        {
            [JsonPropertyName("error_code")]
            public int error_code { get; set; }                         // 错误码

            [JsonPropertyName("version")]
            public string version { get; set; }                         // 版本号

            [JsonPropertyName("msg")]
            public string msg { get; set; }

            [JsonPropertyName("data_port")]
            public int data_port { get; set; }                          // 数据端口
        }
    }
}
