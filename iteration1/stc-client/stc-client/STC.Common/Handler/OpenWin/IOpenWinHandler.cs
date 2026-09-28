using System;

namespace STC.Common.Handler.OpenWin
{
    public interface IOpenWinHandler
    {
        void SubscribePriceWin(Action<object> callBack);
        void UnSubscribePriceWin(Action<object> callBack);
    }
}