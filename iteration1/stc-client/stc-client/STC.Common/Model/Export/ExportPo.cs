using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Export
{
    /// <summary>
    /// 导出对象
    /// </summary>
    public class ExportPo
    {
        private string side;
        private string securityID;
        private string symbol;
        private string settlType;
        private string firmFlag;
        private string orderID;
        private string timeInForce;
        private string orderStatus;
        private string orderStatus2;
        private string price;
        private string netPrice;
        private string orderQty;
        private string execQty;
        private string myDealer;
        private string veneues;
        private string crossVenues;
        private string venuesSelectDisplay;
        private string updateTime;
        private string updateDate;
        private string strategyOrderId;
        private string execID;
        private string clOrdID;
        private string lastPx;
        private string lastQty;
        private string executionTime;
        private string counterPartyName;
        private string counterPartySubName;
        private string executionDate;
        private string syncStatus;
        private string refOrderID;
        private string remark;
        private string orderid;
        private string tradedate;
        private string createtime;
        private string type;
        private string indicator;
        private string indicatorvalue;
        private string indicatorlimitvalue;
        private string indicatorsource;
        private string result;
        private string contentType;
        private string level;
        private string headerTitle;
        private string content;
        private string source;
        private string ptime;
        private string presult;
        private string createdate;
        private string price2;
        private string netPrice2;
        private string qty2;
        private string execQty2;
        private string wholeFlag;
        private string sendOrderType;
        private string bBVol;
        private string bBPx;
        private string bOVol;
        private string bOPx;
        private string isPoint;
        private string offsetFlag;
        private string bookName;
        private string quantID;
        private string info4;

        public string STCCode
        {
            get
            {
                return orderID;
            }

            set
            {
                orderID = value;
            }
        }

        public string Qty
        {
            get
            {
                return orderQty;
            }

            set
            {
                orderQty = value;
            }
        }

        public string Side
        {
            get
            {
                return side;
            }

            set
            {
                side = value;
            }
        }

        public string SecurityID
        {
            get
            {
                return securityID;
            }

            set
            {
                securityID = value;
            }
        }

        public string Symbol
        {
            get
            {
                return symbol;
            }

            set
            {
                symbol = value;
            }
        }

        public string SettlType
        {
            get
            {
                return settlType;
            }

            set
            {
                settlType = value;
            }
        }

        public string FirmFlag
        {
            get
            {
                return firmFlag;
            }

            set
            {
                firmFlag = value;
            }
        }

        public string TimeInForce
        {
            get
            {
                return timeInForce;
            }

            set
            {
                timeInForce = value;
            }
        }

        public string OrderStatus
        {
            get
            {
                return orderStatus;
            }

            set
            {
                orderStatus = value;
            }
        }

        public string OrderStatus2
        {
            get
            {
                return orderStatus2;
            }

            set
            {
                orderStatus2 = value;
            }
        }

        public string Price
        {
            get
            {
                return price;
            }

            set
            {
                price = value;
            }
        }

        public string NetPrice
        {
            get
            {
                return netPrice;
            }

            set
            {
                netPrice = value;
            }
        }

        public string ExecQty
        {
            get
            {
                return execQty;
            }

            set
            {
                execQty = value;
            }
        }

        public string MyDealer
        {
            get
            {
                return myDealer;
            }

            set
            {
                myDealer = value;
            }
        }

        public string Veneues
        {
            get
            {
                return veneues;
            }

            set
            {
                veneues = value;
            }
        }

        public string CrossVenues
        {
            get
            {
                return crossVenues;
            }

            set
            {
                crossVenues = value;
            }
        }

        public string VenuesSelectDisplay
        {
            get
            {
                return venuesSelectDisplay;
            }

            set
            {
                venuesSelectDisplay = value;
            }
        }

        public string UpdateTime
        {
            get
            {
                return updateTime;
            }

            set
            {
                updateTime = value;
            }
        }

        public string UpdateDate
        {
            get
            {
                return updateDate;
            }

            set
            {
                updateDate = value;
            }
        }

        public string StrategyOrderId
        {
            get
            {
                return strategyOrderId;
            }

            set
            {
                strategyOrderId = value;
            }
        }

        public string ExecID
        {
            get
            {
                return execID;
            }

            set
            {
                execID = value;
            }
        }

        public string ClOrdID
        {
            get
            {
                return clOrdID;
            }

            set
            {
                clOrdID = value;
            }
        }

        public string LastPx
        {
            get
            {
                return lastPx;
            }

            set
            {
                lastPx = value;
            }
        }

        public string LastQty
        {
            get
            {
                return lastQty;
            }

            set
            {
                lastQty = value;
            }
        }

        public string ExecutionTime
        {
            get
            {
                return executionTime;
            }

            set
            {
                executionTime = value;
            }
        }

        public string CounterPartyName
        {
            get
            {
                return counterPartyName;
            }

            set
            {
                counterPartyName = value;
            }
        }

        public string CounterPartySubName
        {
            get
            {
                return counterPartySubName;
            }

            set
            {
                counterPartySubName = value;
            }
        }

        public string ExecutionDate
        {
            get
            {
                return executionDate;
            }

            set
            {
                executionDate = value;
            }
        }

        public string SyncStatus
        {
            get
            {
                return syncStatus;
            }

            set
            {
                syncStatus = value;
            }
        }

        public string RefOrderID
        {
            get
            {
                return refOrderID;
            }

            set
            {
                refOrderID = value;
            }
        }

        public string Remark
        {
            get
            {
                return remark;
            }

            set
            {
                remark = value;
            }
        }

        public string Orderid
        {
            get
            {
                return orderid;
            }

            set
            {
                orderid = value;
            }
        }

        public string Tradedate
        {
            get
            {
                return tradedate;
            }

            set
            {
                tradedate = value;
            }
        }

        public string Createtime
        {
            get
            {
                return createtime;
            }

            set
            {
                createtime = value;
            }
        }

        public string Type
        {
            get
            {
                return type;
            }

            set
            {
                type = value;
            }
        }

        public string Indicator
        {
            get
            {
                return indicator;
            }

            set
            {
                indicator = value;
            }
        }

        public string Indicatorvalue
        {
            get
            {
                return indicatorvalue;
            }

            set
            {
                indicatorvalue = value;
            }
        }

        public string Indicatorlimitvalue
        {
            get
            {
                return indicatorlimitvalue;
            }

            set
            {
                indicatorlimitvalue = value;
            }
        }

        public string Indicatorsource
        {
            get
            {
                return indicatorsource;
            }

            set
            {
                indicatorsource = value;
            }
        }

        public string Result
        {
            get
            {
                return result;
            }

            set
            {
                result = value;
            }
        }

        public string ContentType
        {
            get
            {
                return contentType;
            }

            set
            {
                contentType = value;
            }
        }

        public string Level
        {
            get
            {
                return level;
            }

            set
            {
                level = value;
            }
        }

        public string HeaderTitle
        {
            get
            {
                return headerTitle;
            }

            set
            {
                headerTitle = value;
            }
        }

        public string Content
        {
            get
            {
                return content;
            }

            set
            {
                content = value;
            }
        }

        public string Source
        {
            get
            {
                return source;
            }

            set
            {
                source = value;
            }
        }

        public string Ptime
        {
            get
            {
                return ptime;
            }

            set
            {
                ptime = value;
            }
        }

        public string Presult
        {
            get
            {
                return presult;
            }

            set
            {
                presult = value;
            }
        }

        public string Createdate
        {
            get
            {
                return createdate;
            }

            set
            {
                createdate = value;
            }
        }

        public string Price2 {
            get
            {
                return price2;
            }

            set
            {
                price2 = value;
            }
        }
        public string NetPrice2 {
            get
            {
                return netPrice2;
            }

            set
            {
                netPrice2 = value;
            }
        }
        public string Qty2 {
            get
            {
                return qty2;
            }

            set
            {
                qty2 = value;
            }
        }
        public string ExecQty2 {
            get
            {
                return execQty2;
            }

            set
            {
                execQty2 = value;
            }
        }

        public string WholeFlag
        {
            get
            {
                return wholeFlag;
            }

            set
            {
                wholeFlag = value;
            }
        }

        public string SendOrderType
        {
            get
            {
                return sendOrderType;
            }

            set
            {
                sendOrderType = value;
            }
        }
        
        public string BBVol
        {
            get
            {
                return bBVol;
            }

            set
            {
                bBVol = value;
            }
        }
        public string BBPx
        {
            get
            {
                return bBPx;
            }

            set
            {
                bBPx = value;
            }
        }
        public string BOVol
        {
            get
            {
                return bOVol;
            }

            set
            {
                bOVol = value;
            }
        }
        public string BOPx
        {
            get
            {
                return bOPx;
            }

            set
            {
                bOPx = value;
            }
        }
        public string IsPoint
        {
            get
            {
                return isPoint;
            }

            set
            {
                isPoint = value;
            }
        }

        public string OffsetFlag
        {
            get
            {
                return offsetFlag;
            }

            set
            {
                offsetFlag = value;
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

        public string QuantID
        {
            get
            {
                return quantID;
            }

            set
            {
                quantID = value;
            }
        }

        public string Info4
        {
            get
            {
                return info4;
            }

            set
            {
                info4 = value;
            }
        }


    }
}
