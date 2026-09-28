using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Admin
{
    public class HomeGroupData : BaseViewModel
    {
        private string _closeby;
        public string closeby
        {
            get { return _closeby; }
            set
            {
                _closeby = value;

                this.OnPropertyChanged();
            }
        }
        private string _createtime;
        public string createtime
        {
            get { return _createtime; }
            set
            {
                _createtime = value;

                this.OnPropertyChanged();
            }
        }
        private string _groupname;
        public string groupname
        {
            get { return _groupname; }
            set
            {
                _groupname = value;

                this.OnPropertyChanged();
            }
        }
        private string _id;
        public string id
        {
            get { return _id; }
            set
            {
                _id = value;

                this.OnPropertyChanged();
            }
        }
        private int _ordersn;
        public int ordersn
        {
            get { return _ordersn; }
            set
            {
                _ordersn = value;

                this.OnPropertyChanged();
            }
        }
        private string _username;
        public string username
        {
            get { return _username; }
            set
            {
                _username = value;

                this.OnPropertyChanged();
            }
        }
        private string _updatetime;
        public string updatetime
        {
            get { return _updatetime; }
            set
            {
                _updatetime = value;

                this.OnPropertyChanged();
            }
        }
        private string _inf1;
        public string inf1
        {
            get { return _inf1; }
            set
            {
                _inf1 = value;

                this.OnPropertyChanged();
            }
        }
        private string _inf2;
        public string inf2
        {
            get { return _inf2; }
            set
            {
                _inf2 = value;

                this.OnPropertyChanged();
            }
        }
        private List<HomeGroupSecurityIDsInfo> _securityids;
        public List<HomeGroupSecurityIDsInfo> securityids
        {
            get { return _securityids; }
            set
            {
                _securityids = value;

                this.OnPropertyChanged();
            }
        }

        private Dictionary<string, string> _tagDic;
        public Dictionary<string, string> tagDic
        {
            get { return _tagDic; }
            set
            {
                _tagDic = value;

                this.OnPropertyChanged();
            }
        }

        private GroupMsgPo _groupMsgPo;
        public GroupMsgPo groupMsgPo
        {
            get { return _groupMsgPo; }
            set
            {
                _groupMsgPo = value;

                this.OnPropertyChanged();
            }
        }

        private string _IsMyselfGroup;
        public string IsMyselfGroup
        {
            get { return _IsMyselfGroup; }
            set
            {
                _IsMyselfGroup = value;

                this.OnPropertyChanged();
            }
        }

        private string _IsHomeTag;
        public string IsHomeTag
        {
            get { return _IsHomeTag; }
            set
            {
                _IsHomeTag = value;

                this.OnPropertyChanged();
            }
        }

        private string _IsINNERTag;
        public string IsINNERTag
        {
            get { return _IsINNERTag; }
            set
            {
                _IsINNERTag = value;

                this.OnPropertyChanged();
            }
        }

        private string _IsQuantTag;
        public string IsQuantTag
        {
            get { return _IsQuantTag; }
            set
            {
                _IsQuantTag = value;

                this.OnPropertyChanged();
            }
        }

        private string _IsMMTag;
        public string IsMMTag
        {
            get { return _IsMMTag; }
            set
            {
                _IsMMTag = value;

                this.OnPropertyChanged();
            }
        }

        private string _IsMMSHETag;
        public string IsMMSHETag
        {
            get { return _IsMMSHETag; }
            set
            {
                _IsMMSHETag = value;

                this.OnPropertyChanged();
            }
        }

        public static HomeGroupData Copy(HomeGroupData po) 
        {
            HomeGroupData result = new HomeGroupData();
            result.id = po.id;
            result.closeby = po.closeby;
            result.createtime = po.createtime;
            result.groupname = po.groupname;
            result.updatetime = po.updatetime;
            result.username = po.username;
            result.inf1 = po.inf1;
            result.inf2 = po.inf2;
            result.securityids = po.securityids;
            result.tagDic = po.tagDic;
            result.groupMsgPo = po.groupMsgPo;
            result.IsMyselfGroup = po.IsMyselfGroup;
            return result;
        }
    }
}
