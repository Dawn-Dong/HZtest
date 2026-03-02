using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace HZtest.Resources_资源.Control.ViewModel
{
    public class RelativeCoordinateSystemViewModel : INotifyPropertyChanged
    {


        private double _xAxisCoordinateSystemValue;
        private double _yAxisCoordinateSystemValue;
        private double _zAxisCoordinateSystemValue;
        private double _bAxisCoordinateSystemValue;
        private double _cAxisCoordinateSystemValue;
        // ✅ 添加所有命令
        public ICommand ChangeXCommand => new RelayCommand(ShowXInputDialog);
        public ICommand ChangeYCommand => new RelayCommand(ShowYInputDialog);
        public ICommand ChangeZCommand => new RelayCommand(ShowZInputDialog);
        public ICommand ChangeBCommand => new RelayCommand(ShowBInputDialog);
        public ICommand ChangeCCommand => new RelayCommand(ShowCInputDialog);
        // ✅ 所有数据属性都实现INotifyPropertyChanged
        public double XAxisCoordinateSystemValue
        {
            get => _xAxisCoordinateSystemValue;
            set
            {
                _xAxisCoordinateSystemValue = value;
                OnPropertyChanged();
            }
        }

        public double YAxisCoordinateSystemValue
        {
            get => _yAxisCoordinateSystemValue;
            set
            {
                _yAxisCoordinateSystemValue = value;
                OnPropertyChanged();
            }
        }

        public double ZAxisCoordinateSystemValue
        {
            get => _zAxisCoordinateSystemValue;
            set
            {
                _zAxisCoordinateSystemValue = value;
                OnPropertyChanged();
            }
        }

        public double BAxisCoordinateSystemValue
        {
            get => _bAxisCoordinateSystemValue;
            set
            {
                _bAxisCoordinateSystemValue = value;
                OnPropertyChanged();
            }
        }

        public double CAxisCoordinateSystemValue
        {
            get => _cAxisCoordinateSystemValue;
            set
            {
                _cAxisCoordinateSystemValue = value;
                OnPropertyChanged();
            }
        }


        private void ShowXInputDialog(object parameter)
        {
            ShowInputDialog("X轴", XAxisCoordinateSystemValue, value => XAxisCoordinateSystemValue = value);
        }

        private void ShowYInputDialog(object parameter)
        {
            ShowInputDialog("Y轴", YAxisCoordinateSystemValue, value => YAxisCoordinateSystemValue = value);
        }

        private void ShowZInputDialog(object parameter)
        {
            ShowInputDialog("Z轴", ZAxisCoordinateSystemValue, value => ZAxisCoordinateSystemValue = value);
        }

        private void ShowBInputDialog(object parameter)
        {
            ShowInputDialog("B轴", BAxisCoordinateSystemValue, value => BAxisCoordinateSystemValue = value);
        }

        private void ShowCInputDialog(object parameter)
        {
            ShowInputDialog("C轴", CAxisCoordinateSystemValue, value => CAxisCoordinateSystemValue = value);
        }

        private void ShowInputDialog(string axisName, double currentValue, Action<double> setValue)
        {
            // 获取当前活动窗口作为Owner
            Window owner = GetActiveWindow();

            // 创建输入对话框
            var dialog = new Window
            {
                Title = $"修改{axisName}值",
                Width = 300,
                Height = 180,
                WindowStyle = WindowStyle.SingleBorderWindow,
                ResizeMode = ResizeMode.NoResize,
                Background = System.Windows.Media.Brushes.White,
                BorderBrush = System.Windows.Media.Brushes.ForestGreen,
                BorderThickness = new Thickness(1),
                Owner = owner, //设置Owner
                WindowStartupLocation = WindowStartupLocation.CenterOwner //以所属窗口为中心显示
            };

            // 创建内容
            var grid = new Grid { Margin = new Thickness(20) };
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // TextBox行
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Button行

            var textPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 5, 0, 0) // 按钮与输入框的间距
            };
            var label = new TextBlock { Text = $"{axisName}值:", Margin = new Thickness(0, 0, 0, 5) };
            var textBox = new TextBox
            {
                Text = currentValue.ToString(),
                Margin = new Thickness(5, 0, 0, 5),
                Width = 200
            };
            textPanel.Children.Add(label);
            textPanel.Children.Add(textBox);

            Grid.SetRow(textPanel, 0); // ✅ 设置行索引

            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 5, 0, 0) // 按钮与输入框的间距
            };
            var okButton = new Button
            {
                Content = "确定",
                Width = 70,
                Height = 30,
                Margin = new Thickness(5, 0, 0, 0)
            };
            var cancelButton = new Button
            {
                Content = "取消",
                Width = 70,
                Height = 30,
                Margin = new Thickness(5, 0, 0, 0)
            };
            buttonPanel.Children.Add(okButton);
            buttonPanel.Children.Add(cancelButton);
            Grid.SetRow(buttonPanel, 1); // ✅ 设置行索引

            //grid.Children.Add(label);
            grid.Children.Add(textPanel);
            grid.Children.Add(buttonPanel);
            dialog.Content = grid;

            // 确定按钮事件
            okButton.Click += (s, e) =>
            {
                if (double.TryParse(textBox.Text, out double newValue))
                {
                    setValue(newValue);
                    dialog.Close();
                }
                else
                {
                    MessageBox.Show("请输入有效的数字！");
                }
            };

            // 取消按钮事件
            cancelButton.Click += (s, e) => dialog.Close();

            // 确保对话框显示在主窗口内
            dialog.ShowDialog();
        }

        // 获取当前活动窗口（安全实现）
        private Window GetActiveWindow()
        {
            // 优先获取主窗口
            if (Application.Current.MainWindow != null)
                return Application.Current.MainWindow;

            // 获取活动窗口
            return Application.Current.Windows.OfType<Window>()
                .FirstOrDefault(w => w.IsActive);
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }



    }
    // ✅ 修复1：确保 RelayCommand 正确实现（关键！）
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Predicate<object> _canExecute;

        public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute?.Invoke(parameter) ?? true;
        public void Execute(object parameter) => _execute(parameter);
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}
