using STC.Common.Data;
using STC.Common.Model.Table.WorkBrench;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.View.Order.InnerPanel
{
    public class InnerPanelHelper
    {

        public static PriceMatchParam IntentionToPriceParam(IntentionQuoteInfo intention)
        {
            PriceMatchParam result = new PriceMatchParam();
            result.CustomFirmAmount = intention.qty==null? string.Empty: intention.qty.Value.ToString();
            result.Side = intention.side;
            result.Symbol = intention.securityId;
            result.SymbolName = intention.symbol;
            result.WholeFlag = intention.wholeFlag;
            result.NetPrice = intention.cleanPrice == null ? string.Empty : intention.cleanPrice.Value.ToString();
            if (Utils.CommonUtils.IsSpreadOrBasic(intention.BondID) && intention.price.HasValue)
            {
                intention.price = intention.price / 100;
            }
            result.Price = intention.price == null ? string.Empty : intention.price.Value.ToString();
            result.PriceType = intention.priceType;
            result.SettlType = intention.settlType;
            result.InstitutionID = intention.institutionId;
            result.InstitutionName = intention.institutionName;
            result.TraderID = intention.traderId;
            result.TraderName = intention.traderName;

            //IRS识别
            if (intention.qty == null)
                result.CustomFirmAmount = intention.quantity == null ? string.Empty : intention.quantity.Value.ToString();
            result.BondID = intention.BondID;
            if (!string.IsNullOrEmpty(intention.BondID))
            {
                result.Symbol = intention.BondID;
                result.SymbolName = intention.BondID;
            }

            result.TimeToMaturity = intention.timeToMaturity;
            return result;
        }


    }
}
