using STC.Common.Utils;
using STC.Common.Utils.Dps;
using STC.Common.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Frame.ViewModel
{
    public class NetworkSettingViewModel : ViewModelBase
    {
        /// <summary>
        /// 1:不使用代理
        /// 2:使用与Internet Explorer相同的代理
        /// 3:https
        /// </summary>
        private string _accessType = "1";

        public string AccessType
        {
            get { return _accessType; }
            set {
                _accessType = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(ProxyHost));
                this.OnPropertyChanged(nameof(ProxyPort));
                this.OnPropertyChanged(nameof(ProxyUser));
                this.OnPropertyChanged(nameof(ProxyPassword));
            }
        }

        /// <summary>
        /// 代理地址
        /// </summary>
        private string _proxyHost;

        public string ProxyHost
        {
            get { return _proxyHost; }
            set
            {
                _proxyHost = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 代理端口
        /// </summary>
        private string _proxyPort;

        public string ProxyPort
        {
            get { return _proxyPort; }
            set
            {
                _proxyPort = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 代理用户
        /// </summary>
        private string _proxyUser;

        public string ProxyUser
        {
            get { return _proxyUser; }
            set
            {
                _proxyUser = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 代理密码
        /// </summary>
        private string _proxyPassword;

        public string ProxyPassword
        {
            get { return _proxyPassword; }
            set
            {
                _proxyPassword = value;
                this.OnPropertyChanged();
            }
        }

        public NetworkSettingViewModel()
        {
            ProxyInfo proxyInfo = ProxyUtils.ReadProxyInfo();
            this.AccessType = string.IsNullOrEmpty(proxyInfo.AccessType) ? "1" : proxyInfo.AccessType;
            this.ProxyHost = proxyInfo.ProxyAddress;
            this.ProxyPort = proxyInfo.ProxyPort;
            this.ProxyUser = proxyInfo.ProxyUser;
            this.ProxyPassword = proxyInfo.ProxyPasswd;
        }

        public async void SaveProxyInfo()
        {
            // 保存代理信息
            ProxyInfo proxyInfo = new ProxyInfo();
            proxyInfo.AccessType = this.AccessType;
            proxyInfo.ProxyType = "3";//(1:socks4 2:socks5 3:https)
            proxyInfo.ProxyUser = this.ProxyUser;
            proxyInfo.ProxyPasswd = this.ProxyPassword;
            proxyInfo.ProxyAddress = this.ProxyHost;
            proxyInfo.ProxyPort = this.ProxyPort;
            ProxyUtils.SaveProxyInfo(proxyInfo);

            DpsUtil.Instance.SetDpsProxyInfo(proxyInfo);

            // 重连dps
            await Task.Run(() => {
                DpsUtil.Instance.StopDps();
            });
        }
    }
}
