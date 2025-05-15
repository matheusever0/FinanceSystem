using System.Reflection;

namespace Equilibrium.API.Configuration
{
    public static class VersionInfo
    {
        public static string Version => Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0.0";
        public static string BuildDate => System.IO.File.GetLastWriteTime(Assembly.GetExecutingAssembly().Location).ToString("yyyy-MM-dd HH:mm:ss");
    }
}
