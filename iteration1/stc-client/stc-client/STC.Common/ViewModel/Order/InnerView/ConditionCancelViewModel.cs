using Newtonsoft.Json;
using STC.Common.Model;
using STC.Common.Model.TBook;
using STC.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace STC.Common.ViewModel.Order.InnerView
{
    public class ConditionCancelViewModel : BaseViewModel
    {
        private string Bid_Condition_Txt = "对手方最优报价 - 本方报价 >";
        private string Ask_Condition_Txt = "本方报价 - 对手方最优报价 >";

        private string Setting_Condition_Txt = "对手方最优报价&本方报价 价差 >";

        #region 界面属性

        private string conditionTxt;
        public string ConditionTxt
        {
            get { return conditionTxt; }
            set
            {
                conditionTxt = value;
                this.OnPropertyChanged();
            }
        }

        private string morethanPartyBP;
        public string MorethanPartyBP
        {
            get { return morethanPartyBP; }
            set 
            { 
                morethanPartyBP = value;
                this.OnPropertyChanged();
            }
        }

        private string morethanMarketBP;
        public string MorethanMarketBP
        {
            get { return morethanMarketBP; }
            set 
            { 
                morethanMarketBP = value;
                this.OnPropertyChanged();
            }
        }


        private bool morethanPartyBPCheck;
        public bool MorethanPartyBPCheck
        {
            get { return morethanPartyBPCheck; }
            set
            {
                morethanPartyBPCheck = value;
                this.OnPropertyChanged();

                if (value && string.IsNullOrEmpty(morethanPartyBP))
                {
                    MorethanPartyBP = "0";
                }
            }
        }

        private bool morethanMarketBPCheck;
        public bool MorethanMarketBPCheck
        {
            get { return morethanMarketBPCheck; }
            set
            {
                morethanMarketBPCheck = value;
                this.OnPropertyChanged();

            }
        }

        private bool upsideCheck;
        public bool UpsideCheck
        {
            get { return upsideCheck; }
            set
            {
                upsideCheck = value;
                this.OnPropertyChanged();
            }
        }


        private Brush txBrush = new SolidColorBrush(Color.FromRgb(255, 255, 255));
        public Brush TxBrush
        {
            get { return txBrush; }
            set
            {
                txBrush = value;
                this.OnPropertyChanged();
            }
        }

        private Brush bdFocusBrush;
        public Brush BdFocusBrush
        {
            get { return bdFocusBrush; }
            set
            {
                bdFocusBrush = value;
                this.OnPropertyChanged();
            }
        }



        private Visibility priceConditionVisibility = Visibility.Collapsed;
        public Visibility PriceConditionVisibility
        {
            get { return priceConditionVisibility; }
            set
            {
                priceConditionVisibility = value;

                this.OnPropertyChanged();
            }
        }

        private Visibility conditionVisibility = Visibility.Collapsed;
        public Visibility ConditionVisibility
        {
            get { return conditionVisibility; }
            set
            {
                conditionVisibility = value;

                this.OnPropertyChanged();
            }
        }


        #endregion


        


        public void SetContionTxt(int showType)
        {
            if (showType == 1)
            {
                ConditionTxt = Bid_Condition_Txt;

                ConditionVisibility = Visibility.Collapsed;
            }
            else if (showType == 2)
            {
                ConditionTxt = Ask_Condition_Txt;

                ConditionVisibility = Visibility.Collapsed;
            }
            else if (showType == 3)
            {
                ConditionTxt = Setting_Condition_Txt;

                ConditionVisibility = Visibility.Visible;
            }
        }

        public PriceCondition GetPriceCondition()
        {
            PriceCondition priceCondition = new PriceCondition();
            if (morethanMarketBPCheck)
                priceCondition.LatestTransMarket = morethanMarketBP==null?string.Empty:morethanMarketBP;
            if (morethanPartyBPCheck)
                priceCondition.CounterpartyBestOffer = morethanPartyBP==null?string.Empty:morethanPartyBP;

            priceCondition.PriceInversion = upsideCheck ? "1" : "0";


            return priceCondition;
        }

        public bool CheckPriceCondition()
        {
            bool result = true;

            if (morethanPartyBPCheck && string.IsNullOrEmpty(MorethanMarketBP))
                result = false;

            return result;
        }


        public void SetPriceCondition(string condtionJson)
        {
            if (!string.IsNullOrEmpty(condtionJson))
            {
                PriceCondition priceCondition = JsonConvert.DeserializeObject<PriceCondition>(condtionJson);
                if (priceCondition != null)
                {
                    if (string.IsNullOrEmpty(priceCondition.CounterpartyBestOffer))
                    {
                        MorethanPartyBP = string.Empty;
                        if (priceCondition.CounterpartyBestOffer != null)
                            MorethanPartyBPCheck = true;
                        else
                            MorethanPartyBPCheck = false;
                    }
                    else
                    {
                        MorethanPartyBP = priceCondition.CounterpartyBestOffer;
                        MorethanPartyBPCheck = true;
                    }

                    if (string.IsNullOrEmpty(priceCondition.PriceInversion))
                    {
                        UpsideCheck = false;
                    }
                    else
                    {
                        if (priceCondition.PriceInversion.Equals("0"))
                            UpsideCheck = false;
                        else
                            UpsideCheck = true;
                    }

                    if (string.IsNullOrEmpty(priceCondition.LatestTransMarket))
                    {
                        MorethanMarketBP = string.Empty;
                        if (priceCondition.LatestTransMarket != null)
                            MorethanMarketBPCheck = true;
                        else
                            MorethanMarketBPCheck = false;
                    }
                    else
                    {
                        MorethanMarketBP = priceCondition.LatestTransMarket;
                        MorethanMarketBPCheck = true;
                    }
                }
            }
            else
            {
                MorethanPartyBP = string.Empty;
                MorethanPartyBPCheck = false;

                MorethanMarketBP = string.Empty;
                MorethanMarketBPCheck = false;

                UpsideCheck = false;
            }
        }
    }
}
