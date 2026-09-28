using STC.Common.ViewModel;
using STC.Common.ViewModel.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace STC.XBond.Plugin
{
    /// <summary>
    /// 指定渠道配置项
    /// </summary>
    public class ListBoxItemData : ViewModelBase
    {
        private string _venuetypegw;
        public string Venuetypegw { get { return _venuetypegw; } set { _venuetypegw = value; } }
        private string _name;
        public string Name { get { return _name; } set { _name = value; this.OnPropertyChanged(); } }
        private string _code;
        public string Code { get { return _code; } set { _code = value; this.OnPropertyChanged(); } }
        private int _level;
        public int Level { get { return _level; } set { _level = value; this.OnPropertyChanged(); } }
        private int _seq;
        public int Seq { get { return _seq; } set { _seq = value; this.OnPropertyChanged(); } }
        private string _userVenue;
        public string UserVenue { get { return _userVenue; } set { _userVenue = value; this.OnPropertyChanged(); } }
        public SendPriceItem SendPriceItem { get; set; }
        private bool _checked;
        public bool Checked { get { return _checked; } set { _checked = value; this.OnPropertyChanged(); } }
        private Brush _bgColor;
        public Brush BgColor { get { return _bgColor; } set { _bgColor = value; this.OnPropertyChanged(); } }
        private bool _itemEnable;
        public bool ItemEnable { get { return _itemEnable; } set { _itemEnable = value; this.OnPropertyChanged(); } }

        private Visibility itemVisibility = Visibility.Visible;
        public Visibility ItemVisibility { get { return itemVisibility; } set { itemVisibility = value; this.OnPropertyChanged(); } }
    }

    /// <summary>
    /// 指定渠道项排序
    /// </summary>
    public class ListBoxItemSort : IComparer<ListBoxItemData>
    {
        public int Compare(ListBoxItemData x, ListBoxItemData y)
        {
            return x.Seq.CompareTo(y.Seq);
        }
    }
}
