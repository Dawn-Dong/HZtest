using HZtest.Resources_资源.Control.ViewModel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HZtest.Resources_资源.Control
{
    /// <summary>
    /// RelativeCoordinateSystemControl.xaml 的交互逻辑
    /// </summary>
    public partial class RelativeCoordinateSystemControl : UserControl
    {
        // ✅ 关键：添加依赖属性，让外部页面可传递数据
        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register(
                "Data",
                typeof(RelativeCoordinateSystemViewModel),
                typeof(RelativeCoordinateSystemControl),
                new PropertyMetadata(null, OnDataChanged));

        public RelativeCoordinateSystemViewModel Data
        {
            get => (RelativeCoordinateSystemViewModel)GetValue(DataProperty);
            set => SetValue(DataProperty, value);
        }

        private static void OnDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (RelativeCoordinateSystemControl)d;
            control.DataContext = e.NewValue;
        }

        public RelativeCoordinateSystemControl()
        {
            InitializeComponent();
            // 设置默认ViewModel（可选，用于设计时）
            DataContext = new RelativeCoordinateSystemViewModel();
        }
    }
}
