using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using GameUpdater.Services;
using ReactiveUI;
using GameUpdater.Views;

namespace GameUpdater.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public MainWindowViewModel()
        {
            var manageGame = new ManageGameViewModel(this);
            BottomBar = manageGame;
            PopupOpen = false;
            var updater = new Updater(
                IniLoader.Instance.Read("LocalManifest", "General"),
                IniLoader.Instance.Read("ServerManifest", "General"));
            Content = new DownloaderViewModel(this, updater);
            updater.OnPatchFinished += _onPatchFinished;
            updater.OnPatchError += UpdaterOnPatchError;
            updater.StartDownload();
        }

        private async void UpdaterOnPatchError(object sender, string error)
        {
            await Task.Delay(2000);
            if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                await GameUpdaterMessageBox.Open(desktop.MainWindow, error + "\nPlease try again later!", "Error Downloading patch!", GameUpdaterMessageBox.MessageBoxButtons.Ok);
                desktop.Shutdown();
            }
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
            if(Convert.ToBoolean(IniLoader.Instance.Read("AutoStartGame", "Settings")))
                ((StartGameViewModel) Content).StartGame();
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
