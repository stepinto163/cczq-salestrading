using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using com.gateway.log;
using STC.Common.Convertor;
using STC.Common.Data;
using STC.Common.DataProcesser;
using STC.Common.Handler.Base;
using STC.Common.Model.Base;
using STC.Common.Model.Fix.Message;
using STC.Common.Utils;
using STC.Common.ViewModel;
using STC.SalesTrading.Plugin.Abstractions;
using STC.SalesTrading.Plugin.Abstractions.Models;

namespace STC.SalesTrading.Plugin.Shared.Services
{
    /// <summary>
    /// 宿主行情订阅适配层：包 PriceHandler（CT 多档快照）/ LastExecutionProcesser（最新成交）/
    /// CommonUtils 债券主数据字典。对应 PriceAndOrderViewModel 的订阅四步
    /// （RegisterCTPrice 缓存回放+实时推送 / SubscribeLastExecution / 主数据查询 / 就绪事件重查），
    /// 但只取插件详情面板需要的子集；回调统一归集到 UI 线程。
    /// 注意：LastExecutionProcesser 的回调去重键是「声明类型.方法名」，故委托必须为命名方法。
    /// </summary>
    internal sealed class HostQuoteFeed : IHostQuoteFeed
    {
        /// <summary>每侧保留的最大档位数（详情面板为两向阶梯表，超出的低档无展示意义）。</summary>
        private const int MaxLevelsPerSide = 12;

        private string _current;
        private bool _dataStatusHooked;
        private string _sampleLoggedFor;
        private string _lastExecSampleLoggedFor;
        private QuoteFeedLevel[] _lastLevels;
        private QuoteFeedLastExec _lastExec;
        private long _selectionVersion;

        // 命名方法委托（LastExecutionProcesser 按 声明类型.方法名 去重/退订，lambda 不可控）
        private void OnCtPrice(MarketDataSnapshotFullRefresh snapshot)
        {
            long selectionVersion = Interlocked.Read(ref _selectionVersion);
            if (snapshot == null) return;
            // 过滤过期标的（切券后仍在队列里的旧快照不灌新面板）
            if (string.IsNullOrEmpty(_current) || snapshot.SecurityID != _current)
            {
                LogHelper.Info("[HostQuoteFeed] drop stale CT snapshot: got=" + snapshot.SecurityID + " want=" + _current);
                return;
            }
            var levels = ParseLevels(snapshot);
            if (_sampleLoggedFor != _current && snapshot.NoMDEntries != null && snapshot.NoMDEntries.Count > 0)
            {
                var e0 = snapshot.NoMDEntries[0];
                LogHelper.Info("[HostQuoteFeed] first snapshot sample for " + _current
                    + " entries=" + snapshot.NoMDEntries.Count
                    + " Venues='" + e0.Venues + "'"
                    + " VenueTypeGW='" + e0.VenueTypeGW + "'"
                    + " InstitutionName='" + e0.InstitutionName + "'"
                    + " PartyName='" + e0.PartyName + "'"
                    + " PartySubName='" + e0.PartySubName + "'"
                    + " RoleName='" + e0.RoleName + "'"
                    + " levels=" + levels.Length
                    + " bids=" + levels.Count(l => l.Side == QuoteFeedSide.Bid)
                    + " ofrs=" + levels.Count(l => l.Side == QuoteFeedSide.Ofr));
                _sampleLoggedFor = _current;
            }
            if (levels.Length == 0)
            {
                // 线上诊断：有档位但全被过滤（价格字段为空）时留下证据
                LogHelper.Info("[HostQuoteFeed] CT snapshot has " + (snapshot.NoMDEntries?.Count ?? 0)
                    + " entries but 0 usable levels for " + _current
                    + "; firstPx=" + (snapshot.NoMDEntries != null && snapshot.NoMDEntries.Count > 0 ? snapshot.NoMDEntries[0].MDEntryPx : "<none>"));
            }
            RaiseOnUi(snapshot.SecurityID, selectionVersion, levels, currentLevels =>
            {
                _lastLevels = currentLevels;
                _snapshot?.Invoke(currentLevels);
            });
        }

