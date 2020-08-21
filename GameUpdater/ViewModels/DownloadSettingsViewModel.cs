using GameUpdater.Services;
using ReactiveUI;

namespace GameUpdater.ViewModels
{
    public class DownloadSettingsViewModel : ViewModelBase
    {
        private readonly MainWindowViewModel _parent;
        private readonly Updater _updater;
        public DownloadSettingsViewModel(MainWindowViewModel parent, Updater updater)
        {
            _parent = parent;
            _updater = updater;
            Context = this;
        }

        public void SetMaxBytesPerSecond(long bytesPerSecond, double sliderValue)
        {
            _updater.SetMaxBytesPerSecond(bytesPerSecond);
            IniLoader.Instance.AddSetting("Settings", "MaxBytesPerSecond", $"{sliderValue}");
            IniLoader.Instance.SaveSettings();
        }

        public void CloseDownloadSettings()
        {
            _parent.ClosePopup();
        }

        private DownloadSettingsViewModel _context;
        public DownloadSettingsViewModel Context
        {
            get => _context;
            private set => this.RaiseAndSetIfChanged(ref _context, value);
        }
    }
}