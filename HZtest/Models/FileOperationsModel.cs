using HZtest.Infrastructure_基础设施;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace HZtest.Models
{
    /// <summary>
    /// 文件操作模型
    /// </summary>
    public class FileOperationsModel
    {
        /// <summary>
        /// 正在运行的文件
        /// </summary>
        public string RunningFile { get; set; } = string.Empty;
        /// <summary>
        /// 正在运行的文件详情
        /// </summary>
        public string RunningDetailsFile { get; set; } = string.Empty;

        /// <summary>
        /// 目录文件列表
        /// </summary>
        public List<string> DirectoryFileList { get; set; } = new List<string>();

    }

    /// <summary>
    /// 树节点
    /// </summary>
    public class FileNode : PageViewModelBaseClass
    {
        private bool _isExpanded;
        private bool _isSelected;

        public string Name { get; set; }
        public string FullPath { get; set; }      // 完整路径如 "test/ceshi456"
        public bool IsDirectory { get; set; }     // 是否有子项
        public ObservableCollection<FileNode> Children { get; } = new();
        public FileNode Parent { get; set; }

        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                _isExpanded = value;
                OnPropertyChanged();
            }
        }

        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; OnPropertyChanged(); }
        }
    }




}