        private void OnLastExecution(ExecutionReport report)
        {
            long selectionVersion = Interlocked.Read(ref _selectionVersion);
            if (report == null || _current == null || report.SecurityID != _current) return;
            if (_lastExecSampleLoggedFor != _current)
            {
                LogHelper.Info("[HostQuoteFeed] last exec sample for " + _current
                    + " LastPx='" + report.LastPx + "'"
                    + " LastQty='" + report.LastQty + "'"
                    + " Side='" + report.Side + "'"
                    + " ExecType='" + report.ExecType + "'"
                    + " Symbol='" + report.Symbol + "'"
                    + " TransactTime='" + report.TransactTime + "'");
                _lastExecSampleLoggedFor = _current;
            }
            var exec = new QuoteFeedLastExec
            {
                Price = ParseDecimal(report.LastPx),
                Volume = ParseDecimal(report.LastQty),
                Side = report.Side,
                Yield = ParseDecimal(report.Yield),
                Time = ParseDateTime(report.TransactTime),
            };
            RaiseOnUi(report.SecurityID, selectionVersion, exec, currentExecution =>
            {
                _lastExec = currentExecution;
                _lastExecution?.Invoke(currentExecution);
            });
        }

        private void OnLoadDataStatus(bool ready)
        {
            // 债券主数据后台加载完成：重查一次当前标的（首次查询时字典可能尚未就绪）
            if (!ready || string.IsNullOrEmpty(_current)) return;
            RaiseBondInfo(_current);
        }

        public bool IsLive
        {
            get
            {
                try
                {
                    return LocalEnv.UserInfo != null;
                }
                catch (Exception)
                {
                    // 宿主未初始化（设计器/单测/极端时序）：通道视为不可用
                    return false;
                }
            }
        }

        public void Switch(string securityCode)
        {
            if (!IsLive) return;
            var code = securityCode ?? string.Empty;

            // 同券重入（切换同券的不同报价行）：订阅不动，把缓存的快照/成交/主数据向新面板回放——
            // 详情面板每次选中都会新建实例，不回放的话要等下一次实时推送才有数据
            if (code == _current)
            {
                if (string.IsNullOrEmpty(code)) return;
                ReplayCurrent();
                RaiseBondInfo(code);
                return;
            }

            // 切券时先使已排队的旧回调失效，包括 A→B→A 的情况。
            Interlocked.Increment(ref _selectionVersion);
            // 先退旧订（PriceHandler 最后一个回调移除时自动退订 MD topic，不泄漏）
            if (!string.IsNullOrEmpty(_current))
            {
                PriceHandler.Instance.UnregisterCTPrice(_current, OnCtPrice);
                LastExecutionProcesser.Processer.UnSubscribeLastExecution(_current, OnLastExecution);
                LogHelper.Info("[HostQuoteFeed] unsubscribed: " + _current);
            }

            _lastLevels = null;
            _lastExec = null;
            _sampleLoggedFor = null;
            _lastExecSampleLoggedFor = null;
            _current = string.IsNullOrEmpty(code) ? null : code;
            if (_current == null) return;

            PriceHandler.Instance.RegisterCTPrice(_current, OnCtPrice);   // 有缓存立即回放首帧
            LastExecutionProcesser.Processer.SubscribeLastExecution(_current, OnLastExecution);
            LogHelper.Info("[HostQuoteFeed] subscribed: " + _current);

            if (!_dataStatusHooked)
            {
                BaseHandler.Instance.SubscribeLoadDataStatus(OnLoadDataStatus);
                _dataStatusHooked = true;
            }
            RaiseBondInfo(_current);
        }

