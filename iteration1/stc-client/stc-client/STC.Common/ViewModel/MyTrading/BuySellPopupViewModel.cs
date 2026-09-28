using System.Windows.Media;

namespace STC.Common.ViewModel.SendPriceFutureViewLite
{
    /// <summary>
    /// 买卖量列提示窗口
    /// </summary>
    public class BuySellPopupViewModel : ViewModelBase
    {
        private Brush _Color = Brushes.OrangeRed;
        public Brush Color
        {
            get
            {
                return _Color;
            }
            set
            {
                _Color = value;
                this.OnPropertyChanged();
            }
        }

        private string _Sign;
        public string Sign
        {
            get
            {
                return _Sign;
            }
            set
            {
                _Sign = value;
                this.OnPropertyChanged();
            }
        }

        private string _Amount;
        public string Amount
        {
            get
            {
                return _Amount;
            }
            set
            {
                _Amount = value;
                this.OnPropertyChanged();
            }
        }

        public BuySellPopupViewModel()
        {
            //BindingOperations.EnableCollectionSynchronization(CrossDv, CrossOrdersProcesser.Processer.DetailDvLockObj);
        }

        ~BuySellPopupViewModel()
        {
            this.Dispose();
        }

        new public void Dispose()
        {
            base.Dispose();
        }

    }
}
