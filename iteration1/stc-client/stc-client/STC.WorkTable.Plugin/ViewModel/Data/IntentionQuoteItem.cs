using STC.Common.Model.Fix.Message;
using STC.Common.Model.Table;
using STC.Common.Model.TBook;
using STC.Common.Utils;
using STC.Common.ViewModel;
using STC.Frame.Utils;
using STC.WorkTable.Plugin.Utils.Data.CRM.Model.CRM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.WorkTable.Plugin.ViewModel.Data
{
    public class IntentionQuoteItem : TradeRequestData
    {
        private bool _isChecked = false;

        public bool IsChecked
        {
            get { return _isChecked; }
            set
            {
                _isChecked = value;
                this.OnPropertyChanged();
            }
        }

        private CrmInst _selectedInstitution_Edit;

        public CrmInst SelectedInstintuion_Edit
        {
            get { return _selectedInstitution_Edit; }
            set
            {
                _selectedInstitution_Edit = value;
                this.OnPropertyChanged();
            }
        }

        private string _institutionName_Edit;

        public string InstitutionName_Edit
        {
            get { return _institutionName_Edit; }
            set
            {
                _institutionName_Edit = value;
                this.OnPropertyChanged();
            }
        }

        //private string _institutionName_Text;

        //public string InstitutionName_Text
        //{
        //    get { return _institutionName_Text; }
        //    set
        //    {
        //        _institutionName_Text = value;
        //        this.OnPropertyChanged();
        //    }
        //}


        private CrmContact _selectedTrader_Edit;

        public CrmContact SelectedTrader_Edit
        {
            get { return _selectedTrader_Edit; }
            set
            {
                _selectedTrader_Edit = value;
                this.OnPropertyChanged();
            }
        }

        private string _traderName_Edit;

        public string TraderName_Edit
        {
            get { return _traderName_Edit; }
            set
            {
                _traderName_Edit = value;
                this.OnPropertyChanged();
            }
        }

        //private string _traderName_Text;

        //public string TraderName_Text
        //{
        //    get { return _traderName_Text; }
        //    set
        //    {
        //        _traderName_Text = value;
        //        this.OnPropertyChanged();
        //    }
        //}

        private string _side_Edit;

        public string Side_Edit
        {
            get { return _side_Edit; }
            set
            {
                _side_Edit = value;
                this.OnPropertyChanged();
            }
        }

        private decimal? _fullPrice;

        public decimal? FullPrice
        {
            get { return _fullPrice; }
            set
            {
                _fullPrice = value;
                this.OnPropertyChanged();
            }
        }

        private decimal? _price_Edit;

        public decimal? Price_Edit
        {
            get { return _price_Edit; }
            set
            {
                _price_Edit = value;
                this.OnPropertyChanged();
            }
        }

        private decimal? _qty_Edit;

        public decimal? Qty_Edit
        {
            get { return _qty_Edit; }
            set
            {
                _qty_Edit = value;
                this.OnPropertyChanged();
            }
        }

        private string _settlType_edit;

        public string SettlType_Edit
        {
            get { return _settlType_edit; }
            set
            {
                _settlType_edit = value;
                this.OnPropertyChanged();
            }
        }

        private DateTime _edit_settltype_date;

        public DateTime edit_settltype_date
        {
            get { return _edit_settltype_date; }
            set
            {
                _edit_settltype_date = value;
                OnPropertyChanged();
            }
        }

        private int _edit_settletype = int.MinValue;

        public int edit_settletype
        {
            get { return _edit_settletype; }
            set
            {
                _edit_settletype = value;
                this.OnPropertyChanged();
            }
        }

        private string _brokerId = LocalEnv.UserInfo.UserName;

        public string BrokerId
        {
            get { return _brokerId; }
            set
            {
                _brokerId = value;
                this.OnPropertyChanged();
            }
        }

        private string _brokerName = LocalEnv.UserInfo.FullName;

        public string BrokerName
        {
            get { return _brokerName; }
            set
            {
                _brokerName = value;
                this.OnPropertyChanged();
            }
        }

        private string _closeBy;

        public string CloseBy
        {
            get { return _closeBy; }
            set
            {
                _closeBy = value;
                this.OnPropertyChanged();
            }
        }


        private string _orderID;

        public string OrderID
        {
            get
            {
                return _orderID;
            }
            set
            {
                _orderID = value;
                this.OnPropertyChanged();
            }
        }


        private string _isEdit = "2"; // 0:显示 2：不显示

        public string IsEdit
        {
            get
            {
                return _isEdit;
            }
            set
            {
                _isEdit = value;
                this.OnPropertyChanged();
            }
        }

        // 序号
        private int? _no;

        public int? No
        {
            get
            {
                return _no;
            }
            set
            {
                _no = value;
                this.OnPropertyChanged();
            }
        }

        private bool _isConfirmed;
        public bool IsConfirmed
        {
            get
            {
                return _isConfirmed;
            }
            set
            {
                _isConfirmed = value;
                this.OnPropertyChanged();
            }
        }

        private string _institutionInit;

        public string InstitutionInit
        {
            get
            {
                return _institutionInit;
            }
            set
            {
                _institutionInit = value;
                this.OnPropertyChanged();
            }
        }

        private string _traderInit;

        public string TraderInit
        {
            get
            {
                return _traderInit;
            }
            set
            {
                _traderInit = value;
                this.OnPropertyChanged();
            }
        }
    }
}
