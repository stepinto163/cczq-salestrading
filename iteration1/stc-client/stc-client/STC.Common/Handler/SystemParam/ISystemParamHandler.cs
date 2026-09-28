using STC.Common.Model.Fix.Message;
using STC.Common.Model.TBook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Handler.SystemParam
{
    public interface ISystemParamHandler
    {
        void Init();

        List<string> GetAllVenue();
        UserSetting GetUserSetting();

        List<UserVenue> GetUserVenue();

        void SetUserSetting(UserSetting userSetting);

        List<UserVenue> GetUserVenueByCondition(string goodType, string oprationType);

        Venue GetVenueByCondition(string goodsType, string venue);

        List<Venue> GetAllVenueByCondition(string goodsType, string level);

        List<Venue> GetListVenue(string goodsType, string level);

        List<Venue> GetListVenueByCondition(string goodsType, string level);

        List<Venue> GetListVenueByFilter(string goodsType, string level); 

         PResult UpdatesystemParas(UserSetting request);

        UserSetting CopyUserSetting();

        void CopyParam(UserSetting newSetting);

        string GetFirmtonontime();

        List<string> GetCustomVenueSort();

        string GetVenueSort(string venue);
    }
}
