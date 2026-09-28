using com.gateway.log;
using netapi.Api;
using STC.Common.Handler.Admin;
using STC.Common.Handler.Base;
using STC.Common.Handler.PT;
using STC.Common.Model.Admin;
using STC.Common.Model.Base;
using STC.Common.Model.PT;
using STC.Common.Utils;
using STC.PT.Plugin.Handler.Process;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace STC.PT.Plugin.Handler
{
    public class PTExecOrderHandler
    {
        private PTExecOrderHandler()
        {
            BaseHandler.Instance.Init(Instance_OnClear);
        }
        private static PTExecOrderHandler _PTExecOrderHandler = new PTExecOrderHandler();

        public static PTExecOrderHandler Instance { get { return _PTExecOrderHandler; } }
        public PtExecOrderQueryReq request;
        private bool isInit = false;
        public const string _ModelId = "4019";
        public bool ManageYourselfViewAll = false;
        public string user = ApiManager.Instance.GetDefaultApi().UserName;
        public void Init()
        {
            ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    if (!isInit)
                    {
                        List<ModulesAction> Action = AdminHandler.Instance.GetModulesActionById(_ModelId);
                        if (Action != null && Action.Count > 0)
                        {
                            foreach (ModulesAction m in Action)
                            {
                                if ("400702".Equals(m.id))//管理自己&查看全部
                                {
                                    user = "*";
                                    ManageYourselfViewAll = true;
                                }
                                else if ("400703".Equals(m.id))//管理全部
                                {
                                    user = "*";
                                }
                                else
                                {//400701管理自己
                                }
                                break;
                            }
                            PTHandler.Instance.SubscribeExec(Instance_OnExec, user);
                            isInit = true;
                        }
                    }
                }
                catch (Exception e)
                {
                    LogHelper.Error("ExecuterOrderHandler init:", e);
                }
            });
        }

        public void ExecOrderQuery(PtExecOrderQueryReq req)
        {
            //ThreadPool.QueueUserWorkItem(o =>
            //{
            request = req;
            bool flag = true;
            while (flag)
            {
                PtExecordersReplyPo reply = PTHandler.Instance.ExecOrderQuery(req);
                if (reply == null || reply.Datas == null)
                {
                    flag = false;
                    break;
                }
                if (reply != null && reply.Datas != null)
                {
                    foreach (TbookPtExecorders eo in reply.Datas)
                    {
                        PTExecOrderProcesser.Instance.OnOrder(eo);
                        //if (!_CachExecDic.ContainsKey(eo.Execid))
                        //    _CachExecDic.AddOrUpdate(eo.Execid, eo, (key, value) => eo);
                    }
                    if (reply.Datas.Count < req.pageSize)
                    {
                        flag = false;
                    }
                }
                req.pageNum += 1;
            }
            //});
        }


        private void Instance_OnExec(List<TbookPtExecorders> list)
        {
            foreach (TbookPtExecorders order in list)
            {
                if (request != null)
                {
                    if (!string.IsNullOrEmpty(request.securityid) && request.securityid != order.Securityid)
                        return;
                    if (!string.IsNullOrEmpty(request.settltype))
                    {
                        if ((request.settltype.Equals("F") && !order.Settltype.Contains("+")) || (!request.settltype.Equals("F") && !request.settltype.Equals(order.Settltype)))
                            return;
                    }
                }
                PTExecOrderProcesser.Instance.OnOrder(order);
            }
        }

        /// <summary>
        /// 清零回调方法
        /// </summary>
        /// <param name="obj"></param>
        private void Instance_OnClear(ClearPo obj)
        {
            LogHandler.Info("PTExecOrderHandler Clear");
            if (obj != null && "Y".Equals(obj.IsClear))
            {
                //UnSubscribe();
                Clear();

                Init();
               // SearchExeOrder();
            }
        }
        public void UnSubscribe()
        {
            //ThreadPool.QueueUserWorkItem(o =>
            //{
            try
            {
                //PTHandler.Instance.u(Instance_OnOrder);
            }
            catch (Exception e)
            {
                LogHelper.Error("ExecuterOrderHandler UnSubscribe:", e);
            }
            //});
        }

        public void Clear()
        {
            PTExecOrderProcesser.Instance.clear();

        }
    }
}
