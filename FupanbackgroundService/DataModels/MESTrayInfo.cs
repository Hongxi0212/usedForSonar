using AutoScanFQCTest.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static FupanBackgroundService.DataModels.AVIProductInfo;

namespace FupanBackgroundService.DataModels
{

    public enum RecheckResult
    {
        NotChecked = 0,
        OK = 1,
        NG = 2,
        DoNotNeedRecheck = 3,
        type4 = 4,
        type5 = 5,
        ET = 6,
        EMPTY_DEFECT = 7,
        NoCode = 8,                               // 无码
        Unrecheckable = 9,                     // 不可复判
        NotReceived = 10,                       // 未接收
        FailedPositioning = 11,              // 定位失败
        EmptySlot = 12,                          // 空穴
        NEED_CLEAN = 13,                // 待清洗
        MES_NG = 14,                           // MES过站NG
    }

    // MES料盘信息数据模型
    public class MESTrayInfo
    {
        public AVITrayInfo m_AVI_tray_data_Nova;

        public TrayInfo m_AVI_tray_data_Dock;

        public List<RecheckResult[]> m_list_recheck_flags_by_defect;
        public RecheckResult[] m_check_flags_by_product;

        public int[] m_array_indices_of_ET_products = null;
        public int[] m_array_indices_of_defected_products = null;
        public string[] m_array_barcodes_of_defected_products = null;

        // 构造函数
        public MESTrayInfo()
        {
            m_AVI_tray_data_Nova = null;
            m_list_recheck_flags_by_defect = null;
        }

        // 初始化数据
        public void init_data(AVITrayInfo avi_tray_info)
        {
            m_AVI_tray_data_Nova = avi_tray_info;

            m_list_recheck_flags_by_defect = new List<RecheckResult[]>();

            m_check_flags_by_product = new RecheckResult[m_AVI_tray_data_Nova.TotalPcs];
            for (int i = 0; i < m_AVI_tray_data_Nova.TotalPcs; i++)
            {
                m_check_flags_by_product[i] = RecheckResult.DoNotNeedRecheck;
            }
        }

        // 初始化数据
        public void init_data(TrayInfo tray_info)
        {
            m_AVI_tray_data_Dock = tray_info;

            m_list_recheck_flags_by_defect = new List<RecheckResult[]>();

            int nTotalPcs = m_AVI_tray_data_Dock.total_rows * m_AVI_tray_data_Dock.total_columns;

            m_check_flags_by_product = new RecheckResult[nTotalPcs];
            for (int i = 0; i < nTotalPcs; i++)
            {
                m_check_flags_by_product[i] = RecheckResult.DoNotNeedRecheck;
            }
        }
    }

    // 当一盘料被复判完成并成功上传到MES后，将触发此事件，以便通知其他复判站，实现集中复判的功能
    public class TrayRecheckedAndSubmittedEventTelegram
    {
        [JsonPropertyName("table_name")]
        public string table_name { get; set; }

        [JsonPropertyName("set_id")]
        public string set_id { get; set; }

        [JsonPropertyName("product_barcodes")]
        public List<string> product_barcodes { get; set; }

        // 预留字段
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
        public List<SummaryList> summarylist { get; set; }
    }

    public class MesPanelUploadInfo
    {
        [JsonPropertyName("panel")]
        public string Panel { get; set; }

        [JsonPropertyName("resource")]
        public string Resource { get; set; }

        [JsonPropertyName("site")]
        public string Site { get; set; }

        [JsonPropertyName("mac")]
        public string Mac { get; set; }

        [JsonPropertyName("programName")]
        public string ProgramName { get; set; }

        [JsonPropertyName("machine")]
        public string Machine { get; set; }

        [JsonPropertyName("product")]
        public string Product { get; set; }

        [JsonPropertyName("workArea")]
        public string WorkArea { get; set; }

        [JsonPropertyName("testType")]
        public string TestType { get; set; }

        [JsonPropertyName("testTime")]
        public string TestTime { get; set; }

        [JsonPropertyName("operatorName")]
        public string OperatorName { get; set; }

        [JsonPropertyName("operatorType")]
        public string OperatorType { get; set; }

        [JsonPropertyName("testMode")]
        public string TestMode { get; set; }

        [JsonPropertyName("trackType")]
        public string TrackType { get; set; }

