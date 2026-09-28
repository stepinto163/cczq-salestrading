namespace STC.SalesTrading.Plugin.Shell
{
    /// <summary>
    /// 当前登录用户资料（开发期 Mock；接网关后由鉴权上下文填充）。
    /// 对应原型 currentUser：name/roleName/dept/email/phone，用于个人中心弹窗展示。
    /// </summary>
    public class UserProfile
    {
        public string Name { get; set; } = "admin";

        public string RoleName { get; set; } = "交易员";

        public string Dept { get; set; } = "固定收益部";

        public string Email { get; set; } = "admin@cw.com";

        public string Phone { get; set; } = "138-0000-0000";

        /// <summary>头像首字母。</summary>
        public string Initial
        {
            get
            {
                var n = Name ?? string.Empty;
                return string.IsNullOrWhiteSpace(n) ? "A" : n.Substring(0, 1).ToUpperInvariant();
            }
        }
    }
}