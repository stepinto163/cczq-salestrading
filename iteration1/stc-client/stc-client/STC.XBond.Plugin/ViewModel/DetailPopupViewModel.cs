using com.gateway.log;
using STC.Common.DataProcesser;
using STC.Common.Handler.Base;
using STC.Common.Model.Fix.Message;
using STC.Common.Utils;
using STC.XBond.Plugin.Utils;
using STC.XBond.Plugin.Utils.DataProcesser;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace STC.XBond.Plugin.ViewModel
{
    /// <summary>
    /// TradeBook悬浮框DetailPopupView的数据内容
    /// </summary>
    public class DetailPopupViewModel : ViewModelBase
    {
        private string _Title;
        public string Title
        {
            get
            {
                return _Title;
            }

            set
            {
                if (this._Title != value)
                {
                    this._Title = value;
                    this.OnPropertyChanged();
                }
                
            }
        }

        private decimal _Px;
        public decimal Px
        {
            get
            {
                return _Px;
            }

            set
            {
                if (this._Px != value)
                {
                    this._Px = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _Side;
        public string Side
        {
            get
            {
                return _Side;
            }

            set
            {
                if (_Side != value)
                {
                    _Side = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _SecurtityID;
        public string SecurtityID
        {
            get
            {
                return _SecurtityID;
            }

            set
            {
                if (_SecurtityID != value)
                {
                    _SecurtityID = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _Source;
        public string Source
        {
            get
            {
                return _Source;
            }

            set
            {
                if (_Source != value)
                {
                    _Source = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _venuesTypeGW;
        public string VenuesTypeGW
        {
            get
            {
                return _venuesTypeGW;
            }

            set
            {
                if (_venuesTypeGW != value)
                {
                    _venuesTypeGW = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _TotalQty;
        public string TotalQty
        {
            get
            {
                return _TotalQty;
            }

            set
            {
                if (this._TotalQty != value)
                {
                    this._TotalQty = value;
                    this.OnPropertyChanged();
                }
               
            }
        }

        private string _AllSettleType;
        public string AllSettleType
        {
            get
            {
                return _AllSettleType;
            }

            set
            {
                if (this._AllSettleType != value)
                {
                    this._AllSettleType = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _SettleType;
        public string SettleType
        {
            get
            {
                return _SettleType;
            }

            set
            {
                if (this._SettleType != value)
                {
                    this._SettleType = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private Visibility _isShowExpander;
        public Visibility IsShowExpander
        {
            get
            {
                return _isShowExpander;
            }

            set
            {
                if (this._isShowExpander != value)
                {
                    this._isShowExpander = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private DataView _DetailDv;
        public DataView DetailDv
        {
            get
            {
                lock(EmsOrderProcesser.Processer.DetailDvLockObj)
                {
                    if (_DetailDv == null)
                    {
                        _DetailDv = new DataView(EmsOrderProcesser.Processer.DetailDT);
                        _DetailDv.ListChanged += _DetailDv_ListChanged;
                        _DetailDv.Sort = $"{EmsOrderProcesser.KEY_FIELD} desc";
                        this.OnPropertyChanged();
                    }
                }
                return _DetailDv;
            }
        }

        private void _DetailDv_ListChanged(object sender, System.ComponentModel.ListChangedEventArgs e)
        {
            if (e.ListChangedType == System.ComponentModel.ListChangedType.ItemAdded
                || e.ListChangedType == System.ComponentModel.ListChangedType.ItemDeleted)
            {
                IsShowExpander = this._DetailDv.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public DetailPopupViewModel()
        {
            try
            {
                BindingOperations.EnableCollectionSynchronization(this.DetailDv, EmsOrderProcesser.Processer.DetailDvLockObj);
            }
            catch (Exception e)
            {
                LogHelper.Error(e);
            }
        }

        ~DetailPopupViewModel()
        {
            this.Dispose();
        }
        
        new public void Dispose()
        {
            lock (EmsOrderProcesser.Processer.DetailDvLockObj)
            {
                if (_DetailDv != null)
                {
                    _DetailDv.Dispose();
                    _DetailDv = null;
                }
            }

            base.Dispose();
        }


        public void FilterData()
        {
            lock(EmsOrderProcesser.Processer.DetailDvLockObj)
            {
                string side_str = this.Side == "0" ? "1" : "2";
                //string rowFilter = $@"{EmsOrderProcesser.VENUES_TYPE_GW_FIELD} = '{this.VenuesTypeGW}' and {EmsOrderProcesser.VENUES_FIELD} = '{this.Source}' and {EmsOrderProcesser.SIDE_FIELD} = '{side_str}' and {EmsOrderProcesser.SECURITY_ID_FIELD} =  '{this.SecurtityID}' and {EmsOrderProcesser.PX_FIELD} = {this.Px} ";
                string rowFilter = $@" {EmsOrderProcesser.VENUES_FIELD} = '{this.Source}' and {EmsOrderProcesser.SIDE_FIELD} = '{side_str}' and {EmsOrderProcesser.SECURITY_ID_FIELD} =  '{this.SecurtityID}' and {EmsOrderProcesser.PX_FIELD} = {this.Px} ";
                if (string.IsNullOrEmpty(this.SettleType) == false)
                {
                    rowFilter = $"{rowFilter} and {EmsOrderProcesser.SETTLE_TYPE_FIELD} in ({this.SettleType})";
                }
                this._DetailDv.RowFilter = rowFilter;
                IsShowExpander = this._DetailDv.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            }
        }
    }
}
