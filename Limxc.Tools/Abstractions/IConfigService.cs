namespace Limxc.Tools.Abstractions
{
    public interface IConfigService
    {
        string Get(string key, string section = "", string fileName = "");
        T Get<T>(string fileName = "") where T : class, new();
        void Set(string key, string value, string section = "", string fileName = "");
        void Set<T>(T obj, string fileName = "") where T : class, new();
    }
}