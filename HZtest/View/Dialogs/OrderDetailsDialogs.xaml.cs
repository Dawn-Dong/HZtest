using HZtest.ViewModels.Dialogs;
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

namespace HZtest.View.Dialogs
{
    /// <summary>
    /// OrderDetailsDialogs.xaml 的交互逻辑
    /// </summary>
    public partial class OrderDetailsDialogs : UserControl
    {
        public OrderDetailsDialogs(OrderDetailsViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
