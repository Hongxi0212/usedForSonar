using FupanBackgroundService.DataModels;
using FupanBackgroundService.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;
using static FupanBackgroundService.DataModels.AVIProductInfo;

namespace FupanBackgroundService.Logics
{
    public class MESService
    {
        public string singleSideAIInterfaceUrl = "http://OT-DASMTAVIAI.MFLEX.COM.CN:8100/mes/chenling/v2/online-retrial";
        public string mergeSideAIInterfaceUrl = "http://OT-DASMTAVIAI.MFLEX.COM.CN:8100/mes/chenling/v2/online-retrial-merge";

        public bool SubmitSingleSideTrayInfoToAI(TrayInfo tray, ref MesAISingleSideAIInfo response)
        {
            if (tray.products == null || tray.products.Count < 1)
            {
                return false;
            }

            var product = tray.products[0];

            try
            {
                string json = "";

                // <Cam标号, <Pos标号, List<Defect>>>
                //var camWithPosWithDefects = new Dictionary<int, Dictionary<int, List<DefectInfo>>>();
                // <Side, <Cam, <Pos, List<DefectInfo>>>>
                var mesSingleSideAIInfo = new MesAISingleSideAIInfo();

                // 缺陷为空时说明AVI检测结果为PASS，不用上传
                var side = "";
                var imagePaths = new List<string>();
                if (product.m_list_local_imageA_paths_for_channel1.Count > 0)
                {
                    mesSingleSideAIInfo.pointPosInfo = new List<CamPos>() { new CamPos() { side = "A" } };
                    side = "A";
                    imagePaths = product.m_list_local_imageA_paths_for_channel1;
                }
                else if (product.m_list_local_imageB_paths_for_channel1.Count > 0)
                {
                    mesSingleSideAIInfo.pointPosInfo = new List<CamPos>() { new CamPos() { side = "B" } };
                    side = "B";
                    imagePaths = product.m_list_local_imageB_paths_for_channel1;
                }
                else if (product.m_list_local_imageC_paths_for_channel1.Count > 0)
                {
                    mesSingleSideAIInfo.pointPosInfo = new List<CamPos>() { new CamPos() { side = "C" } };
                    side = "C";
                    imagePaths = product.m_list_local_imageC_paths_for_channel1;
                }

                var pointPosInfo = new List<CamPos>();
                var camWithPosWithDefects = new Dictionary<int, Dictionary<int, List<DefectInfo>>>();

                if (product.defects != null && product.defects.Count > 0)
                {
                    for (int j = 0; j < product.defects.Count; j++)
                    {
                        var defect = product.defects[j];

                        if (!camWithPosWithDefects.ContainsKey(defect.aiCam))
                        {
                            camWithPosWithDefects.Add(defect.aiCam, new Dictionary<int, List<DefectInfo>>());
                        }

                        if (!camWithPosWithDefects[defect.aiCam].ContainsKey(defect.aiPos))
                        {
                            camWithPosWithDefects[defect.aiCam].Add(defect.aiPos, new List<DefectInfo>());
                        }

                        if (defect.type == "")
                        {
                            continue;
                        }

                        camWithPosWithDefects[defect.aiCam][defect.aiPos].Add(defect);
                    }
                }
                else
                {
                    mesSingleSideAIInfo.aiFinalRes = "";
                    mesSingleSideAIInfo.tray = tray.set_id;
                    mesSingleSideAIInfo.machine = product.machine_id;
                    mesSingleSideAIInfo.barCode = product.barcode;

                    if (!Global.m_instance.m_bSubmitOKProductsToAI)
                    {
                        response = mesSingleSideAIInfo;

                        json = JsonConvert.SerializeObject(response);

                        saveLocalSubmitJsonFile(product, side, json);
                        return true;
                    }
                    else
                    {
                        foreach (var p in imagePaths)
                        {
                            var lastDashIndex = p.LastIndexOf('\\');
                            var imageName = p.Substring(lastDashIndex + 1);
                            var imageCam = Convert.ToInt32(imageName.Substring(3, 1));
                            var imagePos = Convert.ToInt32(imageName.Substring(7, 1));

                            if (!camWithPosWithDefects.ContainsKey(imageCam))
                            {
                                camWithPosWithDefects.Add(imageCam, new Dictionary<int, List<DefectInfo>>());
                            }

                            if (!camWithPosWithDefects[imageCam].ContainsKey(imagePos))
                            {
                                camWithPosWithDefects[imageCam].Add(imagePos, new List<DefectInfo>());
                            }
                        }
                    }
                }

                foreach (var kvp in camWithPosWithDefects)
                {
                    foreach (var posWithDefects in kvp.Value)
                    {
                        var newCamPos = new CamPos();

                        var pathChannels = new List<string>();
                        var imgDatas = new List<string>();
                        findAndConvertDefectImages(product, side, kvp.Key, posWithDefects.Key, ref pathChannels, ref imgDatas);

                        newCamPos.side = side;
                        newCamPos.camIndex = kvp.Key;
                        newCamPos.posIndex = posWithDefects.Key;
                        newCamPos.defectNames = new List<string>();
                        newCamPos.defects = new List<AIDefect>();
                        newCamPos.pathChannel = pathChannels;
                        newCamPos.imgData = imgDatas;

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
                        }

                        // SZ C 1511 3571 AB 6051 7841
                        // YC A 1878 2124 B 2034 2190
                        switch (kvp.Key)
                        {
                            case 0:
                                if (Global.m_instance.m_strSiteCity == "苏州")
                                {
                                    newCamPos.baseSize = new BaseSize() { height = 6051, width = 7841 };
                                }
                                else if (Global.m_instance.m_strSiteCity == "盐城")
                                {
                                    newCamPos.baseSize = new BaseSize() { height = 1878, width = 2124 };
                                }
                                break;
                            case 1:
                                if (Global.m_instance.m_strSiteCity == "苏州")
                                {
                                    newCamPos.baseSize = new BaseSize() { height = 6051, width = 7841 };
                                }
                                else if (Global.m_instance.m_strSiteCity == "盐城")
                                {
                                    newCamPos.baseSize = new BaseSize() { height = 2034, width = 2190 };
                                }
                                break;
                            case 2:
                                if (Global.m_instance.m_strSiteCity == "苏州")
                                {
                                    newCamPos.baseSize = new BaseSize() { height = 1511, width = 3571 };
                                }
                                break;
                        }

                        pointPosInfo.Add(newCamPos);
                    }
                }

                mesSingleSideAIInfo.barCode = product.barcode;
                mesSingleSideAIInfo.finalRes = "FAIL";
                mesSingleSideAIInfo.machineType = "CL5";
                mesSingleSideAIInfo.tray = product.set_id;
                mesSingleSideAIInfo.testType = "SMT-FAVI";
                mesSingleSideAIInfo.testTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                mesSingleSideAIInfo.machine = product.machine_id;
                mesSingleSideAIInfo.product = Global.m_instance.m_strProductName;
                mesSingleSideAIInfo.pointPosInfo = pointPosInfo;

                json = JsonConvert.SerializeObject(mesSingleSideAIInfo);

                var bRet = false;
                var strMesServerResponse = "";
                try
                {
                    bRet = MesHelper.SendMES(singleSideAIInterfaceUrl, json, ref strMesServerResponse, 1, 5);
                saveSingleSideAIResponseJsonFile(product, side, strMesServerResponse);
                    if (!bRet || !strMesServerResponse.StartsWith("{\"barCode\":"))
                    {
                        response = mesSingleSideAIInfo;
                        response.finalRes = strMesServerResponse;
                        response.pointPosInfo = pointPosInfo;
                        return false;
                    }
                }
                catch(Exception ex)
                {
                    response = mesSingleSideAIInfo;
                }
                finally
                {
                    var aiInfoForSaveJson = mesSingleSideAIInfo;
                    aiInfoForSaveJson.pointPosInfo.ForEach(camPos =>
                    {
                        camPos.pathChannel = new List<string>();
                        camPos.imgData = new List<string>();
                    });

                    json = JsonConvert.SerializeObject(aiInfoForSaveJson);
                saveLocalSubmitJsonFile(product, side, json);
                }

                try
                {
                    response = JsonConvert.DeserializeObject<MesAISingleSideAIInfo>(strMesServerResponse);
                }
                catch (Exception ex)
                {
                    response = mesSingleSideAIInfo;
                    response.finalRes = "反序列化strMesServerResponse遇到异常：" + ex.Message;
                }

                return bRet;
            }
            catch (Exception e)
            {
                MessageBox.Show("中转软件向AI接口提交单面信息时异常，异常信息：" + e.Message);
                response.finalRes = "中转软件向AI接口提交单面信息时异常，异常信息：" + e.Message;
            }

