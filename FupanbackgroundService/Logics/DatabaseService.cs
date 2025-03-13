using FupanBackgroundService.DataModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FupanBackgroundService.Logics
{
    public class DatabaseService
    {
        private static readonly object _lock = new object();

        public object DatabaseHelper { get; private set; }

        // 构造函数
        public DatabaseService()
        {

        }

        // 功能：删除相同set_id的tray数据
        public void DeleteTrayDataIfExist(string strTrayTable, string set_id)
        {
            string deleteQuery = $"DELETE FROM " + strTrayTable + $" WHERE set_id = '{set_id}'";

            // 执行删除操作
            string strError = "";
            Global.m_instance.m_mysql_ops.ExecuteQueryWithoutNeedingResult(deleteQuery, ref strError);
        }

        // 功能：删除相同set_id的product数据
        public void DeleteProductDataIfExist(string strProductTable, string strBarcode)
        {
            string deleteQuery = $"DELETE FROM " + strProductTable + $" WHERE bar_code = '{strBarcode}'";

            // 执行删除操作
            string strError = "";
            Global.m_instance.m_mysql_ops.ExecuteQueryWithoutNeedingResult(deleteQuery, ref strError);
        }

        // 功能：删除相同set_id的defect数据
        public void DeleteDefectDataIfExist(string strDefectTable, string product_id)
        {
            string deleteQuery = $"DELETE FROM " + strDefectTable + $" WHERE product_id = '{product_id}'";

            // 执行删除操作
            string strError = "";
            Global.m_instance.m_mysql_ops.ExecuteQueryWithoutNeedingResult(deleteQuery, ref strError);
        }

        public void InsertSingleSideInfoToAIData(string strTableName, string side, MesAISingleSideAIInfo response)
        {
            string insertQuery = @"INSERT INTO " + strTableName + @" (side, machine, uuid, barcode, aiResult, pointInfoJson) VALUES (@side, @machine, @uuid, @barcode, @aiResult, @pointInfoJson);";
            Global.m_instance.m_mysql_ops.AddSingleSideAIInfoToTable(strTableName, insertQuery, side, response);
        }

        public void DeleteSingleSideAIInfoDataByBarcode(string strTableName, string side, string barcode)
        {
            string deleteQuery = @"DELETE FROM " + strTableName + @" WHERE side = @side AND barcode = @barcode";
            Global.m_instance.m_mysql_ops.DeleteSingleSideAIInfoFromTableByBarcode(strTableName, deleteQuery, side, barcode);
        }

        public void DeleteSingleSideAIInfoDataByUUid(string strTableName, string side, string uuid)
        {
            string deleteQuery = @"DELETE FROM " + strTableName + @" WHERE side = @side AND uuid = @uuid";
            Global.m_instance.m_mysql_ops.DeleteSingleSideAIInfoFromTableByUUid(strTableName, deleteQuery, side, uuid);
        }

        public MesAISingleSideAIInfo FindSingleSideAIInfoDataWithBarcode(string strTableName, string barcode)
        {
            var singleSideInfo = new MesAISingleSideAIInfo() { pointPosInfo = new List<CamPos>() };

            Debugger.Log(0, null, $"88888888 开始以barcode：{barcode}查询数据库中相关的uuid信息");

            string findQuery = @"SELECT uuid FROM " + strTableName + @" WHERE barcode = @barcode";
            var uuid = Global.m_instance.m_mysql_ops.GetSingleSideUUidByBarcode(strTableName, findQuery, barcode);

            Debugger.Log(0, null, $"88888888 通过barcode：{barcode}查询得到uuid为{uuid}");

            Debugger.Log(0, null, $"88888888 开始以uuid：{uuid}查询数据库中相关的SingleSideAIInfo信息");

            findQuery = @"SELECT uuid, aiResult, pointInfoJson FROM " + strTableName + @" WHERE uuid = @uuid";
            string queryResult = Global.m_instance.m_mysql_ops.GetSingleSideAIInfoByUUid(strTableName, findQuery, uuid);

            Debugger.Log(0, null, $"88888888 通过uuid：{uuid}查询得到的结果为：{queryResult}");

            var lines = queryResult.Split('\n');
            var singleSidePassCount = 0;

            singleSideInfo.aiFinalRes = "";

            foreach (var line in lines)
            {
                if (line.Length < 2)
                {
                    continue;
                }

                var fields = line.Split('@');

                if (fields.Count() < 1)
                {
                    continue;
                }

                var camPos = JsonConvert.DeserializeObject<List<CamPos>>(fields[2]);
                singleSideInfo.pointPosInfo.AddRange(camPos);

                if(fields[1]=="FAIL")
                {
                    singleSideInfo.aiFinalRes = "FAIL";
                }
                else if (fields[1] == "PASS")
                {
                    singleSidePassCount++;
                }
            }

            if (singleSidePassCount >= 3)
            {
                singleSideInfo.aiFinalRes = "PASS";
            }

            Debugger.Log(0, null, $"88888888 最后返回的SingleSideAIInfo查询结果为：{JsonConvert.SerializeObject(singleSideInfo)}");

            return singleSideInfo;
        }

        public void InsertMergeSideInfoToAIData(string strTableName, MesAIMergeAIInfo aiInfo)
        {
            string insertQuery = @"INSERT INTO " + strTableName + @" (machine, barcode, aiResult) VALUES (@machine, @barcode, @aiFinalResult);";
            Global.m_instance.m_mysql_ops.InsertMergeInfoInAIDataTable(strTableName, insertQuery, aiInfo);
        }
    }
}
