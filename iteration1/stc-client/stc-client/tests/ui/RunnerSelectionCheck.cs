using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace SalesTrading.UiTests
{
    internal static class RunnerSelectionCheck
    {
        [STAThread]
        public static int Main()
        {
            try
            {
                Application.EnableVisualStyles();
                // 独立验证普通启动默认值，避免继承外部 QTrade 测试窗口的账户设置。
                Environment.SetEnvironmentVariable("STC_UI_USER", null);
                Environment.SetEnvironmentVariable("STC_UI_PASSWORD", null);
                string source = Path.Combine(Directory.GetCurrentDirectory(), "cases-" + Guid.NewGuid().ToString("N"));
                Directory.CreateDirectory(source);
                for (int index = 1; index <= 3; index++)
                {
                    var item = new CaseDefinition { Id = "selection-" + index, Name = "选择检查 " + index };
                    item.Page = index == 2 ? "02 行情中心/01 利率全部" : "01 报价录入/01 精确报价";
                    item.Feature = "选择检查";
                    item.Steps.Add(new TestStep { Name = "仅检查管理器，不执行业务", Action = "management.select" });
                    item.Save(Path.Combine(source, item.Id + ".json"));
                }
                var advancedCase = new CaseDefinition { Id = "advanced-selection", Name = "双账号检查" };
                advancedCase.Steps.Add(new TestStep { Action = "realtime.lifecycle", Name = "观察实时更新" });
                advancedCase.Steps[0].Parameters["page"] = "market";
                advancedCase.Steps[0].Parameters["tab"] = "rateAll";
                advancedCase.Steps[0].Parameters["view"] = "BoardListView";
                advancedCase.Steps[0].Parameters["securityId"] = "240210.IB";
                advancedCase.Steps[0].Parameters["price"] = "0.9";
                advancedCase.Save(Path.Combine(source, advancedCase.Id + ".json"));
                Type suiteType = typeof(CaseDefinition).Assembly.GetType("SalesTrading.UiTests.SuiteRunner", true);
                object suite = Activator.CreateInstance(suiteType);
                foreach (bool enabled in new[] { false, true })
                {
                    suiteType.GetProperty("AdvancedMode").SetValue(suite, enabled);
                    suiteType.GetProperty("ObserverUser").SetValue(suite, "TEST2");
                    suiteType.GetProperty("ObserverPassword").SetValue(suite, "123456");
                    try
                    {
                        var task = (System.Threading.Tasks.Task<bool>)suiteType.GetMethod("RunAsync").Invoke(suite,
                            new object[] { new[] { advancedCase }, "", "test2", "123456", "240210.IB", "" });
                        task.GetAwaiter().GetResult();
                        throw new Exception("未阻止关闭高级模式或重复账号的双账号测试");
                    }
                    catch (InvalidOperationException error)
                    {
                        Require(error.Message.Contains("双账号"), "未在启动前检查双账号配置");
                    }
                }
                Type type = typeof(CaseDefinition).Assembly.GetType("SalesTrading.UiTests.RunnerWindow", true);
                using (Form form = (Form)Activator.CreateInstance(type, new object[] { source, null }))
                {
                    form.Show();
                    Application.DoEvents();
                    BindingFlags fields = BindingFlags.Instance | BindingFlags.NonPublic;
                    Require(((TextBox)type.GetField("user", fields).GetValue(form)).Text == "test2", "默认账号错误");
                    var password = (TextBox)type.GetField("password", fields).GetValue(form);
                    Require(password.Text == "123456" && password.UseSystemPasswordChar, "默认密码或掩码错误");
                    var playback = (Form)type.GetField("playback", fields).GetValue(form);
                    Type playbackType = playback.GetType();
                    Require((int)playbackType.GetProperty("DelayMilliseconds").GetValue(playback) == 2000,
                        "默认1倍速应为2秒");
                    playback.Show();
                    Application.DoEvents();
                    Require(playback.TopMost && !playback.Modal && playback.ClientSize.Width <= 500,
                        "播放控制必须是小型独立置顶浮动窗");
                    var speed = (TrackBar)playbackType.GetField("speed", fields).GetValue(playback);
                    speed.Value = 0;
                    Require((int)playbackType.GetProperty("DelayMilliseconds").GetValue(playback) == 8000, "慢速应为8秒");
                    speed.Value = 5;
                    Require((int)playbackType.GetProperty("DelayMilliseconds").GetValue(playback) == 0, "最快应取消额外等待");
                    speed.Value = 2;
                    playbackType.GetMethod("SetRunning").Invoke(playback, new object[] { true });
                    FindButton(playback, "暂停").PerformClick();
                    Require(FindButton(playback, "继续") != null && FindButton(playback, "下一动作").Enabled,
                        "暂停后应可继续或单步");
                    FindButton(playback, "继续").PerformClick();
                    Require(!FindButton(playback, "下一动作").Enabled, "连续播放时不得单步");
                    using (var picture = new System.Drawing.Bitmap(playback.Width, playback.Height))
                    {
                        playback.DrawToBitmap(picture, new System.Drawing.Rectangle(0, 0, playback.Width, playback.Height));
                        picture.Save(Path.Combine(Directory.GetCurrentDirectory(), "playback.png"));
                    }
                    playbackType.GetMethod("SetRunning").Invoke(playback, new object[] { false });
                    playback.Close();
                    Require(!playback.IsDisposed && !playback.Visible, "关闭控制窗应只隐藏，允许重新打开");
                    var grid = (DataGridView)type.GetField("cases", fields).GetValue(form);
                    var count = (Label)type.GetField("selectionCount", fields).GetValue(form);
                    Require(grid.MultiSelect && grid.Rows.Count == 3, "用例列表未启用多选");
                    FindButton(form, "取消全选").PerformClick();
                    Require(CheckedCount(grid) == 0 && grid.SelectedRows.Count == 0, "取消全选失败");
                    FindButton(form, "全选").PerformClick();
                    Require(CheckedCount(grid) == 3 && grid.SelectedRows.Count == 3, "全选必须同时勾选并高亮");
                    Require(count.Text.Contains("3 / 3"), "勾选数量未更新");
                    FindButton(form, "取消全选").PerformClick();
                    grid.CurrentCell = grid.Rows[0].Cells[1];
                    grid.ClearSelection();
                    grid.Rows[0].Selected = true;
                    grid.Rows[2].Selected = true;
                    Require(grid.SelectedRows.Count == 2, "多行选择未保留");
                    var key = new KeyEventArgs(Keys.Space);
                    typeof(DataGridView).GetMethod("OnKeyDown", fields).Invoke(grid, new object[] { key });
                    Require(CheckedCount(grid) == 2 && !(bool)grid.Rows[1].Cells[0].Value && key.SuppressKeyPress,
                        "空格应只勾选高亮的两行且阻止二次切换");
                    typeof(DataGridView).GetMethod("OnKeyDown", fields).Invoke(grid, new object[] { new KeyEventArgs(Keys.Space) });
                    Require(CheckedCount(grid) == 0, "再次空格未取消多行勾选");
                    grid.CurrentCell = grid.Rows[0].Cells[0];
                    grid.Focus();
                    grid.BeginEdit(false);
                    // 调用真实复选框内容点击处理器，不移动用户正在使用的鼠标。
                    typeof(DataGridViewCheckBoxCell).GetMethod("OnContentClick", fields)
                        .Invoke(grid.Rows[0].Cells[0], new object[] { new DataGridViewCellEventArgs(0, 0) });
                    Application.DoEvents();
                    Require(CheckedCount(grid) == 1 && !grid.IsCurrentCellDirty,
                        "复选框勾选未立即提交：count=" + CheckedCount(grid) + ", dirty=" + grid.IsCurrentCellDirty);
                    FindButton(form, "全选").PerformClick();
                    grid.EndEdit();
                    Require(CheckedCount(grid) == 3, "当前复选框编辑覆盖了全选结果");
                    FindButton(form, "取消全选").PerformClick();
                    FindButton(form, "运行勾选").PerformClick();
                    Require(type.GetField("runner", fields).GetValue(form) == null, "空选择不应启动业务测试");
                    var tree = (TreeView)type.GetField("pageTree", fields).GetValue(form);
                    Require(tree.Width >= 200 && tree.Width < form.ClientSize.Width / 3 && grid.Width > 700,
                        "页面树挤占用例列表宽度");
                    TreeNode allPages = tree.Nodes[0];
                    tree.SelectedNode = allPages.Nodes.Cast<TreeNode>().Single(node => (string)node.Tag == "01 报价录入");
                    Application.DoEvents();
                    Require(grid.Rows.Cast<DataGridViewRow>().Count(row => row.Visible) == 2, "页面树未筛选子页面用例");
                    FindButton(form, "全选").PerformClick();
                    Require(CheckedCount(grid) == 2, "页面范围全选误勾选其他页面");
                    tree.SelectedNode = allPages.Nodes.Cast<TreeNode>().Single(node => (string)node.Tag == "02 行情中心");
                    Application.DoEvents();
                    Require(grid.Rows.Cast<DataGridViewRow>().Count(row => row.Visible) == 1, "切换页面范围失败");
                    FindButton(form, "运行勾选").PerformClick();
                    Require(type.GetField("runner", fields).GetValue(form) == null, "不得运行其他页面隐藏的勾选项");
                    tree.SelectedNode = allPages;
                    Require(CheckedCount(grid) == 2, "切换页面丢失已有勾选状态");
                    var advanced = (CheckBox)type.GetField("advanced", fields).GetValue(form);
                    var observer = (TextBox)type.GetField("observerUser", fields).GetValue(form);
                    var observerPassword = (TextBox)type.GetField("observerPassword", fields).GetValue(form);
                    Require(!advanced.Checked && !observer.Enabled && !observerPassword.Enabled,
                        "普通模式不得启用观察账号或显示高级用例");
                    advanced.Checked = true;
                    Application.DoEvents();
                    Require(grid.Rows.Count == 4 && observer.Enabled && observerPassword.Enabled
                        && observerPassword.UseSystemPasswordChar, "高级模式未显示双账号用例和登录设置");
                    Require(observer.Bottom <= observer.Parent.ClientSize.Height, "观察账号输入框被裁剪");
                    advanced.Checked = false;
                    Application.DoEvents();
                    Require(grid.Rows.Count == 3, "退出高级模式后仍显示双账号用例");
                    advanced.Checked = true;
                    tree.ExpandAll();
                    using (var screenshot = new System.Drawing.Bitmap(form.Width, form.Height))
                    {
                        form.DrawToBitmap(screenshot, new System.Drawing.Rectangle(0, 0, form.Width, form.Height));
                        screenshot.Save(Path.Combine(Directory.GetCurrentDirectory(), "page-tree.png"));
                    }
                    form.Close();
                }
                Console.WriteLine("PASS: account/speed defaults, floating playback window, pause/resume controls, multiple selection, checkbox commit, page hierarchy, scoped select-all, hidden-case exclusion, empty-run guard, advanced mode visibility and observer settings.");
                return 0;
            }
            catch (Exception error)
            {
                Console.Error.WriteLine(error);
                return 1;
            }
        }

        private static int CheckedCount(DataGridView grid)
        {
            return grid.Rows.Cast<DataGridViewRow>().Count(row => (bool?)row.Cells[0].Value == true);
        }

        private static Button FindButton(Control parent, string text)
        {
            foreach (Control child in parent.Controls)
            {
                if (child is Button button && button.Text == text) return button;
                Button nested = FindButton(child, text);
                if (nested != null) return nested;
            }
            return null;
        }

        private static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }

    }
}