        [JsonPropertyName("isRepair")]
        public string isRepair { get; set; }

        [JsonPropertyName("checkPcsDataForAVI")]
        public bool CheckPcsDataForAVI { get; set; }

        [JsonPropertyName("jdeProductName")]
        public string JdeProductName { get; set; }

        [JsonPropertyName("uuid")]
        public string Uuid { get; set; }

        [JsonPropertyName("versionFlag")]
        public string VersionFlag { get; set; }

        [JsonPropertyName("checkDetail")]
        public string CheckDetail { get; set; }

        [JsonPropertyName("pcsSummarys")]
        public List<PieceSummary> pcsSummarys { get; set; }
    }

    public class UVIPanelUploadInfo
    {
        //Panel
        [JsonPropertyName("Panel")]
        public string Panel { get; set; }
        //Resource
        [JsonPropertyName("Resource")]
        public string Resource { get; set; }
        //Product
        [JsonPropertyName("Product")]
        public string Product { get; set; }
        //Machine
        [JsonPropertyName("Machine")]
        public string Machine { get; set; }
        //WorkArea
        [JsonPropertyName("WorkArea")]
        public string WorkArea { get; set; }
        //OperatorName
        [JsonPropertyName("OperatorName")]
        public string OperatorName { get; set; }
        //trackType
        [JsonPropertyName("trackType")]
        public string TrackType { get; set; }
        //TestType
        [JsonPropertyName("TestType")]
        public string TestType { get; set; }
        //Mac
        [JsonPropertyName("Mac")]
        public string Mac { get; set; }
        //Site
        [JsonPropertyName("Site")]
        public string Site { get; set; }
        //ipaddress
        [JsonPropertyName("ipaddress")]
        public string IPAaddress { get; set; }
        //ProgramName
        [JsonPropertyName("ProgramName")]
        public string ProgramName { get; set; }
        //TestTime
        [JsonPropertyName("TestTime")]
        public string TestTime { get; set; }
        //OperatorType
        [JsonPropertyName("OperatorType")]
        public string OperatorType { get; set; }
        //testMode
        [JsonPropertyName("testMode")]
        public string TestMode { get; set; }
        //hasTrackFlag
        [JsonPropertyName("hasTrackFlag")]
        public string hasTrackFlag { get; set; }
        //detaillist[]
        [JsonPropertyName("detaillist")]
        public List<UVIDetail> DetailList { get; set; }
        //summarylist[]
        [JsonPropertyName("summarylist")]
        public List<UVISummary> SummaryList { get; set; }
    }

    public class PieceSummary
    {
        [JsonPropertyName("panelId")]
        public string PanelId { get; set; }

        [JsonPropertyName("pcsSeq")]
        public string PcsSeq { get; set; }

        [JsonPropertyName("testResult")]
        public string TestResult { get; set; }

        [JsonPropertyName("operatorName")]
        public string OperatorName { get; set; }

        [JsonPropertyName("operatorTime")]
        public string OperatorTime { get; set; }

        [JsonPropertyName("verifyResult")]
        public string VerifyResult { get; set; }

        [JsonPropertyName("verifyOperatorName")]
        public string VerifyOperatorName { get; set; }

        [JsonPropertyName("verifyTime")]
        public string VerifyTime { get; set; }

        [JsonPropertyName("RegionArea")]
        public string RegionArea { get; set; }

        [JsonPropertyName("aiResult")]
        public string aiResult { get; set; }

        [JsonPropertyName("aiTime")]
        public string aiTime { get; set; }

        [JsonPropertyName("pcsDetails")]
        public List<DefectDetail> pcsDetails { get; set; }
    }

    public class DefectDetail
    {
        [JsonPropertyName("panelId")]
        public string PanelId { get; set; }

        [JsonPropertyName("pcsBarCode")]
        public string PcsBarCode { get; set; }

        [JsonPropertyName("testType")]
        public string TestType { get; set; }

        [JsonPropertyName("pcsSeq")]
        public string PcsSeq { get; set; }

        [JsonPropertyName("partSeq")]
        public string PartSeq { get; set; }

        [JsonPropertyName("pinSeg")]
        public string PinSeg { get; set; }

        [JsonPropertyName("testResult")]
        public string TestResult { get; set; }

