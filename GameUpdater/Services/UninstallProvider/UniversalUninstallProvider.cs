using System.IO;

namespace GameUpdater.Services.UninstallProvider
{
    public class UniversalUninstallProvider : IUninstallProvider
    {
        public void Uninstall()
        {
            if (string.IsNullOrEmpty(IniLoader.Instance.Read("GameDirectory", "General"))) return;
            Directory.Delete(Path.GetFullPath(IniLoader.Instance.Read("GameDirectory", "General")), true);
        }
    }
}