using System.Threading.Tasks;

namespace HZtest.Interfaces_接口定义
{
    /// <summary>
    /// 对话框服务（支持自定义弹窗）
    /// </summary>
    public interface IDialogService
    {
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
