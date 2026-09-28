using netapi.Api;
using STC.Common.Model.Admin;
using STC.Common.Model.Fix.Message;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Handler.Admin
{
    public interface IAdminHandler
    {
        TQueryMessage SearchSysMsg(SystemMessageReq request);
        TQueryMessage SearchSysMsgmvt(SystemMessageReq request);

        void UpdateSysMsg(SystemMessageReq request);

        void UnSubscribeAll(bool isSubscribe);

        void UnSubscribeOrder(Action<List<TTbookSystemMessage>> onMsgCallBack);

        void Init(Action<List<TTbookSystemMessage>> onAdminMsg);

        List<StcTbookUsers> GetStcTbookUsersByUser(string user);
        STCUserReplyPo stcUserAndGroupsQuery();
        TUsersQuery usersQuery(TUserRequest request = null);
        TRolesQuery rolesQuery();
        TRoleUpdate roleUpdate(RoleUpdateReq req);
        TModulesQuery modulesQuery();
        TModulesActionQuery modulesActionQuery();
        PResult rolesModulesActionUpdate(RolesModulesActionUpdateReq request);
        TRoleModulesActionQuery roleModulesActionQuery(RoleModulesActionQueryReq request);
        PResult userRoleUpdate(UserRequest request);
        STCUserReplyPo stcUserQuery();

        STCUserReplyPo GetCachestcUserQuery();
        List<StcTbookUsers> GetStcTbookUsers();

        HomeGroupUpdateReplyPo homeGroupUpdate(HomeGroupUpdateReq request);
        HomeGroupReplyPo homeGroupQuery();
        PResult homeSecurityidsUpdate(HomeGroupSecurityidsUpdateReq request);
        
        HomeGroupSecurityidsReplyPo homeGroupSecurityidsQuery(HomeGroupSecurityidsUpdateReq request);
        HomeGroupSecurityidsReplyPo GetCacheHomeGroupSecurityidsQuery();

        HomeGroupUpdateReplyPo homeGroupUpdate2(HomeGroupUpdateReq request);
        HomeGroupReplyPo homeGroupQuery2();
        PResult homeSecurityidsUpdate2(HomeGroupSecurityidsUpdateReq request);

        HomeGroupSecurityidsReplyPo homeGroupSecurityidsQuery2(HomeGroupSecurityidsUpdateReq request);
        HomeGroupSecurityidsReplyPo GetCacheHomeGroupSecurityidsQuery2();

        Module GetModuleById(string id);

        bool CheckPermisionById(string parentId, string moduleId);
        bool CheckPermission(string moduleId, string moduleActionId);

        List<ModulesAction> GetModulesActionById(string id);

        PResult BatchUpdateSysMsg(TBatchMsgReq batchMsgReq);
        PResult userUpdate(TUserRequest req);

        TSafeManger safeMangerService(TSafeMangerReq req);

        void ProcessAdminMsg(NetMessage netMsg);

        AccountAndUsersQueryReplyPo getAccountUser();

        Task queryAccountAndUsers();

        TbookSystemParameterReplyPo querySystemParameter(SystemParameterReq request);

        PResult updateSystemGlobalPara(SystemGlobalParamReq request);
        List<Module> GetWebModule();
        //BookBookReplyPo queryBookUser();
        ObservableCollection<TBookBookTreeItemModel> GetTCurrentBookCollection(string user=null);
        ObservableCollection<TBookBookTreeItemModel> GetTAllBookCollection();

        List<TTbookBook> GetTTbookBook(string user= null);

        //Dictionary<string, List<TTbookBook>> GetDicTTbook(string user = null);
        void InitBookDicByUser(string user);
        Dictionary<string, string> GetAllBookDic();
        Dictionary<string, ObservableCollection<TBookBookTreeItemModel>> GetDicTbookAllUser();
        TTbookBook GetTTbookByID(string id);
        List<TTbookBook> GetChildTTbookByID(string id);
        Dictionary<string, List<TTbookBook>> GetDicTTbook(string user = null);

        TbookPositionReplyPo queryPosition(PositionReq request);
        TbookTradeReplyPo queryTrade(PositionReq request);
        BookBookReplyPo QueryBookUserByExternal();
        void QuerySecurityGroupModule();
        string getSecurityGroupUser();
        bool getManageYourselfViewAll();
        ObservableCollection<TBookBookTreeItemModel> GetTParentBookCollection();

        PResult WriteDBLog(DBLogRequest req);

        TbookUserGroupsReplyPo queryUserGroup();

        TbookUserGroups GetUserGroup(string biztypeid);

        void queryManagerUser();

        List<string> GetTraderList(string managerUserName, string type = null);

        List<string> GetManagerList(string traderUserName, string type = null);
        BookBookReplyPo QueryBook();
    }
}
