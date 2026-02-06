using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace HZtest.AdditionalUIAttributes_附加UI属性
{
    /// <summary>
    /// 跑马灯附加属性（MVVM 友好，自动处理文本变更/窗口缩放）
    /// 用法：<TextBlock behaviors:MarqueeHelper.IsMarquee="True" Text="{Binding AlertText}"/>
    /// </summary>
    public class MainWindowMarqueeHelper
    {

        // ===== 附加属性：是否启用跑马灯 =====
        public static readonly DependencyProperty IsMarqueeProperty =
            DependencyProperty.RegisterAttached(
                "IsMarquee", typeof(bool), typeof(MainWindowMarqueeHelper),
                new PropertyMetadata(false, OnIsMarqueeChanged));


        private static readonly DependencyProperty AnimationClockProperty =
    DependencyProperty.RegisterAttached(
        "AnimationClock", typeof(AnimationClock), typeof(MainWindowMarqueeHelper), null);

        //停止方法，备用暂未实现通过
        //private static void SetAnimationClock(DependencyObject obj, AnimationClock value)
        //    => obj.SetValue(AnimationClockProperty, value);
        //private static AnimationClock GetAnimationClock(DependencyObject obj)
        //    => (AnimationClock)obj.GetValue(AnimationClockProperty);


        public static void SetIsMarquee(DependencyObject obj, bool value) => obj.SetValue(IsMarqueeProperty, value);
        public static bool GetIsMarquee(DependencyObject obj) => (bool)obj.GetValue(IsMarqueeProperty);

        // ===== 附加属性：滚动速度（像素/秒）=====
        public static readonly DependencyProperty SpeedProperty =
            DependencyProperty.RegisterAttached(
                "Speed", typeof(double), typeof(MainWindowMarqueeHelper),
                new PropertyMetadata(60.0));

        public static void SetSpeed(DependencyObject obj, double value) => obj.SetValue(SpeedProperty, value);
        public static double GetSpeed(DependencyObject obj) => (double)obj.GetValue(SpeedProperty);

        // ===== 核心逻辑 =====
        private static void OnIsMarqueeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBlock textBlock)
            {
                if ((bool)e.NewValue)
                    AttachEvents(textBlock);
                else
                    DetachEvents(textBlock);
            }
        }

        private static void AttachEvents(TextBlock textBlock)
        {
            textBlock.Loaded += OnTextBlockLoaded;
            textBlock.Unloaded += OnTextBlockUnloaded;
            textBlock.SizeChanged += OnTextBlockSizeChanged;
        }

        private static void DetachEvents(TextBlock textBlock)
        {
            textBlock.Loaded -= OnTextBlockLoaded;
            textBlock.Unloaded -= OnTextBlockUnloaded;
            textBlock.SizeChanged -= OnTextBlockSizeChanged;
            textBlock.BeginAnimation(Canvas.LeftProperty, null);
        }

        private static void OnTextBlockLoaded(object sender, RoutedEventArgs e)
        {
            if (sender is TextBlock tb && VisualTreeHelper.GetParent(tb) is Canvas canvas)
            {
                canvas.ClipToBounds = true; // ✅ 关键：裁剪超出部分
                StartAnimation(tb, canvas);
            }
        }

        private static void OnTextBlockUnloaded(object sender, RoutedEventArgs e)
        {
            if (sender is TextBlock tb) tb.BeginAnimation(Canvas.LeftProperty, null);
        }

        private static void OnTextBlockSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (sender is TextBlock tb &&
                GetIsMarquee(tb) &&
                tb.IsLoaded &&
                VisualTreeHelper.GetParent(tb) is Canvas canvas)
            {
                StartAnimation(tb, canvas); // ✅ 文本变更/窗口缩放时自动重算
            }
        }

        private static void StartAnimation(TextBlock textBlock, Canvas canvas)
        {
            textBlock.Measure(new Size(double.PositiveInfinity, canvas.ActualHeight));
            double textWidth = textBlock.DesiredSize.Width;
            double containerWidth = canvas.ActualWidth;

            // 文本未超宽：居中显示，停止动画
            if (textWidth <= containerWidth)
            {
                Canvas.SetLeft(textBlock, (containerWidth - textWidth) / 2);
                textBlock.BeginAnimation(Canvas.LeftProperty, null);
                return;
            }

            // 停止旧动画，重置起始位置
            textBlock.BeginAnimation(Canvas.LeftProperty, null);
            Canvas.SetLeft(textBlock, containerWidth);

            // ✅ 匀速滚动（WPF 默认行为，不设置 EasingFunction）
            var animation = new DoubleAnimation
            {
                From = containerWidth,
                To = -textWidth,
                Duration = TimeSpan.FromSeconds((textWidth + containerWidth) / GetSpeed(textBlock)),
                RepeatBehavior = RepeatBehavior.Forever,
                //实验



                // EasingFunction = new BounceEase { Bounces = 2, EasingMode = EasingMode.EaseOut }
                // 重要：此处不设置 EasingFunction → 保持匀速
            };

            textBlock.BeginAnimation(Canvas.LeftProperty, animation);
        }






    }
}
