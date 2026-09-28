using com.gateway.log;
using STC.Common.Api;
using STC.Common.Handler.Admin;
using STC.Common.Handler.Cal;
using STC.Common.Model.Base;
using STC.Common.Model.Cal;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.MM;
using STC.Common.Utils;
using STC.Common.View.Component;
using STC.Common.View.UI;
using STC.Frame.CustomUI.Component;
using STC.Mm.Plugin.Command;
using STC.Mm.Plugin.ViewModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace STC.Mm.Plugin.Utils
{
    /// <summary>
    /// 工具类
    /// </summary>
    public class MMCommonUtils
    {
        private static string BridgeWinName = "OrderControl";

        /// <summary>
        /// 根据bond_subType和 ent_cor获得债券类型名称
        /// </summary>
        /// <param name="bond_subType"></param>
        /// <returns>债券类型名称</returns>
        public static string GetBondCategoryName(string securityTypeID)
        {
            if (string.IsNullOrEmpty(securityTypeID) == false)
            {
                switch (securityTypeID)
                {
                    // 【利率债】 
                    case "100001":
                        return "国债";
                    case "100003":
                        return "政策性金融债";
                    case "100011":
                        return "地方政府债";
                    case "100002":
                        return "央票";
                    // 【信用债】
                    case "100027":
                    case "100052":
                        return "政府支持机构债";
                    case "100004":
                    case "100006":
                    case "100010":
                    case "100029":
                    case "100050":
                    case "100055":
                    case "100057":
                    case "100073":
                    case "100025":
                        return "公司债";
                    case "100005":
                    case "100007":
                    case "100009":
                    case "100022":
                    case "100023":
                    case "100024":
                    case "100026":
                    case "100028":
                    case "100053":
                    case "100054":
                    case "100056":
                    case "100082":
                    case "100083":
                        return "金融债";
                    case "100041":
                        return "NCD";
                    case "100071":
                    case "100072":
                        return "ABS";
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// 格式化小数位为4位
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string Format4Digit(string s)
        {
            if (string.IsNullOrEmpty(s)) return "--";

            string text = s;

            int totalWidth = s.Length;
            int pos = s.IndexOf(".");
            if (pos > 0)
            {
                if (s.Substring(pos + 1, s.Length - pos - 1).Length < 4)
                {
                    totalWidth = totalWidth + 4 - s.Substring(pos + 1, s.Length - pos - 1).Length;
                }
            }
            else
            {
                s += ".";
                totalWidth = s.Length + 4;
            }

            text = s.PadRight(totalWidth, '0');

            return text;
        }

        public static bool IsDecimal(string str)
        {
            bool bRet = false;
            decimal d;
            bRet = decimal.TryParse(str, out d);

            if (str != d.ToString())
            {
                bRet = false;
            }

            return bRet;
        }

        public static OrderParamProp OrderParamPropToDefault(OrderParamProp req)
        {
            OrderParamProp retReq = req;
            MMBondParamsSetting paramsSetting = getBondParamsSetting("Default");
            if (paramsSetting != null)
            {
                copySettingToData(retReq, paramsSetting,true);
            }
            else
            {
                retReq.circulSize = string.Empty;
                retReq.riskMinQtySize = string.Empty;
                retReq.riskMaxQtyRatio = string.Empty;
                retReq.securityid = string.Empty;
                retReq.symbol = string.Empty;
                retReq.bidBasePriority = "1,2";
                retReq.bidDefenCalPrice = string.Empty;
                retReq.bidAlgoCalPrice = string.Empty;
                retReq.bidQuoteModel = "Defend";
                retReq.bidBaseQty = "1";
                retReq.bidBasePrice = string.Empty;

                retReq.bidBaseReSendValue = "0";
                retReq.bidBaseWidenValue = "2.5";
                retReq.BaseERValue = "0";
                retReq.BaseERReSendType = "1";
                retReq.baseSettleType = "2";
                retReq.ofrBasePriority = "1,2";
                retReq.ofrDefenCalPrice = string.Empty;
                retReq.ofrAlgoCalPrice = string.Empty;
                retReq.ofrQuoteModel = "Defend";
                retReq.ofrBaseQty = "1";

                retReq.ofrBasePrice = string.Empty;
                retReq.ofrBaseReSendValue = "0";
                retReq.ofrBaseWidenValue = "2.5";
                retReq.bidAlgoDefenValue = "1";
                retReq.bidAlgoSafeValue = "0";
                retReq.bidAlgoTrigValue = "0";
                retReq.ofrAlgoDefenValue = "1";
                retReq.ofrAlgoSafeValue = "0";
                retReq.ofrAlgoTrigValue = "0";
                retReq.algoEnable = "0";

                retReq.algoBaseEnable = "0";
                retReq.algoBaseSecurityid = string.Empty;
                retReq.algoMidType = "2";
                retReq.algoMidDetail = "0";
                retReq.algoMidDetailDefined = "0";
                retReq.algoMidOffsetValue = "0";
                retReq.algoValue = "10";
                retReq.algoCDCEnable = "0";
                retReq.algoCDCDetail = "0";
                retReq.bidAlgoCDCValue = "1";

                retReq.ofrAlgoCDCValue = "1";
                retReq.posLmtEnable = "0";
                retReq.bidPosValue = "0";
                retReq.ofrPosValue = "0";
                retReq.posFakEnable = "1";
                retReq.posFakSpread = "0";
                retReq.dealWidenLowerTime = "30";
                retReq.dealWidenUpperTime = "30";
                retReq.globalWallBid = "0";
                retReq.globalWallOfr = "0";
                retReq.bidStrategyType = "Defend";

                retReq.ofrStrategyType = "Defend";
                retReq.apiBid = string.Empty;
                retReq.apiOfr = string.Empty;
                retReq.apiBidQty = string.Empty;
                retReq.apiOfrQty = string.Empty;
                retReq.apiLastTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                retReq.manualBid = "0";
                retReq.manualOfr = "0";
                retReq.isQuoteSuccess = "0";
                retReq.lastOptTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                retReq.stepLength = "0.25";
                retReq.maxFloorEnable = "0";
                retReq.maxFloor = string.Empty;
                retReq.openQDM2Enable = "0";
                retReq.defenCDCEnable = "1";
                retReq.bidDefenCDCValue = "4.5";
                retReq.ofrDefenCDCValue = "4.5";
                retReq.defenCDCDetail = "0";
                retReq.bidDefenReferMarketType = "1";
                retReq.bidDefenReferLevel = "1";
                retReq.ofrDefenReferMarketType = "1";
                retReq.ofrDefenReferLevel = "1";
                retReq.wallEnable = "1";

                retReq.algoSafeEnable = "1";
                retReq.secondSafeEnable = "1";
                retReq.qdmespSafeEnable = "1";
                retReq.qdmespSafeLevel = "1";
                retReq.bidQdmespSafeValue = "0";
                retReq.ofrQdmespSafeValue = "0";
            }
            retReq.exeBondFollow = "1";
            return retReq;
        }

        public static bool CheckDecimalValid(string str, decimal max, decimal min,int scale, bool isMaxOpenSet = false, bool isMinOpenSet = false)
        {
            bool bRet = true;

            if (!IsDecimal(str)) { return false; }

            decimal dec = decimal.Parse(str);

            if (isMaxOpenSet == true && isMinOpenSet == false)
            {
                if (dec >= max || dec < min)
                {
                    return false;
                }
            }
            else if (isMaxOpenSet == false && isMinOpenSet == true)
            {
                if (dec > max || dec <= min)
                {
                    return false;
                }
            }
            else if (isMaxOpenSet == true && isMinOpenSet == true)
            {
                if (dec >= max || dec <= min)
                {
                    return false;
                }
            }
            else
            {
                if (dec > max || dec < min)
                {
                    return false;
                }
            }
            

            //判断小数位数
            if (scale > 0)
            {
                if (str.IndexOf(".") >= 0)
                {
                    string digital = str.Substring(str.IndexOf(".") + 1, str.Length - str.IndexOf(".") - 1);
                    if (digital.Length > scale)
                    {
                        return false;
                    }
                }
            }

            return bRet;
        }

        private static MediaPlayer player = new MediaPlayer();
        public static void PlayAnsAlertWave()
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + @"control\alarm80.wav";

            if (File.Exists(path))
            {
                DoPlayAnsAlertWave(path);
            }
            else
            {
                MessageBoxGUI msgBox = new MessageBoxGUI();
                msgBox.setWarnMsg("alarm80.wav文件不存在,无法播放报警！", true);
                msgBox.ShowDialog();
            }
        }

        private static void DoPlayAnsAlertWave(string path)
        {
            ThreadPoolHelper.InvokeThreadPool((o) =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    player.Volume = 1;
                    player.Open(new Uri(path, UriKind.Relative));
                    player.Play();
                });
            });
        }


        public static void SaveOrderControlIsSyncQuoteModel(string securityID,string isChecked)
        {
            try
            {
                if (isChecked.Equals("1"))
                {
                    IniFileHelper.WriteValue($"{BridgeWinName}_{ LocalEnv.UserInfo.UserName}_{securityID.Split('.')[0]}", "IsSyncQuoteModel", isChecked + "");
                }
                else
                {
                    IniFileHelper.WriteValue($"{BridgeWinName}_{ LocalEnv.UserInfo.UserName}_{securityID.Split('.')[0]}", "IsSyncQuoteModel", null);

                    string PositionX = IniFileHelper.ReadValue($"{BridgeWinName}_{ LocalEnv.UserInfo.UserName}_{securityID.Split('.')[0]}", "PositionX");

                    if (string.IsNullOrEmpty(PositionX))
                    {
                        ClearSection(securityID);
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
        }

        public static bool ReadOrderControlIsSyncQuoteModel(string securityID)
        {
            bool ret = false;

            try
            {               
                string isChecked = IniFileHelper.ReadValue($"{BridgeWinName}_{ LocalEnv.UserInfo.UserName}_{securityID.Split('.')[0]}", "IsSyncQuoteModel");

                ret = isChecked == "1" ? true : false;

            }
            catch(Exception ex)
            {
                LogHelper.Error(ex);
            }

            return ret;

        }

        //删除ini文件下段落下的所有键
        public static void ClearSection(string securityID)
        {
            try
            {
                ThreadPoolHelper.InvokeThreadPool((o) =>
                {
                    IniFileHelper.WriteValue($"{BridgeWinName}_{ LocalEnv.UserInfo.UserName}_{securityID.Split('.')[0]}", null, null);
                });
            }
            catch(Exception ex)
            {
                LogHelper.Error(ex);
            }
        }

        public static PResult QueryCirculSizeBySecurity(string securityId,bool IsShowWarn = true)
        {
            PResult result = new PResult();

            MMQuerySecurityCirulSizeReplyPo ret = MMHandler.Instance.mmQueryBondBalance(securityId);

            if (ret != null && !string.IsNullOrEmpty(ret.Code) && ret.IsSuccess())
            {
                result.Code = ret.Code;
                result.Info1 = ret.Data;
                result.Msg = ret.Msg;
            }
            else
            {
                if (IsShowWarn)
                {
                    Prompt2Seconds prompt = new Prompt2Seconds();
                    prompt.PromptTitle = "新增债券";
                    prompt.Owner = Application.Current.MainWindow;
                    if (ret != null && !string.IsNullOrEmpty(ret.Msg))
                    {
                        prompt.PromptContext = $"{ret.Msg}";
                    }
                    else
                    {
                        if (ret == null || string.IsNullOrEmpty(ret.Code))
                        { prompt.PromptContext = $"{ApiConsts.Topic_stc_QueryBondBalance}接口错误"; }
                    }
                    prompt.Show();                    
                }

               if (ret != null && ret.Code != null && ret.Msg != null)
                {
                    result.Code = ret.Code;
                    result.Info1 = ret.Data;
                    result.Msg = ret.Msg;
                }
               else
                {
                    result.Code = "error";
                    result.Msg = $"{ApiConsts.Topic_stc_QueryBondBalance}接口错误";
                }
            }

            return result;
        }
        public static RiskQtyBySecurityReplyPo QueryRiskQtyBySecurity(string securityId, bool IsShowWarn = true)
        {
            RiskQtyBySecurityReplyPo ret = MMHandler.Instance.mmQueryRiskQty(securityId);

            if (ret != null && !string.IsNullOrEmpty(ret.Code) && ret.IsSuccess())
            {
            }
            else
            {
                if (IsShowWarn)
                {
                    Prompt2Seconds prompt = new Prompt2Seconds();
                    prompt.PromptTitle = "新增债券";
                    prompt.Owner = Application.Current.MainWindow;
                    if (ret != null && !string.IsNullOrEmpty(ret.Msg))
                    {
                        prompt.PromptContext = $"{ret.Msg}";
                    }
                    else
                    {
                        if (ret == null || string.IsNullOrEmpty(ret.Code))
                        { prompt.PromptContext = $"{ApiConsts.Topic_stc_QueryRiskQty}接口错误"; }
                    }
                    prompt.Show();
                }

                if (ret != null && ret.Code != null && ret.Msg != null)
                {
                }
                else
                {
                    if (ret == null)
                    {
                        ret = new RiskQtyBySecurityReplyPo();
                    }
                    ret.Code = "error";
                    ret.Msg = $"{ApiConsts.Topic_stc_QueryRiskQty}接口错误";
                }
            }

            return ret;
        }

        public static bool IsZero(string str)
        {
            bool bRet = true;

            decimal d;

            if (decimal.TryParse(str, out d))
            {
                if (d > 0)
                {
                    bRet = false;
                }
            }

            return bRet;
        }

        public static void ShowMessageBox(string msg)
        {
            if (!string.IsNullOrEmpty(msg))
            {
                MessageBoxGUI msgBox = new MessageBoxGUI();
                msgBox.setWarnMsg(msg, true);
                msgBox.ShowDialog();
            }           
        }

        public static bool CheckBaseQtyValid(MMOrderParamReq req)
        {
            bool bRet = true; //默认合法
            int bidQty;
            int ofrQty;
            bool isInt_bid;
            bool isInt_ofr;

            decimal circulSize;
            if (string.IsNullOrEmpty(req.circulSize) || !decimal.TryParse(req.circulSize,out circulSize))
            {
                bRet = false;
                ShowMessageBox("债券余额circulSize非法");

                return false;
            }

            isInt_bid = int.TryParse(req.displayBidQty, out bidQty);

            if (isInt_bid)
            {
                //if (req.displayBidQty.ConvertToDecimal().CompareTo(req.riskMinQtySize.ConvertToDecimal() / 1000) < 0)
                //{
                //    bRet = false;
                //    ShowMessageBox($"bid单笔报量需大于等于{req.riskMinQtySize.ConvertToDecimal() / 1000}");
                    
                //    return false;
                //}

                //if (req.displayBidQty.ConvertToDecimal().CompareTo(req.circulSize.ConvertToDecimal() * req.riskMaxQtyRatio.ConvertToDecimal() / 100 * 10) > 0)
                //{
                //    bRet = false;
                //    ShowMessageBox($"bid单笔报量需大于等于{req.riskMinQtySize.ConvertToDecimal() / 1000}且小于等于{req.circulSize.ConvertToDecimal() * req.riskMaxQtyRatio.ConvertToDecimal() / 100 * 10}");

                //    return false;
                //}
            }
            else
            {
                bRet = false;
                ShowMessageBox($"bid单笔报量数据非法");
               
                return false;
            }

            isInt_ofr = int.TryParse(req.displayOfrQty, out ofrQty);

            if (isInt_ofr)
            {
                //if (req.displayOfrQty.ConvertToDecimal().CompareTo(req.riskMinQtySize.ConvertToDecimal() / 1000) < 0)
                //{
                //    bRet = false;
                //    ShowMessageBox("ofr单笔报量需大于等于"+ req.riskMinQtySize.ConvertToDecimal() / 1000);

                //    return false;
                //}

                //if (req.displayOfrQty.ConvertToDecimal().CompareTo(req.circulSize.ConvertToDecimal() * req.riskMaxQtyRatio.ConvertToDecimal() / 100 * 10) > 0)
                //{
                //    bRet = false;
                //    ShowMessageBox($"ofr单笔报量需大于等于{req.riskMinQtySize}且小于等于{req.circulSize.ConvertToDecimal() * req.riskMaxQtyRatio.ConvertToDecimal() / 100 * 10}");
                    
                //    return false;
                //}                
            }
            else
            {
                bRet = false;
                ShowMessageBox("ofr单笔报量数据非法");
                
                return false;
            }

            return bRet;
        }
        /// <summary>
        /// 计算收益率或者净价
        /// </summary>
        /// <param name="calType"></param>
        /// <param name="settleType"></param>
        /// <param name="val"></param>
        public static string CalBond(string securityId, string calType, string settleType, string val, string side)
        {
            string resultYield = ""; 
            decimal d = 0;
            if (string.IsNullOrEmpty(val) || string.IsNullOrEmpty(settleType) || !decimal.TryParse(val, out d))
                return resultYield;
            try
            {
                CalReq requst = new CalReq();
                requst.SecurityID = securityId;
                requst.CalType = calType;//"4";
                requst.SettlType = settleType;// "1";
                requst.Scale = 6;
                requst.Value = decimal.Divide(CommonUtils.ConvertToDecimal(val), 100).ToString();// customPrice;
                if (!string.IsNullOrEmpty(LocalEnv.ConfInfo.CalType))
                    requst.CalPath = LocalEnv.ConfInfo.CalType;

                CalResult result = CalHandler.Instance.CalBond(requst);
                if (result != null && result.Data != null && result.IsSuccess())
                {
                    if (calType.Equals("4"))
                    {
                        if (side.Equals("1"))
                        {
                            resultYield = decimal.Multiply(result.Data.YieldToExecution, 100).ToString("F4");
                        }
                        else
                        {
                            resultYield = decimal.Multiply(result.Data.YieldToExecution, 100).ToString("F4");
                        }
                    }
                    else if (calType.Equals("7"))//d
                    {
                        if (side.Equals("1"))
                        {
                            resultYield = decimal.Multiply(result.Data.Yield, 100).ToString("F4");
                        }
                        else
                        {
                            resultYield = decimal.Multiply(result.Data.Yield, 100).ToString("F4");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                LogHelper.Error(e);
            }
            finally
            {
                    
            }
            return resultYield;
        }
        private static Dictionary<string, MMBondParamsSetting> bondParamsSettingDic = new Dictionary<string, MMBondParamsSetting>();

        public static void updateBondParamsSetting(List<MMBondParamsSetting> list) 
        {
            bondParamsSettingDic.Clear();
            if (list != null) 
            {
                foreach (MMBondParamsSetting po in list) 
                {
                    bondParamsSettingDic.Add(po.paramKey,po);
                }
            }
        }
        public static MMBondParamsSetting getBondParamsSetting(string key) 
        {
            MMBondParamsSetting po = null;
            if (bondParamsSettingDic.TryGetValue(key, out po)) 
            {
                return po;
            }
            return null;
        }

        public static void copySettingToData(OrderParamProp Data,MMBondParamsSetting paramsSetting,bool isDefault)
        {
            if (!string.IsNullOrEmpty(paramsSetting.algoMidType))
            {
                Data.algoMidType = paramsSetting.algoMidType;
            }
            if (!string.IsNullOrEmpty(paramsSetting.algoValue))
            {
                Data.algoValue = paramsSetting.algoValue;
            }
            if (!string.IsNullOrEmpty(paramsSetting.baseSettleType))
            {
                Data.baseSettleType = paramsSetting.baseSettleType;
            }
            if (!string.IsNullOrEmpty(paramsSetting.bidAlgoDefenValue))
            {
                Data.bidAlgoDefenValue = paramsSetting.bidAlgoDefenValue;
            }
            if (!string.IsNullOrEmpty(paramsSetting.bidAlgoSafeValue))
            {
                Data.bidAlgoSafeValue = paramsSetting.bidAlgoSafeValue;
            }
            if (!string.IsNullOrEmpty(paramsSetting.bidAlgoTrigValue))
            {
                Data.bidAlgoTrigValue = paramsSetting.bidAlgoTrigValue;
            }
            if (!string.IsNullOrEmpty(paramsSetting.bidBaseQty))
            {
                Data.bidBaseQty = paramsSetting.bidBaseQty;
            }
            if (!string.IsNullOrEmpty(paramsSetting.bidBaseReSendValue))
            {
                Data.bidBaseReSendValue = paramsSetting.bidBaseReSendValue;
            }
            if (!string.IsNullOrEmpty(paramsSetting.bidBaseWidenValue))
            {
                Data.bidBaseWidenValue = paramsSetting.bidBaseWidenValue;
            }
            if (!string.IsNullOrEmpty(paramsSetting.bidDefenCDCValue))
            {
                Data.bidDefenCDCValue = paramsSetting.bidDefenCDCValue;
            }
            if (!string.IsNullOrEmpty(paramsSetting.bidDefenReferLevel))
            {
                Data.bidDefenReferLevel = paramsSetting.bidDefenReferLevel;
            }
            if (!string.IsNullOrEmpty(paramsSetting.bidDefenReferMarketType))
            {
                Data.bidDefenReferMarketType = paramsSetting.bidDefenReferMarketType;
            }
            if (!string.IsNullOrEmpty(paramsSetting.bidQuoteModel))
            {
                Data.bidQuoteModel = paramsSetting.bidQuoteModel;
            }
            if (!string.IsNullOrEmpty(paramsSetting.dealWidenLowerTime))
            {
                Data.dealWidenLowerTime = paramsSetting.dealWidenLowerTime;
            }
            if (!string.IsNullOrEmpty(paramsSetting.dealWidenUpperTime))
            {
                Data.dealWidenUpperTime = paramsSetting.dealWidenUpperTime;
            }
            if (!string.IsNullOrEmpty(paramsSetting.ofrAlgoDefenValue))
            {
                Data.ofrAlgoDefenValue = paramsSetting.ofrAlgoDefenValue;
            }
            if (!string.IsNullOrEmpty(paramsSetting.ofrAlgoSafeValue))
            {
                Data.ofrAlgoSafeValue = paramsSetting.ofrAlgoSafeValue;
            }
            if (!string.IsNullOrEmpty(paramsSetting.ofrAlgoTrigValue))
            {
                Data.ofrAlgoTrigValue = paramsSetting.ofrAlgoTrigValue;
            }
            if (!string.IsNullOrEmpty(paramsSetting.ofrBaseQty))
            {
                Data.ofrBaseQty = paramsSetting.ofrBaseQty;
            }
            if (!string.IsNullOrEmpty(paramsSetting.ofrBaseReSendValue))
            {
                Data.ofrBaseReSendValue = paramsSetting.ofrBaseReSendValue;
            }
            if (!string.IsNullOrEmpty(paramsSetting.ofrBaseWidenValue))
            {
                Data.ofrBaseWidenValue = paramsSetting.ofrBaseWidenValue;
            }
            if (!string.IsNullOrEmpty(paramsSetting.ofrDefenCDCValue))
            {
                Data.ofrDefenCDCValue = paramsSetting.ofrDefenCDCValue;
            }
            if (!string.IsNullOrEmpty(paramsSetting.ofrDefenReferLevel))
            {
                Data.ofrDefenReferLevel = paramsSetting.ofrDefenReferLevel;
            }
            if (!string.IsNullOrEmpty(paramsSetting.ofrDefenReferMarketType))
            {
                Data.ofrDefenReferMarketType = paramsSetting.ofrDefenReferMarketType;
            }
            if (!string.IsNullOrEmpty(paramsSetting.ofrQuoteModel))
            {
                Data.ofrQuoteModel = paramsSetting.ofrQuoteModel;
            }
            if (!string.IsNullOrEmpty(paramsSetting.defenCDCEnable))
            {
                Data.defenCDCEnable = paramsSetting.defenCDCEnable;
            }
            if (!string.IsNullOrEmpty(paramsSetting.algoSafeEnable))
            {
                Data.algoSafeEnable = paramsSetting.algoSafeEnable;
            }
            if (!string.IsNullOrEmpty(paramsSetting.secondSafeEnable))
            {
                Data.secondSafeEnable = paramsSetting.secondSafeEnable;
            }
            if (!string.IsNullOrEmpty(paramsSetting.qdmespSafeEnable))
            {
                Data.qdmespSafeEnable = paramsSetting.qdmespSafeEnable;
            }
            if (!string.IsNullOrEmpty(paramsSetting.qdmespSafeLevel))
            {
                Data.qdmespSafeLevel = paramsSetting.qdmespSafeLevel;
            }
            if (!string.IsNullOrEmpty(paramsSetting.bidQdmespSafeValue))
            {
                Data.bidQdmespSafeValue = paramsSetting.bidQdmespSafeValue;
            }
            if (!string.IsNullOrEmpty(paramsSetting.ofrQdmespSafeValue))
            {
                Data.ofrQdmespSafeValue = paramsSetting.ofrQdmespSafeValue;
            }
            if (!string.IsNullOrEmpty(paramsSetting.wallEnable))
            {
                Data.wallEnable = paramsSetting.wallEnable;
            }
            if (isDefault)
            {
                if (!string.IsNullOrEmpty(paramsSetting.algoBaseEnable))
                {
                    Data.algoBaseEnable = paramsSetting.algoBaseEnable;
                }
                if (!string.IsNullOrEmpty(paramsSetting.algoMidOffsetValue))
                {
                    Data.algoMidOffsetValue = paramsSetting.algoMidOffsetValue;
                }
                if (!string.IsNullOrEmpty(paramsSetting.bidPosValue))
                {
                    Data.bidPosValue = paramsSetting.bidPosValue;
                }
                if (!string.IsNullOrEmpty(paramsSetting.defaultManualSpread))
                {
                    Data.defaultManualSpread = paramsSetting.defaultManualSpread;
                }
                if (!string.IsNullOrEmpty(paramsSetting.ofrPosValue))
                {
                    Data.ofrPosValue = paramsSetting.ofrPosValue;
                }
                if (!string.IsNullOrEmpty(paramsSetting.posFakEnable))
                {
                    Data.posFakEnable = paramsSetting.posFakEnable;
                }
                if (!string.IsNullOrEmpty(paramsSetting.posFakSpread))
                {
                    Data.posFakSpread = paramsSetting.posFakSpread;
                }
                if (!string.IsNullOrEmpty(paramsSetting.posLmtEnable))
                {
                    Data.posLmtEnable = paramsSetting.posLmtEnable;
                }
                if (!string.IsNullOrEmpty(paramsSetting.stepLength))
                {
                    Data.stepLength = paramsSetting.stepLength;
                }
                if (!string.IsNullOrEmpty(paramsSetting.maxFloorEnable))
                {
                    Data.maxFloorEnable = paramsSetting.maxFloorEnable;
                }
                if (!string.IsNullOrEmpty(paramsSetting.manualBid))
                {
                    Data.manualBid = paramsSetting.manualBid;
                }
                if (!string.IsNullOrEmpty(paramsSetting.manualOfr))
                {
                    Data.manualOfr = paramsSetting.manualOfr;
                }
                if (!string.IsNullOrEmpty(paramsSetting.BaseERReSendType))
                {
                    Data.BaseERReSendType = paramsSetting.BaseERReSendType;
                }
                if (!string.IsNullOrEmpty(paramsSetting.exeManualBid))
                {
                    Data.exeManualBid = paramsSetting.exeManualBid;
                }
                if (!string.IsNullOrEmpty(paramsSetting.exeManualOfr))
                {
                    Data.exeManualOfr = paramsSetting.exeManualOfr;
                }
                if (!string.IsNullOrEmpty(paramsSetting.algoMidDetailDefined))
                {
                    Data.algoMidDetailDefined = paramsSetting.algoMidDetailDefined;
                }
                if (!string.IsNullOrEmpty(paramsSetting.extAlgoMidDetailDefined))
                {
                    Data.extAlgoMidDetailDefined = paramsSetting.extAlgoMidDetailDefined;
                }
            }
        }
        public static void MMOrderParamReqToDefault(MMOrderParamReq Data)
        {
            MMBondParamsSetting paramsSetting = getBondParamsSetting("Default");
            if (paramsSetting != null)
            {
                if (!string.IsNullOrEmpty(paramsSetting.algoMidType))
                {
                    Data.algoMidType = paramsSetting.algoMidType;
                }
                if (!string.IsNullOrEmpty(paramsSetting.algoValue))
                {
                    Data.algoValue = paramsSetting.algoValue;
                }
                if (!string.IsNullOrEmpty(paramsSetting.baseSettleType))
                {
                    Data.baseSettleType = paramsSetting.baseSettleType;
                }
                if (!string.IsNullOrEmpty(paramsSetting.bidAlgoDefenValue))
                {
                    Data.bidAlgoDefenValue = paramsSetting.bidAlgoDefenValue;
                }
                if (!string.IsNullOrEmpty(paramsSetting.bidAlgoSafeValue))
                {
                    Data.bidAlgoSafeValue = paramsSetting.bidAlgoSafeValue;
                }
                if (!string.IsNullOrEmpty(paramsSetting.bidAlgoTrigValue))
                {
                    Data.bidAlgoTrigValue = paramsSetting.bidAlgoTrigValue;
                }
                if (!string.IsNullOrEmpty(paramsSetting.bidBaseQty))
                {
                    Data.bidBaseQty = paramsSetting.bidBaseQty;
                }
                if (!string.IsNullOrEmpty(paramsSetting.bidBaseReSendValue))
                {
                    Data.bidBaseReSendValue = paramsSetting.bidBaseReSendValue;
                }
                if (!string.IsNullOrEmpty(paramsSetting.bidBaseWidenValue))
                {
                    Data.bidBaseWidenValue = paramsSetting.bidBaseWidenValue;
                }
                if (!string.IsNullOrEmpty(paramsSetting.bidDefenCDCValue))
                {
                    Data.bidDefenCDCValue = paramsSetting.bidDefenCDCValue;
                }
                if (!string.IsNullOrEmpty(paramsSetting.bidDefenReferLevel))
                {
                    Data.bidDefenReferLevel = paramsSetting.bidDefenReferLevel;
                }
                if (!string.IsNullOrEmpty(paramsSetting.bidDefenReferMarketType))
                {
                    Data.bidDefenReferMarketType = paramsSetting.bidDefenReferMarketType;
                }
                if (!string.IsNullOrEmpty(paramsSetting.bidQuoteModel))
                {
                    Data.bidQuoteModel = paramsSetting.bidQuoteModel;
                }
                if (!string.IsNullOrEmpty(paramsSetting.dealWidenLowerTime))
                {
                    Data.dealWidenLowerTime = paramsSetting.dealWidenLowerTime;
                }
                if (!string.IsNullOrEmpty(paramsSetting.dealWidenUpperTime))
                {
                    Data.dealWidenUpperTime = paramsSetting.dealWidenUpperTime;
                }
                if (!string.IsNullOrEmpty(paramsSetting.ofrAlgoDefenValue))
                {
                    Data.ofrAlgoDefenValue = paramsSetting.ofrAlgoDefenValue;
                }
                if (!string.IsNullOrEmpty(paramsSetting.ofrAlgoSafeValue))
                {
                    Data.ofrAlgoSafeValue = paramsSetting.ofrAlgoSafeValue;
                }
                if (!string.IsNullOrEmpty(paramsSetting.ofrAlgoTrigValue))
                {
                    Data.ofrAlgoTrigValue = paramsSetting.ofrAlgoTrigValue;
                }
                if (!string.IsNullOrEmpty(paramsSetting.ofrBaseQty))
                {
                    Data.ofrBaseQty = paramsSetting.ofrBaseQty;
                }
                if (!string.IsNullOrEmpty(paramsSetting.ofrBaseReSendValue))
                {
                    Data.ofrBaseReSendValue = paramsSetting.ofrBaseReSendValue;
                }
                if (!string.IsNullOrEmpty(paramsSetting.ofrBaseWidenValue))
                {
                    Data.ofrBaseWidenValue = paramsSetting.ofrBaseWidenValue;
                }
                if (!string.IsNullOrEmpty(paramsSetting.ofrDefenCDCValue))
                {
                    Data.ofrDefenCDCValue = paramsSetting.ofrDefenCDCValue;
                }
                if (!string.IsNullOrEmpty(paramsSetting.ofrDefenReferLevel))
                {
                    Data.ofrDefenReferLevel = paramsSetting.ofrDefenReferLevel;
                }
                if (!string.IsNullOrEmpty(paramsSetting.ofrDefenReferMarketType))
                {
                    Data.ofrDefenReferMarketType = paramsSetting.ofrDefenReferMarketType;
                }
                if (!string.IsNullOrEmpty(paramsSetting.ofrQuoteModel))
                {
                    Data.ofrQuoteModel = paramsSetting.ofrQuoteModel;
                }
                if (!string.IsNullOrEmpty(paramsSetting.algoBaseEnable))
                {
                    Data.algoBaseEnable = paramsSetting.algoBaseEnable;
                }
                if (!string.IsNullOrEmpty(paramsSetting.algoMidOffsetValue))
                {
                    Data.algoMidOffsetValue = paramsSetting.algoMidOffsetValue;
                }
                if (!string.IsNullOrEmpty(paramsSetting.bidPosValue))
                {
                    Data.bidPosValue = paramsSetting.bidPosValue;
                }
                if (!string.IsNullOrEmpty(paramsSetting.defaultManualSpread))
                {
                    Data.defaultManualSpread = paramsSetting.defaultManualSpread;
                }
                if (!string.IsNullOrEmpty(paramsSetting.ofrPosValue))
                {
                    Data.ofrPosValue = paramsSetting.ofrPosValue;
                }
                if (!string.IsNullOrEmpty(paramsSetting.posFakEnable))
                {
                    Data.posFakEnable = paramsSetting.posFakEnable;
                }
                if (!string.IsNullOrEmpty(paramsSetting.posLmtEnable))
                {
                    Data.posLmtEnable = paramsSetting.posLmtEnable;
                }
                if (!string.IsNullOrEmpty(paramsSetting.stepLength))
                {
                    Data.stepLength = paramsSetting.stepLength;
                }
                if (!string.IsNullOrEmpty(paramsSetting.wallEnable))
                {
                    Data.wallEnable = paramsSetting.wallEnable;
                }
                if (!string.IsNullOrEmpty(paramsSetting.maxFloorEnable))
                {
                    Data.maxFloorEnable = paramsSetting.maxFloorEnable;
                }
                if (!string.IsNullOrEmpty(paramsSetting.defenCDCEnable))
                {
                    Data.defenCDCEnable = paramsSetting.defenCDCEnable;
                }
                if (!string.IsNullOrEmpty(paramsSetting.algoSafeEnable))
                {
                    Data.algoSafeEnable = paramsSetting.algoSafeEnable;
                }
                if (!string.IsNullOrEmpty(paramsSetting.secondSafeEnable))
                {
                    Data.secondSafeEnable = paramsSetting.secondSafeEnable;
                }
                if (!string.IsNullOrEmpty(paramsSetting.qdmespSafeEnable))
                {
                    Data.qdmespSafeEnable = paramsSetting.qdmespSafeEnable;
                }
                if (!string.IsNullOrEmpty(paramsSetting.qdmespSafeLevel))
                {
                    Data.qdmespSafeLevel = paramsSetting.qdmespSafeLevel;
                }
                if (!string.IsNullOrEmpty(paramsSetting.bidQdmespSafeValue))
                {
                    Data.bidQdmespSafeValue = paramsSetting.bidQdmespSafeValue;
                }
                if (!string.IsNullOrEmpty(paramsSetting.ofrQdmespSafeValue))
                {
                    Data.ofrQdmespSafeValue = paramsSetting.ofrQdmespSafeValue;
                }
                if (!string.IsNullOrEmpty(paramsSetting.manualBid))
                {
                    Data.manualBid = paramsSetting.manualBid;
                }
                if (!string.IsNullOrEmpty(paramsSetting.manualOfr))
                {
                    Data.manualOfr = paramsSetting.manualOfr;
                }
                if (!string.IsNullOrEmpty(paramsSetting.exeManualBid))
                {
                    Data.exeManualBid = paramsSetting.exeManualBid;
                }
                if (!string.IsNullOrEmpty(paramsSetting.exeManualOfr))
                {
                    Data.exeManualOfr = paramsSetting.exeManualOfr;
                }
                if (!string.IsNullOrEmpty(paramsSetting.algoMidDetailDefined))
                {
                    Data.algoMidDetailDefined = paramsSetting.algoMidDetailDefined;
                }
                if (!string.IsNullOrEmpty(paramsSetting.extAlgoMidDetailDefined))
                {
                    Data.extAlgoMidDetailDefined = paramsSetting.extAlgoMidDetailDefined;
                }
            }
            else
            {
                Data.bidQuoteModel = "Defend";
                Data.bidBaseQty = "1";
                Data.bidBaseReSendValue = "0.3";
                Data.bidBaseWidenValue = "2.5";
                Data.baseSettleType = "2";
                Data.ofrBasePriority = "1,2";
                Data.ofrQuoteModel = "Defend";
                Data.ofrBaseQty = "1";
                Data.ofrBaseReSendValue = "0";
                Data.ofrBaseWidenValue = "2.5";
                Data.bidAlgoDefenValue = "1";
                Data.bidAlgoSafeValue = "0";
                Data.bidAlgoTrigValue = "0";
                Data.ofrAlgoDefenValue = "1";
                Data.ofrAlgoSafeValue = "0";
                Data.ofrAlgoTrigValue = "0";
                Data.algoEnable = "0";
                Data.algoBaseEnable = "0";
                Data.algoMidType = "2";
                Data.algoMidOffsetValue = "0";
                Data.algoValue = "10";
                Data.algoCDCEnable = "0";
                Data.posLmtEnable = "0";
                Data.bidPosValue = "0";
                Data.ofrPosValue = "0";
                Data.posFakEnable = "1";
                Data.posFakSpread = "0";
                Data.dealWidenLowerTime = "30";
                Data.dealWidenUpperTime = "30";
                Data.bidStrategyType = "Defend";

                Data.ofrStrategyType = "Defend";
                //Data.apiLastTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                Data.manualBid = "0";
                Data.manualOfr = "0";
                //Data.lastOptTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                Data.stepLength = "0.25";
                Data.maxFloorEnable = "0";
                //Data.maxFloor = string.Empty;
                Data.openQDM2Enable = "0";
                Data.defenCDCEnable = "1";
                Data.bidDefenCDCValue = "4.5";
                Data.ofrDefenCDCValue = "4.5";
                //Data.defenCDCDetail = "0";
                Data.bidDefenReferMarketType = "1";
                Data.bidDefenReferLevel = "1";
                Data.ofrDefenReferMarketType = "1";
                Data.ofrDefenReferLevel = "1";
                Data.wallEnable = "1";
                //Data.cDCValueType = string.Empty;
                Data.exeManualBid = "0";
                Data.exeManualOfr = "0";
                Data.exeBondPre = "0";
                Data.exeBondFollow = "0";
                Data.algoSafeEnable = "1";
                Data.secondSafeEnable = "1";
                Data.qdmespSafeEnable = "1";
                Data.qdmespSafeLevel = "1";
                Data.bidQdmespSafeValue = "0";
                Data.ofrQdmespSafeValue = "0";
            }
            Data.BaseERValue = "0";
        }
    }
}
