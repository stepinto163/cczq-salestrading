using com.gateway.log;
using netapi.Api;
using STC.Common.Api;
using STC.Common.Model.QQ;
using System;
using System.Collections.Generic;
using System.Linq;

namespace STC.Common.Handler.IM
{
    public class IMHandler : IIMHandler
    {
        private IMHandler() { }
        private static IIMHandler server = new IMHandler();


        public static IIMHandler Instance { get { return server; } }
        private event Action<List<QQUserConfigPo>> OnQQMsg;
        public void Init(Action<List<QQUserConfigPo>> onQQMsg)
        {
            if (onQQMsg != null && (OnQQMsg == null || !OnQQMsg.GetInvocationList().Contains(onQQMsg)))
                OnQQMsg += onQQMsg;

            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                ApiManager.Instance.GetDefaultApi().SubScribeWithImage(Instance_OnQQMsg, new string[] { ApiConsts.DPS_TOPIC_QQ_S }, ApiConsts.STCLITESVR, "");
            });
            //QQUserConfigQuery();
          
        }

        private void Instance_OnQQMsg(object obj)
        {
            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                List<QQUserConfigPo> data = BaseHelper.ConvertStcMessage<List<QQUserConfigPo>>(new string[] { ApiConsts.DPS_TOPIC_QQ_S }, obj);
                if (data != null)
                {
                    if (OnQQMsg != null)
                    {
                        OnQQMsg?.Invoke(data);
                    }
                }
            });
        }


        public void QQUserConfigQuery()
        {
            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                try
                {
                    IMConfigPO o = ApiManager.Instance.GetDefaultApi().Request<IMConfigPO>(ApiConsts.STCLITESVR, ApiConsts.Topic_dps_QqQuery, null);
                    if (o != null && o.Datas != null && o.Datas.Count > 0)
                    {
                        if (OnQQMsg != null)
                        {
                            OnQQMsg?.Invoke(o.Datas);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Error(ex.Message);
                }
            });
        }
    }
}

