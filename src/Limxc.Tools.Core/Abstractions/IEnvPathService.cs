namespace Limxc.Tools.Core.Abstractions
{
    public interface IEnvPathService
    {
        string BaseDirectory { get; }
        string DBFolder { get; }
        string ResFolder { get; }
        string ReportFolder { get; }
        string OutputFolder { get; }
        string SettingFolder { get; }
        string ImageFolder { get; }
    }
}