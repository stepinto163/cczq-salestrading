using com.gateway.log;
using Newtonsoft.Json;
using STC.Common.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace STC.IRS.Plugin.ViewModel.Data
{
    public class FilterGroupItem : BaseViewModel
    {
        private string _action = "0";
        /// <summary>
        /// 查询：action=0，username必传
        /// 新增：action=1，id不传，username、groupname必传
        /// 修改：action=2，id、username必传，其余选传
        /// 删除：action=3，id必传
        /// </summary>
        public string Aciton
        {
            get { return _action; }
            set
            {
                if (_action != value)
                {
                    _action = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _groupID;

        public string GroupID
        {
            get { return _groupID; }
            set { _groupID = value; }
        }

        private string _groupName;

        public string GroupName
        {
            get { return _groupName; }
            set {
                if (_groupName != value)
                {
                    _groupName = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _groupName_Edit;

        public string GroupName_Edit
        {
            get { return _groupName_Edit; }
            set
            {
                if (_groupName_Edit != value)
                {
                    
                    _groupName_Edit = value;
                    SetAction();
                    this.OnPropertyChanged();
                }
            }
        }

        private int _ordersn;

        public int Ordersn
        {
            get { return _ordersn; }
            set { _ordersn = value; }
        }

        private string _userName;

        public string UserName
        {
            get { return _userName; }
            set { _userName = value; }
        }

        private string _terminal;

        public string Terminal
        {
            get { return _terminal; }
            set { _terminal = value; }
        }

        private string _ip;
        public string IP
        {
            get { return _ip; }
            set { _ip = value; }
        }

        private string _closeby;

        public string CloseBy
        {
            get { return _closeby; }
            set { _closeby = value; }
        }

        private string _remark;

        public string Remark
        {
            get { return _remark; }
            set {
                _remark = value;
                this.OnPropertyChanged();
            }
        }

        private string _createTime;

        public string CreateTime
        {
            get { return _createTime; }
            set { _createTime = value; }
        }

        private string _updateTime;

        public string UpdateTime
        {
            get { return _updateTime; }
            set { _updateTime = value; }
        }

        /// <summary>
        /// 分类方式，TAG(标签）/BOND(债券)
        /// </summary>
        private string _Inf1;

        public string Inf1
        {
            get { return _Inf1; }
            set { 
                if (_Inf1 != value)
                {
                    _Inf1 = value;
                    if (value == "BOND")
                    {
                        TagPanelVisibility = Visibility.Collapsed;
                        BondPanelVisibility = Visibility.Visible;
                    }
                    else
                    {
                        TagPanelVisibility = Visibility.Visible;
                        BondPanelVisibility = Visibility.Collapsed;
                    }
                    this.OnPropertyChanged();
                }
            }
        }

        private Visibility _TagPanelVisibility = Visibility.Collapsed;

        public Visibility TagPanelVisibility
        {
            get { return _TagPanelVisibility; }
            set
            {
                if (_TagPanelVisibility != value)
                {
                    _TagPanelVisibility = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private Visibility _BondPanelVisibility = Visibility.Collapsed;

        public Visibility BondPanelVisibility
        {
            get { return _BondPanelVisibility; }
            set
            {
                if (_BondPanelVisibility != value)
                {
                    _BondPanelVisibility = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 债券ID
        /// </summary>
        private string _Inf2;

        public string Inf2
        {
            get { return _Inf2; }
            set { _Inf2 = value; }
        }

        /// <summary>
        /// 关注债券方向，0:All,1:Buy,2:Sell
        /// </summary>
        private string _Inf3;

        public string Inf3
        {
            get { return _Inf3; }
            set { _Inf3 = value; }
        }

        private string _focusTags;

        public string FocusTags
        {
            get { return _focusTags; }
            set
            {
                if (_focusTags != value)
                {
                    _focusTags = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _focusTags_Edit;

        public string FocusTags_Edit
        {
            get { return _focusTags_Edit; }
            set
            {
                if (_focusTags_Edit != value)
                {
                    _focusTags_Edit = value;
                    if (!string.IsNullOrEmpty(_focusTags_Edit))
                    {
                        try
                        {
                            FocusTagsDic_Edit = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(_focusTags_Edit);
                        }
                        catch (Exception e)
                        {
                            LogHelper.Error(e);
                        }
                    }
                    SetAction();
                    this.OnPropertyChanged();
                }
            }
        }

        private bool _isAppTerm;

        public bool IsAppTerm
        {
            get { return _isAppTerm; }
            set
            {
                _isAppTerm = value;
                this.OnPropertyChanged();
            }
        }

        private string _startTermDY;

        public string StartTermDY
        {
            get { return _startTermDY; }
            set
            {
                _startTermDY = value;
                this.OnPropertyChanged();
            }
        }

        private string _startTermDY_Edit;

        public string StartTermDY_Edit
        {
            get { return _startTermDY_Edit; }
            set
            {
                _startTermDY_Edit = value;
                SetAction();
                this.OnPropertyChanged();
            }
        }

        private string _endTermDY;

        public string EndTermDY
        {
            get { return _endTermDY; }
            set
            {
                _endTermDY = value;
                this.OnPropertyChanged();
            }
        }

        private string _endTermDY_Edit;

        public string EndTermDY_Edit
        {
            get { return _endTermDY_Edit; }
            set
            {
                _endTermDY_Edit = value;
                SetAction();
                this.OnPropertyChanged();
            }
        }

        private string _startCoupon;

        public string StartCoupon
        {
            get { return _startCoupon; }
            set
            {
                _startCoupon = value;
                this.OnPropertyChanged();
            }
        }

        private string _startCoupon_Edit;

        public string StartCoupon_Edit
        {
            get { return _startCoupon_Edit; }
            set
            {
                _startCoupon_Edit = value;
                SetAction();
                this.OnPropertyChanged();
            }
        }

        private string _endCoupon;

        public string EndCoupon
        {
            get { return _endCoupon; }
            set
            {
                _endCoupon = value;
                this.OnPropertyChanged();
            }
        }

        private string _endCoupon_Edit;

        public string EndCoupon_Edit
        {
            get { return _endCoupon_Edit; }
            set
            {
                _endCoupon_Edit = value;
                SetAction();
                this.OnPropertyChanged();
            }
        }

        private bool _isCoupon;

        public bool IsCoupon
        {
            get { return _isCoupon; }
            set
            {
                _isCoupon = value;
                this.OnPropertyChanged();
            }
        }

        private bool _isCoupon_Edit;

        public bool IsCoupon_Edit
        {
            get { return _isCoupon_Edit; }
            set
            {
                _isCoupon_Edit = value;
                SetAction();
                this.OnPropertyChanged();
            }
        }

        private string _startPrice;

        public string StartPrice
        {
            get { return _startPrice; }
            set
            {
                _startPrice = value;
                this.OnPropertyChanged();
            }
        }

        private string _startPrice_Edit;

        public string StartPrice_Edit
        {
            get { return _startPrice_Edit; }
            set
            {
                _startPrice_Edit = value;
                SetAction();
                this.OnPropertyChanged();
            }
        }

        private string _endPrice;

        public string EndPrice
        {
            get { return _endPrice; }
            set
            {
                _endPrice = value;
                this.OnPropertyChanged();
            }
        }

        private string _endPrice_Edit;

        public string EndPrice_Edit
        {
            get { return _endPrice_Edit; }
            set
            {
                _endPrice_Edit = value;
                SetAction();
                this.OnPropertyChanged();
            }
        }

        private bool _isPrice;

        public bool IsPrice
        {
            get { return _isPrice; }
            set
            {
                _isPrice = value;
                this.OnPropertyChanged();
            }
        }

        private bool _isPrice_Edit;

        public bool IsPrice_Edit
        {
            get { return _isPrice_Edit; }
            set
            {
                _isPrice_Edit = value;
                SetAction();
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 是否订阅 0:不订阅，1:订阅
        /// </summary>
        private bool _ifSubscribe;

        public bool IfSubscribe
        {
            get { return _ifSubscribe; }
            set
            {
                if (_ifSubscribe != value)
                {
                    _ifSubscribe = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 是否订阅 0:不订阅，1:订阅
        /// </summary>
        private bool _ifSubscribe_Edit;

        public bool IfSubscribe_Edit
        {
            get { return _ifSubscribe_Edit; }
            set
            {
                if (_ifSubscribe_Edit != value)
                {
                    _ifSubscribe_Edit = value;
                    SetAction();
                    this.OnPropertyChanged();
                }
            }
        }

        //private Dictionary<string, List<string>> _focusTagsDic = new Dictionary<string, List<string>>();

        public Dictionary<string, List<string>> FocusTagsDic_Edit { get; set; }

        /// <summary>
        /// 判断是否更新了记录
        /// </summary>
        private void SetAction()
        {
            if (this.Aciton != "1" && this.Aciton != "3")
            {
                if (this.GroupName_Edit != this.GroupName
                    || this.IfSubscribe_Edit != this.IfSubscribe
                    || this.FocusTags_Edit != this.FocusTags
                    || this.IsCoupon_Edit != this.IsCoupon
                    || this.StartCoupon_Edit != this.StartCoupon
                    || this.EndCoupon_Edit != this.EndCoupon
                    || this.StartTermDY_Edit != this.StartTermDY
                    || this.EndTermDY != this.EndTermDY
                    || this.IsPrice_Edit != this.IsPrice
                    || this.StartPrice_Edit != this.StartPrice
                    || this.EndPrice_Edit != this.EndPrice)
                {
                    this.Aciton = "2";
                }
                else
                {
                    this.Aciton = "0";
                }
            }
        }
    }
}
