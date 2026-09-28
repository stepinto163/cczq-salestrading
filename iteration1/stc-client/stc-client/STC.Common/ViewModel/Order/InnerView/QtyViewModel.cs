using STC.Common.Model;
using STC.Common.Model.Base;
using STC.Common.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace STC.Common.ViewModel.Order.InnerView
{
    public class QtyViewModel : BaseViewModel
    {

        public const string TXT_KW = "kw";
        //private string Txt_W = "w";
        public const string TXT_IRS = "百万";
        
        public const string TXT_W = "w";
        /// <summary>
        /// 量是否只读
        /// </summary>
        public bool isreadonly = false;
        public bool IsReadOnly
        {
            get { return isreadonly; }
            set
            {
                isreadonly = value;
                IsBtnEnable = !value;
                this.OnPropertyChanged("IsReadOnly");
            }
        }

        /// <summary>
        /// 控制量的按钮是否可用
        /// </summary>
        public bool isbtnenable = true;
        public bool IsBtnEnable
        {
            get { return isbtnenable; }
            set
            {
                isbtnenable = value;
                this.OnPropertyChanged("IsBtnEnable");
            }
        }

        #region 界面属性

        private string qtyTxt = "kw";

        public string QtyTxt
        {
            get { return qtyTxt; }
            set
            {
                qtyTxt = value;
                this.OnPropertyChanged();
            }
        }

        private int setAmountTxtIndex = -1;

        public int SetAmountTxtIndex
        {
            get
            {
                return setAmountTxtIndex;
            }

            set
            {
                setAmountTxtIndex = value;
                if (setAmountTxtIndex == 0)
                {
                    OnOneClick();
                }
                else if (setAmountTxtIndex == 1)
                {
                    OnThreeClick();
                }
                else if (setAmountTxtIndex == 2)
                {
                    OnFiveClick();
                }
                this.OnPropertyChanged();
            }
        }

        // 显示用
        private string amount;
        public string Amount
        {
            get { return amount; }
            set
            {
                if (value != null && !string.IsNullOrEmpty(value))
                {
                    amount = value;

                    if (qtyTxt == TXT_IRS)
                    {
                        _trueAmount = decimal.Parse(value) * 100;
                    }
                    else if (qtyTxt == TXT_W)
                    {
                        _trueAmount = decimal.Parse(value) * 1;
                    }
                    else
                    {
                        _trueAmount = decimal.Parse(value) * 1000;
                    }
                }
                else
                {
                    amount = string.Empty;

                    _trueAmount = null;
                }
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 万为单位，实际值
        /// </summary>
        private decimal? _trueAmount;

        public decimal? TrueAmount
        {
            get
            {
                return _trueAmount;
            }
            set
            {
                this._trueAmount = value;

                if (value.HasValue)
                {
                    if (QtyTxt == TXT_IRS)
                    {
                        this.Amount = (_trueAmount.Value / 100).ToString("#");
                    }
                    else if (QtyTxt == TXT_W)
                    {
                        this.Amount = (_trueAmount.Value / 1).ToString("#");
                    }
                    else
                    {
                        this.Amount = (_trueAmount.Value / 1000).ToString("0.#");
                    }
                }
                else
                {
                    this.Amount = string.Empty;
                }
            }
        }

        private Brush borderBH;
        public Brush BorderBH
        {
            get { return borderBH; }
            set
            {
                if (value != null)
                {
                    borderBH = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private Brush qtyBorderBH;
        public Brush QtyBorderBH
        {
            get { return qtyBorderBH; }
            set
            {
                if (value != null)
                {
                    qtyBorderBH = value;
                    this.OnPropertyChanged();
                }
            }
        }

        // 显示用
        private string _SecurityID = "";
        public string SecurityID
        {
            get { return _SecurityID; }
            set
            {
                _SecurityID = value;
                this.OnPropertyChanged();
            }
        }
        

        // 显示用
        private string _OneShowAmount = "2";
        public string OneShowAmount
        {
            get { return _OneShowAmount; }
            set
            {
                _OneShowAmount = value;
                this.OnPropertyChanged();
            }
        }

        private string _TwoShowAmount = "3";
        public string TwoShowAmount
        {
            get { return _TwoShowAmount; }
            set
            {
                _TwoShowAmount = value;
                this.OnPropertyChanged();
            }
        }

        private string _FiveShowAmount = "5";
        public string FiveShowAmount
        {
            get { return _FiveShowAmount; }
            set
            {
                _FiveShowAmount = value;
                this.OnPropertyChanged();
            }
        }


        #endregion

        public ICommand AddAmountCommand { get { return new ComRelayCommand(AddAmountClick); } }
        public ICommand ReduceAmountCommand { get { return new ComRelayCommand(ReduceAmountClick); } }


        /// <summary>
        /// 下单量1000
        /// </summary>
        private void OnOneClick()
        {
            if (LocalEnv.ConfInfo.IsPAYL)
            {
                Amount = "2";
            }
            else
            {
                if (!CommonUtils.IsSheBond(SecurityID))
                    Amount = "2";
                else
                    Amount = "20";
            }
        }

        /// <summary>
        /// 下单量3000
        /// </summary>
        private void OnThreeClick()
        {
            if (LocalEnv.ConfInfo.IsPAYL)
            {
                Amount = "3";
            }
            else
            {
                if (!CommonUtils.IsSheBond(SecurityID))
                    Amount = "3";
                else
                    Amount = "30";
            }
        }

        /// <summary>
        /// 下单量5000
        /// </summary>
        private void OnFiveClick()
        {
            if (LocalEnv.ConfInfo.IsPAYL)
            {
                Amount = "5";
            }
            else
            {
                if (!CommonUtils.IsSheBond(SecurityID))
                    Amount = "5";
                else
                    Amount = "50";
            }
        }


        private void AddAmountClick()
        {
            decimal amount = 0;
            if (decimal.TryParse(Amount, out amount))
            {
                if (QtyTxt == TXT_IRS)
                {
                    amount += 1M;
                    if (amount <= 10000)
                    {
                        Amount = amount.ToString();
                    }
                }
                else if (QtyTxt == TXT_W)
                {
                    amount += 1M;
                    if (amount <= 100000000)
                    {
                        Amount = amount.ToString();
                    }
                }
                else
                {
                    if (LocalEnv.ConfInfo.IsPAYL)
                    {
                        amount += 1M;
                    }
                    else
                    {
                        amount += 0.1M;
                    }
                    
                    if (amount <= 1000)
                    {
                        Amount = amount.ToString();
                    }
                }
            }
            else
            {
                Amount = "1";
            }

            SetAmountTxtSelectIndex("1");
        }

        private void ReduceAmountClick()
        {
            decimal amount = 0;
            if (decimal.TryParse(Amount, out amount))
            {
                if (QtyTxt == TXT_IRS)
                {
                    amount -= 1M;
                }
                else if (QtyTxt == TXT_W)
                {
                    amount -= 1M;
                }
                else
                {
                    if (LocalEnv.ConfInfo.IsPAYL)
                    {
                        amount -= 1M;
                    }
                    else
                    {
                        amount -= 0.1M;
                    }
                        
                }
                if (amount >= 0)
                {
                    Amount = amount.ToString();
                }
            }
            else
            {
                Amount = "5";
            }

            SetAmountTxtSelectIndex("5");
        }

        public void SetAmountTxtSelectIndex(string txt)
        {
            if (CommonUtils.IsSheBond(SecurityID))
            {
                List<string> valueList = new List<string>() { "20", "30", "50" };
                if (!valueList.Contains(Amount))
                {
                    SetAmountTxtIndex = -1;
                }
                else
                {
                    int tem2 = valueList.FindLastIndex(x => x == Amount);
                    SetAmountTxtIndex = tem2;
                }
            }
            else
            {
                List<string> valueList = new List<string>() { "2", "3", "5" };
                if (!valueList.Contains(Amount))
                {
                    SetAmountTxtIndex = -1;
                }
                else
                {
                    int tem2 = valueList.FindLastIndex(x => x == Amount);
                    SetAmountTxtIndex = tem2;
                }
            }
        }

        public void Init(string securityID, bool isBillon = false)
        {
            SecurityID = securityID;

            if (LocalEnv.ConfInfo.IsPAYL)
            {
                QtyTxt = TXT_KW;
                OneShowAmount = "2";
                TwoShowAmount = "3";
                FiveShowAmount = "5";
                return;
            }

            var x = CommonUtils.GetBondsInfoPoBySecurityID(securityID);
            if (isBillon)
            {
                QtyTxt = TXT_IRS;
            }
            else
            {
                if (x != null && (x as BondsInfoPo).MarketIndicator == "2")
                {
                    QtyTxt = TXT_IRS;
                    OneShowAmount = "2";
                    TwoShowAmount = "3";
                    FiveShowAmount = "5";
                }
                else if (CommonUtils.IsSheBond(securityID))
                {
                    QtyTxt = TXT_W;
                    OneShowAmount = "20";
                    TwoShowAmount = "30";
                    FiveShowAmount = "50";
                }
                else
                {
                    QtyTxt = TXT_KW;
                    OneShowAmount = "2";
                    TwoShowAmount = "3";
                    FiveShowAmount = "5";
                }
            }
        }

        /// <summary>
        /// 获取显示区的值
        /// </summary>
        /// <returns></returns>
        public decimal GetAmount()
        {
            decimal d = 0;
            decimal.TryParse(amount, out d);
            return d;
        }

        //public decimal GetSourceAmount()
        //{
        //    decimal d = 0;
        //    decimal.TryParse(SourceAmount, out d);
        //    decimal res = 0;
        //    if (IsBrokerSecurity)
        //    {
        //        if (IsIRS)
        //        {
        //            //IRS
        //            res = d * 100;
        //        }
        //        else
        //        {
        //            //现券
        //            res = d * 1000;
        //        }
        //    }
        //    else
        //    {
        //        //银行间
        //        res = d;
        //    }

        //    return res;
        //}
    }
}
