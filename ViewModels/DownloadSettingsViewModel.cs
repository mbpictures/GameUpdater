using Avalonia.Controls;
using GameUpdater.Services;
using ReactiveUI;

namespace GameUpdater.ViewModels
{
    public class DownloadSettingsViewModel : ViewModelBase
    {
        private MainWindowViewModel _parent;
        private Updater _updater;
        public DownloadSettingsViewModel(MainWindowViewModel parent, Updater updater)
        {
            _parent = parent;
            _updater = updater;
            Context = this;
        }

        public void SetMaxBytesPerSecond(long bytesPerSecond)
        {
            _updater.SetMaxBytesPerSecond(bytesPerSecond);
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