using STC.Frame.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Frame.Handler
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
        public event Action<object> OnSet;
        public event Action<object> OnMouseEnter;
        public event Action<object> OnSystemReady;
        public event Action<object> OnTabChange;
        public event Action OnSmallbellChange;
        public event Action OnKeyDown;
        public event Action<object> OnScreenUnlock;

        private DesignHandler()
        {
        }

        public void PublishKeyDown()
        {
            OnKeyDown?.Invoke();
        }

        public void SmallbellChange()
        {
            OnSmallbellChange?.Invoke();
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
            OnShowSetting?.Invoke(sender);

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
    }
}
