using Aliyun.Acs.Core;
using Aliyun.Acs.Core.Exceptions;
using Aliyun.Acs.Core.Profile;
using Newtonsoft.Json;
using STC.Common.Utils;
using STC.Speech.job;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Speech.aliyun
{
    public class RefulshToken
    {
        public static string getToken(string accessKeyID, string accessKeySecret)
        {
            LogHandler.Info("getToken start");
            // 构建一个客户端实例，用于发起请求。
            IClientProfile profile = DefaultProfile.GetProfile(
                 "cn-shanghai",          // 地域ID
                  accessKeyID,      // RAM账号的AccessKey ID
                  accessKeySecret); // RAM账号的AccessKey Secret
            DefaultAcsClient client = new DefaultAcsClient(profile);
            try
            {
                // 构造请求
                CommonRequest request = new CommonRequest();
                request.Domain = "nls-meta.cn-shanghai.aliyuncs.com";
                request.Version = "2019-02-28";

                // 因为是RPC风格接口，需指定ApiName(Action)。
                request.Action = "CreateToken";

                // 发起请求，并得到响应。
                CommonResponse response = client.GetCommonResponse(request);
                TokenSdkModel obj = JsonConvert.DeserializeObject<TokenSdkModel>(response.Data);
                LogHandler.Info("Resulsh Token:" + obj.ToString());
                if (obj.token != null)
                {
                    LogHandler.Info("getToken token:" + obj.token.Id.ToString());
                    return obj.token.Id.ToString();
                }
                else
                {
                    LogHandler.Info("Get Token failed");
                    return "Get Token failed";
                }

            }
            catch (ServerException ex)
            {
                LogHandler.Error(ex.ToString());
                System.Console.WriteLine(ex.ToString());
            }
            catch (ClientException ex)
            {
                LogHandler.Error(ex.ToString());
                System.Console.WriteLine(ex.ToString());
            }
            return null;
        }

    }
}
