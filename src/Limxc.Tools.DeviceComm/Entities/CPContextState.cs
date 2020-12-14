namespace Limxc.Tools.DeviceComm.Entities
{
    public enum CPContextState
    {
        /// <summary>
        /// 等待解析
        /// </summary>
        Waiting = 0,

        /// <summary>
        /// 解析成功
        /// </summary>
        Success = 1,

        /// <summary>
        /// 无需解析
        /// </summary>
        NoNeed = 2,

        /// <summary>
        /// 解析超时(返回值丢失)
        /// </summary>
        Timeout = 3
    }
}