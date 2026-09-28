using STC.Common.Model.Fix.Message;
using STC.Common.Model.TBook;
using STC.Common.Utils;
using STC.Common.ViewModel;
using STC.WorkTable.Plugin.Utils;
using STC.WorkTable.Plugin.View;
using STC.WorkTable.Plugin.ViewModel.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace STC.WorkTable.Plugin.ViewModel
{
    public enum BatchEnumType
    {
        MyOrder,
        MyIntention
    }

    public class BatchResultWindowViewModel : ViewModelBase
    {
        private int totalCount = 0;
        public int TotalCount
        {
            get
            {
                return totalCount;
            }
            set
            {
                totalCount = value;
                this.OnPropertyChanged();
            }
        }

        private int successCount = 0;

        public int SuccessCount
        {
            get { return successCount; }
            set
            {
                successCount = value;
                if (successCount > 0)
                {
                    SuccessVisibility = Visibility.Visible;
                }
                else
                {
                    SuccessVisibility = Visibility.Collapsed;
                }
                this.OnPropertyChanged();
            }
        }


        private int warnCount;

        public int WarnCount
        {
            get { return warnCount; }
            set
            {
                warnCount = value;
                if (warnCount > 0)
                {
                    DealBtnVisibility = Visibility.Visible;
                }
                else
                {
                    DealBtnVisibility = Visibility.Collapsed;
                }
                this.OnPropertyChanged();
            }
        }

        private int prohibitCount;

        public int ProhibitCount
        {
            get { return prohibitCount; }
            set
            {
                prohibitCount = value;
                if (prohibitCount > 0)
                {
                    showLinkVisibility = Visibility.Visible;
                }
                else
                {
                    showLinkVisibility = Visibility.Collapsed;
                }
                this.OnPropertyChanged();
            }
        }

        private int exceptionCount;
        public int ExceptionCount
        {
            get
            {
                return exceptionCount;
            }
            set
            {
                exceptionCount = value;
                if (exceptionCount > 0)
                {
                    ShowExceptionLinkVisibility = Visibility.Visible;
                }
                else
                {
                    ShowExceptionLinkVisibility = Visibility.Collapsed;
                }
            }
        }

        private Visibility successVisibility = Visibility.Collapsed;

        public Visibility SuccessVisibility
        {
            get { return successVisibility; }
            set
            {
                successVisibility = value;
                this.OnPropertyChanged();
            }
        }

        private Visibility showExceptionLinkVisibility = Visibility.Collapsed;

        public Visibility ShowExceptionLinkVisibility
        {
            get { return showExceptionLinkVisibility; }
            set
            {
                showExceptionLinkVisibility = value;
                this.OnPropertyChanged();
            }
        }

        private Visibility showLinkVisibility = Visibility.Collapsed;

        public Visibility ShowLinkVisibility
        {
            get { return showLinkVisibility; }
            set
            {
                showLinkVisibility = value;
                this.OnPropertyChanged();
            }
        }


        private Visibility dealBtnVisibility = Visibility.Collapsed;

        public Visibility DealBtnVisibility
        {
            get { return dealBtnVisibility; }
            set
            {
                dealBtnVisibility = value;
                this.OnPropertyChanged();
            }
        }

        public ICommand ShowExceptionCommand { get; set; }

        public ICommand ShowProhibitCommand { get; set; }

        public ICommand DealCommand { get; set; }

        private Dictionary<string, NewOrderSingle> _newOrderSingleDic;
        private Dictionary<string, TradeRequestData> _IntentionQuoteRequestDic;
        private Dictionary<string, List<TPlaceOrderResult>> _warnReplyDic;
        private Dictionary<string, List<TPlaceOrderResult>> _prohibitReplyDic;
        private Dictionary<string, List<TPlaceOrderResult>> _exceptionReplyDic;
        private int _failCount = 0;

        private bool isWarnClick = false;
        private bool isProhibitClick = false;
        private bool isExceptionClick = false;

        private BatchEnumType _batchEnumType;

        public BatchResultWindowViewModel()
        {
            ShowExceptionCommand = new ComRelayCommand<Window>(ShowException);
            ShowProhibitCommand = new ComRelayCommand<Window>(ShowProhibit);
            DealCommand = new ComRelayCommand<Window>(DealWarn);
        }

        ~BatchResultWindowViewModel()
        {
            //if (_newOrderSingleDic != null) _newOrderSingleDic.Clear();
            //_newOrderSingleDic = null;
            //if (_warnReplyDic != null) _warnReplyDic.Clear();
            //_warnReplyDic = null;
            //if (_prohibitReplyDic != null) _prohibitReplyDic.Clear();
            //_prohibitReplyDic = null;
        }

        public void Init(object batchList,
            Dictionary<string, List<TPlaceOrderResult>> warnReplyDic,
            Dictionary<string, List<TPlaceOrderResult>> prohibitReplyDic,
            int failCount, Dictionary<string, List<TPlaceOrderResult>> exceptionReplyDic = null)
        {
            if (batchList is Dictionary<string, Common.Model.Fix.Message.NewOrderSingle>)
            {
                _batchEnumType = BatchEnumType.MyOrder;
                _newOrderSingleDic = batchList as Dictionary<string, Common.Model.Fix.Message.NewOrderSingle>;
                TotalCount = _newOrderSingleDic.Count;
                
                
            }
            else if (batchList is Dictionary<string, TradeRequestData>)
            {
                _batchEnumType = BatchEnumType.MyIntention;
                _IntentionQuoteRequestDic = batchList as Dictionary<string, TradeRequestData>;
                TotalCount = _IntentionQuoteRequestDic.Count;
            }

            SuccessCount = TotalCount - failCount;
            _failCount = failCount;
            _warnReplyDic = warnReplyDic;
            _prohibitReplyDic = prohibitReplyDic;
            _exceptionReplyDic = exceptionReplyDic;
            WarnCount = _warnReplyDic == null ? 0 : _warnReplyDic.Count;
            ProhibitCount = _prohibitReplyDic == null ? 0 : _prohibitReplyDic.Count;
            ExceptionCount = _exceptionReplyDic == null ? 0 : _exceptionReplyDic.Count;
        }

        private void ShowException(Window win)
        {
            isExceptionClick = true;
            if (_batchEnumType == BatchEnumType.MyIntention)
            {
                if (!WindowManager.Instance.IsExist(WindowKeyHelper.GetBatchExceptionWinKey_Intention()))
                {
                    BatchIntentionWarnView batchIntentionExceptionView = new BatchIntentionWarnView(_exceptionReplyDic, _IntentionQuoteRequestDic);
                    WindowManager.Instance.OpenOnlyOneWindow(WindowKeyHelper.GetBatchExceptionWinKey_Intention(), batchIntentionExceptionView, win);
                }
            }
        }

        private void ShowProhibit(Window win)
        {
            isProhibitClick = true;
            if (_batchEnumType == BatchEnumType.MyIntention)
            {
                if (!WindowManager.Instance.IsExist(WindowKeyHelper.GetBatchProhibitWinKey_Intention()))
                {
                    BatchIntentionWarnView batchIntentionProhibitView = new BatchIntentionWarnView(_prohibitReplyDic, _IntentionQuoteRequestDic);
                    WindowManager.Instance.OpenOnlyOneWindow(WindowKeyHelper.GetBatchProhibitWinKey_Intention(), batchIntentionProhibitView, win);
                }
            }
        }

        private void DealWarn(Window win)
        {
            isWarnClick = true;
            if (_batchEnumType == BatchEnumType.MyIntention)
            {
                if (!WindowManager.Instance.IsExist(WindowKeyHelper.GetBatchWarnWinKey_Intention()))
                {
                    BatchIntentionWarnView batchIntentionWarnView = new BatchIntentionWarnView(_warnReplyDic, _IntentionQuoteRequestDic);
                    WindowManager.Instance.OpenOnlyOneWindow(WindowKeyHelper.GetBatchWarnWinKey_Intention(), batchIntentionWarnView, win);
                }
            }
        }

        public bool IsAllProcess()
        {
            return (WarnCount == 0 || isWarnClick) && (prohibitCount == 0 || isProhibitClick) && (exceptionCount == 0 || isExceptionClick);
        }
    }
}
