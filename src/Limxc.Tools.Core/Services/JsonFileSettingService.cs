using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using Limxc.Tools.Contract.Interfaces;
using Limxc.Tools.Extensions;

namespace Limxc.Tools.Core.Services
{
    public class JsonFileSettingService<T> : BaseFileSettingService<T>
        where T : class, new()
    {
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public JsonFileSettingService(ILogService logService = null)
            : base(logService)
        {
            _jsonSerializerOptions = new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
                WriteIndented = true,
                IgnoreReadOnlyProperties = true,
                ReferenceHandler = ReferenceHandler.IgnoreCycles
            };
        }

        protected override string FileExtension => "json";

        protected override void SaveSetting(T setting)
        {
            var json = JsonSerializer.Serialize(setting, _jsonSerializerOptions);
            json.Save(FullPath, false, Encoding.UTF8);
        }

        protected override T LoadSetting(string path)
        {
            var json = FullPath.Load(Encoding.UTF8);
            return JsonSerializer.Deserialize<T>(json, _jsonSerializerOptions);
        }
    }
}
