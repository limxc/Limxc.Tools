using System;

namespace Limxc.Tools.Entities.DevComm
{
    public class CPContext
    {
        /// <summary>
        ///     无返回值
        /// </summary>
        /// <param name="cmdTemplate"></param>
        /// <param name="desc"></param>
        public CPContext(string cmdTemplate, string desc = "")
        {
            Command = new CPCommand(cmdTemplate);
            Response = new CPResponse();
            Timeout = 0;
            Desc = desc;
        }

        /// <summary>
        ///     有返回值
        /// </summary>
        /// <param name="cmdTemplate"></param>
        /// <param name="respTemplate"></param>
        /// <param name="timeout"></param>
        /// <param name="desc"></param>
        public CPContext(string cmdTemplate, string respTemplate, int timeout = 1000, string desc = "")
        {
            Command = new CPCommand(cmdTemplate);
            Response = new CPResponse(respTemplate);
            Timeout = timeout;
            Desc = desc;
        }

        public string Desc { get; }

        public CPCommand Command { get; }
        public CPResponse Response { get; }

        /// <summary>
        ///     响应时间(毫秒): 下位机处理并返回结果的时常
        ///     0表明没有返回值
        /// </summary>
        public int Timeout { get; }

        /// <summary>
        ///     解析状态
        /// </summary>
        public CPContextState State { get; internal set; } = CPContextState.Waiting;

        public DateTime? SendTime { get; set; }
        public DateTime? ReceivedTime { get; set; }

        public override string ToString()
        {
            return
                $"{Desc} : {Command}    |    State:{State} Send@{SendTime:hh:mm:ss fff}  Receive@{ReceivedTime:hh:mm:ss fff}    |    {Response}";
        }
    }
}