            return false;
        }

        public bool SubmitMergeTrayInfoToAI(MesAIMergeAIInfo mergeInfo)
        {
            if(String.IsNullOrEmpty(mergeInfo.dmCode))
            {
                return false;
            }

            if (mergeInfo.pointPosInfo == null)
            {
                return false;
            }

            var json = JsonConvert.SerializeObject(mergeInfo);

            //saveLocalSubmitJsonFile(product, json);

            string strMesServerResponse = "";
            bool bRet = MesHelper.SendMES(mergeSideAIInterfaceUrl, json, ref strMesServerResponse, 1, 5);

            return bRet;
        }

        private void findAndConvertDefectImages(ProductInfo product, string side, int camIndex, int posIndex, ref List<string> pathChannel, ref List<string> imgData)
        {
            var imageFormat = $"cam{camIndex}Pos{posIndex}";
            switch (side)
            {
                case "A":
                    pathChannel.AddRange(product.m_list_local_imageA_paths_for_channel1.Where(p => p.Contains(imageFormat)));
                    break;
                case "B":
                    pathChannel.AddRange(product.m_list_local_imageB_paths_for_channel1.Where(p => p.Contains(imageFormat)));
                    break;
                case "C":
                    pathChannel.AddRange(product.m_list_local_imageC_paths_for_channel1.Where(p => p.Contains(imageFormat)));
                    break;
            }

            var b64s = new List<string>();
            var tempPath = @"C:\图片暂存图\中转暂存";
            if (!Directory.Exists(tempPath))
            {
                Directory.CreateDirectory(tempPath);
            }

            var tempPathChannel = new List<string>();
            foreach (var p in pathChannel)
            {
                var fileName = Path.GetFileName(p);
                var newPath = Path.Combine(tempPath, fileName);
                if (File.Exists(newPath))
                {
                    File.Delete(newPath);
                }
                File.Copy(p, newPath);
                tempPathChannel.Add(newPath);
            }

            for (int i = 0; i < tempPathChannel.Count; i++)
            {
                var bytes = File.ReadAllBytes(tempPathChannel[i]);
                b64s.Add(Convert.ToBase64String(bytes));
            }

            for (int i = 0; i < b64s.Count; i++)
            {
                b64s[i] = @"data:image/jpeg;base64," + b64s[i];
            }

            imgData = b64s;
        }

