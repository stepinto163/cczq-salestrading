using com.gateway.log;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace STC.Chart.Plugin.FuncEx
{
    public static class FuncEx
    {
        public static T Find<T>(this ObservableCollection<T> ts, Func<T, bool> func) where T : class
        {
            if (ts == null) return null;
            foreach (var item in ts)
            {
                if (func(item))
                {
                    return item;
                }
            }
            return null;
        }

        public static List<T> FindAll<T>(this ObservableCollection<T> ts, Func<T, bool> func) where T : class
        {
            if (ts == null) return null;
            List<T> tsResult = new List<T>();
            foreach (var item in ts)
            {
                if (func(item))
                {
                    tsResult.Add(item);
                }
            }
            return tsResult;
        }
        public static bool Exists<T>(this ObservableCollection<T> ts, Func<T, bool> func) where T : class
        {
            if (ts == null) return false;
            try
            {
                foreach (var item in ts)
                {
                    if (func(item))
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error("FuncEx Exists Error", ex);
            }
            return false;
        }

        public static void Sort<T>(this ObservableCollection<T> collection) where T : class, IComparer<T>
        {

            if (collection == null || collection.Count <= 1) return;

            var lst = collection.ToList();
            lst.Sort(Activator.CreateInstance(typeof(T)) as T);
            var count = collection.Count;
            for (int m = 0; m < count; m++)
            {
                var dex = collection.IndexOf(lst[m]);
                if (dex == m) continue;
                Application.Current.Dispatcher.Invoke(() =>
                {
                    collection.Move(dex, m);
                });
            }
        }

        /// <summary>
        /// 字符串赋值，null或空则不赋值
        /// </summary>
        /// <param name="str"></param>
        /// <param name="value"></param>
        public static bool IsNull(this string str)
        {
            return string.IsNullOrEmpty(str);
        }

    public static bool Remove<T>(this ObservableCollection<T> ts, Func<T, bool> func) where T : class
        {
            if (ts == null) return false;
            foreach (var item in ts)
            {
                if (func(item))
                {
                    return ts.Remove(item);
                }
            }
            return false;
        }

        public static decimal ToDecimal(this string str)
        {
            if (string.IsNullOrEmpty(str)) return 0;
            decimal temp;
            if (decimal.TryParse(str, out temp))
            {
                return temp;
            }
            return 0;
        }

       
        /// <summary>
        /// 设置双向 属性变更绑定
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="control"></param>
        /// <param name="dependencyProperty"></param>
        /// <param name="action"></param>
        public static void SetBindingTwoWay<T>(this Control control, DependencyProperty dependencyProperty, Expression<Func<T>> action)
        {
            var memberExpression = action.Body as MemberExpression;
            if (memberExpression != null)
            {
                var binding = new Binding(memberExpression.Member.Name)
                {
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger =UpdateSourceTrigger.PropertyChanged
                };
                control.SetBinding(dependencyProperty, binding);
            }
        }

    }
}
