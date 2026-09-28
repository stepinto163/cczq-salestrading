using STC.Common.Model;
using STC.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace STC.Common.Data
{
    /// <summary>
    /// UI插件基本界面操作控制
    /// </summary>
    public class DesignHandler
    {
        private static DesignHandler designHandler = new DesignHandler();

        public static DesignHandler Instance { get { return designHandler; } }

        public static bool IsShow = false;

        public event Action<DisplayModel> DesignNotify;
        public event Action<object> OnShowSetting;
        public event Action ShowDesign;
        public event Action<object> OnSet;
        public event Action<object> OnMouseEnter;
        public event Action<object> OnSystemReady;
        public event Action<object> OnTabChange;
        public event Action OnSmallbellChange;// 按钮复位小铃铛
        public event Action<KeyEventArgs> OnKeyDown;
        public event Action<object> OnScreenUnlock;
        //自动登出通知
        public event Action<object> OnSystemExit;
        public event Action<EHotKeySetting> OnHotKey;
        //联动意向单
        public event Action<string> OnIntentionLink;
        //系统设置消息通知
        public event Action<object> OnSettingChange;
        // 打开报价识别
        public event Action<object, List<STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo>> OnShowRecognizeIntentionQuoteWin;
        // 打开IRS报价识别
        public event Action<object> OnShowIRSRecognizeIntentionQuoteWin;
        /// <summary>
        /// 再次激活小铃铛
        /// </summary>
        public event Action<object> OnGlobalActivate;// 主窗放大缩小

        /// <summary>
        /// 保存窗口位置
        /// </summary>
        public event Action<object> OnSaveWindowPos;

        public event Action<object> OnWindowDragMove;

        /// <summary>
        /// 打开券组管理
        /// </summary>
        public event Action<object> OnShowStrategyGroupManagementWin;

        /// <summary>
        /// 券组通知更新
        /// </summary>
        public event Action<object> OnNotifyUpdateGroup;

        /// <summary>
        /// 主窗体最小化通知事件
        /// </summary>
        public event Action<object> OnNotifyWindowState;


        /// <summary>
        /// 交易处理变化通知事件
        /// </summary>
        public event Action<object> OnTransactionModelChange;


        public Func<Interface.IPriceAndOrderViewInterface> OnPriceAndOrderView_BDB;

        public System.Windows.WindowState winstate;

        public event Action<object> OnMax;

        public event Action<object> OnBrokerSetting;

        public event Action<object> OnAgreeRef;

        public void SendBrokerSettingChange(object obj)
        {
            OnBrokerSetting?.Invoke(obj);
        }

        public void PublishKeyDown(KeyEventArgs keyEventArgs)
        {
            OnKeyDown?.Invoke(keyEventArgs);
        }
        private DesignHandler()
        {
        }

        public Interface.IPriceAndOrderViewInterface CreatePriceAndOrderView_BDB()
        {
            return OnPriceAndOrderView_BDB?.Invoke();
        }

        public void SmallbellChange()
        {
            try
            {
                OnSmallbellChange?.Invoke();
            }
            catch
            {
              
            }
        }
        public void setDesign(DisplayModel displayModel)
        {
            if (DesignNotify != null)
                DesignNotify(displayModel);

        }
        public void MouseoverDesignUI(object sender)
        {
            OnMouseEnter?.Invoke(sender);
        }

        public void showDesignUI(object sender)
        {
            //如果BDB，并且不含 XBound插件
            if (STC.Common.Utils.CommonUtils.IsBDBAppName() && !STC.Common.Utils.CommonUtils.IsContainsXBoundPlugin())
            {
                ShowDesign?.Invoke();
            }
            else
            {
                OnShowSetting?.Invoke(sender);
            }

            OnSet?.Invoke(sender);
        }

        public void SendTabChange(object obj)
        {
            OnTabChange?.Invoke(obj);
        }

        public void SystemIsReady(object obj)
        {
            OnSystemReady?.Invoke(obj);
        }

        public void IsScreenUnlock(object obj)
        {
            OnScreenUnlock?.Invoke(obj);
        }

        public void SystemExit(object obj)
        {
            OnSystemExit?.Invoke(obj);
        }

       
        public void TriggerHotKey(EHotKeySetting keyCode)
        {
            OnHotKey?.Invoke(keyCode);
        }

        public void IntentionLink(string intentionid)
        {
            OnIntentionLink?.Invoke(intentionid);
        }

        public void SettingChange(object obj)
        {
            OnSettingChange?.Invoke(obj);
        }

        public void ShowRecognizeIntentionQuoteWin(object obj, List<STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo> textList = null)
        {
            OnShowRecognizeIntentionQuoteWin?.Invoke(obj, textList);
        }

        /// <summary>
        /// 打开IRS识别窗口
        /// </summary>
        /// <param name="obj"></param>
        public void ShowIRSRecognizeIntentionQuoteWin(object obj)
        {
            OnShowIRSRecognizeIntentionQuoteWin?.Invoke(obj);
        }
        
        public void SetGlobalActivate(object obj)
        {
            OnGlobalActivate?.Invoke(obj);
        }

        public void SaveWindowPos(object obj)
        {
            OnSaveWindowPos?.Invoke(obj);
        }

        public void WindowDragMove(object obj)
        {
            OnWindowDragMove?.Invoke(obj);
        }
        
        public void ShowStrategyGroupManagementWin(object obj)
        {
            OnShowStrategyGroupManagementWin?.Invoke(obj);
        }

        public void NotifyUpdateGroup(object obj)
        {
            OnNotifyUpdateGroup?.Invoke(obj);
        }
        public void NotifyWindowState(object obj)
        {
            if (obj.GetType() == typeof(System.Windows.WindowState))
            { 
                this.winstate = (System.Windows.WindowState)obj;
            }
            
            OnNotifyWindowState?.Invoke(obj);
        }

        public void SetToMaxWin(object obj)
        {
            OnMax?.Invoke(obj);
        }

        public void TransactionModelChange(object obj)
        {
            OnTransactionModelChange?.Invoke(obj);
        }

        public void AgreeRef(object obj)
        {
            OnAgreeRef?.Invoke(obj);
        }
    }
}
