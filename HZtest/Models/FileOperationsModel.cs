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


        /// <summary>
        /// 目录文件详情列表
        /// </summary>
        public List<FileDetails> FileDetailsList { get; set; } = new List<FileDetails>();




        }

    /// <summary>
    /// 文件详情
    /// </summary>
    public class FileDetails
    {
        /// <summary>
        /// 文件名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 文件类型
        /// </summary>
        public FileTypeEnum Type { get; set; } = FileTypeEnum.Error;
        /// <summary>
        /// 文件大小KB
        /// </summary>
        public int Size { get; set; } = 0;
        /// <summary>
        /// 文件修改时间
        /// </summary>
        public string ChangeTime { get; set; } = string.Empty;
    }



    ///// <summary>
    ///// 树节点
    ///// </summary>
    //public class FileNode : PageViewModelBaseClass
    //{
    //    private bool _isExpanded;
    //    private bool _isSelected;
    //    /// <summary>
    //    /// 文件名称/文件夹名称
    //    /// </summary>
    //    public string Name { get; set; }
    //    /// <summary>
    //    /// 简洁文件路径
    //    /// </summary>
    //    public string FullPath { get; set; }      // 完整路径如 "test/ceshi456"
    //    /// <summary>
    //    /// 文件类型
    //    /// </summary>
    //    public FileTypeEnum FileType { get; set; } = FileTypeEnum.Error;
    //    /// <summary>
    //    /// 是否是文件夹
    //    /// </summary>
    //    public bool IsDirectory  => FileType == FileTypeEnum.Directory;
    //    /// <summary>
    //    /// 子项集合
    //    /// </summary>
    //    public ObservableCollection<FileNode> Children { get; } = new();
    //    /// <summary>
    //    /// 父节点下所有文件
    //    /// </summary>
    //    public FileNode Parent { get; set; }

    //    /// <summary>
    //    /// 是否展开
    //    /// </summary>
    //    public bool IsExpanded
    //    {
    //        get => _isExpanded;
    //        set
    //        {
    //            _isExpanded = value;
    //            OnPropertyChanged();
    //        }
    //    }
    //    /// <summary>
    //    /// 是否选中
    //    /// </summary>
    //    public bool IsSelected
    //    {
    //        get => _isSelected;
    //        set { _isSelected = value; OnPropertyChanged(); }
    //    }
    //    /// <summary>
    //    /// 完整文件路径
    //    /// </summary>
    //    public string CompleteFullPath => $"../prog/{FullPath}";

    //}


    public class FileNode : PageViewModelBaseClass
    {
        private bool _isExpanded;
        private bool _isSelected;

        public string Name { get; set; }
        public string FullPath { get; set; }
        public FileTypeEnum Type { get; set; }      // 使用枚举
        public bool IsDirectory => Type == FileTypeEnum.Directory;  // 自动判断
        public int Size { get; set; }                // KB

        /// <summary>
        /// 完整文件路径
        /// </summary>
        public string CompleteFullPath => $"../prog/{FullPath}";
        public string ChangeTime { get; set; }       // 原始字符串

        public bool IsExpanded
        {
            get => _isExpanded;
            set { _isExpanded = value; OnPropertyChanged(); }
        }

        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; OnPropertyChanged(); }
        }

        public ObservableCollection<FileNode> Children { get; } = new();
        public FileNode Parent { get; set; }

        // 辅助属性：格式化显示大小
        public string DisplaySize => IsDirectory ? "<DIR>" : $"{Size} KB";

        // 辅助属性：格式化时间
        public string DisplayTime => ChangeTime;
    }

}
