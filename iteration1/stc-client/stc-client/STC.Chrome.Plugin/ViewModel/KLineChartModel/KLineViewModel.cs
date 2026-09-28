using CefSharp;
using com.gateway.log;
using Newtonsoft.Json;
using STC.Chrome.Plugin.View.KLineChart;
using STC.Common.Handler.MDHist;
using STC.Common.Handler.OHLC;
using STC.Common.Model;
using STC.Common.Model.Base;
using STC.Common.Model.Chrome;
using STC.Common.Model.MDHist;
using STC.Common.Model.OHLC;
using STC.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using static STC.Common.Utils.HomePageDataHandler;

namespace STC.Chrome.Plugin.ViewModel.KLineChartModel
{
    public class KLineViewModel : BaseViewModel
    {

        private KLineView ChromeCtrl;
        private string _currentTimeType = "1M";
        private string _currentSide = "e";
        private string _preSecurityID = string.Empty;
        private bool _isFirstLoad = true;

        private object updlock = new object();


        public KLineViewModel()
        {

        }

        public KLineViewModel(KLineView brower)
        {
            ChromeCtrl = brower;
        }

        /// <summary>
        /// 债券代码
        /// </summary>
        private string _securityID = string.Empty;

        /// <summary>
        /// 债券代码
        /// </summary>
        public string SecurityID
        {
            get
            {
                return this._securityID;
            }

            set
            {
                if ((this._securityID != value))
                {

                    _preSecurityID = _securityID;

                    // 切换债券
                    this._securityID = value;

                    this.SetFavoriteSrouce(HomePageDataHandler.IsFavorite(this._securityID));
                }


                if (!string.IsNullOrEmpty(_securityID))
                {
                    LoadKLineData(_securityID, _currentTimeType, _currentSide);
                }
            }
        }
        

        /// <summary>
        /// 获取债券的成交列表，骆驼命名法
        /// </summary>
        /// <param name="securityId"></param>
        /// <returns></returns>
        public void setSecurityID(string securityId)
        {
            // 初始化实例
            this.SecurityID = securityId;
        }

        /// <summary>
        /// 搜索债券，骆驼命名法
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public string getSymbol(string code)
        {
            string res = string.Empty;
            string txt = code.Replace("'", "");
            List<SimpleBondsInfoPo> result = new List<SimpleBondsInfoPo>();
            if (txt.Contains(" "))
            {
                string[] arr = txt.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

                foreach (string str in arr)
                {
                    List<SimpleBondsInfoPo> selectDatas = CommonUtils.GetBondsInfoPo(str, 20);
                    result.AddRange(selectDatas);
                }
            }
            else
                result = CommonUtils.GetBondsInfoPo(txt.Trim(), 20);

            return JsonConvert.SerializeObject(result);
        }


        public void changeKLineData(string timeType, string side)
        {
            this.LoadKLineData(_securityID, timeType.ToUpper(), side);
        }

        /// <summary>
        /// 设置URL，骆驼命名法
        /// </summary>
        /// <param name="url"></param>
        public void setUrl(string code, string url, string securityId)
        {
            Random rd = new Random(10000);
            if (!string.IsNullOrEmpty(code))
            {
                url = AppDomain.CurrentDomain.BaseDirectory + $"bin/HTML/{url}";
            }
            else
            {
                url = AppDomain.CurrentDomain.BaseDirectory + $"bin/HTML/{url}";
            }

            ChromeCtrl.SetAddress(url);

            
        }

        private void SubscribeKLine(string securityId, string timeType, string sideType)
        {
            if (securityId == this.SecurityID)
            {
                OhlcHandler.Instance.SubscribeKLine(sideType, securityId, timeType, OnKLine);
            }
        }

        private void OnKLine(OhlcPo ohlcMsg)
        {
            kLineData lineData = new kLineData();
            lineData.Close = ohlcMsg.Close.ToString();
            lineData.FmtTime = ohlcMsg.TradeDate + " " + ohlcMsg.FmtTime.Insert(2, ":").Insert(5, ":");
            lineData.Open = ohlcMsg.Open.ToString();
            lineData.High = ohlcMsg.High.ToString();
            lineData.Low = ohlcMsg.Low.ToString();

            string res = JsonConvert.SerializeObject(lineData);
            var t = Task.Run(async () =>
            {
                await CallJsAsyncAddKLineData(res);
            });
        }

        private string GetSideType(string side)
        {
            string result = string.Empty;
            if (!string.IsNullOrEmpty(side))
            {
                if (side == "Bid(T+1)")
                    result = "02";
                else if (side == "Bid(T+0)")
                    result = "01";
                else if (side == "Ofr(T+0)")
                    result = "11";
                else if (side == "Ofr(T+1)")
                    result = "12";
                else if (side == "市场成交")
                    result = "e";
            }
            else
            {
                result = "e";
            }

            return result;
        }

