using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.ColumnSetting
{
    public interface ISettingColumnsBase
    {
        ObservableCollection<ColumnsInfo> GetColumnsInfos();

        string GetDefaultColumnsInfoStr();

        void SaveColumnInfo(ObservableCollection<ColumnsInfo> _MyColumnsInfo);

        void DeleteColumnInfo();

        void InitColumnsInfo(string columnsInfoStr = null);

        string GetName();
    }
}
