using System.Text;
using System.Text.Json;
using Limxc.Tools.Contract.Interfaces;
using Limxc.Tools.Extensions;

namespace Limxc.Tools.Core.Services
{
    public class JsonFileSettingService<T> : BaseFileSettingService<T>
        where T : class, new()
    {
        public JsonFileSettingService(ILogService logService = null)
            : base(logService)
        {
        }

        protected override string FileExtension => "json";

        protected override void SaveSetting(T setting)
        {
            var json = setting.ToJson(true);
            json.Save(FullPath, false, false, Encoding.UTF8);
        }

        protected override T LoadSetting(string path)
        {
            var json = FullPath.Load(Encoding.UTF8);
            return JsonSerializer.Deserialize<T>(json);
        }
    }
}