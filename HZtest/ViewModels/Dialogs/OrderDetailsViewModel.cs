using HZtest.Converters;
using HZtest.Interfaces_接口定义;
using HZtest.Models;
using HZtest.Models.BaseClass;
using HZtest.Models.DB;
using HZtest.Models.Response;
using HZtest.Services;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;

namespace HZtest.ViewModels.Dialogs
{
    /// <summary>
    /// 订单详情对话框的 ViewModel
    /// </summary>
    public class OrderDetailsViewModel : IDialogAware, INotifyPropertyChanged
    {
        // 通知接口实现
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        /// <summary>
        /// 对话框打开时，接收父窗口传入的参数
        /// </summary>
        public void OnDialogOpened(object parameter)
        {
            if (parameter is OrderManagementModel currentMode)
            {
                var Order = currentMode;
            }
        }
        // IDialogAware 实现
        public event EventHandler<object> RequestClose;


        // ===== 依赖服务（构造函数注入）=====
        private readonly IStructuredLogger _logger;
        private readonly SqlSugarClient _db;
        #region UI按钮命令
        /// <summary>
        /// 关闭窗口命令
        /// </summary>
        public ICommand CloseCommand { get; }

        /// <summary>
        /// 搜索查询命令
        /// </summary>
        public ICommand SearchCommand { get; }
        #endregion
        #region UI绑定属性
        private long _selectedOrderID = -1;
        public long SelectedOrderID
        {
            get => _selectedOrderID;
            set { _selectedOrderID = value; OnPropertyChanged(); }

        }
        public List<OderDetailsDropdownBoxItem> OrderList { get; private set; } = new List<OderDetailsDropdownBoxItem>();


        private int _selectedOrderDetailsType = -10;
        public int SelectedOrderDetailsType
        {
            get => _selectedOrderDetailsType;
            set { _selectedOrderDetailsType = value; OnPropertyChanged(); }
        }

        public List<EnumDescriptionRegisterAddress> OrderDetailsTypeList { get; } = new List<EnumDescriptionRegisterAddress>();


        private List<OrderDetailsResponse> _orderDetailsList = new List<OrderDetailsResponse>();
        public List<OrderDetailsResponse> OrderDetailsList
        {
            get => _orderDetailsList;
            set { _orderDetailsList = value; OnPropertyChanged(); }
        }

        #endregion

        public OrderDetailsViewModel(IStructuredLogger logger, SqlSugarClient db)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _db = db ?? throw new ArgumentNullException(nameof(db));
            CloseCommand = new RelayCommand(Cancel);
            SearchCommand = new AsyncRelayCommand(Search);
            LoadOrderListAndDetailsTypeList();
        }

        /// <summary>
        /// 加载订单列表和订单详情类型列表
        /// </summary>
        private async void LoadOrderListAndDetailsTypeList()
        {
            try
            {

                OrderList = await _db.QueryableWithAttr<OrderManagementModel>()
                   .Select(o => new OderDetailsDropdownBoxItem
                   {
                       Value = o.Id,
                       Description = $"订单 {o.OrderCode} - {o.ProcessingPartName} _ {o.OrderQuatity}"
                   })
                   .ToListAsync();
                // 插入空选项到最前面
                OrderList.Insert(0, new OderDetailsDropdownBoxItem
                {
                    Value = -1,  // 或 -1
                    Description = "-- 全部 --"
                });

                //// 初始化枚举选项（自动获取Description）
                foreach (var value in Enum.GetValues(typeof(OrderDetailsEnum)))
                {
                    if ((int)value == OrderDetailsEnum.Error.GetHashCode())
                        break;
                    var description = UniversalValueConversion.GetDescription((OrderDetailsEnum)value);
                    OrderDetailsTypeList.Add(new EnumDescriptionRegisterAddress
                    {
                        Value = (int)value,
                        Description = description
                    });
                }
                OrderDetailsTypeList.Insert(0, new EnumDescriptionRegisterAddress
                {
                    Value = -10,
                    Description = "-- 全部 --"

                });

                await Search();
            }
            catch (Exception ex)
            {
                _logger.Error("加载订单列表和订单详情类型列表失败", ex);
            }
        }

        /// <summary>
        /// 取消按钮命令
        /// </summary>
        public void Cancel()
        {
            RequestClose?.Invoke(this, null); // 通知关闭，返回 null 表示取消
        }

        /// <summary>
        /// 筛选数据
        /// </summary>
        private async Task Search()
        {
            try
            {
                OrderDetailsList = await _db.QueryableWithAttr<OrderDetailsModel>()
                    .Includes(o => o.OrderManagement)
                    .WhereIF(SelectedOrderID != -1, o => o.FK_OrderManagementId == SelectedOrderID)
                    .WhereIF(SelectedOrderDetailsType != -10, o => o.OrderDetailsType == (OrderDetailsEnum)SelectedOrderDetailsType)
                    .OrderBy(o => o.SerialNumber)
                    .Select(o => new OrderDetailsResponse
                    {
                        OrderCode = o.OrderManagement.OrderCode,
                        OrderQuantity = o.OrderManagement.OrderQuatity,
                    }, true)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error("查询订单详情数据失败", ex);
            }


        }

    }
}
