using STC.Common.Model.TBook;
using STC.Common.Utils;
using STC.XBond.Plugin.Utils;
using STC.XBond.Plugin.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace STC.XBond.Plugin.ViewModel
{
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


        public ICommand ShowProhibitCommand { get; set; }

        public ICommand DealCommand { get; set; }

        private Dictionary<string, Common.Model.Fix.Message.NewOrderSingle> _newOrderSingleDic;
        private Dictionary<string, List<TPlaceOrderResult>> _warnReplyDic;
        private Dictionary<string, List<TPlaceOrderResult>> _prohibitReplyDic;
        private int _failCount = 0;

        public BatchResultWindowViewModel()
        {
            ShowProhibitCommand = new RelayCommand<Window>(ShowProhibit);
            DealCommand = new RelayCommand<Window>(DealWarn);
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

        public void Init(Dictionary<string, Common.Model.Fix.Message.NewOrderSingle> newOrderSingleDic,
            Dictionary<string, List<TPlaceOrderResult>> warnReplyDic,
            Dictionary<string, List<TPlaceOrderResult>> prohibitReplyDic,
            int failCount)
        {
            _newOrderSingleDic = newOrderSingleDic;
            _warnReplyDic = warnReplyDic;
            _prohibitReplyDic = prohibitReplyDic;
            _failCount = failCount;

            TotalCount = _newOrderSingleDic.Count;
            SuccessCount = TotalCount - failCount;
            WarnCount = _warnReplyDic.Count;
            ProhibitCount = _prohibitReplyDic.Count;
        }

        private void ShowProhibit(Window win)
        {
            if (!WindowManager.Instance.IsExist(WindowKeyHelper.GetBatchProhibitWinKey()))
            {
                BatchWarnView batchProhibitView = new BatchWarnView(_prohibitReplyDic, _newOrderSingleDic);
                WindowManager.Instance.OpenOnlyOneWindow(WindowKeyHelper.GetBatchProhibitWinKey(), batchProhibitView, win);
            }
        }

        private void DealWarn(Window win)
        {
            if (!WindowManager.Instance.IsExist(WindowKeyHelper.GetBatchWarnWinKey()))
            {
                BatchWarnView batchWarnView = new BatchWarnView(_warnReplyDic, _newOrderSingleDic);
                WindowManager.Instance.OpenOnlyOneWindow(WindowKeyHelper.GetBatchWarnWinKey(), batchWarnView, win);
            }
            
        }
    }
}
