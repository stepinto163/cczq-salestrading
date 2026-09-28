using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.MM
{
    public class UserSetting
    {
        private String tip1Enable;//	09:25前未达标提醒
        private String tip2Enable;//	09:25-09:30未达标提醒
        private String tip3Enable;//	盘中断报监控提醒
        private String tip4Enable;//	09:29做市义务确认，声音提醒
        private String bncMoniEnable;//	利率债最优点差监控启用
        private String bncMoniValue;//	利率债最优点差监控值
        private String bcoMoniEnable;//	信用债最优点差监控启用
        private String bcoMoniValue;//	信用债最优点差监控值
        private String bncWidenAddValue;//	利率债双边拉宽使得买卖价差增加
        private String bncWidenSubValue;//	利率债双边收窄使得买卖价差减少
        private String bncWidenSamValue;//	利率债双边同向增减使得报价每次变化
        private String bncWidenSigValue;//	利率债单边增减使得报价每次变化
        private String bcoWidenAddValue;//	信用债双边拉宽使得买卖价差增加
        private String bcoWidenSubValue;//	信用债双边收窄使得买卖价差减少
        private String bcoWidenSamValue;//	信用债双边同向增减使得报价每次变化
        private String dealCloseAll;//	出现成交当前用户全部策略全撤全停 0-不启用 1-启用
        private String bookId;//组合id
        private String bookName;//组合名称
        private List<DealCloseGroupPo> dealCloseGroups;//	出现成交当前用户某一分组内策略自动关闭并撤单

        public string Tip1Enable
        {
            get
            {
                return tip1Enable;
            }

            set
            {
                tip1Enable = value;
            }
        }

        public string Tip2Enable
        {
            get
            {
                return tip2Enable;
            }

            set
            {
                tip2Enable = value;
            }
        }

        public string Tip3Enable
        {
            get
            {
                return tip3Enable;
            }

            set
            {
                tip3Enable = value;
            }
        }
        public string Tip4Enable
        {
            get
            {
                return tip4Enable;
            }

            set
            {
                tip4Enable = value;
            }
        }
        public string BncMoniEnable
        {
            get
            {
                return bncMoniEnable;
            }

            set
            {
                bncMoniEnable = value;
            }
        }

        public string BncMoniValue
        {
            get
            {
                return bncMoniValue;
            }

            set
            {
                bncMoniValue = value;
            }
        }

        public string BcoMoniEnable
        {
            get
            {
                return bcoMoniEnable;
            }

            set
            {
                bcoMoniEnable = value;
            }
        }

        public string BcoMoniValue
        {
            get
            {
                return bcoMoniValue;
            }

            set
            {
                bcoMoniValue = value;
            }
        }

        public string BncWidenAddValue
        {
            get
            {
                return bncWidenAddValue;
            }

            set
            {
                bncWidenAddValue = value;
            }
        }

        public string BncWidenSubValue
        {
            get
            {
                return bncWidenSubValue;
            }

            set
            {
                bncWidenSubValue = value;
            }
        }

        public string BncWidenSamValue
        {
            get
            {
                return bncWidenSamValue;
            }

            set
            {
                bncWidenSamValue = value;
            }
        }

        public string BncWidenSigValue
        {
            get
            {
                return bncWidenSigValue;
            }

            set
            {
                bncWidenSigValue = value;
            }
        }

        public string BcoWidenAddValue
        {
            get
            {
                return bcoWidenAddValue;
            }

            set
            {
                bcoWidenAddValue = value;
            }
        }

        public string BcoWidenSubValue
        {
            get
            {
                return bcoWidenSubValue;
            }

            set
            {
                bcoWidenSubValue = value;
            }
        }

        public string BcoWidenSamValue
        {
            get
            {
                return bcoWidenSamValue;
            }

            set
            {
                bcoWidenSamValue = value;
            }
        }

        public string DealCloseAll
        {
            get
            {
                return dealCloseAll;
            }

            set
            {
                dealCloseAll = value;
            }
        }

        public List<DealCloseGroupPo> DealCloseGroups
        {
            get
            {
                return dealCloseGroups;
            }

            set
            {
                dealCloseGroups = value;
            }
        }

        public string BookId
        {
            get
            {
                return bookId;
            }

            set
            {
                bookId = value;
            }
        }
        public string BookName
        {
            get
            {
                return bookName;
            }

            set
            {
                bookName = value;
            }
        }
    }

    public class DealCloseGroupPo
    {
        private string groupId;//分组id
        private string groupName;//分组名字
        private string dealCloseGroup;//出现成交当前用户当前分组全撤全停 0-不启用 1-启用
        private bool groupCheckEnable;//是否可编辑

        public string GroupId
        {
            get
            {
                return groupId;
            }

            set
            {
                groupId = value;
            }
        }

        public string GroupName
        {
            get
            {
                return groupName;
            }

            set
            {
                groupName = value;
            }
        }

        public bool GroupCheckEnable
        {
            get
            {
                return groupCheckEnable;
            }

            set
            {
                groupCheckEnable = value;
            }
        }

        public string DealCloseGroup
        {
            get
            {
                return dealCloseGroup;
            }

            set
            {
                dealCloseGroup = value;
            }
        }
    }
}
