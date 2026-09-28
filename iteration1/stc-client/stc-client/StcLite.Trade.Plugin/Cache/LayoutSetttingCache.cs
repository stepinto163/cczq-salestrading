using com.gateway.log;
using Newtonsoft.Json;
using BDB.Trade.Plugin.Handler;
using BDB.Trade.Plugin.Model;
using BDB.Trade.Plugin.Utils;
using BDB.Trade.Plugin.ViewModel.UserSetting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using STC.Common.Utils.SqlLite;
using STC.Common.Utils;

namespace BDB.Trade.Plugin.Cache
{
    public class LayoutSetttingCache
    {
        public const string Mode = "Mode";
        private LayoutSetttingCache() { }
        private static LayoutSetttingCache server = new LayoutSetttingCache();


        public static LayoutSetttingCache Instance { get { return server; } }

        public LayoutSettingPo layoutSettingPo = new LayoutSettingPo();

        public QcPanelPoEx CurrentUpdatePanle { get; set; }


        public void SetRowCol(int row, int col)
        {
            layoutSettingPo.Row = row;
            layoutSettingPo.Col = col;
        }


        public void SetLayoutSetting(LayoutSettingPo layoutSetting)
        {
            layoutSettingPo = layoutSetting;
        }

        public LayoutSettingPo GetLayoutSetting()
        {
            return layoutSettingPo;
        }


        public void InitSetting()
        {
            QuoteConfigPo quoteConfigPo = QuoteConfigHandler.Instance.QuoteOneConfig();

            LayoutSettingPo layoutSetting = QuoteSettingMode();

            if (layoutSetting != null)
            {
                layoutSettingPo = layoutSetting;
            }
            else
            {
                SetRowCol(2, 6);
                layoutSettingPo.ShowModel = 2;
            }

        }


        public LayoutSettingPo QuoteSettingMode()
        {
            try
            {
                List<UserConfigPo> list = CardFileManager.Instance.GetAllConfigByCondtion(LocalEnv.UserInfo.UserName, "Mode");
                    //SqlliteUtils.Query<UserConfigPo>
                    //("select * from UserConfigPo where UserName=? and GroupId = 'Mode'",
                    //new object[] { LocalEnv.UserInfo.UserName });
                if (list.Count > 0)
                {
                    UserConfigPo userConfig = list[0];
                    if (!string.IsNullOrEmpty(userConfig.Config))
                    {
                        LayoutSettingPo layoutSetting = JsonConvert.DeserializeObject<LayoutSettingPo>(userConfig.Config);

                        return layoutSetting;

                    }
                }
            }
            catch (Exception e)
            {
                LogHelper.Error("LayoutSetttingCache QuoteSettingMode exception:", e);
            }
            return null;
        }

        public void SaveModeSetting()
        {
            string config = JsonConvert.SerializeObject(layoutSettingPo);
            UserConfigPo userConfig = new UserConfigPo();
            userConfig.UserName = LocalEnv.UserInfo.UserName;
            userConfig.Config = config;
            userConfig.GroupId = Mode;

            QuoteConfigHandler.Instance.IntoSaveQueu(userConfig);
            //SqlliteUtils.Replace<UserConfigPo>(userConfig);
        }
    }
}
