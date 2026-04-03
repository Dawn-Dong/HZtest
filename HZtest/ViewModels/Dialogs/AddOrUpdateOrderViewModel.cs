using HZtest.Interfaces_接口定义;
using HZtest.Models.DB;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;

namespace HZtest.ViewModels.Dialogs
{
    public class AddOrUpdateOrderViewModel : IDialogAware, INotifyPropertyChanged
    {

        // 通知接口实现
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        // IDialogAware 实现
        public event EventHandler<object> RequestClose;
        // ===== 依赖服务（构造函数注入）=====
        private readonly IMessageService _message_service;
        private readonly IStructuredLogger _logger;
        private readonly SqlSugarClient _db;

        // 传递过来的实体内容- 用于区分新增还是修改
        //private OrderManagementModel? OrderManagement = null;  // 当前编辑的对象

        /// <summary>
        /// 对话框打开时，接收父窗口传入的参数
        /// </summary>
        public void OnDialogOpened(object parameter)
        {
            if (parameter is OrderManagementModel currentMode)
            {
                OrderManagement = currentMode;  // 初始化当前选中项
                // 绑定 UI 元素
                OrderCode = currentMode.OrderCode;
                OrderQuatity = currentMode.OrderQuatity;
                ProcessingPartName = currentMode.ProcessingPartName;
                ProcessingPartFilePath = currentMode.ProcessingPartFilePath;

            }
        }

        #region UI 属性


        private OrderManagementModel? OrderManagement;

        // ===== UI 属性=====  尝试使用 OrderManagement 对象的属性进行绑定，避免重复定义属性  并不好用，因为 OrderManagement 可能为 null，且直接绑定 OrderManagement.OrderCode 等属性在 OrderManagement 变更时不会自动更新UI，所以只能单独定义属性进行绑定，并在 OrderManagement 变更时同步这些属性的值
        private string _orderCode = string.Empty;
        public string OrderCode
        {
            get => _orderCode;
            set { _orderCode = value; OnPropertyChanged(); }
        }

        private int _orderQuatity = 0;
        public int OrderQuatity
        {
            get => _orderQuatity;
            set { _orderQuatity = value; OnPropertyChanged(); }
        }

        private string _processingPartName = string.Empty;
        public string ProcessingPartName
        {
            get => _processingPartName;
            set { _processingPartName = value; OnPropertyChanged(); }
        }

        private string _processingPartFilePath = string.Empty;
        public string ProcessingPartFilePath
        {
            get => _processingPartFilePath;
            set { _processingPartFilePath = value; OnPropertyChanged(); }
        }

        public string ConfirmButtonContent => OrderManagement == null ? "新增" : "更新"; // 根据是否空判断是新增还是更新
        #endregion

        #region 按钮命令
        // ===== 按钮命令=====
        /// <summary>
        /// 文件选择命令
        /// </summary>
        public ICommand SelectFileCommand { get; }

        /// <summary>
        /// 新增/更新确认按钮命令
        /// </summary>
        public ICommand ConfirmCommand { get; }

        /// <summary>
        /// 取消按钮命令
        /// </summary>
        public ICommand CloseCommand { get; }


        #endregion

        public AddOrUpdateOrderViewModel(IMessageService message_service, IStructuredLogger logger, SqlSugarClient db)
        {
            _message_service = message_service ?? throw new ArgumentNullException(nameof(message_service));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _db = db ?? throw new ArgumentNullException(nameof(db));

            // 初始化命令
            ConfirmCommand = new RelayCommand(Confirm);
            CloseCommand = new RelayCommand(Cancel);
            SelectFileCommand = new RelayCommand(SelectFile);

        }
        /// <summary>
        /// 选择上传的文件，打开文件对话框并将选择的文件路径设置到 OrderManagement 的 ProcessingPartFilePath 属性上
        /// </summary>
        private void SelectFile()
        {
            try
            {
                // 创建打开文件对话框
                var openFileDialog = new Microsoft.Win32.OpenFileDialog();
                // 设置对话框的过滤器
                openFileDialog.Filter = "文本文件 (*.txt)|*.txt";
                openFileDialog.Title = "选择要上传的文件";
                // 显示对话框
                if (openFileDialog.ShowDialog() == true)
                {
                    // 设置文件路径到绑定属性
                    ProcessingPartFilePath = openFileDialog.FileName;
                }
            }
            catch (Exception ex)
            {
                _message_service.ShowError(ex.Message);
                _logger.Error("打开文件对话框失败", ex);
            }
        }
        /// <summary>
        /// 确认按钮命令 新增/修改
        /// </summary>
        private async void Confirm()
        {
            try
            {
                var orderManagement = new OrderManagementModel()
                {
                    OrderCode = OrderCode,
                    OrderQuatity = OrderQuatity,
                    ProcessingPartName = ProcessingPartName,
                    ProcessingPartFilePath = ProcessingPartFilePath,
                    CreateTime = DateTime.Now, // 设置创建时间，新增和修改都更新这个字段，修改时会被 Updateable 的 IgnoreColumns 忽略掉
                };
                // 验证数据合法性，调用 OrderManagement 的验证方法
                var VerifyData = orderManagement.VerifyData();
                if (!VerifyData.IsSuccess)
                {
                    _message_service.ShowError($"验证未通过：{VerifyData.Message}");
                    return;
                }
                var result = 0;
                var existed = await _db.QueryableWithAttr<OrderManagementModel>()
                                    .Where(it => it.OrderCode == OrderCode)
                                    .WhereIF(OrderManagement != null, it => it.Id != OrderManagement.Id)
                                    .AnyAsync();

                if (existed)
                {
                    _message_service.ShowError($"{(OrderManagement == null ? "新增" : "更新")}失败，报警代码不能重复");
                    return;
                }

                if (OrderManagement == null) // 新增
                {
                    orderManagement?.Id = SnowFlakeSingle.Instance.NextId();
                    result = await _db.InsertableWithAttr(orderManagement).ExecuteCommandAsync();
                }
                else // 更新
                {
                    OrderManagement.ModifyTime = DateTime.Now;
                    OrderManagement.OrderCode = OrderCode;
                    OrderManagement.OrderQuatity = OrderQuatity;
                    OrderManagement.ProcessingPartName = ProcessingPartName;
                    OrderManagement.ProcessingPartFilePath = ProcessingPartFilePath;
                    result = await _db.UpdateableWithAttr(OrderManagement)
                        .IgnoreColumns(it => it.CreateTime)
                        //.Where(it => it.Id == OrderManagement.Id)
                        .ExecuteCommandAsync();
                }

                if (result > 0)
                {
                    _message_service.ShowMessage($"{(OrderManagement == null ? "新增" : "更新")}成功");
                       
                }
                else
                {
                    _message_service.ShowError($"{(OrderManagement == null ? "新增" : "更新")}失败");
                }
                RequestClose?.Invoke(this, orderManagement); // 通知关闭，返回 true 表示成功
            }
            catch (Exception ex)
            {
                _message_service.ShowError(ex.Message);
                _logger.Error("保存订单失败", ex);
            }
        }
        /// <summary>
        /// 取消按钮命令
        /// </summary>
        public void Cancel()
        {
            RequestClose?.Invoke(this, null); // 通知关闭，返回 null 表示取消
        }


    }
}
