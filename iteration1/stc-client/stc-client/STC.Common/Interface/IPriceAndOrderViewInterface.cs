using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Interface
{
    public interface IPriceAndOrderViewInterface
    {
        void InitTableModel(string security, string groupId);

        void SelectionChange(string groudId, string id, string security, string settle, bool needRefresh);
    }
}
