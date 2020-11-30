namespace Limxc.Tools.DeviceComm.Entities
{
    public class CPCmdTaskManager : CPCmd
    {
        /// <summary>
        /// 初始化指令模板,占位符 $n n=1-9位 length=n*2
        /// </summary>
        /// <param name="command"></param>
        /// <param name="respTemplate"></param>
        /// <param name="timeout">读取延时(毫秒):读取完整数据所需时间,每条指令不同</param>
        /// <param name="cmdDesc"></param>
        /// <param name="respDesc"></param>
        public CPCmdTaskManager(string command, string respTemplate, int timeout, int retryCount = 0, string cmdDesc = "", string respDesc = "") : base(command, respTemplate, cmdDesc, respDesc)
        {
            TimeOut = timeout;
            RetryCount = retryCount;
        }

        /// <summary>
        /// 初始化指令模板,占位符 $n n=1-9位 length=n*2
        /// 无返回值
        /// </summary>
        /// <param name="command"></param>
        /// <param name="timeout"></param>
        /// <param name="cmdDesc"></param>
        public CPCmdTaskManager(string command, int timeout, int retryCount = 0, string cmdDesc = "") : this(command, "", timeout, 0, cmdDesc, "")
        { }
         
        /// <summary>
        /// 读取超时(毫秒):读取完整数据所需时间,每条指令不同
        /// </summary>
        public int TimeOut { get; private set; }

        /// <summary>
        /// 最大尝试次数
        /// </summary>
        public int RetryCount { get; private set; }
    }
}