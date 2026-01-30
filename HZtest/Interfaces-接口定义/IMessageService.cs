namespace HZtest.Interfaces_接口定义
{
    public interface IMessageService
    {
        void Show(string message, string title = "提示");
        void ShowError(string message);

        /// <summary>
        /// 显示消息提示
        /// </summary>
        void ShowMessage(string message, string title = "提示");

    }
}