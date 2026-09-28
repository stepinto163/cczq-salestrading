using STC.Common.Handler.Base;
using STC.Common.Model;
using STC.Common.Model.CRM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static STC.Common.View.Component.AutoCompleteComboBox;

namespace STC.Common.ViewModel.Order.InnerPanel
{
    public class TagViewModel : BaseViewModel
    {


        /// <summary>
        /// 0：Bid
        /// 1: Ofr
        /// </summary>
        private string side;
        public string Side
        {
            get { return side; }
            set
            {
                side = value;


                if (side == "0")
                {
                    SyntropyTxt = "Bid";
                    ReverseTxt = "Ofr";

                    CheckSyntropy = false;
                    CheckReverse = true;
                }
                else
                {
                    SyntropyTxt = "Ofr";
                    ReverseTxt = "Bid";


                    CheckSyntropy = false;
                    CheckReverse = true;
                }
            }
        }


        private string syntropyTxt;
        public string SyntropyTxt
        {
            get { return syntropyTxt; }
            set { syntropyTxt = value; this.OnPropertyChanged(); }
        }

        private string reverseTxt;
        public string ReverseTxt
        {
            get { return reverseTxt; }
            set { reverseTxt = value; this.OnPropertyChanged(); }
        }


        private string securityID;
        public string SecurityID
        {
            get { return securityID; }
            set { securityID = value; this.OnPropertyChanged(); }
        }


        private bool checkClientType;
        public bool CheckClientType
        {
            get { return checkClientType; }
            set { checkClientType = value; this.OnPropertyChanged(); }
        }
        


        private string clientTypeEnum;
        public string ClientTypeEnum
        {
            get { return clientTypeEnum; }
            set { clientTypeEnum = value; this.OnPropertyChanged(); }
        }


        private CrmInst _selectedInstitution_Edit;

        public CrmInst SelectedInstintuion_Edit
        {
            get { return _selectedInstitution_Edit; }
            set
            {
                if (_selectedInstitution_Edit != value)
                {
                    _selectedInstitution_Edit = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string clientType;
        public string ClientType
        {
            get { return clientType; }
            set { clientType = value; this.OnPropertyChanged(); }
        }


        private bool checkVIP;
        public bool CheckVIP
        {
            get { return checkVIP; }
            set { checkVIP = value; this.OnPropertyChanged(); }
        }

        /// <summary>
        /// 该券同向
        /// </summary>
        private bool checkSyntropy;
        public bool CheckSyntropy
        {
            get { return checkSyntropy; }
            set { checkSyntropy = value; this.OnPropertyChanged(); }
        }

        /// <summary>
        /// 该券反向
        /// </summary>
        private bool checkReverse;
        public bool CheckReverse
        {
            get { return checkReverse; }
            set { checkReverse = value; this.OnPropertyChanged(); }
        }


        public TagData GetTagData()
        {
            TagData tagData = new TagData();

            if (CheckVIP)
            {
                tagData.VIP = "0001";
            }
            else
            {
                tagData.VIP = "0010";
            }

            tagData.ORG_TYPE = checkClientType ? SelectedInstintuion_Edit !=null ? SelectedInstintuion_Edit.instcode:string.Empty : string.Empty;

            if (checkSyntropy && !CheckReverse)
            {
                tagData.IF_SUB_BOND = Side == "0" ? "BID" : "OFR";
            }
            else if (!checkSyntropy && CheckReverse)
            {
                tagData.IF_SUB_BOND = Side == "0" ? "OFR" : "BID";
            }
            else if (checkSyntropy && CheckReverse)
            {
                tagData.IF_SUB_BOND = "ALL";
            }
            else
            {
                tagData.IF_SUB_BOND = string.Empty;
            }

            return tagData;
        }


        internal void FilterInstitution(AutoCompleteArgs args)
        {
            List<CrmInst> result = new List<CrmInst>();
            Dictionary<string, Model.Table.QuoteTag> dic = BaseHandler.Instance.GetQuoteTagDic();

            if (dic != null && dic.ContainsKey("ORG_TYPE") && dic["ORG_TYPE"] != null
                && dic["ORG_TYPE"].childTags != null && dic["ORG_TYPE"].childTags.Count > 0)
            {
                foreach (Model.Table.ChildTag ct in dic["ORG_TYPE"].childTags)
                {
                    CrmInst bankCrm = new CrmInst();
                    bankCrm.shortnamecn = ct.childTagName;
                    bankCrm.instcode = ct.childTagValue;
                    result.Add(bankCrm);
                }
            }
            //CrmInst bankCrm = new CrmInst();
            //bankCrm.shortnamecn = "银行配置户";
            //bankCrm.instcode = "0000000000000001";

            //CrmInst bankTradeCrm = new CrmInst();
            //bankTradeCrm.shortnamecn = "银行交易户";
            //bankTradeCrm.instcode = "0000000000000111";

            //CrmInst bondCrm = new CrmInst();
            //bondCrm.shortnamecn = "券商";
            //bondCrm.instcode = "0000000000000010";

            //CrmInst fundCrm = new CrmInst();
            //fundCrm.shortnamecn = "基金";
            //fundCrm.instcode = "0000000000000011";

            //CrmInst safeCrm = new CrmInst();
            //safeCrm.shortnamecn = "保险";
            //safeCrm.instcode = "0000000000000101";

            //CrmInst dealMoneyInsCrm = new CrmInst();
            //dealMoneyInsCrm.shortnamecn = "理财子";
            //dealMoneyInsCrm.instcode = "0000000000001000";

            //CrmInst asCrm = new CrmInst();
            //asCrm.shortnamecn = "资管";
            //asCrm.instcode = "0000000000000100";



            //CrmInst futureCrm = new CrmInst();
            //futureCrm.shortnamecn = "期货";
            //futureCrm.instcode = "0000000000001001";

            //CrmInst trustCrm = new CrmInst();
            //trustCrm.shortnamecn = "信托";
            //trustCrm.instcode = "0000000000001010";


            //CrmInst otherCrm = new CrmInst();
            //otherCrm.shortnamecn = "其他";
            //otherCrm.instcode = "0000000000000110";


            //result.Add(bankCrm);
            //result.Add(bankTradeCrm);
            //result.Add(bondCrm);
            //result.Add(fundCrm);
            //result.Add(safeCrm);
            //result.Add(dealMoneyInsCrm);
            //result.Add(asCrm);


            //result.Add(futureCrm);
            //result.Add(trustCrm);
            //result.Add(otherCrm);

            args.DataSource = result;
        }

    }
}
