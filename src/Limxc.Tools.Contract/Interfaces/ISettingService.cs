using System;

namespace Limxc.Tools.Contract.Interfaces
{
    public interface ISettingService<T> : IDisposable where T : class, new()
    {
        Action<T> SettingChanged { get; set; }

        /// <summary>
        /// </summary>
        /// <param name="initOnFailure">失败时重建(初始值)</param>
        /// <returns></returns>
        T Load(bool initOnFailure = true);

        void Save(T setting);
    }
}