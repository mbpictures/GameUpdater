using GameUpdater.Services;
using ReactiveUI;

namespace GameUpdater.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public MainWindowViewModel()
        {
            var updater = new Updater(
                IniLoader.Instance.Read("LocalManifest", "General"),
                IniLoader.Instance.Read("ServerManifest", "General"));
            Content = new DownloaderViewModel(this, updater);
            updater.OnPatchFinished += _onPatchFinished;
            updater.StartDownload();
            var manageGame = new ManageGameViewModel(this);
            BottomBar = manageGame;
            PopupOpen = false;
        }

        public void OpenPopup(ViewModelBase content)
        {
            PopupOpen = true;
            PopupContent = content;
        }

        public void ClosePopup()
        {
            PopupOpen = false;
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
        
        private ViewModelBase _bottomBar;

        public ViewModelBase BottomBar
        {
            get => _bottomBar;
            private set => this.RaiseAndSetIfChanged(ref _bottomBar, value);
        }
        
        private ViewModelBase _popupContent;

        public ViewModelBase PopupContent
        {
            get => _popupContent;
            private set => this.RaiseAndSetIfChanged(ref _popupContent, value);
        }
        
        private bool _popupOpen;

        public bool PopupOpen
        {
            get => _popupOpen;
            private set => this.RaiseAndSetIfChanged(ref _popupOpen, value);
        }
    }
}
