using AutoScanFQCTest.DataModels;
using AutoScanFQCTest.Logics;
using AutoScanFQCTest.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AutoScanFQCTest.DialogWindows
{
    public partial class ManualScanBarcodeAndSubmitMES : Window
    {
        MainWindow m_parent;

        public ManualScanBarcodeAndSubmitMES(MainWindow parent)
        {
            InitializeComponent();

            m_parent = parent;
        }

        private void BtnSubmit_Click(object sender, RoutedEventArgs e)
        {
            string strBarcode = textbox_ScannedBarcode.Text.Trim();

            // 检查条码是否为空或者小于10位
            if (strBarcode.Length < 10)
            {
                System.Windows.MessageBox.Show("条码长度不正确！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // 提交MES
            if (true)
            {
                List<PieceSummary> list_summary = new List<PieceSummary>();
                List<UVIDetail> detailList = new List<UVIDetail>();
                List<UVISummary> summaryList = new List<UVISummary>();

                //string strOperatorName = "72007354";
                string strOperatorName = m_parent.m_global.m_strCurrentOperatorID;
                string testdate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                ProductInfo product = new ProductInfo();

                product.barcode = strBarcode;

                int nColumns = 10;
                int nRows = 10;

                int col = 1;
                int row = 1;

                PieceSummary summary = new PieceSummary();

                List<DefectDetail> list_defects = new List<DefectDetail>();

                summary.PanelId = product.barcode;
                summary.PcsSeq = "1";
                summary.OperatorName = strOperatorName;
                summary.OperatorTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                summary.VerifyOperatorName = strOperatorName;
                summary.VerifyTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                summary.TestResult = "FAIL";
                summary.VerifyResult = "PASS";

                DefectInfo defect = new DefectInfo();
                DefectDetail defect_detail = new DefectDetail();

                defect_detail.PanelId = product.barcode;
                defect_detail.PcsBarCode = product.barcode;
                defect_detail.PcsSeq = "1";
                defect_detail.TestType = "AVI-FQCB";
                defect_detail.PartSeq = "1";
                defect_detail.PinSeg = "1";
                defect_detail.BubbleValue = "";
                defect_detail.DefectCode = defect.type;
                defect_detail.OperatorName = strOperatorName;
                defect_detail.VerifyOperatorName = strOperatorName;
                defect_detail.VerifyTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                defect_detail.Description = defect.type;
                defect_detail.ImagePath = "";
                defect_detail.TestFile = "testFile";
                defect_detail.StrValue1 = "";
                defect_detail.StrValue2 = "";
                defect_detail.StrValue3 = "";
                defect_detail.StrValue4 = "";

                defect_detail.ImagePath = "";

                defect_detail.TestResult = "FAIL";
                defect_detail.VerifyResult = "PASS";

                list_defects.Add(defect_detail);

                summary.pcsDetails = list_defects;

                list_summary.Add(summary);

                MesPanelUploadInfo upload_info = new MesPanelUploadInfo();

                upload_info.Panel = "";
                upload_info.Resource = "";
                upload_info.Machine = "";
                upload_info.Uuid = "";
                upload_info.Machine = "";

                upload_info.OperatorName = strOperatorName;
                upload_info.OperatorType = "operatorType";
                upload_info.TrackType = "0";
                upload_info.WorkArea = "SMT-XRAY";
                upload_info.Mac = "F8-16-54-CD-AF-82";
                upload_info.VersionFlag = "0";
                upload_info.CheckDetail = "1";
                upload_info.CheckPcsDataForAVI = true;
                upload_info.TestTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                upload_info.JdeProductName = "fupan";
                upload_info.Site = "YCSMT";
                upload_info.TestMode = "PANEL";
                upload_info.TestType = "AVI-FQC";
                upload_info.ProgramName = "fupan";
                upload_info.TrackType = "0";
                upload_info.pcsSummarys = list_summary;

                string json = JsonConvert.SerializeObject(upload_info);

                // save json to current directory
                string strJsonFilePath = "D:\\test.json";

                File.WriteAllText(strJsonFilePath, json);

                string strMesServerResponse = "";
                
                bool bRet = m_parent.m_global.m_MES_service.SendMES(m_parent.m_global.m_strMesDataUploadUrl, json, ref strMesServerResponse, 1,15);

                if (true == bRet)
                {
                    MesResponse response = JsonConvert.DeserializeObject<MesResponse>(strMesServerResponse);

                    if (response.Result == "0")
                    {
                        add_text_to_textbox(string.Format("提交MES成功！MES返回信息：{0}", response.Message));

                        System.Windows.MessageBox.Show("提交MES成功！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        add_text_to_textbox(string.Format("提交MES失败！MES返回信息：{0}", response.Message));

                        System.Windows.MessageBox.Show("提交MES失败！" + response.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    add_text_to_textbox("提交MES失败！");

                    System.Windows.MessageBox.Show("提交MES失败！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

        }

        private void add_text_to_textbox(string info)
        {
            // MES返回信息显示在textbox_RunningInfo
            if (true)
            {
                string message = "MES返回信息";

                string time = DateTime.Now.ToLongTimeString().ToString();

                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    // 如果超过5000行，清除前面的3000行
                    if (textbox_RunningInfo.LineCount > 1000)
                    {
                        int index = textbox_RunningInfo.GetCharacterIndexFromLineIndex(300);

                        textbox_RunningInfo.Select(0, index);
                        textbox_RunningInfo.SelectedText = "";
                    }

                    textbox_RunningInfo.AppendText(time + "   " + message + "\n");

                    textbox_RunningInfo.ScrollToEnd();
                });
            }

            textbox_ScannedBarcode.Text = "";
            textbox_ScannedBarcode.Focus();
            textbox_ScannedBarcode.CaretIndex = textbox_ScannedBarcode.Text.Length;
        }

        private void textbox_ScannedBarcode_TextChanged(object sender, TextChangedEventArgs e)
        {
            
        }
    }
}
