using Newtonsoft.Json;
using STC.Mmshe.Plugin.ViewModel;
using System.Windows.Controls;
using System;
using STC.Common.Model.MM;

namespace STC.Mmshe.Plugin.View.Component
{
    /// <summary>
    /// UserControl1.xaml 的交互逻辑
    /// </summary>
    public partial class MyQuotePopupView : UserControl
    {
        private MyQuotePopupViewModel viewModel = null;
        public MyQuotePopupView()
        {
            InitializeComponent();
            viewModel = new MyQuotePopupViewModel();
            this.DataContext = viewModel;
        }

        public void setBidParam(OrderParamProp param, string info5) 
        {
            viewModel.Data = convertMyBidQuotePopupPo(param,info5);
            //viewModel.ForegroundBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FB943C"));
            viewModel.Sign = "+";
        }
        public void setOfrParam(OrderParamProp param, string info5)
        {
            viewModel.Data = convertMyOfrQuotePopupPo(param, info5);
            //viewModel.ForegroundBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#009AFF"));
            viewModel.Sign = "-";
        }
        private MyQuotePopupPo convertMyBidQuotePopupPo(OrderParamProp param, string info5) 
        {
            
            MyQuotePopupPo myQuote = new MyQuotePopupPo();
            myQuote.QuoteModel = param.bidQuoteModel;
           /* myQuote.BasePrice = string.IsNullOrEmpty(param.bidBasePrice)? "--" : param.bidBasePrice;
            myQuote.DefenReferMarketType = param.bidDefenReferMarketType;
            myQuote.DefenReferLevel = param.bidDefenReferLevel;
            myQuote.AlgoDefenValue = param.bidAlgoDefenValue;
            myQuote.globalWall = param.globalWallBid;
            myQuote.AlgoTrigValue = param.bidAlgoTrigValue;*/
            //myQuote.defenCDCDetail = param.defenCDCDetail;
            //myQuote.DefenCDCValue = param.bidDefenCDCValue;
            //myQuote.CalDetail = param.bidCalDetail;
            if (!string.IsNullOrEmpty(info5))
            {
                Inf5 inf5Obj = JsonConvert.DeserializeObject<Inf5>(info5);
                if ("Algorithm".Equals(param.bidQuoteModel))
                {
                    myQuote.CalDetail = "正常报价";
                }
                else
                {
                    myQuote.CalDetail = inf5Obj.bidCalDetail;
                }
                myQuote.DefenCDCValue = inf5Obj.bidDefenCDCValue;
                myQuote.defenCDCDetail = inf5Obj.defenCDCDetail;
                myQuote.BasePrice = string.IsNullOrEmpty(inf5Obj.bid) ? "--" : inf5Obj.bid;
                myQuote.DefenReferMarketType = inf5Obj.bidDefenReferMarketType;
                myQuote.DefenReferLevel = inf5Obj.bidDefenReferLevel;
                myQuote.AlgoDefenValue = inf5Obj.bidAlgoDefenValue;
                myQuote.globalWall = inf5Obj.globalWallBid;
                myQuote.AlgoTrigValue = inf5Obj.bidAlgoTrigValue;
            }
            if ("Defend".Equals(param.bidQuoteModel))
            {
                myQuote.VisibileDefendValue = "Visible";

                myQuote.VisibileAlgorithmValue = "Collapsed";
            }
            if ("Algorithm".Equals(param.bidQuoteModel))
            {
                myQuote.VisibileDefendValue = "Collapsed";
                myQuote.VisibileAlgorithmValue = "Visible";
                myQuote.AlgoBaseSecurityid = string.IsNullOrEmpty(param.algoBaseSecurityid) || "0".Equals(param.algoBaseEnable) ? "--" : param.algoBaseSecurityid;
                //参考价格类型
                if (string.IsNullOrEmpty(param.algoMidType))
                {
                    myQuote.AlgoMidType = "--";
                }
                else if ("1".Equals(param.algoMidType))
                {
                    myQuote.AlgoMidType = "双边中间价";
                }
                else if ("2".Equals(param.algoMidType))
                {
                    myQuote.AlgoMidType = "中债估值";
                }
                else if ("3".Equals(param.algoMidType))
                {
                    myQuote.AlgoMidType = "自定义";
                }
                //参考价格值
                myQuote.AlgoMidValue = param.algoMidPrice;
                //中债估值加上日期信息
                if ("2".Equals(param.algoMidType) && !string.IsNullOrEmpty(param.algoCDCDetail))
                {
                    try {
                        string dateinfo = param.algoCDCDetail.Substring(param.algoCDCDetail.IndexOf("("), param.algoCDCDetail.IndexOf(")") - 3);
                        myQuote.AlgoMidValue = dateinfo + myQuote.AlgoMidValue;
                    }
                    catch (Exception e)
                    {
                       
                    }
                   
                }
                //自定义无偏离
                if ("3".Equals(param.algoMidType))
                {
                    myQuote.AlgoMidOffsetValue = "";
                }
                else {
                    myQuote.AlgoMidOffsetValue = string.IsNullOrEmpty(param.algoMidOffsetValue) ? "--" : " +" + param.algoMidOffsetValue + "";
                }
                myQuote.AlgoValue = string.IsNullOrEmpty(param.algoValue) ? "--" : param.algoValue + "/2";
            }
            return myQuote;
        }
        private MyQuotePopupPo convertMyOfrQuotePopupPo(OrderParamProp param, string info5)
        {
            MyQuotePopupPo myQuote = new MyQuotePopupPo();
            myQuote.QuoteModel = param.ofrQuoteModel;
           /* myQuote.BasePrice = string.IsNullOrEmpty(param.ofrBasePrice) ? "--" : param.ofrBasePrice; ;
            myQuote.DefenReferMarketType = param.ofrDefenReferMarketType;
            myQuote.DefenReferLevel = param.ofrDefenReferLevel;
            myQuote.AlgoDefenValue = param.ofrAlgoDefenValue;
            myQuote.globalWall = param.globalWallOfr;
            myQuote.AlgoTrigValue = param.ofrAlgoTrigValue;
            myQuote.defenCDCDetail = param.defenCDCDetail;
            myQuote.DefenCDCValue = param.ofrDefenCDCValue;
            myQuote.CalDetail = param.ofrCalDetail;*/
            if (!string.IsNullOrEmpty(info5))
            {
                Inf5 inf5Obj = JsonConvert.DeserializeObject<Inf5>(info5);
                myQuote.BasePrice = string.IsNullOrEmpty(inf5Obj.ofr) ? "--" : inf5Obj.ofr; ;
                myQuote.DefenReferMarketType = inf5Obj.ofrDefenReferMarketType;
                myQuote.DefenReferLevel = inf5Obj.ofrDefenReferLevel;
                myQuote.AlgoDefenValue = inf5Obj.ofrAlgoDefenValue;
                myQuote.globalWall = inf5Obj.globalWallOfr;
                myQuote.AlgoTrigValue = inf5Obj.ofrAlgoTrigValue;
                myQuote.defenCDCDetail = inf5Obj.defenCDCDetail;
                myQuote.DefenCDCValue = inf5Obj.ofrDefenCDCValue;
                if ("Algorithm".Equals(param.ofrQuoteModel))
                {
                    myQuote.CalDetail = "正常报价";
                }
                else
                {
                    myQuote.CalDetail = inf5Obj.ofrCalDetail;
                }
            }
            if ("Defend".Equals(param.ofrQuoteModel))
            {
                myQuote.VisibileDefendValue = "Visible";

                myQuote.VisibileAlgorithmValue = "Collapsed";
            }
            if ("Algorithm".Equals(param.ofrQuoteModel))
            {
                myQuote.VisibileDefendValue = "Collapsed";
                myQuote.VisibileAlgorithmValue = "Visible";
                myQuote.AlgoBaseSecurityid = string.IsNullOrEmpty(param.algoBaseSecurityid) || "0".Equals(param.algoBaseEnable) ? "--" : param.algoBaseSecurityid;
                //参考价格类型
                if (string.IsNullOrEmpty(param.algoMidType))
                {
                    myQuote.AlgoMidType = "--";
                }
                else if ("1".Equals(param.algoMidType))
                {
                    myQuote.AlgoMidType = "双边中间价";
                }
                else if ("2".Equals(param.algoMidType))
                {
                    myQuote.AlgoMidType = "中债估值";
                }
                else if ("3".Equals(param.algoMidType))
                {
                    myQuote.AlgoMidType = "自定义";
                }
                //参考中间价
                myQuote.AlgoMidValue = param.algoMidPrice;
                //中债估值加上日期信息
                if ("2".Equals(param.algoMidType) && !string.IsNullOrEmpty(param.algoCDCDetail))
                {
                    try
                    {
                        string dateinfo = param.algoCDCDetail.Substring(param.algoCDCDetail.IndexOf("("), param.algoCDCDetail.IndexOf(")") - 3);
                        myQuote.AlgoMidValue = dateinfo + myQuote.AlgoMidValue;
                    }
                    catch (Exception e)
                    {

                    }

                }
                //自定义无偏离
                if ("3".Equals(param.algoMidType))
                {
                    myQuote.AlgoMidOffsetValue = "";
                }
                else
                {
                    myQuote.AlgoMidOffsetValue = string.IsNullOrEmpty(param.algoMidOffsetValue) ? "--" : " +" + param.algoMidOffsetValue + "";
                }
                myQuote.AlgoValue = string.IsNullOrEmpty(param.algoValue) ? "--" : param.algoValue + "/2";
            }
            return myQuote;
        }
        private string _SecurityID;
        public string SecurityID 
        {
            set 
            {
                _SecurityID = value;
            }
            get 
            {
                return SecurityID;
            }
        }
    }
}
