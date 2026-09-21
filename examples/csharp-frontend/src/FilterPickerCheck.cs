using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;

namespace Ficc.FrontendLearning
{
    // Tiny, offline WinForms interaction check. No quotes, network or benchmark load.
    internal static class FilterPickerCheck
    {
        internal static int Run()
        {
            string output = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "filter-picker-check.txt");
            try
            {
                var fields = new JArray(
                    Field("ownershipType", "string"),
                    Field("urgent", "boolean"),
                    Field("price", "decimal"),
                    Field("securityName", "string"),
                    Field("liquidityGrade", "string"));
                var dictionaries = new JObject
                {
                    ["ownershipType"] = new JArray(
                        new JObject { ["value"] = "YQ", ["label"] = "央企" },
                        new JObject { ["value"] = "GQ", ["label"] = "国企" })
                };
                using (var editor = new ProductFilterForm(fields, new JObject { ["conditions"] = new JArray() }, dictionaries))
                {
                    Exception failure = null;
                    editor.Shown += (sender, args) => editor.BeginInvoke(new Action(() =>
                    {
                        try
                        {
                            var grid = editor.Controls.OfType<DataGridView>().Single();
                            Check(grid.Rows[0].Cells[3] is DataGridViewButtonCell, "top-level dictionary fallback");
                            Check(grid.Rows[1].Cells[3] is DataGridViewButtonCell, "boolean picker enabled");
                            Check(!(grid.Rows[2].Cells[3] is DataGridViewButtonCell), "numeric input has no dead button");
                            Check(!(grid.Rows[3].Cells[3] is DataGridViewButtonCell), "text input has no dead button");
                            Check((string)grid.Rows[4].Cells[3].Value == "字典未提供", "missing enum is explained");
                            ClickPicker(editor, grid, 0, 1);
                            Check((string)grid.Rows[0].Cells[2].Value == "GQ", "enum code returned");
                            ClickPicker(editor, grid, 1, 1);
                            Check((string)grid.Rows[1].Cells[1].Value == "EQ", "boolean EQ");
                            Check(!bool.Parse((string)grid.Rows[1].Cells[2].Value), "boolean false returned");
                            grid.Rows[2].Cells[1].Value = "EXISTS";
                            Check(grid.Rows[2].Cells[3] is DataGridViewButtonCell, "EXISTS enables selector");
                            ClickPicker(editor, grid, 2, 1);
                            Check((string)grid.Rows[2].Cells[1].Value == "EXISTS", "EXISTS preserved");
                            editor.Controls.OfType<FlowLayoutPanel>().Single().Controls.OfType<Button>()
                                .Single(button => button.Text == "应用").PerformClick();
                            var conditions = (JArray)editor.Filter["conditions"];
                            Check(conditions.Count == 3, "three selected conditions");
                            Check(conditions[1]["value"].Type == JTokenType.Boolean, "typed boolean payload");
                            Check(conditions[2]["value"].Type == JTokenType.Boolean, "typed EXISTS payload");
                        }
                        catch (Exception error)
                        {
                            failure = error;
                        }
                        finally
                        {
                            editor.Close();
                        }
                    }));
                    editor.ShowDialog();
                    if (failure != null)
                    {
                        throw failure;
                    }
                }
                File.WriteAllText(output, "PASS enum, boolean, EXISTS, direct input, missing dictionary, and selected payloads");
                return 0;
            }
            catch (Exception error)
            {
                File.WriteAllText(output, error.ToString());
                return 1;
            }
        }

        private static JObject Field(string name, string type)
        {
            return new JObject
            {
                ["field"] = name, ["label"] = name, ["type"] = type,
                ["operators"] = new JArray("EQ", "IN", "EXISTS")
            };
        }

        private static void ClickPicker(Form editor, DataGridView grid, int row, int selected)
        {
            using (var selectTimer = new Timer { Interval = 30 })
            {
                selectTimer.Tick += (sender, args) =>
                {
                    var picker = Application.OpenForms.Cast<Form>().FirstOrDefault(form => form != editor
                        && form.Controls.OfType<CheckedListBox>().Any());
                    if (picker == null)
                    {
                        return;
                    }
                    selectTimer.Stop();
                    picker.Controls.OfType<CheckedListBox>().Single().SetItemChecked(selected, true);
                    picker.Controls.OfType<Button>().Single().PerformClick();
                };
                selectTimer.Start();
                // Exercise the real registered grid event handler and modal picker.
                typeof(DataGridView).GetMethod("OnCellContentClick", BindingFlags.Instance | BindingFlags.NonPublic)
                    .Invoke(grid, new object[] { new DataGridViewCellEventArgs(3, row) });
            }
        }

        private static void Check(bool passed, string description)
        {
            if (!passed)
            {
                throw new InvalidOperationException(description);
            }
        }
    }
}
