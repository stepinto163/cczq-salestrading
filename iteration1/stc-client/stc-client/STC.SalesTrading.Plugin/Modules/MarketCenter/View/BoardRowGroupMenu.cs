using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using STC.SalesTrading.Plugin.Shared.Services;
using STC.SalesTrading.Plugin.Shared.ViewModel;

namespace STC.SalesTrading.Plugin.Modules.MarketCenter.View
{
    /// <summary>
    /// 右键菜单「添加到分组」子菜单的动态装配（原型 contextFavoriteSubmenuHtml + addToContextFavorite +
    /// showNewGroupModal/confirmNewGroup）：列出关注分组（点击入组，已在组里不重复）、
    /// 分隔线、「+ 新建分组」（弹名称输入框，重名报「同名分组已存在」，建组并把当前行债券放入）。
    /// 在各看板的 ContextMenuOpening 里调用（此时行 DataContext 已注入）。
    /// </summary>
public static class BoardRowGroupMenu
{
    private static Style _ctxItemStyle;
    private static Style _ctxSeparatorStyle;
    private static readonly System.Windows.Media.Brush _ctxAccentBrush =
        CreateFrozenBrush(0x79, 0xC0, 0xFF);   // 原型内联 color:#79c0ff
    private static readonly System.Windows.Media.Brush _ctxMutedBrush =
        CreateFrozenBrush(0x7D, 0x85, 0x90);   // 原型内联 color:#7d8590

    private static System.Windows.Media.SolidColorBrush CreateFrozenBrush(byte r, byte g, byte b)
    {
        var brush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(r, g, b));
        brush.Freeze();
        return brush;
    }

    /// <summary>
    /// 从主题字典实例直取右键菜单资源。ContextMenu 是独立视觉树，
    /// menu.TryFindResource 只查自身 Resources → Application → 系统主题，
    /// 走不到视图级合并的 DarkTheme（宿主 App 级也没有插件的 Stc* 键），
    /// 动态子菜单项曾因此全部落到系统默认样式（黑字），深色弹层上文字不可见。
    /// </summary>
    private static void EnsureCtxResources()
    {
        if (_ctxItemStyle != null) return;
        var rd = new ResourceDictionary
        {
            Source = new Uri("/STC.SalesTrading.Plugin;component/Themes/DarkTheme.xaml", UriKind.Relative)
        };
        _ctxItemStyle = (Style)rd["StcMenuItem"];
        _ctxSeparatorStyle = (Style)rd["StcCtxSeparator"];
        // 前景色不取字典画刷：AccentBrush 等的 Color 是嵌套 DynamicResource，独立视觉树下解析不稳
    }

    /// <summary>
    /// 重建「添加到分组」子菜单。markets = 行所属市场集合（普通看板 1 个；
    /// 提醒看板行没有市场字段，传两个市场合并列出，与原型 merged 侧栏口径一致）。
    /// </summary>
    public static void Populate(ContextMenu menu, string code, string name, Window owner, params FilterProfileService.MarketKind[] markets)
    {
        if (menu == null) return;
        EnsureCtxResources();
        MenuItem root = null;
        foreach (var item in menu.Items)
        {
            var candidate = item as MenuItem;
            if (candidate != null && (candidate.Header as string) == "添加到分组")
            {
                root = candidate;
                break;
            }
        }
        if (root == null) return;

        root.Items.Clear();

        // 元组携带市场：入组落库需要（组对象本身无市场信息）
        var groups = new List<(FavBondGroup group, FilterProfileService.MarketKind market)>();
        foreach (var market in markets)
        {
            foreach (var g in FilterProfileService.GetFavGroups(market))
            {
                // 原型过滤「默认」空分组（name==='默认' 且无债券）
                if (g.Name == "默认" && g.Bonds.Count == 0) continue;
                groups.Add((g, market));
            }
        }

        if (groups.Count == 0)
        {
            root.Items.Add(new MenuItem
            {
                Header = "暂无关注分组",
                Style = _ctxItemStyle,
                Foreground = _ctxMutedBrush,
                IsEnabled = false,
            });
        }
        else
        {
            foreach (var entry in groups)
            {
                var captured = entry.group;
                var capturedMarket = entry.market;
                var item = new MenuItem
                {
                    Header = captured.Name,
                    Style = _ctxItemStyle,
                };
                item.Click += (s, e) =>
                {
                    if (!MarketGroupSelectionService.AddBondToFavoriteGroup(captured, code, name)) return;
                    ToastService.Show("已加入分组 " + captured.Name, ToastKind.Success);
                    // 直接落库（此前只写会话内存，刷新即丢）；后台执行——同步等网关会冻结 UI，
                    // 失败保留本地状态，下次弹窗保存兜底。ToastService 线程安全（内部 Dispatcher 分流）
                    var directory = ViewModel.FilterGroupManagerViewModel.Directory;
                    if (directory != null)
                    {
                        System.Threading.Tasks.Task.Run(() =>
                        {
                            try { directory.UpsertFavGroup(capturedMarket, captured); }
                            catch (Exception ex)
                            {
                                ToastService.Show("分组已本地添加，保存失败：" + ex.Message, ToastKind.Error);
                            }
                        });
                    }
                };
                root.Items.Add(item);
            }
        }

        root.Items.Add(new Separator { Style = _ctxSeparatorStyle });

        var createItem = new MenuItem
        {
            Header = "+ 新建分组",
            Style = _ctxItemStyle,
            Foreground = _ctxAccentBrush,
        };
            createItem.Click += (s, e) =>
            {
                var market = markets != null && markets.Length > 0 ? markets[0] : FilterProfileService.MarketKind.Credit;
                NewGroupWithBond(owner, market, code, name);
            };
            root.Items.Add(createItem);
        }

        /// <summary>原型 confirmNewGroup：重名报错，成功建组并把债券塞进去。</summary>
        private static void NewGroupWithBond(Window owner, FilterProfileService.MarketKind market, string code, string name)
        {
            var input = Shared.Components.PromptDialog.Show(owner, "新建分组");
            if (string.IsNullOrEmpty(input)) return;

            var groups = FilterProfileService.GetFavGroups(market);
            foreach (var g in groups)
            {
                if (g.Name == input)
                {
                    ToastService.Show("同名分组已存在", ToastKind.Error);
                    return;
                }
            }

            var group = new FavBondGroup { Name = input };
            if (!string.IsNullOrWhiteSpace(code))
            {
                group.Bonds.Add(new FavBondEntry { Code = code.Trim(), Name = name ?? string.Empty });
            }
            groups.Add(group);
            FilterProfileService.RaiseDataChanged();
            ToastService.Show("已创建分组 " + input, ToastKind.Success);
        }
    }
}
