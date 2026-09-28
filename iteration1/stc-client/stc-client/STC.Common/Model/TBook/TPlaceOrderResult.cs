using STC.Common.Model.Fix.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.TBook
{
    public class TPlaceOrderResult
    {
        public string Code; //"code": "10001",//状态码  10001-提醒、10002-禁止
        public bool DoubleWarn; //"doubleWarn": false,//是否二次提醒
        public string Indicator; //"indicator": "单笔限额",//指标
        public string Indicatorlimitvalue; //"indicatorlimitvalue": "100000000000",//指标限额
        public string Indicatorsource; //"indicatorsource": "STC",//来源
        public string Indicatorvalue; //"indicatorvalue": "10000000",//指标值
        public string Reason; //"reason": "单笔报价量,限额(100000000000,200000002000) 万",//风控不通过的原因
        public string Result; //"result": "取消报价",//风控结果
        public string Type; //"type": "1"//风控类型：1-提醒、2-禁止、3-2次提醒
        public string RequestID;//请求订单编号

        public string State; //仅仅客户端使用， 用于记录批量操作后的操作状态
        public string riskType; ////0,单笔报量，1,中债估值，2,市场偏离，3.市场倒挂，4、成交偏离

        // 客户端自定义错误信息
        public string Side;
        public string SecurityId;
        public decimal? Price;
        public decimal? Qty;
        public List<string> bookIdList;
    }
}
