using netapi.Api;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.Position;
using System;
using System.Collections.Generic;

namespace STC.Common.Handler.Position
{
    public interface IPositionHandler
    {
        /// <summary>
        /// 计算净价和成交价
        /// </summary>
        /// <param name="securityID"></param>
        /// <param name="calType">4 计算净价 2 计算成交价</param>
        /// <param name="settleType"></param>
        /// <param name="price"></param>
        /// <returns></returns>
        string GetNetPrice(string securityID, string calType, string settleType, string price);

        /// <summary>
        /// 计算净价和成交价
        /// </summary>
        /// <param name="securityID"></param>
        /// <param name="calType"></param>
        /// <param name="settleType"></param>
        /// <param name="price"></param>
        /// <param name="isExeBond"></param>
        /// <returns></returns>
        string CalBond(string securityId, string calType, string settleType, string val, bool isExeBond);

        /// <summary>
        /// 手工录入订单
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        PResult ManualInputOrder(ExecutionReportReq req);

        /// <summary>
        /// 金额除以100
        /// </summary>
        /// <param name="price"></param>
        /// <returns></returns>
        string NumChange(string price);


        /// <summary>
        /// 查询所有的组合和组合ID
        /// </summary>
        /// <returns></returns>
        QueryPositionBookResult QueryBookUser();



        /// <summary>
        /// 查询有权限查看的组合
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        QueryPositionBookResult QueryPositionBook(QueryPositionBookReq req);

        /// <summary>
        /// 查询当日持仓明细
        /// </summary>
        /// <param name="onMsgCallBack"></param>
        void SubscribePositionDetail(List<string> lstBookId, Action<List<PositionDetailModel>> callBack);

        /// <summary>
        /// 取消订阅
        /// </summary>
        /// <param name="onMsgCallBack"></param>
        void UnSubscribePositionDetail(Action<List<PositionDetailModel>> callBack);


        /// <summary>
        /// 查询历史持仓
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        QureyHistoryDetailResult QueryHistoryPositionDetail(QueryPositionMvtReq req);

        /// <summary>
        /// 历史成交查询
        /// </summary>
        /// <param name="secid"></param>
        /// <returns></returns>
        QueryExternalResult QueryExternal(QueryExternalReq req);

        /// <summary>
        /// 修改手工订单请求/修改或增加组合接口请求
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        PResult UpdatePosition(UpdatePositionReq req);

        /// <summary>
        /// 订阅成交统计
        /// </summary>
        /// <param name="onMsgCallBack"></param>
        void SubscribeTradeStatistics(List<string> lstBookId, Action<List<TradeStatisticsModel>> callBack);

        /// <summary>
        /// 取消订阅
        /// </summary>
        /// <param name="onMsgCallBack"></param>
        void UnSubscribeTradeStatistics(Action<List<TradeStatisticsModel>> callBack);

        void ProcessPositionDetail(NetMessage netMsg);

        void ProcessTradeStatistics(NetMessage netMsg);
        /// <summary>
        /// 查询合约组合持仓
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        QueryPositionResult QueryPositionDetail(QueryPositionReq req);

        /// <summary>
        /// 重新跑批执行
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        PResult ManualProcessingPosition(ManualProcessingPositionReq req);

        /// <summary>
        /// 当前跑批任务
        /// </summary>
        /// <returns></returns>
        QueryProcessingRecordResult QueryProcessingPosition();

        /// <summary>
        /// 查询跑批历史
        /// </summary>
        /// <returns></returns>
        QueryProcessingRecordResult QueryProcessingPositionHist(QueryProcessingRecordHistReq req);

        void SubscribeProcessingRecordStatus(Action<QueryProcessingRecordResult> callBack);

        void UnSubscribeProcessingRecordStatus(Action<QueryProcessingRecordResult> callBack);

        void ProcessProcessingRecordStatus(NetMessage netMsg);
    }
}