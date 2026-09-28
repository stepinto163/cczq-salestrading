using STC.Common.Model.Table;
using STC.Common.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace STC.Common.View.Component
{
    public class HierarchicalTreeItemModel : ViewModelBase
    {

        public HierarchicalTreeItemModel()
        {
            _nodes = new ObservableCollection<HierarchicalTreeItemModel>();
            _parent = null;
        }

        private ObservableCollection<HierarchicalTreeItemModel> _nodes;
        public ObservableCollection<HierarchicalTreeItemModel> Nodes
        {
            get { return _nodes; }
            set
            {
                this._nodes = value;
                this.OnPropertyChanged();
            }
        }

        private HierarchicalTreeItemModel _parent;
        public HierarchicalTreeItemModel Parent
        {
            get { return _parent; }
            set
            {
                this._parent = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 名称
        /// </summary>
        public string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                this._name = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 是否是叶子
        /// </summary>
        public bool _isLeaf;
        public bool IsLeaf
        {
            get { return _isLeaf; }
            set
            {
                this._isLeaf = value;
                this.OnPropertyChanged();
                if (value)
                {
                    CheckVisiable = Visibility.Visible;
                }
            }
        }

        /// <summary>
        /// 选择框是否可见
        /// </summary>
        public Visibility _checkVisiable = Visibility.Visible;
        public Visibility CheckVisiable
        {
            get { return _checkVisiable; }
            set
            {
                this._checkVisiable = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 是否选中
        /// </summary>
        public bool _isChecked;
        public bool IsChecked
        {
            get { return _isChecked; }
            set
            {
                this._isChecked = value;
                this.OnPropertyChanged();
            }
        }

        public ChildTag Info;

        private string _pinyin;
        public string PinYin
        {
            get
            {
                return _pinyin;
            }
            set
            {
                if (_pinyin != value)
                {
                    _pinyin = value;
                    this.OnPropertyChanged();
                }
            }
        }
    }
}
