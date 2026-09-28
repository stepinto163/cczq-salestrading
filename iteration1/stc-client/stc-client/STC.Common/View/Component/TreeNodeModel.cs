using STC.Common.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.View.Component
{
    public class TreeNodeModel : BaseViewModel
    {
        public TreeNodeModel()
        {
            _nodes = new ObservableCollection<TreeNodeModel>();
        }

        private ObservableCollection<TreeNodeModel> _nodes;
        public ObservableCollection<TreeNodeModel> Nodes
        {
            get { return _nodes; }
            set
            {
                this._nodes = value;
                this.OnPropertyChanged();
            }
        }

        private string _parentnode;
        /// <summary>
        /// 父级节点编号
        /// </summary>
        public string Parentnode
        {
            get { return _parentnode; }
            set { _parentnode = value; }
        }

        private TreeNodeModel _parent;
        [Newtonsoft.Json.JsonIgnore]
        public TreeNodeModel Parent
        {
            get { return _parent; }
            set
            {
                this._parent = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 组合id
        /// </summary>
        private string _id;
        public string Id
        {
            get { return _id; }
            set
            {
                this._id = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 组合名称
        /// </summary>
        private string _name;
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
        /// 用户id
        /// </summary>
        private string _userId;
        public string UserId
        {
            get { return _userId; }
            set
            {
                this._userId = value;
                this.OnPropertyChanged();
            }
        }
        /// <summary>
        /// 用户名
        /// </summary>
        private string _userName;
        public string UserName
        {
            get { return _userName; }
            set
            {
                this._userName = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 组合描述
        /// </summary>
        private string _remark;
        public string Remark
        {
            get { return _remark; }
            set
            {
                this._remark = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 组合类型
        /// </summary>
        private string _bookType;
        public string BookType
        {
            get { return _bookType; }
            set
            {
                this._bookType = value;
                this.OnPropertyChanged();
            }
        }


        private string _Checked = "0";
        /// <summary>
        /// 是否选择
        /// </summary>
        public string Checked
        {
            get { return _Checked; }
            set
            {
                this._Checked = value;
                this.OnPropertyChanged();
            }
        }


        private bool _isvisible = true;
        /// <summary>
        /// 是否可见
        /// </summary>
        public bool IsVisible
        {
            get { return _isvisible; }
            set
            {
                this._isvisible = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 是否有子节点
        /// </summary>
        public bool IsLeaf
        {
            get
            {
                return Nodes.Count > 0 ? false : true;
            }
        }

        private bool _isExpanded;
        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                if (_isExpanded != value)
                {
                    _isExpanded = value;
                    OnPropertyChanged(nameof(IsExpanded));
                }
            }
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                }
            }
        }


        /// <summary>
        /// 恒生基金编号
        /// </summary>
        private string _productid;
        public string ProductId
        {
            get { return _productid; }
            set
            {
                this._productid = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 是否是组
        /// </summary>
        public bool IsGroup
        {
            get
            {
                return Nodes.Count > 0  || (Nodes.Count==0 && string.IsNullOrWhiteSpace(Parentnode)) ? true : false;
            }
        }

    }
}
