using STC.Common.Model.QQ;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Handler
{
    public interface IIMHandler
    {
        void Init(Action<List<QQUserConfigPo>> onQQMsg);
        void QQUserConfigQuery();
    }
}
