using STC.Common.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace STC.Broker.Plugin.Model.Transaction.BridgeModel
{
    /// <summary>
    /// 发单机构视图模型
    /// </summary>
    public class BillingInstModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        /// <summary>
        /// 机构id
        /// </summary>
        public string Idbkey { get; set; }
        /// <summary>
        /// 简称
        /// </summary>
        public string _shortnamecn;

        /// <summary>
        /// 简称
        /// </summary>
        public string Shortnamecn
        {
            get { return _shortnamecn; }
            set
            {
                if (_shortnamecn != value)
                {
                    _shortnamecn = value;
                    NotifyPropertyChanged("Shortnamecn");
                }
            }
        }
        /// <summary>
        /// 机构代码
        /// </summary>
        public string instcode { get; set; }
        /// <summary>
        /// 机构发单数量
        /// </summary>
        public string _quantity;

        /// <summary>
        /// 机构发单数量
        /// </summary>
        public string Quantity
        {
            get { return _quantity; }
            set
            {
                if (_quantity != value)
                {
                    _quantity = value;
                    NotifyPropertyChanged("Quantity");
                }
            }
        }

        /// <summary>
        /// //创建人
        /// </summary>
        public string Createby { get; set; }
        /// <summary>
        /// ncdshortnamelist
        /// </summary>
        public string ncdshortnamelist { get; set; }
        /// <summary>
        /// irsshortnamelist
        /// </summary>
        public string irsshortnamelist { get; set; }

        /// <summary>
        /// 是否显示新增按钮
        /// </summary>
        private Visibility _addVisibility { get; set; } = Visibility.Visible;
        /// <summary>
        /// 是否显示新增按钮
        /// </summary>
        public Visibility AddVisibility
        {
            get { return _addVisibility; }
            set
            {
                if (_addVisibility != value)
                {
                    _addVisibility = value;
                    NotifyPropertyChanged("AddVisibility");
                }
            }
        }

        /// <summary>
        /// 是否显示新增按钮
        /// </summary>
        private Visibility _minusVisibility { get; set; } = Visibility.Visible;
        /// <summary>
        /// 是否显示删除按钮
        /// </summary>
        public Visibility MinusVisibility
        {
            get { return _minusVisibility; }
            set
            {
                if (_minusVisibility != value)
                {
                    _minusVisibility = value;
                    NotifyPropertyChanged("MinusVisibility");
                }
            }
        }

        /// <summary>
        /// 是否显示新增按钮
        /// </summary>
        private SolidColorBrush tbBordBrush  = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#193D37"));
        /// <summary>
        /// 数量框颜色
        /// </summary>
        public SolidColorBrush TbBordBrush
        {
            get { return tbBordBrush; }
            set
            {
                if (tbBordBrush != value)
                {
                    tbBordBrush = value;
                    NotifyPropertyChanged("TbBordBrush");
                }
            }
        }


        /// <summary>
        /// 发单机构状态
        /// </summary>
        public string SendStatus { get; set; }

        /// <summary>
        /// 原因
        /// </summary>
        public string SendStatusreason { get; set; }
    }
}
