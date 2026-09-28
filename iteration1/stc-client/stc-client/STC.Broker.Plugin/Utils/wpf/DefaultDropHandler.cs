using  STC.Broker.Plugin.Utils.wpf.Interface;
using  STC.Broker.Plugin.Utils.wpf.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace  STC.Broker.Plugin.Utils.wpf
{
    internal class DefaultDropHandler : IDropTarget
    {
        public virtual void DragOver(DropInfo dropInfo)
        {
            if (CanAcceptData(dropInfo))
            {
                dropInfo.Effects = DragDropEffects.Copy;
                dropInfo.DropTargetAdorner = DropTargetAdorners.Insert;
            }
        }

        public virtual void Drop(DropInfo dropInfo)
        {
            int num = dropInfo.InsertIndex;
            IList list = GetList(dropInfo.TargetCollection);
            var sourceIndex = list.IndexOf(dropInfo.Data);
            if (sourceIndex > -1 && dropInfo.DragInfo != null && dropInfo.DragInfo.VisualSource == dropInfo.VisualTarget)
            {
                IList list2 = GetList(dropInfo.DragInfo.SourceCollection);
                int num2 = list2.IndexOf(dropInfo.Data);
                if (num2 != -1 && num2 != num)
                {
                    list2.RemoveAt(num2);
                    if (num < num2)
                    {
                        list2.Insert(num, dropInfo.Data);
                    }
                    else
                    {
                        list2.Insert(num - 1, dropInfo.Data);
                    }
                }
            }

            //IEnumerable enumerable = ExtractData(dropInfo.Data);
            //if (dropInfo.DragInfo != null && dropInfo.DragInfo.VisualSource == dropInfo.VisualTarget)
            //{
            //    IList list2 = GetList(dropInfo.DragInfo.SourceCollection);
            //    foreach (object item in enumerable)
            //    {
            //        int num2 = list2.IndexOf(item);
            //        if (num2 != -1)
            //        {
            //            list2.RemoveAt(num2);
            //            if (list2 == list && num2 < num)
            //            {
            //                num--;
            //            }
            //            break;
            //        }
            //    }
            //}

            //foreach (object item2 in enumerable)
            //{
            //    list.Insert(num++, item2);
            //    break;
            //}
        }

        protected static bool CanAcceptData(DropInfo dropInfo)
        {
            if (dropInfo.DragInfo != null)
            {
                if (dropInfo.DragInfo.SourceCollection == dropInfo.TargetCollection)
                {
                    return GetList(dropInfo.TargetCollection) != null;
                }

                if (dropInfo.DragInfo.SourceCollection is ItemCollection)
                {
                    return false;
                }

                if (TestCompatibleTypes(dropInfo.TargetCollection, dropInfo.Data))
                {
                    return !IsChildOf(dropInfo.VisualTargetItem, dropInfo.DragInfo.VisualSourceItem);
                }
            }

            return false;
        }

        protected static IEnumerable ExtractData(object data)
        {
            if (data is IEnumerable && !(data is string))
            {
                return (IEnumerable)data;
            }

            return Enumerable.Repeat(data, 1);
        }

        protected static IList GetList(IEnumerable enumerable)
        {
            if (enumerable is ICollectionView)
            {
                return ((ICollectionView)enumerable).SourceCollection as IList;
            }

            return enumerable as IList;
        }

        protected static bool IsChildOf(UIElement targetItem, UIElement sourceItem)
        {
            for (ItemsControl itemsControl = ItemsControl.ItemsControlFromItemContainer(targetItem); itemsControl != null; itemsControl = ItemsControl.ItemsControlFromItemContainer(itemsControl))
            {
                if (itemsControl == sourceItem)
                {
                    return true;
                }
            }

            return false;
        }

        protected static bool TestCompatibleTypes(IEnumerable target, object data)
        {
            TypeFilter filter = (Type t, object o) => t.IsGenericType && (object)t.GetGenericTypeDefinition() == typeof(IEnumerable<>);
            Type[] source = target.GetType().FindInterfaces(filter, null);
            IEnumerable<Type> source2 = source.Select((Type i) => i.GetGenericArguments().Single());
            if (source2.Count() > 0)
            {
                Type dataType = TypeUtilities.GetCommonBaseClass(ExtractData(data));
                return source2.Any((Type t) => t.IsAssignableFrom(dataType));
            }

            return target is IList;
        }
    }
}
