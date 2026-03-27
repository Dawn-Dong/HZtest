using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace HZtest.AdditionalUIAttributes_附加UI属性.ViewHelper
{
    /// <summary>
    /// 跑马灯附加属性（MVVM 友好，自动处理文本变更/窗口缩放）
    /// 用法：<TextBlock behaviors:MarqueeHelper.IsMarquee="True" Text="{Binding AlertText}"/>
    /// </summary>
    public static class MainWindowMarqueeHelper
    {
        public static readonly DependencyProperty IsMarqueeProperty =
            DependencyProperty.RegisterAttached(
                "IsMarquee",
                typeof(bool),
                typeof(MainWindowMarqueeHelper),
                new PropertyMetadata(false, OnIsMarqueeChanged));

        public static readonly DependencyProperty SpeedProperty =
            DependencyProperty.RegisterAttached(
                "Speed",
                typeof(double),
                typeof(MainWindowMarqueeHelper),
                new PropertyMetadata(70.0));

        private static readonly DependencyProperty MarqueeStateProperty =
            DependencyProperty.RegisterAttached(
                "MarqueeState",
                typeof(MarqueeState),
                typeof(MainWindowMarqueeHelper));

        public static bool GetIsMarquee(DependencyObject obj) => (bool)obj.GetValue(IsMarqueeProperty);
        public static void SetIsMarquee(DependencyObject obj, bool value) => obj.SetValue(IsMarqueeProperty, value);
        public static double GetSpeed(DependencyObject obj) => (double)obj.GetValue(SpeedProperty);
        public static void SetSpeed(DependencyObject obj, double value) => obj.SetValue(SpeedProperty, value);

        private class MarqueeState
        {
            public double CurrentX { get; set; }
            public double Speed { get; set; }
            public DateTime LastFrameTime { get; set; }
        }

        private static void OnIsMarqueeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not TextBlock textBlock) return;
            if (textBlock.Parent is not Canvas canvas) return;

            if ((bool)e.NewValue)
            {
                StartMarquee(textBlock, canvas);
            }
            else
            {
                StopMarquee(textBlock);
            }
        }

        private static void StartMarquee(TextBlock textBlock, Canvas canvas)
        {
            // 等待布局完成
            textBlock.Dispatcher.BeginInvoke(() =>
            {
                var state = new MarqueeState
                {
                    CurrentX = canvas.ActualWidth,
                    Speed = GetSpeed(textBlock),
                    LastFrameTime = DateTime.Now
                };
                textBlock.SetValue(MarqueeStateProperty, state);

                Canvas.SetLeft(textBlock, state.CurrentX);

                // 使用 CompositionTarget.Rendering 实现60fps动画
                CompositionTarget.Rendering += OnRendering;

                void OnRendering(object sender, EventArgs args)
                {
                    if (!GetIsMarquee(textBlock))
                    {
                        CompositionTarget.Rendering -= OnRendering;
                        return;
                    }

                    var now = DateTime.Now;
                    var deltaTime = (now - state.LastFrameTime).TotalSeconds;
                    state.LastFrameTime = now;

                    var textWidth = textBlock.ActualWidth;
                    var canvasWidth = canvas.ActualWidth;

                    if (textWidth <= 0 || canvasWidth <= 0) return;

                    // 计算新位置
                    state.CurrentX -= state.Speed * deltaTime;

                    // 循环滚动：完全离开左侧后，从右侧重新进入
                    if (state.CurrentX < -textWidth)
                    {
                        state.CurrentX = canvasWidth;
                    }

                    Canvas.SetLeft(textBlock, state.CurrentX);
                }
            }, DispatcherPriority.Loaded);
        }

        private static void StopMarquee(TextBlock textBlock)
        {
            textBlock.ClearValue(MarqueeStateProperty);
            Canvas.SetLeft(textBlock, 0);
        }
    }
}
