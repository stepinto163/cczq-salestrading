using com.gateway.log;
using STC.Common.Api;
using STC.Common.Handler.Admin;
using STC.Common.Handler.Cal;
using STC.Common.Handler.RFQ;
using STC.Common.Model.Base;
using STC.Common.Model.Cal;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.RFQ;
using STC.Common.Utils;
using STC.Common.View.Component;
using STC.Common.View.UI;
using STC.Frame.CustomUI.Component;
using STC.Rfq.Plugin.Command;
using STC.Rfq.Plugin.ViewModel;
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

namespace STC.Rfq.Plugin.Utils
{
    /// <summary>
    /// 工具类
    /// </summary>
    public class RfqCommonUtils
    {
        private static string BridgeWinName = "OrderControl";

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
            retReq.securityid = string.Empty;
            retReq.symbol = string.Empty;

            //策略类型 默认为【手动】
            retReq.strategyTypeBid = "Manual";
            retReq.strategyTypeOfr = "Manual";
            // 清算速度 默认为【T + 1】
            retReq.settltype = "2";
            // 有效方向  默认为【不限】，支持下拉选择不限，Bid，Ofr
            retReq.validSide = "All";
            //回复有效时长 默认为【5分钟】
            retReq.quoteValidUntilTimeBid = "5";
            retReq.quoteValidUntilTimeOfr = "5";
            //重发阈值 [0.01, 99.99] 单位是bp。默认为空。分Bid, Ofr方向设置。不对手动、接口策略生效。
            retReq.resendThresholdBid = "";
            retReq.resendThresholdOfr = "";
            //预设回量上限 [0.01, 9999.90] ，单位是KW，默认为空，分Bid, Ofr方向设置。
            retReq.maxQtyBid = "";
            retReq.maxQtyOfr = "";
            //防守点报价 默认为0 支持对Bid, Ofr 分别设置。单位bp，输入范围[-1000.00, 1000.00] 小数2位，默认值为0。
            retReq.defBid = "0";
            retReq.defOfr = "0";
            // 手动报价 默认无值，如果策略类型为【手动】则该字段用户必填,允许分别对Bid, Ofr方向手动报价直接输入数值，范围是(-100, 9999.0000]，允许小数后4位
            retReq.manualBid = "";
            retReq.manualOfr = "";
            //墙设置 默认为0  支持对Bid, Ofr 分别设置。单位bp，输入范围[0.00, 1000.00] 小数2位。
            retReq.defMaxBid = "0";
            retReq.defMaxOfr = "0";
            //算法偏离 默认值为0 单位bp，允许输入范围[-1000.00，1000.00]
            retReq.algoDeviation = "0";
            //算法点差 algoSpread 无默认值，必须填写。单位bp，允许输入范围(0，1000.00]。注意，这里点差必须大于0

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

        public static bool CheckBaseQtyValid(RfqstgViewModel req)
        {
            bool bRet = true; //默认合法
            int bidQty;
            int ofrQty;
            bool isInt_bid;
            bool isInt_ofr;

            //decimal circulSize;
            //if (string.IsNullOrEmpty(req.circulSize) || !decimal.TryParse(req.circulSize,out circulSize))
            //{
            //    bRet = false;
            //    ShowMessageBox("债券余额circulSize非法");

            //    return false;
            //}

            //isInt_bid = int.TryParse(req.displayBidQty, out bidQty);

            //if (isInt_bid)
            //{
            //    //if (req.displayBidQty.ConvertToDecimal().CompareTo(req.riskMinQtySize.ConvertToDecimal() / 1000) < 0)
            //    //{
            //    //    bRet = false;
            //    //    ShowMessageBox($"bid单笔报量需大于等于{req.riskMinQtySize.ConvertToDecimal() / 1000}");
                    
            //    //    return false;
            //    //}

            //    //if (req.displayBidQty.ConvertToDecimal().CompareTo(req.circulSize.ConvertToDecimal() * req.riskMaxQtyRatio.ConvertToDecimal() / 100 * 10) > 0)
            //    //{
            //    //    bRet = false;
            //    //    ShowMessageBox($"bid单笔报量需大于等于{req.riskMinQtySize.ConvertToDecimal() / 1000}且小于等于{req.circulSize.ConvertToDecimal() * req.riskMaxQtyRatio.ConvertToDecimal() / 100 * 10}");

            //    //    return false;
            //    //}
            //}
            //else
            //{
            //    bRet = false;
            //    ShowMessageBox($"bid单笔报量数据非法");
               
            //    return false;
            //}

            //isInt_ofr = int.TryParse(req.displayOfrQty, out ofrQty);

            //if (isInt_ofr)
            //{
            //    //if (req.displayOfrQty.ConvertToDecimal().CompareTo(req.riskMinQtySize.ConvertToDecimal() / 1000) < 0)
            //    //{
            //    //    bRet = false;
            //    //    ShowMessageBox("ofr单笔报量需大于等于"+ req.riskMinQtySize.ConvertToDecimal() / 1000);

            //    //    return false;
            //    //}

            //    //if (req.displayOfrQty.ConvertToDecimal().CompareTo(req.circulSize.ConvertToDecimal() * req.riskMaxQtyRatio.ConvertToDecimal() / 100 * 10) > 0)
            //    //{
            //    //    bRet = false;
            //    //    ShowMessageBox($"ofr单笔报量需大于等于{req.riskMinQtySize}且小于等于{req.circulSize.ConvertToDecimal() * req.riskMaxQtyRatio.ConvertToDecimal() / 100 * 10}");
                    
            //    //    return false;
            //    //}                
            //}
            //else
            //{
            //    bRet = false;
            //    ShowMessageBox("ofr单笔报量数据非法");
                
            //    return false;
            //}

            return bRet;
        }
    }
}