        public void Dispose()
        {
            Interlocked.Increment(ref _selectionVersion);
            string previousCode = _current;
            _current = null;
            _lastLevels = null;
            _lastExec = null;
            try
            {
                if (!string.IsNullOrEmpty(previousCode))
                {
                    PriceHandler.Instance.UnregisterCTPrice(previousCode, OnCtPrice);
                    LastExecutionProcesser.Processer.UnSubscribeLastExecution(previousCode, OnLastExecution);
                    LogHelper.Info("[HostQuoteFeed] unsubscribed: " + previousCode);
                }
                if (_dataStatusHooked)
                {
                    BaseHandler.Instance.UnSubscribeLoadDataStatus(OnLoadDataStatus);
                    _dataStatusHooked = false;
                }
            }
            catch (Exception error)
            {
                LogHelper.Warn(error);
            }
        }

        private Action<QuoteFeedLevel[]> _snapshot;
        private Action<QuoteFeedLastExec> _lastExecution;
        private Action<QuoteFeedBondInfo> _bondInfo;

        public event Action<QuoteFeedLevel[]> Snapshot
        {
            add { _snapshot += value; }
            remove { _snapshot -= value; }
        }

        public event Action<QuoteFeedLastExec> LastExecution
        {
            add { _lastExecution += value; }
            remove { _lastExecution -= value; }
        }

        public event Action<QuoteFeedBondInfo> BondInfo
        {
            add { _bondInfo += value; }
            remove { _bondInfo -= value; }
        }

        private void RaiseBondInfo(string code)
        {
            long selectionVersion = Interlocked.Read(ref _selectionVersion);
            var po = CommonUtils.GetBondsInfoPoBySecurityID(code);
            var info = new QuoteFeedBondInfo
            {
                Code = code,
                ShortName = po?.Short_Name,
                RemainingTime = po?.RemaingingTime,
                CouponRate = po?.Coupon_Rate_Current,
                ValuationShow = ExtractValuationYtm(po),
                MarketIndicator = po?.MarketIndicator,
            };
            RaiseOnUi(code, selectionVersion, info, currentInfo => _bondInfo?.Invoke(currentInfo));
        }

        /// <summary>
        /// 中债估值收益率（纯数值）：取原始字段 CCDCValuation（行权债为 "行权ytm|到期ytm"，按宿主
        /// CCDCValuationShow 同口径取 "|" 前段）。不用 CCDCValuationShow——宿主拼了“中债: /行权: ”前缀，
        /// 而详情面板自带“中债”标签，整串塞入会显示成“中债 中债: …”。
        /// </summary>
        private static string ExtractValuationYtm(BondsInfoPo po)
        {
            var raw = po?.CCDCValua?.CCDCValuation;
            if (string.IsNullOrEmpty(raw)) return null;
            return raw.Split('|')[0].Trim();
        }

        private void ReplayCurrent()
        {
            long selectionVersion = Interlocked.Read(ref _selectionVersion);
            if (_lastLevels != null)
            {
                RaiseOnUi(_current, selectionVersion, _lastLevels, levels => _snapshot?.Invoke(levels));
            }
            if (_lastExec != null)
            {
                RaiseOnUi(_current, selectionVersion, _lastExec, execution => _lastExecution?.Invoke(execution));
            }
        }

        private void RaiseOnUi<T>(string securityCode, long selectionVersion, T arg, Action<T> invoke)
        {
            // 入队时检查不足以隔离旧债券；UI 真正执行时再次校验，缓存也仅在校验通过后更新。
            Action publish = () =>
            {
                if (!string.IsNullOrEmpty(securityCode)
                    && securityCode == _current
                    && selectionVersion == Interlocked.Read(ref _selectionVersion))
                {
                    invoke(arg);
                }
            };
            var ui = Application.Current?.Dispatcher;
            if (ui == null)
            {
                publish();
                return;
            }
            ui.BeginInvoke(publish);
        }

