using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoScanFQCTest.DataModels
{
    public class DefectDefinition
    {
        public static string[] m_strFQCDefectCategories = new string[] { "SMT不良", "组装不良", "其它不良" };

        public static string[] m_strFQCDefectFamilies = new string[] { "开路", "短路", "虚焊", "漏焊", "焊锡量不足", "焊锡量过多", "焊锡短路", "焊锡开路", 
            "焊锡虚焊", "焊锡漏焊", "焊锡不良", "焊锡其它", "元件翻转", "元件错装", "元件偏移", "元件损坏", "元件其它", "其它不良" };

        public static string[] m_strFQCDefectReasons = new string[] { "钢/镍片污染", "钢片开口不良", "错件", "锡盖金", "PSA不良", "PSA异物", "胶不良", 
            "胶散点", "弯折不良", "3D钢片不良" };

        public static string[] m_strFQCTop10Defects = new string[] { "Top1", "Top2", "Top3", "Top4", "Top5", "Top6", "Top7", "Top8", "Top9", "Top10" };
    }
}
