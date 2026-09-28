using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.ViewModel.UI
{
    public class MsgAlertModel : ViewModelBase
    {
        private string _id;
        /// <summary>
        /// ID
        /// </summary>
        public string id
        {
            get { return _id; }
            set
            {
                _id = value;
                this.OnPropertyChanged();
            }
        }

        private string _pid;
        /// <summary>
        /// ID
        /// </summary>
        public string pid
        {
            get { return _pid; }
            set
            {
                _pid = value;
                this.OnPropertyChanged();
            }
        }

        private string _code;
        /// <summary>
        /// 编码
        /// </summary>
        public string code
        {
            get { return _code; }
            set
            {
                _code = value;
                this.OnPropertyChanged();
            }
        }

        private string _name;
        /// <summary>
        /// 显示名称
        /// </summary>
        public string name
        {
            get { return _name; }
            set
            {
                _name = value;
                this.OnPropertyChanged();
            }
        }

        private bool _checked;
        //是否启用
        public bool ischecked
        {
            get { return _checked; }
            set
            {
                _checked = value;
                this.OnPropertyChanged();
            }
        }


        private bool _isenabled = true;
        //是否可用
        public bool isenabled
        {
            get { return _isenabled; }
            set
            {
                _isenabled = value;
                this.OnPropertyChanged();
            }
        }

        private bool _visible = true;
        //是否可见
        public bool visible
        {
            get { return _visible; }
            set
            {
                _visible = value;
                this.OnPropertyChanged();
            }
        }

        private bool _IsGroup = false;
        /// <summary>
        /// 是否是组
        /// </summary>
        public bool isgroup
        {
            get { return _IsGroup; }
            set
            {
                _IsGroup = value;
                this.OnPropertyChanged();
            }
        }

        private double _width;
        /// <summary>
        /// 字段的宽度
        /// </summary>
        public double Width
        {
            get { return _width; }
            set
            {
                _width = value;
                this.OnPropertyChanged();
            }
        }

        private List<MsgAlertModel> _subList;
        /// <summary>
        /// 数据列表
        /// </summary>
        public List<MsgAlertModel> subList
        {
            get { return _subList; }
            set
            {
                _subList = value;
                this.OnPropertyChanged();
            }
        }
    }
}
