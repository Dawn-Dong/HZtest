using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace HZtest.AdditionalUIAttributes_附加UI属性.ViewHelper
{
    public static class InlineBindingHelper
    {
        public static readonly DependencyProperty InlinesSourceProperty =
            DependencyProperty.RegisterAttached(
                "InlinesSource",
                typeof(ObservableCollection<Inline>),
                typeof(InlineBindingHelper),
                new PropertyMetadata(null, OnInlinesSourceChanged));

        public static ObservableCollection<Inline> GetInlinesSource(DependencyObject obj) =>
            (ObservableCollection<Inline>)obj.GetValue(InlinesSourceProperty);

        public static void SetInlinesSource(DependencyObject obj, ObservableCollection<Inline> value) =>
            obj.SetValue(InlinesSourceProperty, value);

        private static void OnInlinesSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not TextBlock textBlock) return;

            // 清理旧事件
            if (e.OldValue is ObservableCollection<Inline> oldCollection)
            {
                oldCollection.CollectionChanged -= (s, args) => UpdateInlines(textBlock, args);
            }

            textBlock.Inlines.Clear();

            if (e.NewValue is not ObservableCollection<Inline> newCollection) return;

            // 添加初始内容
            foreach (var inline in newCollection)
            {
                textBlock.Inlines.Add(inline);
            }

            // 监听变化
            newCollection.CollectionChanged += (s, args) =>
                UpdateInlines(textBlock, args, newCollection);
        }

        private static void UpdateInlines(TextBlock textBlock,
            System.Collections.Specialized.NotifyCollectionChangedEventArgs args,
            ObservableCollection<Inline> collection = null)
        {
            if (args == null && collection != null)
            {
                // 全量更新
                textBlock.Inlines.Clear();
                foreach (var inline in collection)
                {
                    textBlock.Inlines.Add(inline);
                }
                return;
            }

            switch (args.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    foreach (Inline inline in args.NewItems)
                    {
                        textBlock.Inlines.Add(inline);
                    }
                    break;

                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    foreach (Inline inline in args.OldItems)
                    {
                        textBlock.Inlines.Remove(inline);
                    }
                    break;

                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                    textBlock.Inlines.Clear();
                    if (collection != null)
                    {
                        foreach (var inline in collection)
                        {
                            textBlock.Inlines.Add(inline);
                        }
                    }
                    break;
            }
        }
    }
}