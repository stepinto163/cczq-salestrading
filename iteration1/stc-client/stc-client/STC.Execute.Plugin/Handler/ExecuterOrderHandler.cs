using com.gateway.log;
using STC.Common.Data;
using STC.Common.Handler.Admin;
using STC.Common.Handler.Base;
using STC.Common.Handler.Ptr;
using STC.Common.Model.Base;
using STC.Common.Model.Execute;
using STC.Common.Utils;
using STC.Execute.Plugin.Command;
using STC.Execute.Plugin.Handler.Process;
using STC.Execute.Plugin.Utils;
using STC.Frame.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace STC.Execute.Plugin.Handler
{
    public class ExecuterOrderHandler
    {
        private ExecuterOrderHandler()
        {
            BaseHandler.Instance.Init(Instance_OnClear);
        }
        private static ExecuterOrderHandler _executerOrderHandler = new ExecuterOrderHandler();
        
        public static ExecuterOrderHandler Instance { get { return _executerOrderHandler; } }

        public void RegSystemReadyEvent()
        {
            DesignHandler.Instance.OnSystemReady += Instance_OnSystemReady;
        }

        private bool isInit = false;
        public void Init()
        {
            ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    //SearchExeOrder();
                    if (!isInit)
                    {
                        PtrHandler.Instance.Init(Instance_OnOrder, LocalEnv.UserInfo.UserName);
                        isInit = true;
                    }
                }
                catch (Exception e)
                {
                    LogHelper.Error("ExecuterOrderHandler init:", e);
                }
            });
        }

        private void Instance_OnSystemReady(object obj)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                this.Init();
                PopupWindowCommand popupWindowCommand = new PopupWindowCommand();
                //加载打开的窗口
                string lstkeysw = IniFileHelper.ReadValue($"STC.Execute.Plugin-{LocalEnv.UserInfo.UserName}", "Keys");
                string[] keyListw = lstkeysw.Split(',');
                for (int i = 0; i < keyListw.Length; i++)
                {
                    string winid = keyListw[i];
                    if (!string.IsNullOrEmpty(winid))
                    {
                        string para = IniFileHelper.ReadValue($"STC.Execute.Plugin_{LocalEnv.UserInfo.UserName}_{winid}", "CommandParameter");
                        if (!string.IsNullOrEmpty(para))
                        {
                            PopupTypeEnum pye = (PopupTypeEnum)Enum.Parse(typeof(PopupTypeEnum), para);
                            List<object> pobject = new List<object>();
                            pobject.Add(pye);
                            popupWindowCommand.Execute(pye);
                        }
                    }
                }

                DesignHandler.Instance.OnSystemReady -= Instance_OnSystemReady;
            }));
        }

        /// <summary>
        /// 清零回调方法
        /// </summary>
        /// <param name="obj"></param>
        private void Instance_OnClear(ClearPo obj)
        {
            LogHandler.Info("ExecuterOrderHandler Clear");
            if (obj != null && "Y".Equals(obj.IsClear))
            {
                UnSubscribe();
                Clear();

                Init();
                SearchExeOrder();
            }
        }


        public void Clear()
        {
            ExecOrderProcesser.Instance.clear();
          
        }

        public void UnSubscribe()
        {
            //ThreadPool.QueueUserWorkItem(o =>
            //{
                try
                {
                    //SearchExeOrder();

                    PtrHandler.Instance.UnSubscribeOrder(Instance_OnOrder);
                }
                catch (Exception e)
                {
                    LogHelper.Error("ExecuterOrderHandler UnSubscribe:", e);
                }
            //});
        }

        private void Instance_OnOrder(ExecuteOrder order)
        {
            ExecOrderProcesser.Instance.OnOrder(order,false);
        }

        public void SearchExeOrder()
        {
            ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    ExecuteOrderCondition executeOrderCondition = new ExecuteOrderCondition();
                    executeOrderCondition.PageNum = 1;
                    executeOrderCondition.PageSize = 2000;
                    bool isPermission = AdminHandler.Instance.CheckPermisionById(ExecOrderProcesser._ModelId, ExecOrderProcesser._Code);
                    if (!isPermission)
                    {
                        executeOrderCondition.User = LocalEnv.UserInfo.UserName;
                    }
                    bool flag = true;
                    while (flag)
                    {
                        ExeOrderSearchResp exeOrderSearchResp = PtrHandler.Instance.SearchExeOrder(executeOrderCondition);

                        if (exeOrderSearchResp == null || exeOrderSearchResp.Datas == null)
                        {
                            flag = false;
                            break;
                        }

                        if (exeOrderSearchResp != null && exeOrderSearchResp.Datas != null)
                        {
                            foreach (ExecuteOrder eo in exeOrderSearchResp.Datas)
                            {
                                ExecOrderProcesser.Instance.OnOrder(eo, true);
                                // Instance_OnOrder(eo,true);
                            }

                            if (exeOrderSearchResp.Datas.Count < executeOrderCondition.PageSize)
                            {
                                flag = false;
                            }

                        }
                        if (exeOrderSearchResp != null && exeOrderSearchResp.HasMoreData <= 0)
                            break;

                        executeOrderCondition.PageNum += 1;
                    }
                }
                catch (Exception e)
                {
                    LogHelper.Error("ExecuterOrderHandler init:", e);
                }
            });
        }
    }
}