        [JsonPropertyName("operatorName")]
        public string OperatorName { get; set; }

        [JsonPropertyName("verifyResult")]
        public string VerifyResult { get; set; }

        [JsonPropertyName("verifyOperatorName")]
        public string VerifyOperatorName { get; set; }

        [JsonPropertyName("verifyTime")]
        public string VerifyTime { get; set; }

        [JsonPropertyName("defectCode")]
        public string DefectCode { get; set; }

        [JsonPropertyName("bubbleValue")]
        public string BubbleValue { get; set; }

        [JsonPropertyName("testFile")]
        public string TestFile { get; set; }

        [JsonPropertyName("imagePath")]
        public string ImagePath { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("strValue1")]
        public string StrValue1 { get; set; }

        [JsonPropertyName("strValue2")]
        public string StrValue2 { get; set; }

        [JsonPropertyName("strValue3")]
        public string StrValue3 { get; set; }

        [JsonPropertyName("strValue4")]
        public string StrValue4 { get; set; }

        [JsonPropertyName("aiResult")]
        public string aiResult { get; set; }

        [JsonPropertyName("aiTime")]
        public string aiTime { get; set; }

        [JsonPropertyName("aiDefectCode")]
        public string aiDefectCode { get; set; }
    }

    public class UVIDetail
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
    }

    public class UVISummary
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

    #region 用于提交AI复判的模型

    public class PointPosInfoSZ
    {
        [JsonPropertyName("side_A")]
        public List<CamPos> side_A { get; set; }

        [JsonPropertyName("side_B")]
        public List<CamPos> side_B { get; set; }

        [JsonPropertyName("side_C")]
        public List<CamPos> side_C { get; set; }
    }

    public class Side
    {
        [JsonPropertyName("cam0Pos0")]
        public CamPos cam0Pos0 { get; set; }
    }

    public class CamPos
    {
        [JsonPropertyName("side")]
        public string side { get; set; }

        [JsonPropertyName("camIndex")]
        public int camIndex { get; set; }

        [JsonPropertyName("posIndex")]
        public int posIndex { get; set; }

        [JsonPropertyName("baseSize")]
        public BaseSize baseSize { get; set; }

        [JsonPropertyName("pathChannel")]
        public List<string> pathChannel { get; set; }

        [JsonPropertyName("imgData")]
        public List<string> imgData { get; set; }

        [JsonPropertyName("defectNames")]
        public List<string> defectNames { get; set; }

        [JsonPropertyName("defects")]
        public List<AIDefect> defects { get; set; }
    }

    public class BaseSize
    {
        [JsonPropertyName("height")]
        public int height { get; set; }

        [JsonPropertyName("width")]
        public int width { get; set; }
    }

    public class AIDefect
    {
        [JsonPropertyName("defectArea")]
        public List<DefectArea> defectArea { get; set; }

        [JsonPropertyName("defectDetail")]
        public string defectDetail { get; set; }

        [JsonPropertyName("defectType")]
        public string defectType { get; set; }

        [JsonPropertyName("okNgLabel")]
        public string okNgLabel { get; set; }

        [JsonPropertyName("aiResult")]
        public string aiResult { get; set; }

        [JsonPropertyName("aiDefectCode")]
        public List<string> aiDefectCode { get; set; }

        [JsonPropertyName("aiGuid")]
        public string aiGuid { get; set; }

        [JsonPropertyName("aiRemarks")]
        public string aiRemarks { get; set; }

        [JsonPropertyName("roi")]
        public Roi roi { get; set; }
    }

    public class DefectArea
    {
        [JsonPropertyName("height")]
        public double height { get; set; }

        [JsonPropertyName("width")]
        public double width { get; set; }

        [JsonPropertyName("x")]
        public double x { get; set; }

        [JsonPropertyName("y")]
        public double y { get; set; }
    }

    public class Roi
    {
        [JsonPropertyName("height")]
        public double height { get; set; }

        [JsonPropertyName("width")]
        public double width { get; set; }

        [JsonPropertyName("x")]
        public double x { get; set; }

        [JsonPropertyName("y")]
        public double y { get; set; }

        public static implicit operator Roi(DefectArea v)
        {
            return new Roi()
            {
                height = v.height,
                width = v.width,
                x = v.x,
                y = v.y
            };
        }
    }

    #endregion
}
