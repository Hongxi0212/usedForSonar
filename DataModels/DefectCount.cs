using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoScanFQCTest.DataModels
{
    /// <summary>
    /// 缺陷统计实体
    /// </summary>
    public class DefectCount
    {
        /// <summary>
        /// 缺陷名称
        /// </summary>
        [DB("DefectName")]
        public string DefectName { get; set; }
        /// <summary>
        /// 缺陷计数
        /// </summary>
        [DB("Count")]
        public int Count { get; set; }
    }
}
