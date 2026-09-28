using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDB.Common.ViewModel.UI
{
    public class MsgAlertDataModel : ViewModelBase
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
    }
}
