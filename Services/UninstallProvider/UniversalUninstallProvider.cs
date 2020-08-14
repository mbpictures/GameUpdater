using System.IO;

namespace GameUpdater.Services.UninstallProvider
{
    public class UniversalUninstallProvider : UninstallProvider
    {
        public void Uninstall()
        {
            if (string.IsNullOrEmpty(IniLoader.Instance.Read("GameDirectory", "General"))) return;
            var path = Path.GetFullPath(IniLoader.Instance.Read("GameDirectory", "General"));
            Directory.Delete(Path.GetFullPath(IniLoader.Instance.Read("GameDirectory", "General")), true);
        }
    }
}