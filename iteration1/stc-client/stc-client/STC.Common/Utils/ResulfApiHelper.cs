using com.gateway.log;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;

namespace STC.Common.Utils
{
    public class ResulfApiHelper
    {

        public static async Task SendMessageToQQ(string msg)
        {
            string responseData = await HttpPostRequest(msg);

            if (JsonSplit.IsJson(responseData))
            {
                QQDesktopSendMessageResponse resjson = JsonConvert.DeserializeObject<QQDesktopSendMessageResponse>(responseData);
                if ((resjson.errCode ?? "-1") != "0")
                    CommonUtils.Prompt2Second("QQ", resjson.errMsg ?? resjson.message);
            }
        }

        public static string GenMsg(string content, string securityID, string symbol, string side, string join, string price, string[,] vol_delivery, string qq)
        {
            QQHttpSendMessageModel qqsmm = new QQHttpSendMessageModel();
            qqsmm.content = content;
            qqsmm.data = new DataMessage();
            qqsmm.data.id = Guid.NewGuid().ToString("N");
            qqsmm.data.code = securityID;
            qqsmm.data.name = symbol;
            qqsmm.data.side = side;
            qqsmm.data.join = join;
            qqsmm.data.price = price;
            //string[,] vol_delivery = new string[,] { { "2000.5", "1" }, { "3000", "2" } };
            qqsmm.data.vol_delivery = vol_delivery;
            qqsmm.fromQB = LocalEnv.UserInfo.UserName;//"idb_trader10";//LocalEnv.ConfInfo.QQApiSender;
            qqsmm.openChat = true;
            qqsmm.toQQ = qq;//LocalEnv.ConfInfo.QQApiReceiver;//"3003945772";

            return JsonConvert.SerializeObject(qqsmm);
        }

        public static string ConvertSettleType(string oriSettleType)
        {
            bool isT0 = false;
            bool isT1 = false;
            bool isEWD = false;
            if (!string.IsNullOrEmpty(oriSettleType))
            {
                string[] splitArr = oriSettleType.Split(CommonConstants.COMMA_CHAR);

                foreach (string settleType in splitArr)
                {
                    if (settleType.Contains("+") || settleType == "4")
                    {
                        isEWD = true;
                    }
                    else if (settleType == "1")
                    {
                        isT0 = true;
                    }
                    else if (settleType == "2" || settleType == "3")
                    {
                        isT1 = true;
                    }
                }
            }

            string newSettleType = string.Empty;
            if (isT0 && isT1 && isEWD)
            {
                newSettleType = "7";
            }
            else if (isT1 && isEWD)
            {
                newSettleType = "6";
            }
            else if (isT0 && isEWD)
            {
                newSettleType = "5";
            }
            else if (isT0 && isT1)
            {
                newSettleType = "3";
            }
            else if (isEWD)
            {
                newSettleType = "4";
            }
            else if (isT1)
            {
                newSettleType = "2";
            }
            else if (isT0)
            {
                newSettleType = "1";
            }

            return newSettleType;
        }

        /// <summary>
        /// 发送HTTPS POST请求，处理服务端的响应。
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static async Task<string> HttpPostRequest(string msg)
        {
            using (var httpClient = new HttpClient())
            {
                //if (string.IsNullOrEmpty(LocalEnv.ConfInfo.QQApiSender) && string.IsNullOrEmpty(LocalEnv.ConfInfo.QQApiReceiver))
                //    return string.Empty;

                //string url = "https://finq-adapter.qmhost1.com/finq-api/gateway/desktopSendMessage";
                //string url = "https://finq.qmhost1.com/finq-server/v1/fq/action/draft_msg?type=bdb-intent";
                string url = LocalEnv.ConfInfo.QQApiHost;
                
                StringContent content = new StringContent(msg, Encoding.UTF8, "application/json");
                
                try
                {
                    //httpClient.DefaultRequestHeaders.Add("accept", "application/json");
                    httpClient.DefaultRequestHeaders.Add("Authorization", LocalEnv.ConfInfo.Authorization);
                    //httpClient.DefaultRequestHeaders.Add("Content-Type", "application/json");
                    HttpResponseMessage response = await httpClient.PostAsync(url, content);
                    
                    LogHelper.Debug($"RequestMessage:URL:{url} Body:{msg}");
                    // 检查响应是否成功
                    if (response.IsSuccessStatusCode)
                    {
                        // 读取响应内容
                        var responseData = await response.Content.ReadAsStringAsync();
                        LogHelper.Info($"Response Content:{responseData}");
                        return responseData;
                    }

                    string res = response.Content.ReadAsStringAsync().Result;
                    LogHelper.Error($"Response Content:{res}");
                    return res;
                }
                catch (HttpRequestException e)
                {
                    LogHelper.Error($"Response Content:{e.Message}");
                    QQDesktopSendMessageResponse re = new QQDesktopSendMessageResponse();
                    re.errCode = "404";
                    re.message = e.Message;
                    return JsonConvert.SerializeObject(re);
                }
            }
        }


        public static async Task<string> PostJsonAsync(string url, string msg, string token)
        {
            string result = null;

            try
            {
                HttpClient httpClient = new HttpClient();
                httpClient.BaseAddress = new Uri(url);
                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) HttpClient/1.0");
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Language", "zh-CN,zh;q=0.9");
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Cache-Control", "no-cache");
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Pragma", "no-cache");
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Access-Control-Allow-Origin", "*"); // ⚠️ 客户端设置此Header对跨域无效，仅作提示
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Access-Control-Allow-Headers", "*");
                httpClient.DefaultRequestHeaders.Add("Authorization", token);

                // 构建请求体
                var content = new StringContent(msg, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await httpClient.PostAsync(url, content);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    result = await response.Content.ReadAsStringAsync();
                    return result;
                }
            }
            catch (HttpRequestException e)
            {
                LogHelper.Error($"Response Content:{e.Message}");
                QQDesktopSendMessageResponse re = new QQDesktopSendMessageResponse();
                re.errCode = "404";
                re.message = e.Message;
                return JsonConvert.SerializeObject(re);
            }