        // ============ 快照解析（取 CTPriceControler.UpdateDataToBaseDV 的最小子集） ============

        private static QuoteFeedLevel[] ParseLevels(MarketDataSnapshotFullRefresh snapshot)
        {
            var entries = snapshot.NoMDEntries;
            if (entries == null || entries.Count == 0) return Array.Empty<QuoteFeedLevel>();

            var levels = new List<QuoteFeedLevel>(entries.Count);
            foreach (var entry in entries)
            {
                // 无价档（不完整订单）跳过：面板档位表以价格为主键
                var price = ParseDecimal(entry.MDEntryPx);
                if (price == null) continue;

                var side = entry.MDEntryType == "1" ? QuoteFeedSide.Ofr : QuoteFeedSide.Bid;

                // 来源列与 PriceAndOrderView 的 Ofr_Venues_DataTemplate 同源：
                // 渠道码 = Venues 优先、VenueTypeGW 兜底（实盘推送常只填网关类型：QDMESP=双边/ODM=X-Bond）；
                // Source = GenerateSourceByVenues（内部单=“角色-交易员”），展示标签经 VenuesSimpleConvertor，本人单=“我”
                var venueCode = FirstNotEmpty(entry.Venues, entry.VenueTypeGW);
                var source = CTPriceControler.GenerateSourceByVenues(venueCode, entry.PartySubName, entry.RoleName);
                var isMe = LocalEnv.UserInfo != null && LocalEnv.UserInfo.UserId == entry.PartySubID;

                levels.Add(new QuoteFeedLevel
                {
                    Side = side,
                    Price = price.Value,
                    NetPrice = ParseDecimal(entry.NetPrice),
                    Yield = ParseDecimal(entry.Yield),
                    Volume = ParseDecimal(entry.MDEntrySize) ?? 0m,
                    Institution = FirstNotEmpty(entry.InstitutionName, entry.PartyName),
                    Trader = FirstNotEmpty(entry.ShowTrader, entry.TraderName),
                    VenueCode = venueCode,
                    Source = source,
                    SourceLabel = isMe ? "我" : ToVenuesLabel(source),
                    Level = ParseInt(entry.MDPriceLevel),
                    QuoteTime = ParseDateTime(entry.MDEntryDate + " " + entry.MDEntryTime),
                });
            }

            // 最优档：Bid=最高价、Ofr=最低价（MDPriceLevel 各场所语义不一，统一按价格排）
            var bids = levels.Where(l => l.Side == QuoteFeedSide.Bid)
                .OrderByDescending(l => l.Price)
                .Take(MaxLevelsPerSide);
            var ofrs = levels.Where(l => l.Side == QuoteFeedSide.Ofr)
                .OrderBy(l => l.Price)
                .Take(MaxLevelsPerSide);
            return bids.Concat(ofrs).ToArray();
        }

        private static decimal? ParseDecimal(string text)
        {
            return decimal.TryParse(text, out var value) ? value : (decimal?)null;
        }

        private static int ParseInt(string text)
        {
            return int.TryParse(text, out var value) ? value : 0;
        }

        private static DateTime? ParseDateTime(string text)
        {
            return DateTime.TryParse(text, out var value) ? value : (DateTime?)null;
        }

        private static string FirstNotEmpty(string first, string second)
        {
            return string.IsNullOrEmpty(first) ? second : first;
        }

        /// <summary>渠道码 → 展示短标签（直接复用宿主 VenuesSimpleConvertor，保证与行情小窗一致）。</summary>
        private static readonly VenuesSimpleConvertor VenuesLabelConvertor = new VenuesSimpleConvertor();

        private static string ToVenuesLabel(string source)
        {
            if (string.IsNullOrEmpty(source)) return string.Empty;
            var label = VenuesLabelConvertor.Convert(source, null, null, System.Globalization.CultureInfo.InvariantCulture) as string;
            return label ?? string.Empty;
        }
    }
}
