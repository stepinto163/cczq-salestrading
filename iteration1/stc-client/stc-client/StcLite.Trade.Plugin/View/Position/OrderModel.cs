using STC.Common.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDB.Trade.Plugin.View.Position
{
    /// <summary>
    ///  我的需求，已撤销
    /// </summary>
    public class OrderModel : BaseViewModel
    {
        private bool ischecked;
        public bool IsChecked
        {
            get
            {
                return ischecked;
            }
            set
            {
                this.ischecked = value;
                this.OnPropertyChanged(nameof(IsChecked));
            }
        }

        private int checkedCount4MyOrders;
        public int CheckedCount4MyOrders
        {
            get
            {
                return checkedCount4MyOrders;
            }
        }

        private string key;
        public string Key
        {
            get
            {
                return key;
            }
            set
            {
                this.key = value;
                this.OnPropertyChanged(nameof(Key));
            }
        }

        private string stccode;
        public string STCCode
        {
            get
            {
                return stccode;
            }
            set
            {
                this.stccode = value;
                this.OnPropertyChanged(nameof(STCCode));
            }
        }

        private string side;
        public string Side
        {
            get
            {
                return side;
            }
            set
            {
                this.side = value;
                this.OnPropertyChanged(nameof(Side));
            }
        }

        private string securityid;
        public string SecurityID
        {
            get
            {
                return securityid;
            }
            set
            {
                this.securityid = value;
                this.OnPropertyChanged(nameof(SecurityID));
            }
        }

        private string symbol;
        public string Symbol
        {
            get
            {
                return symbol;
            }
            set
            {
                this.symbol = value;
                this.OnPropertyChanged(nameof(Symbol));
            }
        }

        private string netprice;
        public string NetPrice
        {
            get
            {
                return netprice;
            }
            set
            {
                this.netprice = value;
                this.OnPropertyChanged(nameof(NetPrice));
            }
        }

        private string price;
        public string Price
        {
            get
            {
                return price;
            }
            set
            {
                this.price = value;
                this.OnPropertyChanged(nameof(Price));
            }
        }

        private string qty;
        public string Qty
        {
            get
            {
                return qty;
            }
            set
            {
                this.qty = value;
                this.OnPropertyChanged(nameof(Qty));
            }
        }

        private string dealingqty;
        public string DealingQty
        {
            get
            {
                return dealingqty;
            }
            set
            {
                this.dealingqty = value;
                this.OnPropertyChanged(nameof(DealingQty));
            }
        }

        private string execqty;
        public string ExecQty
        {
            get
            {
                return execqty;
            }
            set
            {
                this.execqty = value;
                this.OnPropertyChanged(nameof(ExecQty));
            }
        }

        private string mydealer;
        public string MyDealer
        {
            get
            {
                return mydealer;
            }
            set
            {
                this.mydealer = value;
                this.OnPropertyChanged(nameof(MyDealer));
            }
        }

        private string settletype;
        public string SettleType
        {
            get
            {
                return settletype;
            }
            set
            {
                this.settletype = value;
                this.OnPropertyChanged(nameof(SettleType));
            }
        }

        private string settldate;
        public string SettlDate
        {
            get
            {
                return settldate;
            }
            set
            {
                this.settldate = value;
                this.OnPropertyChanged(nameof(SettlDate));
            }
        }

        private string ordtype;
        public string OrdType
        {
            get
            {
                return ordtype;
            }
            set
            {
                this.ordtype = value;
                this.OnPropertyChanged(nameof(OrdType));
            }
        }

        private string orderid;
        public string OrderID
        {
            get
            {
                return orderid;
            }
            set
            {
                this.orderid = value;
                this.OnPropertyChanged(nameof(OrderID));
            }
        }

        private string venues;
        public string Venues
        {
            get
            {
                return venues;
            }
            set
            {
                this.venues = value;
                this.OnPropertyChanged(nameof(Venues));
            }
        }

        private string orderstatus;
        public string OrderStatus
        {
            get
            {
                return orderstatus;
            }
            set
            {
                this.orderstatus = value;
                this.OnPropertyChanged(nameof(OrderStatus));
            }
        }

        private string transacttime;
        public string TransactTime
        {
            get
            {
                return transacttime;
            }
            set
            {
                this.transacttime = value;
                this.OnPropertyChanged(nameof(TransactTime));
            }
        }

        private string updatetime;
        public string UpdateTime
        {
            get
            {
                return updatetime;
            }
            set
            {
                this.updatetime = value;
                this.OnPropertyChanged(nameof(UpdateTime));
            }
        }

        private string algoname;
        public string AlgoName
        {
            get
            {
                return algoname;
            }
            set
            {
                this.algoname = value;
                this.OnPropertyChanged(nameof(AlgoName));
            }
        }

        private string firmflag;
        public string FirmFlag
        {
            get
            {
                return firmflag;
            }
            set
            {
                this.firmflag = value;
                this.OnPropertyChanged(nameof(FirmFlag));
            }
        }

        private string securitytype;
        public string SecurityType
        {
            get
            {
                return securitytype;
            }
            set
            {
                this.securitytype = value;
                this.OnPropertyChanged(nameof(SecurityType));
            }
        }


        private string nonfirmtime;
        public string NonfirmTime
        {
            get
            {
                return nonfirmtime;
            }
            set
            {
                this.nonfirmtime = value;
                this.OnPropertyChanged(nameof(NonfirmTime));
            }
        }

        private string remainingtime;
        public string RemainingTime
        {
            get
            {
                return remainingtime;
            }
            set
            {
                this.remainingtime = value;
                this.OnPropertyChanged(nameof(RemainingTime));
            }
        }

        private bool showdeletebtn;
        public bool ShowDeleteBtn
        {
            get
            {
                return showdeletebtn;
            }
            set
            {
                this.showdeletebtn = value;
                this.OnPropertyChanged(nameof(ShowDeleteBtn));
            }
        }

        private bool enabledbtn;
        public bool EnabledBtn
        {
            get
            {
                return enabledbtn;
            }
            set
            {
                this.enabledbtn = value;
                this.OnPropertyChanged(nameof(EnabledBtn));
            }
        }

        private string wholeflag;
        public string WholeFlag
        {
            get
            {
                return wholeflag;
            }
            set
            {
                this.wholeflag = value;
                this.OnPropertyChanged(nameof(WholeFlag));
            }
        }

        private string crossvenues;
        public string CrossVenues
        {
            get
            {
                return crossvenues;
            }
            set
            {
                this.crossvenues = value;
                this.OnPropertyChanged(nameof(CrossVenues));
            }
        }

        private string venuesselectf;
        public string VenuesSelectF
        {
            get
            {
                return venuesselectf;
            }
            set
            {
                this.venuesselectf = value;
                this.OnPropertyChanged(nameof(VenuesSelectF));
            }
        }


        private string venuesrdmnum;

        public string VenuesRdmNum
        {
            get
            {
                return venuesrdmnum;
            }
            set
            {
                this.venuesrdmnum = value;
                this.OnPropertyChanged(nameof(VenuesRdmNum));
            }
        }



        private string selfselectvenues;
        public string SelfSelectVenues
        {
            get
            {
                return selfselectvenues;
            }
            set
            {
                this.selfselectvenues = value;
                this.OnPropertyChanged(nameof(SelfSelectVenues));
            }
        }

        private string venuesselectdisplay;
        public string VenuesSelectDisplay
        {
            get
            {
                return venuesselectdisplay;
            }
            set
            {
                this.venuesselectdisplay = value;
                this.OnPropertyChanged(nameof(VenuesSelectDisplay));
            }
        }

        private string institutionid;
        public string InstitutionID
        {
            get
            {
                return institutionid;
            }
            set
            {
                this.institutionid = value;
                this.OnPropertyChanged(nameof(InstitutionID));
            }
        }



        private string institutionname;
        public string InstitutionName
        {
            get
            {
                return institutionname;
            }
            set
            {
                this.institutionname = value;
                this.OnPropertyChanged(nameof(InstitutionName));
            }
        }


        private string traderid;
        public string TraderID
        {
            get
            {
                return traderid;
            }
            set
            {
                this.traderid = value;
                this.OnPropertyChanged(nameof(TraderID));
            }
        }

        private string tradername;
        public string TraderName
        {
            get
            {
                return tradername;
            }
            set
            {
                this.tradername = value;
                this.OnPropertyChanged(nameof(TraderName));
            }
        }

        private string venuetypegw;
        public string VenueTypeGW
        {
            get
            {
                return venuetypegw;
            }
            set
            {
                this.venuetypegw = value;
                this.OnPropertyChanged(nameof(VenueTypeGW));
            }
        }

        private string quantid;
        public string QuantID
        {
            get
            {
                return quantid;
            }
            set
            {
                this.quantid = value;
                this.OnPropertyChanged(nameof(QuantID));
            }
        }


        private string timeinforce;
        public string TimeInForce
        {
            get
            {
                return timeinforce;
            }
            set
            {
                this.timeinforce = value;
                this.OnPropertyChanged(nameof(TimeInForce));
            }
        }


        private string biztype;
        public string BizType
        {
            get
            {
                return biztype;
            }
            set
            {
                this.biztype = value;
                this.OnPropertyChanged(nameof(BizType));
            }
        }

        private string biztypeid;
        public string BizTypeID
        {
            get
            {
                return biztypeid;
            }
            set
            {
                this.biztypeid = value;
                this.OnPropertyChanged(nameof(BizTypeID));
            }
        }


        private string quoteid;
        public string QuoteID
        {
            get
            {
                return quoteid;
            }
            set
            {
                this.quoteid = value;
                this.OnPropertyChanged(nameof(QuoteID));
            }
        }



        private string roleid;
        public string RoleID
        {
            get
            {
                return roleid;
            }
            set
            {
                this.roleid = value;
                this.OnPropertyChanged(nameof(RoleID));
            }
        }


        private string rolename;
        public string RoleName
        {
            get
            {
                return rolename;
            }
            set
            {
                this.rolename = value;
                this.OnPropertyChanged(nameof(RoleName));
            }
        }




        private string userid;
        public string UserID
        {
            get
            {
                return userid;
            }
            set
            {
                this.userid = value;
                this.OnPropertyChanged(nameof(UserID));
            }
        }


        private string showtrader;
        public string ShowTrader
        {
            get
            {
                return showtrader;
            }
            set
            {
                this.showtrader = value;
                this.OnPropertyChanged(nameof(ShowTrader));
            }
        }


        private string terminal;
        public string Terminal
        {
            get
            {
                return terminal;
            }
            set
            {
                this.terminal = value;
                this.OnPropertyChanged(nameof(Terminal));
            }
        }



        private string info3;
        public string Info3
        {
            get
            {
                return info3;
            }
            set
            {
                this.info3 = value;
                this.OnPropertyChanged(nameof(Info3));
            }
        }



        private string exectype;
        public string ExecType
        {
            get
            {
                return exectype;
            }
            set
            {
                this.exectype = value;
                this.OnPropertyChanged(nameof(ExecType));
            }
        }


        private string _warningStatus;
        public string warningStatus
        {
            get
            {
                return _warningStatus;
            }
            set
            {
                this._warningStatus = value;
                this.OnPropertyChanged(nameof(warningStatus));
            }
        }


        private string ioctimeout;
        public string IocTimeOut
        {
            get
            {
                return ioctimeout;
            }
            set
            {
                this.ioctimeout = value;
                this.OnPropertyChanged(nameof(IocTimeOut));
            }
        }

        private string _ordertype;
        public string ordertype
        {
            get
            {
                return _ordertype;
            }
            set
            {
                this._ordertype = value;
                this.OnPropertyChanged(nameof(ordertype));
            }
        }

        private string validuntiltime;
        public string ValidUntilTime
        {
            get
            {
                return validuntiltime;
            }
            set
            {
                this.validuntiltime = value;
                this.OnPropertyChanged(nameof(ValidUntilTime));
            }
        }


        private string ispoint;
        public string IsPoint
        {
            get
            {
                return ispoint;
            }
            set
            {
                this.ispoint = value;
                this.OnPropertyChanged(nameof(IsPoint));
            }
        }

        private string remainseconds;
        public string RemainSeconds
        {
            get
            {
                return remainseconds;
            }
            set
            {
                this.remainseconds = value;
                this.OnPropertyChanged(nameof(RemainSeconds));
            }
        }


        private string partysubname;
        public string PartySubName
        {
            get
            {
                return partysubname;
            }
            set
            {
                this.partysubname = value;
                this.OnPropertyChanged(nameof(PartySubName));
            }
        }


        private string counterpartyname;
        public string CounterPartyName
        {
            get
            {
                return counterpartyname;
            }
            set
            {
                this.counterpartyname = value;
                this.OnPropertyChanged(nameof(CounterPartyName));
            }
        }


        private string info1;
        public string Info1
        {
            get
            {
                return info1;
            }
            set
            {
                this.info1 = value;
                this.OnPropertyChanged(nameof(Info1));
            }
        }



        private string rejecttext;
        public string RejectText
        {
            get
            {
                return rejecttext;
            }
            set
            {
                this.rejecttext = value;
                this.OnPropertyChanged(nameof(RejectText));
            }
        }
    }
}
