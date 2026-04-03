using HZtest.ViewModels.Dialogs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
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
    /// AddOrUpdateOrderDialogs.xaml 的交互逻辑
    /// </summary>
    public partial class AddOrUpdateOrderDialogs : UserControl
    {
        public AddOrUpdateOrderDialogs(AddOrUpdateOrderViewModel viewModel )
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        /// <summary>
        /// 验证输入是否为整数（订单数量输入框专用）
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void IntOnly_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = (TextBox)sender;
            var newText = textBox.Text.Insert(textBox.CaretIndex, e.Text);

            // 允许空、负号、数字
            if (!Regex.IsMatch(newText, @"^-?\d*$"))
            {
                e.Handled = true;  // 拦截非法输入
                return;
            }

            // 最小值验证（假设最小值为 0）
            if (int.TryParse(newText, out int value) && value < 0)
            {
                e.Handled = true;
            }
        }


        /// <summary>
        ///  失去焦点时强制修正
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void IntegerTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = (TextBox)sender;

            if (!int.TryParse(textBox.Text, out int value))
            {
                textBox.Text = "0";  // 非法输入归零
            }
            else if (value < 0)
            {
                textBox.Text = "0";  // 小于最小值，强制为0
            }
        }



    }
}