        private void saveLocalSubmitJsonFile(ProductInfo product, string side, string json)
        {
            var submitFolderPath = "F:\\fupanAIMesData\\submitData";
            var productPath = Path.Combine(submitFolderPath, product.barcode);
            if (!Directory.Exists(productPath))
                Directory.CreateDirectory(productPath);

            var sidePath = Path.Combine(productPath, side);
            if (!Directory.Exists(sidePath))
                Directory.CreateDirectory(sidePath);

            var jsonFileName = $"{DateTime.Now.ToString("HHmmssfff")}.json";
            string jsonFilePath = Path.Combine(sidePath, jsonFileName);

            if (!File.Exists(jsonFilePath))
            {
                File.Create(jsonFilePath).Close();
            }
            else
            {
                File.Delete(jsonFilePath);
                File.Create(jsonFilePath).Close();
            }

            File.WriteAllText(jsonFilePath, json);
        }

        private void saveSingleSideAIResponseJsonFile(ProductInfo product, string side, string json)
        {
            var submitFolderPath = "F:\\fupanAIMesData\\responseData";
            var productPath = Path.Combine(submitFolderPath, product.barcode);
            if (!Directory.Exists(productPath))
                Directory.CreateDirectory(productPath);

            var sidePath = Path.Combine(productPath, side);
            if (!Directory.Exists(sidePath))
                Directory.CreateDirectory(sidePath);

            var jsonFileName = $"{DateTime.Now.ToString("HHmmssfff")}.json";
            string jsonFilePath = Path.Combine(sidePath, jsonFileName);

            if (!File.Exists(jsonFilePath))
            {
                File.Create(jsonFilePath).Close();
            }
            else
            {
                File.Delete(jsonFilePath);
                File.Create(jsonFilePath).Close();
            }

            File.WriteAllText(jsonFilePath, json);
        }
    }

    public class MesAISingleSideAIInfo
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
        public List<CamPos> pointPosInfo { get; set; }
    }

    public class MesAIMergeAIInfo
    {
        [JsonPropertyName("dmCode")]
        public string dmCode { get; set; }

        [JsonPropertyName("testTime")]
        public string testTime { get; set; }

        [JsonPropertyName("testType")]
        public string testType { get; set; }

        [JsonPropertyName("aifinalRes")]
        public string aiFinalRes { get; set; }

        [JsonPropertyName("machine")]
        public string machine { get; set; }

        [JsonPropertyName("machinetype")]
        public string machineType { get; set; }

        [JsonPropertyName("product")]
        public string product { get; set; }

        [JsonPropertyName("pointPosInfo")]
        public List<CamPos> pointPosInfo { get; set; }
    }
}