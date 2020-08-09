using System.Collections.Generic;
using GameUpdater.Services;
using ReactiveUI;

namespace GameUpdater.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public MainWindowViewModel()
        {
            Content = new DownloaderViewModel();
        }

        private ViewModelBase _content;

        public ViewModelBase Content
        {
            get => _content;
            private set => this.RaiseAndSetIfChanged(ref _content, value);
        }
    }
}
