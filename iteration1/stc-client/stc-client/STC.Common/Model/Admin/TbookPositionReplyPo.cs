using STC.Common.Model.Fix.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Admin
{
    public class TbookPositionReplyPo : PResult
    {
        public TbookPositionPo Datas;
        //public List<TtrdAccBalanceSecu> htDatas;
    }

    public class TbookPositionPo
    {
        public List<PositionSum> posDatas;
        public List<TtrdAccBalanceSecu> htDatas;
        public string UpdateTime;
        public int SyncHtCount;

    }
}