            return result;
        }

        public static async Task<string> PostJsonAsync2(string url, string json, string token)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/json; charset=utf-8";
            request.Accept = "application/json, */*";
            request.Headers.Add("Authorization", token);
            //request.Headers.Add("Accept-Encoding", "deflate, br");
            request.Headers.Add("Accept-Language", "zh-CN,zh;q=0.9");
            request.Headers.Add("Cache-Control", "no-cache");
            request.Headers.Add("Pragma", "no-cache");
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) HttpWebRequest/1.0";

            // ⚠️ 仅调试时添加这些Header，服务端才有权限控制跨域
            request.Headers.Add("Access-Control-Allow-Origin", "*");
            request.Headers.Add("Access-Control-Allow-Headers", "*");

            // 写入请求体
            byte[] data = Encoding.UTF8.GetBytes(json);
            using (Stream requestStream = await request.GetRequestStreamAsync())
            {
                await requestStream.WriteAsync(data, 0, data.Length);
            }

            try
            {
                using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync())
                using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    string result = await reader.ReadToEndAsync();

                    LogHelper.Debug($"POST: URL:{url} Body:{json} Status:{response.StatusCode} Response:{result}");

                    if (response.StatusCode == HttpStatusCode.OK)
                        return result;
                    else
                        return null;
                }
            }
            catch (WebException ex)
            {
                HttpWebResponse errorResponse;
                // 捕获错误响应
                if (ex.Response is HttpWebResponse)
                {
                    errorResponse = ex.Response as HttpWebResponse;
                    using (var reader = new StreamReader(errorResponse.GetResponseStream()))
                    {
                        string errorText = await reader.ReadToEndAsync();
                        LogHelper.Error($"POST Failed: {url}, Status: {errorResponse.StatusCode}, Error: {errorText}");
                        return errorText;
                    }
                }

                LogHelper.Error($"POST Exception: {ex.Message}");
                return null;
            }
        }


        public static async Task ManualSendMessageToQQRequest(string qq, string msg)
        {
            await Task.Factory.StartNew(() =>
            {
                Application.Current.Dispatcher.Invoke((Action)(async () =>
                {
                    if (string.IsNullOrEmpty(msg))
                    {
                        return;
                    }

                    ClipboardHelper.CopyToClipboard(msg);

                    if (!string.IsNullOrEmpty(qq))
                    {
                        Process pro = new Process();
                        string arguments = $"tencent://message/?Menu=yes&amp;uin={qq}";
                        Process.Start(arguments);
                        Thread.Sleep(1000);
                        //if (LocalEnv.ConfInfo.QQStartSleep.HasValue && LocalEnv.ConfInfo.QQStartSleep.Value > 0)
                        //    Thread.Sleep(LocalEnv.ConfInfo.QQStartSleep.Value);
                        System.Windows.Forms.SendKeys.SendWait("^V");

                        //if (LocalEnv.ConfInfo.QQSendSleep.HasValue && LocalEnv.ConfInfo.QQSendSleep.Value > 0)
                        //    Thread.Sleep(LocalEnv.ConfInfo.QQSendSleep.Value);

                        //Thread.Sleep(100);
                        //System.Windows.Forms.SendKeys.SendWait("{Enter}");
                    }
                    else
                    {
                        //string errorMsg = string.Format("{0} QQ is null", TraderBName);
                        //CommonUtils.Prompt2Second("QQ", resjson.errMsg ?? resjson.message);
                    }
                }), DispatcherPriority.Send);
            });
        }


        public static string GetQQByBrokerCN(string cnborker)
        {
            string qq = string.Empty;
            if (!string.IsNullOrEmpty(cnborker))
            {
                string brokeren = ComboSourceUtil.Instance.FindEnum(cnborker, "venues_cn_to_en");
                qq = GetQQByBroker(brokeren);
            }
            return qq;
        }

        public static string GetQQByBroker(string cnborker)
        {
            string qq = string.Empty;
            if (!string.IsNullOrEmpty(cnborker))
            {
                switch (cnborker)
                {
                    case "TPSC":
                        qq = LocalEnv.ConfInfo.QQToTPSC;
                        break;
                    case "CTTJ":
                        qq = LocalEnv.ConfInfo.QQToCTTJ;
                        break;
                    case "CNEX":
                        qq = LocalEnv.ConfInfo.QQToCNEX;
                        break;
                    case "CCTB":
                        qq = LocalEnv.ConfInfo.QQToCCTB;
                        break;
                    case "PTCN":
                        qq = LocalEnv.ConfInfo.QQToPTCN;
                        break;
                    case "UEDA":
                        qq = LocalEnv.ConfInfo.QQToUEDA;
                        break;
                }
            }
            return qq;
        }
    }

    public class QQHttpSendMessageModel
    {
        public string content;
        public DataMessage data;
        public string fromQB;
        public bool openChat;
        public string toQQ;
    }

    public class DataMessage
    {
        public string id;
        public string code;
        public string name;
        public string side;
        public string price;
        public string[,] vol_delivery;
        public string join; 
    }

    public class QQDesktopSendMessageModel
    {
        public string message;
        public string sender;
        public string receiver;
        public bool send_direct = true;
        public bool clear_edit = true;
        public string channel = "0";
    }

    public class QQDesktopSendMessageResponse
    {
        public string errCode;
        public string errMsg;
        public string code;
        public string message;
    }
}
;