namespace Limxc.Tools.Contract.Interfaces
{
    public interface IConfigService
    {
        void Set(string key, string value, string section = "Common", string fileName = "Setting.ini");
        string Get(string key, string def = "", string section = "Common", string fileName = "Setting.ini");

        void Set<T>(T obj, string fileName = "Setting.ini") where T : class, new();
        T Get<T>(string fileName = "Setting.ini", T def = default) where T : class, new();

        string GetFilePath(string fileName);
    }
}