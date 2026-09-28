using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using com.gateway.connector;
using com.gateway.connector.proto;
using STC.Common.Model.Fix.Message;
using Newtonsoft.Json;
using com.gateway.log;
using STC.dps.plugin.Handler;

namespace STC.dps.plugin.QM
{
    public class QMMessage : IMessage
    {
        public QMMessage(Proto proto)
        {

        }

        ~QMMessage()
        {
            Dispose();
        }

        public void onMessage(string topic, string t)
        {
            try
            {
                QMHelper.Instance.onPublish(t);
            }
            catch (Exception e)
            {
                LogHelper.Error(e);
            }
        }

        public void Dispose()
        {

        }
    }
}