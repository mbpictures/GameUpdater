using System.Collections.Generic;
using GameUpdater.Services;
using ReactiveUI;

namespace GameUpdater.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public MainWindowViewModel()
        {
            Updater dwnl = new Updater(
                IniLoader.Instance.Read("LocalManifest", "General"),
                IniLoader.Instance.Read("ServerManifest", "General"));
            Content = new DownloaderViewModel(dwnl);
            dwnl.OnPatchFinished += _onPatchFinished;
            dwnl.StartDownload(true);
        }

        private void _onPatchFinished(object sender)
        {
            Content = new StartGameViewModel();
        }

        private ViewModelBase _content;

        public ViewModelBase Content
        {
            get => _content;
            private set => this.RaiseAndSetIfChanged(ref _content, value);
        }
    }
}
