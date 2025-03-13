using AutoScanFQCTest.DataModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutoScanFQCTest.Utilities
{
    public class CSVHelper
    {
        public static void WriteCSV(string fileNamePath, List<ProductResult> listCSV)
        {
            try
            {
                bool exists = Directory.Exists(fileNamePath);
                if (!exists)
                {
                    Directory.CreateDirectory(fileNamePath);
                }
                fileNamePath += "\\复判结果导出-";
                fileNamePath += DateTime.Now.ToString("yyyyMMddhhmmss");
                fileNamePath += ".csv";
                exists = File.Exists(fileNamePath);
                if (exists)
                {
                    File.Delete(fileNamePath);
                }
                File.Create(fileNamePath).Close();
                FileStream fileStream = new FileStream(fileNamePath, FileMode.Create, FileAccess.Write);
                StreamWriter streamWriter = new StreamWriter(fileStream, Encoding.UTF8);
                if (!exists)
                {
                    streamWriter.WriteLine("物料盘id,产品id,复判结果,复判时间,复判人");
                }
                foreach (ProductResult model in listCSV)
                {
                    string data = $"{model.SetId},{model.BarCode},{model.Result},{model.TestDate},{model.Tester}";
                    streamWriter.WriteLine(data);
                }
                streamWriter.Close();
                fileStream.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void ExportDefectStatisticsRecord(string fileNamePath, List<DefectStatistics> defectStatistics)
        {
            try
            {
                bool exists = Directory.Exists(fileNamePath);
                if (!exists)
                {
                    Directory.CreateDirectory(fileNamePath);
                }

                fileNamePath += "\\缺陷统计数据导出-";
                fileNamePath += DateTime.Now.ToString("yyyyMMddhhmmss");
                fileNamePath += ".csv";

                exists = File.Exists(fileNamePath);
                if (exists)
                {
                    File.Delete(fileNamePath);
                }
                File.Create(fileNamePath).Close();
                FileStream fileStream = new FileStream(fileNamePath, FileMode.Create, FileAccess.Write);
                StreamWriter streamWriter = new StreamWriter(fileStream, Encoding.UTF8);

                if (!exists)
                {
                    streamWriter.WriteLine("机台号,料号,时间,产品二维码,缺陷名称,缺陷数量,二次NG数量,二次OK数量,误判率");
                }
                foreach (var d in defectStatistics)
                {
                    if (d.Barcode == "OKProduct")
                    {
                        continue;
                    }
                    var defectCount = d.RecheckOKCount + d.RecheckNGCount;
                    var yield = (1.0 * d.RecheckOKCount / defectCount * 100).ToString() + "%";
                    var dateTimeParts = d.TrayId.Split('_');
                    var time = "";
                    if(dateTimeParts.Length > 2 )
                    {
                        time = $"{dateTimeParts[0]}/{dateTimeParts[1]}/{dateTimeParts[2]} {dateTimeParts[3]}时{dateTimeParts[4]}分{dateTimeParts[5]}";
                    }
                    string data = $"{d.MachineId},{d.ProductId},{time},{d.Barcode},{d.DefectType},{defectCount},{d.RecheckNGCount},{d.RecheckOKCount},{yield}";
                    streamWriter.WriteLine(data);
                }
                streamWriter.Close();
                fileStream.Close();

                MessageBox.Show($"导出报表成功！文件路径为：{fileNamePath}");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void ExportDefectCorrectionRecord(string fileNamePath, List<DefectCorrection> defectCorrections)
        {
			try
			{
                if (defectCorrections.Count < 1)
                {
                    MessageBox.Show("当前缺陷纠正数据为空，请进行纠正后再导出数据！");
                    return;
                }

				bool exists = Directory.Exists(fileNamePath);
				if (!exists)
				{
					Directory.CreateDirectory(fileNamePath);
				}

				fileNamePath += "\\缺陷纠正数据导出-";
				fileNamePath += DateTime.Now.ToString("yyyyMMddhhmmss");
				fileNamePath += ".csv";

				exists = File.Exists(fileNamePath);
				if (exists)
				{
					File.Delete(fileNamePath);
				}
				File.Create(fileNamePath).Close();
				FileStream fileStream = new FileStream(fileNamePath, FileMode.Create, FileAccess.Write);
				StreamWriter streamWriter = new StreamWriter(fileStream, Encoding.UTF8);

				if (!exists)
				{
					streamWriter.WriteLine("原缺陷类型,纠正缺陷类型,产品SN,料盘SN,料号,图片路径,检出时间,纠正时间,纠正员工号");
				}
				foreach (var d in defectCorrections)
				{
                    if (d.OriginalType == "" || d.OriginalType == null)
                    {
                        continue;
                    }

					string data = $"{d.OriginalType},{d.CorrectedType},{d.ProductSN},{d.TraySN},{d.ProductID},{d.ImagePath},{d.TestTime},{d.CorrectTime},{d.OperatorID}";
					streamWriter.WriteLine(data);
				}
				streamWriter.Close();
				fileStream.Close();

				MessageBox.Show($"导出报表成功！文件路径为：{fileNamePath}");
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

        public static void ReadGlueCheckResultTxtFile(string fileNamePath, ref List<int> readResult)
        {
            try
            {
                bool exists = File.Exists(fileNamePath);

                if (exists)
                {
                    FileStream fileStream = new FileStream(fileNamePath, FileMode.Open, FileAccess.Read);
                    StreamReader streamReader = new StreamReader(fileStream, Encoding.UTF8);

                    List<string> lines = new List<string>();
                        lines.Add(streamReader.ReadToEnd());
                    lines = lines[0].Split('\n').ToList();

                    if (lines.Count > 0)
                    {
                        foreach(var l in lines)
                        {
                            var attribute = l.Split(':')[0];
                            var value = l.Split(':')[1].Trim();
                            switch (attribute)
                            {
                                case "总盘数":
                                    readResult[0] = Convert.ToInt32(value);
                                    break;
                                case "良品数":
                                    readResult[1] = Convert.ToInt32(value);
                                    break;
                                case "总产品数":
                                    readResult[2] = Convert.ToInt32(value);
                                    break;
                            }
                        }
                    }
                }
            }
            catch(Exception e)
            {
                throw e;
            }
        }

        public static void WriteGlueCheckResultCSVFile(string fileNamePath, TrayInfo trayInfo, UVIPanelUploadInfo uviInfo)
        {
            try
            {
                if (trayInfo==null)
                {
                    return;
                }

                bool exists = Directory.Exists(fileNamePath);
                if (!exists)
                {
                    Directory.CreateDirectory(fileNamePath);
                }

                fileNamePath += $@"\{DateTime.Now.ToString("yyyyMMdd")}.csv";
                exists = File.Exists(fileNamePath);
                if (exists)
                {
                    FileStream fileStream = new FileStream(fileNamePath, FileMode.Append, FileAccess.Write);
                    StreamWriter streamWriter = new StreamWriter(fileStream, Encoding.UTF8);

                    foreach (var p in trayInfo.products)
                    {
                        if (p.barcode == "" || p.set_id == "")
                        {
                            continue;
                        }

                        for(int i = 0; i < uviInfo.DetailList.Count; i++)
                        {
                            var d = uviInfo.DetailList[i];

                            if (d.testResult == "FAIL")
                            {
                                string data = $"{p.defects[i].r7},{p.barcode},{p.defects[i].r6},{d.verifyResult}";
                                streamWriter.WriteLine(data);
                            }
                        }
                    }
                    streamWriter.Close();
                    fileStream.Close();
                }
                else
                {
                    File.Create(fileNamePath).Close();
                    FileStream fileStream = new FileStream(fileNamePath, FileMode.Create, FileAccess.Write);
                    StreamWriter streamWriter = new StreamWriter(fileStream, Encoding.UTF8);

                    streamWriter.WriteLine("时间,条码,位置,复判结果");
                    foreach (var p in trayInfo.products)
                    {
                        if (p.barcode == "" || p.set_id == "")
                        {
                            continue;
                        }

                        for (int i = 0; i < uviInfo.DetailList.Count; i++)
                        {
                            var d = uviInfo.DetailList[i];

                            if (d.testResult == "FAIL")
                            {
                                string data = $"{p.defects[i].r7},{p.barcode},{p.defects[i].r6},{d.verifyResult}";
                                streamWriter.WriteLine(data);
                            }
                        }
                    }
                    streamWriter.Close();
                    fileStream.Close();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
