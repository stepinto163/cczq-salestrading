using netapi.Api;
using STC.Common.Api;
using STC.Common.Model.FinQ;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.QQ;
using STC.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Handler.FinQ
{
    public class FinQHandler : IFinQHandler
    {
        private FinQHandler() { }
        private static IFinQHandler server = new FinQHandler();

        public static IFinQHandler Instance { get { return server; } }

        public PResult SendFQMessage(SendFQMessageReq request)
        {
            return ApiManager.Instance.GetDefaultApi().Request<PResult>(ApiConsts.FinQSvr, ApiConsts.Topic_stc_finq_sendmessage, request); //ClientApi.Instance.transactionForInner(request);
        }

        public PResult SendDesktopFinQ(SendDesktopFinQReq request)
        {
            return ApiManager.Instance.GetDefaultApi().Request<PResult>(ApiConsts.FinQSvr, ApiConsts.sendDesktopFinQ_Topic, request);
        }

        public GetUserConfigFinqReplyPo GetUserConfig(GetUserConfigFinqRequest request)
        {
            return ApiManager.Instance.GetDefaultApi().Request<GetUserConfigFinqReplyPo>(ApiConsts.FinQSvr, ApiConsts.Topic_stc_finq_getUserConfig, request);
        }

        public PResult SendFQGroupMessage(SendFQGroupMessageReq request)
        {
            return ApiManager.Instance.GetDefaultApi().Request<PResult>(ApiConsts.FinQSvr, ApiConsts.Topic_stc_find_sendGroupMessage, request);
        }

        public void SendWebFQ()
        {
            string url = LocalEnv.ConfInfo.WebFQAddress;
            STC.Common.Model.Chrome.ChromeWinModel cw = new STC.Common.Model.Chrome.ChromeWinModel() { IsOpenInMainTabPage = false, Url = url };
            cw.Key = "WebFQ";
            cw.Title = "报价助手";
            cw.IsShowAddressBar = false;
            cw.IsShowStatusBar = false;
            cw.IsEnableMaxMin = false;
            cw.IsShowAddressViewSourceDevTools = false;
            cw.UseChromePluginObject = true;
            //cw.ActionData = fstm == null ? null : CommonUtils.SerializeObject(fstm);
            //cw.ChangeBondData = updm == null ? null : CommonUtils.SerializeObject(updm);
            AppUtils.Application.PluginManager.Application.BroadCast("PopUpWebFQ", cw);
        }

        public void SendWebFQ(FQDataModel fstm = null, FQUpdateDataModel updm = null)
        {
            string url = LocalEnv.ConfInfo.WebFQAddress;
            STC.Common.Model.Chrome.ChromeWinModel cw = new STC.Common.Model.Chrome.ChromeWinModel() { IsOpenInMainTabPage = false, Url = url };
            cw.Key = "WebFQ";
            cw.Title = "报价助手";
            cw.IsShowAddressBar = false;
            cw.IsShowStatusBar = false;
            cw.IsEnableMaxMin = false;
            cw.IsShowAddressViewSourceDevTools = false;
            cw.UseChromePluginObject = true;
            //cw.ActionData = fstm == null ? null : CommonUtils.SerializeObject(fstm);
            //cw.ChangeBondData = updm == null ? null : CommonUtils.SerializeObject(updm);
            AppUtils.Application.PluginManager.Application.BroadCast("PopUpWebFQ", cw);
        }

        public void SendWebFQ(FQUpdateDataModel fstm = null, FQUpdateDataModel updm = null)
        {
            string url = LocalEnv.ConfInfo.WebFQAddress;
            STC.Common.Model.Chrome.ChromeWinModel cw = new STC.Common.Model.Chrome.ChromeWinModel() { IsOpenInMainTabPage = false, Url = url };
            cw.Key = "WebFQ";
            cw.Title = "报价助手";
            cw.IsShowAddressBar = false;
            cw.IsShowStatusBar = false;
            cw.IsEnableMaxMin = false;
            cw.IsShowAddressViewSourceDevTools = false;
            cw.UseChromePluginObject = true;
            //cw.ActionData = fstm == null ? null : CommonUtils.SerializeObject(fstm);
            //cw.ChangeBondData = updm == null ? null : CommonUtils.SerializeObject(updm);
            AppUtils.Application.PluginManager.Application.BroadCast("PopUpWebFQ", cw);
        }

        public PResult GetFinQServerToken(GetFinQTokenReq request)
        {
            return ApiManager.Instance.GetDefaultApi().Request<PResult>(ApiConsts.FinQSvr, ApiConsts.Topic_stc_finq_getServerToken, request);
        }
    }
}
