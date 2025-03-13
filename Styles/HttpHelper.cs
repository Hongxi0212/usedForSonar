using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AutoScanFQCTest
{
    public class HttpHelper
    {
        /// <summary>
        ///  HTTP GET请求
        /// </summary>
        /// <param name="Url">URL地址</param>
        /// <param name="postDataStr">请求的参数</param>
        /// <param name="result">返回结果</param>
        /// <returns>返回状态：ResponseInfo</returns>
        public static ResponseInfo Get(string Url, string postDataStr,out string result)
        {
            ResponseInfo rp = new ResponseInfo();
            rp.StartTime = DateTime.Now;
            try
            {
                #region HTTP GET
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url + (postDataStr == "" ? "" : "?") + postDataStr);
                request.Method = "GET";
                request.ContentType = "text/html;charset=UTF-8";

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream myResponseStream = response.GetResponseStream();
                StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));
                result = myStreamReader.ReadToEnd();
                myStreamReader.Close();
                myResponseStream.Close();
                #endregion
                rp.ret = ResponseState.Normal.GetHashCode();
            }
            catch (Exception ex)
            {
                result = string.Empty;
                rp.Exception = ex;
            }
            rp.EndTime = DateTime.Now;
            return rp;
        }

        /// <summary>
        /// HTTP GET请求
        /// </summary>
        /// <param name="Url">URL地址</param>
        /// <param name="postDataStr">请求的参数</param>
        /// <param name="dic">返回字典格式</param>
        /// <returns>返回状态：ResponseInfo</returns>
        public static ResponseInfo TryGet(string Url, string postDataStr,out Dictionary<string, dynamic> dic)
        {
            ResponseInfo rp = new ResponseInfo();
            rp.StartTime = DateTime.Now;
            try
            {
                #region HTTP GET
                string aa = Url + (postDataStr == "" ? "" : "?") + postDataStr;
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url + (postDataStr == "" ? "" : "?") + postDataStr);
                request.Method = "GET";
                request.ContentType = "application/json";

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream myResponseStream = response.GetResponseStream();
                StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));
                string temp = myStreamReader.ReadToEnd();
                myStreamReader.Close();
                myResponseStream.Close();
                #endregion
    
                dic = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(temp);
                rp.ret = ResponseState.Normal.GetHashCode();
            }
            catch (Exception ex)
            {
                dic = new Dictionary<string, dynamic>();
                rp.Exception = ex;
            }
            rp.EndTime = DateTime.Now;
            return rp;
        }

        /// <summary>
        /// HTTP POST
        /// </summary>
        /// <param name="posturl">URL地址</param>
        /// <param name="postData">Post请求体</param>
        /// <param name="result">返回结果</param>
        /// <returns>返回状态：ResponseInfo</returns>
        public static ResponseInfo Post(string posturl, string postData,out string result)
        {
            ResponseInfo rp = new ResponseInfo();
            rp.StartTime = DateTime.Now;
            try
            {
                #region HTTP POST
                Stream outstream = null;
                Stream instream = null;
                StreamReader sr = null;
                HttpWebResponse response = null;
                HttpWebRequest request = null;
                Encoding encoding = System.Text.Encoding.GetEncoding("utf-8");
                byte[] data = encoding.GetBytes(postData);
                // 准备请求...

                // 设置参数
                request = WebRequest.Create(posturl) as HttpWebRequest;
                CookieContainer cookieContainer = new CookieContainer();
                request.CookieContainer = cookieContainer;
                request.AllowAutoRedirect = true;
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = data.Length;
                outstream = request.GetRequestStream();
                outstream.Write(data, 0, data.Length);
                outstream.Close();
                //发送请求并获取相应回应数据
                response = request.GetResponse() as HttpWebResponse;
                //直到request.GetResponse()程序才开始向目标网页发送Post请求
                instream = response.GetResponseStream();
                sr = new StreamReader(instream, encoding);
                //返回结果网页（html）代码
                result = sr.ReadToEnd();
                #endregion
                rp.ret = ResponseState.Normal.GetHashCode();
            }
            catch (Exception ex)
            {
                result = string.Empty;
                rp.Exception = ex;
            }
            rp.EndTime = DateTime.Now;
            return rp;
        }

        /// <summary>
        ///  HTTP POST
        /// </summary>
        /// <param name="posturl">URL地址</param>
        /// <param name="postData">Post请求体</param>
        /// <param name="dict">返回字典格式</param>
        /// <returns>返回状态：ResponseInfo</returns>
        public static ResponseInfo TryPost(string posturl, string postData,out Dictionary<string, string> dict)
        {
            ResponseInfo rp = new ResponseInfo();
            rp.StartTime = DateTime.Now;
            try
            {
                #region HTTP POST
                Stream outstream = null;
                Stream instream = null;
                StreamReader sr = null;
                HttpWebResponse response = null;
                HttpWebRequest request = null;
                Encoding encoding = System.Text.Encoding.GetEncoding("utf-8");
                byte[] data = encoding.GetBytes(postData);
                // 准备请求...

                // 设置参数
                request = WebRequest.Create(posturl) as HttpWebRequest;
                CookieContainer cookieContainer = new CookieContainer();
                request.CookieContainer = cookieContainer;
                request.AllowAutoRedirect = true;
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = data.Length;
                outstream = request.GetRequestStream();
                outstream.Write(data, 0, data.Length);
                outstream.Close();
                //发送请求并获取相应回应数据
                response = request.GetResponse() as HttpWebResponse;
                //直到request.GetResponse()程序才开始向目标网页发送Post请求
                instream = response.GetResponseStream();
                sr = new StreamReader(instream, encoding);
                //返回结果网页（html）代码
                string content = sr.ReadToEnd();
                string err = string.Empty;
                #endregion
                dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(content);
                rp.ret = ResponseState.Normal.GetHashCode();
            }
            catch (Exception ex)
            {
                dict = new Dictionary<string, string>();
                rp.Exception = ex;
            }
            rp.EndTime = DateTime.Now;
            return rp;
        }
    }
}
