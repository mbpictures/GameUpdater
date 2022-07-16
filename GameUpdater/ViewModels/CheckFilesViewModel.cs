using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using GameUpdater.Services;
using GameUpdater.Services.Download;
using GameUpdater.Views;
using MessageBox.Avalonia.DTO;
using MessageBox.Avalonia.Enums;
using ReactiveUI;

namespace GameUpdater.ViewModels
{
    public class CheckFilesViewModel : ViewModelBase
    {
        public delegate void CheckFilesFinished(object sender);

        public event CheckFilesFinished OnCheckFilesFinished;

        private readonly int _amountCorruptedFiles;

        public CheckFilesViewModel()
        {
            var updater = new Updater(
                IniLoader.Instance.Read("LocalManifest", "General"),
                IniLoader.Instance.Read("ServerManifest", "General"));
            var fileChecker = new FileChecker(updater.GetPatches(true));
            fileChecker.OnUpdateCurrentInfo += FileCheckerOnUpdateCurrentInfo;
            
            var queue = new Queue<DownloadFile>();
            fileChecker.GetCorruptedFiles().ForEach(file => queue.Enqueue(file));
            _amountCorruptedFiles = queue.Count;
            var downloader = new DownloadManager(queue, 0);
            downloader.OnProgressChanged += _onProgressChanged;
            downloader.OnDownloadComplete += _onDownloadFinished;
            downloader.OnFileChanged += DownloaderOnFileChanged;
            downloader.OnError += DownloaderOnOnError;
            downloader.StartDownload();
        }

        private async void DownloaderOnOnError(object sender, string error)
        {
            await Task.Delay(500);
            if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                await GameUpdaterMessageBox.Open(desktop.MainWindow, error + "\nPlease try again later!", "Error Downloading patch!", GameUpdaterMessageBox.MessageBoxButtons.Ok);
            }
        }

        private void DownloaderOnFileChanged(object sender, string currentFile)
        {
            StatusText = $"Downloading {currentFile}...";
        }

        private void FileCheckerOnUpdateCurrentInfo(object sender, string currentFile)
        {
            StatusText = $"Checking {currentFile}...";
        }

        private void _onProgressChanged(object sender, int progress)
        {
            Progress = progress;
            ProgressText = $"{progress}%";
        }

        private void _onDownloadFinished(object sender)
        {
            var messageBoxStandardWindow = MessageBox.Avalonia.MessageBoxManager.GetMessageBoxStandardWindow(
                new MessageBoxStandardParams
                {
                    ContentTitle = "File Check finished",
                    ContentMessage = $"The File Check has been finished!\n{_amountCorruptedFiles} files repaired!",
                    ButtonDefinitions = ButtonEnum.Ok,
                    Icon = Icon.Info,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                });
            
            messageBoxStandardWindow.Show().ContinueWith(task =>
            {
                if(task.Result == ButtonResult.Ok)
                    OnCheckFilesFinished?.Invoke(this);
            });
        }
        
        private int _progress;
        public int Progress
        {
            get => _progress;
            set => this.RaiseAndSetIfChanged(ref _progress, value);
        }

        private string _progressText;
        public string ProgressText
        {
            get => _progressText;
            set => this.RaiseAndSetIfChanged(ref _progressText, value);
        }
        
        private string _statusText;
        public string StatusText
        {
            get => _statusText;
            set => this.RaiseAndSetIfChanged(ref _statusText, value);
        }
    }
}