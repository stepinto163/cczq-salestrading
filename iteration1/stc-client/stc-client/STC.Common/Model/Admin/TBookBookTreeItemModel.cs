using STC.Common.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace STC.Common.Model.Admin
{
    public class TBookBookTreeItemModel : BaseViewModel
    {
        public TBookBookTreeItemModel()
        {
            _nodes = new ObservableCollection<TBookBookTreeItemModel>();
            _parent = null;
        }

        private ObservableCollection<TBookBookTreeItemModel> _nodes;
        public ObservableCollection<TBookBookTreeItemModel> Nodes
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

        private TBookBookTreeItemModel _parent;

        [Newtonsoft.Json.JsonIgnore]
        public TBookBookTreeItemModel Parent
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
        public string _id;
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
        /// 用户id
        /// </summary>
        public string _userId;
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
        public string _userName;
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
        public string _remark;
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
        public string _bookType;
        public string BookType
        {
            get { return _bookType; }
            set
            {
                this._bookType = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 映射的外部系统信息
        /// </summary>
        public string _external;
        public string External
        {
            get { return _external; }
            set
            {
                this._external = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 树的第几级节点（1，2，3，4）
        /// </summary>
        public int _index = 0;
        public int Index
        {
            get { return _index; }
            set
            {
                this._index = value;
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

        /// <summary>
        /// 是否是子节点
        /// </summary>
        public bool IsLeaf
        {
            get 
            {
                return Nodes.Count > 0 ? false : true;
            }
        }

        /// <summary>
        /// 恒生基金编号
        /// </summary>
        public string _productid;
        public string ProductId
        {
            get { return _productid; }
            set
            {
                this._productid = value;
                this.OnPropertyChanged();
            }
        }
    }
}
