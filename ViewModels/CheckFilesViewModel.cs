using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using GameUpdater.Services;
using MessageBox.Avalonia.DTO;
using MessageBox.Avalonia.Enums;
using ReactiveUI;

namespace GameUpdater.ViewModels
{
    public class CheckFilesViewModel : ViewModelBase
    {
        public delegate void CheckFilesFinished(object sender);

        public event CheckFilesFinished OnCheckFilesFinished;

        private int _amountCorruptedFiles = 0;

        public CheckFilesViewModel()
        {
            Updater updater = new Updater(
                IniLoader.Instance.Read("LocalManifest", "General"),
                IniLoader.Instance.Read("ServerManifest", "General"));
            FileChecker fileChecker = new FileChecker(updater.GetPatches(true));
            fileChecker.OnUpdateCurrentInfo += FileCheckerOnUpdateCurrentInfo;
            
            Queue<DownloadFile> queue = new Queue<DownloadFile>();
            fileChecker.GetCorruptedFiles().ForEach(file => queue.Enqueue(file));
            _amountCorruptedFiles = queue.Count;
            Downloader downloader = new Downloader(queue, 0);
            downloader.OnProgressChanged += _onProgressChanged;
            downloader.OnDownloadComplete += _onDownloadFinished;
            downloader.OnFileChanged += DownloaderOnFileChanged;
            downloader.StartDownload();
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
                new MessageBoxStandardParams()
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