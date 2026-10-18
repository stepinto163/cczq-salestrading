using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;

namespace STC.SalesTrading.Plugin.Shared.Components
{
    /// <summary>
    /// 懒加载搜索取数参数/结果载体（参考 stc-client-lite LazySearchArgs）：
    /// 控件填 Text/PageIndex/PageSize/Parameter，实现方就地回填 Items/HasMore（同一对象进出）。
    /// </summary>
    public class LazySearchArgs
    {
        /// <summary>搜索词（输入框当前文本）。</summary>
        public string Text { get; set; }

        /// <summary>页码（1 起）。</summary>
        public int PageIndex { get; set; } = 1;

        /// <summary>页大小。</summary>
        public int PageSize { get; set; } = 50;

        /// <summary>业务附加参数（控件原样透传，语义由实现方约定，如联动机构 Id）。</summary>
        public object Parameter { get; set; }

        /// <summary>结果回填：本页候选行（行类型任意，列按 ColumnMembers 属性名取值）。</summary>
        public IReadOnlyList<object> Items { get; set; }

        /// <summary>结果回填：是否存在下一页（无总数后端的页码步进依据）。</summary>
        public bool HasMore { get; set; }
    }

    /// <summary>懒加载搜索取数命令：ICommand 形态 + 异步委托（控件 await ExecuteAsync 等结果写回）。</summary>
    public class LazySearchCommand : ICommand
    {
        private readonly Func<LazySearchArgs, Task> execute;

        public LazySearchCommand(Func<LazySearchArgs, Task> execute)
        {
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged { add { } remove { } }

        public void Execute(object parameter)
        {
            if (parameter is LazySearchArgs args) { _ = ExecuteAsync(args); }
        }

        /// <summary>控件取数入口：执行委托并等待完成（结果已写回 args）。</summary>
        public Task ExecuteAsync(LazySearchArgs args)
        {
            return execute(args);
        }
    }
}
