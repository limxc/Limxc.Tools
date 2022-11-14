using System;

namespace Limxc.Tools.Bases.Communication
{
    public class CommContext
    {
        /// <summary>
        ///     无返回值
        /// </summary>
        /// <param name="cmdTemplate"></param>
        /// <param name="desc"></param>
        public CommContext(string cmdTemplate, string desc = "")
        {
            Command = new CommCommand(cmdTemplate);
            Response = new CommResponse();
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
        public CommContext(string cmdTemplate, string respTemplate, int timeout = 1000, string desc = "")
        {
            Command = new CommCommand(cmdTemplate);
            Response = new CommResponse(respTemplate);
            Timeout = timeout;
            Desc = desc;
        }

        public string Desc { get; }

        public CommCommand Command { get; }
        public CommResponse Response { get; }

        /// <summary>
        ///     响应时间(毫秒): 下位机处理并返回结果的时常
        ///     0表明没有返回值
        /// </summary>
        public int Timeout { get; }

        /// <summary>
        ///     解析状态
        /// </summary>
        public CommContextState State { get; internal set; } = CommContextState.Waiting;

        public DateTime? SendTime { get; set; }
        public DateTime? ReceivedTime { get; set; }

        public override string ToString()
        {
            return
                $"{Desc} : {Command}    |    State:{State} Send@{SendTime:hh:mm:ss fff}  Receive@{ReceivedTime:hh:mm:ss fff}    |    {Response}";
        }
    }
}