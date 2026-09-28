using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace netapi.Api
{
    public class BaseConsts
    {
        //public static readonly string CONNECT_STATUS_TOPIC = "ConStatus";

        /**
        public const string JSON = "JSON";

        #region EMSSvr
        public const string EMSSVR = "EMSSvr";
        public const string Topic_stc_ems_execOrderQuery = "execOrderQuery";
        public const string Topic_stc_ems_orderQuery = "orderQuery";
        //请求
        public const string Topic_stc_ems_placeorder = "stc.ems.o.n";
        public const string Topic_stc_ems_cancelorder = "stc.ems.o.c";
        //订阅
        public const string Topic_stc_ems_executionreport_cdh = "stc.ems.c.cdh.**";
        public const string Topic_stc_ems_executionreport_cdh_prefix = "stc.ems.c.cdh.";
        public const string Topic_stc_ems_executionreport_own = "stc.ems.e.odm.**";
        public const string Topic_stc_ems_executionreport_prefix = "stc.ems.e.";
        public const string Topic_stc_ems_all = "stc.ems.e.**";
        public const string Topic_stc_ems_order = "stc.ems.v.**";
        public const string Topic_stc_ems_order_prefix = "stc.ems.v.";
        public const string Topic_stc_ems_updateExecOrder = "updateExecOrder";

        public const string Topic_stc_calSettlementDate = "calSettlementDate";

        #endregion

        #region TBookSvr
        public const string TBookSvr = "TBookSvr";
        public const string Topic_stc_tbook_orderQuery = "orderQuery";
        public const string Topic_stc_tbook_xbond = "stc.tbook.mm.xbond";
        public const string Topic_stc_tbook_order = "stc.tbook.s.**";
        public const string Topic_stc_tbook_prefix = "stc.tbook.s.";
        public const string Topic_stc_tbook_cancel = "stc.tbook.o.c";
        public const string Topic_stc_tbook_placeOrder = "stc.tbook.o.n";
        public const string Topic_stc_tbook_conStatus = "stc.tbook.t.*";
        public const string Topic_stc_tbook_con_prefix = "stc.tbook.t.";
        public const string Topic_stc_tbook_update = "stc.tbook.o.r";

        public const string SORSvrName = "SOR";
        public const string CFETSTradeSvrName = "QDM,NDM";
        public const string XBondSvrName = "ODM";
        public const string CDHSvrName = "CDH";
        public const string Price = "PRICE";
        public const string Trade = "TRADE";
        public const string Inner = "INNER";
        public const string Topic_stc_tbook_system_param = "stc.tbook.systemparas.*";
        public const string Topic_stc_tbook_system_param_all = "stc.tbook.systemparas.all";
        public const string Topic_systemParasUpdate = "systemParasUpdate";
        #endregion

        #region MDSvr
        public const string SOR = "SOR";
        public const string CT = "CT";
        public const string H0 = "h0";
        public const string MDSvr = "MDSvr";
        public const string Topic_stc_md_prefix = "stc.md.";
        public const string Topic_stc_md_prefix_best = "stc.md.h0";
        public const string Topic_stc_md_prefix_ct = "stc.md.ct";
        public const string Topic_stc_md_securityidQuery = "mdSecurityIdQuery";
        public const string Topic_stc_md_prefix_n = "stc.md.p.n";

        #endregion

        #region BaseSvr

        public const string BaseSvr = "BaseSvr";
        public const string Topic_stc_base_search = "stc.base.search";
        public const string Topic_stc_base_clear = "stc.base.clear";
        public const string Topic_stc_queryPartyDealerInfo = "queryPartyDealerInfo";
        #endregion

        #region AdminSvr
        public const string AdminSvr = "AdminSvr";
        public const string Topic_stc_admin_msg = "stc.admin.message";

        public const string Topic_stc_admin_search = "stc.sc.a.s";
        public const string Topic_stc_admin_searchmvt = "stc.sc.a.smvt";
        public const string Topic_stc_admin_update = "stc.sc.a.u";
        public const string Topic_stc_admin_prefix = "stc.sc.sysMessage.";
        public const string Topic_stc_usersQuery = "usersQuery";
        public const string Topic_stc_rolesQuery = "rolesQuery";
        public const string Topic_stc_roleUpdate = "roleUpdate";
        public const string Topic_stc_modulesQuery = "modulesQuery";
        public const string Topic_stc_modulesActionQuery = "modulesActionQuery";
        public const string Topic_stc_rolesModulesActionUpdate = "rolesModulesActionUpdate";
        public const string Topic_stc_roleModulesActionQuery = "roleModulesActionQuery";
        public const string Topic_stc_userQuery = "accountInfoQuery";
        public const string Topic_stc_userRoleUpdate = "userRoleUpdate";
        public const string Topic_stc_homeGroupUpdate = "homeGroupUpdate";
        public const string Topic_stc_homeGroupQuery = "queryHomeGroup";
        public const string Topic_stc_homeGroupSecurityIdsUpdate = "homeSecurityidsUpdate";
        public const string Topic_stc_homeGroupSecurityIdsQuery = "homeGroupSecurityidsQuery";
        public const string Topic_stc_userAndGroupsQuery = "accountAndGroupsQuery";
        #endregion

        #region RMQSvr
        public const string RMQSvr = "RMQSvr";
        public const string Topic_stc_BlockQuery = "blockQuery";
        public const string Topic_stc_BlockNumQuery = "blockNumQuery";
        public const string Topic_stc_BlockUpdate = "blockUpdate";
        public const string Topic_stc_BlockDelete = "blockDelete";
        public const string Topic_stc_BlockReSend = "blockReSend";
        public const string Topic_stc_SorExecAckQuery = "sorExecAckQuery";

        public const string Topic_stc_Block = "stc.rmq.blocknum.execorders";
        #endregion

        #region common
        public const string CDH = "CDH";
        #endregion

        #region CalSvr
        public const string CalSvr = "CalSvr";
        public const string Topic_stc_calbond = "stc.broker.calbond";
        
        #endregion
        #region RiskSvr
        public const string RiskSvr = "RiskSvr";
        public const string Topic_stc_riskParaQuery = "queryRiskParas";
        public const string Topic_stc_riskParaUpdate = "updateRiskParas";

        public const string Topic_stc_risk_prefix = "stc.risk.r.";
        public const string Topic_stc_riskLogprefix = "queryRiskLog";
        #endregion

        #region Inner

        public const string InnerSvr = "INNERSvr";
        public const string Topic_stc_inner_order = "stc.inner.canal";
        public const string Topic_stc_inner_queryInnerOrder = "queryInnerOrder";
        public const string Topic_stc_inner_transaction = "transaction";
        public const string Topic_stc_inner_cancelOrder = "cancelOrder";
        public const string Topic_stc_inner_fakTransaction = "fakTransaction";
        public const string Topic_stc_inner_fakCancel = "fakCancel";
        #endregion

    **/
    }
}
