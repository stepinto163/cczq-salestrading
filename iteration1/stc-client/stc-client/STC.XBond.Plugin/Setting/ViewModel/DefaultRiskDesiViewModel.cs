using IDC.Matching.Plugin.Utils;
using STC.Common.Handler.TBook;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.Risk;
using STC.Frame.UI;
using STC.XBond.Plugin.Utils;
using STC.XBond.Plugin.ViewModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace STC.XBond.Plugin.Setting.ViewModel
{
    public class DefaultRiskDesiViewModel : ViewModelBase
    {
        #region 事件
        public ICommand AddCommand { get { return new RelayCommand(OnAddClick); } }
        public ICommand DownCommand { get { return new RelayCommand(OnDownClick); } }

        public ICommand AddIQtyMaxCommand { get { return new RelayCommand(OnAddIQtyMaxClick); } }
        public ICommand DownIQtyMaxCommand { get { return new RelayCommand(OnDownIQtyMaxClick); } }

        public ICommand AddCQtyMinCommand { get { return new RelayCommand(OnAddCQtyMinClick); } }
        public ICommand DownCQtyMinCommand { get { return new RelayCommand(OnDownCQtyMinClick); } }

        public ICommand AddCQtyMaxCommand { get { return new RelayCommand(OnAddCQtyMaxClick); } }
        public ICommand DownCQtyMaxCommand { get { return new RelayCommand(OnDownCQtyMaxClick); } }


        public ICommand AddIBidCommand { get { return new RelayCommand(OnAddIBidClick); } }
        public ICommand DownIBidCommand { get { return new RelayCommand(OnDownIBidClick); } }

        public ICommand AddIOfrCommand { get { return new RelayCommand(OnAddIOfrClick); } }
        public ICommand DownIOfrCommand { get { return new RelayCommand(OnDownIOfrClick); } }

        public ICommand AddCBidCommand { get { return new RelayCommand(OnAddCBidClick); } }
        public ICommand DownCBidCommand { get { return new RelayCommand(OnDownCBidClick); } }

        public ICommand AddCOfrCommand { get { return new RelayCommand(OnAddCOfrClick); } }
        public ICommand DownCOfrCommand { get { return new RelayCommand(OnDownCOfrClick); } }

        public ICommand AddIMarketBidCommand { get { return new RelayCommand(OnAddIMarketBidClick); } }
        public ICommand DownIMarketBidCommand { get { return new RelayCommand(OnDownIMarketBidClick); } }

        public ICommand AddIDropAwayFromBidCommand { get { return new RelayCommand(OnAddIDropAwayFromBidClick); } }
        public ICommand DownIDropAwayFromBidCommand { get { return new RelayCommand(OnDownIDropAwayFromBidClick); } }

        public ICommand AddIMarketOfrCommand { get { return new RelayCommand(OnAddIMarketOfrClick); } }
        public ICommand DownIMarketOfrCommand { get { return new RelayCommand(OnDownIMarketOfrClick); } }

        public ICommand AddIDropAwayFromOfrCommand { get { return new RelayCommand(OnAddIDropAwayFromOfrClick); } }
        public ICommand DownIDropAwayFromOfrCommand { get { return new RelayCommand(OnDownIDropAwayFromOfrClick); } }

        public ICommand AddCMarketBidCommand { get { return new RelayCommand(OnAddCMarketBidClick); } }
        public ICommand DownCMarketBidCommand { get { return new RelayCommand(OnDownCMarketBidClick); } }

        public ICommand AddCDropAwayFromBidCommand { get { return new RelayCommand(OnAddCDropAwayFromBidClick); } }
        public ICommand DownCDropAwayFromBidCommand { get { return new RelayCommand(OnDownCDropAwayFromBidClick); } }

        public ICommand AddCMarketOfrCommand { get { return new RelayCommand(OnAddCMarketOfrClick); } }
        public ICommand DownCMarketOfrCommand { get { return new RelayCommand(OnDownCMarketOfrClick); } }

        public ICommand AddCDropAwayFromOfrCommand { get { return new RelayCommand(OnAddCDropAwayFromOfrClick); } }
        public ICommand DownCDropAwayFromOfrCommand { get { return new RelayCommand(OnDownCDropAwayFromOfrClick); } }

        public ICommand OkCommand { get { return new RelayCommand(OKClick); } }
        public ICommand CancelCommand { get { return new RelayCommand(CancelClick); } }

        
        #endregion

        private DefaultRiskItem defaultRiskItem = null;

        public DefaultRiskItem DefaultRiskItem
        {
            get
            {
                return defaultRiskItem;
            }

            set
            {
                if (value != null)
                {
                    defaultRiskItem = value;
                    this.OnPropertyChanged();
                }
            }
        }

        public void init()
        {
            DefaultRiskItem = new DefaultRiskItem();
            selectRisk();
        }
        private void selectRisk()
        {
            RiskParaReq req = new RiskParaReq();
            RiskParaReplyPo po = TBookHandler.Instance.queryRiskPara(req);
            if (po != null)
            {
                RiskParaReq rpp = po.datas;
                if(rpp.defaultRiskParameter != null)
                    UpdateDefaultRiskItem(rpp.defaultRiskParameter);
            }
        }

        private void UpdateDefaultRiskItem(DefaultRiskParameter defaultRiskParameter)
        {
            if (defaultRiskParameter != null)
            {
                DefaultRiskItem.CQtyMin = decimal.Parse(defaultRiskParameter.creditRiskParam.qtyMin);
                DefaultRiskItem.CQtyMax = decimal.Parse(defaultRiskParameter.creditRiskParam.qtyMax);
                DefaultRiskItem.CBid = decimal.Parse(defaultRiskParameter.creditRiskParam.bid);
                DefaultRiskItem.COfr = decimal.Parse(defaultRiskParameter.creditRiskParam.ofr);
                DefaultRiskItem.CDropAwayFromBid = decimal.Parse(defaultRiskParameter.creditRiskParam.dropAwayFromBid);
                DefaultRiskItem.CDropAwayFromOfr = decimal.Parse(defaultRiskParameter.creditRiskParam.dropAwayFromOfr);
                DefaultRiskItem.CMarketBid = decimal.Parse(defaultRiskParameter.creditRiskParam.marketBid);
                DefaultRiskItem.CMarketOfr = decimal.Parse(defaultRiskParameter.creditRiskParam.marketOfr);
                if (defaultRiskParameter.creditRiskParam.bidCheck != null && defaultRiskParameter.creditRiskParam.bidCheck == "1")
                {
                    DefaultRiskItem.CBidDefautlCheck = true;
                }
                else
                {
                    DefaultRiskItem.CBidDefautlCheck = false;
                }
                if (defaultRiskParameter.creditRiskParam.ofrCheck != null && defaultRiskParameter.creditRiskParam.ofrCheck == "1")
                {
                    DefaultRiskItem.COfrDefautlCheck = true;
                }
                else
                {
                    DefaultRiskItem.COfrDefautlCheck = false;
                }
                DefaultRiskItem.IQtyMin = decimal.Parse(defaultRiskParameter.interestRateRiskParam.qtyMin);
                DefaultRiskItem.IQtyMax = decimal.Parse(defaultRiskParameter.interestRateRiskParam.qtyMax);
                DefaultRiskItem.IBid = decimal.Parse(defaultRiskParameter.interestRateRiskParam.bid);
                DefaultRiskItem.IOfr = decimal.Parse(defaultRiskParameter.interestRateRiskParam.ofr);
                DefaultRiskItem.IDropAwayFromBid = decimal.Parse(defaultRiskParameter.interestRateRiskParam.dropAwayFromBid);
                DefaultRiskItem.IDropAwayFromOfr = decimal.Parse(defaultRiskParameter.interestRateRiskParam.dropAwayFromOfr);
                DefaultRiskItem.IMarketBid = decimal.Parse(defaultRiskParameter.interestRateRiskParam.marketBid);
                DefaultRiskItem.IMarketOfr = decimal.Parse(defaultRiskParameter.interestRateRiskParam.marketOfr);

                if (defaultRiskParameter.interestRateRiskParam.bidCheck != null && defaultRiskParameter.interestRateRiskParam.bidCheck == "1")
                {
                    DefaultRiskItem.IBidDefautlCheck = true;
                }
                else
                {
                    DefaultRiskItem.IBidDefautlCheck = false;
                }
                if (defaultRiskParameter.interestRateRiskParam.ofrCheck != null && defaultRiskParameter.interestRateRiskParam.ofrCheck == "1")
                {
                    DefaultRiskItem.IOfrDefautlCheck = true;
                }
                else
                {
                    DefaultRiskItem.IOfrDefautlCheck = false;
                }
            }
            else
            {
                DefaultRiskItem = new DefaultRiskItem();
            }
        }


        private void OnDownClick()
        {
            if ((DefaultRiskItem.IQtyMin - 1000) >= 0)
                DefaultRiskItem.IQtyMin = DefaultRiskItem.IQtyMin - 1000;
        }

        private void OnAddClick()
        {
            if (DefaultRiskItem.IQtyMin >= 0 && (DefaultRiskItem.IQtyMin + 1000) <= DefaultRiskItem.IQtyMax)
                DefaultRiskItem.IQtyMin = (DefaultRiskItem.IQtyMin + 1000);
        }

        private void OnDownIQtyMaxClick()
        {
            if ((DefaultRiskItem.IQtyMax - 1000) >= 0 && (DefaultRiskItem.IQtyMin + 1000) <= DefaultRiskItem.IQtyMax)
                DefaultRiskItem.IQtyMax = DefaultRiskItem.IQtyMax - 1000;
        }

        private void OnAddIQtyMaxClick()
        {
            DefaultRiskItem.IQtyMax = (DefaultRiskItem.IQtyMax + 1000);
        }

        private void OnAddIBidClick()
        {
            if (DefaultRiskItem.IBid >= 0 && DefaultRiskItem.IBid+1 < 100)
                DefaultRiskItem.IBid = (DefaultRiskItem.IBid + 1);
        }


        private void OnDownIBidClick()
        {
            if ((DefaultRiskItem.IBid - 1 )>= 0)
                DefaultRiskItem.IBid = DefaultRiskItem.IBid - 1;
        }

        private void OnAddIOfrClick()
        {
            if (DefaultRiskItem.IOfr >= 0 && DefaultRiskItem.IOfr + 1 < 100)
                DefaultRiskItem.IOfr = (DefaultRiskItem.IOfr + 1);
        }

        private void OnDownIOfrClick()
        {
            if ((DefaultRiskItem.IOfr - 1) >= 0)
                DefaultRiskItem.IOfr = DefaultRiskItem.IOfr - 1;
        }
        private void OnAddCQtyMinClick()
        {
            if (DefaultRiskItem.CQtyMin >= 0 && ((DefaultRiskItem.CQtyMin + 1000) <= DefaultRiskItem.CQtyMax))
                DefaultRiskItem.CQtyMin = (DefaultRiskItem.CQtyMin + 1000);
        }

        private void OnDownCQtyMinClick()
        {
            if ((DefaultRiskItem.CQtyMin - 1000) >= 0)
                DefaultRiskItem.CQtyMin = DefaultRiskItem.CQtyMin - 1000;
        }
        private void OnAddCQtyMaxClick()
        {
            DefaultRiskItem.CQtyMax = (DefaultRiskItem.CQtyMax + 1000);
        }
        private void OnDownCQtyMaxClick()
        {
            if ((DefaultRiskItem.CQtyMax - 1000) >= 0 && (DefaultRiskItem.CQtyMin + 1000) <= DefaultRiskItem.CQtyMax)
                DefaultRiskItem.CQtyMax = DefaultRiskItem.CQtyMax - 1000;
        }
        private void OnAddCBidClick()
        {
            if (DefaultRiskItem.CBid >= 0 && DefaultRiskItem.CBid + 1 < 100)
                DefaultRiskItem.CBid = (DefaultRiskItem.CBid + 1);
        }

        private void OnDownCBidClick()
        {
            if ((DefaultRiskItem.CBid - 1) >= 0)
                DefaultRiskItem.CBid = DefaultRiskItem.CBid - 1;
        }

        private void OnAddCOfrClick()
        {
            if (DefaultRiskItem.COfr >= 0 && DefaultRiskItem.COfr + 1 < 100)
                DefaultRiskItem.COfr = (DefaultRiskItem.COfr + 1);
        }

        private void OnDownCOfrClick()
        {
            if ((DefaultRiskItem.COfr - 1) >= 0)
                DefaultRiskItem.COfr = DefaultRiskItem.COfr - 1;
        }

        private void OnAddIMarketBidClick()
        {
            if (DefaultRiskItem.IMarketBid >= 0 && DefaultRiskItem.IMarketBid + 1 < 100)
                DefaultRiskItem.IMarketBid = (DefaultRiskItem.IMarketBid + 1);
        }

        private void OnDownIMarketBidClick()
        {
            if ((DefaultRiskItem.IMarketBid - 1) >= 0)
                DefaultRiskItem.IMarketBid = DefaultRiskItem.IMarketBid - 1;
        }
        private void OnAddIDropAwayFromBidClick()
        {
            if (DefaultRiskItem.IDropAwayFromBid >= 0 && DefaultRiskItem.IDropAwayFromBid + 1 < 100)
                DefaultRiskItem.IDropAwayFromBid = (DefaultRiskItem.IDropAwayFromBid + 1);
        }

        private void OnDownIDropAwayFromBidClick()
        {
            if ((DefaultRiskItem.IDropAwayFromBid - 1) >= 0)
                DefaultRiskItem.IDropAwayFromBid = DefaultRiskItem.IDropAwayFromBid - 1;
        }
        private void OnAddIMarketOfrClick()
        {
            if (DefaultRiskItem.IMarketOfr >= 0 && DefaultRiskItem.IMarketOfr + 1 < 100)
                DefaultRiskItem.IMarketOfr = (DefaultRiskItem.IMarketOfr + 1);
        }
        
        private void OnDownIMarketOfrClick()
        {
            if ((DefaultRiskItem.IMarketOfr - 1) >= 0)
                DefaultRiskItem.IMarketOfr = DefaultRiskItem.IMarketOfr - 1;
        }
        private void OnAddIDropAwayFromOfrClick()
        {
            if (DefaultRiskItem.IDropAwayFromOfr >= 0 && DefaultRiskItem.IDropAwayFromOfr + 1 < 100)
                DefaultRiskItem.IDropAwayFromOfr = (DefaultRiskItem.IDropAwayFromOfr + 1);
        }

        private void OnDownIDropAwayFromOfrClick()
        {
            if ((DefaultRiskItem.IDropAwayFromOfr - 1) >= 0)
                DefaultRiskItem.IDropAwayFromOfr = DefaultRiskItem.IDropAwayFromOfr - 1;
        }
        private void OnAddCMarketBidClick()
        {
            if (DefaultRiskItem.CMarketBid >= 0 && DefaultRiskItem.CMarketBid + 1 < 100)
                DefaultRiskItem.CMarketBid = (DefaultRiskItem.CMarketBid + 1);
        }


        private void OnDownCMarketBidClick()
        {
            if ((DefaultRiskItem.CMarketBid - 1) >= 0)
                DefaultRiskItem.CMarketBid = DefaultRiskItem.CMarketBid - 1;
        }

        private void OnAddCDropAwayFromBidClick()
        {
            if (DefaultRiskItem.CDropAwayFromBid >= 0 && DefaultRiskItem.CDropAwayFromBid + 1 < 100)
                DefaultRiskItem.CDropAwayFromBid = (DefaultRiskItem.CDropAwayFromBid + 1);
        }


        private void OnDownCDropAwayFromBidClick()
        {
            if ((DefaultRiskItem.CDropAwayFromBid - 1) >= 0)
                DefaultRiskItem.CDropAwayFromBid = DefaultRiskItem.CDropAwayFromBid - 1;
        }

        private void OnAddCMarketOfrClick()
        {
            if (DefaultRiskItem.CMarketOfr >= 0 && DefaultRiskItem.CMarketOfr + 1 < 100)
                DefaultRiskItem.CMarketOfr = (DefaultRiskItem.CMarketOfr + 1);
        }

        private void OnDownCMarketOfrClick()
        {
            if ((DefaultRiskItem.CMarketOfr - 1) >= 0)
                DefaultRiskItem.CMarketOfr = DefaultRiskItem.CMarketOfr - 1;
        }
        private void OnAddCDropAwayFromOfrClick()
        {
            if (DefaultRiskItem.CDropAwayFromOfr >= 0 && DefaultRiskItem.CDropAwayFromOfr + 1 < 100)
                DefaultRiskItem.CDropAwayFromOfr = (DefaultRiskItem.CDropAwayFromOfr + 1);
        }

        private void OnDownCDropAwayFromOfrClick()
        {
            if ((DefaultRiskItem.CDropAwayFromOfr - 1) >= 0)
                DefaultRiskItem.CDropAwayFromOfr = DefaultRiskItem.CDropAwayFromOfr - 1;
        }

        private void CancelClick()
        {
            selectRisk();
        }

        private void OKClick()
        {
            if (checkData())
            {
                showMessage(string.Format($"{failureMsg}."));
                return;
            }
            RiskParaReq req = new RiskParaReq();
            DefaultRiskParameter po = new DefaultRiskParameter();
            po.creditRiskParam.qtyMin = DefaultRiskItem.CQtyMin.ToString();
            po.creditRiskParam.qtyMax = DefaultRiskItem.CQtyMax.ToString();
            po.creditRiskParam.bid = DefaultRiskItem.CBid.ToString();
            po.creditRiskParam.ofr = DefaultRiskItem.COfr.ToString();
            po.creditRiskParam.marketBid = DefaultRiskItem.CMarketBid.ToString();
            po.creditRiskParam.marketOfr = DefaultRiskItem.CMarketOfr.ToString();
            po.creditRiskParam.dropAwayFromBid = DefaultRiskItem.CDropAwayFromBid.ToString();
            po.creditRiskParam.dropAwayFromOfr = DefaultRiskItem.CDropAwayFromOfr.ToString();

            po.interestRateRiskParam.qtyMin = DefaultRiskItem.IQtyMin.ToString();
            po.interestRateRiskParam.qtyMax = DefaultRiskItem.IQtyMax.ToString();
            po.interestRateRiskParam.bid = DefaultRiskItem.IBid.ToString();
            po.interestRateRiskParam.ofr = DefaultRiskItem.IOfr.ToString();
            po.interestRateRiskParam.marketBid = DefaultRiskItem.IMarketBid.ToString();
            po.interestRateRiskParam.marketOfr = DefaultRiskItem.IMarketOfr.ToString();
            po.interestRateRiskParam.dropAwayFromBid = DefaultRiskItem.IDropAwayFromBid.ToString();
            po.interestRateRiskParam.dropAwayFromOfr = DefaultRiskItem.IDropAwayFromOfr.ToString();
            if (DefaultRiskItem.IBidDefautlCheck)
            {
                po.interestRateRiskParam.bidCheck = "1";
            }
            else
            {
                po.interestRateRiskParam.bidCheck = "0";
            }
            if (DefaultRiskItem.IOfrDefautlCheck)
            {
                po.interestRateRiskParam.ofrCheck = "1";
            }
            else
            {
                po.interestRateRiskParam.ofrCheck = "0";
            }

            if (DefaultRiskItem.CBidDefautlCheck)
            {
                po.creditRiskParam.bidCheck = "1";
            }
            else
            {
                po.creditRiskParam.bidCheck = "0";
            }
            if (DefaultRiskItem.COfrDefautlCheck)
            {
                po.creditRiskParam.ofrCheck = "1";
            }
            else
            {
                po.creditRiskParam.ofrCheck = "0";
            }
            req.defaultRiskParameter = po;
            PResult reply = TBookHandler.Instance.updateRiskPara(req);
            if (reply != null)
            {
                if (reply.Code == "0000")
                {

                }
                else
                {
                    showMessage(string.Format($"更新失败, 原因:{reply.Msg}."));
                }
            }
        }
        private string failureMsg = "";
        private bool checkData()
        {
            bool status = false;
            failureMsg = "";
            decimal maxQty = DefaultRiskItem.CQtyMax == null ? 0 : (decimal)DefaultRiskItem.CQtyMax;
            if (maxQty % 1000 != 0)
            {
                failureMsg = "单笔报量利率债上限非1000的整数倍";
                status = true;
            }
            decimal minQty = DefaultRiskItem.CQtyMin == null ? 0 : (decimal)DefaultRiskItem.CQtyMin;
            if (minQty % 1000 != 0)
            {
                if (!"".Equals(failureMsg))
                {
                    failureMsg = failureMsg + ",单笔报量利率债下限非1000的整数倍";
                }
                else
                {
                    failureMsg = "单笔报量利率债下限非1000的整数倍";
                }
                status = true;
            }
            decimal maxQty1 = DefaultRiskItem.IQtyMax == null ? 0 : (decimal)DefaultRiskItem.IQtyMax;
            if (maxQty % 1000 != 0)
            {
                failureMsg = "单笔报量信用债上限非1000的整数倍";
                status = true;
            }
            decimal minQty1 = DefaultRiskItem.IQtyMin == null ? 0 : (decimal)DefaultRiskItem.IQtyMin;
            if (minQty % 1000 != 0)
            {
                if (!"".Equals(failureMsg))
                {
                    failureMsg = failureMsg + ",单笔报量信用债下限非1000的整数倍";
                }
                else
                {
                    failureMsg = "单笔报量信用债下限非1000的整数倍";
                }
                status = true;
            }
            return status;
        }
        /// <summary>
        /// 提示消息弹框
        /// </summary>
        /// <param name="message"></param>
        private void showMessage(string message)
        {
            MessageBoxGUI me = new MessageBoxGUI();
            me.setWarnMsg(message);
            me.ShowDialog();
        }
    }
}
