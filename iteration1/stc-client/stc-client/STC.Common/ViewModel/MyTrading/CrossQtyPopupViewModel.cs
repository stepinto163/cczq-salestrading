using com.gateway.log;
using STC.Common.DataProcesser;
using STC.Common.Handler.Cal;
using STC.Common.Model.Cal;
using STC.Common.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace STC.Common.ViewModel.MyTrading
{
    /// <summary>
    /// 我的订单中明细单浮框CrossQtyPopupView的数据内容
    /// </summary>
    public class CrossQtyPopupViewModel : ViewModelBase
    {
        private string _SecurtityID;

        public string SecurtityID
        {
            get
            {
                return _SecurtityID;
            }

            set
            {
                _SecurtityID = value;
                this.OnPropertyChanged();
            }
        }

        private string _Symbol;
        public string Symbol
        {
            get
            {
                return _Symbol;
            }

            set
            {
                _Symbol = value;
                this.OnPropertyChanged();
            }
        }


        private string _STCCode;
        public string STCCode
        {
            get
            {
                return _STCCode;
            }

            set
            {
                _STCCode = value;
                this.OnPropertyChanged();
            }
        }

        private long _ExeQty;
        public long ExeQty
        {
            get
            {
                return _ExeQty;
            }

            set
            {
                _ExeQty = value;
                this.OnPropertyChanged();
            }
        }

        
        private decimal _TotalQty;
        public decimal TotalQty
        {
            get
            {
                return _TotalQty;
            }

            set
            {
                _TotalQty = value;
                this.OnPropertyChanged();
            }
        }

        private string _SettlType;
        public string SettleType
        {
            get
            {
                return _SettlType;
            }
            set
            {
                _SettlType = value;
                this.OnPropertyChanged();
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
                _Side = value;
                this.OnPropertyChanged();
            }
        }

        private string _Price;

        public string Price
        {
            get
            {
                return _Price;
            }
            set
            {
                _Price = value;
                this.OnPropertyChanged();
            }
        }
        

        //#region 表格字段
        //public const string STATUS_FIELD = "Status";
        //public const string ORDERID_FIELD = "OrderID";
        //public const string VENUES_FIELD = "Venues";
        //public const string PRICE_FIELD = "Price";
        //public const string NETPRICE_FIELD = "NetPrice";
        //public const string QTY_FIELD = "Qty";
        //public const string EDIT_NETPRICE_FIELD = "Edit_NetPrice";
        //public const string DISPLAYBTN_FIELD = "DisplayBtn";
        //public const string CLORDID_FIELD = "ClOrdID";
        //#endregion

        private DataView _CrossDv;
        public DataView CrossDv
        {
            get
            {
                lock(CrossOrdersProcesser.Processer.DetailDvLockObj)
                {
                    if (this._CrossDv == null)
                    {
                        _CrossDv = new DataView(CrossOrdersProcesser.Processer.CrossDt);
                        _CrossDv.Sort = $"{CrossOrdersProcesser.BLOCK_NUM} desc,{CrossOrdersProcesser.TRANSACTTIME_FIELD} desc";
                        this.OnPropertyChanged();
                    }
                }
                return _CrossDv;
            }
        }

        public CrossQtyPopupViewModel()
        {
            BindingOperations.EnableCollectionSynchronization(CrossDv, CrossOrdersProcesser.Processer.DetailDvLockObj);
        }

        ~CrossQtyPopupViewModel()
        {
            this.Dispose();
        }

        new public void Dispose()
        {
            // 释放我的订单数据
            lock (CrossOrdersProcesser.Processer.DetailDvLockObj)
            {
                if (_CrossDv != null)
                {
                    _CrossDv.Dispose();
                    _CrossDv = null;
                }
            }

            base.Dispose();
        }

        /// <summary>
        /// 构建表格数据
        /// </summary>
        public void GenCrossDv()
        {
            this.CrossDv.RowFilter = $"{CrossOrdersProcesser.STRATEGYORDERID_FIELD} = '{this.STCCode}' and {CrossOrdersProcesser.SIDE} = '{this.Side}'"; 
        }


        public decimal CalBond(string calType, string settleType, string val)
        {
            //ThreadPool.QueueUserWorkItem(o =>
            //{
                try
                {
                    CalReq requst = new CalReq();
                    requst.SecurityID = this.SecurtityID;
                    requst.CalType = calType;//"4";
                    requst.SettlType = settleType;// "1";
                    requst.Value = val;// customPrice;

                    CalResult result = CalHandler.Instance.CalBond(requst);
                    if (requst != null && result.Data != null)
                    {
                        //Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                        //{
                        if (calType.Equals("4"))
                        {
                            //NetPrice = result.Data.CleanPrice.ToString("N4");
                            return result.Data.CleanPrice;
                        }
                        else if (calType.Equals("2"))
                        {
                            //CustomPrice = (result.Data.Yield * 100).ToString("N4");
                            return result.Data.Yield;
                        }

                        //isSendReq = false;
                        //}));
                    }
                    //else
                        //isSendReq = false;
                }
                catch (Exception e)
                {
                    LogHelper.Error(e);
                    //isSendReq = false;
                }
            // });
            return decimal.Zero;
        }

    }
}
