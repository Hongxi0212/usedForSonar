using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoScanFQCTest.DataModels
{
    /// <summary>
    /// 产品复判结果表
    /// </summary>
    public class ProductResult
    {
        /// <summary>
        /// 物料盘id
        /// </summary>
        [DB("SetId")]
        public string SetId { get; set; }
        /// <summary>
        /// 产品id
        /// </summary>
        [DB("BarCode")]
        public string BarCode { get; set; }
        /// <summary>
        /// 复判结果
        /// </summary>
        [DB("Result")]
        public string Result { get; set; }
        /// <summary>
        /// 复判时间
        /// </summary>
        [DB("TestDate")]
        public string TestDate { get; set; }
        /// <summary>
        /// 复判人
        /// </summary>
        [DB("Tester")]
        public string Tester { get; set; }
    }

    // 复判数据统计缺陷类
    public class DefectRecord
    {
        public string ProductName { get; set; }
        public string Operator { get; set; }
        public string Factory { get; set; }
        public string WorkArea { get; set; }
        public string MachineId { get; set; }
        public string ProductId { get; set; }
        public string Uuid { get; set; }
        public string SetId { get; set; }
        public string RecheckPerson { get; set; }
        public string RecheckTime { get; set; }
        public int NumberOfRows { get; set; }
        public int NumberOfCols { get; set; }
        public int PosRow { get; set; }
        public int PosCol { get; set; }
        public string OriginDefectType { get; set; }
        public int OriginResult { get; set; }
        public string VerifiedDefectType { get; set; }
        public int VerifiedResult { get; set; }
        public string ImageFilePath { get; set; }
        public string R1 { get; set; }
        public string R2 { get; set; }
        public string R3 { get; set; }
        public string R4 { get; set; }
        public string R5 { get; set; }
        public string R6 { get; set; }
        public string R7 { get; set; }
        public string R8 { get; set; }
        public string R9 { get; set; }
        public string R10 { get; set; }
        public DateTime CreateTime { get; set; } = DateTime.Now;
    }

    public class ProductRecord
    {
        public string SetId { get; set; }
        public string Uuid { get; set; }
        public string BarCode { get; set; }
        public string PanelId { get; set; }
        public int PosCol { get; set; }
        public int PosRow { get; set; }
        public string ImgPath { get; set; }
        public int OriginResult { get; set; }
        public int VerifiedResult { get; set; }
    }

    public class TrayRecord
    {
        public string BatchId { get; set; }
        public int NumberOfRows { get; set; }
        public int NumberOfCols { get; set; }
        public string MachineId { get; set; }
        public string Operator { get; set; }
        public string SetId { get; set; }
        public string Uuid { get; set; }
    }

    public class DefectStatistics
    {
        public string MachineId { get; set; }
        public string ProductId { get; set; }
        public string TrayId { get; set; }
        public string Barcode { get; set; }
        public string DefectType { get; set; }
        public DateTime TimeBlock { get; set; }
		public int RecheckNGCount { get; set; }
		public int RecheckOKCount { get; set; }
	}
}
