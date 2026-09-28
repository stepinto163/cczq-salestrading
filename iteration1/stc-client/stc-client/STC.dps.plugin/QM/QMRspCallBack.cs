using com.gateway.connector;
using com.gateway.connector.connector;
using com.gateway.connector.proto;
using com.gateway.connector.tcp;
using com.gateway.connector.utils;
using com.gateway.log;
using dps.connector;
using Newtonsoft.Json;
using STC.dps.plugin.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StcLite.dps.plugin.QM
{
    public class QMRsp
    {
        //public class QMNewRspCallBack : IRspCallBack
        //{
        //    public string content;
        //    private Proto proto;
        //    public QMNewRspCallBack(Proto proto)
        //    {
        //        this.proto = proto;
        //    }
        //    public void onRspMessage(string rsp)
        //    {
        //        proto.Format = Format.REPLY;
        //        proto.Body = Encoding.UTF8.GetBytes(rsp);
        //        ConnectorManager.send(proto.SessionId, proto);
        //        Dictionary<string, object> hm = JsonConvert.DeserializeObject<Dictionary<string, object>>(rsp);
        //        if (hm.ContainsKey("Code") && "0000".Equals(hm["Code"]))
        //        {
        //            NewOrderSingle newOrder = JsonConvert.DeserializeObject<NewOrderSingle>(content);
        //            QMHelper.Instance.AddOrUpdateOnQM(newOrder);
        //        }
        //    }
        //}

        //public class QMReferRspCallBack : IRspCallBack
        //{
        //    public string content;
        //    private Proto proto;
        //    public QMReferRspCallBack(Proto proto)
        //    {
        //        this.proto = proto;
        //    }
        //    public void onRspMessage(string rsp)
        //    {
        //        proto.Format = Format.REPLY;
        //        proto.Body = Encoding.UTF8.GetBytes(rsp);
        //        ConnectorManager.send(proto.SessionId, proto);
        //        Dictionary<string, object> hm = JsonConvert.DeserializeObject<Dictionary<string, object>>(rsp);
        //        if (hm.ContainsKey("Code") && "0000".Equals(hm["Code"]))
        //        {
        //            OrderCancelRequest cancelOrder = JsonConvert.DeserializeObject<OrderCancelRequest>(content);
        //            QMHelper.Instance.CancelOnQM(cancelOrder);
        //        }
        //    }
        //}

        public static Proto generateReplyFailQM(string sid, int cmd, int seq)
        {
            Proto proto = new Proto();
            proto.Cmd = cmd;
            proto.Format = Format.REPLY;
            proto.Seq = seq;
            proto.SessionId = sid;
            Dictionary<string, string> hm = new Dictionary<string, string>();
            hm["Code"] = Constants.CODE_7000;
            hm["Msg"] = Constants.MSG_7000;
            proto.Body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(hm));
            return proto;
        }

        public static Proto generateReplyFailBroker(string sid, int cmd, int seq)
        {
            Proto proto = new Proto();
            proto.Cmd = cmd;
            proto.Format = Format.REPLY;
            proto.Seq = seq;
            proto.SessionId = sid;
            Dictionary<string, string> hm = new Dictionary<string, string>();
            hm["Code"] = Constants.CODE_7001;
            hm["Msg"] = Constants.MSG_7001;
            proto.Body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(hm));
            return proto;
        }
    }
}
