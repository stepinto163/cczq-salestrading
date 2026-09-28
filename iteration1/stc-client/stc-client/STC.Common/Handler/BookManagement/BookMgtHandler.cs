using netapi.Api;
using STC.Common.Api;
using STC.Common.Model.BookManagement;
using STC.Common.Model.Fix.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Handler.BookManagement
{
    public class BookMgtHandler : IBookMgtHandler
    {
        private static BookMgtHandler bookMgtHandler = new BookMgtHandler();

        public static IBookMgtHandler Instance { get { return bookMgtHandler; } }

        private BookMgtHandler()
        {

        }

        private bool isSubscribeExecOrdersForBlock = false;

        private event Action<List<ExecutionReport>> OnBlockOrder;

        // 查询包信息
        public BlockQueryReplyPo BlockQuery(BlockQueryReq req)
        {
            return ApiManager.Instance.GetDefaultApi().Request<BlockQueryReplyPo>(ApiConsts.RMQSvr, ApiConsts.Topic_stc_BlockQuery, req);//ClientApi.Instance.BlockQuery(req);
        }

        // 查询详细信息
        public BlockNumQueryPo BlockNumQuery(BlockNumQueryReq req)
        {
            return ApiManager.Instance.GetDefaultApi().Request<BlockNumQueryPo>(ApiConsts.RMQSvr, ApiConsts.Topic_stc_BlockNumQuery, req);//ClientApi.Instance.BlockNumQuery(req);
        }

        // 更新或新增接口
        public PResult BlockUpdate(List<ExecutionReport> req)
        {
            return ApiManager.Instance.GetDefaultApi().Request<PResult>(ApiConsts.RMQSvr, ApiConsts.Topic_stc_BlockUpdate, req);//ClientApi.Instance.BlockUpdate(req);
        }

        // 删除接口
        public PResult BlockDelete(BlockDeleteReq req)
        {
            return ApiManager.Instance.GetDefaultApi().Request<PResult>(ApiConsts.RMQSvr, ApiConsts.Topic_stc_BlockDelete, req);//ClientApi.Instance.BlockDelete(req);
        }

        // 重复接口
        public PResult BlockReSend(BlockReSendReq req)
        {
            return ApiManager.Instance.GetDefaultApi().Request<PResult>(ApiConsts.RMQSvr, ApiConsts.Topic_stc_BlockReSend, req);//ClientApi.Instance.BlockReSend(req);
        }

        // 注册实时通知接口
        public void SubscribeExecOrdersForBlock(Action<List<ExecutionReport>> onBlockCallBack)
        {
            if (onBlockCallBack != null)
                OnBlockOrder += onBlockCallBack;

            if (!isSubscribeExecOrdersForBlock)
            {
                ApiManager.Instance.GetDefaultApi().SubScribeWithImage(Instance_OnBlockOrder, new string[] { ApiConsts.Topic_stc_Block }, ApiConsts.RMQSvr, "");
                //ClientApi.Instance.SubscribeExecOrdersForBlock(OnBlockOrder);
                isSubscribeExecOrdersForBlock = true;
            }
        }

        private void Instance_OnBlockOrder(object obj)
        {
            List<ExecutionReport> data = BaseHelper.ConvertStcMessage<List<ExecutionReport>>(new string[] { ApiConsts.Topic_stc_Block }, obj);
            if (data != null)
                OnBlockOrder?.Invoke(data);
        }

        // 注销实时通知接口
        public void UnSubscribeExecOrdersForBlock(Action<List<ExecutionReport>> onBlockCallBack)
        {
            if (onBlockCallBack != null && OnBlockOrder != null)
                OnBlockOrder -= onBlockCallBack;
        }

        // 查询Broker信息
        public BlockQueryReplyPo SorExecAckQuery(SorExecAckQueryParas req)
        {
            return ApiManager.Instance.GetDefaultApi().Request<BlockQueryReplyPo>(ApiConsts.RMQSvr, ApiConsts.Topic_stc_SorExecAckQuery, req);//ClientApi.Instance.SorExecAckQuery(req);
        }
    }
}
