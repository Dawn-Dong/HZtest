using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace HZtest.Infrastructure_基础设施
{
    public static class TreeViewHelper
    {
        #region SelectedItem 附加属性 给 TreeView 打补丁，让它支持 MVVM 绑定
        // 附加属性定义
        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.RegisterAttached(
                "SelectedItem",
                typeof(object),
                typeof(TreeViewHelper),
                new PropertyMetadata(null, OnSelectedItemChanged));

        public static object GetSelectedItem(DependencyObject obj) =>
            obj.GetValue(SelectedItemProperty);

        public static void SetSelectedItem(DependencyObject obj, object value) =>
            obj.SetValue(SelectedItemProperty, value);

        private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TreeView treeView)
            {
                // 移除旧事件（防止重复订阅）
                treeView.SelectedItemChanged -= OnTreeViewSelectedItemChanged;

                // 添加新事件
                if (e.NewValue != null)
                {
                    treeView.SelectedItemChanged += OnTreeViewSelectedItemChanged;
                }
            }
        }

        private static void OnTreeViewSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (sender is TreeView treeView)
            {
                // 将 TreeView 的选中项同步到附加属性
                SetSelectedItem(treeView, e.NewValue);
            }
        }

        #endregion
    }
}
