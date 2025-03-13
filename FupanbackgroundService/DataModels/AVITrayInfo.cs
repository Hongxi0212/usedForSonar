using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using System.ComponentModel;

namespace AutoScanFQCTest.DataModels
{
    // AVI料盘信息数据模型
    public class AVITrayInfo
    {
        [JsonPropertyName("batch_id")]
        public string BatchId { get; set; }

        [JsonPropertyName("row")]
        public int Row { get; set; }

        [JsonPropertyName("col")]
        public int Col { get; set; }

        //[JsonPropertyName("exchange_model_")]
        //public int ExchangeModel { get; set; }

        [JsonPropertyName("front")]
        public bool Front { get; set; }

        [JsonPropertyName("mid")]
        public string Mid { get; set; }

        [JsonPropertyName("operator")]
        public string Operator { get; set; }

        [JsonPropertyName("operator_id")]
        public string OperatorId { get; set; }

        [JsonPropertyName("product_id")]
        public string ProductId { get; set; }

        [JsonPropertyName("resource")]
        public string Resource { get; set; }

        [JsonPropertyName("scan_code_mode_")]
        public int ScanCodeMode { get; set; }

        [JsonPropertyName("set_id")]
        public string SetId { get; set; }

        [JsonPropertyName("total_pcs")]
        public int TotalPcs { get; set; }

        [JsonPropertyName("uuid")]
        public string Uuid { get; set; }

        [JsonPropertyName("work_area")]
        public string WorkArea { get; set; }

        [JsonPropertyName("full_status")]
        public string Fullstatus { get; set; }

        [JsonPropertyName("products")]
        public List<Product> Products { get; set; }              // 只包含带有缺陷的产品信息

        [JsonPropertyName("products_to_clean")]
        public List<Product> products_to_clean { get; set; }              // 只包含待清洁产品信息

        [JsonPropertyName("okpanelcount")]
        public int okpanelcount { get; set; }//ok盘数

        [JsonPropertyName("ngpanelcount")]
        public int ngpanelcount { get; set; }//ng盘数

        [JsonPropertyName("okproductcount")]
        public int okproductcount { get; set; }//ok产品数

        [JsonPropertyName("ngproductcount")]
        public int ngproductcount { get; set; }//ng产品数

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

    // 产品数据模型
    public class Product
    {
        public List<string> m_list_local_imageA_paths_for_channel1;
        public List<string> m_list_local_imageB_paths_for_channel1;
        public List<string> m_list_local_imageC_paths_for_channel1;
        public List<string> m_list_local_imageA_paths_for_channel2;
        public List<string> m_list_local_imageB_paths_for_channel2;
        public List<string> m_list_local_imageC_paths_for_channel2;
        public List<string> m_list_remote_imageA_paths_for_channel1;
        public List<string> m_list_remote_imageB_paths_for_channel1;
        public List<string> m_list_remote_imageC_paths_for_channel1;
        public List<string> m_list_remote_imageA_paths_for_channel2;
        public List<string> m_list_remote_imageB_paths_for_channel2;
        public List<string> m_list_remote_imageC_paths_for_channel2;

        [JsonPropertyName("set_id")]
        public string SetId { get; set; }

        [JsonPropertyName("uuid")]
        public string Uuid { get; set; }

        [JsonPropertyName("bar_code")]
        public string BarCode { get; set; }

        [JsonPropertyName("defects")]
        public List<Defect> Defects { get; set; }

        [JsonPropertyName("image")]
        public string Image { get; set; }

        [JsonPropertyName("image_a")]
        public string ImageA { get; set; }

        [JsonPropertyName("image_b")]
        public string ImageB { get; set; }

        [JsonPropertyName("is_null")]
        public string IsNull { get; set; }

        [JsonPropertyName("panel_id")]
        public string PanelId { get; set; }

        [JsonPropertyName("pos_col")]
        public int PosCol { get; set; }

        [JsonPropertyName("pos_row")]
        public int PosRow { get; set; }

        [JsonPropertyName("shareimgPath")]
        public string ShareimgPath { get; set; }

        [JsonPropertyName("recheck_result")]
        public string RecheckResult { get; set; }

        [JsonPropertyName("aiResult")]
        public string aiResult { get; set; }

        [JsonPropertyName("aiTime")]
        public string aiTime { get; set; }

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

    // 缺陷数据模型
    public class Defect
    {
        [JsonPropertyName("id")]
        public int id { get; set; }

        [JsonPropertyName("set_id")]
        public string SetId { get; set; }

        [JsonPropertyName("uuid")]
        public string Uuid { get; set; }

        [JsonPropertyName("product_id")]
        public string product_id { get; set; }              // 必要，二维码

        [JsonPropertyName("height")]
        public double Height { get; set; }                  // 必要，缺陷高度

        [JsonPropertyName("sn")]
        public int Sn { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }                   // 必要，缺陷类型

        [JsonPropertyName("width")]
        public double Width { get; set; }                 // 必要，缺陷宽度

        [JsonPropertyName("x")]
        public double X { get; set; }                    // 必要，缺陷中心坐标X

        [JsonPropertyName("y")]
        public double Y { get; set; }                     // 必要，缺陷中心坐标Y

        [JsonPropertyName("channel_")]
        public int Channel { get; set; }

        [JsonPropertyName("channelNum_")]
        public int ChannelNum { get; set; }

        [JsonPropertyName("area")]
        public double Area { get; set; }

        [JsonPropertyName("um_per_pixel")]
        public double um_per_pixel { get; set; }

        [JsonPropertyName("aiResult")]
        public string aiResult { get; set; }

        [JsonPropertyName("aiDefectCode")]
        public List<string> aiDefectCode { get; set; }

        [JsonPropertyName("aiGuid")]
        public string aiGuid { get; set; }
    }

    // 产品信息序列化和反序列化帮助类
    public class JsonSerializerHelper
    {
        // 序列化产品信息
        public static string SerializeProductInfo(AVITrayInfo productInfo)
        {
            return JsonSerializer.Serialize(productInfo, new JsonSerializerOptions { WriteIndented = true });
        }

        // 反序列化产品信息
        public static AVITrayInfo DeserializeProductInfo(string json)
        {
            return JsonSerializer.Deserialize<AVITrayInfo>(json);
        }
    }

    // 申请虚拟盘号，服务器返回的格式，不用做保存
    public class MESAquireSetId
    {
        // 返回码
        [JsonPropertyName("err_code")]
        public string err_code { get; set; }

        // 数据
        [JsonPropertyName("data")]
        public AquireData data { get; set; }
    }

    public class AquireData
    {
        [JsonPropertyName("uuid")]
        public string uuid { get; set; }

        // 虚拟盘号
        [JsonPropertyName("set_id")]
        public string set_id { get; set; }

        [JsonPropertyName("product_id")]
        public string product_id { get; set; }
    }

}
