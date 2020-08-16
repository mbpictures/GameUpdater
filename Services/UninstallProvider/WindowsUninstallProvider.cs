using System;
using System.Diagnostics;
using Microsoft.Win32;

namespace GameUpdater.Services.UninstallProvider
{
    public class WindowsUninstallProvider : IUninstallProvider
    {
        public void Uninstall()
        {
            if (IniLoader.Instance.Read("UseUniversalUninstall", "Windows") == "True")
            {
                new UniversalUninstallProvider().Uninstall();
                return;
            }
            
            if (string.IsNullOrEmpty(IniLoader.Instance.Read("MsiName", "Windows"))) return;
            var process = new Process();
            var startInfo = new ProcessStartInfo();
            process.StartInfo = startInfo;

            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardError = true;

            startInfo.FileName = "msiexec.exe";
            startInfo.Arguments = _getUninstallString(IniLoader.Instance.Read("MsiName", "Windows"));

            process.Start();
        }
        
        private static string _getUninstallString(string msiName)
        {
            var uninstallString = string.Empty;
            try
            {
                const string path = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Installer\\UserData\\S-1-5-18\\Products";

                var key = Registry.LocalMachine.OpenSubKey(path);

                if (key == null) return uninstallString;
                foreach (var tempKeyName in key.GetSubKeyNames())
                {
                    var tempKey = key.OpenSubKey(tempKeyName + "\\InstallProperties");
                    if (tempKey == null) continue;
                    if (!string.Equals(Convert.ToString(tempKey.GetValue("DisplayName")), msiName,
                        StringComparison.CurrentCultureIgnoreCase)) continue;
                    uninstallString = Convert.ToString(tempKey.GetValue("UninstallString"));
                    uninstallString = uninstallString?.Replace("/I", "/X");
                    uninstallString = uninstallString?.Replace("MsiExec.exe", "").Trim();
                    uninstallString += " /quiet /qn";
                    break;
                }

                return uninstallString;
            }
            catch (Exception ex)
            {
                throw new ApplicationException(ex.Message);
            }
        }
        
    }
}