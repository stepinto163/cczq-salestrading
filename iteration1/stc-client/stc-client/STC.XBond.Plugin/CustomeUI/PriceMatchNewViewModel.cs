using Ats.Foundation.Message;
using STC.Common.Model.Admin;
using STC.Common.Model.Base;
using STC.Common.Utils;
using STC.XBond.Plugin.CustomUI;
using STC.XBond.Plugin.ViewModel;
using STC.XBond.Plugin.ViewModel.Order;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace STC.XBond.Plugin.CustomeUI
{
    public class PriceMatchNewViewModel : ViewModelBase
    {
        private MsgPo _currentMsg = null;
        private FrameworkElement _parentUI = null;

        private string CurrentMsgId = string.Empty;
        private object _lockObj = new object();
        private readonly string FIRM_END_TXT = "已匹配到相关价格如下：";
        private readonly string NONFIRM_END_TXT = "已匹配到订单 （{0}笔），请确认firm量";
        private readonly string FIRM = "firm";
        private readonly string NONFIRM = "nonfirm";
        private readonly string ERROR_CHANNEL = "填写渠道数量错误";
        private readonly string ERROR_AMOUNT = "填写确认数量错误";
        private bool _isFirm = false;
        private PriceMatchDetail _priceMatchDetail = null;

        private BatchPointPriceDomain _batchPointPriceDomain = new BatchPointPriceDomain();

        public BatchPointPriceDomain BatchPointPriceDomain
        {
            get { return _batchPointPriceDomain; }
        }

        #region 界面属性
        
        private Visibility _cancelVisibility = Visibility.Visible;
        public Visibility CancelVisibility
        {
            get
            {
                return _cancelVisibility;
            }

            set
            {
                _cancelVisibility = value;
                this.OnPropertyChanged();
            }
        }

        private Visibility _confirmVisibility = Visibility.Collapsed;
        public Visibility ConfirmVisibility
        {
            get
            {
                return _confirmVisibility;
            }

            set
            {
                _confirmVisibility = value;
                this.OnPropertyChanged();
            }
        }

        #endregion


        public void InitUI(FrameworkElement frameworkElement, MsgPo msg, bool isRealtime)
        {
        }

        private string SetGoodsShortname(string goodcode)
        {
            string goodsType = string.Empty;
            List<BondsInfoPo> bondsInfoPos = CommonUtils.GetBondsInfoPo(goodcode, 1);
            if (bondsInfoPos != null && bondsInfoPos.Count > 0)
                goodsType = bondsInfoPos[0].SECURITY_TYPE;

            return goodsType;
        }
    }
}
