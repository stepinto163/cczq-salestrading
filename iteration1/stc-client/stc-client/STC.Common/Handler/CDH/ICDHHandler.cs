using STC.Common.Model;
using STC.Common.Model.Ans;
using STC.Common.Model.CDH;
using STC.Common.Model.Fix.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Handler.CDH
{
    public interface ICDHHandler
    {
        PResult EnterMarketRequest(QmDataInfo request);
    }
}
