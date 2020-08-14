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
            Content = new DownloaderViewModel(updater);
            updater.OnPatchFinished += _onPatchFinished;
            updater.StartDownload(true);
            var manageGame = new ManageGameViewModel();
            BottomBar = manageGame;
            PopupOpen = false;
            manageGame.OnCheckFiles += _onOpenCheckFiles; 
        }

        private void _onOpenCheckFiles(object sender)
        {
            PopupOpen = true;
            CheckFilesViewModel checkFilesViewModel = new CheckFilesViewModel();
            checkFilesViewModel.OnCheckFilesFinished += o => PopupOpen = false;
            PopupContent = checkFilesViewModel;
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
