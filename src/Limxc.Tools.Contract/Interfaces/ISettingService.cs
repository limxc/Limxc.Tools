using System;

namespace Limxc.Tools.Contract.Interfaces
{
    public interface ISettingService<T> : IDisposable where T : class, new()
    {
        Action<T> SettingChanged { get; set; }
        T Load();
        void Save(T setting);
    }
}