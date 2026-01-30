using System.Windows.Controls;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace HZtest.Interfaces_接口定义
{
    /// <summary>
    /// 对话框服务（支持自定义弹窗）
    /// </summary>
    public interface IDialogService
    {
        /// <summary>
        /// 初始化对话框服务，传入窗口中的根 Grid（用于添加遮罩层）
        /// </summary>
        void Initialize(Grid rootGrid);

        /// <summary>
        /// 显示确认对话框
        /// </summary>
        Task<bool> ShowConfirmAsync(string message, string title = "确认");

        /// <summary>
        /// 显示对话框并返回结果
        /// </summary>
        Task<TResult> ShowDialogAsync<TResult>(string dialogName, object parameter = null);
    }
}