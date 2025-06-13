using System;
using System.Threading.Tasks;

namespace Limxc.Tools.SerialPort
{
    public interface ISerialPortService : IDisposable
    {
        bool IsConnected { get; }

        IObservable<bool> ConnectionState { get; }

        IObservable<byte[]> Received { get; }

        IObservable<string> Log { get; }

        void Start(SerialPortSetting setting, Action<object> configSerialPort = null);

        void Stop();

        /// <summary>
        ///     无返回值发送
        /// </summary>
        /// <param name="bytes"></param>
        void Send(byte[] bytes);

        /// <summary>
        ///     无返回值发送
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        Task SendAsync(byte[] bytes);

        /// <summary>
        ///     在超时时长内,根据模板自动截取返回值
        /// </summary>
        /// <param name="hex"></param>
        /// <param name="timeoutMs">ms</param>
        /// <param name="template"></param>
        /// <param name="sepBegin"></param>
        /// <param name="sepEnd"></param>
        /// <returns></returns>
        Task<string> SendAsync(
            string hex,
            int timeoutMs,
            string template,
            char sepBegin = '[',
            char sepEnd = ']'
        );

        /// <summary>
        ///     发送后等待固定时长后获取返回值, 需后续手动截取
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="waitMs"></param>
        /// <returns></returns>
        Task<byte[]> SendAsync(byte[] bytes, int waitMs);
    }
}