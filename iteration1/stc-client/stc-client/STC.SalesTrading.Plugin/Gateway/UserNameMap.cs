using System.Collections.Generic;
using STC.Common.Handler.Admin;
using STC.Common.Model.Admin;

namespace STC.SalesTrading.Plugin.Gateway
{
    /// <summary>
    /// STC 用户主键（如 u123456789）→ 显示名映射（后端行数据的 ownerUserId 即该主键）。
    /// 来源：宿主 AdminSvr 用户缓存（AdminHandler.GetCachestcUserQuery）；
    /// 当前登录用户以 LocalEnv 三件套兜底——缓存未加载时仍可识别本人。
    /// </summary>
    internal static class UserNameMap
    {
        public static Dictionary<string, string> Build()
        {
            var names = new Dictionary<string, string>(System.StringComparer.Ordinal);
            var datas = AdminHandler.Instance != null ? AdminHandler.Instance.GetCachestcUserQuery() : null;
            var users = datas != null ? datas.Datas : null;
            if (users != null)
            {
                foreach (var user in users)
                {
                    if (user != null && !string.IsNullOrWhiteSpace(user.Userid))
                    {
                        names[user.Userid] = !string.IsNullOrWhiteSpace(user.Name) ? user.Name : user.Username;
                    }
                }
            }

            var ui = STC.Common.Utils.LocalEnv.UserInfo;
            if (ui != null && !string.IsNullOrEmpty(ui.UserId))
            {
                if (!string.IsNullOrWhiteSpace(ui.FullName))
                {
                    names[ui.UserId] = ui.FullName;
                }
                else if (!names.ContainsKey(ui.UserId))
                {
                    names[ui.UserId] = ui.UserName;
                }
            }
            return names;
        }

        /// <summary>审计操作人使用登录名，不能用姓名或当前查看者替代实际操作人。</summary>
        public static Dictionary<string, string> BuildLoginNames()
        {
            var cache = AdminHandler.Instance?.GetCachestcUserQuery();
            var currentUser = STC.Common.Utils.LocalEnv.UserInfo;
            return BuildLoginNames(cache?.Datas, currentUser?.UserId, currentUser?.UserName);
        }

        internal static Dictionary<string, string> BuildLoginNames(
            IEnumerable<StcTbookUsers> users, string currentUserId, string currentLoginName)
        {
            var names = new Dictionary<string, string>(System.StringComparer.Ordinal);
            if (users != null)
            {
                foreach (var user in users)
                {
                    if (user != null && !string.IsNullOrWhiteSpace(user.Userid)
                        && !string.IsNullOrWhiteSpace(user.Username))
                    {
                        names[user.Userid] = user.Username;
                    }
                }
            }
            if (!string.IsNullOrWhiteSpace(currentUserId) && !string.IsNullOrWhiteSpace(currentLoginName))
            {
                names[currentUserId] = currentLoginName;
            }
            return names;
        }

        internal static string ResolveLoginName(string userId, IReadOnlyDictionary<string, string> names)
        {
            string loginName;
            return !string.IsNullOrWhiteSpace(userId) && names.TryGetValue(userId, out loginName)
                ? loginName : "未知用户";
        }

        /// <summary>当前登录用户的主键（LocalEnv；与后端行数据 ownerUserId 同一体系）。</summary>
        public static string CurrentUserId
        {
            get
            {
                var ui = STC.Common.Utils.LocalEnv.UserInfo;
                return ui != null ? ui.UserId : null;
            }
        }
    }
}
