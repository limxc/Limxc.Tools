using System;

namespace Limxc.Tools.Contract.Interfaces
{
    public interface ILogService
    {
        void Debug(string message);
        void Debug(Exception exception, string message);

        void Info(string message);
        void Info(Exception exception, string message);

        void Warn(string message);
        void Warn(Exception exception, string message);

        void Error(string message);
        void Error(Exception exception, string message);

        void Fatal(string message);
        void Fatal(Exception exception, string message);
    }
}