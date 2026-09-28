using STC.Common.Model.BookManagement;
using STC.Common.Model.Fix.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Handler.BookManagement
{
    public interface IBookMgtHandler
    {
        BlockQueryReplyPo BlockQuery(BlockQueryReq req);

        BlockNumQueryPo BlockNumQuery(BlockNumQueryReq req);

        PResult BlockUpdate(List<ExecutionReport> req);

        PResult BlockDelete(BlockDeleteReq req);

        PResult BlockReSend(BlockReSendReq req);

        void SubscribeExecOrdersForBlock(Action<List<ExecutionReport>> onBlockCallBack);

        void UnSubscribeExecOrdersForBlock(Action<List<ExecutionReport>> onBlockCallBack);

        BlockQueryReplyPo SorExecAckQuery(SorExecAckQueryParas req);
    }
}