        public void LoadKLineData(string securityId, string timeType, string side)
        {
            string oldTimeType = _currentTimeType;
            string oldSecurityID = _securityID;
            string oldSide = _currentSide;

            if (string.IsNullOrEmpty(_preSecurityID))
                _preSecurityID = securityId;

            _currentTimeType = timeType;
            _securityID = securityId;
            _currentSide = side;
            ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    string oldSideType = GetSideType(oldSide);
                    if (!string.IsNullOrEmpty(_preSecurityID))
                        OhlcHandler.Instance.UnSubscribeKLine(oldSideType, _preSecurityID, oldTimeType, OnKLine);

                    KLineSearchParam kLineSearchParam = new KLineSearchParam();
                    kLineSearchParam.SecurityID = securityId;//"220210.IB";
                    kLineSearchParam.Text = timeType;
                    kLineSearchParam.Field = "fmtTime,high,open,low,close,tradeDate,numTrades";
                    string sideType = GetSideType(_currentSide);
                    kLineSearchParam.Type = sideType;
                    kLineSearchParam.QueryType = "2";
                    kLineSearchParam.Count = "1000";
                    TKLineReply tKLineReply = MDHistHandler.Instance.SearchKLine(kLineSearchParam);
                    if (tKLineReply != null && !string.IsNullOrEmpty(tKLineReply.Data))
                    {
                        List<kLineData> kLineDatas = JsonConvert.DeserializeObject<List<kLineData>>(tKLineReply.Data);
                        List<kLineData> temList = new List<kLineData>();
                        int count = kLineDatas.Count - 1;
                        for (int i = count; i >= 0; i--)
                        {
                            kLineData lineData = kLineDatas[i];
                            //lineData.FmtTime = lineData.TradeDate + "T" + lineData.FmtTime.Insert(2, ":").Insert(5, ":") + ".000Z";
                            if (timeType != "1D")
                                lineData.FmtTime = lineData.TradeDate + " " + lineData.FmtTime.Insert(2, ":").Insert(5, ":");
                            else
                                lineData.FmtTime = lineData.TradeDate + " 00:00:00";

                            temList.Add(lineData);
                        }
                        string res = JsonConvert.SerializeObject(temList);
                        var t = Task.Run(async () =>
                        {
                            await CallJsAsync(res);
                        });
                    }

                    SubscribeKLine(securityId, _currentTimeType, sideType);
                }
                catch (Exception e)
                {
                    LogHelper.Error("setUrl", e);
                }
                //finally
                //{
                //    var t = Task.Run(async () =>
                //    {
                //        await CallJsAsyncIsShowLoading();
                //    });
                //}
            });
        }



        /// <summary>
        /// c# 回调 Js 方法 MG_CallBack
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private async Task CallJsAsync(string obj)
        {
            try
            {
                if (ChromeCtrl.Browser != null)
                {
                    //ShowDevTool();

                    if (_isFirstLoad)
                    {
                        Thread.Sleep(5000);//等待加载好kline
                        _isFirstLoad = false;
                    }
                    JavascriptResponse x = await ChromeCtrl.Browser.GetBrowser().MainFrame.EvaluateScriptAsync($"MG_Call('{obj}')");
                    

                    //是否执行错误
                    //string res = x.Result.ToString();

                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.Message);
            }
        }

        private async Task CallJsAsyncAddKLineData(string obj)
        {
            try
            {
                if (ChromeCtrl.Browser != null)
                {

                    JavascriptResponse x = await ChromeCtrl.Browser.GetBrowser().MainFrame.EvaluateScriptAsync($"OnKLineData('{obj}')");

                    //是否执行错误
                    //string res = x.Result.ToString();

                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.Message);
            }
        }


        private async Task CallJsAsyncResetSize()
        {
            try
            {
                if (ChromeCtrl.Browser != null)
                {

                    JavascriptResponse x = await ChromeCtrl.Browser.GetBrowser().MainFrame.EvaluateScriptAsync($"ResetSize()");

                    //是否执行错误
                    //string res = x.Result.ToString();

                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.Message);
            }
        }

        public void OnSizeChange()
        {
            var t = Task.Run(async () =>
            {
                await CallJsAsyncResetSize();
            });
        }


        private void ShowDevTool()
        {
            Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() =>
            {
                ChromeCtrl.Browser.GetBrowser().ShowDevTools();
            }));
        }


        public void Init()
        {
            HomePageDataHandler.OnFavoritesChanged += HomePageDataHandler_OnFavoritesChanged;
        }

        private void HomePageDataHandler_OnFavoritesChanged(FavoriteTypeEnum favoriteTypeEnum, string groupID, string securityID)
        {
            string isFavorite = "0";
            if (favoriteTypeEnum == FavoriteTypeEnum.AddBond
                || favoriteTypeEnum == FavoriteTypeEnum.DelBond)
            {
                if (this.SecurityID == securityID)
                {
                    isFavorite = HomePageDataHandler.IsFavorite(this.SecurityID);
                }
            }
            else if (favoriteTypeEnum == FavoriteTypeEnum.DelGroup
                || favoriteTypeEnum == FavoriteTypeEnum.BatchImport)
            {
                isFavorite = HomePageDataHandler.IsFavorite(this.SecurityID);
            }

            SetFavoriteSrouce(isFavorite);
        }

        public void SetFavoriteSrouce(string isFavorite)
        {
            if (isFavorite == "0")
                FavoriteSource = "/STC.Chrome.Plugin;component/Images/stc_icon_favorite_normal.png";
            else if (isFavorite == "1")
                FavoriteSource = "/STC.Chrome.Plugin;component/Images/stc_icon_favorite_seleted.png";
        }

        #region 界面属性绑定

        private string _favoriteSource;
        public string FavoriteSource
        {
            get { return _favoriteSource; }
            set
            {
                _favoriteSource = value;

                this.OnPropertyChanged();
            }
        }
        

        #endregion
    }
}
