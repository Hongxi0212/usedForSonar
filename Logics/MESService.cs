using AutoScanFQCTest.DataModels;
using AutoScanFQCTest.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Effects;

namespace AutoScanFQCTest.Logics
{
	public class LoginUser
	{
		public string Username { get; set; }
	}

	public class MesResponse
	{
		[JsonPropertyName("Result")]
		public string Result { get; set; }

		[JsonPropertyName("Message")]
		public List<string> Message { get; set; }

		[JsonPropertyName("BarcodeLists")]
		public List<object> BarcodeLists { get; set; }

		[JsonPropertyName("ResPanels")]
		public List<object> ResPanels { get; set; }
	}

	public class MesResponsePanel
	{
		[JsonPropertyName("panel")]
		public string Panel { get; set; }

		[JsonPropertyName("position")]
		public int Position { get; set; }

		[JsonPropertyName("isSuccess")]
		public bool IsSuccess { get; set; }

		[JsonPropertyName("colorCode")]
		public int? ColorCode { get; set; }

		[JsonPropertyName("sample")]
		public string Sample { get; set; }

		[JsonPropertyName("message")]
		public string Message { get; set; }
	}

	public class MesAIProductInfoSZ
	{
		[JsonPropertyName("barCode")]
		public string barCode { get; set; }

		[JsonPropertyName("finalRes")]
		public string finalRes { get; set; }

		[JsonPropertyName("aifinalRes")]
		public string aiFinalRes { get; set; }

		[JsonPropertyName("machinetype")]
		public string machineType { get; set; }

		[JsonPropertyName("tray")]
		public string tray { get; set; }

		[JsonPropertyName("xuewei")]
		public string xuewei { get; set; }

		[JsonPropertyName("testTime")]
		public string testTime { get; set; }

		[JsonPropertyName("testType")]
		public string testType { get; set; }

		[JsonPropertyName("machine")]
		public string machine { get; set; }

		[JsonPropertyName("product")]
		public string product { get; set; }

		[JsonPropertyName("pointPosInfo")]
		public PointPosInfoSZ pointPosInfo { get; set; }
	}

	public class MesAIProductInfoYC
	{
		[JsonPropertyName("barCode")]
		public string barCode { get; set; }

		[JsonPropertyName("finalRes")]
		public string finalRes { get; set; }

		[JsonPropertyName("aifinalRes")]
		public string aiFinalRes { get; set; }

		[JsonPropertyName("machinetype")]
		public string machineType { get; set; }

		[JsonPropertyName("tray")]
		public string tray { get; set; }

		[JsonPropertyName("xuewei")]
		public string xuewei { get; set; }

		[JsonPropertyName("testTime")]
		public string testTime { get; set; }

		[JsonPropertyName("testType")]
		public string testType { get; set; }

		[JsonPropertyName("machine")]
		public string machine { get; set; }

		[JsonPropertyName("product")]
		public string product { get; set; }

		[JsonPropertyName("pointPosInfo")]
		public PointPosInfoYC pointPosInfo { get; set; }
	}

	// MES处理类
	public class MESService
	{
		public MainWindow m_parent;

		// 登录信息
		private string m_strLoginInfo = "";

		// 服务器地址
		private string m_strServerAddress = "http://UCVWMEAP01.mflex.com.cn/wiupload/";

		private string m_strLoginURL = "http://UCVWMEAP01.mflex.com.cn/api/SMT/CheckLogin";

		//string m_strLoginURL2 = "http://10.1.34.39:8080/api/avi/MES_SaveResultByPcs/";
		//string m_strLoginURL2 = "http://10.13.5.254:8080/api/avi/MES_SaveResultByPcs/";
		private string m_strLoginURL3 = "http://mycmessmtaviapi.mflex.com.cn/api/avi/MES_SaveResultByPcs/";

		//string m_strLoginURL3 = "http://ot-messmtaviapi.mflex.com.cn/api/avi/MES_SaveResultByPcs/";

		// AI复判线程
		public Task thread_HandleProducttoAI;

		public Queue<ProductInfo> m_productInfos_waitSubmitToAI_queue = new Queue<ProductInfo>();
		private bool isSubmittingAI = false;

		// 构造函数
		public MESService(MainWindow parent)
		{
			m_parent = parent;

			// 启动AI复判线程
			//thread_HandleProducttoAI = Task.Run(thread_handle_productInfo_toAI_queue);
		}

		// 发送数据到MES
		public bool SendMESForDataResend(string strURL, string strDataToSend, ref string strResponse, int timeoutInMilliseconds, int nGetOrPostFlag = 1)
		{
			try
			{
				HttpWebRequest request = (HttpWebRequest)WebRequest.Create(strURL);

				request.Method = "POST";
				request.ContentType = "application/json";
				request.Timeout = timeoutInMilliseconds;
				request.ReadWriteTimeout = timeoutInMilliseconds;

				if (nGetOrPostFlag == 0)
				{
					request.Method = "GET";
				}

				Debugger.Log(0, null, string.Format("222222 SendMES url: {0}", strURL));

				// 发送请求数据
				using (StreamWriter writer = new StreamWriter(request.GetRequestStream(), Encoding.UTF8))
				{
					writer.Write(strDataToSend);
					writer.Flush();
					writer.Close();

					Debugger.Log(0, null, string.Format("222222 MES 数据发送成功！"));
				}

				// 接收响应数据
				using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
				using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
				{
					Debugger.Log(0, null, string.Format("222222 SendMES 111"));

					string responseJson = reader.ReadToEnd();

					Debugger.Log(0, null, string.Format("222222 接收成功，MES服务器返回数据：{0}", responseJson));

					strResponse = responseJson;
				}

				return true;
			}
			catch (Exception ex)
			{
				strResponse = ex.Message;
				Debugger.Log(0, null, string.Format("222222 MES请求异常，可能是连接失败，异常信息：{0}", ex.Message));
				return false;
			}
		}

		// 发送数据到MES
		public bool SendMES(string strURL, string strDataToSend, ref string strResponse, int nGetOrPostFlag = 1, int timeoutInSeconds = 1, int readWriteTimeoutInSeconds = 1)
		{
			try
			{
				HttpWebRequest request = (HttpWebRequest)WebRequest.Create(strURL);

				request.Method = "POST";
				request.ContentType = "application/json";
				request.Timeout = timeoutInSeconds * 1000;
				request.ReadWriteTimeout = readWriteTimeoutInSeconds * 1000;

				if (nGetOrPostFlag == 0)
				{
					request.Method = "GET";
				}

				Debugger.Log(0, null, string.Format("222222 SendMES url: {0}", strURL));

				// 发送请求数据
				using (StreamWriter writer = new StreamWriter(request.GetRequestStream(), Encoding.UTF8))
				{
					writer.Write(strDataToSend);
					writer.Flush();
					writer.Close();

					Debugger.Log(0, null, string.Format("222222 MES 数据发送成功！"));
				}

				// 接收响应数据
				using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
				using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
				{
					Debugger.Log(0, null, string.Format("222222 SendMES 111"));

					string responseJson = reader.ReadToEnd();

					Debugger.Log(0, null, string.Format("222222 接收成功，MES服务器返回数据：{0}", responseJson));

					strResponse = responseJson;
				}

				return true;
			}
			catch (Exception ex)
			{
				strResponse = ex.Message;
				Debugger.Log(0, null, string.Format("222222 MES请求异常，可能是连接失败，异常信息：{0}", ex.Message));
				return false;
			}
		}

		public bool SendMES(string strURL, string strDataToSend, ref List<string> strResponse, int nGetOrPostFlag = 1)
		{
			try
			{
				HttpWebRequest request = (HttpWebRequest)WebRequest.Create(strURL);

				request.Method = "POST";
				request.ContentType = "application/json";

				if (nGetOrPostFlag == 0)
				{
					request.Method = "GET";
				}

				Debugger.Log(0, null, string.Format("222222 SendMES url: {0}", strURL));

				// 发送请求数据
				using (StreamWriter writer = new StreamWriter(request.GetRequestStream(), Encoding.UTF8))
				{
					writer.Write(strDataToSend);
					writer.Flush();
					writer.Close();

					Debugger.Log(0, null, string.Format("222222 MES 数据发送成功！"));
				}

				// 接收响应数据
				using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
				using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
				{
					Debugger.Log(0, null, string.Format("222222 SendMES 111"));

					List<string> responseJson = new List<string>();
					responseJson.Add(reader.ReadToEnd());

					Debugger.Log(0, null, string.Format("222222 接收成功，MES服务器返回数据：{0}", responseJson));

					strResponse = responseJson;
				}

				return true;
			}
			catch (Exception ex)
			{
				Debugger.Log(0, null, string.Format("222222 MES请求异常，可能是连接失败，异常信息：{0}", ex.Message));
				return false;
			}
		}

		// 获取系统信息
		public void GetSysInfo()
		{
			//AutoScanMain.GetBaseInfo(ref m_sysInfo);
		}

		// 登录
		public void Login(string userName, string password)
		{
			// 登录信息
			string message = "";

			LoginUser user = new LoginUser();
			user.Username = userName;

			string json = JsonConvert.SerializeObject(user);

			// 登录
			SendMES(m_parent.m_global.m_strMesDataUploadUrl, json, ref message, 1, 1, 1);

			//if (true == m_dataProvider.CheckLogin(m_login, ref message))
			//{
			//    m_parent.m_global.m_log_presenter.Log("MES登录成功");

			//    m_strLoginInfo = message;
			//}
			//else
			//{
			//    m_parent.m_global.m_log_presenter.Log("MES登录失败");
			//}
		}

		// 获取缺陷描述列表
		public List<string[]> GetDefectReasonList()
		{
			string msg = "";
			List<string[]> defectReasonList = new List<string[]>();
			List<string> etItemList = new List<string>();

			//AutoScanMain.GetComponentDefectReason(m_login, ref defectReasonList, ref etItemList, ref msg);

			return defectReasonList;
		}

		//// FQC取得元件位置并按照不良次数排序返回
		//public bool GetReferenceDesignatorByOrder(Login login, string strPanelBarcode, ref List<string> orderdDesignator, ref string errMsg)
		//{
		//    return AutoScanMain.getReferenceDesignatorByOrder(login, strPanelBarcode, ref orderdDesignator, ref errMsg);
		//}

		//// 根据FQC条码对应的短料号统计TOP10不良并按顺序返回
		//public bool GetCatalognumberDefectCodeByOrder(Login login, string panel, ref List<List<string>> orderDefect, ref string errMsg)
		//{
		//    return AutoScanMain.getCatalognumberDefectCodeByOrder(login, panel, ref orderDefect, ref errMsg);
		//}

		// 提交OK到MES
		public bool SubmitOKToMES()
		{
			//m_dataProvider.MesFunction(model, m_login);

			return true;
		}

		// 发送数据到MES
		public void SendToMES(string sn, string result)
		{
		}

		// 从MES获取数据
		public string GetFromMES(string sn)
		{
			return "OK";
		}

