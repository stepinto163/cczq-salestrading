
using STC.Common.Utils;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web.Script.Serialization;

namespace STC.Speech.aliyun
{
    class ResulfApi
    {
        private string appkey;
        private string token;
        public ResulfApi(string appkey, string token)
        {
            this.appkey = appkey;
            this.token = token;
        }
        public void processGETRequest(string text, string audioSaveFile, string format, int sampleRate)
        {
            /**
             * 设置HTTPS GET请求：
             * 1.使用HTTPS协议
             * 2.语音识别服务域名：nls-gateway.cn-shanghai.aliyuncs.com
             * 3.语音识别接口请求路径：/stream/v1/tts
             * 4.设置必须请求参数：appkey、token、text、format、sample_rate
             * 5.设置可选请求参数：voice、volume、speech_rate、pitch_rate
             */
            string url = "http://nls-gateway.aliyuncs.com/stream/v1/tts";
            url = url + "?appkey=" + appkey;
            url = url + "&token=" + token;
            url = url + "&text=" + text;
            url = url + "&format=" + format;
            url = url + "&sample_rate=" + sampleRate.ToString();
            // voice 发音人，可选，默认是xiaoyun。
            // url = url + "&voice=" + "xiaoyun";
            // volume 音量，范围是0~100，可选，默认50。
            // url = url + "&volume=" + 50;
            // speech_rate 语速，范围是-500~500，可选，默认是0。
            // url = url + "&speech_rate=" + 0;
            // pitch_rate 语调，范围是-500~500，可选，默认是0。
            // url = url + "&pitch_rate=" + 0;
            System.Console.WriteLine(url);
            /**
             * 发送HTTPS GET请求，处理服务端的响应。
             */
            HttpClient client = new HttpClient();
            HttpResponseMessage response = null;
            response = client.GetAsync(url).Result;
            string contentType = null;
            if (response.IsSuccessStatusCode)
            {
                string[] typesArray = response.Content.Headers.GetValues("Content-Type").ToArray();
                if (typesArray.Length > 0)
                {
                    contentType = typesArray.First();
                }
            }
            if ("audio/mpeg".Equals(contentType))
            {
                byte[] audioBuff = response.Content.ReadAsByteArrayAsync().Result;
                FileStream fs = new FileStream(audioSaveFile, FileMode.Create);
                fs.Write(audioBuff, 0, audioBuff.Length);
                fs.Flush();
                fs.Close();
                System.Console.WriteLine("The GET request succeed!");
            }
            else
            {
                // ContentType 为 null 或者为 "application/json"
                System.Console.WriteLine("Response status code and reason phrase: " +
                    response.StatusCode + " " + response.ReasonPhrase);
                string responseBodyAsText = response.Content.ReadAsStringAsync().Result;
                System.Console.WriteLine("The GET request failed: " + responseBodyAsText);
            }
        }
        JavaScriptSerializer serializer = new JavaScriptSerializer();
        public void processPOSTRequest(string text, string audioSaveFile, string format, string voice, int speech_rate)
        {
            /**
             * 设置HTTPS POST请求：
             * 1.使用HTTPS协议
             * 2.语音合成服务域名：nls-gateway.cn-shanghai.aliyuncs.com
             * 3.语音合成接口请求路径：/stream/v1/tts
             * 4.设置必须请求参数：appkey、token、text、format、sample_rate
             * 5.设置可选请求参数：voice、volume、speech_rate、pitch_rate
             */
            LogHandler.Info("text:" + text + ";audioSaveFile:" + audioSaveFile + ";format:" + format + ";voice:" + voice + ";speech_rate:" + speech_rate);
            string url = "https://nls-gateway.cn-shanghai.aliyuncs.com/stream/v1/tts";
            ResulfModel resulf = new ResulfModel();
            resulf.appkey = appkey;
            resulf.token = token;
            resulf.text = text;
            resulf.format = format;
            resulf.sample_rate = 16000;
            resulf.voice = voice;
            resulf.volume = 100;
            resulf.speech_rate = speech_rate;
            String bodyContent = serializer.Serialize(resulf);
            LogHandler.Info("Post JSON:" + bodyContent);
            StringContent content = new StringContent(bodyContent, Encoding.UTF8, "application/json");
            /**
             * 发送HTTPS POST请求，处理服务端的响应。
             */
            HttpClient client = new HttpClient();
            HttpResponseMessage response = client.PostAsync(url, content).Result;
            string contentType = null;
            if (response.IsSuccessStatusCode)
            {
                string[] typesArray = response.Content.Headers.GetValues("Content-Type").ToArray();
                if (typesArray.Length > 0)
                {
                    contentType = typesArray.First();
                }
            }
            if ("audio/mpeg".Equals(contentType))
            {
                byte[] audioBuff = response.Content.ReadAsByteArrayAsync().Result;
                FileStream fs = new FileStream(audioSaveFile, FileMode.Create);

                fs.Write(audioBuff, 0, audioBuff.Length);
                fs.Flush();
                fs.Close();
                LogHandler.Info("fileName:" + fs.Name);
                System.Console.WriteLine("The POST request succeed!");

            }
            else
            {
                LogHandler.Info("Response status code and reason phrase:" + response.StatusCode + " " + response.ReasonPhrase);
                System.Console.WriteLine("Response status code and reason phrase: " +
                    response.StatusCode + " " + response.ReasonPhrase);
                string responseBodyAsText = response.Content.ReadAsStringAsync().Result;
                LogHandler.Info("The POST request failed:" + responseBodyAsText);
                System.Console.WriteLine("The POST request failed: " + responseBodyAsText);
            }
        }
    }
}
