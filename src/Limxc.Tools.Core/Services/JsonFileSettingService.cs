using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
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
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                WriteIndented = true,
                ReferenceHandler = ReferenceHandler.IgnoreCycles,

                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true,

                NumberHandling = JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString
            };
        }

        protected override string FileExtension => "json";

        protected override void SaveSetting(T setting)
        {
            var json = JsonSerializer.Serialize(setting, _jsonSerializerOptions);
            json.Save(FullPath, false, false, Encoding.UTF8);
        }

        protected override T LoadSetting(string path)
        {
            var json = FullPath.Load(Encoding.UTF8);
            return JsonSerializer.Deserialize<T>(json, _jsonSerializerOptions);
        }
    }
}