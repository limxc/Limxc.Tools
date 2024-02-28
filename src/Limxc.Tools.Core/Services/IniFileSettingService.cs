using Limxc.Tools.Contract.Interfaces;
using Limxc.Tools.Core.Dependencies;

namespace Limxc.Tools.Core.Services
{
    public class IniFileSettingService<T> : BaseFileSettingService<T> where T : class, new()
    {
        public IniFileSettingService(ILogService logService = null) : base(logService)
        {
        }

        protected override string FileExtension => "ini";

        protected override void SaveSetting(T setting)
        {
            IniSaved<T>.Save(FullPath, setting);
        }

        protected override T LoadSetting(string path)
        {
            return IniSaved<T>.Load(FullPath);
        }
    }
}