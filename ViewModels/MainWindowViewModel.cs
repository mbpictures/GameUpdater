using System.Collections.Generic;
using GameUpdater.Services;
using ReactiveUI;

namespace GameUpdater.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public MainWindowViewModel()
        {
            Updater dwnl = new Updater("./update.xml", "http://projects.marius-butz.de/updater/update.xml");
            Content = new DownloaderViewModel(dwnl);
            dwnl.OnPatchFinished += _onPatchFinished;
            dwnl.StartDownload();
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
