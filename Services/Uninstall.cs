using System.Collections.Generic;

namespace GameUpdater.Services
{
    public class Uninstall
    {
        private readonly List<UninstallProvider.UninstallProvider> _uninstallProviders;
        public Uninstall()
        {
            _uninstallProviders = new List<UninstallProvider.UninstallProvider>();
            
            #if Windows
                _uninstallProviders.Add(new UninstallProvider.WindowsUninstallProvider());
            #elif Linux
                _uninstallProvider.Add(new UninstallProvider.UniversalUninstallProvider());
            #elif OSX
                _uninstallProvider.Add(new UninstallProvider.UniversalUninstallProvider());
            #endif
        }

        public void PerformUninstall()
        {
            _uninstallProviders.ForEach(provider => provider.Uninstall());
        }
    }
}