using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model
{
    public interface ISetting
    {
        void OnSave();

        string GetName();

        bool NeedSaveButton();
    }
}
