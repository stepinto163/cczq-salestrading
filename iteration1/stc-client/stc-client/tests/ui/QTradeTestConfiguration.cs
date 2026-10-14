using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SalesTrading.UiTests
{
    internal static class QTradeTestConfiguration
    {
        // 复用所选客户端环境的部署配置，仅向需要接入 API 的工作进程传递凭据。
        internal static string LoadToken(string configFile, string atsRoot)
        {
            string root = string.IsNullOrWhiteSpace(atsRoot)
                ? Path.Combine(Path.GetDirectoryName(Path.GetFullPath(configFile)), "..", "ats_root")
                : atsRoot;
            string authPath = Path.GetFullPath(Path.Combine(root, "control", "QtIntakeSvrAuth.properties"));
            if (!File.Exists(authPath))
            {
                throw new InvalidOperationException("所选环境缺少部署生成的 QT 共享配置：" + authPath);
            }
            foreach (string line in File.ReadLines(authPath))
            {
                int separator = line.IndexOf('=');
                if (separator < 0 || line.Substring(0, separator).Trim() != "intake.token")
                {
                    continue;
                }
                string token = line.Substring(separator + 1).Trim();
                if (!string.IsNullOrWhiteSpace(token))
                {
                    return token;
                }
            }
            throw new InvalidOperationException("QT 共享配置缺少 intake.token：" + authPath);
        }

        // 收件人来自实际登录会话；不能用用例常量或默认 QT 账号代替缺失映射。
        internal static string ResolveReceiver(string userId, IEnumerable<KeyValuePair<string, string>> users)
        {
            var matches = users.Where(user => user.Key == userId).ToList();
            if (string.IsNullOrWhiteSpace(userId) || matches.Count != 1)
            {
                throw new InvalidOperationException("用户目录无法唯一定位当前登录用户 " + userId + "；尚未发布消息。");
            }
            string account = (matches[0].Value ?? "").Trim();
            if (!IsNumericAccount(account))
            {
                throw new InvalidOperationException("当前登录用户 " + userId
                    + " 的 stc_tbook_users.inf1 未配置有效 QT 账号；尚未发布消息。");
            }
            if (users.Count(user => (user.Value ?? "").Trim() == account) != 1)
            {
                throw new InvalidOperationException("当前登录用户的 QT 账号映射不唯一；请修正 inf1，尚未发布消息。");
            }
            return account;
        }

        internal static bool IsNumericAccount(string value)
        {
            return !string.IsNullOrWhiteSpace(value) && value.All(character => character >= '0' && character <= '9');
        }
    }
}
