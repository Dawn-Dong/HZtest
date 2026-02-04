using HZtest.Interfaces_接口定义;
using HZtest.Models.Request;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;

namespace HZtest.ViewModels.Dialogs
{
    public class UploadFileViewModel : IDialogAware, INotifyPropertyChanged
    {
        // 通知接口实现
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        // IDialogAware 实现
        public event EventHandler<object> RequestClose;


        private string _localFilePath = string.Empty;

        /// <summary>
        /// 要上传的文件路径
        /// </summary>
        public string LocalFilePath
        {
            get => _localFilePath;
            set { _localFilePath = value; OnPropertyChanged(); }
        }



        private string _fileName = string.Empty;

        /// <summary>
        /// 上传后文件命名
        /// </summary>
        public string FileName
        {
            get => _fileName;
            set { _fileName = value; OnPropertyChanged(); }
        }




        /// <summary>
        /// 对话框打开时，接收父窗口传入的参数
        /// </summary>
        public void OnDialogOpened(object parameter)
        {
            if (parameter is int currentMode)
            {
                var CurrentMode = currentMode;  // 初始化当前选中项
            }
        }

        /// <summary>
        /// 确定按钮命令
        /// </summary>
        public void Confirm()
        {
            var uploadRequest = new FileUploadRequest
            {
                LocalFilePath = this.LocalFilePath,
                FileName = this.FileName,
                //UserOperation= true
            };


            RequestClose?.Invoke(this, uploadRequest); // 通知关闭并返回结果
        }

        /// <summary>
        /// 取消按钮命令
        /// </summary>
        public void Cancel()
        {
            RequestClose?.Invoke(this, null); // 通知关闭，返回 null 表示取消
        }



        private ICommand _selectFileCommand;
        public ICommand SelectFileCommand
        {
            get
            {
                return _selectFileCommand ?? (_selectFileCommand = new RelayCommand(SelectFile));
            }
        }

        private void SelectFile()
        {
            // 创建打开文件对话框
            var openFileDialog = new Microsoft.Win32.OpenFileDialog();

            // 设置对话框的过滤器
            openFileDialog.Filter = "所有文件|*.*";
            openFileDialog.Title = "选择要上传的文件";

            // 显示对话框
            if (openFileDialog.ShowDialog() == true)
            {
                // 设置文件路径到绑定属性
                LocalFilePath = openFileDialog.FileName;
            }
        }

    }
}
