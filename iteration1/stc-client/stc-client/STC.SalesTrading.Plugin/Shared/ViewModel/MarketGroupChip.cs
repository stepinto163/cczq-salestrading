using System;
using System.Windows.Input;
using STC.SalesTrading.Plugin.Shared.Services;

namespace STC.SalesTrading.Plugin.Shared.ViewModel
{
    /// <summary>
    /// 侧栏「关注债券 / 筛选分组」区的一个分组 chip（原型 renderMarketGroupTags 生成的 .filter-tag）。
    /// title = 「名称 · N只债券 / N个条件」；激活态 = 原型 .active（点击切换，蓝/金底高亮）。
    /// 由 <see cref="Modules.MarketCenter.ViewModel.MarketCenterViewModel"/> 按面板市场构建，
    /// 激活集变化时（<see cref="MarketGroupSelectionService.ActiveGroupsChanged"/>）统一回填 IsActive。
    /// </summary>
    public class MarketGroupChip : System.ComponentModel.INotifyPropertyChanged
    {
        public MarketGroupChip(FavBondGroup group)
        {
            GroupId = group.Id;
            Name = group.Name;
            Title = group.Name + " · " + group.Bonds.Count + "只债券";
            IsFavorite = true;
        }

        public MarketGroupChip(FilterNamedGroup group)
        {
            GroupId = group.Id;
            Name = group.Name;
            Title = group.Name + " · " + group.ConditionCount + "个条件";
            IsFavorite = false;
        }

        public string GroupId { get; }
        public string Name { get; }
        public string Title { get; }
        public bool IsFavorite { get; }

        private bool _isActive;
        public bool IsActive
        {
            get { return _isActive; }
            set
            {
                if (_isActive != value)
                {
                    _isActive = value;
                    PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(nameof(IsActive)));
                }
            }
        }

        /// <summary>原型 filter-tag 有 market-group-error 态（分组查询失败）；客户端求值无此态，保留位。</summary>
        public string Error { get; set; }

        private RelayCommand _toggleCommand;
        public ICommand ToggleCommand
        {
            get
            {
                if (_toggleCommand == null)
                {
                    _toggleCommand = new RelayCommand(_ => MarketGroupSelectionService.Toggle(GroupId, IsFavorite));
                }
                return _toggleCommand;
            }
        }

        /// <summary> chip 激活态刷新入口（由 MarketCenterViewModel 在 ActiveGroupsChanged 时批量调用）。</summary>
        public void RefreshActive()
        {
            IsActive = IsFavorite
                ? MarketGroupSelectionService.IsFavoriteActive(GroupId)
                : MarketGroupSelectionService.IsFilterActive(GroupId);
        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        private sealed class RelayCommand : ICommand
        {
            private readonly Action<object> _execute;
            public RelayCommand(Action<object> execute) { _execute = execute; }
            public event EventHandler CanExecuteChanged { add { } remove { } }
            public bool CanExecute(object parameter) { return true; }
            public void Execute(object parameter) { _execute(parameter); }
        }
    }
}