		// 提交
		public bool SubmitPanelRecheckResults(MESTrayInfo MES_product_info, ref MesResponse response, List<string> list_barcodes_to_ignore, List<DataModels.DefectCount> defectCounts)
		{
			isSubmittingAI = false;
			//if (isSubmittingAI)
			//{
			//    MessageBox.Show("正在向AI复判提交数据！请稍后再人工提交");
			//    return false;
			//}

			// 华通客户
			if (1 == m_parent.m_global.m_nCustomerID)
			{
				//SajetService.Service1SoapClient ws = new SajetService.Service1SoapClient();

				//string s = ws.GetTestProgram("1681234-A0-HSY", "DK3004101RLNWW81V", "FCT1");

				if (m_parent.m_global.m_strProductType == "dock")
				{
					for (int i = 0; i < MES_product_info.m_AVI_tray_data_Dock.products.Count; i++)
					{
						ProductInfo product = MES_product_info.m_AVI_tray_data_Dock.products[i];

						if (list_barcodes_to_ignore.Contains(product.barcode))
						{
							continue;
						}

						// 判断是否无码
						if (product.barcode.IndexOf("nocode", StringComparison.OrdinalIgnoreCase) >= 0)
						{
							continue;
						}

						int nColumns = m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Dock.total_columns;
						int nRows = m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Dock.total_rows;

						int col = product.column - 1;
						int row = product.row - 1;

						int nIndex = row * nColumns + col;

						if (MES_product_info.m_check_flags_by_product[nIndex] == RecheckResult.NEED_CLEAN)
							continue;

						RecheckResult[] recheckResults = MES_product_info.m_list_recheck_flags_by_defect[i];

						PieceSummary summary = new PieceSummary();

						List<DefectDetail> list_defects = new List<DefectDetail>();

						string temp = "";

						summary.PanelId = product.barcode + temp;
						summary.PcsSeq = "1";
						summary.OperatorTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
						summary.VerifyTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
						summary.TestResult = "PASS";
						summary.VerifyResult = "PASS";

						if (false == string.IsNullOrEmpty(product.region_area))
							summary.RegionArea = product.region_area;
						else if (false == string.IsNullOrEmpty(product.r2))
							summary.RegionArea = product.r2;
						else
							summary.RegionArea = "";

						summary.RegionArea = summary.RegionArea.Replace('\\', '_');
						summary.RegionArea = summary.RegionArea.Replace('/', '_');

						if (product.defects != null)
						{
							if (product.defects.Count > 0)
								summary.TestResult = "FAIL";
						}

						for (int n = 0; n < recheckResults.Length; n++)
						{
							DefectInfo defect = product.defects[n];

							DefectDetail defect_detail = new DefectDetail();

							if (product.m_list_remote_imageA_paths_for_channel1 != null && product.m_list_remote_imageA_paths_for_channel1.Count > 0)
							{
								defect_detail.ImagePath = product.m_list_remote_imageA_paths_for_channel1[0];
							}

							switch (recheckResults[n])
							{
								case RecheckResult.OK:
									if (true)
									{
										defect_detail.TestResult = "FAIL";
										defect_detail.VerifyResult = "PASS";

										summary.TestResult = "FAIL";
										summary.VerifyResult = "PASS";
									}
									break;

								case RecheckResult.NG:
									if (true)
									{
										defect_detail.TestResult = "FAIL";
										defect_detail.VerifyResult = "FAIL";

										summary.TestResult = "FAIL";
										summary.VerifyResult = "FAIL";
									}
									break;
							}

							list_defects.Add(defect_detail);
						}
					}
				}

				return true;
			}

			List<PieceSummary> list_summary = new List<PieceSummary>();
			List<UVIDetail> detailList = new List<UVIDetail>();
			List<UVISummary> summaryList = new List<UVISummary>();

			//string strOperatorName = "72007354";
			string strOperatorName = m_parent.m_global.m_strCurrentOperatorID;
			string testdate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

			// 封装主干数据
			try
			{
				if (m_parent.m_global.m_strProductType == "nova")
				{
					for (int i = 0; i < MES_product_info.m_AVI_tray_data_Nova.Products.Count; i++)
					{
						Product product = MES_product_info.m_AVI_tray_data_Nova.Products[i];

						if (list_barcodes_to_ignore.Contains(product.BarCode))
						{
							continue;
						}

						int nColumns = m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Nova.Col;
						int nRows = m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Nova.Row;

						int col = product.PosCol - 1;
						int row = product.PosRow - 1;

						int nIndex = row * nColumns + col;

						if (MES_product_info.m_check_flags_by_product[nIndex] == RecheckResult.NEED_CLEAN)
							continue;

						RecheckResult[] recheckResults = MES_product_info.m_list_recheck_flags_by_defect[i];

						PieceSummary summary = new PieceSummary();

						List<DefectDetail> list_defects = new List<DefectDetail>();

						summary.PanelId = product.BarCode;
						summary.PcsSeq = "1";
						summary.OperatorName = strOperatorName;
						summary.OperatorTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
						summary.VerifyOperatorName = strOperatorName;
						summary.VerifyTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
						summary.TestResult = "FAIL";
						summary.VerifyResult = "PASS";
						if (product.aiResult == "NG" || product.aiResult == "FAIL")
							summary.aiResult = "FAIL";
						else
							summary.aiResult = "PASS";
						summary.aiTime = product.aiTime;

						for (int n = 0; n < recheckResults.Length; n++)
						{
							Defect defect = product.Defects[n];

							DefectDetail defect_detail = new DefectDetail();

							if (recheckResults[n] == RecheckResult.OK)
							{
								if (defectCounts.Exists(x => x.DefectName == defect.Type))
								{
									DefectCount dc = defectCounts.Find(x => x.DefectName == defect.Type);
									dc.Count++;
								}
								else
								{
									defectCounts.Add(new DefectCount { DefectName = defect.Type, Count = 1 });
								}
							}

							defect_detail.PanelId = product.BarCode;
							defect_detail.PcsBarCode = product.BarCode;
							defect_detail.PcsSeq = "1";
							defect_detail.TestType = defect.ChannelNum == 1 ? "AVI-FQCA" : "AVI-FQCB";
							defect_detail.PartSeq = "1";
							defect_detail.PinSeg = "1";
							defect_detail.BubbleValue = "";
							defect_detail.DefectCode = defect.Type;
							defect_detail.OperatorName = strOperatorName;
							defect_detail.VerifyOperatorName = strOperatorName;
							defect_detail.VerifyTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
							defect_detail.Description = defect.Type;
							defect_detail.ImagePath = product.ShareimgPath;
							defect_detail.TestFile = "testFile";
							defect_detail.StrValue1 = "";
							defect_detail.StrValue2 = "";
							defect_detail.StrValue3 = "";
							defect_detail.StrValue4 = "";

							// defect.aiDefectCode是一个List<string>，这里拆开，以逗号分隔，变成一个字符串，赋予defect_detail.aiDefectCode
							if (defect.aiDefectCode != null && defect.aiDefectCode.Count > 0)
							{
								defect_detail.aiDefectCode = string.Join(",", defect.aiDefectCode);
							}

							defect_detail.aiResult = defect.aiResult;
							defect_detail.aiTime = product.aiTime;

							if (defect_detail.aiResult == "NG")
							{
								product.aiResult = "FAIL";
								summary.aiResult = "FAIL";
							}
							else
							{
								product.aiResult = "";
								summary.aiResult = "";
							}

							switch (recheckResults[n])
							{
								case RecheckResult.OK:
									if (true)
									{
										defect_detail.TestResult = "FAIL";
										defect_detail.VerifyResult = "PASS";

										product.RecheckResult = "PASS";
									}
									break;

								case RecheckResult.NG:
								default:
									if (true)
									{
										defect_detail.TestResult = "FAIL";
										defect_detail.VerifyResult = "FAIL";

										summary.TestResult = "FAIL";
										summary.VerifyResult = "FAIL";

										product.RecheckResult = "FAIL";
									}
									break;
							}

							list_defects.Add(defect_detail);
						}

						summary.pcsDetails = list_defects;

						list_summary.Add(summary);
					}

					for (int i = 0; i < MES_product_info.m_AVI_tray_data_Nova.products_with_unrecheckable_defect.Count; i++)
					{
						Product product = MES_product_info.m_AVI_tray_data_Nova.products_with_unrecheckable_defect[i];

						if (list_barcodes_to_ignore.Contains(product.BarCode))
						{
							continue;
						}

						// 判断是否无码
						if (product.BarCode.IndexOf("nocode", StringComparison.OrdinalIgnoreCase) >= 0)
						{
							continue;
						}

						PieceSummary summary = new PieceSummary();

						List<DefectDetail> list_defects = new List<DefectDetail>();

						string temp = "";

						summary.PanelId = product.BarCode + temp;
						summary.PcsSeq = "1";
						summary.OperatorName = strOperatorName;
						summary.OperatorTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
						summary.VerifyOperatorName = strOperatorName;
						summary.VerifyTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
						summary.TestResult = "FAIL";
						summary.VerifyResult = "FAIL";
						if (product.aiResult == "NG" || product.aiResult == "FAIL")
							summary.aiResult = "FAIL";
						else
							summary.aiResult = "PASS";
						summary.aiTime = product.aiTime;

						if (true == string.IsNullOrEmpty(product.r2))
							summary.RegionArea = "";
						else
							summary.RegionArea = product.r2;

						summary.TestResult = "FAIL";
						summary.VerifyResult = "FAIL";

                        for (int n = 0; n < product.Defects.Count(); n++)
                        {
                            Defect defect = product.Defects[n];
                            DefectDetail defect_detail = new DefectDetail();

                            defect_detail.PanelId = product.BarCode;
                            defect_detail.PcsBarCode = product.BarCode;
                            defect_detail.PcsSeq = "1";
                            defect_detail.TestType = defect.ChannelNum == 1 ? "AVI-FQCA" : "AVI-FQCB";
                            defect_detail.PartSeq = "1";
                            defect_detail.PinSeg = "1";
                            defect_detail.BubbleValue = "";
                            defect_detail.DefectCode = defect.Type;
                            defect_detail.OperatorName = strOperatorName;
                            defect_detail.VerifyOperatorName = strOperatorName;
                            defect_detail.VerifyTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                            defect_detail.Description = defect.Type;
                            defect_detail.ImagePath = product.ShareimgPath;
                            defect_detail.TestFile = "testFile";
                            defect_detail.StrValue1 = "";
                            defect_detail.StrValue2 = "";
                            defect_detail.StrValue3 = "";
                            defect_detail.StrValue4 = "";

                            if (defect_detail.aiResult == "NG")
                            {
                                product.aiResult = "FAIL";
                                summary.aiResult = "FAIL";
                            }
                            else
                            {
                                product.aiResult = "";
                                summary.aiResult = "";
                            }

                            defect_detail.TestResult = "FAIL";
                            defect_detail.VerifyResult = "FAIL";

                            list_defects.Add(defect_detail);
                        }
                        summary.pcsDetails = list_defects;

                        list_summary.Add(summary);
                    }

                    /*
						if (true)
						{
                            //Defect defect = new Defect();

                            Defect defect = product.Defects[n];
							DefectDetail defect_detail = new DefectDetail();

							defect_detail.PanelId = product.BarCode + temp;
							defect_detail.PcsBarCode = product.BarCode + temp;
							defect_detail.PcsSeq = "1";
							defect_detail.TestType = defect.ChannelNum == 1 ? "AVI-FQCA" : "AVI-FQCB";
							defect_detail.PartSeq = "1";
							defect_detail.PinSeg = "1";
							defect_detail.BubbleValue = "";
							defect_detail.DefectCode = defect.Type;
							defect_detail.OperatorName = strOperatorName;
							defect_detail.VerifyOperatorName = strOperatorName;
							defect_detail.VerifyTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
							defect_detail.Description = defect.Type;
							defect_detail.ImagePath = "";
							defect_detail.TestFile = "testFile";
							defect_detail.StrValue1 = "";
							defect_detail.StrValue2 = "";
							defect_detail.StrValue3 = "";
							defect_detail.StrValue4 = "";

							if (product.m_list_remote_imageA_paths_for_channel1 != null && product.m_list_remote_imageA_paths_for_channel1.Count > 0)
							{
								defect_detail.ImagePath = product.m_list_remote_imageA_paths_for_channel1[0];
							}

							// defect.aiDefectCode是一个List<string>，这里拆开，以逗号分隔，变成一个字符串，赋予defect_detail.aiDefectCode
							if (defect.aiDefectCode != null && defect.aiDefectCode.Count > 0)
							{
								defect_detail.aiDefectCode = string.Join(",", defect.aiDefectCode);
							}

							defect_detail.aiResult = defect.aiResult;
							defect_detail.aiTime = product.aiTime;

							defect_detail.TestResult = "FAIL";
							defect_detail.VerifyResult = "FAIL";

							for (int k = 0; k < m_parent.m_global.m_uncheckable_defect_types.Count; k++)
							{
								// 如果不可复判缺陷类型是启用的
								if (m_parent.m_global.m_uncheckable_defect_enable_flags[k] == true)
								{
									// 如果产品的缺陷类型与当前不可复判缺陷类型匹配
									if (defect.Type == m_parent.m_global.m_uncheckable_defect_types[k])
									{
										defect_detail.VerifyResult = "FAIL";
									}
								}
							}

							list_defects.Add(defect_detail);
						}
                    */
				}
				else if (m_parent.m_global.m_strProductType == "dock")
				{
					if (m_parent.m_global.m_strProductSubType == "glue_check")
					{
						RecheckResult[] recheckResults = MES_product_info.m_list_recheck_flags_by_defect[0];
						var defectList = MES_product_info.m_AVI_tray_data_Dock.products[0].defects;
						var lastPos = 0;
						for (int i = 0; i < recheckResults.Length; i++)
						{
							UVIDetail uviDetail = new UVIDetail();

							var defect = defectList[i];

							if (lastPos == defect.light_channel)
							{
								if (m_parent.m_global.m_nRecheckModeWithAISystem == 3)
								{
									if (detailList.Count == 0)
									{
										uviDetail.defectCode = defect.type;
									}
									else
									{
										if (!detailList[detailList.Count - 1].defectCode.Contains(defect.type))
										{
											detailList[detailList.Count - 1].defectCode += $"; {defect.type}";
											// lastPos = defect.light_channel;
										}
									}
								}

								lastPos = defect.light_channel;
								continue;
							}
							else
							{
								uviDetail.defectCode = defect.type;
								lastPos = defect.light_channel;
							}

							var result = recheckResults[i] == RecheckResult.OK ? "PASS" : "FAIL";
							uviDetail.panelId = defect.set_id;
							uviDetail.pcsBarCode = "NA";
							uviDetail.testType = MES_product_info.m_AVI_tray_data_Dock.r2;
							uviDetail.pcsSeq = defect.light_channel.ToString();
							uviDetail.partSeq = "1";
							uviDetail.pinSeq = "1";
							//如果是null则为良品，改testResult
							uviDetail.testResult = defect.type == null ? "PASS" : "FAIL";
							uviDetail.operatorName = strOperatorName;
							uviDetail.verifyResult = result;
							uviDetail.verifyOperatorName = strOperatorName;
							uviDetail.verifyTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
							uviDetail.testFile = "NA";
							uviDetail.imagePath = MES_product_info.m_AVI_tray_data_Dock.products[0].sideA_image_path;
							uviDetail.description = "NA";
							uviDetail.strValue1 = "NA";
							uviDetail.strValue2 = "NA";
							uviDetail.strValue3 = "NA";
							uviDetail.strValue4 = "";
							uviDetail.strValue5 = "";
							detailList.Add(uviDetail);

							UVISummary uviSummary = new UVISummary();
							uviSummary.panelId = defect.set_id;
							uviSummary.pcsSeq = defect.light_channel.ToString();
							uviSummary.testResult = defect.type == null ? "PASS" : "FAIL";
							uviSummary.operatorName = strOperatorName;
							uviSummary.operatorTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
							uviSummary.verifyResult = result;
							uviSummary.verifyOperatorName = strOperatorName;
							uviSummary.verifyTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
							summaryList.Add(uviSummary);
						}

						defectList = MES_product_info.m_AVI_tray_data_Dock.products[0].defects_with_NULL_or_empty_type_name;
						var hasAddLightChannels = new List<int>();
						for (int i = 0; i < defectList.Count; i++)
						{
							UVIDetail uviDetail = new UVIDetail();
							var defect = defectList[i];

							if (hasAddLightChannels.Contains(defect.light_channel))
							{
								continue;
							}

							var result = "PASS";
							uviDetail.panelId = defect.set_id;
							uviDetail.pcsBarCode = "NA";
							uviDetail.testType = MES_product_info.m_AVI_tray_data_Dock.r2;
							uviDetail.pcsSeq = defect.light_channel.ToString();
							uviDetail.partSeq = "1";
							uviDetail.pinSeq = "1";
							//如果是null则为良品，改testResult
							uviDetail.testResult = result;
							uviDetail.operatorName = strOperatorName;
							uviDetail.verifyResult = result;
							uviDetail.verifyOperatorName = strOperatorName;
							uviDetail.verifyTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
							uviDetail.defectCode = "";
							uviDetail.testFile = "NA";
							uviDetail.imagePath = MES_product_info.m_AVI_tray_data_Dock.products[0].sideA_image_path;
							uviDetail.description = "NA";
							uviDetail.strValue1 = "NA";
							uviDetail.strValue2 = "NA";
							uviDetail.strValue3 = "NA";
							uviDetail.strValue4 = "";
							uviDetail.strValue5 = "";
							detailList.Add(uviDetail);

							UVISummary uviSummary = new UVISummary();
							uviSummary.panelId = defect.set_id;
							uviSummary.pcsSeq = defect.light_channel.ToString();
							uviSummary.testResult = result;
							uviSummary.operatorName = strOperatorName;
							uviSummary.operatorTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
							uviSummary.verifyResult = result;
							uviSummary.verifyOperatorName = strOperatorName;
							uviSummary.verifyTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
							summaryList.Add(uviSummary);

							hasAddLightChannels.Add(defect.light_channel);
						}
					}
					else
					{
						// 不可复判项因需要点击未从products中去除，为避免重复提交，需剔除不可复判项
						List<int> uncheckableIndex = new List<int>();

						for (int i = 0; i < MES_product_info.m_AVI_tray_data_Dock.products.Count; i++)
						{
							bool bUncheckable = false;
							for (int j = 0; j < MES_product_info.m_AVI_tray_data_Dock.products_with_unrecheckable_defect.Count; j++)
							{
								if (MES_product_info.m_AVI_tray_data_Dock.products[i].barcode == MES_product_info.m_AVI_tray_data_Dock.products_with_unrecheckable_defect[j].barcode)
								{
									bUncheckable = true;
								}
							}

							if (bUncheckable)
							{
								uncheckableIndex.Add(i);
							}
						}
						for (int i = uncheckableIndex.Count - 1; i >= 0; i--)
						{
							MES_product_info.m_AVI_tray_data_Dock.products.RemoveAt(uncheckableIndex[i]);
							MES_product_info.m_list_recheck_flags_by_defect.RemoveAt(uncheckableIndex[i]);
						}

						int pcsSeqIndex = 0;
						// 处理正常复判需提交的数据
						for (int i = 0; i < MES_product_info.m_AVI_tray_data_Dock.products.Count; i++)
						{
							ProductInfo product = MES_product_info.m_AVI_tray_data_Dock.products[i];
							pcsSeqIndex++;

							if (list_barcodes_to_ignore.Contains(product.barcode))
							{
								continue;
							}

							// 判断是否无码
							if (product.barcode.IndexOf("nocode", StringComparison.OrdinalIgnoreCase) >= 0)
							{
								continue;
							}

							if (i >= MES_product_info.m_list_recheck_flags_by_defect.Count)
								continue;

							int nColumns = m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Dock.total_columns;
							int nRows = m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Dock.total_rows;

							int col = product.column - 1;
							int row = product.row - 1;

							int nIndex = row * nColumns + col;

							// 判断nIndex是否小于0
							if (nIndex < 0)
							{
								// 记录日志
								m_parent.m_global.m_log_presenter.Log("SubmitPanelRecheckResults() 行列号错误，nIndex < 0");

								continue;
							}

							if (MES_product_info.m_check_flags_by_product[nIndex] == RecheckResult.NEED_CLEAN)
								continue;

							RecheckResult[] recheckResults = MES_product_info.m_list_recheck_flags_by_defect[i];

							PieceSummary summary = new PieceSummary();

							List<DefectDetail> list_defects = new List<DefectDetail>();

							string temp = "";

							summary.PanelId = product.barcode + temp;
							summary.PcsSeq = pcsSeqIndex.ToString();
							summary.OperatorName = strOperatorName;
							summary.OperatorTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
							summary.VerifyOperatorName = strOperatorName;
							summary.VerifyTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
							summary.TestResult = "FAIL";
							summary.VerifyResult = "PASS";

							if (false == string.IsNullOrEmpty(product.region_area))
								summary.RegionArea = product.region_area;
							else if (false == string.IsNullOrEmpty(product.r2))
								summary.RegionArea = product.r2;
							else
								summary.RegionArea = "";

							summary.RegionArea = summary.RegionArea.Replace('\\', '_');
							summary.RegionArea = summary.RegionArea.Replace('/', '_');

							if (product.defects != null)
							{
								if (product.defects.Count > 0)
									summary.TestResult = "FAIL";
								else
									summary.TestResult = "PASS";
							}
							else
								summary.TestResult = "PASS";

							Dictionary<string, string> mesImagePathsandNames = getProductImagePathsandNames(product);
							var allIamges = getProductImagePathsandNames(product.all_image_paths);
							var allIamgesCount = allIamges.Count();

							int partSeqIndex = 0;
							for (int n = 0; n < recheckResults.Length; n++)
							{
								partSeqIndex++;
								DefectInfo defect = product.defects[n];

								DefectDetail defect_detail = new DefectDetail();

								if (recheckResults[n] == RecheckResult.OK)
								{
									if (defectCounts.Exists(x => x.DefectName == defect.type))
									{
										DefectCount dc = defectCounts.Find(x => x.DefectName == defect.type);
										dc.Count++;
									}
									else
									{
										defectCounts.Add(new DefectCount { DefectName = defect.type, Count = 1 });
									}
								}

								defect_detail.PanelId = product.barcode + temp;
								defect_detail.PcsBarCode = product.barcode + temp;
								defect_detail.PcsSeq = summary.PcsSeq;
								defect_detail.TestType = "AVI-FQC";                        // 可以根据defect.side的值来判断是A面/B面/C面，然后赋值给TestType

								if (defect.side == 0)
								{
									defect_detail.TestType = "AVI-FQCA";
								}
								else if (defect.side == 1)
								{
									defect_detail.TestType = "AVI-FQCB";
								}

								defect_detail.PartSeq = partSeqIndex.ToString();
								defect_detail.PinSeg = "1";
								defect_detail.BubbleValue = "";
								defect_detail.DefectCode = defect.type;
								defect_detail.OperatorName = strOperatorName;
								defect_detail.VerifyOperatorName = strOperatorName;
								defect_detail.VerifyTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
								defect_detail.Description = defect.type;
								defect_detail.TestFile = "testFile";
								defect_detail.TestResult = "FAIL";
								defect_detail.StrValue1 = "";
								defect_detail.StrValue2 = "";
								defect_detail.StrValue3 = "";
								defect_detail.StrValue4 = "";
								defect_detail.ImagePath = defect.image_path;

								if (allIamges.ContainsKey(defect.image_path))
								{
									allIamges.Remove(defect.image_path);
								}

								if (product.r3 == "FAIL")
								{
									defect_detail.TestResult = "FAIL";
								}
								else if (product.r3 == "PASS")
								{
									defect_detail.TestResult = "PASS";
								}

								switch (recheckResults[n])
								{
									case RecheckResult.OK:
										defect_detail.VerifyResult = "PASS";

										//                        if (n < allIamgesCount && mesImagePathsandNames.Count > 0)
										//                        {
										//                            var keyOK = mesImagePathsandNames.Where(p => p.Value == "OK");
										//                            if (keyOK.Count() > 0)
										//                            {
										//                                defect_detail.ImagePath = keyOK.First().Key;
										//mesImagePathsandNames.Remove(keyOK.First().Key);
										//                            }
										//                            else
										//                            {
										//                                defect_detail.ImagePath = mesImagePathsandNames.First().Key;
										//mesImagePathsandNames.Remove(mesImagePathsandNames.First().Key);
										//                            }
										//                        }

										break;

									case RecheckResult.NG:
										defect_detail.VerifyResult = "FAIL";
										summary.VerifyResult = "FAIL";

										//                        if (n < allIamgesCount && mesImagePathsandNames.Count > 0)
										//                        {
										//                            var keyNG = mesImagePathsandNames.Where(p => p.Value == "NG");
										//                            if (keyNG.Count() > 0)
										//                            {
										//                                defect_detail.ImagePath = keyNG.First().Key;
										//mesImagePathsandNames.Remove(keyNG.First().Key);
										//                            }
										//                            else
										//                            {
										//                                defect_detail.ImagePath = mesImagePathsandNames.First().Key;
										//mesImagePathsandNames.Remove(mesImagePathsandNames.First().Key);
										//                            }
										//                        }

										summary.TestResult = "FAIL";
										summary.VerifyResult = "FAIL";
										break;
								}

								//纯AI复判模式要求人工复判结果为空
								if (m_parent.m_global.m_nRecheckModeWithAISystem == 3)
								{
									defect_detail.VerifyResult = "";
									summary.VerifyResult = "";
								}

								//if (product.m_list_remote_imageA_paths_for_channel1 != null && product.m_list_remote_imageA_paths_for_channel1.Count > 0)
								//{
								//    defect_detail.ImagePath = product.m_list_remote_imageA_paths_for_channel1[0];
								//}
								//else if (product.m_list_remote_imageB_paths_for_channel1 != null && product.m_list_remote_imageB_paths_for_channel1.Count > 0)
								//{
								//    defect_detail.ImagePath = product.m_list_remote_imageB_paths_for_channel1[0];
								//}
								//else if (product.m_list_remote_imageC_paths_for_channel1 != null && product.m_list_remote_imageB_paths_for_channel1.Count > 0)
								//{
								//    defect_detail.ImagePath = product.m_list_remote_imageC_paths_for_channel1[0];
								//}

								list_defects.Add(defect_detail);
							}

							// 上传MES图片路径，需要所有图片全部上传，在缺陷信息中已上传NG图片，在此通过捏造缺陷上传剩下的OK图片
							foreach (var kvp in allIamges)
							{
								if (kvp.Value == "OK")
								{
									partSeqIndex++;
									DefectInfo defect = new DefectInfo();
									DefectDetail defect_detail = new DefectDetail();

									defect_detail.PanelId = product.barcode + temp;
									defect_detail.PcsBarCode = product.barcode + temp;
									defect_detail.PcsSeq = summary.PcsSeq;
									defect_detail.PartSeq = partSeqIndex.ToString();
									defect_detail.PinSeg = "1";
									defect_detail.BubbleValue = "";
									defect_detail.DefectCode = "PASS";
									defect_detail.OperatorName = strOperatorName;
									defect_detail.VerifyOperatorName = strOperatorName;
									defect_detail.VerifyTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
									defect_detail.Description = "";
									defect_detail.TestFile = "testFile";
									defect_detail.StrValue1 = "";
									defect_detail.StrValue2 = "";
									defect_detail.StrValue3 = "";
									defect_detail.StrValue4 = "";
									defect_detail.ImagePath = kvp.Key;
									defect_detail.TestResult = "PASS";
									defect_detail.VerifyResult = "PASS";

									defect_detail.TestType = "AVI-FQC";
									if (kvp.Key.Contains("imageA"))
									{
										defect_detail.TestType = "AVI-FQCA";
									}
									else if (kvp.Key.Contains("imageB"))
									{
										defect_detail.TestType = "AVI-FQCB";
									}

									//纯AI复判模式要求人工复判结果为空
									if (m_parent.m_global.m_nRecheckModeWithAISystem == 3)
									{
										defect_detail.VerifyResult = "";
										summary.VerifyResult = "";
									}

									list_defects.Add(defect_detail);
								}
							}

							// 小程序查图总共需要四张图片，如果缺陷数量小于四个的话，图片目录字典会有剩余键值，再次捏造缺陷提交
							//if (recheckResults.Length < allIamgesCount && mesImagePathsandNames.Count > 0)
							//{
							//    foreach (var kv in mesImagePathsandNames)
							//    {
							//        DefectInfo defect = new DefectInfo();
							//        DefectDetail defect_detail = new DefectDetail();

							//        defect_detail.PanelId = product.barcode + temp;
							//        defect_detail.PcsBarCode = product.barcode + temp;
							//        defect_detail.PcsSeq = "1";
							//        defect_detail.TestType = "AVI-FQCA";
							//        defect_detail.PartSeq = "1";
							//        defect_detail.PinSeg = "1";
							//        defect_detail.BubbleValue = "";
							//        defect_detail.DefectCode = defect.type;
							//        defect_detail.OperatorName = strOperatorName;
							//        defect_detail.VerifyOperatorName = strOperatorName;
							//        defect_detail.VerifyTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
							//        defect_detail.Description = defect.type;
							//        defect_detail.TestFile = "testFile";
							//        defect_detail.StrValue1 = "";
							//        defect_detail.StrValue2 = "";
							//        defect_detail.StrValue3 = "";
							//        defect_detail.StrValue4 = "";
							//        defect_detail.ImagePath = kv.Key;
							//        defect_detail.TestResult = "FAIL";

							//        if (product.r3 == "FAIL")
							//        {
							//            defect_detail.TestResult = "FAIL";
							//        }
							//        else if (product.r3 == "PASS")
							//        {
							//            defect_detail.TestResult = "PASS";
							//        }

							//        // 上传小程序查图，将捏造缺陷的复判结果与视觉上传的图片结果同步
							//        switch (kv.Value)
							//        {
							//            case "OK":
							//                defect_detail.TestResult = "PASS";
							//                break;
							//            case "NG":
							//                defect_detail.TestResult = "FAIL";
							//                break;
							//        }

							//        //纯AI复判模式要求人工复判结果为空
							//        if (m_parent.m_global.m_nRecheckModeWithAISystem == 3)
							//        {
							//            defect_detail.VerifyResult = "";
							//            summary.VerifyResult = "";
							//        }

							//        list_defects.Add(defect_detail);
							//    }
							//}

							summary.pcsDetails = list_defects;

							list_summary.Add(summary);
						}

						// 处理OK产品的提交数据
						for (int i = 0; i < MES_product_info.m_AVI_tray_data_Dock.OK_products.Count; i++)
						{
							ProductInfo product = MES_product_info.m_AVI_tray_data_Dock.OK_products[i];

							if (list_barcodes_to_ignore.Contains(product.barcode))
							{
								continue;
							}

							// 判断是否无码
							if (product.barcode.IndexOf("nocode", StringComparison.OrdinalIgnoreCase) >= 0)
							{
								continue;
							}

							PieceSummary summary = new PieceSummary();

							List<DefectDetail> list_defects = new List<DefectDetail>();

							string temp = "";

							summary.PanelId = product.barcode + temp;
							summary.PcsSeq = "1";
							summary.OperatorName = strOperatorName;
							summary.OperatorTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
							summary.VerifyOperatorName = strOperatorName;
							summary.VerifyTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
							summary.TestResult = "PASS";
							summary.VerifyResult = "PASS";

							if (false == string.IsNullOrEmpty(product.r2))
								summary.RegionArea = product.r2;
							else if (false == string.IsNullOrEmpty(product.region_area))
								summary.RegionArea = product.region_area;
							else
								summary.RegionArea = "";

							summary.RegionArea = summary.RegionArea.Replace('\\', '_');
							summary.RegionArea = summary.RegionArea.Replace('/', '_');

							Dictionary<string, string> mesImagePathsandNames = getProductImagePathsandNames(product);
							var xiaoChengXuTuPianLuJingHeJieGuo = getProductImagePathsandNames(product.all_image_paths);

							foreach (var kv in mesImagePathsandNames)
							{
								DefectInfo defect = new DefectInfo();
								DefectDetail defect_detail = new DefectDetail();

								defect_detail.PanelId = product.barcode + temp;
								defect_detail.PcsBarCode = product.barcode + temp;
								defect_detail.PcsSeq = "1";
								defect_detail.TestType = "AVI-FQCA";
								defect_detail.PartSeq = "1";
								defect_detail.PinSeg = "1";
								defect_detail.BubbleValue = "";
								defect_detail.DefectCode = defect.type;
								defect_detail.OperatorName = strOperatorName;
								defect_detail.VerifyOperatorName = strOperatorName;
								defect_detail.VerifyTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
								defect_detail.Description = defect.type;
								defect_detail.TestFile = "testFile";
								defect_detail.StrValue1 = "";
								defect_detail.StrValue2 = "";
								defect_detail.StrValue3 = "";
								defect_detail.StrValue4 = "";
								defect_detail.ImagePath = kv.Key;

								defect_detail.TestResult = "PASS";
								defect_detail.VerifyResult = "PASS";

								list_defects.Add(defect_detail);
							}

							// 如果整盘料全是OK，则无pcsDetails无数据会上传失败，添加空缺陷以上传
							if (list_defects.Count == 0)
							{
								list_defects.Add(new DefectDetail
								{
									PanelId = product.barcode + temp,
									PcsBarCode = product.barcode + temp,
									PcsSeq = "1",
									TestType = "AVI-FQCA",
									PartSeq = "1",
									PinSeg = "1",
									BubbleValue = "",
									DefectCode = "",
									OperatorName = strOperatorName,
									VerifyOperatorName = strOperatorName,
									VerifyTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
									Description = "",
									TestFile = "testFile",
									StrValue1 = "",
									StrValue2 = "",
									StrValue3 = "",
									StrValue4 = "",
									ImagePath = "",

									TestResult = "PASS",
									VerifyResult = "PASS"
								});
							}

							summary.pcsDetails = list_defects;

							list_summary.Add(summary);
						}

						// 处理不可复判项产品的提交数据
						for (int i = 0; i < MES_product_info.m_AVI_tray_data_Dock.products_with_unrecheckable_defect.Count; i++)
						{
							ProductInfo product = MES_product_info.m_AVI_tray_data_Dock.products_with_unrecheckable_defect[i];

							if (list_barcodes_to_ignore.Contains(product.barcode))
							{
								continue;
							}

							// 判断是否无码
							if (product.barcode.IndexOf("nocode", StringComparison.OrdinalIgnoreCase) >= 0)
							{
								continue;
							}

							PieceSummary summary = new PieceSummary();
							List<DefectDetail> list_defects = new List<DefectDetail>();

							string temp = "";

							summary.PanelId = product.barcode + temp;
							summary.PcsSeq = "1";
							summary.OperatorName = strOperatorName;
							summary.OperatorTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
							summary.VerifyOperatorName = strOperatorName;
							summary.VerifyTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
							summary.TestResult = "FAIL";
							summary.VerifyResult = "FAIL";

							if (false == string.IsNullOrEmpty(product.r2))
								summary.RegionArea = product.r2;
							else if (false == string.IsNullOrEmpty(product.region_area))
								summary.RegionArea = product.region_area;
							else
								summary.RegionArea = "";

							summary.RegionArea = summary.RegionArea.Replace('\\', '_');
							summary.RegionArea = summary.RegionArea.Replace('/', '_');

							Dictionary<string, string> mesImagePathsandNames = getProductImagePathsandNames(product);
							var xiaoChengXuTuPianLuJingHeJieGuo = getProductImagePathsandNames(product.all_image_paths);

							for (int j = 0; j < mesImagePathsandNames.Count; j++)
							{
								DefectInfo defect = new DefectInfo();
								if (j < product.defects.Count)
								{
									defect = product.defects[j];
								}
								DefectDetail defect_detail = new DefectDetail();

								defect_detail.PanelId = product.barcode + temp;
								defect_detail.PcsBarCode = product.barcode + temp;
								defect_detail.PcsSeq = "1";
								defect_detail.TestType = "AVI-FQCA";
								defect_detail.PartSeq = "1";
								defect_detail.PinSeg = "1";
								defect_detail.BubbleValue = "";
								defect_detail.DefectCode = defect.type;
								defect_detail.OperatorName = strOperatorName;
								defect_detail.VerifyOperatorName = strOperatorName;
								defect_detail.VerifyTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
								defect_detail.Description = defect.type;
								defect_detail.TestFile = "testFile";
								defect_detail.StrValue1 = "";
								defect_detail.StrValue2 = "";
								defect_detail.StrValue3 = "";
								defect_detail.StrValue4 = "";
								defect_detail.ImagePath = mesImagePathsandNames.ElementAt(j).Key;

								defect_detail.TestResult = "FAIL";
								defect_detail.VerifyResult = "FAIL";

								//for (int k = 0; k < m_parent.m_global.m_uncheckable_defect_types.Count; k++)
								//{
								//    // 如果不可复判缺陷类型是启用的
								//    if (m_parent.m_global.m_uncheckable_defect_enable_flags[k] == true)
								//    {
								//        // 如果产品的缺陷类型与当前不可复判缺陷类型匹配
								//        if (defect.type == m_parent.m_global.m_uncheckable_defect_types[k])
								//        {
								//            defect_detail.VerifyResult = "FAIL";
								//        }
								//    }
								//}

								list_defects.Add(defect_detail);
							}

							if (list_defects.Count == 0)
							{
								list_defects.Add(new DefectDetail
								{
									PanelId = product.barcode + temp,
									PcsBarCode = product.barcode + temp,
									PcsSeq = "1",
									TestType = "AVI-FQCA",
									PartSeq = "1",
									PinSeg = "1",
									BubbleValue = "",
									DefectCode = "",
									OperatorName = strOperatorName,
									VerifyOperatorName = strOperatorName,
									VerifyTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
									Description = "",
									TestFile = "testFile",
									StrValue1 = "",
									StrValue2 = "",
									StrValue3 = "",
									StrValue4 = "",
									ImagePath = "",

									TestResult = "FAIL",
									VerifyResult = "FAIL"
								});
							}

							summary.pcsDetails = list_defects;

							list_summary.Add(summary);
						}

						// 处理AI提交
						if (m_parent.m_global.m_strSiteCity == "苏州")
						{
							#region 旧版AI提交处理

							//var barcodewithMesAIProductInfo = m_parent.m_global.m_current_MES_tray_info.m_dictionary_barcode_with_mesAIProductInfoSZ;
							//foreach (var s in list_summary)
							//{
							//    if (s.pcsDetails.Count < 1)
							//    {
							//        continue;
							//    }

							//    if (barcodewithMesAIProductInfo.ContainsKey(s.pcsDetails[0].PcsBarCode))
							//    {
							//        var aiInfo = barcodewithMesAIProductInfo[s.pcsDetails[0].PcsBarCode];
							//        var aiDefects = new List<AIDefect>();

							//        if (aiInfo.pointPosInfo.side_A != null)
							//        {
							//            aiInfo.pointPosInfo.side_A.ForEach(cp => aiDefects.AddRange(cp.defects));
							//        }
							//        if (aiInfo.pointPosInfo.side_B != null)
							//        {
							//            aiInfo.pointPosInfo.side_B.ForEach(cp => aiDefects.AddRange(cp.defects));
							//        }
							//        if (aiInfo.pointPosInfo.side_C != null)
							//        {
							//            aiInfo.pointPosInfo.side_C.ForEach(cp => aiDefects.AddRange(cp.defects));
							//        }

							//        for (int i = 0; i < s.pcsDetails.Count; i++)
							//        {
							//            foreach (var d in aiDefects)
							//            {
							//                if (s.pcsDetails[i].DefectCode == d.defectType)
							//                {
							//                    if (d.aiResult == "PASS" || d.aiResult == "OK")
							//                    {
							//                        s.pcsDetails[i].aiResult = "PASS";
							//                    }
							//                    if (d.aiResult == "FAIL" || d.aiResult == "NG")
							//                    {
							//                        s.pcsDetails[i].aiResult = "FAIL";
							//                    }

							//                    if (d.aiDefectCode.Count > 0)
							//                    {
							//                        s.pcsDetails[i].aiDefectCode = d.aiDefectCode[0];
							//                        s.pcsDetails[i].aiTime = aiInfo.testTime;
							//                    }

							//                    // 采用过该缺陷的AI结果后为避免重复采用对类型赋其他值
							//                    d.defectType = "dumb";

							//                    break;
							//                }
							//            }
							//        }

							//        if (aiInfo.aiFinalRes == "PASS" || aiInfo.aiFinalRes == "OK")
							//        {
							//            s.aiResult = "PASS";
							//        }
							//        if (aiInfo.aiFinalRes == "FAIL" || aiInfo.aiFinalRes == "NG")
							//        {
							//            s.aiResult = "FAIL";
							//        }

							//        s.aiTime = aiInfo.testTime;

							//        barcodewithMesAIProductInfo.Remove(s.pcsDetails[0].PcsBarCode);
							//    }

							//    if (m_parent.m_global.m_bUseAIRecheckResult || m_parent.m_global.m_nRecheckModeWithAISystem == 3)
							//    {
							//        s.VerifyResult = "";
							//        s.VerifyTime = "";
							//    }
							//}

							#endregion 旧版AI提交处理

							if (m_parent.m_global.m_nRecheckModeWithAISystem == 3)
							{
								foreach (var s in list_summary)
								{
									var barcode = s.PanelId;
									var strTableName = "transfer_aiInfo_" + MES_product_info.m_AVI_tray_data_Dock.machine_id;
									strTableName = strTableName.Replace("-", "_");

									var strQuery = "SELECT * FROM " + strTableName + " WHERE barcode = '" + barcode + "';";
									var strQueryResult = "";
									var ts = new TimeSpan();
									m_parent.m_global.m_mysql_ops.QueryTableData(strTableName, strQuery, ref strQueryResult, ref ts);

									var aiResult = "";
									m_parent.m_global.m_database_service.ProcessTransferAIInfoQueryResult(strQueryResult, ref aiResult);
									s.aiResult = "";
									s.aiResult = aiResult;
									s.aiTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
								}
							}
						}
						if (m_parent.m_global.m_strSiteCity == "盐城")
						{
							var barcodewithMesAIProductInfo = m_parent.m_global.m_current_MES_tray_info.m_dictionary_barcode_with_mesAIProductInfoYC;
							foreach (var s in list_summary)
							{
								if (s.pcsDetails.Count < 1)
								{
									continue;
								}

								if (barcodewithMesAIProductInfo.ContainsKey(s.pcsDetails[0].PcsBarCode))
								{
									var aiInfo = barcodewithMesAIProductInfo[s.pcsDetails[0].PcsBarCode];
									var aiDefects = new List<AIDefect>();

									if (aiInfo.pointPosInfo.side_A != null)
									{
										aiDefects.AddRange(aiInfo.pointPosInfo.side_A.cam0Pos0.defects);
									}
									if (aiInfo.pointPosInfo.side_B != null)
									{
										aiDefects.AddRange(aiInfo.pointPosInfo.side_B.cam0Pos0.defects);
									}

									for (int i = 0; i < s.pcsDetails.Count; i++)
									{
										foreach (var d in aiDefects)
										{
											if (s.pcsDetails[i].DefectCode == d.defectType)
											{
												if (d.aiResult == "PASS" || d.aiResult == "OK")
												{
													s.pcsDetails[i].aiResult = "PASS";
												}
												if (d.aiResult == "FAIL" || d.aiResult == "NG")
												{
													s.pcsDetails[i].aiResult = "FAIL";
												}

												if (d.aiDefectCode != null && d.aiDefectCode.Count > 0)
												{
													s.pcsDetails[i].aiDefectCode = d.aiDefectCode[0];
													s.pcsDetails[i].aiTime = aiInfo.testTime;
												}

												// 采用过该缺陷的AI结果后为避免重复采用对类型赋其他值
												d.defectType = "dumb";

												break;
											}
										}
									}

									foreach (var d in s.pcsDetails)
									{
										if (!String.IsNullOrEmpty(d.DefectCode) && String.IsNullOrEmpty(d.aiResult))
										{
											d.aiResult = "";
										}
									}

									if (aiInfo.aiFinalRes == "PASS" || aiInfo.aiFinalRes == "OK")
									{
										s.aiResult = "PASS";
									}
									else if (aiInfo.aiFinalRes == "FAIL" || aiInfo.aiFinalRes == "NG")
									{
										s.aiResult = "FAIL";
									}
									else
									{
										s.aiResult = "";
									}

									s.aiTime = aiInfo.testTime;

									barcodewithMesAIProductInfo.Remove(s.pcsDetails[0].PcsBarCode);
								}

								// 不可复叛项所有ai强制为空
								foreach (var d in s.pcsDetails)
								{
									for (int i = 0; i < m_parent.m_global.m_uncheckable_defect_types.Count(); i++)
									{
										if (m_parent.m_global.m_uncheckable_defect_enable_flags[i] &&
											d.DefectCode == m_parent.m_global.m_uncheckable_defect_types[i])
										{
											d.aiResult = "";
											d.aiTime = "";
											d.aiDefectCode = "";
											d.VerifyResult = "";
											d.VerifyTime = "";

											s.aiResult = "";
											s.aiTime = "";
										}
									}
								}

								if (m_parent.m_global.m_bUseAIRecheckResult || m_parent.m_global.m_nRecheckModeWithAISystem == 3)
								{
									s.VerifyResult = "";
									s.VerifyTime = "";
								}
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				if (m_parent.m_global.m_nRecheckModeWithAISystem == 3)
				{
					m_parent.m_global.m_log_presenter.LogError("SubmitPanelRecheckResults() 异常：" + ex.Message);

					return false;
				}
				else
					MessageBox.Show(m_parent, ex.Message, "提示");
			}
			string json = "";

			// 封装头部信息
			MesPanelUploadInfo aviUploadInfo = new MesPanelUploadInfo();
			UVIPanelUploadInfo uviUploadInfo = new UVIPanelUploadInfo();

			if (m_parent.m_global.m_strProductSubType == "glue_check")
			{
				if (MES_product_info.m_AVI_tray_data_Dock.products.Count > 0)
					uviUploadInfo.Panel = MES_product_info.m_AVI_tray_data_Dock.products[0].set_id;
				else
					uviUploadInfo.Panel = MES_product_info.m_AVI_tray_data_Dock.OK_products[0].set_id;
				uviUploadInfo.Resource = MES_product_info.m_AVI_tray_data_Dock.machine_id;
				uviUploadInfo.Product = MES_product_info.m_AVI_tray_data_Dock.r7.Split('_')[0];
				uviUploadInfo.Machine = MES_product_info.m_AVI_tray_data_Dock.machine_id;

				uviUploadInfo.WorkArea = "SMT-FEB";
				uviUploadInfo.OperatorName = strOperatorName;
				uviUploadInfo.TrackType = "NULL";
				uviUploadInfo.TestType = MES_product_info.m_AVI_tray_data_Dock.r2;
				uviUploadInfo.Mac = MES_product_info.m_AVI_tray_data_Dock.r4;
				uviUploadInfo.Site = MES_product_info.m_AVI_tray_data_Dock.site;
				uviUploadInfo.IPAaddress = MES_product_info.m_AVI_tray_data_Dock.r5;
				uviUploadInfo.ProgramName = MES_product_info.m_AVI_tray_data_Dock.r7;
				uviUploadInfo.TestTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
				uviUploadInfo.OperatorType = strOperatorName;
				uviUploadInfo.isRepair = "1";
				uviUploadInfo.TestMode = MES_product_info.m_AVI_tray_data_Dock.r3;
				uviUploadInfo.hasTrackFlag = "0";

				uviUploadInfo.SummaryList = summaryList;
				uviUploadInfo.DetailList = detailList;

				json = JsonConvert.SerializeObject(uviUploadInfo);
			}
			else
			{
				if (m_parent.m_global.m_strProductType == "nova")
				{
					var trayInfo = MES_product_info.m_AVI_tray_data_Nova;
					List<string> barcodes = new List<string>();

					// 收集所有条形码
					for (int i = 0; i < trayInfo.Products.Count; i++)
					{
						barcodes.Add(trayInfo.Products[i].BarCode);
					}
					for (int i = 0; i < trayInfo.products_with_unrecheckable_defect.Count; i++)
					{
						barcodes.Add(trayInfo.products_with_unrecheckable_defect[i].BarCode);
					}

					aviUploadInfo.Panel = String.Join(",", barcodes);
					aviUploadInfo.Resource = MES_product_info.m_AVI_tray_data_Nova.Mid;
					aviUploadInfo.Machine = MES_product_info.m_AVI_tray_data_Nova.Mid;
					aviUploadInfo.Uuid = MES_product_info.m_AVI_tray_data_Nova.Uuid;
					aviUploadInfo.Machine = MES_product_info.m_AVI_tray_data_Nova.Mid;
					aviUploadInfo.Product = MES_product_info.m_AVI_tray_data_Nova.BatchId;
				}
				else if (m_parent.m_global.m_strProductType == "dock")
				{
					var trayInfo = MES_product_info.m_AVI_tray_data_Dock;

					List<string> barcodes = new List<string>();

					// 收集所有条形码
					for (int i = 0; i < trayInfo.products.Count; i++)
					{
						barcodes.Add(trayInfo.products[i].barcode);
					}
					for (int i = 0; i < trayInfo.OK_products.Count; i++)
					{
						barcodes.Add(trayInfo.OK_products[i].barcode);
					}
					for (int i = 0; i < trayInfo.products_with_unrecheckable_defect.Count; i++)
					{
						barcodes.Add(trayInfo.products_with_unrecheckable_defect[i].barcode);
					}
					aviUploadInfo.Panel = String.Join(",", barcodes);

					aviUploadInfo.Resource = MES_product_info.m_AVI_tray_data_Dock.machine_id;
					aviUploadInfo.Machine = MES_product_info.m_AVI_tray_data_Dock.machine_id;
					aviUploadInfo.Uuid = MES_product_info.m_AVI_tray_data_Dock.set_id;

					// 如果MES_product_info.m_AVI_tray_data_Dock.set_id中包含"SMT_"，则截取从其开始的字符串，赋值给aviUploadInfo.Machine
					if (MES_product_info.m_AVI_tray_data_Dock.set_id.Contains("SMT_"))
					{
						string strMachine = MES_product_info.m_AVI_tray_data_Dock.set_id.Substring(MES_product_info.m_AVI_tray_data_Dock.set_id.IndexOf("SMT_"));
						aviUploadInfo.Machine = strMachine;
						aviUploadInfo.Resource = strMachine;

						m_parent.m_global.m_log_presenter.LogError(string.Format("set_id:{0}, Machine:{1}", MES_product_info.m_AVI_tray_data_Dock.set_id, aviUploadInfo.Machine));
					}
				}

				aviUploadInfo.OperatorName = strOperatorName;
				aviUploadInfo.OperatorType = "operatorType";
				aviUploadInfo.TrackType = "0";
				aviUploadInfo.WorkArea = "SMT-FAVI";
				aviUploadInfo.Mac = "F8-16-54-CD-AF-82";
				aviUploadInfo.VersionFlag = "0";
				aviUploadInfo.CheckDetail = "1";
				aviUploadInfo.CheckPcsDataForAVI = true;
				aviUploadInfo.TestTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
				aviUploadInfo.JdeProductName = m_parent.m_global.m_strProductName;
				aviUploadInfo.Site = m_parent.m_global.m_strSiteCity == "苏州" ? "SZSMT" : "YCSMT";
				aviUploadInfo.TestMode = "PANEL";
				aviUploadInfo.TestType = "AVI-FQC";
				aviUploadInfo.isRepair = "1";
				aviUploadInfo.ProgramName = m_parent.m_global.m_strProgramName;
				aviUploadInfo.Product = m_parent.m_global.m_strProductName;
				aviUploadInfo.TrackType = "0";
				aviUploadInfo.pcsSummarys = list_summary;

				if (m_parent.m_global.m_bUseProductTypeFromAVIData)
				{
					aviUploadInfo.ProgramName = MES_product_info.m_AVI_tray_data_Dock.r4;
					aviUploadInfo.Product = MES_product_info.m_AVI_tray_data_Dock.r4;
				}

				json = JsonConvert.SerializeObject(aviUploadInfo);
			}

			// 创建json文件夹
			string strJsonFolderPath = "D:\\FupanMESData";
			if (!Directory.Exists(strJsonFolderPath))
				Directory.CreateDirectory(strJsonFolderPath);

			// 保存json文件
			if (true)
			{
				string strJsonFileName = "";
				if (m_parent.m_global.m_strProductType == "nova")
				{
					strJsonFileName = $"{MES_product_info.m_AVI_tray_data_Nova.SetId}.json";
				}
				else if (m_parent.m_global.m_strProductType == "dock")
				{
					strJsonFileName = $"{MES_product_info.m_AVI_tray_data_Dock.set_id}.json";
				}

				string strJsonPath = Path.Combine(strJsonFolderPath, strJsonFileName);
				if (!File.Exists(strJsonPath))
				{
					File.Create(strJsonPath).Close();
				}
				else
				{
					File.Delete(strJsonPath);
					File.Create(strJsonPath).Close();
				}
				File.WriteAllText(strJsonPath, json);
			}

			string strMesServerResponse = "";

			if (aviUploadInfo.pcsSummarys.Count == 0)
			{
				response.Message = new List<string> { "上传汇总信息为空，没有需要上传的料片信息！" };
				return false;
			}

			// 是否提交MES前先检查已经跑过样品板
			if (m_parent.m_global.m_bCheckSampleProductStatusBeforeSubmittingRecheckResultsToMES)
			{
				bool bHasCheckExamplePanel = SendMES(m_parent.m_global.m_strMesValidationUrl, json, ref strMesServerResponse, 1, 15);
				if (!strMesServerResponse.StartsWith("{\""))
				{
					response.Message = new List<string> { strMesServerResponse };
					// response.Message[0] = strMesServerResponse;
					return false;
				}
				response = JsonConvert.DeserializeObject<MesResponse>(strMesServerResponse);

				if (strMesServerResponse.Contains("未进行过样品板测试"))
				{
					MessageBox.Show("机台尚未进行样品板测试！请先跑样品板");
					return false;
				}
				else if (response.Result == "-1" || response.Result == "-2")
				{
					return false;
				}
			}

			// 提交复判数据
			strMesServerResponse = "";
			bool bRet = SendMES(m_parent.m_global.m_strMesDataUploadUrl, json, ref strMesServerResponse, 1, 15);

			if (true == bRet)
			{
				if (m_parent.m_global.m_strProductSubType == "glue_check")
				{
					var temp = JsonConvert.DeserializeObject<List<object>>(strMesServerResponse);
					response = new MesResponse
					{
						Result = temp[0].ToString(),
						Message = new List<string> { temp[1].ToString() }
					};
				}
				else
				{
					response = JsonConvert.DeserializeObject<MesResponse>(strMesServerResponse);
				}

				if (response.Result == "0")
				{
					// 保存上一次复判的料号
					//m_parent.m_global.SaveConfigData("config.ini");

					if (m_parent.m_global.m_strProductType == "nova")
					{
						// 缺陷数据统计
						m_parent.m_global.m_mysql_ops.AddDataToDefectsStatisticsTable(m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Nova,
							m_parent.m_global.m_current_MES_tray_info.m_list_recheck_flags_by_defect);

						Application.Current.Dispatcher.Invoke(() =>
						{
							m_parent.page_HomeView.textbox_ScannedBarcode.Text = "";

						});
					}
					if (m_parent.m_global.m_strProductType == "dock")
					{
						if (m_parent.m_global.m_strProductSubType == "glue_check")
						{
							var ip = m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Dock.r5;
							var id = m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Dock.r7.Split('_')[0];
							var date = DateTime.Now.ToString("yyyyMMdd");
							var filePath = $@"\\{ip}\d\ResultImage\复判数据\{date}\{id}";
							CSVHelper.WriteGlueCheckResultCSVFile(filePath, m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Dock, uviUploadInfo);
						}

						Application.Current.Dispatcher.Invoke(() =>
						{
							// 缺陷数据统计
							m_parent.m_global.m_mysql_ops.AddDataToDefectsStatisticsTable(m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Dock,
								m_parent.m_global.m_current_MES_tray_info.m_list_recheck_flags_by_defect);

							// dock产品提交后将提交的json存入复判记录表中RecheckRecord,下次扫码同一盘料时读取记录 删除相同set_id的tray数据
							json = JsonConvert.SerializeObject(m_parent.m_global.m_current_MES_tray_info);
							m_parent.m_global.m_database_service.DeleteRecheckRecordIfExist(MES_product_info.m_AVI_tray_data_Dock.set_id);

							// 插入数据的SQL语句
							string strInsertSQL = @"
                            INSERT INTO RecheckRecord (SetID, JsonData)
                            VALUES (@SetID, @JsonData);";

							// 要增加新的return判断

							// 将产品信息插入数据库
							m_parent.m_global.m_mysql_ops.AddRecheckRecordToTable(strInsertSQL, json, MES_product_info.m_AVI_tray_data_Dock.set_id);
						});
					}

					return true;
				}
				else
				{
					return false;
				}
			}
			else
			{
				return false;
			}
		}

		/// <summary>
		/// 向AI复判接口提交未复判的数据
		/// </summary>
		/// <param name="product">需要提交的ProducuInfo信息</param>
		/// <param name="response">引用传递，接收Mes返回的复判结果信息。MesAIProductInfo可用于提交也可用于返回</param>
		/// <returns>返回是否向Mes成功发送信息，或Mes是否成功接收处理信息</returns>
		public bool SubmitUnrecheckProductInfoToAI(ProductInfo product, ref MesAIProductInfoSZ response)
		{
			try
			{
				if (product == null)
				{
					return false;
				}

				string json = "";

				// <Cam标号, <Pos标号, List<Defect>>>
				//var camWithPosWithDefects = new Dictionary<int, Dictionary<int, List<DefectInfo>>>();
				// <Side, <Cam, <Pos, List<DefectInfo>>>>
				var mesAIInfo = new MesAIProductInfoSZ();
				var sideWithCamPosIndexWithDefects = new Dictionary<int, Dictionary<int, Dictionary<int, List<DefectInfo>>>>();

				for (int j = 0; j < product.defects.Count; j++)
				{
					var defect = product.defects[j];

					if (!sideWithCamPosIndexWithDefects.ContainsKey(defect.side))
					{
						sideWithCamPosIndexWithDefects.Add(defect.side, new Dictionary<int, Dictionary<int, List<DefectInfo>>>());
					}

					if (!sideWithCamPosIndexWithDefects[defect.side].ContainsKey(defect.aiCam))
					{
						sideWithCamPosIndexWithDefects[defect.side].Add(defect.aiCam, new Dictionary<int, List<DefectInfo>>());
					}

					if (!sideWithCamPosIndexWithDefects[defect.side][defect.aiCam].ContainsKey(defect.aiPos))
					{
						sideWithCamPosIndexWithDefects[defect.side][defect.aiCam].Add(defect.aiPos, new List<DefectInfo>());
					}

					sideWithCamPosIndexWithDefects[defect.side][defect.aiCam][defect.aiPos].Add(defect);
				}

				var sideACamPos = new List<CamPos>();
				var sideBCamPos = new List<CamPos>();
				var sideCCamPos = new List<CamPos>();

				foreach (var kvp in sideWithCamPosIndexWithDefects)
				{
					foreach (var camWithPosWithDefects in kvp.Value)
					{
						foreach (var posWithDefects in camWithPosWithDefects.Value)
						{
							var newCamPos = new CamPos();

							newCamPos.camIndex = kvp.Key;
							newCamPos.posIndex = camWithPosWithDefects.Key;
							newCamPos.pathChannel = new List<string>();
							newCamPos.imgData = new List<string>();
							newCamPos.defectNames = new List<string>();
							newCamPos.defects = new List<AIDefect>();

							foreach (var defect in posWithDefects.Value)
							{
								var aiDefect = new AIDefect();
								var defectArea = new DefectArea();

								defectArea.height = defect.height;
								defectArea.width = defect.width;
								defectArea.x = defect.center_x;
								defectArea.y = defect.center_y;

								aiDefect.defectArea = new List<DefectArea> { defectArea };
								aiDefect.defectDetail = defect.type;
								aiDefect.defectType = defect.type;
								aiDefect.okNgLabel = "FAIL";
								aiDefect.roi = defectArea;

								newCamPos.defectNames.Add(defect.type);
								newCamPos.defects.Add(aiDefect);

								var imageFormat = $"Cam{camWithPosWithDefects.Key}Pos{posWithDefects.Key}_{defect.aiImageIndex}";
								switch (defect.side)
								{
									case 0:
										newCamPos.pathChannel.Add(product.m_list_local_imageA_paths_for_channel1.Where(p => p.Contains(imageFormat)).First());
										break;

									case 1:
										newCamPos.pathChannel.Add(product.m_list_local_imageB_paths_for_channel1.Where(p => p.Contains(imageFormat)).First());
										break;

									case 2:
										newCamPos.pathChannel.Add(product.m_list_local_imageC_paths_for_channel1.Where(p => p.Contains(imageFormat)).First());
										break;
								}

								// 如果newCamPos.pathChannel任何元素路径包含"avi_images"，则替换为"avi_images_forAI"
								//List<string> list_image_paths = new List<string>();
								//if (m_parent.m_global.m_nRecheckModeWithAISystem == 3)
								//{
								//    for (int i = 0; i < newCamPos.pathChannel.Count; i++)
								//    {
								//        list_image_paths.Add(newCamPos.pathChannel[i].Replace("avi_images", "avi_images_forAI"));
								//    }
								//}
								//else
								//{
								//    list_image_paths = newCamPos.pathChannel;
								//}

								var b64s = GeneralUtilities.convert_images_to_base64(newCamPos.pathChannel);
								//var b64s = GeneralUtilities.convert_images_to_base64(list_image_paths);

								for (int i = 0; i < b64s.Count; i++)
								{
									b64s[i] = @"data:image/jpeg;base64," + b64s[i];
								}

								newCamPos.imgData.AddRange(b64s);
							}

							// SZ C 1511 3571 AB 6051 7841

							// YC A 1878 2124 B 2034 2190
							switch (kvp.Key)
							{
								case 0:
									newCamPos.baseSize = new BaseSize() { height = 6051, width = 7841 };
									sideACamPos.Add(newCamPos);
									break;

								case 1:
									newCamPos.baseSize = new BaseSize() { height = 6051, width = 7841 };
									sideBCamPos.Add(newCamPos);
									break;

								case 2:
									newCamPos.baseSize = new BaseSize() { height = 1511, width = 3571 };
									sideCCamPos.Add(newCamPos);
									break;
							}
						}
					}
				}

				var pointPosInfo = new PointPosInfoSZ() { side_A = sideACamPos, side_B = sideBCamPos, side_C = sideCCamPos };

				mesAIInfo.barCode = product.barcode;
				mesAIInfo.finalRes = "FAIL";
				mesAIInfo.machineType = "CL5";
				mesAIInfo.tray = product.set_id;
				mesAIInfo.testType = "SMT-FAVI";
				mesAIInfo.testTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
				mesAIInfo.machine = product.machine_id;
				mesAIInfo.product = m_parent.m_global.m_strProductName;

				mesAIInfo.pointPosInfo = pointPosInfo;
				json = JsonConvert.SerializeObject(mesAIInfo);

				// save json to current directory
				string strJsonFolderPath = "D:\\aiRecheckSubmitData";
				strJsonFolderPath = Path.Combine(strJsonFolderPath, m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Dock.set_id);
				if (!Directory.Exists(strJsonFolderPath))
					Directory.CreateDirectory(strJsonFolderPath);

				string strJsonFileName = "";
				strJsonFileName = $"{product.barcode}.json";
				string strJsonPath = Path.Combine(strJsonFolderPath, strJsonFileName);
				if (!File.Exists(strJsonPath))
				{
					File.Create(strJsonPath).Close();
				}
				else
				{
					File.Delete(strJsonPath);
					File.Create(strJsonPath).Close();
				}
				File.WriteAllText(strJsonPath, json);

				string strMesServerResponse = "";
				bool bRet = SendMES(m_parent.m_global.m_strMesAIDataUploadUrl, json, ref strMesServerResponse, 1, 10);
				if (!strMesServerResponse.StartsWith("{\"barCode\":"))
				{
					response.finalRes = strMesServerResponse;
					return false;
				}
				response = JsonConvert.DeserializeObject<MesAIProductInfoSZ>(strMesServerResponse);

				return bRet;
			}
			catch (Exception e)
			{
				MessageBox.Show(e.Message);
			}

			return false;
		}

		public bool SubmitUnrecheckProductInfoToAI(ProductInfo product, ref MesAIProductInfoYC response)
		{
			try
			{
				if (product == null)
				{
					return false;
				}

				string json = "";

				var mesAIInfo = new MesAIProductInfoYC();

				mesAIInfo.barCode = product.barcode;
				//mesAIInfo.finalRes = product.defects.Count > 0 ? "FAIL" : "PASS";
				mesAIInfo.testTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
				mesAIInfo.machine = product.machine_id;
				mesAIInfo.machineType = "CL5";
				mesAIInfo.product = m_parent.m_global.m_strProductName;

				var defectNames_A = new List<string>();
				var defectNames_B = new List<string>();
				var imgData_A = new List<string>();
				var imgData_B = new List<string>();
				var pathChannel_A = new List<string>();
				var pathChannel_B = new List<string>();
				var defects_A = new List<AIDefect>();
				var defects_B = new List<AIDefect>();
				if (product.m_list_local_imageA_paths_for_channel1 != null &&
					product.m_list_local_imageA_paths_for_channel1.Count != 0)
				{
					pathChannel_A.AddRange(product.m_list_local_imageA_paths_for_channel1);

					// 对图像按通道排序
					var temp = new List<string>(pathChannel_A.Count) { "", "", "" };
					foreach (var p in pathChannel_A)
					{
						if (p.EndsWith("_4.jpg"))
						{
							temp[0] = p;
						}
						else if (p.EndsWith("_5.jpg"))
						{
							temp[1] = p;
						}
						else if (p.EndsWith("_6.jpg"))
						{
							temp[2] = p;
						}
					}
					pathChannel_A = temp;

					// 如果pathChannel_A任何元素路径包含"avi_images"，则替换为"avi_images_forAI"
					List<string> list_image_paths = new List<string>();
					if (m_parent.m_global.m_nRecheckModeWithAISystem == 3)
					{
						for (int i = 0; i < pathChannel_A.Count; i++)
						{
							list_image_paths.Add(pathChannel_A[i].Replace("avi_images", "avi_images_forAI"));
						}
					}
					else
					{
						list_image_paths = pathChannel_A;
					}

					//var b64s = GeneralUtilities.convert_images_to_base64(pathChannel_A);
					var b64s = GeneralUtilities.convert_images_to_base64(list_image_paths);

					for (int i = 0; i < b64s.Count; i++)
					{
						b64s[i] = @"data:image/jpeg;base64," + b64s[i];
					}

					imgData_A.AddRange(b64s);
				}

				if (product.m_list_local_imageB_paths_for_channel1 != null &&
					product.m_list_local_imageB_paths_for_channel1.Count != 0)
				{
					pathChannel_B.AddRange(product.m_list_local_imageB_paths_for_channel1);

					// 对图像按通道排序
					var temp = new List<string>(pathChannel_B.Count) { "", "", "" };
					foreach (var p in pathChannel_B)
					{
						if (p.EndsWith("_1.jpg"))
						{
							temp[0] = p;
						}
						else if (p.EndsWith("_2.jpg"))
						{
							temp[1] = p;
						}
						else if (p.EndsWith("_3.jpg"))
						{
							temp[2] = p;
						}
					}

					pathChannel_B = temp;

					// 如果pathChannel_B任何元素路径包含"avi_images"，则替换为"avi_images_forAI"
					List<string> list_image_paths = new List<string>();
					if (m_parent.m_global.m_nRecheckModeWithAISystem == 3)
					{
						for (int i = 0; i < pathChannel_B.Count; i++)
						{
							list_image_paths.Add(pathChannel_B[i].Replace("avi_images", "avi_images_forAI"));
						}
					}
					else
					{
						list_image_paths = pathChannel_B;
					}

					//var b64s = GeneralUtilities.convert_images_to_base64(pathChannel_B);
					var b64s = GeneralUtilities.convert_images_to_base64(list_image_paths);

					for (int i = 0; i < b64s.Count; i++)
					{
						b64s[i] = @"data:image/jpeg;base64," + b64s[i];
					}

					imgData_B.AddRange(b64s);
				}

				for (int j = 0; j < product.defects.Count; j++)
				{
					var defect = product.defects[j];
					var aiDefect = new AIDefect();
					var defectArea = new DefectArea();

					defectArea.height = defect.height;
					defectArea.width = defect.width;
					defectArea.x = defect.center_x;
					defectArea.y = defect.center_y;

					aiDefect.defectArea = new List<DefectArea> { defectArea };
					aiDefect.defectDetail = defect.type;
					aiDefect.defectType = defect.type;
					aiDefect.okNgLabel = "NG";
					aiDefect.roi = defectArea;

					if (defect.side == 1)
					{
						defectNames_B.Add(defect.type);
						defects_B.Add(aiDefect);
					}
					else if (defect.side == 0)
					{
						defectNames_A.Add(defect.type);
						defects_A.Add(aiDefect);
					}
				}

				var camPos_A = new CamPos();
				var camPos_B = new CamPos();

				camPos_A.baseSize = new BaseSize { height = 1878, width = 2124 };
				camPos_A.defectNames = defectNames_A;
				camPos_A.defects = defects_A;
				camPos_A.imgData = imgData_A;
				camPos_A.pathChannel = pathChannel_A;
				camPos_A.camIndex = 0;
				camPos_A.posIndex = 0;
				camPos_B.baseSize = new BaseSize { height = 2034, width = 2190 };
				camPos_B.defectNames = defectNames_B;
				camPos_B.defects = defects_B;
				camPos_B.imgData = imgData_B;
				camPos_B.pathChannel = pathChannel_B;
				camPos_B.camIndex = 0;
				camPos_B.posIndex = 0;

				var pointPosInfo = new PointPosInfoYC();
				var sideA = new Side();
				var sideB = new Side();

				sideA.cam0Pos0 = camPos_A;
				sideB.cam0Pos0 = camPos_B;
				pointPosInfo.side_A = sideA;
				pointPosInfo.side_B = sideB;

				mesAIInfo.pointPosInfo = pointPosInfo;
				json = JsonConvert.SerializeObject(mesAIInfo);

				// save json to current directory
				string strJsonFolderPath = "D:\\aiRecheckSubmitData";
				strJsonFolderPath = Path.Combine(strJsonFolderPath, m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Dock.set_id);

				if (!Directory.Exists(strJsonFolderPath))
					Directory.CreateDirectory(strJsonFolderPath);

				string strJsonFileName = "";
				strJsonFileName = $"{product.barcode}.json";

				string strJsonPath = Path.Combine(strJsonFolderPath, strJsonFileName);
				if (!File.Exists(strJsonPath))
				{
					File.Create(strJsonPath).Close();
				}
				else
				{
					File.Delete(strJsonPath);
					File.Create(strJsonPath).Close();
				}
				File.WriteAllText(strJsonPath, json);

				string strMesServerResponse = "";
				bool bRet = SendMES(m_parent.m_global.m_strMesAIDataUploadUrl, json, ref strMesServerResponse, 1, 10);
				if (!strMesServerResponse.StartsWith("{\"barCode\":"))
				{
					response = mesAIInfo;
					response.finalRes = strMesServerResponse;

					return false;
				}
				response = JsonConvert.DeserializeObject<MesAIProductInfoYC>(strMesServerResponse);

				return bRet;
			}
			catch (Exception e)
			{
				MessageBox.Show(e.Message);
			}

			return false;
		}

		// 检测一个特定的IP地址和端口（比如10.13.5.254:8080）是否可以成功连接
		public static bool IsPortOpen(string host, int port, int timeout)
		{
			try
			{
				using (var client = new TcpClient())
				{
					var result = client.BeginConnect(host, port, null, null);
					var success = result.AsyncWaitHandle.WaitOne(timeout);
					if (!success)
					{
						return false;
					}

					client.EndConnect(result);
					return true;
				}
			}
			catch
			{
				// Any exception will indicate that the port is closed
				return false;
			}
		}

		/// <summary>
		/// 从视觉算法部分传来的json中获取需要上传给Mes的图片路径字段的内容
		/// </summary>
		/// <param name="product">需要获取内容的ProductInfo类</param>
		/// <returns></returns>
		private Dictionary<string, string> getProductImagePathsandNames(ProductInfo product)
		{
			var productImgPathandResult = new Dictionary<string, string>();
			// 视觉传来的json，ProductInfo最后四个字段为需要上传MES的图片路径
			if (product.r7 != "")
			{
				// 判断LastIndexOf('_') - 2是否为负数，如果是则返回空字符串
				if (product.r7.LastIndexOf('_') - 2 < 0)
				{
					return productImgPathandResult;
				}

				// 增加图片路径为键，图片结果为值的对 图片路径最后一个"_"的前两个字符为图片结果
				productImgPathandResult.Add(product.r7, product.r7.Substring(product.r7.LastIndexOf('_') - 2, 2));
			}
			if (product.r8 != "")
			{
				// 判断LastIndexOf('_') - 2是否为负数，如果是则返回空字符串
				if (product.r8.LastIndexOf('_') - 2 < 0)
				{
					return productImgPathandResult;
				}

				productImgPathandResult.Add(product.r8, product.r8.Substring(product.r8.LastIndexOf('_') - 2, 2));
			}
			if (product.r9 != "")
			{
				// 判断LastIndexOf('_') - 2是否为负数，如果是则返回空字符串
				if (product.r9.LastIndexOf('_') - 2 < 0)
				{
					return productImgPathandResult;
				}

				productImgPathandResult.Add(product.r9, product.r9.Substring(product.r9.LastIndexOf('_') - 2, 2));
			}
			if (product.r10 != "")
			{
				// 判断LastIndexOf('_') - 2是否为负数，如果是则返回空字符串
				if (product.r10.LastIndexOf('_') - 2 < 0)
				{
					return productImgPathandResult;
				}

				productImgPathandResult.Add(product.r10, product.r10.Substring(product.r10.LastIndexOf('_') - 2, 2));
			}

			return productImgPathandResult;
		}

		private Dictionary<string, string> getProductImagePathsandNames(string pathsText)
		{
			var results = new Dictionary<string, string>();

			var paths = pathsText.Split(';');
			if (paths.Count() > 0)
			{
				foreach (var p in paths)
				{
					if (!String.IsNullOrEmpty(p))
					{
						results.Add(p, p.Substring(p.LastIndexOf('_') - 2, 2));
					}
				}
			}

			return results;
		}

		public void thread_handle_productInfo_toAI_queue()
		{
			var recheckDone = false;
			while (true)
			{
				if (m_parent.m_global != null && m_parent.m_global.m_bExitProgram)
				{
					break;
				}

				if (m_productInfos_waitSubmitToAI_queue?.Count > 0)
				{
					isSubmittingAI = true;

					var productInfo = m_productInfos_waitSubmitToAI_queue.Dequeue();
					if (productInfo == null)
					{
						m_parent.m_global.m_log_presenter.Log("将ProductInfo存入队列时有误");
						continue;
					}

					int nColumns = m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Dock.total_columns;
					int nRows = m_parent.m_global.m_current_MES_tray_info.m_AVI_tray_data_Dock.total_rows;
					int col = productInfo.column;
					int row = productInfo.row - 1;
					int nIndex = row * nColumns + col;

					if (m_parent.m_global.m_strSiteCity == "苏州")
					{
						var mesAIResponse = new MesAIProductInfoSZ();
						Thread.Sleep(100);
						if (!SubmitUnrecheckProductInfoToAI(productInfo, ref mesAIResponse))
						{
							m_parent.m_global.m_log_presenter.Log($"向提交AI复判提交第{nIndex}个产品的信息失败，失败信息：{mesAIResponse.finalRes}。可无视信息继续人工复判");
						}
						else
						{
							if (!m_parent.m_global.m_current_MES_tray_info.m_dictionary_barcode_with_mesAIProductInfoSZ.ContainsKey(mesAIResponse.barCode))
							{
								m_parent.m_global.m_current_MES_tray_info.m_dictionary_barcode_with_mesAIProductInfoSZ.Add(mesAIResponse.barCode, mesAIResponse);

								m_parent.m_global.m_log_presenter.Log($"后台线程：已提交第{nIndex}个产品SN：{productInfo.barcode}信息到AI复判");
							}
						}
					}
					else if (m_parent.m_global.m_strSiteCity == "盐城")
					{
						var mesAIResponse = new MesAIProductInfoYC();
						Thread.Sleep(100);
						if (!SubmitUnrecheckProductInfoToAI(productInfo, ref mesAIResponse))
						{
							m_parent.m_global.m_log_presenter.Log($"向提交AI复判提交第{nIndex}个产品的信息失败，失败信息：{mesAIResponse.finalRes}。可无视信息继续人工复判");
						}
						else
						{
							if (!m_parent.m_global.m_current_MES_tray_info.m_dictionary_barcode_with_mesAIProductInfoYC.ContainsKey(mesAIResponse.barCode))
							{
								m_parent.m_global.m_current_MES_tray_info.m_dictionary_barcode_with_mesAIProductInfoYC.Add(mesAIResponse.barCode, mesAIResponse);

								m_parent.m_global.m_log_presenter.Log($"后台线程：已提交第{nIndex}个产品SN：{productInfo.barcode}信息到AI复判");
							}
						}
					}

					if (m_productInfos_waitSubmitToAI_queue.Count == 0)
					{
						recheckDone = true;
					}
				}
				else
				{
					isSubmittingAI = false;
				}

				if (recheckDone)
				{
					m_parent.m_global.m_log_presenter.Log("AI复判提交已完成！可以进行人工提交");
					recheckDone = false;
				}
			}
		}
	}
}