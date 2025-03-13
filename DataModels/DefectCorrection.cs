using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoScanFQCTest.DataModels
{
	/// <summary>
	/// 缺陷纠正统计实体
	/// </summary>
	public class DefectCorrection
	{
		/// <summary>
		/// 原错误的缺陷类型
		/// </summary>
		public string OriginalType { get; set; }

		/// <summary>
		/// 纠正后的缺陷类型
		/// </summary>
		public string CorrectedType { get; set; }

		public string ProductSN { get; set; }

		public string ProductID { get; set; }

		public string TraySN { get; set; }

		public string ImagePath {  get; set; }

		public string OperatorID { get; set; }

		public DateTime TestTime { get; set; }

		public DateTime CorrectTime { get; set; }
	}
}
