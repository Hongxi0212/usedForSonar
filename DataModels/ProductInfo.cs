using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using System.ComponentModel;

namespace AutoScanFQCTest.DataModels
{
    // 产品信息数据模型
    public class ProductInfo
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
        public List<Product> Products { get; set; }

    }

    // 产品数据模型
    public class Product
    {
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

        [JsonPropertyName("shareimg_path")]
        public string ShareimgPath { get; set; }
    }

    // 缺陷数据模型
    public class Defect
    {
        [JsonPropertyName("height")]
        public double Height { get; set; }

        [JsonPropertyName("sn")]
        public int Sn { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("width")]
        public double Width { get; set; }

        [JsonPropertyName("x")]
        public double X { get; set; }

        [JsonPropertyName("y")]
        public double Y { get; set; }

        [JsonPropertyName("channel_")]
        public int Channel { get; set; }

        [JsonPropertyName("channelNum_")]
        public int ChannelNum { get; set; }
    }

    // 产品信息序列化和反序列化帮助类
    public class JsonSerializerHelper
    {
        // 序列化产品信息
        public static string SerializeProductInfo(ProductInfo productInfo)
        {
            return JsonSerializer.Serialize(productInfo, new JsonSerializerOptions { WriteIndented = true });
        }

        // 反序列化产品信息
        public static ProductInfo DeserializeProductInfo(string json)
        {
            return JsonSerializer.Deserialize<ProductInfo>(json);
        }
    }


    /// <summary>
    /// 申请虚拟盘号，服务器返回的格式，不用做保存
    /// </summary>
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
