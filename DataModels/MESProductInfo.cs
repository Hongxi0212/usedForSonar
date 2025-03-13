using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AutoScanFQCTest.DataModels
{
    public enum RecheckResult
    {
        NotChecked = 0,
        OK = 1,
        NG = 2,
        DoNotNeedRecheck = 3,
        type4 = 4,
        type5 = 5,
    }

    // MES产品信息数据模型
    public class MESProductInfo
    {
        public AVIProductInfo m_AVI_product_info;

        public RecheckResult[] m_recheck_flags;

        public int[] m_array_indices_of_defected_products = null;
        public string[] m_array_barcodes_of_defected_products = null;

        // 构造函数
        public MESProductInfo()
        {
            m_AVI_product_info = null;
            m_recheck_flags = null;
        }

        // 初始化数据
        public void init_data(AVIProductInfo avi_product_info)
        {
            m_AVI_product_info = avi_product_info;

            m_recheck_flags = new RecheckResult[avi_product_info.TotalPcs];
            for (int i = 0; i < m_recheck_flags.Length; i++)
            {
                m_recheck_flags[i] = RecheckResult.DoNotNeedRecheck;
            }
        }
    }

    public class DetailList
    {
        [JsonPropertyName("panelId")]
        public string panelId { get; set; }

        [JsonPropertyName("pcsBarCode")]
        public string pcsBarCode { get; set; }

        [JsonPropertyName("testType")]
        public string testType { get; set; }

        [JsonPropertyName("pcsSeq")]
        public string pcsSeq { get; set; }

        [JsonPropertyName("partSeq")]
        public string partSeq { get; set; }

        [JsonPropertyName("pinSeq")]
        public string pinSeq { get; set; }

        [JsonPropertyName("testResult")]
        public string testResult { get; set; }

        [JsonPropertyName("operatorName")]
        public string operatorName { get; set; }

        [JsonPropertyName("verifyResult")]
        public string verifyResult { get; set; }

        [JsonPropertyName("verifyOperatorName")]
        public string verifyOperatorName { get; set; }

        [JsonPropertyName("verifyTime")]
        public string verifyTime { get; set; }

        [JsonPropertyName("defectCode")]
        public string defectCode { get; set; }

        [JsonPropertyName("bubbleValue")]
        public string bubbleValue { get; set; }

        [JsonPropertyName("testFile")]
        public string testFile { get; set; }

        [JsonPropertyName("imagePath")]
        public string imagePath { get; set; }

        [JsonPropertyName("description")]
        public string description { get; set; }

        [JsonPropertyName("strValue1")]
        public string strValue1 { get; set; }

        [JsonPropertyName("strValue2")]
        public string strValue2 { get; set; }

        [JsonPropertyName("strValue3")]
        public string strValue3 { get; set; }

        [JsonPropertyName("strValue4")]
        public string strValue4 { get; set; }

        [JsonPropertyName("strValue5")]
        public string strValue5 { get; set; }

        [JsonPropertyName("strValue6")]
        public string strValue6 { get; set; }

        [JsonPropertyName("strValue7")]
        public string strValue7 { get; set; }

        [JsonPropertyName("strValue8")]
        public string strValue8 { get; set; }

        [JsonPropertyName("strValue9")]
        public string strValue9 { get; set; }

        [JsonPropertyName("strValue10")]
        public string strValue10 { get; set; }
    }

    public class SummaryList
    {
        [JsonPropertyName("panelId")]
        public string panelId { get; set; }

        [JsonPropertyName("pcsSeq")]
        public string pcsSeq { get; set; }

        [JsonPropertyName("testResult")]
        public string testResult { get; set; }

        [JsonPropertyName("operatorName")]
        public string operatorName { get; set; }

        [JsonPropertyName("operatorTime")]
        public string operatorTime { get; set; }

        [JsonPropertyName("verifyResult")]
        public string verifyResult { get; set; }

        [JsonPropertyName("verifyOperatorName")]
        public string verifyOperatorName { get; set; }

        [JsonPropertyName("verifyTime")]
        public string verifyTime { get; set; }
    }

    public class MicInfoList
    {
        [JsonPropertyName("panelId")]
        public string panelId { get; set; }

        [JsonPropertyName("pcsSeq")]
        public string pcsSeq { get; set; }

        [JsonPropertyName("micBarCode")]
        public string micBarCode { get; set; }

        [JsonPropertyName("micType")]
        public string micType { get; set; }
    }

    public class PreussresList
    {
        [JsonPropertyName("barcode")]
        public string barcode { get; set; }

        [JsonPropertyName("resourcename")]
        public string resourcename { get; set; }

        [JsonPropertyName("toolname")]
        public string toolname { get; set; }

        [JsonPropertyName("parm_loc")]
        public string parm_loc { get; set; }

        [JsonPropertyName("parm_pin")]
        public string parm_pin { get; set; }

        [JsonPropertyName("parm_cur")]
        public string parm_cur { get; set; }

        [JsonPropertyName("parm_min")]
        public string parm_min { get; set; }

        [JsonPropertyName("parm_max")]
        public string parm_max { get; set; }

        [JsonPropertyName("parm_result")]
        public string parm_result { get; set; }

        [JsonPropertyName("parm_operator")]
        public string parm_operator { get; set; }

        [JsonPropertyName("preesure_date")]
        public string preesure_date { get; set; }
    }

    public class OrionInfoList
    {
        [JsonPropertyName("panelId")]
        public string panelId { get; set; }

        [JsonPropertyName("pcsSeq")]
        public string pcsSeq { get; set; }

        [JsonPropertyName("OrionBarCode")]
        public string OrionBarCode { get; set; }

        [JsonPropertyName("OrionType")]
        public string OrionType { get; set; }
    }

    public class PanelRecheckResult
    {
        [JsonPropertyName("panel")]
        public string panel { get; set; }

        [JsonPropertyName("resource")]
        public string resource { get; set; }

        [JsonPropertyName("machine")]
        public string machine { get; set; }

        [JsonPropertyName("product")]
        public string product { get; set; }

        [JsonPropertyName("workArea")]
        public string workArea { get; set; }

        [JsonPropertyName("testType")]
        public string testType { get; set; }

        [JsonPropertyName("testTime")]
        public string testTime { get; set; }

        [JsonPropertyName("operatorName")]
        public string operatorName { get; set; }

        [JsonPropertyName("operatorType")]
        public string operatorType { get; set; }

        [JsonPropertyName("testMode")]
        public string testMode { get; set; }

        [JsonPropertyName("site")]
        public string site { get; set; }

        [JsonPropertyName("mac")]
        public string mac { get; set; }

        [JsonPropertyName("programName")]
        public string programName { get; set; }

        [JsonPropertyName("trackType")]
        public string trackType { get; set; }

        [JsonPropertyName("hasTrackFlag")]
        public string hasTrackFlag { get; set; }

        [JsonPropertyName("ipaddress")]
        public string ipaddress { get; set; }

        [JsonPropertyName("detaillist")]
        public List<DetailList> detaillist { get; set; }

        [JsonPropertyName("summarylist")]
        public List<SummaryList>  summarylist { get; set; }

        [JsonPropertyName("HasMic")]
        public string HasMic { get; set; }

        [JsonPropertyName("micinfolist")]
        public List<MicInfoList>  micinfolist { get; set; }

        [JsonPropertyName("preussresList")]
        public List<PreussresList>  preussresList { get; set; }

        [JsonPropertyName("HasOrion")]
        public string HasOrion { get; set; }

        [JsonPropertyName("orionInfoList")]
        public List<OrionInfoList> orionInfoList { get; set; }
    }
}
