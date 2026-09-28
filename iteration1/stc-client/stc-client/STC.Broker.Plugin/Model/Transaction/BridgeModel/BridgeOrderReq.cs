using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Broker.Plugin.Model.Transaction.BridgeModel
{
    public class BridgeOrderReq
    {
        public List<SendInstSevieceModel> ofrInstExecOrderReqList = new List<SendInstSevieceModel>();
        public List<SendInstSevieceModel> bidInstExecOrderReqList = new List<SendInstSevieceModel>();
    }
}
