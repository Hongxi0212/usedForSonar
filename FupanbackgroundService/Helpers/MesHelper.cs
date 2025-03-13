using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace FupanBackgroundService.Helpers
{
    public class MesHelper
    {
        // 发送数据到MES
        public static bool SendMES(string strURL, string strDataToSend, ref string strResponse, int nGetOrPostFlag = 1, int timeoutInSeconds = 3, int readWriteTimeoutInSeconds = 3)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(strURL);

                request.Timeout = timeoutInSeconds * 1000;
                request.ReadWriteTimeout = readWriteTimeoutInSeconds * 1000;

                request.Method = "POST"; ;
                request.ContentType = "application/json";

                if (nGetOrPostFlag == 0)
                {
                    request.Method = "GET";
                }

                //Debugger.Log(0, null, string.Format("222222 SendMES url: {0}", strURL));

                // 发送请求数据
                using (StreamWriter writer = new StreamWriter(request.GetRequestStream(), Encoding.UTF8))
                {
                    writer.Write(strDataToSend);
                    writer.Flush();
                    writer.Close();

                    //Debugger.Log(0, null, string.Format("222222 MES 数据发送成功！"));
                }

                // 接收响应数据
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    //Debugger.Log(0, null, string.Format("222222 SendMES 111"));

                    string responseJson = reader.ReadToEnd();

                    //Debugger.Log(0, null, string.Format("222222 接收成功，MES服务器返回数据：{0}", responseJson));

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

        // 发送数据到MES
        public static bool SendMES2(string strURL, string strDataToSend, ref string strResponse, int nGetOrPostFlag = 1, int timeoutInMilliseconds = 200)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(strURL);

                request.Timeout = timeoutInMilliseconds;
                request.ReadWriteTimeout = timeoutInMilliseconds;

                request.Method = "POST"; ;
                request.ContentType = "application/json";

                if (nGetOrPostFlag == 0)
                {
                    request.Method = "GET";
                }

                //Debugger.Log(0, null, string.Format("222222 SendMES url: {0}", strURL));

                // 发送请求数据
                using (StreamWriter writer = new StreamWriter(request.GetRequestStream(), Encoding.UTF8))
                {
                    writer.Write(strDataToSend);
                    writer.Flush();
                    writer.Close();

                    //Debugger.Log(0, null, string.Format("222222 MES 数据发送成功！"));
                }

                // 接收响应数据
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    //Debugger.Log(0, null, string.Format("222222 SendMES 111"));

                    string responseJson = reader.ReadToEnd();

                    //Debugger.Log(0, null, string.Format("222222 接收成功，MES服务器返回数据：{0}", responseJson));

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
    }
}
