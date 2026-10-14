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
                string source = Path.Combine(Directory.GetCurrentDirectory(), "cases");
                Directory.CreateDirectory(source);
                for (int index = 1; index <= 3; index++)
                {
                    var item = new CaseDefinition { Id = "selection-" + index, Name = "选择检查 " + index };
                    item.Page = index == 2 ? "02 行情中心/01 利率全部" : "01 报价录入/01 精确报价";
                    item.Feature = "选择检查";
                    item.Steps.Add(new TestStep { Name = "仅检查管理器，不执行业务", Action = "management.select" });
                    item.Save(Path.Combine(source, item.Id + ".json"));
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
                    Require(((NumericUpDown)type.GetField("operationInterval", fields).GetValue(form)).Value == 2,
                        "默认操作间隔应为2秒");
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
                    tree.ExpandAll();
                    using (var screenshot = new System.Drawing.Bitmap(form.Width, form.Height))
                    {
                        form.DrawToBitmap(screenshot, new System.Drawing.Rectangle(0, 0, form.Width, form.Height));
                        screenshot.Save(Path.Combine(Directory.GetCurrentDirectory(), "page-tree.png"));
                    }
                    form.Close();
                }
                Console.WriteLine("PASS: account/interval defaults, multiple selection, checkbox commit, page hierarchy, scoped select-all, hidden-case exclusion, empty-run guard.");
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
