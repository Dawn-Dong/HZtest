using System;

namespace HZtest.Interfaces_接口定义
{
    /// <summary>
    /// 结构化日志接口 - 通过参数数量区分，无歧义
    /// </summary>
    public interface IStructuredLogger
    {
        // ========== Debug ==========
        void Debug(string message);
        void Debug(string subCategory, string message);
        void Debug(string folder, string subCategory, string message);

        // ========== Info ==========
        void Info(string message);
        void Info(string subCategory, string message);
        void Info(string folder, string subCategory, string message);

        // ========== Error ==========
        void Error(string message);
        void Error(string message, Exception ex);
        void Error(string subCategory, string message);
        void Error(string subCategory, string message, Exception ex);
        void Error(string folder, string subCategory, string message);
        void Error(string folder, string subCategory, string message, Exception ex);

        // ========== Warn ==========
        void Warn(string message);
        void Warn(string subCategory, string message);
        void Warn(string folder, string subCategory, string message);

        // ========== Fatal ==========
        void Fatal(string message);
        void Fatal(string message, Exception ex);
        void Fatal(string subCategory, string message);
        void Fatal(string subCategory, string message, Exception ex);
        void Fatal(string folder, string subCategory, string message);
        void Fatal(string folder, string subCategory, string message, Exception ex);

        // ========== Trace ==========
        void Trace(string message);
        void Trace(string subCategory, string message);
        void Trace(string folder, string subCategory, string message);
    }
}