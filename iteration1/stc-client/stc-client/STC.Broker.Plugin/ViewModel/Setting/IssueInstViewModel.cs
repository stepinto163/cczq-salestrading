using STC.Common.Model;
using STC.Common.Model.PT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Broker.Plugin.ViewModel.Setting
{
    public class IssueInstViewModel : BaseViewModel
    {
        public IssueInstViewModel(IssueInst issueInst)
        {
            InstShortName = issueInst.instShortName;
            InstFullName = issueInst.instFullName;
            InstId = issueInst.instId;
            GroupId = issueInst.groupId;
            Id = issueInst.id;
            IsDefualtRequestSelect = issueInst.defaultRequest == "1" ? true : false;
        }



        private string instShortName;
        public string InstShortName
        {
            get 
            { 
                return instShortName;
            }
            set 
            { 
                instShortName = value; 
                this.OnPropertyChanged();
            }
        }

        private string instFullName;
        public string InstFullName
        {
            get
            {
                return instFullName;
            }
            set
            {
                instFullName = value;
                this.OnPropertyChanged();
            }
        }


        private bool isSelect;
        public bool IsSelect
        {
            get
            {
                return isSelect;
            }
            set
            {
                isSelect = value;
                this.OnPropertyChanged();
            }
        }


        public string InstId { get; set; }


        public string GroupId { get; set; }

        public long Id { get; set; }

        /// <summary>
        /// 默认请求
        /// </summary>
        private bool isDefualtRequestSelect;
        /// <summary>
        ///  默认请求
        /// </summary>
        public bool IsDefualtRequestSelect
        {
            get
            {
                return isDefualtRequestSelect;
            }
            set
            {
                isDefualtRequestSelect = value;
                this.OnPropertyChanged();
            }
        }
    }
}
